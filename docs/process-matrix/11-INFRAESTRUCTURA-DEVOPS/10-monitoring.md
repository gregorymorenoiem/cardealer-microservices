# 📊 Monitoring - Monitoreo y Observabilidad - Matriz de Procesos

> **Stack:** Prometheus + Grafana + Alertmanager  
> **Tracing:** OpenTelemetry + Jaeger  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente                   | Total | Implementado | Pendiente | Estado       |
| ---------------------------- | ----- | ------------ | --------- | ------------ |
| **MON-METRIC-\*** (Métricas) | 5     | 5            | 0         | ✅ 100%      |
| **MON-ALERT-\*** (Alertas)   | 4     | 4            | 0         | ✅ 100%      |
| **MON-DASH-\*** (Dashboards) | 5     | 5            | 0         | ✅ 100%      |
| **MON-TRACE-\*** (Tracing)   | 4     | 2            | 2         | 🟡 50%       |
| **MON-APM-\*** (APM)         | 3     | 0            | 3         | 🔴 Pendiente |
| **Tests**                    | 15    | 12           | 3         | 🟢 80%       |
| **TOTAL**                    | 36    | 28           | 8         | 🟢 78%       |

---

## 1. Información General

### 1.1 Descripción

Sistema de observabilidad completo que proporciona métricas, alertas, dashboards y tracing distribuido para todos los microservicios de OKLA. Permite detectar problemas proactivamente y diagnosticar incidentes.

### 1.2 Pilares de Observabilidad

| Pilar       | Herramienta          | Propósito                                  |
| ----------- | -------------------- | ------------------------------------------ |
| **Metrics** | Prometheus           | Métricas numéricas de sistema y aplicación |
| **Logs**    | Seq/Serilog          | Registros de eventos estructurados         |
| **Traces**  | Jaeger/OpenTelemetry | Tracing distribuido de requests            |
| **Alerts**  | Alertmanager         | Notificaciones de problemas                |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Observability Architecture                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Microservices (Instrumented)                                          │
│   ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐                  │
│   │  Auth    │ │ Vehicles │ │ Billing  │ │   ...    │                  │
│   │ /metrics │ │ /metrics │ │ /metrics │ │ /metrics │                  │
│   └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘                  │
│        │            │            │            │                         │
│        └────────────┴────────────┴────────────┘                        │
│                           │                                             │
│              ┌────────────┴────────────┐                               │
│              │                         │                               │
│              ▼                         ▼                               │
│        ┌───────────┐           ┌───────────────┐                       │
│        │Prometheus │           │  Jaeger       │                       │
│        │  :9090    │           │  :16686       │                       │
│        └─────┬─────┘           └───────────────┘                       │
│              │                                                          │
│      ┌───────┴───────┐                                                 │
│      ▼               ▼                                                 │
│ ┌─────────┐    ┌──────────────┐                                       │
│ │ Grafana │    │ Alertmanager │                                       │
│ │  :3000  │    │    :9093     │                                       │
│ └─────────┘    └──────┬───────┘                                       │
│                       │                                                │
│            ┌──────────┼──────────┐                                    │
│            ▼          ▼          ▼                                    │
│       ┌───────┐  ┌───────┐  ┌─────────┐                               │
│       │ Slack │  │PagerD.│  │  Email  │                               │
│       └───────┘  └───────┘  └─────────┘                               │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Prometheus

### 2.1 Configuración

```yaml
# prometheus.yml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

alerting:
  alertmanagers:
    - static_configs:
        - targets:
            - alertmanager:9093

rule_files:
  - /etc/prometheus/rules/*.yml

scrape_configs:
  # Prometheus self-monitoring
  - job_name: "prometheus"
    static_configs:
      - targets: ["localhost:9090"]

  # Kubernetes pods
  - job_name: "kubernetes-pods"
    kubernetes_sd_configs:
      - role: pod
        namespaces:
          names: ["okla"]
    relabel_configs:
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_scrape]
        action: keep
        regex: true
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_path]
        action: replace
        target_label: __metrics_path__
        regex: (.+)
      - source_labels:
          [__address__, __meta_kubernetes_pod_annotation_prometheus_io_port]
        action: replace
        regex: ([^:]+)(?::\d+)?;(\d+)
        replacement: $1:$2
        target_label: __address__

  # Node exporter
  - job_name: "node"
    kubernetes_sd_configs:
      - role: node
    relabel_configs:
      - source_labels: [__address__]
        regex: (.+):10250
        replacement: $1:9100
        target_label: __address__

  # PostgreSQL
  - job_name: "postgres"
    static_configs:
      - targets: ["postgres-exporter:9187"]

  # Redis
  - job_name: "redis"
    static_configs:
      - targets: ["redis-exporter:9121"]

  # RabbitMQ
  - job_name: "rabbitmq"
    static_configs:
      - targets: ["rabbitmq:15692"]
```

### 2.2 Métricas por Servicio

```csharp
// Program.cs - Configurar métricas
builder.Services.AddMetrics();
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddRuntimeInstrumentation();
        metrics.AddProcessInstrumentation();
        metrics.AddPrometheusExporter();
    });

app.MapPrometheusScrapingEndpoint("/metrics");
```

### 2.3 Custom Metrics

```csharp
public class VehicleMetrics
{
    private readonly Counter<long> _vehiclesCreated;
    private readonly Counter<long> _vehiclesSold;
    private readonly Histogram<double> _searchLatency;
    private readonly ObservableGauge<int> _activeVehicles;

    public VehicleMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("VehiclesSaleService");

        _vehiclesCreated = meter.CreateCounter<long>(
            "vehicles_created_total",
            unit: "vehicles",
            description: "Total vehicles created");

        _vehiclesSold = meter.CreateCounter<long>(
            "vehicles_sold_total",
            unit: "vehicles",
            description: "Total vehicles sold");

        _searchLatency = meter.CreateHistogram<double>(
            "vehicle_search_duration_seconds",
            unit: "s",
            description: "Vehicle search latency");
    }

    public void RecordVehicleCreated(string make, string dealerTier)
    {
        _vehiclesCreated.Add(1,
            new KeyValuePair<string, object?>("make", make),
            new KeyValuePair<string, object?>("dealer_tier", dealerTier));
    }

    public void RecordSearchLatency(double seconds, string searchType)
    {
        _searchLatency.Record(seconds,
            new KeyValuePair<string, object?>("search_type", searchType));
    }
}
```

---

## 3. Grafana Dashboards

### 3.1 Platform Overview Dashboard

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    OKLA Platform Overview                                │
│                    Last 24 hours                                         │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │   Requests   │  │   Errors     │  │  Avg Latency │  │   Uptime     │ │
│  │   1.2M/day   │  │   0.05%      │  │    45ms      │  │   99.99%     │ │
│  │    ↑12%      │  │    ↓20%      │  │    ↓15%      │  │    ━         │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘ │
│                                                                          │
│  Request Rate (per service)                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │     ▄                                                                ││
│  │    ███                                                               ││
│  │   █████      ▄▄▄                                                     ││
│  │  ███████    █████     ▄▄                                             ││
│  │ █████████  ███████  ████  ▄▄▄▄                                      ││
│  │ Gateway   Vehicles  Auth  Billing  Media  Notify                    ││
│  └─────────────────────────────────────────────────────────────────────┘│
│                                                                          │
│  Error Rate                               P95 Latency                   │
│  ┌────────────────────────────────┐     ┌───────────────────────────┐  │
│  │  ▁▁▂▁▁▁▁▁▁▁▁▃▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁ │     │  ▂▃▂▂▂▃▂▂▂▂▅▂▂▂▂▂▂▂▂▂▂▂ │  │
│  │  0.1%           target: <1%    │     │  75ms     target: <200ms  │  │
│  └────────────────────────────────┘     └───────────────────────────┘  │
│                                                                          │
│  Service Health Matrix                                                  │
│  ┌─────────────────────────────────────────────────────────────────────┐│
│  │ Service             │ Status │ CPU  │ Memory │ Replicas │ Errors   ││
│  ├─────────────────────┼────────┼──────┼────────┼──────────┼──────────┤│
│  │ gateway             │ 🟢     │ 15%  │ 256MB  │ 2/2      │ 0        ││
│  │ vehiclessaleservice │ 🟢     │ 35%  │ 512MB  │ 2/2      │ 2        ││
│  │ authservice         │ 🟢     │ 10%  │ 192MB  │ 2/2      │ 0        ││
│  │ billingservice      │ 🟡     │ 45%  │ 384MB  │ 2/2      │ 5        ││
│  │ mediaservice        │ 🟢     │ 25%  │ 320MB  │ 2/2      │ 1        ││
│  │ notificationservice │ 🟢     │ 8%   │ 180MB  │ 2/2      │ 0        ││
│  └─────────────────────────────────────────────────────────────────────┘│
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Business Metrics Dashboard

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    OKLA Business Metrics                                 │
│                    Real-time                                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Today's Summary                                                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │  New Vehicles│  │  Leads       │  │  Payments    │  │  Revenue     │ │
│  │     45       │  │     128      │  │     23       │  │  $12,450     │ │
│  │    ↑20%      │  │    ↑35%      │  │    ↑10%      │  │    ↑25%      │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘ │
│                                                                          │
│  Vehicles by Status                    Leads Funnel                     │
│  ┌────────────────────────────────┐   ┌────────────────────────────────┐│
│  │ 🟢 Active      ████████ 1,234  │   │ New         ████████████ 500   ││
│  │ 🟡 Pending     ██       156    │   │ Contacted   ████████    320   ││
│  │ 🔴 Sold        █████    789    │   │ Qualified   ██████      240   ││
│  │ ⚫ Inactive    ███      345    │   │ Closed      ████        160   ││
│  └────────────────────────────────┘   └────────────────────────────────┘│
│                                                                          │
│  Revenue by Payment Gateway           Active Subscriptions             │
│  ┌────────────────────────────────┐   ┌────────────────────────────────┐│
│  │           ┌─────────┐          │   │ Starter    ██████      45     ││
│  │       ┌───┤  Azul   ├───┐      │   │ Pro        ████████████ 89    ││
│  │       │   │  65%    │   │      │   │ Enterprise ████         28    ││
│  │       │   └─────────┘   │      │   │                               ││
│  │  ┌────┴────┐       ┌────┴────┐ │   │ MRR: $18,540                  ││
│  │  │ Stripe  │       │  Other  │ │   │ Churn: 2.1%                   ││
│  │  │  32%    │       │   3%    │ │   │                               ││
│  │  └─────────┘       └─────────┘ │   └────────────────────────────────┘│
│  └────────────────────────────────┘                                     │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 3.3 Infrastructure Dashboard

```json
{
  "dashboard": {
    "title": "OKLA Infrastructure",
    "panels": [
      {
        "title": "CPU Usage by Pod",
        "type": "timeseries",
        "targets": [
          {
            "expr": "sum(rate(container_cpu_usage_seconds_total{namespace=\"okla\"}[5m])) by (pod)"
          }
        ]
      },
      {
        "title": "Memory Usage by Pod",
        "type": "timeseries",
        "targets": [
          {
            "expr": "sum(container_memory_working_set_bytes{namespace=\"okla\"}) by (pod)"
          }
        ]
      },
      {
        "title": "Network I/O",
        "type": "timeseries",
        "targets": [
          {
            "expr": "sum(rate(container_network_receive_bytes_total{namespace=\"okla\"}[5m])) by (pod)"
          }
        ]
      },
      {
        "title": "Database Connections",
        "type": "gauge",
        "targets": [
          {
            "expr": "pg_stat_activity_count{datname=~\".*service.*\"}"
          }
        ]
      }
    ]
  }
}
```

---

## 4. Alerting

### 4.1 Alert Rules

```yaml
# prometheus-rules.yml
groups:
  - name: okla-service-alerts
    rules:
      # High error rate
      - alert: HighErrorRate
        expr: |
          sum(rate(http_requests_total{status=~"5.."}[5m])) by (service)
          /
          sum(rate(http_requests_total[5m])) by (service)
          > 0.01
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "High error rate on {{ $labels.service }}"
          description: "Error rate is {{ $value | humanizePercentage }}"

      # High latency
      - alert: HighLatency
        expr: |
          histogram_quantile(0.95, 
            sum(rate(http_request_duration_seconds_bucket[5m])) by (le, service)
          ) > 0.5
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High latency on {{ $labels.service }}"
          description: "P95 latency is {{ $value | humanizeDuration }}"

      # Service down
      - alert: ServiceDown
        expr: up{job=~".*service.*"} == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Service {{ $labels.job }} is down"

      # Pod not ready
      - alert: PodNotReady
        expr: |
          kube_pod_status_ready{namespace="okla", condition="true"} == 0
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Pod {{ $labels.pod }} not ready"

  - name: okla-database-alerts
    rules:
      - alert: DatabaseConnectionsHigh
        expr: pg_stat_activity_count > 80
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High database connections ({{ $value }})"

      - alert: DatabaseSlowQueries
        expr: rate(pg_stat_statements_seconds_total[5m]) > 1
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: "Slow database queries detected"

  - name: okla-business-alerts
    rules:
      - alert: NoPaymentsReceived
        expr: |
          sum(increase(payments_processed_total[1h])) == 0
          and hour() >= 8 and hour() <= 22
        for: 1h
        labels:
          severity: warning
        annotations:
          summary: "No payments received in the last hour during business hours"

      - alert: LeadConversionLow
        expr: |
          sum(rate(leads_converted_total[24h]))
          /
          sum(rate(leads_created_total[24h]))
          < 0.1
        for: 24h
        labels:
          severity: info
        annotations:
          summary: "Lead conversion rate is below 10%"
```

### 4.2 Alertmanager Configuration

```yaml
# alertmanager.yml
global:
  resolve_timeout: 5m
  slack_api_url: "${SLACK_WEBHOOK_URL}"

route:
  receiver: "slack-notifications"
  group_by: ["alertname", "service"]
  group_wait: 30s
  group_interval: 5m
  repeat_interval: 4h
  routes:
    - match:
        severity: critical
      receiver: "pagerduty-critical"
      continue: true
    - match:
        severity: critical
      receiver: "slack-critical"
    - match:
        severity: warning
      receiver: "slack-warnings"

receivers:
  - name: "slack-notifications"
    slack_configs:
      - channel: "#okla-alerts"
        send_resolved: true
        title: "{{ .Status | toUpper }}: {{ .CommonAnnotations.summary }}"
        text: "{{ .CommonAnnotations.description }}"

  - name: "slack-critical"
    slack_configs:
      - channel: "#okla-critical"
        send_resolved: true
        color: '{{ if eq .Status "firing" }}danger{{ else }}good{{ end }}'

  - name: "slack-warnings"
    slack_configs:
      - channel: "#okla-warnings"
        send_resolved: true

  - name: "pagerduty-critical"
    pagerduty_configs:
      - service_key: "${PAGERDUTY_SERVICE_KEY}"
        severity: critical

inhibit_rules:
  - source_match:
      severity: "critical"
    target_match:
      severity: "warning"
    equal: ["alertname", "service"]
```

---

## 5. Distributed Tracing

### 5.1 OpenTelemetry Configuration

```csharp
// Program.cs
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "VehiclesSaleService", serviceVersion: "1.0.0")
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName
        }))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = httpContext =>
                !httpContext.Request.Path.StartsWithSegments("/health");
        })
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddSource("VehiclesSaleService")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://jaeger:4317");
        }));
```

### 5.2 Custom Spans

```csharp
public class VehicleSearchService
{
    private static readonly ActivitySource ActivitySource = new("VehiclesSaleService");

    public async Task<SearchResult> SearchAsync(SearchQuery query)
    {
        using var activity = ActivitySource.StartActivity("SearchVehicles");
        activity?.SetTag("search.filters_count", query.Filters.Count);
        activity?.SetTag("search.page", query.Page);
        activity?.SetTag("search.page_size", query.PageSize);

        try
        {
            // Build query
            using (var buildActivity = ActivitySource.StartActivity("BuildQuery"))
            {
                var esQuery = BuildElasticsearchQuery(query);
                buildActivity?.SetTag("query.complexity", esQuery.Complexity);
            }

            // Execute search
            using (var execActivity = ActivitySource.StartActivity("ExecuteSearch"))
            {
                var results = await _elasticClient.SearchAsync(esQuery);
                execActivity?.SetTag("results.count", results.Total);
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            return results;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
```

### 5.3 Trace Visualization

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Jaeger Trace View                                     │
│                    Trace ID: abc123def456                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Total Duration: 245ms                                                  │
│  Services: 4 | Depth: 3                                                 │
│                                                                          │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Gateway.Api                                                        │  │
│  │ GET /api/vehicles/search                          [━━━━━━━━━━] 245ms│  │
│  │                                                                    │  │
│  │   └─ VehiclesSaleService.Api                                      │  │
│  │      SearchVehicles                              [━━━━━━━━━] 180ms │  │
│  │                                                                    │  │
│  │        ├─ BuildQuery                             [━━] 15ms        │  │
│  │        │                                                          │  │
│  │        ├─ Elasticsearch                                           │  │
│  │        │  SearchAsync                            [━━━━━━] 120ms   │  │
│  │        │                                                          │  │
│  │        └─ Redis                                                   │  │
│  │           CacheResults                           [━] 8ms          │  │
│  │                                                                    │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  Span Details: VehiclesSaleService.Api - SearchVehicles                │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ Tags:                                                              │  │
│  │   search.filters_count: 5                                         │  │
│  │   search.page: 1                                                  │  │
│  │   search.page_size: 20                                            │  │
│  │   results.count: 145                                              │  │
│  │   http.status_code: 200                                           │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 6. SLIs/SLOs

### 6.1 Service Level Indicators

| SLI              | Definición             | Query                                                                                    |
| ---------------- | ---------------------- | ---------------------------------------------------------------------------------------- |
| **Availability** | % de requests exitosos | `sum(rate(http_requests_total{status!~"5.."}[5m])) / sum(rate(http_requests_total[5m]))` |
| **Latency P95**  | Latencia percentil 95  | `histogram_quantile(0.95, sum(rate(http_request_duration_seconds_bucket[5m])) by (le))`  |
| **Error Rate**   | % de errores 5xx       | `sum(rate(http_requests_total{status=~"5.."}[5m])) / sum(rate(http_requests_total[5m]))` |
| **Throughput**   | Requests por segundo   | `sum(rate(http_requests_total[5m]))`                                                     |

### 6.2 Service Level Objectives

| Servicio     | SLO Availability | SLO Latency P95 | SLO Error Rate |
| ------------ | ---------------- | --------------- | -------------- |
| **Gateway**  | 99.9%            | < 100ms         | < 0.1%         |
| **Auth**     | 99.9%            | < 200ms         | < 0.1%         |
| **Vehicles** | 99.5%            | < 500ms         | < 0.5%         |
| **Billing**  | 99.9%            | < 300ms         | < 0.1%         |
| **Media**    | 99.0%            | < 1s            | < 1%           |

### 6.3 SLO Alerting

```yaml
- alert: SLOBudgetBurning
  expr: |
    (
      sum(rate(http_requests_total{status=~"5.."}[1h])) 
      / 
      sum(rate(http_requests_total[1h]))
    ) > (1 - 0.999) * 10
  for: 5m
  labels:
    severity: warning
  annotations:
    summary: "SLO error budget burning too fast"
    description: "At current rate, monthly SLO budget will be exhausted in {{ $value }} hours"
```

---

## 7. Kubernetes Deployment

### 7.1 Prometheus Stack

```yaml
# Usando kube-prometheus-stack
apiVersion: helm.cattle.io/v1
kind: HelmChart
metadata:
  name: prometheus
  namespace: monitoring
spec:
  chart: kube-prometheus-stack
  repo: https://prometheus-community.github.io/helm-charts
  targetNamespace: monitoring
  valuesContent: |-
    prometheus:
      prometheusSpec:
        retention: 15d
        storageSpec:
          volumeClaimTemplate:
            spec:
              accessModes: ["ReadWriteOnce"]
              resources:
                requests:
                  storage: 50Gi
    grafana:
      adminPassword: ${GRAFANA_ADMIN_PASSWORD}
      persistence:
        enabled: true
        size: 10Gi
    alertmanager:
      alertmanagerSpec:
        storage:
          volumeClaimTemplate:
            spec:
              accessModes: ["ReadWriteOnce"]
              resources:
                requests:
                  storage: 5Gi
```

---

## 8. On-Call Runbooks

### 8.1 High Error Rate

```markdown
## Runbook: High Error Rate

### Síntomas

- Alerta: HighErrorRate
- Dashboard muestra spike en errores

### Diagnóstico

1. Verificar qué servicio tiene errores:
```

kubectl logs -f deployment/<service> -n okla | grep -i error

```

2. Verificar dependencias:
```

kubectl exec -it deployment/<service> -n okla -- curl -s http://postgres:5432 || echo "DB unreachable"

```

3. Verificar recursos:
```

kubectl top pods -n okla

```

### Mitigación
1. Si es por carga: Escalar replicas
```

kubectl scale deployment/<service> --replicas=4 -n okla

```

2. Si es por dependencia: Verificar y reiniciar dependencia

3. Si es por bug: Rollback
```

kubectl rollout undo deployment/<service> -n okla

```

### Escalación
- Si no se resuelve en 15 min → Escalar a SRE Lead
- Si afecta pagos → Escalar a CTO
```

---

## 📚 Referencias

- [Prometheus Documentation](https://prometheus.io/docs/) - Documentación oficial
- [Grafana Documentation](https://grafana.com/docs/) - Dashboards
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/) - Tracing
- [04-health-checks.md](04-health-checks.md) - Health checks
- [05-logging-service.md](05-logging-service.md) - Logging
