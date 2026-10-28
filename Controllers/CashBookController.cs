using System.Globalization;
using ERP.Identity.Data;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.ViewModels.Accounts;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Accounts/CashBook")]
    public class CashBookController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CashBookController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var viewModel = new AccountLedgerViewModel
            {
                Accounts = await _db.Accounts.Where(a => a.IsActive)
                    .OrderBy(a => a.AccountCode)
                    .Select(a => new AccountOption(a.Id, a.AccountCode, a.AccountName, a.AccountType))
                    .ToListAsync()
            };
            return View(viewModel);
        }

        [HttpGet("GetLedger")]
        public async Task<IActionResult> GetLedger(int accountId, DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
                return BadRequest("From date must be on or before To date.");

            var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.IsActive);
            if (account == null) return NotFound("Account not found.");

            var lines = await _db.JournalEntryLines
                .Include(line => line.JournalEntry)
                .Where(line => line.AccountId == accountId)
                .OrderBy(line => line.JournalEntry!.EntryDate)
                .ThenBy(line => line.JournalEntry!.Id)
                .ToListAsync();

            var debitNormal = account.AccountType is AccountType.Asset or AccountType.Expense;
            var balance = account.OpeningBalance;
            var opening = account.OpeningBalance;
            var totalDebit = 0m;
            var totalCredit = 0m;
            var rows = new List<object>();

            foreach (var line in lines)
            {
                var entryDate = line.JournalEntry!.EntryDate.Date;
                var delta = debitNormal ? line.Debit - line.Credit : line.Credit - line.Debit;

                if (fromDate.HasValue && entryDate < fromDate.Value.Date)
                {
                    balance += delta;
                    opening = balance;
                    continue;
                }
                if (toDate.HasValue && entryDate > toDate.Value.Date) break;

                balance += delta;
                totalDebit += line.Debit;
                totalCredit += line.Credit;
                rows.Add(new
                {
                    date = entryDate,
                    entryNumber = line.JournalEntry.EntryNumber,
                    sourceType = FriendlySourceType(line.JournalEntry.SourceType),
                    description = line.Narration ?? line.JournalEntry.Narration,
                    debit = line.Debit,
                    credit = line.Credit,
                    balance
                });
            }

            return Json(new
            {
                accountName = account.AccountName,
                accountCode = account.AccountCode,
                accountType = account.AccountType.ToString(),
                normalSide = debitNormal ? "Debit" : "Credit",
                openingBalance = opening,
                totalDebit,
                totalCredit,
                closingBalance = balance,
                rows
            });
        }

        [HttpGet("Export")]
        public async Task<IActionResult> Export(int accountId, DateTime? fromDate, DateTime? toDate)
        {
            var result = await GetLedgerDataAsync(accountId, fromDate, toDate);
            if (result == null) return NotFound();
            var headers = new[] { "Date", "Journal Entry", "Source", "Description", "Debit", "Credit", "Remaining Balance" };
            var rows = result.Select(row => new string?[]
            {
                row.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), row.EntryNumber, row.SourceType,
                row.Description, row.Debit.ToString(CultureInfo.InvariantCulture), row.Credit.ToString(CultureInfo.InvariantCulture),
                row.Balance.ToString(CultureInfo.InvariantCulture)
            });
            return File(CsvUtility.WriteWithHeader(headers, rows), "text/csv", $"account_ledger_{accountId}_{DateTime.Today:yyyyMMdd}.csv");
        }

        private async Task<List<AccountLedgerExportRow>?> GetLedgerDataAsync(int accountId, DateTime? fromDate, DateTime? toDate)
        {
            if (!await _db.Accounts.AnyAsync(a => a.Id == accountId && a.IsActive)) return null;
            var lines = await _db.JournalEntryLines.Include(line => line.JournalEntry)
                .Where(line => line.AccountId == accountId)
                .OrderBy(line => line.JournalEntry!.EntryDate).ThenBy(line => line.JournalEntry!.Id)
                .ToListAsync();
            var debitNormal = await _db.Accounts.Where(a => a.Id == accountId)
                .Select(a => a.AccountType == AccountType.Asset || a.AccountType == AccountType.Expense)
                .FirstAsync();
            var opening = await _db.Accounts.Where(a => a.Id == accountId).Select(a => a.OpeningBalance).FirstAsync();
            var balance = opening;
            var rows = new List<AccountLedgerExportRow>();
            foreach (var line in lines)
            {
                var date = line.JournalEntry!.EntryDate.Date;
                var delta = debitNormal ? line.Debit - line.Credit : line.Credit - line.Debit;
                if (fromDate.HasValue && date < fromDate.Value.Date) { balance += delta; continue; }
                if (toDate.HasValue && date > toDate.Value.Date) break;
                balance += delta;
                rows.Add(new AccountLedgerExportRow(date, line.JournalEntry.EntryNumber, FriendlySourceType(line.JournalEntry.SourceType),
                    line.Narration ?? line.JournalEntry.Narration, line.Debit, line.Credit, balance));
            }
            return rows;
        }

        private static string FriendlySourceType(string sourceType) => sourceType switch
        {
            "JournalVoucher" => "Journal Voucher",
            "CashPaymentVoucher" => "Cash Payment Voucher",
            "CashReceiptVoucher" => "Cash Receipt Voucher",
            "BankPaymentVoucher" => "Bank Payment Voucher",
            "BankReceiptVoucher" => "Bank Receipt Voucher",
            "PayRun" => "Payroll",
            _ => sourceType
        };

        private record AccountLedgerExportRow(DateTime Date, string EntryNumber, string SourceType, string? Description, decimal Debit, decimal Credit, decimal Balance);
    }
}