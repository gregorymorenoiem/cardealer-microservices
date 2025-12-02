# Guía de Uso de Telemetría

## Stack de Observabilidad

El proyecto utiliza OpenTelemetry Collector como hub central para procesar y distribuir telemetría a múltiples backends:

- **OpenTelemetry Collector**: Hub central de procesamiento
- **Jaeger**: Distributed tracing (UI principal)
- **Zipkin**: Tracing alternativo (compatible)
- **Prometheus**: Métricas y alertas
- **Grafana**: Visualización de métricas

## URLs de Acceso

| Servicio | URL | Descripción |
|----------|-----|-------------|
| Jaeger UI | http://localhost:16686 | Visualización de traces |
| Zipkin UI | http://localhost:9411 | Traces alternativos |
| Prometheus | http://localhost:9091 | Métricas y queries |
| Grafana | http://localhost:3001 | Dashboards (admin/admin123) |
| OTel Health | http://localhost:13133 | Health check del collector |

## Gestión del Stack

### Iniciar Observabilidad
```powershell
cd backend/observability
.\start-observability.ps1
```

### Detener Observabilidad
```powershell
.\stop-observability.ps1
```

### Verificar Estado
```powershell
.\test-observability.ps1
```

### Enviar Trazas de Prueba
```powershell
.\test-otlp-direct.ps1
```

## Configuración de Servicios

Los servicios están configurados para enviar telemetría automáticamente al OTel Collector:

```json
{
  "OpenTelemetry": {
    "Exporter": {
      "Otlp": {
        "Endpoint": "http://otel-collector:4318",
        "Protocol": "HttpProtobuf"
      }
    },
    "ServiceName": "AuthService",
    "Resources": {
      "service.name": "AuthService",
      "service.namespace": "cardealer",
      "service.version": "1.0.0",
      "deployment.environment": "development"
    },
    "Tracing": {
      "Enabled": true,
      "SamplingRatio": 0.1,
      "ExportIntervalMilliseconds": 5000
    },
    "Metrics": {
      "Enabled": true,
      "ExportIntervalMilliseconds": 60000
    }
  }
}
```

## Visualización de Traces en Jaeger

### 1. Buscar Traces
1. Abrir http://localhost:16686
2. Seleccionar servicio del dropdown (AuthService, UserService, etc.)
3. Ajustar rango de tiempo (últimos 15 minutos)
4. Click en "Find Traces"

### 2. Inspeccionar Trace
- **Timeline**: Duración total y spans
- **Span Details**: Tags, logs, eventos
- **Service Graph**: Dependencias entre servicios

### 3. Filtros Útiles
- **Tags**: `http.status_code=500` (errores)
- **Min Duration**: `100ms` (operaciones lentas)
- **Max Duration**: `5s` (timeouts)
- **Limit Results**: 20 (por defecto)

## Sampling y Volumen

### Configuración Actual
- **Sampling**: 10% de traces
- **Batch Size**: 1024 spans
- **Batch Timeout**: 10s
- **Memory Limit**: 512MB

### Para Testing (Temporal)
Si necesitas ver TODAS las trazas durante testing:

1. Editar `otel-collector-config.yaml`:
```yaml
probabilistic_sampler:
  sampling_percentage: 100.0  # Cambiar de 10.0 a 100.0
```

2. Reiniciar collector:
```powershell
docker restart cardealer-otel-collector
```

3. **¡IMPORTANTE!** Restaurar a 10% después de testing para evitar sobrecarga.

## Métricas del Collector

### Health Check
```powershell
Invoke-WebRequest http://localhost:13133/health
```

### Métricas Internas
```powershell
Invoke-WebRequest http://localhost:8888/metrics
```

Métricas clave:
- `otelcol_receiver_accepted_spans` - Spans recibidos
- `otelcol_exporter_sent_spans` - Spans exportados
- `otelcol_processor_dropped_spans` - Spans descartados
- `otelcol_processor_batch_batch_send_size` - Tamaño de batches

## Troubleshooting

### No aparecen traces en Jaeger

1. Verificar que el servicio está enviando:
```powershell
docker logs <service-container> --tail 50
```

2. Verificar OTel Collector:
```powershell
docker logs cardealer-otel-collector --tail 50
```

3. Verificar métricas:
```powershell
curl http://localhost:8888/metrics | Select-String "accepted_spans"
```

### Sampling descarta todas las trazas

Si ves `otelcol_receiver_accepted_spans > 0` pero `otelcol_exporter_sent_spans = 0`:

- El sampling está descartando todas las trazas
- Temporalmente aumentar `sampling_percentage` a 100% para testing
- O agregar tags de alta prioridad: `sampling.priority=1`

### OTel Collector unhealthy

```powershell
# Ver logs
docker logs cardealer-otel-collector --tail 100

# Verificar configuración
docker exec cardealer-otel-collector /bin/sh -c "cat /etc/otel-collector-config.yaml"

# Reiniciar
docker restart cardealer-otel-collector
```

### Servicios no aparecen en Jaeger

1. Verificar que el servicio está configurado con OpenTelemetry
2. Verificar endpoint: `http://otel-collector:4318`
3. Verificar que el contenedor puede resolver `otel-collector`
4. Verificar sampling no está descartando todo

## Prometheus Queries Útiles

Acceder a http://localhost:9091 y ejecutar:

```promql
# Tasa de requests por servicio
rate(http_requests_total[5m])

# P95 latency
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Error rate
rate(http_requests_total{status=~"5.."}[5m])

# Spans procesados por el collector
rate(otelcol_receiver_accepted_spans[1m])
```

## Integración con .NET

Los servicios usan los siguientes paquetes:

```xml
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.7.0" />
```

### Agregar Telemetría a un Nuevo Servicio

1. Instalar paquetes NuGet
2. Agregar configuración en `appsettings.json` (ver ejemplo arriba)
3. Configurar en `Program.cs`:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otlpEndpoint);
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
    });
```

## Arquitectura del Flujo

```
┌─────────────┐       OTLP        ┌──────────────────┐
│  Services   │ ───────────────▶  │  OTel Collector  │
│ (port 4318) │   HTTP/gRPC       │  (Hub Central)   │
└─────────────┘                   └──────────────────┘
                                           │
                    ┌──────────────────────┼──────────────────────┐
                    │                      │                      │
                    ▼                      ▼                      ▼
              ┌──────────┐          ┌───────────┐         ┌──────────┐
              │  Jaeger  │          │Prometheus │         │  Zipkin  │
              │ (16686)  │          │  (9091)   │         │  (9411)  │
              └──────────┘          └───────────┘         └──────────┘
                                           │
                                           ▼
                                    ┌──────────┐
                                    │ Grafana  │
                                    │  (3001)  │
                                    └──────────┘
```

## Próximos Pasos

1. ✅ Stack de observabilidad funcionando
2. ✅ 8 servicios configurados para telemetría
3. ⏳ Generar tráfico real con servicios en ejecución
4. ⏳ Crear dashboards en Grafana
5. ⏳ Configurar alertas basadas en traces
6. ⏳ Implementar correlation IDs entre servicios

## Referencias

- [OpenTelemetry Docs](https://opentelemetry.io/docs/)
- [Jaeger Docs](https://www.jaegertracing.io/docs/)
- [OTel Collector Config](https://opentelemetry.io/docs/collector/configuration/)
- [.NET OTel Instrumentation](https://github.com/open-telemetry/opentelemetry-dotnet)
