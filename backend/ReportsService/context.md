# ReportsService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** ReportsService
- **Puerto en Desarrollo:** 5031
- **Estado:** 🚧 **EN DESARROLLO - NO DESPLEGADO**
- **Base de Datos:** PostgreSQL (`reportsservice`)
- **Imagen Docker:** Local only

### Propósito
Generación de reportes y análisis de datos. Business intelligence, métricas, KPIs, reportes financieros y operacionales.

---

## 🏗️ ARQUITECTURA

```
ReportsService/
├── ReportsService.Api/
│   ├── Controllers/
│   │   ├── ReportsController.cs
│   │   ├── DashboardsController.cs
│   │   └── ExportsController.cs
│   └── Program.cs
├── ReportsService.Application/
│   └── Services/
│       ├── ReportGeneratorService.cs
│       └── DataAggregationService.cs
├── ReportsService.Domain/
│   ├── Entities/
│   │   ├── Report.cs
│   │   ├── ReportSchedule.cs
│   │   └── Dashboard.cs
│   └── Enums/
│       └── ReportType.cs
└── ReportsService.Infrastructure/
```

---

## 📦 ENTIDADES PRINCIPALES

### Report
```csharp
public class Report
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ReportType Type { get; set; }           // Sales, Inventory, Financial, User, Marketing
    public string? Description { get; set; }
    
    // Parámetros (JSON)
    public string Parameters { get; set; }         // { "startDate": "2026-01-01", "endDate": "2026-01-31" }
    
    // Generación
    public Guid GeneratedByUserId { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ReportStatus Status { get; set; }       // Pending, Processing, Completed, Failed
    
    // Resultado
    public string? ResultData { get; set; }        // JSON con los datos
    public string? FileUrl { get; set; }           // PDF/Excel en S3
    public string? ErrorMessage { get; set; }
    
    // Retención
    public DateTime ExpiresAt { get; set; }        // Auto-delete después de 30 días
}
```

### ReportSchedule
```csharp
public class ReportSchedule
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ReportType ReportType { get; set; }
    
    // Frecuencia
    public ScheduleFrequency Frequency { get; set; } // Daily, Weekly, Monthly
    public string CronExpression { get; set; }       // "0 9 * * 1" (Lunes 9am)
    
    // Parámetros
    public string Parameters { get; set; }           // JSON
    
    // Distribución
    public List<string> EmailRecipients { get; set; }
    public string Format { get; set; }               // "PDF", "Excel", "CSV"
    
    // Estado
    public bool IsActive { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Dashboard
```csharp
public class Dashboard
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    
    // Widgets (JSON array)
    public string Widgets { get; set; }
    // [
    //   { "type": "number", "metric": "totalSales", "label": "Total Sales" },
    //   { "type": "chart", "chartType": "line", "metric": "salesTrend" }
    // ]
    
    // Permisos
    public bool IsPublic { get; set; }
    public Guid? OwnerId { get; set; }
    public List<Guid> SharedWithUserIds { get; set; }
    
    // Refresh
    public int RefreshIntervalMinutes { get; set; } = 15;
    public DateTime? LastRefreshedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

---

## 📡 ENDPOINTS (Propuestos)

### Reportes Predefinidos
- `GET /api/reports/types` - Listar tipos de reportes disponibles
- `POST /api/reports/generate` - Generar reporte
  ```json
  {
    "type": "SalesReport",
    "parameters": {
      "startDate": "2026-01-01",
      "endDate": "2026-01-31",
      "groupBy": "day",
      "includeCharts": true
    },
    "format": "PDF"
  }
  ```
- `GET /api/reports/{id}` - Ver reporte generado
- `GET /api/reports/{id}/download` - Descargar archivo
- `GET /api/reports/history` - Historial de reportes generados

### Reportes Programados
- `POST /api/reports/schedules` - Crear schedule
- `GET /api/reports/schedules` - Listar schedules
- `PUT /api/reports/schedules/{id}` - Actualizar
- `DELETE /api/reports/schedules/{id}` - Eliminar
- `POST /api/reports/schedules/{id}/run-now` - Ejecutar manualmente

### Dashboards
- `GET /api/dashboards` - Listar dashboards
- `GET /api/dashboards/{id}` - Ver dashboard con datos
- `POST /api/dashboards` - Crear dashboard personalizado
- `PUT /api/dashboards/{id}` - Actualizar widgets

---

## 💡 TIPOS DE REPORTES

### 1. Sales Report
```csharp
public class SalesReportData
{
    public decimal TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public decimal AverageTransactionValue { get; set; }
    public List<DailySales> SalesByDay { get; set; }
    public List<ProductSales> TopProducts { get; set; }
    public List<AgentPerformance> SalesByAgent { get; set; }
}
```

### 2. Inventory Report
```csharp
public class InventoryReportData
{
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int SoldListings { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public List<ListingsByCategory> ByCategory { get; set; }
    public List<AgeAnalysis> ListingAge { get; set; }  // Días en inventario
}
```

### 3. Financial Report
```csharp
public class FinancialReportData
{
    public decimal GrossRevenue { get; set; }
    public decimal CommissionsPaid { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal TaxesCollected { get; set; }        // ITBIS
    public List<RevenueStream> RevenueBySource { get; set; }
    public List<MonthlyFinancials> MonthlyBreakdown { get; set; }
}
```

### 4. User Analytics Report
```csharp
public class UserAnalyticsReportData
{
    public int TotalUsers { get; set; }
    public int NewUsers { get; set; }
    public int ActiveUsers { get; set; }
    public List<UserGrowth> GrowthTrend { get; set; }
    public List<UserSegment> BySegment { get; set; }   // Buyers, Sellers, Dealers
    public List<UserActivity> ActivityMetrics { get; set; }
}
```

### 5. Marketing Report
```csharp
public class MarketingReportData
{
    public int TotalLeads { get; set; }
    public decimal LeadConversionRate { get; set; }
    public List<LeadSource> LeadsBySource { get; set; }
    public List<CampaignPerformance> CampaignROI { get; set; }
    public decimal CostPerLead { get; set; }
    public decimal CostPerAcquisition { get; set; }
}
```

---

## 💡 FUNCIONALIDADES PLANEADAS

### Generación Asíncrona
```csharp
// Background job para reportes pesados
public async Task GenerateReportAsync(Guid reportId)
{
    var report = await _context.Reports.FindAsync(reportId);
    report.Status = ReportStatus.Processing;
    await _context.SaveChangesAsync();
    
    try
    {
        // Query datos de múltiples servicios
        var data = await AggregateDataAsync(report.Type, report.Parameters);
        
        // Generar PDF/Excel
        var fileUrl = await _pdfGenerator.GenerateAsync(data);
        
        report.FileUrl = fileUrl;
        report.Status = ReportStatus.Completed;
        report.CompletedAt = DateTime.UtcNow;
    }
    catch (Exception ex)
    {
        report.Status = ReportStatus.Failed;
        report.ErrorMessage = ex.Message;
    }
    
    await _context.SaveChangesAsync();
    
    // Notificar usuario
    await _notificationService.SendAsync(report.GeneratedByUserId, 
        $"Report '{report.Name}' is ready");
}
```

### Export Formats
- **PDF:** Reportes formales con charts
- **Excel:** Datos raw para análisis
- **CSV:** Para importar a otras herramientas
- **JSON:** Para APIs

### Data Caching
- Cache de métricas comunes (30 min TTL)
- Pre-calcular KPIs diariamente
- Materialized views en PostgreSQL

### Chart Generation
Usando Chart.js o similar:
- Line charts (tendencias)
- Bar charts (comparaciones)
- Pie charts (distribución)
- Heatmaps (actividad por hora/día)

---

## 🔗 INTEGRACIÓN CON OTROS SERVICIOS

### VehiclesSaleService / PropertiesSaleService
- Datos de inventario y ventas

### BillingService
- Datos financieros y transacciones

### UserService
- Métricas de usuarios

### CRMService
- Datos de leads y conversiones

### MediaService
- Guardar reportes generados en S3

### NotificationService
- Enviar reportes por email

---

## 📊 MÉTRICAS CLAVE (KPIs)

### Sales KPIs
- **Total Revenue:** Ingresos totales
- **Average Deal Size:** Ticket promedio
- **Conversion Rate:** % de leads que compran
- **Time to Close:** Días promedio desde lead hasta venta

### Inventory KPIs
- **Inventory Turnover:** Rotación de inventario
- **Days on Market:** Promedio de días en venta
- **Sell-Through Rate:** % de listings vendidos

### Marketing KPIs
- **CAC:** Customer Acquisition Cost
- **ROI:** Return on Investment por campaña
- **Lead Quality Score:** Tasa de conversión por fuente

### User KPIs
- **DAU/MAU:** Daily/Monthly Active Users
- **Retention Rate:** % de usuarios que regresan
- **Churn Rate:** % de usuarios que se van

---

**Estado:** 🚧 EN DESARROLLO - No desplegado en producción  
**Versión:** 0.1.0
