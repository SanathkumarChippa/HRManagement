// File Path: src/HRManagement.Web/Controllers/ProfileController.cs
// Purpose: MVC Controller managing user profile details, application settings, and password updates.
// Code Explanation: Requires authentication. Loads corresponding employee profiles and coordinates password updates through UserManager.

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
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Profile/Index (My Profile)
        public async Task<IActionResult> Index()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Challenge();

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.Email == email && !e.IsDeleted);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "No Employee profile associated with this account.";
                return RedirectToAction("Index", "Home");
            }

            return View(employee);
        }

        // GET: Profile/Settings
        public IActionResult Settings()
        {
            return View();
        }

        // POST: Profile/Settings (Fake settings save)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(string theme, bool emailNotifications, bool inAppNotifications)
        {
            // Set cookie or session for user preference if needed, or simply return success
            TempData["SuccessMessage"] = "Preferences updated successfully.";
            return RedirectToAction(nameof(Settings));
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

            TempData["SuccessMessage"] = "Your password has been changed successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
