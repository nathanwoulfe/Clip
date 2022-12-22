namespace Clip.Models;

/// <summary>
/// Describes the current configuration.
/// </summary>
public sealed class ClipConfigurationModel
{
    /// <summary>
    /// Gets a list of all types configured as permitted children.
    /// </summary>
    public IEnumerable<string> AllowedChildren { get; set; }

    /// <summary>
    /// Gets a collection of groups and their permitted types.
    /// </summary>
    public IEnumerable<GroupConfigurationModel> Groups { get; set; }

    /// <summary>
    /// How is this different to ExistingItemCounts? Could be combined.
    /// </summary>
    public IEnumerable<ContentTypeCount> ContentTypeCounts { get; set; }

    /// <summary>
    /// Gets the counts of existing nodes, keyed by the node type.
    /// </summary>
    public Dictionary<string, int> ExistingItemCounts { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClipConfigurationModel"/> class.
    /// </summary>
    public ClipConfigurationModel()
    {
        AllowedChildren = Enumerable.Empty<string>();
        Groups = Enumerable.Empty<GroupConfigurationModel>();
        ContentTypeCounts = Enumerable.Empty<ContentTypeCount>();
        ExistingItemCounts = new();
    }
}
