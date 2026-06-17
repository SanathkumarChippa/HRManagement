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
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Domain.Entities;
using HRManagement.Persistence;

namespace HRManagement.Web.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public EmployeesController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Where(e => !e.IsDeleted)
                .ToListAsync();
            return View(employees);
        }

        public async Task<IActionResult> Details(int id)
        {
            var employee = await _unitOfWork.Employees.GetEmployeeWithDetailsAsync(id);
            if (employee == null)
            {
                return NotFound();
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
            if (ModelState.IsValid)
            {
                var all = await _unitOfWork.Employees.GetAllAsync();
                var count = all.Count + 1;
                employee.EmployeeCode = $"EMP-{employee.DateOfJoining.Year}-{count:D4}";
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

                return RedirectToAction(nameof(Index));
            }

            var depts = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.DepartmentId = new SelectList(depts, "Id", "Name", employee.DepartmentId);
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

            if (ModelState.IsValid)
            {
                var stored = await _unitOfWork.Employees.GetByIdAsync(id);
                if (stored == null)
                {
                    return NotFound();
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

                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }
    }
}
