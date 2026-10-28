using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.ViewModels.Inventory;

namespace ERP.Identity.Models.ViewModels.Purchase
{
    public record GRNOption(int Id, string GRNNumber);

    public class PurchaseInvoiceViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Invoice Number")]
        public string? InvoiceNumber { get; set; }

        [Display(Name = "Vendor Invoice No.")]
        public string? VendorInvoiceNumber { get; set; }

        [Required, Display(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; } = DateTime.Today;

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Payment Terms")]
        [Range(0, 3650)]
        public int? PaymentTermsDays { get; set; }

        [Required, Display(Name = "Vendor")]
        public int VendorId { get; set; }

        [Display(Name = "Purchase Order")]
        public int? PurchaseOrderId { get; set; }

        [Display(Name = "Goods Receipt Note")]
        public int? GoodsReceiptNoteId { get; set; }

        [Display(Name = "Withholding Income Tax")]
        [Range(0, 100)]
        public decimal? WithholdingTaxRate { get; set; }

        public string? Remarks { get; set; }

        public List<PurchaseInvoiceItemViewModel> Items { get; set; } = new() { new() };

        public List<PurchaseInvoiceVendorOption> Vendors { get; set; } = new();
        public List<PurchaseOrderOption> PurchaseOrders { get; set; } = new();
        public List<GRNOption> GRNs { get; set; } = new();
        public List<ProductOption> Products { get; set; } = new();
    }

    public record PurchaseInvoiceVendorOption(int Id, string Name, string VendorCode, int PaymentTermsDays);

    public class PurchaseInvoiceItemViewModel
    {
        [Required(ErrorMessage = "Select a product")]
        public int ProductId { get; set; }

        [Required, Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public decimal Quantity { get; set; }

        [Required, Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal? TaxRate { get; set; }

        public string? Remarks { get; set; }
    }

    public class PurchaseInvoiceListItemViewModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal NetPayableAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
