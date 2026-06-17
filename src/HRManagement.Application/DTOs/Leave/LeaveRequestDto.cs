// File Path: src/HRManagement.Application/DTOs/Leave/LeaveRequestDto.cs
// Purpose: DTO representing a leave request record.
// Code Explanation: Exposes details of a leave application including employee info, leave type, start/end dates, total days, status, and approval notes.

using System;

namespace HRManagement.Application.DTOs.Leave
{
    public class LeaveRequestDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public int? ApprovedById { get; set; }
        public string ApprovedByName { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }
}
