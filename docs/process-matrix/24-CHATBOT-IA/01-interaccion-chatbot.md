# 🤖 Interacción con Chatbot IA

> **Código:** CHATBOT-001  
> **Versión:** 1.0  
> **Última actualización:** Enero 27, 2026  
> **Criticidad:** 🟢 MEDIA (Automatización)  
> **Origen:** WhatsApp Business, Cars.com, Facebook Messenger  
> **Estado de Implementación:** ✅ Backend 100% | 🔴 UI 0%

---

## ✅ AUDITORÍA DE ACCESO UI (Enero 27, 2026)

> **Estado:** 🔴 SERVICIO SIN UI - Backend completo, frontend pendiente.

| Proceso                   | Backend | UI Access | Observación                    |
| ------------------------- | ------- | --------- | ------------------------------ |
| Iniciar sesión chatbot    | ✅ 100% | 🔴 0%     | API lista, widget pendiente    |
| Enviar mensaje            | ✅ 100% | 🔴 0%     | Dialogflow integrado           |
| Ver historial             | ✅ 100% | 🔴 0%     | Endpoint funcional             |
| Transferir a agente       | ✅ 100% | 🔴 0%     | Lógica implementada            |
| Multi-canal (WhatsApp/FB) | ✅ 100% | 🔴 0%     | Webhooks configurados          |
| Límites de interacciones  | ✅ 100% | 🔴 0%     | Sistema de quotas funcionando  |
| Lead qualification        | ✅ 100% | 🔴 0%     | Hot/Warm/Cold automático       |
| Auto-learning             | ✅ 100% | 🔴 0%     | Análisis de preguntas sin resp |

### Rutas UI Pendientes 🔴

- 🔴 `/chatbot` - Widget de chatbot en homepage
- 🔴 `/vehicles/:slug?chat=true` - Chatbot en detalle de vehículo
- 🔴 `/admin/chatbot/config` - Configuración de chatbot (admin)
- 🔴 `/dealer/chatbot/leads` - Leads generados por chatbot
- 🔴 `/dealer/chatbot/analytics` - Estadísticas de conversaciones
- 🔴 `/dealer/chatbot/unanswered` - Preguntas sin respuesta

**Verificación Backend:** ChatbotService existe en `/backend/ChatbotService/` ✅

---

## 📊 Resumen de Implementación

| Componente          | Total | Implementado | Pendiente | Estado  |
| ------------------- | ----- | ------------ | --------- | ------- |
| Controllers         | 4     | 4            | 0         | ✅ 100% |
| CHATBOT-SESSION-\*  | 4     | 4            | 0         | ✅ 100% |
| CHATBOT-MSG-\*      | 3     | 3            | 0         | ✅ 100% |
| CHATBOT-LEAD-\*     | 5     | 5            | 0         | ✅ 100% |
| CHATBOT-CONFIG-\*   | 3     | 3            | 0         | ✅ 100% |
| CHATBOT-MAINT-\*    | 6     | 6            | 0         | ✅ 100% |
| CHATBOT-LEARN-\*    | 3     | 3            | 0         | ✅ 100% |
| Multi-canal (WA/FB) | 2     | 2            | 0         | ✅ 100% |
| Widget Frontend     | 1     | 0            | 1         | 🔴 0%   |
| Admin Panel         | 1     | 0            | 1         | 🔴 0%   |
| Dealer Dashboard    | 1     | 0            | 1         | 🔴 0%   |
| Tests               | 15    | 15           | 0         | ✅ 100% |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 📋 Información General

| Campo                           | Valor                                                            |
| ------------------------------- | ---------------------------------------------------------------- |
| **Servicio**                    | ChatbotService (NUEVO - Enero 2026)                              |
| **Puerto**                      | 5094                                                             |
| **Base de Datos**               | `chatbotservice`                                                 |
| **Tecnología IA**               | Google Dialogflow ES (Standard Plan)                             |
| **Modelo de Pricing**           | 180 gratis/mes + $0.002 por interacción adicional                |
| **Dependencias**                | UserService, VehiclesSaleService, DealerManagementService        |
| **Canales Soportados**          | Web, WhatsApp Business, Facebook Messenger, Instagram Direct     |
| **Límites de Interacciones**    | 10/sesión, 50/usuario/día, 100,000/mes global                    |
| **Objetivo de Ahorro de Costo** | Reducir 70-80% de costos de IA mediante mantenimiento automático |
| **Enfoque**                     | Soporte y orientación (NO ventas) - Chatbot imparcial            |
| **Integración Soporte**         | Conoce TODO: Centro Ayuda + Tickets + Quejas/Reclamos            |
| **Base de Conocimiento**        | 19-SOPORTE/01-centro-ayuda.md + 19-SOPORTE/02-quejas-reclamos.md |

---

## 🎯 Objetivo del Proceso

1. **Soporte 24/7:** Ayudar a usuarios y dealers a usar la plataforma OKLA correctamente
2. **Orientación Imparcial:** Educar sobre derechos del consumidor y mejores prácticas
3. **Experto en Sistema de Soporte:** Conoce TODO el contenido de Centro de Ayuda + Quejas/Reclamos
4. **Prevención de Fraudes:** Sugerencias proactivas para compras seguras
5. **Guía de Reportes:** Orienta cómo reportar problemas en la plataforma (tickets, quejas, fraudes)
6. **Multi-canal:** Un solo chatbot para web, WhatsApp, Facebook
7. **Cost-Efficiency:** Limitar interacciones para controlar costos de Dialogflow
8. **Auto-Learning:** Mejorar respuestas automáticamente basado en preguntas sin respuesta

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      ChatbotService Architecture                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   Usuarios                          ChatbotService                           │
│   ┌────────────────┐              ┌─────────────────────────────────────┐   │
│   │ Web Widget     │──┐           │         Controllers                 │   │
│   │ (React)        │  │           │  • ChatController (sesiones/msgs)   │   │
│   └────────────────┘  │           │  • ConfigurationController          │   │
│   ┌────────────────┐  │           │  • LeadsController                  │   │
│   │ WhatsApp       │  │           │  • MaintenanceController            │   │
│   │ Business       │──┼──────────▶│                                     │   │
│   └────────────────┘  │           │         Dialogflow Service          │   │
│   ┌────────────────┐  │           │  ┌───────────────────────────────┐  │   │
│   │ Facebook       │  │           │  │ • DetectIntent                │  │   │
│   │ Messenger      │──┘           │  │ • Polly Retry (3x)            │  │   │
│   └────────────────┘              │  │ • Circuit Breaker             │  │   │
│                                   │  └───────────────────────────────┘  │   │
│                                   │         Límites Service             │   │
│                                   │  ┌───────────────────────────────┐  │   │
│                                   │  │ • 10/sesión                   │  │   │
│                                   │  │ • 50/usuario/día              │  │   │
│                                   │  │ • 100K/mes global             │  │   │
│                                   │  └───────────────────────────────┘  │   │
│                                   │      Lead Scoring Service           │   │
│                                   │  ┌───────────────────────────────┐  │   │
│                                   │  │ • Hot: >80 score              │  │   │
│                                   │  │ • Warm: 50-80 score           │  │   │
│                                   │  │ • Cold: <50 score             │  │   │
│                                   │  └───────────────────────────────┘  │   │
│                                   └─────────────────────────────────────┘   │
│                                                    │                        │
│                                    ┌───────────────┼───────────────┐        │
│                                    ▼               ▼               ▼        │
│                  ┌────────────────────┐  ┌───────────────┐  ┌────────────┐ │
│                  │ Google Dialogflow  │  │   PostgreSQL  │  │  RabbitMQ  │ │
│                  │ ES (Intent Det.)   │  │  (Sessions,   │  │  (Events)  │ │
│                  │ $0.002/interaction │  │   Leads)      │  │            │ │
│                  └────────────────────┘  └───────────────┘  └────────────┘ │
│                                                                              │
│   Background Tasks (MaintenanceWorkerService)                               │
│   ┌────────────────────────────────────────────────────────────────────┐   │
│   │ • Inventory Sync (cada 60 min)         • Health Monitoring (5 min) │   │
│   │ • Auto-Learning Analysis (diario)      • Cost Optimization          │   │
│   │ • Reports Generation (semanal)         • Expired Sessions Cleanup   │   │
│   └────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Endpoints

### ChatController

| Método | Endpoint                    | Descripción                      | Auth |
| ------ | --------------------------- | -------------------------------- | ---- |
| `POST` | `/api/chat/start`           | Iniciar sesión de chatbot        | ❌   |
| `POST` | `/api/chat/message`         | Enviar mensaje al chatbot        | ❌   |
| `POST` | `/api/chat/end`             | Terminar sesión                  | ❌   |
| `POST` | `/api/chat/transfer-agent`  | Transferir a agente humano       | ❌   |
| `GET`  | `/api/chat/session/{token}` | Obtener información de sesión    | ❌   |
| `GET`  | `/api/chat/messages`        | Historial de mensajes por sesión | ❌   |

### ConfigurationController

| Método | Endpoint                                    | Descripción              | Auth      |
| ------ | ------------------------------------------- | ------------------------ | --------- |
| `GET`  | `/api/configuration/{id}`                   | Configuración de chatbot | ✅ Admin  |
| `GET`  | `/api/configuration/dealer/{dealerId}`      | Config por dealer        | ✅ Dealer |
| `GET`  | `/api/configuration/{id}/quick-responses`   | Respuestas rápidas       | ✅ Admin  |
| `GET`  | `/api/configuration/{id}/vehicles`          | Vehículos disponibles    | ❌        |
| `GET`  | `/api/configuration/{id}/vehicles/search`   | Buscar vehículos         | ❌        |
| `GET`  | `/api/configuration/{id}/vehicles/featured` | Vehículos destacados     | ❌        |

### LeadsController

| Método | Endpoint                      | Descripción               | Auth      |
| ------ | ----------------------------- | ------------------------- | --------- |
| `GET`  | `/api/leads/{id}`             | Detalle de lead           | ✅ Dealer |
| `GET`  | `/api/leads/status/{status}`  | Leads por status          | ✅ Dealer |
| `GET`  | `/api/leads/unassigned`       | Leads sin asignar         | ✅ Admin  |
| `GET`  | `/api/leads/session/{token}`  | Lead de sesión específica | ✅ Dealer |
| `PUT`  | `/api/leads/{id}`             | Actualizar lead           | ✅ Dealer |
| `GET`  | `/api/leads/statistics/today` | Estadísticas del día      | ✅ Dealer |

### MaintenanceController

| Método | Endpoint                                   | Descripción                    | Auth     |
| ------ | ------------------------------------------ | ------------------------------ | -------- |
| `GET`  | `/api/maintenance/{configId}/tasks`        | Tareas de mantenimiento        | ✅ Admin |
| `POST` | `/api/maintenance/tasks/{taskId}/run`      | Ejecutar tarea manualmente     | ✅ Admin |
| `PUT`  | `/api/maintenance/tasks/{taskId}/toggle`   | Habilitar/deshabilitar tarea   | ✅ Admin |
| `GET`  | `/api/maintenance/health/{configId}`       | Reporte de salud del chatbot   | ✅ Admin |
| `GET`  | `/api/maintenance/alerts/{configId}`       | Alertas activas                | ✅ Admin |
| `POST` | `/api/maintenance/sync/{configId}`         | Sincronizar inventario         | ✅ Admin |
| `POST` | `/api/maintenance/learning/{configId}`     | Ejecutar auto-learning         | ✅ Admin |
| `POST` | `/api/maintenance/reports/{configId}`      | Generar reporte                | ✅ Admin |
| `GET`  | `/api/maintenance/costs/{configId}`        | Análisis de costos             | ✅ Admin |
| `GET`  | `/api/maintenance/unanswered/{configId}`   | Preguntas sin respuesta        | ✅ Admin |
| `POST` | `/api/maintenance/unanswered/{id}/process` | Marcar pregunta como procesada | ✅ Admin |

---

## 🗃️ Entidades Principales

### ChatSession

| Campo                       | Tipo      | Descripción                                               |
| --------------------------- | --------- | --------------------------------------------------------- |
| `Id`                        | Guid      | ID único de sesión                                        |
| `SessionToken`              | string    | Token para identificar sesión                             |
| `ChatbotConfigurationId`    | Guid      | Configuración del chatbot                                 |
| `UserId`                    | Guid?     | Usuario (null si anónimo)                                 |
| `UserName`                  | string?   | Nombre del usuario                                        |
| `UserEmail`                 | string?   | Email del usuario                                         |
| `UserPhone`                 | string?   | Teléfono del usuario                                      |
| `SessionType`               | enum      | Anonymous, Authenticated                                  |
| `Channel`                   | string    | web, whatsapp, facebook, instagram                        |
| `Status`                    | enum      | Active, Ended, Transferred, Expired                       |
| `MessageCount`              | int       | Total de mensajes                                         |
| `InteractionCount`          | int       | Interacciones con Dialogflow                              |
| `MaxInteractionsPerSession` | int       | Límite de interacciones (default: 10)                     |
| `InteractionLimitReached`   | bool      | Si alcanzó el límite                                      |
| `TopicCategory`             | string?   | Tema de consulta (buyer_help/dealer_help/consumer_rights) |
| `HelpfulnessRating`         | int?      | Calificación del usuario (1-5)                            |
| `CreatedAt`                 | DateTime  | Fecha de inicio                                           |
| `LastActivityAt`            | DateTime  | Última actividad                                          |
| `EndedAt`                   | DateTime? | Fecha de fin                                              |

### ChatMessage

| Campo                  | Tipo     | Descripción                             |
| ---------------------- | -------- | --------------------------------------- |
| `Id`                   | Guid     | ID único del mensaje                    |
| `SessionId`            | Guid     | FK a ChatSession                        |
| `MessageType`          | enum     | UserMessage, BotResponse, SystemMessage |
| `Content`              | string   | Contenido del mensaje                   |
| `DialogflowIntentName` | string?  | Intent detectado (si aplica)            |
| `DialogflowConfidence` | decimal? | Confianza de Dialogflow (0-1)           |
| `CountsAsInteraction`  | bool     | Si cuenta para límite de interacciones  |
| `VehicleId`            | Guid?    | Vehículo mencionado (si aplica)         |
| `CreatedAt`            | DateTime | Timestamp del mensaje                   |

### ChatLead

| Campo                    | Tipo      | Descripción                          |
| ------------------------ | --------- | ------------------------------------ |
| `Id`                     | Guid      | ID único del lead                    |
| `SessionId`              | Guid      | FK a ChatSession                     |
| `DealerId`               | Guid?     | Dealer asignado                      |
| `UserName`               | string    | Nombre completo                      |
| `UserEmail`              | string?   | Email de contacto                    |
| `UserPhone`              | string?   | Teléfono de contacto                 |
| `InterestedVehicleId`    | Guid?     | Vehículo de interés                  |
| `InterestedVehicleName`  | string?   | Nombre del vehículo                  |
| `BudgetMin`              | decimal?  | Presupuesto mínimo                   |
| `BudgetMax`              | decimal?  | Presupuesto máximo                   |
| `PreferredContactMethod` | enum      | Email, Phone, WhatsApp               |
| `LeadScore`              | int       | Puntaje de calificación (0-100)      |
| `LeadQuality`            | enum      | Hot, Warm, Cold                      |
| `Status`                 | enum      | New, Contacted, Qualified, Converted |
| `AssignedAt`             | DateTime? | Fecha de asignación                  |
| `CreatedAt`              | DateTime  | Fecha de generación                  |

### ChatbotConfiguration

| Campo                            | Tipo   | Descripción                               |
| -------------------------------- | ------ | ----------------------------------------- |
| `Id`                             | Guid   | ID de configuración                       |
| `DealerId`                       | Guid?  | Dealer (null = configuración global)      |
| `Name`                           | string | Nombre del chatbot                        |
| `IsActive`                       | bool   | Si está activo                            |
| `DialogflowProjectId`            | string | ID del proyecto Dialogflow                |
| `Plan`                           | enum   | Free (180/mes), Standard ($0.002)         |
| `MaxInteractionsPerSession`      | int    | Límite por sesión (default: 10)           |
| `MaxInteractionsPerUserPerDay`   | int    | Límite por usuario/día (default: 50)      |
| `MaxInteractionsPerUserPerMonth` | int    | Límite por usuario/mes (default: 500)     |
| `MaxGlobalInteractionsPerMonth`  | int    | Límite global/mes (default: 100,000)      |
| `LimitReachedMessage`            | string | Mensaje cuando alcanza límite             |
| `TransferToAgentOnLimit`         | bool   | Si transfiere a agente al alcanzar límite |
| `BotName`                        | string | Nombre del bot                            |
| `WelcomeMessage`                 | string | Mensaje de bienvenida                     |
| `EnableWebChat`                  | bool   | Habilitar chat web                        |
| `EnableWhatsApp`                 | bool   | Habilitar WhatsApp Business               |
| `EnableFacebook`                 | bool   | Habilitar Facebook Messenger              |
| `EnableAutoInventorySync`        | bool   | Auto-sincronizar inventario               |
| `InventorySyncIntervalMinutes`   | int    | Intervalo de sync (default: 60)           |
| `EnableAutoReports`              | bool   | Generar reportes automáticos              |
| `EnableAutoLearning`             | bool   | Habilitar auto-aprendizaje                |
| `EnableHealthMonitoring`         | bool   | Monitoreo de salud                        |

### MaintenanceTask

| Campo                  | Tipo      | Descripción                               |
| ---------------------- | --------- | ----------------------------------------- |
| `Id`                   | Guid      | ID de tarea                               |
| `ConfigurationId`      | Guid      | FK a ChatbotConfiguration                 |
| `TaskType`             | enum      | InventorySync, AutoLearning, Reports, etc |
| `Name`                 | string    | Nombre de la tarea                        |
| `CronExpression`       | string    | Expresión cron para scheduling            |
| `IsEnabled`            | bool      | Si está habilitada                        |
| `Status`               | enum      | Idle, Running, Success, Failed            |
| `LastRunAt`            | DateTime? | Última ejecución                          |
| `NextRunAt`            | DateTime? | Próxima ejecución                         |
| `TotalExecutions`      | int       | Total de ejecuciones                      |
| `SuccessfulExecutions` | int       | Ejecuciones exitosas                      |
| `AvgExecutionTimeMs`   | long      | Tiempo promedio de ejecución (ms)         |

### UnansweredQuestion

| Campo                 | Tipo     | Descripción                   |
| --------------------- | -------- | ----------------------------- |
| `Id`                  | Guid     | ID de pregunta                |
| `ConfigurationId`     | Guid     | FK a ChatbotConfiguration     |
| `Question`            | string   | Pregunta sin respuesta        |
| `OccurrenceCount`     | int      | Cuántas veces se preguntó     |
| `FirstAskedAt`        | DateTime | Primera vez preguntada        |
| `LastAskedAt`         | DateTime | Última vez preguntada         |
| `AttemptedIntentName` | string?  | Intent que intentó Dialogflow |
| `AttemptedConfidence` | decimal? | Confianza del intent (baja)   |
| `SuggestedIntentName` | string?  | Intent sugerido por IA        |
| `SuggestedResponse`   | string?  | Respuesta sugerida por IA     |
| `IsProcessed`         | bool     | Si ya fue procesada por admin |
| `ProcessedBy`         | string?  | Usuario que la procesó        |

---

## 🔄 Flujos de Proceso

### CHATBOT-SESSION-001: Iniciar Sesión de Chatbot

**Input:** StartSessionCommand

- `UserId` (opcional)
- `UserName`, `UserEmail`, `UserPhone` (opcional)
- `SessionType` (Anonymous/Authenticated)
- `Channel` (web/whatsapp/facebook)
- `UserAgent`, `IpAddress`

**Proceso:**

1. Validar configuración del chatbot activa
2. Verificar límites globales de interacciones (día/mes)
3. Si usuario autenticado, verificar límites por usuario (día/mes)
4. Generar `SessionToken` único
5. Crear registro en `ChatSession` con status=Active
6. Registrar evento `ChatSessionStarted` en RabbitMQ
7. Enviar mensaje de bienvenida automático
8. Retornar `SessionToken` y mensaje de bienvenida

**Output:** StartSessionResult

- `SessionToken`
- `WelcomeMessage`
- `MaxInteractionsRemaining`

**Casos de Error:**

- `ChatbotNotActive`: Si chatbot está desactivado
- `GlobalLimitReached`: Si alcanzó límite global del día/mes
- `UserLimitReached`: Si usuario alcanzó límite diario/mensual

---

### CHATBOT-MSG-001: Enviar Mensaje al Chatbot

**Input:** SendMessageCommand

- `SessionToken`
- `Message` (texto del mensaje)
- `MessageType` (UserMessage/SystemMessage)
- `MediaUrl` (opcional, para imágenes/videos)

**Proceso:**

1. Validar sesión existe y está activa
2. Verificar que no haya alcanzado límite de interacciones por sesión
3. Si no alcanzó límite: Llamar a Dialogflow DetectIntent
   - Retry automático 3x con backoff exponencial (Polly)
   - Circuit breaker si Dialogflow está caído
4. Incrementar `InteractionCount` si fue interacción válida
5. Parsear respuesta de Dialogflow:
   - Si detecta intent de "reportar_problema" → Crear ticket de soporte
   - Si detecta intent de "derechos_consumidor" → Mostrar info legal RD
   - Si detecta intent de "como_publicar" → Mostrar tutorial paso a paso
   - Si detecta intent de "fuera de alcance" → Incrementar contador fallback
6. Guardar mensaje del usuario en `ChatMessage`
7. Guardar respuesta del bot en `ChatMessage`
8. Actualizar `LastActivityAt` de la sesión
9. Si alcanzó límite: Mostrar mensaje de límite alcanzado
   - Opcionalmente transferir a agente humano
10. Publicar evento `MessageSent` en RabbitMQ

**Output:** SendMessageResult

- `BotResponse` (texto)
- `IntentName` (intent detectado)
- `Confidence` (0-1)
- `InteractionsRemaining` (cuántas quedan)
- `LimitReached` (bool)
- `SuggestedActions` (botones, quick replies)
- `TutorialSteps` (si es guía paso a paso)
- `ResourceLinks` (links a FAQs, videos tutoriales)

**Casos de Error:**

- `SessionNotFound`: Token inválido
- `SessionExpired`: Sesión expirada (timeout)
- `InteractionLimitReached`: Ya alcanzó límite
- `DialogflowUnavailable`: Servicio de IA caído

---

### CHATBOT-MSG-002: Transferir a Agente Humano

**Input:** TransferToAgentCommand

- `SessionToken`
- `Reason` (opcional: "ComplexQuery", "LimitReached", "UserRequest")
- `PreferredAgentId` (opcional)

**Proceso:**

1. Validar sesión existe
2. Cambiar status de sesión a `Transferred`
3. Registrar timestamp de transferencia
4. Si hay lead asociado, asignar a agente
5. Notificar a agente disponible via RabbitMQ
6. Enviar email/SMS al agente con resumen de conversación
7. Actualizar estadísticas de transferencias

**Output:** TransferToAgentResult

- `Success` (bool)
- `AgentName` (nombre del agente asignado)
- `EstimatedWaitTimeMinutes` (tiempo de espera)
- `Message` (confirmación para el usuario)

---

### CHATBOT-TICKET-001: Crear Ticket de Soporte (Integración HELP-002)

**Trigger:**

- Dialogflow detecta intent de "reportar_fraude", "reportar_problema_tecnico", "crear_queja"
- Usuario explícitamente pide "crear ticket"
- Chatbot determina que problema no se puede resolver automáticamente

**Proceso:**

1. **Recolectar información conversacional:**
   - Nombre, email, teléfono (de sesión o mensajes)
   - Descripción del problema (extraída de conversación)
   - Categoría detectada automáticamente:
     ```
     "me estafaron" → FraudReport
     "no puedo pagar" → PaymentProblem
     "la app no carga" → TechnicalSupport
     "mi cuenta no funciona" → AccountIssue
     ```
   - Contexto relevante:
     - VehicleId (si mencionó vehículo)
     - DealerId (si mencionó vendedor)
     - OrderId (si mencionó compra)

2. **Buscar en Centro de Ayuda primero (HELP-001):**

   ```http
   GET /api/support/articles/search?q={problema}
   ```

   - Si encuentra artículo relevante:

     ```
     "Encontré esta guía que puede ayudarte:
      📚 {ArticleTitle}
      {ArticleSummary}

      ¿Esto resuelve tu problema?
      [Sí, gracias] [No, necesito más ayuda]"
     ```

   - Si usuario dice "Sí" → Marcar sesión como resolved_by_bot
   - Si usuario dice "No" → Continuar a crear ticket

3. **Calcular Priority automáticamente:**

   ```
   Priority = f(keywords, category, user_history)

   Urgent (respuesta <2h):
     • Contiene: "fraude", "estafa", "robo", "hackeo"
     • Category = FraudReport
     • Monto involucrado > $50,000

   High (respuesta <4h):
     • Contiene: "no puedo", "error", "no funciona"
     • Category = TechnicalSupport, PaymentProblem
     • Usuario es dealer con plan Pro/Enterprise

   Medium (respuesta <24h):
     • Preguntas sobre funcionalidades
     • Category = ListingHelp, AccountIssue

   Low (respuesta <48h):
     • Consultas generales
     • Category = GeneralQuestion, FeatureRequest
   ```

4. **Solicitar evidencia (si aplica):**

   ```
   Para procesar tu reporte de fraude, necesito evidencia:

   📸 Puedes adjuntar:
     • Capturas de conversaciones
     • Fotos del vehículo
     • Recibos de pago
     • Contratos firmados

   [Adjuntar archivos]
   [Continuar sin archivos]
   ```

   - Si usuario adjunta → Upload a MediaService
   - Guardar URLs de archivos para incluir en ticket

5. **Crear ticket vía API:**

   ```http
   POST /api/support/tickets
   Authorization: Bearer {chatbot_token}

   {
     "userId": "{userId}",
     "category": "FraudReport",
     "priority": "Urgent",
     "subject": "Reporte de fraude - {vehicleName}",
     "description": "Usuario reporta: {extracted_from_conversation}",
     "attachmentUrls": ["s3://...1", "s3://...2"],
     "relatedVehicleId": "{vehicleId}",
     "relatedDealerId": "{dealerId}",
     "context": {
       "source": "Chatbot",
       "sessionId": "{sessionId}",
       "conversationSummary": "...",
       "userAgent": "...",
       "referrer": "..."
     }
   }
   ```

6. **Respuesta del API:**

   ```json
   {
     "success": true,
     "ticket": {
       "id": "ticket-12345",
       "ticketNumber": "OKLA-T-2026-00042",
       "status": "New",
       "priority": "Urgent",
       "assignedTo": "team-fraud-investigation",
       "firstResponseDue": "2026-01-27T14:00:00Z"
     }
   }
   ```

7. **Crear también ChatSupportTicket interno:**
   - Registro en base de datos del ChatbotService
   - Permite tracking de tickets creados por chatbot
   - Usado para métricas de efectividad

8. **Confirmar al usuario con información útil:**

   ```
   ✅ Ticket creado exitosamente

   📋 Número de ticket: OKLA-T-2026-00042
   ⏰ Tiempo estimado de respuesta: 2 horas
   👤 Asignado a: Equipo de Fraudes

   📧 Te enviaremos notificaciones por email cuando:
     • Un agente revise tu caso
     • Necesitemos información adicional
     • Tu problema sea resuelto

   💡 Mientras tanto:

   🛡️ Tus derechos como consumidor (Ley 358-05):
     • Derecho a información veraz
     • Derecho de retracto (3 días)
     • Derecho a garantía
     • Derecho a indemnización si hay daño

   🏛️ Si no se resuelve en 10 días, puedes acudir a:
     • Pro Consumidor: 809-567-7397
     • Dirección: Tiradentes esq. Constitución
     • Web: proconsumidor.gob.do

   🔍 Ver estado de tu ticket:
     • Web: okla.com.do/help/tickets
     • O pregúntame: "estado de mi ticket"
   ```

9. **Publicar evento para otros servicios:**

   ```csharp
   await _messageBus.PublishAsync(new TicketCreatedByBotEvent
   {
       TicketId = ticket.Id,
       TicketNumber = ticket.TicketNumber,
       UserId = session.UserId,
       Category = "FraudReport",
       Priority = "Urgent",
       CreatedByBot = true,
       SessionId = session.Id,
       ConversationSummary = conversationSummary
   });
   ```

10. **Actualizar métricas internas:**
    ```
    support_tickets_created_by_bot_total{category="FraudReport"} ++
    support_bot_ticket_creation_success_rate ++
    support_bot_avg_time_to_create_ticket_seconds = X
    ```

**Output:**

- SupportTicket creado en SupportService (HELP-002)
- ChatSupportTicket creado en ChatbotService
- Usuario notificado con número de ticket
- Equipo de soporte notificado
- Audit trail completo

---

### CHATBOT-MAINT-001: Sincronización de Contenido de Ayuda

**Scheduling:** Cada 60 minutos (configurable)

**Proceso:**

1. Obtener contenido actualizado de diferentes fuentes:
   - Artículos del Centro de Ayuda (desde HelpCenterService)
   - Tutoriales y guías (desde ContentService)
   - Leyes y regulaciones actualizadas (Pro Consumidor, DGII)
   - Mejores prácticas para dealers (desde DealerManagementService)
2. Para cada configuración de chatbot:
   - Si es dealer específico: FAQs de ese dealer + FAQs generales
   - Si es global: Todas las FAQs
3. Actualizar tabla `ChatbotFAQ`:
   - Insertar nuevas preguntas/respuestas
   - Actualizar respuestas existentes
   - Marcar como obsoletas FAQs antiguas
4. Generar índice de búsqueda para Dialogflow:
   - Categorías de ayuda disponibles
   - Palabras clave por categoría
   - Sinónimos y variaciones
5. Actualizar intents de Dialogflow con nuevo contenido
6. Registrar log de sync con estadísticas:
   - FAQs nuevas
   - FAQs actualizadas
   - FAQs removidas
   - Duración de sync

---

### CHATBOT-MAINT-002: Auto-Learning de Preguntas Sin Respuesta

**Scheduling:** Diario a las 2:00 AM

**Proceso:**

1. Obtener preguntas sin respuesta (confidence < 0.4)
2. Agrupar preguntas similares usando similarity scoring
3. Para cada grupo de preguntas frecuentes (>5 ocurrencias):
   - Usar Ollama (LLM local) para generar intent sugerido
   - Generar respuesta sugerida basada en documentación existente
   - Crear registro en tabla `UnansweredQuestion`
4. Notificar al equipo de soporte sobre preguntas frecuentes sin respuesta
5. Generar reporte semanal con:
   - Top 10 preguntas sin respuesta
   - Intent sugeridos por IA
   - Respuestas sugeridas
   - Acción recomendada: Crear nuevo intent o mejorar existente

**Ejemplo de Análisis:**

```
Pregunta frecuente (8 ocurrencias):
  "¿Cómo puedo saber si un dealer es confiable?"
  "¿Qué debo verificar antes de comprar?"
  "¿Cómo evito que me estafen?"

Intent Sugerido (Ollama llama3.2):
  verificar_dealer_confiable

Respuesta Sugerida:
  "Para verificar si un dealer es confiable:
   1. Verifica el badge 'Verificado' en su perfil
   2. Lee reseñas de otros compradores
   3. Confirma que tiene RNC registrado
   4. Visita su ubicación física antes de comprar
   5. Solicita historial del vehículo (CARFAX)
   6. Verifica documentos del vehículo (matrícula, título)"

Acción Recomendada:
  ✅ Crear nuevo intent en Dialogflow
  ✅ Agregar a sección 'Protección al Consumidor'
```

- Generar respuesta sugerida basada en:
  - FAQs de la industria automotriz
  - Documentación de OKLA
  - Datos del inventario actual

4. Guardar sugerencias en `UnansweredQuestion`
5. Generar reporte para admin con top 20 preguntas
6. Enviar notificación al admin para revisión
7. Si admin aprueba: Crear nuevo intent en Dialogflow automáticamente

**Output:** Reporte de auto-learning

- Total de preguntas analizadas
- Grupos de preguntas identificados
- Intents sugeridos
- Respuestas sugeridas

---

### CHATBOT-MAINT-003: Health Monitoring

**Scheduling:** Cada 5 minutos

**Proceso:**

1. Verificar conectividad con Dialogflow
2. Medir latencia de respuesta (debe ser < 2 segundos)
3. Verificar límites de interacciones:
   - Alertar si alcanzó 80% del límite diario
   - Alertar si alcanzó 90% del límite mensual
4. Verificar tasas de error:
   - Sesiones expiradas sin actividad (> 30 min)
   - Fallbacks consecutivos (> 3)
   - Errores de Dialogflow (> 5%)
5. Calcular métricas clave:
   - Promedio de interacciones por sesión
   - Tasa de conversión a lead
   - Tiempo promedio de sesión
6. Si detecta problemas críticos:
   - Enviar alerta al admin
   - Opcionalmente desactivar chatbot temporalmente
   - Registrar en `HealthAlert`

---

### CHATBOT-MAINT-004: Generación de Reportes

**Scheduling:** Semanal (lunes 8:00 AM)

**Proceso:**

1. Recolectar métricas de la semana:
   - Total de sesiones iniciadas
   - Total de mensajes enviados
   - Total de interacciones con Dialogflow
   - Leads generados (Hot/Warm/Cold)
   - Tasa de transferencia a agente
   - Preguntas sin respuesta (top 20)
2. Calcular costos:
   - Interacciones totales
   - Interacciones gratis usadas (180/mes)
   - Interacciones pagadas ($0.002 c/u)
   - Costo total de Dialogflow
   - Proyección de costo mensual
3. Generar gráficos:
   - Sesiones por día
   - Leads por calidad
   - Intents más utilizados
   - Canales más activos (web/WhatsApp/FB)
4. Enviar reporte por email al admin/dealer
5. Guardar PDF en MediaService para histórico

**Output:** CostAnalysisReport

- `TotalInteractions`
- `FreeInteractionsUsed`
- `PaidInteractions`
- `TotalCost`
- `ProjectedMonthlyCost`
- `LeadsGenerated`
- `ConversionRate`

---

## 📊 Límites de Interacciones (Cost Control)

### Sistema de Quotas

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        LÍMITES DE INTERACCIONES                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  Nivel 1: POR SESIÓN                                                         │
│  ┌────────────────────────────────────────────────────────────────────┐     │
│  │  Límite: 10 interacciones por sesión                               │     │
│  │  Objetivo: Evitar conversaciones infinitas sin valor               │     │
│  │  Acción: Transferir a agente humano o terminar sesión             │     │
│  └────────────────────────────────────────────────────────────────────┘     │
│                                                                              │
│  Nivel 2: POR USUARIO/DÍA                                                    │
│  ┌────────────────────────────────────────────────────────────────────┐     │
│  │  Límite: 50 interacciones por usuario por día                     │     │
│  │  Objetivo: Evitar abuso de un solo usuario                        │     │
│  │  Acción: Bloquear nuevas sesiones por 24 horas                    │     │
│  └────────────────────────────────────────────────────────────────────┘     │
│                                                                              │
│  Nivel 3: POR USUARIO/MES                                                    │
│  ┌────────────────────────────────────────────────────────────────────┐     │
│  │  Límite: 500 interacciones por usuario por mes                    │     │
│  │  Objetivo: Control de uso excesivo                                │     │
│  │  Acción: Bloquear hasta próximo mes                               │     │
│  └────────────────────────────────────────────────────────────────────┘     │
│                                                                              │
│  Nivel 4: GLOBAL/MES                                                         │
│  ┌────────────────────────────────────────────────────────────────────┐     │
│  │  Límite: 100,000 interacciones globales por mes                   │     │
│  │  Objetivo: Control de presupuesto mensual de Dialogflow           │     │
│  │  Acción: Desactivar chatbot temporalmente si excede               │     │
│  └────────────────────────────────────────────────────────────────────┘     │
│                                                                              │
│  Cálculo de Costo Mensual:                                                   │
│  ────────────────────────────────────────────────────────────────────       │
│  • Primeras 180 interacciones: GRATIS (Dialogflow ES Free Tier)             │
│  • Interacciones 181 en adelante: $0.002 c/u                                 │
│                                                                              │
│  Ejemplo con 100,000 interacciones/mes:                                      │
│  • Gratis: 180 interacciones = $0                                            │
│  • Pagadas: 99,820 interacciones × $0.002 = $199.64/mes                      │
│                                                                              │
│  Proyección de Ahorro con Límites:                                           │
│  • Sin límites: ~150,000 interacciones = $299.64/mes                         │
│  • Con límites: ~100,000 interacciones = $199.64/mes                         │
│  • Ahorro: 33% ($100/mes)                                                    │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

### Verificación de Límites (Algoritmo)

```csharp
public async Task<bool> CheckInteractionLimitsAsync(
    Guid sessionId,
    Guid? userId,
    CancellationToken ct)
{
    var session = await _sessionRepo.GetByIdAsync(sessionId, ct);

    // Nivel 1: Por sesión (10 interacciones)
    if (session.InteractionCount >= session.MaxInteractionsPerSession)
    {
        session.InteractionLimitReached = true;
        await _sessionRepo.UpdateAsync(session, ct);
        return false; // Límite alcanzado
    }

    // Nivel 2: Por usuario/día (50 interacciones)
    if (userId.HasValue)
    {
        var todayCount = await _usageRepo.GetTodayInteractionsAsync(
            session.ConfigurationId,
            userId.Value,
            ct);

        var config = await _configRepo.GetByIdAsync(session.ConfigurationId, ct);
        if (todayCount >= config.MaxInteractionsPerUserPerDay)
        {
            return false; // Límite diario alcanzado
        }
    }

    // Nivel 3: Por usuario/mes (500 interacciones)
    if (userId.HasValue)
    {
        var monthCount = await _usageRepo.GetMonthInteractionsAsync(
            session.ConfigurationId,
            userId.Value,
            ct);

        var config = await _configRepo.GetByIdAsync(session.ConfigurationId, ct);
        if (monthCount >= config.MaxInteractionsPerUserPerMonth)
        {
            return false; // Límite mensual alcanzado
        }
    }

    // Nivel 4: Global/día (calculado de 100K/mes = ~3,333/día)
    var globalToday = await _usageRepo.GetTodayGlobalInteractionsAsync(
        session.ConfigurationId,
        ct);

    var config = await _configRepo.GetByIdAsync(session.ConfigurationId, ct);
    var dailyLimit = config.MaxGlobalInteractionsPerMonth / 30; // ~3,333/día
    if (globalToday >= dailyLimit)
    {
        return false; // Límite global diario alcanzado
    }

    return true; // OK para continuar
}
```

---

## 📊 Métricas de Calidad de Soporte

### Sistema de Evaluación de Ayuda

```
SupportQualityScore = Σ (Factor × Peso)

Factores:
┌──────────────────────────────────────────────────────────────┐
│ Factor                        │ Peso │ Condición           │
├──────────────────────────────────────────────────────────────┤
│ Usuario calificó como útil    │  +40 │ rating >= 4         │
│ Problema resuelto sin agente  │  +30 │ resolvedByBot=true  │
│ Usuario completó tutorial     │  +20 │ tutorialCompleted   │
│ Tiempo de respuesta < 2 seg   │  +10 │ latency < 2s        │
│ Sin fallbacks en conversación │  +10 │ fallbackCount = 0   │
│ Usuario regresó a usar chatbot│  +5  │ returningUser=true  │
└──────────────────────────────────────────────────────────────┘

Clasificación de Sesiones:
  Excelente:  score > 80  → Respuesta perfecta, usuario satisfecho
  Buena:      score 50-80 → Usuario ayudado adecuadamente
  Regular:    score < 50  → Necesita mejorar respuestas o escaló a agente
```

**Categorías de Soporte:**

```
🟢 BUYER_HELP (Ayuda a Compradores):
   • Cómo buscar vehículos
   • Cómo contactar vendedor
   • Derechos del consumidor
   • Señales de alerta de estafas
   • Proceso de compra seguro
   • Cómo verificar historial de vehículo

🔵 DEALER_HELP (Ayuda a Dealers):
   • Cómo crear publicación efectiva
   • Mejores prácticas de fotos
   • Cómo usar analytics dashboard
   • Cómo responder consultas rápido
   • Tips para aumentar visibilidad
   • Gestión de inventario

🟡 CONSUMER_RIGHTS (Derechos del Consumidor):
   • Ley 358-05 (Protección Consumidor)
   • Derecho de retracto (3 días)
   • Cómo reportar fraude
   • Pro Consumidor: cuándo acudir
   • Documentos requeridos para compra
   • Garantías legales en RD
```

---

## 📈 Métricas y Analytics

### KPIs del Chatbot

| Métrica                           | Objetivo     | Cálculo                                        |
| --------------------------------- | ------------ | ---------------------------------------------- |
| **Tasa de Resolución sin Agente** | > 70%        | (Resueltos por bot / Total sesiones) × 100     |
| **Sesiones por día**              | 50-100       | COUNT(sessions WHERE date = today)             |
| **Promedio interacciones/sesión** | 5-8          | AVG(interaction_count)                         |
| **Tasa de transferencia**         | < 15%        | (Sesiones transferidas / Total sesiones) × 100 |
| **Tiempo promedio de sesión**     | 3-5 minutos  | AVG(session_duration)                          |
| **Fallback rate**                 | < 10%        | (Fallback intents / Total intents) × 100       |
| **Satisfacción del usuario**      | > 4.0/5.0    | AVG(helpfulness_rating)                        |
| **Latencia de respuesta**         | < 2 segundos | AVG(dialogflow_response_time)                  |
| **Uptime del chatbot**            | > 99%        | (Minutos activo / Total minutos) × 100         |

### Dashboard para Admin/Dealer

**Sección 1: Resumen Ejecutivo**

- Total de sesiones hoy/semana/mes
- Leads generados (Hot/Warm/Cold)
- Costo de Dialogflow (día/mes)
- Interacciones restantes (límite global)

**Sección 2: Gráficos**

- Sesiones por hora del día (línea)
- Leads por calidad (pie chart)
- Intents más usados (barra)
- Canales más activos (barra: web/WhatsApp/FB)

**Sección 3: Alertas**

- 🔴 Límite global alcanzado 90%
- 🟡 Tasa de fallback > 10%
- 🟡 Latencia > 3 segundos
- 🔴 Dialogflow service down

**Sección 4: Preguntas sin Respuesta**

- Top 20 preguntas más frecuentes
- Sugerencias de auto-learning
- Botón "Crear Intent" para aprobar

---

## 🔗 Integraciones

### SupportService (Centro de Ayuda + Quejas/Reclamos)

**Base de Conocimiento Completa:**

```
Chatbot conoce TODO de:
├── 📚 Centro de Ayuda (HELP-001)
│   ├── 🚗 Comprar un Vehículo
│   │   ├── Cómo buscar vehículos
│   │   ├── Filtros de búsqueda
│   │   ├── Contactar al vendedor
│   │   └── Proceso de compra
│   ├── 💰 Vender tu Vehículo
│   ├── 🏢 Para Dealers
│   ├── 💳 Pagos y Facturación
│   ├── 🔒 Cuenta y Seguridad
│   ├── 🛡️ Confianza y Seguridad
│   │   ├── Consejos para evitar fraudes ⭐
│   │   ├── Reportar un problema ⭐
│   │   ├── Garantía OKLA
│   │   └── Vendedores verificados
│   └── ⚙️ Problemas Técnicos
│
├── 🚨 Sistema de Tickets (HELP-002)
│   ├── Categorías:
│   │   ├── AccountIssue
│   │   ├── PaymentProblem
│   │   ├── FraudReport ⭐
│   │   ├── TechnicalSupport
│   │   └── GeneralQuestion
│   ├── Prioridades:
│   │   ├── Urgent (fraudes, seguridad)
│   │   ├── High (técnicos)
│   │   ├── Medium (ayuda)
│   │   └── Low (general)
│   └── Flujo: Crear → Asignar → Responder → Resolver
│
└── 📋 Quejas y Reclamos (COMPLAINT-001, COMPLAINT-002)
    ├── Queja: Inconformidad con servicio
    ├── Reclamo: Exigir cumplimiento de lo pactado
    ├── Estados: New → UnderReview → Resolved → Closed
    └── SLA: 10 días hábiles para resolver
```

**API Endpoints que Chatbot usa:**

```http
# Búsqueda en Centro de Ayuda
GET /api/support/articles/search?q={query}
GET /api/support/articles/{slug}
GET /api/support/categories

# Crear Ticket de Soporte
POST /api/support/tickets
{
  "category": "FraudReport",
  "priority": "Urgent",
  "subject": "Vendedor me estafó con vehículo",
  "description": "...",
  "relatedVehicleId": "vehicle-123",
  "relatedDealerId": "dealer-456"
}

# Ver mis tickets
GET /api/support/tickets?userId={userId}
GET /api/support/tickets/{ticketId}

# Crear Queja/Reclamo
POST /api/complaints
{
  "type": "Complaint",
  "category": "ServiceQuality",
  "description": "...",
  "evidenceUrls": ["s3://..."]
}
```

**Flujo de Orientación del Chatbot:**

```
Usuario: "Me estafaron con un vehículo"
    ↓
Chatbot detecta intent: reportar_fraude
    ↓
Chatbot pregunta:
  1. ¿Qué pasó exactamente?
  2. ¿Tienes evidencia? (fotos, mensajes, recibos)
  3. ¿Ya contactaste al vendedor?
  4. ¿Cuánto dinero pagaste?
    ↓
Chatbot evalúa severidad:
  • SI es fraude confirmado → Crear ticket URGENT + Guiar a Pro Consumidor
  • SI es malentendido → Sugerir contactar vendedor primero
  • SI es defecto menor → Guiar a crear queja formal
    ↓
Chatbot guía paso a paso:
  1. "Te voy a ayudar a crear un reporte formal"
  2. "Necesito que adjuntes evidencia (capturas, fotos)"
  3. "Creando ticket #OKLA-T-2026-00001..."
  4. "Ticket creado. Equipo responderá en <2 horas"
  5. "Mientras tanto, conoce tus derechos (Ley 358-05)..."
  6. "Si el problema no se resuelve, puedes acudir a Pro Consumidor"
    ↓
Chatbot crea:
  • SupportTicket (HELP-002)
  • O Complaint (COMPLAINT-001)
  • Notifica al equipo
  • Registra en audit trail
```

**Sugerencias Proactivas:**

Cuando usuario pregunta sobre un vehículo, chatbot sugiere:

```
💡 TIPS PARA COMPRA SEGURA:

✅ Antes de visitar:
  • Verifica badge "Verificado" del dealer
  • Lee reseñas de otros compradores
  • Confirma RNC en DGII
  • Búscalo en Google Maps

✅ Durante la visita:
  • Lleva un mecánico de confianza
  • Prueba TODAS las funciones
  • Revisa documentos (matrícula, título)
  • Pide historial del vehículo (CARFAX)

⚠️ SEÑALES DE ALERTA:
  • Precio muy por debajo del mercado
  • Presiona para decidir rápido
  • Pide pago total por adelantado
  • No permite inspección mecánica
  • No tiene ubicación física
  • Documentos incompletos

🚨 SI ALGO SALE MAL:
  Puedo ayudarte a reportar el problema aquí mismo.
  Usa el comando "reportar fraude" o haz clic aquí.
```

### Dialogflow ES

| Configuración    | Valor                              |
| ---------------- | ---------------------------------- |
| **Agent ID**     | okla-automotive-es                 |
| **Language**     | es (Español)                       |
| **Region**       | us-central1                        |
| **API Version**  | v2                                 |
| **Timeout**      | 10 segundos                        |
| **Retry Policy** | 3 intentos con backoff exponencial |

**Intents Principales:**

- `saludar` - Saludo inicial
- `buscar_vehiculo` - Búsqueda por marca/modelo/precio
- `detalles_vehiculo` - Información específica de vehículo
- `agendar_visita` - Solicitar test drive/visita
- `solicitar_contacto` - Generar lead
- `financiamiento` - Información de financiamiento
- `trade_in` - Información de trade-in
- `garantia` - Información de garantías
- `fallback` - Respuesta por defecto

### WhatsApp Business API

| Configuración       | Valor                                            |
| ------------------- | ------------------------------------------------ |
| **Phone Number ID** | Configurado por dealer                           |
| **Webhook URL**     | https://api.okla.com.do/chatbot/webhook/whatsapp |
| **Verify Token**    | Almacenado en ChatbotConfiguration               |
| **Message Format**  | JSON                                             |

**Flujo de Webhook:**

1. WhatsApp envía mensaje a webhook
2. ChatbotService valida verify token
3. Extrae mensaje y sender
4. Crea/actualiza sesión con channel="whatsapp"
5. Procesa mensaje normalmente
6. Envía respuesta vía WhatsApp Business API

### Facebook Messenger

| Configuración    | Valor                                            |
| ---------------- | ------------------------------------------------ |
| **Page ID**      | Configurado por dealer                           |
| **Access Token** | Almacenado encriptado en config                  |
| **Webhook URL**  | https://api.okla.com.do/chatbot/webhook/facebook |
| **Events**       | messages, messaging_postbacks                    |

---

## 🛡️ Seguridad y Privacidad

### Datos Encriptados

- `DialogflowCredentialsJson` (AES-256)
- `WhatsAppAccessToken` (AES-256)
- `FacebookAccessToken` (AES-256)

### Retención de Datos

- Sesiones: 90 días
- Mensajes: 90 días
- Leads: Permanente
- Logs de mantenimiento: 30 días

### GDPR/ARCO Compliance

- Usuario puede solicitar eliminar todas sus conversaciones
- Endpoint: `DELETE /api/chat/user/{userId}/data`
- Anonimización de datos después de 90 días

---

## � Referencias a Documentación de Soporte

**Este chatbot conoce y utiliza TODO el contenido de:**

1. **[19-SOPORTE/01-centro-ayuda.md](../19-SOPORTE/01-centro-ayuda.md)**
   - Todas las categorías del Centro de Ayuda
   - FAQs completas (HELP-001)
   - Sistema de tickets (HELP-002)
   - Respuestas de agentes (HELP-003)

2. **[19-SOPORTE/02-quejas-reclamos.md](../19-SOPORTE/02-quejas-reclamos.md)**
   - Diferencia queja vs reclamo
   - Proceso COMPLAINT-001 (crear queja)
   - Proceso COMPLAINT-002 (resolver queja)
   - SLA de 10 días hábiles
   - Escalación a Pro Consumidor

**Integraciones API usadas:**

```http
# Centro de Ayuda
GET /api/support/articles/search?q={query}
GET /api/support/categories

# Tickets
POST /api/support/tickets
GET /api/support/tickets/{id}

# Quejas
POST /api/complaints
GET /api/complaints/{id}
```

---

## 🚀 Próximos Pasos (Roadmap)

### UI Web (Prioridad Alta) 🔴

- [ ] Widget flotante de chatbot en homepage
- [ ] Panel de chat en detalle de vehículo
- [ ] Dashboard de tickets creados (usuario/dealer)
- [ ] Panel de configuración (admin)
- [ ] Analytics dashboard (admin/dealer)
- [ ] Integración visual con Centro de Ayuda

### Mejoras de IA (Prioridad Media) 🟡

- [ ] Sentiment analysis de mensajes
- [ ] Multi-idioma (inglés, francés-criollo)
- [ ] Detección automática de fraudes por patrones
- [ ] Sugerencias proactivas mejoradas con ML
- [ ] Vector search en base de conocimiento

### Integraciones (Prioridad Baja) 🟢

- [ ] Instagram Direct Messages
- [ ] Telegram Bot
- [ ] Apple Business Chat
- [ ] Google Business Messages

---

**Última actualización:** Enero 27, 2026  
**Documentado por:** Sistema de Documentación Automática  
**Revisado por:** Equipo de Arquitectura OKLA
