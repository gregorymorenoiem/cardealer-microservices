using MediatR;
using CacheService.Domain;

namespace CacheService.Application.Queries;

public record GetStatisticsQuery : IRequest<CacheStatistics>;
