using MediatR;
using RoleService.Application.DTOs.Roles;

namespace RoleService.Application.UseCases.Roles.GetRole;

public record GetRoleByIdQuery(Guid RoleId) : IRequest<RoleDetailsDto?>;
