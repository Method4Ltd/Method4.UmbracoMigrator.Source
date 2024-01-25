using Azure.Storage.Blobs;

namespace Method4.UmbracoMigrator.Source.Core.Services
{
    public interface IMigratorBlobService
    {
        BlobContainerClient GetBlobContainerClient();
    }
}