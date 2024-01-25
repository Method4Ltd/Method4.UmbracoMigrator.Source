using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Method4.UmbracoMigrator.Source.Core.Serializers
{
    internal class MediaSerializer : ContentSerializerBase<IMedia>, ISerializer<IMedia>
    {
        // This is based on the MediaSerializer.cs in uSync8 by Kevin Jump
        // https://github.com/KevinJump/uSync/blob/v8/8.10-main/uSync8.ContentEdition/Serializers/MediaSerializer.cs
        public MediaSerializer(ILocalizationService localizationService,
            IRelationService relationService,
            IEntityService entityService,
            IContentService contentService,
            IMediaService mediaService,
            IDataTypeService dataTypeService)
            : base(localizationService, relationService, entityService, contentService, mediaService, dataTypeService)
        {
        }

        public XElement Serialize(IMedia item)
        {
            var node = InitializeNode(item, "Media");

            var info = SerializeInfo(item);
            var properties = SerializeProperties(item);

            node.Add(info);
            node.Add(properties);

            return node;
        }
    }
}