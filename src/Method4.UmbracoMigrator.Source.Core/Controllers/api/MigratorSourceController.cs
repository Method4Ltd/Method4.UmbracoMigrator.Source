using Method4.UmbracoMigrator.Source.Core.Factories;
using Method4.UmbracoMigrator.Source.Core.Models.DataModels;
using Method4.UmbracoMigrator.Source.Core.Serializers;
using Method4.UmbracoMigrator.Source.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;

namespace Method4.UmbracoMigrator.Source.Core.Controllers.api
{
    public class MigratorSourceController : UmbracoAuthorizedApiController
    {
        private readonly IMigratorContentService _migratorContentService;
        private readonly IMigratorMediaService _migratorMediaService;
        private readonly IMigratorFileService _migratorFileService;
        private readonly IPreviewFactory _previewFactory;
        private readonly ILogger _logger;

        private readonly ISerializer<IContent> _contentSerializer;
        private readonly ISerializer<IMedia> _mediaSerializer;

        public MigratorSourceController(IMigratorContentService migratorContentService, IMigratorMediaService migratorMediaService, ISerializer<IContent> contentSerializer, IMigratorFileService migratorFileService, ISerializer<IMedia> mediaSerializer, IPreviewFactory previewFactory, ILogger logger)
        {
            _migratorContentService = migratorContentService;
            _migratorMediaService = migratorMediaService;
            _contentSerializer = contentSerializer;
            _migratorFileService = migratorFileService;
            _mediaSerializer = mediaSerializer;
            _previewFactory = previewFactory;
            _logger = logger;
        }

        [HttpGet]
        public IHttpActionResult GetRootContent()
        {
            var rootNodePreviews = _migratorContentService
                .GetRootNodePreviews()
                .ToList();
            rootNodePreviews.Insert(0, new NodePreview()
            {
                Id = -20,
                Name = "Recycle Bin",
                IconAlias = "icon-trash",
                IconColour = "color-black",
                SortOrder = -999
            });
            return Json(rootNodePreviews);
        }

        [HttpGet]
        public IHttpActionResult GetRootMedia()
        {
            var rootNodePreviews = _migratorMediaService
                .GetRootNodePreviews()
                .ToList();
            rootNodePreviews.Insert(0, new NodePreview()
            {
                Id = -21,
                Name = "Recycle Bin",
                IconAlias = "icon-trash",
                IconColour = "color-black",
                SortOrder = -999
            });
            return Json(rootNodePreviews);
        }

        [HttpPost]
        public void CreateMigrationSnapshot(ExtractSettings settings)
        {
            _logger.Info<MigratorSourceController>("Creating migration snapshot");

            // Generate Content XML
            _logger.Info<MigratorSourceController>("Starting generation of content XML");
            var contentNodes = new List<IContent>();
            var contentRootCount = 0;
            foreach (var id in settings.SelectedRootNodes)
            {
                contentRootCount++;
                _logger.Info<MigratorSourceController>("Retrieving descendants for root content nodes {count}/{total} - \"{id}\"", contentRootCount, settings.SelectedRootNodes.Length, id);
                contentNodes.AddRange(_migratorContentService.GetRootNodeAndDescendants(id));
            }

            var contentXmlElements = new List<XElement>();
            var contentSerializeCount = 0;
            foreach (var node in contentNodes)
            {
                contentSerializeCount++;
                _logger.Info<MigratorSourceController>("Serializing content nodes {count}/{total} - \"{name}\"", contentSerializeCount, contentNodes.Count, node.Name.Truncate(20));
                if (settings.IncludeOnlyPublished && node.Published == false) { continue; }
                contentXmlElements.Add(_contentSerializer.Serialize(node));
            }

            var contentNodesXml = new XElement("ContentNodes");
            contentNodesXml.Add(contentXmlElements);

            _migratorFileService.SaveContentXML(contentNodesXml);
            _logger.Info<MigratorSourceController>("Finished generation of content XML");

            // Generate Media XML
            _logger.Info<MigratorSourceController>("Starting generation of media XML");
            var mediaNodes = new List<IMedia>();
            var mediaRootCount = 0;
            foreach (var id in settings.SelectedRootMediaNodes)
            {
                mediaRootCount++;
                _logger.Info<MigratorSourceController>("Retrieving descendants for root media nodes {count}/{total} - \"{id}\"", mediaRootCount, settings.SelectedRootMediaNodes.Length, id);
                mediaNodes.AddRange(_migratorMediaService.GetRootNodeAndDescendants(id));
            }

            var mediaXmlElements = new List<XElement>();
            var mediaSerializeCount = 0;
            foreach (var node in mediaNodes)
            {
                mediaSerializeCount++;
                _logger.Info<MigratorSourceController>("Serializing media nodes {count}/{total} - \"{name}\"", mediaSerializeCount, mediaNodes.Count, node.Name.Truncate(20));
                mediaXmlElements.Add(_mediaSerializer.Serialize(node));
            }

            var mediaNodesXml = new XElement("MediaNodes");
            mediaNodesXml.Add(mediaXmlElements);

            _migratorFileService.SaveMediaXML(mediaNodesXml);
            _logger.Info<MigratorSourceController>("Finished generation of media XML");

            // export images to temp folder
            if (settings.IncludeMediaFiles)
            {
                _migratorMediaService.CopyAllMediaFiles();
            }

            // zip it all up
            _migratorFileService.CreateMigrationZipFile();

            // Delete temp files
            _migratorFileService.ClearMigrationSnapshotTempFolder();

            _logger.Info<MigratorSourceController>("Finished creating migration snapshot");
        }

        [HttpDelete]
        public IHttpActionResult DeleteMigrationSnapshot(string fileName)
        {
            _migratorFileService.DeleteMigrationSnapshotFile(fileName);
            return Json(true);
        }

        [HttpDelete]
        public IHttpActionResult DeleteAllMigrationSnapshots()
        {
            _migratorFileService.DeleteAllMigrationSnapshotFiles();
            return Json(true);
        }

        [HttpGet]
        public HttpResponseMessage DownloadMigrationSnapshot(string fileName)
        {
            _logger.Info<MigratorSourceController>("Downloading migration snapshot {filename}", fileName);
            var snapshotFile = _migratorFileService.GetMigrationSnapshotFile(fileName);
            var response = BuildDownloadResponse(snapshotFile, fileName);
            return response;
        }

        [HttpGet]
        public IHttpActionResult GetAllMigrationSnapshots()
        {
            var snapshotFiles = _migratorFileService.GetAllMigrationSnapshotFiles();
            var snapshotPreviews = _previewFactory.ConvertToFilePreviews(snapshotFiles);
            return Json(snapshotPreviews);
        }

        private HttpResponseMessage BuildDownloadResponse(StreamContent document, string fileName)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = document
            };
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = fileName };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            return response;
        }
    }
}