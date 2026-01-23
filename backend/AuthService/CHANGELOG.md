# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.0.0] - 2026-01-XX (Sprint 18: Seguridad Avanzada 2FA)

### Added
- **US-18.1: Recovery Codes Dual Persistence**
  - Recovery codes now persist to BOTH Redis (365 days TTL) AND PostgreSQL
  - Automatic fallback to PostgreSQL if Redis cache miss
  - Recovery codes survive Redis restarts/failures

- **US-18.2: Security Alert Notifications**
  - `SecurityAlertDto` for structured security alert data
  - Email notifications after 3+ failed login/2FA attempts
  - Account lockout email notifications
  - HTML email templates with IP, location, timestamp, and action buttons
  - `IAuthNotificationService.SendSecurityAlertAsync()` method

- **US-18.3: CAPTCHA Integration**
  - `ICaptchaService` interface for Google reCAPTCHA v3
  - `CaptchaService` implementation with score-based verification
  - CAPTCHA required after 2 failed login attempts
  - Configurable via `appsettings.json` (ReCaptcha section)
  - `LoginCommand` now accepts optional `CaptchaToken`

- **US-18.4: Device Fingerprinting**
  - `TrustedDevice` entity for storing device fingerprints
  - `ITrustedDeviceRepository` for device CRUD operations
  - `IDeviceFingerprintService` for device management
  - New device login detection and notifications
  - Maximum 10 devices per user (auto-removes oldest)
  - Device revocation on password change

- **US-18.5: Security Audit Logging (SIEM)**
  - `ISecurityAuditService` interface
  - Structured logging compatible with Splunk/Elasticsearch/Datadog
  - Security event types: LOGIN_SUCCESS, LOGIN_FAILURE, 2FA_SUCCESS, 2FA_FAILURE, etc.
  - Email masking for privacy (jo***@domain.com)
  - CEF (Common Event Format) compatible output

### Changed
- `LoginCommandHandler` now tracks failed attempts with CAPTCHA/alert integration
- `VerifySms2FACodeCommandHandler` sends security alerts after 3 failed attempts
- `RecoveryCodeLoginCommandHandler` uses real IP from `IRequestContext`
- All 2FA handlers now use `_requestContext.IpAddress` instead of hardcoded "127.0.0.1"

### Database
- New table: `trusted_devices` with indexes on user_id and fingerprint_hash

## [2.5.0] - 2026-01-XX (Sprint 13: IP Context & Permission Verification)

### Added
- Real IP address extraction from X-Forwarded-For and X-Real-IP headers
- IPv4-mapped-to-IPv6 address normalization
- Permission verification in RoleService with database lookup

## [Unreleased]

### Added
- OpenTelemetry instrumentation for Tracing and Metrics.
- Polly policies (Retry, Circuit Breaker) for HTTP clients.
- Health Check endpoints (`/health/ready`, `/health/live`).
- XML documentation for Swagger.

## [1.0.0] - 2025-11-01

### Added
- Initial release of AuthService.
- User registration and login endpoints.
- JWT token generation and validation.
- Refresh token support.
- Role-based authorization.
- PostgreSQL integration.
- Redis caching for tokens.
