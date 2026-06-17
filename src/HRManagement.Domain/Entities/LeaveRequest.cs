// File Path: src/HRManagement.Domain/Entities/LeaveRequest.cs
// Purpose: Entity representing a leave application requested by an employee.
// Code Explanation: Inherits from BaseEntity, tracks dates, total days, application reason, approval status (Pending, Approved, Rejected), and holds a reference to the manager or HR manager who approved/rejected it.

using System;
using HRManagement.Domain.Common;

namespace HRManagement.Domain.Entities
{
    public class LeaveRequest : BaseEntity
    {
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        public int LeaveTypeId { get; set; }
        public virtual LeaveType LeaveType { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        
        public int? ApprovedById { get; set; }
        public virtual Employee? ApprovedBy { get; set; }
        
        public string? Comments { get; set; }
    }
}
