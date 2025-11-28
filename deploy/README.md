# Tutorial Completo de Deploy en Windows Server

## 📋 Prerrequisitos

- **Windows Server** con acceso de administrador
- **Folder `deploy`** completo copiado en el servidor
- **Conexión a internet** para descargar imágenes de Docker

---

## 🚀 **PASO 1: Preparar el Servidor**

### 1.1. Copiar el folder `deploy` al servidor

```powershell
# Copia todo el folder 'deploy' a una ubicación en el servidor, por ejemplo:
C:\deploy\
```

### 1.2. Verificar la estructura del folder `deploy`

```
C:\deploy\
├── .env.production
├── configure-docker-service.ps1
├── deploy-on-server.ps1
├── docker-compose.yml
├── errorservice\
│   ├── Dockerfile
│   └── app\
│       └── (DLLs del servicio)
├── authservice\
│   ├── Dockerfile
│   └── app\
│       └── (DLLs del servicio)
└── (demás servicios...)
```

---

## 🛠 **PASO 2: Instalar y Configurar Docker**

### 2.1. Abrir PowerShell **como Administrador**

### 2.2. Navegar al directorio de deploy

```powershell
cd C:\deploy
```

### 2.3. Ejecutar el script de configuración de Docker

```powershell
.\configure-docker-service.ps1
```

**Este script hará:**

- ✅ Verificar si Docker está instalado
- ✅ Instalar Docker si no existe
- ✅ Configurar el servicio para inicio automático
- ✅ Iniciar el servicio Docker

### 2.4. Verificar la instalación de Docker

```powershell
# Verificar versión de Docker
docker --version

# Verificar que Docker esté corriendo
docker info

# Verificar docker-compose
docker-compose --version
```

---

## ⚙️ **PASO 3: Configurar Variables de Entorno**

### 3.1. Editar el archivo `.env.production`

```powershell
# Abrir el archivo en notepad (como administrador)
notepad .env.production
```

### 3.2. Verificar/Configurar las variables (EJEMPLO - usa tus valores reales):

```env
# Database
DB_USER=cargurus_prod_user
DB_PASSWORD=TuPasswordSuperSeguroAqui123!

# JWT
JWT_SECRET=TuJwtSecretSuperSeguroYComplejoAqui2024!Minimo64CaracteresParaSeguridad

# RabbitMQ
RABBITMQ_USER=guest
RABBITMQ_PASS=guest

# External Services
SENDGRID_API_KEY=SG.tu_api_key_real_aqui
SENDGRID_FROM_EMAIL=notificaciones@cargurus.com
TWILIO_ACCOUNT_SID=tu_account_sid_real
TWILIO_AUTH_TOKEN=tu_auth_token_real
TWILIO_FROM_NUMBER=+1234567890
FIREBASE_PROJECT_ID=cargurus-prod
```

---

## 🚀 **PASO 4: Ejecutar el Deploy**

### 4.1. Desde PowerShell (como Administrador) en `C:\deploy`:

```powershell
.\deploy-on-server.ps1
```

### 4.2. El script hará automáticamente:

```
✅ Verificar estructura del deploy
✅ Parar servicios existentes
✅ Construir imágenes Docker
✅ Iniciar todos los contenedores
✅ Verificar estado de los servicios
✅ Mostrar logs de inicio
```

### 4.3. Monitorear el proceso

```powershell
# Ver todos los contenedores
docker ps -a

# Ver logs en tiempo real
docker-compose logs -f

# Ver logs de un servicio específico
docker logs gateway-prod -f
```

---

## 🔍 **PASO 5: Verificar el Deploy**

### 5.1. Verificar estado de todos los servicios

```powershell
# Listar todos los contenedores y su estado
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
```

### 5.2. Verificar salud de los servicios

```powershell
# Health checks manuales
curl http://localhost:80/health
curl http://localhost:5000/health
curl http://localhost:5001/health
```

### 5.3. Verificar que no hay código fuente expuesto

```powershell
# Verificar contenido de un contenedor (solo debería haber DLLs)
docker exec errorservice-prod ls -la /app
```

---

## 🌐 **PASO 6: Configurar Firewall (Si es necesario)**

### 6.1. Abrir puertos en el firewall de Windows

```powershell
# Abrir puerto 80 (HTTP)
New-NetFirewallRule -DisplayName "CarGurus HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow

# Abrir puerto 443 (HTTPS) - si usas SSL
New-NetFirewallRule -DisplayName "CarGurus HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow

# Ver reglas existentes
Get-NetFirewallRule | Where-Object {$_.DisplayName -like "*CarGurus*"}
```

---

## 📊 **PASO 7: Monitoreo y Mantenimiento**

### 7.1. Scripts útiles para el día a día:

**Crear `monitor-services.ps1`:**

```powershell
# monitor-services.ps1
while ($true) {
    Clear-Host
    Write-Host "=== MONITOREO DE MICROSERVICIOS ===" -ForegroundColor Cyan
    Write-Host "Última actualización: $(Get-Date)" -ForegroundColor Yellow
    docker ps --format "table {{.Names}}\t{{.Status}}\t{{.RunningFor}}\t{{.Ports}}"
    Write-Host "`nPresiona Ctrl+C para salir..." -ForegroundColor Gray
    Start-Sleep -Seconds 10
}
```

**Crear `restart-services.ps1`:**

```powershell
# restart-services.ps1
Write-Host "Reiniciando servicios..." -ForegroundColor Yellow
docker-compose restart
Write-Host "Servicios reiniciados" -ForegroundColor Green
```

**Crear `backup-data.ps1`:**

```powershell
# backup-data.ps1
$backupDir = "C:\backups\$(Get-Date -Format 'yyyy-MM-dd_HH-mm-ss')"
New-Item -ItemType Directory -Path $backupDir -Force

Write-Host "Creando backup en: $backupDir" -ForegroundColor Yellow

# Lista de volúmenes a hacer backup
$volumes = @(
    "errorservice_data",
    "authservice_data",
    "notificationservice_data",
    "vehicleservice_data",
    "contactservice_data",
    "mediaservice_data",
    "auditservice_data",
    "adminservice_data"
)

foreach ($volume in $volumes) {
    Write-Host "Backup de $volume..." -ForegroundColor Gray
    docker run --rm -v "${volume}:/source" -v "${backupDir}:/backup" alpine tar czf /backup/${volume}.tar.gz -C /source .
}

Write-Host "✅ Backup completado: $backupDir" -ForegroundColor Green
```

### 7.2. Comandos útiles para troubleshooting:

```powershell
# Ver uso de recursos
docker stats

# Ver logs de todos los servicios
docker-compose logs

# Ver espacio en disco de imágenes y contenedores
docker system df

# Limpiar recursos no utilizados
docker system prune -f

# Reiniciar un servicio específico
docker-compose restart authservice

# Ver variables de entorno de un contenedor
docker exec authservice-prod env
```

---

## 🛡 **PASO 8: Configurar Reinicio Automático**

### 8.1. Los servicios ya están configurados para reinicio automático en `docker-compose.yml` con:

```yaml
restart: unless-stopped
```

### 8.2. Verificar que Docker se inicia automáticamente:

```powershell
Get-Service docker | Select-Object Name, Status, StartType
```

---

## 📝 **Resolución de Problemas Comunes**

### ❌ Error: "Docker not found"

**Solución:** Ejecutar `.\configure-docker-service.ps1` como Administrador

### ❌ Error: "Port already in use"

**Solución:**

```powershell
# Ver qué proceso usa el puerto
netstat -ano | findstr :80

# O cambiar puertos en docker-compose.yml
```

### ❌ Error: "Database connection failed"

**Solución:** Verificar variables en `.env.production` y que las bases de datos estén saludables

```powershell
docker logs errorservice-db-prod
```

### ❌ Error: "Access denied"

**Solución:** Ejecutar PowerShell como Administrador

---

## ✅ **Verificación Final de que Todo Funciona**

```powershell
# 1. Todos los contenedores deben estar en estado "Up"
docker ps

# 2. El gateway debe responder
curl http://localhost/health

# 3. Los servicios individuales deben responder
curl http://localhost:5000/health
curl http://localhost:5001/health

# 4. Verificar que se pueden acceder a las bases de datos
docker exec errorservice-db-prod pg_isready -U cargurus_prod_user -d errorservice
```

---

## 🎯 **Comandos Rápidos de Referencia**

```powershell
# Deploy completo
cd C:\deploy
.\deploy-on-server.ps1

# Monitoreo
docker-compose logs -f

# Reinicio
docker-compose restart

# Parar todo
docker-compose down

# Ver estado
docker ps

# Ver logs de un servicio
docker logs gateway-prod -f
```

**¡Tu aplicación debería estar ahora ejecutándose en http://localhost!** 🚀

¿Necesitas ayuda con algún paso específico o tienes algún error durante el proceso?
