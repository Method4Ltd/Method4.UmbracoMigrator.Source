namespace Method4.UmbracoMigrator.Source.Core.Models.DataModels
{
    public class ExtractSettings
    {
        public int[] SelectedRootNodes { get; set; }
        public int[] SelectedRootMediaNodes { get; set; }
        public bool IncludeMediaFiles { get; set; }
        public bool IncludeOnlyPublished { get; set; }
    }
}