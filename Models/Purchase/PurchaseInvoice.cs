using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Purchase
{
    public class PurchaseInvoice
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>The vendor's own invoice/bill number, for cross-reference.</summary>
        [MaxLength(50)]
        public string? VendorInvoiceNumber { get; set; }

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        public int? PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }

        public int? GoodsReceiptNoteId { get; set; }
        public GoodsReceiptNote? GoodsReceiptNote { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        /// <summary>Sum of line quantity×price, before Sales Tax.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        /// <summary>Sum of per-line Sales Tax (GST) amounts.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        /// <summary>SubTotal + TaxAmount — the gross invoice value before withholding.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>Withholding Income Tax %, deducted from the vendor's payment and remitted to the tax authority separately.</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? WithholdingTaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WithholdingTaxAmount { get; set; }

        /// <summary>TotalAmount − WithholdingTaxAmount — what's actually payable to the vendor.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPayableAmount { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
    }

    public class PurchaseInvoiceItem
    {
        public int Id { get; set; }

        public int PurchaseInvoiceId { get; set; }
        public PurchaseInvoice? PurchaseInvoice { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? TaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        [MaxLength(300)]
        public string? Remarks { get; set; }
    }
}
