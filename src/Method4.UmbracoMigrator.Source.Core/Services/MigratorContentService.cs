using Method4.UmbracoMigrator.Source.Core.Factories;
using Method4.UmbracoMigrator.Source.Core.Models.DataModels;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

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
        public IEnumerable<NodePreview> GetRootNodePreviews()
        {
            var rootNodes = _contentService.GetRootContent();
            return _nodePreviewFactory.ConvertToNodePreviews(rootNodes);
        }

        /// <summary>
        /// Return the root node with all of it's descendants.
        /// Ordered by their Level.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<IContent> GetRootNodeAndDescendants(int id)
        {
            if (id == -20) // Recycle Bin
            {
                var trashedNodes = _contentService.GetPagedContentInRecycleBin(0, int.MaxValue, out var totalInBin);
                return trashedNodes;
            }

            var rootNode = _contentService.GetRootContent().FirstOrDefault(x => x.Id == id);
            if (rootNode == null)
            {
                //throw?
                return new List<IContent>();
            }

            var descendants = new List<IContent> { rootNode };
            descendants.AddRange(GetAllDescendantNodes(rootNode));

            return descendants;
        }

        private IEnumerable<IContent> GetAllDescendantNodes(IContent parent)
        {
            var pageIndex = 0;
            const int pageSize = 100;
            var children = new List<IContent>();

            if (_contentService.HasChildren(parent.Id) == false) return children;

            // Get all of the children of the parent node
            children.AddRange(_contentService.GetPagedChildren(parent.Id, pageIndex, pageSize, out var totalRecords));
            while (children.Count < totalRecords)
            {
                pageIndex++;
                children.AddRange(_contentService.GetPagedChildren(parent.Id, pageIndex, pageSize, out _));
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