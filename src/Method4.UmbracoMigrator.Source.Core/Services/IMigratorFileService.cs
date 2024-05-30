using System.Xml.Linq;

namespace Method4.UmbracoMigrator.Source.Core.Services
{
    public interface IMigratorFileService
    {
        void ClearMigrationSnapshotTempFolder();
        void SaveContentXML(XElement xml);
        void SaveMediaXML(XElement xml);
        void CopyMediaFilesFromDisk();
        void CopyMediaFilesFromBlob();
        void CreateMigrationZipFile();
        List<FileInfo> GetAllMigrationSnapshotFiles();
        byte[] GetMigrationSnapshotFile(string fileName);
        void DeleteAllMigrationSnapshotFiles();
        void DeleteMigrationSnapshotFile(string fileName);
    }
}