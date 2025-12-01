using MediatR;
using RoleService.Application.DTOs.Roles;

namespace RoleService.Application.UseCases.Roles.UpdateRole;

public record UpdateRoleCommand(Guid RoleId, UpdateRoleRequest Request) : IRequest<UpdateRoleResponse>;
