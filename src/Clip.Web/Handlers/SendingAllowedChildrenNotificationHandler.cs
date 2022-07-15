using Clip.Web.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Clip.Web.Handlers
{
    public class SendingAllowedChildrenNotificationHandler : INotificationHandler<SendingAllowedChildrenNotification>
    {
        private readonly IConfigurationService _configService;

        public SendingAllowedChildrenNotificationHandler(IConfigurationService configService) => _configService = configService;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="notification"></param>
        public void Handle(SendingAllowedChildrenNotification notification)
        {
            if (notification is null || !notification.Children.Any()) return;

            var config = _configService.GetConfigurationModel();

            // remove any types not permitted for this user
            notification.Children = notification.Children.Where(x => !config.AllowedChildren.Any() || config.AllowedChildren.Contains(x.Key));

            // no need to check anything further if no allowed children
            if (!notification.Children.Any()) return;

            // next remove any types where the current count has no capacity for more
            notification.Children = notification.Children
                .Where(x =>
                {
                    int limitFromConfig = config.ContentTypeCounts.FirstOrDefault(y => y.Key == x.Key)?.Max ?? 0;
                    int existingInstancesCount = config.ExistingItemCounts.ContainsKey(x.Key) ? config.ExistingItemCounts[x.Key] : 0;

                    return limitFromConfig == 0 || limitFromConfig > existingInstancesCount;
                });
            }
    }
}
