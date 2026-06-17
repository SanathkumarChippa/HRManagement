// File Path: src/HRManagement.Domain/Entities/AuditLog.cs
// Purpose: Entity representing system audit logs tracking data mutation.
// Code Explanation: Inherits from BaseEntity, tracks which user performed what action (Create, Update, Delete) on which table, key ID, along with serialized JSON values of old and new data states.

using System;
using HRManagement.Domain.Common;

namespace HRManagement.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public string? UserId { get; set; }
        public string Action { get; set; } = string.Empty; // Create, Update, Delete
        public string TableName { get; set; } = string.Empty;
        public string PrimaryKey { get; set; } = string.Empty;
        public string? OldValues { get; set; } // JSON format
        public string? NewValues { get; set; } // JSON format
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
