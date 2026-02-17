using MediatR;
using StaffService.Application.DTOs;
using StaffService.Application.Features.Staff.Queries;
using StaffService.Domain.Entities;
using StaffService.Domain.Interfaces;

namespace StaffService.Application.Features.Staff.Handlers;

public class GetStaffByIdQueryHandler : IRequestHandler<GetStaffByIdQuery, StaffDto?>
{
    private readonly IStaffRepository _repository;

    public GetStaffByIdQueryHandler(IStaffRepository repository)
    {
        _repository = repository;
    }

    public async Task<StaffDto?> Handle(GetStaffByIdQuery request, CancellationToken ct)
    {
        var staff = await _repository.GetByIdAsync(request.Id, ct);
        if (staff == null) return null;
        
        return MapToDto(staff);
    }

    private static StaffDto MapToDto(Domain.Entities.Staff staff) => new(
        staff.Id,
        staff.AuthUserId,
        staff.Email,
        staff.FirstName,
        staff.LastName,
        staff.FullName,
        staff.PhoneNumber,
        staff.AvatarUrl,
        staff.EmployeeCode,
        staff.DepartmentId,
        staff.Department?.Name,
        staff.PositionId,
        staff.Position?.Title,
        staff.SupervisorId,
        staff.Supervisor != null ? $"{staff.Supervisor.FirstName} {staff.Supervisor.LastName}" : null,
        staff.Status,
        staff.Role,
        staff.HireDate,
        staff.LastLoginAt,
        staff.TwoFactorEnabled,
        staff.CreatedAt
    );
}

public class GetStaffByAuthUserIdQueryHandler : IRequestHandler<GetStaffByAuthUserIdQuery, StaffDto?>
{
    private readonly IStaffRepository _repository;

    public GetStaffByAuthUserIdQueryHandler(IStaffRepository repository)
    {
        _repository = repository;
    }

    public async Task<StaffDto?> Handle(GetStaffByAuthUserIdQuery request, CancellationToken ct)
    {
        var staff = await _repository.GetByAuthUserIdAsync(request.AuthUserId, ct);
        if (staff == null) return null;
        
        return MapToDto(staff);
    }

    private static StaffDto MapToDto(Domain.Entities.Staff staff) => new(
        staff.Id,
        staff.AuthUserId,
        staff.Email,
        staff.FirstName,
        staff.LastName,
        staff.FullName,
        staff.PhoneNumber,
        staff.AvatarUrl,
        staff.EmployeeCode,
        staff.DepartmentId,
        staff.Department?.Name,
        staff.PositionId,
        staff.Position?.Title,
        staff.SupervisorId,
        staff.Supervisor != null ? $"{staff.Supervisor.FirstName} {staff.Supervisor.LastName}" : null,
        staff.Status,
        staff.Role,
        staff.HireDate,
        staff.LastLoginAt,
        staff.TwoFactorEnabled,
        staff.CreatedAt
    );
}

public class GetStaffByEmailQueryHandler : IRequestHandler<GetStaffByEmailQuery, StaffDto?>
{
    private readonly IStaffRepository _repository;

    public GetStaffByEmailQueryHandler(IStaffRepository repository)
    {
        _repository = repository;
    }

    public async Task<StaffDto?> Handle(GetStaffByEmailQuery request, CancellationToken ct)
    {
        var staff = await _repository.GetByEmailAsync(request.Email, ct);
        if (staff == null) return null;
        
        return new StaffDto(
            staff.Id,
            staff.AuthUserId,
            staff.Email,
            staff.FirstName,
            staff.LastName,
            staff.FullName,
            staff.PhoneNumber,
            staff.AvatarUrl,
            staff.EmployeeCode,
            staff.DepartmentId,
            staff.Department?.Name,
            staff.PositionId,
            staff.Position?.Title,
            staff.SupervisorId,
            staff.Supervisor != null ? $"{staff.Supervisor.FirstName} {staff.Supervisor.LastName}" : null,
            staff.Status,
            staff.Role,
            staff.HireDate,
            staff.LastLoginAt,
            staff.TwoFactorEnabled,
            staff.CreatedAt
        );
    }
}

public class SearchStaffQueryHandler : IRequestHandler<SearchStaffQuery, PaginatedResponse<StaffListItemDto>>
{
    private readonly IStaffRepository _repository;

    public SearchStaffQueryHandler(IStaffRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResponse<StaffListItemDto>> Handle(SearchStaffQuery request, CancellationToken ct)
    {
        var staff = await _repository.SearchAsync(
            request.SearchTerm,
            request.Status,
            request.Role,
            request.DepartmentId,
            request.Page,
            request.PageSize,
            ct
        );

        var totalCount = await _repository.CountAsync(
            request.SearchTerm,
            request.Status,
            request.Role,
            request.DepartmentId,
            ct
        );

        var items = staff.Select(s => new StaffListItemDto(
            s.Id,
            s.Email,
            s.FullName,
            s.AvatarUrl,
            s.Department?.Name,
            s.Position?.Title,
            s.Status,
            s.Role,
            s.LastLoginAt
        ));

        return new PaginatedResponse<StaffListItemDto>(
            items,
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(totalCount / (double)request.PageSize)
        );
    }
}

public class GetStaffSummaryQueryHandler : IRequestHandler<GetStaffSummaryQuery, StaffSummaryDto>
{
    private readonly IStaffRepository _repository;

    public GetStaffSummaryQueryHandler(IStaffRepository repository)
    {
        _repository = repository;
    }

    public async Task<StaffSummaryDto> Handle(GetStaffSummaryQuery request, CancellationToken ct)
    {
        var allStaff = await _repository.GetAllAsync(ct);
        var staffList = allStaff.ToList();

        var byRole = staffList
            .GroupBy(s => s.Role.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var byDepartment = staffList
            .Where(s => s.Department != null)
            .GroupBy(s => s.Department!.Name)
            .ToDictionary(g => g.Key, g => g.Count());

        return new StaffSummaryDto(
            staffList.Count,
            staffList.Count(s => s.Status == StaffStatus.Active),
            staffList.Count(s => s.Status == StaffStatus.Pending),
            staffList.Count(s => s.Status == StaffStatus.Suspended),
            staffList.Count(s => s.Status == StaffStatus.OnLeave),
            staffList.Count(s => s.Status == StaffStatus.Terminated),
            byRole,
            byDepartment
        );
    }
}

public class GetDirectReportsQueryHandler : IRequestHandler<GetDirectReportsQuery, IEnumerable<StaffListItemDto>>
{
    private readonly IStaffRepository _repository;

    public GetDirectReportsQueryHandler(IStaffRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<StaffListItemDto>> Handle(GetDirectReportsQuery request, CancellationToken ct)
    {
        var staff = await _repository.GetDirectReportsAsync(request.SupervisorId, ct);
        
        return staff.Select(s => new StaffListItemDto(
            s.Id,
            s.Email,
            s.FullName,
            s.AvatarUrl,
            s.Department?.Name,
            s.Position?.Title,
            s.Status,
            s.Role,
            s.LastLoginAt
        ));
    }
}

public class GetOrganizationChartQueryHandler : IRequestHandler<GetOrganizationChartQuery, object>
{
    private readonly IStaffRepository _repository;

    public GetOrganizationChartQueryHandler(IStaffRepository repository)
    {
        _repository = repository;
    }

    public async Task<object> Handle(GetOrganizationChartQuery request, CancellationToken ct)
    {
        var allStaff = await _repository.GetAllAsync(ct);
        var staffList = allStaff.ToList();

        // Build org chart recursively
        var roots = request.RootStaffId.HasValue
            ? staffList.Where(s => s.Id == request.RootStaffId.Value)
            : staffList.Where(s => s.SupervisorId == null);

        return roots.Select(s => BuildOrgNode(s, staffList)).ToList();
    }

    private static object BuildOrgNode(Domain.Entities.Staff staff, List<Domain.Entities.Staff> allStaff)
    {
        var directReports = allStaff.Where(s => s.SupervisorId == staff.Id).ToList();
        
        return new
        {
            id = staff.Id,
            name = staff.FullName,
            email = staff.Email,
            role = staff.Role.ToString(),
            department = staff.Department?.Name,
            position = staff.Position?.Title,
            avatar = staff.AvatarUrl,
            children = directReports.Select(r => BuildOrgNode(r, allStaff)).ToList()
        };
    }
}
