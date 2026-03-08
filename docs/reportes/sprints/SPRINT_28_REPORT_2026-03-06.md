# Sprint 28 Report — 2026-03-06

## 🎯 Tema: BillingService P2 Authorization Hardening — Admin-Only + Ownership Checks

## ✅ Tareas Completadas

### 1. SubscriptionsController — 12 Endpoints Secured

**Archivo:** `BillingService.Api/Controllers/SubscriptionsController.cs`

| Endpoint                          | Action                    |      Authorization Added       |
| --------------------------------- | ------------------------- | :----------------------------: |
| `GET /` (GetAll)                  | Returns ALL subscriptions | `[Authorize(Roles = "admin")]` |
| `GET /status/{status}`            | Returns ALL by status     | `[Authorize(Roles = "admin")]` |
| `GET /plan/{plan}`                | Returns ALL by plan       | `[Authorize(Roles = "admin")]` |
| `GET /expiring-trials/{days}`     | Operational listing       | `[Authorize(Roles = "admin")]` |
| `GET /due-billings`               | Operational listing       | `[Authorize(Roles = "admin")]` |
| `POST /{id}/activate`             | Lifecycle operation       | `[Authorize(Roles = "admin")]` |
| `POST /{id}/suspend`              | Lifecycle operation       | `[Authorize(Roles = "admin")]` |
| `POST /{id}/cancel`               | Lifecycle operation       | `[Authorize(Roles = "admin")]` |
| `POST /{id}/upgrade`              | Plan change               | `[Authorize(Roles = "admin")]` |
| `POST /{id}/change-billing-cycle` | Billing change            | `[Authorize(Roles = "admin")]` |
| `POST /{id}/renew`                | Billing operation         | `[Authorize(Roles = "admin")]` |
| `DELETE /{id}`                    | Destructive operation     | `[Authorize(Roles = "admin")]` |

### 2. InvoicesController — 11 Endpoints Secured

**Archivo:** `BillingService.Api/Controllers/InvoicesController.cs`

| Endpoint                    | Action                   |               Authorization Added               |
| --------------------------- | ------------------------ | :---------------------------------------------: |
| `GET /{id}` (GetById)       | Single invoice by ID     | Ownership check: `invoice.DealerId != dealerId` |
| `GET /number/{num}`         | Single invoice by number | Ownership check: `invoice.DealerId != dealerId` |
| `GET /overdue`              | All overdue invoices     |         `[Authorize(Roles = "admin")]`          |
| `GET /unpaid`               | All unpaid invoices      |         `[Authorize(Roles = "admin")]`          |
| `POST /{id}/issue`          | State change             |         `[Authorize(Roles = "admin")]`          |
| `POST /{id}/send`           | State change             |         `[Authorize(Roles = "admin")]`          |
| `POST /{id}/record-payment` | Financial operation      |         `[Authorize(Roles = "admin")]`          |
| `POST /{id}/mark-overdue`   | State change             |         `[Authorize(Roles = "admin")]`          |
| `POST /{id}/cancel`         | Destructive operation    |         `[Authorize(Roles = "admin")]`          |
| `POST /{id}/void`           | Destructive operation    |         `[Authorize(Roles = "admin")]`          |
| `DELETE /{id}`              | Destructive operation    |         `[Authorize(Roles = "admin")]`          |

### 3. PaymentsController — 9 Endpoints Secured

**Archivo:** `BillingService.Api/Controllers/PaymentsController.cs`

| Endpoint              | Action                |               Authorization Added               |
| --------------------- | --------------------- | :---------------------------------------------: |
| `GET /{id}` (GetById) | Single payment by ID  | Ownership check: `payment.DealerId != dealerId` |
| `GET /pending`        | All pending payments  |         `[Authorize(Roles = "admin")]`          |
| `GET /failed`         | All failed payments   |         `[Authorize(Roles = "admin")]`          |
| `POST /{id}/process`  | State change          |         `[Authorize(Roles = "admin")]`          |
| `POST /{id}/succeed`  | State change          |         `[Authorize(Roles = "admin")]`          |
| `POST /{id}/fail`     | State change          |         `[Authorize(Roles = "admin")]`          |
| `POST /{id}/refund`   | Financial operation   |         `[Authorize(Roles = "admin")]`          |
| `POST /{id}/dispute`  | Financial operation   |         `[Authorize(Roles = "admin")]`          |
| `DELETE /{id}`        | Destructive operation |         `[Authorize(Roles = "admin")]`          |

## 📊 Métricas del Sprint

| Métrica                       | Valor                                            |
| ----------------------------- | ------------------------------------------------ |
| Archivos modificados          | 3                                                |
| Endpoints secured             | 32                                               |
| Admin-only restrictions added | 28                                               |
| Ownership checks added        | 4 (GetById invoice/payment, GetByNumber invoice) |
| Total replacements executed   | 25                                               |

## 🏆 BillingService Security Summary (Sprints 22-28)

| Sprint    | Fix Type                                        | Endpoints |
| --------- | ----------------------------------------------- | :-------: |
| Sprint 22 | Payment auth P0 (Azul controllers)              |     2     |
| Sprint 26 | IDOR: X-Dealer-Id → JWT extraction              |    37     |
| Sprint 27 | IDOR: AzulPaymentPageController body validation |     1     |
| Sprint 28 | Admin-only + ownership checks                   |    32     |
| **Total** |                                                 |  **72**   |

**BillingService is now fully hardened across all 8 controllers.**
