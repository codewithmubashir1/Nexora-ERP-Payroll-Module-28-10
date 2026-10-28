using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Sales
{
    public class SalesInvoice
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        /// <summary>Payment terms selected at invoicing time (On Receipt=0, 7/15/30/45/60/75 days). Used client-side to auto-calculate DueDate; DueDate itself is the source of truth once saved.</summary>
        public int? PaymentTermsDays { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int? SaleOrderId { get; set; }
        public SaleOrder? SaleOrder { get; set; }

        public int? DeliveryChallanId { get; set; }
        public DeliveryChallan? DeliveryChallan { get; set; }

        /// <summary>Comma-separated — some customers issue more than one PO number against a single invoice.</summary>
        [MaxLength(500)]
        public string? CustomerPONumbers { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        /// <summary>Sum of line quantity×price, before Sales Tax.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        /// <summary>Sum of per-line Sales Tax (GST) amounts.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        /// <summary>SubTotal + TaxAmount.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
    }

    public class SalesInvoiceItem
    {
        public int Id { get; set; }

        public int SalesInvoiceId { get; set; }
        public SalesInvoice? SalesInvoice { get; set; }

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
