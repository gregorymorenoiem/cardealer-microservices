# 🐰 RabbitMQ Event Infrastructure Audit — OKLA Microservices

**Audit Date:** 2026-03-07  
**Auditor:** GitHub Copilot (automated)  
**Scope:** All microservices in `backend/`

---

## 1. Complete Event Catalog (CarDealer.Contracts)

All shared event contracts live in `_Shared/CarDealer.Contracts/Events/`. Every event extends `EventBase` (which implements `IEvent`).

### 1.1 Auth Events

| Event Class            | `EventType`                  | Domain |
| ---------------------- | ---------------------------- | ------ |
| `UserRegisteredEvent`  | `auth.user.registered`       | Auth   |
| `UserLoggedInEvent`    | `auth.user.loggedin`         | Auth   |
| `UserLoggedOutEvent`   | `auth.user.loggedout`        | Auth   |
| `UserDeletedEvent`     | `auth.user.deleted`          | Auth   |
| `PasswordChangedEvent` | `auth.user.password_changed` | Auth   |

### 1.2 Error Events

| Event Class                | `EventType`            | Domain |
| -------------------------- | ---------------------- | ------ |
| `ErrorCriticalEvent`       | `error.critical`       | Error  |
| `ErrorLoggedEvent`         | `error.logged`         | Error  |
| `ErrorSpikeDetectedEvent`  | `error.spike.detected` | Error  |
| `ServiceDownDetectedEvent` | `error.service.down`   | Error  |

### 1.3 Vehicle Events

| Event Class             | `EventType`            | Domain  |
| ----------------------- | ---------------------- | ------- |
| `VehicleCreatedEvent`   | `vehicle.created`      | Vehicle |
| `VehicleSoldEvent`      | `vehicle.sold`         | Vehicle |
| `VehicleUpdatedEvent`   | `vehicle.updated`      | Vehicle |
| `VehicleDeletedEvent`   | `vehicle.deleted`      | Vehicle |
| `VehiclePublishedEvent` | `vehicle.published`    | Vehicle |
| `LeadCreatedEvent`      | `vehicle.lead.created` | Vehicle |

### 1.4 Billing Events

| Event Class                        | `EventType`                 | Domain  |
| ---------------------------------- | --------------------------- | ------- |
| `PaymentCompletedEvent`            | `billing.payment.completed` | Billing |
| `InvoiceGeneratedEvent`            | `billing.invoice.generated` | Billing |
| `PublicationCreditsPurchasedEvent` | `billing.credits.purchased` | Billing |

### 1.5 Media Events

| Event Class                  | `EventType`               | Domain |
| ---------------------------- | ------------------------- | ------ |
| `MediaUploadedEvent`         | `media.uploaded`          | Media  |
| `MediaProcessedEvent`        | `media.processed`         | Media  |
| `MediaDeletedEvent`          | `media.deleted`           | Media  |
| `MediaProcessingFailedEvent` | `media.processing.failed` | Media  |

### 1.6 KYC Events

| Event Class                    | `EventType`                  | Domain |
| ------------------------------ | ---------------------------- | ------ |
| `KYCProfileStatusChangedEvent` | `kyc.profile.status_changed` | KYC    |

### 1.7 Notification Events

| Event Class                       | `EventType`                     | Domain       |
| --------------------------------- | ------------------------------- | ------------ |
| `NotificationSentEvent`           | `notification.sent`             | Notification |
| `NotificationFailedEvent`         | `notification.failed`           | Notification |
| `TeamsAlertSentEvent`             | `notification.teams.alert.sent` | Notification |
| `EmailNotificationRequestedEvent` | `notification.email.requested`  | Notification |
| `SmsNotificationRequestedEvent`   | `notification.sms.requested`    | Notification |
| `PushNotificationRequestedEvent`  | `notification.push.requested`   | Notification |

### 1.8 Alert Events

| Event Class                | `EventType`                   | Domain |
| -------------------------- | ----------------------------- | ------ |
| `PriceAlertTriggeredEvent` | `alert.price.triggered`       | Alert  |
| `SavedSearchMatchEvent`    | `alert.savedsearch.activated` | Alert  |

### 1.9 User Events

| Event Class                | `EventType`             | Domain |
| -------------------------- | ----------------------- | ------ |
| `UserSettingsChangedEvent` | `user.settings.changed` | User   |

### 1.10 Audit Events

| Event Class                    | `EventType`                 | Domain |
| ------------------------------ | --------------------------- | ------ |
| `AuditLogCreatedEvent`         | `audit.log.created`         | Audit  |
| `ComplianceEventRecordedEvent` | `audit.compliance.recorded` | Audit  |

### 1.11 Seller/Dealer Events

| Event Class                        | `EventType` | Domain |
| ---------------------------------- | ----------- | ------ |
| `SellerCreatedEvent`               | TBD         | Seller |
| `SellerConversionRequestedEvent`   | TBD         | Seller |
| `DealerCreatedEvent`               | TBD         | Dealer |
| `DealerRegistrationRequestedEvent` | TBD         | Dealer |

### 1.12 ContactService Domain Event (local only, not in Contracts)

| Event Class                  | `EventType`                      | Domain  |
| ---------------------------- | -------------------------------- | ------- |
| `ContactRequestCreatedEvent` | `contact.contactrequest.created` | Contact |

---

## 2. Exchange Topology

### 2.1 Exchanges Found

| Exchange Name                              | Type       | Declared By                                                                                                   | Purpose                                       |
| ------------------------------------------ | ---------- | ------------------------------------------------------------------------------------------------------------- | --------------------------------------------- |
| `cardealer.events`                         | **Topic**  | AuthService, ErrorService, NotificationService, VehiclesSaleService, BillingService, KYCService, MediaService | Main event bus for domain events              |
| `errors.exchange`                          | **Topic**  | AuthService (producer), ErrorService (consumer)                                                               | Dedicated error reporting pipeline            |
| `notification-exchange`                    | **Direct** | AuthService (producer), NotificationService (consumer)                                                        | Legacy notification pipeline from AuthService |
| `media.events` (`MediaEventsExchange`)     | **Topic**  | MediaService                                                                                                  | Internal media domain events                  |
| `media.commands` (`MediaCommandsExchange`) | **Direct** | MediaService                                                                                                  | Internal media processing commands            |
| `cardealer.events.dlx`                     | **Direct** | NotificationService consumers                                                                                 | Dead Letter Exchange for failed events        |
| `errors.exchange.dlx`                      | **Direct** | ErrorService consumer                                                                                         | Dead Letter Exchange for failed error events  |
| `notification-exchange.dlx`                | **Direct** | NotificationService (RabbitMQNotificationConsumer)                                                            | DLX for notification-queue                    |
| `media.commands.dlx`                       | **Direct** | MediaService consumer                                                                                         | DLX for media processing                      |

---

## 3. Per-Service Publishing & Consuming Analysis

---

### 3.1 AuthService

#### PUBLISHES:

| Event                 | Exchange                | Routing Key            | Publisher Class                | DLQ/Retry                                  |
| --------------------- | ----------------------- | ---------------------- | ------------------------------ | ------------------------------------------ |
| `UserRegisteredEvent` | `cardealer.events`      | `auth.user.registered` | `UserRegisteredEventPublisher` | ❌ No DLQ — fire-and-forget with try/catch |
| `UserLoggedInEvent`   | `cardealer.events`      | (uses `EventType`)     | `RabbitMqEventPublisher`       | ✅ Circuit Breaker + DLQ fallback          |
| Error events          | `errors.exchange`       | `error.created`        | `RabbitMQErrorProducer`        | ✅ Circuit Breaker + DLQ fallback          |
| Notification events   | `notification-exchange` | `notification.auth`    | `RabbitMQNotificationProducer` | ✅ Circuit Breaker + retry queue           |

#### CONSUMES: None (AuthService does not consume RabbitMQ events)

#### ⚠️ ISSUES:

1. **🔴 CRITICAL — Routing Key Mismatch for PaymentCompletedEvent**: `PaymentCompletedEventPublisher` uses routing key `payment.completed` but the contract's `EventType` is `billing.payment.completed`. The `NotificationService.PaymentReceiptNotificationConsumer` binds with `billing.payment.completed`. **Messages published via `PaymentCompletedEventPublisher` will NEVER reach the consumer.**
   - **However**, `BillingService.RabbitMqEventPublisher` uses `@event.EventType` (= `billing.payment.completed`) as routing key, which IS correct.
   - **Fix**: `PaymentCompletedEventPublisher` appears to be a LEGACY/DUPLICATE publisher. Verify which publisher is actually called in the webhook flow and remove the other.

2. **🟡 WARNING — Dual Publisher Pattern**: `UserRegisteredEventPublisher` (hardcoded routing key `auth.user.registered`) vs `RabbitMqEventPublisher` (generic, uses `EventType`). Two publishers exist for the same purpose. Risk of confusion about which is used.

3. **🟡 WARNING — `UserRegisteredEventPublisher` lacks retry/DLQ**: Just a try/catch + rethrow. If RabbitMQ is down, the user registration may fail.

---

### 3.2 ErrorService

#### PUBLISHES:

| Event                                                                | Exchange           | Routing Key        | Publisher Class          | DLQ/Retry                         |
| -------------------------------------------------------------------- | ------------------ | ------------------ | ------------------------ | --------------------------------- |
| Any `IEvent` (e.g., `ErrorCriticalEvent`, `ErrorSpikeDetectedEvent`) | `cardealer.events` | `@event.EventType` | `RabbitMqEventPublisher` | ✅ Circuit Breaker + InMemory DLQ |

#### CONSUMES:

| Event                            | Exchange          | Queue          | Routing Key     | Consumer Class          | DLQ                                             |
| -------------------------------- | ----------------- | -------------- | --------------- | ----------------------- | ----------------------------------------------- |
| Error events (from all services) | `errors.exchange` | `errors.queue` | `error.created` | `RabbitMQErrorConsumer` | ✅ `errors.queue.dlq` via `errors.exchange.dlx` |

#### ⚠️ ISSUES:

1. **🔴 CRITICAL — Config Drift Between Dev and Release**: Dev `appsettings.json` has `QueueName: "errors.queue"`, `ExchangeName: "errors.exchange"`, `RoutingKey: "error.created"`. But Release build has `QueueName: "error-queue"`, `ExchangeName: "error-exchange"`, `RoutingKey: "error.routing.key"`. **Producer and consumer will use DIFFERENT names in Release**, causing silent message loss.

2. **🟡 WARNING — Two separate exchanges**: `cardealer.events` (Topic) for general events AND `errors.exchange` (Topic) for error-specific events. The ErrorService publishes `ErrorCriticalEvent` to `cardealer.events`, but `RabbitMQErrorConsumer` only listens on `errors.exchange`. This means:
   - Critical error events published to `cardealer.events` are picked up by NotificationService's `ErrorCriticalEventConsumer`.
   - Error logging events from other services go through `errors.exchange` → `errors.queue`.
   - This dual-exchange design works but is confusing and poorly documented.

---

### 3.3 NotificationService

#### PUBLISHES: None (NotificationService only consumes events)

#### CONSUMES:

| Event                                  | Exchange                | Queue                                             | Routing Key                   | Consumer Class                         | DLQ                                                                           | Registered                              |
| -------------------------------------- | ----------------------- | ------------------------------------------------- | ----------------------------- | -------------------------------------- | ----------------------------------------------------------------------------- | --------------------------------------- |
| `UserRegisteredEvent`                  | `cardealer.events`      | `notificationservice.user.registered`             | `auth.user.registered`        | `UserRegisteredNotificationConsumer`   | ✅ `notificationservice.user.registered.dlq` via `cardealer.events.dlx`       | ✅ Program.cs                           |
| `UserLoggedInEvent`                    | `cardealer.events`      | `notificationservice.user.loggedin`               | `auth.user.loggedin`          | `UserLoggedInEventConsumer`            | ✅ `notificationservice.user.loggedin.dlq` via `cardealer.events.dlx`         | ✅ Program.cs                           |
| `UserSettingsChangedEvent`             | `cardealer.events`      | `notificationservice.user.settings.changed`       | `user.settings.changed`       | `UserSettingsChangedEventConsumer`     | ✅ `notificationservice.user.settings.changed.dlq` via `cardealer.events.dlx` | ✅ Program.cs                           |
| `ErrorCriticalEvent`                   | `cardealer.events`      | `notification-service.error-critical`             | `error.critical`              | `ErrorCriticalEventConsumer`           | ✅ `notification-service.error-critical.dlq` via `cardealer.events.dlx`       | ✅ Program.cs                           |
| `VehicleCreatedEvent`                  | `cardealer.events`      | `notificationservice.vehicle.created`             | `vehicle.created`             | `VehicleCreatedNotificationConsumer`   | ❌ No DLQ configured                                                          | ✅ Program.cs                           |
| `PaymentCompletedEvent`                | `cardealer.events`      | `notificationservice.payment.completed`           | `billing.payment.completed`   | `PaymentReceiptNotificationConsumer`   | ❌ No DLQ configured                                                          | ✅ Program.cs                           |
| `InvoiceGeneratedEvent`                | `cardealer.events`      | `notificationservice.invoice.generated`           | `billing.invoice.generated`   | `InvoiceNotificationConsumer`          | ❌ No DLQ configured                                                          | ✅ Program.cs                           |
| `KYCProfileStatusChangedEvent`         | `cardealer.events`      | `notificationservice.kyc.status_changed`          | `kyc.profile.status_changed`  | `KYCStatusChangedNotificationConsumer` | ❌ No DLQ configured                                                          | ✅ via `ServiceCollectionExtensions.cs` |
| `PriceAlertTriggeredEvent`             | `cardealer.events`      | `notificationservice.alert.price_triggered`       | `alert.price.triggered`       | `PriceAlertTriggeredConsumer`          | ❌ No DLQ configured                                                          | ✅ Program.cs                           |
| `SavedSearchMatchEvent`                | `cardealer.events`      | `notificationservice.alert.savedsearch_activated` | `alert.savedsearch.activated` | `SavedSearchActivatedConsumer`         | ❌ No DLQ configured                                                          | ✅ Program.cs                           |
| Legacy notification (from AuthService) | `notification-exchange` | `notification-queue`                              | `notification.auth`           | `RabbitMQNotificationConsumer`         | ✅ `notification-queue.dlq` via `notification-exchange.dlx`                   | ✅ via `ServiceCollectionExtensions.cs` |

#### ⚠️ ISSUES:

1. **🔴 CRITICAL — 6 consumers lack DLQ**: `VehicleCreatedNotificationConsumer`, `PaymentReceiptNotificationConsumer`, `InvoiceNotificationConsumer`, `KYCStatusChangedNotificationConsumer`, `PriceAlertTriggeredConsumer`, `SavedSearchActivatedConsumer` all use `requeue: true` on failure, creating **infinite retry loops**. Failed messages will be re-delivered endlessly.

2. **🟡 WARNING — Queue naming inconsistency**: Most queues use pattern `notificationservice.<domain>.<event>` but `ErrorCriticalEventConsumer` uses `notification-service.error-critical` (dash-separated). Should standardize.

3. **🟡 WARNING — `RabbitMQNotificationConsumer` uses `ExchangeType.Direct`** while `cardealer.events` is **Topic**. This is actually a separate exchange (`notification-exchange`), so it works, but it's the only consumer using a different exchange type for its primary exchange.

---

### 3.4 MediaService

#### PUBLISHES:

| Event                          | Exchange                   | Routing Key        | Publisher Class          | DLQ/Retry            |
| ------------------------------ | -------------------------- | ------------------ | ------------------------ | -------------------- |
| `MediaUploadedEvent` (domain)  | `media.events` (Topic)     | `media.uploaded`   | `RabbitMQMediaProducer`  | ❌ No DLQ            |
| `MediaProcessedEvent` (domain) | `media.events` (Topic)     | `media.processed`  | `RabbitMQMediaProducer`  | ❌ No DLQ            |
| `MediaDeletedEvent` (domain)   | `media.events` (Topic)     | `media.deleted`    | `RabbitMQMediaProducer`  | ❌ No DLQ            |
| Process media command          | `media.commands` (Direct)  | `process.media`    | `RabbitMQMediaProducer`  | ❌ No DLQ on publish |
| Any `IEvent`                   | `cardealer.events` (Topic) | `@event.EventType` | `RabbitMqEventPublisher` | ❌ No DLQ            |

#### CONSUMES:

| Event                 | Exchange         | Queue                 | Routing Key     | Consumer Class          | DLQ                                                   |
| --------------------- | ---------------- | --------------------- | --------------- | ----------------------- | ----------------------------------------------------- |
| Process media command | `media.commands` | `process.media.queue` | `process.media` | `RabbitMQMediaConsumer` | ✅ `process.media.queue.dlq` via `media.commands.dlx` |

#### ⚠️ ISSUES:

1. **🔴 CRITICAL — MediaService publishes to `media.events` but NO ONE consumes it**. The `media.uploaded.queue`, `media.processed.queue`, `media.deleted.queue` queues are declared by MediaService's producer but no consumer binds to them (not in MediaService, not in VehiclesSaleService, not anywhere). Messages accumulate forever.

2. **🟡 WARNING — `RabbitMQMediaProducer` has no retry/DLQ on publish side**. If RabbitMQ is down during a domain event publish, the event is silently lost.

3. **🟡 WARNING — MediaService also has `RabbitMqEventPublisher` for `cardealer.events` but it's unclear when/if it's used vs `RabbitMQMediaProducer`**.

---

### 3.5 ContactService

#### PUBLISHES: ❌ **NONE** — No RabbitMQ integration whatsoever

#### CONSUMES: ❌ **NONE**

#### ⚠️ ISSUES:

1. **🔴 CRITICAL — ContactService has NO RabbitMQ integration**. The `ContactRequestCreatedEvent` domain event exists in `ContactService.Domain/Events/` with `EventType = "contact.contactrequest.created"`, but:
   - No `RabbitMQ` section in `appsettings.json`
   - No `IEventPublisher` registration
   - No publisher class exists
   - The event is NEVER published to RabbitMQ
   - ContactService calls NotificationService via **synchronous HTTP** instead (via `NotificationServiceClient`)

2. **🟡 WARNING — Architectural inconsistency**: All other services use async RabbitMQ for notifications, but ContactService uses sync HTTP. This creates a coupling point and single point of failure.

---

### 3.6 BillingService

#### PUBLISHES:

| Event                   | Exchange           | Routing Key                                   | Publisher Class                           | DLQ/Retry                                  |
| ----------------------- | ------------------ | --------------------------------------------- | ----------------------------------------- | ------------------------------------------ |
| `PaymentCompletedEvent` | `cardealer.events` | `billing.payment.completed` (via `EventType`) | `RabbitMqEventPublisher`                  | ❌ No DLQ — catches exception and rethrows |
| `PaymentCompletedEvent` | `cardealer.events` | `payment.completed` ⚠️                        | `PaymentCompletedEventPublisher` (LEGACY) | ❌ No DLQ                                  |
| `InvoiceGeneratedEvent` | `cardealer.events` | `billing.invoice.generated` (via `EventType`) | `RabbitMqEventPublisher`                  | ❌ Same as above                           |

#### CONSUMES: ❌ **NONE**

#### ⚠️ ISSUES:

1. **🔴 CRITICAL — Routing Key Mismatch**: `PaymentCompletedEventPublisher` hardcodes routing key `payment.completed`, but the consumer (`PaymentReceiptNotificationConsumer`) binds to `billing.payment.completed`. If `PaymentCompletedEventPublisher` is the one being used in the webhook flow, **payment receipt emails are never sent**.
   - The generic `RabbitMqEventPublisher` uses `@event.EventType` = `billing.payment.completed` which IS correct.
   - **Action required**: Determine which publisher the `StripeWebhooksController` actually calls and deprecate the other.

2. **🟡 WARNING — No Circuit Breaker**: `BillingService.RabbitMqEventPublisher` has no Polly resilience. If RabbitMQ is briefly down during a Stripe webhook, the event is lost.

3. **🟡 WARNING — No DLQ on publish side**: Failed publishes throw exceptions. For payment events, this is especially dangerous — the Stripe webhook may succeed but the event never reaches NotificationService.

---

### 3.7 KYCService

#### PUBLISHES:

| Event                          | Exchange           | Routing Key                                    | Publisher Class     | DLQ/Retry                                                  |
| ------------------------------ | ------------------ | ---------------------------------------------- | ------------------- | ---------------------------------------------------------- |
| `KYCProfileStatusChangedEvent` | `cardealer.events` | `kyc.profile.status_changed` (via `EventType`) | `KYCEventPublisher` | ❌ Best-effort — catches exception, logs, does NOT rethrow |

#### CONSUMES: ❌ **NONE**

#### ⚠️ ISSUES:

1. **🟡 WARNING — Silent failure on publish**: `KYCEventPublisher` swallows exceptions. If RabbitMQ is down, the KYC approval/rejection email will never be sent and nobody will know.

---

### 3.8 VehiclesSaleService

#### PUBLISHES:

| Event                 | Exchange           | Routing Key       | Publisher Class                | DLQ/Retry                   |
| --------------------- | ------------------ | ----------------- | ------------------------------ | --------------------------- |
| `VehicleCreatedEvent` | `cardealer.events` | `vehicle.created` | `VehicleCreatedEventPublisher` | ❌ No DLQ — fire-and-forget |

#### CONSUMES: ❌ **NONE**

#### ⚠️ ISSUES:

1. **🟡 WARNING — No retry/DLQ on publish**: Same pattern as AuthService's dedicated publisher — if RabbitMQ is down, the exception propagates and potentially fails the vehicle creation.

---

### 3.9 AdminService

#### PUBLISHES: ❌ **NONE** (uses HTTP to NotificationService)

#### CONSUMES: ❌ **NONE**

No RabbitMQ integration found. Communicates with NotificationService via `INotificationServiceClient` (HTTP).

---

## 4. Event Flow Map

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        EXCHANGES                                         │
├──────────────────────────┬───────────────────┬──────────────────────────┤
│   cardealer.events       │  errors.exchange  │  notification-exchange   │
│   (Topic)                │  (Topic)          │  (Direct)               │
├──────────────────────────┼───────────────────┼──────────────────────────┤
│                          │                   │                          │
│  PUBLISHERS:             │  PUBLISHERS:      │  PUBLISHERS:             │
│  ├ AuthService           │  ├ AuthService    │  └ AuthService           │
│  │  auth.user.registered │  │  error.created │    notification.auth     │
│  │  auth.user.loggedin   │  └ (all services  │                          │
│  ├ VehiclesSaleService   │     via Shared    │                          │
│  │  vehicle.created      │     ErrorHandler) │                          │
│  ├ BillingService        │                   │                          │
│  │  billing.payment.*    │                   │                          │
│  │  billing.invoice.*    │                   │                          │
│  ├ KYCService            │                   │                          │
│  │  kyc.profile.*        │                   │                          │
│  ├ ErrorService          │                   │                          │
│  │  error.critical       │                   │                          │
│  │  error.spike.detected │                   │                          │
│  └ MediaService          │                   │                          │
│     (also media.events)  │                   │                          │
│                          │                   │                          │
│  CONSUMERS:              │  CONSUMERS:       │  CONSUMERS:              │
│  └ NotificationService   │  └ ErrorService   │  └ NotificationService   │
│     (10 consumers)       │    errors.queue   │    notification-queue    │
│                          │                   │                          │
├──────────────────────────┼───────────────────┼──────────────────────────┤
│ media.events (Topic)     │ media.commands    │                          │
│ Publisher: MediaService  │ (Direct)          │                          │
│ Consumer: ❌ NOBODY      │ Consumer: Media   │                          │
│ ⚠️ ORPHANED EXCHANGE    │ Service           │                          │
└──────────────────────────┴───────────────────┴──────────────────────────┘
```

---

## 5. Missing Event Flows

| #   | Gap                                                       | Expected Publisher                                                                      | Expected Consumer                                                                                                                    | Severity  | Status                                               |
| --- | --------------------------------------------------------- | --------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ | --------- | ---------------------------------------------------- |
| 1   | **ContactService → notification on contact request**      | ContactService should publish `contact.contactrequest.created`                          | NotificationService (email to seller)                                                                                                | 🔴 HIGH   | ❌ Uses HTTP instead of events. No async decoupling. |
| 2   | **MediaService → VehiclesSaleService on media processed** | MediaService publishes `media.processed` to `media.events` exchange                     | VehiclesSaleService should consume to update vehicle media status                                                                    | 🔴 HIGH   | ❌ `media.events` has no consumers at all            |
| 3   | **BillingService → subscription activated/cancelled**     | BillingService should publish subscription lifecycle events                             | UserService, AdminService, VehiclesSaleService                                                                                       | 🟡 MEDIUM | ❌ No subscription events exist in Contracts         |
| 4   | **AuthService → welcome email after registration**        | AuthService publishes `auth.user.registered`                                            | NotificationService consumer exists but says "NO longer sends welcome email" — email sent by AuthService directly after verification | 🟢 OK     | ✅ By design (verification-first flow)               |
| 5   | **ContactService → audit on contact creation**            | ContactService should publish event                                                     | AuditService                                                                                                                         | 🟡 MEDIUM | ❌ Not implemented                                   |
| 6   | **MediaService → notification on processing failure**     | MediaService publishes `media.processing.failed`                                        | NotificationService (alert admin)                                                                                                    | 🟡 MEDIUM | ❌ No consumer for `media.processing.failed`         |
| 7   | **VehiclesSaleService → vehicle updated/sold/deleted**    | VehiclesSaleService should publish `vehicle.updated`, `vehicle.sold`, `vehicle.deleted` | NotificationService, AlertService                                                                                                    | 🟡 MEDIUM | ❌ Only `vehicle.created` is published               |

---

## 6. Configuration Consistency Issues

### 6.1 RabbitMQ Connection Config Key Inconsistency

| Service                | Host Key                                   | Username Key                                   | Notes                       |
| ---------------------- | ------------------------------------------ | ---------------------------------------------- | --------------------------- |
| AuthService            | `RabbitMQ:Host`                            | `RabbitMQ:Username`                            | —                           |
| ErrorService Publisher | `RabbitMQ:HostName`                        | `RabbitMQ:UserName`                            | ⚠️ Different casing         |
| ErrorService Consumer  | `RabbitMQ:HostName`                        | `RabbitMQ:UserName`                            | ⚠️ Different casing         |
| NotificationService    | `RabbitMQ:Host` **OR** `RabbitMQ:HostName` | `RabbitMQ:Username` **OR** `RabbitMQ:UserName` | ⚠️ Each consumer tries both |
| MediaService           | `RabbitMQ.HostName` (via Options)          | `RabbitMQ.UserName` (via Options)              | ⚠️ Yet another pattern      |
| BillingService         | `RabbitMQ:Host`                            | `RabbitMQ:Username`                            | —                           |

**Impact**: If the `appsettings` uses `HostName` but the code reads `Host`, the fallback to `localhost` kicks in — which works in dev but **silently fails in production**.

### 6.2 Exchange Name Mismatches

| appsettings Section                               | Value in Dev      | Value in Release/bin   | Risk        |
| ------------------------------------------------- | ----------------- | ---------------------- | ----------- |
| `ErrorService.ExchangeName` (AuthService)         | `errors.exchange` | `errors.exchange`      | ✅ OK       |
| `ErrorService.QueueName` (AuthService)            | `errors.queue`    | `errors.queue`         | ✅ OK       |
| `ErrorService.ExchangeName` (ErrorService source) | `errors.exchange` | `error-exchange` ⚠️    | 🔴 Mismatch |
| `ErrorService.QueueName` (ErrorService source)    | `errors.queue`    | `error-queue` ⚠️       | 🔴 Mismatch |
| `ErrorService.RoutingKey` (ErrorService source)   | `error.created`   | `error.routing.key` ⚠️ | 🔴 Mismatch |

### 6.3 Queue Arguments Immutability Risk

Several queues have DLX arguments set (`x-dead-letter-exchange`, `x-dead-letter-routing-key`). Per RabbitMQ rules, changing these requires **deleting the queue first**. The following queues are at risk if DLX config ever changes:

- `errors.queue` (ErrorService)
- `notification-queue` (NotificationService)
- `notificationservice.user.registered` (NotificationService)
- `notificationservice.user.loggedin` (NotificationService)
- `notificationservice.user.settings.changed` (NotificationService)
- `notification-service.error-critical` (NotificationService)
- `process.media.queue` (MediaService)

---

## 7. DLQ Status Summary

| Service                 | Publisher DLQ                                                | Consumer DLQ                                   | Notes                                    |
| ----------------------- | ------------------------------------------------------------ | ---------------------------------------------- | ---------------------------------------- |
| **AuthService**         | ✅ `RabbitMqEventPublisher` (Circuit Breaker + InMemory DLQ) | N/A (no consumers)                             | `UserRegisteredEventPublisher` lacks DLQ |
|                         | ✅ `RabbitMQErrorProducer` (Circuit Breaker + DLQ)           |                                                |                                          |
|                         | ✅ `RabbitMQNotificationProducer` (retry queue)              |                                                |                                          |
| **ErrorService**        | ✅ `RabbitMqEventPublisher` (Circuit Breaker + InMemory DLQ) | ✅ `errors.queue.dlq` (RabbitMQ-native)        | Good                                     |
| **NotificationService** | N/A                                                          | ⚠️ **Mixed** — 4 consumers have DLQ, 6 don't   | See table in §3.3                        |
| **MediaService**        | ❌ No DLQ on `RabbitMQMediaProducer`                         | ✅ `process.media.queue.dlq` (RabbitMQ-native) | Producer-side gap                        |
| **BillingService**      | ❌ No DLQ, no Circuit Breaker                                | N/A (no consumers)                             | 🔴 Dangerous for payment events          |
| **KYCService**          | ❌ Best-effort, exceptions swallowed                         | N/A                                            | Lost events on RabbitMQ downtime         |
| **VehiclesSaleService** | ❌ No DLQ                                                    | N/A                                            | Exception propagates up                  |
| **ContactService**      | N/A (no RabbitMQ)                                            | N/A                                            | —                                        |
| **AdminService**        | N/A (no RabbitMQ)                                            | N/A                                            | —                                        |

---

## 8. Critical Fixes Required (Prioritized)

### 🔴 P0 — Must Fix Immediately

| #   | Issue                                                                                                                         | Service                            | Fix                                                                                                                                                                                                                                                                                                                                          |
| --- | ----------------------------------------------------------------------------------------------------------------------------- | ---------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | **ErrorService config drift** (dev vs release exchange/queue/routing names)                                                   | ErrorService                       | Ensure `appsettings.json` in source is authoritative. Delete stale Release build artifacts. Verify k8s ConfigMap uses correct names: `errors.exchange`, `errors.queue`, `error.created`.                                                                                                                                                     |
| 2   | **BillingService `PaymentCompletedEventPublisher` routing key mismatch** (`payment.completed` vs `billing.payment.completed`) | BillingService                     | Determine which publisher is used by `StripeWebhooksController`. If it's `RabbitMqEventPublisher` (generic), delete `PaymentCompletedEventPublisher`. If it's the dedicated one, change its routing key to `billing.payment.completed`.                                                                                                      |
| 3   | **6 NotificationService consumers lack DLQ** → infinite retry loops                                                           | NotificationService                | Add DLQ configuration (x-dead-letter-exchange, x-dead-letter-routing-key) to: `VehicleCreatedNotificationConsumer`, `PaymentReceiptNotificationConsumer`, `InvoiceNotificationConsumer`, `KYCStatusChangedNotificationConsumer`, `PriceAlertTriggeredConsumer`, `SavedSearchActivatedConsumer`. Add max retry count check before requeueing. |
| 4   | **MediaService `media.events` exchange has no consumers**                                                                     | MediaService + VehiclesSaleService | Either add consumers in VehiclesSaleService (and NotificationService for `media.processing.failed`) or remove the orphaned exchange/queues.                                                                                                                                                                                                  |

### 🟡 P1 — Fix Soon

| #   | Issue                                                                            | Service             | Fix                                                                                                                                                                                 |
| --- | -------------------------------------------------------------------------------- | ------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 5   | **BillingService publisher has no Circuit Breaker/DLQ**                          | BillingService      | Add Polly Circuit Breaker and DLQ fallback to `RabbitMqEventPublisher`. Payment events must not be lost.                                                                            |
| 6   | **ContactService uses sync HTTP for notifications**                              | ContactService      | Migrate to RabbitMQ event publishing. Add `IEventPublisher` registration, publish `ContactRequestCreatedEvent` to `cardealer.events` exchange. Add consumer in NotificationService. |
| 7   | **RabbitMQ config key inconsistency** (`Host`/`HostName`, `Username`/`UserName`) | All services        | Standardize to a single convention (recommend `Host` + `Username` to match the shared library pattern).                                                                             |
| 8   | **KYCEventPublisher swallows exceptions silently**                               | KYCService          | Add Circuit Breaker + DLQ. At minimum, log at Error level and increment a metric counter for alerting.                                                                              |
| 9   | **Queue naming inconsistency** in NotificationService                            | NotificationService | Rename `notification-service.error-critical` → `notificationservice.error.critical` to match the pattern. ⚠️ Must delete old queue first.                                           |

### 🟢 P2 — Improve

| #   | Issue                                                                                       | Service             | Fix                                                                                                                                      |
| --- | ------------------------------------------------------------------------------------------- | ------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| 10  | **Missing vehicle lifecycle events** (`vehicle.sold`, `vehicle.updated`, `vehicle.deleted`) | VehiclesSaleService | Add publishers for remaining vehicle lifecycle events.                                                                                   |
| 11  | **Missing subscription lifecycle events**                                                   | BillingService      | Create `SubscriptionActivatedEvent`, `SubscriptionCancelledEvent` contracts and publish them.                                            |
| 12  | **Dual publisher pattern in AuthService**                                                   | AuthService         | Consolidate to single generic `RabbitMqEventPublisher` approach. Remove `UserRegisteredEventPublisher` if the generic one is being used. |
| 13  | **AdminService uses HTTP for notifications**                                                | AdminService        | Same as ContactService — consider migrating to event-based notifications for decoupling.                                                 |

---

## 9. Recommended Standardized Pattern

For consistency across all services, every publisher should follow this pattern:

```
Exchange: cardealer.events (Topic, durable)
Routing Key: @event.EventType (e.g., "billing.payment.completed")
DLX: cardealer.events.dlx (Direct, durable)
Queue naming: <servicename>.<domain>.<event> (e.g., notificationservice.billing.payment_completed)
DLQ naming: <queue>.dlq (e.g., notificationservice.billing.payment_completed.dlq)
Publisher: Use Circuit Breaker + DLQ fallback
Consumer: Use max retry count + DLQ on exhaustion
```

---

_End of Audit_
