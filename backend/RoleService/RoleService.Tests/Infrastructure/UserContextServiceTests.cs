using Microsoft.AspNetCore.Http;
using Moq;
using RoleService.Infrastructure.Services;
using System.Security.Claims;

namespace RoleService.Tests.Infrastructure;

public class UserContextServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly UserContextService _userContextService;

    public UserContextServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _userContextService = new UserContextService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void GetCurrentUserId_WithNameIdentifierClaim_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = "user-123";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.GetCurrentUserId();

        // Assert
        Assert.Equal(expectedUserId, result);
    }

    [Fact]
    public void GetCurrentUserId_WithSubClaim_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = "user-456";
        var claims = new List<Claim>
        {
            new Claim("sub", expectedUserId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.GetCurrentUserId();

        // Assert
        Assert.Equal(expectedUserId, result);
    }

    [Fact]
    public void GetCurrentUserId_WithUserIdClaim_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = "user-789";
        var claims = new List<Claim>
        {
            new Claim("userId", expectedUserId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.GetCurrentUserId();

        // Assert
        Assert.Equal(expectedUserId, result);
    }

    [Fact]
    public void GetCurrentUserId_WithNoHttpContext_ReturnsAnonymous()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _userContextService.GetCurrentUserId();

        // Assert
        Assert.Equal("anonymous", result);
    }

    [Fact]
    public void GetCurrentUserId_WithNoUserClaims_ReturnsAnonymous()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.GetCurrentUserId();

        // Assert
        Assert.Equal("anonymous", result);
    }

    [Fact]
    public void GetCurrentUserName_WithNameClaim_ReturnsUserName()
    {
        // Arrange
        var expectedUserName = "john.doe";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, expectedUserName)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.GetCurrentUserName();

        // Assert
        Assert.Equal(expectedUserName, result);
    }

    [Fact]
    public void GetCurrentUserName_WithNoClaims_ReturnsAnonymous()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.GetCurrentUserName();

        // Assert
        Assert.Equal("anonymous", result);
    }

    [Fact]
    public void GetCurrentUserRoles_WithRoleClaims_ReturnsRoles()
    {
        // Arrange
        var expectedRoles = new[] { "Admin", "User", "Manager" };
        var claims = expectedRoles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.GetCurrentUserRoles();

        // Assert
        Assert.Equal(expectedRoles, result);
    }

    [Fact]
    public void GetCurrentUserRoles_WithNoClaims_ReturnsEmpty()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.GetCurrentUserRoles();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void IsAuthenticated_WithAuthenticatedUser_ReturnsTrue()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.IsAuthenticated();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAuthenticated_WithUnauthenticatedUser_ReturnsFalse()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.IsAuthenticated();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetCurrentUserEmail_WithEmailClaim_ReturnsEmail()
    {
        // Arrange
        var expectedEmail = "john.doe@example.com";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, expectedEmail)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.GetCurrentUserEmail();

        // Assert
        Assert.Equal(expectedEmail, result);
    }

    [Fact]
    public void GetCurrentUserEmail_WithNoClaims_ReturnsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _userContextService.GetCurrentUserEmail();

        // Assert
        Assert.Null(result);
    }
}
