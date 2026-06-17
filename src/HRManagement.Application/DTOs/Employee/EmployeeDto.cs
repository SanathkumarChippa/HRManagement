// File Path: src/HRManagement.Application/DTOs/Employee/EmployeeDto.cs
// Purpose: DTO representing detailed employee profile data.
// Code Explanation: Holds Employee properties, department details, and manager reference information.

using System;

namespace HRManagement.Application.DTOs.Employee
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime DateOfJoining { get; set; }
        public string EmploymentStatus { get; set; } = string.Empty;

        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;

        public int? ManagerId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
    }
}
