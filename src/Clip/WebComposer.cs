using Clip.Handlers;
using Clip.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Clip;

/// <summary>
/// Implements the <see cref="IComposer"/> interface to register Clip.
/// </summary>
public sealed class WebComposer : IComposer
{
    /// <inheritdoc/>
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
