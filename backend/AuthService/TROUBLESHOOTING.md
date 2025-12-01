# Troubleshooting Guide: AuthService

## 🔍 Common Issues

### 1. Database Connection Failed
**Symptoms**: API returns 500, logs show `NpgsqlException`.
**Possible Causes**:
- PostgreSQL container is down.
- Connection string is incorrect.
- Network issues between containers.
**Resolution**:
- Check container status: `docker ps`.
- Verify `ConnectionStrings:DefaultConnection` in `appsettings.json`.
- Check logs: `docker logs authservice`.

### 2. RabbitMQ Connection Refused
**Symptoms**: Startup failure or event publishing errors.
**Possible Causes**:
- RabbitMQ is not ready.
- Invalid credentials.
**Resolution**:
- Ensure RabbitMQ management UI is accessible (port 15672).
- Verify `RabbitMQ` settings in configuration.

### 3. Token Validation Failed
**Symptoms**: 401 Unauthorized on protected endpoints.
**Possible Causes**:
- Token expired.
- Clock skew issues.
- Wrong signing key.
**Resolution**:
- Check `exp` claim in JWT.
- Verify `Jwt:Key` matches between generating and validating services.

## 📊 Observability

### Logs
- **Console**: Structured logs via Serilog.
- **File**: Logs stored in `logs/authservice-{date}.txt`.

### Metrics
- **Prometheus**: Metrics exposed at `/metrics` (if configured).
- **Health**: Check `/health/live` and `/health/ready`.

### Tracing
- **Jaeger**: Traces sent to OTLP endpoint (default: `http://localhost:4317`).
