namespace ERP.Identity.Models.Inventory
{
    /// <summary>
    /// Shared approval workflow status for Internal Transfer, Item Adjustment
    /// Note and Consumption Voucher. A document only posts to the stock ledger
    /// once it moves from Pending to Approved.
    /// </summary>
    public enum VoucherStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}
