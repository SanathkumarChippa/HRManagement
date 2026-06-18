using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRManagement.Persistence;

namespace HRManagement.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentNotifications()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                        ?? User.FindFirst("email")?.Value 
                        ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email.ToUpper() == email.ToUpper());
            if (employee == null) return Unauthorized();

            var notifications = await _context.Notifications
                .Where(n => n.EmployeeId == employee.Id && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedDate)
                .Take(10)
                .Select(n => new {
                    n.Id,
                    Title = n.Type, // Type acts as title here, or we can use generic title
                    n.Message,
                    n.IsRead,
                    n.CreatedDate,
                    TimeAgo = GetTimeAgo(n.CreatedDate),
                    TargetUrl = n.TargetUrl ?? "/Home/Index"
                })
                .ToListAsync();

            var unreadCount = await _context.Notifications
                .CountAsync(n => n.EmployeeId == employee.Id && !n.IsRead && !n.IsDeleted);

            return Ok(new { notifications, unreadCount });
        }

        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                        ?? User.FindFirst("email")?.Value 
                        ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email.ToUpper() == email.ToUpper());
            if (employee == null) return Unauthorized();

            var unreadNotifications = await _context.Notifications
                .Where(n => n.EmployeeId == employee.Id && !n.IsRead && !n.IsDeleted)
                .ToListAsync();

            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan <= TimeSpan.FromSeconds(60))
                return "Just now";
            if (timeSpan <= TimeSpan.FromMinutes(60))
                return timeSpan.Minutes > 1 ? $"{timeSpan.Minutes} minutes ago" : "A minute ago";
            if (timeSpan <= TimeSpan.FromHours(24))
                return timeSpan.Hours > 1 ? $"{timeSpan.Hours} hours ago" : "An hour ago";
            if (timeSpan <= TimeSpan.FromDays(30))
                return timeSpan.Days > 1 ? $"{timeSpan.Days} days ago" : "Yesterday";

            return dateTime.ToString("MMM dd, yyyy");
        }
    }
}
