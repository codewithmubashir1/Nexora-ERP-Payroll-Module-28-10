using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Accounts
{
    public enum InventoryVoucherType
    {
        /// <summary>Inventory value goes up — debit Inventory, credit the contra account.</summary>
        Increase = 0,
        /// <summary>Inventory value goes down (write-off, damage, shrinkage) — credit Inventory, debit the contra account.</summary>
        Decrease = 1
    }

    /// <summary>Records the GL/accounting impact of an inventory valuation change. This is separate from Inventory's Item Adjustment Note, which records the physical stock-quantity movement — a write-off typically needs both: an Adjustment Note to remove the units from stock, and an Inventory Voucher to book the value loss.</summary>
    public class InventoryVoucher
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string VoucherNumber { get; set; } = string.Empty;

        public DateTime VoucherDate { get; set; } = DateTime.UtcNow;

        public InventoryVoucherType VoucherType { get; set; } = InventoryVoucherType.Decrease;

        /// <summary>The Inventory asset account.</summary>
        public int InventoryAccountId { get; set; }
        public Account? InventoryAccount { get; set; }

        /// <summary>What's on the other side — e.g. "Inventory Write-off Expense" for a decrease, or a supplier/equity account for an increase.</summary>
        public int ContraAccountId { get; set; }
        public Account? ContraAccount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>Optional traceability link to the physical stock movement that prompted this posting.</summary>
        public int? ItemAdjustmentNoteId { get; set; }
        public ItemAdjustmentNote? ItemAdjustmentNote { get; set; }

        [MaxLength(500)]
        public string? Narration { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
