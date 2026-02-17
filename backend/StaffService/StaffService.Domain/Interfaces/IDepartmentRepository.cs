using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StaffService.Domain.Entities;

namespace StaffService.Domain.Interfaces;

public interface IDepartmentRepository
{
    Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Department?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IEnumerable<Department>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Department>> GetRootDepartmentsAsync(CancellationToken ct = default);
    Task<IEnumerable<Department>> GetChildDepartmentsAsync(Guid parentId, CancellationToken ct = default);
    Task<Department> AddAsync(Department department, CancellationToken ct = default);
    Task UpdateAsync(Department department, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasStaffAsync(Guid id, CancellationToken ct = default);
}

public interface IPositionRepository
{
    Task<Position?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Position?> GetByTitleAsync(string title, CancellationToken ct = default);
    Task<IEnumerable<Position>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Position>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default);
    Task<Position> AddAsync(Position position, CancellationToken ct = default);
    Task UpdateAsync(Position position, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> HasStaffAsync(Guid id, CancellationToken ct = default);
}
