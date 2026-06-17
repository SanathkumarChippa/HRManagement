// File Path: src/HRManagement.Web/Controllers/DepartmentsController.cs
// Purpose: MVC Controller managing Department views.
// Code Explanation: Exposes views to list departments, edit names, and add new departments. Coordinates through Unit of Work.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Domain.Entities;
using HRManagement.Persistence;

namespace HRManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public DepartmentsController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var depts = await _context.Departments
                .Include(d => d.Employees)
                .Where(d => !d.IsDeleted)
                .ToListAsync();
            return View(depts);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                department.CreatedDate = DateTime.UtcNow;
                department.CreatedBy = User.Identity?.Name ?? "System";
                await _unitOfWork.Departments.AddAsync(department);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var stored = await _unitOfWork.Departments.GetByIdAsync(id);
                if (stored == null)
                {
                    return NotFound();
                }

                stored.Name = department.Name;
                stored.UpdatedDate = DateTime.UtcNow;
                stored.UpdatedBy = User.Identity?.Name ?? "System";

                await _unitOfWork.Departments.UpdateAsync(stored);
                await _unitOfWork.SaveAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }
    }
}
