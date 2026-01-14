# 🚀 OKLA Microservices - Startup Guide

## 🔧 Solución del Problema Docker Desktop

Si Docker Desktop se queda atascado en macOS:

### ✅ Opción 1: Reinicio Limpio (Recomendado)

```bash
1. Abre Activity Monitor (⌘ + Espacio → "Activity Monitor")
2. Busca "Docker"
3. Haz click en "Force Quit" (botón arriba a la izquierda)
4. Espera 5 segundos
5. Abre Docker Desktop nuevamente desde Applications
```

### ✅ Opción 2: Reinicio del Sistema

```bash
# Reinicia tu Mac completamente
# Una vez iniciado, abre Docker Desktop
```

### ✅ Opción 3: Resetear Docker (Nuclear)

```bash
1. Docker Desktop > Preferences (⌘,)
2. "Troubleshoot" (abajo a la derecha)
3. "Clean / Purge data"
4. Reinicia Docker
```

---

## 🚀 Levantar Servicios Gradualmente

Una vez que Docker Desktop está funcionando:

### Opción A: Script Automático (Recomendado)

```bash
# Dar permisos de ejecución
chmod +x startup-services.sh

# Ejecutar
./startup-services.sh
```

Este script levanta los servicios en orden:

1. **Infraestructura**: postgres_db, redis, rabbitmq
2. **Auth**: authservice, roleservice
3. **MVP**: vehiclessaleservice, mediaservice, notificationservice, errorservice
4. **Gateway**: Ocelot Gateway
5. **Sprint 1**: maintenanceservice, userservice, billingservice
6. **Sprint 2**: contactservice, comparisonservice, alertservice

### Opción B: Comando Manual Paso a Paso

```bash
# PASO 1: Infraestructura
docker compose up -d postgres_db redis rabbitmq
sleep 5

# PASO 2: Auth
docker compose up -d authservice roleservice
sleep 3

# PASO 3: MVP Services
docker compose up -d vehiclessaleservice mediaservice notificationservice errorservice
sleep 3

# PASO 4: Gateway
docker compose up -d gateway
sleep 3

# PASO 5: Sprint 1 & 2 Services (opcional)
docker compose up -d maintenanceservice userservice billingservice contactservice
```

---

## 🛑 Detener Servicios

### Mantener datos (recomendado)

```bash
./shutdown-services.sh
```

### Eliminar todo (destructivo)

```bash
./shutdown-services.sh --remove-volumes
```

---

## 📊 Verificación

### Ver estado de servicios

```bash
docker compose ps
```

### Ver logs en vivo

```bash
# Todos los servicios
docker compose logs -f

# Un servicio específico
docker compose logs -f gateway
docker compose logs -f vehiclessaleservice
```

### Probar API Gateway

```bash
curl http://localhost:18443/health
```

### Acceder a servicios

```bash
# RabbitMQ Management UI
open http://localhost:15672
# Usuario: guest
# Contraseña: guest

# PostgreSQL
# Host: localhost:5432
# Usuario: postgres
# Contraseña: (ver compose.yaml)

# Redis CLI
docker exec -it $(docker compose ps -q redis) redis-cli
```

---

## 🐛 Troubleshooting

### Error: "docker: command not found"

```bash
# Docker no está instalado o no está en PATH
# Reinstala Docker Desktop: https://www.docker.com/products/docker-desktop
```

### Error: "docker daemon is not running"

```bash
# Docker Desktop no está abierto
# Abre Docker Desktop desde Applications
```

### Error: "Port X is already in use"

```bash
# Otro servicio está usando el puerto
# Ver qué está usando el puerto:
lsof -i :18443

# O cambiar puerto en compose.yaml
```

### Los contenedores se detienen inmediatamente

```bash
# Ver logs detallados
docker compose logs <servicio>

# Verificar configuración de ambiente
cat compose.yaml | grep -A 5 "environment:"
```

### PostgreSQL no se inicializa

```bash
# Verificar inicialización
docker compose logs postgres_db | tail -20

# Esperar más tiempo (hasta 30 segundos)
sleep 30 && docker compose ps postgres_db
```

---

## 📋 Servicios por Sprint

### Sprint 1 (MVP Marketplace)

- ✅ vehiclessaleservice (búsqueda, favoritos)
- ✅ mediaservice (upload imágenes)
- ✅ notificationservice (emails)
- ✅ billingservice (pagos)
- ✅ authservice (login/register)
- ✅ gateway (Ocelot)

### Sprint 2 (Contacto + Comparador)

- 🟡 contactservice (mensajes)
- 🟡 comparisonservice (comparador)
- 🟡 alertservice (alertas de precio)

### Sprint 3 (Publicar Vehículos)

- Sprint 1 services + mediaservice mejorado

### Sprint 4 (Pagos)

- Sprint 1 services + billingservice completo

### Sprint 5-6 (Dealers)

- dealermanagementservice
- inventorymanagementservice
- dealeranalyticsservice

---

## 🔄 Workflow Desarrollo

### Desarrollo diario

```bash
# Iniciar al comienzo del día
./startup-services.sh

# Desarrollar...

# Detener al finalizar
./shutdown-services.sh
```

### Testing

```bash
# En otra terminal, ejecutar tests
cd backend/VehiclesSaleService
dotnet test

# O tests de integración
docker compose run --rm integration-tests
```

### Debugging

```bash
# Ver logs del servicio específico
docker compose logs -f vehiclessaleservice

# Ejecutar comando en contenedor
docker exec -it $(docker compose ps -q vehiclessaleservice) /bin/bash

# Ver recursos usados
docker stats
```

---

## 📈 Monitoreo

### CPU y Memoria

```bash
docker stats --no-stream
```

### Eventos en tiempo real

```bash
docker events --filter type=container
```

### Volúmenes

```bash
docker volume ls
```

---

## ✅ Checklist de Startup Exitoso

- [ ] Docker Desktop abierto y funcionando
- [ ] `docker ps` funciona sin errores
- [ ] postgres_db está "healthy"
- [ ] redis está "Up"
- [ ] rabbitmq está "Up"
- [ ] gateway está "Up"
- [ ] `curl http://localhost:18443/health` retorna 200
- [ ] RabbitMQ UI accesible en http://localhost:15672

---

## 📞 Soporte

Si los scripts no funcionan:

1. Verifica que estés en el directorio correcto:

   ```bash
   pwd
   # Debe terminar en: /cardealer-microservices
   ```

2. Verifica permisos de ejecución:

   ```bash
   ls -l startup-services.sh
   # Debe mostrar: -rwxr-xr-x
   ```

3. Ejecuta con bash explícitamente:

   ```bash
   bash startup-services.sh
   ```

4. Revisa los logs:
   ```bash
   docker compose logs postgres_db | tail -50
   ```

---

**Última actualización:** Enero 13, 2026  
**Versión:** 1.0  
**Estado:** ✅ Testeado
