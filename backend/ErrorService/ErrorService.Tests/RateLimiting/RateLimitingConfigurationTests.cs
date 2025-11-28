using ErrorService.Shared.RateLimiting;
using Xunit;

namespace ErrorService.Tests.RateLimiting
{
    public class RateLimitingConfigurationTests
    {
        [Fact]
        public void RateLimitingConfiguration_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var config = new RateLimitingConfiguration();

            // Assert
            Assert.True(config.Enabled);
            Assert.Equal(100, config.MaxRequests);
            Assert.Equal(60, config.WindowSeconds);
            Assert.True(config.EnableLogging);
            Assert.Contains("127.0.0.1", config.WhitelistedIps);
            Assert.Contains("::1", config.WhitelistedIps);
        }

        [Fact]
        public void RateLimitingConfiguration_CanBeConfigured()
        {
            // Arrange
            var config = new RateLimitingConfiguration
            {
                MaxRequests = 500,
                WindowSeconds = 120,
                Enabled = false,
                EnableLogging = false
            };

            // Act & Assert
            Assert.Equal(500, config.MaxRequests);
            Assert.Equal(120, config.WindowSeconds);
            Assert.False(config.Enabled);
            Assert.False(config.EnableLogging);
        }

        [Fact]
        public void EndpointRateLimitPolicy_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var policy = new EndpointRateLimitPolicy();

            // Assert
            Assert.Equal(100, policy.MaxRequests);
            Assert.Equal(60, policy.WindowSeconds);
            Assert.True(policy.Enabled);
        }

        [Fact]
        public void EndpointRateLimitPolicy_CanBeConfigured()
        {
            // Arrange
            var policy = new EndpointRateLimitPolicy
            {
                Endpoint = "POST:/api/errors",
                MaxRequests = 200,
                WindowSeconds = 120,
                Enabled = true
            };

            // Act & Assert
            Assert.Equal("POST:/api/errors", policy.Endpoint);
            Assert.Equal(200, policy.MaxRequests);
            Assert.Equal(120, policy.WindowSeconds);
            Assert.True(policy.Enabled);
        }

        [Fact]
        public void ClientRateLimitPolicy_CanBeConfigured()
        {
            // Arrange
            var policy = new ClientRateLimitPolicy
            {
                ClientId = "client-123",
                MaxRequests = 50,
                WindowSeconds = 60
            };

            // Act & Assert
            Assert.Equal("client-123", policy.ClientId);
            Assert.Equal(50, policy.MaxRequests);
            Assert.Equal(60, policy.WindowSeconds);
        }

        [Theory]
        [InlineData(10, 30)]
        [InlineData(100, 60)]
        [InlineData(1000, 3600)]
        public void RateLimitingConfiguration_SupportsVariousValues(int maxRequests, int windowSeconds)
        {
            // Arrange & Act
            var config = new RateLimitingConfiguration
            {
                MaxRequests = maxRequests,
                WindowSeconds = windowSeconds
            };

            // Assert
            Assert.Equal(maxRequests, config.MaxRequests);
            Assert.Equal(windowSeconds, config.WindowSeconds);
        }

        [Fact]
        public void RateLimitingConfiguration_WhitelistCanBeEmpty()
        {
            // Arrange & Act
            var config = new RateLimitingConfiguration { WhitelistedIps = new List<string>() };

            // Assert
            Assert.Empty(config.WhitelistedIps);
        }

        [Fact]
        public void RateLimitingConfiguration_WhitelistCanHaveMultipleIps()
        {
            // Arrange
            var ips = new List<string> { "127.0.0.1", "192.168.1.1", "10.0.0.1" };

            // Act
            var config = new RateLimitingConfiguration { WhitelistedIps = ips };

            // Assert
            Assert.Equal(3, config.WhitelistedIps.Count);
            Assert.All(ips, ip => Assert.Contains(ip, config.WhitelistedIps));
        }
    }
}
