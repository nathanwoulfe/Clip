using Clip.Models;
using Clip.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Clip.Controllers;

/// <summary>
/// </summary>
[PluginController(Constants.Name)]
public sealed class ConfigurationController : UmbracoAuthorizedApiController
{
    private readonly IConfigurationService _configService;
    private readonly ILocalizedTextService _localizedTextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationController"/> class.
    /// </summary>
    /// <param name="configService"></param>
    /// <param name="localizedTextService"></param>
    public ConfigurationController(IConfigurationService configService, ILocalizedTextService localizedTextService)
    {
        _configService = configService;
        _localizedTextService = localizedTextService;
    }


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

        BackOfficeNotification notification = new()
        {
            NotificationType = NotificationStyle.Success,
            Header = _localizedTextService.Localize("general", "success"),
            Message = _localizedTextService.Localize("clip", "rulesUpdated"),
        };

        return Ok(new
        {
            notifications = new[] { notification },
        });
    }
}
