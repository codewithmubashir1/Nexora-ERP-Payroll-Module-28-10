using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Sales
{
    public class DeliveryChallan
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string ChallanNo { get; set; } = string.Empty;

        public DateTime ChallanDate { get; set; } = DateTime.UtcNow;

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int? SaleOrderId { get; set; }
        public SaleOrder? SaleOrder { get; set; }

        /// <summary>Warehouse/location goods are shipped from — stock posts out of here on approval.</summary>
        public int LocationId { get; set; }
        public Location? Location { get; set; }

        [MaxLength(30)]
        public string? VehicleNo { get; set; }

        [MaxLength(150)]
        public string? DriverName { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<DeliveryChallanItem> Items { get; set; } = new List<DeliveryChallanItem>();
    }

    public class DeliveryChallanItem
    {
        public int Id { get; set; }

        public int DeliveryChallanId { get; set; }
        public DeliveryChallan? DeliveryChallan { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        /// <summary>Reference sale price at time of dispatch — carried through from a linked Sale Order when one exists, or from the product's own sale price otherwise. Informational; the actual billed price is set on the Sales Invoice.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitPrice { get; set; }

        [MaxLength(300)]
        public string? Remarks { get; set; }
    }
}
