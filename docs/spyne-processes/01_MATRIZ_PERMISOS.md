# 🔐 01 - Matriz de Permisos Spyne

**Última actualización:** Enero 21, 2026  
**Versión:** 1.0.0

---

## 📋 Resumen

Este documento define **EXACTAMENTE** qué puede hacer cada tipo de usuario con las funcionalidades de Spyne en OKLA.

---

## 🎭 Tipos de Usuario en OKLA

### Definición en Código

```csharp
public enum AccountType
{
    Individual = 0,  // Comprador o Vendedor Individual
    Dealer = 1,      // Dealer (con o sin membresía)
    Admin = 2        // Administrador del sistema
}
```

### Subtipos de Usuario

| AccountType | Subtipo              | Descripción                            | Paga         |
| ----------- | -------------------- | -------------------------------------- | ------------ |
| `0`         | Comprador            | Solo busca/compra vehículos            | Gratis       |
| `0`         | Vendedor Individual  | Vende su vehículo personal             | $29/listing  |
| `1`         | Dealer sin Membresía | Dealer registrado pero sin plan activo | -            |
| `1`         | Dealer con Membresía | Dealer con plan Starter/Pro/Enterprise | $49-$299/mes |
| `2`         | Admin                | Staff de OKLA                          | Staff        |

---

## 🔒 Matriz de Permisos Completa

### Por Funcionalidad

| Funcionalidad                     | Comprador | Vendedor Individual | Dealer (sin) | Dealer (con) | Admin |
| --------------------------------- | --------- | ------------------- | ------------ | ------------ | ----- |
| **Subir imágenes**                | ❌        | ✅                  | ✅           | ✅           | ✅    |
| **Background Replacement**        | ❌        | ✅ Auto             | ✅ Auto      | ✅ Elige     | ✅    |
| **Fondo Blanco Infinito (16570)** | ❌        | ✅                  | ✅           | ✅           | ✅    |
| **Fondo Showroom Gris (20883)**   | ❌        | ❌                  | ❌           | ✅           | ✅    |
| **360° Spin**                     | ❌        | ❌                  | ❌           | ✅           | ✅    |
| **Feature Video**                 | ❌        | ❌                  | ❌           | ✅           | ✅    |
| **Hotspots en 360°**              | ❌        | ❌                  | ❌           | ✅           | ✅    |
| **License Plate Masking**         | ❌        | ✅                  | ✅           | ✅           | ✅    |

### Por Background

| Background ID | Nombre          | Individual | Dealer (sin) | Dealer (con) | Admin |
| ------------- | --------------- | ---------- | ------------ | ------------ | ----- |
| `16570`       | Blanco Infinito | ✅ Default | ✅ Default   | ✅ Opción    | ✅    |
| `20883`       | Showroom Gris   | ❌         | ❌           | ✅ Default   | ✅    |

### Por Endpoint

| Endpoint               | Método | Individual | Dealer (sin) | Dealer (con) | Admin |
| ---------------------- | ------ | ---------- | ------------ | ------------ | ----- |
| `/features`            | GET    | ✅         | ✅           | ✅           | ✅    |
| `/backgrounds`         | GET    | ✅         | ✅           | ✅           | ✅    |
| `/transform`           | POST   | ✅         | ✅           | ✅           | ✅    |
| `/transform/batch`     | POST   | ✅         | ✅           | ✅           | ✅    |
| `/spin`                | POST   | ❌ 403     | ❌ 403       | ✅           | ✅    |
| `/status/{jobId}`      | GET    | ✅         | ✅           | ✅           | ✅    |
| `/spin/status/{jobId}` | GET    | ✅         | ✅           | ✅           | ✅    |
| `/health`              | GET    | ✅         | ✅           | ✅           | ✅    |

---

## 🧮 Lógica de Permisos en Código

### Clase SpyneUserContext

```csharp
public class SpyneUserContext
{
    public Guid UserId { get; set; }
    public AccountType AccountType { get; set; }
    public bool HasActiveSubscription { get; set; }

    // ══════════════════════════════════════════════════════════════════
    // REGLA: Solo Dealers CON membresía activa pueden usar 360° Spin
    // ══════════════════════════════════════════════════════════════════
    public bool CanUse360Spin =>
        (AccountType == AccountType.Dealer && HasActiveSubscription) ||
        AccountType == AccountType.Admin;

    // ══════════════════════════════════════════════════════════════════
    // REGLA: Solo Dealers CON membresía pueden usar Showroom Gris
    // ══════════════════════════════════════════════════════════════════
    public bool CanUseShowroomBackground =>
        (AccountType == AccountType.Dealer && HasActiveSubscription) ||
        AccountType == AccountType.Admin;

    // ══════════════════════════════════════════════════════════════════
    // REGLA: Retorna los backgrounds disponibles para este usuario
    // ══════════════════════════════════════════════════════════════════
    public string[] GetAvailableBackgrounds() =>
        CanUseShowroomBackground
            ? SpyneBackgrounds.DealerBackgrounds  // ["16570", "20883"]
            : SpyneBackgrounds.FreeBackgrounds;   // ["16570"]

    // ══════════════════════════════════════════════════════════════════
    // REGLA: Default background según tipo de usuario
    // ══════════════════════════════════════════════════════════════════
    public string GetDefaultBackground() =>
        CanUseShowroomBackground
            ? SpyneBackgrounds.DefaultDealer  // "20883"
            : SpyneBackgrounds.DefaultFree;   // "16570"

    // ══════════════════════════════════════════════════════════════════
    // REGLA: Valida si puede usar un background específico
    // ══════════════════════════════════════════════════════════════════
    public bool CanUseBackground(string backgroundId) =>
        GetAvailableBackgrounds().Contains(backgroundId);
}
```

---

## 📡 Cómo Verificar Permisos (Frontend)

### Paso 1: Llamar al Endpoint de Features

```typescript
// SIEMPRE llamar esto primero al cargar la página de publicación
interface SpyneFeaturesResponse {
  accountType: string;
  hasActiveSubscription: boolean;
  features: {
    backgroundReplacement: FeatureAccess;
    spin360: FeatureAccess;
    featureVideo: FeatureAccess;
  };
}

interface FeatureAccess {
  available: boolean;
  requiresDealerMembership: boolean;
  description: string;
  availableBackgrounds?: string[];
  defaultBackground?: string;
}

// Llamada
const response = await fetch(
  `/api/spyne/vehicle-images/features?accountType=${user.accountType}&hasActiveSubscription=${user.hasActiveSubscription}`,
);
const features: SpyneFeaturesResponse = await response.json();
```

### Paso 2: Renderizar UI Según Permisos

```tsx
function PublishVehiclePage() {
  const [features, setFeatures] = useState<SpyneFeaturesResponse | null>(null);

  useEffect(() => {
    // Cargar features al montar el componente
    fetchFeatures();
  }, []);

  return (
    <div>
      {/* Selector de Background */}
      <BackgroundSelector
        availableBackgrounds={
          features?.features.backgroundReplacement.availableBackgrounds
        }
        defaultBackground={
          features?.features.backgroundReplacement.defaultBackground
        }
      />

      {/* Opción de 360° Spin - Solo si está disponible */}
      {features?.features.spin360.available ? (
        <Spin360Uploader />
      ) : (
        <UpgradePrompt
          feature="360° Spin"
          message="Disponible con membresía Dealer"
          upgradeUrl="/dealer/pricing"
        />
      )}

      {/* Opción de Video - Solo si está disponible */}
      {features?.features.featureVideo.available ? (
        <FeatureVideoGenerator />
      ) : (
        <UpgradePrompt
          feature="Video Promocional"
          message="Disponible con membresía Dealer"
          upgradeUrl="/dealer/pricing"
        />
      )}
    </div>
  );
}
```

---

## 🔄 Ejemplos de Respuestas

### Vendedor Individual (accountType=0)

```json
{
  "accountType": "Individual",
  "hasActiveSubscription": false,
  "features": {
    "backgroundReplacement": {
      "available": true,
      "requiresDealerMembership": false,
      "description": "Fondo Blanco Infinito incluido para mantener calidad de la plataforma",
      "availableBackgrounds": ["16570"],
      "defaultBackground": "16570"
    },
    "spin360": {
      "available": false,
      "requiresDealerMembership": true,
      "description": "Exclusivo para Dealers con membresía activa"
    },
    "featureVideo": {
      "available": false,
      "requiresDealerMembership": true,
      "description": "Exclusivo para Dealers con membresía activa"
    }
  }
}
```

### Dealer con Membresía (accountType=1, hasActiveSubscription=true)

```json
{
  "accountType": "Dealer",
  "hasActiveSubscription": true,
  "features": {
    "backgroundReplacement": {
      "available": true,
      "requiresDealerMembership": false,
      "description": "Acceso a todos los fondos profesionales",
      "availableBackgrounds": ["16570", "20883"],
      "defaultBackground": "20883"
    },
    "spin360": {
      "available": true,
      "requiresDealerMembership": true,
      "description": "Vista 360° interactiva disponible"
    },
    "featureVideo": {
      "available": true,
      "requiresDealerMembership": true,
      "description": "Video promocional con IA disponible"
    }
  }
}
```

### Dealer SIN Membresía (accountType=1, hasActiveSubscription=false)

```json
{
  "accountType": "Dealer",
  "hasActiveSubscription": false,
  "features": {
    "backgroundReplacement": {
      "available": true,
      "requiresDealerMembership": false,
      "description": "Fondo Blanco Infinito incluido para mantener calidad de la plataforma",
      "availableBackgrounds": ["16570"],
      "defaultBackground": "16570"
    },
    "spin360": {
      "available": false,
      "requiresDealerMembership": true,
      "description": "Exclusivo para Dealers con membresía activa"
    },
    "featureVideo": {
      "available": false,
      "requiresDealerMembership": true,
      "description": "Exclusivo para Dealers con membresía activa"
    }
  }
}
```

---

## ⚠️ Manejo de Acceso Denegado

### Respuesta 403 para 360° Spin

Cuando un usuario sin permisos intenta acceder a `/spin`:

```json
{
  "error": "360° Spin requires Dealer membership",
  "feature": "360° Spin",
  "requiredAccountType": "Dealer",
  "requiresActiveSubscription": true,
  "message": "Esta función está disponible exclusivamente para Dealers con membresía activa. Actualiza tu cuenta para acceder a vistas 360° interactivas.",
  "upgradeUrl": "/dealer/pricing"
}
```

### Frontend: Mostrar Upgrade Prompt

```tsx
function UpgradePrompt({ feature, message, upgradeUrl }: UpgradePromptProps) {
  return (
    <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-200 rounded-lg p-6">
      <div className="flex items-center gap-3">
        <LockIcon className="w-8 h-8 text-blue-500" />
        <div>
          <h3 className="font-semibold text-gray-900">{feature}</h3>
          <p className="text-gray-600">{message}</p>
        </div>
      </div>
      <Button href={upgradeUrl} className="mt-4 bg-blue-600 hover:bg-blue-700">
        Ver Planes de Dealer
      </Button>
    </div>
  );
}
```

---

## 📋 Checklist de Validación

### Backend (Controller)

- [ ] Validar `accountType` en cada request
- [ ] Validar `hasActiveSubscription` para features premium
- [ ] Retornar 403 con mensaje claro si no tiene permisos
- [ ] Loggear intentos de acceso denegado
- [ ] Fallback a background permitido si se solicita uno no autorizado

### Frontend

- [ ] Llamar a `/features` al cargar la página
- [ ] Ocultar opciones no disponibles
- [ ] Mostrar upgrade prompts claros
- [ ] Deshabilitar botones de features premium
- [ ] Mostrar preview de lo que incluye el upgrade

---

## 🔗 Navegación

- **Anterior:** [00_INDICE_MAESTRO.md](00_INDICE_MAESTRO.md)
- **Siguiente:** [02_FLUJO_PUBLICACION_VEHICULO.md](02_FLUJO_PUBLICACION_VEHICULO.md)

---

**Equipo OKLA - Enero 2026**
