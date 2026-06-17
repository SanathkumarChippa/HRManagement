// File Path: src/HRManagement.Application/Contracts/Persistence/ILeaveRepository.cs
// Purpose: Repository interface defining data access contracts for LeaveRequests, LeaveTypes, and LeaveBalances.
// Code Explanation: Inherits from IGenericRepository<LeaveRequest> and includes signatures for fetching requests with employee details, filtering by employee ID, listing balances, and retrieving a specific balance profile.

using System.Collections.Generic;
using System.Threading.Tasks;
using HRManagement.Domain.Entities;

namespace HRManagement.Application.Contracts.Persistence
{
    public interface ILeaveRepository : IGenericRepository<LeaveRequest>
    {
        Task<LeaveRequest?> GetLeaveRequestWithDetailsAsync(int id);
        Task<IReadOnlyList<LeaveRequest>> GetLeaveRequestsByEmployeeAsync(int employeeId);
        Task<IReadOnlyList<LeaveRequest>> GetLeaveRequestsWithDetailsAsync();
        Task<IReadOnlyList<LeaveBalance>> GetLeaveBalancesByEmployeeAsync(int employeeId);
        Task<LeaveBalance?> GetLeaveBalanceAsync(int employeeId, int leaveTypeId, int year);
    }
}
