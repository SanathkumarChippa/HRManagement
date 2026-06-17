// File Path: src/HRManagement.Application/Contracts/Persistence/IEmployeeRepository.cs
// Purpose: Repository interface defining data access contracts specific to Employees.
// Code Explanation: Inherits from IGenericRepository<Employee> and declares methods for fetching an employee by code, retrieving complete details with related entities, and getting subordinates list.

using System.Threading.Tasks;
using System.Collections.Generic;
using HRManagement.Domain.Entities;

namespace HRManagement.Application.Contracts.Persistence
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<Employee?> GetEmployeeByCodeAsync(string employeeCode);
        Task<Employee?> GetEmployeeWithDetailsAsync(int id);
        Task<IReadOnlyList<Employee>> GetSubordinatesAsync(int managerId);
    }
}
