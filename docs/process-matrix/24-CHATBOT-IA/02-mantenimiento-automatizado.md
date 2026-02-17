# 🔧 Mantenimiento Automatizado del Chatbot

> **Código:** CHATBOT-002  
> **Versión:** 1.0  
> **Última actualización:** Enero 27, 2026  
> **Criticidad:** 🟡 MEDIA (Eficiencia)  
> **Origen:** Cost optimization best practices  
> **Estado de Implementación:** ✅ Backend 100% | 🔴 UI 0%

---

## 📋 Información General

| Campo              | Valor                                                  |
| ------------------ | ------------------------------------------------------ |
| **Worker Service** | MaintenanceWorkerService (Background Task)             |
| **Scheduling**     | Cron expressions + NCronTab                            |
| **Base de Datos**  | `chatbotservice.maintenance_tasks`                     |
| **Objetivo**       | Reducir costos de Dialogflow en 70-80%                 |
| **Automatización** | Sync inventario, Auto-learning, Reports, Health checks |

---

## 🎯 Objetivo del Proceso

1. **Reducción de Costos:** Mantener costos de IA predecibles y controlados
2. **Mejora Continua:** Auto-aprendizaje basado en preguntas sin respuesta
3. **Actualización Automática:** Sync de inventario sin intervención manual
4. **Monitoreo Proactivo:** Detectar problemas antes que afecten usuarios
5. **Reportes Automáticos:** Insights semanales para toma de decisiones

---

## 📊 Tareas de Mantenimiento

### Resumen de Tareas Automatizadas

| Tarea                      | Frecuencia  | Duración Típica | Prioridad | Estado  |
| -------------------------- | ----------- | --------------- | --------- | ------- |
| **Content/FAQ Sync**       | Cada 60 min | 30-45 seg       | Alta      | ✅ 100% |
| **Auto-Learning Analysis** | Diario 2 AM | 2-3 min         | Media     | ✅ 100% |
| **Cost Report**            | Lunes 8 AM  | 15-20 seg       | Media     | ✅ 100% |
| **Health Monitoring**      | Cada 5 min  | 5 seg           | Alta      | ✅ 100% |
| **Session Cleanup**        | Cada hora   | 10 seg          | Baja      | ✅ 100% |
| **Cache Invalidation**     | Cada 30 min | 2 seg           | Media     | ✅ 100% |

---

## 🏗️ Arquitectura del Worker Service

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    MaintenanceWorkerService                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   Worker Process (Background Service)                                       │
│   ┌────────────────────────────────────────────────────────────────────┐   │
│   │                                                                     │   │
│   │  ExecuteAsync() - Runs every 60 seconds                            │   │
│   │  ┌──────────────────────────────────────────────────────────┐     │   │
│   │  │                                                           │     │   │
│   │  │  1. Load all maintenance tasks from DB                   │     │   │
│   │  │  2. Check if each task is due (based on cron expression) │     │   │
│   │  │  3. Execute due tasks in parallel                        │     │   │
│   │  │  4. Update task status and next run time                 │     │   │
│   │  │  5. Log execution results                                │     │   │
│   │  │  6. Sleep 60 seconds                                     │     │   │
│   │  │                                                           │     │   │
│   │  └──────────────────────────────────────────────────────────┘     │   │
│   │                                                                     │   │
│   └────────────────────────────────────────────────────────────────────┘   │
│                                       │                                      │
│                                       ▼                                      │
│   ┌────────────────────────────────────────────────────────────────────┐   │
│   │                    Task Execution via MediatR                       │   │
│   │  • RunMaintenanceTaskCommand(TaskId, Force, Reason)                │   │
│   │  • Task handler validates configuration                             │   │
│   │  • Executes task-specific logic                                    │   │
│   │  • Returns MaintenanceTaskResult with stats                         │   │
│   └────────────────────────────────────────────────────────────────────┘   │
│                                       │                                      │
│                        ┌──────────────┼──────────────┐                      │
│                        ▼              ▼              ▼                      │
│              ┌────────────┐  ┌────────────┐  ┌────────────┐                │
│              │  Dialogflow│  │ PostgreSQL │  │  RabbitMQ  │                │
│              │   (Intents)│  │   (Tasks)  │  │  (Events)  │                │
│              └────────────┘  └────────────┘  └────────────┘                │
│                                                                              │
│   Cron Expressions (NCronTab)                                               │
│   ┌────────────────────────────────────────────────────────────────────┐   │
│   │  Content/FAQ Sync:    "0 */1 * * *"  (cada hora)                   │   │
│   │  Auto-Learning:       "0 2 * * *"    (diario 2 AM)                 │   │
│   │  Cost Report:         "0 8 * * 1"    (lunes 8 AM)                  │   │
│   │  Health Check:        "*/5 * * * *"  (cada 5 min)                  │   │
│   │  Session Cleanup:     "0 * * * *"    (cada hora)                   │   │
│   └────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Tareas Detalladas

### CHATBOT-MAINT-001: Content/FAQ Sync

**Scheduling:** `0 */1 * * *` (Cada hora en punto)

**Objetivo:** Mantener contenido de ayuda y FAQs sincronizadas con fuentes de conocimiento

**Proceso:**

```
1. Obtener contenido actualizado de diferentes fuentes:

   A) Centro de Ayuda (SupportService - HELP-001):
      GET /api/support/articles?status=published
      GET /api/support/categories
      - Todas las categorías:
        • 🚗 Comprar un Vehículo
        • 💰 Vender tu Vehículo
        • 🏢 Para Dealers
        • 💳 Pagos y Facturación
        • 🔒 Cuenta y Seguridad
        • 🛡️ Confianza y Seguridad ⭐
        • ⚙️ Problemas Técnicos

   B) Sistema de Quejas/Reclamos:
      GET /api/complaints/knowledge-base
      - Preguntas frecuentes sobre:
        • Diferencia queja vs reclamo
        • Cómo crear queja formal
        • Qué documentos adjuntar
        • Cuándo acudir a Pro Consumidor
        • SLA de respuesta (10 días hábiles)

   C) Leyes y Regulaciones (Web Scraping):
      - Pro Consumidor (proconsumidor.gob.do)
      - Ley 358-05 (Protección al Consumidor)
      - DGII (dgii.gov.do) - Verificación RNC

   D) Mejores Prácticas Dealers:
      GET /api/dealer-management/best-practices
      - Tips de publicaciones efectivas
      - Cómo usar analytics
      - Gestión de inventario

2. Compare con tabla ChatbotFAQ:
   - Nuevos artículos → INSERT
   - Artículos actualizados → UPDATE (contenido, categoría)
   - Artículos obsoletos → Soft delete (IsActive = false)

3. Actualizar índices de búsqueda:
   - Categorías disponibles (BuyerHelp, DealerHelp, ConsumerRights)
   - Palabras clave por categoría:
     • "fraude", "estafa" → Confianza y Seguridad
     • "queja", "reclamo" → Sistema de Quejas
     • "pago", "factura" → Pagos y Facturación
   - Sinónimos y variaciones:
     • "engañado" = "estafado" = "fraude"
     • "reporte" = "denuncia" = "queja"
   - Temas frecuentes detectados por ML

4. Si hay cambios significativos (>10 nuevos artículos):
   - Actualizar intents de Dialogflow:
     • reportar_fraude
     • crear_queja
     • derechos_consumidor
     • verificar_dealer
   - Actualizar entities (temas, categorías)
   - Re-entrenar modelo con nuevos ejemplos

5. Log resultado:
   - Artículos nuevos: X (por fuente)
   - Artículos actualizados: Y
   - Artículos removidos: Z
   - Duración: N segundos
```

**Ejemplo de Log:**

```json
{
  "taskId": "abc-123",
  "taskType": "ContentFAQSync",
  "executedAt": "2026-01-27T10:00:00Z",
  "duration": 45,
  "success": true,
  "result": {
    "sources": {
      "helpCenter": {
        "articlesAdded": 8,
        "articlesUpdated": 15,
        "articlesRemoved": 2,
        "totalActive": 387
      },
      "complaintsKB": {
        "articlesAdded": 3,
        "articlesUpdated": 5,
        "totalActive": 42
      },
      "legalContent": {
        "articlesAdded": 2,
        "articlesUpdated": 1,
        "totalActive": 28
      }
    },
    "categories": {
      "buyerHelp": 156,
      "dealerHelp": 124,
      "consumerRights": 107,
      "fraudPrevention": 72
    },
    "dialogflowUpdated": true,
    "intentsUpdated": ["reportar_fraude", "crear_queja", "verificar_dealer"]
  }
}
```

**Casos de Error:**

- `HelpCenterServiceUnavailable`: Retry en 5 minutos
- `DialogflowUpdateFailed`: Log warning, continuar (no crítico)
- `DatabaseError`: Rollback, retry en 10 minutos

---

### CHATBOT-MAINT-002: Auto-Learning Analysis

**Scheduling:** `0 2 * * *` (Diario a las 2:00 AM)

**Objetivo:** Analizar preguntas sin respuesta y generar intents sugeridos automáticamente

**Proceso:**

```
1. Query UnansweredQuestion WHERE IsProcessed = false AND OccurrenceCount > 3
2. Agrupar preguntas similares:
   - Usar similarity scoring (Levenshtein distance)
   - Agrupar si similarity > 80%
3. Para cada grupo (top 20 por OccurrenceCount):
   a) Preparar prompt para Ollama (LLM local):
      "Pregunta frecuente: {question}
       Contexto: Marketplace de vehículos en RD
       Inventario: {marcas_disponibles}
       Generar:
       1. Nombre de intent sugerido
       2. Frases de entrenamiento (5 variaciones)
       3. Respuesta sugerida"

   b) Llamar a Ollama API:
      POST http://host.docker.internal:11434/api/generate
      Model: llama3.2
      Temperature: 0.3 (más determinístico)

   c) Parsear respuesta de Ollama

   d) Validar respuesta generada:
      - Intent name válido (snake_case)
      - Frases de entrenamiento (mín 3)
      - Respuesta no vacía

   e) Guardar en UnansweredQuestion:
      - SuggestedIntentName
      - SuggestedResponse

4. Generar reporte PDF con sugerencias
5. Enviar email al admin con PDF adjunto
6. Publicar evento AutoLearningCompleted en RabbitMQ
```

**Ejemplo de Input/Output de Ollama:**

**Input:**

```json
{
  "model": "llama3.2",
  "prompt": "Pregunta frecuente: ¿Cómo puedo saber si un dealer es confiable?\nContexto: Marketplace de vehículos en República Dominicana\nObjetivo: Proteger consumidores de fraudes\n\nGenera:\n1. Nombre de intent sugerido (snake_case)\n2. 5 frases de entrenamiento variadas\n3. Respuesta profesional y útil que oriente al usuario",
  "temperature": 0.3,
  "max_tokens": 500
}
```

**Output:**

```
Intent sugerido: verificar_dealer_confiable

Frases de entrenamiento:
1. ¿Cómo sé si un dealer es legítimo?
2. Quiero verificar que el vendedor sea confiable
3. ¿Qué debo revisar antes de comprar?
4. Señales de que un dealer es serio
5. Cómo evitar que me estafen al comprar

Respuesta sugerida:
Para verificar si un dealer es confiable, revisa estos puntos:

🟢 **Verificaciones en OKLA:**
1. Badge "Verificado" ✅ en su perfil
2. Reseñas de compradores anteriores
3. RNC registrado y visible
4. Dirección física confirmada

🟡 **Antes de visitar:**
5. Google Maps: Verifica ubicación real
6. Redes sociales: Busca presencia online
7. Llama antes: Confirma disponibilidad

🔴 **Señales de ALERTA:**
⚠️ Precios muy por debajo del mercado
⚠️ Pide pago total por adelantado
⚠️ No permite prueba de manejo
⚠️ Presiona para decidir rápido
⚠️ No tiene ubicación física

📜 **Tus derechos (Ley 358-05):**
• Derecho de retracto (3 días)
• Garantía mínima legal
• Documentos completos al comprar

¿Quieres saber cómo verificar historial del vehículo?
```

**Métricas de Éxito:**

```
Objetivo: Reducir preguntas sin respuesta en 50% mensual
Métrica: (Preguntas procesadas / Total preguntas) × 100
Target: > 80% de sugerencias aprobadas por admin
```

---

### CHATBOT-MAINT-003: Cost Report

**Scheduling:** `0 8 * * 1` (Cada lunes a las 8:00 AM)

**Objetivo:** Generar reporte semanal de costos de Dialogflow y métricas de uso

**Proceso:**

```
1. Recolectar métricas de la semana (lunes-domingo):

   a) Sesiones:
      - Total sesiones iniciadas
      - Promedio interacciones/sesión
      - Duración promedio de sesión
      - Tasa de sesiones completadas vs. expiradas

   b) Interacciones:
      - Total interacciones con Dialogflow
      - Interacciones gratis (de 180/mes)
      - Interacciones pagadas
      - Costo acumulado semanal
      - Proyección mensual

   c) Leads:
      - Total leads generados
      - Breakdown por calidad (Hot/Warm/Cold)
      - Costo por lead (CPL)
      - Tasa de conversión sesión→lead

   d) Intents:
      - Top 10 intents más utilizados
      - Tasa de fallback
      - Intents con baja confianza (<0.6)

   e) Canales:
      - Distribución: web/WhatsApp/Facebook
      - Tasa de engagement por canal

2. Calcular proyecciones:
   - Si continúa al ritmo actual:
     - Interacciones totales del mes
     - Costo total del mes
     - CPL proyectado

3. Generar alertas si:
   - Costo semanal > $50 (rojo)
   - Tasa fallback > 15% (amarillo)
   - CPL > $2 (amarillo)
   - Latencia promedio > 3 seg (amarillo)

4. Crear PDF con gráficos:
   - Chart: Sesiones por día de la semana
   - Chart: Leads por calidad (pie)
   - Chart: Intents más usados (bar)
   - Chart: Costo diario (line)

5. Enviar email al admin/dealer con PDF adjunto
6. Guardar PDF en MediaService: /reports/chatbot/weekly-{date}.pdf
```

**Estructura del Reporte PDF:**

```
┌─────────────────────────────────────────────────────────────┐
│            OKLA Chatbot - Reporte Semanal                   │
│            Semana: 20-26 Enero 2026                          │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  📊 RESUMEN EJECUTIVO                                        │
│  ────────────────────────────────────────────────────────   │
│  Total Sesiones:             234                            │
│  Interacciones Dialogflow:   1,458                          │
│  Leads Generados:            47                             │
│  Costo de la Semana:         $2.56                          │
│  Proyección Mensual:         $11.09                         │
│                                                              │
│  💰 ANÁLISIS DE COSTOS                                       │
│  ────────────────────────────────────────────────────────   │
│  Interacciones Gratis Usadas:    180 / 180 (100%)           │
│  Interacciones Pagadas:          1,278 × $0.002 = $2.56     │
│  CPL (Costo Por Lead):           $2.56 / 47 = $0.054        │
│  ✅ Bajo objetivo de <$1.00/lead                            │
│                                                              │
│  🎯 LEADS GENERADOS                                          │
│  ────────────────────────────────────────────────────────   │
│  🔥 Hot Leads (score >80):     12 (25.5%)                   │
│  🟠 Warm Leads (50-80):        23 (48.9%)                   │
│  🔵 Cold Leads (<50):          12 (25.5%)                   │
│                                                              │
│  📈 RENDIMIENTO                                              │
│  ────────────────────────────────────────────────────────   │
│  Tasa de Conversión:           20.1% (47/234)               │
│  Interacciones/Sesión:         6.2 (promedio)               │
│  Duración Promedio:            4.3 minutos                  │
│  Tasa de Fallback:             8.7% ✅                      │
│  Latencia Promedio:            1.8 seg ✅                   │
│                                                              │
│  🚀 TOP 5 INTENTS MÁS USADOS                                 │
│  ────────────────────────────────────────────────────────   │
│  1. buscar_vehiculo           234 (16%)                     │
│  2. detalles_vehiculo         187 (12.8%)                   │
│  3. solicitar_contacto        98 (6.7%)                     │
│  4. financiamiento            76 (5.2%)                     │
│  5. agendar_visita            54 (3.7%)                     │
│                                                              │
│  📱 DISTRIBUCIÓN POR CANAL                                   │
│  ────────────────────────────────────────────────────────   │
│  Web:              189 (80.8%)                              │
│  WhatsApp:         32 (13.7%)                               │
│  Facebook:         13 (5.6%)                                │
│                                                              │
│  ⚠️ RECOMENDACIONES                                          │
│  ────────────────────────────────────────────────────────   │
│  • Excelente desempeño esta semana ✅                       │
│  • CPL muy bajo ($0.054), objetivo alcanzado               │
│  • Considerar aumentar límite global para captar más leads  │
│  • Revisar 3 preguntas sin respuesta acumuladas            │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

### CHATBOT-MAINT-004: Health Monitoring

**Scheduling:** `*/5 * * * *` (Cada 5 minutos)

**Objetivo:** Monitorear salud del chatbot y detectar problemas proactivamente

**Proceso:**

```
1. Ping Dialogflow:
   - Hacer request de prueba con timeout de 5 seg
   - Si falla: Incrementar contador de errores
   - Si >3 fallos consecutivos: Alerta crítica

2. Medir latencia:
   - Promediar últimas 10 requests a Dialogflow
   - Si promedio > 3 seg: Alerta de latencia
   - Si > 5 seg: Alerta crítica

3. Verificar límites:
   - Query InteractionUsage del día actual
   - Calcular % del límite global usado
   - Si >80%: Alerta warning
   - Si >90%: Alerta critical
   - Si =100%: Desactivar chatbot temporalmente

4. Verificar tasas de error:
   - Calcular % de sesiones expiradas (sin EndedAt)
   - Si >20%: Alerta (usuarios abandonan)
   - Calcular % de fallbacks consecutivos (>3)
   - Si >15%: Alerta (bot no entiende)

5. Verificar base de datos:
   - Ping PostgreSQL
   - Si falla: Alerta crítica

6. Generar HealthReport:
   {
     "timestamp": "...",
     "overallStatus": "Healthy/Degraded/Critical",
     "dialogflow": {
       "isAvailable": true,
       "latencyMs": 1234,
       "errorRate": 0.02
     },
     "interactions": {
       "todayCount": 1234,
       "limitPercent": 41.3,
       "limitStatus": "OK"
     },
     "sessions": {
       "activeCount": 23,
       "expiredRate": 0.08
     },
     "alerts": []
   }

7. Si hay alertas:
   - Enviar notificación push al admin
   - Enviar SMS si es crítico
   - Log en ErrorService
```

**Tipos de Alertas:**

| Alert Code            | Severity | Trigger                     | Action                  |
| --------------------- | -------- | --------------------------- | ----------------------- |
| `DIALOGFLOW_DOWN`     | 🔴 High  | 3 fallos consecutivos       | Desactivar chatbot      |
| `LATENCY_HIGH`        | 🟡 Med   | Latencia > 3 seg            | Investigar              |
| `LIMIT_NEAR`          | 🟡 Med   | 80% del límite global       | Notificar admin         |
| `LIMIT_REACHED`       | 🔴 High  | 100% del límite             | Desactivar hasta mañana |
| `FALLBACK_HIGH`       | 🟡 Med   | Tasa fallback > 15%         | Revisar intents         |
| `DB_UNAVAILABLE`      | 🔴 High  | PostgreSQL no responde      | Reintentar, escalar     |
| `SESSION_EXPIRY_HIGH` | 🟡 Med   | >20% sesiones sin finalizar | Revisar timeout config  |

---

### CHATBOT-MAINT-005: Session Cleanup

**Scheduling:** `0 * * * *` (Cada hora en punto)

**Objetivo:** Limpiar sesiones expiradas y liberar recursos

**Proceso:**

```
1. Query ChatSession WHERE:
   - Status = 'Active'
   - LastActivityAt < NOW() - 30 minutes
   - EndedAt IS NULL

2. Para cada sesión expirada:
   - Cambiar Status = 'Expired'
   - SetEndedAt = NOW()
   - Calcular SessionDurationSeconds

3. Query ChatSession WHERE:
   - Status IN ('Ended', 'Expired', 'Transferred')
   - EndedAt < NOW() - 90 days

4. Soft delete sesiones antiguas:
   - No eliminar registro (mantener histórico)
   - Pero marcar como archivado para excluir de queries

5. Invalidar cache de Redis:
   - Remover sessions keys expiradas
   - Remover user quotas caducadas (>24h)

6. Log resultado:
   - Sesiones expiradas: X
   - Sesiones archivadas: Y
   - Cache keys removidas: Z
```

---

### CHATBOT-MAINT-006: Cache Invalidation

**Scheduling:** `*/30 * * * *` (Cada 30 minutos)

**Objetivo:** Mantener cache de Redis sincronizado

**Proceso:**

```
1. Invalidar cache de vehículos:
   - Remover keys: chatbot:vehicles:{configId}
   - Solo si hubo sync reciente

2. Invalidar cache de quick responses:
   - Remover keys: chatbot:quick_responses:{configId}
   - Solo si hubo cambios en configuración

3. Actualizar counters de quotas:
   - Recalcular interacciones del día por usuario
   - Actualizar Redis: user:{userId}:interactions:today

4. Limpiar locks stale:
   - Remover locks de Redis >10 minutos
   - Formato: lock:maintenance:{taskId}
```

---

## 📊 Métricas de Mantenimiento

### KPIs de las Tareas

| Tarea              | SLA Duración | SLA Success Rate | Actual Avg | Actual Success |
| ------------------ | ------------ | ---------------- | ---------- | -------------- |
| Inventory Sync     | < 60 seg     | > 99%            | 35 seg     | 99.8%          |
| Auto-Learning      | < 5 min      | > 90%            | 2.5 min    | 95%            |
| Cost Report        | < 30 seg     | > 99%            | 18 seg     | 100%           |
| Health Monitoring  | < 10 seg     | > 99.9%          | 4 seg      | 99.9%          |
| Session Cleanup    | < 15 seg     | > 99%            | 8 seg      | 100%           |
| Cache Invalidation | < 5 seg      | > 99%            | 2 seg      | 100%           |

---

## 🔧 Configuración del Worker

### appsettings.json

```json
{
  "Maintenance": {
    "EnableAutomatedTasks": true,
    "CheckIntervalSeconds": 60,
    "TaskTimeoutMinutes": 10,
    "ConcurrentTaskLimit": 3,
    "RetryPolicy": {
      "MaxRetries": 3,
      "RetryDelaySeconds": 30
    }
  },
  "InventorySync": {
    "CronExpression": "0 */1 * * *",
    "VehicleServiceUrl": "http://vehiclessaleservice:8080",
    "BatchSize": 100,
    "UpdateDialogflowOnChange": true
  },
  "AutoLearning": {
    "CronExpression": "0 2 * * *",
    "OllamaUrl": "http://host.docker.internal:11434",
    "Model": "llama3.2",
    "Temperature": 0.3,
    "MaxTokens": 500,
    "MinOccurrenceCount": 3,
    "TopQuestionsToProcess": 20
  },
  "CostReport": {
    "CronExpression": "0 8 * * 1",
    "SendToEmails": ["admin@okla.com.do"],
    "IncludePdf": true,
    "AlertThresholds": {
      "WeeklyCostUsd": 50.0,
      "FallbackRate": 0.15,
      "CostPerLead": 2.0
    }
  },
  "HealthMonitoring": {
    "CronExpression": "*/5 * * * *",
    "PingTimeoutSeconds": 5,
    "LatencyThresholdMs": 3000,
    "LatencyCriticalMs": 5000,
    "GlobalLimitWarningPercent": 80,
    "GlobalLimitCriticalPercent": 90
  }
}
```

---

## 🚨 Manejo de Errores

### Retry Logic

```csharp
// Polly retry policy para tareas críticas
var retryPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (exception, timespan, retryCount, context) =>
        {
            _logger.LogWarning(
                "Retry {RetryCount} after {TimeSpan}ms due to {Exception}",
                retryCount, timespan.TotalMilliseconds, exception.Message);
        });
```

### Circuit Breaker

```csharp
// Circuit breaker para Dialogflow
var circuitBreakerPolicy = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromMinutes(1),
        onBreak: (exception, duration) =>
        {
            _logger.LogError("Circuit breaker opened for {Duration}", duration);
            // Desactivar chatbot temporalmente
        },
        onReset: () =>
        {
            _logger.LogInformation("Circuit breaker reset");
            // Reactivar chatbot
        });
```

---

## 📈 Monitoreo y Observabilidad

### Logs Estructurados

```json
{
  "timestamp": "2026-01-27T10:00:00Z",
  "level": "Information",
  "service": "ChatbotService",
  "component": "MaintenanceWorker",
  "taskType": "InventorySync",
  "taskId": "abc-123",
  "duration": 32456,
  "success": true,
  "result": {
    "vehiclesAdded": 5,
    "vehiclesUpdated": 12,
    "vehiclesRemoved": 3
  }
}
```

### Métricas Prometheus

```
# Duración de tareas
chatbot_maintenance_task_duration_seconds{task="inventory_sync"} 32.456

# Éxito/Fallo
chatbot_maintenance_task_success_total{task="inventory_sync"} 1
chatbot_maintenance_task_failure_total{task="inventory_sync"} 0

# Interacciones
chatbot_interactions_total{config="global"} 1458
chatbot_interactions_cost_usd{config="global"} 2.56
```

---

## 🎯 Objetivo de Reducción de Costos

### Estrategia de Ahorro

```
SIN MANTENIMIENTO AUTOMATIZADO:
──────────────────────────────────────────────────────────
• Inventario desactualizado → Usuarios frustrados
• Preguntas sin respuesta → Más fallbacks
• Fallbacks → Más interacciones para obtener respuesta
• Más interacciones → Mayor costo

Ejemplo: Usuario pregunta por Honda Civic 2022
  1. "Busco Honda Civic 2022" (1 interacción)
  2. "No tenemos ese modelo" (fallback)
  3. "¿Y Honda Civic 2021?" (1 interacción)
  4. "Déjame buscar..." (fallback)
  5. Transferencia a agente (frustracion)

Total: 2 interacciones + transferencia = Usuario insatisfecho

CON MANTENIMIENTO AUTOMATIZADO:
──────────────────────────────────────────────────────────
• Inventario sincronizado → Respuestas precisas
• Auto-learning → Menos fallbacks
• Respuestas directas → Menos interacciones

Ejemplo mejorado:
  1. "Busco Honda Civic 2022" (1 interacción)
  2. "Tenemos 3 Honda Civic 2022 disponibles:
      • $25,000 - 15,000 km - Santo Domingo
      • $26,500 - 8,000 km - Santiago
      • $24,000 - 22,000 km - La Vega"
  3. Usuario satisfecho, procede a agendar visita

Total: 1 interacción = Usuario satisfecho + Lead generado

AHORRO ESTIMADO:
──────────────────────────────────────────────────────────
Sin automatización: ~150,000 interacciones/mes
  • 180 gratis + 149,820 × $0.002 = $299.64/mes

Con automatización: ~100,000 interacciones/mes
  • 180 gratis + 99,820 × $0.002 = $199.64/mes

Ahorro mensual: $100/mes (33%)
Ahorro anual: $1,200/año

Factores de ahorro:
• ✅ Sync inventario → -20% interacciones redundantes
• ✅ Auto-learning → -15% fallbacks
• ✅ Quick responses → -10% consultas simples
• ✅ Health monitoring → -5% errores técnicos
────────────────────────
TOTAL: ~50% menos interacciones = ~50% ahorro de costos
```

---

## 🚀 Próximos Pasos

### Mejoras Planificadas

- [ ] **Machine Learning para predicción de límites**
  - Predecir cuándo se alcanzará límite global
  - Ajustar límites dinámicamente según demanda
- [ ] **Auto-scaling de workers**
  - Si hay muchas tareas pendientes, escalar workers
  - Usar Kubernetes HPA (Horizontal Pod Autoscaler)

- [ ] **Dashboard en tiempo real**
  - Ver estado de tareas en vivo
  - Ejecutar tareas manualmente con un click
  - Ver logs de ejecución

- [ ] **Notificaciones más granulares**
  - Slack integration para alertas
  - Telegram bot para notificaciones admin
  - Dashboard de alertas en UI admin

- [ ] **Optimización de auto-learning**
  - Usar modelos más avanzados (GPT-4, Claude)
  - Fine-tuning de Ollama con datos de OKLA
  - A/B testing de respuestas generadas

---

**Última actualización:** Enero 27, 2026  
**Documentado por:** Sistema de Documentación Automática  
**Revisado por:** Equipo de Arquitectura OKLA
