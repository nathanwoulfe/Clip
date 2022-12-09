using Umbraco.Cms.Core;

namespace Clip.Models;

public class ClipConfigurationModel
{
    public IEnumerable<string> AllowedChildren { get; set; }

    public IEnumerable<GroupConfigurationModel> Groups { get; set; }

    public IEnumerable<ContentTypeCount> ContentTypeCounts { get; set; }

    public Dictionary<string, int> ExistingItemCounts { get; set; }

    public ClipConfigurationModel()
    {
        AllowedChildren = Enumerable.Empty<string>();
        Groups = Enumerable.Empty<GroupConfigurationModel>();
        ContentTypeCounts = Enumerable.Empty<ContentTypeCount>();
        ExistingItemCounts = new();
    }
}
