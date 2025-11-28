using MediatR;
using AuthService.Application.DTOs.Auth;

namespace AuthService.Application.Features.TwoFactor.Commands.VerifyPhoneNumber;

public record VerifyPhoneNumberCommand(string UserId, string PhoneNumber, string VerificationCode)
    : IRequest<VerifyPhoneNumberResponse>;