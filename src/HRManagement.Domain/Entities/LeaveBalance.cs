// File Path: src/HRManagement.Domain/Entities/LeaveBalance.cs
// Purpose: Tracks allocated, used, and pending leave balances for employees.
// Code Explanation: Inherits from BaseEntity, contains links to Employee and LeaveType, and counts of AllocatedDays, UsedDays, PendingDays, and the current Year.

using HRManagement.Domain.Common;

namespace HRManagement.Domain.Entities
{
    public class LeaveBalance : BaseEntity
    {
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        public int LeaveTypeId { get; set; }
        public virtual LeaveType LeaveType { get; set; } = null!;

        public int AllocatedDays { get; set; }
        public int UsedDays { get; set; }
        public int PendingDays { get; set; }
        public int Year { get; set; }
    }
}
