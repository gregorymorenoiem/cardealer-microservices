using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;

namespace UserService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for modules catalog
/// </summary>
public class ModuleRepository : IModuleRepository
{
    private readonly ApplicationDbContext _context;

    public ModuleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Module>> GetAllActiveAsync()
    {
        return await _context.Modules
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<Module?> GetByIdAsync(Guid moduleId)
    {
        return await _context.Modules
            .FirstOrDefaultAsync(m => m.Id == moduleId);
    }

    public async Task<Module?> GetByNameAsync(string name)
    {
        return await _context.Modules
            .FirstOrDefaultAsync(m => m.Name == name);
    }
}
