# 📚 API Documentation & Tutorials

Este directorio contiene la documentación completa de la API de CarDealer/OKLA.

---

## 📋 Documentos Disponibles

| Documento                                              | Descripción                                | Audiencia                       |
| ------------------------------------------------------ | ------------------------------------------ | ------------------------------- |
| [API_TUTORIAL.md](API_TUTORIAL.md)                     | Tutorial completo con flujos de trabajo    | Desarrolladores Frontend/Mobile |
| [API_COMPLETE_REFERENCE.md](API_COMPLETE_REFERENCE.md) | Referencia técnica de todos los endpoints  | Desarrolladores Backend/API     |
| [DATABASE_ARCHITECTURE.md](DATABASE_ARCHITECTURE.md)   | Arquitectura de base de datos por servicio | DBAs, Arquitectos               |

---

## 🚀 Inicio Rápido

### URL Base

| Ambiente       | URL                       |
| -------------- | ------------------------- |
| **Producción** | `https://api.okla.com.do` |
| **Desarrollo** | `http://localhost:18443`  |

### Autenticación

Todos los endpoints protegidos requieren un token JWT:

```bash
# 1. Login
curl -X POST https://api.okla.com.do/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com", "password": "password123"}'

# 2. Usar token en peticiones
curl https://api.okla.com.do/api/users/me \
  -H "Authorization: Bearer {accessToken}"
```

---

## 📖 Contenido por Documento

### 1. API_TUTORIAL.md

Tutorial paso a paso con ejemplos prácticos:

- 🔐 **Autenticación** - Registro, login, refresh tokens
- 🚗 **Vehículos** - Catálogo, CRUD, gestión de estados
- 🔍 **Búsqueda** - Filtros, paginación, parámetros
- 📞 **Contacto** - Sistema de solicitudes y mensajes
- 💳 **Pagos** - Integración Stripe + Azul
- 🏪 **Dealers** - Registro, planes, dashboard
- 🔔 **Notificaciones** - Listado y preferencias
- 📸 **Media** - Upload de imágenes a S3

### 2. API_COMPLETE_REFERENCE.md

Referencia técnica completa:

- Configuración del Gateway
- Todos los endpoints por servicio
- Esquemas de request/response
- Códigos de error
- Rate limits

### 3. DATABASE_ARCHITECTURE.md

Arquitectura de datos:

- Diagramas ER por servicio
- Tablas y columnas
- Relaciones entre servicios
- Eventos de dominio (RabbitMQ)
- Índices de Elasticsearch

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         API GATEWAY (Ocelot)                                │
│                     https://api.okla.com.do                                 │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
         ┌───────────────────────────┼───────────────────────────┐
         │                           │                           │
         ▼                           ▼                           ▼
┌─────────────────┐     ┌─────────────────────┐     ┌─────────────────┐
│   AuthService   │     │ VehiclesSaleService │     │  BillingService │
└─────────────────┘     └─────────────────────┘     └─────────────────┘
         │                           │                           │
         ▼                           ▼                           ▼
┌─────────────────┐     ┌─────────────────────┐     ┌─────────────────┐
│   UserService   │     │    SearchService    │     │  MediaService   │
└─────────────────┘     └─────────────────────┘     └─────────────────┘
```

---

## 📡 Servicios Principales

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

## 🔗 Recursos Adicionales

- **Postman Collection**: `docs/postman/CarDealer-API.postman_collection.json`
- **Swagger UI**: `https://api.okla.com.do/swagger` (desarrollo)
- **Health Check**: `https://api.okla.com.do/health`

---

## 📞 Soporte

| Canal         | Contacto                   |
| ------------- | -------------------------- |
| Email Técnico | api-support@okla.com.do    |
| Documentación | https://docs.okla.com.do   |
| Status Page   | https://status.okla.com.do |

---

**Última actualización:** Enero 2026
