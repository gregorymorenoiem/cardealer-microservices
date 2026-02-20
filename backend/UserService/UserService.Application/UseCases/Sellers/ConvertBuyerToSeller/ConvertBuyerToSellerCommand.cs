using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.UseCases.Sellers.ConvertBuyerToSeller;

/// <summary>
/// Command to convert a buyer account to a seller account.
/// UserId is extracted from the authenticated user's claims in the controller.
/// </summary>
public record ConvertBuyerToSellerCommand(
    Guid UserId,
    ConvertBuyerToSellerRequest Request,
    string? IdempotencyKey = null,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<SellerConversionResultDto>;
