using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.ViewModels.Inventory;

namespace ERP.Identity.Models.ViewModels.Purchase
{
    public class PurchaseOrderViewModel
    {
        public int Id { get; set; }

        [Display(Name = "PO Number")]
        public string? PONumber { get; set; }

        [Required, Display(Name = "PO Date")]
        public DateTime PODate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Vendor")]
        public int VendorId { get; set; }

        [Display(Name = "Expected Delivery")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        public string? Remarks { get; set; }

        public List<PurchaseOrderItemViewModel> Items { get; set; } = new() { new() };

        public List<VendorOption> Vendors { get; set; } = new();
        public List<ProductOption> Products { get; set; } = new();
    }

    public class PurchaseOrderItemViewModel
    {
        [Required(ErrorMessage = "Select a product")]
        public int ProductId { get; set; }

        [Required, Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public decimal Quantity { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal? SalesTaxRate { get; set; }

        public string? Remarks { get; set; }
    }

    public class PurchaseOrderListItemViewModel
    {
        public int Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public DateTime PODate { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
