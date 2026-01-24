using UserService.Domain.Entities;

namespace UserService.Domain.Interfaces;

/// <summary>
/// Repository interface for modules (catalog of available modules)
/// </summary>
public interface IModuleRepository
{
    Task<List<Module>> GetAllActiveAsync();
    Task<Module?> GetByIdAsync(Guid moduleId);
    Task<Module?> GetByNameAsync(string name);
}
