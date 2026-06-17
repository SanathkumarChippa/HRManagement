// File Path: src/HRManagement.Persistence/Repositories/LeaveRepository.cs
// Purpose: Concrete implementation of LeaveRequest, LeaveBalance, and LeaveType data access repository.
// Code Explanation: Inherits from GenericRepository<LeaveRequest>, implements ILeaveRepository to fetch detailed leave applications, list balances, and check specific Category balances.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Domain.Entities;

namespace HRManagement.Persistence.Repositories
{
    public class LeaveRepository : GenericRepository<LeaveRequest>, ILeaveRepository
    {
        public LeaveRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<LeaveRequest?> GetLeaveRequestWithDetailsAsync(int id)
        {
            return await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.ApprovedBy)
                .FirstOrDefaultAsync(lr => lr.Id == id);
        }

        public async Task<IReadOnlyList<LeaveRequest>> GetLeaveRequestsByEmployeeAsync(int employeeId)
        {
            return await _context.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId)
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.ApprovedBy)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<LeaveRequest>> GetLeaveRequestsWithDetailsAsync()
        {
            return await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.ApprovedBy)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<LeaveBalance>> GetLeaveBalancesByEmployeeAsync(int employeeId)
        {
            return await _context.LeaveBalances
                .Where(lb => lb.EmployeeId == employeeId)
                .Include(lb => lb.LeaveType)
                .ToListAsync();
        }

        public async Task<LeaveBalance?> GetLeaveBalanceAsync(int employeeId, int leaveTypeId, int year)
        {
            return await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.LeaveTypeId == leaveTypeId && lb.Year == year);
        }
    }
}
