using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.ModelBinders;
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace Clip.Web.Controllers
{
    [Tree(UmbConstants.Applications.Settings, Constants.TreeAlias, SortOrder = 20, TreeGroup = UmbConstants.Trees.Groups.Settings)]
    [PluginController(Constants.Name)]
    public class ClipTreeController : TreeController
    {
        public ClipTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IEventAggregator eventAggregator)
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        {
        }

        protected override ActionResult<TreeNode?> CreateRootNode(FormCollection queryStrings)
        {
            ActionResult<TreeNode?> rootResult = base.CreateRootNode(queryStrings);

            if (rootResult.Result is not null) return rootResult;            

            TreeNode? root = rootResult.Value;

            if (root is null) return root;

            root.RoutePath = $"{UmbConstants.Applications.Settings}/{Constants.TreeAlias}/overview";
            root.Icon = UmbConstants.Icons.Folder;
            root.HasChildren = false;
            root.MenuUrl = null;

            return root;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings)
        {
            throw new NotImplementedException();
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings)
        {
            throw new NotImplementedException();
        }
    }
}