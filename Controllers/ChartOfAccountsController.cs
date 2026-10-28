using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Accounts/ChartOfAccounts")]
    public class ChartOfAccountsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ChartOfAccountsController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _db.Accounts.Include(a => a.ParentAccount).OrderBy(a => a.AccountCode).ToListAsync();

            var lineTotals = await _db.JournalEntryLines
                .GroupBy(l => l.AccountId)
                .Select(g => new { AccountId = g.Key, Debit = g.Sum(l => l.Debit), Credit = g.Sum(l => l.Credit) })
                .ToDictionaryAsync(x => x.AccountId, x => x);

            var list = accounts.Select(a =>
            {
                var totals = lineTotals.TryGetValue(a.Id, out var t) ? t : null;
                var debit = totals?.Debit ?? 0;
                var credit = totals?.Credit ?? 0;

                var isDebitNormal = a.AccountType == AccountType.Asset || a.AccountType == AccountType.Expense;
                var currentBalance = isDebitNormal
                    ? a.OpeningBalance + debit - credit
                    : a.OpeningBalance + credit - debit;

                return new AccountListItemViewModel
                {
                    Id = a.Id,
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    AccountType = a.AccountType.ToString(),
                    ParentAccountName = a.ParentAccount?.AccountName,
                    OpeningBalance = a.OpeningBalance,
                    CurrentBalance = currentBalance,
                    IsSystemAccount = a.IsSystemAccount,
                    IsActive = a.IsActive
                };
            }).ToList();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new AccountFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var account = await _db.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            var vm = new AccountFormViewModel
            {
                Id = account.Id,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                AccountType = account.AccountType,
                ParentAccountId = account.ParentAccountId,
                OpeningBalance = account.OpeningBalance,
                Description = account.Description,
                IsActive = account.IsActive,
                IsSystemAccount = account.IsSystemAccount
            };

            await PopulateDropdownsAsync(vm, excludeId: id);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Accountant}")]
        public async Task<IActionResult> Save(AccountFormViewModel model)
        {
            var duplicateExists = await _db.Accounts.AnyAsync(a => a.AccountCode == model.AccountCode && a.Id != model.Id);
            if (duplicateExists)
                ModelState.AddModelError(nameof(model.AccountCode), "An account with this code already exists.");

            if (model.ParentAccountId == model.Id && model.Id != 0)
                ModelState.AddModelError(nameof(model.ParentAccountId), "An account cannot be its own parent.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model, excludeId: model.Id);
                return View("Form", model);
            }

            var isNew = model.Id == 0;
            Account account;

            if (isNew)
            {
                account = new Account { CreatedOn = DateTime.UtcNow };
                _db.Accounts.Add(account);
            }
            else
            {
                var existing = await _db.Accounts.FindAsync(model.Id);
                if (existing == null) return NotFound();
                account = existing;
            }

            account.AccountCode = model.AccountCode;
            account.AccountName = model.AccountName;
            account.AccountType = model.AccountType;
            account.ParentAccountId = model.ParentAccountId;
            account.OpeningBalance = model.OpeningBalance;
            account.Description = model.Description;
            account.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = isNew ? "Account created successfully." : "Account updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var account = await _db.Accounts.FindAsync(id);
            if (account == null) return Json(new { success = false, message = "Account not found." });

            if (account.IsSystemAccount)
                return Json(new { success = false, message = "This is a system account used by automated postings and cannot be deleted." });

            var hasChildren = await _db.Accounts.AnyAsync(a => a.ParentAccountId == id);
            if (hasChildren)
                return Json(new { success = false, message = "This account has sub-accounts and cannot be deleted." });

            var hasEntries = await _db.JournalEntryLines.AnyAsync(l => l.AccountId == id);
            if (hasEntries)
                return Json(new { success = false, message = "This account has posted transactions and cannot be deleted. Deactivate it instead." });

            _db.Accounts.Remove(account);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Account deleted." });
        }

        private async Task PopulateDropdownsAsync(AccountFormViewModel vm, int? excludeId = null)
        {
            var query = _db.Accounts.Where(a => a.IsActive);
            if (excludeId.HasValue) query = query.Where(a => a.Id != excludeId.Value);

            vm.ParentAccounts = await query
                .OrderBy(a => a.AccountCode)
                .Select(a => new AccountOption(a.Id, a.AccountCode, a.AccountName, a.AccountType))
                .ToListAsync();
        }
    }
}
