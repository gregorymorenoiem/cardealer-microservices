using MediatR;
using AuthService.Application.DTOs.PhoneVerification;

namespace AuthService.Application.Features.TwoFactor.Commands.SendPhoneVerification;

public record SendPhoneVerificationCommand(string UserId, string PhoneNumber)
    : IRequest<SendPhoneVerificationResponse>;