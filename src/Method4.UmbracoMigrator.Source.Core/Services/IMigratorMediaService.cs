using Method4.UmbracoMigrator.Source.Core.Models.DataModels;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Method4.UmbracoMigrator.Source.Core.Services
{
    public interface IMigratorMediaService
    {
        List<NodePreview> GetRootNodePreviews();
        List<IMedia> GetRootNodeAndDescendants(int id);
        List<IMedia> GetAllMediaNodes();
        void CopyAllMediaFiles();
    }
}