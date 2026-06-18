// File Path: src/HRManagement.Web/Controllers/ProfileController.cs
// Purpose: MVC Controller managing user profile details, application settings, and password updates.
// Code Explanation: Requires authentication. Loads corresponding employee profiles and coordinates password updates through UserManager.

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRManagement.Domain.Entities;
using HRManagement.Persistence;
using HRManagement.Web.Models;

namespace HRManagement.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: Profile/Index (My Profile)
        public async Task<IActionResult> Index()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                        ?? User.FindFirst("email")?.Value 
                        ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return Challenge();

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.Email.ToUpper() == email.ToUpper() && !e.IsDeleted);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "No Employee profile associated with this account.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                ViewBag.Role = roles.FirstOrDefault() ?? "Employee";
            }
            else
            {
                ViewBag.Role = "Employee";
            }

            return View(employee);
        }

        // GET: Profile/Settings
        public async Task<IActionResult> Settings()
        {
            if (!User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Access Denied. You are not authorized to view Settings.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var model = new SettingsViewModel
            {
                ThemePreference = user.ThemePreference ?? "light",
                EmailAlertsEnabled = user.EmailAlertsEnabled,
                InAppNotificationsEnabled = user.InAppNotificationsEnabled
            };
            return View(model);
        }

        // POST: Profile/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SettingsViewModel model)
        {
            if (!User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Access Denied. You are not authorized to view Settings.";
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            user.ThemePreference = model.ThemePreference;
            user.EmailAlertsEnabled = model.EmailAlertsEnabled;
            user.InAppNotificationsEnabled = model.InAppNotificationsEnabled;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Preferences updated successfully.";
                Response.Cookies.Append("ThemePreference", user.ThemePreference ?? "light", new Microsoft.AspNetCore.Http.CookieOptions { Expires = System.DateTimeOffset.UtcNow.AddYears(1) });
            }

            return RedirectToAction(nameof(Settings));
        }

        // POST: Profile/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string phoneNumber, string gender, Microsoft.AspNetCore.Http.IFormFile? profilePhoto)
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                        ?? User.FindFirst("email")?.Value 
                        ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return Challenge();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email.ToUpper() == email.ToUpper() && !e.IsDeleted);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee profile not found.";
                return RedirectToAction(nameof(Index));
            }

            employee.FirstName = firstName;
            employee.LastName = lastName;
            employee.PhoneNumber = phoneNumber;
            employee.Gender = gender;
            employee.UpdatedDate = System.DateTime.UtcNow;
            employee.UpdatedBy = email;

            if (profilePhoto != null && profilePhoto.Length > 0)
            {
                var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile_photos");
                if (!System.IO.Directory.Exists(uploadsFolder))
                {
                    System.IO.Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = System.Guid.NewGuid().ToString() + System.IO.Path.GetExtension(profilePhoto.FileName);
                var filePath = System.IO.Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await profilePhoto.CopyToAsync(fileStream);
                }

                employee.ProfilePicturePath = "/uploads/profile_photos/" + uniqueFileName;
            }

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile details updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Profile/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            user.MustChangePassword = false;
            user.IsFirstLogin = false;
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Your password has been changed successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Profile/ChangePasswordJson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordJson(string currentPassword, string newPassword, string confirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "User not found." });

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "The new password and confirmation password do not match." });
            }

            var regex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$");
            if (!regex.IsMatch(newPassword))
            {
                return Json(new { success = false, message = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character." });
            }

            var changeResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!changeResult.Succeeded)
            {
                var errors = string.Join(" ", changeResult.Errors.Select(e => e.Description));
                return Json(new { success = false, message = errors });
            }

            user.MustChangePassword = false;
            user.IsFirstLogin = false;
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            return Json(new { success = true });
        }

        // GET: Profile/ForceChangePassword
        [HttpGet]
        public async Task<IActionResult> ForceChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!user.MustChangePassword)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}
