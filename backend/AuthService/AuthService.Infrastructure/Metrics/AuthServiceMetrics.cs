using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AuthService.Infrastructure.Metrics;

/// <summary>
/// Custom metrics for AuthService using OpenTelemetry.
/// Tracks authentication events, 2FA operations, and security metrics.
/// </summary>
public class AuthServiceMetrics
{
    private readonly Meter _meter;
    private readonly Counter<long> _loginAttemptsCounter;
    private readonly Counter<long> _loginSuccessCounter;
    private readonly Counter<long> _loginFailureCounter;
    private readonly Counter<long> _registrationCounter;
    private readonly Counter<long> _twoFactorEnabledCounter;
    private readonly Counter<long> _twoFactorVerificationCounter;
    private readonly Counter<long> _passwordResetCounter;
    private readonly Counter<long> _externalAuthCounter;
    private readonly Histogram<double> _authenticationDuration;
    private readonly Counter<long> _securityThreatsCounter;

    public AuthServiceMetrics()
    {
        _meter = new Meter("AuthService", "1.0.0");

        // Login metrics
        _loginAttemptsCounter = _meter.CreateCounter<long>(
            "auth.login.attempts",
            description: "Total number of login attempts");

        _loginSuccessCounter = _meter.CreateCounter<long>(
            "auth.login.success",
            description: "Number of successful logins");

        _loginFailureCounter = _meter.CreateCounter<long>(
            "auth.login.failure",
            description: "Number of failed login attempts");

        // Registration metrics
        _registrationCounter = _meter.CreateCounter<long>(
            "auth.registration.total",
            description: "Total number of user registrations");

        // 2FA metrics
        _twoFactorEnabledCounter = _meter.CreateCounter<long>(
            "auth.2fa.enabled",
            description: "Number of users enabling 2FA");

        _twoFactorVerificationCounter = _meter.CreateCounter<long>(
            "auth.2fa.verification",
            description: "Number of 2FA verification attempts");

        // Password reset metrics
        _passwordResetCounter = _meter.CreateCounter<long>(
            "auth.password.reset",
            description: "Number of password reset requests");

        // External auth metrics
        _externalAuthCounter = _meter.CreateCounter<long>(
            "auth.external.attempts",
            description: "External authentication attempts");

        // Performance metrics
        _authenticationDuration = _meter.CreateHistogram<double>(
            "auth.duration.milliseconds",
            unit: "ms",
            description: "Authentication operation duration");

        // Security metrics
        _securityThreatsCounter = _meter.CreateCounter<long>(
            "auth.security.threats",
            description: "Number of detected security threats (SQL Injection, XSS)");
    }

    public void RecordLoginAttempt(string email, bool success, string? failureReason = null)
    {
        _loginAttemptsCounter.Add(1, new KeyValuePair<string, object?>("email_domain", GetEmailDomain(email)));

        if (success)
        {
            _loginSuccessCounter.Add(1);
        }
        else
        {
            _loginFailureCounter.Add(1,
                new KeyValuePair<string, object?>("reason", failureReason ?? "unknown"));
        }
    }

    public void RecordRegistration(string email, bool hasExternalProvider = false)
    {
        _registrationCounter.Add(1,
            new KeyValuePair<string, object?>("email_domain", GetEmailDomain(email)),
            new KeyValuePair<string, object?>("external_provider", hasExternalProvider));
    }

    public void Record2FAEnabled(string userId, string method)
    {
        _twoFactorEnabledCounter.Add(1,
            new KeyValuePair<string, object?>("method", method));
    }

    public void Record2FAVerification(bool success, string method)
    {
        _twoFactorVerificationCounter.Add(1,
            new KeyValuePair<string, object?>("success", success),
            new KeyValuePair<string, object?>("method", method));
    }

    public void RecordPasswordReset(string email, string method)
    {
        _passwordResetCounter.Add(1,
            new KeyValuePair<string, object?>("method", method));
    }

    public void RecordExternalAuth(string provider, bool success)
    {
        _externalAuthCounter.Add(1,
            new KeyValuePair<string, object?>("provider", provider),
            new KeyValuePair<string, object?>("success", success));
    }

    public void RecordAuthenticationDuration(string operation, double durationMs, bool success)
    {
        _authenticationDuration.Record(durationMs,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("success", success));
    }

    public void RecordSecurityThreat(string threatType, string field)
    {
        _securityThreatsCounter.Add(1,
            new KeyValuePair<string, object?>("threat_type", threatType),
            new KeyValuePair<string, object?>("field", field));
    }

    private string GetEmailDomain(string email)
    {
        var parts = email.Split('@');
        return parts.Length > 1 ? parts[1].ToLowerInvariant() : "unknown";
    }
}
