using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRManagement.Persistence;

namespace HRManagement.Web.ViewComponents
{
    public class UserProfileViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public UserProfileViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var email = UserClaimsPrincipal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                        ?? UserClaimsPrincipal.FindFirst("email")?.Value
                        ?? UserClaimsPrincipal.Identity?.Name;
            string fullName = "Unknown User";
            string role = UserClaimsPrincipal.IsInRole("Admin") ? "System Administrator" : 
                          (UserClaimsPrincipal.IsInRole("HR Manager") ? "HR Manager" : "Employee");

            if (!string.IsNullOrEmpty(email))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email.ToUpper() == email.ToUpper() && !e.IsDeleted);
                if (employee != null)
                {
                    fullName = $"{employee.FirstName} {employee.LastName}";
                }
                else
                {
                    fullName = email;
                }
            }

            return View(new { FullName = fullName, Role = role });
        }
    }
}
