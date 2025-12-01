using RoleService.Domain.Entities;
using RoleService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleService.Infrastructure.Persistence
{
    public class EfRoleLogRepository : IRoleLogRepository
    {
        private readonly ApplicationDbContext _context;

        public EfRoleLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RoleLog?> GetByIdAsync(Guid id)
        {
            return await _context.RoleLogs.FindAsync(id);
        }

        public async Task<IEnumerable<RoleLog>> GetAsync(RoleLogQuery query)
        {
            var dbQuery = _context.RoleLogs.AsNoTracking().AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(query.ServiceName))
            {
                dbQuery = dbQuery.Where(e => e.ServiceName == query.ServiceName);
            }

            if (query.From.HasValue)
            {
                dbQuery = dbQuery.Where(e => e.OccurredAt >= query.From.Value);
            }

            if (query.To.HasValue)
            {
                dbQuery = dbQuery.Where(e => e.OccurredAt <= query.To.Value);
            }

            // Ordenar y paginar
            dbQuery = dbQuery.OrderByDescending(e => e.OccurredAt)
                            .Skip((query.Page - 1) * query.PageSize)
                            .Take(query.PageSize);

            return await dbQuery.ToListAsync();
        }

        public async Task AddAsync(RoleLog roleLog)
        {
            _context.RoleLogs.Add(roleLog);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var roleLog = await _context.RoleLogs.FindAsync(id);
            if (roleLog != null)
            {
                _context.RoleLogs.Remove(roleLog);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<string>> GetServiceNamesAsync()
        {
            return await _context.RoleLogs
                .AsNoTracking()
                .Select(e => e.ServiceName)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();
        }

        public async Task<RoleLogStats> GetStatsAsync(DateTime? from = null, DateTime? to = null)
        {
            var baseQuery = _context.RoleLogs.AsNoTracking().AsQueryable();

            if (from.HasValue)
            {
                baseQuery = baseQuery.Where(e => e.OccurredAt >= from.Value);
            }

            if (to.HasValue)
            {
                baseQuery = baseQuery.Where(e => e.OccurredAt <= to.Value);
            }

            var totalErrorsTask = baseQuery.CountAsync();

            var errorsLast24HoursTask = baseQuery
                .Where(e => e.OccurredAt >= DateTime.UtcNow.AddHours(-24))
                .CountAsync();

            var errorsLast7DaysTask = baseQuery
                .Where(e => e.OccurredAt >= DateTime.UtcNow.AddDays(-7))
                .CountAsync();

            var errorsByServiceTask = baseQuery
                .GroupBy(e => e.ServiceName)
                .Select(g => new { ServiceName = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ServiceName, x => x.Count);

            var errorsByStatusCodeTask = baseQuery
                .Where(e => e.StatusCode.HasValue)
                .GroupBy(e => e.StatusCode!.Value)
                .Select(g => new { StatusCode = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StatusCode, x => x.Count);

            await Task.WhenAll(totalErrorsTask, errorsLast24HoursTask, errorsLast7DaysTask, errorsByServiceTask, errorsByStatusCodeTask);

            return new RoleLogStats
            {
                TotalErrors = totalErrorsTask.Result,
                ErrorsLast24Hours = errorsLast24HoursTask.Result,
                ErrorsLast7Days = errorsLast7DaysTask.Result,
                ErrorsByService = errorsByServiceTask.Result,
                ErrorsByStatusCode = errorsByStatusCodeTask.Result
            };
        }
    }
}
