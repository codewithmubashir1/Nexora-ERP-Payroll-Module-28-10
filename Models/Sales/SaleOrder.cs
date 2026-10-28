using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Sales
{
    public class SaleOrder
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string SONumber { get; set; } = string.Empty;

        public DateTime SODate { get; set; } = DateTime.UtcNow;

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>Comma-separated — some customers issue more than one PO number against a single order.</summary>
        [MaxLength(500)]
        public string? CustomerPONumbers { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<SaleOrderItem> Items { get; set; } = new List<SaleOrderItem>();
    }

    public class SaleOrderItem
    {
        public int Id { get; set; }

        public int SaleOrderId { get; set; }
        public SaleOrder? SaleOrder { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? SalesTaxRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        [MaxLength(300)]
        public string? Remarks { get; set; }
    }
}
