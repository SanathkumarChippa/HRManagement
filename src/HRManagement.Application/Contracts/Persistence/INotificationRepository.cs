// File Path: src/HRManagement.Application/Contracts/Persistence/INotificationRepository.cs
// Purpose: Repository interface defining data access contracts for Notifications.
// Code Explanation: Inherits from IGenericRepository<Notification> and provides methods for fetching notifications by employee ID and marking them as read.

using System.Collections.Generic;
using System.Threading.Tasks;
using HRManagement.Domain.Entities;

namespace HRManagement.Application.Contracts.Persistence
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IReadOnlyList<Notification>> GetNotificationsByEmployeeAsync(int employeeId);
        Task MarkAllAsReadAsync(int employeeId);
    }
}
