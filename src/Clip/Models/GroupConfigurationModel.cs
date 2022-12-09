using Umbraco.Cms.Core;

namespace Clip.Models;

public class GroupConfigurationModel
{
    public int GroupId { get; set; }
    public IEnumerable<Udi> Udis { get; set; } = Enumerable.Empty<Udi>();
}
