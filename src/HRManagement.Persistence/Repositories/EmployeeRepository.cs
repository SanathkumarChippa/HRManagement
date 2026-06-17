// File Path: src/HRManagement.Persistence/Repositories/EmployeeRepository.cs
// Purpose: Concrete implementation of Employee data access repository.
// Code Explanation: Inherits from GenericRepository<Employee>, implements IEmployeeRepository to fetch employee profiles, unique codes, and managerial subordinate structures.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Domain.Entities;

namespace HRManagement.Persistence.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Employee?> GetEmployeeByCodeAsync(string employeeCode)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
        }

        public async Task<Employee?> GetEmployeeWithDetailsAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Include(e => e.LeaveBalances)
                .ThenInclude(lb => lb.LeaveType)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IReadOnlyList<Employee>> GetSubordinatesAsync(int managerId)
        {
            return await _context.Employees
                .Where(e => e.ManagerId == managerId)
                .Include(e => e.Department)
                .ToListAsync();
        }
    }
}
