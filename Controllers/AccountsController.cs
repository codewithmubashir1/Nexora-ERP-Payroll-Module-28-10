using ERP.Identity.Data;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>Accounts module landing page — cash/bank position, pending approvals, AR/AP snapshot.</summary>
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountsController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var summary = await BuildSummaryAsync();
            ViewBag.CashAndBankBalance = summary.cashAndBankBalance;
            ViewBag.TotalReceivable = summary.totalReceivable;
            ViewBag.TotalPayable = summary.totalPayable;
            ViewBag.PendingApprovals = summary.pendingApprovals;
            ViewBag.AccountCount = summary.accountCount;
            ViewBag.EmployeeCount = summary.employeeCount;

            return View();
        }

        [HttpGet("Accounts/GetSummary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await BuildSummaryAsync();
            return Json(summary);
        }

        private async Task<dynamic> BuildSummaryAsync()
        {
            var accounts = await _db.Accounts.Where(a => a.IsActive).ToListAsync();
            var lineTotals = await _db.JournalEntryLines
                .GroupBy(l => l.AccountId)
                .Select(g => new { AccountId = g.Key, Debit = g.Sum(l => l.Debit), Credit = g.Sum(l => l.Credit) })
                .ToDictionaryAsync(x => x.AccountId, x => x);

            decimal BalanceOf(Account a)
            {
                var t = lineTotals.TryGetValue(a.Id, out var v) ? v : null;
                var debit = t?.Debit ?? 0;
                var credit = t?.Credit ?? 0;
                var isDebitNormal = a.AccountType == AccountType.Asset || a.AccountType == AccountType.Expense;
                return isDebitNormal ? a.OpeningBalance + debit - credit : a.OpeningBalance + credit - debit;
            }

            var cashAndBankBalance = accounts
                .Where(a => a.AccountType == AccountType.Asset && (a.AccountName.Contains("Cash") || a.AccountName.Contains("Bank")))
                .Sum(BalanceOf);

            var salesInvoices = await _db.SalesInvoices.Where(i => i.Status == VoucherStatus.Approved).SumAsync(i => (decimal?)i.TotalAmount) ?? 0;
            var salesReceived = await _db.SalesInvoicePayments.Where(p => p.Status == VoucherStatus.Approved).SumAsync(p => (decimal?)p.Amount) ?? 0;
            var creditNotes = await _db.CreditNotes.Where(c => c.Status == VoucherStatus.Approved).SumAsync(c => (decimal?)c.TotalAmount) ?? 0;
            var totalReceivable = salesInvoices - salesReceived - creditNotes;

            var purchaseInvoices = await _db.PurchaseInvoices.Where(i => i.Status == VoucherStatus.Approved).SumAsync(i => (decimal?)i.NetPayableAmount) ?? 0;
            var purchasePaid = await _db.PurchaseInvoicePayments.Where(p => p.Status == VoucherStatus.Approved).SumAsync(p => (decimal?)p.Amount) ?? 0;
            var debitNotes = await _db.DebitNotes.Where(d => d.Status == VoucherStatus.Approved).SumAsync(d => (decimal?)d.TotalAmount) ?? 0;
            var totalPayable = purchaseInvoices - purchasePaid - debitNotes;

            var pendingApprovals =
                await _db.JournalVouchers.CountAsync(v => v.Status == VoucherStatus.Pending) +
                await _db.CashPaymentVouchers.CountAsync(v => v.Status == VoucherStatus.Pending) +
                await _db.CashReceiptVouchers.CountAsync(v => v.Status == VoucherStatus.Pending) +
                await _db.BankPaymentVouchers.CountAsync(v => v.Status == VoucherStatus.Pending) +
                await _db.BankReceiptVouchers.CountAsync(v => v.Status == VoucherStatus.Pending) +
                await _db.InventoryVouchers.CountAsync(v => v.Status == VoucherStatus.Pending);

            var accountCount = await _db.Accounts.CountAsync();
            var employeeCount = await _db.Employees.CountAsync();

            return new
            {
                cashAndBankBalance,
                totalReceivable,
                totalPayable,
                pendingApprovals,
                accountCount,
                employeeCount
            };
        }
    }
}
