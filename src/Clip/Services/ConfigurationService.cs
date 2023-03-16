using Clip.Models;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants;

namespace Clip.Services;

internal sealed class ConfigurationService : IConfigurationService
{
    private const string ExistingItemCountQuery = @"
        SELECT Count(*) as Count, N.uniqueId, N.nodeObjectType
        FROM umbracoContent C
        INNER JOIN cmsContentType CT ON CT.nodeId = C.contentTypeId
        INNER JOIN umbracoContentVersion V ON V.nodeId = C.nodeId
        INNER JOIN umbracoNode N on N.id = CT.nodeId
        WHERE V.[current] = 1
        GROUP BY N.uniqueID, CT.alias, N.nodeObjectType
        ORDER BY N.uniqueID";

    private readonly IScopeProvider _scopeProvider;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly IContentTypeService _contentTypeService;
    private readonly IEntityService _entityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    /// <param name="scopeProvider"></param>
    /// <param name="backOfficeSecurityAccessor"></param>
    /// <param name="eventMessagesFactory"></param>
    /// <param name="contentTypeService"></param>
    /// <param name="entityService"></param>
    public ConfigurationService(
        IScopeProvider scopeProvider,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEventMessagesFactory eventMessagesFactory,
        IContentTypeService contentTypeService,
        IEntityService entityService)
    {
        _scopeProvider = scopeProvider;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _eventMessagesFactory = eventMessagesFactory;
        _contentTypeService = contentTypeService;
        _entityService = entityService;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public ClipConfigurationModel Get()
    {
        ClipConfigurationModel? settings = GetInternal();

        if (settings is null)
        {
            return new();
        }

        settings.ExistingItemCounts = GetExistingItemCounts();

        foreach (ContentTypeCount c in settings.ContentTypeCounts)
        {
            string g = c.Udi.ToString();
            if (settings.ExistingItemCounts.TryGetValue(g, out int count))
            {
                c.Count = count;
            }
        }

        return settings;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ClipConfigurationModel? GetConfigurationModel()
    {
        ClipConfigurationModel model = new();

        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

        if (currentUser is null)
        {
            return null;
        }

        ClipConfigurationModel? settings = GetInternal();

        if (settings is null)
        {
            return null;
        }

        //only the first two values are stored
        model.Groups = settings.Groups;
        model.ContentTypeCounts = settings.ContentTypeCounts;

        // these two are generated because they may change between requests
        model.ExistingItemCounts = GetExistingItemCounts();
        (model.AllowedChildren, model.AllowedElements) = GetAllowedChildren(currentUser, settings.Groups);

        return model;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="model"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Save(ClipConfigurationModel model)
    {
        EventMessages evtMsgs = _eventMessagesFactory.Get();

        using IScope scope = _scopeProvider.CreateScope();
        ContentCreationRulesSchema poco = scope.Database.Fetch<ContentCreationRulesSchema>()?.FirstOrDefault() ?? new();
        poco.Value = JsonConvert.SerializeObject(model);
        scope.Database.Save(poco);
        _ = scope.Complete();

        evtMsgs.Add(new("Success", "Content Creation Rules updated", EventMessageType.Success));
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, int> GetExistingItemCounts()
    {
        using IScope scope = _scopeProvider.CreateScope();
        IEnumerable<ContentTypeCount> results = scope.Database.Query<ContentTypeCount>(ExistingItemCountQuery);
        _ = scope.Complete();

        return results.ToDictionary(x => x.Udi.ToString(), x => x.Count);
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="groups"></param>
    /// <returns></returns>
    private (IEnumerable<string>? DocumentTypes, IEnumerable<string>? ElementTypes) GetAllowedChildren(IUser currentUser, IEnumerable<GroupConfigurationModel> groups)
    {
        if (groups is null || !groups.Any())
        {
            return (null, null);
        }

        // need to get all the permitted types for all groups where the current user is a member;
        IEnumerable<int> groupIds = currentUser.Groups.Select(g => g.Id);

        IEnumerable<GroupConfigurationModel> groupsWhereUserIsAMember = groups.Where(g => groupIds.Contains(g.GroupId));

        // if the user has no groups with any configuration, allow normal content creation
        if (!groupsWhereUserIsAMember.Any())
        {
            return (null, null);
        }

        IEnumerable<Udi>? allowedUdis = groupsWhereUserIsAMember.SelectMany(g => g.Udis).Distinct();

        if (allowedUdis is null || !allowedUdis.Any())
        {
            return (null, null);
        }

        // another layer of the onion - media is never an element type, but we need to separate these
        // before fetching content types to check docs are element or not

        List<int> typeIds = new();
        List<Udi> mediaUdis = new();

        foreach (Udi? allowedUdi in allowedUdis)
        {
            if (allowedUdi is null)
            {
                continue;
            }

            if (allowedUdi.EntityType == UdiEntityType.MediaType)
            {
                mediaUdis.Add(allowedUdi);
                continue;
            }

            Attempt<int> id = _entityService.GetId(allowedUdi);
            if (id.Success)
            {
                typeIds.Add(id.Result);
            }
        }

        IEnumerable<IContentType>? types = typeIds.Any() ? _contentTypeService.GetAll(typeIds.ToArray()) : null;

        // return separate lists of document (plus media) and element types
        List<string> typeUdis = types?.Where(x => !x.IsElement).Select(x => x.GetUdi().ToString())?.ToList() ?? new();
        typeUdis.AddRange(mediaUdis.Select(x => x.ToString()));

        return (typeUdis, types?.Where(x => x.IsElement).Select(x => x.GetUdi().ToString()));
    }


    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    private ClipConfigurationModel? GetInternal()
    {
        using IScope scope = _scopeProvider.CreateScope();
        string? settingsStr = scope.Database.Fetch<ContentCreationRulesSchema>()?.FirstOrDefault()?.Value;
        _ = scope.Complete();

        if (settingsStr is null)
        {
            return null;
        }

        return JsonConvert.DeserializeObject<ClipConfigurationModel>(settingsStr) ?? null;
    }
}
