// File Path: src/HRManagement.API/Controllers/EmployeesController.cs
// Purpose: API controller managing Employee endpoints.
// Code Explanation: Provides endpoints to list, get, create, update, and soft-delete employees. Features search, filtering (by department and status), pagination, sorting, and automatically generates unique employee codes and leave balances during creation.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Application.DTOs.Employee;
using HRManagement.Domain.Entities;
using HRManagement.Persistence;

namespace HRManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public EmployeesController(IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int? departmentId,
            [FromQuery] string? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isAscending = true)
        {
            var employees = await _unitOfWork.Employees.GetAllAsync();
            var query = employees.AsQueryable();

            // 1. Search (Fuzzy Name/Email/Code match)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(e => 
                    e.FirstName.ToLower().Contains(searchLower) || 
                    e.LastName.ToLower().Contains(searchLower) || 
                    e.Email.ToLower().Contains(searchLower) || 
                    e.EmployeeCode.ToLower().Contains(searchLower));
            }

            // 2. Filter
            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.EmploymentStatus.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Sort
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("LastName", StringComparison.OrdinalIgnoreCase))
                {
                    query = isAscending ? query.OrderBy(e => e.LastName) : query.OrderByDescending(e => e.LastName);
                }
                else if (sortBy.Equals("EmployeeCode", StringComparison.OrdinalIgnoreCase))
                {
                    query = isAscending ? query.OrderBy(e => e.EmployeeCode) : query.OrderByDescending(e => e.EmployeeCode);
                }
                else
                {
                    query = isAscending ? query.OrderBy(e => e.FirstName) : query.OrderByDescending(e => e.FirstName);
                }
            }

            // 4. Paginate
            var totalCount = query.Count();
            var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            
            // Map to details DTO
            var dtos = new List<EmployeeDto>();
            foreach (var item in items)
            {
                var fullDetails = await _unitOfWork.Employees.GetEmployeeWithDetailsAsync(item.Id);
                if (fullDetails != null)
                {
                    dtos.Add(_mapper.Map<EmployeeDto>(fullDetails));
                }
            }

            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = dtos
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _unitOfWork.Employees.GetEmployeeWithDetailsAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<EmployeeDto>(employee));
        }

        [Authorize(Policy = "AdminOrHR")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            var employee = _mapper.Map<Employee>(dto);
            employee.CreatedDate = DateTime.UtcNow;

            // Generate Unique Employee Code
            var allEmployees = await _unitOfWork.Employees.GetAllAsync();
            var nextSequence = allEmployees.Count + 1;
            employee.EmployeeCode = $"EMP-{dto.DateOfJoining.Year}-{nextSequence:D4}";

            var result = await _unitOfWork.Employees.AddAsync(employee);
            await _unitOfWork.SaveAsync();

            // Seed default leave balances for this employee
            var leaveTypes = await _context.LeaveTypes.ToListAsync();
            foreach (var lt in leaveTypes)
            {
                var balance = new LeaveBalance
                {
                    EmployeeId = result.Id,
                    LeaveTypeId = lt.Id,
                    AllocatedDays = lt.DefaultAllocationDays,
                    UsedDays = 0,
                    PendingDays = 0,
                    Year = DateTime.UtcNow.Year
                };
                await _context.LeaveBalances.AddAsync(balance);
            }
            await _unitOfWork.SaveAsync();

            var mappedResult = await _unitOfWork.Employees.GetEmployeeWithDetailsAsync(result.Id);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, _mapper.Map<EmployeeDto>(mappedResult));
        }

        [Authorize(Policy = "AdminOrHR")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateEmployeeDto dto)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _mapper.Map(dto, employee);
            employee.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Employees.UpdateAsync(employee);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            await _unitOfWork.Employees.DeleteAsync(employee);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}
