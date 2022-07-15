namespace Clip.Web.Models
{
    public class GroupConfigurationModel
    {
        public int GroupId { get; set; }
        public IEnumerable<Guid> ContentTypeKeys { get; set; } = new List<Guid>();
    }
}
