using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.Purchase;

namespace ERP.Identity.Models.Accounts
{
    /// <summary>Bank funds going out — debits a contra account, credits the selected Bank account. Supports cheque tracking for the unified Cheque Register.</summary>
    public class BankPaymentVoucher
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string VoucherNumber { get; set; } = string.Empty;

        public DateTime VoucherDate { get; set; } = DateTime.UtcNow;

        public int BankAccountId { get; set; }
        public Account? BankAccount { get; set; }

        public int ContraAccountId { get; set; }
        public Account? ContraAccount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(150)]
        public string? PaidTo { get; set; }

        public PartyType PartyType { get; set; } = PartyType.None;
        public int? PartyId { get; set; }

        /// <summary>Bank Transfer or Cheque — reuses the Purchase module's PaymentMethod enum (Cash isn't a valid option here).</summary>
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;

        [MaxLength(50)]
        public string? ChequeNumber { get; set; }

        public DateTime? ChequeDate { get; set; }

        /// <summary>Only meaningful when PaymentMethod is Cheque — feeds the unified Cheque Register.</summary>
        public ChequeClearanceStatus? ClearanceStatus { get; set; }

        [MaxLength(500)]
        public string? Narration { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Bank funds coming in — debits the selected Bank account, credits a contra account.</summary>
    public class BankReceiptVoucher
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string VoucherNumber { get; set; } = string.Empty;

        public DateTime VoucherDate { get; set; } = DateTime.UtcNow;

        public int BankAccountId { get; set; }
        public Account? BankAccount { get; set; }

        public int ContraAccountId { get; set; }
        public Account? ContraAccount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(150)]
        public string? ReceivedFrom { get; set; }

        public PartyType PartyType { get; set; } = PartyType.None;
        public int? PartyId { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;

        [MaxLength(50)]
        public string? ChequeNumber { get; set; }

        public DateTime? ChequeDate { get; set; }

        public ChequeClearanceStatus? ClearanceStatus { get; set; }

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
