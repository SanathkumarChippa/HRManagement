// File Path: src/HRManagement.Persistence/ApplicationDbContext.cs
// Purpose: Entity Framework Core DbContext for database operations, integrated with custom Identity models.
// Code Explanation: Inherits from IdentityDbContext using ApplicationUser and ApplicationRole. Automates auditing and soft delete inside SaveChangesAsync, configures entity mapping (including self-referencing Employee manager relations, indexing for EmployeeCode, and RowVersion optimistic concurrency via Fluent API), and sets up Global Query Filters to automatically ignore soft-deleted entities.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HRManagement.Domain.Common;
using HRManagement.Domain.Entities;

namespace HRManagement.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<LeaveType> LeaveTypes { get; set; } = null!;
        public DbSet<LeaveBalance> LeaveBalances { get; set; } = null!;
        public DbSet<LeaveRequest> LeaveRequests { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Custom ApplicationUser mappings
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(u => u.Employee)
                    .WithOne()
                    .HasForeignKey<ApplicationUser>(u => u.EmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // 2. Global Soft Delete Query Filters & Concurrency token for all BaseEntity types
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    // Global Soft Delete Filter
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var propertyMethodInfo = typeof(EF).GetMethod("Property")!.MakeGenericMethod(typeof(bool));
                    var isDeletedProperty = System.Linq.Expressions.Expression.Call(propertyMethodInfo, parameter, System.Linq.Expressions.Expression.Constant("IsDeleted"));
                    var compareExpression = System.Linq.Expressions.Expression.Equal(isDeletedProperty, System.Linq.Expressions.Expression.Constant(false));
                    var lambda = System.Linq.Expressions.Expression.Lambda(compareExpression, parameter);
                    
                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);

                    // RowVersion Optimistic Concurrency Configuration
                    builder.Entity(entityType.ClrType)
                        .Property(nameof(BaseEntity.RowVersion))
                        .IsRowVersion()
                        .ValueGeneratedOnAddOrUpdate();
                }
            }

            // 3. Employee Configuration
            builder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");
                entity.HasIndex(e => e.EmployeeCode).IsUnique();
                entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);

                // Manager self-referencing relationship
                entity.HasOne(e => e.Manager)
                    .WithMany(m => m.Subordinates)
                    .HasForeignKey(e => e.ManagerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Department relationship
                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 4. Department Configuration
            builder.Entity<Department>(entity =>
            {
                entity.ToTable("Departments");
                entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            });

            // 5. LeaveType Configuration
            builder.Entity<LeaveType>(entity =>
            {
                entity.ToTable("LeaveTypes");
                entity.Property(lt => lt.Name).IsRequired().HasMaxLength(100);
            });

            // 6. LeaveBalance Configuration
            builder.Entity<LeaveBalance>(entity =>
            {
                entity.ToTable("LeaveBalances");
                entity.HasKey(lb => lb.Id);
                
                entity.HasOne(lb => lb.Employee)
                    .WithMany(e => e.LeaveBalances)
                    .HasForeignKey(lb => lb.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(lb => lb.LeaveType)
                    .WithMany()
                    .HasForeignKey(lb => lb.LeaveTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 7. LeaveRequest Configuration
            builder.Entity<LeaveRequest>(entity =>
            {
                entity.ToTable("LeaveRequests");
                
                entity.HasOne(lr => lr.Employee)
                    .WithMany(e => e.LeaveRequests)
                    .HasForeignKey(lr => lr.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(lr => lr.LeaveType)
                    .WithMany()
                    .HasForeignKey(lr => lr.LeaveTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(lr => lr.ApprovedBy)
                    .WithMany()
                    .HasForeignKey(lr => lr.ApprovedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 8. Notification Configuration
            builder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.Property(n => n.Message).IsRequired().HasMaxLength(500);

                entity.HasOne(n => n.Employee)
                    .WithMany(e => e.Notifications)
                    .HasForeignKey(n => n.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 9. AuditLog Configuration
            builder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs");
                entity.Property(a => a.Action).IsRequired().HasMaxLength(50);
                entity.Property(a => a.TableName).IsRequired().HasMaxLength(100);
            });

            // 10. RefreshToken Configuration
            builder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.Property(r => r.Token).IsRequired().HasMaxLength(256);
            });

            // Automatically apply the 'kumarcapstone_' prefix to all tables
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                {
                    entityType.SetTableName("kumarcapstone_" + tableName);
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = OnBeforeSaveChanges("System"); // Defaulting to system, will be updated via user identity context in HTTP Pipeline

            // Apply Soft Delete & Auditing logic
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.UtcNow;
                        entry.Entity.CreatedBy ??= "System";
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedDate = DateTime.UtcNow;
                        entry.Entity.UpdatedBy ??= "System";
                        break;

                    case EntityState.Deleted:
                        // Intercept standard delete and convert it into soft delete
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedDate = DateTime.UtcNow;
                        entry.Entity.DeletedBy ??= "System";
                        break;
                }
            }

            var result = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChanges(auditEntries);
            return result;
        }

        private List<AuditEntry> OnBeforeSaveChanges(string userId)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Entity.GetType().Name,
                    UserId = userId
                };
                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue!;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = "Create";
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;
                            break;

                        case EntityState.Deleted:
                            auditEntry.AuditType = "Delete";
                            auditEntry.OldValues[propertyName] = property.OriginalValue!;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedProperties.Add(propertyName);
                                auditEntry.AuditType = "Update";
                                auditEntry.OldValues[propertyName] = property.OriginalValue!;
                                auditEntry.NewValues[propertyName] = property.CurrentValue!;
                            }
                            break;
                    }
                }
            }

            foreach (var auditEntry in auditEntries.Where(_ => !_.RequiresTemporaryKeys))
            {
                AuditLogs.Add(auditEntry.ToAuditLog());
            }

            return auditEntries.Where(_ => _.RequiresTemporaryKeys).ToList();
        }

        private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

            foreach (var auditEntry in auditEntries)
            {
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue!;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue!;
                    }
                }
                AuditLogs.Add(auditEntry.ToAuditLog());
            }

            return base.SaveChangesAsync();
        }
    }

    // Helper class for tracking audit state changes
    internal class AuditEntry
    {
        public AuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            Entry = entry;
        }

        public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; }
        public string? UserId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string AuditType { get; set; } = string.Empty;
        public Dictionary<string, object> KeyValues { get; } = new();
        public Dictionary<string, object> OldValues { get; } = new();
        public Dictionary<string, object> NewValues { get; } = new();
        public List<string> ChangedProperties { get; } = new();
        public List<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry> TemporaryProperties { get; } = new();

        public bool RequiresTemporaryKeys => Entry.Properties.Any(p => p.Metadata.IsPrimaryKey() && p.IsTemporary);

        public AuditLog ToAuditLog()
        {
            return new AuditLog
            {
                UserId = UserId,
                Action = AuditType,
                TableName = TableName,
                Timestamp = DateTime.UtcNow,
                PrimaryKey = System.Text.Json.JsonSerializer.Serialize(KeyValues),
                OldValues = OldValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(OldValues),
                NewValues = NewValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(NewValues),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = UserId ?? "System"
            };
        }
    }
}
