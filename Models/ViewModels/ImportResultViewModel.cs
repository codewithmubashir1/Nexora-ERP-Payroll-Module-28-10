namespace ERP.Identity.Models.ViewModels
{
    public class ImportResultViewModel
    {
        public int TotalRows { get; set; }
        public int CreatedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();

        public bool Success => ErrorCount == 0;

        public string Summary =>
            $"{CreatedCount} created, {UpdatedCount} updated, {ErrorCount} skipped out of {TotalRows} rows.";
    }
}
