using ERP.Identity.Data;
using ERP.Identity.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>Purchase module landing page — quick counts + pending approvals + shortcuts to every sub-entity.</summary>
    [Authorize]
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PurchasesController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var summary = await BuildSummaryAsync();
            ViewBag.VendorCount = summary.vendorCount;
            ViewBag.POCount = summary.poCount;
            ViewBag.GRNCount = summary.grnCount;
            ViewBag.InvoiceCount = summary.invoiceCount;
            ViewBag.PaymentCount = summary.paymentCount;
            ViewBag.ChequeCount = summary.chequeCount;
            ViewBag.PendingApprovals = summary.pendingApprovals;
            ViewBag.OutstandingPayables = summary.outstandingPayables;

            return View();
        }

        /// <summary>Polled by the dashboard's auto-refresh JS every few seconds — returns the same numbers as JSON.</summary>
        [HttpGet("Purchases/GetSummary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await BuildSummaryAsync();
            return Json(summary);
        }

        private async Task<dynamic> BuildSummaryAsync()
        {
            var vendorCount = await _db.Vendors.CountAsync();
            var poCount = await _db.PurchaseOrders.CountAsync();
            var grnCount = await _db.GoodsReceiptNotes.CountAsync();
            var invoiceCount = await _db.PurchaseInvoices.CountAsync();
            var paymentCount = await _db.PurchaseInvoicePayments.CountAsync();
            var chequeCount = await _db.IssuedCheques.CountAsync();

            var pendingApprovals =
                await _db.PurchaseOrders.CountAsync(p => p.Status == VoucherStatus.Pending) +
                await _db.InwardGatePasses.CountAsync(g => g.Status == VoucherStatus.Pending) +
                await _db.GoodsReceiptNotes.CountAsync(g => g.Status == VoucherStatus.Pending) +
                await _db.PurchaseInvoices.CountAsync(p => p.Status == VoucherStatus.Pending) +
                await _db.PurchaseInvoicePayments.CountAsync(p => p.Status == VoucherStatus.Pending) +
                await _db.DebitNotes.CountAsync(d => d.Status == VoucherStatus.Pending) +
                await _db.IssuedCheques.CountAsync(c => c.Status == VoucherStatus.Pending) +
                await _db.VendorAdvances.CountAsync(a => a.Status == VoucherStatus.Pending);

            var invoices = await _db.PurchaseInvoices.Where(i => i.Status == VoucherStatus.Approved).ToListAsync();
            var paidByInvoice = await _db.PurchaseInvoicePayments
                .Where(p => p.Status == VoucherStatus.Approved)
                .GroupBy(p => p.PurchaseInvoiceId)
                .Select(g => new { InvoiceId = g.Key, Paid = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.InvoiceId, x => x.Paid);

            var outstandingPayables = invoices
                .Sum(i => i.NetPayableAmount - (paidByInvoice.TryGetValue(i.Id, out var paid) ? paid : 0));

            return new
            {
                vendorCount,
                poCount,
                grnCount,
                invoiceCount,
                paymentCount,
                chequeCount,
                pendingApprovals,
                outstandingPayables
            };
        }
    }
}
