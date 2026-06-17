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
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                currentEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            }

            // 2. Fetch General System Metrics
            ViewBag.TotalEmployees = await _context.Employees.CountAsync(e => !e.IsDeleted);
            ViewBag.ActiveEmployees = await _context.Employees.CountAsync(e => e.EmploymentStatus == "Active" && !e.IsDeleted);
            ViewBag.DepartmentCount = await _context.Departments.CountAsync(d => !d.IsDeleted);
            
            // On Leave Today
            ViewBag.OnLeaveToday = await _context.LeaveRequests
                .CountAsync(lr => lr.Status == "Approved" && 
                                  DateTime.Today >= lr.StartDate.Date && 
                                  DateTime.Today <= lr.EndDate.Date && 
                                  !lr.IsDeleted);

            // Pending Approvals
            ViewBag.PendingApprovals = await _context.LeaveRequests
                .CountAsync(lr => lr.Status == "Pending" && !lr.IsDeleted);

            if (User.IsInRole("Admin"))
            {
                ViewBag.DashboardType = "Admin";
                
                // Employee Growth (Headcount over years)
                var growthData = await _context.Employees
                    .Where(e => !e.IsDeleted)
                    .GroupBy(e => e.DateOfJoining.Year)
                    .Select(g => new { Year = g.Key, Count = g.Count() })
                    .OrderBy(g => g.Year)
                    .ToListAsync();
                
                ViewBag.GrowthYears = growthData.Select(g => g.Year.ToString()).ToList();
                ViewBag.GrowthCounts = growthData.Select(g => g.Count).ToList();
            }
            else if (User.IsInRole("HR Manager"))
            {
                ViewBag.DashboardType = "HRManager";

                // Department Distribution
                var deptData = await _context.Employees
                    .Where(e => !e.IsDeleted)
                    .GroupBy(e => e.Department.Name)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .ToListAsync();

                ViewBag.DeptNames = deptData.Select(d => d.Name).ToList();
                ViewBag.DeptCounts = deptData.Select(d => d.Count).ToList();

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
