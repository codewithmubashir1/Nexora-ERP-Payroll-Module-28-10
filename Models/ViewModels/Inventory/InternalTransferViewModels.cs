using System.ComponentModel.DataAnnotations;

namespace ERP.Identity.Models.ViewModels.Inventory
{
    public record ProductOption(int Id, string Name, string Sku, decimal CostPrice = 0, decimal SalePrice = 0, string? PackingUnitName = null, decimal? PackingQuantity = null, string? Code = null);

    public class InternalTransferViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Transfer No.")]
        public string? TransferNo { get; set; }

        [Required, Display(Name = "Transfer Date")]
        public DateTime TransferDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "From Location")]
        public int FromLocationId { get; set; }

        [Required, Display(Name = "To Location")]
        public int ToLocationId { get; set; }

        public string? Remarks { get; set; }

        public List<InternalTransferItemViewModel> Items { get; set; } = new() { new() };

        public List<LocationOption> Locations { get; set; } = new();
        public List<ProductOption> Products { get; set; } = new();
    }

    public class InternalTransferItemViewModel
    {
        [Required(ErrorMessage = "Select a product")]
        public int ProductId { get; set; }

        [Required, Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public decimal Quantity { get; set; }

        public string? Remarks { get; set; }
    }

    public class InternalTransferListItemViewModel
    {
        public int Id { get; set; }
        public string TransferNo { get; set; } = string.Empty;
        public DateTime TransferDate { get; set; }
        public string FromLocation { get; set; } = string.Empty;
        public string ToLocation { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
