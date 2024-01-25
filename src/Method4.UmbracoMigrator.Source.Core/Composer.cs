using Method4.UmbracoMigrator.Source.Core.Factories;
using Method4.UmbracoMigrator.Source.Core.Serializers;
using Method4.UmbracoMigrator.Source.Core.Services;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;

namespace Method4.UmbracoMigrator.Source.Core
{
    public class Composer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            // Factories
            composition.Register<IPreviewFactory, PreviewFactory>();

            // Serializers
            composition.Register<ISerializer<IContent>, ContentSerializer>();
            composition.Register<ISerializer<IMedia>, MediaSerializer>();

            // Services
            composition.Register<IMigratorBlobService, MigratorBlobService>();
            composition.Register<IMigratorFileService, MigratorFileService>();
            composition.Register<IMigratorMediaService, MigratorMediaService>();
            composition.Register<IMigratorContentService, MigratorContentService>();
        }
    }
}