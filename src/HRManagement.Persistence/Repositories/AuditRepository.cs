// File Path: src/HRManagement.Persistence/Repositories/AuditRepository.cs
// Purpose: Concrete implementation of AuditLog data access repository.
// Code Explanation: Inherits from GenericRepository<AuditLog>, implements IAuditRepository to query logs based on table names or date spans.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Domain.Entities;

namespace HRManagement.Persistence.Repositories
{
    public class AuditRepository : GenericRepository<AuditLog>, IAuditRepository
    {
        public AuditRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<AuditLog>> GetAuditLogsByTableAsync(string tableName)
        {
            return await _context.AuditLogs
                .Where(a => a.TableName == tableName)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<AuditLog>> GetAuditLogsByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.AuditLogs
                .Where(a => a.Timestamp >= start && a.Timestamp <= end)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}
