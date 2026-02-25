# 🚀 PLAN DE IMPLEMENTACIÓN: SellerProfile Data Model Enhancement

**Fecha:** 24 de febrero de 2026  
**Versión:** 1.0  
**Aprobación:** Pendiente  
**Estimado:** 2-3 sprints  
**Riesgo:** 🟠 ALTO (con mitigación → 🟡 MEDIO)

---

## 📋 Executive Summary

Implementación segura de 4 cambios propuestos en SellerProfile:

1. ✅ **Persistir Especialidades** (LOW RISK - ADITIVO)
2. 🔶 **Expandir Ubicación** (MEDIUM RISK - CAMBIO DE ESTRUCTURA)
3. ⚠️ **Remover Phone Duplicado** (HIGH RISK - REQUIERE COORDINACIÓN)
4. 🟢 **Agregar Campos Opcionales** (LOW RISK - FUTURO)

**RECOMENDACIÓN:** Implementar en 3 FASES INDEPENDIENTES

---

## 🗓️ Timeline Recomendado

```
SPRINT 1 (Semana 1-2):
├─ FASE 1: Especialidades
│  ├─ Backend: Migration + Entity + DTOs + Handlers
│  ├─ Frontend: Validar UI existe (ya captura)
│  └─ Integración: Event + Consumers
│
└─ Testing: Unit + Integration + Manual QA

SPRINT 2 (Semana 3-4):
├─ FASE 2: Ubicación Expandida
│  ├─ Backend: DTOs + Handlers + Migration
│  ├─ Frontend: ProfileStep + ProfilePage
│  └─ Integración: Búsqueda + Filtros
│
└─ Testing: Unit + Integration + Manual QA

SPRINT 3 (Semana 5-6):
├─ FASE 3: Remover Phone Duplicado
│  ├─ Backend: DTOs (no quitar campo, solo no capturar)
│  ├─ Frontend: Remover input de ProfileStep
│  └─ Validación: Teléfono viene de Account
│
├─ Testing: Completo
└─ Post-Launch: Monitoring + Rollback Plan
```

---

## 🔴 FASE 1: Persistir Especialidades (LOW RISK)

### Razón: ADITIVO (no rompe datos existentes)

### **1.1 Backend - Database**

#### Migration Script

```csharp
// File: Infrastructure/Migrations/20260224_AddSpecialitiesToSellerProfile.cs

public partial class AddSpecialitiesToSellerProfile : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string[]>(
            name: "specialties",
            table: "seller_profiles",
            type: "text[]",              // PostgreSQL JSON array
            nullable: false,
            defaultValue: Array.Empty<string>());

        // Índice para búsqueda por especialidades
        migrationBuilder.CreateIndex(
            name: "idx_seller_profiles_specialties",
            table: "seller_profiles",
            column: "specialties",
            filter: null);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "idx_seller_profiles_specialties",
            table: "seller_profiles");

        migrationBuilder.DropColumn(
            name: "specialties",
            table: "seller_profiles");
    }
}
```

**VALIDACIÓN:**

```bash
# Test migration locally
dotnet ef database update --context UserServiceDbContext
# Verificar que columna se agregó
# Rollback: dotnet ef database update <PreviousMigrationName>
```

---

### **1.2 Backend - Entity**

```csharp
// File: Domain/Entities/SellerProfile.cs

public class SellerProfile
{
    // ... existing properties ...

    // NUEVO:
    /// <summary>
    /// Especialidades del vendedor (ej: "Sedanes", "4x4", "Importación")
    /// </summary>
    public string[] Specialties { get; set; } = Array.Empty<string>();
}
```

---

### **1.3 Backend - DTOs**

```csharp
// File: Application/DTOs/SellerProfileDtos.cs

public class CreateSellerProfileRequest
{
    // ... existing properties ...

    /// <summary>
    /// Especialidades del vendedor (opcional)
    /// Ej: ["Sedanes", "Automáticos", "Importación"]
    /// </summary>
    public string[]? Specialties { get; set; }
}

public class UpdateSellerProfileRequest
{
    // ... existing properties ...

    /// <summary>
    /// Especialidades del vendedor (opcional)
    /// </summary>
    public string[]? Specialties { get; set; }
}

public class SellerProfileDto
{
    // ... existing properties ...

    /// <summary>
    /// Especialidades del vendedor
    /// </summary>
    public string[] Specialties { get; set; } = Array.Empty<string>();
}

public class SellerPublicProfileDto
{
    // ... existing properties ...

    /// <summary>
    /// Especialidades del vendedor (público)
    /// </summary>
    public string[] Specialties { get; set; } = new();
}
```

---

### **1.4 Backend - Handlers**

```csharp
// File: Application/UseCases/Sellers/CreateSellerProfileCommandHandler.cs

public async Task<SellerProfileDto> Handle(CreateSellerProfileCommand request, CancellationToken cancellationToken)
{
    // ... validation ...

    var sellerProfile = new SellerProfile
    {
        // ... mapping existing fields ...

        // NUEVO:
        Specialties = request.Specialties ?? Array.Empty<string>()
    };

    // ... persist and publish ...
}

// File: Application/UseCases/Sellers/UpdateSellerProfileCommandHandler.cs

public async Task<SellerProfileDto> Handle(UpdateSellerProfileCommand request, CancellationToken cancellationToken)
{
    var profile = await _repository.GetByIdAsync(request.SellerId);

    // ... update existing fields ...

    // NUEVO:
    if (request.Specialties != null)
    {
        profile.Specialties = request.Specialties;
    }

    // ... persist ...
}

// File: Application/UseCases/Sellers/GetSellerProfileQueryHandler.cs

private SellerProfileDto MapToDto(SellerProfile profile)
{
    return new SellerProfileDto
    {
        // ... existing mappings ...

        // NUEVO:
        Specialties = profile.Specialties ?? Array.Empty<string>()
    };
}
```

---

### **1.5 Backend - RabbitMQ Event**

```csharp
// File: Domain/Events/SellerProfileCreated.cs

public record SellerProfileCreatedEvent(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string Phone,
    string City,
    string Country,
    string[] Specialties  // NUEVO
) : EventBase
{
    public override string EventType => "seller.profile.created";
}
```

---

### **1.6 Frontend - No cambios (ya captura)**

✅ ProfileStep.tsx ya captura specialties
✅ Validaciones ya existen
✅ Services ya envían specialties

**VERIFICAR:**

```typescript
// services/sellers.ts - verificar que incluye
const payload = {
  // ... other fields ...
  specialties: data.specialties, // ✅ Debe estar
};
```

---

### **1.7 Testing - FASE 1**

#### Unit Tests (Backend)

```csharp
[Test]
public async Task CreateSellerProfile_WithSpecialties_PersistsCorrectly()
{
    // Arrange
    var request = new CreateSellerProfileRequest
    {
        // ... fields ...
        Specialties = new[] { "Sedanes", "Automáticos" }
    };

    // Act
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert
    Assert.AreEqual(2, result.Specialties.Length);
    Assert.Contains("Sedanes", result.Specialties);
}

[Test]
public async Task UpdateSellerProfile_WithSpecialties_UpdatesCorrectly()
{
    // Similar...
}

[Test]
public async Task GetSellerProfile_ReturnsSpecialties()
{
    // Query debe incluir especialidades
}
```

#### Integration Tests

```csharp
[Test]
public async Task CreateAndGetSellerProfile_SpecialtiesRoundtrip()
{
    // API: POST /api/sellers → especialidades
    // API: GET /api/sellers/{id} → retorna especialidades
}

[Test]
public async Task RabbitMQ_SellerProfileCreated_IncludesSpecialties()
{
    // Event debe incluir especialidades
    // Consumidores deben recibir
}
```

#### Manual QA - FASE 1

- [ ] Registrar vendedor con especialidades
  - Navegar a /vender/registro
  - Step 2: Seleccionar especialidades
  - Submit: Verificar en DB que se guardaron
- [ ] Editar perfil: Agregar/Remover especialidades
  - Navegar a /cuenta/perfil
  - Modificar specialties
  - Guardar: Verificar cambios persistieron
- [ ] API direct test
  ```bash
  curl -X GET http://localhost:8000/api/sellers/{id} \
    -H "Authorization: Bearer $TOKEN" | jq .specialties
  ```

---

## 🟠 FASE 2: Expandir Ubicación (MEDIUM RISK)

### Razón: Cambio de estructura (location string → 4 campos)

### **2.1 Backend - Database**

#### Migration Script

```csharp
// File: Infrastructure/Migrations/20260310_ExpandLocationInSellerProfile.cs

public partial class ExpandLocationInSellerProfile : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Los campos ya existen (Address, City, State, ZipCode)
        // Solo necesitamos actualizar los índices

        migrationBuilder.CreateIndex(
            name: "idx_seller_profiles_city_state",
            table: "seller_profiles",
            columns: new[] { "city", "state" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "idx_seller_profiles_city_state",
            table: "seller_profiles");
    }
}
```

**NOTA:** No necesita migración de datos porque los campos ya existen. Solo estamos cambiando cómo se capturan.

---

### **2.2 Backend - DTOs**

```csharp
// File: Application/DTOs/SellerProfileDtos.cs

public class CreateSellerProfileRequest
{
    // REMOVER:
    // public string? Location { get; set; }

    // AGREGAR/REEMPLAZAR:
    /// <summary>
    /// Ciudad (ej: "Santo Domingo")
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Provincia/Estado (ej: "Distrito Nacional")
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Dirección específica (opcional)
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Código postal (opcional)
    /// </summary>
    public string? ZipCode { get; set; }
}

// UpdateSellerProfileRequest: similar (todos optional)

// SellerProfileDto, SellerPublicProfileDto:
// Ya tienen estos campos, solo validar que se mapean
```

---

### **2.3 Backend - Handlers**

```csharp
// File: Application/UseCases/Sellers/CreateSellerProfileCommandHandler.cs

public async Task<SellerProfileDto> Handle(CreateSellerProfileCommand request, CancellationToken cancellationToken)
{
    var sellerProfile = new SellerProfile
    {
        // ... other fields ...

        // NUEVO MAPEO:
        City = request.City,
        State = request.State,
        Address = request.Address,
        ZipCode = request.ZipCode

        // NO MAPEAR: Location (string genérico) - ELIMINADO
    };
}
```

---

### **2.4 Frontend - ProfileStep**

```typescript
// File: components/seller-wizard/profile-step.tsx

export function ProfileStep({ ... }) {
    // REMOVER input de location
    // AGREGAR 4 inputs:

    return (
        <form>
            {/* ... other fields ... */}

            {/* Ubicación expandida */}
            <fieldset className="space-y-4 border rounded-lg p-4">
                <legend className="font-semibold">Ubicación</legend>

                {/* Provincia - Dropdown */}
                <div>
                    <Label htmlFor="state">Provincia *</Label>
                    <select
                        id="state"
                        value={data.state ?? ''}
                        onChange={(e) => {
                            onChange({
                                state: e.target.value,
                                city: '' // Reset city cuando cambia province
                            });
                        }}
                        required
                    >
                        <option value="">Selecciona provincia</option>
                        {RD_PROVINCES.map(p => (
                            <option key={p.id} value={p.name}>
                                {p.name}
                            </option>
                        ))}
                    </select>
                </div>

                {/* Ciudad - Dropdown (filtrado por provincia) */}
                <div>
                    <Label htmlFor="city">Ciudad *</Label>
                    <select
                        id="city"
                        value={data.city ?? ''}
                        onChange={(e) => onChange({ city: e.target.value })}
                        disabled={!data.state}
                        required
                    >
                        <option value="">Selecciona ciudad</option>
                        {RD_PROVINCES
                            .find(p => p.name === data.state)
                            ?.cities.map(c => (
                                <option key={c} value={c}>{c}</option>
                            ))}
                    </select>
                </div>

                {/* Dirección - Input opcional */}
                <div>
                    <Label htmlFor="address">Dirección (opcional)</Label>
                    <input
                        id="address"
                        type="text"
                        value={data.address ?? ''}
                        onChange={(e) => onChange({ address: e.target.value })}
                        placeholder="Ej: Calle Las Flores #123"
                        maxLength={200}
                    />
                </div>

                {/* Código Postal - Input opcional */}
                <div>
                    <Label htmlFor="zipCode">Código Postal (opcional)</Label>
                    <input
                        id="zipCode"
                        type="text"
                        value={data.zipCode ?? ''}
                        onChange={(e) => onChange({ zipCode: e.target.value })}
                        placeholder="Ej: 28000"
                        maxLength={10}
                    />
                </div>
            </fieldset>
        </form>
    );
}
```

---

### **2.5 Frontend - Validations**

```typescript
// File: lib/validations/seller-onboarding.ts

export const sellerProfileSchema = z.object({
  // ... other fields ...

  // REMOVER:
  // location: z.string().max(200),

  // AGREGAR:
  state: z.string().min(2, "Selecciona una provincia"),
  city: z.string().min(2, "Selecciona una ciudad"),
  address: z.string().max(200).optional(),
  zipCode: z.string().max(10).optional(),
});
```

---

### **2.6 Frontend - Registration Page**

```typescript
// File: app/(main)/vender/registro/page.tsx

interface ProfileData {
  // ... other fields ...

  // REMOVER:
  // location: string;

  // AGREGAR:
  state: string;
  city: string;
  address?: string;
  zipCode?: string;
}

// En handleSubmit():
const profilePayload = {
  // ... other fields ...
  state: profileData.state,
  city: profileData.city,
  address: profileData.address,
  zipCode: profileData.zipCode,
  // NO incluir "location"
};
```

---

### **2.7 Frontend - Profile Settings Page**

```typescript
// File: app/(main)/cuenta/perfil/page.tsx

// Actualizar sync:
React.useEffect(() => {
  if (sellerQuery.data) {
    const s = sellerQuery.data;
    setSellerForm({
      // ... other fields ...
      state: s.state || "",
      city: s.city || "",
      address: s.address || "",
      zipCode: s.zipCode || "",
      // NO: location
    });
  }
}, [sellerQuery.data]);

// Actualizar form UI similar a ProfileStep
```

---

### **2.8 Testing - FASE 2**

#### Unit Tests

```csharp
[Test]
public async Task CreateSellerProfile_WithExpandedLocation_MapsCorrectly()
{
    var request = new CreateSellerProfileRequest
    {
        City = "Santo Domingo",
        State = "Distrito Nacional",
        Address = "Calle Las Flores #123",
        ZipCode = "28000"
    };

    var result = await handler.Handle(request, CancellationToken.None);

    Assert.AreEqual("Santo Domingo", result.City);
    Assert.AreEqual("Distrito Nacional", result.State);
}
```

#### Integration Tests

```csharp
[Test]
public async Task Search_ByCityAndState_Works()
{
    // POST /api/sellers/search?city=Santo%20Domingo&state=DN
    // Debe retornar sellers en esa ubicación
}
```

#### Manual QA - FASE 2

- [ ] Registrar vendedor: Seleccionar provincia → ciudad
- [ ] Editar perfil: Cambiar ubicación
- [ ] Verificar DB: city y state se llenan correctamente
- [ ] Búsqueda: Filtrar por ciudad (si existe)

---

## ⚠️ FASE 3: Remover Phone Duplicado (HIGH RISK)

### Razón: Requiere coordinación UI/Backend, puede confundir datos

### **3.1 Backend - DTOs**

```csharp
// File: Application/DTOs/SellerProfileDtos.cs

public class CreateSellerProfileRequest
{
    // MANTENER (viene de Account Step):
    public string Phone { get; set; } = string.Empty;

    // No cambios - el teléfono viene del Account Step, no del Profile Step
}

public class UpdateSellerProfileRequest
{
    // OPCIONAL: permitir actualizar teléfono en settings post-registro
    public string? Phone { get; set; }
}
```

---

### **3.2 Frontend - ProfileStep**

```typescript
// File: components/seller-wizard/profile-step.tsx

export function ProfileStep({ ... }) {
    // REMOVER completamente:
    // <input name="phone" ... />

    // El teléfono se captura en AccountStep, no aquí
}
```

---

### **3.3 Frontend - Registration Page**

```typescript
// File: app/(main)/vender/registro/page.tsx

interface ProfileData {
  // REMOVER:
  // phone: string;
  // El teléfono viene de AccountData
}

// En createSellerProfile():
const profilePayload = {
  userId: user?.id,
  displayName: profileData.displayName,
  businessName: profileData.businessName,
  description: profileData.description,
  phone: accountData.phone, // ← FUENTE ÚNICA: Account Step
  state: profileData.state,
  city: profileData.city,
  // NO: profileData.phone (eliminado)
};
```

---

### **3.4 Frontend - Documentation**

```typescript
// Comment en ProfileStep:
/**
 * Note: Phone is NOT captured in this step.
 * Teléfono es capturado en Account Step (Paso 1).
 * Si el usuario quiere cambiar teléfono después del registro,
 * puede hacerlo en /cuenta/perfil (usar Account service, no Profile).
 */
```

---

### **3.5 Testing - FASE 3**

#### Manual QA - CRÍTICO

- [ ] Registro: Ingresa phone en Step 1, NO aparece input en Step 2
- [ ] Registro: Phone se guarda correctamente (es del Step 1)
- [ ] Editar perfil: Phone viene del Account (lecturas/escrituras no mezclan)
- [ ] API direct: GET /api/sellers/{id} tiene phone (del Account)

---

## 🟢 FASE 4 (FUTURO): Agregar Campos Opcionales

Este cambio está PLANIFICADO pero NO se implementa en este ciclo:

- [ ] DateOfBirth (en settings)
- [ ] Nationality (en settings)
- [ ] AlternatePhone (en settings)
- [ ] WhatsApp (en settings)
- [ ] AvatarUrl / Photo (en settings)
- [ ] PreferredContactMethod (en settings)

**Cuándo:** Próximo sprint cuando se implemente "Seller Settings" completo.

---

## 📋 Full Implementation Checklist

### **FASE 1: Especialidades**

- [ ] Migration script escrito y testeado (dry-run)
- [ ] Entity: `Specialties[]` agregado
- [ ] DTOs: Todos incluyen `Specialties`
- [ ] Handlers: Mapean `Specialties`
- [ ] Event: Incluye `Specialties`
- [ ] Frontend: Validar UI ya captura (✓)
- [ ] Tests: Unit + Integration
- [ ] Manual QA: Registro + Edit + API
- [ ] Merge to main
- [ ] Deploy a prod

### **FASE 2: Ubicación Expandida**

- [ ] DTOs: `location` → `city`, `state`, `address`, `zipCode`
- [ ] Handlers: Nuevo mapeo de ubicación
- [ ] Frontend - ProfileStep: 4 inputs + validation
- [ ] Frontend - ProfilePage: Sync + UI
- [ ] Frontend - Registration: Nuevo payload
- [ ] Búsqueda: Actualizar queries si necesario
- [ ] Tests: Unit + Integration + Components
- [ ] Manual QA: Registro + Edit + búsqueda
- [ ] Merge to main
- [ ] Deploy a prod

### **FASE 3: Remover Phone Duplicado**

- [ ] DTOs: Clarificar que phone viene de Account
- [ ] Frontend - ProfileStep: Remover input phone
- [ ] Frontend - Registration: Usar phone de Account
- [ ] Documentation: Comentarios claros
- [ ] Tests: Manual QA completa
- [ ] Merge to main
- [ ] Deploy a prod

### **Post-Deployment**

- [ ] Monitoring de errores (logs, Sentry)
- [ ] Verificar datos en prod
- [ ] Rollback plan activado (si needed)

---

## ⛔ Rollback Plan

Si algo falla:

### **Rollback FASE 1 (Especialidades)**

```bash
# BD
dotnet ef database update <PreviousMigration>
rm Migration_AddSpecialitiesToSellerProfile.cs

# Backend code
git revert <commits>

# Frontend
git revert <commits>

# Redeployment
# 1. DB migration revert
# 2. Deploy backend
# 3. Deploy frontend
```

### **Rollback FASE 2 (Ubicación)**

```bash
# Similar a FASE 1
# No hay cambios de BD (campos ya existen)
# Solo revert de código frontend/backend

# Verificar que datos en 4 campos aún existen en BD
SELECT city, state, address, zipcode FROM seller_profiles LIMIT 10;
```

### **Rollback FASE 3 (Phone)**

```bash
# No hay cambios de BD
# Solo revert de código
# Frontend: agregar phone input back a ProfileStep
# Backend: ignorar (DTOs ya no lo toman de Profile)
```

---

## 🔐 Safety Measures

✅ **Backups:**

- [ ] Backup BD antes de cada fase
- [ ] Backup de código (tags en git)

✅ **Rollback Prepared:**

- [ ] Scripts de rollback escritos
- [ ] Tested localmente

✅ **Monitoring:**

- [ ] Error tracking (Sentry/logging)
- [ ] Performance metrics
- [ ] User feedback

✅ **Communication:**

- [ ] Team notificado
- [ ] Deployment windows acordados
- [ ] Post-deploy checklist

---

## 📞 Escalation Path

Si hay problemas:

1. **CRÍTICO (API down, data loss):**
   - Trigger inmediato rollback
   - Notify team
   - Post-mortem

2. **ALTO (Some users affected):**
   - Evaluate fix vs rollback
   - 30min decision window

3. **MEDIO (Edge cases):**
   - Hotfix + monitoring
   - No rollback

4. **BAJO (Minor bugs):**
   - Next sprint fix

---

## ✅ Final Checklist Before Launch

- [ ] All 3 phases planned
- [ ] Each phase tested independently
- [ ] No data loss risk identified
- [ ] Frontend + Backend sync verified
- [ ] Rollback plans documented
- [ ] Team trained on plan
- [ ] Monitoring configured
- [ ] Deployment windows booked
- [ ] Stakeholders notified
- [ ] Go/No-go decision made

---

## 🎯 Success Criteria

✅ **FASE 1 Success:**

- Especialidades se guardan en BD
- Se retornan en API responses
- Consumidores reciben en eventos RabbitMQ
- Tests 100% passing

✅ **FASE 2 Success:**

- Ubicación se captura en 4 campos
- Se mapea correctamente en BD
- Búsqueda por city/state funciona
- No se pierden datos existentes

✅ **FASE 3 Success:**

- Phone viene de Account Step (único)
- ProfileStep NO captura phone
- API maneja correctamente
- No hay duplicación

✅ **Overall Success:**

- 0 data loss
- 0 critical bugs
- <5 bugs menores
- Team can support changes

---

**PRÓXIMO PASO:** Aprobación de este plan antes de implementación.
