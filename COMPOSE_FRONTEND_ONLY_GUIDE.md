# 🚀 Compose Frontend-Only Setup Guide

**Fecha:** Enero 9, 2026  
**Propósito:** Levantar SOLO los servicios necesarios para el desarrollo del frontend

---

## 📋 Servicios Incluidos

### 🔴 CRÍTICOS (4)

1. **AuthService** - Puerto 15001 - Autenticación JWT
2. **VehiclesSaleService** - Puerto 15010 - CRUD de vehículos
3. **MediaService** - Puerto 15020 - Gestión de imágenes
4. **Gateway** - Puerto 18443 - API router (Ocelot)

### 🟠 IMPORTANTES (4)

1. **UserService** - Puerto 15002 - Perfiles de usuario
2. **ContactService** - Puerto 15003 - Mensajería
3. **NotificationService** - Puerto 15005 - Email/SMS/Push
4. **AdminService** - Puerto 15007 - Panel admin

### 🔵 INFRAESTRUCTURA (4)

1. **PostgreSQL** - Puerto 5433 - Base de datos
2. **RabbitMQ** - Puerto 5672 / 15672 (UI) - Message broker
3. **Redis** - Puerto 6379 - Cache
4. **Consul** - Puerto 8500 - Service discovery

**Total: 12 servicios** (vs. 56 en el archivo original)

---

## ⚡ Ventajas vs. Compose Original

| Aspecto               | Original | Frontend-Only |
| --------------------- | -------- | ------------- |
| **Servicios**         | 56       | 12            |
| **RAM Estimada**      | 8-10 GB  | 2-3 GB        |
| **CPU Estimada**      | 80-100%  | 20-30%        |
| **Tiempo de startup** | 2-3 min  | 30-45 seg     |
| **Peso Docker**       | 15+ GB   | 4-5 GB        |
| **Complejidad**       | Alta     | Baja          |

---

## 🚀 Cómo Usar

### 1️⃣ Levantar los Servicios

```bash
# Dentro del directorio raíz del proyecto
docker-compose -f compose.frontend-only.yaml up -d
```

### 2️⃣ Verificar que Todo está Funcionando

```bash
# Ver estado de los containers
docker-compose -f compose.frontend-only.yaml ps

# Ver logs en tiempo real
docker-compose -f compose.frontend-only.yaml logs -f

# Ver logs de un servicio específico
docker-compose -f compose.frontend-only.yaml logs -f gateway

# Chequear health checks
docker-compose -f compose.frontend-only.yaml ps | grep healthy
```

### 3️⃣ Acceder a los Servicios

**Desde Frontend (localhost:3000):**

```
GET  http://localhost:18443/health
POST http://localhost:18443/api/auth/login
GET  http://localhost:18443/api/vehicles
GET  http://localhost:18443/api/vehicles/{id}
POST http://localhost:18443/api/vehicles
```

**Directo a Servicios (para debugging):**

```
http://localhost:15001/swagger    # AuthService
http://localhost:15010/swagger    # VehiclesSaleService
http://localhost:15020/swagger    # MediaService
http://localhost:15002/swagger    # UserService
http://localhost:15003/swagger    # ContactService
http://localhost:15005/swagger    # NotificationService
http://localhost:15007/swagger    # AdminService
```

**RabbitMQ Management:**

```
http://localhost:15672
Username: guest
Password: guest
```

**Consul Service Discovery:**

```
http://localhost:8500/ui/
```

**Redis CLI:**

```bash
docker-compose -f compose.frontend-only.yaml exec redis redis-cli
```

**PostgreSQL CLI:**

```bash
docker-compose -f compose.frontend-only.yaml exec postgres_db psql -U postgres
```

---

## 🔧 Configuración de Variables de Entorno

Crear archivo `.env` en la raíz del proyecto:

```env
# ════════════════════════════════════════════════════════════
# Configuración de Base de Datos
# ════════════════════════════════════════════════════════════
POSTGRES_PASSWORD=password  # Cambiar en producción

# ════════════════════════════════════════════════════════════
# JWT Configuration
# ════════════════════════════════════════════════════════════
JWT__KEY=clave-super-secreta-desarrollo-32-caracteres-aaa

# ════════════════════════════════════════════════════════════
# S3 / MinIO Configuration (si usas S3 local)
# ════════════════════════════════════════════════════════════
S3_ENDPOINT=http://localhost:9000
S3_ACCESS_KEY=minioadmin
S3_SECRET_KEY=minioadmin
S3_BUCKET=okla

# ════════════════════════════════════════════════════════════
# Frontend URL (para CORS)
# ════════════════════════════════════════════════════════════
FRONTEND_URL=http://localhost:3000
```

---

## 🛑 Detener los Servicios

```bash
# Parar todos los containers
docker-compose -f compose.frontend-only.yaml down

# Parar y eliminar volúmenes (CUIDADO: Pierde datos de BD)
docker-compose -f compose.frontend-only.yaml down -v

# Parar un servicio específico
docker-compose -f compose.frontend-only.yaml stop authservice

# Reiniciar un servicio
docker-compose -f compose.frontend-only.yaml restart vehiclessaleservice
```

---

## 📊 Monitoreo

### Ver Consumo de Recursos

```bash
# Terminal 1: Monitorear recursos
docker stats

# Terminal 2: Ver logs con filtros
docker-compose -f compose.frontend-only.yaml logs --tail=50 -f gateway
```

### Health Checks

```bash
# Verificar health checks (verde = healthy)
docker-compose -f compose.frontend-only.yaml ps

# Output esperado:
# NAME                    STATUS
# authservice             healthy
# vehiclessaleservice     healthy
# mediaservice            healthy
# userservice             healthy
# contactservice          healthy
# notificationservice     healthy
# adminservice            healthy
# gateway                 healthy
# postgres_db             healthy
# rabbitmq                healthy
# redis                   healthy
# consul                  healthy
```

---

## 🐛 Troubleshooting

### "Connection refused en gateway"

**Causa:** Los servicios backend aún se están iniciando

**Solución:**

```bash
# Esperar a que todos estén healthy
docker-compose -f compose.frontend-only.yaml ps

# Ver logs de gateway
docker-compose -f compose.frontend-only.yaml logs gateway

# Reintentar conexión después de 30 segundos
```

### "PostgreSQL connection refused"

**Causa:** La base de datos está inicializando

**Solución:**

```bash
# Esperar a health check
docker-compose -f compose.frontend-only.yaml logs postgres_db

# Verificar health
docker-compose -f compose.frontend-only.yaml ps | grep postgres_db
```

### "RabbitMQ not responding"

**Causa:** Message broker necesita más tiempo

**Solución:**

```bash
# Ver estado
docker-compose -f compose.frontend-only.yaml logs rabbitmq

# Reiniciar si es necesario
docker-compose -f compose.frontend-only.yaml restart rabbitmq
```

### "Puerto ya en uso"

**Causa:** El puerto ya está ocupado

**Solución:**

```bash
# Encontrar proceso en puerto (ejemplo: 18443)
lsof -i :18443

# Matar proceso
kill -9 <PID>

# O cambiar puerto en compose (ver sección de puertos)
```

---

## 🔗 Relaciones de Dependencias

```
┌─────────────────────────────────────────────────────────────┐
│                    FRONTEND (localhost:3000)                │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│         GATEWAY (localhost:18443) - Ocelot Router           │
│              ↓           ↓            ↓                      │
└─────────────────────────────────────────────────────────────┘
    │           │            │           │           │
    ▼           ▼            ▼           ▼           ▼
┌───────┐ ┌──────────┐ ┌──────────┐ ┌────────┐ ┌────────┐
│ Auth  │ │Vehicles  │ │  Media   │ │ User   │ │Contact │
│       │ │  Sale    │ │          │ │        │ │        │
└───────┘ └──────────┘ └──────────┘ └────────┘ └────────┘
    │           │            │           │           │
    └───────────┴────────────┴───────────┴───────────┘
               │
      ┌────────┼────────┐
      ▼        ▼        ▼
    ┌───┐  ┌──────┐  ┌─────┐
    │PG │  │RabMQ │  │Redis│
    └───┘  └──────┘  └─────┘
```

---

## 📈 Optimizaciones de Recursos

### Limitar CPU y RAM por Servicio

El archivo `compose.frontend-only.yaml` ya incluye limits:

```yaml
deploy:
  resources:
    limits:
      cpus: "0.5" # Max 0.5 CPU cores
      memory: 384M # Max 384 MB RAM
    reservations:
      memory: 256M # Reserved RAM
```

**Total aproximado:**

- PostgreSQL: 1 GB
- RabbitMQ: 512 MB
- Redis: 512 MB
- Cada microservicio: 384 MB × 7 = 2.7 GB
- **Total: ~5 GB RAM**

### Optimizaciones Adicionales

```bash
# Ver uso actual
docker stats --no-stream

# Limpiar imágenes no usadas
docker image prune -a

# Limpiar volúmenes no usados
docker volume prune

# Limpiar todo (CUIDADO)
docker system prune -a
```

---

## 🎯 Flujo de Desarrollo Recomendado

### 1️⃣ Primera Vez

```bash
# 1. Levantar infrastructure
docker-compose -f compose.frontend-only.yaml up -d postgres_db rabbitmq redis consul

# 2. Esperar 30 segundos
sleep 30

# 3. Levantar servicios
docker-compose -f compose.frontend-only.yaml up -d authservice vehiclessaleservice mediaservice userservice contactservice notificationservice adminservice

# 4. Esperar a health checks
docker-compose -f compose.frontend-only.yaml ps

# 5. Levantar gateway (último)
docker-compose -f compose.frontend-only.yaml up -d gateway

# 6. Verificar
curl http://localhost:18443/health
```

### 2️⃣ Desarrollo Diario

```bash
# Levantar todo (lazy start)
docker-compose -f compose.frontend-only.yaml up -d

# O iniciar en foreground para ver logs
docker-compose -f compose.frontend-only.yaml up

# En otra terminal, trabajar en frontend
cd frontend/web
npm install
npm run dev
```

### 3️⃣ Debugging

```bash
# Terminal 1: Ver logs del gateway
docker-compose -f compose.frontend-only.yaml logs -f gateway

# Terminal 2: Ver logs de un servicio
docker-compose -f compose.frontend-only.yaml logs -f vehiclessaleservice

# Terminal 3: Ejecutar frontend
cd frontend/web && npm run dev
```

---

## ✅ Checklist de Verificación

- [ ] Docker Desktop está corriendo
- [ ] `compose.frontend-only.yaml` está en el directorio raíz
- [ ] Variables de entorno en `.env` (opcional)
- [ ] Ejecutar: `docker-compose -f compose.frontend-only.yaml up -d`
- [ ] Esperar a health checks: `docker-compose -f compose.frontend-only.yaml ps`
- [ ] Verificar Gateway: `curl http://localhost:18443/health`
- [ ] Verificar RabbitMQ UI: `http://localhost:15672`
- [ ] Verificar Consul: `http://localhost:8500/ui/`
- [ ] Ejecutar frontend: `npm run dev`
- [ ] Probar API calls desde frontend

---

## 📞 Soporte

### Comandos Útiles

```bash
# Listar containers
docker-compose -f compose.frontend-only.yaml ps

# Ver logs de todos
docker-compose -f compose.frontend-only.yaml logs

# Entrar en shell de un container
docker-compose -f compose.frontend-only.yaml exec gateway /bin/bash

# Ejecutar comando en un container
docker-compose -f compose.frontend-only.yaml exec authservice dotnet ef migrations list

# Rebuildar imágenes
docker-compose -f compose.frontend-only.yaml build --no-cache

# Ver redes
docker network ls
docker network inspect cardealer-microservices_cargurus-net

# Ver volúmenes
docker volume ls
docker volume inspect cardealer-microservices_postgres_data
```

### Performance Tips

```bash
# Usar BuildKit para builds más rápidos
export DOCKER_BUILDKIT=1
docker-compose -f compose.frontend-only.yaml build

# Usar cache de Docker
docker-compose -f compose.frontend-only.yaml build --cache-from

# Limpiar buildx cache
docker buildx prune
```

---

## 🎓 Relación con compose.yaml Original

| Archivo                      | Servicios | Uso                         |
| ---------------------------- | --------- | --------------------------- |
| `compose.yaml`               | 56        | Producción local (completo) |
| `compose.frontend-only.yaml` | 12        | Desarrollo frontend (lean)  |

**Recomendación:**

- **Desarrollo:** Usar `compose.frontend-only.yaml` (rápido, menos recursos)
- **Testing completo:** Usar `compose.yaml` (todos los servicios)
- **CI/CD:** Usar `compose.yaml` (validar integración completa)

---

## 📝 Próximos Pasos

1. **Usar este compose en CI/CD:** GitHub Actions solo levanta frontend-only
2. **Crear compose-mini.yaml:** Mínimo viable (solo PG + Gateway + 1 service)
3. **Documentar perfiles:** Usar Docker Compose profiles para flexibilidad
4. **Automatizar:** Script que levanta servicios con healthchecks

---

_Última actualización: Enero 9, 2026_  
_Creado para facilitar desarrollo rápido del frontend_  
_Email: gmoreno@okla.com.do_
