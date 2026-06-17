// File Path: src/HRManagement.Domain/Entities/LeaveType.cs
// Purpose: Entity representing different categories of leave (Casual, Sick, Paid, Maternity).
// Code Explanation: Inherits from BaseEntity, contains type Name (e.g. "Casual Leave") and DefaultAllocationDays (e.g. 12).

using HRManagement.Domain.Common;

namespace HRManagement.Domain.Entities
{
    public class LeaveType : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int DefaultAllocationDays { get; set; }
    }
}
