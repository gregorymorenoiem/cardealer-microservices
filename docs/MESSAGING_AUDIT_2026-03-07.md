# 🔍 Cross-Service RabbitMQ Messaging Audit Report

**Date:** 2026-03-07  
**Auditor:** GitHub Copilot (Claude Opus 4.6)  
**Scope:** AuthService, ContactService, ErrorService, MediaService, NotificationService, AdminService, Gateway + `_Shared/`

---

## 📊 Summary

| Severity        | Count  |
| --------------- | ------ |
| 🔴 **Critical** | 3      |
| 🟠 **High**     | 5      |
| 🟡 **Medium**   | 6      |
| 🔵 **Low**      | 4      |
| **Total**       | **18** |

---

## 📋 Cross-Reference Table: Event Flow Map

### A. `cardealer.events` Exchange (Topic, durable)

| Event Name                   | EventType (RoutingKey)        | Publisher Service                                                      | Consumer Service                                             | Queue                                             | Exchange           | Status                                     |
| ---------------------------- | ----------------------------- | ---------------------------------------------------------------------- | ------------------------------------------------------------ | ------------------------------------------------- | ------------------ | ------------------------------------------ |
| UserRegisteredEvent          | `auth.user.registered`        | AuthService (`UserRegisteredEventPublisher`, `RabbitMqEventPublisher`) | NotificationService (`UserRegisteredNotificationConsumer`)   | `notificationservice.user.registered`             | `cardealer.events` | ✅ Matched                                 |
| UserLoggedInEvent            | `auth.user.loggedin`          | AuthService (`RabbitMqEventPublisher`)                                 | NotificationService (`UserLoggedInEventConsumer`)            | `notificationservice.user.loggedin`               | `cardealer.events` | ✅ Matched                                 |
| PasswordChangedEvent         | `auth.password.changed`       | AuthService (`RabbitMqEventPublisher`)                                 | —                                                            | —                                                 | `cardealer.events` | ⚠️ Orphan Publisher                        |
| UserLoggedOutEvent           | `auth.user.loggedout`         | AuthService (`RabbitMqEventPublisher`)                                 | —                                                            | —                                                 | `cardealer.events` | ⚠️ Orphan Publisher                        |
| UserDeletedEvent             | `auth.user.deleted`           | AuthService (`RabbitMqEventPublisher`)                                 | —                                                            | —                                                 | `cardealer.events` | ⚠️ Orphan Publisher                        |
| ErrorCriticalEvent           | `error.critical`              | ErrorService (`RabbitMqEventPublisher`)                                | NotificationService (`ErrorCriticalEventConsumer`)           | `notification-service.error-critical`             | `cardealer.events` | ✅ Matched                                 |
| ErrorLoggedEvent             | `error.logged`                | ErrorService (`RabbitMqEventPublisher`)                                | —                                                            | —                                                 | `cardealer.events` | ⚠️ Orphan Publisher                        |
| ErrorSpikeDetectedEvent      | `error.spike.detected`        | ErrorService (`RabbitMqEventPublisher`)                                | —                                                            | —                                                 | `cardealer.events` | ⚠️ Orphan Publisher                        |
| ServiceDownDetectedEvent     | `error.service.down`          | ErrorService (`RabbitMqEventPublisher`)                                | —                                                            | —                                                 | `cardealer.events` | ⚠️ Orphan Publisher                        |
| KYCProfileStatusChangedEvent | `kyc.profile.status_changed`  | KYCService (external)                                                  | NotificationService (`KYCStatusChangedNotificationConsumer`) | `notificationservice.kyc.status_changed`          | `cardealer.events` | ✅ Matched (publisher outside audit scope) |
| VehicleCreatedEvent          | `vehicle.created`             | VehiclesSaleService (external)                                         | NotificationService (`VehicleCreatedNotificationConsumer`)   | `notificationservice.vehicle.created`             | `cardealer.events` | ✅ Matched (publisher outside audit scope) |
| UserSettingsChangedEvent     | `user.settings.changed`       | UserService (external)                                                 | NotificationService (`UserSettingsChangedEventConsumer`)     | `notificationservice.user.settings.changed`       | `cardealer.events` | ✅ Matched (publisher outside audit scope) |
| PriceAlertTriggeredEvent     | `alert.price.triggered`       | AlertService (external)                                                | NotificationService (`PriceAlertTriggeredConsumer`)          | `notificationservice.alert.price_triggered`       | `cardealer.events` | ✅ Matched (publisher outside audit scope) |
| SavedSearchMatchEvent        | `alert.savedsearch.activated` | AlertService (external)                                                | NotificationService (`SavedSearchActivatedConsumer`)         | `notificationservice.alert.savedsearch_activated` | `cardealer.events` | ✅ Matched (publisher outside audit scope) |
| InvoiceGeneratedEvent        | `billing.invoice.generated`   | BillingService (external)                                              | NotificationService (`InvoiceNotificationConsumer`)          | `notificationservice.invoice.generated`           | `cardealer.events` | ✅ Matched (publisher outside audit scope) |
| PaymentCompletedEvent        | `billing.payment.completed`   | BillingService (external)                                              | NotificationService (`PaymentReceiptNotificationConsumer`)   | `notificationservice.payment.completed`           | `cardealer.events` | ✅ Matched (publisher outside audit scope) |

### B. `errors.exchange` Exchange (Topic, durable) — Error Logging Pipeline

| Event Name         | RoutingKey      | Publisher Service                                           | Consumer Service                       | Queue          | Exchange          | Status                        |
| ------------------ | --------------- | ----------------------------------------------------------- | -------------------------------------- | -------------- | ----------------- | ----------------------------- |
| RabbitMQErrorEvent | `error.created` | AuthService (`RabbitMQErrorProducer`)                       | ErrorService (`RabbitMQErrorConsumer`) | `errors.queue` | `errors.exchange` | ❌ **Exchange Type Mismatch** |
| RabbitMQErrorEvent | `error.created` | `CarDealer.Shared.ErrorHandling` (`RabbitMQErrorPublisher`) | ErrorService (`RabbitMQErrorConsumer`) | `errors.queue` | `errors.exchange` | ❌ **Exchange Type Mismatch** |

### C. `notification-exchange` Exchange (Direct, durable) — Notification Commands

| Event Name        | RoutingKey          | Publisher Service                            | Consumer Service                                     | Queue                | Exchange                | Status                                    |
| ----------------- | ------------------- | -------------------------------------------- | ---------------------------------------------------- | -------------------- | ----------------------- | ----------------------------------------- |
| NotificationEvent | `notification.auth` | AuthService (`RabbitMQNotificationProducer`) | NotificationService (`RabbitMQNotificationConsumer`) | `notification-queue` | `notification-exchange` | ❌ **Exchange Name Mismatch vs Settings** |

### D. `media.events` / `media.commands` Exchanges (MediaService internal)

| Event Name                   | RoutingKey        | Publisher                              | Consumer                               | Queue                   | Exchange                  | Status               |
| ---------------------------- | ----------------- | -------------------------------------- | -------------------------------------- | ----------------------- | ------------------------- | -------------------- |
| MediaUploadedEvent (Domain)  | `media.uploaded`  | MediaService (`RabbitMQMediaProducer`) | —                                      | `media.uploaded.queue`  | `media.events` (Topic)    | ⚠️ No consumer found |
| MediaProcessedEvent (Domain) | `media.processed` | MediaService (`RabbitMQMediaProducer`) | —                                      | `media.processed.queue` | `media.events` (Topic)    | ⚠️ No consumer found |
| MediaDeletedEvent (Domain)   | `media.deleted`   | MediaService (`RabbitMQMediaProducer`) | —                                      | `media.deleted.queue`   | `media.events` (Topic)    | ⚠️ No consumer found |
| ProcessMediaCommand          | `media.process`   | MediaService (`RabbitMQMediaProducer`) | MediaService (`RabbitMQMediaConsumer`) | `process.media.queue`   | `media.commands` (Direct) | ✅ Matched           |

---

## 🔴 Critical Findings (3)

### CRIT-01: Exchange Type Mismatch — `errors.exchange` (Direct vs Topic)

**Location:**

- Publisher: [AuthService/...Messaging/RabbitMQErrorProducer.cs](backend/AuthService/AuthService.Infrastructure/Services/Messaging/RabbitMQErrorProducer.cs#L68) → declares `errors.exchange` as **`ExchangeType.Direct`**
- Consumer: [ErrorService/...Messaging/RabbitMQErrorConsumer.cs](backend/ErrorService/ErrorService.Infrastructure/Services/Messaging/RabbitMQErrorConsumer.cs#L76-L78) → declares `errors.exchange` as **`ExchangeType.Topic`**
- Shared publisher: [CarDealer.Shared.ErrorHandling/...RabbitMQErrorPublisher.cs](backend/_Shared/CarDealer.Shared.ErrorHandling/Services/RabbitMQErrorPublisher.cs#L187-L190) → declares `errors.exchange` as **`ExchangeType.Topic`**

**Description:** The AuthService `RabbitMQErrorProducer` declares the exchange as `Direct`, while the ErrorService consumer and the shared `RabbitMQErrorPublisher` both declare it as `Topic`. Whichever service starts first will create the exchange, and the second will crash with **`PRECONDITION_FAILED - inequivalent arg 'type'`**. If the shared publisher created it first (as Topic), the AuthService producer will fail on startup.

**Impact:** Runtime crash / error events silently dropped from AuthService.

**Fix:** Change [AuthService RabbitMQErrorProducer](backend/AuthService/AuthService.Infrastructure/Services/Messaging/RabbitMQErrorProducer.cs#L68) to use `ExchangeType.Topic` to match the consumer and the shared library. Since Topic exchanges support exact routing keys as well, no other changes needed.

---

### CRIT-02: Exchange Name Mismatch — `notification-exchange` (AuthService) vs `cardealer.events` (NotificationService Settings)

**Location:**

- AuthService producer config: `NotificationServiceRabbitMQSettings.ExchangeName` default = `"cardealer.events"` in [class](backend/AuthService/AuthService.Infrastructure/Services/Messaging/NotificationServiceRabbitMQSettings.cs#L9), but appsettings.json overrides to `"notification-exchange"` in [appsettings.json](backend/AuthService/AuthService.Api/appsettings.json#L75)
- NotificationService consumer: [NotificationServiceRabbitMQSettings](backend/NotificationService/NotificationService.Infrastructure/Messaging/NotificationServiceRabbitMQSettings.cs#L15) default = `"notification-exchange"`, [appsettings.json](backend/NotificationService/NotificationService.Api/appsettings.json#L29) = `"notification-exchange"`

**Description:** The AuthService `NotificationServiceRabbitMQSettings` class default is `"cardealer.events"`, but **both** AuthService and NotificationService `appsettings.json` override it to `"notification-exchange"`. The inconsistency is in the **C# class defaults vs config file** — if the config key is missing or misnamed, different services fall back to different defaults. The actual runtime may work IF config is loaded correctly, but this is fragile.

Additionally, the AuthService `RabbitMQNotificationProducer` declares `notification-exchange` as **`ExchangeType.Direct`**, and the NotificationService `RabbitMQNotificationConsumer` also declares it as **`ExchangeType.Direct`** — so the exchange type is consistent. However, this is a **separate exchange** from `cardealer.events`, creating two parallel messaging pathways (one for direct notification commands, one for domain events), which is unnecessarily confusing.

**Impact:** If config is not loaded, default mismatch causes messages published to `cardealer.events` to never reach consumers on `notification-exchange`.

**Fix:** Align the C# class defaults. Set `AuthService.NotificationServiceRabbitMQSettings.ExchangeName` default to `"notification-exchange"` to match everything else.

---

### CRIT-03: ErrorService Default Exchange Name — `error-exchange` (class) vs `errors.exchange` (config)

**Location:**

- [ErrorServiceRabbitMQSettings](backend/ErrorService/ErrorService.Infrastructure/Services/Messaging/RabbitMQErrorConsumer.cs#L28) class default: `"error-exchange"`
- [AuthService ErrorServiceRabbitMQSettings](backend/AuthService/AuthService.Infrastructure/Services/Messaging/ErrorServiceRabbitMQSettings.cs#L8) class default: `"error-exchange"`
- [appsettings.json (ErrorService)](backend/ErrorService/ErrorService.Api/appsettings.json#L48): `"errors.exchange"`
- [appsettings.json (AuthService)](backend/AuthService/AuthService.Api/appsettings.json#L69): `"errors.exchange"`
- [ErrorService bin/Release appsettings.json](backend/ErrorService/ErrorService.Api/bin/Release/net8.0/appsettings.json): still has `"error-exchange"` (stale build artifact!)

**Description:** The C# class defaults say `"error-exchange"` but appsettings overrides to `"errors.exchange"`. The **Release build artifacts** still have the old `"error-exchange"` value. If the application is deployed from a stale Release build (or config is not loaded), the producer and consumer will target different exchanges.

**Impact:** Error events could be published to `error-exchange` while the consumer listens on `errors.exchange`, causing 100% message loss.

**Fix:**

1. Align class defaults to `"errors.exchange"` in both `ErrorServiceRabbitMQSettings` classes.
2. Clean and rebuild Release artifacts.

---

## 🟠 High Findings (5)

### HIGH-01: Duplicate `RabbitMQErrorEvent` Class Across Services

**Locations:**

- [AuthService.Shared/ErrorMessages/RabbitMQErrorEvent.cs](backend/AuthService/AuthService.Shared/ErrorMessages/RabbitMQErrorEvent.cs)
- [ErrorService.Shared/ErrorMessages/RabbitMQErrorEvent.cs](backend/ErrorService/ErrorService.Shared/ErrorMessages/RabbitMQErrorEvent.cs)

**Description:** Both services define their own `RabbitMQErrorEvent` with the same properties but in different namespaces. The AuthService version has a constructor overload; the ErrorService version doesn't. The `ServiceName` default differs (`"AuthService"` vs `"UnknownService"`). While JSON serialization fields match (both use `[JsonPropertyName]`), any future divergence will cause deserialization failures.

**Fix:** Move `RabbitMQErrorEvent` to `CarDealer.Contracts` or `CarDealer.Shared` and reference from both services.

---

### HIGH-02: Duplicate `FailedEvent` Class in 4+ Services

**Locations:**

- [AuthService.Shared/Messaging/FailedEvent.cs](backend/AuthService/AuthService.Shared/Messaging/FailedEvent.cs)
- [NotificationService.Shared/Messaging/FailedEvent.cs](backend/NotificationService/NotificationService.Shared/Messaging/FailedEvent.cs)
- [MediaService.Shared/Messaging/FailedEvent.cs](backend/MediaService/MediaService.Shared/Messaging/FailedEvent.cs)
- [ErrorService.Infrastructure/Messaging/FailedEvent.cs](backend/ErrorService/ErrorService.Infrastructure/Messaging/FailedEvent.cs)

**Description:** Four separate copies of `FailedEvent` exist. They have minor differences:

- AuthService: `NextRetryAt` is `DateTime?` (nullable), max backoff capped at 16 minutes
- NotificationService/MediaService: `NextRetryAt` is `DateTime` (non-nullable), no cap
- ErrorService: `NextRetryAt` is `DateTime?`, max capped at 16 minutes

The shared library already provides `DeadLetterEvent` in `CarDealer.Shared.Messaging` which supersedes all of these.

**Fix:** Migrate all services to use `CarDealer.Shared.Messaging.DeadLetterEvent` and `ISharedDeadLetterQueue` (PostgreSQL-backed). Remove per-service `FailedEvent` classes.

---

### HIGH-03: Duplicate `IDeadLetterQueue` Interface in 3+ Services

**Locations:**

- [ErrorService.Infrastructure/Messaging/IDeadLetterQueue.cs](backend/ErrorService/ErrorService.Infrastructure/Messaging/IDeadLetterQueue.cs) — synchronous `void Enqueue()`
- Similar in UserService, RoleService
- Shared: [CarDealer.Shared/Messaging/ISharedDeadLetterQueue.cs](backend/_Shared/CarDealer.Shared/Messaging/ISharedDeadLetterQueue.cs) — async `Task EnqueueAsync()`

**Description:** Per-service `IDeadLetterQueue` interfaces use synchronous methods and in-memory storage (data loss on pod restart). The shared `ISharedDeadLetterQueue` uses async methods with PostgreSQL persistence. AuthService's `RabbitMqEventPublisher` still references the old `IDeadLetterQueue`.

**Fix:** Migrate all services to `ISharedDeadLetterQueue` from `CarDealer.Shared`.

---

### HIGH-04: Duplicate Domain Event Classes — MediaService Uses Local Versions Instead of Shared Contracts

**Locations:**

- [MediaService.Domain/Events/MediaUploadedEvent.cs](backend/MediaService/MediaService.Domain/Events/MediaUploadedEvent.cs) — extends local `DomainEvent` base
- [CarDealer.Contracts/Events/Media/MediaUploadedEvent.cs](backend/_Shared/CarDealer.Contracts/Events/Media/MediaUploadedEvent.cs) — extends shared `EventBase`

**Description:** MediaService publishes `MediaService.Domain.Events.MediaUploadedEvent` (extends `DomainEvent` with `string EventId`, `DateTime OccurredOn`), but the shared contract defines `CarDealer.Contracts.Events.Media.MediaUploadedEvent` (extends `EventBase` with `Guid EventId`, `DateTime OccurredAt`). The properties are **completely different**:

| Property   | Domain (Local)                                                                         | Contracts (Shared)                                                     |
| ---------- | -------------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| MediaId    | `string`                                                                               | `Guid`                                                                 |
| EventId    | `string`                                                                               | `Guid`                                                                 |
| Timestamp  | `OccurredOn`                                                                           | `OccurredAt`                                                           |
| Additional | `OwnerId`, `Context`, `MediaType`, `FileName`, `ContentType`, `FileSize`, `StorageKey` | `FileName`, `ContentType`, `FileSizeBytes`, `UploadedBy`, `UploadedAt` |

If any other service tries to consume MediaService events using the shared contracts, **deserialization will fail completely**.

**Fix:** Migrate MediaService publishers to use `CarDealer.Contracts.Events.Media.*` classes, or create a mapping layer.

---

### HIGH-05: AuthService Has Two Separate Publishers for the Same Event

**Locations:**

- [UserRegisteredEventPublisher.cs](backend/AuthService/AuthService.Infrastructure/Events/UserRegisteredEventPublisher.cs) — dedicated publisher, creates own `IConnection`
- [RabbitMqEventPublisher.cs](backend/AuthService/AuthService.Infrastructure/Messaging/RabbitMqEventPublisher.cs) — generic `IEventPublisher`, creates own `IConnection`

**Description:** AuthService has two classes that can publish `UserRegisteredEvent`. Both create their own RabbitMQ connections (not using `SharedRabbitMqConnection`). If both are registered and invoked, the same event could be published twice, or one may be using stale configuration.

**Fix:** Remove `UserRegisteredEventPublisher` and use only the generic `RabbitMqEventPublisher` through `IEventPublisher`. Also migrate to `SharedRabbitMqConnection`.

---

## 🟡 Medium Findings (6)

### MED-01: Orphan Publishers — 5 Events with No Consumer

| Event                      | Publisher    | Routing Key             |
| -------------------------- | ------------ | ----------------------- |
| `PasswordChangedEvent`     | AuthService  | `auth.password.changed` |
| `UserLoggedOutEvent`       | AuthService  | `auth.user.loggedout`   |
| `UserDeletedEvent`         | AuthService  | `auth.user.deleted`     |
| `ErrorLoggedEvent`         | ErrorService | `error.logged`          |
| `ErrorSpikeDetectedEvent`  | ErrorService | `error.spike.detected`  |
| `ServiceDownDetectedEvent` | ErrorService | `error.service.down`    |

**Description:** These events are defined in `CarDealer.Contracts` and can be published via the generic `RabbitMqEventPublisher`, but no consumer subscribes to them. Messages go into the void.

**Impact:** Wasted messages; if future consumers are added, they'll miss all historical events.

**Fix:** Either add consumers in NotificationService (e.g., send security email on `auth.password.changed`, alert on `error.spike.detected`) or stop publishing them until consumers exist.

---

### MED-02: Orphan Media Event Queues — Published but No Consumer

**Location:** [RabbitMQMediaProducer.cs](backend/MediaService/MediaService.Infrastructure/Messaging/RabbitMQMediaProducer.cs#L65-L84)

**Description:** `media.uploaded.queue`, `media.processed.queue`, and `media.deleted.queue` are declared and bound, but the `RabbitMQMediaConsumer` only consumes from `process.media.queue`. Messages accumulate silently.

**Fix:** Either create consumers for these event queues (e.g., for analytics, cache invalidation) or stop declaring/binding them.

---

### MED-03: `VehicleCreatedNotificationConsumer` — No DLQ Configuration

**Location:** [VehicleCreatedNotificationConsumer.cs](backend/NotificationService/NotificationService.Infrastructure/Messaging/VehicleCreatedNotificationConsumer.cs#L152-L157)

**Description:** Queue is declared with `arguments: null` — no dead letter exchange is configured. If a message is NACKed without requeue, it is permanently lost. Other consumers (UserRegistered, UserLoggedIn, KYCStatusChanged, ErrorCritical) all have DLX configured correctly.

**Fix:** Add DLX arguments consistent with other consumers:

```csharp
var queueArgs = new Dictionary<string, object>
{
    { "x-dead-letter-exchange", "cardealer.events.dlx" },
    { "x-dead-letter-routing-key", "vehicle.created" }
};
```

⚠️ This requires **deleting the existing queue first** since queue arguments are immutable.

---

### MED-04: `InvoiceNotificationConsumer` and `PaymentReceiptNotificationConsumer` — No DLQ Configuration

**Locations:**

- [InvoiceNotificationConsumer.cs](backend/NotificationService/NotificationService.Infrastructure/Messaging/InvoiceNotificationConsumer.cs#L147-L153)
- [PaymentReceiptNotificationConsumer.cs](backend/NotificationService/NotificationService.Infrastructure/Messaging/PaymentReceiptNotificationConsumer.cs#L151-L157)

**Description:** Same as MED-03. Both declare queues without DLX arguments. Failed billing notification messages will be lost.

**Fix:** Add DLX arguments to both queues. Requires queue deletion first.

---

### MED-05: Inconsistent DLX Routing Key Pattern

**Locations:**

- `UserRegisteredNotificationConsumer`: DLX routing key = `"auth.user.registered"` (same as main routing key)
- `PriceAlertTriggeredConsumer`: DLX routing key = `"dlx.alert.price.triggered"` (prefixed with `dlx.`)
- `SavedSearchActivatedConsumer`: DLX routing key = `"dlx.alert.savedsearch.activated"` (prefixed with `dlx.`)
- `KYCStatusChangedNotificationConsumer`: DLX routing key = `"kyc.profile.status_changed"` (same as main)

**Description:** Some consumers use the same routing key for DLQ binding, others prefix with `dlx.`. This inconsistency means:

1. For `PriceAlertTriggeredConsumer` and `SavedSearchActivatedConsumer`, the DLQ binding uses `dlx.{routingKey}` but the queue's `x-dead-letter-routing-key` also uses `dlx.{routingKey}`. This is correct internally but inconsistent with other consumers.
2. If someone queries the DLX for all failed messages with a wildcard pattern, they'll need two different patterns.

**Fix:** Standardize on one pattern. Recommended: use the original routing key (without prefix) for all DLX routing keys.

---

### MED-06: `ErrorCriticalEventConsumer` Uses Synchronous `EventingBasicConsumer`

**Location:** [ErrorCriticalEventConsumer.cs](backend/NotificationService/NotificationService.Infrastructure/Messaging/ErrorCriticalEventConsumer.cs#L60)

**Description:** Uses `EventingBasicConsumer` (synchronous) while all other consumers in the codebase use `AsyncEventingBasicConsumer`. The connection factory is created **without** `DispatchConsumersAsync = true`. Using async event handlers inside a synchronous consumer can cause deadlocks.

**Fix:** Switch to `AsyncEventingBasicConsumer` and add `DispatchConsumersAsync = true` to the `ConnectionFactory`.

---

## 🔵 Low Findings (4)

### LOW-01: Multiple RabbitMQ Connections per Service

**Description:** Most services create their own `ConnectionFactory` + `IConnection` per publisher/consumer class instead of using the shared `SharedRabbitMqConnection` from `CarDealer.Shared.Messaging`. AuthService alone creates 3+ connections (one per producer class). Under auto-scaling, this leads to connection exhaustion.

**Fix:** Inject `SharedRabbitMqConnection` and call `CreateChannel()` per class.

---

### LOW-02: `PriceAlertTriggeredConsumer` Adds `x-message-ttl` to Queue

**Location:** [PriceAlertTriggeredConsumer.cs](backend/NotificationService/NotificationService.Infrastructure/Messaging/PriceAlertTriggeredConsumer.cs#L167)

**Description:** This consumer adds `x-message-ttl: 86400000` (24h) to the queue arguments. No other consumer does this. Messages older than 24h will be automatically expired and routed to DLX. This may cause legitimate alerts to be lost if the service is down for >24h.

**Fix:** Remove `x-message-ttl` for consistency, or add it to all consumer queues if it's a desired policy.

---

### LOW-03: AuthService Domain Events vs Shared Contracts

**Locations:**

- [AuthService.Domain/Events/UserRegisteredEvent.cs](backend/AuthService/AuthService.Domain/Events/UserRegisteredEvent.cs) — MediatR `INotification`, `string UserId`
- [CarDealer.Contracts/Events/Auth/UserRegisteredEvent.cs](backend/_Shared/CarDealer.Contracts/Events/Auth/UserRegisteredEvent.cs) — `EventBase`, `Guid UserId`

**Description:** AuthService has internal domain events (MediatR notifications) that mirror shared contracts but with different types (`string` vs `Guid` for `UserId`). The internal ones are used for in-process MediatR handlers; the shared ones for RabbitMQ. This is an acceptable pattern but creates naming confusion.

**Fix:** Rename the domain events (e.g., `UserRegisteredDomainEvent`) to distinguish from the shared contracts.

---

### LOW-04: ContactService, AdminService, and Gateway Have No Messaging

**Description:** `ContactService`, `AdminService`, and `Gateway` have zero RabbitMQ code. ContactService has a `ContactRequestCreatedEvent` domain event but never publishes it to RabbitMQ. AdminService and Gateway don't participate in event-driven messaging at all.

**Impact:** `ContactRequestCreatedEvent` only triggers in-process MediatR handlers. If cross-service notification is needed (e.g., notify a dealer when a buyer contacts them), a publisher must be added.

**Fix:** Consider adding a publisher for `ContactRequestCreatedEvent` in ContactService if cross-service notification is desired.

---

## 🏥 Dead Letter Chain Integrity

| Consumer                               | Has DLX?       | DLX Exchange                         | DLQ Queue                                       | Circular Risk? |
| -------------------------------------- | -------------- | ------------------------------------ | ----------------------------------------------- | -------------- |
| `UserRegisteredNotificationConsumer`   | ✅             | `cardealer.events.dlx` (Direct)      | `notificationservice.user.registered.dlq`       | ❌ No          |
| `UserLoggedInEventConsumer`            | ✅             | `cardealer.events.dlx` (Direct)      | `notificationservice.user.loggedin.dlq`         | ❌ No          |
| `KYCStatusChangedNotificationConsumer` | ✅             | `cardealer.events.dlx` (Direct)      | `notificationservice.kyc.status_changed.dlq`    | ❌ No          |
| `ErrorCriticalEventConsumer`           | ✅             | `cardealer.events.dlx` (Direct)      | `notification-service.error-critical.dlq`       | ❌ No          |
| `UserSettingsChangedEventConsumer`     | ✅             | `cardealer.events.dlx` (Direct)      | `notificationservice.user.settings.changed.dlq` | ❌ No          |
| `PriceAlertTriggeredConsumer`          | ✅             | `cardealer.events.dlx` (Direct)      | implicit from args                              | ❌ No          |
| `SavedSearchActivatedConsumer`         | ✅             | `cardealer.events.dlx` (Direct)      | implicit from args                              | ❌ No          |
| `VehicleCreatedNotificationConsumer`   | ❌ **MISSING** | —                                    | —                                               | —              |
| `InvoiceNotificationConsumer`          | ❌ **MISSING** | —                                    | —                                               | —              |
| `PaymentReceiptNotificationConsumer`   | ❌ **MISSING** | —                                    | —                                               | —              |
| `RabbitMQNotificationConsumer`         | ✅             | `notification-exchange.dlx` (Direct) | `notification-queue.dlq`                        | ❌ No          |
| `RabbitMQErrorConsumer`                | ✅             | `errors.exchange.dlx` (Direct)       | `errors.queue.dlq`                              | ❌ No          |
| `RabbitMQMediaConsumer`                | ✅             | `media.commands.dlx` (Direct)        | `process.media.queue.dlq`                       | ❌ No          |

**Verdict:** No circular DL routing detected. 3 consumers missing DLQ entirely (MED-03, MED-04).

---

## 🔄 Consumer Retry & Error Handling Patterns

| Consumer                               | JSON Error Handling               | Max Retries       | Backoff                  | ACK/NACK                 |
| -------------------------------------- | --------------------------------- | ----------------- | ------------------------ | ------------------------ |
| `UserRegisteredNotificationConsumer`   | ✅ NACKs to DLQ                   | 3                 | Exponential (2s, 4s, 8s) | ✅ Correct               |
| `UserLoggedInEventConsumer`            | ✅ NACKs to DLQ                   | 3                 | Exponential              | ✅ Correct               |
| `ErrorCriticalEventConsumer`           | ✅ NACKs no-requeue               | No retries        | None                     | ⚠️ No retry at all       |
| `KYCStatusChangedNotificationConsumer` | ✅ NACKs to DLQ on deser fail     | Unlimited requeue | None                     | ⚠️ Requeues indefinitely |
| `VehicleCreatedNotificationConsumer`   | ✅ NACKs no-requeue on deser fail | Unlimited requeue | None                     | ⚠️ Requeues indefinitely |
| `InvoiceNotificationConsumer`          | ✅ NACKs no-requeue on deser fail | Unlimited requeue | None                     | ⚠️ Requeues indefinitely |
| `PaymentReceiptNotificationConsumer`   | ✅ Same as above                  | Unlimited requeue | None                     | ⚠️ Requeues indefinitely |
| `PriceAlertTriggeredConsumer`          | ✅ NACKs no-requeue on deser fail | Unlimited requeue | None                     | ⚠️ Requeues indefinitely |
| `SavedSearchActivatedConsumer`         | ✅ Same as above                  | Unlimited requeue | None                     | ⚠️ Requeues indefinitely |
| `RabbitMQNotificationConsumer`         | ✅ NACKs no-requeue               | No retries        | None                     | ⚠️ No retry              |
| `RabbitMQErrorConsumer`                | ✅ NACKs with x-death check       | 5                 | Via requeue              | ✅ Correct               |
| `RabbitMQMediaConsumer`                | ✅ NACKs no-requeue               | No retries        | None                     | ✅ Goes to DLQ           |

**Key concern:** Several consumers do `BasicNack(requeue: true)` on transient errors without a retry limit. Without DLQ, this creates an **infinite retry loop** for persistent failures (e.g., database down). Consumers with DLQ configured are safe since RabbitMQ routes to DLQ after NACK without requeue.

---

## 📝 Recommendations Summary (Priority Order)

1. **[CRIT-01]** Fix `errors.exchange` type from `Direct` to `Topic` in AuthService `RabbitMQErrorProducer`
2. **[CRIT-02]** Align `NotificationServiceRabbitMQSettings.ExchangeName` default to `"notification-exchange"` in AuthService
3. **[CRIT-03]** Align `ErrorServiceRabbitMQSettings.ExchangeName` default to `"errors.exchange"` in both AuthService and ErrorService; rebuild Release artifacts
4. **[HIGH-01/02/03]** Consolidate duplicated `RabbitMQErrorEvent`, `FailedEvent`, and `IDeadLetterQueue` into shared libraries
5. **[HIGH-04]** Migrate MediaService to use `CarDealer.Contracts` event types
6. **[HIGH-05]** Remove duplicate `UserRegisteredEventPublisher` in AuthService
7. **[MED-03/04]** Add DLQ to `VehicleCreated`, `Invoice`, `PaymentReceipt` consumer queues (requires queue deletion)
8. **[MED-05]** Standardize DLX routing key pattern across all consumers
9. **[MED-06]** Switch `ErrorCriticalEventConsumer` to `AsyncEventingBasicConsumer`
10. **[LOW-01]** Migrate all services to use `SharedRabbitMqConnection`
