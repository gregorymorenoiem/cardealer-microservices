using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoleService.Domain.Entities;

namespace RoleService.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.Priority)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .IsRequired();

        builder.Property(r => r.IsSystemRole)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.UpdatedBy)
            .HasMaxLength(100);

        // Index único en Name
        builder.HasIndex(r => r.Name)
            .IsUnique();

        // Index en IsActive para queries comunes
        builder.HasIndex(r => r.IsActive);

        // Index en Priority para ordenamiento
        builder.HasIndex(r => r.Priority);
    }
}
