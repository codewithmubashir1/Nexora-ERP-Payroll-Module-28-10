using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Purchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>Read-only unified view across every cheque-based transaction in the app, with a shared clearance-status action.</summary>
    [Authorize]
    [Route("Accounts/ChequesManagement")]
    public class ChequesManagementController : Controller
    {
        private const string ApproverRoles = $"{AppRoles.Admin},{AppRoles.Manager},{AppRoles.Accountant}";

        private readonly ApplicationDbContext _db;

        public ChequesManagementController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var rows = new List<object>();

            rows.AddRange(await _db.IssuedCheques.Include(c => c.Vendor).Select(c => new
            {
                Source = "IssuedCheque",
                c.Id,
                Direction = "Issued (Payment)",
                c.ChequeNumber,
                c.ChequeDate,
                c.BankName,
                PartyName = c.Vendor!.Name,
                c.Amount,
                ClearanceStatus = c.ClearanceStatus.ToString()
            }).ToListAsync());

            rows.AddRange(await _db.SalesInvoicePayments
                .Where(p => p.PaymentMethod == PaymentMethod.Cheque)
                .Include(p => p.Customer)
                .Select(p => new
                {
                    Source = "SalesInvoicePayment",
                    p.Id,
                    Direction = "Received",
                    ChequeNumber = p.ChequeNumber ?? "—",
                    ChequeDate = p.PaymentDate,
                    BankName = p.BankName,
                    PartyName = p.Customer!.Name,
                    p.Amount,
                    ClearanceStatus = (p.ClearanceStatus ?? ChequeClearanceStatus.Outstanding).ToString()
                }).ToListAsync());

            rows.AddRange(await _db.BankPaymentVouchers
                .Where(v => v.PaymentMethod == PaymentMethod.Cheque)
                .Select(v => new
                {
                    Source = "BankPaymentVoucher",
                    v.Id,
                    Direction = "Issued (Payment)",
                    ChequeNumber = v.ChequeNumber ?? "—",
                    ChequeDate = v.ChequeDate ?? v.VoucherDate,
                    BankName = (string?)null,
                    PartyName = v.PaidTo ?? "—",
                    v.Amount,
                    ClearanceStatus = (v.ClearanceStatus ?? ChequeClearanceStatus.Outstanding).ToString()
                }).ToListAsync());

            rows.AddRange(await _db.BankReceiptVouchers
                .Where(v => v.PaymentMethod == PaymentMethod.Cheque)
                .Select(v => new
                {
                    Source = "BankReceiptVoucher",
                    v.Id,
                    Direction = "Received",
                    ChequeNumber = v.ChequeNumber ?? "—",
                    ChequeDate = v.ChequeDate ?? v.VoucherDate,
                    BankName = (string?)null,
                    PartyName = v.ReceivedFrom ?? "—",
                    v.Amount,
                    ClearanceStatus = (v.ClearanceStatus ?? ChequeClearanceStatus.Outstanding).ToString()
                }).ToListAsync());

            return Json(new { data = rows });
        }

        [HttpPost("SetClearance")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = ApproverRoles)]
        public async Task<IActionResult> SetClearance(string source, int id, ChequeClearanceStatus status)
        {
            switch (source)
            {
                case "IssuedCheque":
                    var issued = await _db.IssuedCheques.FindAsync(id);
                    if (issued == null) return Json(new { success = false, message = "Cheque not found." });
                    issued.ClearanceStatus = status;
                    break;

                case "SalesInvoicePayment":
                    var received = await _db.SalesInvoicePayments.FindAsync(id);
                    if (received == null) return Json(new { success = false, message = "Cheque not found." });
                    received.ClearanceStatus = status;
                    break;

                case "BankPaymentVoucher":
                    var bpv = await _db.BankPaymentVouchers.FindAsync(id);
                    if (bpv == null) return Json(new { success = false, message = "Cheque not found." });
                    bpv.ClearanceStatus = status;
                    break;

                case "BankReceiptVoucher":
                    var brv = await _db.BankReceiptVouchers.FindAsync(id);
                    if (brv == null) return Json(new { success = false, message = "Cheque not found." });
                    brv.ClearanceStatus = status;
                    break;

                default:
                    return Json(new { success = false, message = "Unknown cheque source." });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = $"Cheque marked as {status}." });
        }
    }
}
