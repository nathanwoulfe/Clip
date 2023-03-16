using Umbraco.Cms.Core.Models.ContentEditing;

namespace Clip.Executors;

public interface IPermittedTypesFilteringExecutor
{
    IEnumerable<ContentTypeBasic> Execute(IEnumerable<ContentTypeBasic> items, bool isMediaRequest = false);
    IEnumerable<ContentItemDisplay> Execute(IEnumerable<ContentItemDisplay> items, bool isMediaRequest = false);
}
