using ERP.Identity.Models;
using ERP.Identity.Models.Accounts;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.Payroll;
using ERP.Identity.Models.Purchase;
using ERP.Identity.Models.Sales;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ---------------- Inventory module ----------------
        public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
        public DbSet<InternalTransfer> InternalTransfers => Set<InternalTransfer>();
        public DbSet<InternalTransferItem> InternalTransferItems => Set<InternalTransferItem>();
        public DbSet<ItemAdjustmentNote> ItemAdjustmentNotes => Set<ItemAdjustmentNote>();
        public DbSet<ItemAdjustmentNoteItem> ItemAdjustmentNoteItems => Set<ItemAdjustmentNoteItem>();
        public DbSet<ConsumptionVoucher> ConsumptionVouchers => Set<ConsumptionVoucher>();
        public DbSet<ConsumptionVoucherItem> ConsumptionVoucherItems => Set<ConsumptionVoucherItem>();
        public DbSet<CustomerProductAlias> CustomerProductAliases => Set<CustomerProductAlias>();
        public DbSet<ProductPriceHistory> ProductPriceHistories => Set<ProductPriceHistory>();

        public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();

        // ---------------- Purchase module ----------------
        public DbSet<VendorCategory> VendorCategories => Set<VendorCategory>();
        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
        public DbSet<InwardGatePass> InwardGatePasses => Set<InwardGatePass>();
        public DbSet<InwardGatePassItem> InwardGatePassItems => Set<InwardGatePassItem>();
        public DbSet<GoodsReceiptNote> GoodsReceiptNotes => Set<GoodsReceiptNote>();
        public DbSet<GoodsReceiptNoteItem> GoodsReceiptNoteItems => Set<GoodsReceiptNoteItem>();
        public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
        public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
        public DbSet<PurchaseInvoicePayment> PurchaseInvoicePayments => Set<PurchaseInvoicePayment>();
        public DbSet<DebitNote> DebitNotes => Set<DebitNote>();
        public DbSet<DebitNoteItem> DebitNoteItems => Set<DebitNoteItem>();
        public DbSet<IssuedCheque> IssuedCheques => Set<IssuedCheque>();
        public DbSet<VendorAdvance> VendorAdvances => Set<VendorAdvance>();

        // ---------------- Sales module ----------------
        public DbSet<CustomerCategory> CustomerCategories => Set<CustomerCategory>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<SaleOrder> SaleOrders => Set<SaleOrder>();
        public DbSet<SaleOrderItem> SaleOrderItems => Set<SaleOrderItem>();
        public DbSet<DeliveryChallan> DeliveryChallans => Set<DeliveryChallan>();
        public DbSet<DeliveryChallanItem> DeliveryChallanItems => Set<DeliveryChallanItem>();
        public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
        public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
        public DbSet<SalesInvoicePayment> SalesInvoicePayments => Set<SalesInvoicePayment>();
        public DbSet<CreditNote> CreditNotes => Set<CreditNote>();
        public DbSet<CreditNoteItem> CreditNoteItems => Set<CreditNoteItem>();

        // ---------------- Accounts module ----------------
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
        public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
        public DbSet<JournalVoucher> JournalVouchers => Set<JournalVoucher>();
        public DbSet<JournalVoucherLine> JournalVoucherLines => Set<JournalVoucherLine>();
        public DbSet<CashPaymentVoucher> CashPaymentVouchers => Set<CashPaymentVoucher>();
        public DbSet<CashReceiptVoucher> CashReceiptVouchers => Set<CashReceiptVoucher>();
        public DbSet<BankPaymentVoucher> BankPaymentVouchers => Set<BankPaymentVoucher>();
        public DbSet<BankReceiptVoucher> BankReceiptVouchers => Set<BankReceiptVoucher>();
        public DbSet<InventoryVoucher> InventoryVouchers => Set<InventoryVoucher>();

        // ---------------- Payroll module ----------------
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<ShiftTiming> ShiftTimings => Set<ShiftTiming>();
        public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
        public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
        public DbSet<Holiday> Holidays => Set<Holiday>();
        public DbSet<ReimbursementType> ReimbursementTypes => Set<ReimbursementType>();
        public DbSet<TaxSlab> TaxSlabs => Set<TaxSlab>();
        public DbSet<StatutoryDeduction> StatutoryDeductions => Set<StatutoryDeduction>();
        public DbSet<SalaryIncrement> SalaryIncrements => Set<SalaryIncrement>();
        public DbSet<AttendancePolicy> AttendancePolicies => Set<AttendancePolicy>();
        public DbSet<OtherPayrollSettings> OtherPayrollSettings => Set<OtherPayrollSettings>();
        public DbSet<AttendanceDevice> AttendanceDevices => Set<AttendanceDevice>();
        public DbSet<MachineIntegrationSetting> MachineIntegrationSettings => Set<MachineIntegrationSetting>();
        public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
        public DbSet<LoanAdvanceRequest> LoanAdvanceRequests => Set<LoanAdvanceRequest>();
        public DbSet<MidSalaryRequest> MidSalaryRequests => Set<MidSalaryRequest>();
        public DbSet<LeaveEncashmentRequest> LeaveEncashmentRequests => Set<LeaveEncashmentRequest>();
        public DbSet<ReimbursementRequest> ReimbursementRequests => Set<ReimbursementRequest>();
        public DbSet<PayRun> PayRuns => Set<PayRun>();
        public DbSet<Payslip> Payslips => Set<Payslip>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.FullName).HasMaxLength(150).IsRequired();
                b.Property(u => u.EmployeeCode).HasMaxLength(50);
                b.Property(u => u.Designation).HasMaxLength(100);
                b.Property(u => u.Department).HasMaxLength(100);
                b.Property(u => u.BloodGroup).HasMaxLength(3);
            });

            ConfigureInventory(builder);
            ConfigurePurchase(builder);
            ConfigureSales(builder);
            ConfigureAccounts(builder);
            ConfigurePayroll(builder);

            // The rest of the ERP schema (Sales, Purchases, Accounts, Payroll,
            // Chart of Accounts, Company, Settings) will be added the same way
            // as additional modules are built.
        }

        private static void ConfigureInventory(ModelBuilder builder)
        {
            builder.Entity<Product>(b =>
            {
                b.HasIndex(p => p.Sku).IsUnique();
                b.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(p => p.Unit)
                    .WithMany()
                    .HasForeignKey(p => p.UnitId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(p => p.DefaultLocation)
                    .WithMany()
                    .HasForeignKey(p => p.DefaultLocationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<StockTransaction>(b =>
            {
                b.HasOne(s => s.Product)
                    .WithMany(p => p.StockTransactions)
                    .HasForeignKey(s => s.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.Location)
                    .WithMany()
                    .HasForeignKey(s => s.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasIndex(s => new { s.ProductId, s.LocationId });
            });

            builder.Entity<InternalTransfer>(b =>
            {
                b.HasIndex(t => t.TransferNo).IsUnique();
                b.HasOne(t => t.FromLocation).WithMany().HasForeignKey(t => t.FromLocationId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(t => t.ToLocation).WithMany().HasForeignKey(t => t.ToLocationId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<InternalTransferItem>(b =>
            {
                b.HasOne(i => i.InternalTransfer).WithMany(t => t.Items).HasForeignKey(i => i.InternalTransferId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ItemAdjustmentNote>(b =>
            {
                b.HasIndex(n => n.AdjustmentNo).IsUnique();
                b.HasOne(n => n.Location).WithMany().HasForeignKey(n => n.LocationId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ItemAdjustmentNoteItem>(b =>
            {
                b.HasOne(i => i.ItemAdjustmentNote).WithMany(n => n.Items).HasForeignKey(i => i.ItemAdjustmentNoteId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ConsumptionVoucher>(b =>
            {
                b.HasIndex(v => v.VoucherNo).IsUnique();
                b.HasOne(v => v.Location).WithMany().HasForeignKey(v => v.LocationId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ConsumptionVoucherItem>(b =>
            {
                b.HasOne(i => i.ConsumptionVoucher).WithMany(v => v.Items).HasForeignKey(i => i.ConsumptionVoucherId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CustomerProductAlias>(b =>
            {
                b.HasOne(a => a.Product).WithMany().HasForeignKey(a => a.ProductId).OnDelete(DeleteBehavior.Restrict);
                b.HasIndex(a => new { a.CustomerName, a.ProductId });
            });

            builder.Entity<ProductPriceHistory>(b =>
            {
                b.HasOne(h => h.Product).WithMany().HasForeignKey(h => h.ProductId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(h => h.ProductId);
            });
        }

        private static void ConfigurePurchase(ModelBuilder builder)
        {
            builder.Entity<Vendor>(b =>
            {
                b.HasIndex(v => v.VendorCode).IsUnique();
                b.HasOne(v => v.VendorCategory).WithMany(c => c.Vendors).HasForeignKey(v => v.VendorCategoryId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PurchaseOrder>(b =>
            {
                b.HasIndex(p => p.PONumber).IsUnique();
                b.HasOne(p => p.Vendor).WithMany().HasForeignKey(p => p.VendorId).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<PurchaseOrderItem>(b =>
            {
                b.HasOne(i => i.PurchaseOrder).WithMany(p => p.Items).HasForeignKey(i => i.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<InwardGatePass>(b =>
            {
                b.HasIndex(g => g.GatePassNo).IsUnique();
                b.HasOne(g => g.Vendor).WithMany().HasForeignKey(g => g.VendorId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(g => g.PurchaseOrder).WithMany().HasForeignKey(g => g.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<InwardGatePassItem>(b =>
            {
                b.HasOne(i => i.InwardGatePass).WithMany(g => g.Items).HasForeignKey(i => i.InwardGatePassId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<GoodsReceiptNote>(b =>
            {
                b.HasIndex(g => g.GRNNumber).IsUnique();
                b.HasOne(g => g.Vendor).WithMany().HasForeignKey(g => g.VendorId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(g => g.PurchaseOrder).WithMany().HasForeignKey(g => g.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(g => g.InwardGatePass).WithMany().HasForeignKey(g => g.InwardGatePassId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(g => g.Location).WithMany().HasForeignKey(g => g.LocationId).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<GoodsReceiptNoteItem>(b =>
            {
                b.HasOne(i => i.GoodsReceiptNote).WithMany(g => g.Items).HasForeignKey(i => i.GoodsReceiptNoteId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PurchaseInvoice>(b =>
            {
                b.HasIndex(p => p.InvoiceNumber).IsUnique();
                b.HasOne(p => p.Vendor).WithMany().HasForeignKey(p => p.VendorId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(p => p.PurchaseOrder).WithMany().HasForeignKey(p => p.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(p => p.GoodsReceiptNote).WithMany().HasForeignKey(p => p.GoodsReceiptNoteId).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<PurchaseInvoiceItem>(b =>
            {
                b.HasOne(i => i.PurchaseInvoice).WithMany(p => p.Items).HasForeignKey(i => i.PurchaseInvoiceId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<IssuedCheque>(b =>
            {
                b.HasIndex(c => c.ChequeReferenceNo).IsUnique();
                b.HasOne(c => c.Vendor).WithMany().HasForeignKey(c => c.VendorId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<VendorAdvance>(b =>
            {
                b.HasIndex(a => a.AdvanceNumber).IsUnique();
                b.HasOne(a => a.Vendor).WithMany().HasForeignKey(a => a.VendorId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(a => a.IssuedCheque).WithMany().HasForeignKey(a => a.IssuedChequeId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PurchaseInvoicePayment>(b =>
            {
                b.HasIndex(p => p.PaymentNumber).IsUnique();
                b.HasOne(p => p.Vendor).WithMany().HasForeignKey(p => p.VendorId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(p => p.PurchaseInvoice).WithMany().HasForeignKey(p => p.PurchaseInvoiceId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(p => p.IssuedCheque).WithMany().HasForeignKey(p => p.IssuedChequeId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(p => p.VendorAdvance).WithMany().HasForeignKey(p => p.VendorAdvanceId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<DebitNote>(b =>
            {
                b.HasIndex(d => d.DebitNoteNumber).IsUnique();
                b.HasOne(d => d.Vendor).WithMany().HasForeignKey(d => d.VendorId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(d => d.PurchaseInvoice).WithMany().HasForeignKey(d => d.PurchaseInvoiceId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(d => d.GoodsReceiptNote).WithMany().HasForeignKey(d => d.GoodsReceiptNoteId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(d => d.Location).WithMany().HasForeignKey(d => d.LocationId).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<DebitNoteItem>(b =>
            {
                b.HasOne(i => i.DebitNote).WithMany(d => d.Items).HasForeignKey(i => i.DebitNoteId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureSales(ModelBuilder builder)
        {
            builder.Entity<Customer>(b =>
            {
                b.HasIndex(c => c.CustomerCode).IsUnique();
                b.HasOne(c => c.CustomerCategory).WithMany(cc => cc.Customers).HasForeignKey(c => c.CustomerCategoryId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SaleOrder>(b =>
            {
                b.HasIndex(s => s.SONumber).IsUnique();
                b.HasOne(s => s.Customer).WithMany().HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<SaleOrderItem>(b =>
            {
                b.HasOne(i => i.SaleOrder).WithMany(s => s.Items).HasForeignKey(i => i.SaleOrderId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<DeliveryChallan>(b =>
            {
                b.HasIndex(d => d.ChallanNo).IsUnique();
                b.HasOne(d => d.Customer).WithMany().HasForeignKey(d => d.CustomerId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(d => d.SaleOrder).WithMany().HasForeignKey(d => d.SaleOrderId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(d => d.Location).WithMany().HasForeignKey(d => d.LocationId).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<DeliveryChallanItem>(b =>
            {
                b.HasOne(i => i.DeliveryChallan).WithMany(d => d.Items).HasForeignKey(i => i.DeliveryChallanId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SalesInvoice>(b =>
            {
                b.HasIndex(s => s.InvoiceNumber).IsUnique();
                b.HasOne(s => s.Customer).WithMany().HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.SaleOrder).WithMany().HasForeignKey(s => s.SaleOrderId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.DeliveryChallan).WithMany().HasForeignKey(s => s.DeliveryChallanId).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<SalesInvoiceItem>(b =>
            {
                b.HasOne(i => i.SalesInvoice).WithMany(s => s.Items).HasForeignKey(i => i.SalesInvoiceId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SalesInvoicePayment>(b =>
            {
                b.HasIndex(p => p.PaymentNumber).IsUnique();
                b.HasOne(p => p.Customer).WithMany().HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(p => p.SalesInvoice).WithMany().HasForeignKey(p => p.SalesInvoiceId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CreditNote>(b =>
            {
                b.HasIndex(c => c.CreditNoteNumber).IsUnique();
                b.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(c => c.SalesInvoice).WithMany().HasForeignKey(c => c.SalesInvoiceId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(c => c.DeliveryChallan).WithMany().HasForeignKey(c => c.DeliveryChallanId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(c => c.Location).WithMany().HasForeignKey(c => c.LocationId).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<CreditNoteItem>(b =>
            {
                b.HasOne(i => i.CreditNote).WithMany(c => c.Items).HasForeignKey(i => i.CreditNoteId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureAccounts(ModelBuilder builder)
        {
            builder.Entity<Account>(b =>
            {
                b.HasIndex(a => a.AccountCode).IsUnique();
                b.HasOne(a => a.ParentAccount).WithMany(a => a.ChildAccounts).HasForeignKey(a => a.ParentAccountId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Employee>(b =>
            {
                b.HasIndex(e => e.EmployeeCode).IsUnique();
            });

            builder.Entity<JournalEntry>(b =>
            {
                b.HasIndex(j => j.EntryNumber).IsUnique();
            });
            builder.Entity<JournalEntryLine>(b =>
            {
                b.HasOne(l => l.JournalEntry).WithMany(j => j.Lines).HasForeignKey(l => l.JournalEntryId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(l => l.Account).WithMany().HasForeignKey(l => l.AccountId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<JournalVoucher>(b =>
            {
                b.HasIndex(j => j.VoucherNumber).IsUnique();
            });
            builder.Entity<JournalVoucherLine>(b =>
            {
                b.HasOne(l => l.JournalVoucher).WithMany(j => j.Lines).HasForeignKey(l => l.JournalVoucherId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(l => l.Account).WithMany().HasForeignKey(l => l.AccountId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CashPaymentVoucher>(b =>
            {
                b.HasIndex(c => c.VoucherNumber).IsUnique();
                b.HasOne(c => c.CashAccount).WithMany().HasForeignKey(c => c.CashAccountId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(c => c.ContraAccount).WithMany().HasForeignKey(c => c.ContraAccountId).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<CashReceiptVoucher>(b =>
            {
                b.HasIndex(c => c.VoucherNumber).IsUnique();
                b.HasOne(c => c.CashAccount).WithMany().HasForeignKey(c => c.CashAccountId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(c => c.ContraAccount).WithMany().HasForeignKey(c => c.ContraAccountId).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<BankPaymentVoucher>(b =>
            {
                b.HasIndex(c => c.VoucherNumber).IsUnique();
                b.HasOne(c => c.BankAccount).WithMany().HasForeignKey(c => c.BankAccountId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(c => c.ContraAccount).WithMany().HasForeignKey(c => c.ContraAccountId).OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<BankReceiptVoucher>(b =>
            {
                b.HasIndex(c => c.VoucherNumber).IsUnique();
                b.HasOne(c => c.BankAccount).WithMany().HasForeignKey(c => c.BankAccountId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(c => c.ContraAccount).WithMany().HasForeignKey(c => c.ContraAccountId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<InventoryVoucher>(b =>
            {
                b.HasIndex(i => i.VoucherNumber).IsUnique();
                b.HasOne(i => i.InventoryAccount).WithMany().HasForeignKey(i => i.InventoryAccountId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(i => i.ContraAccount).WithMany().HasForeignKey(i => i.ContraAccountId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(i => i.ItemAdjustmentNote).WithMany().HasForeignKey(i => i.ItemAdjustmentNoteId).OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigurePayroll(ModelBuilder builder)
        {
            builder.Entity<Department>(b =>
            {
                b.HasOne(d => d.ParentDepartment).WithMany(d => d.ChildDepartments).HasForeignKey(d => d.ParentDepartmentId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(d => d.HeadEmployee).WithMany().HasForeignKey(d => d.HeadEmployeeId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Employee>(b =>
            {
                b.HasOne(e => e.DepartmentRef).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(e => e.ShiftTiming).WithMany().HasForeignKey(e => e.ShiftTimingId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AttendanceRecord>(b =>
            {
                b.HasOne(a => a.Employee).WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                b.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();
            });

            builder.Entity<SalaryIncrement>(b =>
            {
                b.HasOne(s => s.Employee).WithMany().HasForeignKey(s => s.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<LeaveRequest>(b =>
            {
                b.HasIndex(l => l.RequestNumber).IsUnique();
                b.HasOne(l => l.Employee).WithMany().HasForeignKey(l => l.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(l => l.LeaveType).WithMany().HasForeignKey(l => l.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<LoanAdvanceRequest>(b =>
            {
                b.HasIndex(l => l.RequestNumber).IsUnique();
                b.HasOne(l => l.Employee).WithMany().HasForeignKey(l => l.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<MidSalaryRequest>(b =>
            {
                b.HasIndex(m => m.RequestNumber).IsUnique();
                b.HasOne(m => m.Employee).WithMany().HasForeignKey(m => m.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<LeaveEncashmentRequest>(b =>
            {
                b.HasIndex(l => l.RequestNumber).IsUnique();
                b.HasOne(l => l.Employee).WithMany().HasForeignKey(l => l.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(l => l.LeaveType).WithMany().HasForeignKey(l => l.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ReimbursementRequest>(b =>
            {
                b.HasIndex(r => r.RequestNumber).IsUnique();
                b.HasOne(r => r.Employee).WithMany().HasForeignKey(r => r.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(r => r.ReimbursementType).WithMany().HasForeignKey(r => r.ReimbursementTypeId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PayRun>(b =>
            {
                b.HasIndex(p => p.RunNumber).IsUnique();
            });

            builder.Entity<Payslip>(b =>
            {
                b.HasIndex(p => p.PayslipNumber).IsUnique();
                b.HasOne(p => p.PayRun).WithMany(r => r.Payslips).HasForeignKey(p => p.PayRunId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(p => p.Employee).WithMany().HasForeignKey(p => p.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
