# 🚨 Regulatory Alerts - Alertas Regulatorias - Matriz de Procesos

> **Servicio:** ComplianceService / AlertModule  
> **Puerto:** 5027  
> **Última actualización:** Enero 21, 2026  
> **Estado:** 🟢 ACTIVO

---

## 📊 Resumen de Implementación

| Componente    | Total | Implementado | Pendiente | Estado |
| ------------- | ----- | ------------ | --------- | ------ |
| Controllers   | 1     | 0            | 1         | 🔴     |
| REG-MON-\*    | 5     | 0            | 5         | 🔴     |
| REG-ALERT-\*  | 4     | 0            | 4         | 🔴     |
| REG-SCRAPE-\* | 4     | 0            | 4         | 🔴     |
| REG-NOTIF-\*  | 3     | 0            | 3         | 🔴     |
| Tests         | 0     | 0            | 12        | 🔴     |

**Leyenda:** ✅ Implementado + Tested | 🟢 Implementado | 🟡 En Progreso | 🔴 Pendiente

---

## 1. Información General

### 1.1 Descripción

Sistema de alertas para monitorear cambios regulatorios en República Dominicana que afecten las operaciones de OKLA. Monitorea fuentes oficiales (DGII, DGCP, Superintendencia de Bancos, Pro Consumidor) y genera alertas automáticas al equipo de compliance.

### 1.2 Fuentes Monitoreadas

| Fuente             | URL                  | Frecuencia | Contenido             |
| ------------------ | -------------------- | ---------- | --------------------- |
| **DGII**           | dgii.gov.do          | Diario     | Impuestos, RNC        |
| **Pro Consumidor** | proconsumidor.gob.do | Diario     | Protección consumidor |
| **SB**             | sb.gob.do            | Semanal    | Regulación financiera |
| **DGCP**           | dgcp.gob.do          | Semanal    | Compras públicas      |
| **INDOTEL**        | indotel.gob.do       | Semanal    | Telecomunicaciones    |
| **AIRD**           | aird.org.do          | Mensual    | Industria             |

### 1.3 Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Regulatory Alerts Architecture                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   External Sources                                                      │
│   ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐     │
│   │   DGII   │ │   Pro    │ │   SB     │ │   DGCP   │ │ INDOTEL  │     │
│   │          │ │Consumidor│ │          │ │          │ │          │     │
│   └─────┬────┘ └─────┬────┘ └─────┬────┘ └─────┬────┘ └─────┬────┘     │
│         │            │            │            │            │           │
│         └────────────┴────────────┴────────────┴────────────┘           │
│                                   │                                      │
│                                   ▼                                      │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                    Source Crawler Jobs                           │   │
│   │   (Hangfire scheduled jobs - scraping & RSS parsing)            │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│                                   ▼                                      │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                    Document Processor                            │   │
│   │                                                                   │   │
│   │   1. Extract text from PDFs/HTML                                │   │
│   │   2. Classify by category (tax, consumer, finance)              │   │
│   │   3. Detect keywords relevant to OKLA                           │   │
│   │   4. Calculate impact score                                      │   │
│   │   5. Generate summary (optional: AI-assisted)                   │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│                                   ▼                                      │
│   ┌─────────────────────────────────────────────────────────────────┐   │
│   │                    Alert Engine                                  │   │
│   │                                                                   │   │
│   │   If impactScore >= threshold:                                  │   │
│   │     - Create RegulatoryAlert                                    │   │
│   │     - Notify Compliance team                                    │   │
│   │     - Schedule follow-up tasks                                  │   │
│   └───────────────────────────────┬─────────────────────────────────┘   │
│                                   │                                      │
│              ┌────────────────────┼────────────────────┐                │
│              │                    │                    │                 │
│              ▼                    ▼                    ▼                 │
│   ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐        │
│   │   PostgreSQL     │ │  Notifications   │ │    Teams/Slack   │        │
│   │   (Alerts DB)    │ │   (Email/SMS)    │ │   (Compliance)   │        │
│   └──────────────────┘ └──────────────────┘ └──────────────────┘        │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Endpoints

### 2.1 Alertas Regulatorias

| Método | Endpoint                                  | Descripción          | Auth       |
| ------ | ----------------------------------------- | -------------------- | ---------- |
| `GET`  | `/api/regulatory-alerts`                  | Listar alertas       | Compliance |
| `GET`  | `/api/regulatory-alerts/{id}`             | Detalle de alerta    | Compliance |
| `POST` | `/api/regulatory-alerts/{id}/acknowledge` | Acusar recibo        | Compliance |
| `POST` | `/api/regulatory-alerts/{id}/assign`      | Asignar responsable  | Admin      |
| `PUT`  | `/api/regulatory-alerts/{id}/status`      | Actualizar estado    | Compliance |
| `POST` | `/api/regulatory-alerts/{id}/action-plan` | Crear plan de acción | Compliance |

### 2.2 Fuentes

| Método | Endpoint                                  | Descripción       | Auth       |
| ------ | ----------------------------------------- | ----------------- | ---------- |
| `GET`  | `/api/regulatory-sources`                 | Listar fuentes    | Compliance |
| `POST` | `/api/regulatory-sources`                 | Agregar fuente    | Admin      |
| `PUT`  | `/api/regulatory-sources/{id}`            | Actualizar fuente | Admin      |
| `POST` | `/api/regulatory-sources/{id}/force-scan` | Escaneo manual    | Admin      |

### 2.3 Keywords y Configuración

| Método   | Endpoint                               | Descripción       | Auth       |
| -------- | -------------------------------------- | ----------------- | ---------- |
| `GET`    | `/api/regulatory-alerts/keywords`      | Listar keywords   | Compliance |
| `POST`   | `/api/regulatory-alerts/keywords`      | Agregar keyword   | Admin      |
| `DELETE` | `/api/regulatory-alerts/keywords/{id}` | Eliminar keyword  | Admin      |
| `GET`    | `/api/regulatory-alerts/config`        | Obtener config    | Admin      |
| `PUT`    | `/api/regulatory-alerts/config`        | Actualizar config | Admin      |

---

## 3. Entidades

### 3.1 RegulatoryAlert

```csharp
public class RegulatoryAlert
{
    public Guid Id { get; set; }

    // Source
    public Guid SourceId { get; set; }
    public RegulatorySource Source { get; set; } = null!;
    public string SourceUrl { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }

    // Classification
    public AlertCategory Category { get; set; }
    public AlertSeverity Severity { get; set; }
    public int ImpactScore { get; set; } // 1-100

    // Content
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string FullText { get; set; } = string.Empty;
    public List<string> MatchedKeywords { get; set; } = new();
    public string? DocumentUrl { get; set; }

    // Processing
    public AlertStatus Status { get; set; }
    public DateTime DetectedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedById { get; set; }
    public Guid? AssignedToId { get; set; }

    // Action
    public DateTime? ActionDeadline { get; set; }
    public string? ActionPlan { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum AlertCategory
{
    Tax,                // DGII - Impuestos
    ConsumerProtection, // Pro Consumidor
    Finance,            // Superintendencia de Bancos
    PublicProcurement,  // DGCP
    Telecommunications, // INDOTEL
    DataPrivacy,        // Protección de datos
    BusinessRegistration, // Registro mercantil
    LaborLaw,           // Leyes laborales
    Other
}

public enum AlertSeverity
{
    Low,        // Informativo
    Medium,     // Monitorear
    High,       // Requiere acción
    Critical    // Acción inmediata
}

public enum AlertStatus
{
    New,
    Acknowledged,
    UnderReview,
    ActionRequired,
    InProgress,
    Resolved,
    NotApplicable
}
```

### 3.2 RegulatorySource

```csharp
public class RegulatorySource
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public SourceType Type { get; set; }
    public AlertCategory DefaultCategory { get; set; }

    // Crawling
    public string? RssFeedUrl { get; set; }
    public string? CrawlPattern { get; set; } // CSS selector or regex
    public int ScanFrequencyHours { get; set; } = 24;
    public DateTime? LastScannedAt { get; set; }
    public DateTime? NextScanAt { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
    public int FailedScanCount { get; set; }

    public DateTime CreatedAt { get; set; }
}

public enum SourceType
{
    RssFeed,
    WebScraping,
    ApiEndpoint,
    EmailSubscription,
    Manual
}
```

### 3.3 RegulatoryKeyword

```csharp
public class RegulatoryKeyword
{
    public Guid Id { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public KeywordType Type { get; set; }
    public int WeightMultiplier { get; set; } = 1; // 1-5
    public AlertCategory? Category { get; set; } // null = all
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public enum KeywordType
{
    Exact,      // Coincidencia exacta
    Contains,   // Contiene
    Regex       // Expresión regular
}
```

---

## 4. Keywords de Monitoreo

### 4.1 Keywords por Categoría

```yaml
Tax:
  - "ITBIS"
  - "impuesto vehicular"
  - "placa"
  - "DGII"
  - "NCF"
  - "facturación electrónica"
  - "retención"
  - "anticipo"

ConsumerProtection:
  - "Pro Consumidor"
  - "garantía"
  - "publicidad engañosa"
  - "protección al consumidor"
  - "derecho de retracto"
  - "venta de vehículos"

Finance:
  - "lavado de activos"
  - "financiamiento"
  - "tasa de interés"
  - "Superintendencia de Bancos"
  - "debida diligencia"
  - "KYC"

DataPrivacy:
  - "datos personales"
  - "privacidad"
  - "consentimiento"
  - "protección de datos"

BusinessRegistration:
  - "registro mercantil"
  - "RNC"
  - "persona jurídica"
  - "comercio electrónico"
```

---

## 5. Procesos Detallados

### 5.1 REG-001: Escanear Fuente Regulatoria

| Paso | Acción                          | Sistema             | Validación        |
| ---- | ------------------------------- | ------------------- | ----------------- |
| 1    | Job scheduled se activa         | Hangfire            | Source.NextScanAt |
| 2    | Determinar tipo de fuente       | Crawler             | SourceType        |
| 3    | Ejecutar scraping/RSS           | Crawler             | Response OK       |
| 4    | Parsear documentos nuevos       | Crawler             | NewDocs found     |
| 5    | Por cada documento nuevo:       | Processor           | -                 |
| 6    | - Extraer texto                 | Processor           | Text extracted    |
| 7    | - Buscar keywords               | Processor           | Keywords matched  |
| 8    | - Calcular impact score         | Processor           | Score 1-100       |
| 9    | - Clasificar severidad          | Processor           | Severity assigned |
| 10   | Crear alerta si score >= 30     | AlertEngine         | Alert saved       |
| 11   | Notificar si severity >= Medium | NotificationService | Notification sent |
| 12   | Actualizar LastScannedAt        | ComplianceService   | Source updated    |

```csharp
public class ScanRegulatorySourceJob
{
    public async Task ExecuteAsync(Guid sourceId, CancellationToken ct)
    {
        var source = await _sourceRepository.GetByIdAsync(sourceId, ct);
        if (source == null || !source.IsActive) return;

        try
        {
            // 1. Fetch new documents
            List<RawDocument> documents;

            switch (source.Type)
            {
                case SourceType.RssFeed:
                    documents = await _rssCrawler.FetchAsync(source.RssFeedUrl!, ct);
                    break;
                case SourceType.WebScraping:
                    documents = await _webCrawler.ScrapeAsync(source.BaseUrl, source.CrawlPattern!, ct);
                    break;
                default:
                    throw new NotSupportedException($"Source type {source.Type} not supported");
            }

            // 2. Filter only new documents (not already processed)
            var existingUrls = await _alertRepository.GetProcessedUrlsAsync(source.Id, ct);
            var newDocuments = documents.Where(d => !existingUrls.Contains(d.Url)).ToList();

            foreach (var doc in newDocuments)
            {
                await ProcessDocumentAsync(source, doc, ct);
            }

            // 3. Update source scan status
            source.LastScannedAt = DateTime.UtcNow;
            source.NextScanAt = DateTime.UtcNow.AddHours(source.ScanFrequencyHours);
            source.FailedScanCount = 0;

            await _sourceRepository.UpdateAsync(source, ct);
        }
        catch (Exception ex)
        {
            source.FailedScanCount++;
            await _sourceRepository.UpdateAsync(source, ct);

            _logger.LogError(ex, "Failed to scan source {SourceId}", sourceId);

            if (source.FailedScanCount >= 5)
            {
                await NotifySourceFailureAsync(source, ex, ct);
            }
        }
    }

    private async Task ProcessDocumentAsync(RegulatorySource source, RawDocument doc, CancellationToken ct)
    {
        // 1. Extract text
        string fullText = doc.ContentType switch
        {
            "application/pdf" => await _pdfExtractor.ExtractTextAsync(doc.Content),
            "text/html" => _htmlExtractor.ExtractText(doc.Content),
            _ => doc.Content
        };

        // 2. Match keywords
        var keywords = await _keywordRepository.GetActiveAsync(source.DefaultCategory, ct);
        var matchedKeywords = new List<string>();
        var weightedScore = 0;

        foreach (var keyword in keywords)
        {
            bool matches = keyword.Type switch
            {
                KeywordType.Exact => fullText.Contains(keyword.Keyword, StringComparison.OrdinalIgnoreCase),
                KeywordType.Contains => fullText.ToLower().Contains(keyword.Keyword.ToLower()),
                KeywordType.Regex => Regex.IsMatch(fullText, keyword.Keyword, RegexOptions.IgnoreCase),
                _ => false
            };

            if (matches)
            {
                matchedKeywords.Add(keyword.Keyword);
                weightedScore += 10 * keyword.WeightMultiplier;
            }
        }

        // 3. Calculate impact score (0-100)
        var impactScore = Math.Min(100, weightedScore);

        // 4. Skip if low impact
        if (impactScore < 30) return;

        // 5. Determine severity
        var severity = impactScore switch
        {
            >= 80 => AlertSeverity.Critical,
            >= 60 => AlertSeverity.High,
            >= 40 => AlertSeverity.Medium,
            _ => AlertSeverity.Low
        };

        // 6. Generate summary (could use AI)
        var summary = GenerateSummary(fullText, matchedKeywords);

        // 7. Create alert
        var alert = new RegulatoryAlert
        {
            SourceId = source.Id,
            SourceUrl = doc.Url,
            PublishedAt = doc.PublishedAt ?? DateTime.UtcNow,
            Category = source.DefaultCategory,
            Severity = severity,
            ImpactScore = impactScore,
            Title = doc.Title,
            Summary = summary,
            FullText = fullText.Substring(0, Math.Min(10000, fullText.Length)),
            MatchedKeywords = matchedKeywords,
            DocumentUrl = doc.DocumentUrl,
            Status = AlertStatus.New,
            DetectedAt = DateTime.UtcNow,
            ActionDeadline = CalculateDeadline(severity)
        };

        await _alertRepository.AddAsync(alert, ct);

        // 8. Notify if Medium+ severity
        if (severity >= AlertSeverity.Medium)
        {
            await NotifyComplianceTeamAsync(alert, ct);
        }
    }

    private DateTime CalculateDeadline(AlertSeverity severity)
    {
        return severity switch
        {
            AlertSeverity.Critical => DateTime.UtcNow.AddDays(1),
            AlertSeverity.High => DateTime.UtcNow.AddDays(3),
            AlertSeverity.Medium => DateTime.UtcNow.AddDays(7),
            _ => DateTime.UtcNow.AddDays(14)
        };
    }
}
```

### 5.2 REG-002: Procesar Alerta Regulatoria

| Paso | Acción                          | Sistema            | Validación        |
| ---- | ------------------------------- | ------------------ | ----------------- |
| 1    | Compliance recibe notificación  | Email/Teams        | Alert created     |
| 2    | Abre alerta en dashboard        | Frontend           | Alert loaded      |
| 3    | Revisa documento original       | Frontend           | Doc available     |
| 4    | Acusa recibo (Acknowledge)      | ComplianceService  | Status updated    |
| 5    | Evalúa impacto en OKLA          | Compliance Officer | Manual            |
| 6    | Si aplica: crear plan de acción | ComplianceService  | ActionPlan saved  |
| 7    | Asignar responsable             | ComplianceService  | AssignedTo set    |
| 8    | Seguimiento hasta resolución    | ComplianceService  | Status tracking   |
| 9    | Documentar resolución           | ComplianceService  | Resolution saved  |
| 10   | Cerrar alerta                   | ComplianceService  | Status = Resolved |

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     Regulatory Alert Workflow                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   ┌─────────────┐                                                       │
│   │    NEW      │ Alerta detectada                                      │
│   └──────┬──────┘                                                       │
│          │                                                               │
│          ▼                                                               │
│   ┌─────────────────┐                                                   │
│   │  ACKNOWLEDGED   │ Compliance acusó recibo                           │
│   └────────┬────────┘                                                   │
│            │                                                             │
│            ▼                                                             │
│   ┌─────────────────┐      ┌─────────────────┐                          │
│   │  UNDER_REVIEW   │─────▶│  NOT_APPLICABLE │                          │
│   └────────┬────────┘      └─────────────────┘                          │
│            │ Aplica a OKLA                                              │
│            ▼                                                             │
│   ┌─────────────────┐                                                   │
│   │ ACTION_REQUIRED │ Se creó plan de acción                            │
│   └────────┬────────┘                                                   │
│            │                                                             │
│            ▼                                                             │
│   ┌─────────────────┐                                                   │
│   │   IN_PROGRESS   │ Implementando cambios                             │
│   └────────┬────────┘                                                   │
│            │                                                             │
│            ▼                                                             │
│   ┌─────────────────┐                                                   │
│   │    RESOLVED     │ Cambios implementados y documentados              │
│   └─────────────────┘                                                   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Notificaciones

### 6.1 Templates de Notificación

**Email - Nueva Alerta Regulatoria:**

```html
Asunto: [{{ severity }}] Nueva Alerta Regulatoria - {{ category }} Se ha
detectado una nueva regulación que puede afectar a OKLA: 📋 Fuente: {{
source.name }} 📅 Fecha: {{ publishedAt }} ⚠️ Severidad: {{ severity }} 📊
Impact Score: {{ impactScore }}/100 Título: {{ title }} Resumen: {{ summary }}
Keywords detectadas: {{ matchedKeywords }} Fecha límite de acción: {{
actionDeadline }} 🔗 Ver documento original: {{ documentUrl }} 🔗 Revisar en
OKLA Admin: {{ adminUrl }}
```

**Teams - Alerta Crítica:**

```json
{
  "@type": "MessageCard",
  "themeColor": "FF0000",
  "title": "🚨 ALERTA REGULATORIA CRÍTICA",
  "sections": [
    {
      "facts": [
        { "name": "Fuente", "value": "{{ source.name }}" },
        { "name": "Categoría", "value": "{{ category }}" },
        { "name": "Impact Score", "value": "{{ impactScore }}/100" }
      ],
      "text": "{{ summary }}"
    }
  ],
  "potentialAction": [
    {
      "@type": "OpenUri",
      "name": "Ver Alerta",
      "targets": [{ "uri": "{{ adminUrl }}" }]
    }
  ]
}
```

---

## 7. Reglas de Negocio

| Código  | Regla                                  | Validación          |
| ------- | -------------------------------------- | ------------------- |
| REG-R01 | Alerta crítica requiere acuse en 4h    | Deadline check      |
| REG-R02 | Fuente fallida 5 veces se desactiva    | FailedScanCount     |
| REG-R03 | Solo Compliance puede resolver alertas | Role check          |
| REG-R04 | Resolución requiere documentación      | Resolution required |
| REG-R05 | Keywords con weight >= 3 = High        | Weight calculation  |
| REG-R06 | Escanear DGII diario, otros semanal    | Frequency config    |

---

## 8. Códigos de Error

| Código    | HTTP | Mensaje                            | Causa            |
| --------- | ---- | ---------------------------------- | ---------------- |
| `REG_001` | 404  | Alert not found                    | Alerta no existe |
| `REG_002` | 400  | Cannot resolve without action plan | Falta plan       |
| `REG_003` | 403  | Only compliance can resolve        | Sin permiso      |
| `REG_004` | 500  | Source crawling failed             | Error scraping   |
| `REG_005` | 400  | Invalid keyword regex              | Regex inválido   |

---

## 9. Eventos RabbitMQ

| Evento                             | Exchange            | Descripción      |
| ---------------------------------- | ------------------- | ---------------- |
| `RegulatoryAlertCreatedEvent`      | `compliance.events` | Nueva alerta     |
| `RegulatoryAlertAcknowledgedEvent` | `compliance.events` | Acusada          |
| `RegulatoryAlertResolvedEvent`     | `compliance.events` | Resuelta         |
| `SourceScanFailedEvent`            | `compliance.events` | Fallo de escaneo |

---

## 10. Configuración

```json
{
  "RegulatoryAlerts": {
    "MinImpactScoreForAlert": 30,
    "SeverityThresholds": {
      "Low": 30,
      "Medium": 40,
      "High": 60,
      "Critical": 80
    },
    "AcknowledgeDeadlines": {
      "Critical": 4,
      "High": 24,
      "Medium": 72,
      "Low": 168
    },
    "DefaultScanFrequencyHours": {
      "DGII": 24,
      "ProConsumidor": 24,
      "SB": 168,
      "DGCP": 168
    },
    "NotifyOnSeverity": ["Medium", "High", "Critical"],
    "MaxFailedScansBeforeDisable": 5
  }
}
```

---

## 11. Métricas Prometheus

```
# Alerts
regulatory_alerts_total{category="...", severity="..."}
regulatory_alerts_pending{status="..."}

# Processing
regulatory_source_scan_duration_seconds{source="..."}
regulatory_source_scan_failures_total{source="..."}

# Response time
regulatory_alert_acknowledge_time_seconds{severity="..."}
regulatory_alert_resolution_time_seconds{severity="..."}
```

---

## 📚 Referencias

- [01-compliance-service.md](../08-COMPLIANCE-LEGAL-RD/01-compliance-service.md) - Servicio de compliance
- [03-kyc-verification.md](../08-COMPLIANCE-LEGAL-RD/03-kyc-verification.md) - Verificación KYC
- [04-teams-integration.md](../07-NOTIFICACIONES/04-teams-integration.md) - Notificaciones Teams
