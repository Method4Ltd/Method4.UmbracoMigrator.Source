using Method4.UmbracoMigrator.Source.Core.Factories;
using Method4.UmbracoMigrator.Source.Core.Serializers;
using Method4.UmbracoMigrator.Source.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;

namespace Method4.UmbracoMigrator.Source.Core
{
    public class Composer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // Factories
            builder.Services.AddTransient<IPreviewFactory, PreviewFactory>();

            // Serializers
            builder.Services.AddTransient<ISerializer<IContent>, ContentSerializer>();
            builder.Services.AddTransient<ISerializer<IMedia>, MediaSerializer>();

            // Services
            builder.Services.AddTransient<IMigratorBlobService, MigratorBlobService>();
            builder.Services.AddTransient<IMigratorFileService, MigratorFileService>();
            builder.Services.AddTransient<IMigratorMediaService, MigratorMediaService>();
            builder.Services.AddTransient<IMigratorContentService, MigratorContentService>();
        }
    }
}