// File Path: src/HRManagement.Web/Controllers/AuditController.cs
// Purpose: MVC Controller for displaying system-wide Audit Logs.
// Code Explanation: Accessible only by Admin role. Fetches logs and displays them in a readable format.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Persistence;

namespace HRManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,HR Manager")]
    public class AuditController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public AuditController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var query = _context.AuditLogs.AsNoTracking();

            if (User.IsInRole("HR Manager"))
            {
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole != null)
                {
                    var adminUserIds = await _context.UserRoles
                        .Where(ur => ur.RoleId == adminRole.Id)
                        .Select(ur => ur.UserId)
                        .ToListAsync();

                    var adminUserEmails = await _context.Users
                        .Where(u => adminUserIds.Contains(u.Id))
                        .Select(u => u.Email)
                        .ToListAsync();

                    query = query.Where(l => 
                        l.UserId == null || 
                        (!adminUserEmails.Contains(l.UserId) && !adminUserIds.Contains(l.UserId) && l.UserId != "admin@hrmanagement.com")
                    );
                }
            }

            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            return View(logs);
        }
    }
}
