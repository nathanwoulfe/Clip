using Umbraco.Cms.Core;

namespace Clip.Models;

/// <summary>
/// Describes the permitted content types to be created by the group.
/// </summary>
public sealed class GroupConfigurationModel
{
    /// <summary>
    /// Gets the group ID.
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// Gets the collection of Udis the group is permitted to create.
    /// </summary>
    public IEnumerable<Udi> Udis { get; set; } = Enumerable.Empty<Udi>();
}
