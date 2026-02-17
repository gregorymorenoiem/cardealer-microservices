using Microsoft.EntityFrameworkCore;
using StaffService.Domain.Entities;
using StaffService.Domain.Interfaces;

namespace StaffService.Infrastructure.Persistence.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly StaffDbContext _context;

    public DepartmentRepository(StaffDbContext context)
    {
        _context = context;
    }

    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Departments
            .Include(d => d.ParentDepartment)
            .Include(d => d.Head)
            .Include(d => d.StaffMembers)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<Department?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.Name.ToLower() == name.ToLower(), ct);
    }

    public async Task<IEnumerable<Department>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Departments
            .Include(d => d.ParentDepartment)
            .Include(d => d.Head)
            .Include(d => d.StaffMembers)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Department>> GetRootDepartmentsAsync(CancellationToken ct = default)
    {
        return await _context.Departments
            .Include(d => d.ChildDepartments)
            .Include(d => d.StaffMembers)
            .Where(d => d.ParentDepartmentId == null)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Department>> GetChildDepartmentsAsync(Guid parentId, CancellationToken ct = default)
    {
        return await _context.Departments
            .Include(d => d.StaffMembers)
            .Where(d => d.ParentDepartmentId == parentId)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);
    }

    public async Task<Department> AddAsync(Department department, CancellationToken ct = default)
    {
        _context.Departments.Add(department);
        await _context.SaveChangesAsync(ct);
        return department;
    }

    public async Task UpdateAsync(Department department, CancellationToken ct = default)
    {
        department.UpdatedAt = DateTime.UtcNow;
        _context.Departments.Update(department);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var department = await _context.Departments.FindAsync(new object[] { id }, ct);
        if (department != null)
        {
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Departments.AnyAsync(d => d.Id == id, ct);
    }

    public async Task<bool> HasStaffAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Staff.AnyAsync(s => s.DepartmentId == id, ct);
    }
}

public class PositionRepository : IPositionRepository
{
    private readonly StaffDbContext _context;

    public PositionRepository(StaffDbContext context)
    {
        _context = context;
    }

    public async Task<Position?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Positions
            .Include(p => p.Department)
            .Include(p => p.StaffMembers)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Position?> GetByTitleAsync(string title, CancellationToken ct = default)
    {
        return await _context.Positions
            .FirstOrDefaultAsync(p => p.Title.ToLower() == title.ToLower(), ct);
    }

    public async Task<IEnumerable<Position>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Positions
            .Include(p => p.Department)
            .Include(p => p.StaffMembers)
            .OrderBy(p => p.Title)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Position>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default)
    {
        return await _context.Positions
            .Include(p => p.StaffMembers)
            .Where(p => p.DepartmentId == departmentId)
            .OrderBy(p => p.Title)
            .ToListAsync(ct);
    }

    public async Task<Position> AddAsync(Position position, CancellationToken ct = default)
    {
        _context.Positions.Add(position);
        await _context.SaveChangesAsync(ct);
        return position;
    }

    public async Task UpdateAsync(Position position, CancellationToken ct = default)
    {
        position.UpdatedAt = DateTime.UtcNow;
        _context.Positions.Update(position);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var position = await _context.Positions.FindAsync(new object[] { id }, ct);
        if (position != null)
        {
            _context.Positions.Remove(position);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Positions.AnyAsync(p => p.Id == id, ct);
    }

    public async Task<bool> HasStaffAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Staff.AnyAsync(s => s.PositionId == id, ct);
    }
}
