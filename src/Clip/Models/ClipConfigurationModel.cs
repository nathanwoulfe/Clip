using System.Text.Json.Serialization;

namespace Clip.Models;

/// <summary>
/// Describes the current configuration.
/// </summary>
public sealed class ClipConfigurationModel
{
    /// <summary>
    /// Gets a list of all types configured as permitted children.
    /// When null, all types are permitted, when empty no types are permitted.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<string>? AllowedChildren { get; set; }

    /// <summary>
    /// Gets a list of all types configured as permitted elements.
    /// When null, all elements are permitted, when empty no elements are permitted.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<string>? AllowedElements { get; set; }

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
        Groups = Enumerable.Empty<GroupConfigurationModel>();
        ContentTypeCounts = Enumerable.Empty<ContentTypeCount>();
        ExistingItemCounts = new();
    }
}
