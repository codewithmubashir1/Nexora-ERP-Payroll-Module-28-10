namespace ERP.Identity.Models.ViewModels
{
    /// <summary>Drives the reusable _ImportModal partial used across every bulk-import feature.</summary>
    public class ImportModalViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public string TemplateUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
