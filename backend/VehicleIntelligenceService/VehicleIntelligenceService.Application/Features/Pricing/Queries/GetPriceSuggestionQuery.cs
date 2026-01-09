using MediatR;
using VehicleIntelligenceService.Application.DTOs;

namespace VehicleIntelligenceService.Application.Features.Pricing.Queries;

public record GetPriceSuggestionQuery(PriceSuggestionRequestDto Request) : IRequest<PriceSuggestionDto>;
