using MediatR;

namespace RoleService.Application.UseCases.Roles.DeleteRole;

public record DeleteRoleCommand(Guid RoleId) : IRequest<bool>;
