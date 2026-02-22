using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.Domain.Entities;
using UserService.Domain.Entities.Privacy;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.BackgroundJobs;

/// <summary>
/// Background worker that runs daily and anonymizes accounts whose 15-day
/// grace period has elapsed since the deletion request was confirmed.
/// Complies with Ley 172-13 (Dominican Republic data protection law).
/// </summary>
public class AccountDeletionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AccountDeletionWorker> _logger;

    // Run once per day
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);

    public AccountDeletionWorker(IServiceScopeFactory scopeFactory, ILogger<AccountDeletionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AccountDeletionWorker started.");

        // Small initial delay so the service has time to start up fully
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredDeletionRequestsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "AccountDeletionWorker encountered an error during processing.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("AccountDeletionWorker stopped.");
    }

    private async Task ProcessExpiredDeletionRequestsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var privacyRepo = scope.ServiceProvider.GetRequiredService<IPrivacyRequestRepository>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Find all confirmed deletion requests whose grace period has ended
        var expiredRequests = await privacyRepo.GetExpiredGracePeriodRequestsAsync();
        var toProcess = expiredRequests
            .Where(r => r.IsConfirmed && r.Status == PrivacyRequestStatus.Processing)
            .ToList();

        if (!toProcess.Any())
        {
            _logger.LogDebug("AccountDeletionWorker: no expired deletion requests to process.");
            return;
        }

        _logger.LogInformation("AccountDeletionWorker: processing {Count} expired deletion request(s).", toProcess.Count);

        foreach (var request in toProcess)
        {
            try
            {
                await AnonymizeUserAsync(request, userRepo, privacyRepo, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to anonymize user for PrivacyRequest {RequestId}.", request.Id);
                // Continue with the next request — don't stop the entire batch
            }
        }
    }

    private async Task AnonymizeUserAsync(
        PrivacyRequest request,
        IUserRepository userRepo,
        IPrivacyRequestRepository privacyRepo,
        CancellationToken ct)
    {
        var user = await userRepo.GetByIdAsync(request.UserId);
        if (user == null)
        {
            _logger.LogWarning("AccountDeletionWorker: user {UserId} not found for request {RequestId}. Marking completed.", request.UserId, request.Id);
            request.Status = PrivacyRequestStatus.Completed;
            request.CompletedAt = DateTime.UtcNow;
            await privacyRepo.UpdateAsync(request);
            return;
        }

        // Anonymize PII — replace real data with non-identifying placeholders
        // Keeps the record for audit / billing integrity (Ley 172-13 permits retention of non-PII)
        var anonymizedId = request.Id.ToString("N")[..8]; // short deterministic suffix

        user.FirstName = "Cuenta";
        user.LastName = "Eliminada";
        user.Email = $"deleted_{anonymizedId}@anon.okla.do";
        user.PhoneNumber = string.Empty;
        user.ProfilePicture = null;
        user.City = null;
        user.Province = null;
        user.BusinessName = null;
        user.BusinessPhone = null;
        user.BusinessAddress = null;
        user.RNC = null;
        user.PasswordHash = string.Empty;
        user.IsActive = false;

        await userRepo.UpdateAsync(user);

        // Mark the privacy request as completed
        request.Status = PrivacyRequestStatus.Completed;
        request.CompletedAt = DateTime.UtcNow;
        await privacyRepo.UpdateAsync(request);

        _logger.LogInformation(
            "AccountDeletionWorker: anonymized user {UserId} (request {RequestId}).",
            request.UserId, request.Id);
    }
}
