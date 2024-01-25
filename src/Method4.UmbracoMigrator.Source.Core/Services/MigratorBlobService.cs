using Azure.Storage.Blobs;
using System.Configuration;
using Umbraco.Core.Logging;

namespace Method4.UmbracoMigrator.Source.Core.Services
{
    public class MigratorBlobService : IMigratorBlobService
    {
        private readonly ILogger _logger;

        private readonly string _blobConnectionString;
        private readonly string _blobContainerName;

        public MigratorBlobService(ILogger logger)
        {
            _logger = logger;

            _blobConnectionString = ConfigurationManager.AppSettings["AzureBlobFileSystem.ConnectionString:media"] ?? null;
            _blobContainerName = ConfigurationManager.AppSettings["AzureBlobFileSystem.ContainerName:media"] ?? null;
        }

        /// <summary>
        /// Get the BlobContainerClient object
        /// </summary>
        /// <returns></returns>
        public BlobContainerClient GetBlobContainerClient()
        {
            _logger.Info<MigratorBlobService>("Connecting to blob container {containerName}", _blobContainerName);
            var blobServiceClient = new BlobServiceClient(_blobConnectionString);
            return blobServiceClient.GetBlobContainerClient(_blobContainerName);
        }
    }
}