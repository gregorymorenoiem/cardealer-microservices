# 📋 AUDIT: Consistencia de Datos - Perfil de Vendedor

**Fecha:** 24 de febrero de 2026  
**Auditor:** GitHub Copilot  
**Estado:** ⚠️ **INCONSISTENCIAS DETECTADAS**

---

## 📊 Resumen Ejecutivo

Se ha realizado un análisis exhaustivo de los campos recopilados durante el **registro de vendedor** versus los campos mostrados en el **perfil de vendedor**. Se encontraron:

✅ **5 campos consistentes**  
⚠️ **3 campos con discrepancias**  
❌ **2 campos faltantes en registro**

---

## 🔍 Análisis Detallado

### **PASO 1: REGISTRO DE CUENTA**

_(Step 1: Account Creation)_

**Ubicación:** `frontend/web-next/src/app/(main)/vender/registro/page.tsx`

**Campos recopilados:**

```typescript
interface AccountData {
  firstName: string;
  lastName: string;
  email: string;
  phone: string; // ✓ Teléfono capturado aquí
  password: string;
  confirmPassword: string;
  accountType: "individual" | "dealer";
  acceptTerms: boolean;
}
```

**Schema de validación:**  
`frontend/web-next/src/lib/validations/seller-onboarding.ts`

---

### **PASO 2: PERFIL DE VENDEDOR**

_(Step 2: Seller Profile Setup)_

**Ubicación:** `frontend/web-next/src/components/seller-wizard/profile-step.tsx`

**Campos recopilados en formulario:**

```typescript
interface ProfileData {
  displayName: string; // Nombre público
  businessName: string; // Nombre del negocio (solo dealers)
  rnc: string; // Impuesto (solo dealers)
  description: string; // Descripción / Bio
  phone: string; // ⚠️ Teléfono DUPLICADO
  location: string; // Ubicación
  specialties: string[]; // Especialidades
}
```

---

### **PASO 3: ALMACENAMIENTO EN BASE DE DATOS**

_(SellerProfile Entity)_

**Ubicación:** `backend/UserService/UserService.Domain/Entities/SellerProfile.cs`

**Campos persistidos:**

```csharp
public class SellerProfile
{
    // INFORMACIÓN PERSONAL
    public string FullName { get; set; }        // ❌ NO SE CAPTURA EN FORMULARIO
    public DateTime? DateOfBirth { get; set; }  // ❌ NO SE CAPTURA EN FORMULARIO
    public string? Nationality { get; set; }    // ❌ NO SE CAPTURA EN FORMULARIO
    public string? Bio { get; set; }            // ✓ Description → Bio
    public string? AvatarUrl { get; set; }      // ❌ NO SE CAPTURA (foto perfil)

    // CONTACTO
    public string Phone { get; set; }           // ⚠️ DUPLICADO (Account + Profile)
    public string? AlternatePhone { get; set; } // ❌ NO SE CAPTURA EN FORMULARIO
    public string? WhatsApp { get; set; }       // ❌ NO SE CAPTURA EN FORMULARIO
    public string Email { get; set; }           // ✓ De Account Data

    // UBICACIÓN
    public string Address { get; set; }         // ❌ NO SE CAPTURA (solo City)
    public string City { get; set; }            // ⚠️ SE CAPTURA COMO "location"
    public string State { get; set; }           // ❌ NO SE CAPTURA EN FORMULARIO
    public string? ZipCode { get; set; }        // ❌ NO SE CAPTURA EN FORMULARIO

    // OTROS
    public string? PreferredContactMethod { get; set; }  // ❌ NO SE CAPTURA
    public bool AcceptsOffers { get; set; }    // ❌ NO SE CAPTURA EN FORMULARIO
    public bool ShowPhone { get; set; }         // ❌ NO SE CAPTURA EN FORMULARIO
    public bool ShowLocation { get; set; }      // ❌ NO SE CAPTURA EN FORMULARIO
}
```

---

## 📋 MAPEO DE CAMPOS: REGISTRO vs PERFIL vs DB

| #   | Campo UI Profile         | Registro Step 1 | Registro Step 2 | SellerProfile Entity | Estado           | Observación                                                   |
| --- | ------------------------ | --------------- | --------------- | -------------------- | ---------------- | ------------------------------------------------------------- |
| 1   | **Nombre del negocio**   | ❌              | displayName     | FullName             | ⚠️ INCONSISTENTE | Campo UI habla de "negocio" pero se captura como displayName  |
| 2   | **Nombre público**       | ❌              | displayName     | Bio                  | ✓ Mapeado        | Se captura en Step 2                                          |
| 3   | **Descripción**          | ❌              | description     | Bio                  | ✓ Mapeado        | Máx 500 caracteres                                            |
| 4   | **Teléfono de contacto** | ✓ (AccountStep) | phone           | Phone                | ⚠️ DUPLICADO     | Se solicita en AMBOS steps (línea 1 y 2)                      |
| 5   | **Ubicación**            | ❌              | location        | City + Address       | ⚠️ INCOMPLETO    | "location" string → solo se mapea a City, Address queda vacío |
| 6   | **Especialidades**       | ❌              | specialties[]   | ❌ NO MAPEADO        | ❌ FALTANTE      | Se captura en formulario pero NO se persiste en SellerProfile |
| 7   | _(ausente)_              | ❌              | _(ausente)_     | DateOfBirth          | ❌ NO CAPTURADO  | Base de datos tiene campo pero form no lo recopila            |
| 8   | _(ausente)_              | ❌              | _(ausente)_     | Nationality          | ❌ NO CAPTURADO  | Base de datos tiene campo pero form no lo recopila            |
| 9   | _(ausente)_              | ❌              | _(ausente)_     | Address              | ❌ INCOMPLETO    | Solo se captura "location" (city), address queda vacío        |
| 10  | _(ausente)_              | ❌              | _(ausente)_     | State/ZipCode        | ❌ NO CAPTURADOS | No hay campos en el formulario                                |

---

## ⚠️ PROBLEMAS IDENTIFICADOS

### **PROBLEMA #1: Teléfono Duplicado** 🔴

**Descripción:**  
El teléfono se solicita en DOS momentos diferentes:

1. **Account Step (Paso 1)** - Registro de cuenta
   - Campo: `phone`
   - Validación: Numérico, 10 dígitos

2. **Profile Step (Paso 2)** - Perfil de vendedor
   - Campo: `phone`
   - Validación: Numérico, 10 dígitos

**Impacto:**

- Usuario confusión: ¿Cuál es la diferencia?
- Datos duplicados: Se captura dos veces
- Riesgo: Valores inconsistentes en Account vs Profile

**Recomendación:**

```
✅ OPCIÓN A (Preferida): Usar el teléfono de Account Step
  - No solicitar teléfono en Profile Step
  - Permitir editar teléfono en settings del perfil después

✅ OPCIÓN B: Diferenciar propósitos
  - Account.phone = Teléfono principal (personal)
  - Profile.phone = Teléfono de contacto (para compradores)
  - Profile.whatsapp = Teléfono WhatsApp (opcional)
```

---

### **PROBLEMA #2: Ubicación Incompleta** 🟡

**Descripción:**  
El formulario solicita `location` (string genérico), pero la BD espera separación en `City` + `Address` + `State` + `ZipCode`.

**Código actual (Profile Step):**

```typescript
location: z.string()
  .max(200, "La ubicación no puede exceder 200 caracteres")
  .optional();
```

**Mapeo en Backend:**

```typescript
// Solo se mapea a City
// Address, State, ZipCode quedan vacíos
```

**Impacto:**

- Datos incompletos en BD
- No se puede mostrar dirección completa
- Dificulta búsquedas geográficas

**Recomendación:**

```
✅ OPCIÓN A: Expandir formulario
  - Reemplazar "location" string por:
    1. Provincia/Estado (dropdown)
    2. Ciudad (dropdown o autocompletar)
    3. Dirección (opcional)
    4. Código Postal (opcional)

✅ OPCIÓN B: Usar Google Places API
  - Permitir búsqueda de dirección con autocomplete
  - Auto-llenar City, Address, coordinates
```

---

### **PROBLEMA #3: Especialidades No Persistidas** 🔴

**Descripción:**  
Las especialidades se capturan en el formulario, pero NO se guardan en la BD.

**En el formulario (Profile Step):**

```typescript
specialties: z.array(z.string()).optional().default([]),
```

**En SellerProfile Entity:**

```csharp
// ❌ NO EXISTE CAMPO PARA ESPECIALIDADES
```

**Impacto:**

- Dato perdido después del registro
- Usuario no ve sus especialidades guardadas
- No se pueden filtrar vendedores por especialidad

**Recomendación:**

```
✅ Agregar campo a SellerProfile:
  public string[] Specialties { get; set; } = Array.Empty<string>();

✅ Agregar a CreateSellerProfileRequest DTO:
  public string[]? Specialties { get; set; }

✅ Mapear en handler:
  sellerProfile.Specialties = request.Specialties ?? Array.Empty<string>();

✅ Actualizar BD:
  CREATE MIGRATION AddSpecialties
```

---

### **PROBLEMA #4: Campos Faltantes en Formulario** 🟡

**Campos existentes en BD pero NO en formulario:**

| Campo                       | Tipo     | Razón probable                                       |
| --------------------------- | -------- | ---------------------------------------------------- |
| `FullName`                  | string   | Se captura como displayName, pero BD espera FullName |
| `DateOfBirth`               | DateTime | No se solicita en registro (recomendable agregarlo)  |
| `Nationality`               | string   | No se solicita en registro                           |
| `AvatarUrl`                 | string   | Foto de perfil (debería capturarse después)          |
| `AlternatePhone`            | string   | Teléfono alternativo (opcional)                      |
| `WhatsApp`                  | string   | Número WhatsApp (opcional)                           |
| `State/Province`            | string   | No se captura con "location" genérico                |
| `ZipCode`                   | string   | No se captura con "location" genérico                |
| `Address`                   | string   | No se captura con "location" genérico                |
| `PreferredContactMethod`    | string   | Debería elegir: teléfono, WhatsApp, email            |
| `ShowPhone`, `ShowLocation` | bool     | Preferencias de privacidad                           |

**Recomendación:**

```
Considerar agregar campos opcionales post-registro:
- Foto de perfil (en settings)
- Fecha de nacimiento (en settings)
- Teléfono alternativo (en settings)
- WhatsApp (en settings)
- Preferencias de privacidad (en settings)
```

---

### **PROBLEMA #5: Inconsistencia en Nomenclatura** 🟡

**En formulario (frontend):**

- `displayName` → Nombre público / Nombre de empresa

**En BD (backend):**

- `FullName` → Nombre completo personal

**Confusión:**

```
Paso 1: Se captura firstName + lastName (nombre personal)
Paso 2: Se captura displayName (nombre público/empresa)
BD: Se espera FullName

¿Se debe mapear (firstName + lastName) o displayName a FullName?
```

**Solución recomendada:**

```typescript
// En el handler cuando se crea el perfil:
if (isDealer) {
  sellerProfile.FullName = request.BusinessName;  // Nombre legal
} else {
  sellerProfile.FullName = $"{user.FirstName} {user.LastName}";  // Nombre personal
}
```

---

## 📊 TABLA RESUMEN: CAMPOS POR UBICACIÓN

### **Frontend: Formulario de Registro**

**Step 1 - Account:**

- firstName ✓
- lastName ✓
- email ✓
- phone ✓
- password ✓
- confirmPassword ✓
- accountType ✓
- acceptTerms ✓

**Step 2 - Profile:**

- displayName ✓
- businessName (dealers)
- rnc (dealers)
- description ✓
- phone ✓ (⚠️ DUPLICADO)
- location ✓
- specialties ✓

### **Backend: SellerProfile Entity**

✓ Campos bien mapeados:

- Email (de Account)
- Bio (de description)
- Phone (de Account o Profile)
- City (de location, parcialmente)

❌ Campos NO mapeados:

- DateOfBirth
- Nationality
- Address (esperado de location)
- State (esperado de location)
- ZipCode
- Specialties (se captura pero no se persiste)
- AlternatePhone
- WhatsApp
- PreferredContactMethod
- ShowPhone, ShowLocation
- AvatarUrl

---

## ✅ RECOMENDACIONES FINALES

### **CRÍTICA (Hacer ahora):**

1. **Resolver teléfono duplicado**
   - Eliminar teléfono de Profile Step
   - Usar el del Account Step
   - Permitir edición posterior en settings

2. **Persistir especialidades**
   - Agregar campo `Specialties[]` a SellerProfile
   - Crear migration en BD
   - Mapear en CreateSellerProfileHandler

3. **Expandir ubicación**
   - Cambiar `location` string a Provincia + Ciudad
   - Usar RD_PROVINCES constante existente
   - Mapear City y State correctamente

### **IMPORTANTE (Hacer en próximo sprint):**

4. **Agregar campos opcionales en settings post-registro:**
   - Foto de perfil (AvatarUrl)
   - Teléfono alternativo (AlternatePhone)
   - WhatsApp (WhatsApp)
   - Preferencias de privacidad (ShowPhone, ShowLocation)

5. **Clarificar displayName vs FullName**
   - Actualizar mapeo en handler
   - Documentar en código

### **RECOMENDADO (Future):**

6. **Captura adicional de datos:**
   - Fecha de nacimiento (para verificación)
   - Nacionalidad (datos estadísticos)
   - Foto de perfil (en Step 2 o post-registro)
   - Horarios de contacto
   - Idiomas que habla

---

## 📝 Código de Referencia

### CreateSellerProfileHandler ubicación:

**Archivo:** `backend/UserService/UserService.Application/Features/Sellers/Commands/CreateSellerProfileCommand.cs`

```csharp
// Aquí es donde se mapean los datos del formulario a la BD
// REVISAR mapeo actual y aplicar recomendaciones
```

### Frontend service:

**Archivo:** `frontend/web-next/src/services/sellers.ts` o similar

```typescript
// Donde se envía el formulario al backend
// Verificar que se incluya:
// - Specialties como array
// - Phone correcto
// - Location mapeada a city/address
```

---

## 🎯 Conclusión

**Estado General:** ⚠️ **REQUIERE CORRECCIONES**

Los datos del perfil **NO están completamente alineados** entre:

1. Lo que el formulario captura (frontend)
2. Lo que la BD espera (backend)
3. Lo que el usuario ve en el UI

Se requieren ajustes inmediatos en los puntos críticos identificados.

---

**Próximos pasos:**

1. [ ] Revisar y confirmar mapeos en CreateSellerProfileHandler
2. [ ] Eliminar teléfono duplicado
3. [ ] Agregar persistencia de especialidades
4. [ ] Expandir campos de ubicación
5. [ ] Documentar decisiones de diseño en code comments
