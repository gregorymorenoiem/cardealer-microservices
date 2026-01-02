# 🐳 Guía de Instalación de Docker Desktop en Windows

**Versión:** 1.0  
**Fecha:** Diciembre 2025  
**Autor:** CarDealer DevOps Team

---

## 📋 Tabla de Contenidos

1. [Requisitos del Sistema](#requisitos-del-sistema)
2. [Paso 1: Habilitar WSL2](#paso-1-habilitar-wsl2)
3. [Paso 2: Instalar WSL2](#paso-2-instalar-wsl2)
4. [Paso 3: Descargar Docker Desktop](#paso-3-descargar-docker-desktop)
5. [Paso 4: Instalar Docker Desktop](#paso-4-instalar-docker-desktop)
6. [Paso 5: Configurar Docker Desktop](#paso-5-configurar-docker-desktop)
7. [Paso 6: Verificar Instalación](#paso-6-verificar-instalación)
8. [Solución de Problemas](#solución-de-problemas)
9. [Comandos Útiles](#comandos-útiles)

---

## 📌 Requisitos del Sistema

### Hardware

| Componente | Requisito Mínimo |
|------------|------------------|
| **Procesador** | 64-bit con SLAT (Second Level Address Translation) |
| **RAM** | 4 GB mínimo (8 GB recomendado) |
| **Disco** | 20 GB de espacio libre |
| **Virtualización** | Habilitada en BIOS/UEFI |

### Software

| Componente | Versión Requerida |
|------------|-------------------|
| **Windows 10** | Versión 2004 o superior (Build 19041+) |
| **Windows 11** | Cualquier versión |
| **WSL 2** | Requerido (se instala en esta guía) |

### Verificar Versión de Windows

Abre PowerShell y ejecuta:

```powershell
winver
```

O desde la terminal:

```powershell
[System.Environment]::OSVersion.Version
```

---

## 🔧 Paso 1: Habilitar WSL2

WSL2 (Windows Subsystem for Linux 2) es **obligatorio** para Docker Desktop.

### Opción A: Instalación Automática (Recomendado)

1. Abre **PowerShell como Administrador**:
   - Presiona `Win + X`
   - Selecciona "Terminal (Admin)" o "Windows PowerShell (Admin)"

2. Ejecuta el comando:

```powershell
wsl --install
```

3. **Reinicia tu computadora** cuando termine.

### Opción B: Instalación Manual (Si la automática falla)

Si `wsl --install` no funciona, sigue estos pasos:

#### 1. Habilitar características de Windows

Ejecuta en PowerShell como Administrador:

```powershell
# Habilitar WSL
dism.exe /online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart

# Habilitar Virtual Machine Platform
dism.exe /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart
```

O usando PowerShell cmdlets:

```powershell
# Habilitar WSL
Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Windows-Subsystem-Linux -NoRestart

# Habilitar Virtual Machine Platform
Enable-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform -NoRestart
```

#### 2. Reiniciar la computadora

```powershell
Restart-Computer
```

#### 3. Descargar e instalar el Kernel de WSL2

Después de reiniciar, descarga el kernel update:

```powershell
# Descargar WSL2 Kernel Update
$kernelUrl = "https://wslstorestorage.blob.core.windows.net/wslblob/wsl_update_x64.msi"
$outputPath = "$env:USERPROFILE\Downloads\wsl_update_x64.msi"
Invoke-WebRequest -Uri $kernelUrl -OutFile $outputPath -UseBasicParsing

# Instalar
Start-Process msiexec.exe -ArgumentList "/i", $outputPath, "/quiet" -Wait
Write-Host "WSL2 Kernel instalado correctamente" -ForegroundColor Green
```

#### 4. Establecer WSL2 como versión por defecto

```powershell
wsl --set-default-version 2
```

---

## 🐧 Paso 2: Instalar una Distribución Linux

Docker Desktop usa WSL2 pero no requiere una distribución. Sin embargo, es recomendable tener una:

### Instalar Ubuntu (Recomendado)

```powershell
wsl --install -d Ubuntu
```

### Otras distribuciones disponibles

```powershell
# Ver distribuciones disponibles
wsl --list --online

# Instalar otra distribución
wsl --install -d Debian
wsl --install -d openSUSE-42
```

### Verificar instalación de WSL

```powershell
wsl --list --verbose
```

Deberías ver algo como:

```
  NAME                   STATE           VERSION
* Ubuntu                 Running         2
```

---

## 📥 Paso 3: Descargar Docker Desktop

### Opción A: Descarga Manual

1. Ve a: https://www.docker.com/products/docker-desktop/
2. Haz clic en "Download for Windows"
3. Guarda el archivo `Docker Desktop Installer.exe`

### Opción B: Descarga con PowerShell

```powershell
# Descargar Docker Desktop
$dockerUrl = "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe"
$outputPath = "$env:USERPROFILE\Downloads\DockerDesktopInstaller.exe"

Write-Host "Descargando Docker Desktop..." -ForegroundColor Cyan
Invoke-WebRequest -Uri $dockerUrl -OutFile $outputPath -UseBasicParsing

Write-Host "Descarga completada: $outputPath" -ForegroundColor Green
```

### Opción C: Usar Winget (Windows Package Manager)

```powershell
winget install Docker.DockerDesktop
```

### Opción D: Usar Chocolatey

```powershell
choco install docker-desktop -y
```

---

## ⚙️ Paso 4: Instalar Docker Desktop

### Instalación Interactiva

1. Ejecuta `Docker Desktop Installer.exe`
2. En el asistente de instalación:
   - ✅ Marca "Use WSL 2 instead of Hyper-V"
   - ✅ Marca "Add shortcut to desktop" (opcional)
3. Haz clic en "Ok" para comenzar la instalación
4. Espera a que termine (puede tomar 5-10 minutos)
5. Haz clic en "Close and restart" cuando termine

### Instalación Silenciosa (Sin GUI)

```powershell
# Instalación silenciosa con WSL2 backend
$installerPath = "$env:USERPROFILE\Downloads\DockerDesktopInstaller.exe"
Start-Process -FilePath $installerPath -ArgumentList "install", "--quiet", "--accept-license" -Wait

Write-Host "Docker Desktop instalado. Reinicia tu PC." -ForegroundColor Green
```

---

## 🔧 Paso 5: Configurar Docker Desktop

### Primer Inicio

1. Después de reiniciar, Docker Desktop debería iniciar automáticamente
2. Si no, búscalo en el menú Inicio: "Docker Desktop"
3. Acepta los términos de servicio
4. Espera a que Docker Engine inicie (icono de ballena en la bandeja del sistema)

### Configuración Recomendada

Abre Docker Desktop → ⚙️ Settings:

#### General
```
✅ Start Docker Desktop when you log in
✅ Use the WSL 2 based engine
```

#### Resources → WSL Integration
```
✅ Enable integration with my default WSL distro
✅ Ubuntu (o tu distribución preferida)
```

#### Resources → Advanced
```
CPUs: 4 (o la mitad de tus cores)
Memory: 4 GB (o 8 GB si tienes suficiente RAM)
Swap: 1 GB
Disk image size: 60 GB
```

#### Docker Engine (daemon.json)

Para proyectos de desarrollo, puedes agregar:

```json
{
  "builder": {
    "gc": {
      "defaultKeepStorage": "20GB",
      "enabled": true
    }
  },
  "experimental": false,
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  }
}
```

---

## ✅ Paso 6: Verificar Instalación

### Verificar Docker

Abre PowerShell y ejecuta:

```powershell
# Verificar versión de Docker
docker --version

# Verificar versión de Docker Compose
docker compose version

# Verificar que el daemon esté corriendo
docker info

# Ejecutar contenedor de prueba
docker run hello-world
```

### Resultado Esperado

```
Hello from Docker!
This message shows that your installation appears to be working correctly.
...
```

### Script de Verificación Completa

```powershell
# verify-docker.ps1
Write-Host "=== Verificación de Docker Desktop ===" -ForegroundColor Cyan

# 1. Docker CLI
Write-Host "`n1. Docker CLI:" -ForegroundColor Yellow
docker --version

# 2. Docker Compose
Write-Host "`n2. Docker Compose:" -ForegroundColor Yellow
docker compose version

# 3. Docker Info
Write-Host "`n3. Docker Engine:" -ForegroundColor Yellow
$info = docker info 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Docker Engine está corriendo" -ForegroundColor Green
    docker info --format '{{.ServerVersion}}'
} else {
    Write-Host "   ❌ Docker Engine no está corriendo" -ForegroundColor Red
}

# 4. WSL Integration
Write-Host "`n4. WSL Integration:" -ForegroundColor Yellow
wsl --list --verbose

# 5. Test Container
Write-Host "`n5. Test Container:" -ForegroundColor Yellow
$result = docker run --rm hello-world 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Contenedor de prueba ejecutado correctamente" -ForegroundColor Green
} else {
    Write-Host "   ❌ Error ejecutando contenedor de prueba" -ForegroundColor Red
}

Write-Host "`n=== Verificación Completada ===" -ForegroundColor Cyan
```

---

## 🔧 Solución de Problemas

### Error: "Docker Desktop is unable to start"

**Causa:** WSL2 no está correctamente instalado.

**Solución:**
```powershell
# 1. Actualizar WSL
wsl --update

# 2. Reiniciar WSL
wsl --shutdown

# 3. Reiniciar Docker Desktop
```

### Error: "WSL 2 installation is incomplete"

**Solución:**
```powershell
# Descargar e instalar el kernel manualmente
$kernelUrl = "https://wslstorestorage.blob.core.windows.net/wslblob/wsl_update_x64.msi"
Invoke-WebRequest -Uri $kernelUrl -OutFile "wsl_update.msi" -UseBasicParsing
Start-Process msiexec.exe -ArgumentList "/i", "wsl_update.msi", "/quiet" -Wait
```

### Error: "Virtualization must be enabled"

**Solución:**
1. Reinicia tu PC y entra al BIOS/UEFI
2. Busca opciones de virtualización:
   - Intel: "Intel VT-x" o "Intel Virtualization Technology"
   - AMD: "AMD-V" o "SVM Mode"
3. Habilita la opción y guarda cambios

Verificar desde Windows:
```powershell
# Verificar si la virtualización está habilitada
Get-ComputerInfo | Select-Object HyperVisorPresent, HyperVRequirementVirtualizationFirmwareEnabled
```

### Error: "Cannot connect to the Docker daemon"

**Solución:**
```powershell
# Reiniciar el servicio de Docker
Restart-Service -Name "com.docker.service"

# O reiniciar Docker Desktop
Stop-Process -Name "Docker Desktop" -Force -ErrorAction SilentlyContinue
Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
```

### Error: "Access denied" o problemas de permisos

**Solución:**
Agregar tu usuario al grupo docker-users:
```powershell
# Ejecutar como Administrador
net localgroup docker-users $env:USERNAME /add
# Cerrar sesión y volver a iniciar
```

### Docker Desktop muy lento

**Solución:**
1. Aumentar recursos en Settings → Resources
2. Excluir carpetas de proyecto del antivirus
3. Usar volumes en lugar de bind mounts para mejor rendimiento

```powershell
# Limpiar recursos no utilizados
docker system prune -af
docker volume prune -f
```

### Liberar espacio en disco

```powershell
# Ver uso de disco
docker system df

# Limpiar todo lo no utilizado
docker system prune -af --volumes

# Limpiar imágenes huérfanas
docker image prune -af

# Limpiar build cache
docker builder prune -af
```

---

## 📝 Comandos Útiles

### Gestión de Docker Desktop

```powershell
# Iniciar Docker Desktop
Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"

# Detener Docker Desktop
Stop-Process -Name "Docker Desktop" -Force

# Reiniciar Docker Desktop
Stop-Process -Name "Docker Desktop" -Force; Start-Sleep 5; Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
```

### Gestión de WSL

```powershell
# Ver distribuciones
wsl --list --verbose

# Reiniciar WSL
wsl --shutdown

# Actualizar WSL
wsl --update

# Establecer versión por defecto
wsl --set-default-version 2
```

### Comandos Docker Básicos

```powershell
# Ver contenedores corriendo
docker ps

# Ver todos los contenedores
docker ps -a

# Ver imágenes
docker images

# Ver uso de recursos
docker stats

# Logs de un contenedor
docker logs <container_name>

# Entrar a un contenedor
docker exec -it <container_name> bash
```

### Docker Compose

```powershell
# Levantar servicios
docker-compose up -d

# Ver estado
docker-compose ps

# Ver logs
docker-compose logs -f

# Detener servicios
docker-compose down

# Reconstruir e iniciar
docker-compose up -d --build
```

---

## 🎉 ¡Instalación Completa!

Una vez completados todos los pasos, deberías tener Docker Desktop funcionando correctamente en Windows.

### Verificación Final

```powershell
docker run -d -p 80:80 nginx
Start-Process "http://localhost"
```

Si ves la página de bienvenida de Nginx, ¡Docker está funcionando perfectamente!

Para limpiar:
```powershell
docker stop $(docker ps -q)
docker rm $(docker ps -aq)
```

---

## 📚 Referencias

- [Documentación Oficial de Docker Desktop](https://docs.docker.com/desktop/install/windows-install/)
- [Documentación de WSL](https://learn.microsoft.com/en-us/windows/wsl/install)
- [Docker Desktop Release Notes](https://docs.docker.com/desktop/release-notes/)
- [WSL Releases](https://github.com/microsoft/WSL/releases)

---

*Última actualización: Diciembre 2025*
