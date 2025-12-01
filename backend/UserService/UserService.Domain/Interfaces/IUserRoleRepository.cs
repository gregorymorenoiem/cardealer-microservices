using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.Domain.Entities;

namespace UserService.Domain.Interfaces
{
    public interface IUserRoleRepository
    {
        Task<UserRole?> GetByIdAsync(Guid id);
        Task<UserRole?> GetByUserAndRoleAsync(Guid userId, Guid roleId);
        Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId);
        Task<UserRole> AddAsync(UserRole userRole);
        Task UpdateAsync(UserRole userRole);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid userId, Guid roleId);
    }
}
