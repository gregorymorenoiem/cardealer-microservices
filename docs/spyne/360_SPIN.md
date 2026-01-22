# 🔄 360° Spin - Guía Completa

**Última actualización:** Enero 21, 2026  
**Estado:** ✅ Implementado

---

## � Acceso y Permisos

> **⚠️ IMPORTANTE:** 360° Spin es una función exclusiva para **Dealers con membresía activa**.

| Tipo de Usuario          | Acceso a 360° Spin | Backgrounds Disponibles        |
| ------------------------ | ------------------ | ------------------------------ |
| **Vendedor Individual**  | ❌ No              | Solo "Blanco Infinito" (16570) |
| **Dealer sin membresía** | ❌ No              | Solo "Blanco Infinito" (16570) |
| **Dealer con membresía** | ✅ Sí              | Todos (16570, 20883)           |
| **Admin**                | ✅ Sí              | Todos                          |

### ¿Por qué esta restricción?

- **Calidad de la plataforma**: El fondo "Blanco Infinito" está disponible para TODOS los vendedores, asegurando que todas las publicaciones tengan calidad profesional.
- **Valor para Dealers**: El 360° Spin y el fondo "Showroom Gris" son funcionalidades premium que justifican la membresía.
- **El Dealer elige**: Los dealers pueden elegir entre "Blanco Infinito" o "Showroom Gris" según su preferencia.

---

## �📋 Índice

1. [¿Qué es 360° Spin?](#-qué-es-360-spin)
2. [Cómo Funciona](#-cómo-funciona)
3. [Requisitos de Imágenes](#-requisitos-de-imágenes)
4. [API Endpoints](#-api-endpoints)
5. [Ejemplos de Uso](#-ejemplos-de-uso)
6. [Integración en Frontend](#-integración-en-frontend)
7. [Tiempos de Procesamiento](#-tiempos-de-procesamiento)

---

## 🎯 ¿Qué es 360° Spin?

360° Spin es una funcionalidad de Spyne AI que crea una **vista interactiva 360°** de un vehículo a partir de múltiples fotografías tomadas alrededor del mismo.

### Beneficios

| Característica  | Descripción                                           |
| --------------- | ----------------------------------------------------- |
| **Interactivo** | El usuario puede rotar el vehículo con el mouse/touch |
| **Profesional** | Fondos de estudio aplicados automáticamente           |
| **Inmersivo**   | Experiencia de showroom virtual                       |
| **Embebible**   | Se integra fácilmente en cualquier web                |

### Ejemplo Visual

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│                       📸 INPUT (6-72 fotos)                     │
│                                                                 │
│    📷1 → 📷2 → 📷3 → 📷4 → 📷5 → 📷6 → ... → 📷36              │
│    0°    10°   20°   30°   40°   50°        360°               │
│                                                                 │
│                            ⬇️                                   │
│                    🤖 Spyne AI Processing                       │
│                            ⬇️                                   │
│                                                                 │
│                    🔄 OUTPUT (360° Spin)                        │
│                    ┌─────────────────┐                         │
│                    │    ◀️ 🚗 ▶️      │                         │
│                    │    Arrastrar    │                         │
│                    └─────────────────┘                         │
│                    URL embebible                               │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## ⚙️ Cómo Funciona

### 1. Captura de Imágenes

Toma fotografías del vehículo desde diferentes ángulos:

```
     📷 Front (0°)
         │
   📷    │    📷
  315°   │   45°
    \    │    /
     \   │   /
📷────── 🚗 ──────📷
270°    / | \    90°
       /  │  \
     📷   │   📷
    225°  │  135°
         │
     📷 Rear (180°)
```

### 2. Envío a Spyne

```json
POST /api/vehicle-images/spin
{
  "imageUrls": [
    "https://tu-servidor.com/fotos/angulo-0.jpg",
    "https://tu-servidor.com/fotos/angulo-60.jpg",
    "https://tu-servidor.com/fotos/angulo-120.jpg",
    "https://tu-servidor.com/fotos/angulo-180.jpg",
    "https://tu-servidor.com/fotos/angulo-240.jpg",
    "https://tu-servidor.com/fotos/angulo-300.jpg"
  ],
  "stockNumber": "VEH-001",
  "backgroundId": "20883"
}
```

### 3. Procesamiento por IA

Spyne automáticamente:

1. **Detecta el vehículo** en cada imagen
2. **Reemplaza fondos** con fondo de estudio
3. **Normaliza exposición** entre frames
4. **Genera el 360° interactivo** con transiciones suaves

### 4. Resultado

```json
{
  "jobId": "xxx",
  "status": "completed",
  "embedUrl": "https://spyne-player.com/spin/xxx",
  "processedImages": [...]
}
```

---

## 📸 Requisitos de Imágenes

### Cantidad de Imágenes

| Cantidad | Resultado                        | Recomendación    |
| -------- | -------------------------------- | ---------------- |
| 6        | Básico (60° entre frames)        | Mínimo aceptable |
| 12       | Bueno (30° entre frames)         | Economía         |
| 24       | Muy bueno (15° entre frames)     | Balance          |
| **36**   | **Excelente (10° entre frames)** | **Recomendado**  |
| 72       | Ultra suave (5° entre frames)    | Premium          |

### Mejores Prácticas de Captura

✅ **HACER:**

- Usar trípode o estabilizador
- Mantener altura consistente (nivel del hood/capó)
- Mantener distancia consistente al vehículo
- Iluminación uniforme
- Ángulos equidistantes (cada 10°, 15°, 30°, etc.)
- Capturar el vehículo completo en cada foto

❌ **EVITAR:**

- Variar altura entre fotos
- Acercarse/alejarse entre fotos
- Sombras fuertes o cambios de luz
- Objetos en movimiento en fondo
- Vehículo cortado en cualquier frame

### Especificaciones Técnicas

| Aspecto     | Mínimo     | Recomendado      |
| ----------- | ---------- | ---------------- |
| Resolución  | 1280x720   | 1920x1080+       |
| Formato     | JPG, PNG   | JPG              |
| Tamaño      | -          | < 5MB por imagen |
| Orientación | Horizontal | Horizontal       |
| Aspecto     | 16:9 o 4:3 | 16:9             |

---

## 🌐 API Endpoints

### POST `/api/vehicle-images/spin`

Genera un 360° Spin a partir de múltiples imágenes.

**Request:**

```json
{
  "imageUrls": ["url1", "url2", ...], // 6-72 URLs
  "vin": "1HGCM82633A123456",         // Opcional
  "stockNumber": "STOCK-001",         // Opcional
  "backgroundId": "20883",            // Default: 20883
  "maskLicensePlate": true,           // Default: true
  "enableHotspots": true              // Default: true
}
```

**Response (202 Accepted):**

```json
{
  "jobId": "6410b405-b9f5-4b21-8ecf-4d78a51ae165",
  "status": "processing",
  "message": "360° spin generation started with 36 images. Estimated processing time: 7 minutes.",
  "imageCount": 36,
  "estimatedMinutes": 7,
  "checkStatusUrl": "/api/vehicle-images/spin/status/6410b405-..."
}
```

### GET `/api/vehicle-images/spin/status/{jobId}`

Verifica el estado del 360° Spin.

**Response (200 OK):**

```json
{
  "jobId": "6410b405-b9f5-4b21-8ecf-4d78a51ae165",
  "status": "completed",
  "spinId": "spin-abc123",
  "embedUrl": "https://spyne-player.com/spin/abc123",
  "spinAiStatus": "DONE",
  "processedImages": [
    {
      "imageId": "img-001",
      "frameNumber": "1",
      "originalUrl": "https://...",
      "processedUrl": "https://spyne-media.s3.amazonaws.com/...",
      "status": "COMPLETED",
      "category": "Exterior",
      "angle": 0
    }
    // ... más imágenes
  ],
  "totalFrames": 36,
  "completedFrames": 36
}
```

---

## 📝 Ejemplos de Uso

### Ejemplo Básico (6 imágenes)

```bash
curl -X POST http://localhost:15070/api/vehicle-images/spin \
  -H "Content-Type: application/json" \
  -d '{
    "imageUrls": [
      "https://ejemplo.com/frente.jpg",
      "https://ejemplo.com/frente-derecha.jpg",
      "https://ejemplo.com/lateral-derecha.jpg",
      "https://ejemplo.com/trasera.jpg",
      "https://ejemplo.com/lateral-izquierda.jpg",
      "https://ejemplo.com/frente-izquierda.jpg"
    ],
    "stockNumber": "SPIN-001"
  }'
```

### Ejemplo con 36 imágenes (Recomendado)

```bash
# Generar array de URLs (ejemplo con imágenes numeradas)
curl -X POST http://localhost:15070/api/vehicle-images/spin \
  -H "Content-Type: application/json" \
  -d '{
    "imageUrls": [
      "https://cdn.example.com/vehicles/VEH-001/spin/001.jpg",
      "https://cdn.example.com/vehicles/VEH-001/spin/002.jpg",
      "https://cdn.example.com/vehicles/VEH-001/spin/003.jpg",
      ... (hasta 36 imágenes)
    ],
    "stockNumber": "VEH-001",
    "vin": "1HGCM82633A123456",
    "backgroundId": "20883",
    "maskLicensePlate": true,
    "enableHotspots": true
  }'
```

### Verificar Estado

```bash
curl http://localhost:15070/api/vehicle-images/spin/status/{jobId}
```

---

## 🖥️ Integración en Frontend

### React Component

```tsx
import { useState, useEffect } from "react";

interface SpinViewerProps {
  vehicleId: string;
  spinJobId: string;
}

export function SpinViewer({ vehicleId, spinJobId }: SpinViewerProps) {
  const [spinData, setSpinData] = useState<SpinStatus | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const pollStatus = async () => {
      const response = await fetch(
        `/api/spyne/vehicle-images/spin/status/${spinJobId}`,
      );
      const data = await response.json();
      setSpinData(data);

      if (data.status === "completed") {
        setLoading(false);
      } else if (data.status === "processing") {
        // Poll again in 10 seconds
        setTimeout(pollStatus, 10000);
      }
    };

    pollStatus();
  }, [spinJobId]);

  if (loading) {
    return (
      <div className="spin-loading">
        <p>Generando vista 360°...</p>
        <progress
          value={spinData?.completedFrames}
          max={spinData?.totalFrames}
        />
        <span>
          {spinData?.completedFrames}/{spinData?.totalFrames} frames
        </span>
      </div>
    );
  }

  // Embed the 360° viewer
  return (
    <div className="spin-viewer">
      <iframe
        src={spinData?.embedUrl}
        width="100%"
        height="500"
        frameBorder="0"
        allowFullScreen
        title={`360° View - ${vehicleId}`}
      />
    </div>
  );
}
```

### HTML Embed

```html
<!-- Cuando el spin está completado -->
<iframe
  src="https://spyne-player.com/spin/{spinId}"
  width="800"
  height="600"
  frameborder="0"
  allowfullscreen
>
</iframe>
```

---

## ⏱️ Tiempos de Procesamiento

| Cantidad de Imágenes | Tiempo Estimado |
| -------------------- | --------------- |
| 6 imágenes           | 2-3 minutos     |
| 12 imágenes          | 3-4 minutos     |
| 24 imágenes          | 5-6 minutos     |
| 36 imágenes          | 6-8 minutos     |
| 72 imágenes          | 8-12 minutos    |

### Factores que Afectan el Tiempo

- **Resolución de imágenes** - Mayor resolución = más tiempo
- **Complejidad del fondo** - Fondos complicados tardan más
- **Carga del servidor Spyne** - Varía según demanda
- **Calidad del vehículo** - Vehículos con muchos detalles

### Recomendaciones

1. **Usar Webhooks** en producción para no hacer polling
2. **Mostrar progreso** al usuario mientras procesa
3. **Cachear resultados** - El embedUrl es permanente
4. **Comprimir imágenes** antes de enviar (< 5MB cada una)

---

## 🎛️ Opciones Avanzadas

### Hotspots

Puntos interactivos que se pueden agregar al 360° para marcar:

- Daños del vehículo
- Características especiales
- Puntos de interés

```json
{
  "enableHotspots": true
}
```

### Backgrounds Disponibles

| ID      | Nombre          | Acceso                               |
| ------- | --------------- | ------------------------------------ |
| `16570` | Blanco Infinito | ✅ **TODOS** (Individual + Dealer)   |
| `20883` | Showroom Gris   | 🔒 Solo Dealers con membresía activa |

> **Nota**: El Dealer puede elegir cuál de los dos fondos usar para su 360° Spin.
> Para backgrounds personalizados adicionales, contacta a Spyne.

---

## 🔧 Troubleshooting

### Error: "Minimum 6 images required"

**Causa:** Se enviaron menos de 6 imágenes.

**Solución:** Agregar más imágenes al array `imageUrls`.

### Status permanece en "processing"

**Causa:** El 360° Spin tarda 3-10 minutos en procesar.

**Solución:**

- Esperar más tiempo (hasta 15 minutos para 72 imágenes)
- Verificar que las URLs de las imágenes son accesibles
- Configurar webhook para notificación automática

### Spin se ve "jumpy" o entrecortado

**Causa:** Pocas imágenes o ángulos inconsistentes.

**Solución:**

- Usar 36+ imágenes
- Asegurar ángulos equidistantes
- Mantener altura y distancia consistentes

---

## 📚 Referencias

- [Spyne Docs - 360° Spin](https://docs.spyne.ai/reference/360-spin)
- [Spyne Docs - Merchandise Process](https://docs.spyne.ai/docs/transform-your-first-vehicle-1)
- [Best Practices for 360° Photography](https://www.spyne.ai/blog/360-car-photography)

---

**Autor:** Equipo OKLA  
**Versión:** 1.0.0  
**Última actualización:** Enero 21, 2026
