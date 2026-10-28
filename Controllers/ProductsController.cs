using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.Inventory;
using ERP.Identity.Models.ViewModels;
using ERP.Identity.Models.ViewModels.Inventory;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Identity.Controllers
{
    [Authorize]
    [Route("Inventory/Products")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileUploadService _fileUploadService;
        private readonly IStockService _stockService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(ApplicationDbContext db, IFileUploadService fileUploadService, IStockService stockService, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _fileUploadService = fileUploadService;
            _stockService = stockService;
            _userManager = userManager;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _db.ProductCategories.Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new ProductCategoryOption(c.Id, c.Name))
                .ToListAsync();

            return View();
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var products = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            var stockByProduct = await _stockService.GetCurrentStockForAllProductsAsync();

            // Flag duplicate product codes (case-insensitive, blank codes ignored) so the
            // "duplicate SKU/code" filter on the Products page can work without a round trip.
            var duplicateCodes = products
                .Where(p => !string.IsNullOrWhiteSpace(p.ProductCode))
                .GroupBy(p => p.ProductCode!.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var list = products.Select(p =>
            {
                var stock = stockByProduct.TryGetValue(p.Id, out var qty) ? qty : 0;
                var stockStatus = stock <= 0 ? "Out" : (p.ReorderLevel > 0 && stock <= p.ReorderLevel ? "Low" : "Normal");

                return new ProductListItemViewModel
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    ProductCode = p.ProductCode,
                    HsCode = p.HsCode,
                    Name = p.Name,
                    CategoryName = p.Category?.Name ?? "—",
                    CategoryId = p.CategoryId,
                    UnitShortName = p.Unit?.ShortName ?? "—",
                    CostPrice = p.CostPrice,
                    SalePrice = p.SalePrice,
                    ReorderLevel = p.ReorderLevel,
                    CurrentStock = stock,
                    StockStatus = stockStatus,
                    IsActive = p.IsActive,
                    IsDuplicate = !string.IsNullOrWhiteSpace(p.ProductCode) && duplicateCodes.Contains(p.ProductCode.Trim()),
                    ProductImagePath = p.ProductImagePath
                };
            }).ToList();

            return Json(new { data = list });
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new ProductFormViewModel();
            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            var vm = new ProductFormViewModel
            {
                Id = product.Id,
                Sku = product.Sku,
                ProductCode = product.ProductCode,
                HsCode = product.HsCode,
                Name = product.Name,
                CategoryId = product.CategoryId,
                UnitId = product.UnitId,
                CostPrice = product.CostPrice,
                SalePrice = product.SalePrice,
                TaxRate = product.TaxRate,
                ReorderLevel = product.ReorderLevel,
                OpeningStock = product.OpeningStock,
                DefaultLocationId = product.DefaultLocationId,
                IsSaleable = (product.ProductType & ProductType.Saleable) != 0,
                IsPurchasable = (product.ProductType & ProductType.Purchasable) != 0,
                IsService = (product.ProductType & ProductType.Service) != 0,
                IsConsumable = (product.ProductType & ProductType.Consumable) != 0,
                PackingUnitName = product.PackingUnitName,
                PackingQuantity = product.PackingQuantity,
                Description = product.Description,
                IsActive = product.IsActive,
                ExistingProductImagePath = product.ProductImagePath
            };

            await PopulateDropdownsAsync(vm);
            return View("Form", vm);
        }

        [HttpPost("Save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ProductFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View("Form", model);
            }

            var isNew = model.Id == 0;
            Product product;
            decimal? oldCostPrice = null;
            decimal? oldSalePrice = null;

            if (isNew)
            {
                product = new Product { CreatedOn = DateTime.UtcNow };
                product.Sku = await GenerateSkuAsync();
                _db.Products.Add(product);
            }
            else
            {
                var existing = await _db.Products.FindAsync(model.Id);
                if (existing == null) return NotFound();
                product = existing;
                oldCostPrice = existing.CostPrice;
                oldSalePrice = existing.SalePrice;
            }

            product.ProductCode = model.ProductCode;
            product.HsCode = model.HsCode;
            product.Name = model.Name;
            product.CategoryId = model.CategoryId;
            product.UnitId = model.UnitId;
            product.CostPrice = model.CostPrice;
            product.SalePrice = model.SalePrice;
            product.TaxRate = model.TaxRate;
            product.ReorderLevel = model.ReorderLevel;
            product.DefaultLocationId = model.DefaultLocationId;
            product.Description = model.Description;
            product.IsActive = model.IsActive;

            var productType = ProductType.None;
            if (model.IsSaleable) productType |= ProductType.Saleable;
            if (model.IsPurchasable) productType |= ProductType.Purchasable;
            if (model.IsService) productType |= ProductType.Service;
            if (model.IsConsumable) productType |= ProductType.Consumable;
            product.ProductType = productType;

            product.PackingUnitName = model.PackingUnitName;
            product.PackingQuantity = model.PackingQuantity;

            // Opening stock is only meaningful — and only posted to the ledger — once, at creation.
            if (isNew) product.OpeningStock = model.OpeningStock;

            if (model.ProductImage != null && model.ProductImage.Length > 0)
            {
                var oldPath = product.ProductImagePath;
                product.ProductImagePath = await _fileUploadService.SaveProductImageAsync(model.ProductImage, product.Sku);
                _fileUploadService.DeleteIfExists(oldPath);
            }

            // Price change history — only logged on edits where either price actually moved.
            if (!isNew && oldCostPrice.HasValue && oldSalePrice.HasValue &&
                (oldCostPrice.Value != product.CostPrice || oldSalePrice.Value != product.SalePrice))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                _db.ProductPriceHistories.Add(new ProductPriceHistory
                {
                    ProductId = product.Id,
                    OldCostPrice = oldCostPrice.Value,
                    NewCostPrice = product.CostPrice,
                    OldSalePrice = oldSalePrice.Value,
                    NewSalePrice = product.SalePrice,
                    ChangedById = currentUser?.Id,
                    ChangedByName = currentUser?.FullName,
                    ChangedOn = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            if (isNew && product.OpeningStock != 0 && product.DefaultLocationId.HasValue)
            {
                await _stockService.PostOpeningStockAsync(product, product.DefaultLocationId.Value);
            }

            TempData["SuccessMessage"] = isNew ? "Product created successfully." : "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return Json(new { success = false, message = "Product not found." });

            var hasMovements = await _db.StockTransactions.AnyAsync(s => s.ProductId == id);
            if (hasMovements)
                return Json(new { success = false, message = "This product has stock movements and cannot be deleted. Deactivate it instead." });

            _fileUploadService.DeleteIfExists(product.ProductImagePath);
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Product deleted." });
        }

        // ==================================================================
        // Import / Export
        // ==================================================================

        [HttpGet("Template/Product")]
        public IActionResult ProductTemplate()
        {
            var csv = CsvUtility.WriteWithHeader(
                new[] { "ProductCode", "Name", "Category", "Unit", "CostPrice", "SalePrice", "TaxRate", "ReorderLevel", "HsCode", "DefaultLocation", "Description" },
                new[]
                {
                    new[] { "RM-STEEL-01", "Steel Rod 10mm", "Raw Material", "Pcs", "150.00", "0", "5", "20", "7213.10", "Main Warehouse", "Sample row — delete before importing" }
                });

            return File(csv, "text/csv", "product_import_template.csv");
        }

        [HttpGet("Template/OpeningStock")]
        public IActionResult OpeningStockTemplate()
        {
            var csv = CsvUtility.WriteWithHeader(
                new[] { "ProductCodeOrSku", "Location", "Quantity" },
                new[] { new[] { "SKU-000001", "Main Warehouse", "100" } });

            return File(csv, "text/csv", "opening_stock_import_template.csv");
        }

        [HttpGet("Template/CustomerProductNames")]
        public IActionResult CustomerProductNamesTemplate()
        {
            var csv = CsvUtility.WriteWithHeader(
                new[] { "CustomerName", "ProductCodeOrSku", "CustomerProductName", "CustomerProductCode" },
                new[] { new[] { "Acme Corp", "SKU-000001", "10mm Rebar", "ACM-1001" } });

            return File(csv, "text/csv", "customer_product_names_template.csv");
        }

        /// <summary>Bulk create/update products by ProductCode. Unmatched Category/Unit/Location names are reported as row errors.</summary>
        [HttpPost("Import")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.StoreIncharge},{AppRoles.Manager}")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            var result = new ImportResultViewModel();
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please choose a CSV file." });

            try
            {
                var rows = CsvUtility.ParseWithHeader(file.OpenReadStream());
                result.TotalRows = rows.Count;

                var categories = (await _db.ProductCategories.ToListAsync())
                    .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
                var units = (await _db.UnitsOfMeasure.ToListAsync())
                    .GroupBy(u => u.ShortName, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
                var locations = (await _db.Locations.ToListAsync())
                    .GroupBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
                // Products can legitimately have duplicate ProductCodes already (that's what the
                // "duplicate code" filter on the grid surfaces) — group instead of building a
                // dictionary directly, which would throw on the first collision.
                var existingByCode = (await _db.Products.Where(p => p.ProductCode != null).ToListAsync())
                    .GroupBy(p => p.ProductCode!, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                // SKU numbering: track a running counter locally instead of calling
                // GenerateSkuAsync() (which queries Products.Count()) per row — that count
                // doesn't change until SaveChangesAsync runs, so every new product in the
                // same batch would otherwise get assigned the exact same SKU and the final
                // save would fail on the unique constraint. Also guard against gaps left by
                // deleted products, where Count()+1 could collide with an existing SKU.
                var usedSkus = (await _db.Products.Select(p => p.Sku).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var skuCounter = usedSkus.Count;

                string NextSku()
                {
                    string candidate;
                    do
                    {
                        skuCounter++;
                        candidate = $"SKU-{skuCounter:D6}";
                    } while (!usedSkus.Add(candidate));
                    return candidate;
                }

                var currentUser = await _userManager.GetUserAsync(User);

                var rowNum = 1;
                foreach (var row in rows)
                {
                    rowNum++;
                    var name = row.Get("Name");
                    var productCode = row.Get("ProductCode");
                    var categoryName = row.Get("Category");
                    var unitName = row.Get("Unit");

                    if (string.IsNullOrWhiteSpace(name)) { result.Errors.Add($"Row {rowNum}: Name is required."); result.ErrorCount++; continue; }
                    if (string.IsNullOrWhiteSpace(categoryName) || !categories.TryGetValue(categoryName, out var categoryId))
                    { result.Errors.Add($"Row {rowNum}: Category '{categoryName}' not found."); result.ErrorCount++; continue; }
                    if (string.IsNullOrWhiteSpace(unitName) || !units.TryGetValue(unitName, out var unitId))
                    { result.Errors.Add($"Row {rowNum}: Unit '{unitName}' not found."); result.ErrorCount++; continue; }

                    decimal.TryParse(row.Get("CostPrice"), out var costPrice);
                    decimal.TryParse(row.Get("SalePrice"), out var salePrice);
                    decimal? taxRate = decimal.TryParse(row.Get("TaxRate"), out var tr) ? tr : null;
                    decimal.TryParse(row.Get("ReorderLevel"), out var reorderLevel);

                    int? defaultLocationId = null;
                    var locationName = row.Get("DefaultLocation");
                    if (!string.IsNullOrWhiteSpace(locationName))
                    {
                        if (!locations.TryGetValue(locationName, out var locId))
                        { result.Errors.Add($"Row {rowNum}: Location '{locationName}' not found."); result.ErrorCount++; continue; }
                        defaultLocationId = locId;
                    }

                    Product product;
                    bool isNewProduct;
                    decimal? oldCostPrice = null;
                    decimal? oldSalePrice = null;

                    if (!string.IsNullOrWhiteSpace(productCode) && existingByCode.TryGetValue(productCode, out var existing))
                    {
                        product = existing;
                        isNewProduct = false;
                        oldCostPrice = existing.CostPrice;
                        oldSalePrice = existing.SalePrice;
                    }
                    else
                    {
                        product = new Product { CreatedOn = DateTime.UtcNow, Sku = NextSku() };
                        _db.Products.Add(product);
                        isNewProduct = true;
                    }

                    product.ProductCode = productCode;
                    product.Name = name;
                    product.CategoryId = categoryId;
                    product.UnitId = unitId;
                    product.CostPrice = costPrice;
                    product.SalePrice = salePrice;
                    product.TaxRate = taxRate;
                    product.ReorderLevel = reorderLevel;
                    product.HsCode = row.Get("HsCode");
                    product.DefaultLocationId = defaultLocationId;
                    product.Description = row.Get("Description");
                    product.IsActive = true;

                    if (!isNewProduct && oldCostPrice.HasValue && oldSalePrice.HasValue &&
                        (oldCostPrice.Value != costPrice || oldSalePrice.Value != salePrice))
                    {
                        _db.ProductPriceHistories.Add(new ProductPriceHistory
                        {
                            ProductId = product.Id,
                            OldCostPrice = oldCostPrice.Value,
                            NewCostPrice = costPrice,
                            OldSalePrice = oldSalePrice.Value,
                            NewSalePrice = salePrice,
                            ChangedById = currentUser?.Id,
                            ChangedByName = currentUser?.FullName,
                            ChangedOn = DateTime.UtcNow
                        });
                    }

                    if (isNewProduct) result.CreatedCount++; else result.UpdatedCount++;
                }

                await _db.SaveChangesAsync();
                return Json(new { success = result.Success, summary = result.Summary, errorCount = result.ErrorCount, errors = result.Errors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Import failed: " + ex.Message });
            }
        }

        /// <summary>Bulk-posts opening stock quantities to the ledger for existing products.</summary>
        [HttpPost("Import/OpeningStock")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.StoreIncharge},{AppRoles.Manager}")]
        public async Task<IActionResult> ImportOpeningStock(IFormFile file)
        {
            var result = new ImportResultViewModel();
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please choose a CSV file." });

            try
            {
                var rows = CsvUtility.ParseWithHeader(file.OpenReadStream());
                result.TotalRows = rows.Count;

                var products = await _db.Products.ToListAsync();
                var locations = (await _db.Locations.ToListAsync())
                    .GroupBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

                var rowNum = 1;
                foreach (var row in rows)
                {
                    rowNum++;
                    var code = row.Get("ProductCodeOrSku");
                    var locationName = row.Get("Location");

                    var product = products.FirstOrDefault(p =>
                        string.Equals(p.Sku, code, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.ProductCode, code, StringComparison.OrdinalIgnoreCase));

                    if (product == null) { result.Errors.Add($"Row {rowNum}: Product '{code}' not found."); result.ErrorCount++; continue; }
                    if (string.IsNullOrWhiteSpace(locationName) || !locations.TryGetValue(locationName, out var locationId))
                    { result.Errors.Add($"Row {rowNum}: Location '{locationName}' not found."); result.ErrorCount++; continue; }
                    if (!decimal.TryParse(row.Get("Quantity"), out var quantity) || quantity <= 0)
                    { result.Errors.Add($"Row {rowNum}: Quantity must be a positive number."); result.ErrorCount++; continue; }

                    _db.StockTransactions.Add(new Models.Inventory.StockTransaction
                    {
                        ProductId = product.Id,
                        LocationId = locationId,
                        TransactionType = Models.Inventory.StockTransactionType.OpeningStock,
                        Quantity = quantity,
                        ReferenceType = "Product",
                        ReferenceId = product.Id,
                        ReferenceNo = product.Sku,
                        Remarks = "Bulk opening stock import",
                        TransactionDate = DateTime.UtcNow
                    });

                    result.UpdatedCount++;
                }

                await _db.SaveChangesAsync();
                return Json(new { success = result.Success, summary = result.Summary, errorCount = result.ErrorCount, errors = result.Errors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Import failed: " + ex.Message });
            }
        }

        /// <summary>Bulk create/update customer-specific product name/code aliases.</summary>
        [HttpPost("Import/CustomerProductNames")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.StoreIncharge},{AppRoles.Manager}")]
        public async Task<IActionResult> ImportCustomerProductNames(IFormFile file)
        {
            var result = new ImportResultViewModel();
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please choose a CSV file." });

            try
            {
                var rows = CsvUtility.ParseWithHeader(file.OpenReadStream());
                result.TotalRows = rows.Count;

                var products = await _db.Products.ToListAsync();
                var existingAliases = await _db.CustomerProductAliases.ToListAsync();

                var rowNum = 1;
                foreach (var row in rows)
                {
                    rowNum++;
                    var customerName = row.Get("CustomerName");
                    var code = row.Get("ProductCodeOrSku");

                    if (string.IsNullOrWhiteSpace(customerName))
                    { result.Errors.Add($"Row {rowNum}: CustomerName is required."); result.ErrorCount++; continue; }

                    var product = products.FirstOrDefault(p =>
                        string.Equals(p.Sku, code, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.ProductCode, code, StringComparison.OrdinalIgnoreCase));

                    if (product == null) { result.Errors.Add($"Row {rowNum}: Product '{code}' not found."); result.ErrorCount++; continue; }

                    var alias = existingAliases.FirstOrDefault(a =>
                        a.ProductId == product.Id && string.Equals(a.CustomerName, customerName, StringComparison.OrdinalIgnoreCase));

                    var isNew = alias == null;
                    if (isNew)
                    {
                        alias = new CustomerProductAlias { CustomerName = customerName, ProductId = product.Id, CreatedOn = DateTime.UtcNow };
                        _db.CustomerProductAliases.Add(alias);
                    }

                    alias!.CustomerProductName = row.Get("CustomerProductName");
                    alias.CustomerProductCode = row.Get("CustomerProductCode");

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

        // ==================================================================
        // Price history
        // ==================================================================

        [HttpGet("PriceHistory/{id:int}")]
        public async Task<IActionResult> PriceHistory(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpGet("GetPriceHistory/{id:int}")]
        public async Task<IActionResult> GetPriceHistory(int id)
        {
            var history = await _db.ProductPriceHistories
                .Where(h => h.ProductId == id)
                .OrderByDescending(h => h.ChangedOn)
                .Select(h => new
                {
                    h.OldCostPrice,
                    h.NewCostPrice,
                    h.OldSalePrice,
                    h.NewSalePrice,
                    ChangedBy = h.ChangedByName ?? "—",
                    h.ChangedOn
                })
                .ToListAsync();

            return Json(new { data = history });
        }

        // ==================================================================
        // Low stock alerts (feeds the navbar notification bell)
        // ==================================================================

        [HttpGet("~/Inventory/LowStockAlerts")]
        public async Task<IActionResult> LowStockAlerts()
        {
            var products = await _db.Products.Where(p => p.IsActive).ToListAsync();
            var stockByProduct = await _stockService.GetCurrentStockForAllProductsAsync();

            var alerts = products
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Sku,
                    ReorderLevel = p.ReorderLevel,
                    CurrentStock = stockByProduct.TryGetValue(p.Id, out var qty) ? qty : 0
                })
                .Where(p => p.CurrentStock <= 0 || (p.ReorderLevel > 0 && p.CurrentStock <= p.ReorderLevel))
                .OrderBy(p => p.CurrentStock)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Sku,
                    p.ReorderLevel,
                    p.CurrentStock,
                    Status = p.CurrentStock <= 0 ? "Out" : "Low"
                })
                .ToList();

            return Json(new { count = alerts.Count, alerts });
        }

        // ---------------- helpers ----------------

        private async Task<string> GenerateSkuAsync()
        {
            var count = await _db.Products.CountAsync();
            var candidate = $"SKU-{(count + 1):D6}";

            // Guards against gaps left by deleted products, where Count()+1 could
            // collide with a SKU that already exists further along the sequence.
            while (await _db.Products.AnyAsync(p => p.Sku == candidate))
            {
                count++;
                candidate = $"SKU-{(count + 1):D6}";
            }

            return candidate;
        }

        private async Task PopulateDropdownsAsync(ProductFormViewModel vm)
        {
            vm.Categories = await _db.ProductCategories.Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new ProductCategoryOption(c.Id, c.Name))
                .ToListAsync();

            vm.Units = await _db.UnitsOfMeasure.Where(u => u.IsActive)
                .OrderBy(u => u.Name)
                .Select(u => new UnitOption(u.Id, u.Name, u.ShortName))
                .ToListAsync();

            vm.Locations = await _db.Locations.Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .Select(l => new LocationOption(l.Id, l.Name))
                .ToListAsync();
        }
    }
}
