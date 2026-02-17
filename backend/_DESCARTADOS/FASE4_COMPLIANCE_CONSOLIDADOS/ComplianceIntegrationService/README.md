# C12: ComplianceIntegrationService

## Descripción

Servicio de integración con entes reguladores de República Dominicana para el cumplimiento de leyes locales.

## Entes Reguladores Soportados

| Ente                       | Nombre Completo                                                 | Propósito                              |
| -------------------------- | --------------------------------------------------------------- | -------------------------------------- |
| **DGII**                   | Dirección General de Impuestos Internos                         | Reportes fiscales (606-609, IR, ITBIS) |
| **UAF**                    | Unidad de Análisis Financiero                                   | Reportes ROS (Ley 155-17 PLD)          |
| **ProConsumidor**          | Instituto Nacional de Protección de los Derechos del Consumidor | Garantías y reclamaciones              |
| **SuperintendenciaBancos** | Superintendencia de Bancos                                      | Regulaciones financieras               |
| **INDOTEL**                | Instituto Dominicano de las Telecomunicaciones                  | Comercio electrónico                   |
| **ProCompetencia**         | Pro-Competencia                                                 | Libre competencia                      |
| **OGTIC**                  | Oficina Gubernamental de Tecnologías                            | Datos abiertos y gobierno digital      |
| **DGA**                    | Dirección General de Aduanas                                    | Importaciones/Exportaciones            |
| **TSS**                    | Tesorería de la Seguridad Social                                | Aportes laborales                      |

## Tipos de Integración

- **Api**: Conexión directa vía REST/SOAP
- **Sftp**: Transferencia segura de archivos
- **WebService**: Servicios web tradicionales
- **FileUpload**: Carga manual de archivos
- **Webhook**: Notificaciones en tiempo real
- **Email**: Reportes por correo electrónico
- **DirectDb**: Conexión directa a base de datos
- **Batch**: Procesamiento por lotes
- **RealTime**: Sincronización en tiempo real

## Tipos de Reportes

| Tipo                     | Descripción                         | Frecuencia  |
| ------------------------ | ----------------------------------- | ----------- |
| ROS                      | Reporte de Operaciones Sospechosas  | Evento      |
| DeclaracionIR            | Declaración Impuesto sobre la Renta | Anual       |
| ReporteITBIS             | Reporte ITBIS                       | Mensual     |
| FacturaElectronica       | Facturación Electrónica             | Tiempo Real |
| Reporte606               | Compras de bienes y servicios       | Mensual     |
| Reporte607               | Ventas de bienes y servicios        | Mensual     |
| Reporte608               | Comprobantes anulados               | Mensual     |
| Reporte609               | Pagos al exterior                   | Mensual     |
| DeclaracionTSS           | Declaración Seguridad Social        | Mensual     |
| DebidaDiligencia         | Debida Diligencia de Clientes       | Evento      |
| MatrizRiesgo             | Matriz de Riesgo                    | Trimestral  |
| ReportePersonasExpuestas | Personas Políticamente Expuestas    | Mensual     |

## Endpoints Principales

### Configuraciones de Integración

- `GET /api/integrations` - Listar todas las integraciones
- `GET /api/integrations/{id}` - Obtener integración por ID
- `POST /api/integrations` - Crear nueva integración
- `PUT /api/integrations/{id}` - Actualizar integración
- `DELETE /api/integrations/{id}` - Eliminar integración

### Credenciales

- `GET /api/integrations/{integrationId}/credentials` - Listar credenciales
- `POST /api/integrations/{integrationId}/credentials` - Agregar credencial

### Transmisiones de Datos

- `GET /api/integrations/transmissions` - Listar transmisiones
- `POST /api/integrations/transmissions` - Crear transmisión
- `POST /api/integrations/transmissions/{id}/retry` - Reintentar transmisión fallida
- `GET /api/integrations/transmissions/pending-retries` - Transmisiones pendientes de reintento
- `GET /api/integrations/transmissions/by-date` - Transmisiones por rango de fecha

### Webhooks

- `GET /api/integrations/{integrationId}/webhooks` - Listar webhooks
- `POST /api/integrations/{integrationId}/webhooks` - Crear webhook

### Estadísticas y Salud

- `GET /api/integrations/statistics` - Estadísticas generales
- `GET /api/integrations/{id}/statistics` - Estadísticas por integración
- `GET /api/integrations/health` - Estado de salud de todas las integraciones

## Estructura del Proyecto

```
ComplianceIntegrationService/
├── ComplianceIntegrationService.Domain/
│   ├── Common/
│   │   └── EntityBase.cs
│   ├── Entities/
│   │   └── ComplianceIntegrationEntities.cs
│   ├── Enums/
│   │   └── ComplianceIntegrationEnums.cs
│   └── Interfaces/
│       └── IComplianceRepositories.cs
├── ComplianceIntegrationService.Application/
│   ├── DTOs/
│   │   └── ComplianceIntegrationDtos.cs
│   ├── Features/
│   │   └── Integrations/
│   │       ├── Commands/
│   │       └── Queries/
│   └── Validators/
│       └── ComplianceValidators.cs
├── ComplianceIntegrationService.Infrastructure/
│   ├── Persistence/
│   │   └── ComplianceIntegrationDbContext.cs
│   └── Repositories/
│       └── ComplianceRepositories.cs
└── ComplianceIntegrationService.Api/
    ├── Controllers/
    │   └── IntegrationsController.cs
    └── Program.cs
```

## Configuración

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=compliance_integration_db;Username=postgres;Password=postgres"
  }
}
```

## Puerto por Defecto

- **Puerto**: 5032

## Compilación

```bash
cd backend/ComplianceIntegrationService
dotnet build ComplianceIntegrationService.sln
```

## Docker

```bash
docker build -t okla-compliance-integration:latest .
docker run -p 5032:8080 okla-compliance-integration:latest
```
