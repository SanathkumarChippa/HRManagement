using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using HRManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HRManagement.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace HRManagement.Web.Filters
{
    public class ActiveEmployeeFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ActiveEmployeeFilter(ApplicationDbContext dbContext, SignInManager<ApplicationUser> signInManager)
        {
            _dbContext = dbContext;
            _signInManager = signInManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userPrincipal = context.HttpContext.User;
            if (userPrincipal.Identity?.IsAuthenticated == true)
            {
                var controller = context.RouteData.Values["controller"]?.ToString();
                var action = context.RouteData.Values["action"]?.ToString();

                // Exclude Account Controller login/logout to prevent redirect loops
                if (controller == "Account" && (action == "Login" || action == "Logout" || action == "AccessDenied"))
                {
                    await next();
                    return;
                }

                var email = userPrincipal.FindFirst(ClaimTypes.Email)?.Value 
                            ?? userPrincipal.FindFirst("email")?.Value 
                            ?? userPrincipal.Identity.Name;

                if (!string.IsNullOrEmpty(email))
                {
                    var employee = await _dbContext.Employees
                        .FirstOrDefaultAsync(e => e.Email.ToUpper() == email.ToUpper() && !e.IsDeleted);

                    if (employee != null)
                    {
                        var status = employee.EmploymentStatus;
                        if (status == "Resigned" || status == "Terminated")
                        {
                            // Sign out
                            await _signInManager.SignOutAsync();

                            // Set TempData error message
                            if (context.Controller is Controller mvcController)
                            {
                                mvcController.TempData["Error"] = "Your account has been marked as Resigned or Terminated. Please contact HR.";
                                mvcController.TempData["ErrorMessage"] = "Your account has been marked as Resigned or Terminated. Please contact HR.";
                            }

                            // Redirect to login
                            context.Result = new RedirectToActionResult("Login", "Account", null);
                            return;
                        }
                    }
                }
            }

            await next();
        }
    }
}
