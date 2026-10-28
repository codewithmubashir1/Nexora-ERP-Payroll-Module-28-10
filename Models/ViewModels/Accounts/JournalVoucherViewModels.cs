using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.Accounts;

namespace ERP.Identity.Models.ViewModels.Accounts
{
    public class JournalVoucherViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Voucher Number")]
        public string? VoucherNumber { get; set; }

        [Required, Display(Name = "Voucher Date")]
        public DateTime VoucherDate { get; set; } = DateTime.Today;

        public string? Narration { get; set; }

        public List<JournalVoucherLineViewModel> Lines { get; set; } = new() { new(), new() };

        public List<AccountOption> Accounts { get; set; } = new();
        public List<CustomerLite> Customers { get; set; } = new();
        public List<VendorLite> Vendors { get; set; } = new();
        public List<EmployeeOption> Employees { get; set; } = new();
    }

    public class JournalVoucherLineViewModel
    {
        [Required(ErrorMessage = "Select an account")]
        public int AccountId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Debit { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Credit { get; set; }

        public PartyType PartyType { get; set; } = PartyType.None;
        public int? PartyId { get; set; }

        public string? Narration { get; set; }
    }

    // Lightweight party option records shared across voucher forms — avoids pulling in
    // the full Sales/Purchase view-model namespaces just for a dropdown pair.
    public record CustomerLite(int Id, string Name, string CustomerCode);
    public record VendorLite(int Id, string Name, string VendorCode);

    public class JournalVoucherListItemViewModel
    {
        public int Id { get; set; }
        public string VoucherNumber { get; set; } = string.Empty;
        public DateTime VoucherDate { get; set; }
        public string? Narration { get; set; }
        public decimal TotalAmount { get; set; }
        public int LineCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
