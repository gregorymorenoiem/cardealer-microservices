# 🎨 Background Replacement - Guía Completa

**Última actualización:** Enero 21, 2026  
**Basado en:** Documentación oficial Spyne (https://docs.spyne.ai)

---

## 📋 Índice

1. [¿Qué es Background Replacement?](#-qué-es-background-replacement)
2. [Cómo Funciona](#-cómo-funciona)
3. [Parámetros de Procesamiento](#-parámetros-de-procesamiento)
4. [Backgrounds Disponibles](#-backgrounds-disponibles)
5. [Número de Placa](#-número-de-placa)
6. [Clasificación de Imágenes](#-clasificación-de-imágenes)
7. [Calidad de Imagen](#-calidad-de-imagen)
8. [Flujo de Procesamiento](#-flujo-de-procesamiento)
9. [Ejemplos de Request](#-ejemplos-de-request)
10. [Otros Productos Spyne](#-otros-productos-spyne)

---

## 🎯 ¿Qué es Background Replacement?

Background Replacement es la característica principal de Spyne AI que **reemplaza automáticamente el fondo de las fotos de vehículos** por fondos profesionales de estudio.

### Beneficios

| Antes                                            | Después                          |
| ------------------------------------------------ | -------------------------------- |
| Fondo desordenado (estacionamiento, calle, etc.) | Fondo de showroom profesional    |
| Sombras inconsistentes                           | Iluminación uniforme             |
| Distracciones visuales                           | Enfoque 100% en el vehículo      |
| Apariencia amateur                               | Calidad de concesionario premium |

### Ejemplo Visual

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│   📷 IMAGEN ORIGINAL              🎨 IMAGEN PROCESADA           │
│   ┌─────────────────┐             ┌─────────────────┐          │
│   │   🏬🌳🚗🌳🏢   │    ──>     │   ⬜⬜🚗⬜⬜   │          │
│   │   (parking lot) │             │   (studio bg)   │          │
│   └─────────────────┘             └─────────────────┘          │
│                                                                 │
│   Input URL                       Output URL                    │
│   (tu imagen original)            (imagen procesada en S3)     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## ⚙️ Cómo Funciona

### 1. Envío de Imagen

Envías la URL de la imagen original a Spyne:

```json
{
  "stockNumber": "VIN-12345",
  "media": { "image": true },
  "mediaInput": {
    "imageData": [{ "url": "https://tu-servidor.com/fotos/carro.jpg" }]
  }
}
```

### 2. Procesamiento por IA

Spyne realiza automáticamente:

1. **Detección del Vehículo** - Identifica el vehículo en la imagen
2. **Segmentación** - Separa el vehículo del fondo
3. **Clasificación** - Categoriza como Exterior, Interior, o Misc
4. **Reemplazo de Fondo** - Aplica el background seleccionado
5. **Corrección de Sombras** - Genera sombras naturales
6. **Optimización** - Mejora colores y exposición

### 3. Resultado

Spyne retorna URLs de las imágenes procesadas:

```json
{
  "imageData": [
    {
      "inputImage": "https://tu-servidor.com/fotos/carro.jpg",
      "outputImage": "https://spyne-media.s3.amazonaws.com/.../processed.jpg",
      "backgroundId": "20883",
      "category": "Exterior"
    }
  ]
}
```

---

## 🎛️ Parámetros de Procesamiento

### processingDetails

Este es el objeto que controla CÓMO se procesa la imagen:

```json
{
  "processingDetails": {
    "backgroundId": "20883",
    "numberPlateLogo": 1,
    "image": {
      "backgroundType": "legacy"
    },
    "extractCatalogCount": 5
  }
}
```

### Desglose de Parámetros

| Parámetro              | Tipo       | Descripción               | Valores                    |
| ---------------------- | ---------- | ------------------------- | -------------------------- |
| `backgroundId`         | string     | ID del fondo a aplicar    | `"20883"`, `"16570"`, etc. |
| `numberPlateLogo`      | int/string | Enmascarar placa          | `0`, `1`, o URL de imagen  |
| `image.backgroundType` | string     | Tipo de procesamiento     | `"legacy"` (recomendado)   |
| `extractCatalogCount`  | int        | Frames a extraer de video | `1` a `36`                 |

---

## 🖼️ Backgrounds Disponibles

### Cómo Obtener tus Backgrounds

Los backgroundIds disponibles dependen de tu cuenta Spyne. Contacta a tu representante de Spyne para obtener la lista.

### Backgrounds de Ejemplo (Documentación)

| ID      | Descripción          | Uso Recomendado                     |
| ------- | -------------------- | ----------------------------------- |
| `20883` | Showroom gris neutro | **Default** - Ideal para la mayoría |
| `16570` | Blanco infinito      | Minimalista, estilo catálogo        |

### Visualización

```
┌──────────────────┬──────────────────┐
│    ID: 20883     │    ID: 16570     │
│  ┌────────────┐  │  ┌────────────┐  │
│  │   🚗       │  │  │   🚗       │  │
│  │ ▓▓▓▓▓▓▓▓▓▓ │  │  │ ░░░░░░░░░░ │  │
│  │  Gray BG   │  │  │  White BG  │  │
│  └────────────┘  │  └────────────┘  │
│  Showroom Gray   │  Infinite White  │
└──────────────────┴──────────────────┘
```

### Custom Backgrounds

Spyne puede configurar backgrounds personalizados para tu empresa:

- Tu showroom real
- Colores corporativos
- Escenas específicas (playa, montaña, ciudad)

> 📧 Contacta a support@spyne.ai para backgrounds custom.

---

## 🔢 Número de Placa

Spyne puede **enmascarar automáticamente las placas** para proteger la privacidad.

### Opciones

| Valor   | Resultado              | Ejemplo          |
| ------- | ---------------------- | ---------------- |
| `0`     | Sin enmascarar         | La placa visible |
| `1`     | Blur/Rectángulo blanco | █████████        |
| `"URL"` | Logo personalizado     | 🏢 (tu logo)     |

### Ejemplo en Request

```json
// Sin enmascarar
"processingDetails": {
  "numberPlateLogo": 0
}

// Blur automático (recomendado)
"processingDetails": {
  "numberPlateLogo": 1
}

// Logo personalizado del dealer
"processingDetails": {
  "numberPlateLogo": "https://mi-dealer.com/logo-placa.png"
}
```

### Visualización

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│  numberPlateLogo: 0        numberPlateLogo: 1               │
│  ┌─────────────┐           ┌─────────────┐                 │
│  │    🚗       │           │    🚗       │                 │
│  │  [ABC-123]  │           │  [███████]  │                 │
│  └─────────────┘           └─────────────┘                 │
│  Placa visible             Placa enmascarada               │
│                                                             │
│  numberPlateLogo: "URL"                                    │
│  ┌─────────────┐                                           │
│  │    🚗       │                                           │
│  │  [MI LOGO]  │                                           │
│  └─────────────┘                                           │
│  Logo del dealer                                           │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 📁 Clasificación de Imágenes

Spyne clasifica automáticamente cada imagen en **3 categorías**:

### Categorías

| Categoría    | Descripción                     | Procesamiento            |
| ------------ | ------------------------------- | ------------------------ |
| **Exterior** | Vista exterior del vehículo     | Background reemplazado   |
| **Interior** | Dashboard, asientos, etc.       | Sin cambio de background |
| **Misc**     | Detalles (motor, llantas, etc.) | Sin cambio de background |

### Comportamiento por Categoría

```
┌────────────────────────────────────────────────────────────────┐
│                                                                │
│  📸 EXTERIOR                                                   │
│  ├── Background: REEMPLAZADO por backgroundId                │
│  ├── Sombras: Generadas automáticamente                      │
│  └── Resultado: Imagen con fondo de estudio                  │
│                                                                │
│  📸 INTERIOR                                                   │
│  ├── Background: NO CAMBIA (es interior del vehículo)        │
│  ├── Mejoras: Color, exposición, nitidez                     │
│  └── Resultado: Interior mejorado                            │
│                                                                │
│  📸 MISC (Detalles)                                           │
│  ├── Background: Puede o no cambiar según contexto           │
│  ├── Mejoras: Nitidez, detalle                               │
│  └── Resultado: Detalle mejorado                             │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

### Ángulos Detectados (Exterior)

Spyne también detecta el ángulo de la foto exterior:

| Ángulo          | Descripción          |
| --------------- | -------------------- |
| `front`         | Vista frontal        |
| `rear`          | Vista trasera        |
| `side`          | Vista lateral        |
| `front-quarter` | Tres cuartos frontal |
| `rear-quarter`  | Tres cuartos trasero |

---

## 📊 Calidad de Imagen

### Requisitos de Entrada

| Aspecto         | Mínimo         | Recomendado        |
| --------------- | -------------- | ------------------ |
| **Resolución**  | 800x600        | 1920x1080+         |
| **Formato**     | JPG, PNG, WebP | JPG                |
| **Tamaño**      | -              | < 10MB             |
| **Orientación** | Horizontal     | Horizontal         |
| **Iluminación** | Visible        | Luz natural difusa |

### Mejores Prácticas

✅ **HACER:**

- Centrar el vehículo en el frame
- Capturar el vehículo completo
- Usar iluminación uniforme
- Fotografiar en horizontal

❌ **EVITAR:**

- Vehículos cortados
- Sombras fuertes
- Objetos obstruyendo el vehículo
- Fotos muy oscuras o sobreexpuestas

---

## 🔄 Flujo de Procesamiento

### Estados del Procesamiento

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│  1. SUBMITTED                                                   │
│     └── Imagen enviada, esperando en cola                      │
│             │                                                   │
│             ▼                                                   │
│  2. PROCESSING                                                  │
│     └── IA procesando (60-120 segundos)                        │
│             │                                                   │
│             ▼                                                   │
│  3. DONE / FAILED                                               │
│     ├── DONE: outputImage disponible                           │
│     └── FAILED: Error en procesamiento                         │
│             │                                                   │
│             ▼ (si QC habilitado)                               │
│  4. QC_PENDING → QC_DONE / QC_REJECTED                         │
│     └── Revisión humana del resultado                          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Tiempos Típicos

| Etapa              | Tiempo          |
| ------------------ | --------------- |
| En cola            | 0-30 segundos   |
| Procesamiento AI   | 60-120 segundos |
| QC (si habilitado) | 1-24 horas      |
| **Total (sin QC)** | **2-3 minutos** |

### Polling vs Webhook

**Polling (no recomendado para producción):**

```
Tu Server ──GET status──> Spyne ──"PROCESSING"──> Tu Server
    │                                                  │
    └──── esperar 15 seg ────────────────────────────┘
    │
Tu Server ──GET status──> Spyne ──"DONE"──────> Tu Server
```

**Webhook (recomendado):**

```
Tu Server ──POST process──> Spyne
    │
    │  (Spyne procesa en background)
    │
    ▼
Spyne ──POST webhook──> Tu Server (cuando termina)
```

---

## 📝 Ejemplos de Request

### Ejemplo Básico

```json
POST https://api.spyne.ai/api/pv1/merchandise/process
Authorization: Bearer YOUR_API_KEY
Content-Type: application/json

{
  "stockNumber": "TEST-001",
  "vin": "1HGCM82633A123456",
  "media": {
    "image": true
  },
  "mediaInput": {
    "imageData": [
      {
        "url": "https://ejemplo.com/mi-foto.jpg"
      }
    ]
  },
  "processingDetails": {
    "backgroundId": "20883",
    "image": {
      "backgroundType": "legacy"
    }
  }
}
```

### Ejemplo con Múltiples Imágenes

```json
{
  "stockNumber": "BATCH-001",
  "media": { "image": true },
  "mediaInput": {
    "imageData": [
      { "url": "https://ejemplo.com/frente.jpg" },
      { "url": "https://ejemplo.com/lateral.jpg" },
      { "url": "https://ejemplo.com/trasera.jpg" },
      { "url": "https://ejemplo.com/interior-1.jpg" },
      { "url": "https://ejemplo.com/interior-2.jpg" }
    ]
  },
  "processingDetails": {
    "backgroundId": "20883",
    "numberPlateLogo": 1,
    "image": { "backgroundType": "legacy" }
  }
}
```

### Ejemplo con Logo en Placa

```json
{
  "stockNumber": "DEALER-001",
  "media": { "image": true },
  "mediaInput": {
    "imageData": [{ "url": "https://ejemplo.com/vehiculo.jpg" }]
  },
  "processingDetails": {
    "backgroundId": "20883",
    "numberPlateLogo": "https://mi-dealer.com/assets/plate-logo.png",
    "image": { "backgroundType": "legacy" }
  }
}
```

### Respuesta Exitosa

```json
{
  "vin": "1HGCM82633A123456",
  "dealerId": "9d2c25f546",
  "dealerVinId": "e2adca58-66bc-4d56-ad16-f73823af9ba1",
  "mediaData": {
    "image": {
      "skuId": "TEST-001-img",
      "aiStatus": "DONE",
      "qcStatus": "pending",
      "imageData": [
        {
          "status": "COMPLETED",
          "frameNo": 1,
          "imageId": "img_001",
          "imageName": "frente.jpg",
          "inputImage": "https://ejemplo.com/frente.jpg",
          "outputImage": "https://spyne-media.s3.amazonaws.com/2026-01-21/abc123.jpg",
          "backgroundId": "20883",
          "category": "Exterior",
          "angle": "front"
        },
        {
          "status": "COMPLETED",
          "frameNo": 2,
          "imageId": "img_002",
          "imageName": "interior-1.jpg",
          "inputImage": "https://ejemplo.com/interior-1.jpg",
          "outputImage": "https://spyne-media.s3.amazonaws.com/2026-01-21/def456.jpg",
          "backgroundId": null,
          "category": "Interior",
          "angle": null
        }
      ]
    }
  }
}
```

---

## 🎬 Otros Productos Spyne

Además de Background Replacement, Spyne ofrece:

### 1. 360° Spin

Crea vistas 360° interactivas del vehículo.

```json
{
  "media": {
    "image": true,
    "spin": true // ← Habilitar 360°
  },
  "mediaInput": {
    "spinData": {
      "interiorSpin": false,
      "spinType": "turntable"
    }
  }
}
```

### 2. Feature Video

Video automático mostrando características del vehículo.

```json
{
  "media": {
    "image": true,
    "featureVideo": true // ← Habilitar video
  }
}
```

### 3. Hotspots

Puntos interactivos en la imagen (daños, características).

```json
{
  "processingDetails": {
    "hotspots": true
  }
}
```

### 4. Window Sticker

Genera window sticker automático.

### 5. Video Upload

Sube video y extrae frames automáticamente:

```json
{
  "media": { "image": true },
  "mediaInput": {
    "video": {
      "url": "https://ejemplo.com/video-vehiculo.mp4"
    }
  },
  "processingDetails": {
    "extractCatalogCount": 8 // Extraer 8 frames
  }
}
```

> 📝 Estos productos pueden requerir activación en tu cuenta. Contacta a Spyne.

---

## 📊 Resumen de processingDetails

```json
{
  "processingDetails": {
    // BACKGROUND REPLACEMENT
    "backgroundId": "20883", // ID del fondo
    "image": {
      "backgroundType": "legacy" // Tipo de procesamiento
    },

    // LICENSE PLATE
    "numberPlateLogo": 1, // 0, 1, o URL

    // VIDEO EXTRACTION
    "extractCatalogCount": 8, // Frames a extraer

    // OPTIONAL FEATURES
    "hotspots": true, // Puntos interactivos
    "windowSticker": true // Window sticker
  }
}
```

---

## 🎯 Checklist de Implementación

### Para OKLA

- [x] Configurar API Key en appsettings.json
- [x] Implementar SpyneApiClient con Bearer auth
- [x] Usar backgroundId "20883" por default
- [x] Mapear correctamente `mediaData` (no `outputData`)
- [x] Implementar endpoint /transform
- [x] Implementar endpoint /status/{jobId}
- [ ] Configurar webhook en Spyne Console
- [ ] Implementar endpoint de webhook
- [ ] Agregar retry logic para polling
- [ ] Cachear resultados en Redis
- [ ] UI para seleccionar background

### Para Dealers

- [ ] Subir foto del vehículo
- [ ] Seleccionar background (dropdown)
- [ ] Ver preview del resultado
- [ ] Descargar imagen procesada
- [ ] Bulk upload de múltiples fotos

---

## 📚 Referencias

- [Spyne Docs - Background Replacement](https://docs.spyne.ai/docs/background-replacement)
- [Spyne Docs - Merchandise Process](https://docs.spyne.ai/reference/merchandiseprocessusingpost)
- [Spyne Docs - Get Media](https://docs.spyne.ai/docs/get-media)
- [Spyne Console](https://console.spyne.ai/)

---

**Autor:** Equipo OKLA  
**Versión:** 1.0.0  
**Última actualización:** Enero 21, 2026
