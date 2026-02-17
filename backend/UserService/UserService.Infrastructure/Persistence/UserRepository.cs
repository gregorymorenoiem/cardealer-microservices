using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Persistence
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<User>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<User> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<int> CountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<IEnumerable<User>> GetAllWithFiltersAsync(
            string? search = null,
            string? type = null,
            string? status = null,
            bool? verified = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Users.AsQueryable();

            // Exclude platform admin accounts from regular user listings
            query = query.Where(u => u.AccountType != AccountType.Admin && u.AccountType != AccountType.PlatformEmployee);

            // Search filter (name, email, phone) - using existing columns
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(searchLower) ||
                    u.FirstName.ToLower().Contains(searchLower) ||
                    u.LastName.ToLower().Contains(searchLower) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchLower)));
            }

            // Type filter (buyer, seller, dealer) - using AccountType enum
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = type.ToLower() switch
                {
                    "dealer" => query.Where(u => u.AccountType == AccountType.Dealer),
                    "seller" => query.Where(u => u.AccountType == AccountType.Seller),
                    "buyer" => query.Where(u => u.AccountType == AccountType.Buyer || u.AccountType == AccountType.Guest),
                    _ => query
                };
            }

            // Status filter - using IsActive only (IsSuspended/IsBanned may not exist)
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = status.ToLower() switch
                {
                    "active" => query.Where(u => u.IsActive),
                    "pending" => query.Where(u => !u.IsActive),
                    _ => query
                };
            }

            // Verified filter - using EmailConfirmed
            if (verified.HasValue)
            {
                query = query.Where(u => u.EmailConfirmed == verified.Value);
            }

            return await query
                .Include(u => u.UserRoles)
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountWithFiltersAsync(
            string? search = null,
            string? type = null,
            string? status = null,
            bool? verified = null)
        {
            var query = _context.Users.AsQueryable();

            // Exclude platform admin accounts from regular user counts
            query = query.Where(u => u.AccountType != AccountType.Admin && u.AccountType != AccountType.PlatformEmployee);

            // Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(searchLower) ||
                    u.FirstName.ToLower().Contains(searchLower) ||
                    u.LastName.ToLower().Contains(searchLower) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchLower)));
            }

            // Type filter
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = type.ToLower() switch
                {
                    "dealer" => query.Where(u => u.AccountType == AccountType.Dealer),
                    "seller" => query.Where(u => u.AccountType == AccountType.Seller),
                    "buyer" => query.Where(u => u.AccountType == AccountType.Buyer || u.AccountType == AccountType.Guest),
                    _ => query
                };
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = status.ToLower() switch
                {
                    "active" => query.Where(u => u.IsActive),
                    "pending" => query.Where(u => !u.IsActive),
                    _ => query
                };
            }

            // Verified filter
            if (verified.HasValue)
            {
                query = query.Where(u => u.EmailConfirmed == verified.Value);
            }

            return await query.CountAsync();
        }

        public async Task<int> CountByStatusAsync(bool isActive, bool isSuspended, bool isBanned)
        {
            // Simplified version using only IsActive
            return await _context.Users
                .Where(u => u.IsActive == isActive)
                .CountAsync();
        }

        public async Task<int> CountActiveAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .CountAsync();
        }

        public async Task<int> CountCreatedSinceAsync(DateTime since)
        {
            return await _context.Users
                .Where(u => u.AccountType != AccountType.Admin && u.AccountType != AccountType.PlatformEmployee)
                .Where(u => u.CreatedAt >= since)
                .CountAsync();
        }
    }
}
