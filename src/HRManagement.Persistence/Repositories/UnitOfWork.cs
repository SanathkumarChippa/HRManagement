// File Path: src/HRManagement.Persistence/Repositories/UnitOfWork.cs
// Purpose: Concrete implementation of Unit of Work pattern managing EF Core transaction lifecycle.
// Code Explanation: Implements IUnitOfWork and manages lazy instantiation of individual repositories, disposing of the underlying DbContext on lifecycle end.

using System;
using System.Threading.Tasks;
using HRManagement.Application.Contracts.Persistence;

namespace HRManagement.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IEmployeeRepository? _employees;
        private IDepartmentRepository? _departments;
        private ILeaveRepository? _leaves;
        private INotificationRepository? _notifications;
        private IAuditRepository? _auditLogs;
        private IWorkforceReportRepository? _workforceReports;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEmployeeRepository Employees => _employees ??= new EmployeeRepository(_context);
        public IDepartmentRepository Departments => _departments ??= new DepartmentRepository(_context);
        public ILeaveRepository Leaves => _leaves ??= new LeaveRepository(_context);
        public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);
        public IAuditRepository AuditLogs => _auditLogs ??= new AuditRepository(_context);
        public IWorkforceReportRepository WorkforceReports => _workforceReports ??= new WorkforceReportRepository(_context);

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
