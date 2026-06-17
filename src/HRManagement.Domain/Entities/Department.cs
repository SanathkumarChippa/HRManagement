// File Path: src/HRManagement.Domain/Entities/Department.cs
// Purpose: Entity representing a corporate department within the organization.
// Code Explanation: Inherits from BaseEntity, defines the Department Name, and has a one-to-many relationship with Employees.

using System.Collections.Generic;
using HRManagement.Domain.Common;

namespace HRManagement.Domain.Entities
{
    public class Department : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        // Navigation Properties
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
