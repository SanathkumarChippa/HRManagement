// File Path: src/HRManagement.Application/DTOs/Employee/CreateEmployeeDto.cs
// Purpose: DTO representing creation payloads for employees.
// Code Explanation: Holds fields necessary to register a new employee.

using System;

namespace HRManagement.Application.DTOs.Employee
{
    public class CreateEmployeeDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime DateOfJoining { get; set; }
        public int DepartmentId { get; set; }
        public int? ManagerId { get; set; }
    }
}
