using MediatR;
using ListingAgent.Application.DTOs;

namespace ListingAgent.Application.Features.Listing.Queries;

public sealed record OptimizeListingQuery(ListingOptimizationRequest Request) : IRequest<ListingOptimizationResponse>;
