// File Path: src/HRManagement.Application/Contracts/Persistence/IDepartmentRepository.cs
// Purpose: Repository interface defining data access contracts specific to Departments.
// Code Explanation: Inherits from IGenericRepository<Department> and adds a method for retrieving department details with employees.

using System.Threading.Tasks;
using HRManagement.Domain.Entities;

namespace HRManagement.Application.Contracts.Persistence
{
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        Task<Department?> GetDepartmentWithEmployeesAsync(int id);
    }
}
