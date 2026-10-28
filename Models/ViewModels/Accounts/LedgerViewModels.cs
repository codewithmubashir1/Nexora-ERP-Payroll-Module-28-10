namespace ERP.Identity.Models.ViewModels.Accounts
{
    public class LedgerViewModel
    {
        public List<CustomerLite> Customers { get; set; } = new();
        public List<VendorLite> Vendors { get; set; } = new();
        public List<EmployeeOption> Employees { get; set; } = new();
    }

    public class AccountLedgerViewModel
    {
        public List<AccountOption> Accounts { get; set; } = new();
    }

    public class LedgerRow
    {
        public DateTime Date { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningBalance { get; set; }
    }
}
