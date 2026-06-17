// File Path: src/HRManagement.Application/DTOs/Reporting/DepartmentHeadcountReportDto.cs
// Purpose: Data Transfer Object representing department headcount analytical report statistics.
// Code Explanation: Holds basic report metrics including Department Name, Total Active Employees, and Total Leaves Approved for that department.

namespace HRManagement.Application.DTOs.Reporting
{
    public class DepartmentHeadcountReportDto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public int TotalActiveLeaves { get; set; }
    }
}
