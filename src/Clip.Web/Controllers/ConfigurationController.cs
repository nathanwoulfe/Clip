using Clip.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Packaging;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Clip.Web.Controllers
{
    /// <summary>
    /// </summary>
    [PluginController(Constants.Name)]
    public class ConfigurationController : UmbracoAuthorizedApiController
    {
        private readonly IKeyValueService _keyValueService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IScopeProvider _scopeProvider;

        // stack of deps to spin up a contenttypecontroller instance
        private readonly ICultureDictionary _cultureDictionary;
        private readonly IContentService _contentService;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly IContentTypeService _contentTypeService;
        private readonly IDataTypeService _dataTypeService;
        private readonly IFileService _fileService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly ILogger<ContentTypeController> _logger;
        private readonly PackageDataInstallation _packageDataInstallation;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly EditorValidatorCollection _editorValidatorCollection;
        private readonly IEntityXmlSerializer _serializer;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IUmbracoMapper _umbracoMapper;

        public ConfigurationController(
            IKeyValueService keyValueService,
            IScopeProvider scopeProvider,
            ICultureDictionary cultureDictionary,
            IContentTypeService contentTypeService,
            IMediaTypeService mediaTypeService,
            IMemberTypeService memberTypeService,
            IUmbracoMapper umbracoMapper,
            ILocalizedTextService localizedTextService,
            IEntityXmlSerializer serializer,
            PropertyEditorCollection propertyEditors,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IDataTypeService dataTypeService,
            IShortStringHelper shortStringHelper,
            IFileService fileService,
            ILogger<ContentTypeController> logger,
            IContentService contentService,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IHostingEnvironment hostingEnvironment,
            EditorValidatorCollection editorValidatorCollection,
            PackageDataInstallation packageDataInstallation)
        {
            _keyValueService = keyValueService ?? throw new ArgumentNullException(nameof(keyValueService));
            _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));

            _cultureDictionary = cultureDictionary;
            _contentTypeService = contentTypeService;
            _mediaTypeService = mediaTypeService;
            _memberTypeService = memberTypeService;
            _serializer = serializer;
            _propertyEditors = propertyEditors;
            _contentTypeService = contentTypeService;
            _umbracoMapper = umbracoMapper;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _dataTypeService = dataTypeService;
            _shortStringHelper = shortStringHelper;
            _localizedTextService = localizedTextService;
            _fileService = fileService;
            _logger = logger;
            _contentService = contentService;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _hostingEnvironment = hostingEnvironment;
            _editorValidatorCollection = editorValidatorCollection;
            _packageDataInstallation = packageDataInstallation;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetConfig()
        {
            ClipConfigurationModel model = new();

            string? settingsStr = _keyValueService.GetValue(Constants.Key);            

            if (settingsStr is null) return new JsonResult(model);

            var settings = JsonConvert.DeserializeObject<ClipConfigurationModel>(settingsStr) ?? model;
            settings.ExistingItemCounts = GetContentTypeCounts();

            foreach (var c in settings.ContentTypeCounts)
            {
                c.Count = settings.ExistingItemCounts.ContainsKey(c.Key) ? settings.ExistingItemCounts[c.Key] : 0;
            }

            return new JsonResult(settings);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveConfig(ClipConfigurationModel model)
        {
            _keyValueService.SetValue(Constants.Key, JsonConvert.SerializeObject(model));
            return Ok();
        }


        /// <summary>
        /// Interceptor requests this method, which gets all allowed children from content type controller
        /// then returns the filtered subset only
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(int contentId)
        {
            var controller = new ContentTypeController(
                _cultureDictionary,
                _contentTypeService,
                _mediaTypeService,
                _memberTypeService,
                _umbracoMapper,
                _localizedTextService,
                _serializer,
                _propertyEditors,
                _backofficeSecurityAccessor,
                _dataTypeService,
                _shortStringHelper,
                _fileService,
                _logger,
                _contentService,
                _contentTypeBaseServiceProvider,
                _hostingEnvironment,
                _editorValidatorCollection,
                _packageDataInstallation);

            var allowedTypes = controller.GetAllowedChildren(contentId);
            var config = GetConfigurationModel();

            // remove any types not permitted for this user
            allowedTypes = allowedTypes.Where(x => !config.AllowedChildren.Any() || config.AllowedChildren.Contains(x.Key));

            if (!allowedTypes.Any()) return allowedTypes;

            // next remove any types where the current count has no capacity for more
            var contentTypeCounts = GetContentTypeCounts();
            allowedTypes = allowedTypes
                .Where(x =>
                {
                    int limitFromConfig = config.ContentTypeCounts.FirstOrDefault(y => y.Key == x.Key)?.Count ?? 0;
                    int existingInstancesCount = contentTypeCounts.ContainsKey(x.Key) ? contentTypeCounts[x.Key] : 0;

                    return limitFromConfig == 0 || limitFromConfig < existingInstancesCount;
                });

            return allowedTypes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ClipConfigurationModel GetConfigurationModel()
        {
            ClipConfigurationModel model = new();

            IUser? currentUser = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

            if (currentUser is null) return model;

            string? settingsStr = _keyValueService.GetValue(Constants.Key);

            if (settingsStr is null) return model;

            ClipConfigurationModel configModel = JsonConvert.DeserializeObject<ClipConfigurationModel>(settingsStr) ?? model;

            model.AllowedChildren = GetAllowedChildren(currentUser, configModel.Groups);
            model.Groups = configModel.Groups;
            model.ContentTypeCounts = configModel.ContentTypeCounts;

            return model;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Dictionary<Guid, int> GetContentTypeCounts()
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
    }
}
