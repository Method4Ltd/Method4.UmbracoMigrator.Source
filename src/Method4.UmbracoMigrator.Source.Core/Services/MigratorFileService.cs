using Method4.UmbracoMigrator.Source.Core.Controllers.api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Xml.Linq;

namespace Method4.UmbracoMigrator.Source.Core.Services
{
    internal class MigratorFileService : IMigratorFileService
    {
        private readonly ILogger<MigratorSourceController> _logger;
        private readonly IMigratorBlobService _migratorBlobService;

        private readonly string migrationSnapshotsPath;
        private readonly string migrationSnapshotTempPath;
        private readonly string migrationSnapshotMediaTempPath;
        private readonly string mediaDiskFolderPath;

        private readonly string contentXmlFilename = "Content.xml";
        private readonly string mediaXmlFilename = "Media.xml";

        public MigratorFileService(ILogger<MigratorSourceController> logger,
            IMigratorBlobService migratorBlobService,
            IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _migratorBlobService = migratorBlobService;

            var appDataPath = Path.Combine(webHostEnvironment.ContentRootPath, "App_Data");

            migrationSnapshotsPath = Path.Combine(appDataPath, "TEMP", "M4Migrator", "snapshots");
            migrationSnapshotTempPath = Path.Combine(appDataPath, "TEMP", "M4Migrator", "temp");
            migrationSnapshotMediaTempPath = Path.Combine(migrationSnapshotTempPath, "Media");

            mediaDiskFolderPath = Path.Combine(webHostEnvironment.WebRootPath, "media");

            if (!Directory.Exists(migrationSnapshotsPath)) { Directory.CreateDirectory(migrationSnapshotsPath); }
        }

        /// <summary>
        /// Delete the Clearing Migration Snapshot temp folder if it exists
        /// </summary>
        public void ClearMigrationSnapshotTempFolder()
        {
            _logger.LogInformation("Clearing Migration Snapshot temp folder");
            if (Directory.Exists(migrationSnapshotTempPath))
            {
                Directory.Delete(migrationSnapshotTempPath, true);
            }
        }

        /// <summary>
        /// Copy the Media from blob storage to the temp migration snapshot folder
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void CopyMediaFilesFromBlob()
        {
            try
            {
                _logger.LogInformation("Starting download Media from blob storage");
                if (!Directory.Exists(migrationSnapshotMediaTempPath)) { Directory.CreateDirectory(migrationSnapshotMediaTempPath); }

                var blobContainerClient = _migratorBlobService.GetBlobContainerClient();
                var blobs = blobContainerClient.GetBlobs();
                var blobCount = 0;
                var blobTotal = blobs.Count();
                foreach (var blobItem in blobs)
                {
                    blobCount++;
                    _logger.LogInformation("Downloading blob {count}/{total}", blobCount, blobTotal);

                    var blobClient = blobContainerClient.GetBlobClient(blobItem.Name);

                    // Create the folder structure for this media item
                    var blobItemFolders = blobItem.Name.Split('/');
                    var newPath = migrationSnapshotMediaTempPath;
                    foreach (var folder in blobItemFolders)
                    {
                        if (Array.IndexOf(blobItemFolders, folder) == (blobItemFolders.Length - 1)) { break; }

                        newPath = Path.Combine(newPath, folder);
                        if (!Directory.Exists(newPath)) { Directory.CreateDirectory(newPath); }
                    }

                    // Download it!
                    //var path = $@"{migrationSnapshotMediaTempPath}\{blobItem.Name}";
                    var path = Path.Combine(migrationSnapshotMediaTempPath, blobItem.Name);
                    var fileStream = File.OpenWrite(path);
                    blobClient.DownloadTo(fileStream);
                    fileStream.Close();
                }
                _logger.LogInformation("Finished downloading Media from blob storage");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed copying media files to the temp folder from blob. {errorMessage}", ex.Message);
                throw new Exception("Failed copying media files to the temp folder from blob", ex);
            }
        }

        /// <summary>
        /// Copy the local Media folder to the temp migration snapshot folder
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void CopyMediaFilesFromDisk()
        {
            try
            {
                _logger.LogInformation("Starting copy of Media from disk");
                CopyDirectory(mediaDiskFolderPath, migrationSnapshotMediaTempPath);
                _logger.LogInformation("Finished copying Media from disk");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed copying media files to the temp folder from disk. {errorMessage}", ex.Message);
                throw new Exception("Failed copying media files to the temp folder from disk", ex);
            }
        }

        /// <summary>
        /// Save the Content.xml to the Migration Snapshot temp folder
        /// </summary>
        /// <param name="xml"></param>
        /// <exception cref="Exception"></exception>
        public void SaveContentXML(XElement xml)
        {
            try
            {
                _logger.LogInformation("Saving Content XML temp file");
                var path = $@"{migrationSnapshotTempPath}\{contentXmlFilename}";
                using (var stream = OpenWrite(migrationSnapshotTempPath, contentXmlFilename))
                {
                    xml.Save(stream);
                    stream.Flush();
                    stream.Close();
                    _logger.LogInformation("Saved Content XML temp file to the path: {path}", path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed saving Content XML temp file. {errorMessage}", ex.Message);
                throw new Exception("Failed saving Content XML temp file", ex);
            }
        }

        /// <summary>
        /// Save the Media.xml to the Migration Snapshot temp folder
        /// </summary>
        /// <param name="xml"></param>
        /// <exception cref="Exception"></exception>
        public void SaveMediaXML(XElement xml)
        {
            try
            {
                _logger.LogInformation("Saving Media XML temp file");
                var path = $@"{migrationSnapshotTempPath}\{mediaXmlFilename}";
                using (var stream = OpenWrite(migrationSnapshotTempPath, mediaXmlFilename))
                {
                    xml.Save(stream);
                    stream.Flush();
                    stream.Close();
                    _logger.LogInformation("Saved Media XML temp file to the path: {path}", path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed saving Media XML temp file. {errorMessage}", ex.Message);
                throw new Exception("Failed saving Media XML temp file", ex);
            }
        }

        /// <summary>
        /// Create a new snapshot zip folder using the files in the Migration Snapshot temp folder
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void CreateMigrationZipFile()
        {
            _logger.LogInformation("Starting creation of .zip file");
            if (!Directory.Exists(migrationSnapshotsPath)) { Directory.CreateDirectory(migrationSnapshotsPath); }

            var zipPath = $@"{migrationSnapshotsPath}\snapshot__{DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss")}.zip";
            ZipFile.CreateFromDirectory(migrationSnapshotTempPath, zipPath);
            _logger.LogInformation("Finished creation of .zip file - \"{zipPath}\"", zipPath);
        }

        public List<FileInfo> GetAllMigrationSnapshotFiles()
        {
            var snapshots = new List<FileInfo>();
            var snapshotPaths = Directory.GetFiles(migrationSnapshotsPath);

            foreach (var snapshotPath in snapshotPaths)
            {
                snapshots.Add(new FileInfo(snapshotPath));
            }

            return snapshots;
        }

        public byte[] GetMigrationSnapshotFile(string fileName)
        {
            byte[]? fileToReturn = null;
            if (!Directory.Exists(migrationSnapshotsPath)) { Directory.CreateDirectory(migrationSnapshotsPath); }
            var snapshotPaths = Directory.GetFiles(migrationSnapshotsPath);
            foreach (var path in snapshotPaths)
            {
                if (path.Contains(fileName) == false) continue;

                fileToReturn = File.ReadAllBytes(path);
            }

            if (fileToReturn == null)
            {
                throw new FileNotFoundException("Could not find '{fileName}'", fileName);
            }

            return fileToReturn;
        }

        public void DeleteAllMigrationSnapshotFiles()
        {
            _logger.LogInformation("Deleting all migration snapshots");
            if (!Directory.Exists(migrationSnapshotsPath)) { Directory.CreateDirectory(migrationSnapshotsPath); }
            var snapshotPaths = Directory.GetFiles(migrationSnapshotsPath);
            foreach (var path in snapshotPaths)
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        public void DeleteMigrationSnapshotFile(string fileName)
        {
            _logger.LogInformation("Deleting migration snapshot {filename}", fileName);
            var snapshotPaths = Directory.GetFiles(migrationSnapshotsPath);
            foreach (var path in snapshotPaths)
            {
                if (path.Contains(fileName) == false) continue;

                if (File.Exists(path)) File.Delete(path);
            }
        }

        private FileStream OpenWrite(string containingFolderPath, string fileName)
        {
            var path = $@"{containingFolderPath}\{fileName}";
            if (File.Exists(path)) { File.Delete(path); }
            if (!Directory.Exists(containingFolderPath)) { Directory.CreateDirectory(containingFolderPath); }
            return File.OpenWrite(path);
        }

        private void CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            if (Directory.Exists(targetDirectory)) { Directory.Delete(targetDirectory, true); }

            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo sourceFile in source.GetFiles())
            {
                _logger.LogInformation(@"Copying {0}\{1}", target.FullName, sourceFile.Name);
                sourceFile.CopyTo(Path.Combine(target.FullName, sourceFile.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}
