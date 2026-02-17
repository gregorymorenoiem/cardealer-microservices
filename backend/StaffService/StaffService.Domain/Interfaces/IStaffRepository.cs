using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StaffService.Domain.Entities;

namespace StaffService.Domain.Interfaces;

public interface IStaffRepository
{
    Task<Staff?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Staff?> GetByAuthUserIdAsync(Guid authUserId, CancellationToken ct = default);
    Task<Staff?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<Staff>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Staff>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default);
    Task<IEnumerable<Staff>> GetDirectReportsAsync(Guid supervisorId, CancellationToken ct = default);
    Task<IEnumerable<Staff>> SearchAsync(string? searchTerm, StaffStatus? status, StaffRole? role, Guid? departmentId, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountAsync(string? searchTerm, StaffStatus? status, StaffRole? role, Guid? departmentId, CancellationToken ct = default);
    Task<Staff> AddAsync(Staff staff, CancellationToken ct = default);
    Task UpdateAsync(Staff staff, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
