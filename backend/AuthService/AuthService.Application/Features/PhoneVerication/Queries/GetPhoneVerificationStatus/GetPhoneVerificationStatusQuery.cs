using AuthService.Application.DTOs.PhoneVerification;
using MediatR;

namespace AuthService.Application.Features.PhoneVerification.Queries.GetPhoneVerificationStatus;

public record GetPhoneVerificationStatusQuery(string UserId) : IRequest<PhoneVerificationStatusResponse>;
