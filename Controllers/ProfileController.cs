using ERP.Identity.Models;
using ERP.Identity.Models.ViewModels;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Identity.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IFileUploadService _fileUploadService;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IFileUploadService fileUploadService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileUploadService = fileUploadService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var vm = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                EmployeeCode = user.EmployeeCode,
                Designation = user.Designation,
                Department = user.Department,
                Gender = user.Gender,
                BloodGroup = user.BloodGroup,
                DateOfBirth = user.DateOfBirth,
                JoiningDate = user.JoiningDate,
                NationalId = user.NationalId,
                Address = user.Address,
                City = user.City,
                Country = user.Country,
                EmergencyContactName = user.EmergencyContactName,
                EmergencyContactPhone = user.EmergencyContactPhone,
                Bio = user.Bio,
                ProfileImagePath = user.ProfileImagePath,
                CurrentRole = roles.FirstOrDefault()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.ProfileImagePath = user.ProfileImagePath;
                var roles = await _userManager.GetRolesAsync(user);
                model.CurrentRole = roles.FirstOrDefault();
                return View(model);
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.EmployeeCode = model.EmployeeCode;
            user.Designation = model.Designation;
            user.Department = model.Department;
            user.Gender = model.Gender;
            user.BloodGroup = model.BloodGroup;
            user.DateOfBirth = model.DateOfBirth;
            user.JoiningDate = model.JoiningDate;
            user.NationalId = model.NationalId;
            user.Address = model.Address;
            user.City = model.City;
            user.Country = model.Country;
            user.EmergencyContactName = model.EmergencyContactName;
            user.EmergencyContactPhone = model.EmergencyContactPhone;
            user.Bio = model.Bio;

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                try
                {
                    var oldPath = user.ProfileImagePath;
                    user.ProfileImagePath = await _fileUploadService.SaveAvatarAsync(model.ProfileImage, user.Id);
                    _fileUploadService.DeleteIfExists(oldPath);
                }
                catch (InvalidOperationException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToAction(nameof(Index));
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Profile updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }
    }
}
