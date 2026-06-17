// File Path: src/HRManagement.Domain/Entities/RefreshToken.cs
// Purpose: Entity representing refresh tokens associated with user accounts for persistent JWT session management.
// Code Explanation: Inherits from BaseEntity, stores the actual token value, expiration date, revocation status, IP logs, and active status computed helper properties.

using System;
using HRManagement.Domain.Common;

namespace HRManagement.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string? CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => Revoked == null && !IsExpired;
    }
}
