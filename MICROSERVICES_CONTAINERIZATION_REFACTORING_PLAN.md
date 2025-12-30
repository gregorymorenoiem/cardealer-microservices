# 🐳 Plan de Refactorización: Microservicios Autosuficientes para CI/CD

**Fecha de Creación**: 30 de Diciembre de 2025  
**Objetivo**: Hacer que cada microservicio sea completamente autosuficiente, dependiendo solo de secretos inyectados  
**Total de Microservicios**: 32 servicios identificados  
**Duración Estimada**: 32 Sprints (1 sprint por microservicio)

---

## 📋 Resumen Ejecutivo

### Problema Actual
Los microservicios tienen dependencias de:
1. **Archivos de configuración externos** (firebase-service-account.json, certificados SSL)
2. **Secretos hardcodeados** en appsettings.json
3. **Paths absolutos** en configuraciones
4. **Conexiones a servicios externos** sin fallback

### Solución Propuesta
1. **Externalización completa de configuración** via variables de entorno
2. **Secretos solo vía Docker Secrets / Kubernetes Secrets**
3. **Dockerfiles multi-stage optimizados y estandarizados**
4. **Health checks robustos** para orquestadores
5. **Graceful degradation** cuando servicios externos no están disponibles

---

## 🎯 Checklist de Refactorización por Microservicio

Para cada microservicio, se deben completar las siguientes tareas:

### ✅ Fase 1: Configuración (Prioridad Alta)
- [ ] Mover todos los secretos a variables de entorno
- [ ] Crear `appsettings.Docker.json` sin secretos
- [ ] Implementar patrón de configuración por entorno
- [ ] Eliminar paths absolutos

### ✅ Fase 2: Dockerfile (Prioridad Alta)
- [ ] Dockerfile multi-stage optimizado
- [ ] Dockerfile.dev para desarrollo
- [ ] Dockerfile.prod para producción
- [ ] Non-root user configurado
- [ ] Health check en Dockerfile

### ✅ Fase 3: Resiliencia (Prioridad Media)
- [ ] Graceful degradation para dependencias externas
- [ ] Retry policies con Polly
- [ ] Circuit breaker configurado
- [ ] Startup probes correctos

### ✅ Fase 4: Documentación (Prioridad Media)
- [ ] README actualizado con variables de entorno requeridas
- [ ] docker-compose.example.yml
- [ ] Documentar secretos requeridos

---

## 📊 Estado Actual de Microservicios

| # | Microservicio | Dockerfile | Secrets Externos | DB Propia | Estado |
|---|---------------|------------|------------------|-----------|--------|
| 1 | AdminService | ✅ | ❌ | ❌ | 🟡 Parcial |
| 2 | ApiDocsService | ✅ | ❌ | ❌ | 🟢 Listo |
| 3 | AppointmentService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 4 | AuditService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 5 | AuthService | ✅ | JWT Keys | ✅ | 🔴 Pendiente |
| 6 | BackupDRService | ✅ | S3/Azure Keys | ❌ | 🔴 Pendiente |
| 7 | BillingService | ✅ | Payment Keys | ✅ | 🔴 Pendiente |
| 8 | CacheService | ✅ | Redis | ❌ | 🟡 Parcial |
| 9 | ConfigurationService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 10 | ContactService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 11 | CRMService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 12 | ErrorService | ✅ | Elasticsearch | ✅ | 🔴 Pendiente |
| 13 | FeatureToggleService | ✅ | ❌ | ✅ | 🔴 Build Errors |
| 14 | FileStorageService | ✅ | S3/Azure | ❌ | 🔴 Pendiente |
| 15 | FinanceService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 16 | Gateway | ✅ | ❌ | ❌ | 🟡 Parcial |
| 17 | HealthCheckService | ✅ | ❌ | ❌ | 🟢 Listo |
| 18 | IdempotencyService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 19 | IntegrationService | ✅ | External APIs | ✅ | 🔴 Pendiente |
| 20 | InvoicingService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 21 | LoggingService | ✅ | ❌ | ❌ | 🟢 Listo |
| 22 | MarketingService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 23 | MediaService | ✅ | S3/Azure | ✅ | 🔴 Pendiente |
| 24 | MessageBusService | ✅ | RabbitMQ | ❌ | 🟡 Parcial |
| 25 | NotificationService | ✅ | SendGrid/Twilio/Firebase | ✅ | 🔴 Crítico |
| 26 | ProductService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 27 | RateLimitingService | ✅ | Redis | ❌ | 🟡 Parcial |
| 28 | RealEstateService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 29 | ReportsService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 30 | RoleService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 31 | SchedulerService | ✅ | ❌ | ✅ | 🟡 Parcial |
| 32 | SearchService | ✅ | Elasticsearch | ❌ | 🔴 Pendiente |
| 33 | ServiceDiscovery | ✅ | Consul | ❌ | 🟡 Parcial |
| 34 | TracingService | ✅ | Jaeger/Zipkin | ❌ | 🟡 Parcial |
| 35 | UserService | ✅ | ❌ | ✅ | 🟡 Parcial |

---

## 🚀 SPRINTS DE REFACTORIZACIÓN

### Orden de Prioridad
1. **CRÍTICOS** (Servicios con secretos sensibles hardcodeados)
2. **CORE** (Servicios fundamentales del sistema)
3. **INFRAESTRUCTURA** (Servicios de soporte)
4. **SECUNDARIOS** (Servicios de negocio adicionales)

---

# 📦 SPRINT 1: _Shared (Librerías Compartidas) ✅ COMPLETADO

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  
**Prioridad**: 🔴 CRÍTICA (Base para todos los demás)  
**Estado**: ✅ **COMPLETADO** (30 Dic 2025)

## Objetivo
Preparar las librerías compartidas para soportar configuración externalizada.

## Tareas Completadas

### 1.1 CarDealer.Shared - Configuración Base ✅
```
Archivos creados:
- _Shared/CarDealer.Shared/Secrets/ISecretProvider.cs
- _Shared/CarDealer.Shared/Secrets/EnvironmentSecretProvider.cs
- _Shared/CarDealer.Shared/Secrets/DockerSecretProvider.cs
- _Shared/CarDealer.Shared/Secrets/CompositeSecretProvider.cs
- _Shared/CarDealer.Shared/Secrets/SecretProviderExtensions.cs
- _Shared/CarDealer.Shared/Secrets/SecretKeys.cs
- _Shared/CarDealer.Shared/Secrets/ConnectionStringBuilder.cs
- _Shared/CarDealer.Shared/Database/DatabaseSecretExtensions.cs
```

- [x] Crear `ISecretProvider` interface para abstracción de secretos
- [x] Crear `EnvironmentSecretProvider` que lea de ENV vars
- [x] Crear `DockerSecretProvider` que lea de /run/secrets/
- [x] Crear `CompositeSecretProvider` para combinar fuentes con fallback
- [x] Crear `ConnectionStringBuilder` para construir connections desde secretos
- [x] Crear `SecretKeys` con constantes de nombres estándar
- [x] Crear `DatabaseSecretExtensions` para usar secretos en DB config

### 1.2 CarDealer.Contracts - Sin cambios necesarios ✅
- [x] Verificado que no hay secretos
- [x] No requiere cambios

### 1.3 Documentación ✅
- [x] Crear README de librerías compartidas
- [x] Documentar patrones de configuración

## Entregables ✅
- [x] `ISecretProvider` implementado con múltiples proveedores
- [x] Configuración Docker-ready con `AddDatabaseFromSecrets<T>()`
- [x] Documentación completa en README.md
- [x] Build exitoso verificado

---

# 📦 SPRINT 2: NotificationService (CRÍTICO - Secretos Sensibles)

**Duración**: 1 sesión  
**Tokens Estimados**: ~25,000  
**Prioridad**: 🔴 CRÍTICA

## Problema Actual
- SendGrid API Key hardcodeada
- Twilio credentials hardcodeados
- Firebase service account como archivo montado
- Templates path hardcodeado

## Tareas

### 2.1 Externalización de Secretos
```
Variables de entorno requeridas:
- SENDGRID_API_KEY
- TWILIO_ACCOUNT_SID
- TWILIO_AUTH_TOKEN
- TWILIO_FROM_NUMBER
- FIREBASE_PROJECT_ID
- FIREBASE_PRIVATE_KEY (base64 encoded)
- FIREBASE_CLIENT_EMAIL
```

- [ ] Actualizar `appsettings.json` - remover todos los secretos
- [ ] Crear `appsettings.Docker.json` con placeholders
- [ ] Modificar `Program.cs` para leer de environment
- [ ] Implementar `FirebaseCredentialProvider` para leer de ENV
- [ ] Actualizar Dockerfile para copiar templates

### 2.2 Dockerfile Optimizado
```dockerfile
# NotificationService/Dockerfile.prod
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
# Multi-stage build optimizado
# Templates embebidos en imagen
# Health check configurado
```

- [ ] Crear Dockerfile.prod multi-stage
- [ ] Embed templates en la imagen
- [ ] Configurar health checks
- [ ] Non-root user

### 2.3 Graceful Degradation
- [ ] Fallback cuando SendGrid no disponible (queue messages)
- [ ] Fallback cuando Firebase no configurado (skip push)
- [ ] Logs descriptivos de configuración faltante

### 2.4 Documentación
- [ ] README con todas las variables requeridas
- [ ] docker-compose.notification.example.yml

## Variables de Entorno Finales
```env
# Required
NOTIFICATION_DB_CONNECTION_STRING=
RABBITMQ_CONNECTION_STRING=

# Email (optional - graceful degradation)
SENDGRID_API_KEY=
SENDGRID_FROM_EMAIL=
SENDGRID_FROM_NAME=

# SMS (optional)
TWILIO_ACCOUNT_SID=
TWILIO_AUTH_TOKEN=
TWILIO_FROM_NUMBER=

# Push Notifications (optional)
FIREBASE_PROJECT_ID=
FIREBASE_PRIVATE_KEY_BASE64=
FIREBASE_CLIENT_EMAIL=
```

---

# 📦 SPRINT 3: AuthService (CRÍTICO - JWT Keys)

**Duración**: 1 sesión  
**Tokens Estimados**: ~20,000  
**Prioridad**: 🔴 CRÍTICA

## Problema Actual
- JWT Secret Key hardcodeada en appsettings
- Conexión a Redis hardcodeada
- Dependencia de ErrorService sin fallback

## Tareas

### 3.1 Externalización de Secretos
```
Variables de entorno requeridas:
- JWT_SECRET_KEY (min 32 chars)
- JWT_ISSUER
- JWT_AUDIENCE
- AUTH_DB_CONNECTION_STRING
- REDIS_CONNECTION_STRING
```

- [ ] Mover JWT config a environment variables
- [ ] Crear extension method `AddJwtAuthenticationFromEnv()`
- [ ] Actualizar `appsettings.json` sin secretos
- [ ] Implementar validación de configuración al startup

### 3.2 Dockerfile Optimizado
- [ ] Crear Dockerfile.prod
- [ ] Health check específico para auth
- [ ] Configurar startup probe

### 3.3 Resiliencia
- [ ] Fallback cuando Redis no disponible (in-memory cache)
- [ ] Fallback cuando ErrorService no disponible (local logging)
- [ ] Circuit breaker para dependencias

## Variables de Entorno Finales
```env
# Required
AUTH_DB_CONNECTION_STRING=Host=...;Database=authservice;...
JWT_SECRET_KEY=your-super-secret-key-min-32-chars
JWT_ISSUER=AuthService
JWT_AUDIENCE=CarDealerClients
JWT_EXPIRES_MINUTES=60
JWT_REFRESH_TOKEN_EXPIRES_DAYS=7

# Optional
REDIS_CONNECTION_STRING=redis:6379
ERRORSERVICE_URL=http://errorservice:80
```

---

# 📦 SPRINT 4: ErrorService (CORE)

**Duración**: 1 sesión  
**Tokens Estimados**: ~18,000  
**Prioridad**: 🟡 ALTA

## Problema Actual
- Elasticsearch credentials en appsettings
- DB connection hardcodeada

## Tareas

### 4.1 Externalización
- [ ] Mover Elasticsearch config a ENV
- [ ] Mover DB connection a ENV
- [ ] Crear mode "standalone" sin Elasticsearch

### 4.2 Dockerfile
- [ ] Verificar Dockerfile.prod existente
- [ ] Optimizar layers
- [ ] Health check

### 4.3 Graceful Degradation
- [ ] Funcionar sin Elasticsearch (solo DB)
- [ ] Queue errors cuando DB no disponible

## Variables de Entorno
```env
ERROR_DB_CONNECTION_STRING=
ELASTICSEARCH_URL=
ELASTICSEARCH_USERNAME=
ELASTICSEARCH_PASSWORD=
ELASTICSEARCH_ENABLE=false
```

---

# 📦 SPRINT 5: Gateway (CORE)

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  
**Prioridad**: 🟡 ALTA

## Tareas
- [ ] Externalizar configuración de Ocelot
- [ ] Mover routes a configuración dinámica
- [ ] Health check de todos los downstream services
- [ ] Rate limiting configurable via ENV
- [ ] CORS configurable via ENV

## Variables de Entorno
```env
GATEWAY_DOWNSTREAM_SERVICES=authservice:80,errorservice:80,...
CORS_ALLOWED_ORIGINS=https://app.example.com
RATE_LIMIT_PER_SECOND=100
```

---

# 📦 SPRINT 6: UserService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  
**Prioridad**: 🟡 ALTA

## Tareas
- [ ] Externalizar DB connection
- [ ] Externalizar RabbitMQ connection
- [ ] Dockerfile.prod optimizado
- [ ] Health checks

---

# 📦 SPRINT 7: RoleService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

## Tareas
- [ ] Externalizar DB connection
- [ ] Dockerfile.prod
- [ ] Health checks

---

# 📦 SPRINT 8: ProductService

**Duración**: 1 sesión  
**Tokens Estimados**: ~18,000  

## Tareas
- [ ] Externalizar DB connection
- [ ] Externalizar cache connection
- [ ] Dockerfile.prod
- [ ] Health checks

---

# 📦 SPRINT 9: MediaService (Secretos S3/Azure)

**Duración**: 1 sesión  
**Tokens Estimados**: ~22,000  
**Prioridad**: 🔴 CRÍTICA

## Problema Actual
- AWS/Azure credentials pueden estar hardcodeadas
- Storage paths hardcodeados

## Tareas
- [ ] Externalizar storage credentials
- [ ] Soporte multi-provider (S3, Azure, Local)
- [ ] Dockerfile con soporte para secrets
- [ ] Graceful degradation a local storage

## Variables de Entorno
```env
STORAGE_PROVIDER=S3|Azure|Local
AWS_ACCESS_KEY_ID=
AWS_SECRET_ACCESS_KEY=
AWS_BUCKET_NAME=
AWS_REGION=
# O
AZURE_STORAGE_CONNECTION_STRING=
AZURE_CONTAINER_NAME=
```

---

# 📦 SPRINT 10: FileStorageService

**Duración**: 1 sesión  
**Tokens Estimados**: ~18,000  

## Tareas
- [ ] Similar a MediaService
- [ ] Externalizar storage config
- [ ] Multi-provider support

---

# 📦 SPRINT 11: CacheService

**Duración**: 1 sesión  
**Tokens Estimados**: ~12,000  

## Tareas
- [ ] Externalizar Redis connection
- [ ] Soporte fallback a in-memory
- [ ] Health checks

---

# 📦 SPRINT 12: MessageBusService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

## Tareas
- [ ] Externalizar RabbitMQ connection
- [ ] Connection retry policies
- [ ] Dead letter queue config
- [ ] Health checks

---

# 📦 SPRINT 13: SearchService

**Duración**: 1 sesión  
**Tokens Estimados**: ~18,000  

## Tareas
- [ ] Externalizar Elasticsearch config
- [ ] Graceful degradation sin ES
- [ ] Index configuration via ENV

---

# 📦 SPRINT 14: BillingService (Secretos de Pago)

**Duración**: 1 sesión  
**Tokens Estimados**: ~22,000  
**Prioridad**: 🔴 CRÍTICA

## Tareas
- [ ] Externalizar payment gateway credentials
- [ ] Stripe/PayPal keys via ENV
- [ ] Webhook secrets via ENV
- [ ] PCI compliance considerations

---

# 📦 SPRINT 15: InvoicingService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

---

# 📦 SPRINT 16: FinanceService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

---

# 📦 SPRINT 17: CRMService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

---

# 📦 SPRINT 18: ContactService

**Duración**: 1 sesión  
**Tokens Estimados**: ~12,000  

---

# 📦 SPRINT 19: AppointmentService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

---

# 📦 SPRINT 20: MarketingService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

---

# 📦 SPRINT 21: ReportsService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

---

# 📦 SPRINT 22: AuditService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

---

# 📦 SPRINT 23: AdminService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

---

# 📦 SPRINT 24: SchedulerService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

---

# 📦 SPRINT 25: HealthCheckService

**Duración**: 1 sesión  
**Tokens Estimados**: ~10,000  
**Estado**: Ya bastante listo

---

# 📦 SPRINT 26: LoggingService

**Duración**: 1 sesión  
**Tokens Estimados**: ~12,000  

---

# 📦 SPRINT 27: TracingService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

---

# 📦 SPRINT 28: ConfigurationService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

---

# 📦 SPRINT 29: FeatureToggleService (FIX BUILD ERRORS)

**Duración**: 1 sesión  
**Tokens Estimados**: ~25,000  
**Prioridad**: 🔴 CRÍTICA

## Problema Actual
- 19 errores de compilación
- Firmas de método incorrectas
- Tipos faltantes

## Tareas
- [ ] Corregir todos los errores de compilación
- [ ] Añadir ExperimentStatus enum faltante
- [ ] Corregir firmas de IABTestingService
- [ ] Actualizar ABExperiment entity
- [ ] Verificar tests

---

# 📦 SPRINT 30: IntegrationService

**Duración**: 1 sesión  
**Tokens Estimados**: ~18,000  

## Tareas
- [ ] Externalizar todas las API keys externas
- [ ] Graceful degradation por integración

---

# 📦 SPRINT 31: BackupDRService

**Duración**: 1 sesión  
**Tokens Estimados**: ~18,000  

## Tareas
- [ ] Externalizar S3/Azure backup credentials
- [ ] Encryption keys via secrets
- [ ] Schedule configurable via ENV

---

# 📦 SPRINT 32: RealEstateService

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

---

# 📦 SPRINT 33: ApiDocsService

**Duración**: 1 sesión  
**Tokens Estimados**: ~10,000  
**Estado**: Ya bastante listo

---

# 📦 SPRINT 34: IdempotencyService

**Duración**: 1 sesión  
**Tokens Estimados**: ~12,000  

---

# 📦 SPRINT 35: RateLimitingService

**Duración**: 1 sesión  
**Tokens Estimados**: ~12,000  

---

# 📦 SPRINT 36: ServiceDiscovery

**Duración**: 1 sesión  
**Tokens Estimados**: ~15,000  

---

# 📦 SPRINT 37: Infraestructura (monitoring, observability, postgresql)

**Duración**: 1 sesión  
**Tokens Estimados**: ~20,000  

## Tareas
- [ ] Dockerfiles para componentes de monitoring
- [ ] Configuración de Prometheus via ENV
- [ ] Grafana dashboards como ConfigMaps
- [ ] PostgreSQL init scripts

---

# 📦 SPRINT 38: docker-compose.prod.yml Final

**Duración**: 1 sesión  
**Tokens Estimados**: ~25,000  

## Tareas
- [ ] Crear docker-compose.prod.yml completo
- [ ] Todos los servicios incluidos
- [ ] Secrets management configurado
- [ ] Networks optimizadas
- [ ] Health checks unificados
- [ ] .env.production.example actualizado

---

# 📦 SPRINT 39: CI/CD Pipelines

**Duración**: 1 sesión  
**Tokens Estimados**: ~30,000  

## Tareas
- [ ] `.github/workflows/ci.yml` - Build y Tests
- [ ] `.github/workflows/cd.yml` - Deploy
- [ ] `.github/workflows/docker-build.yml` - Docker images
- [ ] Configurar matriz de servicios
- [ ] Secretos en GitHub Actions

---

# 📦 SPRINT 40: Tests de Integración E2E

**Duración**: 1 sesión  
**Tokens Estimados**: ~20,000  

## Tareas
- [ ] Script de verificación de todos los servicios
- [ ] Test de docker-compose up completo
- [ ] Verificación de health checks
- [ ] Test de secretos inyectados correctamente

---

## 📋 Template de Variables de Entorno por Servicio

Cada servicio debe documentar sus variables en formato:

```env
# =============================================
# SERVICE_NAME - Environment Variables
# =============================================

# Required - Application will fail without these
SERVICE_DB_CONNECTION_STRING=Host=db;Database=service;Username=user;Password=pass

# Required for Features - Graceful degradation if missing
FEATURE_API_KEY=your-api-key

# Optional - Has sensible defaults
LOG_LEVEL=Information
ASPNETCORE_ENVIRONMENT=Production
```

---

## 🔐 Gestión de Secretos Recomendada

### Desarrollo Local
```yaml
# docker-compose.override.yml
services:
  myservice:
    env_file:
      - .env.local  # NO commitear este archivo
```

### CI/CD (GitHub Actions)
```yaml
env:
  JWT_SECRET_KEY: ${{ secrets.JWT_SECRET_KEY }}
  SENDGRID_API_KEY: ${{ secrets.SENDGRID_API_KEY }}
```

### Producción (Docker Swarm)
```yaml
services:
  myservice:
    secrets:
      - jwt_secret
      - db_password

secrets:
  jwt_secret:
    external: true
  db_password:
    external: true
```

### Producción (Kubernetes)
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: myservice-secrets
type: Opaque
data:
  JWT_SECRET_KEY: base64-encoded-value
```

---

## ✅ Criterios de Aceptación por Sprint

Cada sprint se considera completado cuando:

1. ✅ **Build exitoso**: `docker build .` sin errores
2. ✅ **Sin secretos hardcodeados**: `grep -r "password\|apikey\|secret" --include="*.json"` vacío
3. ✅ **Health check funcional**: `/health` retorna 200
4. ✅ **Graceful startup**: Servicio arranca aunque dependencias fallen
5. ✅ **Variables documentadas**: README con todas las ENV vars
6. ✅ **Tests pasan**: `dotnet test` exitoso

---

## 📊 Métricas de Éxito del Proyecto

| Métrica | Objetivo | Actual |
|---------|----------|--------|
| Servicios con Dockerfile.prod | 100% | 62% |
| Servicios sin secretos hardcodeados | 100% | ~30% |
| Servicios con health checks | 100% | ~75% |
| Build de CI exitoso | 100% | 0% |
| Documentación completa | 100% | 50% |

---

## 🚀 Orden de Ejecución Recomendado

1. **Sprint 1**: _Shared (base para todo)
2. **Sprint 2**: NotificationService (más secretos sensibles)
3. **Sprint 3**: AuthService (crítico para seguridad)
4. **Sprint 4**: ErrorService (dependencia común)
5. **Sprint 5**: Gateway (entry point)
6. **Sprint 29**: FeatureToggleService (fix build errors)
7. **Sprints 6-28, 30-36**: Resto de servicios por orden de dependencia
8. **Sprint 37**: Infraestructura
9. **Sprint 38**: docker-compose final
10. **Sprint 39**: CI/CD pipelines
11. **Sprint 40**: Tests E2E

---

*Plan generado: 30 de Diciembre de 2025*  
*Versión: 1.0*
