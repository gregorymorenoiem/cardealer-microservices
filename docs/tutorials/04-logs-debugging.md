# Tutorial 4: Logs y Debugging

**Duración:** 50 minutos  
**Nivel:** Intermedio  
**Requisitos:** Tutorial 3 completado

---

## 📋 Resumen

Aprenderás técnicas esenciales de debugging en Kubernetes. Desde ver logs en tiempo real hasta ejecutar comandos dentro de pods y hacer port-forwarding para acceder a servicios internos.

### Lo que aprenderás:
- Ver logs de pods en tiempo real
- Logs de pods multi-contenedor
- Filtrar y buscar en logs
- Ejecutar comandos dentro de pods (exec)
- Port-forwarding para debugging local
- Copiar archivos desde/hacia pods
- Técnicas de troubleshooting comunes

---

## 📑 Índice

1. [Ver logs básicos](#1-ver-logs-básicos)
2. [Logs en tiempo real (follow)](#2-logs-en-tiempo-real-follow)
3. [Logs de pods multi-contenedor](#3-logs-de-pods-multi-contenedor)
4. [Filtrar y buscar en logs](#4-filtrar-y-buscar-en-logs)
5. [Ejecutar comandos en pods (exec)](#5-ejecutar-comandos-en-pods-exec)
6. [Port-forwarding para debugging](#6-port-forwarding-para-debugging)
7. [Copiar archivos (cp)](#7-copiar-archivos-cp)
8. [Debugging de servicios](#8-debugging-de-servicios)
9. [Cheat Sheet](#9-cheat-sheet)
10. [Ejercicios prácticos](#10-ejercicios-prácticos)

---

## 1. Ver logs básicos

### ¿Por qué ver logs?

Los logs son tu **primera línea de defensa** en debugging. Te muestran:
- Errores de aplicación
- Requests HTTP
- Conexiones a base de datos
- Excepciones y stack traces
- Mensajes de inicio/shutdown

### Ver logs de un pod

```bash
kubectl logs <nombre-del-pod> -n okla
```

**Ejemplo:**
```bash
# Primero obtén el nombre exacto del pod
kubectl get pods -n okla | grep gateway

# Ver logs
kubectl logs gateway-77d6c65764-w2mmr -n okla
```

**Salida (ejemplo de .NET):**
```
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://[::]:8080
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /app
```

### Ver últimas N líneas

```bash
# Últimas 50 líneas
kubectl logs gateway-77d6c65764-w2mmr -n okla --tail=50

# Últimas 10 líneas
kubectl logs gateway-77d6c65764-w2mmr -n okla --tail=10
```

**¿Por qué?** Si los logs tienen miles de líneas, solo te interesan las más recientes.

### Ver logs desde hace X tiempo

```bash
# Últimos 5 minutos
kubectl logs gateway-77d6c65764-w2mmr -n okla --since=5m

# Última hora
kubectl logs gateway-77d6c65764-w2mmr -n okla --since=1h

# Últimas 24 horas
kubectl logs gateway-77d6c65764-w2mmr -n okla --since=24h
```

### Ver logs del contenedor anterior (crashed)

Si un pod está en **CrashLoopBackOff**, el contenedor actual está crasheado. Necesitas ver logs del intento anterior:

```bash
kubectl logs gateway-77d6c65764-w2mmr -n okla --previous
```

**Ejemplo de uso:**
```bash
# Ver estado
kubectl get pods -n okla
NAME                       READY   STATUS             RESTARTS
gateway-xxx                0/1     CrashLoopBackOff   5

# Ver logs del crash anterior
kubectl logs gateway-xxx -n okla --previous
```

---

## 2. Logs en tiempo real (follow)

### Flag -f (follow)

Similar a `tail -f` en Linux. Muestra logs en tiempo real conforme se generan.

```bash
kubectl logs -f gateway-77d6c65764-w2mmr -n okla
```

**¿Cuándo usar?**
- Debugging de requests en vivo
- Monitorear despliegues
- Ver comportamiento en tiempo real
- Esperar un log específico

### Follow con límite de líneas

```bash
# Últimas 20 líneas + follow
kubectl logs -f gateway-77d6c65764-w2mmr -n okla --tail=20
```

### Follow desde hace X tiempo

```bash
# Follow desde hace 5 minutos
kubectl logs -f gateway-77d6c65764-w2mmr -n okla --since=5m
```

### Cancelar follow

Presiona **Ctrl+C** para detener el stream de logs.

### Follow de múltiples pods (deployment)

```bash
# Ver logs de TODOS los pods del deployment gateway
kubectl logs -f deployment/gateway -n okla
```

**¿Qué hace esto?**
- Si hay 1 réplica, muestra logs de ese pod
- Si hay múltiples réplicas, sigue el primer pod encontrado
- Si el pod se reinicia, automáticamente sigue el nuevo pod

**Limitación:** Solo sigue un pod a la vez. Para ver todos, necesitas herramientas como `stern`.

---

## 3. Logs de pods multi-contenedor

### ¿Qué es un pod multi-contenedor?

Algunos pods tienen múltiples contenedores:
- **Main container** - Tu aplicación
- **Sidecar container** - Logging, proxy, etc.
- **Init container** - Inicialización (corre antes del main)

### Ver contenedores de un pod

```bash
kubectl describe pod postgres-0 -n okla | grep -A5 "Containers:"
```

**Salida:**
```
Containers:
  postgres:
    Image:      postgres:16
    Port:       5432/TCP
```

En este caso solo hay 1 contenedor. Veamos un ejemplo hipotético con 2:

### Especificar contenedor en logs

```bash
# Pod con 2 contenedores: app y logger
kubectl logs my-pod -c app -n okla        # Logs del contenedor 'app'
kubectl logs my-pod -c logger -n okla     # Logs del contenedor 'logger'
```

### Ver logs de todos los contenedores

```bash
kubectl logs my-pod --all-containers -n okla
```

### Ver logs de init containers

```bash
kubectl logs my-pod -n okla --container=init-db
```

**¿Cuándo usar?** Si un pod no inicia, puede ser porque el init container falló.

---

## 4. Filtrar y buscar en logs

### Usar grep para buscar

```bash
# Buscar "error" en logs
kubectl logs gateway-77d6c65764-w2mmr -n okla | grep -i error

# Buscar requests HTTP 500
kubectl logs gateway-77d6c65764-w2mmr -n okla | grep "500"

# Buscar conexiones a base de datos
kubectl logs vehiclessaleservice-xxx -n okla | grep -i "database"
```

### Grep con contexto

```bash
# Mostrar 5 líneas antes y después del match
kubectl logs gateway-xxx -n okla | grep -A5 -B5 "error"
```

### Buscar en tiempo real con grep

```bash
# Follow y filtrar solo errores
kubectl logs -f gateway-xxx -n okla | grep -i error
```

### Contar ocurrencias

```bash
# Contar cuántos errores hay
kubectl logs gateway-xxx -n okla | grep -i error | wc -l

# Contar requests HTTP 200
kubectl logs gateway-xxx -n okla | grep "200 OK" | wc -l
```

### Buscar múltiples términos

```bash
# Buscar error O exception
kubectl logs gateway-xxx -n okla | grep -iE "error|exception"

# Buscar NullReferenceException O ArgumentException
kubectl logs authservice-xxx -n okla | grep -iE "NullReference|Argument"
```

### Excluir líneas

```bash
# Ver logs pero excluir logs de health checks
kubectl logs gateway-xxx -n okla | grep -v "/health"
```

---

## 5. Ejecutar comandos en pods (exec)

### ¿Por qué exec?

A veces los logs no son suficientes. Necesitas:
- Verificar archivos de configuración
- Probar conectividad (ping, curl, wget)
- Ver variables de entorno
- Instalar herramientas de debugging
- Ejecutar queries SQL directamente

### Entrar en modo interactivo (shell)

```bash
kubectl exec -it <pod-name> -n okla -- sh
```

**Ejemplo:**
```bash
kubectl exec -it gateway-77d6c65764-w2mmr -n okla -- sh
```

**Salida:**
```
/app #  
```

Ahora estás **dentro del contenedor**. Puedes ejecutar comandos:

```sh
# Ver archivos
ls -la

# Ver contenido de archivo
cat ocelot.json

# Ver variables de entorno
env

# Ver procesos
ps aux

# Probar conectividad
ping postgres

# Salir
exit
```

### Ejecutar comando único (sin entrar al shell)

```bash
# Ver variables de entorno
kubectl exec gateway-xxx -n okla -- env

# Ver contenido de archivo
kubectl exec gateway-xxx -n okla -- cat /app/ocelot.json

# Listar archivos
kubectl exec gateway-xxx -n okla -- ls -la /app

# Verificar proceso corriendo
kubectl exec gateway-xxx -n okla -- ps aux | grep dotnet
```

### Probar conectividad entre servicios

```bash
# Desde el gateway, probar conexión a vehiclessaleservice
kubectl exec -it gateway-xxx -n okla -- sh

# Dentro del pod:
wget -qO- http://vehiclessaleservice:8080/health
# O con curl si está instalado:
curl http://vehiclessaleservice:8080/health
```

**¿Para qué sirve?** Verificar que el servicio es alcanzable desde el gateway (debugging de rutas).

### Ejecutar comandos en PostgreSQL

```bash
# Entrar al pod de postgres
kubectl exec -it postgres-0 -n okla -- bash

# Conectar a psql
psql -U postgres -d vehiclessaleservice

# Ejecutar query
SELECT COUNT(*) FROM vehicles;

# Salir
\q
exit
```

**Atajo (todo en un comando):**
```bash
kubectl exec -it postgres-0 -n okla -- psql -U postgres -d vehiclessaleservice -c "SELECT COUNT(*) FROM vehicles;"
```

### Instalar herramientas temporales

```bash
kubectl exec -it gateway-xxx -n okla -- sh

# Instalar curl (Alpine Linux)
apk add curl

# Instalar nano (Alpine)
apk add nano

# Instalar netcat
apk add netcat-openbsd
```

**⚠️ Nota:** Estos cambios se pierden cuando el pod se reinicia.

---

## 6. Port-forwarding para debugging

### ¿Qué es port-forwarding?

Permite acceder a un servicio interno del cluster desde tu máquina local **sin exponerlo públicamente**.

```
Tu máquina (localhost:8080) → kubectl port-forward → Pod en K8s (8080)
```

### Sintaxis básica

```bash
kubectl port-forward <recurso> <puerto-local>:<puerto-remoto> -n <namespace>
```

### Ejemplo: Acceder al Gateway localmente

```bash
kubectl port-forward deployment/gateway 18443:8080 -n okla
```

**Salida:**
```
Forwarding from 127.0.0.1:18443 -> 8080
Forwarding from [::1]:18443 -> 8080
```

Ahora puedes acceder al gateway desde tu navegador:
```
http://localhost:18443/health
```

### Ejemplo: Acceder a PostgreSQL localmente

```bash
kubectl port-forward postgres-0 5433:5432 -n okla
```

Ahora puedes conectar con cualquier cliente SQL:
```
Host: localhost
Port: 5433
User: postgres
Database: vehiclessaleservice
```

### Ejemplo: Acceder a RabbitMQ Management UI

```bash
kubectl port-forward rabbitmq-0 15673:15672 -n okla
```

Abrir en navegador:
```
http://localhost:15673
```

### Port-forward en background

```bash
# Agregar & al final
kubectl port-forward deployment/gateway 18443:8080 -n okla &

# Ver procesos en background
jobs

# Traer a foreground
fg 1

# Detener
kill %1
```

### Port-forward de múltiples servicios

```bash
# Terminal 1
kubectl port-forward deployment/gateway 18443:8080 -n okla

# Terminal 2
kubectl port-forward postgres-0 5433:5432 -n okla

# Terminal 3
kubectl port-forward rabbitmq-0 15673:15672 -n okla
```

### Usar con herramientas locales

```bash
# Port-forward PostgreSQL
kubectl port-forward postgres-0 5433:5432 -n okla &

# Conectar con psql local
psql -h localhost -p 5433 -U postgres -d vehiclessaleservice

# Hacer dump de la BD
pg_dump -h localhost -p 5433 -U postgres vehiclessaleservice > backup.sql
```

---

## 7. Copiar archivos (cp)

### ¿Cuándo copiar archivos?

- Extraer logs personalizados
- Obtener archivos de configuración
- Subir archivos temporales para testing
- Backups manuales

### Copiar desde pod a local

```bash
kubectl cp <namespace>/<pod>:<ruta-remota> <ruta-local>
```

**Ejemplo:**
```bash
# Copiar ocelot.json del gateway a tu máquina
kubectl cp okla/gateway-xxx:/app/ocelot.json ./ocelot-from-pod.json

# Copiar logs de aplicación
kubectl cp okla/authservice-xxx:/app/logs/app.log ./app.log
```

### Copiar desde local a pod

```bash
kubectl cp <ruta-local> <namespace>/<pod>:<ruta-remota>
```

**Ejemplo:**
```bash
# Subir un archivo de config temporal
kubectl cp ./test-config.json okla/gateway-xxx:/tmp/test-config.json

# Verificar que se copió
kubectl exec gateway-xxx -n okla -- ls -la /tmp/
```

### Copiar directorios

```bash
# Copiar directorio completo desde pod
kubectl cp okla/gateway-xxx:/app/logs ./gateway-logs/

# Copiar directorio completo al pod
kubectl cp ./config-files okla/gateway-xxx:/tmp/configs/
```

### Especificar contenedor

Si el pod tiene múltiples contenedores:

```bash
kubectl cp okla/my-pod:/app/data.txt ./data.txt -c my-container
```

---

## 8. Debugging de servicios

### Verificar DNS interno

```bash
# Entrar a cualquier pod
kubectl exec -it gateway-xxx -n okla -- sh

# Probar resolución DNS
nslookup vehiclessaleservice
nslookup vehiclessaleservice.okla.svc.cluster.local

# Probar conectividad
wget -qO- http://vehiclessaleservice:8080/health
```

**Respuesta esperada:**
```
Server:    10.245.0.10
Address 1: 10.245.0.10 kube-dns.kube-system.svc.cluster.local

Name:      vehiclessaleservice
Address 1: 10.116.49.128 vehiclessaleservice.okla.svc.cluster.local
```

### Verificar puertos abiertos

```bash
kubectl exec -it gateway-xxx -n okla -- sh

# Instalar netcat
apk add netcat-openbsd

# Verificar si el puerto 8080 está abierto en vehiclessaleservice
nc -zv vehiclessaleservice 8080
```

**Salida esperada:**
```
vehiclessaleservice (10.116.49.128:8080) open
```

### Verificar variables de entorno

```bash
# Ver todas las variables
kubectl exec gateway-xxx -n okla -- env

# Buscar variables específicas
kubectl exec gateway-xxx -n okla -- env | grep -E "RABBIT|POSTGRES"
```

### Verificar montaje de volumes

```bash
# Ver si el ConfigMap está montado correctamente
kubectl exec gateway-xxx -n okla -- ls -la /app/ocelot.json

# Ver contenido
kubectl exec gateway-xxx -n okla -- cat /app/ocelot.json | head -20
```

### Troubleshooting: Servicio no responde

```bash
# 1. Verificar que el pod está Running
kubectl get pods -n okla -l app=vehiclessaleservice

# 2. Ver logs recientes
kubectl logs -f deployment/vehiclessaleservice -n okla --tail=50

# 3. Describir el pod para ver eventos
kubectl describe pod vehiclessaleservice-xxx -n okla

# 4. Verificar health endpoint desde otro pod
kubectl exec -it gateway-xxx -n okla -- wget -qO- http://vehiclessaleservice:8080/health

# 5. Port-forward y probar desde local
kubectl port-forward deployment/vehiclessaleservice 8081:8080 -n okla
curl http://localhost:8081/health
```

### Troubleshooting: Gateway retorna 503

```bash
# 1. Verificar que el gateway puede alcanzar el servicio
kubectl exec -it gateway-xxx -n okla -- wget -qO- http://vehiclessaleservice:8080/health

# 2. Verificar configuración de Ocelot
kubectl exec gateway-xxx -n okla -- cat /app/ocelot.json | grep -A10 "vehiclessaleservice"

# 3. Ver logs del gateway para ver el error exacto
kubectl logs -f gateway-xxx -n okla | grep -i "503"

# 4. Verificar que el Service existe y tiene endpoints
kubectl get svc vehiclessaleservice -n okla
kubectl get endpoints vehiclessaleservice -n okla
```

---

## 9. Cheat Sheet

### Comandos de Logs

| Comando | Descripción |
|---------|-------------|
| `kubectl logs <pod> -n okla` | Ver logs |
| `kubectl logs -f <pod> -n okla` | Follow logs |
| `kubectl logs <pod> -n okla --tail=50` | Últimas 50 líneas |
| `kubectl logs <pod> -n okla --since=5m` | Últimos 5 minutos |
| `kubectl logs <pod> -n okla --previous` | Logs del contenedor anterior |
| `kubectl logs deployment/<nombre> -n okla` | Logs del deployment |
| `kubectl logs <pod> -c <container> -n okla` | Logs de contenedor específico |

### Comandos de Exec

| Comando | Descripción |
|---------|-------------|
| `kubectl exec -it <pod> -n okla -- sh` | Shell interactivo |
| `kubectl exec <pod> -n okla -- <cmd>` | Ejecutar comando |
| `kubectl exec <pod> -n okla -- env` | Ver variables de entorno |
| `kubectl exec <pod> -n okla -- cat <file>` | Ver archivo |
| `kubectl exec -it postgres-0 -n okla -- psql -U postgres` | Conectar a PostgreSQL |

### Comandos de Port-Forward

| Comando | Descripción |
|---------|-------------|
| `kubectl port-forward <pod> 8080:8080 -n okla` | Port-forward de pod |
| `kubectl port-forward deployment/<nombre> 8080:8080 -n okla` | Port-forward de deployment |
| `kubectl port-forward svc/<nombre> 8080:8080 -n okla` | Port-forward de service |

### Comandos de Copy

| Comando | Descripción |
|---------|-------------|
| `kubectl cp okla/<pod>:<remote> <local>` | Copiar desde pod |
| `kubectl cp <local> okla/<pod>:<remote>` | Copiar a pod |

---

## 10. Ejercicios prácticos

### Ejercicio 1: Analizar logs de un servicio

1. Ve los últimos 30 logs del servicio `authservice`
2. Busca errores en los logs
3. Cuenta cuántos logs hay en total

<details>
<summary>Solución</summary>

```bash
# 1
kubectl logs deployment/authservice -n okla --tail=30

# 2
kubectl logs deployment/authservice -n okla | grep -i error

# 3
kubectl logs deployment/authservice -n okla | wc -l
```
</details>

### Ejercicio 2: Ejecutar comandos en un pod

1. Entra al pod del `gateway` en modo interactivo
2. Lista los archivos en `/app`
3. Verifica que existe `ocelot.json`
4. Sal del pod

<details>
<summary>Solución</summary>

```bash
# 1
kubectl exec -it deployment/gateway -n okla -- sh

# 2
ls -la /app

# 3
cat /app/ocelot.json | head -10

# 4
exit
```
</details>

### Ejercicio 3: Port-forwarding

1. Haz port-forward del servicio `vehiclessaleservice` al puerto local 8081
2. Prueba el endpoint de health desde tu navegador: http://localhost:8081/health
3. Detén el port-forward (Ctrl+C)

<details>
<summary>Solución</summary>

```bash
# 1
kubectl port-forward deployment/vehiclessaleservice 8081:8080 -n okla

# 2
# Abrir navegador: http://localhost:8081/health
# O desde otra terminal:
curl http://localhost:8081/health

# 3
# Presionar Ctrl+C en la terminal del port-forward
```
</details>

### Ejercicio 4: Verificar conectividad entre servicios

1. Entra al pod del `gateway`
2. Instala `curl` (si no está)
3. Prueba conectividad a `authservice:8080/health`
4. Prueba conectividad a `vehiclessaleservice:8080/health`

<details>
<summary>Solución</summary>

```bash
# 1
kubectl exec -it deployment/gateway -n okla -- sh

# 2
apk add curl

# 3
curl http://authservice:8080/health

# 4
curl http://vehiclessaleservice:8080/health

exit
```
</details>

### Ejercicio 5: Debugging completo

1. Ve los logs en tiempo real del `gateway`
2. En otra terminal, haz port-forward del gateway a puerto local 18443
3. Desde otra terminal, haz un request a http://localhost:18443/api/vehicles
4. Observa el log del request en la primera terminal

<details>
<summary>Solución</summary>

```bash
# Terminal 1
kubectl logs -f deployment/gateway -n okla

# Terminal 2
kubectl port-forward deployment/gateway 18443:8080 -n okla

# Terminal 3
curl http://localhost:18443/api/vehicles

# Observar en Terminal 1 el log del request
```
</details>

---

## 🎉 ¡Felicidades!

Has completado el Tutorial 4. Ahora sabes:
- ✅ Ver logs básicos y en tiempo real
- ✅ Filtrar y buscar en logs con grep
- ✅ Ejecutar comandos dentro de pods (exec)
- ✅ Port-forwarding para debugging local
- ✅ Copiar archivos desde/hacia pods
- ✅ Técnicas de debugging de servicios
- ✅ Troubleshooting de problemas comunes

---

**Anterior:** [03 - ConfigMaps y Secrets](./03-configmaps-secrets.md)  
**Siguiente:** [05 - DNS y SSL con cert-manager](./05-dns-ssl.md)
