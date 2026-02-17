using MediatR;
using Microsoft.Extensions.Logging;
using StaffService.Application.Clients;
using StaffService.Application.DTOs;
using StaffService.Application.Features.Departments.Commands;
using StaffService.Application.Features.Departments.Queries;
using StaffService.Application.Features.Staff.Commands;
using StaffService.Domain.Entities;
using StaffService.Domain.Interfaces;

namespace StaffService.Application.Features.Departments.Handlers;

// Query Handlers

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto?>
{
    private readonly IDepartmentRepository _repository;

    public GetDepartmentByIdQueryHandler(IDepartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<DepartmentDto?> Handle(GetDepartmentByIdQuery request, CancellationToken ct)
    {
        var dept = await _repository.GetByIdAsync(request.Id, ct);
        if (dept == null) return null;
        
        return new DepartmentDto(
            dept.Id,
            dept.Name,
            dept.Description,
            dept.Code,
            dept.ParentDepartmentId,
            dept.ParentDepartment?.Name,
            dept.HeadId,
            dept.Head != null ? $"{dept.Head.FirstName} {dept.Head.LastName}" : null,
            dept.StaffMembers?.Count ?? 0,
            dept.IsActive
        );
    }
}

public class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, IEnumerable<DepartmentDto>>
{
    private readonly IDepartmentRepository _repository;

    public GetAllDepartmentsQueryHandler(IDepartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<DepartmentDto>> Handle(GetAllDepartmentsQuery request, CancellationToken ct)
    {
        var departments = await _repository.GetAllAsync(ct);
        
        return departments.Select(dept => new DepartmentDto(
            dept.Id,
            dept.Name,
            dept.Description,
            dept.Code,
            dept.ParentDepartmentId,
            dept.ParentDepartment?.Name,
            dept.HeadId,
            dept.Head != null ? $"{dept.Head.FirstName} {dept.Head.LastName}" : null,
            dept.StaffMembers?.Count ?? 0,
            dept.IsActive
        ));
    }
}

public class GetDepartmentTreeQueryHandler : IRequestHandler<GetDepartmentTreeQuery, IEnumerable<DepartmentTreeDto>>
{
    private readonly IDepartmentRepository _repository;

    public GetDepartmentTreeQueryHandler(IDepartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<DepartmentTreeDto>> Handle(GetDepartmentTreeQuery request, CancellationToken ct)
    {
        var allDepartments = (await _repository.GetAllAsync(ct)).ToList();
        var roots = allDepartments.Where(d => d.ParentDepartmentId == null);
        
        return roots.Select(d => BuildTree(d, allDepartments));
    }

    private static DepartmentTreeDto BuildTree(Department dept, List<Department> allDepartments)
    {
        var children = allDepartments
            .Where(d => d.ParentDepartmentId == dept.Id)
            .Select(d => BuildTree(d, allDepartments))
            .ToList();
        
        return new DepartmentTreeDto(
            dept.Id,
            dept.Name,
            dept.Code,
            dept.StaffMembers?.Count ?? 0,
            children
        );
    }
}

// Command Handlers

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<DepartmentDto>>
{
    private readonly IDepartmentRepository _repository;
    private readonly IAuditServiceClient _auditClient;
    private readonly ILogger<CreateDepartmentCommandHandler> _logger;

    public CreateDepartmentCommandHandler(
        IDepartmentRepository repository,
        IAuditServiceClient auditClient,
        ILogger<CreateDepartmentCommandHandler> logger)
    {
        _repository = repository;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<Result<DepartmentDto>> Handle(CreateDepartmentCommand request, CancellationToken ct)
    {
        // Check for duplicate name
        var existing = await _repository.GetByNameAsync(request.Name, ct);
        if (existing != null)
            return Result<DepartmentDto>.Failure("A department with this name already exists");

        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Code = request.Code,
            ParentDepartmentId = request.ParentDepartmentId,
            HeadId = request.HeadId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(department, ct);

        // Audit
        await _auditClient.LogActionAsync(
            null,
            "CREATE_DEPARTMENT",
            "Department",
            department.Id.ToString(),
            request,
            null,
            null,
            ct
        );

        _logger.LogInformation("Department {DepartmentId} ({Name}) created", department.Id, department.Name);

        // Reload with relationships
        department = await _repository.GetByIdAsync(department.Id, ct);

        return Result<DepartmentDto>.Success(new DepartmentDto(
            department!.Id,
            department.Name,
            department.Description,
            department.Code,
            department.ParentDepartmentId,
            department.ParentDepartment?.Name,
            department.HeadId,
            department.Head != null ? $"{department.Head.FirstName} {department.Head.LastName}" : null,
            0,
            department.IsActive
        ));
    }
}

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Result<DepartmentDto>>
{
    private readonly IDepartmentRepository _repository;
    private readonly IAuditServiceClient _auditClient;
    private readonly ILogger<UpdateDepartmentCommandHandler> _logger;

    public UpdateDepartmentCommandHandler(
        IDepartmentRepository repository,
        IAuditServiceClient auditClient,
        ILogger<UpdateDepartmentCommandHandler> logger)
    {
        _repository = repository;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<Result<DepartmentDto>> Handle(UpdateDepartmentCommand request, CancellationToken ct)
    {
        var department = await _repository.GetByIdAsync(request.Id, ct);
        if (department == null)
            return Result<DepartmentDto>.Failure("Department not found", 404);

        department.Name = request.Name;
        department.Description = request.Description;
        department.Code = request.Code;
        department.ParentDepartmentId = request.ParentDepartmentId;
        department.HeadId = request.HeadId;
        department.IsActive = request.IsActive;

        await _repository.UpdateAsync(department, ct);

        // Audit
        await _auditClient.LogActionAsync(
            null,
            "UPDATE_DEPARTMENT",
            "Department",
            department.Id.ToString(),
            request,
            null,
            null,
            ct
        );

        _logger.LogInformation("Department {DepartmentId} updated", department.Id);

        // Reload with relationships
        department = await _repository.GetByIdAsync(request.Id, ct);

        return Result<DepartmentDto>.Success(new DepartmentDto(
            department!.Id,
            department.Name,
            department.Description,
            department.Code,
            department.ParentDepartmentId,
            department.ParentDepartment?.Name,
            department.HeadId,
            department.Head != null ? $"{department.Head.FirstName} {department.Head.LastName}" : null,
            department.StaffMembers?.Count ?? 0,
            department.IsActive
        ));
    }
}

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, Result<bool>>
{
    private readonly IDepartmentRepository _repository;
    private readonly IAuditServiceClient _auditClient;
    private readonly ILogger<DeleteDepartmentCommandHandler> _logger;

    public DeleteDepartmentCommandHandler(
        IDepartmentRepository repository,
        IAuditServiceClient auditClient,
        ILogger<DeleteDepartmentCommandHandler> logger)
    {
        _repository = repository;
        _auditClient = auditClient;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteDepartmentCommand request, CancellationToken ct)
    {
        var department = await _repository.GetByIdAsync(request.Id, ct);
        if (department == null)
            return Result<bool>.Failure("Department not found", 404);

        // Check for staff members
        if (await _repository.HasStaffAsync(request.Id, ct))
            return Result<bool>.Failure("Cannot delete department with assigned staff members");

        await _repository.DeleteAsync(request.Id, ct);

        // Audit
        await _auditClient.LogActionAsync(
            null,
            "DELETE_DEPARTMENT",
            "Department",
            department.Id.ToString(),
            new { Name = department.Name },
            null,
            null,
            ct
        );

        _logger.LogInformation("Department {DepartmentId} ({Name}) deleted", department.Id, department.Name);

        return Result<bool>.Success(true);
    }
}
