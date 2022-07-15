namespace Clip.Web.Models
{
    public class ClipConfigurationModel
    {
        public IEnumerable<Guid> AllowedChildren { get; set; }

        public IEnumerable<GroupConfigurationModel> Groups { get; set; }

        public IEnumerable<ContentTypeCount> ContentTypeCounts { get; set; }

        public Dictionary<Guid, int> ExistingItemCounts { get; set; }

        public ClipConfigurationModel()
        {
            AllowedChildren = Enumerable.Empty<Guid>();
            Groups = Enumerable.Empty<GroupConfigurationModel>();
            ContentTypeCounts = Enumerable.Empty<ContentTypeCount>();
            ExistingItemCounts = new();
        }
    }
}
