using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoleService.Domain.Entities;

namespace RoleService.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Resource)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Action)
            .IsRequired()
            .HasConversion<int>(); // Almacenar enum como int

        builder.Property(p => p.Module)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.IsSystemPermission)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Index compuesto en Resource + Action (combinación debe ser única por contexto)
        builder.HasIndex(p => new { p.Resource, p.Action, p.Module })
            .IsUnique();

        // Index en Module para queries por módulo
        builder.HasIndex(p => p.Module);

        // Index en Resource para queries por recurso
        builder.HasIndex(p => p.Resource);
    }
}
