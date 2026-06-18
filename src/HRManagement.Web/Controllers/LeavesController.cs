// File Path: src/HRManagement.Web/Controllers/LeavesController.cs
// Purpose: MVC Controller managing LeaveRequest views and workflows.
// Code Explanation: Provides pages for Employees to apply/view history and Admin/HR Managers to approve or reject requests via AJAX.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Domain.Entities;
using HRManagement.Persistence;

namespace HRManagement.Web.Controllers
{
    [Authorize]
    public class LeavesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public LeavesController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                        ?? User.FindFirst("email")?.Value 
                        ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(email)) return null;
            return await _context.Employees.FirstOrDefaultAsync(e => e.Email.ToUpper() == email.ToUpper() && !e.IsDeleted);
        }

        // GET: Leaves (Apply Form & Current Balances)
        public async Task<IActionResult> Index()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
            {
                return Challenge();
            }

            var currentYear = DateTime.UtcNow.Year;
            var balances = await _unitOfWork.Leaves.GetLeaveBalancesByEmployeeAsync(employee.Id);
            var leaveTypes = await _context.LeaveTypes.Where(lt => !lt.IsDeleted).ToListAsync();
            
            var currentYearBalances = balances.Where(b => b.Year == currentYear).ToList();
            bool hasChanges = false;

            foreach (var lt in leaveTypes)
            {
                if (!currentYearBalances.Any(b => b.LeaveTypeId == lt.Id))
                {
                    var newBalance = new LeaveBalance
                    {
                        EmployeeId = employee.Id,
                        LeaveTypeId = lt.Id,
                        AllocatedDays = lt.DefaultAllocationDays,
                        UsedDays = 0,
                        PendingDays = 0,
                        Year = currentYear,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "System-SelfHealing"
                    };
                    await _context.LeaveBalances.AddAsync(newBalance);
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
                // Reload balances
                balances = await _unitOfWork.Leaves.GetLeaveBalancesByEmployeeAsync(employee.Id);
            }
            
            ViewBag.LeaveTypeId = new SelectList(leaveTypes, "Id", "Name");
            ViewBag.Balances = balances;
            
            return View();
        }

        // POST: Leaves/Apply (AJAX Submit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int leaveTypeId, DateTime startDate, DateTime endDate, string reason)
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
            {
                return Json(new { success = false, message = "Session expired or employee profile missing." });
            }

            var totalDays = (endDate.Date - startDate.Date).Days + 1;
            if (totalDays <= 0)
            {
                return Json(new { success = false, message = "End date must be on or after start date." });
            }

            // 1. Validate Balance
            var balance = await _unitOfWork.Leaves.GetLeaveBalanceAsync(employee.Id, leaveTypeId, startDate.Year);
            if (balance == null)
            {
                return Json(new { success = false, message = "No leave balance allocation found for this category and year." });
            }

            var availableDays = balance.AllocatedDays - balance.UsedDays - balance.PendingDays;
            if (totalDays > availableDays)
            {
                return Json(new { success = false, message = $"Insufficient leave balance. Requested: {totalDays}, Available: {availableDays}." });
            }

            // 2. Validate Overlapping Requests
            var hasOverlap = await _context.LeaveRequests
                .AnyAsync(lr => lr.EmployeeId == employee.Id && 
                                lr.Status != "Rejected" && 
                                lr.Status != "Cancelled" && 
                                (startDate.Date <= lr.EndDate.Date && endDate.Date >= lr.StartDate.Date));

            if (hasOverlap)
            {
                return Json(new { success = false, message = "Overlapping leave request already exists for the selected dates." });
            }

            // 3. Create Request
            var leaveRequest = new LeaveRequest
            {
                EmployeeId = employee.Id,
                LeaveTypeId = leaveTypeId,
                StartDate = startDate,
                EndDate = endDate,
                TotalDays = totalDays,
                Reason = reason,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = employee.Email
            };

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
                    CreatedDate = DateTime.UtcNow,
                    TargetUrl = "/Leaves/Queue"
                };
                await _context.Notifications.AddAsync(notification);
                await _unitOfWork.SaveAsync();
            }

            return Json(new { success = true, message = "Leave request submitted successfully." });
        }

        // GET: Leaves/History
        public async Task<IActionResult> History()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
            {
                return Challenge();
            }

            var requests = await _context.LeaveRequests
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.EmployeeId == employee.Id && !lr.IsDeleted)
                .OrderByDescending(lr => lr.StartDate)
                .ToListAsync();

            return View(requests);
        }

        // GET: Leaves/Queue (Approvals Queue for Admin & HR Manager)
        [Authorize(Roles = "Admin,HR Manager")]
        public async Task<IActionResult> Queue()
        {
            var requests = await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.Status == "Pending" && !lr.IsDeleted)
                .OrderByDescending(lr => lr.CreatedDate)
                .ToListAsync();

            return View(requests);
        }

        // POST: Leaves/Approve (AJAX)
        [HttpPost]
        [Authorize(Roles = "Admin,HR Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? comment)
        {
            var request = await _unitOfWork.Leaves.GetLeaveRequestWithDetailsAsync(id);
            if (request == null)
            {
                return Json(new { success = false, message = "Leave request not found." });
            }

            if (request.Status != "Pending")
            {
                return Json(new { success = false, message = "Only pending requests can be approved." });
            }

            var approver = await GetCurrentEmployeeAsync();
            if (approver == null)
            {
                return Json(new { success = false, message = "Approver profile not found." });
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
            request.UpdatedBy = User.Identity?.Name ?? "System";

            await _unitOfWork.SaveAsync();

            // Notify Employee
            var notification = new Notification
            {
                EmployeeId = request.EmployeeId,
                Message = $"Your leave request from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd} has been APPROVED.",
                IsRead = false,
                Type = "Approval",
                CreatedDate = DateTime.UtcNow,
                TargetUrl = "/Leaves/History"
            };
            await _context.Notifications.AddAsync(notification);
            await _unitOfWork.SaveAsync();

            return Json(new { success = true, message = "Leave request approved." });
        }

        // POST: Leaves/Reject (AJAX)
        [HttpPost]
        [Authorize(Roles = "Admin,HR Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? comment)
        {
            var request = await _unitOfWork.Leaves.GetLeaveRequestWithDetailsAsync(id);
            if (request == null)
            {
                return Json(new { success = false, message = "Leave request not found." });
            }

            if (request.Status != "Pending")
            {
                return Json(new { success = false, message = "Only pending requests can be rejected." });
            }

            var approver = await GetCurrentEmployeeAsync();
            if (approver == null)
            {
                return Json(new { success = false, message = "Approver profile not found." });
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
            request.UpdatedBy = User.Identity?.Name ?? "System";

            await _unitOfWork.SaveAsync();

            // Notify Employee
            var notification = new Notification
            {
                EmployeeId = request.EmployeeId,
                Message = $"Your leave request from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd} has been REJECTED.",
                IsRead = false,
                Type = "Approval",
                CreatedDate = DateTime.UtcNow,
                TargetUrl = "/Leaves/History"
            };
            await _context.Notifications.AddAsync(notification);
            await _unitOfWork.SaveAsync();

            return Json(new { success = true, message = "Leave request rejected." });
        }

        // POST: Leaves/Cancel (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var request = await _unitOfWork.Leaves.GetLeaveRequestWithDetailsAsync(id);
            if (request == null)
            {
                return Json(new { success = false, message = "Leave request not found." });
            }

            var employee = await GetCurrentEmployeeAsync();
            if (employee == null || request.EmployeeId != employee.Id)
            {
                return Json(new { success = false, message = "Unauthorized." });
            }

            if (request.Status != "Pending" && request.Status != "Approved")
            {
                return Json(new { success = false, message = "Cannot cancel requests that are already rejected or cancelled." });
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
                        return Json(new { success = false, message = "Cannot cancel approved leaves that have already started." });
                    }
                    balance.UsedDays -= request.TotalDays;
                }
            }

            request.Status = "Cancelled";
            request.UpdatedDate = DateTime.UtcNow;
            request.UpdatedBy = User.Identity?.Name ?? "System";

            await _unitOfWork.SaveAsync();

            return Json(new { success = true, message = "Leave request cancelled." });
        }
    }
}
