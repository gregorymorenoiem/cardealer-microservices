using AuthService.Application.DTOs.PhoneVerification;
using MediatR;

namespace AuthService.Application.Features.PhoneVerification.Commands.UpdatePhoneNumber;

public record UpdatePhoneNumberCommand(string UserId, string NewPhoneNumber)
    : IRequest<SendPhoneVerificationResponse>;