using Microsoft.EntityFrameworkCore;
using PostgresDbService.Domain.Entities;
using PostgresDbService.Domain.Interfaces;
using PostgresDbService.Infrastructure.Persistence;
using System.Text.Json;

namespace PostgresDbService.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation for all microservice data
/// Provides CRUD operations with JSONB support
/// </summary>
public class GenericRepository : IGenericRepository
{
    private readonly CentralizedDbContext _context;

    public GenericRepository(CentralizedDbContext context)
    {
        _context = context;
    }

    public async Task<GenericEntity?> GetByIdAsync(string serviceName, string entityType, string entityId)
    {
        return await _context.GenericEntities
            .FirstOrDefaultAsync(e => e.ServiceName == serviceName && 
                                     e.EntityType == entityType && 
                                     e.EntityId == entityId &&
                                     !e.IsDeleted);
    }

    public async Task<List<GenericEntity>> GetByServiceAsync(string serviceName, string entityType, int page = 1, int pageSize = 50)
    {
        return await _context.GenericEntities
            .Where(e => e.ServiceName == serviceName && e.EntityType == entityType)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<GenericEntity> CreateAsync(GenericEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.Version = 1;
        
        _context.GenericEntities.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<GenericEntity> UpdateAsync(GenericEntity entity)
    {
        var existing = await GetByIdAsync(entity.ServiceName, entity.EntityType, entity.EntityId);
        if (existing == null)
            throw new InvalidOperationException($"Entity not found: {entity.ServiceName}.{entity.EntityType}.{entity.EntityId}");

        // Optimistic concurrency check
        if (existing.Version != entity.Version)
            throw new InvalidOperationException("Concurrency conflict: Entity was modified by another process");

        existing.DataJson = entity.DataJson;
        existing.IndexData = entity.IndexData;
        existing.Status = entity.Status;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.UpdatedBy = entity.UpdatedBy;
        existing.Version++;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(string serviceName, string entityType, string entityId, string deletedBy = "System")
    {
        var entity = await GetByIdAsync(serviceName, entityType, entityId);
        if (entity == null) return false;

        // Soft delete
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = deletedBy;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = deletedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(string serviceName, string entityType, string entityId)
    {
        return await _context.GenericEntities
            .AnyAsync(e => e.ServiceName == serviceName && 
                          e.EntityType == entityType && 
                          e.EntityId == entityId &&
                          !e.IsDeleted);
    }

    public async Task<List<GenericEntity>> QueryAsync(string serviceName, string entityType, string jsonPath, object value)
    {
        var valueJson = JsonSerializer.Serialize(value);
        
        return await _context.GenericEntities
            .Where(e => e.ServiceName == serviceName && e.EntityType == entityType)
            .Where(e => EF.Functions.JsonExtractPath(e.DataJson, jsonPath) == valueJson)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<GenericEntity>> QueryByJsonAsync(string serviceName, string entityType, string jsonQuery)
    {
        // Raw SQL for complex JSONB queries
        var sql = $@"
            SELECT * FROM generic_entities 
            WHERE service_name = {{0}} 
            AND entity_type = {{1}} 
            AND is_deleted = false
            AND data_json @@ {{2}}::jsonpath
            ORDER BY created_at DESC";

        return await _context.GenericEntities
            .FromSqlRaw(sql, serviceName, entityType, jsonQuery)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string serviceName, string entityType)
    {
        return await _context.GenericEntities
            .CountAsync(e => e.ServiceName == serviceName && e.EntityType == entityType);
    }

    public async Task<List<GenericEntity>> GetAllByIdsAsync(string serviceName, string entityType, List<string> entityIds)
    {
        return await _context.GenericEntities
            .Where(e => e.ServiceName == serviceName && 
                       e.EntityType == entityType && 
                       entityIds.Contains(e.EntityId))
            .ToListAsync();
    }

    public async Task<List<GenericEntity>> BulkCreateAsync(List<GenericEntity> entities)
    {
        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.CreatedAt = now;
            entity.Version = 1;
        }

        _context.GenericEntities.AddRange(entities);
        await _context.SaveChangesAsync();
        return entities;
    }

    public async Task<List<GenericEntity>> BulkUpdateAsync(List<GenericEntity> entities)
    {
        var now = DateTime.UtcNow;
        var updatedEntities = new List<GenericEntity>();

        foreach (var entity in entities)
        {
            var existing = await GetByIdAsync(entity.ServiceName, entity.EntityType, entity.EntityId);
            if (existing != null)
            {
                existing.DataJson = entity.DataJson;
                existing.IndexData = entity.IndexData;
                existing.Status = entity.Status;
                existing.UpdatedAt = now;
                existing.UpdatedBy = entity.UpdatedBy;
                existing.Version++;
                updatedEntities.Add(existing);
            }
        }

        await _context.SaveChangesAsync();
        return updatedEntities;
    }

    public async Task<bool> BulkDeleteAsync(string serviceName, string entityType, List<string> entityIds, string deletedBy = "System")
    {
        var entities = await GetAllByIdsAsync(serviceName, entityType, entityIds);
        var now = DateTime.UtcNow;

        foreach (var entity in entities)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = now;
            entity.DeletedBy = deletedBy;
            entity.UpdatedAt = now;
            entity.UpdatedBy = deletedBy;
        }

        await _context.SaveChangesAsync();
        return entities.Count > 0;
    }

    public async Task<List<GenericEntity>> SearchAsync(string serviceName, string entityType, string searchTerm, List<string> searchFields)
    {
        if (string.IsNullOrEmpty(searchTerm) || !searchFields.Any())
            return await GetByServiceAsync(serviceName, entityType);

        var query = _context.GenericEntities
            .Where(e => e.ServiceName == serviceName && e.EntityType == entityType);

        // Build OR conditions for each search field
        var searchConditions = searchFields
            .Select(field => $"data_json->>'{field}' ILIKE '%{searchTerm}%'")
            .ToList();

        var whereClause = string.Join(" OR ", searchConditions);
        
        return await query
            .Where(e => EF.Functions.Like(e.DataJson, $"%{searchTerm}%"))
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<GenericEntity>> GetByDateRangeAsync(string serviceName, string entityType, DateTime fromDate, DateTime toDate)
    {
        return await _context.GenericEntities
            .Where(e => e.ServiceName == serviceName && 
                       e.EntityType == entityType &&
                       e.CreatedAt >= fromDate && 
                       e.CreatedAt <= toDate)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<GenericEntity>> GetRecentAsync(string serviceName, string entityType, int count = 10)
    {
        return await _context.GenericEntities
            .Where(e => e.ServiceName == serviceName && e.EntityType == entityType)
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}