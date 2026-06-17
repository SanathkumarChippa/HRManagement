// File Path: src/HRManagement.Application/DTOs/Leave/LeaveBalanceDto.cs
// Purpose: DTO representing leave balance limits.
// Code Explanation: Holds allocation statistics including total allocated, used, pending days, and year category.

namespace HRManagement.Application.DTOs.Leave
{
    public class LeaveBalanceDto
    {
        public int Id { get; set; }
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public int AllocatedDays { get; set; }
        public int UsedDays { get; set; }
        public int PendingDays { get; set; }
        public int Year { get; set; }
    }
}
