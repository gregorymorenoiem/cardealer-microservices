using Microsoft.EntityFrameworkCore;
using AppointmentService.Domain.Entities;
using AppointmentService.Domain.Interfaces;
using AppointmentService.Infrastructure.Persistence;

namespace AppointmentService.Infrastructure.Repositories;

public class TimeSlotRepository : ITimeSlotRepository
{
    private readonly AppointmentDbContext _context;

    public TimeSlotRepository(AppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<TimeSlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TimeSlots.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<TimeSlot>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TimeSlots
            .OrderBy(t => t.DayOfWeek)
            .ThenBy(t => t.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TimeSlot>> GetByDayOfWeekAsync(DayOfWeek dayOfWeek, CancellationToken cancellationToken = default)
    {
        return await _context.TimeSlots
            .Where(t => t.DayOfWeek == dayOfWeek)
            .OrderBy(t => t.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TimeSlot>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TimeSlots
            .Where(t => t.IsActive)
            .OrderBy(t => t.DayOfWeek)
            .ThenBy(t => t.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<TimeSlot> AddAsync(TimeSlot timeSlot, CancellationToken cancellationToken = default)
    {
        _context.TimeSlots.Add(timeSlot);
        await _context.SaveChangesAsync(cancellationToken);
        return timeSlot;
    }

    public async Task UpdateAsync(TimeSlot timeSlot, CancellationToken cancellationToken = default)
    {
        _context.TimeSlots.Update(timeSlot);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var timeSlot = await GetByIdAsync(id, cancellationToken);
        if (timeSlot != null)
        {
            _context.TimeSlots.Remove(timeSlot);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TimeSlots.AnyAsync(t => t.Id == id, cancellationToken);
    }
}
