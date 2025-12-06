using AppointmentService.Domain.Entities;

namespace AppointmentService.Domain.Interfaces;

public interface ITimeSlotRepository
{
    Task<TimeSlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeSlot>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeSlot>> GetByDayOfWeekAsync(DayOfWeek dayOfWeek, CancellationToken cancellationToken = default);
    Task<IEnumerable<TimeSlot>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<TimeSlot> AddAsync(TimeSlot timeSlot, CancellationToken cancellationToken = default);
    Task UpdateAsync(TimeSlot timeSlot, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
