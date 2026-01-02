# 🚀 Plan de Sprints Q1 2026 - Producción CarDealer Microservices

**Proyecto:** CarDealer Microservices  
**Período:** Enero - Marzo 2026  
**Objetivo:** Llevar el sistema a producción enterprise-ready  
**Fecha creación:** 2 Enero 2026  
**Versión:** 1.0

---

## 📋 RESUMEN EJECUTIVO

| Métrica | Valor |
|---------|-------|
| **Sprints totales** | 24 sprints |
| **Duración** | 12 semanas (Q1 2026) |
| **Tokens estimados** | ~720,000 |
| **Milestones** | 4 (M1-M4) |
| **Objetivo final** | 99.9% uptime, producción enterprise |

### Distribución por Fase

| Fase | Semanas | Sprints | Tokens Est. | Objetivo |
|------|:-------:|:-------:|:-----------:|----------|
| **Fase 1:** Seguridad | 1-2 | 6 | ~150,000 | Producción básica segura |
| **Fase 2:** Optimización | 3-4 | 6 | ~140,000 | Performance y consolidación |
| **Fase 3:** Kubernetes | 5-8 | 8 | ~280,000 | Infraestructura escalable |
| **Fase 4:** Go-Live | 9-12 | 4 | ~150,000 | Producción enterprise |
| **TOTAL** | 12 | 24 | ~720,000 | ✅ Production Ready |

---

## 🔐 FASE 1: SEGURIDAD Y ESTABILIDAD (Semanas 1-2)

**Milestone M1:** Producción Básica (15 Enero 2026)  
**Objetivo:** Sistema seguro y estable para staging/producción inicial

### Sprint 1.1: HashiCorp Vault Setup
**Fecha:** 6-7 Enero 2026  
**Tokens estimados:** ~25,000  
**Prioridad:** P0 (Crítico)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 1.1.1 | Desplegar Vault en Docker | ~5,000 | Container + volumen persistente | - |
| 1.1.2 | Configurar políticas de acceso | ~5,000 | Policies por servicio/rol | 1.1.1 |
| 1.1.3 | Migrar JWT keys a Vault | ~5,000 | Rotar y almacenar en Vault | 1.1.2 |
| 1.1.4 | Migrar DB credentials a Vault | ~5,000 | 20 conexiones PostgreSQL | 1.1.2 |
| 1.1.5 | Integrar servicios .NET con Vault | ~5,000 | VaultSharp en todos los servicios | 1.1.3, 1.1.4 |

**Entregables:**
- [ ] Vault operativo en `vault:8200`
- [ ] 36 secretos migrados desde compose.yaml
- [ ] Documentación de rotación de secretos

**Criterios de Aceptación:**
- Vault health check OK
- Ningún secreto en código/compose
- Servicios arrancan con secrets de Vault

---

### Sprint 1.2: SSL/TLS Certificates
**Fecha:** 7-8 Enero 2026  
**Tokens estimados:** ~20,000  
**Prioridad:** P0 (Crítico)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 1.2.1 | Configurar cert-manager | ~4,000 | Let's Encrypt o self-signed | - |
| 1.2.2 | Generar certificados Gateway | ~4,000 | Wildcard *.cardealer.local | 1.2.1 |
| 1.2.3 | Configurar HTTPS en Gateway | ~4,000 | Ocelot con SSL termination | 1.2.2 |
| 1.2.4 | mTLS entre servicios | ~4,000 | Opcional: comunicación interna | 1.2.3 |
| 1.2.5 | Actualizar compose.yaml | ~4,000 | Puertos 443, volumen certs | 1.2.3 |

**Entregables:**
- [ ] Gateway accesible en HTTPS (puerto 443)
- [ ] Certificados auto-renovables
- [ ] Redirección HTTP → HTTPS

---

### Sprint 1.3: Rate Limiting Global
**Fecha:** 8-9 Enero 2026  
**Tokens estimados:** ~25,000  
**Prioridad:** P1 (Alto)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 1.3.1 | Configurar AspNetCoreRateLimit | ~5,000 | En Gateway con Redis backend | - |
| 1.3.2 | Definir políticas por endpoint | ~5,000 | /auth: 10/min, /api: 100/min | 1.3.1 |
| 1.3.3 | Rate limit por IP | ~5,000 | Límites globales por cliente | 1.3.1 |
| 1.3.4 | Rate limit por usuario | ~5,000 | Límites por JWT/API Key | 1.3.1 |
| 1.3.5 | Dashboard de rate limiting | ~5,000 | Métricas en Redis | 1.3.2, 1.3.3, 1.3.4 |

**Entregables:**
- [ ] Rate limiting activo en Gateway
- [ ] Headers `X-RateLimit-*` en responses
- [ ] Endpoint `/api/rate-limit/status`

---

### Sprint 1.4: Circuit Breakers (Polly)
**Fecha:** 9-10 Enero 2026  
**Tokens estimados:** ~25,000  
**Prioridad:** P1 (Alto)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 1.4.1 | Instalar Polly en Gateway | ~5,000 | Microsoft.Extensions.Http.Polly | - |
| 1.4.2 | Circuit breaker por downstream | ~5,000 | 5 fallos → open 30s | 1.4.1 |
| 1.4.3 | Retry policies | ~5,000 | 3 retries con exponential backoff | 1.4.1 |
| 1.4.4 | Timeout policies | ~5,000 | 30s timeout por request | 1.4.1 |
| 1.4.5 | Fallback responses | ~5,000 | Respuestas por defecto en degraded | 1.4.2 |

**Entregables:**
- [ ] Polly configurado en Gateway
- [ ] Servicios aislados en caso de fallo
- [ ] Logs de circuit breaker events

---

### Sprint 1.5: Health Dashboard (HealthChecksUI)
**Fecha:** 13-14 Enero 2026  
**Tokens estimados:** ~20,000  
**Prioridad:** P1 (Alto)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 1.5.1 | Instalar HealthChecksUI | ~4,000 | AspNetCore.HealthChecks.UI | - |
| 1.5.2 | Configurar todos los endpoints | ~4,000 | 35 servicios monitoreados | 1.5.1 |
| 1.5.3 | Health checks de dependencias | ~4,000 | PostgreSQL, Redis, RabbitMQ | 1.5.1 |
| 1.5.4 | Webhooks de alertas | ~4,000 | Slack/Teams/Email en failures | 1.5.2 |
| 1.5.5 | Dashboard público | ~4,000 | UI en `/health-ui` | 1.5.2 |

**Entregables:**
- [ ] Dashboard en `https://gateway/health-ui`
- [ ] Alertas automáticas en Slack
- [ ] Histórico de health status

---

### Sprint 1.6: Logging Centralizado
**Fecha:** 14-15 Enero 2026  
**Tokens estimados:** ~25,000  
**Prioridad:** P1 (Alto)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 1.6.1 | Desplegar Seq o Elasticsearch | ~5,000 | Container + volumen | - |
| 1.6.2 | Configurar Serilog sink | ~5,000 | Todos los servicios → Seq | 1.6.1 |
| 1.6.3 | Structured logging estándar | ~5,000 | Formato JSON unificado | 1.6.2 |
| 1.6.4 | Correlation IDs | ~5,000 | Trace ID en todos los logs | 1.6.2 |
| 1.6.5 | Retention policies | ~5,000 | 30 días en Seq | 1.6.1 |

**Entregables:**
- [ ] Seq/Elasticsearch operativo
- [ ] Todos los logs centralizados
- [ ] Búsqueda por correlation ID

---

### 🎯 Milestone M1: Producción Básica (15 Enero)

**Checklist de validación:**
- [ ] ✅ Vault operativo con todos los secretos
- [ ] ✅ HTTPS en Gateway (SSL/TLS)
- [ ] ✅ Rate limiting activo
- [ ] ✅ Circuit breakers configurados
- [ ] ✅ Health dashboard visible
- [ ] ✅ Logs centralizados en Seq

**Métricas objetivo:**
| KPI | Target M1 |
|-----|:---------:|
| Uptime | 99% |
| Response P95 | <1s |
| Secrets en código | 0 |

---

## ⚡ FASE 2: OPTIMIZACIÓN Y CONSOLIDACIÓN (Semanas 3-4)

**Milestone M2:** Optimización (31 Enero 2026)  
**Objetivo:** Performance mejorado y arquitectura simplificada

### Sprint 2.1: Redis Caching Layer
**Fecha:** 20-21 Enero 2026  
**Tokens estimados:** ~25,000  
**Prioridad:** P1 (Alto)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 2.1.1 | Definir estrategia de cache | ~5,000 | Cache-aside, TTLs por tipo | - |
| 2.1.2 | Cache en AuthService | ~5,000 | Tokens, user profiles | 2.1.1 |
| 2.1.3 | Cache en ProductService | ~5,000 | Catálogo, búsquedas | 2.1.1 |
| 2.1.4 | Cache en ConfigurationService | ~5,000 | Feature flags, configs | 2.1.1 |
| 2.1.5 | Cache invalidation | ~5,000 | Pub/Sub para invalidar | 2.1.2, 2.1.3, 2.1.4 |

**Entregables:**
- [ ] Caching activo en 3+ servicios
- [ ] Hit rate >80% en endpoints frecuentes
- [ ] TTLs documentados

---

### Sprint 2.2: Database Optimization
**Fecha:** 21-22 Enero 2026  
**Tokens estimados:** ~20,000  
**Prioridad:** P2 (Medio)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 2.2.1 | Connection pooling | ~4,000 | Npgsql MaxPoolSize=20 | - |
| 2.2.2 | Query optimization | ~4,000 | Índices en columnas frecuentes | - |
| 2.2.3 | Lazy loading review | ~4,000 | Evitar N+1 queries | - |
| 2.2.4 | Read replicas (prep) | ~4,000 | Diseño para read scaling | - |
| 2.2.5 | Vacuum automático | ~4,000 | Configurar en PostgreSQL | - |

**Entregables:**
- [ ] Connection pooling en todos los servicios
- [ ] Query performance <100ms P95
- [ ] Documentación de índices

---

### Sprint 2.3: Response Compression
**Fecha:** 22-23 Enero 2026  
**Tokens estimados:** ~15,000  
**Prioridad:** P3 (Bajo)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 2.3.1 | Gzip en Gateway | ~5,000 | UseResponseCompression() | - |
| 2.3.2 | Brotli como alternativa | ~5,000 | Para browsers modernos | 2.3.1 |
| 2.3.3 | Exclude binary content | ~5,000 | No comprimir images/files | 2.3.1 |

**Entregables:**
- [ ] Compression activo en Gateway
- [ ] ~60% reducción en payload size

---

### Sprint 2.4: Fusionar MediaService + FileStorageService
**Fecha:** 23-24 Enero 2026  
**Tokens estimados:** ~30,000  
**Prioridad:** P2 (Medio)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 2.4.1 | Análisis de overlaps | ~5,000 | Endpoints duplicados | - |
| 2.4.2 | Diseño servicio unificado | ~5,000 | MediaService como base | 2.4.1 |
| 2.4.3 | Migrar código FileStorage | ~8,000 | S3/Azure providers | 2.4.2 |
| 2.4.4 | Actualizar dependencias | ~6,000 | Referencias en otros servicios | 2.4.3 |
| 2.4.5 | Eliminar FileStorageService | ~6,000 | Deprecar y remover | 2.4.4 |

**Entregables:**
- [ ] MediaService con storage providers
- [ ] FileStorageService eliminado
- [ ] 34 servicios (antes 35)

---

### Sprint 2.5: Fusionar AdminService + UserService
**Fecha:** 27-28 Enero 2026  
**Tokens estimados:** ~25,000  
**Prioridad:** P2 (Medio)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 2.5.1 | Análisis de endpoints Admin | ~5,000 | User management en Admin | - |
| 2.5.2 | Migrar endpoints a UserService | ~8,000 | Admin features → User | 2.5.1 |
| 2.5.3 | Permisos RBAC | ~6,000 | Admin role en UserService | 2.5.2 |
| 2.5.4 | Eliminar AdminService | ~6,000 | Deprecar y remover | 2.5.3 |

**Entregables:**
- [ ] UserService con admin endpoints
- [ ] AdminService eliminado
- [ ] 33 servicios (antes 34)

---

### Sprint 2.6: API Versioning
**Fecha:** 28-31 Enero 2026  
**Tokens estimados:** ~25,000  
**Prioridad:** P2 (Medio)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 2.6.1 | Instalar Asp.Versioning | ~5,000 | En todos los servicios | - |
| 2.6.2 | Definir v1 actual | ~5,000 | Todos los endpoints → /v1/ | 2.6.1 |
| 2.6.3 | Configurar Gateway routing | ~5,000 | /api/v1/* routing | 2.6.2 |
| 2.6.4 | Swagger por versión | ~5,000 | Docs separados v1, v2 | 2.6.2 |
| 2.6.5 | Deprecation headers | ~5,000 | Sunset header para v0 | 2.6.2 |

**Entregables:**
- [ ] Todos los endpoints en `/api/v1/`
- [ ] Swagger con versiones
- [ ] Backward compatibility

---

### 🎯 Milestone M2: Optimización (31 Enero)

**Checklist de validación:**
- [ ] ✅ Redis caching operativo
- [ ] ✅ DB queries optimizadas
- [ ] ✅ Response compression activo
- [ ] ✅ 33 servicios (2 fusionados)
- [ ] ✅ API versioning `/v1/`

**Métricas objetivo:**
| KPI | Target M2 |
|-----|:---------:|
| Response P95 | <500ms |
| Cache hit rate | >80% |
| Servicios | 33 |

---

## ☸️ FASE 3: KUBERNETES MIGRATION (Semanas 5-8)

**Milestone M3:** Kubernetes Ready (28 Febrero 2026)  
**Objetivo:** Infraestructura escalable y auto-healing

### Sprint 3.1: Kubernetes Cluster Setup
**Fecha:** 3-5 Febrero 2026  
**Tokens estimados:** ~35,000  
**Prioridad:** P1 (Alto)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 3.1.1 | Provisionar cluster K8s | ~7,000 | DigitalOcean/AWS EKS | - |
| 3.1.2 | Configurar namespaces | ~7,000 | dev, staging, prod | 3.1.1 |
| 3.1.3 | Instalar Ingress Controller | ~7,000 | nginx-ingress | 3.1.1 |
| 3.1.4 | Configurar cert-manager | ~7,000 | Let's Encrypt certs | 3.1.3 |
| 3.1.5 | Configurar DNS | ~7,000 | *.cardealer.com → Ingress | 3.1.3 |

**Entregables:**
- [ ] Cluster K8s operativo
- [ ] Ingress con SSL
- [ ] DNS configurado

---

### Sprint 3.2: StatefulSets (Databases)
**Fecha:** 5-7 Febrero 2026  
**Tokens estimados:** ~35,000  
**Prioridad:** P1 (Alto)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 3.2.1 | PostgreSQL Operator | ~10,000 | CrunchyData o Zalando | 3.1.1 |
| 3.2.2 | Redis StatefulSet | ~8,000 | Con persistencia | 3.1.1 |
| 3.2.3 | RabbitMQ Operator | ~8,000 | rabbitmq-cluster-operator | 3.1.1 |
| 3.2.4 | Backup automático | ~5,000 | Velero para snapshots | 3.2.1 |
| 3.2.5 | Migrar datos existentes | ~4,000 | pg_dump → K8s PostgreSQL | 3.2.1 |

**Entregables:**
- [ ] PostgreSQL HA en K8s
- [ ] Redis cluster
- [ ] RabbitMQ cluster
- [ ] Backups automatizados

---

### Sprint 3.3: Core Services Deployment
**Fecha:** 10-12 Febrero 2026  
**Tokens estimados:** ~35,000  
**Prioridad:** P1 (Alto)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 3.3.1 | Helm charts base | ~7,000 | Template reutilizable | - |
| 3.3.2 | Deploy AuthService | ~7,000 | Deployment + Service + HPA | 3.2.1 |
| 3.3.3 | Deploy Gateway | ~7,000 | Ingress + TLS | 3.3.2 |
| 3.3.4 | Deploy ErrorService | ~7,000 | Deployment + Service | 3.3.2 |
| 3.3.5 | Deploy NotificationService | ~7,000 | Deployment + Service | 3.3.2 |

**Entregables:**
- [ ] 4 servicios core en K8s
- [ ] Gateway accesible externamente
- [ ] HPA configurado

---

### Sprint 3.4: User & Product Services
**Fecha:** 12-14 Febrero 2026  
**Tokens estimados:** ~35,000  
**Prioridad:** P1 (Alto)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 3.4.1 | Deploy UserService | ~7,000 | + RoleService integrado | 3.3.3 |
| 3.4.2 | Deploy ProductService | ~7,000 | Deployment + HPA | 3.3.3 |
| 3.4.3 | Deploy MediaService | ~7,000 | + PVC para storage | 3.3.3 |
| 3.4.4 | Deploy BillingService | ~7,000 | Stripe webhooks | 3.3.3 |
| 3.4.5 | Deploy CRMService | ~7,000 | Deployment básico | 3.3.3 |

**Entregables:**
- [ ] 5 servicios adicionales en K8s
- [ ] Total: 9 servicios migrados

---

### Sprint 3.5: Infrastructure Services
**Fecha:** 17-19 Febrero 2026  
**Tokens estimados:** ~35,000  
**Prioridad:** P2 (Medio)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 3.5.1 | Deploy CacheService | ~7,000 | Conectado a Redis cluster | 3.2.2 |
| 3.5.2 | Deploy MessageBusService | ~7,000 | Conectado a RabbitMQ | 3.2.3 |
| 3.5.3 | Deploy ConfigurationService | ~7,000 | ConfigMaps como fuente | 3.3.3 |
| 3.5.4 | Deploy SchedulerService | ~7,000 | Hangfire con PostgreSQL | 3.2.1 |
| 3.5.5 | Deploy LoggingService | ~7,000 | Con Loki/Elasticsearch | 3.3.3 |

**Entregables:**
- [ ] 5 servicios infra en K8s
- [ ] Total: 14 servicios migrados

---

### Sprint 3.6: Business Services Batch 1
**Fecha:** 19-21 Febrero 2026  
**Tokens estimados:** ~35,000  
**Prioridad:** P2 (Medio)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 3.6.1 | Deploy ContactService | ~7,000 | | 3.3.3 |
| 3.6.2 | Deploy AppointmentService | ~7,000 | | 3.3.3 |
| 3.6.3 | Deploy InvoicingService | ~7,000 | | 3.3.3 |
| 3.6.4 | Deploy FinanceService | ~7,000 | | 3.3.3 |
| 3.6.5 | Deploy AuditService | ~7,000 | | 3.3.3 |

**Entregables:**
- [ ] 5 servicios negocio en K8s
- [ ] Total: 19 servicios migrados

---

### Sprint 3.7: Business Services Batch 2
**Fecha:** 24-26 Febrero 2026  
**Tokens estimados:** ~35,000  
**Prioridad:** P2 (Medio)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 3.7.1 | Deploy MarketingService | ~7,000 | | 3.3.3 |
| 3.7.2 | Deploy IntegrationService | ~7,000 | | 3.3.3 |
| 3.7.3 | Deploy ReportsService | ~7,000 | | 3.3.3 |
| 3.7.4 | Deploy SearchService | ~7,000 | Con Elasticsearch | 3.3.3 |
| 3.7.5 | Deploy BackupDRService | ~7,000 | Con Velero | 3.3.3 |

**Entregables:**
- [ ] 5 servicios adicionales en K8s
- [ ] Total: 24 servicios migrados

---

### Sprint 3.8: Remaining Services + Network Policies
**Fecha:** 26-28 Febrero 2026  
**Tokens estimados:** ~35,000  
**Prioridad:** P2 (Medio)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 3.8.1 | Deploy servicios restantes | ~10,000 | 9 servicios finales | 3.3.3 |
| 3.8.2 | Network Policies | ~8,000 | Aislar namespaces | 3.8.1 |
| 3.8.3 | Resource Limits | ~7,000 | CPU/Memory por pod | 3.8.1 |
| 3.8.4 | Pod Disruption Budgets | ~5,000 | Min replicas durante updates | 3.8.1 |
| 3.8.5 | Validación E2E | ~5,000 | Test de todos los servicios | 3.8.1 |

**Entregables:**
- [ ] 33 servicios en K8s
- [ ] Network policies activas
- [ ] Resource limits definidos

---

### 🎯 Milestone M3: Kubernetes Ready (28 Febrero)

**Checklist de validación:**
- [ ] ✅ Cluster K8s operativo
- [ ] ✅ 33 servicios migrados
- [ ] ✅ HPA en servicios críticos
- [ ] ✅ Network policies
- [ ] ✅ Backups automatizados

**Métricas objetivo:**
| KPI | Target M3 |
|-----|:---------:|
| Uptime | 99.5% |
| Pod restart rate | <1/día |
| Scale time | <60s |

---

## 🚀 FASE 4: OBSERVABILIDAD Y GO-LIVE (Semanas 9-12)

**Milestone M4:** Producción Enterprise (31 Marzo 2026)  
**Objetivo:** Sistema monitoreado 24/7 con DR probado

### Sprint 4.1: Prometheus + Grafana
**Fecha:** 3-7 Marzo 2026  
**Tokens estimados:** ~40,000  
**Prioridad:** P1 (Alto)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 4.1.1 | Deploy Prometheus Operator | ~8,000 | kube-prometheus-stack | - |
| 4.1.2 | ServiceMonitors | ~8,000 | Scrape todos los servicios | 4.1.1 |
| 4.1.3 | Deploy Grafana | ~8,000 | Dashboards predefinidos | 4.1.1 |
| 4.1.4 | Dashboard por servicio | ~8,000 | Latency, errors, throughput | 4.1.3 |
| 4.1.5 | Dashboard de negocio | ~8,000 | Users, transactions, revenue | 4.1.3 |

**Entregables:**
- [ ] Prometheus operativo
- [ ] 10+ dashboards en Grafana
- [ ] Métricas de negocio

---

### Sprint 4.2: Alerting + On-Call
**Fecha:** 10-14 Marzo 2026  
**Tokens estimados:** ~35,000  
**Prioridad:** P1 (Alto)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 4.2.1 | AlertManager setup | ~7,000 | Routing de alertas | 4.1.1 |
| 4.2.2 | Alertas críticas | ~7,000 | Downtime, error rate >1% | 4.2.1 |
| 4.2.3 | Alertas de warning | ~7,000 | Latency, disk space | 4.2.1 |
| 4.2.4 | Integración PagerDuty/Opsgenie | ~7,000 | Escalamiento automático | 4.2.1 |
| 4.2.5 | Runbooks en alertas | ~7,000 | Links a documentación | 4.2.2 |

**Entregables:**
- [ ] 20+ alertas configuradas
- [ ] Escalamiento automático
- [ ] Runbooks documentados

---

### Sprint 4.3: Load Testing + Chaos Engineering
**Fecha:** 17-21 Marzo 2026  
**Tokens estimados:** ~35,000  
**Prioridad:** P2 (Medio)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 4.3.1 | K6 load tests | ~7,000 | Scripts por endpoint crítico | - |
| 4.3.2 | Baseline performance | ~7,000 | Establecer métricas base | 4.3.1 |
| 4.3.3 | Stress testing | ~7,000 | Encontrar límites | 4.3.2 |
| 4.3.4 | Chaos Monkey setup | ~7,000 | LitmusChaos o ChaosMesh | - |
| 4.3.5 | GameDays | ~7,000 | Simular fallos controlados | 4.3.4 |

**Entregables:**
- [ ] Suite de load tests
- [ ] Baseline documentado
- [ ] Primer GameDay ejecutado

---

### Sprint 4.4: DR Testing + Production Cutover
**Fecha:** 24-31 Marzo 2026  
**Tokens estimados:** ~40,000  
**Prioridad:** P0 (Crítico)

| ID | Tarea | Tokens | Descripción | Dependencia |
|----|-------|:------:|-------------|-------------|
| 4.4.1 | DR drill completo | ~8,000 | Simular pérdida de región | - |
| 4.4.2 | Restore testing | ~8,000 | Validar backups funcionan | 4.4.1 |
| 4.4.3 | Runbooks finales | ~8,000 | Documentación operativa | 4.4.1 |
| 4.4.4 | Production cutover plan | ~8,000 | Checklist de go-live | - |
| 4.4.5 | Go-Live | ~8,000 | Migración de tráfico real | 4.4.4 |

**Entregables:**
- [ ] DR probado y documentado
- [ ] Runbooks completos
- [ ] Sistema en producción

---

### 🎯 Milestone M4: Producción Enterprise (31 Marzo)

**Checklist de validación:**
- [ ] ✅ Prometheus + Grafana operativos
- [ ] ✅ 20+ alertas configuradas
- [ ] ✅ Load tests ejecutados
- [ ] ✅ DR probado
- [ ] ✅ Tráfico de producción activo

**Métricas objetivo:**
| KPI | Target M4 |
|-----|:---------:|
| Uptime | 99.9% |
| Response P95 | <200ms |
| MTTR | <15 min |
| Deploy frequency | 1/día |

---

## 📊 RESUMEN TOTAL

### Sprints por Fase

```
FASE 1: Seguridad y Estabilidad (6 sprints)
├── Sprint 1.1: Vault Setup
├── Sprint 1.2: SSL/TLS
├── Sprint 1.3: Rate Limiting
├── Sprint 1.4: Circuit Breakers
├── Sprint 1.5: Health Dashboard
└── Sprint 1.6: Logging Centralizado

FASE 2: Optimización (6 sprints)
├── Sprint 2.1: Redis Caching
├── Sprint 2.2: DB Optimization
├── Sprint 2.3: Response Compression
├── Sprint 2.4: Fusionar Media+FileStorage
├── Sprint 2.5: Fusionar Admin+User
└── Sprint 2.6: API Versioning

FASE 3: Kubernetes (8 sprints)
├── Sprint 3.1: Cluster Setup
├── Sprint 3.2: StatefulSets
├── Sprint 3.3: Core Services
├── Sprint 3.4: User & Product
├── Sprint 3.5: Infrastructure
├── Sprint 3.6: Business Batch 1
├── Sprint 3.7: Business Batch 2
└── Sprint 3.8: Remaining + Policies

FASE 4: Go-Live (4 sprints)
├── Sprint 4.1: Prometheus + Grafana
├── Sprint 4.2: Alerting
├── Sprint 4.3: Load Testing
└── Sprint 4.4: DR + Cutover
```

### Timeline Visual

```
Enero 2026
├── Sem 1 (6-10):   Sprint 1.1, 1.2, 1.3
├── Sem 2 (13-17):  Sprint 1.4, 1.5, 1.6
│                    └── 🎯 M1: Producción Básica (15 Ene)
├── Sem 3 (20-24):  Sprint 2.1, 2.2, 2.3
└── Sem 4 (27-31):  Sprint 2.4, 2.5, 2.6
                     └── 🎯 M2: Optimización (31 Ene)

Febrero 2026
├── Sem 5 (3-7):    Sprint 3.1, 3.2
├── Sem 6 (10-14):  Sprint 3.3, 3.4
├── Sem 7 (17-21):  Sprint 3.5, 3.6
└── Sem 8 (24-28):  Sprint 3.7, 3.8
                     └── 🎯 M3: Kubernetes Ready (28 Feb)

Marzo 2026
├── Sem 9 (3-7):    Sprint 4.1
├── Sem 10 (10-14): Sprint 4.2
├── Sem 11 (17-21): Sprint 4.3
└── Sem 12 (24-31): Sprint 4.4
                     └── 🎯 M4: Producción Enterprise (31 Mar)
```

---

## 📝 DEPENDENCIAS Y RIESGOS

### Dependencias Críticas

| Sprint | Depende de | Riesgo si falla |
|--------|------------|-----------------|
| 1.1 Vault | - | 🔴 Secretos expuestos |
| 1.2 SSL | 1.1 | 🔴 Tráfico inseguro |
| 3.1 K8s | M2 completo | 🟠 Retraso migración |
| 4.4 Go-Live | M3 completo | 🔴 No hay producción |

### Riesgos Identificados

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|:------------:|:-------:|------------|
| Falta de recursos DevOps | Media | Alto | Contratar/capacitar |
| Incompatibilidad K8s | Baja | Alto | POC temprano |
| Datos inconsistentes en migración | Media | Alto | Backup + validación |
| Downtime durante cutover | Media | Medio | Blue-green deployment |

---

## ✅ PRÓXIMOS PASOS

1. **Inmediato (Día 1):** Iniciar Sprint 1.1 - Vault Setup
2. **Esta semana:** Completar M1 requirements (Sprints 1.1-1.6)
3. **Antes del 15 Ene:** Validar M1 y preparar M2

**Recursos requeridos:**
- 1 DevOps Engineer (full-time)
- 1 Backend Developer (50%)
- 1 Security Engineer (20%)
- Infraestructura cloud ($500-1000/mes)

---

*Documento creado: 2 Enero 2026*  
*Próxima revisión: 15 Enero 2026 (M1)*
