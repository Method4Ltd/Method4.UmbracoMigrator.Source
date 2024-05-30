using Method4.UmbracoMigrator.Source.Core.Models.DataModels;
using Umbraco.Cms.Core.Models;

namespace Method4.UmbracoMigrator.Source.Core.Services
{
    public interface IMigratorMediaService
    {
        IEnumerable<NodePreview> GetRootNodePreviews();
        IEnumerable<IMedia> GetRootNodeAndDescendants(int id);
        void CopyAllMediaFiles();
    }
}