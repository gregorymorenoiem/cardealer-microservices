# 📚 ÍNDICE MAESTRO - Procesos Spyne AI en OKLA

**Última actualización:** Enero 21, 2026  
**Versión:** 1.0.0  
**Autor:** Equipo OKLA

---

## 🎯 Propósito de esta Documentación

Este directorio contiene la documentación **COMPLETA Y DEFINITIVA** de todos los procesos que OKLA debe ejecutar a través de la API de Spyne AI.

> ⚠️ **IMPORTANTE:** Esta documentación está diseñada para eliminar la improvisación. Cada proceso tiene un orden específico que DEBE seguirse.

---

## 📁 Estructura de Documentos

| #   | Documento                                                                 | Descripción                                     | Usuarios Afectados  |
| --- | ------------------------------------------------------------------------- | ----------------------------------------------- | ------------------- |
| 01  | [MATRIZ_PERMISOS.md](01_MATRIZ_PERMISOS.md)                               | Matriz completa de permisos por tipo de usuario | Todos               |
| 02  | [FLUJO_PUBLICACION_VEHICULO.md](02_FLUJO_PUBLICACION_VEHICULO.md)         | Proceso completo al publicar un vehículo        | Individual + Dealer |
| 03  | [BACKGROUND_REPLACEMENT_PROCESO.md](03_BACKGROUND_REPLACEMENT_PROCESO.md) | Proceso detallado de reemplazo de fondo         | Todos               |
| 04  | [360_SPIN_PROCESO.md](04_360_SPIN_PROCESO.md)                             | Proceso completo de 360° Spin                   | Solo Dealers        |
| 05  | [FEATURE_VIDEO_PROCESO.md](05_FEATURE_VIDEO_PROCESO.md)                   | Proceso de generación de video (futuro)         | Solo Dealers        |
| 06  | [MANEJO_ERRORES.md](06_MANEJO_ERRORES.md)                                 | Guía de manejo de todos los errores posibles    | Desarrolladores     |
| 07  | [INTEGRACION_FRONTEND.md](07_INTEGRACION_FRONTEND.md)                     | Guía de integración en React                    | Desarrolladores     |
| 08  | [WEBHOOKS_CALLBACKS.md](08_WEBHOOKS_CALLBACKS.md)                         | Configuración de notificaciones async           | Desarrolladores     |
| 09  | [COSTOS_LIMITES.md](09_COSTOS_LIMITES.md)                                 | Costos y límites de uso de Spyne                | Administración      |
| 10  | [CASOS_USO_COMPLETOS.md](10_CASOS_USO_COMPLETOS.md)                       | Casos de uso end-to-end con ejemplos            | Todos               |

---

## 🔄 Diagrama General de Procesos

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                           FLUJO GENERAL SPYNE EN OKLA                               │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  ┌──────────────────┐                                                               │
│  │  USUARIO INICIA  │                                                               │
│  │  PUBLICACIÓN     │                                                               │
│  └────────┬─────────┘                                                               │
│           │                                                                         │
│           ▼                                                                         │
│  ┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐            │
│  │ 1. VERIFICAR     │     │ 2. OBTENER       │     │ 3. MOSTRAR       │            │
│  │    PERMISOS      │────▶│    FEATURES      │────▶│    OPCIONES      │            │
│  │    (AccountType) │     │    DISPONIBLES   │     │    AL USUARIO    │            │
│  └──────────────────┘     └──────────────────┘     └────────┬─────────┘            │
│                                                              │                      │
│           ┌──────────────────────────────────────────────────┘                      │
│           │                                                                         │
│           ▼                                                                         │
│  ┌──────────────────────────────────────────────────────────────────────┐          │
│  │                    USUARIO SUBE IMÁGENES                              │          │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  │          │
│  │  │  1-20 imgs  │  │  6-72 imgs  │  │  Interior   │  │   Varios    │  │          │
│  │  │  Exterior   │  │  360° Spin  │  │   5-10 imgs │  │   Ángulos   │  │          │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘  │          │
│  └───────────────────────────────────┬──────────────────────────────────┘          │
│                                      │                                              │
│           ┌──────────────────────────┴───────────────────────────┐                 │
│           │                                                       │                 │
│           ▼                                                       ▼                 │
│  ┌──────────────────┐                                   ┌──────────────────┐       │
│  │ INDIVIDUAL       │                                   │ DEALER           │       │
│  │ ───────────────  │                                   │ ─────────────    │       │
│  │ • Background:    │                                   │ • Background:    │       │
│  │   Solo 16570     │                                   │   16570 o 20883  │       │
│  │ • 360° Spin: ❌  │                                   │ • 360° Spin: ✅  │       │
│  │ • Video: ❌      │                                   │ • Video: ✅      │       │
│  └────────┬─────────┘                                   └────────┬─────────┘       │
│           │                                                       │                 │
│           ▼                                                       ▼                 │
│  ┌──────────────────┐                                   ┌──────────────────┐       │
│  │ 4. TRANSFORM     │                                   │ 4. TRANSFORM     │       │
│  │    IMAGES        │                                   │    + 360° SPIN   │       │
│  │    (Background)  │                                   │    + VIDEO       │       │
│  └────────┬─────────┘                                   └────────┬─────────┘       │
│           │                                                       │                 │
│           └───────────────────────┬───────────────────────────────┘                 │
│                                   │                                                 │
│                                   ▼                                                 │
│                          ┌──────────────────┐                                       │
│                          │ 5. POLLING       │                                       │
│                          │    STATUS        │◀──────────────┐                       │
│                          │    (cada 10s)    │               │                       │
│                          └────────┬─────────┘               │                       │
│                                   │                         │                       │
│                    ┌──────────────┼──────────────┐          │                       │
│                    ▼              ▼              ▼          │                       │
│              "processing"   "completed"    "failed"         │                       │
│                    │              │              │          │                       │
│                    └──────────────┤              ▼          │                       │
│                          Retry ───┘        Log Error        │                       │
│                                                             │                       │
│                                   │                                                 │
│                                   ▼                                                 │
│                          ┌──────────────────┐                                       │
│                          │ 6. GUARDAR URLs  │                                       │
│                          │    PROCESADAS    │                                       │
│                          │    EN DB         │                                       │
│                          └────────┬─────────┘                                       │
│                                   │                                                 │
│                                   ▼                                                 │
│                          ┌──────────────────┐                                       │
│                          │ 7. PUBLICACIÓN   │                                       │
│                          │    COMPLETA      │                                       │
│                          │    ✅            │                                       │
│                          └──────────────────┘                                       │
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 🎭 Tipos de Usuario y Permisos

| Tipo de Usuario          | Background Replacement | 360° Spin    | Feature Video | Backgrounds  |
| ------------------------ | ---------------------- | ------------ | ------------- | ------------ |
| **Comprador**            | ❌ No aplica           | ❌ No aplica | ❌ No aplica  | N/A          |
| **Vendedor Individual**  | ✅ Automático          | ❌ No        | ❌ No         | Solo 16570   |
| **Dealer sin Membresía** | ✅ Automático          | ❌ No        | ❌ No         | Solo 16570   |
| **Dealer con Membresía** | ✅ Elige fondo         | ✅ Sí        | ✅ Sí         | 16570, 20883 |
| **Admin**                | ✅ Todos               | ✅ Sí        | ✅ Sí         | Todos        |

---

## 📞 Endpoints Principales

### Consulta de Permisos (SIEMPRE PRIMERO)

```bash
# PASO 1: Verificar qué features tiene disponible el usuario
GET /api/vehicle-images/features?accountType={0|1|2}&hasActiveSubscription={true|false}
```

### Consulta de Backgrounds

```bash
# PASO 2: Obtener backgrounds disponibles
GET /api/vehicle-images/backgrounds?accountType={0|1|2}&hasActiveSubscription={true|false}
```

### Transformación de Imágenes

```bash
# PASO 3: Enviar imágenes para procesamiento
POST /api/vehicle-images/transform
POST /api/vehicle-images/transform/batch
```

### 360° Spin (Solo Dealers)

```bash
# PASO 3b: Generar 360° Spin
POST /api/vehicle-images/spin
```

### Verificación de Estado

```bash
# PASO 4: Polling hasta completar
GET /api/vehicle-images/status/{jobId}
GET /api/vehicle-images/spin/status/{jobId}
```

---

## 🔢 Orden de Ejecución Obligatorio

### Para Vendedor Individual

```
1. GET /features?accountType=0 ─────────────────────────┐
                                                        │
2. GET /backgrounds?accountType=0 ──────────────────────┤ CONSULTA
                                                        │
3. POST /transform (backgroundId=16570) ────────────────┤ PROCESAMIENTO
   └─ accountType: 0                                    │
   └─ hasActiveSubscription: false                      │
                                                        │
4. GET /status/{jobId} (cada 10s) ──────────────────────┤ POLLING
   └─ Hasta status="completed"                          │
                                                        │
5. Guardar processedUrl en DB ──────────────────────────┘ FINALIZACIÓN
```

### Para Dealer con Membresía

```
1. GET /features?accountType=1&hasActiveSubscription=true ───┐
                                                              │
2. GET /backgrounds?accountType=1&hasActiveSubscription=true ┤ CONSULTA
                                                              │
3a. POST /transform (backgroundId=16570 o 20883) ─────────────┤ PROCESAMIENTO
    └─ accountType: 1                                         │ IMÁGENES
    └─ hasActiveSubscription: true                            │
                                                              │
3b. POST /spin (si tiene 6+ imágenes exterior) ───────────────┤ 360° SPIN
    └─ accountType: 1                                         │
    └─ hasActiveSubscription: true                            │
                                                              │
4a. GET /status/{jobId} (cada 10s) ───────────────────────────┤ POLLING
4b. GET /spin/status/{jobId} (cada 10s) ──────────────────────┤ IMÁGENES
                                                              │ + SPIN
                                                              │
5. Guardar URLs procesadas en DB ─────────────────────────────┘ FINALIZACIÓN
```

---

## ⚠️ Reglas Críticas

### NUNCA Hacer:

1. ❌ **NO** llamar a `/spin` sin verificar primero que el usuario es Dealer con membresía
2. ❌ **NO** usar backgroundId=20883 para usuarios Individual
3. ❌ **NO** asumir que el procesamiento es instantáneo (siempre hacer polling)
4. ❌ **NO** enviar menos de 6 imágenes para 360° Spin
5. ❌ **NO** enviar más de 72 imágenes para 360° Spin
6. ❌ **NO** ignorar los errores de la API

### SIEMPRE Hacer:

1. ✅ **SIEMPRE** verificar permisos con `/features` antes de mostrar opciones al usuario
2. ✅ **SIEMPRE** validar el backgroundId antes de enviar a Spyne
3. ✅ **SIEMPRE** implementar timeout y reintentos en el polling
4. ✅ **SIEMPRE** guardar el jobId para poder recuperar el estado
5. ✅ **SIEMPRE** loggear errores para debugging
6. ✅ **SIEMPRE** mostrar progreso al usuario durante el procesamiento

---

## 📊 Códigos de Estado HTTP

| Código | Significado           | Acción Requerida          |
| ------ | --------------------- | ------------------------- |
| `200`  | Éxito                 | Procesar respuesta        |
| `202`  | Aceptado (procesando) | Iniciar polling           |
| `400`  | Error de validación   | Mostrar error al usuario  |
| `403`  | Sin permisos          | Mostrar opción de upgrade |
| `404`  | Job no encontrado     | Verificar jobId           |
| `429`  | Rate limit            | Esperar y reintentar      |
| `502`  | Error de Spyne        | Reintentar o escalar      |

---

## 🔗 Navegación Rápida

- **Siguiente:** [01_MATRIZ_PERMISOS.md](01_MATRIZ_PERMISOS.md)
- **Documentación Spyne Principal:** [../spyne/README.md](../spyne/README.md)
- **API Configuration:** [../spyne/API_CONFIGURATION.md](../spyne/API_CONFIGURATION.md)

---

**Equipo OKLA - Enero 2026**
