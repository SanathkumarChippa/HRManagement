// File Path: src/HRManagement.Web/Controllers/EmployeesController.cs
// Purpose: MVC Controller managing Employee views.
// Code Explanation: Provides standard views to view lists, create employees, edit records, and show employee profile details, utilizing IUnitOfWork and mapping.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Domain.Entities;
using HRManagement.Persistence;

namespace HRManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,HR Manager")]
    public class EmployeesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeesController(IUnitOfWork unitOfWork, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var query = _context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Where(e => !e.IsDeleted);

            if (User.IsInRole("Employee"))
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                            ?? User.FindFirst("email")?.Value 
                            ?? User.Identity?.Name;
                if (!string.IsNullOrEmpty(email))
                {
                    query = query.Where(e => e.Email.ToUpper() == email.ToUpper());
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
                        
                    query = query.Where(e => !adminEmployeeIds.Contains(e.Id));
                }
            }

            var employees = await query.ToListAsync();
            return View(employees);
        }

        public async Task<IActionResult> Details(int id)
        {
            var employee = await _unitOfWork.Employees.GetEmployeeWithDetailsAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Employee"))
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                            ?? User.FindFirst("email")?.Value 
                            ?? User.Identity?.Name;
                if (string.IsNullOrEmpty(email) || employee.Email.ToUpper() != email.ToUpper()) return Forbid();
            }
            else if (User.IsInRole("HR Manager"))
            {
                if (await IsAdminEmployeeAsync(employee.Id)) return Forbid();
            }

            return View(employee);
        }

        [Authorize(Roles = "Admin,HR Manager")]
        public async Task<IActionResult> Create()
        {
            var depts = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.DepartmentId = new SelectList(depts, "Id", "Name");

            var managers = await _context.Employees.Where(e => !e.IsDeleted).ToListAsync();
            ViewBag.ManagerId = new SelectList(managers, "Id", "FirstName");

            return View();
        }

        [Authorize(Roles = "Admin,HR Manager")]
        [HttpPost]
        public async Task<IActionResult> Create(Employee employee)
        {
            ModelState.Remove(nameof(employee.EmployeeCode));
            ModelState.Remove(nameof(employee.RowVersion));
            ModelState.Remove(nameof(employee.Department));
            ModelState.Remove(nameof(employee.Manager));

            if (ModelState.IsValid)
            {
                bool emailExists = await _context.Employees.AnyAsync(e => e.Email == employee.Email && !e.IsDeleted);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "An employee with this email address already exists.");
                }
                else
                {
                    // Auto-generate unique sequential Employee Code
                    string prefix = $"EMP-{employee.DateOfJoining.Year}-";
                    var lastEmployee = await _context.Employees
                        .IgnoreQueryFilters()
                        .Where(e => e.EmployeeCode.StartsWith(prefix))
                        .OrderByDescending(e => e.EmployeeCode)
                        .FirstOrDefaultAsync();

                    int nextNumber = 1;
                    if (lastEmployee != null)
                    {
                        var parts = lastEmployee.EmployeeCode.Split('-');
                        if (parts.Length == 3 && int.TryParse(parts[2], out int lastNum))
                        {
                            nextNumber = lastNum + 1;
                        }
                    }
                    employee.EmployeeCode = $"{prefix}{nextNumber:D4}";
                    employee.CreatedDate = DateTime.UtcNow;

                    await _unitOfWork.Employees.AddAsync(employee);
                    await _unitOfWork.SaveAsync();

                    // Seed Leave Balances
                    var leaveTypes = await _context.LeaveTypes.ToListAsync();
                    foreach (var lt in leaveTypes)
                    {
                        var balance = new LeaveBalance
                        {
                            EmployeeId = employee.Id,
                            LeaveTypeId = lt.Id,
                            AllocatedDays = lt.DefaultAllocationDays,
                            UsedDays = 0,
                            PendingDays = 0,
                            Year = DateTime.UtcNow.Year
                        };
                        await _context.LeaveBalances.AddAsync(balance);
                    }
                    await _unitOfWork.SaveAsync();

                    // Auto-create Identity User
                    var tempPassword = $"Temp@{new Random().Next(10000, 100000)}";
                    var user = new ApplicationUser
                    {
                        UserName = employee.Email,
                        Email = employee.Email,
                        EmailConfirmed = true,
                        IsActive = true,
                        EmployeeId = employee.Id,
                        MustChangePassword = true,
                        IsFirstLogin = true
                    };

                    var userResult = await _userManager.CreateAsync(user, tempPassword);
                    if (userResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Employee");
                        TempData["TempCredentialsEmail"] = employee.Email;
                        TempData["TempCredentialsPassword"] = tempPassword;
                        TempData["TempCredentialsCode"] = employee.EmployeeCode;
                    }

                    // Create Custom Audit Log
                    var auditLog = new AuditLog
                    {
                        Action = "Employee Created",
                        TableName = "Employees",
                        PrimaryKey = employee.Id.ToString(),
                        UserId = User.Identity?.Name ?? "System",
                        CreatedBy = User.Identity?.Name ?? "System",
                        CreatedDate = DateTime.UtcNow,
                        NewValues = System.Text.Json.JsonSerializer.Serialize(new {
                            Email = employee.Email,
                            TemporaryPasswordGenerated = "Yes"
                        })
                    };
                    await _context.AuditLogs.AddAsync(auditLog);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Employee created successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var depts = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.DepartmentId = new SelectList(depts, "Id", "Name", employee.DepartmentId);
            
            var managers = await _context.Employees.Where(e => !e.IsDeleted).ToListAsync();
            ViewBag.ManagerId = new SelectList(managers, "Id", "FirstName", employee.ManagerId);

            return View(employee);
        }

        [Authorize(Roles = "Admin,HR Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            if (User.IsInRole("HR Manager"))
            {
                if (await IsAdminEmployeeAsync(employee.Id)) return Forbid();
            }

            var depts = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.DepartmentId = new SelectList(depts, "Id", "Name", employee.DepartmentId);

            var managers = await _context.Employees.Where(e => e.Id != id && !e.IsDeleted).ToListAsync();
            ViewBag.ManagerId = new SelectList(managers, "Id", "FirstName", employee.ManagerId);

            return View(employee);
        }

        [Authorize(Roles = "Admin,HR Manager")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return BadRequest();
            }

            ModelState.Remove(nameof(employee.EmployeeCode));
            ModelState.Remove(nameof(employee.RowVersion));
            ModelState.Remove(nameof(employee.Department));
            ModelState.Remove(nameof(employee.Manager));

            if (ModelState.IsValid)
            {
                bool emailExists = await _context.Employees.AnyAsync(e => e.Email == employee.Email && e.Id != id && !e.IsDeleted);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "An employee with this email address already exists.");
                }
                else
                {
                    var stored = await _unitOfWork.Employees.GetByIdAsync(id);
                    if (stored == null)
                    {
                        return NotFound();
                    }

                    if (User.IsInRole("HR Manager"))
                    {
                        if (await IsAdminEmployeeAsync(stored.Id)) return Forbid();
                    }

                    stored.FirstName = employee.FirstName;
                    stored.LastName = employee.LastName;
                    stored.Email = employee.Email;
                    stored.PhoneNumber = employee.PhoneNumber;
                    stored.Gender = employee.Gender;
                    stored.Designation = employee.Designation;
                    stored.DepartmentId = employee.DepartmentId;
                    stored.ManagerId = employee.ManagerId;
                    stored.EmploymentStatus = employee.EmploymentStatus;
                    stored.UpdatedDate = DateTime.UtcNow;

                    await _unitOfWork.Employees.UpdateAsync(stored);
                    await _unitOfWork.SaveAsync();

                    TempData["SuccessMessage"] = "Employee updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var depts = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.DepartmentId = new SelectList(depts, "Id", "Name", employee.DepartmentId);

            var managers = await _context.Employees.Where(e => e.Id != id && !e.IsDeleted).ToListAsync();
            ViewBag.ManagerId = new SelectList(managers, "Id", "FirstName", employee.ManagerId);

            return View(employee);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            if (User.IsInRole("HR Manager"))
            {
                if (await IsAdminEmployeeAsync(employee.Id)) return Forbid();
            }

            _context.Employees.Remove(employee); // Converts to soft delete via EF interceptor
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee soft-deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletedList()
        {
            var deletedEmployees = await _context.Employees
                .IgnoreQueryFilters()
                .Where(e => e.IsDeleted)
                .Include(e => e.Department)
                .ToListAsync();
            return View(deletedEmployees);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var employee = await _context.Employees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            if (!employee.IsDeleted)
            {
                return BadRequest("Employee is not deleted.");
            }

            employee.IsDeleted = false;
            employee.DeletedDate = null;
            employee.DeletedBy = null;
            employee.UpdatedDate = DateTime.UtcNow;
            employee.UpdatedBy = User.Identity?.Name ?? "System";

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee restored successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> IsAdminEmployeeAsync(int employeeId)
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null) return false;

            var adminUserIds = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();
            
            var adminEmployeeIds = await _context.Users
                .Where(u => adminUserIds.Contains(u.Id) && u.EmployeeId != null)
                .Select(u => u.EmployeeId)
                .ToListAsync();
                
            return adminEmployeeIds.Contains(employeeId);
        }
    }
}
