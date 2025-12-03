# 📞 ContactService

Servicio de gestión de contactos y comunicación con clientes para el sistema CarDealer.

## 📋 Descripción

Microservicio responsable de gestionar contactos de clientes, prospectos, solicitudes de información y seguimiento de comunicaciones.

## 🚀 Características

- **Gestión de Contactos**: CRUD completo de contactos
- **Categorización**: Clientes, Prospectos, Leads
- **Historial de Comunicación**: Registro de interacciones
- **Integración con CRM**: Sincronización de datos
- **Búsqueda Avanzada**: Por múltiples criterios
- **Tags y Segmentación**: Organización flexible
- **Export/Import**: CSV, Excel
- **GDPR Compliance**: Gestión de consentimientos

## 🏗️ Arquitectura

```
ContactService.Api (Puerto 5007)
├── Controllers/
│   ├── ContactController.cs
│   └── CommunicationController.cs
├── ContactService.Application/
│   ├── Commands/
│   │   ├── CreateContactCommand
│   │   ├── UpdateContactCommand
│   │   └── DeleteContactCommand
│   ├── Queries/
│   │   ├── GetContactQuery
│   │   ├── SearchContactsQuery
│   │   └── GetCommunicationHistoryQuery
│   └── Services/
│       └── ContactManager
├── ContactService.Domain/
│   ├── Entities/
│   │   ├── Contact
│   │   ├── Communication
│   │   └── ContactTag
│   ├── Enums/
│   │   ├── ContactType
│   │   └── CommunicationChannel
│   └── ValueObjects/
│       ├── Email
│       ├── PhoneNumber
│       └── Address
└── ContactService.Infrastructure/
    ├── Data/
    ├── Repositories/
    ├── External/
    │   └── CRMIntegrationService
    └── Export/
```

## 📦 Dependencias Principales

- **Entity Framework Core 8.0**
- **MediatR 12.2.0** - CQRS
- **FluentValidation 11.8.0**
- **EPPlus 7.0.0** - Excel export
- **CsvHelper 30.0.1** - CSV handling
- **Serilog** - Logging

## ⚙️ Configuración

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=contactdb;..."
  },
  "ServiceUrls": {
    "NotificationService": "http://localhost:5003",
    "AuditService": "http://localhost:5002"
  },
  "Export": {
    "MaxRecords": 10000,
    "AllowedFormats": ["CSV", "Excel"]
  }
}
```

## 🔌 API Endpoints

### Contactos
```http
GET    /api/contacts                # Listar contactos
GET    /api/contacts/{id}           # Obtener contacto
POST   /api/contacts                # Crear contacto
PUT    /api/contacts/{id}           # Actualizar contacto
DELETE /api/contacts/{id}           # Eliminar contacto (soft delete)
```

### Búsqueda
```http
GET /api/contacts/search?query=John&type=Lead&tags=vip
GET /api/contacts/email/{email}
GET /api/contacts/phone/{phone}
```

### Comunicaciones
```http
GET  /api/contacts/{id}/communications        # Historial
POST /api/contacts/{id}/communications        # Registrar comunicación
```

### Export/Import
```http
GET  /api/contacts/export?format=csv
POST /api/contacts/import                     # Importar CSV/Excel
```

### Tags
```http
GET    /api/contacts/{id}/tags               # Obtener tags
POST   /api/contacts/{id}/tags               # Agregar tag
DELETE /api/contacts/{id}/tags/{tagId}       # Remover tag
```

## 📝 Ejemplos de Uso

### Crear Contacto
```bash
curl -X POST http://localhost:5007/api/contacts \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "phone": "+1-555-0123",
    "type": "Lead",
    "source": "Website",
    "address": {
      "street": "123 Main St",
      "city": "New York",
      "state": "NY",
      "zipCode": "10001",
      "country": "USA"
    },
    "tags": ["vip", "urgent"],
    "notes": "Interested in luxury vehicles"
  }'
```

**Respuesta**:
```json
{
  "id": "contact-123",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phone": "+1-555-0123",
  "type": "Lead",
  "status": "Active",
  "createdAt": "2024-01-15T10:30:00Z",
  "lastContactedAt": null,
  "tags": ["vip", "urgent"]
}
```

### Registrar Comunicación
```bash
curl -X POST http://localhost:5007/api/contacts/contact-123/communications \
  -H "Content-Type: application/json" \
  -d '{
    "channel": "Email",
    "subject": "Follow-up on BMW inquiry",
    "notes": "Customer interested in 2024 models",
    "communicatedAt": "2024-01-15T14:00:00Z"
  }'
```

### Buscar Contactos
```bash
curl -X GET "http://localhost:5007/api/contacts/search?query=John&type=Lead&tags=vip&page=1&pageSize=20"
```

### Exportar Contactos
```bash
curl -X GET "http://localhost:5007/api/contacts/export?format=csv&type=Client" \
  -H "Authorization: Bearer <token>" \
  -o contacts.csv
```

## 📊 Modelo de Datos

### Contact Entity
```csharp
public class Contact
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public ContactType Type { get; set; }  // Lead, Prospect, Client
    public ContactStatus Status { get; set; }  // Active, Inactive, Converted
    public string Source { get; set; }  // Website, Referral, Walk-in
    public Address Address { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastContactedAt { get; set; }
    public bool IsDeleted { get; set; }
    public List<ContactTag> Tags { get; set; }
    public List<Communication> Communications { get; set; }
}
```

### Communication Entity
```csharp
public class Communication
{
    public Guid Id { get; set; }
    public Guid ContactId { get; set; }
    public CommunicationChannel Channel { get; set; }  // Email, Phone, SMS, Meeting
    public string Subject { get; set; }
    public string Notes { get; set; }
    public DateTime CommunicatedAt { get; set; }
    public string CommunicatedBy { get; set; }  // User ID
}
```

## 🧪 Testing

```bash
# Tests unitarios
dotnet test ContactService.Tests/

# Tests de integración
dotnet test ContactService.Tests/ --filter "Category=Integration"

# Con cobertura
dotnet test /p:CollectCoverage=true
```

**Nota**: Este servicio necesita tests completos (MEDIA-2 en el plan).

## 🐳 Docker

```bash
# Build
docker build -t contactservice:latest .

# Run
docker run -d -p 5007:80 \
  -e ConnectionStrings__DefaultConnection="..." \
  --name contactservice \
  contactservice:latest
```

## 📊 Base de Datos

### Tablas
- `Contacts` - Información de contactos
- `Communications` - Historial de comunicaciones
- `ContactTags` - Tags asignados
- `Tags` - Catálogo de tags

### Índices
```sql
CREATE INDEX IX_Contacts_Email ON Contacts(Email);
CREATE INDEX IX_Contacts_Phone ON Contacts(Phone);
CREATE INDEX IX_Contacts_Type_Status ON Contacts(Type, Status);
CREATE INDEX IX_Contacts_LastContactedAt ON Contacts(LastContactedAt DESC);
CREATE INDEX IX_Communications_ContactId ON Communications(ContactId);
```

### Soft Delete
```sql
-- Los contactos se marcan como eliminados, no se borran físicamente
UPDATE Contacts SET IsDeleted = 1 WHERE Id = @contactId;
```

## 🔐 Seguridad & GDPR

### Consentimientos
```http
GET  /api/contacts/{id}/consents
POST /api/contacts/{id}/consents/marketing
```

### Derecho al Olvido
```http
POST /api/contacts/{id}/anonymize  # Anonimizar datos
POST /api/contacts/{id}/export-data  # Exportar datos personales
```

### Data Retention
- Contactos inactivos: 2 años
- Historial de comunicaciones: 5 años
- Logs de auditoría: 7 años

## 📈 Monitoreo

### Métricas
- `contacts_created_total` - Contactos creados
- `contacts_converted_total` - Leads convertidos a clientes
- `communications_total` - Comunicaciones registradas
- `export_requests_total` - Solicitudes de exportación

### KPIs
- Conversion rate (Lead → Client)
- Average time to conversion
- Communication frequency
- Response time

## 🔄 Integraciones

### NotificationService
- Envío de emails a contactos
- SMS notifications
- Push notifications

### AuditService
- Log de todas las operaciones CRUD
- Tracking de cambios en datos sensibles

### CRM Externo (Opcional)
- Sincronización bidireccional
- Webhook notifications

## 📱 Import/Export

### CSV Import Format
```csv
FirstName,LastName,Email,Phone,Type,Source,Tags
John,Doe,john@example.com,+1-555-0123,Lead,Website,"vip,urgent"
Jane,Smith,jane@example.com,+1-555-0124,Client,Referral,"loyal"
```

### Excel Export
- Sheet 1: Contact Information
- Sheet 2: Communication History
- Sheet 3: Tags & Categories

## 🚦 Estado

- ✅ **Build**: OK
- ⚠️ **Tests**: Pendiente (MEDIA-2)
- ✅ **Docker**: Configurado
- ✅ **GDPR**: Implementado

---

**Puerto**: 5007  
**Base de Datos**: PostgreSQL (contactdb)  
**Estado**: ⚠️ Tests Pendientes
