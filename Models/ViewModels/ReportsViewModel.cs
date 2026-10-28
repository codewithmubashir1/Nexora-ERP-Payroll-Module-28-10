namespace ERP.Identity.Models.ViewModels
{
    public record ReportTypeOption(string Key, string Label, string DateField);

    public class ReportsViewModel
    {
        public List<ReportTypeOption> ReportTypes { get; set; } = new();
    }
}