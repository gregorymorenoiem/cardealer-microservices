using Microsoft.EntityFrameworkCore;
using AppointmentService.Domain.Entities;

namespace AppointmentService.Infrastructure.Persistence;

public class AppointmentDbContext : DbContext
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();

    public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.ScheduledDate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.AssignedToUserId);

            entity.Property(e => e.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CustomerEmail).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CustomerPhone).HasMaxLength(50);
            entity.Property(e => e.VehicleDescription).HasMaxLength(500);
            entity.Property(e => e.AssignedToUserName).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId);
            entity.HasIndex(e => e.DayOfWeek);
            entity.HasIndex(e => e.IsActive);
        });
    }
}
