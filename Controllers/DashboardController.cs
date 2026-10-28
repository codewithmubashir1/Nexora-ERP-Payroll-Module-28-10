using ERP.Identity.Data;
using ERP.Identity.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>Main landing dashboard — real, live counts pulled from the database (no placeholder numbers).</summary>
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            await PopulateSummaryAsync();
            return View();
        }

        /// <summary>Polled by the dashboard's auto-refresh JS every few seconds — returns the same numbers as JSON.</summary>
        [HttpGet("Dashboard/GetSummary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await BuildSummaryAsync();
            return Json(summary);
        }

        private async Task PopulateSummaryAsync()
        {
            var summary = await BuildSummaryAsync();
            ViewBag.SalesOrdersThisMonth = summary.salesOrdersThisMonth;
            ViewBag.PurchaseOrderCount = summary.purchaseOrderCount;
            ViewBag.ProductsInStockCount = summary.productsInStockCount;
            ViewBag.PendingApprovalsTotal = summary.pendingApprovalsTotal;
            ViewBag.CustomerCount = summary.customerCount;
            ViewBag.VendorCount = summary.vendorCount;
            ViewBag.EmployeeCount = summary.employeeCount;
            ViewBag.PayrollNetThisMonth = summary.payrollNetThisMonth;
        }

        private async Task<dynamic> BuildSummaryAsync()
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var salesOrdersThisMonth = await _db.SaleOrders.CountAsync(s => s.SODate >= monthStart);
            var purchaseOrderCount = await _db.PurchaseOrders.CountAsync(p => p.PODate >= monthStart);
            var customerCount = await _db.Customers.CountAsync(c => c.IsActive);
            var vendorCount = await _db.Vendors.CountAsync(v => v.IsActive);
            var employeeCount = await _db.Employees.CountAsync(e => e.IsActive);
            var payrollNetThisMonth = await _db.PayRuns
                .Where(p => p.Month == now.Month && p.Year == now.Year)
                .SumAsync(p => (decimal?)p.TotalNetAmount) ?? 0m;

            // "Items in stock" = distinct products currently holding positive stock anywhere.
            var productsInStockCount = await _db.StockTransactions
                .GroupBy(t => t.ProductId)
                .Select(g => new { ProductId = g.Key, Balance = g.Sum(t => t.Quantity) })
                .CountAsync(x => x.Balance > 0);

            var pendingApprovalsTotal =
                await _db.SaleOrders.CountAsync(s => s.Status == VoucherStatus.Pending) +
                await _db.DeliveryChallans.CountAsync(d => d.Status == VoucherStatus.Pending) +
                await _db.SalesInvoices.CountAsync(s => s.Status == VoucherStatus.Pending) +
                await _db.SalesInvoicePayments.CountAsync(p => p.Status == VoucherStatus.Pending) +
                await _db.CreditNotes.CountAsync(c => c.Status == VoucherStatus.Pending) +
                await _db.PurchaseOrders.CountAsync(p => p.Status == VoucherStatus.Pending) +
                await _db.InwardGatePasses.CountAsync(g => g.Status == VoucherStatus.Pending) +
                await _db.GoodsReceiptNotes.CountAsync(g => g.Status == VoucherStatus.Pending) +
                await _db.PurchaseInvoices.CountAsync(p => p.Status == VoucherStatus.Pending) +
                await _db.PurchaseInvoicePayments.CountAsync(p => p.Status == VoucherStatus.Pending) +
                await _db.DebitNotes.CountAsync(d => d.Status == VoucherStatus.Pending) +
                await _db.InternalTransfers.CountAsync(t => t.Status == VoucherStatus.Pending) +
                await _db.ItemAdjustmentNotes.CountAsync(a => a.Status == VoucherStatus.Pending) +
                await _db.ConsumptionVouchers.CountAsync(c => c.Status == VoucherStatus.Pending);

            var monthLabels = new List<string>();
            var salesAmounts = new List<decimal>();
            var purchaseAmounts = new List<decimal>();
            var firstMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
            for (var offset = 0; offset < 6; offset++)
            {
                var periodStart = firstMonth.AddMonths(offset);
                var periodEnd = periodStart.AddMonths(1);
                monthLabels.Add(periodStart.ToString("MMM"));
                salesAmounts.Add(await _db.SaleOrders
                    .Where(s => s.SODate >= periodStart && s.SODate < periodEnd)
                    .SumAsync(s => (decimal?)s.TotalAmount) ?? 0m);
                purchaseAmounts.Add(await _db.PurchaseOrders
                    .Where(p => p.PODate >= periodStart && p.PODate < periodEnd)
                    .SumAsync(p => (decimal?)p.TotalAmount) ?? 0m);
            }

            var approvalStatuses = new[]
            {
                new
                {
                    label = "Pending",
                    sales = await _db.SaleOrders.CountAsync(s => s.Status == VoucherStatus.Pending),
                    purchases = await _db.PurchaseOrders.CountAsync(p => p.Status == VoucherStatus.Pending)
                },
                new
                {
                    label = "Approved",
                    sales = await _db.SaleOrders.CountAsync(s => s.Status == VoucherStatus.Approved),
                    purchases = await _db.PurchaseOrders.CountAsync(p => p.Status == VoucherStatus.Approved)
                },
                new
                {
                    label = "Rejected",
                    sales = await _db.SaleOrders.CountAsync(s => s.Status == VoucherStatus.Rejected),
                    purchases = await _db.PurchaseOrders.CountAsync(p => p.Status == VoucherStatus.Rejected)
                }
            };

            return new
            {
                salesOrdersThisMonth,
                purchaseOrderCount,
                productsInStockCount,
                pendingApprovalsTotal,
                customerCount,
                vendorCount,
                employeeCount,
                payrollNetThisMonth,
                monthLabels,
                salesAmounts,
                purchaseAmounts,
                approvalStatuses
            };
        }
    }
}
