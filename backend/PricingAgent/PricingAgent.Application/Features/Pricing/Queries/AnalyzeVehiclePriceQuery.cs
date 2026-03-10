using MediatR;
using PricingAgent.Application.DTOs;

namespace PricingAgent.Application.Features.Pricing.Queries;

/// <summary>
/// MediatR query to analyze vehicle pricing using LLM.
/// </summary>
public sealed record AnalyzeVehiclePriceQuery(PricingAnalysisRequest Request) : IRequest<PricingAnalysisResponse>;
