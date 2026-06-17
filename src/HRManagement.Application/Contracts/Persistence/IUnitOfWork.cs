// File Path: src/HRManagement.Application/Contracts/Persistence/IUnitOfWork.cs
// Purpose: Interface defining the Unit of Work pattern, coordinating repositories.
// Code Explanation: Implements IDisposable, aggregates standard CRUD repositories along with the custom ADO.NET reporting repository, and exposes SaveAsync for atomic transaction management.

using System;
using System.Threading.Tasks;

namespace HRManagement.Application.Contracts.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IEmployeeRepository Employees { get; }
        IDepartmentRepository Departments { get; }
        ILeaveRepository Leaves { get; }
        INotificationRepository Notifications { get; }
        IAuditRepository AuditLogs { get; }
        IWorkforceReportRepository WorkforceReports { get; }
        
        Task<int> SaveAsync();
    }
}
