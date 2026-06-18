// File Path: src/HRManagement.Web/Controllers/SearchController.cs
// Purpose: MVC Controller for searching across Employees, Departments, and Audit Logs.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRManagement.Domain.Entities;
using HRManagement.Persistence;

namespace HRManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,HR Manager")]
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new SearchResultsViewModel { Query = string.Empty });
            }

            var cleanQuery = query.Trim();
            var results = new SearchResultsViewModel { Query = cleanQuery };

            // 1. Search Employees
            var employeeQuery = _context.Employees
                .Include(e => e.Department)
                .Where(e => !e.IsDeleted);

            if (User.IsInRole("Employee"))
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                            ?? User.FindFirst("email")?.Value 
                            ?? User.Identity?.Name;
                if (!string.IsNullOrEmpty(email))
                {
                    employeeQuery = employeeQuery.Where(e => e.Email.ToUpper() == email.ToUpper());
                }
            }
            else if (User.IsInRole("HR Manager"))
            {
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole != null)
                {
                    var adminUserIds = await _context.UserRoles
                        .Where(ur => ur.RoleId == adminRole.Id)
                        .Select(ur => ur.UserId)
                        .ToListAsync();
                    
                    var adminEmployeeIds = await _context.Users
                        .Where(u => adminUserIds.Contains(u.Id) && u.EmployeeId != null)
                        .Select(u => u.EmployeeId)
                        .ToListAsync();
                        
                    employeeQuery = employeeQuery.Where(e => !adminEmployeeIds.Contains(e.Id));
                }
            }

            results.Employees = await employeeQuery
                .Where(e => e.FirstName.Contains(cleanQuery) || 
                            e.LastName.Contains(cleanQuery) || 
                            e.Email.Contains(cleanQuery) || 
                            e.EmployeeCode.Contains(cleanQuery) || 
                            e.Designation.Contains(cleanQuery))
                .ToListAsync();

            // 2. Search Departments (Admin only)
            if (User.IsInRole("Admin"))
            {
                results.Departments = await _context.Departments
                    .Where(d => !d.IsDeleted && d.Name.Contains(cleanQuery))
                    .ToListAsync();
            }

            // 3. Search Audit Logs (Admin and HR Managers only)
            if (User.IsInRole("Admin") || User.IsInRole("HR Manager"))
            {
                var logsQuery = _context.AuditLogs.AsNoTracking();
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

                        logsQuery = logsQuery.Where(l => 
                            l.UserId == null || 
                            (!adminUserEmails.Contains(l.UserId) && !adminUserIds.Contains(l.UserId) && l.UserId != "admin@hrmanagement.com")
                        );
                    }
                }

                results.AuditLogs = await logsQuery
                    .Where(l => l.Action.Contains(cleanQuery) || 
                                l.TableName.Contains(cleanQuery) || 
                                (l.PrimaryKey != null && l.PrimaryKey.Contains(cleanQuery)) || 
                                (l.UserId != null && l.UserId.Contains(cleanQuery)) ||
                                (l.OldValues != null && l.OldValues.Contains(cleanQuery)) ||
                                (l.NewValues != null && l.NewValues.Contains(cleanQuery)))
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();
            }

            return View(results);
        }
    }

    public class SearchResultsViewModel
    {
        public string Query { get; set; } = string.Empty;
        public List<Employee> Employees { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public List<AuditLog> AuditLogs { get; set; } = new();
    }
}
