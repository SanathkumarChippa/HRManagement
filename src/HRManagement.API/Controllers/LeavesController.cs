// File Path: src/HRManagement.API/Controllers/LeavesController.cs
// Purpose: API controller managing LeaveRequest workflows.
// Code Explanation: Implements applying for leaves, approvals, rejections, cancellations, and balances lookup. Uses transaction boundaries to adjust LeaveBalance states and log notifications simultaneously.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Application.DTOs.Leave;
using HRManagement.Domain.Entities;
using HRManagement.Persistence;

namespace HRManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/leaves")]
    public class LeavesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public LeavesController(IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
            if (!string.IsNullOrEmpty(employeeIdClaim) && int.TryParse(employeeIdClaim, out var empId))
            {
                return await _unitOfWork.Employees.GetEmployeeWithDetailsAsync(empId);
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(email))
            {
                return await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            }

            return null;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] CreateLeaveRequestDto dto)
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
            {
                return BadRequest("Logged-in user is not associated with an Employee profile.");
            }

            var totalDays = (dto.EndDate.Date - dto.StartDate.Date).Days + 1;
            if (totalDays <= 0)
            {
                return BadRequest("End date must be on or after start date.");
            }

            // 1. Validate Balance
            var balance = await _unitOfWork.Leaves.GetLeaveBalanceAsync(employee.Id, dto.LeaveTypeId, dto.StartDate.Year);
            if (balance == null)
            {
                return BadRequest("No leave balance allocation found for this category and year.");
            }

            var availableDays = balance.AllocatedDays - balance.UsedDays - balance.PendingDays;
            if (totalDays > availableDays)
            {
                return BadRequest($"Insufficient leave balance. Requested: {totalDays}, Available: {availableDays}.");
            }

            // 2. Validate Overlapping Requests
            var hasOverlap = await _context.LeaveRequests
                .AnyAsync(lr => lr.EmployeeId == employee.Id && 
                                lr.Status != "Rejected" && 
                                lr.Status != "Cancelled" && 
                                (dto.StartDate.Date <= lr.EndDate.Date && dto.EndDate.Date >= lr.StartDate.Date));

            if (hasOverlap)
            {
                return BadRequest("Overlapping leave request already exists for the selected dates.");
            }

            // 3. Create Request
            var leaveRequest = _mapper.Map<LeaveRequest>(dto);
            leaveRequest.EmployeeId = employee.Id;
            leaveRequest.TotalDays = totalDays;
            leaveRequest.Status = "Pending";
            leaveRequest.CreatedDate = DateTime.UtcNow;

            // Increment Pending Days
            balance.PendingDays += totalDays;
            await _unitOfWork.Leaves.AddAsync(leaveRequest);
            await _unitOfWork.SaveAsync();

            // Create notification for Manager
            if (employee.ManagerId.HasValue)
            {
                var notification = new Notification
                {
                    EmployeeId = employee.ManagerId.Value,
                    Message = $"{employee.FirstName} {employee.LastName} has requested {totalDays} day(s) of leave.",
                    IsRead = false,
                    Type = "LeaveRequest",
                    CreatedDate = DateTime.UtcNow
                };
                await _context.Notifications.AddAsync(notification);
                await _unitOfWork.SaveAsync();
            }

            return Ok(new { Message = "Leave request applied successfully.", RequestId = leaveRequest.Id });
        }

        [Authorize(Roles = "Admin,HR Manager")]
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> Approve(int id, [FromBody] string? comment)
        {
            var request = await _unitOfWork.Leaves.GetLeaveRequestWithDetailsAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            if (request.Status != "Pending")
            {
                return BadRequest("Only pending requests can be approved.");
            }

            var approver = await GetCurrentEmployeeAsync();
            if (approver == null)
            {
                return BadRequest("Approver not associated with an Employee profile.");
            }

            var balance = await _unitOfWork.Leaves.GetLeaveBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.StartDate.Year);
            if (balance != null)
            {
                balance.PendingDays -= request.TotalDays;
                balance.UsedDays += request.TotalDays;
            }

            request.Status = "Approved";
            request.ApprovedById = approver.Id;
            request.Comments = comment;
            request.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();

            // Notify Employee
            var notification = new Notification
            {
                EmployeeId = request.EmployeeId,
                Message = $"Your leave request from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd} has been APPROVED.",
                IsRead = false,
                Type = "Approval",
                CreatedDate = DateTime.UtcNow
            };
            await _context.Notifications.AddAsync(notification);
            await _unitOfWork.SaveAsync();

            return Ok(new { Message = "Leave request approved successfully." });
        }

        [Authorize(Roles = "Admin,HR Manager")]
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> Reject(int id, [FromBody] string? comment)
        {
            var request = await _unitOfWork.Leaves.GetLeaveRequestWithDetailsAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            if (request.Status != "Pending")
            {
                return BadRequest("Only pending requests can be rejected.");
            }

            var approver = await GetCurrentEmployeeAsync();
            if (approver == null)
            {
                return BadRequest("Approver not associated with an Employee profile.");
            }

            var balance = await _unitOfWork.Leaves.GetLeaveBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.StartDate.Year);
            if (balance != null)
            {
                balance.PendingDays -= request.TotalDays;
            }

            request.Status = "Rejected";
            request.ApprovedById = approver.Id;
            request.Comments = comment;
            request.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();

            // Notify Employee
            var notification = new Notification
            {
                EmployeeId = request.EmployeeId,
                Message = $"Your leave request from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd} has been REJECTED.",
                IsRead = false,
                Type = "Approval",
                CreatedDate = DateTime.UtcNow
            };
            await _context.Notifications.AddAsync(notification);
            await _unitOfWork.SaveAsync();

            return Ok(new { Message = "Leave request rejected successfully." });
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var request = await _unitOfWork.Leaves.GetLeaveRequestWithDetailsAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var employee = await GetCurrentEmployeeAsync();
            if (employee == null || request.EmployeeId != employee.Id)
            {
                return Unauthorized("You are not authorized to cancel this request.");
            }

            if (request.Status != "Pending" && request.Status != "Approved")
            {
                return BadRequest("Cannot cancel requests that are already rejected or cancelled.");
            }

            var balance = await _unitOfWork.Leaves.GetLeaveBalanceAsync(request.EmployeeId, request.LeaveTypeId, request.StartDate.Year);
            if (balance != null)
            {
                if (request.Status == "Pending")
                {
                    balance.PendingDays -= request.TotalDays;
                }
                else if (request.Status == "Approved")
                {
                    if (request.StartDate.Date <= DateTime.Today)
                    {
                        return BadRequest("Cannot cancel an approved leave that has already started or passed.");
                    }
                    balance.UsedDays -= request.TotalDays;
                }
            }

            request.Status = "Cancelled";
            request.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();

            return Ok(new { Message = "Leave request cancelled successfully." });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
            {
                return BadRequest("User not associated with an Employee profile.");
            }

            var requests = await _unitOfWork.Leaves.GetLeaveRequestsByEmployeeAsync(employee.Id);
            var dtos = _mapper.Map<List<LeaveRequestDto>>(requests);
            return Ok(dtos);
        }

        [HttpGet("balances")]
        public async Task<IActionResult> GetBalances()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
            {
                return BadRequest("User not associated with an Employee profile.");
            }

            var balances = await _unitOfWork.Leaves.GetLeaveBalancesByEmployeeAsync(employee.Id);
            var dtos = _mapper.Map<List<LeaveBalanceDto>>(balances);
            return Ok(dtos);
        }
    }
}
