using Clip.Web.Models;
using Clip.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

namespace Clip.Web.Controllers
{
    /// <summary>
    /// </summary>
    [PluginController(Constants.Name)]
    public class ConfigurationController : UmbracoAuthorizedApiController
    {
        private readonly IConfigurationService _configService;

        public ConfigurationController(IConfigurationService configService) =>  _configService = configService;        


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            ClipConfigurationModel settings = _configService.Get();
            return new JsonResult(settings);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Save(ClipConfigurationModel model)
        {
            _configService.Save(model);
            return Ok();
        }
    }
}
