// File Path: src/HRManagement.Domain/Entities/ApplicationRole.cs
// Purpose: Custom Identity Role class for RBAC (Role-Based Access Control).
// Code Explanation: Inherits from IdentityRole and adds a custom description field for system roles.

using Microsoft.AspNetCore.Identity;

namespace HRManagement.Domain.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }

        public ApplicationRole() : base()
        {
        }

        public ApplicationRole(string roleName, string? description = null) : base(roleName)
        {
            Description = description;
        }
    }
}
