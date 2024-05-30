using Method4.UmbracoMigrator.Source.Core.Models.DataModels;
using Umbraco.Cms.Core.Models;

namespace Method4.UmbracoMigrator.Source.Core.Factories
{
    public interface IPreviewFactory
    {
        NodePreview ConvertToNodePreview(IContentBase item);
        List<NodePreview> ConvertToNodePreviews(IEnumerable<IContent> items);
        List<NodePreview> ConvertToNodePreviews(IEnumerable<IMedia> items);
        FilePreview ConvertToFilePreview(FileInfo item);
        List<FilePreview> ConvertToFilePreviews(IEnumerable<FileInfo> items);
    }

    public class PreviewFactory : IPreviewFactory
    {
        public PreviewFactory() { }

        public FilePreview ConvertToFilePreview(FileInfo item)
        {
            var filePreview = new FilePreview()
            {
                FileName = item.Name,
                CreateDate = item.CreationTime,
                SizeBytes = item.Length
            };
            return filePreview;
        }

        public List<FilePreview> ConvertToFilePreviews(IEnumerable<FileInfo> items)
        {
            var filePreviews = new List<FilePreview>();

            if (items?.Any() == true)
            {
                foreach (var item in items)
                {
                    filePreviews.Add(ConvertToFilePreview(item));
                }
            }
            return filePreviews;
        }

        public NodePreview ConvertToNodePreview(IContentBase item)
        {
            var nodePreview = new NodePreview()
            {
                Id = item.Id,
                Name = item.Name,
                SortOrder = item.SortOrder,
                IconAlias = item.ContentType.Icon,
                IconColour = null
            };
            return nodePreview;
        }

        public List<NodePreview> ConvertToNodePreviews(IEnumerable<IContent> items)
        {
            var nodePreviews = new List<NodePreview>();

            if (items?.Any() == true)
            {
                foreach (var item in items)
                {
                    nodePreviews.Add(ConvertToNodePreview(item));
                }
            }
            return nodePreviews;
        }

        public List<NodePreview> ConvertToNodePreviews(IEnumerable<IMedia> items)
        {
            var nodePreviews = new List<NodePreview>();

            if (items?.Any() == true)
            {
                foreach (var item in items)
                {
                    nodePreviews.Add(ConvertToNodePreview(item));
                }
            }
            return nodePreviews;
        }
    }
}