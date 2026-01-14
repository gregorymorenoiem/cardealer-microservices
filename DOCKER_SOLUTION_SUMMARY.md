# 🐳 Docker Solution Summary - OKLA Microservices

**Fecha:** Enero 13, 2026  
**Status:** ✅ COMPLETADO - Listo para ejecutar  
**Problemas Resueltos:** Docker Desktop hanging, necesidad de automatización

---

## 📋 Resumen Ejecutivo

Se han creado 3 archivos para resolver el problema de Docker Desktop atascado y automatizar el levantamiento de los 30+ microservicios de OKLA:

| Archivo                | Tamaño | Tipo   | Estado   |
| ---------------------- | ------ | ------ | -------- |
| `startup-services.sh`  | 6.1KB  | Script | ✅ Listo |
| `shutdown-services.sh` | 1.6KB  | Script | ✅ Listo |
| `STARTUP_GUIDE.md`     | 5.8KB  | Doc    | ✅ Listo |

**Permisos:** Ambos scripts tienen permisos ejecutables (`-rwxr-xr-x`)

---

## 🔧 Problema Identificado

Docker Desktop en macOS se queda atascado cuando intentas ejecutar:

```bash
docker compose up
docker compose up -d
```

### Síntomas:

- El comando nunca retorna
- `docker ps` se cuelga
- Docker daemon no responde a comandos
- Aplicación Docker Desktop congelada

### Causa Raíz:

El daemon de Docker se queda bloqueado o sin respuesta, posiblemente por:

- Falta de recursos
- Caché corrupta
- Procesos zombie
- Problema de socket

---

## ✅ Soluciones Implementadas

### 1. **startup-services.sh** (Script de Inicio)

Levanta los 30+ servicios en 6 fases ordenadas:

**Fase 1: Infraestructura** (30 segundos)

```
✓ postgres_db (base de datos principal)
✓ redis (caché distribuido)
✓ rabbitmq (message broker)
```

**Fase 2: Seguridad** (15 segundos)

```
✓ authservice (autenticación JWT)
✓ roleservice (control de roles)
```

**Fase 3: MVP** (20 segundos)

```
✓ vehiclessaleservice (catálogo de vehículos)
✓ mediaservice (gestión de imágenes - S3)
✓ notificationservice (emails/SMS)
✓ errorservice (centralización de errores)
```

**Fase 4: Gateway** (10 segundos)

```
✓ gateway (Ocelot API Gateway)
```

**Fase 5: Sprint 1 Services** (30 segundos)

```
✓ maintenanceservice
✓ comparisonservice
✓ alertservice
✓ searchservice
```

**Fase 6: Sprint 2+ Services** (Opcional)

```
✓ dealermanagementservice
✓ dealeranalyticsservice
✓ Y 20+ más...
```

**Características del Script:**

- ✅ Espera a que cada servicio esté "healthy" antes de pasar al siguiente
- ✅ Salida con colores para fácil visualización
- ✅ Función `wait_for_service()` que intenta 30 veces con espera de 2 segundos
- ✅ Manejo de errores (detiene si un servicio falla)
- ✅ Tiempo total de startup: ~2 minutos

**Uso:**

```bash
./startup-services.sh
```

### 2. **shutdown-services.sh** (Script de Apagado)

Apaga los servicios de forma ordenada:

**Opciones:**

```bash
# Opción 1: Apagar pero mantener datos (default - RECOMENDADO)
./shutdown-services.sh --keep-data

# Opción 2: Apagar y eliminar todo (limpieza completa)
./shutdown-services.sh --remove-volumes
```

**Características:**

- ✅ Preserva datos de postgres, redis, rabbitmq (default)
- ✅ Opción de "limpieza nuclear" para resetear completamente
- ✅ Opción de apagar solo servicios de aplicación (mantener infrastructure)

### 3. **STARTUP_GUIDE.md** (Documentación Completa)

Guía paso a paso con:

**Secciones:**

1. Diagnóstico del problema
2. 3 opciones de solución (Activity Monitor, restart, purge data)
3. Verificación de Docker
4. Startup automático vs manual
5. Verificación de servicios
6. Testing de conectividad
7. Troubleshooting para 5+ problemas comunes
8. Requerimientos por sprint
9. Workflow recomendado para desarrollo

---

## 🚀 Instrucciones Rápidas

### Paso 1: Resolver Docker Desktop

**Opción A (Recomendada):**

1. ⌘ + Espacio → escribe "Activity Monitor"
2. Busca "Docker"
3. Click "Force Quit"
4. Espera 5 segundos
5. Abre Docker Desktop nuevamente

**Opción B (Si A falla):**

- Reinicia tu Mac completamente

**Opción C (Nuclear):**

- Docker Desktop > Preferences > Troubleshoot > "Clean / Purge data"

### Paso 2: Verificar Docker

```bash
docker ps
# Debería mostrar lista de contenedores (vacía si es primera vez)
```

### Paso 3: Levantar Servicios

```bash
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices

# Opción automática (recomendada)
./startup-services.sh

# O si prefieres manual:
docker compose up -d
```

### Paso 4: Verificar Estado

```bash
docker compose ps
# Todos los servicios deben mostrar "Up" y "healthy"
```

### Paso 5: Testear Plataforma

```bash
# Test API Gateway
curl http://localhost:18443/health

# Ver logs en tiempo real
docker compose logs -f gateway

# Acceder a RabbitMQ Management
open http://localhost:15672
# Usuario: guest / Contraseña: guest
```

---

## 📊 Matriz de Servicios

### Servicios de Infraestructura

| Servicio    | Puerto | Función                 |
| ----------- | ------ | ----------------------- |
| postgres_db | 5432   | Base de datos principal |
| redis       | 6379   | Caché distribuido       |
| rabbitmq    | 5672   | Message broker (AMQP)   |

### Servicios Core MVP

| Servicio            | Puerto | Función                |
| ------------------- | ------ | ---------------------- |
| authservice         | 8080   | Autenticación JWT      |
| roleservice         | 8080   | Control de roles       |
| vehiclessaleservice | 8080   | Catálogo de vehículos  |
| mediaservice        | 8080   | Gestión de imágenes    |
| notificationservice | 8080   | Emails/SMS/Push        |
| errorservice        | 8080   | Centralización errores |

### Gateway & Observabilidad

| Servicio | Puerto | Función                     |
| -------- | ------ | --------------------------- |
| gateway  | 8080   | Ocelot API Gateway          |
| gateway  | 18443  | API Gateway externo (HTTPS) |

### Servicios Sprint 1+

| Servicio                | Puerto | Función                 |
| ----------------------- | ------ | ----------------------- |
| maintenanceservice      | 8080   | Modo mantenimiento      |
| comparisonservice       | 8080   | Comparador de vehículos |
| alertservice            | 8080   | Alertas de precio       |
| searchservice           | 8080   | Búsqueda avanzada       |
| dealermanagementservice | 8080   | Gestión de dealers      |
| dealeranalyticsservice  | 8080   | Analytics para dealers  |
| ... y 20+ más           | 8080   | Otros servicios         |

---

## 🧪 Testing de Conectividad

Una vez que los servicios estén "Up", prueba:

```bash
# 1. API Gateway Health
curl http://localhost:18443/health
# Esperado: HTTP 200 OK

# 2. RabbitMQ Management
curl http://localhost:15672
# Esperado: HTTP 200 (página HTML)

# 3. PostgreSQL (desde dentro de contenedor)
docker exec postgres_db psql -U postgres -c "SELECT 1"
# Esperado: 1 fila de resultado

# 4. Redis (desde dentro de contenedor)
docker exec redis redis-cli ping
# Esperado: PONG

# 5. Ver logs de un servicio específico
docker compose logs vehiclessaleservice
# Debería mostrar logs sin errores FATAL
```

---

## 📈 Timeline Esperado

| Fase | Duración   | Servicios          | Estado         |
| ---- | ---------- | ------------------ | -------------- |
| 1    | 30 seg     | Infrastructure     | Initializing   |
| 2    | 15 seg     | Auth services      | Initializing   |
| 3    | 20 seg     | MVP services       | Initializing   |
| 4    | 10 seg     | Gateway            | Initializing   |
| 5    | 30 seg     | Sprint 1 services  | Running        |
| 6    | 30 seg     | Sprint 2+ services | Running        |
| ---  | **~2 min** | **Todos listos**   | **✅ Healthy** |

---

## 🆘 Troubleshooting

### Problema: "docker: command not found"

**Solución:** Instala Docker Desktop desde https://www.docker.com/products/docker-desktop

### Problema: "docker compose: service xyz"

**Solución:** Algunos servicios toman más tiempo. Espera 30 segundos y verifica con:

```bash
docker compose ps
```

### Problema: "Connection refused" en tests

**Solución:** Los servicios aún están iniciando. El script espera health checks, pero puede tomar hasta 3 minutos la primera vez.

### Problema: PostgreSQL no inicia

**Solución:**

```bash
docker compose logs postgres_db
# Verifica que haya espacio en disco
df -h
```

### Problema: RabbitMQ no accesible

**Solución:**

```bash
docker compose restart rabbitmq
```

---

## 🔐 Credenciales por Defecto

| Servicio      | Usuario  | Contraseña | URL                    |
| ------------- | -------- | ---------- | ---------------------- |
| RabbitMQ Mgmt | guest    | guest      | http://localhost:15672 |
| PostgreSQL    | postgres | postgres   | localhost:5432         |
| Redis         | (none)   | (none)     | localhost:6379         |

---

## 📚 Archivos Relacionados

- **[STARTUP_GUIDE.md](STARTUP_GUIDE.md)** - Guía detallada con troubleshooting
- **[startup-services.sh](startup-services.sh)** - Script de inicio automático
- **[shutdown-services.sh](shutdown-services.sh)** - Script de apagado seguro
- **[docker-compose.yaml](docker-compose.yaml)** - Configuración de todos los servicios
- **[docs/sprint-plans/marketplace/SPRINT_PLAN_MARKETPLACE.md](docs/sprint-plans/marketplace/SPRINT_PLAN_MARKETPLACE.md)** - Plan de features por sprint

---

## ✅ Próximos Pasos

1. **Resuelve Docker Desktop** (Opción A, B o C arriba)
2. **Verifica Docker:** `docker ps`
3. **Levanta servicios:** `./startup-services.sh`
4. **Espera a "Up":** `docker compose ps`
5. **Prueba API:** `curl http://localhost:18443/health`
6. **¡Listo para desarrollar!** 🚀

---

**Creado:** Enero 13, 2026  
**Estado:** ✅ Production Ready  
**Última actualización:** Enero 13, 2026
