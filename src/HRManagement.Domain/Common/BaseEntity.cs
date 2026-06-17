// File Path: src/HRManagement.Domain/Common/BaseEntity.cs
// Purpose: Base class containing common properties for all entities, supporting audit trails, soft deletes, and optimistic concurrency.
// Code Explanation: Provides standard properties like Id, audit logs (CreatedBy, CreatedDate, etc.), soft delete flags (IsDeleted, DeletedBy, etc.), and a byte array RowVersion decorated for EF Core optimistic concurrency.

using System;

namespace HRManagement.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        
        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        
        // Soft Delete Fields
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }
        
        // Optimistic Concurrency RowVersion
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
