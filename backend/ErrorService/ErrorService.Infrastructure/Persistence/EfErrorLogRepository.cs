using ErrorService.Domain.Entities;
using ErrorService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErrorService.Infrastructure.Persistence
{
    public class EfErrorLogRepository : IErrorLogRepository
    {
        private readonly ApplicationDbContext _context;

        public EfErrorLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorLog?> GetByIdAsync(Guid id)
        {
            return await _context.ErrorLogs.FindAsync(id);
        }

        public async Task<IEnumerable<ErrorLog>> GetAsync(ErrorQuery query)
        {
            var dbQuery = _context.ErrorLogs.AsNoTracking().AsQueryable();

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

        public async Task AddAsync(ErrorLog errorLog)
        {
            _context.ErrorLogs.Add(errorLog);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var errorLog = await _context.ErrorLogs.FindAsync(id);
            if (errorLog != null)
            {
                _context.ErrorLogs.Remove(errorLog);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<string>> GetServiceNamesAsync()
        {
            return await _context.ErrorLogs
                .AsNoTracking()
                .Select(e => e.ServiceName)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();
        }

        public async Task<ErrorStats> GetStatsAsync(DateTime? from = null, DateTime? to = null)
        {
            var baseQuery = _context.ErrorLogs.AsNoTracking().AsQueryable();

            if (from.HasValue)
            {
                baseQuery = baseQuery.Where(e => e.OccurredAt >= from.Value);
            }

            if (to.HasValue)
            {
                baseQuery = baseQuery.Where(e => e.OccurredAt <= to.Value);
            }

            // Performance: Consolidate 5 sequential queries into 2
            var now = DateTime.UtcNow;
            var stats = await baseQuery
                .GroupBy(e => 1)
                .Select(g => new
                {
                    TotalErrors = g.Count(),
                    ErrorsLast24Hours = g.Count(e => e.OccurredAt >= now.AddHours(-24)),
                    ErrorsLast7Days = g.Count(e => e.OccurredAt >= now.AddDays(-7))
                })
                .FirstOrDefaultAsync();

            var errorsByService = await baseQuery
                .GroupBy(e => e.ServiceName)
                .Select(g => new { ServiceName = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ServiceName, x => x.Count);

            var errorsByStatusCode = await baseQuery
                .Where(e => e.StatusCode.HasValue)
                .GroupBy(e => e.StatusCode!.Value)
                .Select(g => new { StatusCode = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StatusCode, x => x.Count);

            return new ErrorStats
            {
                TotalErrors = stats?.TotalErrors ?? 0,
                ErrorsLast24Hours = stats?.ErrorsLast24Hours ?? 0,
                ErrorsLast7Days = stats?.ErrorsLast7Days ?? 0,
                ErrorsByService = errorsByService,
                ErrorsByStatusCode = errorsByStatusCode
            };
        }
    }
}