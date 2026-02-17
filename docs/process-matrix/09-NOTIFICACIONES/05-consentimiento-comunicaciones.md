# 📧 Consentimiento para Comunicaciones - Opt-in/Opt-out

> **Marco Legal:** Ley 172-13 - Protección de Datos Personales  
> **Regulador:** Superintendencia de Bancos / Procuraduría General  
> **Última actualización:** Enero 25, 2026  
> **Estado de Implementación:** 🟡 50% Backend | 🟡 40% UI

---

## ⚠️ AUDITORÍA DE ACCESO UI (Enero 25, 2026)

| Proceso                         | Backend      | UI Access     | Observación        |
| ------------------------------- | ------------ | ------------- | ------------------ |
| CONSENT-REG-001 Registro        | ✅ Existe    | ✅ /register  | Checkbox marketing |
| CONSENT-PREF-001 Preferencias   | 🟡 Parcial   | 🟡 /settings  | Básico             |
| CONSENT-UNSUB-001 Darse de baja | 🟡 Parcial   | 🟡 Email link | Funciona           |
| CONSENT-AUDIT-001 Auditoría     | 🔴 Pendiente | 🔴 Falta      | Sin registro       |

### Rutas UI Existentes ✅

- `/register` → Checkboxes de consentimiento
- `/settings/notifications` → Preferencias básicas
- `/unsubscribe?token=xxx` → Darse de baja por email

### Rutas UI Faltantes 🔴

- `/settings/privacy/consent-history` → Historial de consentimientos
- `/consent/update` → Actualizar consentimientos

---

## 📊 Resumen de Implementación

| Componente                         | Total | Implementado | Pendiente | Estado         |
| ---------------------------------- | ----- | ------------ | --------- | -------------- |
| **CONSENT-REG-\*** (Registro)      | 3     | 2            | 1         | 🟡 Parcial     |
| **CONSENT-PREF-\*** (Preferencias) | 4     | 2            | 2         | 🟡 Parcial     |
| **CONSENT-UNSUB-\*** (Baja)        | 3     | 2            | 1         | 🟡 Parcial     |
| **CONSENT-AUDIT-\*** (Auditoría)   | 4     | 0            | 4         | 🔴 Pendiente   |
| **Tests**                          | 10    | 3            | 7         | 🟡 Parcial     |
| **TOTAL**                          | 24    | 9            | 15        | 🟡 40% Backend |

---

## 1. Información General

### 1.1 Requisitos Legales

La Ley 172-13 y la Ley 126-02 (Comercio Electrónico) establecen:

| Requisito                 | Descripción                                       |
| ------------------------- | ------------------------------------------------- |
| **Consentimiento previo** | Usuario debe autorizar antes de recibir marketing |
| **Claro e inequívoco**    | No pre-marcado, acción afirmativa del usuario     |
| **Específico**            | Por cada tipo de comunicación                     |
| **Revocable**             | Poder darse de baja en cualquier momento          |
| **Documentado**           | Registrar fecha, hora, IP del consentimiento      |

### 1.2 Tipos de Comunicación

| Tipo              | Descripción                         | Consentimiento Requerido |
| ----------------- | ----------------------------------- | ------------------------ |
| **Transaccional** | Confirmaciones, facturas, seguridad | ❌ No (obligatorio)      |
| **Servicio**      | Alertas de cuenta, cambios de TOS   | ❌ No (obligatorio)      |
| **Marketing**     | Promociones, ofertas, newsletter    | ✅ Sí                    |
| **Partners**      | Ofertas de terceros                 | ✅ Sí (separado)         |
| **Investigación** | Encuestas, feedback                 | ✅ Sí                    |

---

## 2. Procesos de Implementación

### 2.1 CONSENT-REG: Consentimiento en Registro

#### CONSENT-REG-001: Checkboxes de Registro

| Campo       | Valor           |
| ----------- | --------------- |
| **Proceso** | CONSENT-REG-001 |
| **Ruta**    | `/register`     |
| **Estado**  | ✅ Implementado |

**UI Actual:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│  CREAR CUENTA                                                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  [Formulario de registro...]                                            │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                                                                   │  │
│  │  [✓] Acepto los Términos y Condiciones y la Política de          │  │
│  │      Privacidad (obligatorio)                                    │  │
│  │                                                                   │  │
│  │  [ ] Deseo recibir ofertas, promociones y novedades de OKLA      │  │
│  │      por email                                                    │  │
│  │                                                                   │  │
│  │  [ ] Acepto recibir ofertas de partners seleccionados de OKLA    │  │
│  │                                                                   │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  [Crear Cuenta]                                                         │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

**Requisitos Cumplidos:**

- ✅ Checkboxes NO pre-marcados
- ✅ Términos separados de marketing
- ✅ Partners separado de OKLA
- ✅ Texto claro y visible

#### CONSENT-REG-002: Registro del Consentimiento

| Campo       | Valor                  |
| ----------- | ---------------------- |
| **Proceso** | CONSENT-REG-002        |
| **Nombre**  | Guardar Consentimiento |
| **Estado**  | 🟡 Parcial             |

**Datos a Registrar:**

```json
{
  "user_id": "uuid",
  "consent_type": "marketing_email",
  "granted": true,
  "timestamp": "2026-01-25T10:30:00Z",
  "ip_address": "192.168.1.100",
  "user_agent": "Mozilla/5.0...",
  "source": "registration_form",
  "version": "1.0" // versión del texto de consentimiento
}
```

---

### 2.2 CONSENT-PREF: Preferencias de Comunicación

#### CONSENT-PREF-001: Centro de Preferencias

| Campo       | Valor                     |
| ----------- | ------------------------- |
| **Proceso** | CONSENT-PREF-001          |
| **Ruta**    | `/settings/notifications` |
| **Estado**  | 🟡 Parcial                |

**UI Propuesta Mejorada:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📧 PREFERENCIAS DE COMUNICACIÓN                                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  COMUNICACIONES OBLIGATORIAS                                            │
│  (No puedes desactivar estas notificaciones)                            │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ ✓ Confirmaciones de cuenta y seguridad                           │  │
│  │ ✓ Transacciones y pagos                                          │  │
│  │ ✓ Cambios en términos de servicio                                │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  EMAIL                                                                  │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ [🔘] Newsletter semanal - Novedades del mercado automotriz       │  │
│  │ [🔘] Ofertas y promociones de OKLA                                │  │
│  │ [⚪] Ofertas de partners seleccionados                            │  │
│  │ [🔘] Alertas de nuevos vehículos (según mis búsquedas)           │  │
│  │ [⚪] Encuestas y feedback                                         │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  SMS                                                                    │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ [🔘] Códigos de verificación (obligatorio)                       │  │
│  │ [⚪] Alertas de precio en favoritos                               │  │
│  │ [⚪] Ofertas y promociones                                        │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  PUSH NOTIFICATIONS                                                     │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ [🔘] Mensajes nuevos                                              │  │
│  │ [🔘] Actualizaciones en mis anuncios                              │  │
│  │ [⚪] Recomendaciones personalizadas                               │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  [Guardar Preferencias]                                                 │
│                                                                         │
│  ℹ️ Último cambio: 20/01/2026 a las 14:35                              │
│  [Ver historial de cambios]                                             │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

#### CONSENT-PREF-002: Granularidad por Canal

| Campo       | Valor                  |
| ----------- | ---------------------- |
| **Proceso** | CONSENT-PREF-002       |
| **Nombre**  | Preferencias por Canal |
| **Estado**  | 🔴 Pendiente           |

**Matriz de Preferencias:**

| Categoría         | Email          | SMS            | Push   | WhatsApp |
| ----------------- | -------------- | -------------- | ------ | -------- |
| Transaccional     | ✅ Obligatorio | ⚪ N/A         | ⚪ N/A | ⚪ N/A   |
| Seguridad         | ✅ Obligatorio | ✅ Obligatorio | 🔘     | ⚪ N/A   |
| Marketing OKLA    | 🔘             | 🔘             | 🔘     | 🔘       |
| Partners          | 🔘             | 🔘             | 🔘     | 🔘       |
| Alertas vehículos | 🔘             | 🔘             | 🔘     | 🔘       |
| Encuestas         | 🔘             | ⚪ N/A         | ⚪ N/A | ⚪ N/A   |

---

### 2.3 CONSENT-UNSUB: Darse de Baja

#### CONSENT-UNSUB-001: Link en Emails

| Campo       | Valor                             |
| ----------- | --------------------------------- |
| **Proceso** | CONSENT-UNSUB-001                 |
| **Ruta**    | `/unsubscribe?token=xxx&type=xxx` |
| **Estado**  | ✅ Implementado                   |

**Footer de Emails:**

```html
<p style="font-size: 12px; color: #666;">
  Recibiste este email porque te suscribiste a OKLA.
  <a href="https://okla.com.do/unsubscribe?token=abc123&type=marketing">
    Darse de baja
  </a>
  |
  <a href="https://okla.com.do/settings/notifications">
    Gestionar preferencias
  </a>
</p>
```

#### CONSENT-UNSUB-002: Página de Confirmación

| Campo       | Valor                  |
| ----------- | ---------------------- |
| **Proceso** | CONSENT-UNSUB-002      |
| **Ruta**    | `/unsubscribe/confirm` |
| **Estado**  | 🟡 Parcial             |

**UI:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📧 CANCELAR SUSCRIPCIÓN                                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ¿Seguro que deseas darte de baja?                                      │
│                                                                         │
│  Dejarás de recibir: Ofertas y promociones de OKLA                     │
│                                                                         │
│  Seguirás recibiendo:                                                   │
│  • Confirmaciones de transacciones                                      │
│  • Alertas de seguridad                                                 │
│  • Notificaciones de tu cuenta                                          │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ ( ) Solo cancelar "Ofertas y promociones"                        │  │
│  │ ( ) Cancelar TODAS las comunicaciones de marketing               │  │
│  │ ( ) Preferir recibir menos emails (mensual en vez de semanal)   │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  [Mantener Suscripción]              [Confirmar Cancelación]           │
│                                                                         │
│  ¿Cambiar preferencias? [Ir a Configuración]                           │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

### 2.4 CONSENT-AUDIT: Registro de Auditoría

#### CONSENT-AUDIT-001: Tabla de Consentimientos

| Campo       | Valor                       |
| ----------- | --------------------------- |
| **Proceso** | CONSENT-AUDIT-001           |
| **Nombre**  | Registro de Consentimientos |
| **Estado**  | 🔴 Pendiente                |

**Modelo de Datos:**

```csharp
public class ConsentRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Tipo de consentimiento
    public ConsentType Type { get; set; }
    public string Channel { get; set; } // email, sms, push, whatsapp

    // Estado
    public bool Granted { get; set; }
    public DateTime Timestamp { get; set; }

    // Contexto
    public string Source { get; set; } // registration, settings, unsubscribe
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }

    // Versión del texto legal
    public string ConsentTextVersion { get; set; }
    public string ConsentTextHash { get; set; }

    // Si fue revocado
    public DateTime? RevokedAt { get; set; }
    public string RevokedReason { get; set; }
}

public enum ConsentType
{
    TermsOfService,           // Términos (obligatorio)
    PrivacyPolicy,            // Privacidad (obligatorio)
    MarketingOkla,            // Marketing de OKLA
    MarketingPartners,        // Marketing de partners
    VehicleAlerts,            // Alertas de vehículos
    PriceDropAlerts,          // Alertas de bajada de precio
    Newsletter,               // Newsletter
    Surveys,                  // Encuestas
    Personalization,          // Perfilamiento/recomendaciones
    ThirdPartySharing         // Compartir con terceros
}
```

#### CONSENT-AUDIT-002: Historial de Usuario

| Campo       | Valor                               |
| ----------- | ----------------------------------- |
| **Proceso** | CONSENT-AUDIT-002                   |
| **Ruta**    | `/settings/privacy/consent-history` |
| **Estado**  | 🔴 Pendiente                        |

**UI Propuesta:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│  📋 HISTORIAL DE CONSENTIMIENTOS                                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Este es el registro de tus consentimientos de comunicación.            │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ 25/01/2026 14:35                                                 │  │
│  │ ❌ Newsletter semanal - Desactivado                               │  │
│  │    Fuente: Página de preferencias                                │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ 20/01/2026 10:15                                                 │  │
│  │ ✅ Alertas de precio - Activado                                   │  │
│  │    Fuente: Página de preferencias                                │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ 15/06/2025 09:30                                                 │  │
│  │ ✅ Marketing OKLA - Activado                                      │  │
│  │ ✅ Partners - Activado                                            │  │
│  │ ✅ Términos v1.0 - Aceptado                                       │  │
│  │ ✅ Privacidad v1.0 - Aceptado                                     │  │
│  │    Fuente: Registro de cuenta                                    │  │
│  │    IP: 192.168.xxx.xxx                                           │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  [Descargar Historial Completo]                                         │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Endpoints API

### 3.1 ConsentController

| Método | Endpoint                   | Descripción                  | Auth | Estado |
| ------ | -------------------------- | ---------------------------- | ---- | ------ |
| `GET`  | `/api/consent/preferences` | Mis preferencias actuales    | ✅   | 🟡     |
| `PUT`  | `/api/consent/preferences` | Actualizar preferencias      | ✅   | 🟡     |
| `GET`  | `/api/consent/history`     | Historial de consentimientos | ✅   | 🔴     |
| `POST` | `/api/consent/grant`       | Otorgar consentimiento       | ✅   | 🔴     |
| `POST` | `/api/consent/revoke`      | Revocar consentimiento       | ✅   | 🔴     |

### 3.2 UnsubscribeController (Público)

| Método | Endpoint                    | Descripción      | Auth | Estado |
| ------ | --------------------------- | ---------------- | ---- | ------ |
| `GET`  | `/api/unsubscribe/validate` | Validar token    | ❌   | ✅     |
| `POST` | `/api/unsubscribe/confirm`  | Confirmar baja   | ❌   | ✅     |
| `GET`  | `/api/unsubscribe/options`  | Opciones de baja | ❌   | 🟡     |

---

## 4. Integración con NotificationService

### 4.1 Verificación Antes de Enviar

```csharp
public class NotificationService
{
    public async Task<bool> SendMarketingEmail(Guid userId, string templateId)
    {
        // 1. Verificar consentimiento
        var hasConsent = await _consentService.HasConsent(
            userId,
            ConsentType.MarketingOkla,
            "email"
        );

        if (!hasConsent)
        {
            _logger.LogInformation("User {UserId} has not consented to marketing emails", userId);
            return false;
        }

        // 2. Enviar email
        await _emailService.Send(userId, templateId);
        return true;
    }
}
```

### 4.2 Lista de Supresión

Mantener lista de emails/teléfonos que se han dado de baja para evitar envíos accidentales:

```csharp
public class SuppressionList
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public SuppressionType Type { get; set; } // marketing, all
    public DateTime AddedAt { get; set; }
    public string Reason { get; set; }
}
```

---

## 5. Cumplimiento CAN-SPAM / GDPR

Aunque las leyes dominicanas son el marco principal, seguir buenas prácticas internacionales:

| Requisito                  | CAN-SPAM        | GDPR      | RD (172-13) | Implementado |
| -------------------------- | --------------- | --------- | ----------- | ------------ |
| Opt-in requerido           | ❌ (opt-out OK) | ✅        | ✅          | ✅           |
| Link de baja               | ✅              | ✅        | ✅          | ✅           |
| Identificar remitente      | ✅              | ✅        | ✅          | ✅           |
| Dirección física           | ✅              | ✅        | 🟡          | ✅ Footer    |
| Procesar baja en 10 días   | ✅              | Inmediato | 10 días     | ✅ Inmediato |
| Registro de consentimiento | ❌              | ✅        | ✅          | 🔴 Pendiente |
| Portabilidad               | ❌              | ✅        | ✅          | 🔴 Pendiente |

---

## 6. Cronograma de Implementación

### Fase 1: Q1 2026 - Base ✅

- [x] Checkboxes en registro
- [x] Página de preferencias básica
- [x] Link de unsubscribe en emails

### Fase 2: Q1 2026 - Mejoras 🟡

- [ ] Granularidad por canal
- [ ] Página de confirmación de baja mejorada
- [ ] Registro de auditoría

### Fase 3: Q2 2026 - Completo 🔴

- [ ] Historial de consentimientos para usuario
- [ ] Exportación de historial
- [ ] Dashboard admin de consentimientos
- [ ] Integración con lista de supresión

---

## 7. Referencias

| Documento           | Ubicación                   |
| ------------------- | --------------------------- |
| Ley 172-13          | congreso.gob.do             |
| 02-ley-172-13.md    | 08-COMPLIANCE-LEGAL-RD      |
| 06-derechos-arco.md | 02-USUARIOS-DEALERS         |
| NotificationService | backend/NotificationService |

---

**Última revisión:** Enero 25, 2026  
**Próxima revisión:** Febrero 25, 2026  
**Responsable:** Equipo de Desarrollo OKLA  
**Prioridad:** 🟡 MEDIA (Funcionalidad básica existe)
