# CarDealer.Contracts

**Shared Event Contracts Library** for CarDealer Microservices Architecture

## 📋 Overview

`CarDealer.Contracts` is a **shared library** that contains **ONLY event DTOs** (Data Transfer Objects) used for **event-driven communication** between microservices. This library eliminates circular dependencies by providing a single source of truth for all events.

## 🎯 Purpose

- **Eliminate Circular Dependencies**: No service needs to reference another service's `.Shared` project
- **Event-Driven Architecture**: All inter-service communication happens via RabbitMQ events
- **Type Safety**: Strongly-typed event contracts across all microservices
- **Versioning**: Single library to manage event schema versions

## 📦 What's Included

### Base Abstractions
- `IEvent` - Base interface for all events (EventId, OccurredAt, EventType)
- `EventBase` - Abstract base class with auto-generation logic

### Events by Service

#### 🔐 Auth Events (5)
| Event Type | Class | Description |
|------------|-------|-------------|
| `auth.user.registered` | `UserRegisteredEvent` | New user registration |
| `auth.user.loggedin` | `UserLoggedInEvent` | User login |
| `auth.user.loggedout` | `UserLoggedOutEvent` | User logout |
| `auth.password.changed` | `PasswordChangedEvent` | Password change |
| `auth.user.deleted` | `UserDeletedEvent` | User deletion |

#### 🚨 Error Events (4)
| Event Type | Class | Description |
|------------|-------|-------------|
| `error.critical` | `ErrorCriticalEvent` | **Critical errors (triggers Teams alerts)** ⭐ |
| `error.logged` | `ErrorLoggedEvent` | General error logging |
| `error.spike.detected` | `ErrorSpikeDetectedEvent` | Error spike detection |
| `error.service.down` | `ServiceDownDetectedEvent` | Service health monitoring |

#### 🚗 Vehicle Events (4)
| Event Type | Class | Description |
|------------|-------|-------------|
| `vehicle.created` | `VehicleCreatedEvent` | New vehicle added to inventory |
| `vehicle.updated` | `VehicleUpdatedEvent` | Vehicle information updated |
| `vehicle.deleted` | `VehicleDeletedEvent` | Vehicle removed from inventory |
| `vehicle.sold` | `VehicleSoldEvent` | Vehicle sold to customer |

#### 📁 Media Events (4)
| Event Type | Class | Description |
|------------|-------|-------------|
| `media.uploaded` | `MediaUploadedEvent` | Media file uploaded |
| `media.processed` | `MediaProcessedEvent` | Media processing completed |
| `media.deleted` | `MediaDeletedEvent` | Media file deleted |
| `media.processing.failed` | `MediaProcessingFailedEvent` | Media processing failed |

#### 🔔 Notification Events (3)
| Event Type | Class | Description |
|------------|-------|-------------|
| `notification.sent` | `NotificationSentEvent` | Notification sent successfully |
| `notification.failed` | `NotificationFailedEvent` | Notification delivery failed |
| `notification.teams.alert.sent` | `TeamsAlertSentEvent` | **Teams alert sent** (for critical errors) ⭐ |

#### 📊 Audit Events (2)
| Event Type | Class | Description |
|------------|-------|-------------|
| `audit.log.created` | `AuditLogCreatedEvent` | Audit log entry created |
| `audit.compliance.recorded` | `ComplianceEventRecordedEvent` | Compliance event recorded |

### Common DTOs
- `PaginationDto` - Standard pagination
- `ApiResponse<T>` - Standard API response wrapper
- `ErrorDetailsDto` - Standard error details

### Enumerations
- `ServiceNames` - All microservice names

## 🚀 Usage

### 1. Add Package Reference
```bash
dotnet add package CarDealer.Contracts
```

### 2. Publish an Event
```csharp
using CarDealer.Contracts.Events.Error;
using RabbitMQ.Client;

var errorEvent = new ErrorCriticalEvent
{
    ErrorId = Guid.NewGuid(),
    ServiceName = "VehicleService",
    ExceptionType = "NullReferenceException",
    Message = "Critical error in vehicle creation",
    StatusCode = 500,
    Endpoint = "/api/vehicles"
};

// Publish to RabbitMQ
await _eventPublisher.PublishAsync(errorEvent);
```

### 3. Subscribe to an Event
```csharp
using CarDealer.Contracts.Events.Error;

public class TeamsAlertConsumer : IEventConsumer<ErrorCriticalEvent>
{
    public async Task HandleAsync(ErrorCriticalEvent @event)
    {
        // Send Teams alert when critical error occurs
        await _teamsService.SendAlertAsync(@event);
    }
}
```

## 📐 Architecture Pattern

```
┌─────────────────────────────────────────────────────────┐
│                  CarDealer.Contracts                    │
│                  (Shared Library)                       │
│                                                         │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Events    │  │     DTOs     │  │    Enums     │  │
│  │   (22)      │  │     (3)      │  │     (1)      │  │
│  └─────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
         ▲              ▲              ▲              ▲
         │              │              │              │
         │              │              │              │
    ┌────┴────┐    ┌───┴────┐    ┌───┴────┐    ┌───┴────┐
    │  Error  │    │  Auth  │    │Vehicle │    │ Media  │
    │ Service │    │Service │    │Service │    │Service │
    └─────────┘    └────────┘    └────────┘    └────────┘
         │              │              │              │
         └──────────────┴──────────────┴──────────────┘
                           │
                      RabbitMQ
                           │
                 ┌─────────┴─────────┐
                 │                   │
          ┌──────┴──────┐    ┌──────┴──────┐
          │Notification │    │   Audit     │
          │  Service    │    │  Service    │
          └─────────────┘    └─────────────┘
```

## ⭐ Critical Flow: Teams Alerts

**Scenario**: ErrorService detects HTTP 500 error → NotificationService sends Teams alert

1. **ErrorService** publishes `ErrorCriticalEvent`:
   ```csharp
   var criticalError = new ErrorCriticalEvent
   {
       ServiceName = "VehicleService",
       StatusCode = 500,
       Message = "Database connection failed"
   };
   await _publisher.PublishAsync(criticalError);
   ```

2. **RabbitMQ** routes event to `error.critical` queue

3. **NotificationService** consumes `ErrorCriticalEvent`:
   ```csharp
   public async Task HandleAsync(ErrorCriticalEvent @event)
   {
       await _teamsService.SendCriticalAlertAsync(@event);
       
       // Publish confirmation
       await _publisher.PublishAsync(new TeamsAlertSentEvent
       {
           ServiceName = @event.ServiceName,
           AlertType = "Critical"
       });
   }
   ```

## 🔒 Dependencies

- **.NET 8.0**
- **NO other project references** (this is critical!)
- **NO NuGet packages** (pure POCOs)

## 📦 NuGet Configuration

```xml
<PropertyGroup>
  <PackageId>CarDealer.Contracts</PackageId>
  <Version>1.0.0</Version>
  <Authors>CarDealer Team</Authors>
  <Description>Shared event contracts for CarDealer microservices</Description>
</PropertyGroup>
```

## ✅ Phase 1 Checklist

- [x] Project created (.NET 8.0)
- [x] Added to CarDealer.sln
- [x] Directory structure created
- [x] IEvent interface
- [x] EventBase abstract class
- [x] Auth events (5/5)
- [x] Error events (4/4) **including ErrorCriticalEvent for Teams alerts**
- [x] Vehicle events (4/4)
- [x] Media events (4/4)
- [x] Notification events (3/3)
- [x] Audit events (2/2)
- [x] Common DTOs (3/3)
- [x] Enums (1/1)
- [x] Project compiles successfully
- [ ] NuGet package configuration
- [ ] Event serialization tests
- [ ] Documentation complete

## 📚 Related Documentation

- [PLAN_REFACTORIZACION_MICROSERVICIOS.md](../../PLAN_REFACTORIZACION_MICROSERVICIOS.md) - Full refactoring plan
- [ARQUITECTURA_MICROSERVICIOS.md](../../ARQUITECTURA_MICROSERVICIOS.md) - Architecture analysis
- [TEST_PLAN.md](../../backend/IntegrationTests/TEST_PLAN.md) - Testing strategy

---

**Total Events**: 22  
**Total DTOs**: 3  
**Total Enums**: 1  
**Zero Dependencies**: ✅  
**Zero Circular References**: ✅
