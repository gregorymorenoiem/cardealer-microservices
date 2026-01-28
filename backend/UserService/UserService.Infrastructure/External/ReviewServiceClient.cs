using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ServiceDiscovery.Application.Interfaces;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.External;

/// <summary>
/// Cliente HTTP para comunicarse con ReviewService
/// </summary>
public class ReviewServiceClient : IReviewServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReviewServiceClient> _logger;
    private readonly IServiceDiscovery _serviceDiscovery;
    private readonly IMemoryCache _cache;
    private const int CacheTtlMinutes = 2;

    public ReviewServiceClient(
        HttpClient httpClient,
        ILogger<ReviewServiceClient> logger,
        IServiceDiscovery serviceDiscovery,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _serviceDiscovery = serviceDiscovery;
        _cache = cache;
    }

    private async Task<string> GetServiceUrlAsync()
    {
        try
        {
            var instance = await _serviceDiscovery.FindServiceInstanceAsync("ReviewService");
            return instance != null ? $"http://{instance.Host}:{instance.Port}" : "http://reviewservice:80";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error resolving ReviewService from Consul, using fallback");
            return "http://reviewservice:80";
        }
    }

    /// <summary>
    /// Obtiene las reseñas de un vendedor
    /// </summary>
    public async Task<SellerReviewsResult> GetSellerReviewsAsync(
        Guid sellerId,
        int page = 1,
        int pageSize = 10,
        int? rating = null)
    {
        try
        {
            var cacheKey = $"seller-reviews:{sellerId}:{page}:{pageSize}:{rating ?? 0}";
            if (_cache.TryGetValue(cacheKey, out SellerReviewsResult? cached))
            {
                _logger.LogDebug("Seller reviews for {SellerId} retrieved from cache", sellerId);
                return cached!;
            }

            var baseUrl = await GetServiceUrlAsync();
            var url = $"{baseUrl}/api/reviews/seller/{sellerId}?page={page}&pageSize={pageSize}";
            if (rating.HasValue)
            {
                url += $"&rating={rating.Value}";
            }

            _logger.LogDebug("Fetching seller reviews from {Url}", url);

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get seller reviews. Status: {StatusCode}", response.StatusCode);
                return new SellerReviewsResult { Page = page, PageSize = pageSize };
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ReviewsApiResponse>();
            if (apiResponse?.Data == null)
            {
                return new SellerReviewsResult { Page = page, PageSize = pageSize };
            }

            var result = new SellerReviewsResult
            {
                Reviews = apiResponse.Data.Select(r => new SellerReviewItemDto
                {
                    Id = r.Id,
                    ReviewerId = r.ReviewerId,
                    ReviewerName = r.ReviewerName ?? "Usuario anónimo",
                    ReviewerPhotoUrl = r.ReviewerPhotoUrl,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    VehicleTitle = r.VehicleTitle,
                    IsVerifiedPurchase = r.IsVerifiedPurchase,
                    Reply = r.Reply != null ? new ReviewReplyDto
                    {
                        Content = r.Reply.Content,
                        RepliedAt = r.Reply.RepliedAt
                    } : null
                }).ToList(),
                AverageRating = apiResponse.AverageRating,
                TotalCount = apiResponse.TotalCount,
                RatingDistribution = apiResponse.RatingDistribution ?? new Dictionary<int, int>
                {
                    { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 }
                },
                Page = apiResponse.Page,
                PageSize = apiResponse.PageSize,
                TotalPages = apiResponse.TotalPages
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheTtlMinutes));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching seller reviews for {SellerId}", sellerId);
            return new SellerReviewsResult { Page = page, PageSize = pageSize };
        }
    }

    /// <summary>
    /// Obtiene el resumen de ratings de un vendedor
    /// </summary>
    public async Task<SellerRatingSummary?> GetSellerRatingSummaryAsync(Guid sellerId)
    {
        try
        {
            var cacheKey = $"seller-rating-summary:{sellerId}";
            if (_cache.TryGetValue(cacheKey, out SellerRatingSummary? cached))
            {
                return cached;
            }

            var baseUrl = await GetServiceUrlAsync();
            var url = $"{baseUrl}/api/reviews/seller/{sellerId}/summary";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get seller rating summary. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<SellerRatingSummary>();
            if (result != null)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheTtlMinutes));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching seller rating summary for {SellerId}", sellerId);
            return null;
        }
    }

    // Internal DTOs for API response mapping
    private class ReviewsApiResponse
    {
        public List<ReviewDto> Data { get; set; } = new();
        public double AverageRating { get; set; }
        public int TotalCount { get; set; }
        public Dictionary<int, int>? RatingDistribution { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    private class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public string? ReviewerPhotoUrl { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? VehicleTitle { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public ReplyDto? Reply { get; set; }
    }

    private class ReplyDto
    {
        public string Content { get; set; } = string.Empty;
        public DateTime RepliedAt { get; set; }
    }
}
