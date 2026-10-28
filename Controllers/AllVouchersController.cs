using ERP.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>Read-only combined view across Journal, Cash Payment/Receipt, Bank Payment/Receipt, and Inventory Vouchers.</summary>
    [Authorize]
    [Route("Accounts/AllVouchers")]
    public class AllVouchersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AllVouchersController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var rows = new List<object>();

            rows.AddRange(await _db.JournalVouchers.Select(v => new
            {
                v.Id,
                VoucherType = "Journal Voucher",
                v.VoucherNumber,
                v.VoucherDate,
                Detail = v.Narration ?? "—",
                Amount = v.TotalAmount,
                Status = v.Status.ToString(),
                PrintUrl = "/Accounts/JournalVouchers/Print/" + v.Id,
                ViewUrl = "/Accounts/JournalVouchers/View/" + v.Id
            }).ToListAsync());

            rows.AddRange(await _db.CashPaymentVouchers.Select(v => new
            {
                v.Id,
                VoucherType = "Cash Payment Voucher",
                v.VoucherNumber,
                v.VoucherDate,
                Detail = v.PaidTo ?? "—",
                v.Amount,
                Status = v.Status.ToString(),
                PrintUrl = "/Accounts/CashPaymentVouchers/Print/" + v.Id,
                ViewUrl = (string?)null
            }).ToListAsync());

            rows.AddRange(await _db.CashReceiptVouchers.Select(v => new
            {
                v.Id,
                VoucherType = "Cash Receipt Voucher",
                v.VoucherNumber,
                v.VoucherDate,
                Detail = v.ReceivedFrom ?? "—",
                v.Amount,
                Status = v.Status.ToString(),
                PrintUrl = "/Accounts/CashReceiptVouchers/Print/" + v.Id,
                ViewUrl = (string?)null
            }).ToListAsync());

            rows.AddRange(await _db.BankPaymentVouchers.Select(v => new
            {
                v.Id,
                VoucherType = "Bank Payment Voucher",
                v.VoucherNumber,
                v.VoucherDate,
                Detail = v.PaidTo ?? "—",
                v.Amount,
                Status = v.Status.ToString(),
                PrintUrl = "/Accounts/BankPaymentVouchers/Print/" + v.Id,
                ViewUrl = (string?)null
            }).ToListAsync());

            rows.AddRange(await _db.BankReceiptVouchers.Select(v => new
            {
                v.Id,
                VoucherType = "Bank Receipt Voucher",
                v.VoucherNumber,
                v.VoucherDate,
                Detail = v.ReceivedFrom ?? "—",
                v.Amount,
                Status = v.Status.ToString(),
                PrintUrl = "/Accounts/BankReceiptVouchers/Print/" + v.Id,
                ViewUrl = (string?)null
            }).ToListAsync());

            rows.AddRange(await _db.InventoryVouchers.Select(v => new
            {
                v.Id,
                VoucherType = "Inventory Voucher",
                v.VoucherNumber,
                v.VoucherDate,
                Detail = v.VoucherType.ToString(),
                v.Amount,
                Status = v.Status.ToString(),
                PrintUrl = "/Accounts/InventoryVouchers/Print/" + v.Id,
                ViewUrl = (string?)null
            }).ToListAsync());

            return Json(new { data = rows });
        }
    }
}
