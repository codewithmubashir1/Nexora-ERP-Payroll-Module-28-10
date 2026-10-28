using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Sales;
using ERP.Identity.Models.ViewModels;
using ERP.Identity.Models.ViewModels.Sales;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Sales/Customers")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CustomersController(ApplicationDbContext db) => _db = db;

        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Customers
                .Include(c => c.CustomerCategory)
                .OrderByDescending(c => c.Id)
                .Select(c => new CustomerListItemViewModel
                {
                    Id = c.Id,
                    CustomerCode = c.CustomerCode,
                    Name = c.Name,
                    CategoryName = c.CustomerCategory!.Name,
                    ContactPerson = c.ContactPerson,
                    Phone = c.Phone,
                    Email = c.Email,
                    PaymentTermsDays = c.PaymentTermsDays,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new CustomerFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _db.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            var vm = new CustomerFormViewModel
            {
                Id = customer.Id,
                CustomerCode = customer.CustomerCode,
                Name = customer.Name,
                CustomerCategoryId = customer.CustomerCategoryId,
                ContactPerson = customer.ContactPerson,
                Phone = customer.Phone,
                Email = customer.Email,
                Address = customer.Address,
                City = customer.City,
                Country = customer.Country,
                NTNNumber = customer.NTNNumber,
                SalesTaxRegistrationNumber = customer.SalesTaxRegistrationNumber,
                OurVendorCodeAtCustomer = customer.OurVendorCodeAtCustomer,
                PaymentTermsDays = customer.PaymentTermsDays,
                OpeningBalance = customer.OpeningBalance,
                IsActive = customer.IsActive
            };

            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(CustomerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var isNew = model.Id == 0;
            Customer customer;

            if (isNew)
            {
                customer = new Customer { CreatedOn = DateTime.UtcNow, CustomerCode = await GenerateCustomerCodeAsync() };
                _db.Customers.Add(customer);
            }
            else
            {
                var existing = await _db.Customers.FindAsync(model.Id);
                if (existing == null) return NotFound();
                customer = existing;
            }

            customer.Name = model.Name;
            customer.CustomerCategoryId = model.CustomerCategoryId;
            customer.ContactPerson = model.ContactPerson;
            customer.Phone = model.Phone;
            customer.Email = model.Email;
            customer.Address = model.Address;
            customer.City = model.City;
            customer.Country = model.Country;
            customer.NTNNumber = model.NTNNumber;
            customer.SalesTaxRegistrationNumber = model.SalesTaxRegistrationNumber;
            customer.OurVendorCodeAtCustomer = model.OurVendorCodeAtCustomer;
            customer.PaymentTermsDays = model.PaymentTermsDays;
            customer.OpeningBalance = model.OpeningBalance;
            customer.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = isNew ? "Customer created successfully." : "Customer updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _db.Customers.FindAsync(id);
            if (customer == null) return Json(new { success = false, message = "Customer not found." });

            var isReferenced =
                await _db.SaleOrders.AnyAsync(s => s.CustomerId == id) ||
                await _db.DeliveryChallans.AnyAsync(d => d.CustomerId == id) ||
                await _db.SalesInvoices.AnyAsync(s => s.CustomerId == id);

            if (isReferenced)
                return Json(new { success = false, message = "This customer has sales activity and cannot be deleted. Deactivate it instead." });

            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Customer deleted." });
        }

        [HttpGet("Template")]
        public IActionResult Template()
        {
            var csv = CsvUtility.WriteWithHeader(
                new[] { "Name", "Category", "ContactPerson", "Phone", "Email", "Address", "City", "Country", "NTNNumber", "SalesTaxRegistrationNumber", "PaymentTermsDays", "OpeningBalance" },
                new[] { new[] { "Atlas Honda Limited", "Corporate", "Jane Doe", "+92 300 0000000", "jane@atlashonda.com", "26/27-km Lahore-Sheikhupura Road", "Sheikhupura", "Pakistan", "0801063-1", "02-06-8711-001-37", "30", "0" } });

            return File(csv, "text/csv", "customer_import_template.csv");
        }

        [HttpPost("Import")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Manager}")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            var result = new ImportResultViewModel();
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please choose a CSV file." });

            try
            {
                var rows = CsvUtility.ParseWithHeader(file.OpenReadStream());
                result.TotalRows = rows.Count;

                var categories = (await _db.CustomerCategories.ToListAsync())
                    .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
                var existingByName = (await _db.Customers.ToListAsync())
                    .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var customerCounter = await _db.Customers.CountAsync();
                var rowNum = 1;

                foreach (var row in rows)
                {
                    rowNum++;
                    var name = row.Get("Name");
                    var categoryName = row.Get("Category");

                    if (string.IsNullOrWhiteSpace(name)) { result.Errors.Add($"Row {rowNum}: Name is required."); result.ErrorCount++; continue; }
                    if (string.IsNullOrWhiteSpace(categoryName) || !categories.TryGetValue(categoryName, out var categoryId))
                    { result.Errors.Add($"Row {rowNum}: Category '{categoryName}' not found."); result.ErrorCount++; continue; }

                    int.TryParse(row.Get("PaymentTermsDays"), out var terms);
                    decimal.TryParse(row.Get("OpeningBalance"), out var opening);

                    Customer customer;
                    bool isNew;

                    if (existingByName.TryGetValue(name, out var existing))
                    {
                        customer = existing;
                        isNew = false;
                    }
                    else
                    {
                        customerCounter++;
                        customer = new Customer { CreatedOn = DateTime.UtcNow, CustomerCode = $"CUS-{customerCounter:D6}" };
                        _db.Customers.Add(customer);
                        isNew = true;
                    }

                    customer.Name = name;
                    customer.CustomerCategoryId = categoryId;
                    customer.ContactPerson = row.Get("ContactPerson");
                    customer.Phone = row.Get("Phone");
                    customer.Email = row.Get("Email");
                    customer.Address = row.Get("Address");
                    customer.City = row.Get("City");
                    customer.Country = row.Get("Country");
                    customer.NTNNumber = row.Get("NTNNumber");
                    customer.SalesTaxRegistrationNumber = row.Get("SalesTaxRegistrationNumber");
                    customer.PaymentTermsDays = terms;
                    customer.OpeningBalance = opening;
                    customer.IsActive = true;

                    if (isNew) result.CreatedCount++; else result.UpdatedCount++;
                }

                await _db.SaveChangesAsync();
                return Json(new { success = result.Success, summary = result.Summary, errorCount = result.ErrorCount, errors = result.Errors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Import failed: " + ex.Message });
            }
        }

        private async Task<string> GenerateCustomerCodeAsync()
        {
            var count = await _db.Customers.CountAsync();
            var candidate = $"CUS-{(count + 1):D6}";
            while (await _db.Customers.AnyAsync(c => c.CustomerCode == candidate))
            {
                count++;
                candidate = $"CUS-{(count + 1):D6}";
            }
            return candidate;
        }

        private async Task PopulateDropdownsAsync(CustomerFormViewModel vm)
        {
            vm.Categories = await _db.CustomerCategories.Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new CustomerCategoryOption(c.Id, c.Name))
                .ToListAsync();
        }
    }
}
