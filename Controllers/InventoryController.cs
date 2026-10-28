using ERP.Identity.Data;
using ERP.Identity.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>Inventory module landing page — quick counts + low-stock alerts + shortcuts to every sub-entity.</summary>
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public InventoryController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var summary = await BuildSummaryAsync();
            ViewBag.ProductCount = summary.productCount;
            ViewBag.CategoryCount = summary.categoryCount;
            ViewBag.LocationCount = summary.locationCount;
            ViewBag.TransferCount = summary.transferCount;
            ViewBag.AdjustmentCount = summary.adjustmentCount;
            ViewBag.ConsumptionCount = summary.consumptionCount;
            ViewBag.PendingApprovals = summary.pendingApprovals;

            var stockTotals = await _db.StockTransactions
                .GroupBy(s => s.ProductId)
                .Select(g => new { ProductId = g.Key, Total = g.Sum(s => s.Quantity) })
                .ToListAsync();

            var lowStock = await _db.Products
                .Where(p => p.IsActive)
                .Select(p => new { p.Id, p.Name, p.Sku, p.ReorderLevel })
                .ToListAsync();

            ViewBag.LowStockProducts = lowStock
                .Select(p => new
                {
                    p.Name,
                    p.Sku,
                    p.ReorderLevel,
                    CurrentStock = stockTotals.FirstOrDefault(s => s.ProductId == p.Id)?.Total ?? 0
                })
                .Where(p => p.ReorderLevel > 0 && p.CurrentStock <= p.ReorderLevel)
                .OrderBy(p => p.CurrentStock)
                .Take(8)
                .ToList();

            return View();
        }

        /// <summary>Polled by the dashboard's auto-refresh JS every few seconds — returns the summary counts as JSON.</summary>
        [HttpGet("Inventory/GetSummary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await BuildSummaryAsync();
            return Json(summary);
        }

        private async Task<dynamic> BuildSummaryAsync()
        {
            var productCount = await _db.Products.CountAsync();
            var categoryCount = await _db.ProductCategories.CountAsync();
            var locationCount = await _db.Locations.CountAsync();
            var transferCount = await _db.InternalTransfers.CountAsync();
            var adjustmentCount = await _db.ItemAdjustmentNotes.CountAsync();
            var consumptionCount = await _db.ConsumptionVouchers.CountAsync();

            var pendingApprovals =
                await _db.InternalTransfers.CountAsync(t => t.Status == VoucherStatus.Pending) +
                await _db.ItemAdjustmentNotes.CountAsync(n => n.Status == VoucherStatus.Pending) +
                await _db.ConsumptionVouchers.CountAsync(v => v.Status == VoucherStatus.Pending);

            return new
            {
                productCount,
                categoryCount,
                locationCount,
                transferCount,
                adjustmentCount,
                consumptionCount,
                pendingApprovals
            };
        }
    }
}
