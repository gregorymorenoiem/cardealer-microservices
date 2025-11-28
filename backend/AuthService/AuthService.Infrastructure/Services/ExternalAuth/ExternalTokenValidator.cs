using System.Text.Json;
using AuthService.Domain.Enums;
using AuthService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services.ExternalAuth;

public class ExternalTokenValidator : IExternalTokenValidator
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalTokenValidator> _logger;

    public ExternalTokenValidator(HttpClient httpClient, ILogger<ExternalTokenValidator> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<(bool isValid, string email, string userId, string name)> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");

            if (!response.IsSuccessStatusCode)
                return (false, string.Empty, string.Empty, string.Empty);

            var content = await response.Content.ReadAsStringAsync();
            var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(content);

            if (tokenInfo == null || string.IsNullOrEmpty(tokenInfo.email))
                return (false, string.Empty, string.Empty, string.Empty);

            return (true, tokenInfo.email, tokenInfo.sub, tokenInfo.name ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google token");
            return (false, string.Empty, string.Empty, string.Empty);
        }
    }

    public async Task<(bool isValid, string email, string userId, string name)> ValidateMicrosoftTokenAsync(string idToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {idToken}");

            var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");

            if (!response.IsSuccessStatusCode)
                return (false, string.Empty, string.Empty, string.Empty);

            var content = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<MicrosoftUserInfo>(content);

            if (userInfo == null || string.IsNullOrEmpty(userInfo.mail))
                return (false, string.Empty, string.Empty, string.Empty);

            return (true, userInfo.mail, userInfo.id, userInfo.displayName ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Microsoft token");
            return (false, string.Empty, string.Empty, string.Empty);
        }
    }

    private class GoogleTokenInfo
    {
        public string email { get; set; } = string.Empty;
        public string sub { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public bool email_verified { get; set; }
    }

    private class MicrosoftUserInfo
    {
        public string id { get; set; } = string.Empty;
        public string mail { get; set; } = string.Empty;
        public string displayName { get; set; } = string.Empty;
        public string userPrincipalName { get; set; } = string.Empty;
    }
}
