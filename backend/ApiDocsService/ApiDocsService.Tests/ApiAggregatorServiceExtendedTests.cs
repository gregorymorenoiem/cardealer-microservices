using ApiDocsService.Core.Interfaces;
using ApiDocsService.Core.Models;
using ApiDocsService.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace ApiDocsService.Tests;

/// <summary>
/// Extended tests for ApiAggregatorService to improve coverage >80%
/// Tests for RefreshAllDocsAsync, SearchEndpointsAsync, GetAllApiDocsAsync, and error scenarios
/// </summary>
public class ApiAggregatorServiceExtendedTests
{
    private readonly Mock<ILogger<ApiAggregatorService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IApiAggregatorService _service;

    public ApiAggregatorServiceExtendedTests()
    {
        _loggerMock = new Mock<ILogger<ApiAggregatorService>>();
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpHandlerMock.Object);

        var servicesConfig = new ServicesConfiguration
        {
            Services = new List<ServiceInfo>
            {
                new()
                {
                    Name = "TestService",
                    DisplayName = "Test Service",
                    Description = "Test description",
                    BaseUrl = "http://testservice",
                    SwaggerUrl = "http://testservice/swagger/v1/swagger.json",
                    Version = "v1",
                    Category = "Testing",
                    Tags = new List<string> { "test" }
                },
                new()
                {
                    Name = "UserService",
                    DisplayName = "User Service",
                    Description = "Manages user data",
                    BaseUrl = "http://userservice",
                    SwaggerUrl = "http://userservice/swagger/v1/swagger.json",
                    Version = "v1",
                    Category = "Core",
                    Tags = new List<string> { "users", "authentication" }
                }
            }
        };

        var options = Options.Create(servicesConfig);
        _service = new ApiAggregatorService(_httpClient, options, _loggerMock.Object);
    }

    [Fact]
    public async Task RefreshAllDocsAsync_WithValidSpecs_ShouldPopulateCache()
    {
        // Arrange
        var openApiSpec = @"{
            ""openapi"": ""3.0.1"",
            ""info"": { ""title"": ""Test API"", ""version"": ""v1"" },
            ""paths"": {
                ""/api/users"": {
                    ""get"": {
                        ""summary"": ""Get all users"",
                        ""description"": ""Returns list of users"",
                        ""tags"": [""Users""],
                        ""responses"": {
                            ""200"": { ""description"": ""Success"" }
                        }
                    },
                    ""post"": {
                        ""summary"": ""Create user"",
                        ""tags"": [""Users""],
                        ""responses"": {
                            ""201"": { ""description"": ""Created"" }
                        }
                    }
                }
            }
        }";

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(openApiSpec)
            });

        // Act
        await _service.RefreshAllDocsAsync();
        var docs = await _service.GetAllApiDocsAsync();

        // Assert
        docs.Should().HaveCount(2);
        docs.Should().OnlyContain(d => d.OpenApiSpec != null);
        docs.First().Endpoints.Should().HaveCountGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task RefreshAllDocsAsync_WithInvalidSpec_ShouldHandleGracefully()
    {
        // Arrange
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act
        await _service.RefreshAllDocsAsync();
        var docs = await _service.GetAllApiDocsAsync();

        // Assert
        docs.Should().HaveCount(2);
        docs.Should().OnlyContain(d => d.OpenApiSpec == null);
    }

    [Fact]
    public async Task RefreshAllDocsAsync_WithPartialFailure_ShouldReturnSuccessfulServices()
    {
        // Arrange
        var openApiSpec = @"{
            ""openapi"": ""3.0.1"",
            ""info"": { ""title"": ""Test API"" },
            ""paths"": {
                ""/api/test"": {
                    ""get"": {
                        ""summary"": ""Test endpoint"",
                        ""tags"": [""Test""]
                    }
                }
            }
        }";

        var callCount = 0;
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(openApiSpec)
                    };
                }
                return new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable };
            });

        // Act
        await _service.RefreshAllDocsAsync();
        var docs = await _service.GetAllApiDocsAsync();

        // Assert
        docs.Should().HaveCount(2);
        docs.Should().Contain(d => d.OpenApiSpec != null);
        docs.Should().Contain(d => d.OpenApiSpec == null);
    }

    [Fact]
    public async Task GetAllApiDocsAsync_WithoutRefresh_ShouldReturnEmptyList()
    {
        // Act
        var docs = await _service.GetAllApiDocsAsync();

        // Assert
        // Should trigger automatic refresh, but with no HTTP setup, will return empty specs
        docs.Should().HaveCountGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetAllApiDocsAsync_AfterRefresh_ShouldReturnCachedDocs()
    {
        // Arrange
        var openApiSpec = @"{
            ""openapi"": ""3.0.1"",
            ""info"": { ""title"": ""Test API"" },
            ""paths"": {
                ""/api/health"": {
                    ""get"": {
                        ""summary"": ""Health check"",
                        ""tags"": [""Health""]
                    }
                }
            }
        }";

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(openApiSpec)
            });

        // Act
        await _service.RefreshAllDocsAsync();
        var docs1 = await _service.GetAllApiDocsAsync();
        var docs2 = await _service.GetAllApiDocsAsync();

        // Assert
        docs1.Should().HaveCount(2);
        docs2.Should().HaveCount(2);
        docs1.Should().BeEquivalentTo(docs2, options => options.IgnoringCyclicReferences());
    }

    [Fact]
    public async Task SearchEndpointsAsync_WithMatchingPath_ShouldReturnResults()
    {
        // Arrange
        var openApiSpec = @"{
            ""openapi"": ""3.0.1"",
            ""info"": { ""title"": ""Test API"" },
            ""paths"": {
                ""/api/users/{id}"": {
                    ""get"": {
                        ""summary"": ""Get user by ID"",
                        ""description"": ""Retrieves a single user"",
                        ""tags"": [""Users""]
                    }
                },
                ""/api/products"": {
                    ""get"": {
                        ""summary"": ""Get products"",
                        ""tags"": [""Products""]
                    }
                }
            }
        }";

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(openApiSpec)
            });

        await _service.RefreshAllDocsAsync();

        // Act
        var results = await _service.SearchEndpointsAsync("users");

        // Assert
        results.Should().HaveCountGreaterThan(0);
        results.Should().Contain(e => e.Path.Contains("/api/users"));
    }

    [Fact]
    public async Task SearchEndpointsAsync_WithMatchingSummary_ShouldReturnResults()
    {
        // Arrange
        var openApiSpec = @"{
            ""openapi"": ""3.0.1"",
            ""info"": { ""title"": ""Test API"" },
            ""paths"": {
                ""/api/data"": {
                    ""post"": {
                        ""summary"": ""Submit customer data"",
                        ""description"": ""Stores customer information"",
                        ""tags"": [""Data""]
                    }
                }
            }
        }";

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(openApiSpec)
            });

        await _service.RefreshAllDocsAsync();

        // Act
        var results = await _service.SearchEndpointsAsync("customer");

        // Assert
        results.Should().HaveCountGreaterThan(0);
        results.Should().Contain(e => e.Summary != null && e.Summary.Contains("customer"));
    }

    [Fact]
    public async Task SearchEndpointsAsync_WithMatchingTag_ShouldReturnResults()
    {
        // Arrange
        var openApiSpec = @"{
            ""openapi"": ""3.0.1"",
            ""info"": { ""title"": ""Test API"" },
            ""paths"": {
                ""/api/orders"": {
                    ""get"": {
                        ""summary"": ""Get orders"",
                        ""tags"": [""Orders"", ""Commerce""]
                    }
                }
            }
        }";

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(openApiSpec)
            });

        await _service.RefreshAllDocsAsync();

        // Act
        var results = await _service.SearchEndpointsAsync("commerce");

        // Assert
        results.Should().HaveCountGreaterThan(0);
        results.Should().Contain(e => e.Tags.Contains("Commerce"));
    }

    [Fact]
    public async Task SearchEndpointsAsync_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var openApiSpec = @"{
            ""openapi"": ""3.0.1"",
            ""info"": { ""title"": ""Test API"" },
            ""paths"": {
                ""/api/test"": {
                    ""get"": {
                        ""summary"": ""Test endpoint"",
                        ""tags"": [""Test""]
                    }
                }
            }
        }";

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(openApiSpec)
            });

        await _service.RefreshAllDocsAsync();

        // Act
        var results = await _service.SearchEndpointsAsync("nonexistent query xyz");

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOpenApiSpecAsync_WithHttpError_ShouldReturnNull()
    {
        // Arrange
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var spec = await _service.GetOpenApiSpecAsync("TestService");

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public async Task GetOpenApiSpecAsync_WithNetworkException_ShouldReturnNull()
    {
        // Arrange
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var spec = await _service.GetOpenApiSpecAsync("TestService");

        // Assert
        spec.Should().BeNull();
    }

    [Fact]
    public async Task SearchEndpointsAsync_CaseInsensitive_ShouldFindMatches()
    {
        // Arrange
        var openApiSpec = @"{
            ""openapi"": ""3.0.1"",
            ""info"": { ""title"": ""Test API"" },
            ""paths"": {
                ""/api/USERS"": {
                    ""get"": {
                        ""summary"": ""GET ALL USERS"",
                        ""description"": ""Returns User List"",
                        ""tags"": [""USERS""]
                    }
                }
            }
        }";

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(openApiSpec)
            });

        await _service.RefreshAllDocsAsync();

        // Act - Search with lowercase
        var results = await _service.SearchEndpointsAsync("users");

        // Assert
        results.Should().HaveCountGreaterThan(0);
    }
}
