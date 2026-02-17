# 🧪 OKLA QA Environment - Guía Completa

**Fecha:** Enero 2026  
**Versión:** 1.0

---

## 📋 Resumen

Este ambiente de QA permite levantar todo el sistema OKLA (frontend + backend + infraestructura) en contenedores Docker, con datos de prueba pre-poblados a través de los APIs de cada microservicio.

### ¿Por qué usar APIs para el seeding?

1. **Prueba el flujo completo** - Validaciones, lógica de negocio, eventos
2. **IDs generados correctamente** - Los microservicios generan `Guid.NewGuid()` en C#
3. **Triggers de eventos** - RabbitMQ procesa notificaciones y auditoría
4. **Más realista** - Simula cómo un usuario real usa el sistema
5. **Detecta bugs temprano** - Si el seeding falla, hay un problema en el API

---

## 🚀 Inicio Rápido

### Prerrequisitos

- **Docker Desktop** >= 4.0
- **Puertos disponibles:**
  - 3000 (Frontend)
  - 18443 (API Gateway)
  - 5432 (PostgreSQL)
  - 6379 (Redis)
  - 5672, 15672 (RabbitMQ)

### Paso 1: Clonar y navegar

```bash
cd cardealer-microservices/qa-environment/seed-scripts
```

### Paso 2: Dar permisos de ejecución

```bash
chmod +x *.sh
```

### Paso 3: Iniciar el ambiente

```bash
./setup-qa.sh start
```

Este comando:

1. Levanta PostgreSQL, Redis, RabbitMQ
2. Inicia todos los microservicios
3. Inicia el frontend
4. Espera a que todo esté healthy

### Paso 4: Poblar datos de prueba

```bash
./setup-qa.sh seed
```

Este comando:

1. Crea usuarios (admins, dealers, sellers, buyers)
2. Crea 15 vehículos de prueba
3. Todo vía API (no SQL directo)

### Paso 5: ¡Listo para probar!

- **Frontend:** http://localhost:3000
- **API Gateway:** http://localhost:18443
- **Swagger:** http://localhost:18443/swagger
- **RabbitMQ:** http://localhost:15672

---

## 👥 Credenciales de Prueba

### Administradores

| Email               | Password         | Rol         |
| ------------------- | ---------------- | ----------- |
| superadmin@okla.com | SuperAdmin123!@# | Super Admin |
| admin@okla.com      | Admin123!@#      | Admin       |

### Dealers

| Email            | Password     | Descripción                        |
| ---------------- | ------------ | ---------------------------------- |
| dealer1@okla.com | Dealer123!@# | Auto Import Premium, Santo Domingo |
| dealer2@okla.com | Dealer123!@# | Vehículos del Cibao, Santiago      |
| dealer3@okla.com | Dealer123!@# | Punta Cana Motors, La Altagracia   |

### Vendedores Individuales

| Email            | Password     | Descripción                       |
| ---------------- | ------------ | --------------------------------- |
| seller1@okla.com | Seller123!@# | Vendedor particular Santo Domingo |
| seller2@okla.com | Seller123!@# | Vendedor particular Santiago      |
| seller3@okla.com | Seller123!@# | Vendedor particular Puerto Plata  |

### Compradores

| Email           | Password    |
| --------------- | ----------- |
| buyer1@okla.com | Buyer123!@# |
| buyer2@okla.com | Buyer123!@# |
| buyer3@okla.com | Buyer123!@# |
| buyer4@okla.com | Buyer123!@# |
| buyer5@okla.com | Buyer123!@# |

---

## 🚗 Vehículos de Prueba

El seeding crea 15 vehículos en diferentes categorías:

### Sedanes (3)

- 2024 Toyota Corolla LE - RD$1,350,000
- 2023 Honda Civic Touring - RD$1,650,000
- 2022 Nissan Sentra SR - RD$1,150,000

### SUVs (5)

- 2024 Toyota RAV4 Hybrid XLE - RD$2,850,000
- 2023 Honda CR-V EX-L AWD - RD$2,200,000
- 2023 Hyundai Tucson Limited - RD$1,950,000
- 2024 Kia Sportage X-Pro - RD$2,400,000
- 2023 Mazda CX-5 Signature - RD$2,500,000

### Pickups (3)

- 2023 Toyota Hilux SRV 4x4 - RD$2,800,000
- 2022 Ford Ranger Wildtrak - RD$2,600,000
- 2024 Chevrolet Colorado ZR2 - RD$3,200,000

### Todoterrenos (2)

- 2023 Jeep Wrangler Rubicon 4xe - RD$4,200,000
- 2022 Toyota Land Cruiser 300 - RD$6,500,000

### Económicos (2)

- 2023 Suzuki Swift GL - RD$750,000
- 2022 Kia Picanto EX - RD$650,000

---

## 📋 Comandos Disponibles

```bash
# Iniciar todo el ambiente
./setup-qa.sh start

# Poblar datos de prueba
./setup-qa.sh seed

# Ver estado de servicios
./setup-qa.sh status

# Ver logs (todos los servicios)
./setup-qa.sh logs

# Ver logs de un servicio específico
./setup-qa.sh logs gateway
./setup-qa.sh logs authservice
./setup-qa.sh logs vehiclessaleservice

# Detener todo
./setup-qa.sh stop

# Reiniciar todo
./setup-qa.sh restart

# Limpiar y reiniciar (ELIMINA TODOS LOS DATOS)
./setup-qa.sh clean
```

---

## 🔍 Verificaciones para QA

### 1. Verificar que los servicios están corriendo

```bash
./setup-qa.sh status
```

Todos deben mostrar "Up" y los health checks deben ser ✓

### 2. Verificar APIs

```bash
# Health del Gateway
curl http://localhost:18443/health

# Listar vehículos
curl http://localhost:18443/api/vehicles

# Login
curl -X POST http://localhost:18443/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "buyer1@okla.com", "password": "Buyer123!@#"}'
```

### 3. Verificar Frontend

1. Abrir http://localhost:3000
2. Hacer login con buyer1@okla.com / Buyer123!@#
3. Navegar por vehículos
4. Probar favoritos, comparación, búsqueda

---

## 🐛 Troubleshooting

### El Gateway no responde

```bash
# Ver logs del gateway
./setup-qa.sh logs gateway

# Verificar que el contenedor está corriendo
docker ps | grep gateway
```

### Los servicios no inician

```bash
# Ver todos los logs
./setup-qa.sh logs

# Verificar PostgreSQL
docker exec -it qa-postgres psql -U postgres -c '\l'
```

### El seeding falla

```bash
# Verificar que los servicios están healthy
./setup-qa.sh status

# Reintentar después de esperar
sleep 30
./setup-qa.sh seed
```

### Limpiar y empezar de nuevo

```bash
# Esto elimina TODOS los datos
./setup-qa.sh clean

# Volver a iniciar
./setup-qa.sh start
./setup-qa.sh seed
```

---

## 📡 Endpoints Principales

### AuthService

| Método | Endpoint           | Descripción         |
| ------ | ------------------ | ------------------- |
| POST   | /api/auth/register | Registrar usuario   |
| POST   | /api/auth/login    | Login (retorna JWT) |
| POST   | /api/auth/refresh  | Refrescar token     |
| GET    | /api/auth/me       | Usuario actual      |

### VehiclesSaleService

| Método | Endpoint             | Descripción           |
| ------ | -------------------- | --------------------- |
| GET    | /api/vehicles        | Listar vehículos      |
| GET    | /api/vehicles/{id}   | Detalle de vehículo   |
| POST   | /api/vehicles        | Crear vehículo (auth) |
| PUT    | /api/vehicles/{id}   | Actualizar vehículo   |
| DELETE | /api/vehicles/{id}   | Eliminar vehículo     |
| GET    | /api/vehicles/search | Búsqueda con filtros  |

### UserService

| Método | Endpoint        | Descripción       |
| ------ | --------------- | ----------------- |
| GET    | /api/users/{id} | Obtener usuario   |
| PUT    | /api/users/{id} | Actualizar perfil |

### ContactService

| Método | Endpoint      | Descripción                |
| ------ | ------------- | -------------------------- |
| POST   | /api/contacts | Enviar mensaje de contacto |
| GET    | /api/contacts | Listar contactos recibidos |

---

## 🏗️ Arquitectura del Ambiente QA

```
┌─────────────────────────────────────────────────────────────────┐
│                       QA Environment                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────┐      ┌─────────────┐      ┌─────────────┐     │
│  │  Frontend   │──────│   Gateway   │──────│  Services   │     │
│  │   :3000     │      │   :18443    │      │  :5001-5010 │     │
│  └─────────────┘      └─────────────┘      └─────────────┘     │
│                                                   │             │
│                                                   ▼             │
│  ┌─────────────┐      ┌─────────────┐      ┌─────────────┐     │
│  │  PostgreSQL │      │    Redis    │      │  RabbitMQ   │     │
│  │   :5432     │      │   :6379     │      │  :5672/:15672│    │
│  └─────────────┘      └─────────────┘      └─────────────┘     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Servicios incluidos:

1. **Frontend** (React) - UI de la aplicación
2. **Gateway** (Ocelot) - Enrutamiento de APIs
3. **AuthService** - Autenticación y JWT
4. **UserService** - Gestión de usuarios
5. **RoleService** - Roles y permisos
6. **VehiclesSaleService** - Vehículos y catálogo
7. **MediaService** - Archivos e imágenes
8. **NotificationService** - Emails y notificaciones
9. **BillingService** - Pagos y suscripciones
10. **ErrorService** - Logging centralizado

---

## ✅ Checklist de QA

### Funcionalidades Básicas

- [ ] Login con diferentes tipos de usuario
- [ ] Registro de nuevo usuario
- [ ] Ver listado de vehículos
- [ ] Filtrar y buscar vehículos
- [ ] Ver detalle de vehículo
- [ ] Agregar a favoritos
- [ ] Comparar vehículos (hasta 3)
- [ ] Enviar mensaje a vendedor

### Flujos de Dealer

- [ ] Login como dealer
- [ ] Ver dashboard de dealer
- [ ] Publicar nuevo vehículo
- [ ] Editar vehículo existente
- [ ] Ver estadísticas de vehículos

### Flujos de Admin

- [ ] Login como admin
- [ ] Ver panel de administración
- [ ] Gestionar usuarios
- [ ] Moderar vehículos

### Integraciones

- [ ] Notificaciones por email (revisar MailHog si está configurado)
- [ ] Uploads de imágenes
- [ ] Eventos de RabbitMQ procesados

---

## 📞 Soporte

Si encuentras problemas:

1. Revisa los logs: `./setup-qa.sh logs`
2. Verifica el estado: `./setup-qa.sh status`
3. Intenta limpiar: `./setup-qa.sh clean && ./setup-qa.sh start`
4. Contacta al equipo de desarrollo

---

**¡Happy Testing! 🎉**
