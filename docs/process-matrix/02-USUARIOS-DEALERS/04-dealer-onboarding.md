# 🤝 Dealer Onboarding - Matriz de Procesos

> **Servicio:** UserService / DealerOnboardingV2Controller  
> **Puerto:** 5004  
> **Última actualización:** Enero 23, 2026  
> **Estado:** 🟢 ACTIVO  
> **Estado de Implementación:** ✅ 98% Completo

---

## 📊 Resumen de Implementación

| Componente                | Total | Implementado | Pendiente | Estado  |
| ------------------------- | ----- | ------------ | --------- | ------- |
| **Controllers**           | 1     | 1            | 0         | ✅ 100% |
| **Procesos (ONBOARD-\*)** | 6     | 6            | 0         | ✅ 100% |
| **Pasos de Onboarding**   | 8     | 8            | 0         | ✅ 100% |
| **Tests Unitarios**       | 12    | 12           | 0         | ✅ 100% |
| **Entidades**             | 3     | 3            | 0         | ✅ 100% |
| **Integración Azul**      | 1     | 1            | 0         | ✅ 100% |
| **RabbitMQ Events**       | 7     | 7            | 0         | ✅ 100% |
| **NotificationService**   | 5     | 5            | 0         | ✅ 100% |

### Archivos Implementados

| Archivo                           | Ubicación               | Estado         |
| --------------------------------- | ----------------------- | -------------- |
| `DealerOnboarding.cs`             | Domain/Entities         | ✅ Nuevo       |
| `DealerOnboardingStatus`          | Domain/Entities         | ✅ Nuevo       |
| `DealerOnboardingEvents.cs`       | Domain/Events           | ✅ Nuevo       |
| `DealerOnboardingV2Controller.cs` | Api/Controllers         | ✅ Nuevo       |
| `DealerOnboardingTests.cs`        | Tests/Domain/Entities   | ✅ Nuevo       |
| `NotificationServiceClient.cs`    | Infrastructure/External | ✅ Actualizado |
| `INotificationServiceClient.cs`   | Application/Interfaces  | ✅ Actualizado |

### Pendientes de Integración

- 🟡 Integración real con API de Azul (Banco Popular) - Mock implementado
- 🟡 Migración de base de datos (EF Core) - Pendiente ejecutar

### Leyenda de Estados

- ✅ **IMPLEMENTADO Y PROBADO**: Código completo con tests
- 🟢 **IMPLEMENTADO**: Código completo, falta testing
- 🟡 **EN PROGRESO**: Implementación parcial
- 🔴 **PENDIENTE**: No implementado

---

## 1. Información General

### 1.1 Descripción

Sistema de registro y onboarding de dealers (concesionarios) en la plataforma OKLA. Gestiona todo el proceso desde la solicitud inicial hasta la activación de la cuenta, incluyendo verificación de documentos, **cobro de suscripción mensual vía Azul (Banco Popular)** y asignación de plan.

> ⚠️ **IMPORTANTE: Modelo de Negocio**
>
> Los dealers/vendedores **PAGAN A OKLA** por publicar anuncios de vehículos en la plataforma.
> La venta de vehículos (transacción entre comprador y vendedor) es **EXTERNA a la plataforma**.
>
> ```
> ┌─────────────────────────────────────────────────────────────────────┐
> │                    MODELO DE NEGOCIO OKLA                          │
> ├─────────────────────────────────────────────────────────────────────┤
> │                                                                     │
> │   DEALER/VENDEDOR ───[PAGA SUSCRIPCIÓN]───> OKLA                   │
> │        │                                      │                     │
> │        │ RD$2,900-14,900/mes                 │ Procesa pagos       │
> │        │ (según plan)                        │ vía AzulPaymentSvc  │
> │        │                                      │                     │
> │   ═══════════════════════════════════════════════════════════════  │
> │                                                                     │
> │   COMPRADOR ─────[PAGA VEHÍCULO]─────> VENDEDOR                   │
> │        │                                    │                       │
> │        │ Transacción EXTERNA               │ Transferencia/        │
> │        │ NO pasa por OKLA                  │ Efectivo/Cheque       │
> │                                                                     │
> └─────────────────────────────────────────────────────────────────────┘
> ```

### 1.2 Dependencias

| Servicio            | Propósito                                    |
| ------------------- | -------------------------------------------- |
| AzulPaymentService  | **Cobrar suscripción mensual a los dealers** |
| BillingService      | Gestión de suscripciones y facturas          |
| KYCService          | Verificación de identidad y documentos       |
| MediaService        | Almacenamiento de documentos                 |
| NotificationService | Emails de onboarding                         |
| AuthService         | Creación de usuario                          |

### 1.3 Flujo General

```
┌──────────────────────────────────────────────────────────────────────────┐
│                    FLUJO DE ONBOARDING DE DEALER                          │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│   1. SOLICITUD                2. VERIFICACIÓN              3. ACTIVACIÓN  │
│   ────────────                ────────────────             ────────────── │
│   ┌─────────────┐             ┌─────────────┐              ┌──────────┐  │
│   │  Registro   │ ──────────> │   KYC +     │ ──────────>  │   Azul   │  │
│   │  Inicial    │             │ Documentos  │              │ Customer │  │
│   └─────────────┘             └─────────────┘              └──────────┘  │
│         │                           │                            │        │
│         ▼                           ▼                            ▼        │
│   ┌─────────────┐             ┌─────────────┐              ┌──────────┐  │
│   │   Email     │             │   Admin     │              │  Plan    │  │
│   │ Verificar   │             │  Revisión   │              │ Activo   │  │
│   └─────────────┘             └─────────────┘              └──────────┘  │
│                                                                           │
│   Status: Pending ────> UnderReview ────> Approved ────> Active          │
│                                                                           │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints API

### 2.1 DealerOnboardingV2Controller ✅ IMPLEMENTADO

| Método | Endpoint                                                | Descripción                 | Auth | Roles        | Estado |
| ------ | ------------------------------------------------------- | --------------------------- | ---- | ------------ | ------ |
| `POST` | `/api/dealer-onboarding/register`                       | Registrar nuevo dealer      | ❌   | Public       | ✅     |
| `POST` | `/api/dealer-onboarding/verify-email`                   | Verificar email con token   | ❌   | Public       | ✅     |
| `POST` | `/api/dealer-onboarding/{dealerId}/resend-verification` | Reenviar email verificación | ❌   | Public       | ✅     |
| `GET`  | `/api/dealer-onboarding/{dealerId}/status`              | Estado del onboarding       | ✅   | Owner        | ✅     |
| `PUT`  | `/api/dealer-onboarding/{dealerId}/documents`           | Actualizar IDs documentos   | ✅   | Owner        | ✅     |
| `POST` | `/api/dealer-onboarding/{dealerId}/approve`             | Aprobar dealer              | ✅   | Admin        | ✅     |
| `POST` | `/api/dealer-onboarding/{dealerId}/reject`              | Rechazar solicitud          | ✅   | Admin        | ✅     |
| `PUT`  | `/api/dealer-onboarding/{dealerId}/subscription`        | Guardar datos suscripción   | ✅   | Owner        | ✅     |
| `POST` | `/api/dealer-onboarding/{dealerId}/activate`            | Activar dealer              | ✅   | Admin/System | ✅     |
| `GET`  | `/api/dealer-onboarding/pending`                        | Listar pendientes (admin)   | ✅   | Admin        | ✅     |

---

## 3. Entidades y Enums

### 3.1 DealerOnboardingStatus (Enum)

```csharp
public enum DealerOnboardingStatus
{
    Pending = 0,          // Registro inicial pendiente
    EmailVerified = 1,    // Email verificado
    DocumentsSubmitted = 2, // Documentos subidos
    UnderReview = 3,      // En revisión por admin
    Approved = 4,         // Aprobado, pendiente pago
    PaymentSetup = 5,     // Azul configurado (suscripción activa)
    Active = 6,           // Completamente activo
    Rejected = 7,         // Rechazado
    Suspended = 8         // Suspendido
}
```

### 3.2 DealerType (Enum)

```csharp
public enum DealerType
{
    Independent = 0,      // Dealer independiente
    Chain = 1,            // Cadena de concesionarios
    MultipleStore = 2,    // Múltiples sucursales
    Franchise = 3         // Franquicia
}
```

### 3.3 DealerOnboarding (Entidad)

```csharp
public class DealerOnboarding
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DealerOnboardingStatus Status { get; set; }

    // Información del negocio
    public string BusinessName { get; set; }
    public string BusinessLegalName { get; set; }
    public string RNC { get; set; }                // Registro Nacional Contribuyente
    public DealerType Type { get; set; }
    public string? Description { get; set; }

    // Contacto
    public string Email { get; set; }
    public string Phone { get; set; }
    public string? MobilePhone { get; set; }
    public string? Website { get; set; }

    // Ubicación
    public string Address { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Representante Legal
    public string LegalRepName { get; set; }
    public string LegalRepCedula { get; set; }
    public string LegalRepPosition { get; set; }

    // Suscripción (Pagos A OKLA)
    // El dealer PAGA a OKLA, NO recibe pagos
    public string? AzulCustomerId { get; set; }      // ID del dealer como CLIENTE en Azul
    public string? AzulSubscriptionId { get; set; }  // ID de la suscripción recurrente

    // Plan
    public DealerPlan RequestedPlan { get; set; }
    public bool IsEarlyBirdEligible { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? DocumentsSubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? ApprovedBy { get; set; }
}
```

### 3.4 DealerPlan (Enum)

```csharp
public enum DealerPlan
{
    None = 0,
    Starter = 1,        // RD$2,900/mes - 10 vehículos
    Professional = 2,   // RD$5,900/mes - 50 vehículos
    Enterprise = 3      // RD$14,900/mes - Ilimitado
}
```

---

## 4. Procesos Detallados

### 4.1 ONBOARD-001: Registro Inicial de Dealer

| Campo       | Valor                                |
| ----------- | ------------------------------------ |
| **ID**      | ONBOARD-001                          |
| **Nombre**  | Registro Inicial de Dealer           |
| **Actor**   | Nuevo Dealer                         |
| **Trigger** | POST /api/dealer-onboarding/register |

#### Flujo del Proceso

| Paso | Acción                       | Sistema             | Validación             |
| ---- | ---------------------------- | ------------------- | ---------------------- |
| 1    | Dealer accede a landing      | Frontend            | /dealer                |
| 2    | Click "Registrarme"          | Frontend            | Formulario             |
| 3    | Ingresar email y password    | Frontend            | Email válido           |
| 4    | Ingresar datos del negocio   | Frontend            | RNC formato            |
| 5    | Ingresar ubicación           | Frontend            | Provincia RD           |
| 6    | Ingresar representante legal | Frontend            | Cédula válida          |
| 7    | Seleccionar plan             | Frontend            | Starter/Pro/Enterprise |
| 8    | Aceptar términos             | Frontend            | Checkbox               |
| 9    | Submit registro              | API                 | POST /register         |
| 10   | Validar RNC único            | UserService         | No existe              |
| 11   | Validar email único          | AuthService         | No existe              |
| 12   | Crear usuario                | AuthService         | Role = Dealer          |
| 13   | Crear DealerOnboarding       | Database            | Status = Pending       |
| 14   | Enviar email verificación    | NotificationService | Token 24h              |
| 15   | Publicar evento              | RabbitMQ            | dealer.registered      |

#### Request

```json
{
  "email": "contacto@autosdelcaribe.com.do",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!",
  "businessName": "Autos del Caribe",
  "businessLegalName": "Autos del Caribe SRL",
  "rnc": "131456789",
  "type": "Independent",
  "description": "Concesionario especializado en vehículos importados",
  "phone": "809-555-0100",
  "mobilePhone": "829-555-0100",
  "website": "https://autosdelcaribe.com.do",
  "address": "Av. Winston Churchill #75",
  "city": "Santo Domingo",
  "province": "Distrito Nacional",
  "postalCode": "10101",
  "legalRepName": "Juan Carlos Rodríguez",
  "legalRepCedula": "001-1234567-8",
  "legalRepPosition": "Gerente General",
  "requestedPlan": "Pro",
  "acceptedTerms": true
}
```

#### Response

```json
{
  "success": true,
  "dealerId": "uuid",
  "userId": "uuid",
  "status": "Pending",
  "message": "Registro exitoso. Por favor verifica tu email.",
  "nextStep": "Verificar email",
  "isEarlyBirdEligible": true
}
```

---

### 4.2 ONBOARD-002: Verificación de Email

| Campo       | Valor                 |
| ----------- | --------------------- |
| **ID**      | ONBOARD-002           |
| **Nombre**  | Verificación de Email |
| **Actor**   | Dealer                |
| **Trigger** | Click link en email   |

#### Flujo del Proceso

| Paso | Acción                  | Sistema     | Validación             |
| ---- | ----------------------- | ----------- | ---------------------- |
| 1    | Dealer recibe email     | Inbox       | Con link verificación  |
| 2    | Click en link           | Frontend    | Token en URL           |
| 3    | Validar token           | AuthService | No expirado (24h)      |
| 4    | Marcar email verificado | AuthService | EmailVerified = true   |
| 5    | Actualizar onboarding   | UserService | Status = EmailVerified |
| 6    | Redirect a documentos   | Frontend    | /dealer/documents      |
| 7    | Publicar evento         | RabbitMQ    | dealer.email_verified  |

---

### 4.3 ONBOARD-003: Subida de Documentos

| Campo       | Valor                    |
| ----------- | ------------------------ |
| **ID**      | ONBOARD-003              |
| **Nombre**  | Subida de Documentos KYC |
| **Actor**   | Dealer                   |
| **Trigger** | Upload en dashboard      |

#### Documentos Requeridos

| Documento                | Descripción                     | Formato | Obligatorio |
| ------------------------ | ------------------------------- | ------- | ----------- |
| RNC                      | Registro Nacional Contribuyente | PDF     | ✅          |
| Licencia Comercial       | Expedida por DGII               | PDF     | ✅          |
| Cédula Representante     | Cédula del rep. legal           | PDF/JPG | ✅          |
| Contrato Social          | Para SRL/SA                     | PDF     | Condicional |
| Poder Legal              | Si aplica                       | PDF     | ❌          |
| Comprobante de Dirección | Factura servicios               | PDF     | ✅          |

#### Flujo del Proceso

| Paso | Acción                       | Sistema             | Validación                 |
| ---- | ---------------------------- | ------------------- | -------------------------- |
| 1    | Dealer accede a documentos   | Dashboard           | Status = EmailVerified     |
| 2    | Upload documento RNC         | MediaService        | PDF < 5MB                  |
| 3    | Upload licencia comercial    | MediaService        | PDF < 5MB                  |
| 4    | Upload cédula representante  | MediaService        | PDF/JPG < 5MB              |
| 5    | Upload comprobante dirección | MediaService        | PDF < 5MB                  |
| 6    | Validar todos obligatorios   | UserService         | Completos                  |
| 7    | Crear registros KYC          | KYCService          | Por cada documento         |
| 8    | Actualizar status            | Database            | DocumentsSubmitted         |
| 9    | Notificar a admins           | NotificationService | Nueva solicitud            |
| 10   | Publicar evento              | RabbitMQ            | dealer.documents_submitted |

---

### 4.4 ONBOARD-004: Revisión por Admin

| Campo       | Valor                           |
| ----------- | ------------------------------- |
| **ID**      | ONBOARD-004                     |
| **Nombre**  | Revisión de Solicitud           |
| **Actor**   | Admin                           |
| **Trigger** | Notificación de nueva solicitud |

#### Flujo del Proceso

| Paso | Acción                     | Sistema         | Validación           |
| ---- | -------------------------- | --------------- | -------------------- |
| 1    | Admin accede a panel       | Admin Dashboard | Rol Admin            |
| 2    | Ver solicitudes pendientes | AdminService    | Lista con filtros    |
| 3    | Seleccionar dealer         | AdminService    | Ver detalle          |
| 4    | Revisar información        | AdminService    | Datos de negocio     |
| 5    | Revisar documentos         | KYCService      | Ver cada documento   |
| 6    | Validar RNC en DGII        | Manual/API      | Verificar existencia |
| 7    | Validar cédula en JCE      | Manual/API      | Verificar identidad  |
| 8    | Decisión: Aprobar/Rechazar | AdminService    | Con notas            |

#### Si Aprobado

| Paso | Acción                  | Sistema             | Validación        |
| ---- | ----------------------- | ------------------- | ----------------- |
| 9    | Click "Aprobar"         | AdminService        | Confirmación      |
| 10   | Actualizar status       | Database            | Approved          |
| 11   | Enviar email aprobación | NotificationService | Con instrucciones |
| 12   | Publicar evento         | RabbitMQ            | dealer.approved   |

#### Si Rechazado

| Paso | Acción               | Sistema             | Validación      |
| ---- | -------------------- | ------------------- | --------------- |
| 9    | Click "Rechazar"     | AdminService        | Requiere razón  |
| 10   | Ingresar motivo      | AdminService        | Obligatorio     |
| 11   | Actualizar status    | Database            | Rejected        |
| 12   | Enviar email rechazo | NotificationService | Con razón       |
| 13   | Publicar evento      | RabbitMQ            | dealer.rejected |

---

### 4.5 ONBOARD-005: Pago de Suscripción

| Campo       | Valor                      |
| ----------- | -------------------------- |
| **ID**      | ONBOARD-005                |
| **Nombre**  | Pago de Suscripción a OKLA |
| **Actor**   | Dealer                     |
| **Trigger** | Aprobación completada      |

> ⚠️ **NOTA IMPORTANTE:**
> El dealer **PAGA A OKLA** por el servicio de publicación.
> OKLA **RECIBE** el pago usando su cuenta comercial en Azul.
> El dealer NO necesita una cuenta de comercio en Azul.

#### Planes Disponibles

| Plan         | Precio/Mes | Precio Early Bird | Límite Vehículos | Incluye                                     |
| ------------ | ---------- | ----------------- | ---------------- | ------------------------------------------- |
| Starter      | RD$2,900   | RD$2,320 (-20%)   | 10               | Perfil básico, estadísticas básicas         |
| Professional | RD$5,900   | RD$4,720 (-20%)   | 50               | Badge verificado, importación CSV           |
| Enterprise   | RD$14,900  | RD$11,920 (-20%)  | Ilimitado        | API, múltiples sucursales, gerente dedicado |

#### Flujo del Proceso

| Paso | Acción                      | Sistema            | Validación             |
| ---- | --------------------------- | ------------------ | ---------------------- |
| 1    | Dealer accede a checkout    | Frontend           | Status = Approved      |
| 2    | Mostrar plan seleccionado   | Frontend           | Precio + features      |
| 3    | Si Early Bird               | Frontend           | Aplicar descuento 20%  |
| 4    | Ingresar datos tarjeta      | Frontend           | Validación Luhn        |
| 5    | Enviar a AzulPaymentService | API                | POST /subscriptions    |
| 6    | Crear suscripción en Azul   | AzulPaymentService | Cobro recurrente       |
| 7    | Si Early Bird               | AzulPaymentService | Trial 90 días (gratis) |
| 8    | Guardar AzulCustomerId      | Database           | ID como cliente        |
| 9    | Guardar AzulSubscriptionId  | Database           | ID de suscripción      |
| 10   | Actualizar status           | Database           | PaymentSetup           |
| 11   | Publicar evento             | RabbitMQ           | dealer.payment_setup   |

#### Request (Crear Suscripción)

```json
{
  "userId": "dealer-uuid",
  "planName": "Pro",
  "amount": 5900,
  "currency": "DOP",
  "frequency": "Monthly",
  "cardNumber": "4111111111111111",
  "cardExpiryMonth": "12",
  "cardExpiryYear": "2028",
  "cardCVV": "123",
  "cardholderName": "Juan Carlos Rodríguez",
  "customerEmail": "contacto@autosdelcaribe.com.do",
  "customerPhone": "8095550100",
  "isEarlyBird": true,
  "trialDays": 90
}
```

#### Response

```json
{
  "success": true,
  "subscriptionId": "sub_azul_abc123",
  "customerId": "cus_azul_xyz789",
  "status": "Active",
  "trialEndsAt": "2026-04-23T00:00:00Z",
  "nextBillingDate": "2026-04-23T00:00:00Z",
  "message": "Suscripción creada. Tu cuenta estará activa en breve."
}

---

### 4.6 ONBOARD-006: Activación de Cuenta

| Campo       | Valor                   |
| ----------- | ----------------------- |
| **ID**      | ONBOARD-006             |
| **Nombre**  | Activación Final        |
| **Actor**   | Sistema                 |
| **Trigger** | Suscripción confirmada  |

#### Flujo del Proceso

| Paso | Acción                       | Sistema             | Validación             |
| ---- | ---------------------------- | ------------------- | ---------------------- |
| 1    | Confirmar pago/trial activo  | AzulPaymentService  | status = Active        |
| 2    | Crear entidad Dealer         | DealerService       | Desde onboarding       |
| 3    | Asignar límites de plan      | DealerService       | MaxVehicles según plan |
| 4    | Asignar badge si Early Bird  | DealerService       | "Miembro Fundador"     |
| 5    | Actualizar status            | Database            | Active                 |
| 6    | Actualizar rol usuario       | AuthService         | DealerActive           |
| 7    | Enviar email bienvenida      | NotificationService | Con guía inicio        |
| 8    | Publicar evento              | RabbitMQ            | dealer.activated       |
| 9    | Redirect a dashboard         | Frontend            | /dealer/dashboard      |

#### Email de Bienvenida

Incluye:
- Confirmación del plan activo
- Si Early Bird: recordatorio de que tiene 90 días de trial
- Límite de vehículos según plan
- Link para publicar primer vehículo
- Guía de inicio rápido

---

## 5. Diagramas

### 5.1 Diagrama de Estados

```

┌─────────┐
│ Pending │
└────┬────┘
│ Email verificado
▼
┌──────────────────┐
│ EmailVerified │
└────────┬─────────┘
│ Documentos subidos
▼
┌───────────────────────┐
│ DocumentsSubmitted │
└──────────┬────────────┘
│ En revisión
▼
┌─────────────┐
│ UnderReview │
└──────┬──────┘
│
┌──────┴──────┐
▼ ▼
┌──────────┐ ┌──────────┐
│ Approved │ │ Rejected │
└────┬─────┘ └──────────┘
│ Pago configurado
▼
┌──────────────┐
│ PaymentSetup │
└──────┬───────┘
│ Activación
▼
┌─────────┐
│ Active │
└─────────┘

```

---

## 6. Reglas de Negocio

### 6.1 Validaciones

| Campo     | Regla                                     |
| --------- | ----------------------------------------- |
| RNC       | 9 u 11 dígitos, único en sistema          |
| Cédula    | Formato 000-0000000-0, válida en JCE      |
| Email     | Único, debe ser corporativo (recomendado) |
| Teléfono  | Formato dominicano (809/829/849)          |
| Provincia | Solo provincias de RD (32)                |

### 6.2 Límites de Tiempo

| Paso               | Tiempo Máximo  |
| ------------------ | -------------- |
| Verificación email | 24 horas       |
| Subida documentos  | 7 días         |
| Revisión por admin | 48 horas (SLA) |
| Configuración pago | 30 días        |

### 6.3 Early Bird

| Condición                 | Beneficio                 |
| ------------------------- | ------------------------- |
| Registro antes 31/01/2026 | 20% descuento de por vida |
| Early Bird                | 90 días trial (sin cargo) |
| Early Bird                | Badge "Miembro Fundador"  |

---

## 7. Eventos RabbitMQ

| Evento                       | Exchange        | Payload                             |
| ---------------------------- | --------------- | ----------------------------------- |
| `dealer.registered`          | `dealer.events` | `{ dealerId, businessName, email }` |
| `dealer.email_verified`      | `dealer.events` | `{ dealerId }`                      |
| `dealer.documents_submitted` | `dealer.events` | `{ dealerId, documentCount }`       |
| `dealer.approved`            | `dealer.events` | `{ dealerId, approvedBy }`          |
| `dealer.rejected`            | `dealer.events` | `{ dealerId, reason }`              |
| `dealer.payment_setup`       | `dealer.events` | `{ dealerId, azulCustomerId }`      |
| `dealer.activated`           | `dealer.events` | `{ dealerId, plan, isEarlyBird }`   |

---

## 8. Métricas

### 8.1 Prometheus

```

# Onboarding

dealer_registrations_total
dealer_conversions_total{plan="starter|pro|enterprise"}
dealer_rejections_total{reason="..."}

# Funnel

dealer_email_verified_total
dealer_documents_submitted_total
dealer_approved_total
dealer_activated_total

# Timing

dealer_onboarding_duration_seconds{step="registration|verification|documents|review|payment|activation"}
dealer_review_time_seconds

```

### 8.2 KPIs

| KPI               | Fórmula                    | Meta     |
| ----------------- | -------------------------- | -------- |
| Conversion Rate   | Activated / Registered     | > 60%    |
| Verification Rate | EmailVerified / Registered | > 90%    |
| Approval Rate     | Approved / UnderReview     | > 85%    |
| Time to Activate  | Promedio días              | < 5 días |

---

## 9. Integración Frontend

### 9.1 Arquitectura de Componentes

```

frontend/web/src/
├── pages/dealer/
│ ├── DealerOnboardingPageV2.tsx # Paso 1: Registro
│ ├── DealerEmailVerificationPage.tsx # Paso 2: Verificar email
│ ├── DealerDocumentsPage.tsx # Paso 3: Subir documentos
│ ├── DealerPaymentSetupPage.tsx # Paso 4: Pagar suscripción
│ └── DealerOnboardingStatusPage.tsx # Paso 5: Estado/Activación
├── services/
│ ├── azulPaymentService.ts # Cliente API para Azul
│ └── dealerOnboardingService.ts # Cliente API para onboarding
└── hooks/
├── useAzulPayment.ts # Hooks React Query para pagos
└── useDealerOnboarding.ts # Hooks React Query para onboarding

````

### 9.2 Flujo de Pago (DealerPaymentSetupPage)

```typescript
// 1. Usuario selecciona plan y llega a /dealer/onboarding/payment-setup?plan=professional
// 2. Se muestra el plan con precio (Early Bird si aplica)
// 3. Usuario ingresa datos de tarjeta
// 4. Se crea suscripción en AzulPaymentService
// 5. Se actualiza onboarding con IDs de Azul
// 6. Redirect a status page

const handleSubmit = async () => {
  // Crear suscripción - Dealer PAGA a OKLA
  const subscription = await subscriptionMutation.mutateAsync({
    dealerId,
    planId: selectedPlan.id,
    amount: finalPrice,  // RD$ con Early Bird aplicado
    isEarlyBird,
    cardNumber,
    cardExpiryMonth,
    cardExpiryYear,
    cardCVV,
    cardholderName,
  });

  // Guardar IDs en onboarding
  await updateAzulIdsMutation.mutateAsync({
    dealerId,
    data: {
      azulCustomerId: subscription.subscriptionId,
      azulSubscriptionId: subscription.azulSubscriptionId,
      enrollEarlyBird: isEarlyBird,
    },
  });

  navigate('/dealer/onboarding/status');
};
````

### 9.3 Hooks Principales

| Hook                       | Archivo                | Función                           |
| -------------------------- | ---------------------- | --------------------------------- |
| `useRegisterDealer`        | useDealerOnboarding.ts | Registrar nuevo dealer            |
| `useVerifyDealerEmail`     | useDealerOnboarding.ts | Verificar código de email         |
| `useUpdateDealerDocuments` | useDealerOnboarding.ts | Subir IDs de documentos           |
| `useUpdateAzulIds`         | useDealerOnboarding.ts | Guardar IDs de suscripción Azul   |
| `useDealerSubscription`    | useAzulPayment.ts      | Crear suscripción recurrente      |
| `useOnboardingProgress`    | useDealerOnboarding.ts | Obtener estado actual del proceso |

### 9.4 Validaciones de Tarjeta (Frontend)

```typescript
// Validación Luhn para número de tarjeta
export const validateCardNumber = (cardNumber: string): boolean => {
  const digits = cardNumber.replace(/\D/g, "");
  if (digits.length < 13 || digits.length > 19) return false;

  let sum = 0;
  let isEven = false;

  for (let i = digits.length - 1; i >= 0; i--) {
    let digit = parseInt(digits[i], 10);
    if (isEven) {
      digit *= 2;
      if (digit > 9) digit -= 9;
    }
    sum += digit;
    isEven = !isEven;
  }

  return sum % 10 === 0;
};

// Detectar marca de tarjeta
export const getCardBrand = (cardNumber: string): string => {
  if (/^4/.test(cardNumber)) return "Visa";
  if (/^5[1-5]/.test(cardNumber)) return "MasterCard";
  if (/^3[47]/.test(cardNumber)) return "American Express";
  return "Desconocida";
};
```

### 9.5 Formato de Moneda (DOP)

```typescript
// Formatear precio en Pesos Dominicanos
export const formatDOPAmount = (amount: number): string => {
  return new Intl.NumberFormat("es-DO", {
    style: "currency",
    currency: "DOP",
    minimumFractionDigits: 2,
  }).format(amount);
};

// Calcular precio Early Bird (20% descuento)
export const calculateEarlyBirdPrice = (plan: DealerPlan): number => {
  return Math.round(plan.monthlyPrice * 0.8);
};
```

### 9.6 Rutas del Frontend

| Ruta                               | Componente                  | Descripción            |
| ---------------------------------- | --------------------------- | ---------------------- |
| `/dealer/onboarding`               | DealerOnboardingPageV2      | Formulario de registro |
| `/dealer/onboarding/verify-email`  | DealerEmailVerificationPage | Verificación de email  |
| `/dealer/onboarding/documents`     | DealerDocumentsPage         | Subir documentos KYC   |
| `/dealer/onboarding/payment-setup` | DealerPaymentSetupPage      | Pagar suscripción      |
| `/dealer/onboarding/status`        | DealerOnboardingStatusPage  | Estado del proceso     |

---

## 📚 Referencias

- [01-user-service.md](01-user-service.md) - Servicio de usuarios
- [02-dealer-management.md](02-dealer-management.md) - Gestión de dealers
- [04-kyc-service.md](../01-AUTENTICACION-SEGURIDAD/04-kyc-service.md) - Verificación KYC
- [03-azul-payment.md](../05-PAGOS-FACTURACION/03-azul-payment.md) - Integración Azul (Banco Popular)

```

```
