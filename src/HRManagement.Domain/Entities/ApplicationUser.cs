// File Path: src/HRManagement.Domain/Entities/ApplicationUser.cs
// Purpose: Custom Identity User class representing users with system access.
// Code Explanation: Inherits from IdentityUser and includes a flag to enable/disable access (IsActive), profile logging (CreatedDate), and a link to the Employee record.

using System;
using Microsoft.AspNetCore.Identity;

namespace HRManagement.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Link to Employee Profile if applicable
        public int? EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        // Settings
        public string ThemePreference { get; set; } = "light";
        public bool EmailAlertsEnabled { get; set; } = true;
        public bool InAppNotificationsEnabled { get; set; } = true;
        public bool MustChangePassword { get; set; } = false;
        public bool IsFirstLogin { get; set; } = true;
    }
}
