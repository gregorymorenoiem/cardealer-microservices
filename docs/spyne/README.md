# � Documentación Spyne AI - OKLA

Bienvenido a la documentación de integración de Spyne AI para OKLA.

## 📁 Estructura de Documentación

| Archivo                                                | Descripción                            |
| ------------------------------------------------------ | -------------------------------------- |
| [API_CONFIGURATION.md](API_CONFIGURATION.md)           | Guía completa de configuración del API |
| [BACKGROUND_REPLACEMENT.md](BACKGROUND_REPLACEMENT.md) | Todo sobre Background Replacement      |
| [360_SPIN.md](360_SPIN.md)                             | **Guía de 360° Spin interactivo**      |

---

## 🚀 Quick Start

### 1. Obtener API Key

```bash
# Visitar https://console.spyne.ai/ > Developer Hub > API Keys
# Generar nueva key y copiarla
```

### 2. Configurar Variables

```env
SpyneApi__ApiKey=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
SpyneApi__BaseUrl=https://api.spyne.ai/api
SpyneApi__TimeoutSeconds=120
```

### 3. Transformar Primera Imagen

```bash
curl -X POST http://localhost:15070/api/vehicle-images/transform \
  -H "Content-Type: application/json" \
  -d '{"imageUrl": "https://example.com/car.jpg", "stockNumber": "TEST-001"}'
```

### 4. Verificar Resultado

```bash
curl http://localhost:15070/api/vehicle-images/status/{jobId}
```

---

## 📊 SpyneIntegrationService

| Puerto | Host Docker                  | Gateway Route |
| ------ | ---------------------------- | ------------- |
| 15070  | spyneintegrationservice:8080 | /api/spyne/\* |

### Endpoints Disponibles

| Método | Endpoint                                  | Acceso     | Descripción                   |
| ------ | ----------------------------------------- | ---------- | ----------------------------- |
| `GET`  | `/api/vehicle-images/features`            | ✅ Público | Ver features disponibles      |
| `GET`  | `/api/vehicle-images/backgrounds`         | ✅ Público | Listar backgrounds (filtrado) |
| `POST` | `/api/vehicle-images/transform`           | ✅ Todos   | Transformar imagen            |
| `POST` | `/api/vehicle-images/transform/batch`     | ✅ Todos   | Transformar múltiples         |
| `POST` | `/api/vehicle-images/spin`                | 🔒 Dealers | **Generar 360° Spin**         |
| `GET`  | `/api/vehicle-images/status/{jobId}`      | ✅ Todos   | Estado del job                |
| `GET`  | `/api/vehicle-images/spin/status/{jobId}` | ✅ Todos   | **Estado del 360° Spin**      |
| `GET`  | `/health`                                 | ✅ Público | Health check                  |

---

## 🔗 Enlaces Útiles

- [Spyne Console](https://console.spyne.ai/)
- [Documentación Oficial](https://docs.spyne.ai/docs)
- [API Reference](https://docs.spyne.ai/reference)
- [Transform your first Vehicle](https://docs.spyne.ai/docs/transform-your-first-vehicle-1)

---

## 🎯 Funcionalidades de Spyne

### ✅ Implementadas en OKLA

| Feature                | Estado | Acceso                            | Descripción                             |
| ---------------------- | ------ | --------------------------------- | --------------------------------------- |
| Background Replacement | ✅     | ✅ Todos (Blanco Infinito)        | Reemplazar fondos por estudio           |
| License Plate Masking  | ✅     | ✅ Todos                          | Enmascarar placas                       |
| Image Classification   | ✅     | ✅ Todos                          | Clasificar Exterior/Interior/Misc       |
| Showroom Background    | ✅     | 🔒 Solo Dealers con membresía     | Background Showroom Gris (20883)        |
| **360° Spin**          | ✅     | 🔒 **Solo Dealers con membresía** | **Vista interactiva 360° del vehículo** |

### 🔒 Política de Acceso

| Tipo de Usuario          | Blanco Infinito (16570) | Showroom Gris (20883) | 360° Spin |
| ------------------------ | ----------------------- | --------------------- | --------- |
| **Vendedor Individual**  | ✅ Gratis               | ❌                    | ❌        |
| **Dealer sin membresía** | ✅ Gratis               | ❌                    | ❌        |
| **Dealer con membresía** | ✅ Incluido             | ✅ Incluido           | ✅        |

> **Nota**: El fondo "Blanco Infinito" está disponible para TODOS los vendedores para mantener la calidad visual de la plataforma.

### 🔜 Próximamente

| Feature       | Estado | Acceso     | Descripción                   |
| ------------- | ------ | ---------- | ----------------------------- |
| Feature Video | 🔜     | 🔒 Dealers | Videos automáticos            |
| Hotspots      | 🔜     | 🔒 Dealers | Puntos interactivos           |
| Webhooks      | 🔜     | ✅ Todos   | Notificaciones en tiempo real |

---

**Puerto Local:** 15070  
**Gateway:** http://localhost:18443/api/spyne/\*
