# 📱 Communications APIs

**Categoría:** Comunicaciones y Mensajería  
**APIs:** 3 (Twilio WhatsApp, Twilio SMS, SendGrid)  
**Estado:** En Implementación (Fase 1)  
**Prioridad:** 🔴 CRÍTICA - Semana 1-2

---

## 📖 Resumen de Categoría

Las APIs de comunicaciones permiten a OKLA enviar mensajes a través de múltiples canales: WhatsApp, SMS y Email. Estas son fundamentales para:

- 📲 **Notificaciones en tiempo real** a compradores y vendedores
- 💬 **Conversaciones directas** entre usuarios
- 📧 **Confirmaciones de transacciones** y recordatorios
- 🔔 **Alertas de precio** y nuevas publicaciones

### 💡 Casos de Uso en OKLA

1. **WhatsApp (Twilio):** Contacto directo buyer-seller
2. **SMS (Twilio):** Códigos OTP, recordatorios
3. **Email (SendGrid):** Notificaciones masivas, newsletters

---

## 🔗 APIs en Esta Categoría

| #   | API                 | Endpoint             | Prioridad  | Estado   |
| --- | ------------------- | -------------------- | ---------- | -------- |
| 1   | **Twilio WhatsApp** | `/messages/whatsapp` | 🔴 Crítica | Semana 1 |
| 2   | **Twilio SMS**      | `/messages/sms`      | 🔴 Crítica | Semana 2 |
| 3   | **SendGrid Email**  | `/messages/email`    | 🟠 Alta    | Semana 2 |

---

## 🏗️ Arquitectura de Integración

```
┌─────────────────────────────────────────────────────────────┐
│                    OKLA FRONTEND                            │
│  (React - web/mobile con botones de contacto)              │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│             OKLA BACKEND (NotificationService)              │
│  POST /api/notifications/send                               │
│  - Valida destinatario                                      │
│  - Selecciona canal (WhatsApp/SMS/Email)                   │
│  - Enqueue en RabbitMQ                                      │
└────────────────────┬────────────────────────────────────────┘
                     │
        ┌────────────┼────────────┐
        │            │            │
        ▼            ▼            ▼
   ┌─────────┐ ┌─────────┐ ┌──────────┐
   │ TWILIO  │ │ TWILIO  │ │ SENDGRID │
   │WhatsApp │ │   SMS   │ │  EMAIL   │
   └─────────┘ └─────────┘ └──────────┘
        │            │            │
        └────────────┼────────────┘
                     │
                     ▼
             Usuario Final
```

---

## 📊 Matriz Comparativa

| Característica | WhatsApp   | SMS       | Email       |
| -------------- | ---------- | --------- | ----------- |
| **Costo**      | $0.015/msg | $0.05/msg | $0.0001/msg |
| **Velocidad**  | 5-15s      | 1-5s      | 1-10min     |
| **Entrega**    | 99.5%      | 99%       | 98%         |
| **Tipo**       | P2P        | P2P       | Masivo      |
| **Límite/día** | 1,000      | 10,000    | Ilimitado   |

---

## 💻 Stack Técnico

### Backend (.NET 8)

**NotificationService**

- Framework: .NET 8 LTS
- ORM: Entity Framework Core
- Message Queue: RabbitMQ
- APIs: Twilio, SendGrid

**DbContext**

```csharp
DbSet<Notification>
DbSet<NotificationTemplate>
DbSet<NotificationLog>
DbSet<PhoneNumber>
DbSet<EmailTemplate>
```

### Frontend (React 19 + TypeScript)

**Hook personalizado**

```typescript
const useNotifications = (userId: string) => {
  // Fetch notifications
  // Real-time updates con WebSocket
};
```

**Componentes**

- `ContactModal` - Selector de canal
- `NotificationCenter` - Centro de notificaciones
- `TemplateBuilder` - Editor de templates

### Database (PostgreSQL)

**Tablas requeridas:**

- `notifications` - Log de notificaciones enviadas
- `notification_templates` - Templates reutilizables
- `notification_preferences` - Preferencias del usuario
- `phone_numbers` - Números verificados
- `email_addresses` - Emails verificados

---

## 🔐 Seguridad

### Autenticación

- JWT Bearer token en headers
- API keys para Twilio y SendGrid en ConfigMap (Kubernetes)

### Autorización

- Usuario solo puede enviar mensajes a contactos propios
- Admin puede ver todos los logs

### Rate Limiting

- WhatsApp: 1,000 msg/día por dealer
- SMS: 10,000 msg/día por dealer
- Email: Ilimitado

### Validación

- Números de teléfono en E.164 format
- Emails validos con disposición
- Contenido sin spam

---

## 📋 Checklist de Implementación

### Backend

- [ ] NotificationService.Domain (entidades)
- [ ] NotificationService.Application (CQRS)
- [ ] NotificationService.Infrastructure (repos)
- [ ] NotificationService.Api (controllers)
- [ ] Integración con Twilio
- [ ] Integración con SendGrid
- [ ] RabbitMQ listener
- [ ] Tests unitarios (95% coverage)

### Frontend

- [ ] `useNotifications` hook
- [ ] `ContactModal` component
- [ ] `NotificationCenter` page
- [ ] Integraciones API
- [ ] Tests E2E

### DevOps

- [ ] Secrets: TWILIO_ACCOUNT_SID, TWILIO_AUTH_TOKEN
- [ ] Secrets: SENDGRID_API_KEY
- [ ] ConfigMap para templates
- [ ] Health checks

### QA

- [ ] Unit tests
- [ ] Integration tests
- [ ] E2E tests
- [ ] Load testing (1,000 msg/sec)

---

## 📚 Documentos Detallados

Cada API tiene su propia documentación:

1. **[TWILIO_WHATSAPP_API.md](TWILIO_WHATSAPP_API.md)**

   - Endpoints
   - Ejemplos de código
   - Casos de uso
   - Troubleshooting

2. **[TWILIO_SMS_API.md](TWILIO_SMS_API.md)**

   - Endpoints
   - Ejemplos de código
   - Casos de uso
   - Troubleshooting

3. **[SENDGRID_EMAIL_API.md](SENDGRID_EMAIL_API.md)**
   - Endpoints
   - Ejemplos de código
   - Casos de uso
   - Troubleshooting

---

## 🚀 Timeline de Implementación

| Semana | Tarea                             | Estado          |
| ------ | --------------------------------- | --------------- |
| 1      | WhatsApp: Backend + Frontend      | ⏳ Por comenzar |
| 2      | SMS + Email: Backend + Frontend   | ⏳ Por comenzar |
| 3      | Testing E2E y validación          | ⏳ Por comenzar |
| 4      | Deploy a staging y productionizar | ⏳ Por comenzar |

---

## 💰 Costos Estimados (Mensual)

**Baseline (1,000 dealers activos):**

| API       | Costo/msg | Msgs/mes | Costo/mes   |
| --------- | --------- | -------- | ----------- |
| WhatsApp  | $0.015    | 500K     | $7,500      |
| SMS       | $0.05     | 100K     | $5,000      |
| Email     | $0.0001   | 1M       | $100        |
| **TOTAL** | -         | -        | **$12,600** |

**A escala (10,000 dealers):**

- WhatsApp: $75,000/mes
- SMS: $50,000/mes
- Email: $1,000/mes
- **TOTAL: $126,000/mes**

---

## 📞 Soporte y Contacto

| Responsable   | Rol              | Contacto     |
| ------------- | ---------------- | ------------ |
| Backend Team  | Integración APIs | #backend     |
| Frontend Team | UI/UX            | #frontend    |
| DevOps        | Infrastructure   | #devops      |
| PM            | Coordinación     | #engineering |

---

## 📖 Referencias

- **Twilio WhatsApp:** https://www.twilio.com/docs/whatsapp
- **Twilio SMS:** https://www.twilio.com/docs/sms
- **SendGrid:** https://docs.sendgrid.com/api-reference/

---

**Versión:** 1.0  
**Última actualización:** Enero 15, 2026  
**Estado:** ✅ Documentación de Categoría Completada
