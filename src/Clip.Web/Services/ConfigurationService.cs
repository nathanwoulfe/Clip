using Clip.Web.Models;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Clip.Web.Services
{
    public interface IConfigurationService
    {
        ClipConfigurationModel GetConfigurationModel();

        void Save(ClipConfigurationModel model);

        ClipConfigurationModel Get();
    }

    internal class ConfigurationService : IConfigurationService
    {
        private readonly IKeyValueService _keyValueService;
        private readonly IScopeProvider _scopeProvider;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        public ConfigurationService(
            IKeyValueService keyValueService, 
            IScopeProvider scopeProvider, 
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            _keyValueService = keyValueService;
            _scopeProvider = scopeProvider;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ClipConfigurationModel Get()
        {
            ClipConfigurationModel model = new();

            string? settingsStr = _keyValueService.GetValue(Constants.Key);

            if (settingsStr is null) return model;

            var settings = JsonConvert.DeserializeObject<ClipConfigurationModel>(settingsStr) ?? model;
            settings.ExistingItemCounts = GetExistingItemCounts();

            foreach (var c in settings.ContentTypeCounts)
            {
                c.Count = settings.ExistingItemCounts.ContainsKey(c.Key) ? settings.ExistingItemCounts[c.Key] : 0;
            }

            return settings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public ClipConfigurationModel GetConfigurationModel()
        {
            ClipConfigurationModel model = new();

            IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

            if (currentUser is null) return model;

            string? settingsStr = _keyValueService.GetValue(Constants.Key);

            if (settingsStr is null) return model;

            ClipConfigurationModel configModel = JsonConvert.DeserializeObject<ClipConfigurationModel>(settingsStr) ?? model;

            //only the first two values are stored
            model.Groups = configModel.Groups;
            model.ContentTypeCounts = configModel.ContentTypeCounts;
            // these two are generated because they may change between requests
            model.ExistingItemCounts = GetExistingItemCounts();
            model.AllowedChildren = GetAllowedChildren(currentUser, configModel.Groups);

            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Save(ClipConfigurationModel model)
        {
            _keyValueService.SetValue(Constants.Key, JsonConvert.SerializeObject(model));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Dictionary<Guid, int> GetExistingItemCounts()
        {
            string sql = @"SELECT Count(*) as Count, n.uniqueID as [Key]
                FROM umbracoContent C
                INNER JOIN cmsContentType CT ON CT.nodeId = C.contentTypeId
                INNER JOIN umbracoContentVersion V ON V.nodeId = C.nodeId
                INNER JOIN umbracoNode N on N.id = CT.nodeId
                WHERE V.[current] = 1 AND CT.isElement = 0
                GROUP BY N.uniqueID, CT.alias
                ORDER BY N.uniqueID";

            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            IEnumerable<ContentTypeCount> results = scope.Database.Query<ContentTypeCount>(sql);

            return results.ToDictionary(x => x.Key, x => x.Count);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        private static IEnumerable<Guid> GetAllowedChildren(IUser currentUser, IEnumerable<GroupConfigurationModel> groups)
        {
            if (groups is null || !groups.Any()) return Enumerable.Empty<Guid>();

            // need to get all the permitted types for all groups where the current user is a member;
            var groupIds = currentUser.Groups.Select(g => g.Id);
            var allowedChildren = groups.Where(g => groupIds.Contains(g.GroupId))?.SelectMany(g => g.ContentTypeKeys);

            if (allowedChildren is null || !allowedChildren.Any()) return Enumerable.Empty<Guid>();

            // allowedChildren is an enumerable of comma-separated strings, so make one big comma-separated string
            // then split the whole thing into an enumerable and remove duplicates
            return allowedChildren.Distinct();
        }
    }
}
