using AuthService.Application.DTOs.Auth;
using AuthService.Application.DTOs.PhoneVerification;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Shared.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.PhoneVerification.Queries.GetPhoneVerificationStatus;

public class GetPhoneVerificationStatusQueryHandler : IRequestHandler<GetPhoneVerificationStatusQuery, PhoneVerificationStatusResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetPhoneVerificationStatusQueryHandler> _logger;

    public GetPhoneVerificationStatusQueryHandler(
        IUserRepository userRepository,
        ILogger<GetPhoneVerificationStatusQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<PhoneVerificationStatusResponse> Handle(GetPhoneVerificationStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId)
                ?? throw new NotFoundException("User not found.");

            return new PhoneVerificationStatusResponse(
                user.PhoneNumberConfirmed,
                user.PhoneNumber,
                user.UpdatedAt ?? user.CreatedAt
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting phone verification status for user {UserId}", request.UserId);
            throw;
        }
    }
}
