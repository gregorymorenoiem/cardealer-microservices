# 🎁 Early Bird Program - 3 Meses GRATIS

## 📋 Descripción

Programa de lanzamiento que ofrece **3 meses gratis** a todos los usuarios que se inscriban temprano. Los miembros Early Bird reciben el **badge permanente de "Miembro Fundador"** que se mantiene incluso después de usar el beneficio.

## 🎯 Beneficios

### Para Usuarios

- ✅ **3 meses gratis** de cualquier plan
- ✅ **Badge "Miembro Fundador"** permanente
- ✅ Prioridad en soporte (futuro)
- ✅ Acceso anticipado a nuevas features (futuro)

### Para la Plataforma

- 📈 Impulsar adopción temprana
- 🎯 Construir base de usuarios leales
- 💬 Obtener feedback valioso
- 🔥 Crear FOMO (fear of missing out)

## 🏗️ Arquitectura

### Entidad: EarlyBirdMember

```csharp
public class EarlyBirdMember
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public DateTime FreeUntil { get; private set; }        // EnrolledAt + 3 meses
    public bool HasUsedBenefit { get; private set; }       // Si ya usó los 3 meses gratis
    public DateTime? BenefitUsedAt { get; private set; }
    public string? SubscriptionIdWhenUsed { get; private set; }

    // Métodos de negocio
    public bool IsInFreePeriod()                           // Si aún está en período gratuito
    public void MarkBenefitUsed(Guid subscriptionId)       // Marcar beneficio como usado
    public int GetRemainingFreeDays()                      // Días gratis restantes
    public bool HasFounderBadge()                          // Siempre true (badge permanente)
}
```

### Tabla: early_bird_members

| Columna                  | Tipo         | Descripción                                      |
| ------------------------ | ------------ | ------------------------------------------------ |
| `Id`                     | UUID         | Primary key                                      |
| `UserId`                 | UUID         | FK a usuario (UNIQUE)                            |
| `EnrolledAt`             | TIMESTAMP    | Fecha de inscripción                             |
| `FreeUntil`              | TIMESTAMP    | Hasta cuándo tiene gratis (EnrolledAt + 3 meses) |
| `HasUsedBenefit`         | BOOLEAN      | Si ya usó el beneficio                           |
| `BenefitUsedAt`          | TIMESTAMP    | Cuándo usó el beneficio                          |
| `SubscriptionIdWhenUsed` | VARCHAR(100) | ID de suscripción cuando usó el beneficio        |
| `CreatedAt`              | TIMESTAMP    | Timestamp de creación                            |

**Índices:**

- `idx_early_bird_user` UNIQUE en `UserId`
- `idx_early_bird_used` en `HasUsedBenefit`
- `idx_early_bird_free_until` en `FreeUntil`

## 📡 API Endpoints

### Para Usuarios

| Método | Endpoint                        | Descripción                  |
| ------ | ------------------------------- | ---------------------------- |
| `GET`  | `/api/billing/earlybird/status` | Obtener mi estado Early Bird |
| `POST` | `/api/billing/earlybird/enroll` | Inscribirme en Early Bird    |

### Para Admins

| Método | Endpoint                                       | Descripción                      |
| ------ | ---------------------------------------------- | -------------------------------- |
| `GET`  | `/api/billing/earlybird/user/{userId}`         | Ver estado de usuario específico |
| `POST` | `/api/billing/earlybird/admin/enroll/{userId}` | Inscribir usuario manualmente    |
| `GET`  | `/api/billing/earlybird/stats`                 | Estadísticas del programa        |

## 📝 Ejemplos de Uso

### 1. Verificar mi estado Early Bird

```bash
curl -X GET http://localhost:8080/api/billing/earlybird/status \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Respuesta (NO inscrito):**

```json
{
  "isEnrolled": false,
  "hasFounderBadge": false,
  "isInFreePeriod": false,
  "remainingFreeDays": 0,
  "message": "Usuario no inscrito en Early Bird"
}
```

**Respuesta (INSCRITO y activo):**

```json
{
  "isEnrolled": true,
  "hasFounderBadge": true,
  "isInFreePeriod": true,
  "remainingFreeDays": 85,
  "enrolledAt": "2026-01-08T15:00:00Z",
  "freeUntil": "2026-04-08T15:00:00Z",
  "hasUsedBenefit": false,
  "message": "¡Tienes 85 días gratis restantes!"
}
```

**Respuesta (Beneficio usado):**

```json
{
  "isEnrolled": true,
  "hasFounderBadge": true,
  "isInFreePeriod": false,
  "remainingFreeDays": 0,
  "enrolledAt": "2026-01-08T15:00:00Z",
  "freeUntil": "2026-04-08T15:00:00Z",
  "hasUsedBenefit": true,
  "benefitUsedAt": "2026-01-15T10:00:00Z",
  "message": "Beneficio usado - Tienes el badge de Miembro Fundador"
}
```

### 2. Inscribirse en Early Bird

```bash
curl -X POST http://localhost:8080/api/billing/earlybird/enroll \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "freeMonths": 3
  }'
```

**Respuesta:**

```json
{
  "isEnrolled": true,
  "hasFounderBadge": true,
  "isInFreePeriod": true,
  "remainingFreeDays": 90,
  "enrolledAt": "2026-01-08T16:00:00Z",
  "freeUntil": "2026-04-08T16:00:00Z",
  "hasUsedBenefit": false,
  "message": "¡Bienvenido al programa Early Bird! Tienes 3 meses gratis."
}
```

### 3. Estadísticas del Programa (Admin)

```bash
curl -X GET http://localhost:8080/api/billing/earlybird/stats \
  -H "Authorization: Bearer ADMIN_JWT_TOKEN"
```

**Respuesta:**

```json
{
  "totalEnrolled": 1523,
  "activeMembers": 892,
  "membersWhoUsedBenefit": 631
}
```

## 🔄 Flujo de Negocio

### Caso 1: Usuario se inscribe en Early Bird

```
1. Usuario hace POST /api/billing/earlybird/enroll
2. Sistema verifica que no esté inscrito
3. Crea EarlyBirdMember con FreeUntil = now + 3 meses
4. Retorna status con badge de Miembro Fundador
```

### Caso 2: Usuario crea suscripción (con Early Bird activo)

```
1. Usuario hace POST /api/billing/subscriptions
2. Sistema verifica IsInFreePeriod() → true
3. Aplica 3 meses gratis a la suscripción
4. Marca beneficio como usado: MarkBenefitUsed(subscriptionId)
5. Usuario mantiene badge de Miembro Fundador permanentemente
```

### Caso 3: Usuario crea suscripción (Early Bird expirado)

```
1. Usuario hace POST /api/billing/subscriptions
2. Sistema verifica IsInFreePeriod() → false
3. Aplica precio normal desde el inicio
4. Badge de Miembro Fundador sigue activo (permanente)
```

## 💻 Integración con Suscripciones

### Modificar SubscriptionController.CreateSubscription()

```csharp
[HttpPost]
public async Task<ActionResult<SubscriptionDto>> CreateSubscription(
    [FromBody] CreateSubscriptionRequest request)
{
    var userId = GetCurrentUserId();

    // Verificar Early Bird status
    var earlyBird = await _earlyBirdRepository.GetByUserIdAsync(userId);

    int freeMonths = 0;
    if (earlyBird != null && earlyBird.IsInFreePeriod())
    {
        freeMonths = 3;
        earlyBird.MarkBenefitUsed(subscription.Id);
        await _earlyBirdRepository.UpdateAsync(earlyBird);
    }

    // Crear suscripción con meses gratis
    var subscription = new Subscription(
        userId,
        request.Plan,
        request.Cycle,
        request.Price,
        trialDays: freeMonths * 30  // Convertir meses a días
    );

    await _subscriptionRepository.CreateAsync(subscription);

    return Ok(MapToDto(subscription));
}
```

## 🎨 Frontend - Badge de Miembro Fundador

### React Component (Ejemplo)

```tsx
import { useEarlyBirdStatus } from "@/hooks/useEarlyBirdStatus";

export const FounderBadge = () => {
  const { data: earlyBird } = useEarlyBirdStatus();

  if (!earlyBird?.hasFounderBadge) return null;

  return (
    <div className="inline-flex items-center gap-2 px-3 py-1 bg-gradient-to-r from-yellow-400 to-orange-500 rounded-full text-white text-sm font-semibold shadow-lg">
      <Trophy className="w-4 h-4" />
      <span>Miembro Fundador</span>
    </div>
  );
};
```

### Mostrar en perfil de usuario

```tsx
export const UserProfile = () => {
  const { data: earlyBird } = useEarlyBirdStatus();

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Avatar src={user.avatar} />
        <div>
          <h2>{user.name}</h2>
          {earlyBird?.hasFounderBadge && <FounderBadge />}
        </div>
      </div>

      {earlyBird?.isInFreePeriod && (
        <Alert>
          <Gift className="w-4 h-4" />
          <AlertDescription>
            ¡Tienes {earlyBird.remainingFreeDays} días gratis restantes!
          </AlertDescription>
        </Alert>
      )}
    </div>
  );
};
```

## 📊 Métricas a Trackear

### KPIs del Programa

1. **Tasa de conversión Early Bird → Suscripción Paga**
   - Cuántos users que usaron los 3 meses gratis continúan pagando
2. **Tiempo promedio hasta conversión**
   - Días desde inscripción hasta crear primera suscripción
3. **Valor del badge**
   - Retención de Miembros Fundadores vs usuarios regulares
4. **Velocidad de adopción**
   - Inscripciones Early Bird por día/semana

### Queries útiles

```sql
-- Total inscritos
SELECT COUNT(*) FROM early_bird_members;

-- Inscritos que aún no usan el beneficio
SELECT COUNT(*) FROM early_bird_members
WHERE "HasUsedBenefit" = false
AND "FreeUntil" >= NOW();

-- Tasa de conversión a pago
SELECT
  COUNT(*) FILTER (WHERE "HasUsedBenefit" = true) * 100.0 / COUNT(*) as conversion_rate
FROM early_bird_members;

-- Días promedio hasta usar beneficio
SELECT AVG(EXTRACT(EPOCH FROM ("BenefitUsedAt" - "EnrolledAt"))/86400) as avg_days
FROM early_bird_members
WHERE "HasUsedBenefit" = true;
```

## 🚀 Estrategia de Lanzamiento

### Fase 1: Pre-lanzamiento (Semana -2 a -1)

- [ ] Crear landing page con countdown
- [ ] Publicar teaser en redes sociales
- [ ] Email a lista de espera
- [ ] Mensajes: "Solo primeros 1000", "Oferta limitada"

### Fase 2: Lanzamiento (Día 0)

- [ ] Activar endpoint `/api/billing/earlybird/enroll`
- [ ] Banner prominente en homepage
- [ ] Push notification a todos los usuarios
- [ ] Comunicación clara: "3 meses GRATIS + Badge Fundador"

### Fase 3: Post-lanzamiento (Semana 1-4)

- [ ] Emails de recordatorio a no-inscritos
- [ ] Testimoniales de Early Birds en homepage
- [ ] Contador de "X usuarios ya se inscribieron"
- [ ] Deadline claro: "Cierra el 31 de Enero"

### Fase 4: Cierre (Después del deadline)

- [ ] Desactivar inscripciones nuevas
- [ ] Mantener beneficios de inscritos
- [ ] Badge de Fundador permanece para siempre
- [ ] Campaigns de retención para cuando expire el free period

## ⚠️ Consideraciones Importantes

### 1. Límite de Inscripciones (Opcional)

```csharp
// En EarlyBirdController.Enroll()
const int MAX_EARLY_BIRD_MEMBERS = 5000;

var totalEnrolled = await _repository.GetTotalEnrolledCountAsync();
if (totalEnrolled >= MAX_EARLY_BIRD_MEMBERS)
{
    return BadRequest(new {
        error = "Programa Early Bird cerrado - Límite alcanzado"
    });
}
```

### 2. Fecha Límite de Inscripción

```csharp
var ENROLLMENT_DEADLINE = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);

if (DateTime.UtcNow > ENROLLMENT_DEADLINE)
{
    return BadRequest(new {
        error = "Programa Early Bird cerrado - Fecha límite superada"
    });
}
```

### 3. Prevenir Abuso

- Un usuario = una inscripción (UNIQUE constraint en UserId)
- Validar que userId sea real (verificar en UserService)
- No permitir re-inscripción si ya usó el beneficio

### 4. Comunicación Clara

**En UI:**

- Mostrar countdown "Quedan X días para inscribirte"
- Explicar qué pasa después de los 3 meses gratis
- Dejar claro que el badge es permanente

**En Emails:**

- Recordatorio a los 7 días de que expire el free period
- Opciones de upgrade cuando expire
- Destacar que el badge se mantiene

## 🎉 Mensajes de Marketing

### Homepage Banner

```
🎁 OFERTA DE LANZAMIENTO
¡Los primeros 5,000 usuarios obtienen 3 MESES GRATIS!
Además: Badge permanente de "Miembro Fundador" 🏆
[Inscribirse Ahora] ⏰ Cierra el 31 de Enero
```

### Email de Inscripción Exitosa

```
Subject: ¡Bienvenido al programa Early Bird! 🎉

Hola {name},

¡Felicidades! Ya eres oficialmente un Miembro Fundador de OKLA.

Tu beneficio:
✅ 3 meses GRATIS de cualquier plan
✅ Badge exclusivo de "Miembro Fundador" (permanente)
✅ Prioridad en soporte

Tu período gratuito vence el: 8 de Abril, 2026

¿Qué sigue?
1. Explora todas las funcionalidades
2. Publica tus vehículos sin límite
3. Cuando estés listo, elige tu plan (antes de que expire)

[Empezar Ahora]

Gracias por ser parte de nuestro lanzamiento,
El equipo de OKLA
```

---

**Última actualización:** Enero 8, 2026  
**Sprint:** Sprint 1 - Búsqueda y Descubrimiento  
**Feature Owner:** Equipo Billing
