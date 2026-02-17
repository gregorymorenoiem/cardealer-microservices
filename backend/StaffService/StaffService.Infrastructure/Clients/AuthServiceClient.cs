using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StaffService.Application.Clients;

namespace StaffService.Infrastructure.Clients;

public class AuthServiceClient : IAuthServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthServiceClient> _logger;

    public AuthServiceClient(HttpClient httpClient, ILogger<AuthServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CreateAuthUserResult> CreateStaffUserAsync(CreateStaffUserRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/admin/register", new
            {
                email = request.Email,
                password = request.Password,
                firstName = request.FirstName,
                lastName = request.LastName,
                phoneNumber = request.PhoneNumber,
                role = request.Role
            }, ct);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CreateAuthUserResponse>(cancellationToken: ct);
                return new CreateAuthUserResult(true, result?.UserId, null);
            }

            var error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Failed to create auth user for {Email}: {Error}", request.Email, error);
            return new CreateAuthUserResult(false, null, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating auth user for {Email}", request.Email);
            return new CreateAuthUserResult(false, null, ex.Message);
        }
    }

    public async Task<bool> DisableUserAsync(Guid authUserId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/auth/admin/users/{authUserId}/disable", null, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling auth user {UserId}", authUserId);
            return false;
        }
    }

    public async Task<bool> EnableUserAsync(Guid authUserId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/auth/admin/users/{authUserId}/enable", null, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling auth user {UserId}", authUserId);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid authUserId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/auth/admin/users/{authUserId}", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting auth user {UserId}", authUserId);
            return false;
        }
    }

    public async Task<SecurityStatusResult> GetSecurityStatusAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/auth/admin/security-status", ct);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SecurityStatusResult>(cancellationToken: ct);
                return result ?? new SecurityStatusResult(false, null, 0);
            }

            return new SecurityStatusResult(false, null, 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security status");
            return new SecurityStatusResult(false, null, 0);
        }
    }

    public async Task<bool> DeleteDefaultAdminAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync("/api/auth/admin/default-admin", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting default admin");
            return false;
        }
    }

    private class CreateAuthUserResponse
    {
        public Guid UserId { get; set; }
    }
}
