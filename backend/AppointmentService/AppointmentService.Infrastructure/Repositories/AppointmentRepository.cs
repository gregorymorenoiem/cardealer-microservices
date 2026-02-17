using Microsoft.EntityFrameworkCore;
using AppointmentService.Domain.Entities;
using AppointmentService.Domain.Interfaces;
using AppointmentService.Infrastructure.Persistence;

namespace AppointmentService.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppointmentDbContext _context;

    public AppointmentRepository(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .OrderByDescending(a => a.ScheduledDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByDealerIdAsync(Guid dealerId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.DealerId == dealerId)
            .OrderByDescending(a => a.ScheduledDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.ScheduledDate >= startDate.Date && a.ScheduledDate <= endDate.Date)
            .OrderBy(a => a.ScheduledDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByTypeAsync(AppointmentType type, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.Type == type)
            .OrderByDescending(a => a.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByAssignedUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.AssignedToUserId == userId)
            .OrderByDescending(a => a.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAsync(int days, CancellationToken cancellationToken = default)
    {
        var endDate = DateTime.UtcNow.Date.AddDays(days);
        return await _context.Appointments
            .Where(a => a.ScheduledDate >= DateTime.UtcNow.Date && a.ScheduledDate <= endDate)
            .Where(a => a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed)
            .OrderBy(a => a.ScheduledDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetPendingRemindersAsync(CancellationToken cancellationToken = default)
    {
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        return await _context.Appointments
            .Where(a => a.ScheduledDate == tomorrow)
            .Where(a => a.ReminderSentAt == null)
            .Where(a => a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed)
            .ToListAsync(cancellationToken);
    }

    public async Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);
        return appointment;
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await GetByIdAsync(id, cancellationToken);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments.AnyAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> HasConflictAsync(DateTime date, TimeOnly startTime, TimeOnly endTime, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .Where(a => a.ScheduledDate == date.Date)
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .Where(a => a.StartTime < endTime && a.EndTime > startTime);

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
