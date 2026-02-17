# 🗑️ Microservicios Descartados

> **Última actualización:** Enero 23, 2026  
> **Razón principal:** Corrección del modelo de negocio de OKLA

---

## ⚠️ IMPORTANTE

Los microservicios en esta carpeta fueron **planificados incorrectamente** asumiendo que OKLA sería un **marketplace transaccional** donde la plataforma procesaría pagos de vehículos.

### Modelo Correcto de OKLA

```
┌────────────────────────────────────────────────────────────────────────┐
│              OKLA ES PLATAFORMA DE PUBLICIDAD                          │
├────────────────────────────────────────────────────────────────────────┤
│                                                                        │
│   ✅ Dealers PAGAN a OKLA: Suscripción mensual RD$2,900-14,900        │
│   ✅ Sellers PAGAN a OKLA: Publicación única RD$1,500                  │
│                                                                        │
│   ❌ OKLA NO procesa pagos de vehículos                                │
│   ❌ OKLA NO retiene dinero de compradores                             │
│   ❌ OKLA NO transfiere dinero a vendedores                            │
│   ❌ OKLA NO cobra comisión por ventas                                 │
│                                                                        │
│   LA TRANSACCIÓN DEL VEHÍCULO OCURRE DIRECTAMENTE:                     │
│   Comprador ───[Paga en efectivo/banco]───> Vendedor                  │
│                                                                        │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 📋 Microservicios Descartados

### 🔐 EscrowService

| Campo                  | Valor                              |
| ---------------------- | ---------------------------------- |
| **Puerto Planificado** | 5047                               |
| **Base de Datos**      | escrow_db                          |
| **Fecha de Descarte**  | Enero 23, 2026                     |
| **Razón**              | OKLA no procesa pagos de vehículos |

**Descripción Original:**  
Sistema de pagos en garantía (escrow) para transacciones de alto valor entre compradores y vendedores.

**Por qué NO aplica:**

- OKLA no recibe dinero de compradores
- OKLA no transfiere dinero a vendedores
- Las transacciones de vehículos son externas a la plataforma

**Estructura del Servicio:**

```
EscrowService/
├── EscrowService.sln
├── Dockerfile
├── EscrowService.Api/
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings.json
├── EscrowService.Application/
│   ├── Commands/
│   ├── DTOs/
│   ├── Handlers/
│   └── Validators/
├── EscrowService.Domain/
│   ├── Entities/
│   └── Interfaces/
├── EscrowService.Infrastructure/
│   └── Persistence/
└── EscrowService.Tests/
```

---

## ⚠️ NO USAR ESTOS SERVICIOS

Estos microservicios:

1. ❌ NO deben ser desplegados
2. ❌ NO deben ser referenciados en el código activo
3. ❌ NO deben ser incluidos en docker-compose
4. ❌ NO deben ser incluidos en CI/CD

Se mantienen archivados para:

1. ✅ Referencia histórica
2. ✅ Evitar que se vuelvan a proponer
3. ✅ Posible reutilización de código para otros propósitos

---

## 📚 Documentación Relacionada

- [Documentación descartada](../docs/_DESCARTADOS/README.md)
- [Modelo de negocio correcto](../docs/process-matrix/02-USUARIOS-DEALERS/04-dealer-onboarding.md)
- [Integración de cobros Azul](../docs/process-matrix/05-PAGOS-FACTURACION/03-azul-payment.md)

---

## 📁 FASE1_ELIMINADOS — Auditoría Febrero 2026 (17 servicios)

Servicios eliminados por ser: infraestructura redundante con Kubernetes, scaffolds vacíos, prematuros, o duplicados.

| Servicio | Razón de eliminación | Reemplazo |
|----------|---------------------|-----------|
| **ServiceDiscovery** | Redundante — K8s DNS provee service discovery nativo | K8s DNS / CoreDNS |
| **HealthCheckService** | Redundante — K8s liveness/readiness probes | K8s probes nativos |
| **PostgresDbService** | Redundante — PostgreSQL es infraestructura, no microservicio | StatefulSet |
| **LoggingService** | Redundante — Seq ya maneja logs centralizados | Seq |
| **TracingService** | Redundante — Jaeger ya maneja distributed tracing | Jaeger |
| **BackupDRService** | Redundante — Digital Ocean managed backups | DO Managed Backups |
| **FeatureToggleService** | Scaffold vacío — absorbible en ConfigurationService | ConfigurationService |
| **FeatureStoreService** | Prematuro — requiere pipeline de ML que no existe | Futuro |
| **DataPipelineService** | Prematuro — requiere infraestructura ETL | Futuro |
| **UserBehaviorService** | Prematuro — requiere tracking de eventos maduro | EventTrackingService |
| **SearchService** | Scaffold vacío — Elasticsearch no implementado | VehiclesSaleService |
| **InvoicingService** | Duplicado — BillingService ya maneja facturación | BillingService |
| **FinanceService** | No aplica — OKLA es marketplace de anuncios | N/A |
| **BankReconciliationService** | No aplica — no hay pagos custodiales | N/A |
| **ECommerceComplianceService** | Scaffold vacío — sin implementación | ComplianceService |
| **AntiMoneyLaunderingService** | Prematuro — AML requiere bureaus de crédito | KYCService |
| **DigitalSignatureService** | Prematuro — requiere autoridades certificadoras RD | Futuro |

---

## 📁 FASE4_COMPLIANCE_CONSOLIDADOS — Auditoría Febrero 2026 (7 servicios)

Cluster de compliance/legal consolidado para reducir fragmentación excesiva (13→4 servicios).

### Absorbidos en ComplianceService

| Servicio | Funcionalidad | Ahora en |
|----------|---------------|----------|
| **ComplianceReportingService** | Reportes DGII, UAF, schedules | ComplianceService |
| **ComplianceIntegrationService** | Integraciones externas de compliance | ComplianceService |
| **RegulatoryAlertService** | Alertas regulatorias | ComplianceService |

### Para fusionar en LegalService (pendiente crear)

| Servicio | Funcionalidad | Ahora en |
|----------|---------------|----------|
| **LegalDocumentService** | Documentos legales | LegalService (pendiente) |
| **ContractService** | Gestión de contratos | LegalService (pendiente) |
| **ConsumerProtectionService** | Pro-Consumidor RD | LegalService (pendiente) |
| **DisputeService** | Resolución de disputas | LegalService (pendiente) |

---

## 🔄 Controllers Reubicados (Fase 2 y 3)

Controllers duplicados removidos de servicios activos (archivados en `{Servicio}/_REMOVED_CONTROLLERS/`):

| Servicio Origen | Controller Removido | Owner correcto |
|----------------|---------------------|----------------|
| UserService | DealersController, DealerOnboarding(V2), DealerModules | DealerManagementService |
| UserService | DealerEmployeesController | StaffService |
| AdminService | PlatformEmployeesController | StaffService |
| PaymentService | SubscriptionsController | BillingService |
| DealerManagementService | SubscriptionsController | BillingService |
| ChatbotService | LeadsController | CRMService |
| ChatbotService | MaintenanceController | MaintenanceService |
| IdempotencyService | OrdersController (misplaced) | N/A |
| VehiclesSaleService | ImportController | InventoryManagementService |
