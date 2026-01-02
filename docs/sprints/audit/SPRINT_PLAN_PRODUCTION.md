# 🚀 Sprint Plan - Producción 100%

**Proyecto**: CarDealer Microservices - Production Readiness  
**Fecha Inicio**: Diciembre 3, 2025  
**Estado Actual**: 70% Production Ready  
**Objetivo**: 100% Production Ready

---

## 📊 Resumen Ejecutivo

### Estado Actual
- ✅ **Desarrollo**: 100% (26 servicios compilando, 227 tests passing)
- ✅ **Dockerfiles**: 100% (24/24 servicios)
- ✅ **Documentación**: 100% (13 servicios documentados)
- ❌ **Runtime Validation**: 0% (Docker no iniciado)
- ❌ **DevOps/CI/CD**: 0%
- ❌ **Monitoreo**: 0%

### Progreso General: **70% → 100%**

---

# 🎯 SPRINT 1: Runtime Validation & Critical Setup
**Duración**: 3-5 días  
**Objetivo**: Validar que todo funciona en runtime y configurar secretos  
**Prioridad**: 🔴 CRÍTICA - BLOQUEANTE PARA PRODUCCIÓN

## User Stories

### 🔴 US-1.1: Docker Build Verification
**Prioridad**: CRÍTICA  
**Estimación**: 4 horas  
**Asignado a**: DevOps Team

**Criterios de Aceptación**:
- [ ] Docker Desktop iniciado y funcionando
- [ ] Todas las 24 imágenes construidas sin errores
- [ ] Tamaño de imágenes verificado (< 500MB cada una)
- [ ] Logs de build revisados, sin warnings críticos

**Tareas**:
```powershell
# 1. Iniciar Docker Desktop
# 2. Limpiar imágenes antiguas
docker system prune -a --volumes -f

# 3. Construir todas las imágenes
cd backend
docker-compose build --no-cache

# 4. Verificar imágenes creadas
docker images | Select-String "cardealer"

# 5. Verificar tamaños
docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}"
```

**Definición de Hecho (DoD)**:
- ✅ 24/24 imágenes construidas exitosamente
- ✅ Documentación de errores (si los hay) creada
- ✅ Build time registrado para baseline

---

### 🔴 US-1.2: Infrastructure Services Startup
**Prioridad**: CRÍTICA  
**Estimación**: 2 horas  
**Asignado a**: DevOps Team  
**Depende de**: US-1.1

**Criterios de Aceptación**:
- [ ] Consul iniciado y saludable (port 8500)
- [ ] Redis iniciado y saludable (port 6379)
- [ ] RabbitMQ iniciado y saludable (ports 5672, 15672)
- [ ] PostgreSQL instances iniciadas (errorservice-db, authservice-db, etc.)

**Tareas**:
```powershell
# 1. Iniciar servicios de infraestructura
docker-compose up -d consul redis rabbitmq

# 2. Iniciar bases de datos
docker-compose up -d errorservice-db authservice-db auditservice-db

# 3. Esperar healthy status
Start-Sleep -Seconds 30

# 4. Verificar status
docker ps --filter "status=running"
docker ps --filter "health=healthy"

# 5. Verificar Consul UI
Start-Process "http://localhost:8500/ui"

# 6. Verificar RabbitMQ Management
Start-Process "http://localhost:15672"  # guest/guest
```

**Definición de Hecho (DoD)**:
- ✅ Todos los servicios infrastructure en estado "healthy"
- ✅ Consul UI accesible
- ✅ RabbitMQ Management UI accesible
- ✅ Bases de datos aceptando conexiones

---

### 🔴 US-1.3: Core Services Deployment
**Prioridad**: CRÍTICA  
**Estimación**: 4 horas  
**Asignado a**: Backend Team  
**Depende de**: US-1.2

**Criterios de Aceptación**:
- [ ] Gateway iniciado y saludable (port 5000)
- [ ] ServiceDiscovery iniciado y registrado en Consul
- [ ] HealthCheckService iniciado
- [ ] AuthService iniciado (port 5085)
- [ ] UserService iniciado (port 5001)
- [ ] RoleService iniciado (port 5002)
- [ ] VehicleService iniciado (port 5009)
- [ ] ContactService iniciado (port 5007)

**Tareas**:
```powershell
# 1. Iniciar Gateway
docker-compose up -d gateway

# 2. Verificar Gateway health
curl http://localhost:5000/health

# 3. Iniciar servicios core progresivamente
docker-compose up -d serviceregistry healthcheckservice
docker-compose up -d authservice roleservice userservice
docker-compose up -d vehicleservice contactservice

# 4. Monitorear logs
docker-compose logs -f --tail=50 gateway authservice vehicleservice

# 5. Verificar registro en Consul
curl http://localhost:8500/v1/catalog/services
```

**Definición de Hecho (DoD)**:
- ✅ 8 servicios core funcionando
- ✅ Todos los servicios registrados en Consul
- ✅ Health endpoints respondiendo HTTP 200
- ✅ Logs sin errores críticos

---

### 🔴 US-1.4: Health Endpoints Runtime Validation
**Prioridad**: CRÍTICA  
**Estimación**: 6 horas  
**Asignado a**: QA Team  
**Depende de**: US-1.3

**Criterios de Aceptación**:
- [ ] 24/24 servicios con `/health` respondiendo HTTP 200
- [ ] **5 servicios específicos verificados**:
  - ConfigurationService
  - AdminService
  - CacheService
  - ErrorService
  - MessageBusService
- [ ] Endpoints `/health/ready` y `/health/live` validados (AuthService, AuditService)
- [ ] Health Checks UI accesible (AuditService `/health-ui`)

**Tareas**:
```powershell
# 1. Script de validación de health endpoints
$services = @(
    @{Name="Gateway"; Port=5000},
    @{Name="VehicleService"; Port=5009},
    @{Name="ContactService"; Port=5007},
    @{Name="AuthService"; Port=5085},
    @{Name="UserService"; Port=5001},
    @{Name="RoleService"; Port=5002},
    @{Name="ConfigurationService"; Port="TBD"},
    @{Name="AdminService"; Port="TBD"},
    @{Name="CacheService"; Port="TBD"},
    @{Name="ErrorService"; Port="TBD"},
    @{Name="MessageBusService"; Port="TBD"}
)

foreach ($service in $services) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$($service.Port)/health" -UseBasicParsing
        Write-Host "✅ $($service.Name): $($response.StatusCode)" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ $($service.Name): FAILED - $($_.Exception.Message)" -ForegroundColor Red
    }
}

# 2. Verificar endpoints avanzados
curl http://localhost:5085/health/ready  # AuthService
curl http://localhost:5085/health/live   # AuthService
Start-Process "http://localhost:[PORT]/health-ui"  # AuditService
```

**Definición de Hecho (DoD)**:
- ✅ Documento con puertos de todos los servicios creado
- ✅ 100% de health endpoints validados
- ✅ Endpoints faltantes agregados (si es necesario)
- ✅ Script de validación automatizado creado

---

### 🔴 US-1.5: Service-to-Service Communication Testing
**Prioridad**: CRÍTICA  
**Estimación**: 6 horas  
**Asignado a**: Integration Team  
**Depende de**: US-1.4

**Criterios de Aceptación**:
- [ ] Gateway puede rutear requests a servicios backend
- [ ] AuthService puede validar tokens JWT
- [ ] UserService puede verificar permisos con RoleService
- [ ] NotificationService puede enviar mensajes vía MessageBus
- [ ] CacheService puede almacenar/recuperar datos de Redis

**Tareas**:
```powershell
# 1. Test Gateway routing
curl http://localhost:5000/api/vehicles  # → VehicleService
curl http://localhost:5000/api/contacts  # → ContactService
curl http://localhost:5000/api/users     # → UserService

# 2. Test Authentication flow
# Register user → Login → Get JWT → Call protected endpoint

# 3. Test MessageBus
# Trigger notification → Verify RabbitMQ queue → Verify delivery

# 4. Test Cache
# Store data → Retrieve data → Verify Redis

# 5. Test Consul service discovery
# Stop one VehicleService instance → Verify Gateway redirects to other instance
```

**Definición de Hecho (DoD)**:
- ✅ Todos los flujos críticos funcionando
- ✅ Postman collection creado con tests end-to-end
- ✅ Documentación de flujos de integración creada
- ✅ Issues encontrados documentados en backlog

---

### 🔴 US-1.6: Secrets Management Implementation
**Prioridad**: CRÍTICA - SEGURIDAD  
**Estimación**: 8 horas  
**Asignado a**: Security Team  
**Depende de**: US-1.3

**Criterios de Aceptación**:
- [ ] Azure Key Vault o HashiCorp Vault configurado
- [ ] Connection strings migrados a secretos
- [ ] JWT signing keys almacenados en vault
- [ ] API keys de servicios externos en vault
- [ ] Secretos removidos de appsettings.json

**Tareas**:
```powershell
# Opción A: Azure Key Vault
# 1. Crear Key Vault en Azure
az keyvault create --name cardealer-kv --resource-group cardealer-rg

# 2. Agregar secretos
az keyvault secret set --vault-name cardealer-kv --name "ConnectionStrings--ErrorService" --value "..."
az keyvault secret set --vault-name cardealer-kv --name "JwtSettings--SecretKey" --value "..."

# 3. Configurar Managed Identity para servicios
# 4. Actualizar código para leer de Key Vault

# Opción B: HashiCorp Vault
# 1. Iniciar Vault en Docker
docker run -d --name vault -p 8200:8200 vault:latest

# 2. Inicializar y unseal
# 3. Crear secrets paths
# 4. Configurar AppRole authentication
```

**Definición de Hecho (DoD)**:
- ✅ Vault configurado y funcionando
- ✅ Todos los secretos migrados
- ✅ Servicios leyendo secretos correctamente
- ✅ appsettings.json sin secretos hardcoded
- ✅ Documentación de gestión de secretos creada

---

### 🔴 US-1.7: Security Scanning
**Prioridad**: CRÍTICA - SEGURIDAD  
**Estimación**: 4 horas  
**Asignado a**: Security Team  
**Depende de**: US-1.1

**Criterios de Aceptación**:
- [ ] Docker images escaneadas con `docker scan` o Trivy
- [ ] Vulnerabilidades CRITICAL y HIGH resueltas
- [ ] NuGet packages escaneados
- [ ] Reporte de vulnerabilidades generado

**Tareas**:
```powershell
# 1. Escanear imágenes Docker
docker scan gateway:latest
docker scan vehicleservice:latest
docker scan authservice:latest
# ... (repetir para todas las imágenes)

# Alternativa: Trivy (más completo)
trivy image gateway:latest
trivy image vehicleservice:latest

# 2. Escanear dependencias NuGet
dotnet list package --vulnerable --include-transitive

# 3. Generar reporte consolidado
# 4. Crear plan de remediación para vulnerabilidades encontradas
```

**Definición de Hecho (DoD)**:
- ✅ Todas las imágenes escaneadas
- ✅ Reporte de vulnerabilidades generado
- ✅ Vulnerabilidades críticas resueltas
- ✅ Plan de remediación para vulnerabilidades medias/bajas

---

## 📋 Sprint 1 - Checklist Final

- [ ] **US-1.1**: Docker Build Verification (4h)
- [ ] **US-1.2**: Infrastructure Services Startup (2h)
- [ ] **US-1.3**: Core Services Deployment (4h)
- [ ] **US-1.4**: Health Endpoints Runtime Validation (6h)
- [ ] **US-1.5**: Service-to-Service Communication Testing (6h)
- [ ] **US-1.6**: Secrets Management Implementation (8h)
- [ ] **US-1.7**: Security Scanning (4h)

**Total Estimado**: 34 horas (4-5 días)  
**Sprint Goal**: Sistema validado en runtime, seguro y listo para CI/CD

---

# 🎯 SPRINT 2: DevOps & CI/CD
**Duración**: 5-7 días  
**Objetivo**: Automatizar build, test y deployment  
**Prioridad**: 🟡 ALTA - NECESARIO PARA PRODUCCIÓN

## User Stories

### 🟡 US-2.1: GitHub Actions CI Pipeline
**Prioridad**: ALTA  
**Estimación**: 8 horas  
**Asignado a**: DevOps Team

**Criterios de Aceptación**:
- [ ] Pipeline activado en cada push a `main`
- [ ] Build automático de todos los servicios
- [ ] Ejecución automática de los 227 tests
- [ ] Construcción de imágenes Docker
- [ ] Quality gates configurados (cobertura mínima, build success)

**Tareas**:
```yaml
# .github/workflows/ci.yml
name: CI Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore backend/CarDealer.sln
    
    - name: Build
      run: dotnet build backend/CarDealer.sln --no-restore
    
    - name: Run tests
      run: dotnet test backend/CarDealer.sln --no-build --verbosity normal
    
    - name: Build Docker images
      run: |
        cd backend
        docker-compose build
    
    - name: Scan Docker images
      run: |
        # Trivy scanning
        trivy image gateway:latest
```

**Definición de Hecho (DoD)**:
- ✅ Pipeline ejecutándose correctamente
- ✅ Tests pasando en CI
- ✅ Imágenes Docker construyéndose en CI
- ✅ Notificaciones configuradas (email/Slack)

---

### 🟡 US-2.2: Container Registry Setup
**Prioridad**: ALTA  
**Estimación**: 4 horas  
**Asignado a**: DevOps Team  
**Depende de**: US-2.1

**Criterios de Aceptación**:
- [ ] Azure Container Registry (ACR) o Docker Hub configurado
- [ ] Pipeline push images al registry después de build exitoso
- [ ] Versionado de imágenes implementado (tags semánticos)
- [ ] Imágenes de desarrollo vs producción separadas

**Tareas**:
```powershell
# Opción A: Azure Container Registry
az acr create --name cardealerregistry --resource-group cardealer-rg --sku Basic

# Opción B: Docker Hub
# Configurar en GitHub Secrets: DOCKER_USERNAME, DOCKER_PASSWORD

# Pipeline step para push
- name: Push to registry
  run: |
    docker tag gateway:latest cardealerregistry.azurecr.io/gateway:${{ github.sha }}
    docker tag gateway:latest cardealerregistry.azurecr.io/gateway:latest
    docker push cardealerregistry.azurecr.io/gateway:${{ github.sha }}
    docker push cardealerregistry.azurecr.io/gateway:latest
```

**Definición de Hecho (DoD)**:
- ✅ Registry configurado y accesible
- ✅ Pipeline pushea imágenes automáticamente
- ✅ Versionado semántico funcionando
- ✅ Cleanup de imágenes antiguas configurado

---

### 🟡 US-2.3: Automated Deployment to Staging
**Prioridad**: ALTA  
**Estimación**: 8 horas  
**Asignado a**: DevOps Team  
**Depende de**: US-2.2

**Criterios de Aceptación**:
- [ ] Ambiente de staging configurado (Azure, AWS o servidor dedicado)
- [ ] Deployment automático después de build exitoso en `develop` branch
- [ ] Rollback automático en caso de falla
- [ ] Smoke tests ejecutados post-deployment

**Tareas**:
```yaml
# .github/workflows/deploy-staging.yml
name: Deploy to Staging

on:
  push:
    branches: [ develop ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - name: Login to Azure
      run: az login --service-principal ...
    
    - name: Deploy to AKS
      run: |
        kubectl apply -f k8s/staging/
        kubectl rollout status deployment/gateway
    
    - name: Run smoke tests
      run: |
        curl https://staging.cardealer.com/health
        # ... otros smoke tests
```

**Definición de Hecho (DoD)**:
- ✅ Staging environment funcionando
- ✅ Deployment automático exitoso
- ✅ Smoke tests pasando
- ✅ Rollback probado y funcionando

---

### 🟡 US-2.4: Database Migrations Automation
**Prioridad**: ALTA  
**Estimación**: 6 horas  
**Asignado a**: Backend Team

**Criterios de Aceptación**:
- [ ] Scripts de migración para todos los servicios con DB
- [ ] Migraciones ejecutadas automáticamente en deployment
- [ ] Rollback de migraciones posible
- [ ] Backup automático antes de migración

**Tareas**:
```powershell
# 1. Crear migration scripts con EF Core
cd backend/ErrorService/ErrorService.Infrastructure
dotnet ef migrations add InitialCreate
dotnet ef migrations script --output Scripts/Migration_001.sql

# 2. Script de deployment con backup
./backup-database.ps1
dotnet ef database update

# 3. Integrar en pipeline CD
- name: Run migrations
  run: |
    dotnet ef database update --project ErrorService.Infrastructure
    dotnet ef database update --project AuthService.Infrastructure
```

**Definición de Hecho (DoD)**:
- ✅ Scripts de migración para todos los servicios
- ✅ Backup automático configurado
- ✅ Migraciones ejecutándose en CD pipeline
- ✅ Rollback procedure documentado y probado

---

### 🟡 US-2.5: Quality Gates Configuration
**Prioridad**: MEDIA  
**Estimación**: 4 horas  
**Asignado a**: DevOps Team

**Criterios de Aceptación**:
- [ ] Code coverage mínimo configurado (70%)
- [ ] Build debe pasar todos los tests
- [ ] Análisis estático configurado (SonarQube/SonarCloud)
- [ ] No vulnerabilidades CRITICAL permitidas

**Tareas**:
```yaml
# Quality gates en pipeline
- name: Code coverage
  run: |
    dotnet test --collect:"XPlat Code Coverage"
    # Fail if coverage < 70%

- name: SonarQube scan
  run: |
    dotnet sonarscanner begin /k:"cardealer" /d:sonar.login=${{ secrets.SONAR_TOKEN }}
    dotnet build
    dotnet sonarscanner end /d:sonar.login=${{ secrets.SONAR_TOKEN }}
```

**Definición de Hecho (DoD)**:
- ✅ Quality gates configurados
- ✅ Pipeline falla si no se cumplen standards
- ✅ Reportes de calidad visibles
- ✅ Team notificado de violations

---

## 📋 Sprint 2 - Checklist Final

- [ ] **US-2.1**: GitHub Actions CI Pipeline (8h)
- [ ] **US-2.2**: Container Registry Setup (4h)
- [ ] **US-2.3**: Automated Deployment to Staging (8h)
- [ ] **US-2.4**: Database Migrations Automation (6h)
- [ ] **US-2.5**: Quality Gates Configuration (4h)

**Total Estimado**: 30 horas (5-7 días)  
**Sprint Goal**: CI/CD completamente automatizado con quality gates

---

# 🎯 SPRINT 3: Monitoring & Observability
**Duración**: 5-7 días  
**Objetivo**: Visibilidad completa del sistema en producción  
**Prioridad**: 🟡 ALTA - CRÍTICO PARA OPERACIONES

## User Stories

### 🟡 US-3.1: Prometheus Deployment
**Prioridad**: ALTA  
**Estimación**: 6 horas  
**Asignado a**: DevOps Team

**Criterios de Aceptación**:
- [ ] Prometheus desplegado en cluster
- [ ] Todos los servicios exponiendo métricas en `/metrics`
- [ ] Service discovery configurado para scraping automático
- [ ] Métricas básicas recolectándose (CPU, memoria, requests, latency)

**Tareas**:
```yaml
# docker-compose.yml - Agregar Prometheus
prometheus:
  image: prom/prometheus:latest
  container_name: prometheus
  ports:
    - "9090:9090"
  volumes:
    - ./prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
  networks:
    - cargurus-net

# prometheus.yml
scrape_configs:
  - job_name: 'cardealer-services'
    consul_sd_configs:
      - server: 'consul:8500'
    relabel_configs:
      - source_labels: [__meta_consul_service]
        target_label: service
```

**Definición de Hecho (DoD)**:
- ✅ Prometheus funcionando y scrappeando métricas
- ✅ Dashboards básicos de Prometheus funcionando
- ✅ Retention policy configurado
- ✅ Alerting rules básicas configuradas

---

### 🟡 US-3.2: Grafana Dashboards
**Prioridad**: ALTA  
**Estimación**: 8 horas  
**Asignado a**: DevOps Team  
**Depende de**: US-3.1

**Criterios de Aceptación**:
- [ ] Grafana desplegado y conectado a Prometheus
- [ ] Dashboard de overview del sistema (CPU, memoria, requests)
- [ ] Dashboard por servicio (latency, error rate, throughput)
- [ ] Dashboard de infraestructura (Consul, Redis, RabbitMQ, PostgreSQL)
- [ ] Dashboards accesibles para todo el team

**Tareas**:
```yaml
# docker-compose.yml - Agregar Grafana
grafana:
  image: grafana/grafana:latest
  container_name: grafana
  ports:
    - "3000:3000"
  environment:
    - GF_SECURITY_ADMIN_PASSWORD=admin
  volumes:
    - grafana-storage:/var/lib/grafana
    - ./grafana/dashboards:/etc/grafana/provisioning/dashboards
  networks:
    - cargurus-net

# Importar dashboards preconstruidos
# - .NET Application Metrics
# - PostgreSQL Dashboard
# - RabbitMQ Dashboard
# - Redis Dashboard
```

**Definición de Hecho (DoD)**:
- ✅ Grafana accesible en http://localhost:3000
- ✅ 4+ dashboards configurados y funcionando
- ✅ Alertas visuales configuradas
- ✅ Dashboards exportados a JSON (version control)

---

### 🟡 US-3.3: Alerting Configuration
**Prioridad**: ALTA  
**Estimación**: 6 horas  
**Asignado a**: DevOps Team  
**Depende de**: US-3.2

**Criterios de Aceptación**:
- [ ] Alertmanager configurado
- [ ] Alertas críticas configuradas:
  - Servicio down (health check failing)
  - Alta latencia (p95 > 1s)
  - Error rate elevado (> 5%)
  - Alta utilización de recursos (CPU > 80%, memoria > 85%)
- [ ] Notificaciones enviadas a Slack/Teams/Email
- [ ] On-call rotation configurada

**Tareas**:
```yaml
# alertmanager.yml
route:
  group_by: ['alertname', 'service']
  receiver: 'slack-notifications'
  
receivers:
  - name: 'slack-notifications'
    slack_configs:
      - api_url: 'https://hooks.slack.com/services/...'
        channel: '#cardealer-alerts'

# Prometheus alert rules
groups:
  - name: cardealer
    rules:
      - alert: ServiceDown
        expr: up{job="cardealer-services"} == 0
        for: 1m
        annotations:
          summary: "Service {{ $labels.service }} is down"
      
      - alert: HighLatency
        expr: http_request_duration_seconds{quantile="0.95"} > 1
        for: 5m
```

**Definición de Hecho (DoD)**:
- ✅ Alertmanager funcionando
- ✅ 10+ reglas de alertas configuradas
- ✅ Notificaciones llegando correctamente
- ✅ Documentación de respuesta a alertas creada

---

### 🟡 US-3.4: Log Aggregation with Seq/ELK
**Prioridad**: MEDIA  
**Estimación**: 8 horas  
**Asignado a**: DevOps Team

**Criterios de Aceptación**:
- [ ] Seq o ELK Stack desplegado
- [ ] Todos los servicios enviando logs centralizados
- [ ] Logs estructurados (JSON format)
- [ ] Búsqueda y filtrado funcionando
- [ ] Dashboards de logs creados

**Tareas**:
```yaml
# Opción A: Seq (más simple)
seq:
  image: datalust/seq:latest
  container_name: seq
  ports:
    - "5341:80"
  environment:
    - ACCEPT_EULA=Y
  volumes:
    - seq-data:/data

# Configurar Serilog en servicios
builder.Host.UseSerilog((context, config) =>
{
    config
        .WriteTo.Console()
        .WriteTo.Seq("http://seq:5341");
});

# Opción B: ELK Stack
# - Elasticsearch
# - Logstash
# - Kibana
```

**Definición de Hecho (DoD)**:
- ✅ Sistema de logs funcionando
- ✅ Logs de todos los servicios centralizados
- ✅ Búsqueda rápida (<1s para queries)
- ✅ Dashboards de análisis de logs creados

---

### 🟡 US-3.5: Distributed Tracing (Jaeger/Zipkin)
**Prioridad**: MEDIA  
**Estimación**: 6 horas  
**Asignado a**: Backend Team

**Criterios de Aceptación**:
- [ ] Jaeger o Zipkin desplegado
- [ ] OpenTelemetry configurado en todos los servicios
- [ ] Traces end-to-end visibles (Gateway → Backend services)
- [ ] Latencia por servicio visible
- [ ] Dependency graph generado

**Tareas**:
```yaml
# docker-compose.yml - Agregar Jaeger
jaeger:
  image: jaegertracing/all-in-one:latest
  container_name: jaeger
  ports:
    - "16686:16686"  # UI
    - "4317:4317"    # OTLP gRPC
    - "4318:4318"    # OTLP HTTP

# Ya configurado en servicios via OpenTelemetry
# Solo necesita apuntar a Jaeger endpoint
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://jaeger:4317");
        });
    });
```

**Definición de Hecho (DoD)**:
- ✅ Jaeger UI accesible
- ✅ Traces visibles para requests end-to-end
- ✅ Latency breakdown por servicio funcionando
- ✅ Performance bottlenecks identificables

---

## 📋 Sprint 3 - Checklist Final

- [ ] **US-3.1**: Prometheus Deployment (6h)
- [ ] **US-3.2**: Grafana Dashboards (8h)
- [ ] **US-3.3**: Alerting Configuration (6h)
- [ ] **US-3.4**: Log Aggregation with Seq/ELK (8h)
- [ ] **US-3.5**: Distributed Tracing (6h)

**Total Estimado**: 34 horas (5-7 días)  
**Sprint Goal**: Observabilidad completa del sistema

---

# 🎯 SPRINT 4: Testing & Performance
**Duración**: 7-10 días  
**Objetivo**: Expandir cobertura de tests y validar performance  
**Prioridad**: 🟢 MEDIA - IMPORTANTE PARA CALIDAD

## User Stories

### 🟢 US-4.1: Test Coverage Expansion - Wave 1
**Prioridad**: MEDIA  
**Estimación**: 16 horas  
**Asignado a**: QA Team

**Criterios de Aceptación**:
- [ ] Tests para 8 servicios adicionales
- [ ] Mínimo 20 tests por servicio
- [ ] Pattern WebApplicationFactory aplicado
- [ ] 100% pass rate

**Servicios Target**:
1. AdminService
2. ErrorService
3. NotificationService
4. MessageBusService
5. MediaService
6. FileStorageService
7. ConfigurationService
8. FeatureToggleService

**Definición de Hecho (DoD)**:
- ✅ 160+ tests nuevos creados
- ✅ Total: 227 + 160 = 387 tests
- ✅ Todos los tests pasando
- ✅ Documentación FILES_CREATED_TESTS.md para cada servicio

---

### 🟢 US-4.2: Test Coverage Expansion - Wave 2
**Prioridad**: MEDIA  
**Estimación**: 16 horas  
**Asignado a**: QA Team  
**Depende de**: US-4.1

**Criterios de Aceptación**:
- [ ] Tests para 7 servicios adicionales
- [ ] Mínimo 20 tests por servicio
- [ ] Total acumulado > 500 tests

**Servicios Target**:
1. AuditService
2. AuthService
3. SchedulerService
4. SearchService
5. TracingService
6. LoggingService
7. HealthCheckService

**Definición de Hecho (DoD)**:
- ✅ 140+ tests nuevos creados
- ✅ Total: 387 + 140 = 527 tests
- ✅ Code coverage > 70%

---

### 🟢 US-4.3: Test Coverage Expansion - Wave 3
**Prioridad**: MEDIA  
**Estimación**: 16 horas  
**Asignado a**: QA Team  
**Depende de**: US-4.2

**Criterios de Aceptación**:
- [ ] Tests para servicios restantes
- [ ] Target: 1000+ tests totales
- [ ] Code coverage > 75%

**Servicios Target**:
1. ApiDocsService
2. BackupDRService
3. CacheService
4. IdempotencyService
5. RateLimitingService
6. ServiceDiscovery
7. UserService
8. RoleService

**Definición de Hecho (DoD)**:
- ✅ 1000+ tests totales
- ✅ 100% servicios con tests
- ✅ Coverage report generado

---

### 🟢 US-4.4: Load Testing Setup
**Prioridad**: MEDIA  
**Estimación**: 8 horas  
**Asignado a**: Performance Team

**Criterios de Aceptación**:
- [ ] k6 o JMeter configurado
- [ ] Scripts de load testing para servicios críticos
- [ ] Baseline de performance establecido
- [ ] Tests ejecutándose en CI/CD

**Tareas**:
```javascript
// k6-load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    stages: [
        { duration: '2m', target: 100 },  // Ramp-up
        { duration: '5m', target: 100 },  // Stay
        { duration: '2m', target: 200 },  // Spike
        { duration: '5m', target: 200 },  // Stay
        { duration: '2m', target: 0 },    // Ramp-down
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'], // 95% < 500ms
        http_req_failed: ['rate<0.05'],   // <5% failures
    },
};

export default function () {
    let response = http.get('http://localhost:5000/api/vehicles');
    check(response, { 'status is 200': (r) => r.status === 200 });
    sleep(1);
}
```

**Definición de Hecho (DoD)**:
- ✅ Scripts de load testing creados
- ✅ Baseline documentado (requests/sec, latency p95, p99)
- ✅ Tests ejecutándose periódicamente
- ✅ Performance reports generados

---

### 🟢 US-4.5: Performance Optimization
**Prioridad**: MEDIA  
**Estimación**: 12 horas  
**Asignado a**: Backend Team  
**Depende de**: US-4.4

**Criterios de Aceptación**:
- [ ] Bottlenecks identificados y documentados
- [ ] Top 5 bottlenecks optimizados
- [ ] Mejora medible en performance (20%+ en latency)
- [ ] Database queries optimizados (índices agregados)

**Tareas**:
```sql
-- Análisis de queries lentas
SELECT * FROM pg_stat_statements ORDER BY mean_time DESC LIMIT 10;

-- Agregar índices necesarios
CREATE INDEX idx_vehicles_make ON Vehicles(Make);
CREATE INDEX idx_contacts_email ON Contacts(Email);

-- Implementar caching para queries frecuentes
services.AddMemoryCache();
services.AddDistributedRedisCache(options =>
{
    options.Configuration = "redis:6379";
});
```

**Definición de Hecho (DoD)**:
- ✅ Performance mejorado 20%+
- ✅ Índices optimizados
- ✅ Caching strategy implementada
- ✅ Nuevo baseline establecido

---

## 📋 Sprint 4 - Checklist Final

- [ ] **US-4.1**: Test Coverage Expansion - Wave 1 (16h)
- [ ] **US-4.2**: Test Coverage Expansion - Wave 2 (16h)
- [ ] **US-4.3**: Test Coverage Expansion - Wave 3 (16h)
- [ ] **US-4.4**: Load Testing Setup (8h)
- [ ] **US-4.5**: Performance Optimization (12h)

**Total Estimado**: 68 horas (7-10 días)  
**Sprint Goal**: 1000+ tests y performance optimizado

---

# 🎯 SPRINT 5: Production Deployment & Documentation
**Duración**: 5-7 días  
**Objetivo**: Deploy a producción y documentación final  
**Prioridad**: 🟢 ALTA - DEPLOYMENT FINAL

## User Stories

### 🟢 US-5.1: Kubernetes Manifests Creation
**Prioridad**: ALTA  
**Estimación**: 12 horas  
**Asignado a**: DevOps Team

**Criterios de Aceptación**:
- [ ] Deployments para todos los servicios
- [ ] Services (ClusterIP, LoadBalancer)
- [ ] ConfigMaps y Secrets
- [ ] Ingress configuration
- [ ] HorizontalPodAutoscaler configurado

**Tareas**:
```yaml
# k8s/production/gateway-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gateway
spec:
  replicas: 3
  selector:
    matchLabels:
      app: gateway
  template:
    metadata:
      labels:
        app: gateway
    spec:
      containers:
      - name: gateway
        image: cardealerregistry.azurecr.io/gateway:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 20
          periodSeconds: 5

---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: gateway-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: gateway
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

**Definición de Hecho (DoD)**:
- ✅ Manifests para 24 servicios creados
- ✅ ConfigMaps/Secrets configurados
- ✅ Autoscaling configurado
- ✅ Deployments validados en staging

---

### 🟢 US-5.2: Production Environment Setup
**Prioridad**: ALTA  
**Estimación**: 8 horas  
**Asignado a**: DevOps Team  
**Depende de**: US-5.1

**Criterios de Aceptación**:
- [ ] Kubernetes cluster creado (AKS/EKS/GKE)
- [ ] Namespaces configurados (production, staging)
- [ ] RBAC configurado
- [ ] Network policies aplicadas
- [ ] Persistent volumes configurados

**Tareas**:
```powershell
# Crear AKS cluster
az aks create `
    --resource-group cardealer-prod-rg `
    --name cardealer-aks `
    --node-count 3 `
    --node-vm-size Standard_D4s_v3 `
    --enable-addons monitoring `
    --generate-ssh-keys

# Configurar kubectl
az aks get-credentials --resource-group cardealer-prod-rg --name cardealer-aks

# Crear namespaces
kubectl create namespace production
kubectl create namespace staging

# Aplicar network policies
kubectl apply -f k8s/network-policies/
```

**Definición de Hecho (DoD)**:
- ✅ Cluster funcionando y accesible
- ✅ Namespaces configurados
- ✅ Security policies aplicadas
- ✅ Monitoring integrado (Prometheus operator)

---

### 🟢 US-5.3: Production Deployment
**Prioridad**: CRÍTICA  
**Estimación**: 6 horas  
**Asignado a**: DevOps Team  
**Depende de**: US-5.2

**Criterios de Aceptación**:
- [ ] Todos los servicios desplegados en producción
- [ ] Health checks pasando
- [ ] Servicios accesibles desde Internet
- [ ] SSL/TLS configurado
- [ ] DNS configurado

**Tareas**:
```powershell
# Deploy infrastructure
kubectl apply -f k8s/production/infrastructure/

# Deploy services
kubectl apply -f k8s/production/services/

# Verificar deployments
kubectl get pods -n production
kubectl get services -n production

# Verificar health checks
kubectl exec -it gateway-xxxx -n production -- curl http://localhost/health

# Configurar Ingress con SSL
kubectl apply -f k8s/production/ingress.yaml
```

**Definición de Hecho (DoD)**:
- ✅ Todos los servicios running en production
- ✅ 0 pods en estado CrashLoopBackOff
- ✅ Health checks verdes
- ✅ SSL/TLS funcionando
- ✅ API accesible: https://api.cardealer.com

---

### 🟢 US-5.4: Post-Deployment Validation
**Prioridad**: CRÍTICA  
**Estimación**: 4 horas  
**Asignado a**: QA Team  
**Depende de**: US-5.3

**Criterios de Aceptación**:
- [ ] Smoke tests ejecutados y pasando
- [ ] Health endpoints respondiendo
- [ ] Service discovery funcionando
- [ ] Logging funcionando
- [ ] Metrics siendo recolectadas
- [ ] Alertas configuradas y funcionando

**Tareas**:
```powershell
# Smoke tests
curl https://api.cardealer.com/health
curl https://api.cardealer.com/api/vehicles
curl https://api.cardealer.com/api/contacts

# Verificar Grafana dashboards
# Verificar Jaeger traces
# Verificar Seq/ELK logs
# Verificar Prometheus metrics

# Trigger test alert
# Verificar que llega notificación
```

**Definición de Hecho (DoD)**:
- ✅ Todos los smoke tests pasando
- ✅ Monitoring funcionando
- ✅ Alertas configuradas
- ✅ Team notificado de deployment exitoso

---

### 🟢 US-5.5: Documentation Finalization
**Prioridad**: ALTA  
**Estimación**: 12 horas  
**Asignado a**: Technical Writer + Team

**Criterios de Aceptación**:
- [ ] README.md actualizado con info de producción
- [ ] Deployment runbooks creados
- [ ] Troubleshooting guides actualizados
- [ ] Architecture Decision Records (ADRs) documentados
- [ ] API documentation (Swagger) accesible
- [ ] Onboarding guide para nuevos developers

**Tareas**:
```markdown
# Documentación a crear/actualizar:

1. DEPLOYMENT_GUIDE.md
   - Pre-requisitos
   - Pasos de deployment
   - Rollback procedures
   - Troubleshooting

2. RUNBOOKS.md
   - Respuesta a alertas
   - Procedimientos de emergencia
   - Escalation paths

3. ARCHITECTURE.md
   - Diagrams actualizados
   - Component interactions
   - Data flows
   - Security architecture

4. API_DOCUMENTATION.md
   - Swagger UI links
   - Authentication guide
   - Rate limiting info
   - Examples

5. ONBOARDING.md
   - Setup de ambiente local
   - Guía de contribución
   - Code standards
   - Testing guidelines
```

**Definición de Hecho (DoD)**:
- ✅ 5+ documentos creados/actualizados
- ✅ Swagger UI accesible en producción
- ✅ Team puede navegar documentación fácilmente
- ✅ Onboarding guide validado con nuevo developer

---

### 🟢 US-5.6: Backup & Disaster Recovery Testing
**Prioridad**: ALTA  
**Estimación**: 6 horas  
**Asignado a**: DevOps Team

**Criterios de Aceptación**:
- [ ] Backups automáticos configurados (databases, volumes)
- [ ] Backup retention policy configurado
- [ ] Disaster recovery plan documentado
- [ ] Restore procedure probado exitosamente

**Tareas**:
```powershell
# Configurar backups automáticos de PostgreSQL
# Usar Azure Backup, AWS Backup o Velero (Kubernetes)

# Test restore procedure
1. Crear backup completo
2. Simular desastre (eliminar namespace)
3. Restore desde backup
4. Validar integridad de datos
5. Documentar tiempo de recovery (RTO/RPO)
```

**Definición de Hecho (DoD)**:
- ✅ Backups ejecutándose diariamente
- ✅ Restore procedure validado
- ✅ RTO < 4 horas, RPO < 1 hora
- ✅ Disaster recovery plan documentado

---

## 📋 Sprint 5 - Checklist Final

- [ ] **US-5.1**: Kubernetes Manifests Creation (12h)
- [ ] **US-5.2**: Production Environment Setup (8h)
- [ ] **US-5.3**: Production Deployment (6h)
- [ ] **US-5.4**: Post-Deployment Validation (4h)
- [ ] **US-5.5**: Documentation Finalization (12h)
- [ ] **US-5.6**: Backup & Disaster Recovery Testing (6h)

**Total Estimado**: 48 horas (5-7 días)  
**Sprint Goal**: Sistema en producción y documentado

---

# 📊 Resumen de Sprints

## Timeline Global

| Sprint | Objetivo | Duración | Horas | Prioridad |
|--------|----------|----------|-------|-----------|
| **Sprint 1** | Runtime Validation & Security | 4-5 días | 34h | 🔴 CRÍTICA |
| **Sprint 2** | DevOps & CI/CD | 5-7 días | 30h | 🟡 ALTA |
| **Sprint 3** | Monitoring & Observability | 5-7 días | 34h | 🟡 ALTA |
| **Sprint 4** | Testing & Performance | 7-10 días | 68h | 🟢 MEDIA |
| **Sprint 5** | Production Deployment | 5-7 días | 48h | 🟢 ALTA |

**Total**: 26-36 días (5-7 semanas)  
**Total Horas**: 214 horas

---

## Progreso Hacia 100% Producción

### Después de Sprint 1
- ✅ Runtime validado
- ✅ Seguridad básica
- **Producción Ready**: 75%

### Después de Sprint 2
- ✅ CI/CD funcionando
- ✅ Deployments automatizados
- **Producción Ready**: 85%

### Después de Sprint 3
- ✅ Monitoreo completo
- ✅ Alertas configuradas
- **Producción Ready**: 90%

### Después de Sprint 4
- ✅ 1000+ tests
- ✅ Performance optimizado
- **Producción Ready**: 95%

### Después de Sprint 5
- ✅ En producción
- ✅ Documentado
- **Producción Ready**: 100% 🎉

---

## 🎯 Definición de "Producción 100%"

### Criterios de Aceptación Globales

#### Funcionalidad
- [x] Todos los servicios compiling (26/26) ✅
- [ ] Todos los servicios running en producción (24/24)
- [ ] Health checks verdes (24/24)
- [ ] Service discovery funcionando
- [ ] Gateway ruteando correctamente

#### Calidad
- [x] 227 tests pasando ✅
- [ ] 1000+ tests totales
- [ ] Code coverage > 75%
- [ ] 0 vulnerabilidades CRITICAL
- [ ] Performance SLAs cumplidos (p95 < 500ms)

#### Seguridad
- [ ] Secretos en vault (no hardcoded)
- [ ] HTTPS/TLS configurado
- [ ] RBAC configurado
- [ ] Network policies aplicadas
- [ ] Security scans pasando

#### DevOps
- [ ] CI/CD funcionando
- [ ] Deployments automatizados
- [ ] Rollback procedure validado
- [ ] Database migrations automatizadas
- [ ] Quality gates configurados

#### Observabilidad
- [ ] Prometheus recolectando métricas
- [ ] Grafana dashboards funcionando
- [ ] Alertas configuradas y probadas
- [ ] Logs centralizados
- [ ] Distributed tracing funcionando

#### Operaciones
- [ ] Backups automáticos configurados
- [ ] Disaster recovery plan validado
- [ ] Runbooks documentados
- [ ] On-call rotation configurado
- [ ] Escalation procedures definidos

#### Documentación
- [x] 13 servicios documentados ✅
- [ ] Deployment guide completo
- [ ] API documentation (Swagger)
- [ ] Troubleshooting guides
- [ ] Onboarding guide
- [ ] Architecture decision records

---

## 🚀 Quick Start - Sprint 1

Para comenzar inmediatamente con Sprint 1:

```powershell
# 1. Iniciar Docker Desktop
# (Ejecutar manualmente)

# 2. Verificar Docker
docker --version
docker ps

# 3. Limpiar sistema
docker system prune -a --volumes -f

# 4. Construir imágenes
cd C:\Users\gmoreno\source\repos\cardealer\backend
docker-compose build --no-cache

# 5. Iniciar infraestructura
docker-compose up -d consul redis rabbitmq

# 6. Verificar healthy status
Start-Sleep -Seconds 30
docker ps --filter "health=healthy"

# 7. Iniciar servicios core
docker-compose up -d gateway vehicleservice contactservice authservice

# 8. Verificar health endpoints
curl http://localhost:5000/health
curl http://localhost:5009/health
curl http://localhost:5007/health

# ✅ Sprint 1 - US-1.1 a US-1.3 COMPLETADOS!
```

---

## 📞 Soporte

**Sprint Master**: TBD  
**Product Owner**: TBD  
**Tech Lead**: TBD  

**Daily Standup**: 10:00 AM  
**Sprint Review**: Último viernes de sprint  
**Sprint Retrospective**: Después de review  

---

**Creado**: Diciembre 3, 2025  
**Última Actualización**: Diciembre 3, 2025  
**Estado**: READY TO START  
**Siguiente Acción**: Iniciar Sprint 1 - US-1.1
