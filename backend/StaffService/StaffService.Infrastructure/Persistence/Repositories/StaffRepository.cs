using Microsoft.EntityFrameworkCore;
using StaffService.Domain.Entities;
using StaffService.Domain.Interfaces;

namespace StaffService.Infrastructure.Persistence.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly StaffDbContext _context;

    public StaffRepository(StaffDbContext context)
    {
        _context = context;
    }

    public async Task<Staff?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Staff
            .Include(s => s.Department)
            .Include(s => s.Position)
            .Include(s => s.Supervisor)
            .Include(s => s.Permissions)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<Staff?> GetByAuthUserIdAsync(Guid authUserId, CancellationToken ct = default)
    {
        return await _context.Staff
            .Include(s => s.Department)
            .Include(s => s.Position)
            .Include(s => s.Supervisor)
            .FirstOrDefaultAsync(s => s.AuthUserId == authUserId, ct);
    }

    public async Task<Staff?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Staff
            .Include(s => s.Department)
            .Include(s => s.Position)
            .Include(s => s.Supervisor)
            .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), ct);
    }

    public async Task<IEnumerable<Staff>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Staff
            .Include(s => s.Department)
            .Include(s => s.Position)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Staff>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default)
    {
        return await _context.Staff
            .Include(s => s.Position)
            .Where(s => s.DepartmentId == departmentId)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Staff>> GetDirectReportsAsync(Guid supervisorId, CancellationToken ct = default)
    {
        return await _context.Staff
            .Include(s => s.Department)
            .Include(s => s.Position)
            .Where(s => s.SupervisorId == supervisorId)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Staff>> SearchAsync(
        string? searchTerm,
        StaffStatus? status,
        StaffRole? role,
        Guid? departmentId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.Staff
            .Include(s => s.Department)
            .Include(s => s.Position)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s =>
                s.Email.ToLower().Contains(term) ||
                s.FirstName.ToLower().Contains(term) ||
                s.LastName.ToLower().Contains(term) ||
                (s.EmployeeCode != null && s.EmployeeCode.ToLower().Contains(term)));
        }

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        if (role.HasValue)
            query = query.Where(s => s.Role == role.Value);

        if (departmentId.HasValue)
            query = query.Where(s => s.DepartmentId == departmentId.Value);

        return await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(
        string? searchTerm,
        StaffStatus? status,
        StaffRole? role,
        Guid? departmentId,
        CancellationToken ct = default)
    {
        var query = _context.Staff.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s =>
                s.Email.ToLower().Contains(term) ||
                s.FirstName.ToLower().Contains(term) ||
                s.LastName.ToLower().Contains(term) ||
                (s.EmployeeCode != null && s.EmployeeCode.ToLower().Contains(term)));
        }

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        if (role.HasValue)
            query = query.Where(s => s.Role == role.Value);

        if (departmentId.HasValue)
            query = query.Where(s => s.DepartmentId == departmentId.Value);

        return await query.CountAsync(ct);
    }

    public async Task<Staff> AddAsync(Staff staff, CancellationToken ct = default)
    {
        _context.Staff.Add(staff);
        await _context.SaveChangesAsync(ct);
        return staff;
    }

    public async Task UpdateAsync(Staff staff, CancellationToken ct = default)
    {
        staff.UpdatedAt = DateTime.UtcNow;
        _context.Staff.Update(staff);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var staff = await _context.Staff.FindAsync(new object[] { id }, ct);
        if (staff != null)
        {
            _context.Staff.Remove(staff);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Staff.AnyAsync(s => s.Id == id, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return await _context.Staff.AnyAsync(s => s.Email.ToLower() == email.ToLower(), ct);
    }
}
