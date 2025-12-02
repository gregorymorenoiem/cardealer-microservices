# Observability Stack - Quick Start Guide
# OpenTelemetry Collector + Jaeger + Zipkin + Prometheus + Grafana

## Overview
Complete observability infrastructure for distributed tracing and metrics collection:
- **OpenTelemetry Collector**: Central telemetry hub
- **Jaeger**: Distributed tracing UI and backend
- **Zipkin**: Alternative tracing backend
- **Prometheus**: Metrics collection
- **Grafana**: Unified visualization

## Architecture
```
Microservices → OpenTelemetry Collector → Jaeger/Zipkin (Traces)
                         ↓
                    Prometheus (Metrics)
                         ↓
                    Grafana (Visualization)
```

## Prerequisites
1. Docker and Docker Compose installed
2. CarDealer network created: `docker network create cardealer-network`
3. Microservices configured with OpenTelemetry SDK

## Quick Start

### 1. Start Observability Stack
```powershell
cd c:\Users\gmoreno\source\repos\cardealer\backend\observability
docker-compose -f docker-compose.observability.yml up -d
```

### 2. Verify Services
```powershell
docker-compose -f docker-compose.observability.yml ps
```

Expected services:
- ✅ cardealer-otel-collector (port 4317, 4318)
- ✅ cardealer-jaeger (port 16686)
- ✅ cardealer-zipkin (port 9411)
- ✅ cardealer-prometheus-obs (port 9091)
- ✅ cardealer-grafana-obs (port 3001)

### 3. Access Web UIs

**Jaeger Tracing UI**
- URL: http://localhost:16686
- Search traces by service, operation, tags
- View trace details and dependencies

**Zipkin UI**
- URL: http://localhost:9411
- Alternative tracing interface
- Dependency graph visualization

**Grafana**
- URL: http://localhost:3001
- Credentials: `admin` / `admin123`
- Pre-configured datasources: Prometheus, Jaeger, Zipkin
- OpenTelemetry Collector dashboard

**Prometheus**
- URL: http://localhost:9091
- Query metrics from OTel Collector
- View targets and configuration

**OTel Collector Health**
- URL: http://localhost:13133/health
- zPages: http://localhost:55679/debug/tracez

## Configuration

### OpenTelemetry Collector

#### Receivers
- **OTLP gRPC**: `localhost:4317` (preferred)
- **OTLP HTTP**: `localhost:4318`
- **Jaeger**: Multiple protocols (14250, 14268, 6831, 6832)
- **Zipkin**: `localhost:9411`
- **Prometheus**: Scrapes from services

#### Processors
- **Batch**: Groups spans/metrics for efficiency (10s timeout)
- **Memory Limiter**: Prevents OOM (512MB limit)
- **Resource**: Adds environment attributes
- **Probabilistic Sampler**: 10% sampling rate
- **Filter**: Removes unnecessary metrics

#### Exporters
- **OTLP/Jaeger**: gRPC to Jaeger backend
- **Zipkin**: JSON format to Zipkin
- **Prometheus**: Exposes metrics on port 8889
- **Logging**: Console output for debugging
- **File**: Backup traces to disk

### Configure Services to Use Collector

Update `appsettings.json` in each microservice:

```json
{
  "OpenTelemetry": {
    "ServiceName": "AuthService",
    "Endpoint": "http://otel-collector:4318",
    "Protocol": "HttpProtobuf",
    "SamplingRatio": 0.1,
    "ExportIntervalMilliseconds": 5000
  }
}
```

## Service Integration

### .NET Microservices Setup

All services should already have OpenTelemetry configured. Verify `Program.cs`:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://otel-collector:4318");
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://otel-collector:4318");
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
        }));
```

### Verify Telemetry Export

1. Make requests to your services
2. Check Jaeger UI for traces: http://localhost:16686
3. Select service from dropdown
4. Click "Find Traces"
5. View detailed span information

## Monitoring & Troubleshooting

### View Collector Logs
```powershell
docker logs cardealer-otel-collector -f
```

### Check Collector Health
```powershell
curl http://localhost:13133/health
```

### Debug with zPages
- Tracez: http://localhost:55679/debug/tracez
- Pipelinez: http://localhost:55679/debug/pipelinez
- Servicez: http://localhost:55679/debug/servicez

### Common Issues

#### No traces in Jaeger
1. Verify OTel Collector is receiving spans:
   ```powershell
   docker logs cardealer-otel-collector | Select-String "TracesReceived"
   ```
2. Check service configuration endpoint
3. Verify network connectivity

#### High Memory Usage
- Adjust `memory_limiter` in config
- Increase `batch` timeout
- Reduce `probabilistic_sampler` percentage

#### Slow Performance
- Enable batching (already configured)
- Use gRPC instead of HTTP (port 4317)
- Increase `send_batch_size`

## Performance Tuning

### Sampling Configuration
Edit `otel-collector-config.yaml`:
```yaml
processors:
  probabilistic_sampler:
    sampling_percentage: 10.0  # Adjust: 1-100
```

### Batch Settings
```yaml
processors:
  batch:
    timeout: 10s              # Adjust: 1s-30s
    send_batch_size: 1024     # Adjust: 512-2048
    send_batch_max_size: 2048
```

### Memory Limits
```yaml
processors:
  memory_limiter:
    check_interval: 1s
    limit_mib: 512           # Adjust based on available RAM
    spike_limit_mib: 128
```

After changes, reload configuration:
```powershell
docker-compose -f docker-compose.observability.yml restart otel-collector
```

## Grafana Dashboards

### Pre-configured Datasources
- Prometheus-Observability (metrics)
- Jaeger (traces)
- Zipkin (traces)

### Import Additional Dashboards

1. Go to Grafana: http://localhost:3001
2. Click + → Import
3. Use dashboard IDs:
   - **15983**: OpenTelemetry Collector
   - **13639**: Jaeger Metrics
   - **3662**: Prometheus Stats

### Custom Queries

**Find slow requests:**
```promql
histogram_quantile(0.95, 
  rate(http_request_duration_seconds_bucket[5m])
) > 2
```

**Trace error rate:**
```promql
rate(traces_error_total[5m])
```

## Jaeger Features

### Search Traces
- By service name
- By operation name
- By tags (e.g., `http.status_code=500`)
- By duration (min/max)
- By time range

### System Architecture
- View service dependencies
- Identify bottlenecks
- Analyze call patterns

### Trace Comparison
- Compare similar traces
- Identify performance regressions
- Analyze different code paths

## Data Retention

### Jaeger
- Default: In-memory (ephemeral)
- Current: Badger DB (persistent)
- Retention: Unlimited (manual cleanup required)

### Prometheus
- Retention: 30 days
- Storage: ~1-5GB estimated

### Collector Logs
- Max size: 100MB per file
- Max age: 7 days
- Max backups: 3 files

## Security Considerations

### Production Recommendations
1. **Enable TLS** for OTLP endpoints
2. **Restrict ports** using firewall rules
3. **Use authentication** for Jaeger/Grafana
4. **Encrypt sensitive data** in traces
5. **Implement rate limiting** on receivers

### Remove Sensitive Data
Add to `otel-collector-config.yaml`:
```yaml
processors:
  attributes/privacy:
    actions:
      - key: http.request.header.authorization
        action: delete
      - key: user.email
        action: hash
```

## Maintenance

### Backup Jaeger Data
```powershell
docker run --rm -v observability_jaeger-data:/data -v ${PWD}:/backup alpine tar czf /backup/jaeger-backup.tar.gz -C /data .
```

### Clear Old Traces
```powershell
docker exec cardealer-jaeger /go/bin/jaeger-all-in-one --badger.maintenance-interval=1m
```

### Update Stack
```powershell
docker-compose -f docker-compose.observability.yml pull
docker-compose -f docker-compose.observability.yml up -d
```

### Stop Stack
```powershell
docker-compose -f docker-compose.observability.yml down
```

### Remove All Data
```powershell
docker-compose -f docker-compose.observability.yml down -v
```

## Integration with Existing Monitoring

### Link with Prometheus Alerts
The observability Prometheus runs on port 9091 to avoid conflicts with the existing monitoring stack (port 9090). You can:
1. Keep both stacks separate
2. Merge configurations
3. Use Grafana to query both

### Unified Grafana
Consider using one Grafana instance with multiple datasources:
- Prometheus (monitoring): http://prometheus:9090
- Prometheus (observability): http://prometheus-obs:9090
- Jaeger: http://jaeger:16686

## Next Steps

1. ✅ Generate traffic to services
2. ✅ View traces in Jaeger
3. ✅ Create custom Grafana dashboards
4. ✅ Set up trace-based alerts
5. ✅ Configure sampling strategies
6. ✅ Implement distributed context propagation
7. ✅ Add custom spans in code
8. ✅ Export to additional backends (if needed)

## Support Resources

- OpenTelemetry Docs: https://opentelemetry.io/docs/
- Jaeger Docs: https://www.jaegertracing.io/docs/
- Zipkin Docs: https://zipkin.io/
- OTel Collector: https://opentelemetry.io/docs/collector/

## Port Reference

| Service | Port | Purpose |
|---------|------|---------|
| OTel Collector | 4317 | OTLP gRPC |
| OTel Collector | 4318 | OTLP HTTP |
| OTel Collector | 8889 | Prometheus exporter |
| OTel Collector | 8888 | Collector metrics |
| OTel Collector | 13133 | Health check |
| OTel Collector | 55679 | zPages debug |
| Jaeger | 16686 | UI |
| Jaeger | 14250 | gRPC collector |
| Zipkin | 9411 | UI + API |
| Prometheus | 9091 | UI + API |
| Grafana | 3001 | UI |

---

**Status:** Production-ready  
**Last Updated:** December 2, 2025
