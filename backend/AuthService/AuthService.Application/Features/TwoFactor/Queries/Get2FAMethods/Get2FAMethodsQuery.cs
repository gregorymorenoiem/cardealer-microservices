using MediatR;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Queries.Get2FAMethods;

public record Get2FAMethodsQuery(string UserId) : IRequest<Get2FAMethodsResponse>;
