# 🚀 SPRINT DE DEPLOY - Staging en Digital Ocean

> **Objetivo**: Poner CarDealer funcionando en un servidor de staging  
> **Fecha inicio**: 4 de Diciembre 2025  
> **Estimación**: 2-3 días de trabajo

---

## 📋 RESUMEN DE SPRINTS

| Sprint | Nombre | Tareas | Estado |
|--------|--------|--------|--------|
| Sprint A | Cuentas y Recursos | 4 tareas | ⏳ PENDIENTE |
| Sprint B | Configuración Servidor | 6 tareas | ⏳ PENDIENTE |
| Sprint C | Docker y Contenedores | 5 tareas | ⏳ PENDIENTE |
| Sprint D | Deploy de Aplicación | 6 tareas | ⏳ PENDIENTE |
| Sprint E | Verificación y Monitoreo | 4 tareas | ⏳ PENDIENTE |

---

## 🔵 SPRINT A: Cuentas y Recursos Cloud

### A.1: Crear cuenta en Digital Ocean
**Tiempo estimado**: 5 minutos

**Pasos**:
1. Ir a https://www.digitalocean.com/
2. Click "Sign Up"
3. Usar GitHub login (recomendado) o email
4. Verificar email
5. Agregar método de pago (tarjeta)

**📸 EVIDENCIA REQUERIDA**:
```
Envíame un screenshot del dashboard de Digital Ocean mostrando tu cuenta creada.
O dime: "Cuenta DO creada, mi email es: xxx@xxx.com"
```

**Estado**: [ ] Pendiente

---

### A.2: Crear cuenta en Docker Hub
**Tiempo estimado**: 3 minutos

**Pasos**:
1. Ir a https://hub.docker.com/
2. Click "Sign Up"
3. Crear usuario (IMPORTANTE: recuerda este nombre)
4. Verificar email

**📸 EVIDENCIA REQUERIDA**:
```
Dime: "Docker Hub creado, mi usuario es: [tu-usuario]"
```

**Estado**: [ ] Pendiente

---

### A.3: Crear Droplet en Digital Ocean
**Tiempo estimado**: 5 minutos

**Especificaciones EXACTAS**:
| Campo | Valor |
|-------|-------|
| Image | Ubuntu 22.04 (LTS) x64 |
| Plan | Basic - Regular - $24/mo (4GB RAM / 2 CPUs) |
| Datacenter | New York 1 (NYC1) |
| Authentication | **Password** (más fácil para empezar) |
| Hostname | `cardealer-staging` |

**Pasos**:
1. Dashboard → Create → Droplets
2. Seleccionar Ubuntu 22.04
3. Plan: Basic → Regular → $24/mo
4. Datacenter: NYC1
5. Authentication: Password (anota la contraseña!)
6. Hostname: cardealer-staging
7. Click "Create Droplet"

**📸 EVIDENCIA REQUERIDA**:
```
Envíame:
1. La IP del Droplet (ejemplo: 164.92.xxx.xxx)
2. Confirma que usaste password authentication
```

**Estado**: [ ] Pendiente

---

### A.4: Generar Token de Docker Hub
**Tiempo estimado**: 2 minutos

**Pasos**:
1. Docker Hub → Account Settings → Security
2. Click "New Access Token"
3. Nombre: "cardealer-deploy"
4. Permissions: Read & Write
5. Click "Generate"
6. **COPIA EL TOKEN** (solo se muestra una vez)

**📸 EVIDENCIA REQUERIDA**:
```
Dime: "Token generado y guardado"
(NO me envíes el token, es secreto)
```

**Estado**: [ ] Pendiente

---

## 🟢 SPRINT B: Configuración del Servidor

> ⚠️ **IMPORTANTE**: Para este sprint, yo te ayudaré a ejecutar comandos en el servidor.
> Necesito que me des acceso SSH temporal o ejecutes los comandos que te indique.

### B.1: Conectar al Servidor por SSH
**Tiempo estimado**: 2 minutos

**Desde PowerShell en tu máquina**:
```powershell
ssh root@[TU_IP_DEL_DROPLET]
```

Cuando pregunte password, ingresa el que configuraste.

**📸 EVIDENCIA REQUERIDA**:
```
Envíame el output del comando cuando estés conectado.
Debe mostrar algo como: "root@cardealer-staging:~#"
```

**Estado**: [ ] Pendiente

---

### B.2: Actualizar Sistema Operativo
**Tiempo estimado**: 3 minutos

**Comandos a ejecutar** (te los daré uno por uno):
```bash
apt update && apt upgrade -y
```

**📸 EVIDENCIA REQUERIDA**:
```
Dime cuando termine. Debe decir algo como "X upgraded, Y newly installed"
```

**Estado**: [ ] Pendiente

---

### B.3: Instalar Docker
**Tiempo estimado**: 2 minutos

**Comando**:
```bash
curl -fsSL https://get.docker.com -o get-docker.sh && sh get-docker.sh
```

**📸 EVIDENCIA REQUERIDA**:
```
Ejecuta: docker --version
Envíame el resultado (ejemplo: "Docker version 24.0.7")
```

**Estado**: [ ] Pendiente

---

### B.4: Instalar Docker Compose
**Tiempo estimado**: 1 minuto

**Comando**:
```bash
apt install docker-compose-plugin -y
```

**📸 EVIDENCIA REQUERIDA**:
```
Ejecuta: docker compose version
Envíame el resultado
```

**Estado**: [ ] Pendiente

---

### B.5: Configurar Firewall
**Tiempo estimado**: 2 minutos

**Comandos**:
```bash
ufw allow 22
ufw allow 80
ufw allow 443
ufw allow 5672
ufw allow 15672
ufw --force enable
```

**📸 EVIDENCIA REQUERIDA**:
```
Ejecuta: ufw status
Envíame el resultado
```

**Estado**: [ ] Pendiente

---

### B.6: Crear Estructura de Directorios
**Tiempo estimado**: 1 minuto

**Comandos**:
```bash
mkdir -p /opt/cardealer
mkdir -p /opt/cardealer/data/postgres
mkdir -p /opt/cardealer/data/redis
mkdir -p /opt/cardealer/data/rabbitmq
cd /opt/cardealer
pwd
```

**📸 EVIDENCIA REQUERIDA**:
```
El último comando debe mostrar: /opt/cardealer
```

**Estado**: [ ] Pendiente

---

## 🟡 SPRINT C: Docker y Contenedores

### C.1: Login a Docker Hub desde el Servidor
**Tiempo estimado**: 1 minuto

**Comando**:
```bash
docker login -u [TU_USUARIO_DOCKERHUB]
```
(Te pedirá el token que generaste en A.4)

**📸 EVIDENCIA REQUERIDA**:
```
Debe decir: "Login Succeeded"
```

**Estado**: [ ] Pendiente

---

### C.2: Crear archivo docker-compose.yml
**Tiempo estimado**: 5 minutos

**Comando** (yo te daré el contenido exacto):
```bash
nano /opt/cardealer/docker-compose.yml
```

Te proporcionaré el contenido completo para pegar.

**📸 EVIDENCIA REQUERIDA**:
```
Ejecuta: cat /opt/cardealer/docker-compose.yml | head -20
Envíame las primeras 20 líneas
```

**Estado**: [ ] Pendiente

---

### C.3: Crear archivo .env con variables
**Tiempo estimado**: 3 minutos

**Comando**:
```bash
nano /opt/cardealer/.env
```

Te daré el contenido con valores seguros.

**📸 EVIDENCIA REQUERIDA**:
```
Ejecuta: cat /opt/cardealer/.env | grep -v PASSWORD | grep -v SECRET
Envíame el resultado (sin mostrar passwords)
```

**Estado**: [ ] Pendiente

---

### C.4: Construir Imágenes Docker Localmente
**Tiempo estimado**: 15-20 minutos

Esto lo haremos desde tu máquina local y luego subiremos a Docker Hub.

**📸 EVIDENCIA REQUERIDA**:
```
Te iré pidiendo confirmaciones durante el proceso
```

**Estado**: [ ] Pendiente

---

### C.5: Push de Imágenes a Docker Hub
**Tiempo estimado**: 10-15 minutos

**Comandos** (desde tu máquina local):
```powershell
# Te daré los comandos exactos
```

**📸 EVIDENCIA REQUERIDA**:
```
Ir a Docker Hub y verificar que las imágenes están ahí
Envíame screenshot o lista de imágenes
```

**Estado**: [ ] Pendiente

---

## 🔴 SPRINT D: Deploy de Aplicación

### D.1: Pull de Imágenes en el Servidor
**Tiempo estimado**: 5-10 minutos

**Comando en el servidor**:
```bash
cd /opt/cardealer
docker compose pull
```

**📸 EVIDENCIA REQUERIDA**:
```
Debe mostrar que descargó todas las imágenes
```

**Estado**: [ ] Pendiente

---

### D.2: Iniciar Base de Datos y Servicios Core
**Tiempo estimado**: 2 minutos

**Comando**:
```bash
docker compose up -d postgres redis rabbitmq
```

**📸 EVIDENCIA REQUERIDA**:
```
Ejecuta: docker compose ps
Envíame el resultado
```

**Estado**: [ ] Pendiente

---

### D.3: Esperar que PostgreSQL esté listo
**Tiempo estimado**: 30 segundos

**Comando**:
```bash
docker compose logs postgres | tail -10
```

**📸 EVIDENCIA REQUERIDA**:
```
Debe mostrar: "database system is ready to accept connections"
```

**Estado**: [ ] Pendiente

---

### D.4: Iniciar Microservicios
**Tiempo estimado**: 2 minutos

**Comando**:
```bash
docker compose up -d
```

**📸 EVIDENCIA REQUERIDA**:
```
Ejecuta: docker compose ps
Todos los servicios deben estar "Up"
```

**Estado**: [ ] Pendiente

---

### D.5: Verificar Logs de Servicios
**Tiempo estimado**: 2 minutos

**Comandos**:
```bash
docker compose logs gateway --tail 20
docker compose logs authservice --tail 20
```

**📸 EVIDENCIA REQUERIDA**:
```
No deben haber errores críticos en rojo
Envíame cualquier error que veas
```

**Estado**: [ ] Pendiente

---

### D.6: Ejecutar Migraciones de Base de Datos
**Tiempo estimado**: 5 minutos

**Comandos** (te los daré específicos):
```bash
# Migraciones para cada servicio
```

**📸 EVIDENCIA REQUERIDA**:
```
Confirmación de que las migraciones corrieron
```

**Estado**: [ ] Pendiente

---

## 🟣 SPRINT E: Verificación y Monitoreo

### E.1: Verificar Health Checks
**Tiempo estimado**: 2 minutos

**Desde tu máquina local (PowerShell)**:
```powershell
# Reemplaza IP con la de tu servidor
Invoke-WebRequest -Uri "http://[TU_IP]/health" -UseBasicParsing
```

**📸 EVIDENCIA REQUERIDA**:
```
Debe responder con StatusCode: 200
```

**Estado**: [ ] Pendiente

---

### E.2: Probar API Gateway
**Tiempo estimado**: 2 minutos

**Comando**:
```powershell
Invoke-WebRequest -Uri "http://[TU_IP]/swagger" -UseBasicParsing
```

**📸 EVIDENCIA REQUERIDA**:
```
Debe responder StatusCode: 200
```

**Estado**: [ ] Pendiente

---

### E.3: Verificar RabbitMQ Management
**Tiempo estimado**: 1 minuto

**En tu navegador**:
```
http://[TU_IP]:15672
Usuario: guest
Password: (el que configuramos)
```

**📸 EVIDENCIA REQUERIDA**:
```
Screenshot del dashboard de RabbitMQ
```

**Estado**: [ ] Pendiente

---

### E.4: Test End-to-End
**Tiempo estimado**: 5 minutos

Probaremos crear un usuario y hacer login.

**📸 EVIDENCIA REQUERIDA**:
```
Respuesta exitosa de la API
```

**Estado**: [ ] Pendiente

---

## 📊 TRACKING DE PROGRESO

### Sprint A - Cuentas y Recursos
- [ ] A.1: Cuenta Digital Ocean
- [ ] A.2: Cuenta Docker Hub
- [ ] A.3: Droplet creado
- [ ] A.4: Token Docker Hub

### Sprint B - Configuración Servidor
- [ ] B.1: Conexión SSH
- [ ] B.2: Sistema actualizado
- [ ] B.3: Docker instalado
- [ ] B.4: Docker Compose instalado
- [ ] B.5: Firewall configurado
- [ ] B.6: Directorios creados

### Sprint C - Docker y Contenedores
- [ ] C.1: Login Docker Hub
- [ ] C.2: docker-compose.yml creado
- [ ] C.3: .env configurado
- [ ] C.4: Imágenes construidas
- [ ] C.5: Imágenes en Docker Hub

### Sprint D - Deploy
- [ ] D.1: Pull de imágenes
- [ ] D.2: Servicios core iniciados
- [ ] D.3: PostgreSQL listo
- [ ] D.4: Microservicios iniciados
- [ ] D.5: Logs verificados
- [ ] D.6: Migraciones ejecutadas

### Sprint E - Verificación
- [ ] E.1: Health checks OK
- [ ] E.2: Gateway respondiendo
- [ ] E.3: RabbitMQ accesible
- [ ] E.4: Test E2E exitoso

---

## 🚨 PLAN DE ROLLBACK

Si algo falla, ejecutar en el servidor:
```bash
cd /opt/cardealer
docker compose down
docker compose logs > /tmp/error-logs.txt
```

Y enviarme el archivo de logs.

---

## ⏰ SIGUIENTE PASO

**Empieza por la tarea A.1**: Crear cuenta en Digital Ocean

Cuando tengas la evidencia de cada tarea, envíamela y te confirmo ✅ antes de pasar a la siguiente.

---

*¿Listo para empezar? Dime cuando hayas completado A.1*
