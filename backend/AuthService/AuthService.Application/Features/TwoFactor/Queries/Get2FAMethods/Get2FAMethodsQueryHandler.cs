using AuthService.Shared.Exceptions;
using AuthService.Domain.Interfaces.Repositories;
using MediatR;
using AuthService.Domain.Enums;
using AuthService.Application.DTOs.TwoFactor;

namespace AuthService.Application.Features.TwoFactor.Queries.Get2FAMethods;

public class Get2FAMethodsQueryHandler : IRequestHandler<Get2FAMethodsQuery, Get2FAMethodsResponse>
{
    private readonly IUserRepository _userRepository;

    public Get2FAMethodsQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Get2FAMethodsResponse> Handle(Get2FAMethodsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException("User not found.");

        var twoFactorAuth = await _userRepository.GetTwoFactorAuthAsync(user.Id);

        if (twoFactorAuth == null || !user.IsTwoFactorEnabled)
        {
            return new Get2FAMethodsResponse(
                user.Id,
                TwoFactorAuthType.Authenticator,
                new List<TwoFactorAuthType>(),
                false,
                !string.IsNullOrEmpty(user.PhoneNumber),
                false
            );
        }

        return new Get2FAMethodsResponse(
            user.Id,
            twoFactorAuth.PrimaryMethod,
            twoFactorAuth.EnabledMethods,
            true,
            !string.IsNullOrEmpty(twoFactorAuth.PhoneNumber),
            twoFactorAuth.EnabledMethods.Contains(TwoFactorAuthType.Authenticator)
        );
    }
}
