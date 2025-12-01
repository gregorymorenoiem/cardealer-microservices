# 📋 Plan de Implementación - Message Bus Service y Configuration Service

**Fecha:** 1 de diciembre de 2025  
**Proyecto:** CarDealer Microservices  
**Servicios:** Message Bus Service + Configuration Service  
**Tiempo Total Estimado:** ~7 horas 10 minutos

---

## **1. Message Bus Service** (Servicio de Mensajería Asíncrona)

### **Funcionalidades:**
- ✉️ **Publicación de Mensajes**: Enviar mensajes a topics/queues con prioridad y TTL
- 📥 **Suscripción a Topics**: Consumir mensajes de topics específicos
- 🔄 **Retry Logic**: Reintentos automáticos con backoff exponencial
- 💀 **Dead Letter Queue**: Manejo de mensajes fallidos
- 📊 **Message Tracking**: Seguimiento de estado de mensajes (Pending, Processing, Completed, Failed)
- 🔔 **Event Broadcasting**: Publicar eventos de dominio a múltiples suscriptores
- 📦 **Batch Publishing**: Envío masivo de mensajes
- 🔍 **Message History**: Consultar historial de mensajes enviados/recibidos

### **Tareas y Tiempos:**

| # | Tarea | Tiempo Estimado |
|---|-------|----------------|
| 1 | Diseñar arquitectura + elegir tecnología (RabbitMQ) | ⏱️ **15 min** |
| 2 | Capa de Dominio (entidades, enums, value objects) | ⏱️ **20 min** |
| 3 | Capa de Aplicación (interfaces, comandos, handlers) | ⏱️ **30 min** |
| 4 | Capa de Infraestructura (RabbitMQ client, EF Core) | ⏱️ **35 min** |
| 5 | API Controllers (3 controllers con endpoints REST) | ⏱️ **25 min** |
| 6 | Configuración (appsettings, DI, Program.cs) | ⏱️ **15 min** |
| 7 | Tests unitarios (10 tests mínimo) | ⏱️ **30 min** |

**Subtotal Message Bus Service: ~2h 50min**

---

## **2. Configuration Service** (Servicio de Configuración Centralizada)

### **Funcionalidades:**
- ⚙️ **Configuración por Entorno**: Settings para Dev/Staging/Production
- 🔐 **Secrets Management**: Almacenamiento encriptado de secretos (API keys, connection strings)
- 🎚️ **Feature Flags**: Activar/desactivar features sin redeploy
- 📜 **Configuration History**: Auditoría de cambios en configuraciones
- 🔄 **Hot Reload**: Actualización de configuración sin reiniciar servicios
- 🌍 **Multi-Tenant Support**: Configuraciones por cliente/organización
- 🔍 **Configuration Validation**: Validar formato y valores de configuraciones
- 📤 **Bulk Import/Export**: Importar/exportar configuraciones en JSON/YAML

### **Tareas y Tiempos:**

| # | Tarea | Tiempo Estimado |
|---|-------|----------------|
| 8 | Diseñar arquitectura + estructura de configuraciones | ⏱️ **15 min** |
| 9 | Capa de Dominio (entidades, enums para secrets/flags) | ⏱️ **20 min** |
| 10 | Capa de Aplicación (interfaces, comandos CQRS) | ⏱️ **30 min** |
| 11 | Capa de Infraestructura (encriptación AES, EF Core) | ⏱️ **40 min** |
| 12 | API Controllers (3 controllers con versionado) | ⏱️ **25 min** |
| 13 | Configuración (PostgreSQL, encryption keys, DI) | ⏱️ **20 min** |
| 14 | Tests unitarios (12 tests mínimo) | ⏱️ **35 min** |

**Subtotal Configuration Service: ~3h 5min**

---

## **3. Integración y Documentación**

### **Tareas y Tiempos:**

| # | Tarea | Tiempo Estimado |
|---|-------|----------------|
| 15 | Actualizar docker-compose.yml (RabbitMQ, PostgreSQL) | ⏱️ **10 min** |
| 16 | Integrar con AdminService (ejemplo de uso) | ⏱️ **20 min** |
| 17 | Compilación y tests finales (ambos servicios) | ⏱️ **15 min** |
| 18 | Git commit + push (Message Bus Service) | ⏱️ **5 min** |
| 19 | Git commit + push (Configuration Service) | ⏱️ **5 min** |
| 20 | Documentación README.md con diagramas | ⏱️ **20 min** |

**Subtotal Integración: ~1h 15min**

---

## ⏰ **RESUMEN DE TIEMPOS**

### **Desglose por Fase:**
- 🏗️ **Message Bus Service**: 2h 50min (40%)
- ⚙️ **Configuration Service**: 3h 5min (43%)
- 🔗 **Integración + Docs**: 1h 15min (17%)

### **TIEMPO TOTAL: ~7 horas 10 minutos**

---

## 🛠️ **Stack Tecnológico**

### **Message Bus Service:**
- **Message Broker**: RabbitMQ 3.x
- **Pattern**: Publisher/Subscriber + Request/Reply
- **Database**: PostgreSQL (message history)
- **Framework**: ASP.NET Core 8.0
- **CQRS**: MediatR 12.2.0
- **ORM**: Entity Framework Core 8.0
- **Testing**: xUnit 2.5.3 + Moq 4.20.70

### **Configuration Service:**
- **Encryption**: AES-256 (secrets)
- **Database**: PostgreSQL (configurations + history)
- **Framework**: ASP.NET Core 8.0
- **CQRS**: MediatR 12.2.0
- **ORM**: Entity Framework Core 8.0
- **Optional**: Azure Key Vault integration
- **Testing**: xUnit 2.5.3 + Moq 4.20.70

### **Arquitectura:**
- Clean Architecture (Domain, Application, Infrastructure, API)
- CQRS Pattern con MediatR
- Repository Pattern
- Dependency Injection

---

## 📝 **Lista de Tareas Detallada**

### **Message Bus Service (Tareas 1-7)**

#### **1. Diseñar arquitectura de Message Bus Service** ⏱️ 15 min
- Definir estructura de proyectos (Domain, Application, Infrastructure, API)
- Elegir RabbitMQ como message broker
- Definir patrones: Publisher/Subscriber, Request/Reply
- Diseñar flujo de mensajes y dead letter queue

#### **2. Implementar Message Bus Service - Capa de Dominio** ⏱️ 20 min
- Crear entidades:
  - `Message` (Id, Topic, Payload, Status, Priority, CreatedAt, ProcessedAt)
  - `MessageBatch` (Id, Messages, Status)
  - `Subscription` (Id, Topic, ConsumerName, IsActive)
  - `DeadLetterMessage` (Id, OriginalMessage, FailureReason, RetryCount)
- Definir enums:
  - `MessageStatus` (Pending, Processing, Completed, Failed)
  - `MessagePriority` (Low, Normal, High, Critical)

#### **3. Implementar Message Bus Service - Capa de Aplicación** ⏱️ 30 min
- Crear interfaces:
  - `IMessagePublisher` (PublishAsync, PublishBatchAsync)
  - `IMessageSubscriber` (SubscribeAsync, UnsubscribeAsync)
  - `IDeadLetterManager` (GetDeadLettersAsync, RetryAsync, DiscardAsync)
- Crear comandos CQRS:
  - `PublishMessageCommand` + Handler
  - `SubscribeToTopicCommand` + Handler
  - `RetryDeadLetterCommand` + Handler
  - `GetMessageHistoryQuery` + Handler

#### **4. Implementar Message Bus Service - Capa de Infraestructura** ⏱️ 35 min
- Implementar `RabbitMQPublisher` (IMessagePublisher)
- Implementar `RabbitMQSubscriber` (IMessageSubscriber)
- Implementar `DeadLetterManager` (IDeadLetterManager)
- Configurar Entity Framework DbContext
- Crear repositorios para Message, Subscription, DeadLetterMessage
- Configurar ConnectionStrings (RabbitMQ + PostgreSQL)

#### **5. Implementar Message Bus Service - API Controllers** ⏱️ 25 min
- `MessagesController`:
  - POST /api/messages (publicar mensaje)
  - POST /api/messages/batch (publicar lote)
  - GET /api/messages/history (historial)
- `SubscriptionsController`:
  - POST /api/subscriptions (crear suscripción)
  - DELETE /api/subscriptions/{id} (cancelar)
  - GET /api/subscriptions (listar)
- `DeadLetterController`:
  - GET /api/deadletters (listar mensajes fallidos)
  - POST /api/deadletters/{id}/retry (reintentar)
  - DELETE /api/deadletters/{id} (descartar)

#### **6. Configurar Message Bus Service - appsettings y Program.cs** ⏱️ 15 min
- Configurar `appsettings.json`:
  - RabbitMQ connection (host, port, username, password)
  - PostgreSQL connection string
  - Retry policies (max retries, backoff)
- Configurar `Program.cs`:
  - Registrar MediatR
  - Registrar servicios DI (IMessagePublisher, IMessageSubscriber, etc.)
  - Configurar Entity Framework

#### **7. Crear tests para Message Bus Service** ⏱️ 30 min
- `PublishMessageCommandHandlerTests` (2 tests)
- `SubscribeToTopicCommandHandlerTests` (2 tests)
- `RetryDeadLetterCommandHandlerTests` (2 tests)
- `MessagesControllerTests` (2 tests)
- `DeadLetterControllerTests` (2 tests)
- **Total: 10 tests**

---

### **Configuration Service (Tareas 8-14)**

#### **8. Diseñar arquitectura de Configuration Service** ⏱️ 15 min
- Definir estructura por entorno (Dev/Staging/Prod)
- Diseñar modelo de secrets encriptados
- Definir feature flags (boolean, percentage rollout)
- Diseñar auditoría de cambios (ConfigurationHistory)

#### **9. Implementar Configuration Service - Capa de Dominio** ⏱️ 20 min
- Crear entidades:
  - `ConfigurationItem` (Id, Key, Value, Environment, Type, CreatedAt, UpdatedAt)
  - `ConfigurationHistory` (Id, ConfigurationId, OldValue, NewValue, ChangedBy, ChangedAt)
  - `EncryptedSecret` (Id, Key, EncryptedValue, Environment)
  - `FeatureFlag` (Id, Name, IsEnabled, RolloutPercentage, Environment)
- Definir enums:
  - `ConfigurationType` (String, Number, Boolean, Json)
  - `Environment` (Development, Staging, Production)

#### **10. Implementar Configuration Service - Capa de Aplicación** ⏱️ 30 min
- Crear interfaces:
  - `IConfigurationManager` (GetAsync, SetAsync, DeleteAsync)
  - `ISecretManager` (GetSecretAsync, SetSecretAsync, DeleteSecretAsync)
  - `IFeatureFlagManager` (IsEnabledAsync, ToggleAsync, SetRolloutAsync)
- Crear comandos CQRS:
  - `SetConfigurationCommand` + Handler
  - `GetConfigurationQuery` + Handler
  - `SetSecretCommand` + Handler
  - `ToggleFeatureCommand` + Handler
  - `GetConfigurationHistoryQuery` + Handler

#### **11. Implementar Configuration Service - Capa de Infraestructura** ⏱️ 40 min
- Implementar `AesEncryptionService` (encrypt/decrypt con AES-256)
- Implementar `ConfigurationManager` (IConfigurationManager)
- Implementar `SecretManager` (ISecretManager) con encriptación
- Implementar `FeatureFlagManager` (IFeatureFlagManager)
- Configurar Entity Framework DbContext
- Crear repositorios para ConfigurationItem, EncryptedSecret, FeatureFlag
- (Opcional) Integración con Azure Key Vault

#### **12. Implementar Configuration Service - API Controllers** ⏱️ 25 min
- `ConfigurationsController`:
  - GET /api/configurations/{environment}/{key}
  - POST /api/configurations (crear/actualizar)
  - DELETE /api/configurations/{id}
  - GET /api/configurations/history/{id}
- `SecretsController`:
  - GET /api/secrets/{environment}/{key}
  - POST /api/secrets (crear/actualizar secret encriptado)
  - DELETE /api/secrets/{id}
- `FeatureFlagsController`:
  - GET /api/features/{environment}/{name}
  - POST /api/features/{name}/toggle
  - PUT /api/features/{name}/rollout (actualizar porcentaje)

#### **13. Configurar Configuration Service - appsettings y Program.cs** ⏱️ 20 min
- Configurar `appsettings.json`:
  - PostgreSQL connection string
  - Encryption key (AES-256)
  - (Opcional) Azure Key Vault settings
- Configurar `Program.cs`:
  - Registrar MediatR
  - Registrar servicios DI (IConfigurationManager, ISecretManager, etc.)
  - Configurar Entity Framework
  - Registrar AesEncryptionService

#### **14. Crear tests para Configuration Service** ⏱️ 35 min
- `SetConfigurationCommandHandlerTests` (2 tests)
- `GetConfigurationQueryHandlerTests` (2 tests)
- `AesEncryptionServiceTests` (2 tests - encrypt/decrypt)
- `ToggleFeatureCommandHandlerTests` (2 tests)
- `ConfigurationsControllerTests` (2 tests)
- `SecretsControllerTests` (2 tests)
- **Total: 12 tests**

---

### **Integración y Documentación (Tareas 15-20)**

#### **15. Actualizar docker-compose.yml** ⏱️ 10 min
- Agregar servicio `rabbitmq` (port 5672, management UI 15672)
- Agregar servicio `messagebus` (MessageBusService)
- Agregar servicio `configservice` (ConfigurationService)
- Configurar dependencias (PostgreSQL, RabbitMQ)
- Configurar networks y volumes

#### **16. Integrar servicios con AdminService** ⏱️ 20 min
- AdminService consume configuraciones desde ConfigurationService
- AdminService publica eventos via Message Bus (VehicleApproved, ReportResolved)
- Crear ejemplo de suscriptor en AdminService
- Agregar HttpClient para ConfigurationService en AdminService

#### **17. Compilar y ejecutar tests finales** ⏱️ 15 min
- `dotnet build MessageBusService.sln`
- `dotnet build ConfigurationService.sln`
- `dotnet test MessageBusService.Tests` (10 tests)
- `dotnet test ConfigurationService.Tests` (12 tests)
- Verificar 0 errores, 22+ tests pasando

#### **18. Git commit y push de Message Bus Service** ⏱️ 5 min
- `git add backend/MessageBusService`
- `git commit -m "feat(MessageBusService): Implement RabbitMQ-based message bus with dead letter queue"`
- `git push`

#### **19. Git commit y push de Configuration Service** ⏱️ 5 min
- `git add backend/ConfigurationService`
- `git commit -m "feat(ConfigurationService): Implement centralized config with AES encryption and feature flags"`
- `git push`

#### **20. Documentación y README** ⏱️ 20 min
- Crear `backend/MessageBusService/README.md`:
  - Arquitectura y componentes
  - Endpoints REST
  - Ejemplos de publicación/suscripción
  - Configuración RabbitMQ
- Crear `backend/ConfigurationService/README.md`:
  - Arquitectura y secrets management
  - Endpoints REST
  - Ejemplos de uso (configurations, secrets, feature flags)
  - Configuración de encriptación
- Actualizar `README.md` principal del proyecto

---

## 🎯 **Criterios de Éxito**

### **Message Bus Service:**
- ✅ RabbitMQ integrado y funcional
- ✅ Dead letter queue implementada
- ✅ 10+ tests pasando
- ✅ Endpoints REST operativos
- ✅ Compilación sin errores

### **Configuration Service:**
- ✅ Secrets encriptados con AES-256
- ✅ Feature flags operativos
- ✅ 12+ tests pasando
- ✅ Endpoints REST operativos
- ✅ Compilación sin errores

### **Integración:**
- ✅ docker-compose actualizado
- ✅ AdminService integrado con ambos servicios
- ✅ Documentación completa
- ✅ Commits pusheados a Git

---

## 📚 **Referencias**

### **Message Bus:**
- RabbitMQ Tutorials: https://www.rabbitmq.com/tutorials
- Publisher/Subscriber Pattern
- Dead Letter Exchanges (DLX)

### **Configuration:**
- AES-256 Encryption en .NET
- Feature Flags Best Practices
- Azure Key Vault (opcional)

---

**Estado:** ✅ Plan aprobado - Listo para implementación  
**Próximo paso:** Iniciar Tarea #1 - Diseñar arquitectura de Message Bus Service
