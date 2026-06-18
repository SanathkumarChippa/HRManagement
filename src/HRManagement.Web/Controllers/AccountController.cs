// File Path: src/HRManagement.Web/Controllers/AccountController.cs
// Purpose: Controller managing Web MVC user authentication.
// Code Explanation: Uses SignInManager to sign users in/out, sets up secure cookie credentials, and routes users to appropriate portals on login success.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRManagement.Domain.Entities;
using HRManagement.Application.Models.Identity;

namespace HRManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AuthRequest request, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var user = await _userManager.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Account not found or inactive.");
                return View(request);
            }

            if (user.Employee != null && (user.Employee.EmploymentStatus == "Resigned" || user.Employee.EmploymentStatus == "Terminated"))
            {
                ModelState.AddModelError(string.Empty, "Your employment is no longer active. Please contact HR.");
                return View(request);
            }

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                Response.Cookies.Append("ThemePreference", user.ThemePreference ?? "light", new Microsoft.AspNetCore.Http.CookieOptions { Expires = System.DateTimeOffset.UtcNow.AddYears(1) });
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
