// File Path: src/HRManagement.API/Controllers/NotificationsController.cs
// Purpose: API controller managing Notification endpoints.
// Code Explanation: Implements endpoints to retrieve notifications, fetch unread counts, and batch mark notifications read for the current user.

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Persistence;

namespace HRManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public NotificationsController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        private async Task<int?> GetCurrentEmployeeIdAsync()
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (!string.IsNullOrEmpty(employeeIdClaim) && int.TryParse(employeeIdClaim, out var empId))
            {
                return empId;
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
                return employee?.Id;
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var empId = await GetCurrentEmployeeIdAsync();
            if (!empId.HasValue)
            {
                return BadRequest("User profile not found.");
            }

            var notifications = await _unitOfWork.Notifications.GetNotificationsByEmployeeAsync(empId.Value);
            return Ok(notifications);
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkAsRead()
        {
            var empId = await GetCurrentEmployeeIdAsync();
            if (!empId.HasValue)
            {
                return BadRequest("User profile not found.");
            }

            await _unitOfWork.Notifications.MarkAllAsReadAsync(empId.Value);
            await _unitOfWork.SaveAsync();

            return Ok(new { Message = "All notifications marked as read." });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var empId = await GetCurrentEmployeeIdAsync();
            if (!empId.HasValue)
            {
                return BadRequest("User profile not found.");
            }

            var count = await _context.Notifications
                .CountAsync(n => n.EmployeeId == empId.Value && !n.IsRead && n.IsDeleted == false);

            return Ok(new { UnreadCount = count });
        }
    }
}
