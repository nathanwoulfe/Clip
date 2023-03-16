using Clip.Executors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Clip.Handlers;

internal sealed class SendingAllowedChildrenNotificationHandler : INotificationHandler<SendingAllowedChildrenNotification>
{
    private readonly IPermittedTypesFilteringExecutor _permittedTypesFilteringExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendingAllowedChildrenNotificationHandler"/> class.
    /// </summary>
    /// <param name="permittedTypesFilteringExecutor"></param>
    public SendingAllowedChildrenNotificationHandler(IPermittedTypesFilteringExecutor permittedTypesFilteringExecutor) =>
        _permittedTypesFilteringExecutor = permittedTypesFilteringExecutor;

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

        // when requesting doc or media, allow all if any of the other type exists
        // this is because a group may have some doc type specified, but no media, so should
        // still be able to select any media
        bool isMediaRequest = notification.UmbracoContext.OriginalRequestUrl.AbsolutePath.Contains("/umbracoapi/mediatype", StringComparison.InvariantCultureIgnoreCase);

        notification.Children = _permittedTypesFilteringExecutor.Execute(notification.Children, isMediaRequest);
    }
}
