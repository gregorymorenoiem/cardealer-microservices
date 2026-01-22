# 🎁 Programa de Referidos

> **Código:** REF-001, REF-002, REF-003  
> **Versión:** 1.0  
> **Última actualización:** Enero 21, 2026  
> **Criticidad:** 🟡 ALTA (Growth orgánico)

---

## 📋 Información General

| Campo             | Valor                                            |
| ----------------- | ------------------------------------------------ |
| **Servicio**      | ReferralService                                  |
| **Puerto**        | 5088                                             |
| **Base de Datos** | `referralservice`                                |
| **Dependencias**  | UserService, BillingService, NotificationService |

---

## 🎯 Objetivo del Proceso

1. **Adquirir usuarios nuevos** a bajo costo (CAC)
2. **Recompensar lealtad** de usuarios existentes
3. **Crear viralidad** orgánica del producto
4. **Trackear atribución** de referidos

---

## 💰 Estructura de Recompensas

### Para Compradores

| Acción del Referido | Recompensa Referidor  | Recompensa Referido   |
| ------------------- | --------------------- | --------------------- |
| Se registra         | RD$ 0                 | RD$ 0                 |
| Primera compra      | **RD$ 2,500 crédito** | **RD$ 1,000 crédito** |

### Para Vendedores Individuales

| Acción del Referido       | Recompensa Referidor  | Recompensa Referido   |
| ------------------------- | --------------------- | --------------------- |
| Se registra como vendedor | RD$ 0                 | RD$ 0                 |
| Primera publicación       | **RD$ 500 crédito**   | **RD$ 500 descuento** |
| Primera venta completada  | **RD$ 1,500 crédito** | RD$ 0                 |

### Para Dealers

| Acción del Referido         | Recompensa Referidor | Recompensa Referido            |
| --------------------------- | -------------------- | ------------------------------ |
| Se suscribe Plan Starter    | **1 mes gratis**     | **1 mes gratis adicional**     |
| Se suscribe Plan Pro        | **2 meses gratis**   | **1 mes gratis adicional**     |
| Se suscribe Plan Enterprise | **3 meses gratis**   | **2 meses gratis adicionales** |

---

## 📡 Endpoints

| Método | Endpoint                             | Descripción                   | Auth |
| ------ | ------------------------------------ | ----------------------------- | ---- |
| `GET`  | `/api/referrals/code`                | Obtener mi código de referido | ✅   |
| `GET`  | `/api/referrals/link`                | Obtener mi link de referido   | ✅   |
| `GET`  | `/api/referrals/stats`               | Mis estadísticas de referidos | ✅   |
| `GET`  | `/api/referrals/history`             | Historial de referidos        | ✅   |
| `POST` | `/api/referrals/apply`               | Aplicar código de referido    | ✅   |
| `GET`  | `/api/referrals/rewards`             | Mis recompensas pendientes    | ✅   |
| `POST` | `/api/referrals/rewards/{id}/redeem` | Canjear recompensa            | ✅   |

---

## 🗃️ Entidades

### ReferralCode

```csharp
public class ReferralCode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Código
    public string Code { get; set; }                 // JUAN-OKLA-2026
    public string ShortLink { get; set; }            // okla.do/r/JUAN
    public string FullLink { get; set; }             // https://okla.com.do/ref?code=JUAN-OKLA-2026

    // Configuración
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }

    // Tipo de usuario
    public UserReferralType UserType { get; set; }

    // Stats
    public int TotalReferrals { get; set; }
    public int SuccessfulReferrals { get; set; }
    public decimal TotalEarnings { get; set; }

    public DateTime CreatedAt { get; set; }
}

public enum UserReferralType
{
    Buyer,
    Seller,
    Dealer,
    Influencer,
    Partner
}
```

### Referral

```csharp
public class Referral
{
    public Guid Id { get; set; }
    public Guid ReferralCodeId { get; set; }
    public Guid ReferrerId { get; set; }             // Usuario que refiere
    public Guid ReferredUserId { get; set; }         // Usuario referido

    // Status
    public ReferralStatus Status { get; set; }

    // Tracking
    public DateTime SignupAt { get; set; }
    public DateTime? QualifiedAt { get; set; }       // Cuando cumple condición
    public string QualifyingAction { get; set; }     // FIRST_PURCHASE, FIRST_LISTING, SUBSCRIPTION

    // Contexto
    public string Source { get; set; }               // web, app, whatsapp, email
    public string Campaign { get; set; }
    public string LandingPage { get; set; }

    // Recompensas
    public Guid? ReferrerRewardId { get; set; }
    public Guid? ReferredRewardId { get; set; }
}

public enum ReferralStatus
{
    Pending,         // Registrado pero no calificado
    Qualified,       // Cumplió condición
    Rewarded,        // Recompensas entregadas
    Expired,         // No calificó a tiempo (30 días)
    Fraudulent       // Detectado como fraude
}
```

### ReferralReward

```csharp
public class ReferralReward
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ReferralId { get; set; }

    // Tipo
    public RewardType Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }             // DOP

    // Estado
    public RewardStatus Status { get; set; }

    // Uso
    public DateTime? RedeemedAt { get; set; }
    public Guid? AppliedToOrderId { get; set; }
    public Guid? AppliedToSubscriptionId { get; set; }

    // Expiración
    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

public enum RewardType
{
    AccountCredit,       // Crédito para cualquier uso
    ListingDiscount,     // Descuento en publicación
    SubscriptionCredit,  // Crédito para suscripción
    FreeMonth,           // Mes gratis de suscripción
    BoostCredit          // Crédito para boost
}

public enum RewardStatus
{
    Pending,         // Esperando que referido califique
    Available,       // Listo para usar
    Redeemed,        // Usado
    Expired          // Expiró sin usar
}
```

---

## 📊 Proceso REF-001: Generar y Compartir Código

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: REF-001 - Generar y Compartir Código de Referido              │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-REG                                               │
│ Sistemas: ReferralService                                              │
│ Duración: Instantáneo                                                  │
│ Criticidad: BAJA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                             | Sistema             | Actor     | Evidencia      | Código     |
| ---- | ------- | ---------------------------------- | ------------------- | --------- | -------------- | ---------- |
| 1    | 1.1     | Usuario accede a "Invitar Amigos"  | Frontend            | USR-REG   | Page accessed  | EVD-LOG    |
| 2    | 2.1     | GET /api/referrals/code            | Gateway             | USR-REG   | **Request**    | EVD-LOG    |
| 2    | 2.2     | Si no existe: generar código único | ReferralService     | Sistema   | Code generated | EVD-LOG    |
| 2    | 2.3     | Generar short link                 | ReferralService     | Sistema   | Link generated | EVD-LOG    |
| 3    | 3.1     | Mostrar código y link              | Frontend            | USR-REG   | Code displayed | EVD-SCREEN |
| 3    | 3.2     | Mostrar opciones de compartir      | Frontend            | USR-REG   | Share options  | EVD-SCREEN |
| 4    | 4.1     | Click "Copiar Link"                | Frontend            | USR-REG   | Link copied    | EVD-LOG    |
| 4    | 4.2     | O click "Compartir WhatsApp"       | Frontend            | USR-REG   | WhatsApp share | EVD-LOG    |
| 4    | 4.3     | O click "Compartir Facebook"       | Frontend            | USR-REG   | Facebook share | EVD-LOG    |
| 4    | 4.4     | O click "Enviar por Email"         | Frontend            | USR-REG   | Email share    | EVD-LOG    |
| 5    | 5.1     | Si email: mostrar formulario       | Frontend            | USR-REG   | Email form     | EVD-SCREEN |
| 5    | 5.2     | Ingresar email(s) del amigo        | Frontend            | USR-REG   | Emails input   | EVD-LOG    |
| 5    | 5.3     | **Enviar invitación**              | NotificationService | SYS-NOTIF | **Email sent** | EVD-COMM   |

### Link de Referido - Ejemplo

```
https://okla.com.do/ref?code=JUAN-OKLA-2026
                    ↓ redirect
https://okla.com.do/register?ref=JUAN-OKLA-2026

Mensaje WhatsApp pre-llenado:
"¡Hola! 🚗 Te invito a OKLA, la mejor plataforma para comprar y vender vehículos en RD.
Usa mi código JUAN-OKLA-2026 y obtén beneficios exclusivos.
Regístrate aquí: https://okla.do/r/JUAN"
```

---

## 📊 Proceso REF-002: Registrarse con Código de Referido

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: REF-002 - Registrarse con Código de Referido                  │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: USR-ANON                                              │
│ Sistemas: ReferralService, AuthService, UserService                    │
│ Duración: 1-2 minutos                                                  │
│ Criticidad: MEDIA                                                       │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                                    | Sistema             | Actor     | Evidencia            | Código     |
| ---- | ------- | --------------------------------------------------------- | ------------------- | --------- | -------------------- | ---------- |
| 1    | 1.1     | Usuario hace clic en link de referido                     | Browser             | USR-ANON  | Link clicked         | EVD-LOG    |
| 1    | 1.2     | Guardar código en cookie/localStorage                     | Frontend            | Sistema   | Code stored          | EVD-LOG    |
| 1    | 1.3     | Redirigir a registro                                      | Frontend            | Sistema   | Redirect             | EVD-LOG    |
| 2    | 2.1     | Formulario de registro                                    | Frontend            | USR-ANON  | Form displayed       | EVD-SCREEN |
| 2    | 2.2     | Código pre-llenado y visible                              | Frontend            | USR-ANON  | Code shown           | EVD-LOG    |
| 2    | 2.3     | Completar registro                                        | Frontend            | USR-ANON  | Form submitted       | EVD-LOG    |
| 3    | 3.1     | POST /api/auth/register                                   | Gateway             | USR-ANON  | **Request**          | EVD-AUDIT  |
| 3    | 3.2     | **Crear usuario**                                         | AuthService         | Sistema   | **User created**     | EVD-AUDIT  |
| 4    | 4.1     | POST /api/referrals/apply                                 | Gateway             | USR-REG   | **Request**          | EVD-AUDIT  |
| 4    | 4.2     | Validar código existe y activo                            | ReferralService     | Sistema   | Code validated       | EVD-LOG    |
| 4    | 4.3     | Verificar no es auto-referido                             | ReferralService     | Sistema   | Fraud check          | EVD-LOG    |
| 4    | 4.4     | **Crear Referral** (Status=Pending)                       | ReferralService     | Sistema   | **Referral created** | EVD-AUDIT  |
| 5    | 5.1     | **Notificar a referidor**                                 | NotificationService | SYS-NOTIF | **Notification**     | EVD-COMM   |
| 5    | 5.2     | "¡Juan se registró! Falta que complete su primera compra" | Push                | SYS-NOTIF | Push sent            | EVD-COMM   |
| 6    | 6.1     | **Audit trail**                                           | AuditService        | Sistema   | Complete audit       | EVD-AUDIT  |

---

## 📊 Proceso REF-003: Calificar Referido y Entregar Recompensas

```
┌─────────────────────────────────────────────────────────────────────────┐
│ PROCESO: REF-003 - Calificar Referido y Entregar Recompensas           │
├─────────────────────────────────────────────────────────────────────────┤
│ Actor Iniciador: Sistema (event-driven)                                │
│ Sistemas: ReferralService, BillingService, NotificationService         │
│ Duración: Instantáneo                                                  │
│ Criticidad: ALTA                                                        │
└─────────────────────────────────────────────────────────────────────────┘
```

| Paso | Subpaso | Acción                                                  | Sistema             | Actor     | Evidencia                 | Código    |
| ---- | ------- | ------------------------------------------------------- | ------------------- | --------- | ------------------------- | --------- |
| 1    | 1.1     | **Evento: ReferredUserQualified**                       | RabbitMQ            | Sistema   | **Event received**        | EVD-EVENT |
| 1    | 1.2     | Tipos: FIRST_PURCHASE, FIRST_LISTING, SUBSCRIPTION      | ReferralService     | Sistema   | Event type                | EVD-LOG   |
| 2    | 2.1     | Buscar Referral pendiente                               | ReferralService     | Sistema   | Referral found            | EVD-LOG   |
| 2    | 2.2     | Verificar no ha expirado (30 días)                      | ReferralService     | Sistema   | Expiry check              | EVD-LOG   |
| 3    | 3.1     | **Actualizar Status = Qualified**                       | ReferralService     | Sistema   | **Status updated**        | EVD-AUDIT |
| 3    | 3.2     | Registrar QualifiedAt                                   | ReferralService     | Sistema   | Timestamp saved           | EVD-LOG   |
| 4    | 4.1     | Calcular recompensa referidor                           | ReferralService     | Sistema   | Reward calculated         | EVD-LOG   |
| 4    | 4.2     | **Crear ReferralReward (referidor)**                    | ReferralService     | Sistema   | **Reward created**        | EVD-AUDIT |
| 4    | 4.3     | Calcular recompensa referido                            | ReferralService     | Sistema   | Reward calculated         | EVD-LOG   |
| 4    | 4.4     | **Crear ReferralReward (referido)**                     | ReferralService     | Sistema   | **Reward created**        | EVD-AUDIT |
| 5    | 5.1     | Si tipo crédito: agregar a wallet                       | BillingService      | Sistema   | **Credit added**          | EVD-AUDIT |
| 5    | 5.2     | Si tipo mes gratis: extender suscripción                | BillingService      | Sistema   | **Subscription extended** | EVD-AUDIT |
| 6    | 6.1     | **Notificar a referidor**                               | NotificationService | SYS-NOTIF | **Reward notification**   | EVD-COMM  |
| 6    | 6.2     | "🎉 ¡Ganaste RD$2,500! Juan completó su primera compra" | Push/Email          | SYS-NOTIF | Notification sent         | EVD-COMM  |
| 6    | 6.3     | **Notificar a referido**                                | NotificationService | SYS-NOTIF | **Reward notification**   | EVD-COMM  |
| 6    | 6.4     | "🎁 ¡Tienes RD$1,000 de crédito!"                       | Push/Email          | SYS-NOTIF | Notification sent         | EVD-COMM  |
| 7    | 7.1     | Actualizar stats del referidor                          | ReferralService     | Sistema   | Stats updated             | EVD-LOG   |
| 8    | 8.1     | **Audit trail completo**                                | AuditService        | Sistema   | Complete audit            | EVD-AUDIT |

### Evidencia de Referido Exitoso

```json
{
  "processCode": "REF-003",
  "referral": {
    "id": "ref-12345",
    "referrer": {
      "id": "user-001",
      "name": "María García",
      "code": "MARIA-OKLA-2026"
    },
    "referred": {
      "id": "user-002",
      "name": "Juan Pérez",
      "email": "juan@email.com"
    },
    "timeline": {
      "signupAt": "2026-01-15T10:00:00Z",
      "qualifiedAt": "2026-01-21T15:30:00Z",
      "qualifyingAction": "FIRST_PURCHASE",
      "daysToQualify": 6
    },
    "rewards": {
      "referrer": {
        "id": "reward-001",
        "type": "ACCOUNT_CREDIT",
        "amount": 2500,
        "status": "AVAILABLE",
        "expiresAt": "2026-04-21T15:30:00Z"
      },
      "referred": {
        "id": "reward-002",
        "type": "ACCOUNT_CREDIT",
        "amount": 1000,
        "status": "AVAILABLE",
        "expiresAt": "2026-04-21T15:30:00Z"
      }
    },
    "context": {
      "source": "whatsapp",
      "orderId": "order-67890",
      "orderAmount": 850000
    }
  }
}
```

---

## 🛡️ Prevención de Fraude

### Reglas Anti-Fraude

```csharp
public class ReferralFraudRules
{
    // No auto-referidos
    public bool SameIP { get; set; }
    public bool SameDevice { get; set; }
    public bool SameEmail { get; set; }
    public bool SamePhone { get; set; }
    public bool SamePaymentMethod { get; set; }

    // Patrones sospechosos
    public int ReferralsFromSameIP { get; set; }     // Max 2
    public int ReferralsIn24Hours { get; set; }      // Max 5
    public int ReferralWithInstantQualify { get; set; } // Flagged if < 1 hour

    // Verificaciones
    public bool ReferredHasVerifiedEmail { get; set; }
    public bool ReferredHasVerifiedPhone { get; set; }
    public decimal MinPurchaseAmount { get; set; }   // RD$ 50,000
}
```

---

## 📊 Métricas Prometheus

```yaml
# Adquisición
referral_signups_total{source, campaign}
referral_qualified_total{action_type}
referral_conversion_rate
referral_time_to_qualify_days

# Recompensas
referral_rewards_issued_total{type}
referral_rewards_redeemed_total
referral_rewards_expired_total
referral_rewards_value_total

# Top Referrers
referral_top_referrer_count{user_id}
referral_top_referrer_earnings{user_id}

# Fraude
referral_fraud_blocked_total{reason}
```

---

## 🏆 Leaderboard de Referidos

```
┌─────────────────────────────────────────────────────────────────────────┐
│ 🏆 TOP REFERIDORES DEL MES - ENERO 2026                                │
├────┬─────────────────┬────────────┬──────────────┬────────────────────┤
│ #  │ Usuario         │ Referidos  │ Convertidos  │ Ganancias          │
├────┼─────────────────┼────────────┼──────────────┼────────────────────┤
│ 🥇 │ @influencer_rd  │ 45         │ 28           │ RD$ 70,000         │
│ 🥈 │ @carlosd        │ 32         │ 18           │ RD$ 45,000         │
│ 🥉 │ @mariag         │ 28         │ 15           │ RD$ 37,500         │
│ 4  │ @juanp          │ 22         │ 12           │ RD$ 30,000         │
│ 5  │ @anamr          │ 18         │ 10           │ RD$ 25,000         │
└────┴─────────────────┴────────────┴──────────────┴────────────────────┘
```

---

## 🔗 Referencias

- [02-USUARIOS-AUTENTICACION/01-user-service.md](../02-USUARIOS-AUTENTICACION/01-user-service.md)
- [05-PAGOS-FACTURACION/01-billing-service.md](../05-PAGOS-FACTURACION/01-billing-service.md)
- [07-NOTIFICACIONES/01-notification-service.md](../07-NOTIFICACIONES/01-notification-service.md)
