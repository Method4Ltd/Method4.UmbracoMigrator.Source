using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.ModelBinders;

namespace Method4.UmbracoMigrator.Source.Core.Controllers.Trees
{
    [Tree(Constants.Applications.Settings, "migratorSource",
        TreeGroup = "migration",
        TreeTitle = "migratorSource", 
        SortOrder = 40)]
    [PluginController("method4UmbracoMigratorSource")]
    public class MigratorTreeController : TreeController
    {
        public MigratorTreeController(
            ILocalizedTextService localizedTextService,
            UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
            IEventAggregator eventAggregator)
            : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
        { }

        protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            root.Value.RoutePath = $"{this.SectionAlias}/migratorSource/dashboard";
            root.Value.Icon = "icon-truck";
            root.Value.HasChildren = false;
            root.Value.MenuUrl = null;

            return root.Value;
        }

        protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings)
        {
            return null;
        }

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings)
        {
            return new TreeNodeCollection();
        }
    }
}