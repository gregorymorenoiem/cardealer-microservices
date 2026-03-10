using MediatR;
using AnalyticsAgent.Application.DTOs;

namespace AnalyticsAgent.Application.Features.Analytics.Queries;

public sealed record GenerateInsightsQuery(AnalyticsInsightRequest Request) : IRequest<AnalyticsInsightResponse>;
