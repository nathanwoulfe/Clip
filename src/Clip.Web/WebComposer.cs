using Clip.Web.Handlers;
using Clip.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Clip.Web
{
    public class WebComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddTransient<IConfigurationService, ConfigurationService>();

            builder
                .AddNotificationHandler<SendingAllowedChildrenNotification, SendingAllowedChildrenNotificationHandler>()
                .AddNotificationHandler<ServerVariablesParsingNotification, ServerVariablesParsingNotificationHandler>();
        }
    }
}
