using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RateLimitingService.Core.Interfaces;
using RateLimitingService.Core.Models;
using RateLimitingService.Infrastructure.Data;

namespace RateLimitingService.Infrastructure.Repositories;

/// <summary>
/// PostgreSQL repository for rate limit violations
/// </summary>
public class RateLimitViolationRepository : IRateLimitViolationRepository
{
    private readonly RateLimitDbContext _context;
    private readonly ILogger<RateLimitViolationRepository> _logger;

    public RateLimitViolationRepository(
        RateLimitDbContext context,
        ILogger<RateLimitViolationRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddViolationAsync(RateLimitViolation violation)
    {
        try
        {
            _context.Violations.Add(violation);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Violation recorded for {Identifier} on {Endpoint}",
                violation.Identifier, violation.Endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording violation for {Identifier}", violation.Identifier);
            throw;
        }
    }

    public async Task<IEnumerable<RateLimitViolation>> GetViolationsAsync(
        string identifier,
        RateLimitIdentifierType? type = null,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 100)
    {
        var query = _context.Violations.AsQueryable();

        query = query.Where(v => v.Identifier == identifier);

        if (type.HasValue)
        {
            query = query.Where(v => v.IdentifierType == type.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(v => v.ViolatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(v => v.ViolatedAt <= to.Value);
        }

        return await query
            .OrderByDescending(v => v.ViolatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetViolationCountAsync(
        string identifier,
        RateLimitIdentifierType? type = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        var query = _context.Violations.AsQueryable();

        query = query.Where(v => v.Identifier == identifier);

        if (type.HasValue)
        {
            query = query.Where(v => v.IdentifierType == type.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(v => v.ViolatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(v => v.ViolatedAt <= to.Value);
        }

        return await query.CountAsync();
    }

    public async Task<IEnumerable<(string Identifier, int Count)>> GetTopViolatorsAsync(
        RateLimitIdentifierType? type = null,
        DateTime? from = null,
        DateTime? to = null,
        int limit = 10)
    {
        var query = _context.Violations.AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(v => v.IdentifierType == type.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(v => v.ViolatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(v => v.ViolatedAt <= to.Value);
        }

        var result = await query
            .GroupBy(v => v.Identifier)
            .Select(g => new { Identifier = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(limit)
            .ToListAsync();

        return result.Select(r => (r.Identifier, r.Count));
    }

    public async Task<int> CleanOldViolationsAsync(DateTime olderThan)
    {
        try
        {
            var deleted = await _context.Violations
                .Where(v => v.ViolatedAt < olderThan)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Cleaned {Count} old violations before {Date}", deleted, olderThan);
            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning old violations");
            throw;
        }
    }
}
