using ERP.Identity.Data;
using ERP.Identity.Models.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Services
{
    public interface IStockService
    {
        Task<string> GenerateDocumentNumberAsync(string prefix, IQueryable<string> existingNumbers);

        Task PostOpeningStockAsync(Product product, int locationId);

        Task PostInternalTransferAsync(InternalTransfer transfer);

        Task PostAdjustmentAsync(ItemAdjustmentNote note);

        Task PostConsumptionAsync(ConsumptionVoucher voucher);

        /// <summary>
        /// Generic single-line ledger post, for modules outside Inventory (e.g. Purchase's
        /// Goods Receipt Note / Debit Note) that need to move stock without depending on
        /// Inventory's own document types.
        /// </summary>
        Task PostStockMovementAsync(int productId, int locationId, decimal signedQuantity, StockTransactionType type,
            string referenceType, int referenceId, string? referenceNo, string? remarks, DateTime? transactionDate = null);

        /// <summary>Current stock for one product at one location.</summary>
        Task<decimal> GetCurrentStockAsync(int productId, int locationId);

        /// <summary>Current stock for one product summed across every location.</summary>
        Task<decimal> GetCurrentStockAsync(int productId);

        /// <summary>Current stock for every product, summed across locations. Keyed by ProductId.</summary>
        Task<Dictionary<int, decimal>> GetCurrentStockForAllProductsAsync();
    }

    public class StockService : IStockService
    {
        private readonly ApplicationDbContext _db;

        public StockService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerateDocumentNumberAsync(string prefix, IQueryable<string> existingNumbers)
        {
            // Format: {PREFIX}-{yyyyMM}-{seq:D4}, resetting every calendar month —
            // e.g. PO-202609-0001. Only numbers already carrying this month's stamp
            // count toward the sequence, so the count naturally resets when the
            // month rolls over.
            var monthStamp = DateTime.UtcNow.ToString("yyyyMM");
            var fullPrefix = $"{prefix}-{monthStamp}-";

            var count = await existingNumbers.CountAsync(n => n.StartsWith(fullPrefix));
            return $"{fullPrefix}{(count + 1):D4}";
        }

        public async Task PostOpeningStockAsync(Product product, int locationId)
        {
            if (product.OpeningStock == 0) return;

            _db.StockTransactions.Add(new StockTransaction
            {
                ProductId = product.Id,
                LocationId = locationId,
                TransactionType = StockTransactionType.OpeningStock,
                Quantity = product.OpeningStock,
                ReferenceType = "Product",
                ReferenceId = product.Id,
                ReferenceNo = product.Sku,
                Remarks = "Opening stock at product creation",
                TransactionDate = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }

        public async Task PostInternalTransferAsync(InternalTransfer transfer)
        {
            foreach (var item in transfer.Items)
            {
                _db.StockTransactions.Add(new StockTransaction
                {
                    ProductId = item.ProductId,
                    LocationId = transfer.FromLocationId,
                    TransactionType = StockTransactionType.TransferOut,
                    Quantity = -Math.Abs(item.Quantity),
                    ReferenceType = "InternalTransfer",
                    ReferenceId = transfer.Id,
                    ReferenceNo = transfer.TransferNo,
                    Remarks = item.Remarks,
                    TransactionDate = transfer.TransferDate
                });

                _db.StockTransactions.Add(new StockTransaction
                {
                    ProductId = item.ProductId,
                    LocationId = transfer.ToLocationId,
                    TransactionType = StockTransactionType.TransferIn,
                    Quantity = Math.Abs(item.Quantity),
                    ReferenceType = "InternalTransfer",
                    ReferenceId = transfer.Id,
                    ReferenceNo = transfer.TransferNo,
                    Remarks = item.Remarks,
                    TransactionDate = transfer.TransferDate
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task PostAdjustmentAsync(ItemAdjustmentNote note)
        {
            foreach (var item in note.Items)
            {
                var signedQty = item.AdjustmentType == AdjustmentType.Increase
                    ? Math.Abs(item.Quantity)
                    : -Math.Abs(item.Quantity);

                _db.StockTransactions.Add(new StockTransaction
                {
                    ProductId = item.ProductId,
                    LocationId = note.LocationId,
                    TransactionType = item.AdjustmentType == AdjustmentType.Increase
                        ? StockTransactionType.AdjustmentIncrease
                        : StockTransactionType.AdjustmentDecrease,
                    Quantity = signedQty,
                    ReferenceType = "ItemAdjustmentNote",
                    ReferenceId = note.Id,
                    ReferenceNo = note.AdjustmentNo,
                    Remarks = item.Remarks ?? note.Reason,
                    TransactionDate = note.AdjustmentDate
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task PostConsumptionAsync(ConsumptionVoucher voucher)
        {
            foreach (var item in voucher.Items)
            {
                _db.StockTransactions.Add(new StockTransaction
                {
                    ProductId = item.ProductId,
                    LocationId = voucher.LocationId,
                    TransactionType = StockTransactionType.Consumption,
                    Quantity = -Math.Abs(item.Quantity),
                    ReferenceType = "ConsumptionVoucher",
                    ReferenceId = voucher.Id,
                    ReferenceNo = voucher.VoucherNo,
                    Remarks = item.Remarks ?? voucher.Purpose,
                    TransactionDate = voucher.VoucherDate
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task PostStockMovementAsync(int productId, int locationId, decimal signedQuantity, StockTransactionType type,
            string referenceType, int referenceId, string? referenceNo, string? remarks, DateTime? transactionDate = null)
        {
            _db.StockTransactions.Add(new StockTransaction
            {
                ProductId = productId,
                LocationId = locationId,
                TransactionType = type,
                Quantity = signedQuantity,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                ReferenceNo = referenceNo,
                Remarks = remarks,
                TransactionDate = transactionDate ?? DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }

        public async Task<decimal> GetCurrentStockAsync(int productId, int locationId)
        {
            return await _db.StockTransactions
                .Where(s => s.ProductId == productId && s.LocationId == locationId)
                .SumAsync(s => (decimal?)s.Quantity) ?? 0m;
        }

        public async Task<decimal> GetCurrentStockAsync(int productId)
        {
            return await _db.StockTransactions
                .Where(s => s.ProductId == productId)
                .SumAsync(s => (decimal?)s.Quantity) ?? 0m;
        }

        public async Task<Dictionary<int, decimal>> GetCurrentStockForAllProductsAsync()
        {
            return await _db.StockTransactions
                .GroupBy(s => s.ProductId)
                .Select(g => new { ProductId = g.Key, Total = g.Sum(s => s.Quantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.Total);
        }
    }
}
