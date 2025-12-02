# Monitoring Stack - Quick Start Guide
# CarDealer Microservices Monitoring with Prometheus, Grafana, and Alertmanager

## Overview
This monitoring stack provides comprehensive observability for the CarDealer microservices platform:
- **Prometheus**: Metrics collection, storage, and alerting
- **Grafana**: Visualization dashboards
- **Alertmanager**: Alert routing and notifications
- **Node Exporter**: Server/host metrics
- **PostgreSQL Exporter**: Database metrics

## Architecture
```
Microservices → Prometheus → Grafana (Visualization)
                    ↓
               Alertmanager → Email/Slack Notifications
```

## Prerequisites
1. Docker and Docker Compose installed
2. CarDealer network created: `docker network create cardealer-network`
3. All microservices running and exposing `/metrics` endpoint

## Quick Start

### 1. Start the Monitoring Stack
```powershell
cd c:\Users\gmoreno\source\repos\cardealer\backend\monitoring
docker-compose -f docker-compose.monitoring.yml up -d
```

### 2. Verify Services are Running
```powershell
docker-compose -f docker-compose.monitoring.yml ps
```

Expected output:
- cardealer-prometheus: Up (port 9090)
- cardealer-alertmanager: Up (port 9093)
- cardealer-grafana: Up (port 3000)
- cardealer-node-exporter: Up (port 9100)
- cardealer-postgres-exporter: Up (port 9187)

### 3. Access Web UIs

**Prometheus**
- URL: http://localhost:9090
- Features:
  - Query metrics: Go to Status → Targets
  - View alerts: Go to Alerts
  - Execute PromQL queries

**Alertmanager**
- URL: http://localhost:9093
- Features:
  - View active alerts
  - Configure silences
  - Test alert routing

**Grafana**
- URL: http://localhost:3000
- Default credentials: admin / admin123
- First login: Add Prometheus datasource
  - URL: http://prometheus:9090
  - Access: Server (default)

## Configuration Files

### Alert Rules
Located in `prometheus/rules/`:
- `audit-alerts.yml` - AuditService alerts (10 rules)
- `media-alerts.yml` - MediaService alerts (10 rules)
- `notification-alerts.yml` - NotificationService alerts (10 rules)
- `gateway-alerts.yml` - Gateway alerts (5 rules)
- `auth-alerts.yml` - AuthService alerts (10 rules)
- `error-alerts.yml` - ErrorService alerts (10 rules)
- `user-alerts.yml` - UserService alerts (10 rules)
- `role-alerts.yml` - RoleService alerts (10 rules)

**Total: 75 alert rules** across 8 services

### Alert Severity Levels
- **Critical**: Immediate action required (circuit breaker open, high error rates)
- **Warning**: Attention needed (high memory, slow queries)

### Example Alerts
```yaml
# High Error Rate Alert
- alert: HighAuditQueryErrorRate
  expr: rate(http_requests_failed_total{service="auditservice"}[5m]) > 0.1
  for: 5m
  labels:
    severity: critical
  annotations:
    summary: "High error rate in AuditService"
    description: "Error rate is {{ $value }} requests/sec"
```

## Alertmanager Configuration

### Email Notifications
Edit `prometheus/alertmanager.yml`:
```yaml
global:
  smtp_smarthost: 'smtp.gmail.com:587'
  smtp_from: 'alerts@cardealer.com'
  smtp_auth_username: 'your-email@gmail.com'
  smtp_auth_password: 'your-app-password'
```

### Slack Integration (Optional)
Add to receivers in `alertmanager.yml`:
```yaml
slack_configs:
  - api_url: 'https://hooks.slack.com/services/YOUR/WEBHOOK/URL'
    channel: '#alerts'
    title: '[{{ .Status | toUpper }}] {{ .GroupLabels.alertname }}'
```

After changes, reload configuration:
```powershell
curl -X POST http://localhost:9093/-/reload
```

## Prometheus Configuration

### Adding New Service
Edit `prometheus/prometheus.yml`:
```yaml
scrape_configs:
  - job_name: 'newservice'
    static_configs:
      - targets: ['newservice:80']
        labels:
          service: 'newservice'
          tier: 'backend'
```

Reload Prometheus:
```powershell
curl -X POST http://localhost:9090/-/reload
```

## Grafana Dashboards

### Import Pre-built Dashboards
1. Go to Grafana: http://localhost:3000
2. Click + → Import
3. Use dashboard IDs:
   - **3662**: Prometheus 2.0 Stats
   - **1860**: Node Exporter Full
   - **9628**: PostgreSQL Database
   - **4701**: JVM Metrics

### Custom Dashboard
Create dashboard for CarDealer metrics:
- Service health (up/down status)
- Request rate per service
- Error rate by service
- Response time P95/P99
- Circuit breaker status
- Database connection pool

## Troubleshooting

### Prometheus Not Scraping Targets
1. Check target status: http://localhost:9090/targets
2. Verify network connectivity:
   ```powershell
   docker exec cardealer-prometheus wget -O- http://auditservice/metrics
   ```
3. Ensure services are on same network:
   ```powershell
   docker network inspect cardealer-network
   ```

### Alerts Not Firing
1. Check Prometheus alerts: http://localhost:9090/alerts
2. Verify alert rules loaded:
   ```powershell
   docker exec cardealer-prometheus promtool check rules /etc/prometheus/rules/*.yml
   ```
3. Check Alertmanager logs:
   ```powershell
   docker logs cardealer-alertmanager
   ```

### Grafana Datasource Error
1. Verify Prometheus URL in datasource: `http://prometheus:9090`
2. Test connection from Grafana container:
   ```powershell
   docker exec cardealer-grafana wget -O- http://prometheus:9090/api/v1/status/config
   ```

## Maintenance

### View Logs
```powershell
# Prometheus
docker logs cardealer-prometheus -f

# Alertmanager
docker logs cardealer-alertmanager -f

# Grafana
docker logs cardealer-grafana -f
```

### Backup Configuration
```powershell
# Backup Prometheus data
docker run --rm -v monitoring_prometheus-data:/data -v ${PWD}:/backup alpine tar czf /backup/prometheus-backup.tar.gz -C /data .

# Backup Grafana data
docker run --rm -v monitoring_grafana-data:/data -v ${PWD}:/backup alpine tar czf /backup/grafana-backup.tar.gz -C /data .
```

### Update Services
```powershell
docker-compose -f docker-compose.monitoring.yml pull
docker-compose -f docker-compose.monitoring.yml up -d
```

### Stop Monitoring Stack
```powershell
docker-compose -f docker-compose.monitoring.yml down
```

### Clean Up (Remove Data)
```powershell
docker-compose -f docker-compose.monitoring.yml down -v
```

## Performance Tuning

### Prometheus Storage
- Default retention: 30 days
- Storage location: `/prometheus` volume
- Estimated size: ~1-5GB for 8 services

Adjust retention in docker-compose:
```yaml
command:
  - '--storage.tsdb.retention.time=90d'  # Change to 90 days
```

### Query Performance
- Use recording rules for expensive queries
- Limit time range in Grafana dashboards
- Use appropriate scrape intervals (15s default)

## Security Best Practices

1. **Change default Grafana password** after first login
2. **Use HTTPS** in production (reverse proxy required)
3. **Restrict network access** to monitoring ports
4. **Use secrets** for SMTP/Slack credentials
5. **Enable authentication** on Prometheus/Alertmanager

## Next Steps

1. ✅ Configure email notifications in Alertmanager
2. ✅ Import Grafana dashboards
3. ✅ Test alert firing by triggering conditions
4. ✅ Set up Slack integration (optional)
5. ✅ Create custom dashboards for business metrics
6. ✅ Configure PostgreSQL Exporter for all databases
7. ✅ Implement OpenTelemetry Collector integration

## Metrics Exposed by Services

All microservices expose the following metrics at `/metrics`:
- `http_requests_total` - Total HTTP requests
- `http_requests_failed_total` - Failed HTTP requests
- `http_request_duration_seconds` - Request duration histogram
- `circuit_breaker_state` - Circuit breaker state (0=closed, 1=open)
- `database_connections_active` - Active database connections
- `message_queue_messages_processed` - Messages processed (RabbitMQ)

## Support

For issues or questions:
- Check logs: `docker logs <container-name>`
- Verify configuration: `promtool check config prometheus.yml`
- Test alerts: `amtool check-config alertmanager.yml`
- Review Grafana datasource connectivity

## Resources

- Prometheus Documentation: https://prometheus.io/docs/
- Grafana Dashboards: https://grafana.com/grafana/dashboards/
- Alertmanager Guide: https://prometheus.io/docs/alerting/latest/alertmanager/
- PromQL Tutorial: https://prometheus.io/docs/prometheus/latest/querying/basics/
