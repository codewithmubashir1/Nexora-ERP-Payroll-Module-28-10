using ERP.Identity.Models;
using ERP.Identity.Models.ViewModels;
using ERP.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Identity.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileUploadService _fileUploadService;

        public UsersController(UserManager<ApplicationUser> userManager, IFileUploadService fileUploadService)
        {
            _userManager = userManager;
            _fileUploadService = fileUploadService;
        }

        public IActionResult Index() => View();

        // Feeds the DataTables grid via AJAX.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = _userManager.Users.ToList();
            var list = new List<UserListItemViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber,
                    Department = u.Department,
                    Designation = u.Designation,
                    Role = roles.FirstOrDefault() ?? "—",
                    IsActive = u.IsActive,
                    ProfileImagePath = u.ProfileImagePath,
                    CreatedOn = u.CreatedOn
                });
            }

            return Json(new { data = list });
        }

        [HttpGet]
        public IActionResult Create() => View("Form", new UserFormViewModel());

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var vm = new UserFormViewModel
            {
                Id = user.Id,
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
                IsActive = user.IsActive,
                SelectedRole = roles.FirstOrDefault(),
                ExistingProfileImagePath = user.ProfileImagePath
            };

            return View("Form", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(UserFormViewModel model)
        {
            var isNew = string.IsNullOrEmpty(model.Id);

            if (isNew && string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(model.Password), "Password is required for a new user.");

            if (!ModelState.IsValid)
            {
                model.ExistingProfileImagePath = null;
                return View("Form", model);
            }

            ApplicationUser user;

            if (isNew)
            {
                user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    FullName = model.FullName,
                    CreatedOn = DateTime.UtcNow
                };
            }
            else
            {
                user = (await _userManager.FindByIdAsync(model.Id!))!;
                if (user == null) return NotFound();
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email;
            }

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
            user.IsActive = model.IsActive;

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var oldPath = user.ProfileImagePath;
                user.ProfileImagePath = await _fileUploadService.SaveAvatarAsync(model.ProfileImage, isNew ? Guid.NewGuid().ToString("N") : user.Id);
                _fileUploadService.DeleteIfExists(oldPath);
            }

            IdentityResult result;

            if (isNew)
            {
                result = await _userManager.CreateAsync(user, model.Password!);
            }
            else
            {
                result = await _userManager.UpdateAsync(user);

                if (result.Succeeded && !string.IsNullOrWhiteSpace(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                }
            }

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View("Form", model);
            }

            if (!string.IsNullOrWhiteSpace(model.SelectedRole))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any()) await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }

            TempData["SuccessMessage"] = isNew ? "User created successfully." : "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Called via SweetAlert2 confirmation + fetch() from the Users grid.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return Json(new { success = false, message = "User not found." });

            _fileUploadService.DeleteIfExists(user.ProfileImagePath);
            var result = await _userManager.DeleteAsync(user);

            return Json(new
            {
                success = result.Succeeded,
                message = result.Succeeded ? "User deleted." : "Could not delete user."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return Json(new { success = false, message = "User not found." });

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            return Json(new { success = true, isActive = user.IsActive });
        }
        [HttpGet("Template")]
        public IActionResult Template()
        {
            var csv = Services.CsvUtility.WriteWithHeader(
                new[] { "FullName", "Email", "PhoneNumber", "Role", "Department", "Designation", "Password" },
                new[]
                {
                    new[] { "Jane Doe", "jane.doe@example.com", "+1 555 0100", AppRoles.StandardUser, "Sales", "Sales Executive", "" }
                });

            return File(csv, "text/csv", "user_import_template.csv");
        }

        [HttpPost("Import")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            var result = new ImportResultViewModel();
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please choose a CSV file." });

            try
            {
                var rows = Services.CsvUtility.ParseWithHeader(file.OpenReadStream());
                result.TotalRows = rows.Count;

                var rowNum = 1;
                foreach (var row in rows)
                {
                    rowNum++;
                    var email = row.Get("Email");
                    var fullName = row.Get("FullName");

                    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(fullName))
                    {
                        result.Errors.Add($"Row {rowNum}: FullName and Email are required.");
                        result.ErrorCount++;
                        continue;
                    }

                    var existing = await _userManager.FindByEmailAsync(email);
                    var role = row.Get("Role");
                    var validRole = !string.IsNullOrWhiteSpace(role) && AppRoles.All.Contains(role) ? role : AppRoles.StandardUser;

                    if (existing != null)
                    {
                        existing.FullName = fullName;
                        existing.PhoneNumber = row.Get("PhoneNumber");
                        existing.Department = row.Get("Department");
                        existing.Designation = row.Get("Designation");
                        await _userManager.UpdateAsync(existing);

                        var currentRoles = await _userManager.GetRolesAsync(existing);
                        if (!currentRoles.Contains(validRole))
                        {
                            if (currentRoles.Any()) await _userManager.RemoveFromRolesAsync(existing, currentRoles);
                            await _userManager.AddToRoleAsync(existing, validRole);
                        }

                        result.UpdatedCount++;
                        continue;
                    }

                    var password = row.Get("Password");
                    if (string.IsNullOrWhiteSpace(password))
                        password = $"Welcome@{Random.Shared.Next(1000, 9999)}";

                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = fullName,
                        PhoneNumber = row.Get("PhoneNumber"),
                        Department = row.Get("Department"),
                        Designation = row.Get("Designation"),
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow
                    };

                    var createResult = await _userManager.CreateAsync(user, password);
                    if (!createResult.Succeeded)
                    {
                        result.Errors.Add($"Row {rowNum}: {string.Join(" ", createResult.Errors.Select(e => e.Description))}");
                        result.ErrorCount++;
                        continue;
                    }

                    await _userManager.AddToRoleAsync(user, validRole);
                    result.CreatedCount++;
                }

                return Json(new { success = result.Success, summary = result.Summary, errorCount = result.ErrorCount, errors = result.Errors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Import failed: " + ex.Message });
            }
        }
    }
}
