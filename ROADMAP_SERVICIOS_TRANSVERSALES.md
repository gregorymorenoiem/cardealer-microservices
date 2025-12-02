# 🗺️ Roadmap Completo - Servicios Transversales CarDealer Microservices

**Fecha:** 1 de diciembre de 2025  
**Proyecto:** CarDealer Microservices  
**Objetivo:** Implementar todos los servicios transversales faltantes  
**Tiempo Total Estimado:** ~120 horas (~15 días laborables)

---

## 📊 Estado Actual de Servicios

### ✅ **Servicios Implementados:**
- AuthService (Autenticación/Autorización)
- UserService (Gestión de usuarios)
- RoleService (RBAC)
- AdminService (Moderación + Tests)
- MediaService (Gestión de archivos)
- NotificationService (Email notifications)
- AuditService (Registro de auditoría)
- ErrorService (Tracking de errores + Rate Limiting básico)
- Gateway (API Gateway básico)
- VehicleService
- ContactService

### ⚠️ **Servicios Parciales (Necesitan Mejoras):**
1. **Gateway** - Requiere: Rate Limiting global, Circuit Breaker, caché distribuido
2. **ErrorService** - Tiene Rate Limiting básico, necesita distribución con Redis
3. **MediaService** - Falta: CDN integration, virus scanning, storage abstracto

### ❌ **Servicios Faltantes (17 servicios):**

---

## 🎯 Priorización de Servicios

### 🔴 **ALTA PRIORIDAD** (Implementar primero - 5 servicios)

| # | Servicio | Tiempo Estimado | Justificación |
|---|----------|-----------------|---------------|
| 1 | Message Bus Service | 7h | Desacoplar comunicación entre servicios |
| 2 | Configuration Service | 7h | Gestión centralizada de configs y secrets |
| 3 | Distributed Cache Service (Redis) | 6h | Performance y session management |
| 4 | Centralized Logging Service | 8h | Debugging esencial en producción |
| 5 | Service Discovery Service | 8h | Escalabilidad y eliminación de URLs hardcoded |

**Subtotal Alta Prioridad: ~36 horas (~4.5 días)**

---

### 🟡 **MEDIA PRIORIDAD** (Observabilidad y Operaciones - 4 servicios)

| # | Servicio | Tiempo Estimado | Justificación |
|---|----------|-----------------|---------------|
| 6 | Distributed Tracing Service | 7h | Observabilidad de flujos entre servicios |
| 7 | Metrics & Monitoring Service | 8h | Proactividad y alertas tempranas |
| 8 | Schxzeduler Service | 6h | Automatización de tareas recurrentes |
| 9 | Health Check Aggregator | 5h | Monitoreo centralizado de salud |

**Subtotal Media Prioridad: ~26 horas (~3.25 días)**

---

### 🟢 **BAJA PRIORIDAD** (Según necesidad - 8 servicios)

| # | Servicio | Tiempo Estimado | Justificación |
|---|----------|-----------------|---------------|
| 10 | ✅ Search Service (Elasticsearch) | 9h | Cuando aumente el volumen de datos |
| 11 | ✅ Feature Toggle Service | 5h | Para CD/CI avanzado y A/B testing |
| 12 | ✅ API Documentation Aggregator | 4h | Developer experience |
| 13 | ✅ Idempotency Service | 6h | Prevenir operaciones duplicadas críticas |
| 14 | ✅ Rate Limiting Service (Distribuido) | 5h | Mejorar el actual con Redis |
| 15 | ✅ Backup & DR Service | 10h | Disaster recovery automatizado |
| 16 | File Storage Service (Mejorar MediaService) | 7h | CDN, virus scan, storage abstracto |
| 17 | Circuit Breaker Service | 6h | Resilience patterns avanzados |

**Subtotal Baja Prioridad: ~52 horas (~6.5 días)**

---

## 📋 Plan Detallado por Servicio

---

## 🔴 **ALTA PRIORIDAD**

---

### **1. Message Bus Service** ⏱️ 7 horas

**Propósito:** Comunicación asíncrona entre servicios mediante RabbitMQ

#### **Funcionalidades:**
- ✉️ Publicación/Suscripción a topics
- 🔄 Retry logic con backoff exponencial
- 💀 Dead Letter Queue para mensajes fallidos
- 📊 Tracking de mensajes (Pending, Processing, Completed, Failed)
- 🔔 Event broadcasting para eventos de dominio
- 📦 Batch publishing

#### **Tareas:**

| Tarea | Tiempo |
|-------|--------|
| Diseñar arquitectura + elegir RabbitMQ | 15 min |
| Capa de Dominio (Message, Subscription, DeadLetterMessage) | 20 min |
| Capa de Aplicación (IMessagePublisher, comandos CQRS) | 30 min |
| Capa de Infraestructura (RabbitMQ client, EF Core) | 35 min |
| API Controllers (Messages, Subscriptions, DeadLetter) | 25 min |
| Configuración (appsettings, DI, MediatR) | 15 min |
| Tests unitarios (10+ tests) | 30 min |
| Docker compose + integración | 20 min |
| Git commit + documentación | 15 min |

**Stack:** RabbitMQ 3.x, EF Core, MediatR, PostgreSQL

---

### **2. Configuration Service** ⏱️ 7 horas

**Propósito:** Gestión centralizada de configuraciones, secrets y feature flags

#### **Funcionalidades:**
- ⚙️ Configuraciones por entorno (Dev/Staging/Prod)
- 🔐 Secrets encriptados con AES-256
- 🎚️ Feature flags con rollout porcentual
- 📜 Historial de cambios (auditoría)
- 🔄 Hot reload sin reiniciar servicios
- 🌍 Multi-tenant support
- 📤 Import/Export JSON/YAML

#### **Tareas:**

| Tarea | Tiempo |
|-------|--------|
| Diseñar arquitectura + modelo de datos | 15 min |
| Capa de Dominio (ConfigurationItem, EncryptedSecret, FeatureFlag) | 20 min |
| Capa de Aplicación (IConfigurationManager, ISecretManager) | 30 min |
| Capa de Infraestructura (AES encryption, EF Core) | 40 min |
| API Controllers (Configurations, Secrets, FeatureFlags) | 25 min |
| Configuración (PostgreSQL, encryption keys, DI) | 20 min |
| Tests unitarios (12+ tests) | 35 min |
| Docker compose + integración | 20 min |
| Git commit + documentación | 15 min |

**Stack:** AES-256, PostgreSQL, EF Core, MediatR, (Opcional) Azure Key Vault

---

### **3. Distributed Cache Service (Redis)** ⏱️ 6 horas

**Propósito:** Caché distribuido para performance y session storage

#### **Funcionalidades:**
- 🚀 Response caching distribuido
- 👤 Session storage
- 🔢 Rate limiting counters
- 🔒 Distributed locks
- 🗄️ Database query caching
- ⏱️ TTL configurable por key
- 📊 Cache statistics y monitoring

#### **Tareas:**

| Tarea | Tiempo |
|-------|--------|
| Diseñar arquitectura de caché + políticas TTL | 15 min |
| Capa de Dominio (CacheEntry, CacheStatistics) | 15 min |
| Capa de Aplicación (ICacheManager, comandos) | 25 min |
| Capa de Infraestructura (StackExchange.Redis) | 40 min |
| API Controllers (Cache management endpoints) | 20 min |
| Configuración (Redis connection, DI) | 15 min |
| Tests unitarios (8+ tests) | 25 min |
| Integrar con servicios existentes (UserService ejemplo) | 30 min |
| Docker compose + Redis container | 15 min |
| Git commit + documentación | 20 min |

**Stack:** Redis 7.x, StackExchange.Redis, ASP.NET Core

---

### **4. Centralized Logging Service** ⏱️ 8 horas

**Propósito:** Agregación y análisis centralizado de logs

#### **Funcionalidades:**
- 📝 Agregación de logs de todos los servicios
- 🔍 Búsqueda full-text y filtrado avanzado
- 🔗 Correlación por RequestId/TraceId
- 🚨 Alertas basadas en patrones de logs
- 📊 Dashboards de visualización
- 🗂️ Retención y archivado configurable
- 📈 Análisis de tendencias

#### **Tareas:**

| Tarea | Tiempo |
|-------|--------|
| Diseñar arquitectura (Seq o ELK Stack) | 20 min |
| Configurar Seq/Elasticsearch + Kibana | 40 min |
| Capa de Dominio (LogEntry, LogLevel, LogSource) | 20 min |
| Capa de Aplicación (ILogAggregator, queries) | 30 min |
| Capa de Infraestructura (Serilog sinks, Seq client) | 45 min |
| API Controllers (Logs query endpoints) | 25 min |
| Configuración (Serilog, structured logging) | 30 min |
| Integrar Serilog en todos los servicios existentes | 60 min |
| Tests unitarios (8+ tests) | 25 min |
| Docker compose + Seq container | 20 min |
| Git commit + documentación | 25 min |

**Stack:** Seq (o ELK), Serilog, ASP.NET Core, Elasticsearch (opcional)

---

### **5. Service Discovery Service** ⏱️ 8 horas

**Propósito:** Registro y descubrimiento dinámico de servicios

#### **Funcionalidades:**
- 🔍 Service registration automático
- 💓 Health checks distribuidos
- ⚖️ Load balancing dinámico
- 🔄 Failover automático
- 📍 DNS-based service discovery
- 🗺️ Service mesh básico
- 📊 Service catalog

#### **Tareas:**

| Tarea | Tiempo |
|-------|--------|
| Diseñar arquitectura (Consul o Eureka) | 20 min |
| Configurar Consul server + agents | 40 min |
| Capa de Dominio (ServiceInstance, HealthStatus) | 20 min |
| Capa de Aplicación (IServiceRegistry, IServiceDiscovery) | 35 min |
| Capa de Infraestructura (Consul client integration) | 50 min |
| API Controllers (Service registry endpoints) | 25 min |
| Configuración (Consul connection, health endpoints) | 30 min |
| Modificar servicios existentes para auto-registro | 70 min |
| Tests unitarios (10+ tests) | 30 min |
| Docker compose + Consul container | 25 min |
| Git commit + documentación | 25 min |

**Stack:** Consul (o Eureka), ASP.NET Core Health Checks, Docker

---

## 🟡 **MEDIA PRIORIDAD**

---

### **6. Distributed Tracing Service** ⏱️ 7 horas

**Propósito:** Seguimiento de requests a través de múltiples servicios

#### **Funcionalidades:**
- 🔗 Trace propagation (W3C Trace Context)
- 📊 Visualización de flujos de requests
- ⏱️ Análisis de latencia entre servicios
- 🐛 Debugging de sistemas distribuidos
- 📈 Performance insights
- 🚨 Alertas de anomalías de latencia

#### **Tareas:**

| Tarea | Tiempo |
|-------|--------|
| Diseñar arquitectura (Jaeger o Zipkin) | 20 min |
| Configurar Jaeger server + collectors | 35 min |
| Capa de Dominio (Span, Trace, TraceContext) | 20 min |
| Capa de Aplicación (ITracer, trace propagation) | 30 min |
| Capa de Infraestructura (OpenTelemetry integration) | 50 min |
| Instrumentar servicios existentes (middleware) | 70 min |
| API Controllers (Tracing query endpoints) | 20 min |
| Configuración (OpenTelemetry, exporters) | 25 min |
| Tests unitarios (8+ tests) | 25 min |
| Docker compose + Jaeger container | 20 min |
| Git commit + documentación | 25 min |

**Stack:** Jaeger (o Zipkin), OpenTelemetry, ASP.NET Core

---

### **7. Metrics & Monitoring Service** ⏱️ 8 horas

**Propósito:** Recolección y visualización de métricas técnicas y de negocio

#### **Funcionalidades:**
- 📊 Métricas RED (Rate, Errors, Duration)
- 💻 Métricas USE (Utilization, Saturation, Errors)
- 📈 Business metrics customizadas
- 🚨 Alertas configurables
- 📉 Dashboards interactivos
- 🔔 Integración con canales de notificación

#### **Tareas:**

| Tarea | Tiempo |
|-------|--------|
| Diseñar arquitectura (Prometheus + Grafana) | 20 min |
| Configurar Prometheus + Grafana + Alertmanager | 45 min |
| Capa de Dominio (Metric, MetricType, Alert) | 20 min |
| Capa de Aplicación (IMetricsCollector, queries) | 30 min |
| Capa de Infraestructura (Prometheus exporter) | 40 min |
| Instrumentar servicios con métricas customizadas | 80 min |
| Crear dashboards Grafana (5+ dashboards) | 60 min |
| Configurar alertas (CPU, memory, error rate) | 30 min |
| Tests unitarios (8+ tests) | 25 min |
| Docker compose + Prometheus + Grafana | 25 min |
| Git commit + documentación | 25 min |

**Stack:** Prometheus, Grafana, Alertmanager, ASP.NET Core

---

### **8. Scheduler Service** ✅ 6 horas - **COMPLETADO**

**Propósito:** Ejecución de tareas programadas y jobs recurrentes

#### **Funcionalidades:**
- ⏰ Cron jobs distribuidos
- 🔄 Tareas recurrentes (limpieza, reportes, notificaciones)
- 🎯 Job orchestration
- ♻️ Retry logic y error handling
- 📊 Job execution history
- 🚫 Job cancellation

#### **Tareas:**

| Tarea | Tiempo | Estado |
|-------|--------|--------|
| Diseñar arquitectura (Hangfire o Quartz.NET) | 15 min | ✅ |
| Capa de Dominio (Job, JobExecution, JobSchedule) | 20 min | ✅ |
| Capa de Aplicación (IJobScheduler, comandos) | 30 min | ✅ |
| Capa de Infraestructura (Hangfire integration) | 40 min | ✅ |
| API Controllers (Jobs management endpoints) | 25 min | ✅ |
| Configuración (Hangfire dashboard, PostgreSQL) | 20 min | ✅ |
| Crear jobs de ejemplo (cleanup, reports) | 30 min | ✅ |
| Tests unitarios (8+ tests) | 25 min | ✅ |
| Docker compose + integración | 15 min | ✅ |
| Git commit + documentación | 20 min | ✅ |

**Stack:** Hangfire 1.8.14, PostgreSQL, ASP.NET Core 8.0, MediatR

**Implementación:**
- ✅ Clean Architecture con 4 capas
- ✅ 7 Comandos CQRS (Create, Update, Delete, Enable, Disable, Pause, Trigger)
- ✅ 6 Queries (GetById, GetAll, GetActive, GetExecutions, etc.)
- ✅ Hangfire Dashboard en `/hangfire`
- ✅ 3 Jobs de ejemplo (CleanupOldExecutionsJob, DailyStatsReportJob, HealthCheckJob)
- ✅ 17 Tests unitarios pasando
- ✅ EF Core con PostgreSQL + Migraciones
- ✅ Docker + docker-compose configurado
- ✅ README completo con documentación API

---

### **9. Health Check Aggregator** ✅ 5 horas - **COMPLETADO**

**Propósito:** Monitoreo centralizado de salud de todos los servicios

#### **Funcionalidades:**
- 💓 Health endpoints en todos los servicios
- 📊 Dashboard de estado general del sistema
- 🔍 Dependency checks (DB, external APIs, Redis, RabbitMQ)
- ⚠️ Degraded states
- 🚨 Alertas de servicios caídos
- 📈 Uptime tracking

#### **Tareas:**

| Tarea | Tiempo | Estado |
|-------|--------|--------|
| Diseñar arquitectura de health checks | 15 min | ✅ |
| Capa de Dominio (HealthCheck, HealthStatus, Dependency) | 15 min | ✅ |
| Capa de Aplicación (IHealthAggregator, queries) | 25 min | ✅ |
| Capa de Infraestructura (Health check probes) | 30 min | ✅ |
| API Controllers (Health aggregator endpoint) | 20 min | ✅ |
| Configuración (health check UI, intervalos) | 20 min | ✅ |
| Agregar health endpoints en todos los servicios | 50 min | ⏳ |
| Dashboard UI para visualización | 30 min | ⏳ |
| Tests unitarios (6+ tests) | 20 min | ✅ |
| Docker compose health checks | 15 min | ✅ |
| Git commit + documentación | 20 min | ✅ |

**Stack:** HttpClient, ASP.NET Core 8.0, MediatR

**Implementación:**
- ✅ Clean Architecture con 4 capas
- ✅ 3 Entidades (SystemHealth, ServiceHealth, DependencyHealth)
- ✅ 2 Enums (HealthStatus, DependencyType)
- ✅ 3 Queries (GetSystemHealth, GetServiceHealth, GetRegisteredServices)
- ✅ HttpHealthChecker para verificación vía HTTP
- ✅ HealthAggregator con verificación paralela
- ✅ 13 Tests unitarios pasando
- ✅ API RESTful con 4 endpoints
- ✅ Docker + docker-compose configurado (puerto 15092)
- ✅ README completo con casos de uso y alertas
- ⏳ Pendiente: Agregar health endpoints en servicios legacy
- ⏳ Pendiente: Dashboard UI (opcional)

---

## 🟢 **BAJA PRIORIDAD**

---

### **10. ✅ Search Service (Elasticsearch)** ⏱️ 9 horas **COMPLETADO**

**Propósito:** Búsqueda avanzada y full-text search

**Estado**: ✅ **Implementado completamente** (2 diciembre 2025)

#### **Funcionalidades Implementadas:**
- ✅ Full-text search con NEST 7.17.5 y Elasticsearch 8.11
- ✅ Fuzzy matching tolerante a errores
- ✅ Exact match, Wildcard y Prefix search
- ✅ Highlighting de resultados
- ✅ Paginación eficiente
- ✅ Sorting por relevancia o campos custom
- ✅ Bulk indexing de documentos
- ✅ Index management completo (crear, eliminar, reindexar)
- ✅ Statistics y metadata de índices

#### **Implementación:**

**Clean Architecture (4 capas):**
- `SearchService.Domain` - 2 entidades, 2 value objects, 3 enums, 2 interfaces
- `SearchService.Application` - 4 queries, 6 commands, 3 handlers con MediatR
- `SearchService.Infrastructure` - ElasticsearchRepository (NEST), IndexManager
- `SearchService.Api` - 3 controllers (Search, Index, Stats), 15+ endpoints

**Stack Técnico:**
- ASP.NET Core 8.0
- Elasticsearch 8.11.0
- NEST 7.17.5 (cliente oficial .NET)
- MediatR 12.4.1
- Docker multi-stage build
- 14 unit tests (todos pasando)

**Endpoints Principales:**
- `POST /api/search/query` - Búsqueda avanzada con filtros
- `GET /api/search/{index}/{id}` - Obtener documento
- `POST /api/index/{index}/document` - Indexar documento
- `POST /api/index/{index}/bulk` - Indexación masiva
- `GET /api/stats/{index}` - Estadísticas del índice

**Docker:**
- Puerto: 15093
- Elasticsearch: 9200 (single-node, sin seguridad para dev)
- Health checks configurados
- Volume persistence: `elasticsearch_data`

**Documentación:**
- README.md completo (200+ líneas)
- Ejemplos de uso para vehículos, usuarios, contactos
- Guía de troubleshooting
- Performance benchmarks

**Pendientes (opcionales):**
- [ ] Implementar sinónimos en búsquedas
- [ ] Dashboard de analytics con Kibana
- [ ] Machine Learning ranking (Learning to Rank)
- [ ] Faceted search para filtros dinámicos avanzados

---

### **11. Feature Toggle Service** ✅ 5 horas - **COMPLETADO**

**Propósito:** Activación/desactivación de features en runtime

**Estado**: ✅ **Implementado completamente** (14 enero 2025)

#### **Funcionalidades Implementadas:**
- ✅ Feature flags por entorno (Development, Staging, Production)
- ✅ Gradual rollouts con porcentaje configurable
- ✅ Feature flags por usuario específico
- ✅ Evaluación de flags con contexto (userId, environment, attributes)
- ✅ CRUD completo de feature flags
- ✅ Historial de cambios (auditoría)
- ✅ Cache con Redis para rendimiento
- ✅ Seed data con flags predeterminados

#### **Implementación:**

**Clean Architecture (4 capas):**
- `FeatureToggleService.Domain` - 3 entidades (FeatureFlag, FeatureFlagRule, FlagEvaluation), 3 enums, 2 interfaces
- `FeatureToggleService.Application` - 5 comandos CQRS, 4 queries, MediatR handlers
- `FeatureToggleService.Infrastructure` - EF Core PostgreSQL, Repository, Seeder
- `FeatureToggleService.Api` - FeatureFlagsController (9 endpoints)

**Stack Técnico:**
- ASP.NET Core 8.0
- Entity Framework Core 8.0.0 + PostgreSQL
- MediatR 12.4.1
- Redis para caché
- 29 unit tests (todos pasando)

**Endpoints:**
- `GET /api/feature-flags` - Lista todos los flags
- `GET /api/feature-flags/{id}` - Obtiene un flag
- `GET /api/feature-flags/key/{key}` - Obtiene flag por clave
- `POST /api/feature-flags` - Crea un flag
- `PUT /api/feature-flags/{id}` - Actualiza un flag
- `DELETE /api/feature-flags/{id}` - Elimina un flag
- `POST /api/feature-flags/{id}/toggle` - Activa/desactiva
- `POST /api/feature-flags/evaluate` - Evalúa un flag
- `GET /api/feature-flags/environment/{env}` - Flags por entorno

**Docker:**
- Puerto: 15094
- Container: featuretoggleservice
- PostgreSQL: featuretoggleservice-db (puerto 25441)
- Dependencias: PostgreSQL, Redis

#### **Tareas Completadas:**

| Tarea | Tiempo | Estado |
|-------|--------|--------|
| Diseñar arquitectura de feature flags | 15 min | ✅ |
| Capa de Dominio (FeatureFlag, FeatureFlagRule, FlagEvaluation) | 20 min | ✅ |
| Capa de Aplicación (5 comandos, 4 queries CQRS) | 30 min | ✅ |
| Capa de Infraestructura (EF Core, Repository, Seeder) | 35 min | ✅ |
| API Controller (9 endpoints) | 25 min | ✅ |
| Configuración (PostgreSQL, Redis, DI) | 20 min | ✅ |
| 29 Unit tests | 35 min | ✅ |
| Dockerfile multi-stage | 15 min | ✅ |
| docker-compose integration | 15 min | ✅ |
| README.md documentación | 20 min | ✅ |

**Stack:** PostgreSQL, Redis (cache), EF Core, MediatR, ASP.NET Core 8.0

---

### **12. API Documentation Aggregator** ✅ 4 horas - **COMPLETADO**

**Propósito:** Documentación centralizada de todas las APIs

**Estado**: ✅ **Implementado completamente** (14 enero 2025)

#### **Funcionalidades Implementadas:**
- ✅ Swagger/OpenAPI aggregator desde múltiples servicios
- ✅ Service discovery y estado de documentación
- ✅ Búsqueda de endpoints por nombre/descripción
- ✅ Caché de especificaciones OpenAPI
- ✅ Health checks de servicios documentados
- ✅ Refresh manual de documentación
- ✅ Extracción automática de endpoints desde specs

#### **Implementación:**

**Arquitectura Simplificada (3 capas):**
- `ApiDocsService.Core` - Modelos (ServiceInfo, EndpointInfo), Interfaces (IApiAggregatorService), Services (ApiAggregatorService)
- `ApiDocsService.Api` - DocsController (7 endpoints), Program.cs con DI
- `ApiDocsService.Tests` - 19 unit tests con Moq

**Stack Técnico:**
- ASP.NET Core 8.0
- HttpClient (fetching de specs)
- IMemoryCache (caché de documentación)
- Serilog 8.0.0 (logging)
- Swashbuckle 6.5.0 (Swagger UI propio)
- 19 unit tests (todos pasando)

**Endpoints:**
- `GET /api/docs/services` - Lista todos los servicios
- `GET /api/docs/services/{name}` - Info de un servicio
- `GET /api/docs/services/{name}/spec` - Spec OpenAPI de un servicio
- `GET /api/docs/aggregated` - Spec agregada de todos los servicios
- `POST /api/docs/refresh` - Actualizar caché
- `GET /api/docs/health` - Estado de salud de servicios
- `GET /api/docs/search?query={term}` - Buscar endpoints

**Docker:**
- Puerto: 15095
- Container: apidocsservice
- Sin dependencias de BD (solo HTTP)

**Documentación:**
- README.md completo con ejemplos
- Configuración de servicios en appsettings.json

#### **Tareas Completadas:**

| Tarea | Tiempo | Estado |
|-------|--------|--------|
| Diseñar arquitectura de agregación | 15 min | ✅ |
| Core Models y Interfaces | 20 min | ✅ |
| ApiAggregatorService implementation | 30 min | ✅ |
| DocsController con 7 endpoints | 25 min | ✅ |
| Configuración appsettings con 15 servicios | 15 min | ✅ |
| 19 Unit tests | 30 min | ✅ |
| Dockerfile multi-stage | 10 min | ✅ |
| docker-compose integration | 15 min | ✅ |
| README.md documentación | 15 min | ✅ |
| Git commit + solución | 15 min | ✅ |

**Stack:** Swashbuckle, HttpClient, IMemoryCache, ASP.NET Core 8.0

---

### **13. Idempotency Service** ✅ 6 horas - **COMPLETADO**

**Propósito:** Prevenir operaciones duplicadas en requests críticos

**Estado**: ✅ **Implementado completamente** (2 diciembre 2025)

#### **Funcionalidades Implementadas:**
- ✅ RequestId-based deduplication con header X-Idempotency-Key
- ✅ TTL configurable (1 min - 7 días, default 24h)
- ✅ Almacenamiento distribuido con Redis
- ✅ Middleware ASP.NET Core para verificación automática
- ✅ Detección de conflictos por hash de request
- ✅ Estadísticas de duplicados bloqueados
- ✅ Estados: Processing, Completed, Failed

#### **Implementación:**

**Arquitectura Simplificada (3 capas):**
- `IdempotencyService.Core` - Models (IdempotencyRecord, IdempotencyCheckResult, IdempotencyOptions), Interfaces (IIdempotencyService), Services (RedisIdempotencyService)
- `IdempotencyService.Api` - IdempotencyController (6 endpoints), IdempotencyMiddleware
- `IdempotencyService.Tests` - 22 unit tests con Moq

**Stack Técnico:**
- ASP.NET Core 8.0
- StackExchange.Redis (IDistributedCache)
- Serilog 8.0.0 (logging)
- Swashbuckle 6.5.0 (Swagger)
- 22 unit tests (todos pasando)

**Endpoints:**
- `GET /api/idempotency/{key}` - Obtener registro
- `POST /api/idempotency/check` - Verificar estado
- `POST /api/idempotency` - Crear registro manual
- `DELETE /api/idempotency/{key}` - Eliminar registro
- `GET /api/idempotency/stats` - Estadísticas
- `POST /api/idempotency/cleanup` - Limpieza manual

**Docker:**
- Puerto: 15096
- Container: idempotencyservice
- Dependencias: Redis

#### **Tareas Completadas:**

| Tarea | Tiempo | Estado |
|-------|--------|--------|
| Diseñar arquitectura de idempotency | 15 min | ✅ |
| Models (IdempotencyRecord, CheckResult, Options) | 20 min | ✅ |
| IIdempotencyService interface | 10 min | ✅ |
| RedisIdempotencyService implementation | 35 min | ✅ |
| IdempotencyMiddleware | 30 min | ✅ |
| IdempotencyController (6 endpoints) | 25 min | ✅ |
| Configuración (Redis, appsettings) | 15 min | ✅ |
| 22 Unit tests | 30 min | ✅ |
| Dockerfile + docker-compose | 15 min | ✅ |
| README.md documentación | 20 min | ✅ |

**Stack:** Redis, StackExchange.Redis, ASP.NET Core Middleware, IDistributedCache

---

### **14. Rate Limiting Service (Distribuido)** ⏱️ 5 horas

**Propósito:** Rate limiting distribuido mejorado con Redis

#### **Funcionalidades:**
- 🚦 Rate limiting por IP, usuario, API key
- 📊 Cuotas por tier de usuario (free, premium, enterprise)
- ⏱️ Sliding window algorithm
- 🌍 Rate limiting global (cross-service)
- 📈 Analytics de uso

#### **Tareas:**

| Tarea | Tiempo |
|-------|--------|
| Diseñar arquitectura distribuida con Redis | 15 min |
| Capa de Dominio (RateLimitPolicy, Quota) | 15 min |
| Capa de Aplicación (IRateLimiter, algoritmos) | 30 min |
| Capa de Infraestructura (Redis counters) | 35 min |
| Middleware de rate limiting | 30 min |
| API Controllers (Rate limit management) | 20 min |
| Configuración (policies por endpoint) | 20 min |
| Migrar rate limiting de ErrorService | 25 min |
| Tests unitarios (8+ tests) | 25 min |
| Git commit + documentación | 15 min |

**Stack:** Redis, ASP.NET Core Middleware, Sliding Window

---

### **15. Backup & DR Service** ✅ 10 horas - **COMPLETADO**

**Propósito:** Backups automatizados y disaster recovery

**Estado**: ✅ **Implementado completamente** (2 diciembre 2025)

#### **Funcionalidades Implementadas:**
- ✅ Database backups automatizados (PostgreSQL con pg_dump)
- ✅ Point-in-time recovery con restore points
- ✅ Backup types: Full, Incremental, Differential
- ✅ Storage providers: Local filesystem, Azure Blob (extensible)
- ✅ Scheduling con expresiones Cron (Cronos)
- ✅ Verificación de integridad con checksums SHA-256
- ✅ Compresión de backups
- ✅ Retención configurable con cleanup automático
- ✅ Estadísticas de backups y restores

#### **Implementación:**

**Arquitectura Simplificada (3 capas):**
- `BackupDRService.Core` - Models (BackupJob, BackupResult, RestorePoint, RestoreResult, BackupOptions, BackupStatistics), Interfaces (IBackupService, IRestoreService, IStorageProvider, IDatabaseBackupProvider), Services (BackupService, RestoreService, LocalStorageProvider, PostgreSqlBackupProvider)
- `BackupDRService.Api` - 2 Controllers (BackupController, RestoreController), Program.cs con DI
- `BackupDRService.Tests` - 85 unit tests (models, services, controllers)

**Stack Técnico:**
- ASP.NET Core 8.0
- PostgreSQL pg_dump/pg_restore
- Npgsql 8.0.5
- Azure.Storage.Blobs 12.19.1 (extensible)
- Cronos 0.8.4 (cron expressions)
- Serilog 8.0.0 (logging)
- 85 unit tests (todos pasando)

**Endpoints de Backup:**
- `GET /api/backup/jobs` - Lista todos los jobs
- `GET /api/backup/jobs/enabled` - Lista jobs habilitados
- `GET /api/backup/jobs/{id}` - Obtiene un job
- `GET /api/backup/jobs/by-name/{name}` - Busca job por nombre
- `POST /api/backup/jobs` - Crea un job
- `PUT /api/backup/jobs/{id}` - Actualiza un job
- `DELETE /api/backup/jobs/{id}` - Elimina un job
- `POST /api/backup/jobs/{id}/enable` - Habilita un job
- `POST /api/backup/jobs/{id}/disable` - Deshabilita un job
- `POST /api/backup/jobs/{id}/execute` - Ejecuta backup manualmente
- `POST /api/backup/results/{id}/cancel` - Cancela backup en ejecución
- `GET /api/backup/results` - Resultados recientes
- `GET /api/backup/jobs/{jobId}/results` - Resultados por job
- `GET /api/backup/results/{id}` - Obtiene un resultado
- `GET /api/backup/results/by-date` - Resultados por rango de fechas
- `POST /api/backup/results/{id}/verify` - Verifica integridad
- `GET /api/backup/statistics` - Estadísticas del sistema
- `POST /api/backup/cleanup` - Limpia backups expirados

**Endpoints de Restore:**
- `GET /api/restore/points` - Lista restore points
- `GET /api/restore/points/available` - Lista points disponibles
- `GET /api/restore/points/{id}` - Obtiene un point
- `POST /api/restore/points` - Crea restore point
- `DELETE /api/restore/points/{id}` - Elimina point
- `POST /api/restore/points/{id}/verify` - Verifica point
- `POST /api/restore/points/{id}/test` - Testea point
- `POST /api/restore/points/{id}/restore` - Restaura desde point
- `POST /api/restore/from-backup/{backupResultId}` - Restaura desde backup
- `POST /api/restore/results/{id}/cancel` - Cancela restauración
- `GET /api/restore/results` - Lista resultados
- `GET /api/restore/results/recent` - Resultados recientes
- `POST /api/restore/cleanup` - Limpia points expirados

**Docker:**
- Puerto: 15098
- Container: backupdrservice
- Volume: backupdr_data para persistencia
- PostgreSQL client tools incluidos en imagen

#### **Tareas Completadas:**

| Tarea | Tiempo | Estado |
|-------|--------|--------|
| Diseñar estrategia de backup y DR | 30 min | ✅ |
| Capa de Dominio (6 Models, 4 Interfaces) | 30 min | ✅ |
| Capa de Aplicación (4 Services) | 45 min | ✅ |
| Capa de API (2 Controllers, 32 endpoints) | 40 min | ✅ |
| Configuración (appsettings, DI) | 20 min | ✅ |
| 85 Unit tests | 60 min | ✅ |
| Dockerfile + docker-compose | 20 min | ✅ |
| README.md documentación | 25 min | ✅ |
| Git commit + solución | 15 min | ✅ |

**Stack:** PostgreSQL pg_dump/pg_restore, Azure Blob Storage (extensible), Cronos, Serilog

---

### **16. File Storage Service (Mejorar MediaService)** ⏱️ 7 horas

**Propósito:** Mejorar MediaService con storage abstracto y CDN

#### **Funcionalidades:**
- ☁️ Storage abstracto (Azure Blob, S3, local)
- 🌐 CDN integration
- 🖼️ Image optimization y thumbnails
- 🦠 Virus scanning (ClamAV)
- 📋 Metadata extraction (EXIF, video info)
- 🔗 Presigned URLs para acceso temporal

#### **Tareas:**

| Tarea | Tiempo |
|-------|--------|
| Diseñar arquitectura de storage abstracto | 20 min |
| Capa de Aplicación (IStorageProvider interface) | 25 min |
| Implementar AzureBlobStorageProvider | 40 min |
| Implementar LocalFileStorageProvider | 30 min |
| Integrar CDN (Azure CDN o CloudFlare) | 35 min |
| Image optimization (ImageSharp) | 40 min |
| Virus scanning con ClamAV | 45 min |
| Presigned URLs y tokens temporales | 30 min |
| Metadata extraction | 25 min |
| Migrar MediaService existente | 40 min |
| Tests unitarios (10+ tests) | 30 min |
| Git commit + documentación | 20 min |

**Stack:** Azure Blob Storage, ImageSharp, ClamAV, CDN

---

### **17. Circuit Breaker Service** ⏱️ 6 horas

**Propósito:** Resilience patterns para llamadas a servicios externos

#### **Funcionalidades:**
- 🔌 Circuit breaker pattern
- ♻️ Retry con backoff exponencial
- ⏱️ Timeout policies
- 🛡️ Bulkhead isolation
- 📊 Circuit state monitoring
- 🚨 Alertas de circuitos abiertos

#### **Tareas:**

| Tarea | Tiempo |
|-------|--------|
| Diseñar arquitectura con Polly | 15 min |
| Capa de Dominio (CircuitState, Policy) | 15 min |
| Capa de Aplicación (IResilienceService, policies) | 30 min |
| Capa de Infraestructura (Polly integration) | 40 min |
| Middleware para HTTP clients | 30 min |
| API Controllers (Circuit monitoring endpoints) | 20 min |
| Configuración (policies por servicio) | 20 min |
| Aplicar a HttpClients existentes | 50 min |
| Dashboard de estado de circuitos | 30 min |
| Tests unitarios (8+ tests) | 25 min |
| Git commit + documentación | 15 min |

**Stack:** Polly, ASP.NET Core, SignalR (dashboard)

---

## ⏰ **RESUMEN DE TIEMPOS**

### **Por Prioridad:**

| Prioridad | Servicios | Tiempo Total | Días Laborables |
|-----------|-----------|--------------|-----------------|
| 🔴 Alta | 5 servicios | ~36 horas | ~4.5 días |
| 🟡 Media | 4 servicios | ~26 horas | ~3.25 días |
| 🟢 Baja | 8 servicios | ~52 horas | ~6.5 días |
| **TOTAL** | **17 servicios** | **~114 horas** | **~14.25 días** |

### **Mejoras a Servicios Existentes:**

| Servicio | Mejoras Necesarias | Tiempo |
|----------|-------------------|--------|
| Gateway | Rate Limiting global, Circuit Breaker, Caché | 4h |
| ErrorService | Migrar Rate Limiting a Redis distribuido | 2h |
| MediaService | Ver servicio #16 (File Storage Service) | 7h |

**Total Mejoras: ~13 horas**

---

## 📅 **Plan de Implementación Recomendado**

### **Fase 1: Infraestructura Core** (Semana 1-2)
1. ✅ Message Bus Service (7h)
2. ✅ Configuration Service (7h)
3. ✅ Distributed Cache Service (6h)
4. ✅ Centralized Logging Service (8h)
5. ✅ Service Discovery Service (8h)

**Total Fase 1: ~36 horas**

---

### **Fase 2: Observabilidad** (Semana 2-3)
6. Distributed Tracing Service (7h)
7. Metrics & Monitoring Service (8h)
8. Health Check Aggregator (5h)

**Total Fase 2: ~20 horas**

---

### **Fase 3: Operaciones** (Semana 3-4)
9. Scheduler Service (6h)
10. Idempotency Service (6h)
11. Rate Limiting Service (5h)
12. Circuit Breaker Service (6h)

**Total Fase 3: ~23 horas**

---

### **Fase 4: Features Avanzados** (Según necesidad)
13. Search Service (9h)
14. Feature Toggle Service (5h)
15. API Documentation Aggregator (4h)
16. File Storage Service (7h)
17. Backup & DR Service (10h)

**Total Fase 4: ~35 horas**

---

## 🛠️ **Stack Tecnológico Global**

### **Message & Events:**
- RabbitMQ 3.x (Message Bus)
- Apache Kafka (opcional para event streaming)

### **Storage & Cache:**
- Redis 7.x (Cache distribuido, Rate Limiting, Idempotency)
- PostgreSQL 15.x (Persistencia general)
- Azure Blob Storage / S3 (File storage)

### **Observability:**
- Seq / ELK Stack (Logging)
- Jaeger / Zipkin (Tracing)
- Prometheus + Grafana (Metrics)

### **Service Mesh:**
- Consul (Service Discovery)
- Polly (Resilience)

### **Scheduling:**
- Hangfire / Quartz.NET

### **Search:**
- Elasticsearch 8.x + Kibana

### **.NET Ecosystem:**
- ASP.NET Core 8.0
- Entity Framework Core 8.0
- MediatR 12.2.0
- Serilog
- OpenTelemetry
- xUnit + Moq (Testing)

---

## 🎯 **Criterios de Éxito Global**

- ✅ **200+ tests unitarios** pasando (promedio 12 tests/servicio)
- ✅ **0 errores de compilación**
- ✅ **Docker Compose funcional** con todos los servicios
- ✅ **Documentación completa** (README por servicio)
- ✅ **Commits organizados** con mensajes descriptivos
- ✅ **Observabilidad completa** (logs, traces, metrics)
- ✅ **Resilience patterns** implementados
- ✅ **Configuración centralizada** operativa
- ✅ **Service discovery** eliminando URLs hardcoded

---

## 📊 **Dependencias entre Servicios**

```
Message Bus Service
    ↓
Configuration Service
    ↓
Distributed Cache Service
    ↓
Service Discovery ← Centralized Logging
    ↓                      ↓
Distributed Tracing → Metrics & Monitoring
    ↓                      ↓
Health Check Aggregator ← Scheduler Service
    ↓
Circuit Breaker → Rate Limiting → Idempotency
    ↓
Feature Toggles → Search Service
    ↓
API Documentation → Backup & DR
```

---

## 📝 **Próximos Pasos**

1. ✅ **Revisar y aprobar este roadmap**
2. 🚀 **Comenzar con Fase 1** (Message Bus Service)
3. 📋 **Tracking de progreso** con todo list actualizado
4. 🔄 **Iteraciones cortas** (1 servicio a la vez)
5. ✅ **Tests y documentación** por cada servicio
6. 🔁 **Git commits incrementales**

---

**Estado:** 📋 Plan completo - Pendiente de aprobación  
**Inicio propuesto:** Fase 1 - Message Bus Service  
**Objetivo:** Arquitectura de microservices empresarial completa
