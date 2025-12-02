# SchedulerService

Servicio de programación de tareas distribuido construido con Hangfire y ASP.NET Core 8.0. Proporciona capacidades robustas de programación cron, ejecución de trabajos en segundo plano y monitoreo a través de un dashboard web interactivo.

## 🏗️ Arquitectura

El servicio sigue los principios de **Clean Architecture** con separación clara de responsabilidades:

```
SchedulerService/
├── SchedulerService.Domain/         # Entidades y lógica de negocio
│   ├── Entities/
│   │   ├── Job.cs                   # Definición de trabajo programado
│   │   └── JobExecution.cs          # Historial de ejecución
│   ├── Enums/
│   │   ├── JobStatus.cs             # Estados del trabajo
│   │   └── ExecutionStatus.cs       # Estados de ejecución
│   └── Interfaces/
│       └── IJobScheduler.cs         # Abstracción para programación
│
├── SchedulerService.Application/    # Lógica de aplicación y CQRS
│   ├── Commands/
│   │   └── JobCommands.cs           # Crear, actualizar, eliminar trabajos
│   ├── Queries/
│   │   └── JobQueries.cs            # Consultas de trabajos y ejecuciones
│   ├── Handlers/
│   │   ├── JobCommandHandlers.cs    # Manejadores de comandos
│   │   └── JobQueryHandlers.cs      # Manejadores de consultas
│   └── Interfaces/
│       ├── IJobRepository.cs
│       └── IJobExecutionRepository.cs
│
├── SchedulerService.Infrastructure/ # Infraestructura técnica
│   ├── Data/
│   │   └── SchedulerDbContext.cs    # Contexto de EF Core
│   ├── Repositories/
│   │   ├── JobRepository.cs
│   │   └── JobExecutionRepository.cs
│   ├── Services/
│   │   └── HangfireJobScheduler.cs  # Implementación con Hangfire
│   ├── Jobs/                        # Trabajos de ejemplo
│   │   ├── CleanupOldExecutionsJob.cs
│   │   ├── DailyStatsReportJob.cs
│   │   └── HealthCheckJob.cs
│   └── DependencyInjection.cs       # Registro de servicios
│
└── SchedulerService.Api/            # API REST
    ├── Controllers/
    │   ├── JobsController.cs        # CRUD y gestión de trabajos
    │   └── ExecutionsController.cs  # Historial de ejecuciones
    └── Program.cs                   # Configuración y arranque
```

## 🚀 Características

### Programación de Trabajos
- ✅ **Trabajos Recurrentes**: Programación con expresiones cron
- ✅ **Trabajos Diferidos**: Ejecución única después de un delay
- ✅ **Reintento Automático**: Hasta 3 reintentos en caso de fallo
- ✅ **Gestión de Estado**: Enable, Disable, Pause, Trigger manual
- ✅ **Timeout Configurable**: Control de tiempo máximo de ejecución
- ✅ **Parámetros Dinámicos**: Diccionarios JSON para cada trabajo

### Monitoreo y Observabilidad
- 📊 **Dashboard de Hangfire**: Interfaz web en `/hangfire`
- 📈 **Historial de Ejecuciones**: Tracking completo de cada ejecución
- ⏱️ **Métricas de Duración**: Tiempo de ejecución en milisegundos
- 🔍 **Stack Traces**: Detalles de errores para debugging
- 🏥 **Health Checks**: Endpoint `/health` para monitoring

### Persistencia
- 💾 **PostgreSQL**: Almacenamiento de trabajos y ejecuciones
- 🗄️ **Entity Framework Core 8.0**: ORM con migraciones automáticas
- 📊 **Hangfire PostgreSQL**: Metadata de trabajos en BD

## 📡 API REST

### JobsController

#### Obtener todos los trabajos
```http
GET /api/jobs
```

#### Obtener trabajos activos
```http
GET /api/jobs/active
```

#### Obtener trabajo por ID
```http
GET /api/jobs/{id}
```

#### Crear nuevo trabajo
```http
POST /api/jobs
Content-Type: application/json

{
  "name": "SendWeeklyReport",
  "description": "Envía reporte semanal a clientes",
  "cronExpression": "0 9 * * MON",
  "jobType": "WeeklyReportJob",
  "retryCount": 3,
  "timeoutSeconds": 300,
  "parameters": {
    "emailTemplate": "weekly-report",
    "recipientGroup": "premium-users"
  }
}
```

#### Actualizar trabajo
```http
PUT /api/jobs/{id}
Content-Type: application/json

{
  "name": "SendWeeklyReport",
  "description": "Envía reporte semanal actualizado",
  "cronExpression": "0 10 * * MON",
  "jobType": "WeeklyReportJob",
  "retryCount": 5,
  "timeoutSeconds": 600,
  "parameters": {
    "emailTemplate": "weekly-report-v2",
    "recipientGroup": "all-users"
  }
}
```

#### Eliminar trabajo
```http
DELETE /api/jobs/{id}
```

#### Habilitar trabajo
```http
POST /api/jobs/{id}/enable
```

#### Deshabilitar trabajo
```http
POST /api/jobs/{id}/disable
```

#### Pausar trabajo
```http
POST /api/jobs/{id}/pause
```

#### Ejecutar trabajo inmediatamente
```http
POST /api/jobs/{id}/trigger
```

### ExecutionsController

#### Obtener ejecuciones recientes
```http
GET /api/executions/recent?pageSize=100
```

#### Obtener ejecución por ID
```http
GET /api/executions/{id}
```

#### Obtener ejecuciones de un trabajo
```http
GET /api/executions/job/{jobId}?pageSize=50
```

## 🕐 Expresiones Cron

Formato: `{segundo} {minuto} {hora} {día} {mes} {día-semana}`

### Ejemplos Comunes

| Expresión | Descripción |
|-----------|-------------|
| `0 * * * *` | Cada hora en punto |
| `*/5 * * * *` | Cada 5 minutos |
| `0 9 * * *` | Diario a las 9:00 AM |
| `0 0 * * MON` | Cada lunes a medianoche |
| `0 0 1 * *` | Primer día de cada mes |
| `0 9-17 * * MON-FRI` | Lunes a viernes, 9 AM - 5 PM |
| `0 0 * * SUN` | Cada domingo a medianoche |

## 🔧 Configuración

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=schedulerservice;Username=postgres;Password=postgres"
  },
  "Hangfire": {
    "ServerName": "SchedulerService",
    "WorkerCount": 5,
    "DashboardPath": "/hangfire",
    "DashboardTitle": "Scheduler Service Dashboard"
  },
  "JobSettings": {
    "CleanupOldExecutions": {
      "RetentionDays": 30,
      "CronExpression": "0 0 * * *"
    },
    "DailyStatsReport": {
      "CronExpression": "0 1 * * *"
    },
    "HealthCheck": {
      "CronExpression": "*/5 * * * *"
    }
  }
}
```

### Variables de Entorno (Docker)
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:80
ConnectionStrings__DefaultConnection=Host=schedulerservice-db;Database=schedulerservice;Username=postgres;Password=password
Hangfire__ServerName=SchedulerService-Docker
Hangfire__WorkerCount=5
```

## 🐳 Docker

### Construcción
```bash
docker build -t schedulerservice:latest -f Dockerfile .
```

### Ejecución con Docker Compose
```bash
# Desde el directorio backend/
docker-compose up -d schedulerservice schedulerservice-db

# Ver logs
docker-compose logs -f schedulerservice

# Detener
docker-compose down
```

El servicio estará disponible en:
- **API**: http://localhost:15091
- **Hangfire Dashboard**: http://localhost:15091/hangfire
- **Health Check**: http://localhost:15091/health

## 💻 Desarrollo Local

### Prerrequisitos
- .NET 8.0 SDK
- PostgreSQL 16
- (Opcional) Docker Desktop

### Configuración
1. Crear base de datos:
```sql
CREATE DATABASE schedulerservice;
```

2. Restaurar paquetes:
```bash
dotnet restore
```

3. Aplicar migraciones:
```bash
dotnet ef database update --project SchedulerService.Infrastructure --startup-project SchedulerService.Api
```

4. Ejecutar:
```bash
dotnet run --project SchedulerService.Api
```

### Tests
```bash
# Ejecutar todos los tests
dotnet test

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Tests específicos
dotnet test --filter "FullyQualifiedName~JobTests"
```

## 📝 Crear un Trabajo Personalizado

### 1. Crear la clase del trabajo
```csharp
// Infrastructure/Jobs/SendEmailReportJob.cs
public class SendEmailReportJob
{
    private readonly ILogger<SendEmailReportJob> _logger;
    private readonly IEmailService _emailService;

    public SendEmailReportJob(
        ILogger<SendEmailReportJob> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task Execute(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("Starting email report job");
        
        var template = parameters.GetValueOrDefault("emailTemplate");
        var recipients = parameters.GetValueOrDefault("recipients");
        
        await _emailService.SendReportAsync(template, recipients);
        
        _logger.LogInformation("Email report sent successfully");
    }
}
```

### 2. Registrar en DependencyInjection.cs
```csharp
services.AddScoped<SendEmailReportJob>();
```

### 3. Programar vía API
```http
POST /api/jobs
Content-Type: application/json

{
  "name": "EmailReport",
  "cronExpression": "0 8 * * MON",
  "jobType": "SendEmailReportJob",
  "parameters": {
    "emailTemplate": "weekly-report",
    "recipients": "team@company.com"
  }
}
```

## 🔒 Seguridad

### Autenticación del Dashboard
Para producción, configurar autenticación en `Program.cs`:

```csharp
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new MyAuthorizationFilter() }
});
```

### Implementar filtro:
```csharp
public class MyAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}
```

## 📊 Monitoreo con Prometheus

El servicio expone métricas compatibles con Prometheus:
- Trabajos programados totales
- Ejecuciones exitosas/fallidas
- Duración promedio de ejecuciones
- Workers activos

## 🛠️ Troubleshooting

### La migración no se aplica automáticamente
```bash
dotnet ef database update --project SchedulerService.Infrastructure --startup-project SchedulerService.Api
```

### Hangfire no inicia trabajos
1. Verificar que el trabajo esté **Enabled**
2. Verificar expresión cron: https://crontab.guru/
3. Revisar logs del servidor Hangfire
4. Verificar que HangfireServer esté corriendo

### Error de conexión a PostgreSQL
1. Verificar que PostgreSQL esté corriendo
2. Verificar cadena de conexión en `appsettings.json`
3. Verificar credenciales de base de datos

## 📚 Stack Tecnológico

- **ASP.NET Core 8.0**: Framework web
- **Hangfire 1.8.14**: Motor de programación de trabajos
- **PostgreSQL 16**: Base de datos relacional
- **Entity Framework Core 8.0**: ORM
- **MediatR 12.4.1**: Patrón CQRS
- **Npgsql 8.0**: Driver PostgreSQL
- **xUnit**: Framework de testing

## 📄 Licencia

MIT License - Ver archivo LICENSE para más detalles.

## 🤝 Contribuir

1. Fork el proyecto
2. Crear una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir un Pull Request

## 📞 Soporte

Para reportar bugs o solicitar features, crear un issue en GitHub.
