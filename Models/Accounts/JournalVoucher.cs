using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Identity.Models.Inventory;

namespace ERP.Identity.Models.Accounts
{
    public class JournalVoucher
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string VoucherNumber { get; set; } = string.Empty;

        public DateTime VoucherDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Narration { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public VoucherStatus Status { get; set; } = VoucherStatus.Pending;
        public string? ApprovedById { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<JournalVoucherLine> Lines { get; set; } = new List<JournalVoucherLine>();
    }

    public class JournalVoucherLine
    {
        public int Id { get; set; }

        public int JournalVoucherId { get; set; }
        public JournalVoucher? JournalVoucher { get; set; }

        public int AccountId { get; set; }
        public Account? Account { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Debit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; }

        public PartyType PartyType { get; set; } = PartyType.None;
        public int? PartyId { get; set; }

        [MaxLength(300)]
        public string? Narration { get; set; }
    }
}
