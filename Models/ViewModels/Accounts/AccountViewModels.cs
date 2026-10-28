using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.Accounts;

namespace ERP.Identity.Models.ViewModels.Accounts
{
    public class AccountFormViewModel
    {
        public int Id { get; set; }

        [Required, Display(Name = "Account Code")]
        public string AccountCode { get; set; } = string.Empty;

        [Required, Display(Name = "Account Name")]
        public string AccountName { get; set; } = string.Empty;

        [Required, Display(Name = "Account Type")]
        public AccountType AccountType { get; set; }

        [Display(Name = "Parent Account")]
        public int? ParentAccountId { get; set; }

        [Display(Name = "Opening Balance")]
        public decimal OpeningBalance { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public bool IsSystemAccount { get; set; }

        public List<AccountOption> ParentAccounts { get; set; } = new();
    }

    public record AccountOption(int Id, string AccountCode, string AccountName, AccountType AccountType);

    public class AccountListItemViewModel
    {
        public int Id { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string? ParentAccountName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public bool IsSystemAccount { get; set; }
        public bool IsActive { get; set; }
    }
}
