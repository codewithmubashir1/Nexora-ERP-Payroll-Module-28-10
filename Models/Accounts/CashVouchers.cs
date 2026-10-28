using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Accounts
{
    /// <summary>Cash going out — debits a contra account (expense/asset), credits the selected Cash account.</summary>
    public class CashPaymentVoucher
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string VoucherNumber { get; set; } = string.Empty;

        public DateTime VoucherDate { get; set; } = DateTime.UtcNow;

        public int CashAccountId { get; set; }
        public Account? CashAccount { get; set; }

        /// <summary>The expense/asset account being debited (what the cash was spent on).</summary>
        public int ContraAccountId { get; set; }
        public Account? ContraAccount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(150)]
        public string? PaidTo { get; set; }

        public PartyType PartyType { get; set; } = PartyType.None;
        public int? PartyId { get; set; }

        [MaxLength(500)]
        public string? Narration { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Cash coming in — debits the selected Cash account, credits a contra account (income/AR settled).</summary>
    public class CashReceiptVoucher
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string VoucherNumber { get; set; } = string.Empty;

        public DateTime VoucherDate { get; set; } = DateTime.UtcNow;

        public int CashAccountId { get; set; }
        public Account? CashAccount { get; set; }

        /// <summary>The income/AR account being credited (what the cash was received for).</summary>
        public int ContraAccountId { get; set; }
        public Account? ContraAccount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(150)]
        public string? ReceivedFrom { get; set; }

        public PartyType PartyType { get; set; } = PartyType.None;
        public int? PartyId { get; set; }

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
