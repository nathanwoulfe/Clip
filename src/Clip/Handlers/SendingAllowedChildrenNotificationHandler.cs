using Clip.Models;
using Clip.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants;

namespace Clip.Handlers;

internal sealed class SendingAllowedChildrenNotificationHandler : INotificationHandler<SendingAllowedChildrenNotification>
{
    private readonly IConfigurationService _configService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendingAllowedChildrenNotificationHandler"/> class.
    /// </summary>
    /// <param name="configService"></param>
    public SendingAllowedChildrenNotificationHandler(IConfigurationService configService) => _configService = configService;

    /// <summary>
    ///
    /// </summary>
    /// <param name="notification"></param>
    public void Handle(SendingAllowedChildrenNotification notification)
    {
        if (notification is null || !notification.Children.Any())
        {
            return;
        }

        ClipConfigurationModel config = _configService.GetConfigurationModel();

        // when requesting doc or media, allow all if any of the other type exists
        // this is because a group may have some doc type specified, but no media, so should
        // still be able to select any media
        bool isMediaRequest = notification.UmbracoContext.OriginalRequestUrl.AbsolutePath.Contains("/umbracoapi/mediatype", StringComparison.InvariantCultureIgnoreCase);

        // remove any types not permitted for this user
        notification.Children = config.AllowedChildren.Any()
            ? notification.Children.Where(c => IsValidChild(c, config, isMediaRequest))
            : Enumerable.Empty<ContentTypeBasic>();

        if (!notification.Children.Any())
        {
            return;
        }

        // next remove any types where the current count has no capacity for more
        notification.Children = notification.Children.Where(x => HasCapacity(x, config));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="c"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    private static bool HasCapacity(ContentTypeBasic c, ClipConfigurationModel config)
    {
        int limitFromConfig = config.ContentTypeCounts.FirstOrDefault(y => y.Udi == c.Udi)?.Max ?? 0;
        string? udiString = c.Udi?.ToString();
        int existingInstancesCount = udiString is not null && config.ExistingItemCounts.ContainsKey(udiString) ? config.ExistingItemCounts[udiString] : 0;

        return limitFromConfig == 0 || limitFromConfig > existingInstancesCount;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="c"></param>
    /// <param name="config"></param>
    /// <param name="isMediaRequest"></param>
    /// <returns></returns>
    private static bool IsValidChild(ContentTypeBasic c, ClipConfigurationModel config, bool isMediaRequest)
    {
        // if child is included in allowed children
        if (c.Udi is not null && config.AllowedChildren.Contains(c.Udi.ToString()))
        {
            return true;
        }

        // if getting media types, and this is type is not in allowed children, type is not permitted
        // would have returned true above if it were listed
        if (isMediaRequest && config.AllowedChildren.Any(x => x.Contains(UdiEntityType.MediaType)))
        {
            return false;
        }

        // if getting document types, and this is type is not in allowed children, type is not permitted
        // would have returned true above if it were listed
        if (!isMediaRequest && config.AllowedChildren.Any(x => x.Contains(UdiEntityType.DocumentType)))
        {
            return false;
        }

        return true;
    }
}
