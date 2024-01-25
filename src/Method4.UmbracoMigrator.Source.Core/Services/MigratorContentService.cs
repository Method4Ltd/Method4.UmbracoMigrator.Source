using Method4.UmbracoMigrator.Source.Core.Factories;
using Method4.UmbracoMigrator.Source.Core.Models.DataModels;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Method4.UmbracoMigrator.Source.Core.Services
{
    public class MigratorContentService : IMigratorContentService
    {
        private readonly IContentService _contentService;
        private readonly IPreviewFactory _nodePreviewFactory;

        public MigratorContentService(IContentService contentService, IPreviewFactory nodePreviewFactory)
        {
            _contentService = contentService;
            _nodePreviewFactory = nodePreviewFactory;
        }

        /// <summary>
        /// Get all root content nodes as preview models
        /// </summary>
        /// <returns></returns>
        public List<NodePreview> GetRootNodePreviews()
        {
            var rootNodes = _contentService.GetRootContent().ToList();
            return _nodePreviewFactory.ConvertToNodePreviews(rootNodes);
        }

        /// <summary>
        /// Return the root node with all of it's descendants.
        /// Ordered by their Level.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<IContent> GetRootNodeAndDescendants(int id)
        {
            var rootNode = _contentService.GetRootContent().FirstOrDefault(x => x.Id == id);
            if (rootNode == null)
            {
                //throw?
                return new List<IContent>();
            }

            var descendants = new List<IContent> { rootNode };
            descendants.AddRange(GetAllDescendantNodes(rootNode));
            descendants.OrderByDescending(x => x.Level);

            return descendants;
        }

        private List<IContent> GetAllDescendantNodes(IContent parent)
        {
            var pageIndex = 0;
            var pageSize = 100;
            var children = new List<IContent>();

            if (_contentService.HasChildren(parent.Id) == false) return children;

            // Get all of the children of the parent node
            children = _contentService.GetPagedChildren(parent.Id, pageIndex, pageSize, out long totalRecords).ToList();
            while (children.Count < totalRecords)
            {
                pageIndex++;
                children.AddRange(_contentService.GetPagedChildren(parent.Id, pageIndex, pageSize, out long totalRecords2).ToList());
            }

            var descendants = new List<IContent>();
            descendants.AddRange(children);
            // Get the children of the children
            foreach (var child in children)
            {
                if (_contentService.HasChildren(child.Id) == false) continue;

                descendants.AddRange(GetAllDescendantNodes(child));
            }

            return descendants;
        }
    }
}