// File Path: src/HRManagement.Application/DTOs/Leave/CreateLeaveRequestDto.cs
// Purpose: DTO representing a leave request application.
// Code Explanation: Holds fields submitted when requesting time off, like leave type, dates, and reason.

using System;

namespace HRManagement.Application.DTOs.Leave
{
    public class CreateLeaveRequestDto
    {
        public int LeaveTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
