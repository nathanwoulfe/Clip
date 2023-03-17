using Clip.Models;
using Clip.Repositories;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants;

namespace Clip.Services;

internal sealed class ConfigurationService : IConfigurationService
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly IContentTypeService _contentTypeService;
    private readonly IEntityService _entityService;
    private readonly IConfigurationRepository _configurationRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor"></param>
    /// <param name="eventMessagesFactory"></param>
    /// <param name="contentTypeService"></param>
    /// <param name="entityService"></param>
    public ConfigurationService(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEventMessagesFactory eventMessagesFactory,
        IContentTypeService contentTypeService,
        IEntityService entityService,
        IConfigurationRepository configurationRepository)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _eventMessagesFactory = eventMessagesFactory;
        _contentTypeService = contentTypeService;
        _entityService = entityService;
        _configurationRepository = configurationRepository;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public ClipConfigurationModel Get()
    {
        ClipConfigurationModel? settings = _configurationRepository.Get();

        if (settings is null)
        {
            return new();
        }

        settings.ExistingItemCounts = _configurationRepository.GetItemCounts();

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

        ClipConfigurationModel? settings = _configurationRepository.Get();

        if (settings is null)
        {
            return null;
        }

        // only the first two values are stored
        model.Groups = settings.Groups;
        model.ContentTypeCounts = settings.ContentTypeCounts;

        // these two are generated because they may change between requests
        model.ExistingItemCounts = _configurationRepository.GetItemCounts();
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

        _configurationRepository.Save(model);

        evtMsgs.Add(new("Success", "Content Creation Rules updated", EventMessageType.Success));
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="currentUser"></param>
    /// <param name="groups"></param>
    /// <returns></returns>
    internal (IEnumerable<string>? DocumentTypes, IEnumerable<string>? ElementTypes) GetAllowedChildren(IUser currentUser, IEnumerable<GroupConfigurationModel> groups)
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
}
