using Clip.Models;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Clip.Services;

public interface IConfigurationService
{
    ClipConfigurationModel GetConfigurationModel();

    void Save(ClipConfigurationModel model);

    ClipConfigurationModel Get();
}

internal class ConfigurationService : IConfigurationService
{
    private const string _ExistingItemCountQuery = @"
        SELECT Count(*) as Count, n.uniqueId, n.nodeObjectType
        FROM umbracoContent C
        INNER JOIN cmsContentType CT ON CT.nodeId = C.contentTypeId
        INNER JOIN umbracoContentVersion V ON V.nodeId = C.nodeId
        INNER JOIN umbracoNode N on N.id = CT.nodeId
        WHERE V.[current] = 1 AND CT.isElement = 0
        GROUP BY N.uniqueID, CT.alias
        ORDER BY N.uniqueID";

    private readonly IScopeProvider _scopeProvider;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEventMessagesFactory _eventMessagesFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    /// <param name="scopeProvider"></param>
    /// <param name="backOfficeSecurityAccessor"></param>
    /// <param name="eventMessagesFactory"></param>
    public ConfigurationService(
        IScopeProvider scopeProvider,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEventMessagesFactory eventMessagesFactory)
    {
        _scopeProvider = scopeProvider;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _eventMessagesFactory = eventMessagesFactory;
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
    public ClipConfigurationModel GetConfigurationModel()
    {
        ClipConfigurationModel model = new();

        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

        if (currentUser is null)
        {
            return model;
        }

        ClipConfigurationModel? settings = GetInternal();

        if (settings is null)
        {
            return model;
        }

        //only the first two values are stored
        model.Groups = settings.Groups;
        model.ContentTypeCounts = settings.ContentTypeCounts;

        // these two are generated because they may change between requests
        model.ExistingItemCounts = GetExistingItemCounts();
        model.AllowedChildren = GetAllowedChildren(currentUser, settings.Groups);

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
        IEnumerable<ContentTypeCount> results = scope.Database.Query<ContentTypeCount>(_ExistingItemCountQuery);
        _ = scope.Complete();

        return results.ToDictionary(x => x.Udi.ToString(), x => x.Count);
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="groups"></param>
    /// <returns></returns>
    private static IEnumerable<string> GetAllowedChildren(IUser currentUser, IEnumerable<GroupConfigurationModel> groups)
    {
        if (groups is null || !groups.Any())
        {
            return Enumerable.Empty<string>();
        }

        // need to get all the permitted types for all groups where the current user is a member;
        IEnumerable<int> groupIds = currentUser.Groups.Select(g => g.Id);

        IEnumerable<string>? allowedChildren = groups
            .Where(g => groupIds.Contains(g.GroupId))
            .SelectMany(g => g.Udis.Select(u => u.ToString()));

        return (allowedChildren?.Any() ?? false) ? allowedChildren.Distinct() : Enumerable.Empty<string>();
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
