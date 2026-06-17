// File Path: src/HRManagement.Persistence/Repositories/WorkforceReportRepository.cs
// Purpose: Repository implementation executing direct ADO.NET SQL commands.
// Code Explanation: Extracts the underlying database connection from EF Core, opens it, constructs a raw SQL query selecting counts and aggregations, executes an asynchronous DbDataReader, and maps rows directly to DepartmentHeadcountReportDto instances.

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Application.DTOs.Reporting;

namespace HRManagement.Persistence.Repositories
{
    public class WorkforceReportRepository : IWorkforceReportRepository
    {
        private readonly ApplicationDbContext _context;

        public WorkforceReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<DepartmentHeadcountReportDto>> GetDepartmentHeadcountReportAsync()
        {
            var reports = new List<DepartmentHeadcountReportDto>();
            var connection = _context.Database.GetDbConnection();

            await using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT 
                        d.Name AS DepartmentName,
                        COUNT(e.Id) AS TotalEmployees,
                        SUM(CASE WHEN lr.Status = 'Approved' THEN 1 ELSE 0 END) AS TotalActiveLeaves
                    FROM Departments d
                    LEFT JOIN Employees e ON d.Id = e.DepartmentId AND e.IsDeleted = 0
                    LEFT JOIN LeaveRequests lr ON e.Id = lr.EmployeeId AND lr.Status = 'Approved' AND lr.IsDeleted = 0
                    WHERE d.IsDeleted = 0
                    GROUP BY d.Name";
                
                command.CommandType = CommandType.Text;

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        reports.Add(new DepartmentHeadcountReportDto
                        {
                            DepartmentName = reader.GetString(0),
                            TotalEmployees = reader.GetInt32(1),
                            TotalActiveLeaves = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
                        });
                    }
                }
            }

            return reports;
        }
    }
}
