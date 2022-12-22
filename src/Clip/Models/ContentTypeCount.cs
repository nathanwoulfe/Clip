using Umbraco.Cms.Core;
using static Umbraco.Cms.Core.Constants;

namespace Clip.Models;

/// <summary>
/// Describes the state of a content (ie Document or Media) type.
/// </summary>
public sealed class ContentTypeCount
{
    /// <summary>
    /// Gets the current number of instances of this type.
    /// </summary>
    public int Count { get; set; } = 0;

    /// <summary>
    /// Gets the configured maximum number of instances for this type.
    /// </summary>
    public int Max { get; set; }

    /// <summary>
    /// Gets the Udi for the entity type (either DocumentType or MediaType).
    /// </summary>
    public Udi Udi
    {
        get
        {
            return NodeObjectType == ObjectTypes.DocumentType
                ? new GuidUdi(UdiEntityType.DocumentType, UniqueId)
                : new GuidUdi(UdiEntityType.MediaType, UniqueId);
        }
    }

    /// <summary>
    /// Gets the Guid for the object type.
    /// </summary>
    public Guid NodeObjectType { get; set; }

    /// <summary>
    /// Gets the Guid for the object.
    /// </summary>
    public Guid UniqueId { get; set; }
}
