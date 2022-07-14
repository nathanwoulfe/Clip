using Clip.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace Clip.Web.Controllers
{
    /// <summary>
    /// </summary>
    [PluginController(Constants.Name)]
    public class ConfigurationController : UmbracoAuthorizedApiController
    {
        private readonly IKeyValueService _keyValueService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        public ConfigurationController(IKeyValueService keyValueService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            _keyValueService = keyValueService ?? throw new ArgumentNullException(nameof(keyValueService));
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backOfficeSecurityAccessor));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetConfig()
        {
            string? settingsStr = _keyValueService.GetValue(Constants.Key);

            if (settingsStr is null) return Ok();

            var settings = JsonConvert.DeserializeObject<IEnumerable<ClipConfigurationModel>>(settingsStr);

            return Ok(new
            {
                settings
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveConfig(IEnumerable<ClipConfigurationModel> model)
        {
            _keyValueService.SetValue(Constants.Key, JsonConvert.SerializeObject(model));
            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAllowedChildren()
        {
            IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

            if (currentUser is null) return BadRequest();

            string? settingsStr = _keyValueService.GetValue(Constants.Key);

            if (settingsStr is null) return Ok(Enumerable.Empty<string>());

            var settings = JsonConvert.DeserializeObject<IEnumerable<ClipConfigurationModel>>(settingsStr);

            if (settings is null || !settings.Any()) return Ok(Enumerable.Empty<string>());

            // need to get all the permitted types for all groups where the current user is a member;
            var groupIds = currentUser.Groups.Select(g => g.Id);
            var allowedChildren = settings.Where(s => groupIds.Contains(s.GroupId))?.SelectMany(s => s.ContentTypeKeys);

            if (allowedChildren is null || !allowedChildren.Any()) return Ok(Enumerable.Empty<string>());

            // allowedChildren is an enumerable of comma-separated strings, so make one big comma-separated string
            // then split the whole thing into an enumerable and remove duplicates
            return Ok(string.Join(",", allowedChildren).Split(',').Distinct());
        }
    }
}
