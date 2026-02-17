# CacheService - Context Documentation

## 📋 INFORMACIÓN GENERAL

- **Nombre del Servicio:** CacheService
- **Puerto en Desarrollo:** 5009
- **Estado:** ⚠️ **SOLO DESARROLLO LOCAL**
- **Cache Backend:** Redis
- **Base de Datos:** N/A (stateless)
- **Imagen Docker:** Local only

### Propósito
Servicio de abstracción sobre Redis para gestión de cache distribuido. Proporciona API REST para operaciones de cache (Get, Set, Delete), con soporte para TTL, cache tags y invalidación masiva.

---

## 🏗️ ARQUITECTURA

```
CacheService/
├── CacheService.Api/
│   ├── Controllers/
│   │   └── CacheController.cs
│   └── Program.cs
├── CacheService.Application/
│   └── Services/
│       └── CacheService.cs
├── CacheService.Domain/
│   └── Interfaces/
│       └── ICacheService.cs
└── CacheService.Infrastructure/
    └── Redis/
        └── RedisCacheService.cs
```

---

## 📡 ENDPOINTS API

#### GET `/api/cache/{key}`
Obtener valor del cache.

#### POST `/api/cache`
Guardar en cache.

**Request:**
```json
{
  "key": "user:123",
  "value": "{...}",
  "expirationSeconds": 3600
}
```

#### DELETE `/api/cache/{key}`
Eliminar del cache.

#### DELETE `/api/cache/pattern/{pattern}`
Eliminar múltiples keys por patrón.

---

## 🔧 CONFIGURACIÓN

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "DefaultExpirationSeconds": 3600
  }
}
```

---

## 📝 CASOS DE USO

- Cache de permisos de usuarios (RoleService)
- Cache de catálogo de vehículos (VehiclesSaleService)
- Cache de configuraciones (ConfigurationService)
- Session storage

---

**Estado:** Solo desarrollo local - No desplegado en producción  
**Versión:** 1.0.0
