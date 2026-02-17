using Microsoft.EntityFrameworkCore;
using PostgresDbService.Domain.Entities;
using CarDealer.Shared.MultiTenancy;
using System.Text.Json;

namespace PostgresDbService.Infrastructure.Persistence;

/// <summary>
/// Centralized database context for all microservices
/// Uses JSONB for flexible schema support
/// </summary>
public class CentralizedDbContext : MultiTenantDbContext
{
    public CentralizedDbContext(DbContextOptions<CentralizedDbContext> options, ITenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    public DbSet<GenericEntity> GenericEntities => Set<GenericEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GenericEntity>(entity =>
        {
            entity.ToTable("generic_entities");
            
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Service and entity identification
            entity.Property(e => e.ServiceName)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.Property(e => e.EntityId)
                .IsRequired()
                .HasMaxLength(50);
            
            // JSONB data storage  
            entity.Property(e => e.DataJson)
                .IsRequired()
                .HasColumnType("jsonb")
                .HasDefaultValue("{}");
                
            entity.Property(e => e.IndexData)
                .HasColumnType("jsonb");
            
            // Metadata
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");
                
            entity.Property(e => e.Version)
                .IsRequired()
                .HasDefaultValue(1);
            
            // Audit fields
            entity.Property(e => e.CreatedAt)
                .IsRequired();
                
            entity.Property(e => e.UpdatedAt);
            
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100);
                
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100);
            
            // Soft delete
            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
                
            entity.Property(e => e.DeletedAt);
            
            entity.Property(e => e.DeletedBy)
                .HasMaxLength(100);
            
            // Indexes for performance
            entity.HasIndex(e => new { e.ServiceName, e.EntityType, e.EntityId })
                .IsUnique()
                .HasDatabaseName("IX_generic_entities_unique_entity");
                
            entity.HasIndex(e => new { e.ServiceName, e.EntityType })
                .HasDatabaseName("IX_generic_entities_service_type");
                
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_generic_entities_created_at");
                
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_generic_entities_status");
                
            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_generic_entities_is_deleted");
            
            // GIN index for JSONB data (PostgreSQL specific)
            entity.HasIndex(e => e.DataJson)
                .HasDatabaseName("IX_generic_entities_data_json")
                .HasMethod("gin");
                
            entity.HasIndex(e => e.IndexData)
                .HasDatabaseName("IX_generic_entities_index_data") 
                .HasMethod("gin");

            // Composite indexes for common queries
            entity.HasIndex(e => new { e.ServiceName, e.EntityType, e.Status, e.IsDeleted })
                .HasDatabaseName("IX_generic_entities_service_type_status");
                
            entity.HasIndex(e => new { e.ServiceName, e.EntityType, e.CreatedAt })
                .HasDatabaseName("IX_generic_entities_service_type_created");
        });
        
        // Configure query filters for soft delete
        modelBuilder.Entity<GenericEntity>()
            .HasQueryFilter(e => !e.IsDeleted);
    }
}