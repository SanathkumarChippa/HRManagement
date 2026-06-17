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
    [Authorize(Roles = "Admin")]
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
            var logs = await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
            return View(logs);
        }
    }
}
