using System.ComponentModel.DataAnnotations;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.Purchase;

namespace ERP.Identity.Models.ViewModels.Accounts
{
    public class CashPaymentVoucherViewModel
    {
        public int Id { get; set; }
        public string? VoucherNumber { get; set; }

        [Required, Display(Name = "Voucher Date")]
        public DateTime VoucherDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Cash Account")]
        public int CashAccountId { get; set; }

        [Required, Display(Name = "Debit Account (what was paid for)")]
        public int ContraAccountId { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Display(Name = "Paid To")]
        public string? PaidTo { get; set; }

        public PartyType PartyType { get; set; } = PartyType.None;
        public int? PartyId { get; set; }

        public string? Narration { get; set; }

        public List<AccountOption> CashAccounts { get; set; } = new();
        public List<AccountOption> ContraAccounts { get; set; } = new();
        public List<CustomerLite> Customers { get; set; } = new();
        public List<VendorLite> Vendors { get; set; } = new();
        public List<EmployeeOption> Employees { get; set; } = new();
    }

    public class CashReceiptVoucherViewModel
    {
        public int Id { get; set; }
        public string? VoucherNumber { get; set; }

        [Required, Display(Name = "Voucher Date")]
        public DateTime VoucherDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Cash Account")]
        public int CashAccountId { get; set; }

        [Required, Display(Name = "Credit Account (what was received for)")]
        public int ContraAccountId { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Display(Name = "Received From")]
        public string? ReceivedFrom { get; set; }

        public PartyType PartyType { get; set; } = PartyType.None;
        public int? PartyId { get; set; }

        public string? Narration { get; set; }

        public List<AccountOption> CashAccounts { get; set; } = new();
        public List<AccountOption> ContraAccounts { get; set; } = new();
        public List<CustomerLite> Customers { get; set; } = new();
        public List<VendorLite> Vendors { get; set; } = new();
        public List<EmployeeOption> Employees { get; set; } = new();
    }

    public class BankPaymentVoucherViewModel
    {
        public int Id { get; set; }
        public string? VoucherNumber { get; set; }

        [Required, Display(Name = "Voucher Date")]
        public DateTime VoucherDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Bank Account")]
        public int BankAccountId { get; set; }

        [Required, Display(Name = "Debit Account (what was paid for)")]
        public int ContraAccountId { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Display(Name = "Paid To")]
        public string? PaidTo { get; set; }

        public PartyType PartyType { get; set; } = PartyType.None;
        public int? PartyId { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;

        [Display(Name = "Cheque Number")]
        public string? ChequeNumber { get; set; }

        [Display(Name = "Cheque Date")]
        public DateTime? ChequeDate { get; set; }

        public string? Narration { get; set; }

        public List<AccountOption> BankAccounts { get; set; } = new();
        public List<AccountOption> ContraAccounts { get; set; } = new();
        public List<CustomerLite> Customers { get; set; } = new();
        public List<VendorLite> Vendors { get; set; } = new();
        public List<EmployeeOption> Employees { get; set; } = new();
    }

    public class BankReceiptVoucherViewModel
    {
        public int Id { get; set; }
        public string? VoucherNumber { get; set; }

        [Required, Display(Name = "Voucher Date")]
        public DateTime VoucherDate { get; set; } = DateTime.Today;

        [Required, Display(Name = "Bank Account")]
        public int BankAccountId { get; set; }

        [Required, Display(Name = "Credit Account (what was received for)")]
        public int ContraAccountId { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Display(Name = "Received From")]
        public string? ReceivedFrom { get; set; }

        public PartyType PartyType { get; set; } = PartyType.None;
        public int? PartyId { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;

        [Display(Name = "Cheque Number")]
        public string? ChequeNumber { get; set; }

        [Display(Name = "Cheque Date")]
        public DateTime? ChequeDate { get; set; }

        public string? Narration { get; set; }

        public List<AccountOption> BankAccounts { get; set; } = new();
        public List<AccountOption> ContraAccounts { get; set; } = new();
        public List<CustomerLite> Customers { get; set; } = new();
        public List<VendorLite> Vendors { get; set; } = new();
        public List<EmployeeOption> Employees { get; set; } = new();
    }

    public class SimpleVoucherListItemViewModel
    {
        public int Id { get; set; }
        public string VoucherNumber { get; set; } = string.Empty;
        public DateTime VoucherDate { get; set; }
        public string ContraAccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? PartyOrNote { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
