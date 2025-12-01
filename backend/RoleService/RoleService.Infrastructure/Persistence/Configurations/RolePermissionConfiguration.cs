using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoleService.Domain.Entities;

namespace RoleService.Infrastructure.Persistence.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.RoleId)
            .IsRequired();

        builder.Property(rp => rp.PermissionId)
            .IsRequired();

        builder.Property(rp => rp.AssignedAt)
            .IsRequired();

        builder.Property(rp => rp.AssignedBy)
            .IsRequired()
            .HasMaxLength(100);

        // Relación con Role
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Permission
        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index único compuesto: Un role no puede tener el mismo permiso dos veces
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique();

        // Index en RoleId para queries de permisos por role
        builder.HasIndex(rp => rp.RoleId);

        // Index en PermissionId para queries de roles por permiso
        builder.HasIndex(rp => rp.PermissionId);
    }
}
