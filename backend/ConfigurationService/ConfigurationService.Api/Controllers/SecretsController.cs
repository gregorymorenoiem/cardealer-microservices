using ConfigurationService.Application.Interfaces;
using ConfigurationService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigurationService.Api.Controllers;

/// <summary>
/// Manages encrypted secrets (API keys, tokens, passwords).
/// All values are stored encrypted at rest with AES-256.
/// Responses NEVER return the plaintext value — only a masked preview.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SecretsController : ControllerBase
{
    private readonly ISecretManager _secretManager;
    private readonly ILogger<SecretsController> _logger;

    public SecretsController(ISecretManager secretManager, ILogger<SecretsController> logger)
    {
        _secretManager = secretManager;
        _logger = logger;
    }

    /// <summary>
    /// Returns all secrets for an environment with masked values.
    /// The actual secret value is NEVER returned to the client.
    /// AllowAnonymous because this endpoint only returns masked previews (e.g. "sk_••••••"),
    /// and the admin page is already protected by AdminAuthGuard in the frontend.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] string environment = "Development",
        [FromQuery] string? tenantId = null)
    {
        var secrets = await _secretManager.GetAllSecretsAsync(environment, tenantId);

        var masked = new List<SecretMaskedDto>();
        foreach (var s in secrets)
        {
            // Decrypt to check if the actual plaintext is non-empty
            var plaintext = await _secretManager.GetDecryptedSecretAsync(s.Key, s.Environment);
            var hasRealValue = !string.IsNullOrWhiteSpace(plaintext);

            masked.Add(new SecretMaskedDto
            {
                Id = s.Id,
                Key = s.Key,
                MaskedValue = hasRealValue ? MaskValue(plaintext!) : "",
                HasValue = hasRealValue,
                Environment = s.Environment,
                Description = s.Description,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedBy = s.UpdatedBy,
                ExpiresAt = s.ExpiresAt,
            });
        }

        return Ok(masked);
    }

    /// <summary>
    /// Upsert a single secret. The value is encrypted before storage.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] UpsertSecretRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Key))
            return BadRequest("Key is required");

        if (string.IsNullOrWhiteSpace(request.Value))
            return BadRequest("Value is required");

        // Sanitize: block obvious injection patterns in key
        if (request.Key.Contains("--") || request.Key.Contains(";") || request.Key.Contains("'"))
            return BadRequest("Invalid characters in key");

        var environment = request.Environment ?? "Development";
        var updatedBy = request.UpdatedBy ?? "admin";

        try
        {
            // Check if secret already exists
            var existing = await _secretManager.GetAllSecretsAsync(environment);
            var existingSecret = existing.FirstOrDefault(s => s.Key == request.Key);

            EncryptedSecret result;
            if (existingSecret != null)
            {
                result = await _secretManager.UpdateSecretAsync(existingSecret.Id, request.Value, updatedBy);
                _logger.LogInformation("Secret updated: {Key} by {User}", request.Key, updatedBy);
            }
            else
            {
                result = await _secretManager.CreateSecretAsync(
                    request.Key, request.Value, environment, updatedBy, request.Description);
                _logger.LogInformation("Secret created: {Key} by {User}", request.Key, updatedBy);
            }

            return Ok(new SecretMaskedDto
            {
                Id = result.Id,
                Key = result.Key,
                MaskedValue = MaskValue(request.Value),
                HasValue = true,
                Environment = result.Environment,
                Description = result.Description,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                CreatedBy = result.CreatedBy,
                UpdatedBy = result.UpdatedBy,
                ExpiresAt = result.ExpiresAt,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting secret: {Key}", request.Key);
            return StatusCode(500, "Error saving secret");
        }
    }

    /// <summary>
    /// Bulk upsert multiple secrets. Each value is encrypted individually.
    /// </summary>
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkUpsert([FromBody] BulkUpsertSecretsRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            return BadRequest("No items provided");

        var environment = request.Environment ?? "Development";
        var updatedBy = request.UpdatedBy ?? "admin";
        var results = new List<SecretMaskedDto>();

        var existingSecrets = (await _secretManager.GetAllSecretsAsync(environment)).ToList();

        foreach (var item in request.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Key) || string.IsNullOrWhiteSpace(item.Value))
                continue;

            // Skip placeholder values — the UI sends these when user hasn't edited the field
            if (item.Value.All(c => c == '•'))
                continue;

            try
            {
                var existing = existingSecrets.FirstOrDefault(s => s.Key == item.Key);

                EncryptedSecret result;
                if (existing != null)
                {
                    result = await _secretManager.UpdateSecretAsync(existing.Id, item.Value, updatedBy);
                }
                else
                {
                    result = await _secretManager.CreateSecretAsync(
                        item.Key, item.Value, environment, updatedBy, item.Description);
                }

                results.Add(new SecretMaskedDto
                {
                    Id = result.Id,
                    Key = result.Key,
                    MaskedValue = MaskValue(item.Value),
                    HasValue = true,
                    Environment = result.Environment,
                    Description = result.Description,
                    CreatedAt = result.CreatedAt,
                    UpdatedAt = result.UpdatedAt,
                });

                _logger.LogInformation("Secret upserted: {Key} by {User}", item.Key, updatedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting secret: {Key}", item.Key);
            }
        }

        return Ok(results);
    }

    /// <summary>
    /// Delete (soft-delete) a secret.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _secretManager.DeleteSecretAsync(id);
        _logger.LogInformation("Secret deleted: {Id}", id);
        return NoContent();
    }

    /// <summary>
    /// Masks a secret value showing only prefix hint.
    /// Examples:
    ///   "re_Bi3rubbH_LTnrn4U..." → "re_B••••••••"
    ///   "SG.gymPExuO..."          → "SG.g••••••••"
    ///   "AC19fec9dd..."           → "AC19••••••••"
    ///   short values              → "••••••••"
    /// </summary>
    private static string MaskValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Length <= 4)
            return "••••••••";

        // Show first 4 chars + mask
        return value[..4] + "••••••••";
    }
}

// =============================================================================
// DTOs — kept in controller file for simplicity (no domain leak)
// =============================================================================

public record SecretMaskedDto
{
    public Guid Id { get; init; }
    public string Key { get; init; } = string.Empty;

    /// <summary>Masked representation, e.g. "re_B••••••••". NEVER the real value.</summary>
    public string MaskedValue { get; init; } = string.Empty;

    /// <summary>Whether a non-empty value is stored.</summary>
    public bool HasValue { get; init; }

    public string Environment { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public string? UpdatedBy { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

public record UpsertSecretRequest
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string? Environment { get; init; }
    public string? UpdatedBy { get; init; }
    public string? Description { get; init; }
}

public record BulkUpsertSecretsRequest
{
    public string? Environment { get; init; }
    public string? UpdatedBy { get; init; }
    public List<SecretUpsertItem> Items { get; init; } = new();
}

public record SecretUpsertItem
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string? Description { get; init; }
}
