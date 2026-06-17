// File Path: src/HRManagement.Persistence/Repositories/DepartmentRepository.cs
// Purpose: Concrete implementation of Department data access repository.
// Code Explanation: Inherits from GenericRepository<Department> and implements IDepartmentRepository to fetch departments along with employee lists.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Domain.Entities;

namespace HRManagement.Persistence.Repositories
{
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Department?> GetDepartmentWithEmployeesAsync(int id)
        {
            return await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
