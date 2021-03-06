using Clip.Web.Controllers;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Clip.Web.Handlers
{
    public class ServerVariablesParsingNotificationHandler : INotificationHandler<ServerVariablesParsingNotification>
    {
        private readonly IRuntimeState _runtimeState;
        private readonly LinkGenerator _linkGenerator;

        public ServerVariablesParsingNotificationHandler(
            LinkGenerator linkGenerator,
            IRuntimeState runtimeState)
        {
            _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
            _runtimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
        }

        public void Handle(ServerVariablesParsingNotification notification)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
                return;

            Dictionary<string, object> umbracoSettings =
                notification.ServerVariables["umbracoSettings"] as Dictionary<string, object> ?? new Dictionary<string, object>();

            notification.ServerVariables.Add("Clip", new Dictionary<string, object>
            {
                { "pluginPath", $"{umbracoSettings["appPluginsPath"]}/Clip/Backoffice" },
                { "configurationApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<ConfigurationController>(x => x.Get()) ?? "" },
            });
        }        
    }
}
