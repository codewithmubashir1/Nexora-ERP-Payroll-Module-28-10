using ERP.Identity.Data;
using ERP.Identity.Models;
using ERP.Identity.Models.ViewModels;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Identity.Controllers
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileUploadService _fileUploadService;
        private readonly ICompanyInfoService _companyInfoService;

        public CompanyController(ApplicationDbContext db, IFileUploadService fileUploadService, ICompanyInfoService companyInfoService)
        {
            _db = db;
            _fileUploadService = fileUploadService;
            _companyInfoService = companyInfoService;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _companyInfoService.GetAsync();

            var vm = new CompanySettingsViewModel
            {
                CompanyName = settings.CompanyName,
                Address = settings.Address,
                Email = settings.Email,
                Phone = settings.Phone,
                NTNNumber = settings.NTNNumber,
                SalesTaxRegistrationNumber = settings.SalesTaxRegistrationNumber,
                ExistingLogoPath = settings.LogoPath,
                BankName = settings.BankName,
                BankAccountTitle = settings.BankAccountTitle,
                BankAccountNumber = settings.BankAccountNumber
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Index(CompanySettingsViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var settings = await _companyInfoService.GetAsync();

            settings.CompanyName = model.CompanyName;
            settings.Address = model.Address;
            settings.Email = model.Email;
            settings.Phone = model.Phone;
            settings.NTNNumber = model.NTNNumber;
            settings.SalesTaxRegistrationNumber = model.SalesTaxRegistrationNumber;
            settings.BankName = model.BankName;
            settings.BankAccountTitle = model.BankAccountTitle;
            settings.BankAccountNumber = model.BankAccountNumber;
            settings.UpdatedOn = DateTime.UtcNow;

            if (model.Logo != null && model.Logo.Length > 0)
            {
                var oldPath = settings.LogoPath;
                settings.LogoPath = await _fileUploadService.SaveCompanyLogoAsync(model.Logo);
                _fileUploadService.DeleteIfExists(oldPath);
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Company settings updated. This letterhead now appears on every printed document.";
            return RedirectToAction(nameof(Index));
        }
    }
}
