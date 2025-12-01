using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Persistence
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserRole?> GetByIdAsync(Guid id)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .FirstOrDefaultAsync(ur => ur.Id == id);
        }

        public async Task<UserRole?> GetByUserAndRoleAsync(Guid userId, Guid roleId)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        public async Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .OrderByDescending(ur => ur.AssignedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId)
        {
            return await _context.UserRoles
                .Include(ur => ur.User)
                .Where(ur => ur.RoleId == roleId && ur.IsActive)
                .OrderByDescending(ur => ur.AssignedAt)
                .ToListAsync();
        }

        public async Task<UserRole> AddAsync(UserRole userRole)
        {
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
            return userRole;
        }

        public async Task UpdateAsync(UserRole userRole)
        {
            _context.UserRoles.Update(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var userRole = await GetByIdAsync(id);
            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid roleId)
        {
            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        }
    }
}
