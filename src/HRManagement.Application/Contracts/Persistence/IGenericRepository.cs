// File Path: src/HRManagement.Application/Contracts/Persistence/IGenericRepository.cs
// Purpose: Base generic repository interface defining standard CRUD operations.
// Code Explanation: Provides standard query/write signatures (GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync, ExistsAsync) using generics, restricted to reference types.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRManagement.Application.Contracts.Persistence
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(int id);
    }
}
