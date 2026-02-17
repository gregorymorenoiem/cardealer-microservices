using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdminService.Infrastructure.Persistence;

/// <summary>
/// Implementation of IPlatformEmployeeRepository
/// Returns empty data - will be connected to database later
/// </summary>
public class EfPlatformEmployeeRepository : IPlatformEmployeeRepository
{
    private readonly ILogger<EfPlatformEmployeeRepository> _logger;

    public EfPlatformEmployeeRepository(ILogger<EfPlatformEmployeeRepository> logger)
    {
        _logger = logger;
    }

    public async Task<PlatformEmployee?> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting employee by ID: {Id}", id);
        await Task.CompletedTask;
        return null;
    }

    public async Task<PlatformEmployee?> GetByEmailAsync(string email)
    {
        _logger.LogDebug("Getting employee by email: {Email}", email);
        await Task.CompletedTask;
        return null;
    }

    public async Task<PlatformEmployee?> GetByUserIdAsync(Guid userId)
    {
        _logger.LogDebug("Getting employee by user ID: {UserId}", userId);
        await Task.CompletedTask;
        return null;
    }

    public async Task<(IEnumerable<PlatformEmployee> Employees, int TotalCount)> GetAllAsync(
        string? status = null,
        string? role = null,
        string? department = null,
        int page = 1,
        int pageSize = 20)
    {
        _logger.LogDebug("Getting all employees");
        await Task.CompletedTask;
        return (new List<PlatformEmployee>(), 0);
    }

    public async Task<PlatformEmployee> AddAsync(PlatformEmployee employee)
    {
        _logger.LogDebug("Adding employee: {Id}", employee.Id);
        await Task.CompletedTask;
        return employee;
    }

    public async Task UpdateAsync(PlatformEmployee employee)
    {
        _logger.LogDebug("Updating employee: {Id}", employee.Id);
        await Task.CompletedTask;
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        _logger.LogDebug("Soft deleting employee: {Id}", id);
        await Task.CompletedTask;
    }

    public async Task<PlatformEmployeeInvitation?> GetInvitationByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting invitation by ID: {Id}", id);
        await Task.CompletedTask;
        return null;
    }

    public async Task<PlatformEmployeeInvitation?> GetInvitationByTokenAsync(string token)
    {
        _logger.LogDebug("Getting invitation by token");
        await Task.CompletedTask;
        return null;
    }

    public async Task<PlatformEmployeeInvitation?> GetPendingInvitationByEmailAsync(string email)
    {
        _logger.LogDebug("Getting pending invitation by email: {Email}", email);
        await Task.CompletedTask;
        return null;
    }

    public async Task<IEnumerable<PlatformEmployeeInvitation>> GetInvitationsAsync(InvitationStatus? status = null)
    {
        _logger.LogDebug("Getting invitations");
        await Task.CompletedTask;
        return new List<PlatformEmployeeInvitation>();
    }

    public async Task<PlatformEmployeeInvitation> AddInvitationAsync(PlatformEmployeeInvitation invitation)
    {
        _logger.LogDebug("Adding invitation: {Email}", invitation.Email);
        await Task.CompletedTask;
        return invitation;
    }

    public async Task UpdateInvitationAsync(PlatformEmployeeInvitation invitation)
    {
        _logger.LogDebug("Updating invitation: {Id}", invitation.Id);
        await Task.CompletedTask;
    }

    public async Task<(IEnumerable<EmployeeActivity> Activities, int TotalCount)> GetActivityAsync(
        Guid employeeId,
        DateTime from,
        DateTime to,
        int page = 1,
        int pageSize = 50)
    {
        _logger.LogDebug("Getting activity for employee: {EmployeeId}", employeeId);
        await Task.CompletedTask;
        return (new List<EmployeeActivity>(), 0);
    }

    public async Task LogActivityAsync(EmployeeActivity activity)
    {
        _logger.LogDebug("Logging activity for employee: {EmployeeId}", activity.EmployeeId);
        await Task.CompletedTask;
    }
}
