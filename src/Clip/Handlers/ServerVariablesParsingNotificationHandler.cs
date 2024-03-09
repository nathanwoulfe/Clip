using Clip.Controllers;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Clip.Handlers;

internal sealed class ServerVariablesParsingNotificationHandler : INotificationHandler<ServerVariablesParsingNotification>
{
    private readonly IRuntimeState _runtimeState;
    private readonly LinkGenerator _linkGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerVariablesParsingNotificationHandler"/> class.
    /// </summary>
    /// <param name="linkGenerator"></param>
    /// <param name="runtimeState"></param>
    public ServerVariablesParsingNotificationHandler(
        LinkGenerator linkGenerator,
        IRuntimeState runtimeState)
    {
        _linkGenerator = linkGenerator;
        _runtimeState = runtimeState;
    }

    public void Handle(ServerVariablesParsingNotification notification)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        Dictionary<string, object> umbracoSettings =
            notification.ServerVariables["umbracoSettings"] as Dictionary<string, object> ?? new Dictionary<string, object>();

        notification.ServerVariables.Add(Constants.Name, new Dictionary<string, object>
        {
            { "pluginPath", $"{umbracoSettings["appPluginsPath"]}/Clip/backoffice" },
            { "configurationApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<ConfigurationController>(x => x.Get()) ?? string.Empty },
        });
    }
}
