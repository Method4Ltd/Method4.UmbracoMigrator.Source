using Method4.UmbracoMigrator.Source.Core.Models.DataModels;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Method4.UmbracoMigrator.Source.Core.Services
{
    public interface IMigratorContentService
    {
        List<NodePreview> GetRootNodePreviews();
        List<IContent> GetRootNodeAndDescendants(int id);
    }
}
