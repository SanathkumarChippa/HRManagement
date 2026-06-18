// File Path: src/HRManagement.Web/Controllers/HomeController.cs
// Purpose: Controller serving the dynamic dashboards.
// Code Explanation: Requires authentication. Inspects roles (Admin, HR Manager, Employee) and compiles target metrics (headcounts, active leaves, pending counts, department allocations) to populate charts and cards.

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Domain.Entities;
using HRManagement.Persistence;

namespace HRManagement.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public HomeController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Resolve Current Employee if applicable
            Employee? currentEmployee = null;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                        ?? User.FindFirst("email")?.Value 
                        ?? User.Identity?.Name;
            if (!string.IsNullOrEmpty(email))
            {
                currentEmployee = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Manager)
                    .FirstOrDefaultAsync(e => e.Email.ToUpper() == email.ToUpper() && !e.IsDeleted);
            }

            // 2. Fetch General System Metrics
            ViewBag.TotalEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted);
            ViewBag.ActiveEmployees = await _context.Employees.CountAsync(e => e.EmploymentStatus == "Active" && !e.IsDeleted);
            ViewBag.DepartmentCount = await _context.Departments.CountAsync(d => !d.IsDeleted);

            var today = DateTime.UtcNow;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            ViewBag.NewHiresThisMonth = await _context.Employees.CountAsync(e => !e.IsDeleted && e.DateOfJoining >= startOfMonth);
            
            var lastMonthCount = await _context.Employees.CountAsync(e => !e.IsDeleted && e.DateOfJoining < startOfMonth);
            if (lastMonthCount > 0) {
                ViewBag.GrowthPercentage = Math.Round((double)ViewBag.NewHiresThisMonth / lastMonthCount * 100, 1);
            } else {
                ViewBag.GrowthPercentage = ViewBag.NewHiresThisMonth > 0 ? 100 : 0;
            }
            
            // On Leave Today
            ViewBag.OnLeaveToday = await _context.LeaveRequests
                .CountAsync(lr => lr.Status == "Approved" && 
                                  DateTime.Today >= lr.StartDate.Date && 
                                  DateTime.Today <= lr.EndDate.Date && 
                                  !lr.IsDeleted);

            // Pending Approvals
            ViewBag.PendingApprovals = await _context.LeaveRequests
                .CountAsync(lr => lr.Status == "Pending" && !lr.IsDeleted);

            // Total Leave Requests
            ViewBag.TotalLeaveRequests = await _context.LeaveRequests.CountAsync(lr => !lr.IsDeleted);

            // Department Distribution (for Admin & HR Manager Charts)
            var deptData = await _context.Employees
                .Where(e => !e.IsDeleted)
                .GroupBy(e => e.Department.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.DeptNames = deptData.Select(d => d.Name).ToList();
            ViewBag.DeptCounts = deptData.Select(d => d.Count).ToList();

            if (User.IsInRole("Admin"))
            {
                ViewBag.DashboardType = "Admin";
                
                // Employee Growth (Monthly headcount for current year)
                var currentYear = DateTime.UtcNow.Year;
                var allEmployees = await _context.Employees
                    .Where(e => !e.IsDeleted && e.DateOfJoining <= new DateTime(currentYear, 12, 31))
                    .ToListAsync();
                
                var monthlyCounts = new List<int>();
                for (int m = 1; m <= 12; m++)
                {
                    var lastDayOfMonth = new DateTime(currentYear, m, DateTime.DaysInMonth(currentYear, m));
                    // Count employees who joined on or before the last day of the month
                    var count = allEmployees.Count(e => e.DateOfJoining <= lastDayOfMonth);
                    monthlyCounts.Add(count);
                }
                
                ViewBag.GrowthMonths = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                ViewBag.GrowthCounts = monthlyCounts;
            }
            else if (User.IsInRole("HR Manager"))
            {
                ViewBag.DashboardType = "HRManager";

                // Leave Status Distribution
                var approvedCount = await _context.LeaveRequests.CountAsync(lr => lr.Status == "Approved" && !lr.IsDeleted);
                var pendingCount = await _context.LeaveRequests.CountAsync(lr => lr.Status == "Pending" && !lr.IsDeleted);
                var rejectedCount = await _context.LeaveRequests.CountAsync(lr => lr.Status == "Rejected" && !lr.IsDeleted);

                ViewBag.LeaveStatusNames = new List<string> { "Approved", "Pending", "Rejected" };
                ViewBag.LeaveStatusCounts = new List<int> { approvedCount, pendingCount, rejectedCount };
            }
            else // Employee
            {
                ViewBag.DashboardType = "Employee";
                
                if (currentEmployee != null)
                {
                    ViewBag.Employee = currentEmployee;

                    // Leave balances DTO
                    var balances = await _unitOfWork.Leaves.GetLeaveBalancesByEmployeeAsync(currentEmployee.Id);
                    ViewBag.LeaveBalances = balances;

                    // Personal leave trends
                    var myTrends = await _context.LeaveRequests
                        .Where(lr => lr.EmployeeId == currentEmployee.Id && lr.Status == "Approved" && !lr.IsDeleted)
                        .GroupBy(lr => lr.StartDate.Month)
                        .Select(g => new { Month = g.Key, Count = g.Count() })
                        .OrderBy(g => g.Month)
                        .ToListAsync();

                    var months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                    ViewBag.TrendMonths = myTrends.Select(t => months[t.Month - 1]).ToList();
                    ViewBag.TrendCounts = myTrends.Select(t => t.Count).ToList();

                    // Recent leave requests (Issue 2)
                    ViewBag.RecentRequests = await _context.LeaveRequests
                        .Include(lr => lr.LeaveType)
                        .Where(lr => lr.EmployeeId == currentEmployee.Id && !lr.IsDeleted)
                        .OrderByDescending(lr => lr.CreatedDate)
                        .Take(5)
                        .ToListAsync();

                    // Recent notifications (Issue 2)
                    ViewBag.RecentNotifications = await _context.Notifications
                        .Where(n => n.EmployeeId == currentEmployee.Id && !n.IsDeleted)
                        .OrderByDescending(n => n.CreatedDate)
                        .Take(5)
                        .ToListAsync();
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("Home/Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("NotFound");
            }
            return View("Error500");
        }

        [Route("Home/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error500");
        }
    }
}
