// File Path: src/HRManagement.Domain/Entities/Employee.cs
// Purpose: Entity representing an employee in the platform.
// Code Explanation: Inherits from BaseEntity, defines fields like EmployeeCode, FirstName, LastName, Email, PhoneNumber, Gender, Designation, DateOfJoining, and EmploymentStatus. Includes a self-referencing navigation property for ManagerId/Manager/Subordinates.

using System;
using System.Collections.Generic;
using HRManagement.Domain.Common;

namespace HRManagement.Domain.Entities
{
    public class Employee : BaseEntity
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime DateOfJoining { get; set; }
        public string EmploymentStatus { get; set; } = "Active"; // Active, Resigned, Terminated
        public string? ProfilePicturePath { get; set; }
        
        // Foreign Keys
        public int DepartmentId { get; set; }
        public virtual Department? Department { get; set; }

        public int? ManagerId { get; set; }
        public virtual Employee? Manager { get; set; }

        // Navigation Properties
        public virtual ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
        public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
