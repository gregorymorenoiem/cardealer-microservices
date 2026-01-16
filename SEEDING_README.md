# 📌 SEEDING V2.0 - Documentación Completa

**Fecha:** Enero 15, 2026  
**Estado:** ✅ Documentación Completada  
**Prioridad:** ⭐⭐⭐ CRÍTICO

---

## 📚 Documentos Creados

### 1. **SEEDING_INTEGRATION_GUIDE.md**

- **Tipo:** Guía de Arquitectura
- **Contenido:**
  - 📐 Flujo de 5 fases
  - 🔧 Arquitectura con MediaService
  - 📝 Ejemplos de código C# y SQL
  - ✅ Checklist de integración
  - 🎯 Estado actual del proyecto

**Ubicación:** `/cardealer-microservices/SEEDING_INTEGRATION_GUIDE.md`

---

### 2. **SEEDING_EXECUTION.md**

- **Tipo:** Guía de Ejecución Paso a Paso
- **Contenido:**
  - ⏱️ Tiempo total: ~85 minutos
  - 🚀 Opción rápida (sin imágenes)
  - ⭐ Opción completa (con MediaService)
  - ✅ Pre-requisitos y verificación
  - 📊 Scripts bash listos para ejecutar
  - 🆘 Troubleshooting completo

**Ubicación:** `/cardealer-microservices/SEEDING_EXECUTION.md`

---

## 🏗️ Arquitectura de Seeding

```
FASE 1: SQL Catálogo (3 min)
├─ script: scripts/seed_catalog.sql
├─ Datos: 20 marcas, 35 modelos, 100 trims
└─ Verificación: SELECT COUNT(*) FROM vehicle_makes;

FASE 2: API Vehículos (30 min)
├─ Endpoint: POST /api/vehicles
├─ Datos: 150 vehículos sin imágenes
├─ Script: seeding-vehicles.sh (nuevo)
└─ Verificación: SELECT COUNT(*) FROM vehicles;

FASE 3: API MediaService Imágenes (45 min)
├─ Endpoint: POST /api/media/upload
├─ Puerto: 16070 (MediaService)
├─ Datos: 1,500 imágenes (10 por vehículo)
├─ Script: seeding-images.sh (nuevo)
└─ Verificación: SELECT COUNT(*) FROM media_files;

FASE 4: API Asociar Imágenes (0 min - Automático)
├─ Endpoint: PUT /api/vehicles/{vehicleId}/images
├─ Nota: MediaService asocia automáticamente
└─ Fallback: Manual si es necesario

FASE 5: SQL Homepage (2 min)
├─ SQL: Seed 8 secciones + 90 asignaciones
├─ Secciones: Carousel, Sedanes, SUVs, Camionetas, etc.
└─ Verificación: SELECT COUNT(*) FROM homepage_section_configs;

TOTAL: 80 MINUTOS
```

---

## ⭐ Característica Clave: MediaService

### El Cambio Importante

**ANTES (Incorrecto):**

- Intentar subir imágenes directamente en VehiclesSaleService
- Problema: Violación de arquitectura de microservicios
- Resultado: ❌ No funciona

**AHORA (Correcto):**

- MediaService es el responsable de TODAS las imágenes
- VehiclesSaleService solo almacena referencias (mediaIds)
- Imagen principal, galería, datos: TODO en MediaService
- Resultado: ✅ Arquitectura limpia y escalable

### Flujo Correcto

```
1. Crear vehículo (SIN imágenes)
   POST /api/vehicles
   {title, price, year, ..., images: []}

2. Subir imágenes a MediaService
   POST /api/media/upload
   file = imagen
   vehicleId = {vehicleId}

3. MediaService retorna mediaIds

4. Opcionalmente: Asociar imágenes
   PUT /api/vehicles/{vehicleId}/images
   {mediaIds: [...]}
```

---

## 📝 Scripts Listos para Usar

### Script 1: seeding-vehicles.sh

- **Ubicación:** `/cardealer-microservices/seeding-vehicles.sh`
- **Función:** Crear 150 vehículos vía API
- **Tiempo:** 30 minutos
- **Uso:** `./seeding-vehicles.sh`

### Script 2: seeding-images.sh

- **Ubicación:** `/cardealer-microservices/seeding-images.sh`
- **Función:** Subir 1,500 imágenes a MediaService
- **Tiempo:** 45 minutos
- **Uso:** `./seeding-images.sh`
- **Fuente de imágenes:** picsum.photos (random)

### Script 3: seed_catalog.sql

- **Ubicación:** `/cardealer-microservices/scripts/seed_catalog.sql`
- **Función:** Crear catálogo (marcas, modelos, trims)
- **Tiempo:** 3 minutos
- **Uso:** `docker exec -i postgres_db psql ... < scripts/seed_catalog.sql`

---

## ✅ Verificación en 3 Pasos

```bash
# PASO 1: Catálogo
docker exec postgres_db psql -U postgres -d vehiclessaleservice \
  -c "SELECT COUNT(*) FROM vehicle_makes;"
# Esperado: 20

# PASO 2: Vehículos
docker exec postgres_db psql -U postgres -d vehiclessaleservice \
  -c "SELECT COUNT(*) FROM vehicles WHERE status = 'Active';"
# Esperado: 150

# PASO 3: Imágenes (en mediaservice DB)
docker exec postgres_db psql -U postgres -d mediaservice \
  -c "SELECT COUNT(*) FROM media_files WHERE entity_type = 'Vehicle';"
# Esperado: 1500
```

---

## 🎯 Próximos Pasos

### Ahora (Está Listo)

✅ Documentación completa  
✅ Arquitectura definida  
✅ Scripts escritos  
✅ Ejecutar: Siga `SEEDING_EXECUTION.md`

### Después (Ejecutar)

⏳ Ejecutar Fase 1: SQL Catálogo (3 min)
⏳ Ejecutar Fase 2: Vehículos via API (30 min)
⏳ Ejecutar Fase 3: Imágenes via MediaService (45 min)
⏳ Ejecutar Fase 5: Homepage Sections (2 min)

### Finalmente (Validar)

⏳ Verificación completa
⏳ Frontend testing con datos reales
⏳ 150 vehículos + 1,500 imágenes listos

---

## 📊 Estadísticas Finales

| Elemento               | Cantidad | Fuente                |
| ---------------------- | -------- | --------------------- |
| **Marcas**             | 20       | SQL (Fase 1)          |
| **Modelos**            | 35+      | SQL (Fase 1)          |
| **Trims**              | 100+     | SQL (Fase 1)          |
| **Vehículos**          | 150      | API (Fase 2)          |
| **Imágenes**           | 1,500    | MediaService (Fase 3) |
| **Homepage Secciones** | 8        | SQL (Fase 5)          |
| **Asignaciones**       | 90       | SQL (Fase 5)          |
| **Usuarios Dealers**   | TBD      | Admin panel           |
| **Tiempo Total**       | ~85 min  | Ejecución             |

---

## 🔗 Archivos Relacionados

- [`SEEDING_INTEGRATION_GUIDE.md`](./SEEDING_INTEGRATION_GUIDE.md) - Guía de arquitectura
- [`SEEDING_EXECUTION.md`](./SEEDING_EXECUTION.md) - Guía de ejecución
- [`scripts/seed_catalog.sql`](./scripts/seed_catalog.sql) - Script SQL del catálogo
- [`seeding-vehicles.sh`](./seeding-vehicles.sh) - Script bash para vehículos
- [`seeding-images.sh`](./seeding-images.sh) - Script bash para imágenes (en progreso)
- `.github/copilot-instructions.md` - Contexto del proyecto
- Databases: vehiclessaleservice, mediaservice, adminservice, etc.

---

## 💡 Notas Importantes

### MediaService es Crítico

- ⭐ MediaService maneja TODAS las imágenes
- 🔗 VehiclesSaleService solo guarda referencias
- 📦 Separación limpia de responsabilidades
- 🚀 Escalable (puede crecer independientemente)

### Verificación es Obligatoria

- ✅ Siempre verificar con SQL después de cada fase
- ✅ Antes de pasar a siguiente fase
- ✅ Números exactos: 20, 35, 100, 150, 1500

### Troubleshooting

- 🔧 Revisar `SEEDING_EXECUTION.md` sección "Troubleshooting"
- 🔧 Verificar logs: `docker logs {service}`
- 🔧 Verificar conectividad: `curl http://localhost:{port}/health`

---

## 🎉 Resumen

**Estado:** ✅ DOCUMENTACIÓN COMPLETADA

Tienes todo lo necesario para ejecutar seeding de 150 vehículos + 1,500 imágenes en 85 minutos:

1. **Lee** `SEEDING_EXECUTION.md` para entender el flujo
2. **Ejecuta** cada fase en orden (1, 2, 3, 5)
3. **Verifica** con SQL después de cada fase
4. **Prueba** el frontend con datos reales

Las imágenes se manejan correctamente con **MediaService** en puerto 16070.

---

_Documentación completada: Enero 15, 2026_  
_Arquitectura: Microservicios con MediaService para imágenes_  
_Listos para ejecutar seeding cuando sea necesario_ ✅
