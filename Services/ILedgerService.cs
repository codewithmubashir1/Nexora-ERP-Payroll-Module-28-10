using ERP.Identity.Data;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.ViewModels.Accounts;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Services
{
    public record LedgerResult(decimal OpeningBalance, List<LedgerRow> Rows, decimal ClosingBalance);

    public interface ILedgerService
    {
        Task<LedgerResult> BuildCustomerLedgerAsync(int customerId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<LedgerResult> BuildVendorLedgerAsync(int vendorId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<LedgerResult> BuildEmployeeLedgerAsync(int employeeId, DateTime? fromDate = null, DateTime? toDate = null);
    }

    public class LedgerService : ILedgerService
    {
        private readonly ApplicationDbContext _db;

        public LedgerService(ApplicationDbContext db) => _db = db;

        public async Task<LedgerResult> BuildCustomerLedgerAsync(int customerId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var customer = await _db.Customers.FindAsync(customerId);
            var rows = new List<LedgerRow>();

            // Debits increase what the customer owes us (AR).
            var invoices = await _db.SalesInvoices
                .Where(i => i.CustomerId == customerId && i.Status == VoucherStatus.Approved)
                .ToListAsync();
            rows.AddRange(invoices.Select(i => new LedgerRow
            {
                Date = i.InvoiceDate, DocumentType = "Sales Invoice", DocumentNumber = i.InvoiceNumber,
                Description = "Sales invoice raised", Debit = i.TotalAmount, Credit = 0
            }));

            // Credits reduce what the customer owes us.
            var payments = await _db.SalesInvoicePayments
                .Where(p => p.CustomerId == customerId && p.Status == VoucherStatus.Approved)
                .ToListAsync();
            rows.AddRange(payments.Select(p => new LedgerRow
            {
                Date = p.PaymentDate, DocumentType = "Payment Received", DocumentNumber = p.PaymentNumber,
                Description = "Payment received", Debit = 0, Credit = p.Amount
            }));

            var creditNotes = await _db.CreditNotes
                .Where(c => c.CustomerId == customerId && c.Status == VoucherStatus.Approved)
                .ToListAsync();
            rows.AddRange(creditNotes.Select(c => new LedgerRow
            {
                Date = c.CreditNoteDate, DocumentType = "Credit Note", DocumentNumber = c.CreditNoteNumber,
                Description = c.Reason ?? "Credit note issued", Debit = 0, Credit = c.TotalAmount
            }));

            var taggedLines = await GetTaggedJournalLinesAsync(PartyType.Customer, customerId);
            rows.AddRange(taggedLines);

            var openingBalance = customer?.OpeningBalance ?? 0;
            return BuildResult(openingBalance, rows, fromDate, toDate, creditNormal: false);
        }

        public async Task<LedgerResult> BuildVendorLedgerAsync(int vendorId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var vendor = await _db.Vendors.FindAsync(vendorId);
            var rows = new List<LedgerRow>();

            // Credits increase what we owe the vendor (AP).
            var invoices = await _db.PurchaseInvoices
                .Where(i => i.VendorId == vendorId && i.Status == VoucherStatus.Approved)
                .ToListAsync();
            rows.AddRange(invoices.Select(i => new LedgerRow
            {
                Date = i.InvoiceDate, DocumentType = "Purchase Invoice", DocumentNumber = i.InvoiceNumber,
                Description = "Purchase invoice received", Debit = 0, Credit = i.NetPayableAmount
            }));

            // Debits reduce what we owe the vendor.
            var payments = await _db.PurchaseInvoicePayments
                .Where(p => p.VendorId == vendorId && p.Status == VoucherStatus.Approved)
                .ToListAsync();
            rows.AddRange(payments.Select(p => new LedgerRow
            {
                Date = p.PaymentDate, DocumentType = "Payment Made", DocumentNumber = p.PaymentNumber,
                Description = "Payment made", Debit = p.Amount, Credit = 0
            }));

            var debitNotes = await _db.DebitNotes
                .Where(d => d.VendorId == vendorId && d.Status == VoucherStatus.Approved)
                .ToListAsync();
            rows.AddRange(debitNotes.Select(d => new LedgerRow
            {
                Date = d.DebitNoteDate, DocumentType = "Debit Note", DocumentNumber = d.DebitNoteNumber,
                Description = d.Reason ?? "Debit note issued", Debit = d.TotalAmount, Credit = 0
            }));

            var taggedLines = await GetTaggedJournalLinesAsync(PartyType.Vendor, vendorId);
            rows.AddRange(taggedLines);

            var openingBalance = vendor?.OpeningBalance ?? 0;
            return BuildResult(openingBalance, rows, fromDate, toDate, creditNormal: true);
        }

        public async Task<LedgerResult> BuildEmployeeLedgerAsync(int employeeId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var rows = new List<LedgerRow>();

            // Credits are what we owe the employee, debits are what they owe us.
            // Salary is credited gross and every deduction is debited, so the running
            // balance is what is genuinely still payable to them.
            var payslips = await _db.Payslips
                .Where(p => p.EmployeeId == employeeId && p.Status == VoucherStatus.Approved)
                .ToListAsync();
            var payRunIds = payslips.Where(p => p.PayRunId.HasValue).Select(p => p.PayRunId!.Value).Distinct().ToList();
            var postedPayRunIds = payRunIds.Count == 0
                ? new HashSet<int>()
                : await _db.JournalEntries.Where(entry => entry.SourceType == "PayRun" && payRunIds.Contains(entry.SourceId))
                    .Select(entry => entry.SourceId).ToHashSetAsync();

            foreach (var payslip in payslips)
            {
                if (payslip.PayRunId.HasValue && postedPayRunIds.Contains(payslip.PayRunId.Value)) continue;
                var periodEnd = new DateTime(payslip.Year, payslip.Month, DateTime.DaysInMonth(payslip.Year, payslip.Month));
                var period = periodEnd.ToString("MMM yyyy");
                var grossEarned = payslip.NetPay + payslip.TotalDeductions;

                rows.Add(new LedgerRow
                {
                    Date = periodEnd,
                    DocumentType = "Payslip",
                    DocumentNumber = payslip.PayslipNumber,
                    Description = $"Salary earned for {period}",
                    Debit = 0,
                    Credit = grossEarned
                });

                if (payslip.CurrentAdvanceGiven > 0)
                {
                    rows.Add(new LedgerRow
                    {
                        Date = periodEnd,
                        DocumentType = "Advance Issued",
                        DocumentNumber = payslip.PayslipNumber,
                        Description = $"Advance / loan disbursed in {period}",
                        Debit = payslip.CurrentAdvanceGiven,
                        Credit = 0
                    });
                }

                if (payslip.AdvanceDeducted > 0)
                {
                    rows.Add(new LedgerRow
                    {
                        Date = periodEnd,
                        DocumentType = "Advance Recovered",
                        DocumentNumber = payslip.PayslipNumber,
                        Description = $"Salary withheld to recover an advance for {period}",
                        Debit = payslip.AdvanceDeducted,
                        Credit = 0
                    });

                    rows.Add(new LedgerRow
                    {
                        Date = periodEnd,
                        DocumentType = "Advance Recovered",
                        DocumentNumber = payslip.PayslipNumber,
                        Description = $"Employee advance receivable reduced for {period}",
                        Debit = 0,
                        Credit = payslip.AdvanceDeducted
                    });
                }

                // The flat monthly amount is taken out of salary and immediately added
                // back as a balance held on the employee's behalf, so it shows as a
                // matched pair and stays in their closing balance.
                if (payslip.FixedMonthlyDeduction > 0)
                {
                    rows.Add(new LedgerRow
                    {
                        Date = periodEnd,
                        DocumentType = "Monthly Deduction",
                        DocumentNumber = payslip.PayslipNumber,
                        Description = $"Fixed monthly deduction withheld from {period} salary",
                        Debit = payslip.FixedMonthlyDeduction,
                        Credit = 0
                    });

                    rows.Add(new LedgerRow
                    {
                        Date = periodEnd,
                        DocumentType = "Employee Fund",
                        DocumentNumber = payslip.PayslipNumber,
                        Description = $"Monthly contribution credited to employee ledger ({period})",
                        Debit = 0,
                        Credit = payslip.FixedMonthlyDeduction
                    });
                }

                if (payslip.IncomeTaxDeduction > 0 || payslip.StatutoryDeductions > 0 || payslip.OtherDeductions > 0)
                {
                    var other = payslip.IncomeTaxDeduction + payslip.StatutoryDeductions + payslip.OtherDeductions;
                    rows.Add(new LedgerRow
                    {
                        Date = periodEnd,
                        DocumentType = "Salary Deductions",
                        DocumentNumber = payslip.PayslipNumber,
                        Description = $"Tax, statutory and other deductions for {period}",
                        Debit = other,
                        Credit = 0
                    });
                }
            }

            // Approved reimbursements and leave encashments are also payable to them.
            var reimbursements = await _db.ReimbursementRequests
                .Where(r => r.EmployeeId == employeeId && r.Status == VoucherStatus.Approved)
                .ToListAsync();

            rows.AddRange(reimbursements.Select(r => new LedgerRow
            {
                Date = r.ApprovedOn ?? r.ExpenseDate,
                DocumentType = "Reimbursement",
                DocumentNumber = r.RequestNumber,
                Description = r.Description ?? "Expense reimbursement approved",
                Debit = 0,
                Credit = r.Amount
            }));

            var encashments = await _db.LeaveEncashmentRequests
                .Where(l => l.EmployeeId == employeeId && l.Status == VoucherStatus.Approved)
                .ToListAsync();

            rows.AddRange(encashments.Select(l => new LedgerRow
            {
                Date = l.ApprovedOn ?? l.CreatedOn,
                DocumentType = "Leave Encashment",
                DocumentNumber = l.RequestNumber,
                Description = $"{l.DaysToEncash} day(s) of leave encashed",
                Debit = 0,
                Credit = l.Amount
            }));

            // Anything else posted against this employee through a voucher.
            rows.AddRange(await GetTaggedJournalLinesAsync(PartyType.Employee, employeeId));

            return BuildResult(0, rows, fromDate, toDate, creditNormal: true);
        }

        private async Task<List<LedgerRow>> GetTaggedJournalLinesAsync(PartyType partyType, int partyId)
        {
            var lines = await _db.JournalEntryLines
                .Include(l => l.JournalEntry)
                .Where(l => l.PartyType == partyType && l.PartyId == partyId)
                .ToListAsync();

            return lines.Select(l => new LedgerRow
            {
                Date = l.JournalEntry!.EntryDate,
                DocumentType = FriendlySourceType(l.JournalEntry.SourceType),
                DocumentNumber = l.JournalEntry.EntryNumber,
                Description = l.Narration ?? l.JournalEntry.Narration,
                Debit = l.Debit,
                Credit = l.Credit
            }).ToList();
        }

        private static string FriendlySourceType(string sourceType) => sourceType switch
        {
            "JournalVoucher" => "Journal Voucher",
            "CashPaymentVoucher" => "Cash Payment Voucher",
            "CashReceiptVoucher" => "Cash Receipt Voucher",
            "BankPaymentVoucher" => "Bank Payment Voucher",
            "BankReceiptVoucher" => "Bank Receipt Voucher",
            "InventoryVoucher" => "Inventory Voucher",
            _ => sourceType
        };

        private static LedgerResult BuildResult(decimal openingBalance, List<LedgerRow> rows, DateTime? fromDate, DateTime? toDate, bool creditNormal)
        {
            rows = rows.OrderBy(r => r.Date).ToList();

            // Running balance always starts from the true opening balance, even if a
            // from-date filter is applied below — so the filtered view's starting
            // balance still reflects everything that happened before the window.
            var runningBalance = openingBalance;
            foreach (var row in rows)
            {
                runningBalance += creditNormal ? row.Credit - row.Debit : row.Debit - row.Credit;
                row.RunningBalance = runningBalance;
            }

            var openingBalanceForWindow = openingBalance;
            if (fromDate.HasValue)
            {
                var priorRows = rows.Where(r => r.Date < fromDate.Value).ToList();
                openingBalanceForWindow = priorRows.Any() ? priorRows.Last().RunningBalance : openingBalance;
                rows = rows.Where(r => r.Date >= fromDate.Value).ToList();
            }
            if (toDate.HasValue)
            {
                rows = rows.Where(r => r.Date <= toDate.Value).ToList();
            }

            var closingBalance = rows.Any() ? rows.Last().RunningBalance : openingBalanceForWindow;
            return new LedgerResult(openingBalanceForWindow, rows, closingBalance);
        }
    }
}
