# 💰 Tabla Completa de Proveedores y Precios

## 📋 Resumen Ejecutivo

Este documento consolida todos los proveedores disponibles para el procesamiento 360° de vehículos, con sus costos, características y recomendaciones.

## 🎬 Proveedores de Video360Service

### Extracción de Frames de Video

| #   | Proveedor      | Costo/Vehículo | Plan Mensual | Incluye      | Calidad                | Velocidad | Estado     |
| --- | -------------- | -------------- | ------------ | ------------ | ---------------------- | --------- | ---------- |
| 1   | **ApyHub**     | **$0.009**     | $9/mes       | 1,000 videos | ⭐⭐⭐⭐ Muy Buena     | ~45s      | ✅ Activo  |
| 2   | **FFmpeg-API** | **$0.011**     | $11/mes      | 1,000 videos | ⭐⭐⭐⭐⭐ Excelente   | ~30s      | ✅ DEFAULT |
| 3   | **Cloudinary** | **$0.012**     | $12/mes      | 1,000 videos | ⭐⭐⭐⭐ Buena         | ~60s      | ✅ Activo  |
| 4   | **Imgix**      | **$0.018**     | $18/mes      | 1,000 videos | ⭐⭐⭐⭐⭐ Excelente   | ~40s      | ✅ Activo  |
| 5   | **Shotstack**  | **$0.05**      | $50/mes      | 1,000 videos | ⭐⭐⭐⭐⭐ Profesional | ~20s      | ✅ Activo  |

### Características por Proveedor

| Proveedor      | API      | Formatos            | Resolución Max | Latencia | Regiones     |
| -------------- | -------- | ------------------- | -------------- | -------- | ------------ |
| **ApyHub**     | REST     | MP4, MOV            | 4K             | Media    | Global       |
| **FFmpeg-API** | REST     | MP4, MOV, AVI, WebM | 4K             | Baja     | US, EU       |
| **Cloudinary** | REST/SDK | Todos               | 8K             | Media    | Global (CDN) |
| **Imgix**      | REST     | MP4, WebM           | 4K             | Baja     | US, EU, Asia |
| **Shotstack**  | REST     | Todos               | 8K             | Muy Baja | Global       |

## 🎨 Proveedores de BackgroundRemovalService

### Eliminación de Fondos (por imagen)

| #   | Proveedor          | Costo/Imagen | Costo × 6 | Calidad                | Velocidad | Tipo        |
| --- | ------------------ | ------------ | --------- | ---------------------- | --------- | ----------- |
| 1   | **Local (ML)**     | **$0.00**    | **$0.00** | ⭐⭐⭐ Variable        | ~5s/img   | Sin costo   |
| 2   | **Slazzer**        | **$0.02**    | **$0.12** | ⭐⭐⭐⭐ Buena         | ~3s/img   | Económico   |
| 3   | **ClipDrop**       | **$0.05**    | **$0.30** | ⭐⭐⭐⭐⭐ Excelente   | ~2s/img   | DEFAULT     |
| 4   | **Photoroom**      | **$0.05**    | **$0.30** | ⭐⭐⭐⭐ Muy Buena     | ~3s/img   | Alternativo |
| 5   | **Removal.AI**     | **$0.08**    | **$0.48** | ⭐⭐⭐⭐ Buena         | ~4s/img   | Backup      |
| 6   | **Clipping Magic** | **$0.10**    | **$0.60** | ⭐⭐⭐⭐⭐ Excelente   | ~2s/img   | Premium     |
| 7   | **Remove.bg**      | **$0.20**    | **$1.20** | ⭐⭐⭐⭐⭐ Profesional | ~1s/img   | Premium     |

### Características por Proveedor

| Proveedor          | Especialidad | Batch | Resolución Max | HD Extra | Vehículos  |
| ------------------ | ------------ | ----- | -------------- | -------- | ---------- |
| **Local (ML)**     | General      | ✅    | 2K             | N/A      | ⭐⭐⭐     |
| **Slazzer**        | E-commerce   | ✅    | 4K             | No       | ⭐⭐⭐⭐   |
| **ClipDrop**       | Vehículos    | ✅    | 4K             | No       | ⭐⭐⭐⭐⭐ |
| **Photoroom**      | Productos    | ✅    | 4K             | Gratis   | ⭐⭐⭐⭐   |
| **Removal.AI**     | General      | ✅    | 4K             | +$0.02   | ⭐⭐⭐     |
| **Clipping Magic** | Profesional  | ❌    | 8K             | No       | ⭐⭐⭐⭐⭐ |
| **Remove.bg**      | Cualquier    | ✅    | 25MP           | +$0.03   | ⭐⭐⭐⭐⭐ |

## 💵 Costo Total por Vehículo

### Combinaciones de Proveedores

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    COSTO POR VEHÍCULO 360° COMPLETO                         │
│                    (Extracción + 6 × Eliminación de Fondo)                  │
└─────────────────────────────────────────────────────────────────────────────┘

╔══════════════════════════════════════════════════════════════════════════════╗
║  💚 OPCIÓN ECONÓMICA                                    TOTAL: $0.129       ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  Video360:         ApyHub           $0.009                                   ║
║  Background × 6:   Slazzer          $0.02 × 6 = $0.12                        ║
║                                     ─────────────────                        ║
║                                     $0.129/vehículo                          ║
╚══════════════════════════════════════════════════════════════════════════════╝

╔══════════════════════════════════════════════════════════════════════════════╗
║  💙 OPCIÓN RECOMENDADA (Balance Calidad/Precio)         TOTAL: $0.311  ⭐   ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  Video360:         FFmpeg-API       $0.011                                   ║
║  Background × 6:   ClipDrop         $0.05 × 6 = $0.30                        ║
║                                     ─────────────────                        ║
║                                     $0.311/vehículo                          ║
╚══════════════════════════════════════════════════════════════════════════════╝

╔══════════════════════════════════════════════════════════════════════════════╗
║  💜 OPCIÓN PREMIUM (Máxima Calidad)                     TOTAL: $1.25        ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  Video360:         Shotstack        $0.05                                    ║
║  Background × 6:   Remove.bg        $0.20 × 6 = $1.20                        ║
║                                     ─────────────────                        ║
║                                     $1.25/vehículo                           ║
╚══════════════════════════════════════════════════════════════════════════════╝

╔══════════════════════════════════════════════════════════════════════════════╗
║  🆓 OPCIÓN GRATUITA (ML Local)                          TOTAL: $0.00        ║
╠══════════════════════════════════════════════════════════════════════════════╣
║  Video360:         FFmpeg Local     $0.00 (requiere servidor con GPU)        ║
║  Background × 6:   U2-Net Local     $0.00 (requiere servidor con GPU)        ║
║                                     ─────────────────                        ║
║                                     $0.00/vehículo                           ║
║                                                                              ║
║  ⚠️ Notas:                                                                   ║
║  - Requiere GPU NVIDIA con mínimo 8GB VRAM                                  ║
║  - Costos de infraestructura: ~$150-300/mes                                 ║
║  - Rentable a partir de ~1,000 vehículos/mes                                ║
╚══════════════════════════════════════════════════════════════════════════════╝
```

## 📊 Proyección de Costos Mensuales

### Por Volumen de Vehículos

| Vehículos/Mes | Económico | Recomendado | Premium    | Local (GPU) |
| ------------- | --------- | ----------- | ---------- | ----------- |
| 50            | $6.45     | $15.55      | $62.50     | ~$150\*     |
| 100           | $12.90    | $31.10      | $125.00    | ~$150\*     |
| 250           | $32.25    | $77.75      | $312.50    | ~$150\*     |
| 500           | $64.50    | $155.50     | $625.00    | ~$200\*     |
| 1,000         | $129.00   | $311.00     | $1,250.00  | ~$250\*     |
| 2,500         | $322.50   | $777.50     | $3,125.00  | ~$300\*     |
| 5,000         | $645.00   | $1,555.00   | $6,250.00  | ~$350\*     |
| 10,000        | $1,290.00 | $3,110.00   | $12,500.00 | ~$400\*     |

> \*Costos de GPU local incluyen: servidor GPU (~$100-250/mes), electricidad, mantenimiento

### Punto de Equilibrio: Local vs Cloud

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    ANÁLISIS DE PUNTO DE EQUILIBRIO                          │
└─────────────────────────────────────────────────────────────────────────────┘

  Costo Mensual ($)
  │
  │
  │         Premium ($1.25/veh)
  │        /
  │       /
  │      /            Recomendado ($0.311/veh)
  │     /            /
  │    /            /       Económico ($0.129/veh)
  │   /            /       /
  │  /            /       /
  │ /            /       /        ┌─── Local GPU (~$200 fijo)
  │/────────────/───────/─────────│───────────────────────────
  │            /       /          │
  │           /       /           │
  │──────────/───────/────────────│──────────────────────────► Vehículos/Mes
  0        500    1,000         2,000

  Puntos de equilibrio con Local:
  • vs Económico:    ~1,550 vehículos/mes
  • vs Recomendado:  ~643 vehículos/mes
  • vs Premium:      ~160 vehículos/mes
```

## 🎯 Recomendaciones por Caso de Uso

### 1. Startup / MVP (< 100 vehículos/mes)

```
Recomendación: ECONÓMICO ($0.129/vehículo)
├── Video360:  ApyHub ($9/mes)
├── Background: Slazzer ($0.02 × 6 = $0.12)
├── Costo mensual: ~$12.90
└── Razón: Mínimo costo mientras validas el mercado
```

### 2. Negocio Establecido (100-500 vehículos/mes)

```
Recomendación: RECOMENDADO ($0.311/vehículo) ⭐
├── Video360:  FFmpeg-API ($11/mes)
├── Background: ClipDrop ($0.05 × 6 = $0.30)
├── Costo mensual: $31-155
└── Razón: Balance óptimo calidad/precio para crecimiento
```

### 3. Dealer Grande (500-2,000 vehículos/mes)

```
Recomendación: HÍBRIDO
├── Primary:   Recomendado ($0.311/vehículo)
├── Fallback:  Económico para overflow
├── Considerar: Migración a Local si > 1,500 vehículos/mes
└── Costo mensual: $155-622
```

### 4. Marketplace / Plataforma (> 2,000 vehículos/mes)

```
Recomendación: LOCAL GPU
├── Infraestructura: GPU server ($200-400/mes)
├── Costo por vehículo: ~$0.10-0.20 (infraestructura/vehículos)
├── Ahorro vs Recomendado: 35-70%
├── Ahorro vs Premium: 80-90%
└── Razón: ROI positivo, control total, sin límites
```

### 5. Concesionario Premium

```
Recomendación: PREMIUM ($1.25/vehículo)
├── Video360:  Shotstack ($50/mes)
├── Background: Remove.bg ($0.20 × 6 = $1.20)
├── Costo mensual: $62.50-625
└── Razón: Máxima calidad para vehículos de alto valor
```

## 📈 ROI del Procesamiento 360°

### Impacto en Ventas

| Métrica                | Sin 360° | Con 360° | Mejora |
| ---------------------- | -------- | -------- | ------ |
| Tiempo en página       | 45s      | 2m 30s   | +233%  |
| Tasa de conversión     | 1.2%     | 2.8%     | +133%  |
| Consultas por vehículo | 3        | 8        | +167%  |
| Días para venta        | 45       | 28       | -38%   |
| Precio de venta        | Base     | +3-5%    | +3-5%  |

### Cálculo de ROI

```
Ejemplo: Dealer con 50 vehículos/mes

INVERSIÓN MENSUAL (Opción Recomendada):
├── Costo 360°: 50 × $0.311 = $15.55
└── TOTAL INVERSIÓN: $15.55/mes

RETORNO MENSUAL:
├── Vehículos adicionales vendidos: +5 (por menor tiempo de venta)
├── Precio adicional por vehículo: +$500 (mejor presentación)
├── Ingreso adicional: 5 × $500 = $2,500
└── ROI: ($2,500 - $15.55) / $15.55 = 16,000% 🚀

Incluso con cálculos conservadores (1 venta adicional + $200/vehículo):
├── Ingreso adicional: 1 × $200 = $200
└── ROI: ($200 - $15.55) / $15.55 = 1,186%
```

## 🔧 Configuración de Fallback

### Orden de Prioridad por Servicio

#### Video360Service

```
1. FFmpeg-API (priority: 100) - DEFAULT
   └── Fallback a: ApyHub
2. ApyHub (priority: 90)
   └── Fallback a: Imgix
3. Imgix (priority: 80)
   └── Fallback a: Cloudinary
4. Cloudinary (priority: 70)
   └── Fallback a: Shotstack
5. Shotstack (priority: 50)
   └── Fallback a: ERROR (notificar admin)
```

#### BackgroundRemovalService

```
1. ClipDrop (priority: 100) - DEFAULT
   └── Fallback a: Slazzer
2. Slazzer (priority: 90)
   └── Fallback a: Photoroom
3. Photoroom (priority: 80)
   └── Fallback a: Clipping Magic
4. Clipping Magic (priority: 70)
   └── Fallback a: Removal.AI
5. Removal.AI (priority: 60)
   └── Fallback a: Remove.bg
6. Remove.bg (priority: 50)
   └── Fallback a: Local ML
7. Local ML (priority: 0)
   └── Fallback a: ERROR (imagen sin procesar)
```

## 🏷️ Créditos y Enlaces

### Proveedores de Video360

| Proveedor  | Website                                  | Documentación                                                                     |
| ---------- | ---------------------------------------- | --------------------------------------------------------------------------------- |
| FFmpeg-API | [ffmpeg-api.com](https://ffmpeg-api.com) | [API Docs](https://ffmpeg-api.com/docs)                                           |
| ApyHub     | [apyhub.com](https://apyhub.com)         | [Video Docs](https://apyhub.com/utility/video)                                    |
| Cloudinary | [cloudinary.com](https://cloudinary.com) | [Video API](https://cloudinary.com/documentation/video_manipulation_and_delivery) |
| Imgix      | [imgix.com](https://imgix.com)           | [Video](https://docs.imgix.com/apis/rendering/video)                              |
| Shotstack  | [shotstack.io](https://shotstack.io)     | [API Docs](https://shotstack.io/docs/guide/)                                      |

### Proveedores de Background Removal

| Proveedor      | Website                                        | Documentación                                    |
| -------------- | ---------------------------------------------- | ------------------------------------------------ |
| ClipDrop       | [clipdrop.co](https://clipdrop.co)             | [API Docs](https://clipdrop.co/apis)             |
| Remove.bg      | [remove.bg](https://remove.bg)                 | [API Docs](https://www.remove.bg/api)            |
| Photoroom      | [photoroom.com](https://photoroom.com)         | [API Docs](https://www.photoroom.com/api)        |
| Slazzer        | [slazzer.com](https://slazzer.com)             | [API Docs](https://slazzer.com/api)              |
| Clipping Magic | [clippingmagic.com](https://clippingmagic.com) | [API Docs](https://clippingmagic.com/api)        |
| Removal.AI     | [removal.ai](https://removal.ai)               | [API Docs](https://removal.ai/api-documentation) |

---

## 📄 Resumen de Documentación

| Documento                                                                | Descripción                       |
| ------------------------------------------------------------------------ | --------------------------------- |
| [README.md](./README.md)                                                 | Visión general del sistema 360°   |
| [01-VISION-GENERAL.md](./01-VISION-GENERAL.md)                           | Flujo paso a paso completo        |
| [02-VIDEO360SERVICE.md](./02-VIDEO360SERVICE.md)                         | Servicio de extracción de frames  |
| [03-BACKGROUNDREMOVALSERVICE.md](./03-BACKGROUNDREMOVALSERVICE.md)       | Servicio de eliminación de fondos |
| [04-VEHICLE360PROCESSINGSERVICE.md](./04-VEHICLE360PROCESSINGSERVICE.md) | Orquestador central               |
| [05-INTEGRACION-FRONTEND.md](./05-INTEGRACION-FRONTEND.md)               | Integración React y Flutter       |
| **06-TABLA-PROVEEDORES-PRECIOS.md**                                      | Este documento (precios y ROI)    |

---

**Última actualización:** Enero 2026  
**Versión:** 1.0.0
