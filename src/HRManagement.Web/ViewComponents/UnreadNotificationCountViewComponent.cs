// File Path: src/HRManagement.Web/ViewComponents/UnreadNotificationCountViewComponent.cs
// Purpose: View Component to display the unread notification badge count.
// Code Explanation: Injects ApplicationDbContext, fetches the current user's email, locates the Employee record, and queries the database for the number of unread notifications, returning a badge indicator if greater than zero.

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRManagement.Persistence;

namespace HRManagement.Web.ViewComponents
{
    public class UnreadNotificationCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public UnreadNotificationCountViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var email = UserClaimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return Content(string.Empty);
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (employee == null)
            {
                return Content(string.Empty);
            }

            var count = await _context.Notifications
                .CountAsync(n => n.EmployeeId == employee.Id && !n.IsRead && !n.IsDeleted);

            return View(count);
        }
    }
}
