using Clip.Executors;
using Clip.Handlers;
using Clip.Repositories;
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
        _ = builder.Services.AddTransient<IConfigurationRepository, ConfigurationRepository>();
        _ = builder.Services.AddTransient<IPermittedTypesFilteringExecutor, PermittedTypesFilteringExecutor>();
        _ = builder.Services.AddMvc(options => options.Filters.Add<GetEmptyByKeysActionFilter>());

        _ = builder
            .AddNotificationHandler<UmbracoApplicationStartedNotification, ApplicationStartedHandler>()
            .AddNotificationHandler<SendingAllowedChildrenNotification, SendingAllowedChildrenNotificationHandler>()
            .AddNotificationHandler<ServerVariablesParsingNotification, ServerVariablesParsingNotificationHandler>();
    }
}
