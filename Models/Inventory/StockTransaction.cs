using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Identity.Models.Inventory
{
    public enum StockTransactionType
    {
        OpeningStock = 0,
        TransferOut = 1,
        TransferIn = 2,
        AdjustmentIncrease = 3,
        AdjustmentDecrease = 4,
        Consumption = 5,
        PurchaseReceipt = 6,   // Goods Receipt Note approved — stock in
        PurchaseReturn = 7,    // Debit Note (goods returned to vendor) approved — stock out
        SalesDelivery = 8,     // Delivery Challan approved — stock out
        SalesReturn = 9        // Credit Note (goods returned by customer) approved — stock in
    }

    /// <summary>
    /// The single source of truth for stock quantities. Internal Transfers, Item
    /// Adjustment Notes and Consumption Vouchers all post rows here instead of
    /// mutating a "quantity on hand" field directly, so current stock is always
    /// computable as SUM(Quantity) per Product/Location and every movement stays
    /// traceable back to the document that caused it.
    /// </summary>
    public class StockTransaction
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int LocationId { get; set; }
        public Location? Location { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public StockTransactionType TransactionType { get; set; }

        /// <summary>Signed quantity: positive increases stock, negative decreases it.</summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        /// <summary>"Product" | "InternalTransfer" | "ItemAdjustmentNote" | "ConsumptionVoucher"</summary>
        public string ReferenceType { get; set; } = string.Empty;

        public int ReferenceId { get; set; }

        public string? ReferenceNo { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
