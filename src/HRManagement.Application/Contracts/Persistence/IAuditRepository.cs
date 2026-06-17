// File Path: src/HRManagement.Application/Contracts/Persistence/IAuditRepository.cs
// Purpose: Repository interface defining data access contracts for AuditLogs.
// Code Explanation: Inherits from IGenericRepository<AuditLog> and adds signatures for retrieving logs for specific tables, records, or dates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HRManagement.Domain.Entities;

namespace HRManagement.Application.Contracts.Persistence
{
    public interface IAuditRepository : IGenericRepository<AuditLog>
    {
        Task<IReadOnlyList<AuditLog>> GetAuditLogsByTableAsync(string tableName);
        Task<IReadOnlyList<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime start, DateTime end);
    }
}
