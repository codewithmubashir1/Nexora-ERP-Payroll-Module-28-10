using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Identity.Models.Accounts
{
    /// <summary>One balanced (debits = credits) posting to the general ledger. Created automatically when a voucher is approved — never edited directly.</summary>
    public class JournalEntry
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string EntryNumber { get; set; } = string.Empty;

        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        /// <summary>Which voucher type created this entry, e.g. "JournalVoucher", "CashPaymentVoucher".</summary>
        [Required, MaxLength(50)]
        public string SourceType { get; set; } = string.Empty;

        /// <summary>Id of the source voucher, for traceability and to avoid double-posting on re-approval.</summary>
        public int SourceId { get; set; }

        [MaxLength(500)]
        public string? Narration { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    }

    public class JournalEntryLine
    {
        public int Id { get; set; }

        public int JournalEntryId { get; set; }
        public JournalEntry? JournalEntry { get; set; }

        public int AccountId { get; set; }
        public Account? Account { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Debit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; }

        /// <summary>Optional tag linking this line to a Customer/Vendor/Employee ledger.</summary>
        public PartyType PartyType { get; set; } = PartyType.None;
        public int? PartyId { get; set; }

        [MaxLength(300)]
        public string? Narration { get; set; }
    }
}
