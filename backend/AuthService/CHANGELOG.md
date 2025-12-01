# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
