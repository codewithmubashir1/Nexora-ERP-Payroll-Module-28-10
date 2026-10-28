using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.ViewModels.Inventory;

namespace ERP.Identity.Models.ViewModels.Purchase
{
    public class DebitNoteViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Debit Note No.")]
        public string? DebitNoteNumber { get; set; }

        [Required, Display(Name = "Debit Note Date")]
        public DateTime DebitNoteDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Vendor")]
        public int VendorId { get; set; }

        [Display(Name = "Purchase Invoice")]
        public int? PurchaseInvoiceId { get; set; }

        [Display(Name = "Goods Receipt Note")]
        public int? GoodsReceiptNoteId { get; set; }

        [Display(Name = "Return Location")]
        public int? LocationId { get; set; }

        public string? Reason { get; set; }
        public string? Remarks { get; set; }

        public List<DebitNoteItemViewModel> Items { get; set; } = new() { new() };

        public List<VendorOption> Vendors { get; set; } = new();
        public List<InvoiceOption> Invoices { get; set; } = new();
        public List<GRNOption> GRNs { get; set; } = new();
        public List<LocationOption> Locations { get; set; } = new();
        public List<ProductOption> Products { get; set; } = new();
    }

    public class DebitNoteItemViewModel
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

    public class DebitNoteListItemViewModel
    {
        public int Id { get; set; }
        public string DebitNoteNumber { get; set; } = string.Empty;
        public DateTime DebitNoteDate { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? Reason { get; set; }
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
