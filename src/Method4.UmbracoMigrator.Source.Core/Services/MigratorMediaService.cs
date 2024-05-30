using Method4.UmbracoMigrator.Source.Core.Factories;
using Method4.UmbracoMigrator.Source.Core.Models.DataModels;
using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Method4.UmbracoMigrator.Source.Core.Services
{
    public class MigratorMediaService : IMigratorMediaService
    {
        private readonly IMediaService _mediaService;
        private readonly IMigratorFileService _migratorFileService;
        private readonly IPreviewFactory _nodePreviewFactory;

        private readonly bool isBlob;

        public MigratorMediaService(IMediaService mediaService, 
            IMigratorFileService migratorFileService, 
            IPreviewFactory nodePreviewFactory,
            IConfiguration config)
        {
            _mediaService = mediaService;
            _migratorFileService = migratorFileService;
            _nodePreviewFactory = nodePreviewFactory;

            var blobConnectionString = config.GetValue<string>("Umbraco:Storage:AzureBlob:Media:ConnectionString") ?? null;
            isBlob = !blobConnectionString.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Get all root media nodes as preview models
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NodePreview> GetRootNodePreviews()
        {
            var rootNodes = _mediaService.GetRootMedia();
            return _nodePreviewFactory.ConvertToNodePreviews(rootNodes);
        }

        /// <summary>
        /// Return the root node with all of it's descendants.
        /// Ordered by their Level.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<IMedia> GetRootNodeAndDescendants(int id)
        {
            if (id == -21) // Recycle Bin
            {
                var trashedNodes = _mediaService.GetPagedMediaInRecycleBin(0, int.MaxValue, out var totalInBin);
                return trashedNodes;
            }

            var rootNode = _mediaService.GetRootMedia().FirstOrDefault(x => x.Id == id);
            if (rootNode == null)
            {
                //throw?
                return new List<IMedia>();
            }

            var descendants = new List<IMedia> { rootNode };
            descendants.AddRange(GetAllDescendantNodes(rootNode));

            return descendants;
        }

        /// <summary>
        /// Copy all of the Media files to the temp folder
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void CopyAllMediaFiles()
        {
            if (isBlob)
            {
                _migratorFileService.CopyMediaFilesFromBlob();
            }
            else
            {
                _migratorFileService.CopyMediaFilesFromDisk();
            }
        }

        private IEnumerable<IMedia> GetAllDescendantNodes(IMedia parent)
        {
            var pageIndex = 0;
            const int pageSize = 100;
            var children = new List<IMedia>();

            if (_mediaService.HasChildren(parent.Id) == false) return children;

            // Get all of the children of the parent node
            children.AddRange(_mediaService.GetPagedChildren(parent.Id, pageIndex, pageSize, out var totalRecords));
            while (children.Count < totalRecords)
            {
                pageIndex++;
                children.AddRange(_mediaService.GetPagedChildren(parent.Id, pageIndex, pageSize, out _));
            }

            var descendants = new List<IMedia>();
            descendants.AddRange(children);
            // Get the children of the children
            foreach (var child in children)
            {
                if (_mediaService.HasChildren(child.Id) == false) continue;

                descendants.AddRange(GetAllDescendantNodes(child));
            }

            return descendants;
        }
    }
}