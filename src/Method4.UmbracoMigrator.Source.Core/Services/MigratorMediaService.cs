using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Method4.UmbracoMigrator.Source.Core.Factories;
using Method4.UmbracoMigrator.Source.Core.Models.DataModels;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Method4.UmbracoMigrator.Source.Core.Services
{
    public class MigratorMediaService : IMigratorMediaService
    {
        private readonly IMediaService _mediaService;
        private readonly IMigratorFileService _migratorFileService;
        private readonly IPreviewFactory _nodePreviewFactory;

        private readonly bool isBlob;

        public MigratorMediaService(IMediaService mediaService, IMigratorFileService migratorFileService, IPreviewFactory nodePreviewFactory)
        {
            _mediaService = mediaService;
            _migratorFileService = migratorFileService;
            _nodePreviewFactory = nodePreviewFactory;

            string blobConnectionString = ConfigurationManager.AppSettings["AzureBlobFileSystem.ConnectionString:media"];
            isBlob = !blobConnectionString.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Get all root media nodes as preview models
        /// </summary>
        /// <returns></returns>
        public List<NodePreview> GetRootNodePreviews()
        {
            var rootNodes = _mediaService.GetRootMedia().ToList();
            return _nodePreviewFactory.ConvertToNodePreviews(rootNodes);
        }

        /// <summary>
        /// Return the root node with all of it's descendants.
        /// Ordered by their Level.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<IMedia> GetRootNodeAndDescendants(int id)
        {
            var rootNode = _mediaService.GetRootMedia().FirstOrDefault(x => x.Id == id);
            if (rootNode == null)
            {
                //throw?
                return new List<IMedia>();
            }

            var descendants = new List<IMedia> { rootNode };
            descendants.AddRange(GetAllDescendantNodes(rootNode));
            descendants.OrderByDescending(x => x.Level);

            return descendants;
        }

        /// <summary>
        /// Return all media nodes.
        /// </summary>
        /// <returns></returns>
        public List<IMedia> GetAllMediaNodes()
        {
            var rootNodes = _mediaService.GetRootMedia();
            if (rootNodes == null)
            {
                //throw?
                return new List<IMedia>();
            }

            var descendants = new List<IMedia>();
            descendants.AddRange(rootNodes);

            foreach (var rootNode in rootNodes)
            {
                descendants.AddRange(GetAllDescendantNodes(rootNode));
            }

            descendants.OrderByDescending(x => x.Level);

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

        private List<IMedia> GetAllDescendantNodes(IMedia parent)
        {
            var pageIndex = 0;
            var pageSize = 100;
            var children = new List<IMedia>();

            if (_mediaService.HasChildren(parent.Id) == false) return children;

            // Get all of the children of the parent node
            children = _mediaService.GetPagedChildren(parent.Id, pageIndex, pageSize, out long totalRecords).ToList();
            while (children.Count < totalRecords)
            {
                pageIndex++;
                children.AddRange(_mediaService.GetPagedChildren(parent.Id, pageIndex, pageSize, out long totalRecords2).ToList());
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