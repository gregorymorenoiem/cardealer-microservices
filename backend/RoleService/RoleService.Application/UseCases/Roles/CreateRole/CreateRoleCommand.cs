using MediatR;
using RoleService.Application.DTOs.Roles;

namespace RoleService.Application.UseCases.Roles.CreateRole;

public record CreateRoleCommand(CreateRoleRequest Request) : IRequest<CreateRoleResponse>;
