# 🚗 CarDealer API Tutorial - Guía Completa

Este documento proporciona un tutorial exhaustivo sobre cómo utilizar la API de CarDealer, explicando cada flujo de trabajo y proceso disponible en la plataforma.

---

## 📋 Tabla de Contenidos

1. [Introducción](#introducción)
2. [Arquitectura de la API](#arquitectura-de-la-api)
3. [Autenticación](#autenticación)
4. [Flujos de Usuario](#flujos-de-usuario)
5. [Gestión de Vehículos](#gestión-de-vehículos)
6. [Sistema de Búsqueda](#sistema-de-búsqueda)
7. [Proceso de Contacto](#proceso-de-contacto)
8. [Sistema de Pagos](#sistema-de-pagos)
9. [Gestión de Dealers](#gestión-de-dealers)
10. [Notificaciones](#notificaciones)
11. [Media y Archivos](#media-y-archivos)
12. [Ejemplos Prácticos](#ejemplos-prácticos)

---

## Introducción

CarDealer es una plataforma de marketplace para compra y venta de vehículos implementada con arquitectura de microservicios. La API está expuesta a través de un **API Gateway (Ocelot)** que enruta las peticiones a los servicios correspondientes.

### URL Base

| Ambiente       | URL                       |
| -------------- | ------------------------- |
| **Producción** | `https://api.okla.com.do` |
| **Desarrollo** | `http://localhost:18443`  |

### Formato de Respuesta

Todas las respuestas siguen el formato JSON estándar:

```json
{
  "success": true,
  "data": { ... },
  "message": "Operación exitosa",
  "errors": []
}
```

### Códigos de Estado HTTP

| Código | Significado                |
| ------ | -------------------------- |
| `200`  | Éxito                      |
| `201`  | Recurso creado             |
| `400`  | Error de validación        |
| `401`  | No autenticado             |
| `403`  | No autorizado              |
| `404`  | Recurso no encontrado      |
| `429`  | Rate limit excedido        |
| `500`  | Error interno del servidor |

---

## Arquitectura de la API

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              CLIENTES                                       │
│                    (Web App, Mobile App, Third Party)                       │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         API GATEWAY (Ocelot)                                │
│                     https://api.okla.com.do                                 │
│                                                                             │
│  • Autenticación JWT          • Rate Limiting                               │
│  • Enrutamiento               • Load Balancing                              │
│  • CORS                       • Circuit Breaker                             │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
         ┌───────────────────────────┼───────────────────────────┐
         │                           │                           │
         ▼                           ▼                           ▼
┌─────────────────┐     ┌─────────────────────┐     ┌─────────────────┐
│   AuthService   │     │ VehiclesSaleService │     │  BillingService │
│   (Puerto 8080) │     │    (Puerto 8080)    │     │  (Puerto 8080)  │
└─────────────────┘     └─────────────────────┘     └─────────────────┘
         │                           │                           │
         ▼                           ▼                           ▼
┌─────────────────┐     ┌─────────────────────┐     ┌─────────────────┐
│   UserService   │     │    SearchService    │     │  MediaService   │
│   (Puerto 8080) │     │    (Puerto 8080)    │     │  (Puerto 8080)  │
└─────────────────┘     └─────────────────────┘     └─────────────────┘
```

### Servicios Principales

| Servicio                | Ruta Base                       | Descripción             |
| ----------------------- | ------------------------------- | ----------------------- |
| AuthService             | `/api/auth`                     | Autenticación y tokens  |
| UserService             | `/api/users`                    | Gestión de usuarios     |
| VehiclesSaleService     | `/api/vehicles`, `/api/catalog` | Vehículos y catálogo    |
| BillingService          | `/api/billing`                  | Pagos y suscripciones   |
| MediaService            | `/api/media`                    | Archivos e imágenes     |
| ContactService          | `/api/contact`                  | Solicitudes de contacto |
| NotificationService     | `/api/notifications`            | Notificaciones          |
| DealerManagementService | `/api/dealers`                  | Gestión de dealers      |
| SearchService           | `/api/search`                   | Búsqueda avanzada       |

---

## Autenticación

CarDealer utiliza **JWT (JSON Web Tokens)** para autenticación. El flujo completo incluye registro, login, refresh tokens y logout.

### Flujo de Autenticación

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        FLUJO DE AUTENTICACIÓN                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. REGISTRO                                                                │
│     POST /api/auth/register                                                 │
│     ↓                                                                       │
│     Email de verificación enviado                                           │
│     ↓                                                                       │
│  2. VERIFICAR EMAIL                                                         │
│     GET /api/auth/verify-email?token={token}                                │
│     ↓                                                                       │
│  3. LOGIN                                                                   │
│     POST /api/auth/login                                                    │
│     ↓                                                                       │
│     Recibe: accessToken + refreshToken                                      │
│     ↓                                                                       │
│  4. USAR API                                                                │
│     Header: Authorization: Bearer {accessToken}                             │
│     ↓                                                                       │
│  5. TOKEN EXPIRADO                                                          │
│     POST /api/auth/refresh                                                  │
│     ↓                                                                       │
│     Nuevo accessToken                                                       │
│     ↓                                                                       │
│  6. LOGOUT                                                                  │
│     POST /api/auth/logout                                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1. Registro de Usuario

```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "usuario@example.com",
  "password": "MiPassword123!",
  "confirmPassword": "MiPassword123!",
  "fullName": "Juan Pérez",
  "phoneNumber": "+18091234567",
  "accountType": "Individual"
}
```

**Tipos de cuenta (`accountType`):**

- `Individual` - Usuario comprador o vendedor individual
- `Dealer` - Concesionario/Dealer

**Respuesta exitosa:**

```json
{
  "success": true,
  "message": "Cuenta creada. Por favor verifica tu email.",
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "email": "usuario@example.com",
    "requiresEmailVerification": true
  }
}
```

### 2. Verificar Email

```http
GET /api/auth/verify-email?token=abc123xyz
```

### 3. Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@example.com",
  "password": "MiPassword123!"
}
```

**Respuesta exitosa:**

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...",
    "expiresAt": "2026-01-20T14:30:00Z",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "email": "usuario@example.com",
      "fullName": "Juan Pérez",
      "roles": ["User"],
      "accountType": "Individual"
    }
  }
}
```

### 4. Usar Token en Peticiones

```http
GET /api/users/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 5. Refresh Token

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
}
```

### 6. Logout

```http
POST /api/auth/logout
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
}
```

### Recuperación de Contraseña

```http
# Paso 1: Solicitar reset
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "usuario@example.com"
}

# Paso 2: Reset con token (recibido por email)
POST /api/auth/reset-password
Content-Type: application/json

{
  "token": "reset-token-from-email",
  "newPassword": "NuevoPassword123!",
  "confirmPassword": "NuevoPassword123!"
}
```

---

## Flujos de Usuario

### Obtener Perfil del Usuario Actual

```http
GET /api/users/me
Authorization: Bearer {token}
```

**Respuesta:**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "usuario@example.com",
  "fullName": "Juan Pérez",
  "phoneNumber": "+18091234567",
  "avatarUrl": "https://cdn.okla.com.do/avatars/user123.jpg",
  "accountType": "Individual",
  "isVerified": true,
  "createdAt": "2026-01-15T10:00:00Z",
  "roles": ["User"],
  "preferences": {
    "language": "es",
    "currency": "DOP",
    "notifications": {
      "email": true,
      "push": true,
      "sms": false
    }
  }
}
```

### Actualizar Perfil

```http
PUT /api/users/me
Authorization: Bearer {token}
Content-Type: application/json

{
  "fullName": "Juan Pérez García",
  "phoneNumber": "+18099876543",
  "preferences": {
    "language": "es",
    "notifications": {
      "email": true,
      "push": true,
      "sms": true
    }
  }
}
```

### Subir Avatar

```http
POST /api/users/me/avatar
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: [imagen.jpg]
```

---

## Gestión de Vehículos

### Flujo de Publicación de Vehículo

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    FLUJO: PUBLICAR UN VEHÍCULO                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. OBTENER CATÁLOGO                                                        │
│     GET /api/catalog/makes                    → Lista de marcas            │
│     GET /api/catalog/models/{makeId}          → Modelos por marca          │
│     GET /api/catalog/years                    → Años disponibles           │
│     GET /api/catalog/categories               → Categorías                 │
│     ↓                                                                       │
│  2. SUBIR IMÁGENES                                                          │
│     POST /api/media/upload                    → Subir cada imagen          │
│     ↓                                                                       │
│  3. CREAR VEHÍCULO                                                          │
│     POST /api/vehicles                        → Crear publicación          │
│     ↓                                                                       │
│  4. PAGAR PUBLICACIÓN (si Individual)                                       │
│     POST /api/billing/checkout                → Checkout Stripe/Azul       │
│     ↓                                                                       │
│  5. VEHÍCULO ACTIVO                                                         │
│     Status: "Active" → Visible en búsquedas                                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Catálogo de Vehículos

#### Obtener Marcas

```http
GET /api/catalog/makes
```

**Respuesta:**

```json
{
  "data": [
    {
      "id": 1,
      "name": "Toyota",
      "logoUrl": "https://cdn.okla.com.do/makes/toyota.png"
    },
    {
      "id": 2,
      "name": "Honda",
      "logoUrl": "https://cdn.okla.com.do/makes/honda.png"
    },
    {
      "id": 3,
      "name": "Hyundai",
      "logoUrl": "https://cdn.okla.com.do/makes/hyundai.png"
    }
  ]
}
```

#### Obtener Modelos por Marca

```http
GET /api/catalog/models/1
```

**Respuesta:**

```json
{
  "data": [
    { "id": 101, "name": "Corolla", "makeId": 1 },
    { "id": 102, "name": "Camry", "makeId": 1 },
    { "id": 103, "name": "RAV4", "makeId": 1 },
    { "id": 104, "name": "Hilux", "makeId": 1 }
  ]
}
```

#### Obtener Años Disponibles

```http
GET /api/catalog/years
```

**Respuesta:**

```json
{
  "data": [2026, 2025, 2024, 2023, 2022, 2021, 2020, 2019, 2018, 2017]
}
```

#### Obtener Categorías

```http
GET /api/catalog/categories
```

**Respuesta:**

```json
{
  "data": [
    { "id": 1, "name": "Sedán", "slug": "sedan" },
    { "id": 2, "name": "SUV", "slug": "suv" },
    { "id": 3, "name": "Camioneta", "slug": "camioneta" },
    { "id": 4, "name": "Deportivo", "slug": "deportivo" },
    { "id": 5, "name": "Eléctrico", "slug": "electrico" }
  ]
}
```

### Crear Publicación de Vehículo

```http
POST /api/vehicles
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Toyota Corolla 2023 - Excelente Estado",
  "description": "Vehículo en perfectas condiciones, único dueño, todos los servicios al día.",
  "makeId": 1,
  "modelId": 101,
  "year": 2023,
  "categoryId": 1,
  "price": 1250000,
  "currency": "DOP",
  "mileage": 25000,
  "fuelType": "Gasoline",
  "transmission": "Automatic",
  "color": "Blanco",
  "interiorColor": "Negro",
  "engineSize": "1.8L",
  "cylinders": 4,
  "doors": 4,
  "seats": 5,
  "drivetrain": "FWD",
  "vin": "JTDKN3DU5A0123456",
  "condition": "Used",
  "location": {
    "city": "Santo Domingo",
    "province": "Distrito Nacional",
    "address": "Av. Abraham Lincoln"
  },
  "features": [
    "Aire acondicionado",
    "Bluetooth",
    "Cámara de reversa",
    "Sensores de estacionamiento",
    "Pantalla táctil"
  ],
  "images": [
    {
      "mediaId": "media-uuid-1",
      "isPrimary": true,
      "order": 1
    },
    {
      "mediaId": "media-uuid-2",
      "isPrimary": false,
      "order": 2
    }
  ]
}
```

**Tipos de Combustible (`fuelType`):**

- `Gasoline` - Gasolina
- `Diesel` - Diésel
- `Electric` - Eléctrico
- `Hybrid` - Híbrido
- `PlugInHybrid` - Híbrido enchufable
- `NaturalGas` - Gas natural

**Tipos de Transmisión (`transmission`):**

- `Automatic` - Automática
- `Manual` - Manual
- `CVT` - Transmisión variable continua
- `SemiAutomatic` - Semi-automática

**Condición (`condition`):**

- `New` - Nuevo
- `Used` - Usado
- `Certified` - Certificado

**Respuesta:**

```json
{
  "success": true,
  "data": {
    "id": "vehicle-uuid",
    "slug": "toyota-corolla-2023-excelente-estado",
    "status": "PendingPayment",
    "createdAt": "2026-01-20T10:00:00Z"
  }
}
```

### Listar Vehículos del Usuario

```http
GET /api/vehicles/my-listings?page=1&pageSize=10
Authorization: Bearer {token}
```

### Actualizar Vehículo

```http
PUT /api/vehicles/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "price": 1150000,
  "description": "Precio reducido. ¡Excelente oportunidad!"
}
```

### Cambiar Estado del Vehículo

```http
PATCH /api/vehicles/{id}/status
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "Sold"
}
```

**Estados disponibles (`status`):**

- `Draft` - Borrador
- `PendingPayment` - Pendiente de pago
- `PendingReview` - En revisión
- `Active` - Activo (visible)
- `Paused` - Pausado
- `Sold` - Vendido
- `Expired` - Expirado

### Eliminar Vehículo

```http
DELETE /api/vehicles/{id}
Authorization: Bearer {token}
```

---

## Sistema de Búsqueda

### Búsqueda de Vehículos

```http
GET /api/vehicles?page=1&pageSize=20&sortBy=price&sortOrder=asc
```

**Parámetros de Query:**

| Parámetro      | Tipo    | Descripción                                                 |
| -------------- | ------- | ----------------------------------------------------------- |
| `page`         | int     | Número de página (default: 1)                               |
| `pageSize`     | int     | Items por página (default: 20, max: 100)                    |
| `sortBy`       | string  | Campo para ordenar: `price`, `year`, `mileage`, `createdAt` |
| `sortOrder`    | string  | `asc` o `desc`                                              |
| `makeId`       | int     | Filtrar por marca                                           |
| `modelId`      | int     | Filtrar por modelo                                          |
| `yearMin`      | int     | Año mínimo                                                  |
| `yearMax`      | int     | Año máximo                                                  |
| `priceMin`     | decimal | Precio mínimo                                               |
| `priceMax`     | decimal | Precio máximo                                               |
| `fuelType`     | string  | Tipo de combustible                                         |
| `transmission` | string  | Tipo de transmisión                                         |
| `condition`    | string  | Condición (New, Used)                                       |
| `category`     | string  | Slug de categoría                                           |
| `city`         | string  | Ciudad                                                      |
| `province`     | string  | Provincia                                                   |
| `q`            | string  | Texto de búsqueda libre                                     |

**Ejemplo de búsqueda avanzada:**

```http
GET /api/vehicles?makeId=1&yearMin=2020&yearMax=2024&priceMax=1500000&transmission=Automatic&city=Santo%20Domingo&sortBy=price&sortOrder=asc
```

**Respuesta:**

```json
{
  "data": [
    {
      "id": "vehicle-uuid-1",
      "slug": "toyota-corolla-2023",
      "title": "Toyota Corolla 2023",
      "price": 1250000,
      "currency": "DOP",
      "year": 2023,
      "mileage": 25000,
      "fuelType": "Gasoline",
      "transmission": "Automatic",
      "location": {
        "city": "Santo Domingo",
        "province": "Distrito Nacional"
      },
      "primaryImage": "https://cdn.okla.com.do/vehicles/img1.jpg",
      "createdAt": "2026-01-15T10:00:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalItems": 156,
    "totalPages": 8
  }
}
```

### Obtener Detalle de Vehículo

```http
GET /api/vehicles/{slug}
```

**Respuesta:**

```json
{
  "data": {
    "id": "vehicle-uuid",
    "slug": "toyota-corolla-2023-excelente-estado",
    "title": "Toyota Corolla 2023 - Excelente Estado",
    "description": "Vehículo en perfectas condiciones...",
    "price": 1250000,
    "currency": "DOP",
    "make": { "id": 1, "name": "Toyota" },
    "model": { "id": 101, "name": "Corolla" },
    "year": 2023,
    "category": { "id": 1, "name": "Sedán" },
    "mileage": 25000,
    "fuelType": "Gasoline",
    "transmission": "Automatic",
    "color": "Blanco",
    "interiorColor": "Negro",
    "engineSize": "1.8L",
    "cylinders": 4,
    "doors": 4,
    "seats": 5,
    "drivetrain": "FWD",
    "condition": "Used",
    "location": {
      "city": "Santo Domingo",
      "province": "Distrito Nacional"
    },
    "features": ["Aire acondicionado", "Bluetooth", "Cámara de reversa"],
    "images": [
      {
        "id": "img-1",
        "url": "https://cdn.okla.com.do/vehicles/img1.jpg",
        "isPrimary": true
      }
    ],
    "seller": {
      "id": "seller-uuid",
      "name": "Juan Pérez",
      "type": "Individual",
      "rating": 4.8,
      "totalListings": 3,
      "memberSince": "2025-06-01"
    },
    "stats": {
      "views": 245,
      "favorites": 12,
      "contacts": 5
    },
    "createdAt": "2026-01-15T10:00:00Z",
    "updatedAt": "2026-01-18T15:30:00Z"
  }
}
```

### Homepage Sections

```http
GET /api/homepagesections/homepage
```

**Respuesta:**

```json
{
  "data": [
    {
      "name": "Carousel Principal",
      "slug": "carousel-principal",
      "maxItems": 5,
      "vehicles": [
        /* vehículos destacados */
      ]
    },
    {
      "name": "SUVs",
      "slug": "suvs",
      "accentColor": "blue",
      "vehicles": [
        /* vehículos SUV */
      ]
    },
    {
      "name": "Sedanes",
      "slug": "sedanes",
      "vehicles": [
        /* vehículos sedán */
      ]
    }
  ]
}
```

---

## Proceso de Contacto

### Flujo de Contacto con Vendedor

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    FLUJO: CONTACTAR VENDEDOR                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. VER VEHÍCULO                                                            │
│     GET /api/vehicles/{slug}                                                │
│     ↓                                                                       │
│  2. ENVIAR SOLICITUD DE CONTACTO                                            │
│     POST /api/contact/requests                                              │
│     ↓                                                                       │
│  3. VENDEDOR RECIBE NOTIFICACIÓN                                            │
│     (Email + Push + In-App)                                                 │
│     ↓                                                                       │
│  4. VENDEDOR RESPONDE                                                       │
│     POST /api/contact/requests/{id}/messages                                │
│     ↓                                                                       │
│  5. COMPRADOR RECIBE RESPUESTA                                              │
│     ↓                                                                       │
│  6. CONTINUAR CONVERSACIÓN                                                  │
│     GET /api/contact/requests/{id}/messages                                 │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Enviar Solicitud de Contacto

```http
POST /api/contact/requests
Authorization: Bearer {token}
Content-Type: application/json

{
  "vehicleId": "vehicle-uuid",
  "message": "Hola, estoy interesado en el vehículo. ¿Está disponible para verlo este fin de semana?",
  "contactPreference": "WhatsApp",
  "phoneNumber": "+18091234567"
}
```

**Preferencias de contacto (`contactPreference`):**

- `Email`
- `Phone`
- `WhatsApp`
- `Any`

### Listar Mis Solicitudes de Contacto

```http
# Como comprador
GET /api/contact/requests/sent?page=1&pageSize=10
Authorization: Bearer {token}

# Como vendedor
GET /api/contact/requests/received?page=1&pageSize=10
Authorization: Bearer {token}
```

### Obtener Conversación

```http
GET /api/contact/requests/{requestId}/messages
Authorization: Bearer {token}
```

### Responder a Solicitud

```http
POST /api/contact/requests/{requestId}/messages
Authorization: Bearer {token}
Content-Type: application/json

{
  "message": "¡Hola! Sí, el vehículo está disponible. ¿Le parece el sábado a las 10am?"
}
```

### Marcar como Leído

```http
PATCH /api/contact/requests/{requestId}/read
Authorization: Bearer {token}
```

---

## Sistema de Pagos

CarDealer soporta dos pasarelas de pago: **Stripe** (tarjetas internacionales) y **Azul** (tarjetas dominicanas).

### Flujo de Pago para Publicación

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    FLUJO: PAGAR PUBLICACIÓN                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. CREAR VEHÍCULO                                                          │
│     POST /api/vehicles → Status: "PendingPayment"                          │
│     ↓                                                                       │
│  2. CREAR SESIÓN DE CHECKOUT                                                │
│     POST /api/billing/checkout/session                                      │
│     ↓                                                                       │
│  3. SELECCIONAR MÉTODO DE PAGO                                              │
│     ├── Stripe → Redirect a Stripe Checkout                                │
│     └── Azul → Redirect a Azul Payment Page                                │
│     ↓                                                                       │
│  4. COMPLETAR PAGO                                                          │
│     (En página de pasarela)                                                 │
│     ↓                                                                       │
│  5. WEBHOOK RECIBE CONFIRMACIÓN                                             │
│     POST /api/billing/webhooks/stripe (o /azul)                            │
│     ↓                                                                       │
│  6. VEHÍCULO ACTIVADO                                                       │
│     Status: "Active"                                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Precios de Publicación

| Tipo de Usuario     | Precio por Publicación | Duración  |
| ------------------- | ---------------------- | --------- |
| Individual          | RD$ 500                | 30 días   |
| Dealer (Starter)    | Incluido en plan       | Ilimitado |
| Dealer (Pro)        | Incluido en plan       | Ilimitado |
| Dealer (Enterprise) | Incluido en plan       | Ilimitado |

### Crear Sesión de Checkout

```http
POST /api/billing/checkout/session
Authorization: Bearer {token}
Content-Type: application/json

{
  "items": [
    {
      "type": "VehicleListing",
      "vehicleId": "vehicle-uuid",
      "quantity": 1
    }
  ],
  "paymentMethod": "Stripe",
  "successUrl": "https://okla.com.do/payment/success",
  "cancelUrl": "https://okla.com.do/payment/cancel"
}
```

**Respuesta:**

```json
{
  "success": true,
  "data": {
    "sessionId": "cs_test_xxx",
    "checkoutUrl": "https://checkout.stripe.com/pay/cs_test_xxx",
    "expiresAt": "2026-01-20T11:00:00Z"
  }
}
```

### Verificar Estado de Pago

```http
GET /api/billing/payments/{paymentId}
Authorization: Bearer {token}
```

---

## Gestión de Dealers

### Flujo de Registro de Dealer

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    FLUJO: REGISTRAR DEALER                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. REGISTRO BÁSICO                                                         │
│     POST /api/auth/register (accountType: "Dealer")                        │
│     ↓                                                                       │
│  2. COMPLETAR PERFIL DE DEALER                                              │
│     POST /api/dealers                                                       │
│     ↓                                                                       │
│  3. SUBIR DOCUMENTOS                                                        │
│     POST /api/dealers/{id}/documents                                        │
│     (RNC, Licencia Comercial, Cédula)                                      │
│     ↓                                                                       │
│  4. VERIFICACIÓN (1-2 días)                                                 │
│     Status: "UnderReview"                                                   │
│     ↓                                                                       │
│  5. SELECCIONAR PLAN                                                        │
│     POST /api/billing/subscriptions                                         │
│     ↓                                                                       │
│  6. DEALER ACTIVO                                                           │
│     Status: "Active", VerificationStatus: "Verified"                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Planes de Dealer

| Plan           | Precio Mensual | Vehículos | Características                               |
| -------------- | -------------- | --------- | --------------------------------------------- |
| **Starter**    | $49/mes        | 15        | Badge verificado, Estadísticas básicas        |
| **Pro**        | $129/mes       | 50        | Todo Starter + Import CSV, Prioridad búsqueda |
| **Enterprise** | $299/mes       | Ilimitado | Todo Pro + API access, Multi-sucursal         |

### Crear Perfil de Dealer

```http
POST /api/dealers
Authorization: Bearer {token}
Content-Type: application/json

{
  "businessName": "Auto Premium RD",
  "legalName": "Auto Premium RD SRL",
  "rnc": "131234567",
  "dealerType": "Independent",
  "email": "info@autopremiumrd.com",
  "phone": "+18095551234",
  "mobilePhone": "+18095551235",
  "website": "https://autopremiumrd.com",
  "address": "Av. 27 de Febrero #123",
  "city": "Santo Domingo",
  "province": "Distrito Nacional",
  "description": "Dealer especializado en vehículos de lujo y deportivos.",
  "establishedDate": "2020-01-15",
  "employeeCount": 12
}
```

**Tipos de Dealer (`dealerType`):**

- `Independent` - Dealer independiente
- `Chain` - Cadena
- `MultipleStore` - Múltiples tiendas
- `Franchise` - Franquicia

### Subir Documentos

```http
POST /api/dealers/{dealerId}/documents
Authorization: Bearer {token}
Content-Type: multipart/form-data

documentType: "RNC"
file: [documento.pdf]
```

**Tipos de documentos (`documentType`):**

- `RNC` - Registro Nacional del Contribuyente
- `BusinessLicense` - Licencia comercial
- `IdentificationCard` - Cédula del representante
- `ProofOfAddress` - Comprobante de dirección
- `InsuranceCertificate` - Certificado de seguro

### Obtener Dashboard de Dealer

```http
GET /api/dealers/me/dashboard
Authorization: Bearer {token}
```

**Respuesta:**

```json
{
  "data": {
    "dealer": {
      "id": "dealer-uuid",
      "businessName": "Auto Premium RD",
      "status": "Active",
      "verificationStatus": "Verified",
      "currentPlan": "Pro",
      "maxActiveListings": 50
    },
    "stats": {
      "activeListings": 23,
      "totalViews": 15420,
      "totalContacts": 89,
      "inventoryValue": 45000000
    },
    "subscription": {
      "plan": "Pro",
      "status": "Active",
      "currentPeriodEnd": "2026-02-20T00:00:00Z",
      "cancelAtPeriodEnd": false
    }
  }
}
```

### Suscribirse a un Plan

```http
POST /api/billing/subscriptions
Authorization: Bearer {token}
Content-Type: application/json

{
  "planId": "pro",
  "paymentMethod": "Stripe"
}
```

---

## Notificaciones

### Listar Notificaciones

```http
GET /api/notifications?page=1&pageSize=20&unreadOnly=false
Authorization: Bearer {token}
```

**Respuesta:**

```json
{
  "data": [
    {
      "id": "notif-uuid-1",
      "type": "ContactRequest",
      "title": "Nueva solicitud de contacto",
      "body": "Juan Pérez está interesado en tu Toyota Corolla 2023",
      "isRead": false,
      "data": {
        "vehicleId": "vehicle-uuid",
        "contactRequestId": "contact-uuid"
      },
      "createdAt": "2026-01-20T10:30:00Z"
    }
  ],
  "unreadCount": 5
}
```

### Marcar como Leída

```http
PATCH /api/notifications/{id}/read
Authorization: Bearer {token}
```

### Marcar Todas como Leídas

```http
POST /api/notifications/mark-all-read
Authorization: Bearer {token}
```

### Configurar Preferencias de Notificación

```http
PUT /api/notifications/preferences
Authorization: Bearer {token}
Content-Type: application/json

{
  "email": {
    "contactRequests": true,
    "messages": true,
    "priceAlerts": true,
    "marketing": false
  },
  "push": {
    "contactRequests": true,
    "messages": true,
    "priceAlerts": true
  },
  "sms": {
    "contactRequests": false,
    "messages": false
  }
}
```

---

## Media y Archivos

### Subir Imagen

Para archivos grandes, se usa un proceso de upload en dos pasos:

#### Paso 1: Iniciar Upload

```http
POST /api/media/upload/init
Authorization: Bearer {token}
Content-Type: application/json

{
  "fileName": "vehiculo-frontal.jpg",
  "contentType": "image/jpeg",
  "fileSize": 2500000,
  "category": "vehicle"
}
```

**Respuesta:**

```json
{
  "data": {
    "uploadId": "upload-uuid",
    "uploadUrl": "https://s3.amazonaws.com/okla-media/presigned-url...",
    "expiresAt": "2026-01-20T11:00:00Z"
  }
}
```

#### Paso 2: Subir a S3

```http
PUT {uploadUrl}
Content-Type: image/jpeg

[binary data]
```

#### Paso 3: Finalizar Upload

```http
POST /api/media/upload/finalize
Authorization: Bearer {token}
Content-Type: application/json

{
  "uploadId": "upload-uuid"
}
```

**Respuesta:**

```json
{
  "data": {
    "mediaId": "media-uuid",
    "url": "https://cdn.okla.com.do/vehicles/vehiculo-frontal.jpg",
    "thumbnailUrl": "https://cdn.okla.com.do/vehicles/vehiculo-frontal-thumb.jpg"
  }
}
```

### Upload Simplificado (archivos pequeños)

```http
POST /api/media/upload
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: [imagen.jpg]
category: vehicle
```

---

## Ejemplos Prácticos

### Ejemplo 1: Comprador Buscando Vehículo

```bash
# 1. Login
curl -X POST https://api.okla.com.do/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "comprador@example.com", "password": "password123"}'

# Guardar token
TOKEN="eyJhbGci..."

# 2. Buscar vehículos
curl "https://api.okla.com.do/api/vehicles?makeId=1&yearMin=2020&priceMax=1500000" \
  -H "Authorization: Bearer $TOKEN"

# 3. Ver detalle de vehículo
curl "https://api.okla.com.do/api/vehicles/toyota-corolla-2023-excelente"

# 4. Contactar vendedor
curl -X POST https://api.okla.com.do/api/contact/requests \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "vehicleId": "vehicle-uuid",
    "message": "Hola, estoy interesado. ¿Podemos coordinar una visita?",
    "contactPreference": "WhatsApp"
  }'
```

### Ejemplo 2: Vendedor Individual Publicando Vehículo

```bash
# 1. Subir imágenes
curl -X POST https://api.okla.com.do/api/media/upload \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@foto1.jpg" \
  -F "category=vehicle"
# → mediaId: "media-uuid-1"

# 2. Crear vehículo
curl -X POST https://api.okla.com.do/api/vehicles \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Honda Civic 2022",
    "makeId": 2,
    "modelId": 201,
    "year": 2022,
    "price": 980000,
    "mileage": 35000,
    "transmission": "Automatic",
    "fuelType": "Gasoline",
    "categoryId": 1,
    "condition": "Used",
    "location": {"city": "Santiago", "province": "Santiago"},
    "images": [{"mediaId": "media-uuid-1", "isPrimary": true, "order": 1}]
  }'
# → vehicleId: "vehicle-uuid", status: "PendingPayment"

# 3. Crear sesión de pago
curl -X POST https://api.okla.com.do/api/billing/checkout/session \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "items": [{"type": "VehicleListing", "vehicleId": "vehicle-uuid"}],
    "paymentMethod": "Stripe",
    "successUrl": "https://okla.com.do/success",
    "cancelUrl": "https://okla.com.do/cancel"
  }'
# → checkoutUrl: "https://checkout.stripe.com/..."

# 4. Usuario completa pago en Stripe
# 5. Webhook actualiza vehículo a status: "Active"
```

### Ejemplo 3: Dealer Gestionando Inventario

```bash
# 1. Ver dashboard
curl https://api.okla.com.do/api/dealers/me/dashboard \
  -H "Authorization: Bearer $TOKEN"

# 2. Listar mis vehículos
curl "https://api.okla.com.do/api/vehicles/my-listings?page=1&status=Active" \
  -H "Authorization: Bearer $TOKEN"

# 3. Actualizar precio
curl -X PUT https://api.okla.com.do/api/vehicles/vehicle-uuid \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"price": 1100000}'

# 4. Pausar vehículo
curl -X PATCH https://api.okla.com.do/api/vehicles/vehicle-uuid/status \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status": "Paused"}'

# 5. Ver estadísticas
curl https://api.okla.com.do/api/vehicles/vehicle-uuid/stats \
  -H "Authorization: Bearer $TOKEN"
```

---

## Errores Comunes

### Error de Autenticación

```json
{
  "success": false,
  "error": {
    "code": "UNAUTHORIZED",
    "message": "Token inválido o expirado"
  }
}
```

**Solución:** Obtener nuevo token con `/api/auth/refresh`

### Error de Validación

```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Error de validación",
    "details": [
      { "field": "price", "message": "El precio debe ser mayor a 0" },
      { "field": "year", "message": "El año debe estar entre 1900 y 2027" }
    ]
  }
}
```

### Rate Limit Excedido

```json
{
  "success": false,
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Demasiadas peticiones. Intente de nuevo en 60 segundos.",
    "retryAfter": 60
  }
}
```

---

## SDKs y Librerías

### JavaScript/TypeScript

```typescript
import { CarDealerClient } from "@cardealer/sdk";

const client = new CarDealerClient({
  baseUrl: "https://api.okla.com.do",
});

// Login
const auth = await client.auth.login({
  email: "user@example.com",
  password: "password123",
});

// Buscar vehículos
const vehicles = await client.vehicles.search({
  makeId: 1,
  yearMin: 2020,
  priceMax: 1500000,
});
```

### cURL Examples Collection

Disponible en: `/docs/postman/CarDealer-API.postman_collection.json`

---

## Soporte

| Canal         | Contacto                   |
| ------------- | -------------------------- |
| Email Técnico | api-support@okla.com.do    |
| Documentación | https://docs.okla.com.do   |
| Status Page   | https://status.okla.com.do |

---

**Última actualización:** Enero 2026  
**Versión de API:** v1.0
