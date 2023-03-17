using Clip.Executors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Clip;

internal class GetEmptyByKeysActionFilter : IActionFilter
{
    private const string GetEmptyByKeysPath = "/umbraco/backoffice/umbracoapi/content/GetEmptyByKeys";

    private readonly IPermittedTypesFilteringExecutor _permittedTypesFilteringExecutor;

    public GetEmptyByKeysActionFilter(IPermittedTypesFilteringExecutor permittedTypesFilteringExecutor) =>
        _permittedTypesFilteringExecutor = permittedTypesFilteringExecutor;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        string? path = context.HttpContext.Request.Path.Value;

        if (!path?.EndsWith(GetEmptyByKeysPath, StringComparison.CurrentCultureIgnoreCase) ?? true)
        {
            return;
        }

        IDictionary<Guid, ContentItemDisplay>? response = GetResponseBody(context);

        if (response is null)
        {
            return;
        }

        IEnumerable<Guid> filteredKeys = _permittedTypesFilteringExecutor.Execute(response.Values).Select(x => x.ContentTypeKey);

        foreach (KeyValuePair<Guid, ContentItemDisplay> value in response)
        {
            if (!filteredKeys.Contains(value.Key))
            {
                _ = response.Remove(value.Key);
            }
        }
    }

    private static IDictionary<Guid, ContentItemDisplay>? GetResponseBody(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is IDictionary<Guid, ContentItemDisplay> value)
        {
            return value;
        }

        return null;
    }
}
