using ERP.Identity.Models;
using ERP.Identity.Models.Inventory;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Data
{
    /// <summary>
    /// Seeds the six ERP roles, one default Admin account, and a handful of
    /// Inventory lookups so the module is usable immediately after
    /// `dotnet ef database update`.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var db = services.GetRequiredService<ApplicationDbContext>();

            foreach (var role in AppRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = config["SeedAdmin:Email"] ?? "admin@erp.local";
            var adminPassword = config["SeedAdmin:Password"] ?? "Admin@12345";
            var adminFullName = config["SeedAdmin:FullName"] ?? "System Administrator";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = adminFullName,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, AppRoles.Admin);
                }
            }

            await SeedInventoryAsync(db);
            await SeedPurchaseAsync(db);
            await SeedSalesAsync(db);
            await SeedAccountsAsync(db);
            await SeedPayrollAsync(db);
        }

        private static async Task SeedInventoryAsync(ApplicationDbContext db)
        {
            if (!await db.UnitsOfMeasure.AnyAsync())
            {
                db.UnitsOfMeasure.AddRange(
                    new UnitOfMeasure { Name = "Pieces", ShortName = "Pcs" },
                    new UnitOfMeasure { Name = "Kilogram", ShortName = "Kg" },
                    new UnitOfMeasure { Name = "Litre", ShortName = "Ltr" },
                    new UnitOfMeasure { Name = "Box", ShortName = "Box" },
                    new UnitOfMeasure { Name = "Carton", ShortName = "Ctn" },
                    new UnitOfMeasure { Name = "Dozen", ShortName = "Dzn" },
                    new UnitOfMeasure { Name = "Meter", ShortName = "Mtr" },
                    new UnitOfMeasure { Name = "Pack", ShortName = "Pack" }
                );
            }

            if (!await db.Locations.AnyAsync())
            {
                db.Locations.Add(new Location { Name = "Main Warehouse", Code = "WH-MAIN", IsActive = true });
            }

            if (!await db.ProductCategories.AnyAsync())
            {
                db.ProductCategories.AddRange(
                    new ProductCategory { Name = "General", Code = "GEN" },
                    new ProductCategory { Name = "Raw Material", Code = "RM" },
                    new ProductCategory { Name = "Finished Goods", Code = "FG" },
                    new ProductCategory { Name = "Consumables", Code = "CON" }
                );
            }

            await db.SaveChangesAsync();
        }

        private static async Task SeedPurchaseAsync(ApplicationDbContext db)
        {
            if (!await db.VendorCategories.AnyAsync())
            {
                db.VendorCategories.AddRange(
                    new Models.Purchase.VendorCategory { Name = "Raw Material Supplier", Code = "RMS" },
                    new Models.Purchase.VendorCategory { Name = "Service Provider", Code = "SRV" },
                    new Models.Purchase.VendorCategory { Name = "Equipment Supplier", Code = "EQP" },
                    new Models.Purchase.VendorCategory { Name = "General", Code = "GEN" }
                );
                await db.SaveChangesAsync();
            }
        }

        private static async Task SeedSalesAsync(ApplicationDbContext db)
        {
            if (!await db.CustomerCategories.AnyAsync())
            {
                db.CustomerCategories.AddRange(
                    new Models.Sales.CustomerCategory { Name = "Retail", Code = "RTL" },
                    new Models.Sales.CustomerCategory { Name = "Wholesale", Code = "WHL" },
                    new Models.Sales.CustomerCategory { Name = "Corporate", Code = "CRP" },
                    new Models.Sales.CustomerCategory { Name = "General", Code = "GEN" }
                );
                await db.SaveChangesAsync();
            }
        }

        private static async Task SeedAccountsAsync(ApplicationDbContext db)
        {
            if (!await db.Accounts.AnyAsync())
            {
                db.Accounts.AddRange(
                    new Models.Accounts.Account { AccountCode = "1000", AccountName = "Cash in Hand", AccountType = Models.Accounts.AccountType.Asset, IsSystemAccount = true },
                    new Models.Accounts.Account { AccountCode = "1010", AccountName = "Bank Account", AccountType = Models.Accounts.AccountType.Asset, IsSystemAccount = true },
                    new Models.Accounts.Account { AccountCode = "1100", AccountName = "Accounts Receivable", AccountType = Models.Accounts.AccountType.Asset, IsSystemAccount = true },
                    new Models.Accounts.Account { AccountCode = "1200", AccountName = "Inventory", AccountType = Models.Accounts.AccountType.Asset, IsSystemAccount = true },
                    new Models.Accounts.Account { AccountCode = "2000", AccountName = "Accounts Payable", AccountType = Models.Accounts.AccountType.Liability, IsSystemAccount = true },
                    new Models.Accounts.Account { AccountCode = "2100", AccountName = "Withholding Tax Payable", AccountType = Models.Accounts.AccountType.Liability, IsSystemAccount = true },
                    new Models.Accounts.Account { AccountCode = "3000", AccountName = "Owner's Equity", AccountType = Models.Accounts.AccountType.Equity, IsSystemAccount = true },
                    new Models.Accounts.Account { AccountCode = "4000", AccountName = "Sales Revenue", AccountType = Models.Accounts.AccountType.Income, IsSystemAccount = true },
                    new Models.Accounts.Account { AccountCode = "4900", AccountName = "Other Income", AccountType = Models.Accounts.AccountType.Income },
                    new Models.Accounts.Account { AccountCode = "5000", AccountName = "Cost of Goods Sold", AccountType = Models.Accounts.AccountType.Expense, IsSystemAccount = true },
                    new Models.Accounts.Account { AccountCode = "5100", AccountName = "Inventory Write-off Expense", AccountType = Models.Accounts.AccountType.Expense, IsSystemAccount = true },
                    new Models.Accounts.Account { AccountCode = "5200", AccountName = "Salaries & Wages", AccountType = Models.Accounts.AccountType.Expense },
                    new Models.Accounts.Account { AccountCode = "5300", AccountName = "Rent Expense", AccountType = Models.Accounts.AccountType.Expense },
                    new Models.Accounts.Account { AccountCode = "5400", AccountName = "Utilities Expense", AccountType = Models.Accounts.AccountType.Expense },
                    new Models.Accounts.Account { AccountCode = "5500", AccountName = "Office Supplies Expense", AccountType = Models.Accounts.AccountType.Expense },
                    new Models.Accounts.Account { AccountCode = "5900", AccountName = "Miscellaneous Expense", AccountType = Models.Accounts.AccountType.Expense }
                );
                await db.SaveChangesAsync();
            }

            var payrollAccounts = new[]
            {
                (Code: "2200", Name: "Payroll Payable", Type: Models.Accounts.AccountType.Liability),
                (Code: "2110", Name: "Payroll Deductions Payable", Type: Models.Accounts.AccountType.Liability),
                (Code: "5200", Name: "Salaries & Wages", Type: Models.Accounts.AccountType.Expense)
            };
            foreach (var account in payrollAccounts)
            {
                if (await db.Accounts.AnyAsync(existing => existing.AccountName == account.Name)) continue;
                var code = account.Code;
                var suffix = 1;
                while (await db.Accounts.AnyAsync(existing => existing.AccountCode == code))
                    code = $"{account.Code}-{suffix++:D2}";
                db.Accounts.Add(new Models.Accounts.Account
                {
                    AccountCode = code,
                    AccountName = account.Name,
                    AccountType = account.Type,
                    IsSystemAccount = true
                });
            }
            await db.SaveChangesAsync();
        }

        private static async Task SeedPayrollAsync(ApplicationDbContext db)
        {
            if (!await db.Departments.AnyAsync())
            {
                db.Departments.AddRange(
                    new Models.Payroll.Department { Name = "Administration", Code = "ADM" },
                    new Models.Payroll.Department { Name = "Finance & Accounts", Code = "FIN" },
                    new Models.Payroll.Department { Name = "Sales & Marketing", Code = "SLS" },
                    new Models.Payroll.Department { Name = "Warehouse & Inventory", Code = "WHS" },
                    new Models.Payroll.Department { Name = "Human Resources", Code = "HR" }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.ShiftTimings.AnyAsync())
            {
                db.ShiftTimings.AddRange(
                    new Models.Payroll.ShiftTiming { Name = "08 Hours", StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(17, 0, 0), BreakMinutes = 60, GracePeriodMinutes = 15 },
                    new Models.Payroll.ShiftTiming { Name = "10 Hours", StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(19, 0, 0), BreakMinutes = 60, GracePeriodMinutes = 15 },
                    new Models.Payroll.ShiftTiming { Name = "12 Hours", StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(20, 0, 0), BreakMinutes = 0, GracePeriodMinutes = 15 }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.LeaveTypes.AnyAsync())
            {
                db.LeaveTypes.AddRange(
                    new Models.Payroll.LeaveType { Name = "Annual Leave", DaysPerYear = 14, IsPaid = true, IsEncashable = true },
                    new Models.Payroll.LeaveType { Name = "Sick Leave", DaysPerYear = 10, IsPaid = true, IsEncashable = false },
                    new Models.Payroll.LeaveType { Name = "Casual Leave", DaysPerYear = 8, IsPaid = true, IsEncashable = false },
                    new Models.Payroll.LeaveType { Name = "Unpaid Leave", DaysPerYear = 0, IsPaid = false, IsEncashable = false }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.ReimbursementTypes.AnyAsync())
            {
                db.ReimbursementTypes.AddRange(
                    new Models.Payroll.ReimbursementType { Name = "Travel & Conveyance" },
                    new Models.Payroll.ReimbursementType { Name = "Medical" },
                    new Models.Payroll.ReimbursementType { Name = "Office Supplies" },
                    new Models.Payroll.ReimbursementType { Name = "Client Entertainment" }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.TaxSlabs.AnyAsync())
            {
                // Illustrative starter slabs — replace via Settings to match your jurisdiction's actual tax law.
                db.TaxSlabs.AddRange(
                    new Models.Payroll.TaxSlab { MinIncome = 0, MaxIncome = 50000, RatePercent = 0, FixedAmount = 0 },
                    new Models.Payroll.TaxSlab { MinIncome = 50000, MaxIncome = 100000, RatePercent = 5, FixedAmount = 0 },
                    new Models.Payroll.TaxSlab { MinIncome = 100000, MaxIncome = 300000, RatePercent = 12.5m, FixedAmount = 2500 },
                    new Models.Payroll.TaxSlab { MinIncome = 300000, MaxIncome = null, RatePercent = 20, FixedAmount = 27500 }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.StatutoryDeductions.AnyAsync())
            {
                db.StatutoryDeductions.AddRange(
                    new Models.Payroll.StatutoryDeduction { Name = "EOBI", RatePercent = 1, MaxApplicableSalary = 29000 },
                    new Models.Payroll.StatutoryDeduction { Name = "Provident Fund", RatePercent = 8.33m, MaxApplicableSalary = null }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.AttendancePolicies.AnyAsync())
            {
                db.AttendancePolicies.AddRange(
                    new Models.Payroll.AttendancePolicy { PolicyType = Models.Payroll.AttendancePolicyType.LateComing, GracePeriodMinutes = 15, MaxOccurrencesPerMonth = 3, PenaltyOnExceeding = Models.Payroll.PenaltyType.Warning },
                    new Models.Payroll.AttendancePolicy { PolicyType = Models.Payroll.AttendancePolicyType.LateSitting, GracePeriodMinutes = 30, MaxOccurrencesPerMonth = 0, PenaltyOnExceeding = Models.Payroll.PenaltyType.None },
                    new Models.Payroll.AttendancePolicy { PolicyType = Models.Payroll.AttendancePolicyType.EarlyGoing, GracePeriodMinutes = 15, MaxOccurrencesPerMonth = 3, PenaltyOnExceeding = Models.Payroll.PenaltyType.Warning },
                    new Models.Payroll.AttendancePolicy { PolicyType = Models.Payroll.AttendancePolicyType.HolidayWorking, CompensationType = "CompOff", OvertimeRateMultiplier = 2 }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}
