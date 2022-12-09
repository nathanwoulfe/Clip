using Umbraco.Cms.Core;
using static Umbraco.Cms.Core.Constants;

namespace Clip.Models;

public class ContentTypeCount
{
    public int Count { get; set; } = 0;
    public int Max { get; set; }
    public Udi Udi
    {
        get
        {
            return NodeObjectType == ObjectTypes.DocumentType
                ? new GuidUdi(UdiEntityType.DocumentType, UniqueId)
                : new GuidUdi(UdiEntityType.MediaType, UniqueId);
        }
    }
    public Guid NodeObjectType { get; set; }
    public Guid UniqueId { get; set; }

}
