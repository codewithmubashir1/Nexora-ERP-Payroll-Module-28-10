using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Identity.Models.Accounts
{
    public enum AccountType
    {
        Asset = 0,
        Liability = 1,
        Equity = 2,
        Income = 3,
        Expense = 4
    }

    /// <summary>Which ledger party (if any) a voucher line is tagged against, for Customer/Vendor/Employee ledger derivation.</summary>
    public enum PartyType
    {
        None = 0,
        Customer = 1,
        Vendor = 2,
        Employee = 3
    }

    public class Account
    {
        public int Id { get; set; }

        /// <summary>System-generated or manually assigned, e.g. 1000, 1010. Kept editable since chart-of-account numbering conventions vary by company.</summary>
        [Required, MaxLength(20)]
        public string AccountCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string AccountName { get; set; } = string.Empty;

        public AccountType AccountType { get; set; }

        public int? ParentAccountId { get; set; }
        public Account? ParentAccount { get; set; }

        /// <summary>Marks the handful of accounts the app posts to automatically (Cash, Bank, Inventory, AR, AP, Sales, COGS...) — protected from deletion.</summary>
        public bool IsSystemAccount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<Account> ChildAccounts { get; set; } = new List<Account>();
    }
}
