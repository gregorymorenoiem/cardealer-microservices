# 🎓 Onboarding de Comprador

> **Código:** ONBOARD-001, ONBOARD-002  
> **Versión:** 1.0  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** 🟡 30% Backend | 🟡 40% UI
> **Criticidad:** 🟢 MEDIA (Activación de usuarios)

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso            | Backend | UI Access | Observación              |
| ------------------ | ------- | --------- | ------------------------ |
| Onboarding Status  | 🟡 40%  | 🟡 50%    | Flujo de registro básico |
| Preferencias       | 🟡 30%  | 🟡 40%    | Perfil básico            |
| Steps Progresivos  | 🔴 0%   | 🔴 0%     | Sin wizard de pasos      |
| Guías Interactivas | 🔴 0%   | 🔴 0%     | Sin tooltips/tours       |

### Rutas UI Existentes ✅

- `/register` - Registro básico
- `/settings/profile` - Completar perfil

### Rutas UI Faltantes 🔴

- `/welcome` - Wizard de bienvenida
- `/onboarding/preferences` - Selección de preferencias
- Tooltips y product tours (Intercom/Pendo style)

**Nota:** Onboarding básico funciona. Tours guiados y gamificación son Fase 2.

---

## 📊 Resumen de Implementación

| Componente    | Total | Implementado | Pendiente | Estado             |
| ------------- | ----- | ------------ | --------- | ------------------ |
| Controllers   | 1     | 0            | 1         | 🔴 Pendiente       |
| ONB-STATUS-\* | 3     | 1            | 2         | 🟡 Parcial         |
| ONB-PREF-\*   | 3     | 1            | 2         | 🟡 Parcial         |
| ONB-STEP-\*   | 4     | 0            | 4         | 🔴 Pendiente       |
| ONB-GUIDE-\*  | 3     | 0            | 3         | 🔴 Pendiente       |
| Tests         | 10    | 3            | 7         | 🟡 Parcial         |
| **TOTAL**     | 24    | 5            | 19        | 🟡 30% BE + 40% UI |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## �📋 Información General

| Campo             | Valor                                          |
| ----------------- | ---------------------------------------------- |
| **Servicio**      | UserService (extendido)                        |
| **Puerto**        | 5004                                           |
| **Base de Datos** | `userservice`                                  |
| **Dependencias**  | AuthService, NotificationService, AlertService |

---

## 🎯 Objetivo del Proceso

1. **Activar usuarios nuevos:** Guiarlos hasta primera acción valiosa
2. **Reducir fricción:** Simplificar primeros pasos
3. **Educar:** Mostrar funcionalidades clave
4. **Personalizar:** Configurar preferencias iniciales

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       Onboarding Architecture                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   User Journey                       Core Service                            │
│   ┌────────────────┐              ┌─────────────────────────────────────┐   │
│   │ Sign Up        │──┐           │      UserService (extended)          │   │
│   │ (Registration) │  │           │  ┌───────────────────────────────┐  │   │
│   └────────────────┘  │           │  │ Controllers                   │  │   │
│   ┌────────────────┐  │           │  │ • OnboardingController        │  │   │
│   │ Set Preferences│──┼──────────▶│  │ • PreferencesController       │  │   │
│   │ (Brands, Price)│  │           │  └───────────────────────────────┘  │   │
│   └────────────────┘  │           │  ┌───────────────────────────────┐  │   │
│   ┌────────────────┐  │           │  │ Onboarding Engine             │  │   │
│   │ Complete Steps │──┘           │  │ • Progress tracking           │  │   │
│   │ (Checklist)    │              │  │ • Step completion             │  │   │
│   └────────────────┘              │  │ • Personalization             │  │   │
│                                   │  └───────────────────────────────┘  │   │
│   Output                          │  ┌───────────────────────────────┐  │   │
│   ┌────────────────┐              │  │ Domain                        │  │   │
│   │ Personalized   │◀─────────────│  │ • UserOnboarding              │  │   │
│   │ Recommendations│              │  │ • OnboardingStep              │  │   │
│   └────────────────┘              │  │ • UserPreferences             │  │   │
│                                   │  └───────────────────────────────┘  │   │
│                                   └─────────────────────────────────────┘   │
│                                                    │                        │
│                                    ┌───────────────┼───────────────┐        │
│                                    ▼               ▼               ▼        │
│                            ┌────────────┐  ┌────────────┐  ┌────────────┐  │
│                            │ PostgreSQL │  │   Redis    │  │  RabbitMQ  │  │
│                            │ (Onboard   │  │  (Prefs    │  │ (Complete  │  │
│                            │  Status)   │  │  Cache)    │  │  Events)   │  │
│                            └────────────┘  └────────────┘  └────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Endpoints

| Método | Endpoint                                     | Descripción                    | Auth |
| ------ | -------------------------------------------- | ------------------------------ | ---- |
| `GET`  | `/api/users/onboarding/status`               | Estado del onboarding          | ✅   |
| `POST` | `/api/users/onboarding/preferences`          | Guardar preferencias           | ✅   |
| `POST` | `/api/users/onboarding/step/{step}/complete` | Marcar paso completado         | ✅   |
| `POST` | `/api/users/onboarding/skip`                 | Saltar onboarding              | ✅   |
| `GET`  | `/api/users/onboarding/recommendations`      | Recomendaciones personalizadas | ✅   |

---

## 🗃️ Entidades

### UserOnboarding

```csharp
public class UserOnboarding
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Estado general
    public OnboardingStatus Status { get; set; }
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public decimal CompletionPercent { get; set; }

    // Pasos
    public List<OnboardingStep> Steps { get; set; }

    // Preferencias capturadas
    public UserPreferences Preferences { get; set; }

    // Timeline
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? SkippedAt { get; set; }
    public int DaysToComplete { get; set; }
}

public enum OnboardingStatus
{
    NotStarted,
    InProgress,
    Completed,
    Skipped
}

public class OnboardingStep
{
    public int Order { get; set; }
    public string StepId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string IconName { get; set; }
    public StepType Type { get; set; }

    public bool IsRequired { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Contenido
    public string ActionUrl { get; set; }
    public string ActionLabel { get; set; }

    // Recompensa
    public string RewardText { get; set; }
    public int? PointsReward { get; set; }
}

public enum StepType
{
    Info,           // Solo informativo
    Action,         // Requiere hacer algo
    Preference,     // Configurar preferencia
    Verification    // Verificar algo
}
```

### UserPreferences

```csharp
public class UserPreferences
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Búsqueda de vehículo
    public UserIntent Intent { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public List<string> PreferredMakes { get; set; }
    public List<string> PreferredBodyTypes { get; set; }
    public int? MinYear { get; set; }
    public int? MaxMileage { get; set; }
    public List<string> MustHaveFeatures { get; set; }

    // Localización
    public string PreferredCity { get; set; }
    public string PreferredProvince { get; set; }
    public int? SearchRadiusKm { get; set; }

    // Comunicación
    public List<NotificationChannel> PreferredChannels { get; set; }
    public string PreferredContactTime { get; set; }
    public string PreferredLanguage { get; set; }

    // Financiamiento
    public bool InterestedInFinancing { get; set; }
    public FinancingPreference? FinancingType { get; set; }
    public bool HasTradeIn { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public enum UserIntent
{
    JustBrowsing,       // Solo mirando
    BuyingSoon,         // Comprando en 1-3 meses
    BuyingNow,          // Listo para comprar
    Researching         // Investigando opciones
}

public enum FinancingPreference
{
    CashOnly,
    NeedFinancing,
    OpenToBoth,
    PreApproved
}
```

---

## 📊 Proceso ONBOARD-001: Onboarding de Comprador

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: ONBOARD-001 - Onboarding de Nuevo Comprador                   │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG (nuevo)                                       │
│ Sistemas: UserService, NotificationService, AlertService              │
│ Duración: 2-5 minutos                                                  │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                                 | Sistema             | Actor     | Evidencia               | Código     |
| ---- | ------- | ------------------------------------------------------ | ------------------- | --------- | ----------------------- | ---------- |
| 1    | 1.1     | Usuario completa registro                              | AuthService         | USR-NEW   | **Registration**        | EVD-AUDIT  |
| 1    | 1.2     | Redirigir a onboarding                                 | Frontend            | Sistema   | Redirect                | EVD-LOG    |
| 2    | 2.1     | **Paso 1: Bienvenida**                                 | Frontend            | USR-REG   | Welcome shown           | EVD-SCREEN |
| 2    | 2.2     | Video tour (30 seg) o skip                             | Frontend            | USR-REG   | Video viewed/skipped    | EVD-LOG    |
| 3    | 3.1     | **Paso 2: ¿Qué estás buscando?**                       | Frontend            | USR-REG   | Question shown          | EVD-SCREEN |
| 3    | 3.2     | Seleccionar intent (JustBrowsing/BuyingSoon/BuyingNow) | Frontend            | USR-REG   | Intent selected         | EVD-LOG    |
| 4    | 4.1     | **Paso 3: Presupuesto**                                | Frontend            | USR-REG   | Budget form             | EVD-SCREEN |
| 4    | 4.2     | Slider de rango de precio                              | Frontend            | USR-REG   | Budget set              | EVD-LOG    |
| 5    | 5.1     | **Paso 4: Marcas favoritas**                           | Frontend            | USR-REG   | Makes selection         | EVD-SCREEN |
| 5    | 5.2     | Seleccionar 1-5 marcas                                 | Frontend            | USR-REG   | Makes selected          | EVD-LOG    |
| 6    | 6.1     | **Paso 5: Tipo de vehículo**                           | Frontend            | USR-REG   | Body types              | EVD-SCREEN |
| 6    | 6.2     | Seleccionar (Sedan, SUV, Pickup, etc.)                 | Frontend            | USR-REG   | Types selected          | EVD-LOG    |
| 7    | 7.1     | **Paso 6: Ubicación**                                  | Frontend            | USR-REG   | Location form           | EVD-SCREEN |
| 7    | 7.2     | Ingresar ciudad/provincia                              | Frontend            | USR-REG   | Location set            | EVD-LOG    |
| 8    | 8.1     | POST /api/users/onboarding/preferences                 | Gateway             | USR-REG   | **Request**             | EVD-AUDIT  |
| 8    | 8.2     | **Guardar UserPreferences**                            | UserService         | Sistema   | **Preferences saved**   | EVD-AUDIT  |
| 9    | 9.1     | **Crear alerta de búsqueda automática**                | AlertService        | Sistema   | **Alert created**       | EVD-AUDIT  |
| 9    | 9.2     | "Te notificaremos cuando haya vehículos que coincidan" | Frontend            | Sistema   | Confirmation            | EVD-SCREEN |
| 10   | 10.1    | **Paso 7: Notificaciones**                             | Frontend            | USR-REG   | Notifications prefs     | EVD-SCREEN |
| 10   | 10.2    | Elegir canales (Email, Push, WhatsApp)                 | Frontend            | USR-REG   | Channels selected       | EVD-LOG    |
| 11   | 11.1    | **Mostrar recomendaciones personalizadas**             | VehiclesSaleService | Sistema   | Recommendations         | EVD-SCREEN |
| 11   | 11.2    | Basadas en preferencias recién ingresadas              | Frontend            | USR-REG   | Listings shown          | EVD-LOG    |
| 12   | 12.1    | **Marcar onboarding completado**                       | UserService         | Sistema   | **Onboarding complete** | EVD-AUDIT  |
| 13   | 13.1    | **Email de bienvenida**                                | NotificationService | SYS-NOTIF | **Welcome email**       | EVD-COMM   |
| 14   | 14.1    | **Audit trail**                                        | AuditService        | Sistema   | Complete audit          | EVD-AUDIT  |

### Evidencia de Onboarding

```json
{
  "processCode": "ONBOARD-001",
  "onboarding": {
    "userId": "user-12345",
    "status": "COMPLETED",
    "steps": [
      {
        "order": 1,
        "stepId": "welcome",
        "completed": true,
        "completedAt": "..."
      },
      {
        "order": 2,
        "stepId": "intent",
        "completed": true,
        "completedAt": "..."
      },
      {
        "order": 3,
        "stepId": "budget",
        "completed": true,
        "completedAt": "..."
      },
      {
        "order": 4,
        "stepId": "makes",
        "completed": true,
        "completedAt": "..."
      },
      {
        "order": 5,
        "stepId": "body_types",
        "completed": true,
        "completedAt": "..."
      },
      {
        "order": 6,
        "stepId": "location",
        "completed": true,
        "completedAt": "..."
      },
      {
        "order": 7,
        "stepId": "notifications",
        "completed": true,
        "completedAt": "..."
      }
    ],
    "preferences": {
      "intent": "BUYING_SOON",
      "budget": {
        "min": 500000,
        "max": 1500000
      },
      "preferredMakes": ["Toyota", "Honda", "Hyundai"],
      "preferredBodyTypes": ["SUV", "SEDAN"],
      "location": {
        "city": "Santo Domingo",
        "province": "Distrito Nacional",
        "searchRadius": 50
      },
      "notifications": {
        "email": true,
        "push": true,
        "whatsapp": false
      },
      "financing": {
        "interested": true,
        "type": "NEED_FINANCING"
      }
    },
    "outcomes": {
      "alertCreated": true,
      "alertId": "alert-67890",
      "recommendationsShown": 12,
      "vehiclesSaved": 3
    },
    "timing": {
      "startedAt": "2026-01-21T10:30:00Z",
      "completedAt": "2026-01-21T10:33:45Z",
      "durationSeconds": 225
    }
  }
}
```

---

## 📊 Proceso ONBOARD-002: Re-engagement de Onboarding Incompleto

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: ONBOARD-002 - Re-engagement de Onboarding Incompleto          │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: SYS-SCHEDULER                                         │
│ Sistemas: UserService, NotificationService                             │
│ Triggers: 24h, 72h, 7d después de abandono                            │
│ Criticidad: BAJA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                        | Sistema             | Actor     | Evidencia         | Código   |
| ---- | ------- | --------------------------------------------- | ------------------- | --------- | ----------------- | -------- |
| 1    | 1.1     | Job detecta onboarding incompleto             | Scheduler           | Sistema   | Detection         | EVD-LOG  |
| 1    | 1.2     | Verificar no ha vuelto en 24h                 | UserService         | Sistema   | Inactivity check  | EVD-LOG  |
| 2    | 2.1     | **Enviar recordatorio**                       | NotificationService | SYS-NOTIF | **Reminder sent** | EVD-COMM |
| 2    | 2.2     | "Completa tu perfil y recibe recomendaciones" | Email               | SYS-NOTIF | Email sent        | EVD-COMM |
| 3    | 3.1     | Si no completa en 72h: segundo recordatorio   | Scheduler           | Sistema   | Second trigger    | EVD-LOG  |
| 3    | 3.2     | Push notification con incentivo               | Push                | SYS-NOTIF | Push sent         | EVD-COMM |
| 4    | 4.1     | Si no completa en 7 días                      | Scheduler           | Sistema   | Final trigger     | EVD-LOG  |
| 4    | 4.2     | Marcar como "skipped"                         | UserService         | Sistema   | Status updated    | EVD-LOG  |
| 4    | 4.3     | Usar defaults para recomendaciones            | UserService         | Sistema   | Defaults applied  | EVD-LOG  |

---

## 📱 UI Mockup del Onboarding

```
┌─────────────────────────────────────────────────────────────────────────┐
│ ●●●○○○○  OKLA                                              [Saltar →]  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│                         🚗                                              │
│                                                                         │
│            ¿Cuál es tu presupuesto?                                    │
│                                                                         │
│   RD$ 500,000 ────────●────────────── RD$ 3,000,000                   │
│                                                                         │
│   Rango seleccionado:                                                  │
│   ┌─────────────────────────────────────────────────┐                  │
│   │  RD$ 800,000  -  RD$ 1,500,000                 │                  │
│   └─────────────────────────────────────────────────┘                  │
│                                                                         │
│   💡 El 45% de los vehículos en OKLA están en este rango              │
│                                                                         │
│                                                                         │
│                                                                         │
│   ┌─────────────────────────────────────────────────┐                  │
│   │              Continuar →                        │                  │
│   └─────────────────────────────────────────────────┘                  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Métricas Prometheus

```yaml
# Onboarding
onboarding_started_total
onboarding_completed_total
onboarding_skipped_total
onboarding_completion_rate
onboarding_step_completion{step}
onboarding_duration_seconds

# Re-engagement
onboarding_reminder_sent_total{trigger}
onboarding_reminder_conversion_rate

# Quality
onboarding_to_first_save_days
onboarding_to_first_contact_days
onboarding_to_first_purchase_days
```

---

## 🎯 Checklist de Pasos de Onboarding

| Paso                     | Obligatorio | Recompensa             | Tiempo Est. |
| ------------------------ | ----------- | ---------------------- | ----------- |
| 1. Bienvenida/Video      | ❌          | -                      | 30 seg      |
| 2. Intent (¿qué buscas?) | ✅          | -                      | 10 seg      |
| 3. Presupuesto           | ✅          | -                      | 15 seg      |
| 4. Marcas favoritas      | ❌          | 🎁 Búsqueda guardada   | 20 seg      |
| 5. Tipo de vehículo      | ❌          | -                      | 15 seg      |
| 6. Ubicación             | ✅          | -                      | 15 seg      |
| 7. Notificaciones        | ✅          | 🎁 Alertas automáticas | 10 seg      |
| **TOTAL**                | 4/7         |                        | ~2 min      |

---

## 🔗 Referencias

- [02-USUARIOS-AUTENTICACION/01-user-service.md](../02-USUARIOS-AUTENTICACION/01-user-service.md)
- [17-ENGAGEMENT-RETENCION/01-alertas-busquedas-guardadas.md](01-alertas-busquedas-guardadas.md)
- [07-NOTIFICACIONES/01-notification-service.md](../07-NOTIFICACIONES/01-notification-service.md)
