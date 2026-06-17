// File Path: src/HRManagement.Domain/Entities/Notification.cs
// Purpose: Entity representing system notifications generated for employees.
// Code Explanation: Inherits from BaseEntity, links to an Employee, and stores notification text, read status, and category type.

using HRManagement.Domain.Common;

namespace HRManagement.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string Type { get; set; } = string.Empty; // System, LeaveRequest, Approval, Warning
    }
}
