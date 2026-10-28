using ERP.Identity.Data;
using ERP.Identity.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    /// <summary>AR/AP aging — how much has been invoiced, recovered/paid, and is still outstanding, per customer and vendor.</summary>
    [Authorize]
    [Route("Accounts/InvoicesAndRecoveries")]
    public class InvoicesAndRecoveriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public InvoicesAndRecoveriesController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("GetCustomerSummary")]
        public async Task<IActionResult> GetCustomerSummary()
        {
            var customers = await _db.Customers.Where(c => c.IsActive).ToListAsync();

            var invoices = await _db.SalesInvoices.Where(i => i.Status == VoucherStatus.Approved).ToListAsync();
            var payments = await _db.SalesInvoicePayments.Where(p => p.Status == VoucherStatus.Approved).ToListAsync();
            var creditNotes = await _db.CreditNotes.Where(c => c.Status == VoucherStatus.Approved).ToListAsync();

            var today = DateTime.UtcNow.Date;

            var result = customers.Select(c =>
            {
                var customerInvoices = invoices.Where(i => i.CustomerId == c.Id).ToList();
                var totalInvoiced = customerInvoices.Sum(i => i.TotalAmount);
                var totalRecovered = payments.Where(p => p.CustomerId == c.Id).Sum(p => p.Amount)
                                    + creditNotes.Where(cn => cn.CustomerId == c.Id).Sum(cn => cn.TotalAmount);
                var outstanding = totalInvoiced - totalRecovered;

                var oldestUnpaidDate = customerInvoices
                    .Where(i => i.DueDate.HasValue && i.DueDate.Value < today)
                    .OrderBy(i => i.DueDate)
                    .Select(i => (DateTime?)i.DueDate!.Value)
                    .FirstOrDefault();

                var daysOverdue = oldestUnpaidDate.HasValue && outstanding > 0 ? (today - oldestUnpaidDate.Value).Days : 0;

                return new
                {
                    c.Id,
                    c.CustomerCode,
                    Name = c.Name,
                    TotalInvoiced = totalInvoiced,
                    TotalRecovered = totalRecovered,
                    Outstanding = outstanding,
                    DaysOverdue = daysOverdue
                };
            })
            .Where(r => r.TotalInvoiced > 0)
            .OrderByDescending(r => r.Outstanding)
            .ToList();

            return Json(new { data = result });
        }

        [HttpGet("GetVendorSummary")]
        public async Task<IActionResult> GetVendorSummary()
        {
            var vendors = await _db.Vendors.Where(v => v.IsActive).ToListAsync();

            var invoices = await _db.PurchaseInvoices.Where(i => i.Status == VoucherStatus.Approved).ToListAsync();
            var payments = await _db.PurchaseInvoicePayments.Where(p => p.Status == VoucherStatus.Approved).ToListAsync();
            var debitNotes = await _db.DebitNotes.Where(d => d.Status == VoucherStatus.Approved).ToListAsync();

            var today = DateTime.UtcNow.Date;

            var result = vendors.Select(v =>
            {
                var vendorInvoices = invoices.Where(i => i.VendorId == v.Id).ToList();
                var totalInvoiced = vendorInvoices.Sum(i => i.NetPayableAmount);
                var totalRecovered = payments.Where(p => p.VendorId == v.Id).Sum(p => p.Amount)
                                    + debitNotes.Where(d => d.VendorId == v.Id).Sum(d => d.TotalAmount);
                var outstanding = totalInvoiced - totalRecovered;

                var oldestUnpaidDate = vendorInvoices
                    .Where(i => i.DueDate.HasValue && i.DueDate.Value < today)
                    .OrderBy(i => i.DueDate)
                    .Select(i => (DateTime?)i.DueDate!.Value)
                    .FirstOrDefault();

                var daysOverdue = oldestUnpaidDate.HasValue && outstanding > 0 ? (today - oldestUnpaidDate.Value).Days : 0;

                return new
                {
                    v.Id,
                    v.VendorCode,
                    Name = v.Name,
                    TotalInvoiced = totalInvoiced,
                    TotalRecovered = totalRecovered,
                    Outstanding = outstanding,
                    DaysOverdue = daysOverdue
                };
            })
            .Where(r => r.TotalInvoiced > 0)
            .OrderByDescending(r => r.Outstanding)
            .ToList();

            return Json(new { data = result });
        }
    }
}
