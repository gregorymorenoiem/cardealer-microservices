using Microsoft.EntityFrameworkCore;
using StaffService.Domain.Entities;

namespace StaffService.Infrastructure.Persistence;

public class StaffDbContext : DbContext
{
    public StaffDbContext(DbContextOptions<StaffDbContext> options) : base(options)
    {
    }

    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<StaffInvitation> StaffInvitations => Set<StaffInvitation>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<StaffPermission> StaffPermissions => Set<StaffPermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Staff entity configuration
        modelBuilder.Entity<Staff>(entity =>
        {
            entity.ToTable("staff");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.TerminationReason).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(256);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.AuthUserId).IsUnique();
            entity.HasIndex(e => e.EmployeeCode);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Role);

            // Self-referencing relationship for supervisor
            entity.HasOne(e => e.Supervisor)
                .WithMany(e => e.DirectReports)
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Department relationship
            entity.HasOne(e => e.Department)
                .WithMany(e => e.StaffMembers)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Position relationship
            entity.HasOne(e => e.Position)
                .WithMany(e => e.StaffMembers)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ignore computed properties
            entity.Ignore(e => e.FullName);
            entity.Ignore(e => e.IsActive);
        });

        // StaffInvitation entity configuration
        modelBuilder.Entity<StaffInvitation>(entity =>
        {
            entity.ToTable("staff_invitations");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Token).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(500);

            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ExpiresAt);

            // Relationship with invited by staff (optional - inviter may be a system admin not in staff table)
            entity.HasOne(e => e.InvitedByStaff)
                .WithMany()
                .HasForeignKey(e => e.InvitedBy)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Relationship with created staff
            entity.HasOne(e => e.Staff)
                .WithOne(s => s.Invitation)
                .HasForeignKey<StaffInvitation>(e => e.StaffId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relationship with supervisor (assigned on acceptance)
            entity.HasOne(e => e.Supervisor)
                .WithMany()
                .HasForeignKey(e => e.SupervisorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Department relationship
            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Position relationship
            entity.HasOne(e => e.Position)
                .WithMany()
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ignore computed properties
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsValid);
        });

        // Department entity configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("departments");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedBy).HasMaxLength(256);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Code);

            // Self-referencing relationship for parent department
            entity.HasOne(e => e.ParentDepartment)
                .WithMany(e => e.ChildDepartments)
                .HasForeignKey(e => e.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Department head relationship
            entity.HasOne(e => e.Head)
                .WithMany()
                .HasForeignKey(e => e.HeadId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Position entity configuration
        modelBuilder.Entity<Position>(entity =>
        {
            entity.ToTable("positions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CreatedBy).HasMaxLength(256);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);

            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.Code);

            // Department relationship
            entity.HasOne(e => e.Department)
                .WithMany(e => e.Positions)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // StaffPermission entity configuration
        modelBuilder.Entity<StaffPermission>(entity =>
        {
            entity.ToTable("staff_permissions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Permission).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(500);

            entity.HasIndex(e => new { e.StaffId, e.Permission }).IsUnique();

            // Staff relationship
            entity.HasOne(e => e.Staff)
                .WithMany(e => e.Permissions)
                .HasForeignKey(e => e.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore computed properties
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsActive);
        });

        // Seed default departments
        SeedDefaultData(modelBuilder);
    }

    private static void SeedDefaultData(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow;
        
        // Default departments
        var techDeptId = Guid.Parse("00000000-0000-0000-0001-000000000001");
        var opsDeptId = Guid.Parse("00000000-0000-0000-0001-000000000002");
        var supportDeptId = Guid.Parse("00000000-0000-0000-0001-000000000003");
        var complianceDeptId = Guid.Parse("00000000-0000-0000-0001-000000000004");

        modelBuilder.Entity<Department>().HasData(
            new Department
            {
                Id = techDeptId,
                Name = "Technology",
                Description = "Engineering and Development",
                Code = "TECH",
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "system"
            },
            new Department
            {
                Id = opsDeptId,
                Name = "Operations",
                Description = "Business Operations",
                Code = "OPS",
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "system"
            },
            new Department
            {
                Id = supportDeptId,
                Name = "Customer Support",
                Description = "Customer Service and Support",
                Code = "SUP",
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "system"
            },
            new Department
            {
                Id = complianceDeptId,
                Name = "Compliance",
                Description = "Legal and Regulatory Compliance",
                Code = "COMP",
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "system"
            }
        );

        // Default positions
        modelBuilder.Entity<Position>().HasData(
            new Position
            {
                Id = Guid.Parse("00000000-0000-0000-0002-000000000001"),
                Title = "Software Engineer",
                Description = "Develops and maintains software applications",
                Code = "SWE",
                DepartmentId = techDeptId,
                DefaultRole = StaffRole.Support,
                Level = 2,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "system"
            },
            new Position
            {
                Id = Guid.Parse("00000000-0000-0000-0002-000000000002"),
                Title = "Support Specialist",
                Description = "Provides customer support",
                Code = "SS",
                DepartmentId = supportDeptId,
                DefaultRole = StaffRole.Support,
                Level = 1,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "system"
            },
            new Position
            {
                Id = Guid.Parse("00000000-0000-0000-0002-000000000003"),
                Title = "Content Moderator",
                Description = "Moderates platform content",
                Code = "MOD",
                DepartmentId = opsDeptId,
                DefaultRole = StaffRole.Moderator,
                Level = 1,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "system"
            },
            new Position
            {
                Id = Guid.Parse("00000000-0000-0000-0002-000000000004"),
                Title = "Compliance Officer",
                Description = "Ensures regulatory compliance",
                Code = "CO",
                DepartmentId = complianceDeptId,
                DefaultRole = StaffRole.Compliance,
                Level = 3,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "system"
            },
            new Position
            {
                Id = Guid.Parse("00000000-0000-0000-0002-000000000005"),
                Title = "System Administrator",
                Description = "Manages system access and security",
                Code = "SYSADMIN",
                DepartmentId = techDeptId,
                DefaultRole = StaffRole.Admin,
                Level = 3,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = "system"
            }
        );
    }
}
