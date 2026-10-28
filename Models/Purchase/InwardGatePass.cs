using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Purchase
{
    public class InwardGatePass
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string GatePassNo { get; set; } = string.Empty;

        public DateTime GatePassDate { get; set; } = DateTime.UtcNow;

        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        /// <summary>Optional link back to the originating PO for traceability.</summary>
        public int? PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }

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

        public ICollection<InwardGatePassItem> Items { get; set; } = new List<InwardGatePassItem>();
    }

    public class InwardGatePassItem
    {
        public int Id { get; set; }

        public int InwardGatePassId { get; set; }
        public InwardGatePass? InwardGatePass { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        /// <summary>Quantity declared as arriving — subject to verification at GRN stage.</summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal DeclaredQuantity { get; set; }

        /// <summary>Reference cost at time of arrival — carried through from a linked PO when one exists, or from the product's own cost price otherwise. Informational; the actual billed cost is set on the GRN/Purchase Invoice.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitCost { get; set; }

        [MaxLength(300)]
        public string? Remarks { get; set; }
    }
}
