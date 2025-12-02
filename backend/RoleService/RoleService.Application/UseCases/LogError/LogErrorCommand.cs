using MediatR;
using RoleService.Application.DTOs;

namespace RoleService.Application.UseCases.LogError;

public record LogErrorCommand(LogErrorRequest Request) : IRequest<LogErrorResponse>;
