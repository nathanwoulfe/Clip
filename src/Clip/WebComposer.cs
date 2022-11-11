using Clip.Handlers;
using Clip.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Clip;

public class WebComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.ManifestFilters().Append<ClipManifestFilter>();

        builder.Services.AddTransient<IConfigurationService, ConfigurationService>();

        builder
            .AddNotificationHandler<UmbracoApplicationStartedNotification, ApplicationStartedHandler>()
            .AddNotificationHandler<SendingAllowedChildrenNotification, SendingAllowedChildrenNotificationHandler>()
            .AddNotificationHandler<ServerVariablesParsingNotification, ServerVariablesParsingNotificationHandler>();
    }
}
