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
        _ = builder.ManifestFilters().Append<ClipManifestFilter>();

        _ = builder.Services.AddTransient<IConfigurationService, ConfigurationService>();

        _ = builder
            .AddNotificationHandler<UmbracoApplicationStartedNotification, ApplicationStartedHandler>()
            .AddNotificationHandler<SendingAllowedChildrenNotification, SendingAllowedChildrenNotificationHandler>()
            .AddNotificationHandler<ServerVariablesParsingNotification, ServerVariablesParsingNotificationHandler>();
    }
}
