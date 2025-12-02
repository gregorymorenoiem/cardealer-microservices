using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.UseCases.LogError;

public record LogErrorCommand(LogErrorRequest Request) : IRequest<LogErrorResponse>;
