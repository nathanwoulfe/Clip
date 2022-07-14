using Clip.Web.Controllers;
using Clip.Web.Models;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Clip.Web
{
    public class ServerVariablesParsingNotificationHandler : INotificationHandler<ServerVariablesParsingNotification>
    {
        private readonly IRuntimeState _runtimeState;
        private readonly LinkGenerator _linkGenerator;
        private readonly IKeyValueService _keyValueService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        public ServerVariablesParsingNotificationHandler(
            LinkGenerator linkGenerator, 
            IRuntimeState runtimeState, 
            IKeyValueService keyValueService, 
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
            _runtimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
            _keyValueService = keyValueService ?? throw new ArgumentNullException(nameof(keyValueService));
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backOfficeSecurityAccessor));
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
                { "configurationApiBaseUrl", _linkGenerator.GetUmbracoApiServiceBaseUrl<ConfigurationController>(x => x.GetConfig()) ?? "" },
                { "allowedChildren", GetAllowedChildren() }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetAllowedChildren()
        {
            IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

            if (currentUser is null) return Enumerable.Empty<string>();

            string? settingsStr = _keyValueService.GetValue(Constants.Key);

            if (settingsStr is null) return Enumerable.Empty<string>();

            var settings = JsonConvert.DeserializeObject<IEnumerable<ClipConfigurationModel>>(settingsStr);

            if (settings is null || !settings.Any()) return Enumerable.Empty<string>();

            // need to get all the permitted types for all groups where the current user is a member;
            var groupIds = currentUser.Groups.Select(g => g.Id);
            var allowedChildren = settings.Where(s => groupIds.Contains(s.GroupId))?.SelectMany(s => s.ContentTypeKeys);

            if (allowedChildren is null || !allowedChildren.Any()) return Enumerable.Empty<string>();

            // allowedChildren is an enumerable of comma-separated strings, so make one big comma-separated string
            // then split the whole thing into an enumerable and remove duplicates
            return string.Join(",", allowedChildren).Split(',').Distinct();
        }
    }
}
