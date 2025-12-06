using AppointmentService.Domain.Entities;

namespace AppointmentService.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByTypeAsync(AppointmentType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByAssignedUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetUpcomingAsync(int days, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetPendingRemindersAsync(CancellationToken cancellationToken = default);
    Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasConflictAsync(DateTime date, TimeOnly startTime, TimeOnly endTime, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
