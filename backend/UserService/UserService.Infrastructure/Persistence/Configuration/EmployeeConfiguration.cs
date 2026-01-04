using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence.Configuration
{
    /// <summary>
    /// Configuración EF Core para entidades de empleados
    /// </summary>
    public class DealerEmployeeConfiguration : IEntityTypeConfiguration<DealerEmployee>
    {
        public void Configure(EntityTypeBuilder<DealerEmployee> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DealerRole)
                .IsRequired();

            builder.Property(e => e.Permissions)
                .IsRequired()
                .HasDefaultValue("[]")
                .HasMaxLength(2000); // JSON array de permissions

            builder.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(EmployeeStatus.Pending);

            builder.Property(e => e.InvitationDate)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(e => e.Notes)
                .HasMaxLength(500);

            // Relaciones
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.InvitedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.DealerId);
            builder.HasIndex(e => e.Status);
        }
    }

    public class PlatformEmployeeConfiguration : IEntityTypeConfiguration<PlatformEmployee>
    {
        public void Configure(EntityTypeBuilder<PlatformEmployee> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.PlatformRole)
                .IsRequired();

            builder.Property(e => e.Permissions)
                .IsRequired()
                .HasDefaultValue("[]")
                .HasMaxLength(2000);

            builder.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(EmployeeStatus.Active);

            builder.Property(e => e.HireDate)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(e => e.Department)
                .HasMaxLength(100);

            builder.Property(e => e.Notes)
                .HasMaxLength(500);

            // Relaciones
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => e.Status);
        }
    }

    public class DealerEmployeeInvitationConfiguration : IEntityTypeConfiguration<DealerEmployeeInvitation>
    {
        public void Configure(EntityTypeBuilder<DealerEmployeeInvitation> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.DealerRole)
                .IsRequired();

            builder.Property(e => e.Permissions)
                .IsRequired()
                .HasDefaultValue("[]")
                .HasMaxLength(2000);

            builder.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(InvitationStatus.Pending);

            builder.Property(e => e.InvitationDate)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(256);

            // Relaciones
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.InvitedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(e => e.Email);
            builder.HasIndex(e => e.Token).IsUnique();
            builder.HasIndex(e => e.Status);
        }
    }

    public class PlatformEmployeeInvitationConfiguration : IEntityTypeConfiguration<PlatformEmployeeInvitation>
    {
        public void Configure(EntityTypeBuilder<PlatformEmployeeInvitation> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(e => e.PlatformRole)
                .IsRequired();

            builder.Property(e => e.Permissions)
                .IsRequired()
                .HasDefaultValue("[]")
                .HasMaxLength(2000);

            builder.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(InvitationStatus.Pending);

            builder.Property(e => e.InvitationDate)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(256);

            // Relaciones
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.InvitedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(e => e.Email);
            builder.HasIndex(e => e.Token).IsUnique();
            builder.HasIndex(e => e.Status);
        }
    }
}
