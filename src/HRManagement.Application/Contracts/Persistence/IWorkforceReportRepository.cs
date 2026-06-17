// File Path: src/HRManagement.Application/Contracts/Persistence/IWorkforceReportRepository.cs
// Purpose: Repository interface defining reporting operations using direct database queries.
// Code Explanation: Declares a method for fetching department headcount summaries as DTO items.

using System.Collections.Generic;
using System.Threading.Tasks;
using HRManagement.Application.DTOs.Reporting;

namespace HRManagement.Application.Contracts.Persistence
{
    public interface IWorkforceReportRepository
    {
        Task<IReadOnlyList<DepartmentHeadcountReportDto>> GetDepartmentHeadcountReportAsync();
    }
}
