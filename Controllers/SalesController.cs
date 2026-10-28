using ERP.Identity.Data;
using ERP.Identity.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>Sales module landing page — quick counts + pending approvals + shortcuts to every sub-entity.</summary>
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SalesController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var summary = await BuildSummaryAsync();
            ViewBag.CustomerCount = summary.customerCount;
            ViewBag.SaleOrderCount = summary.saleOrderCount;
            ViewBag.DeliveryChallanCount = summary.deliveryChallanCount;
            ViewBag.InvoiceCount = summary.invoiceCount;
            ViewBag.PendingApprovals = summary.pendingApprovals;
            ViewBag.OutstandingReceivable = summary.outstandingReceivable;

            return View();
        }

        /// <summary>Polled by the dashboard's auto-refresh JS every few seconds — returns the same numbers as JSON.</summary>
        [HttpGet("Sales/GetSummary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await BuildSummaryAsync();
            return Json(summary);
        }

        private async Task<dynamic> BuildSummaryAsync()
        {
            var customerCount = await _db.Customers.CountAsync();
            var saleOrderCount = await _db.SaleOrders.CountAsync();
            var deliveryChallanCount = await _db.DeliveryChallans.CountAsync();
            var invoiceCount = await _db.SalesInvoices.CountAsync();

            var pendingApprovals =
                await _db.SaleOrders.CountAsync(s => s.Status == VoucherStatus.Pending) +
                await _db.DeliveryChallans.CountAsync(d => d.Status == VoucherStatus.Pending) +
                await _db.SalesInvoices.CountAsync(s => s.Status == VoucherStatus.Pending) +
                await _db.SalesInvoicePayments.CountAsync(p => p.Status == VoucherStatus.Pending) +
                await _db.CreditNotes.CountAsync(c => c.Status == VoucherStatus.Pending);

            var totalReceivable = await _db.SalesInvoices
                .Where(i => i.Status == VoucherStatus.Approved)
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;
            var totalReceived = await _db.SalesInvoicePayments
                .Where(p => p.Status == VoucherStatus.Approved)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            var outstandingReceivable = totalReceivable - totalReceived;

            return new
            {
                customerCount,
                saleOrderCount,
                deliveryChallanCount,
                invoiceCount,
                pendingApprovals,
                outstandingReceivable
            };
        }
    }
}
