# 🚀 Guía de Deploy a Digital Ocean - Staging

> **Para**: Desarrolladores nuevos en CI/CD  
> **Objetivo**: Servidor de staging en Digital Ocean  
> **Fecha**: 4 de Diciembre 2025

---

## 📍 Dónde Estás Ahora

```
✅ Código en GitHub (main branch)
✅ CI/CD Pipeline configurado
✅ Tests pasando (1,483+ tests)
✅ Docker images definidas
⏳ SIGUIENTE → Deploy a Digital Ocean
```

---

## 🗺️ Roadmap Completo

```
┌─────────────────────────────────────────────────────────────────┐
│                    TU CAMINO A PRODUCCIÓN                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  FASE 1: CI (Ya completado ✅)                                   │
│  ┌──────────┐   ┌──────────┐   ┌──────────┐                     │
│  │  Código  │ → │  GitHub  │ → │  Tests   │                     │
│  │  Local   │   │  Actions │   │  Pasan   │                     │
│  └──────────┘   └──────────┘   └──────────┘                     │
│                                                                  │
│  FASE 2: Container Registry (Siguiente paso)                    │
│  ┌──────────┐   ┌──────────┐   ┌──────────┐                     │
│  │  Build   │ → │  Push a  │ → │ Imágenes │                     │
│  │  Docker  │   │ Registry │   │  Listas  │                     │
│  └──────────┘   └──────────┘   └──────────┘                     │
│                                                                  │
│  FASE 3: Deploy a Digital Ocean                                  │
│  ┌──────────┐   ┌──────────┐   ┌──────────┐                     │
│  │  Pull    │ → │  Deploy  │ → │   App    │                     │
│  │ Imágenes │   │  Stack   │   │  LIVE!   │                     │
│  └──────────┘   └──────────┘   └──────────┘                     │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📋 PASO 1: Crear Cuenta en Digital Ocean

### 1.1 Registrarte
1. Ir a https://www.digitalocean.com/
2. Crear cuenta (puedes usar GitHub login)
3. Agregar método de pago

### 1.2 Obtener Crédito Gratis (Opcional)
- Digital Ocean ofrece $200 de crédito por 60 días para nuevos usuarios
- Buscar "DigitalOcean $200 credit" para links de referencia

---

## 📋 PASO 2: Crear Droplet (Servidor Virtual)

### 2.1 Especificaciones Recomendadas para Staging

| Componente | Recomendación | Costo/mes |
|------------|---------------|-----------|
| **Droplet Size** | 4GB RAM / 2 vCPUs | ~$24/mes |
| **Imagen** | Ubuntu 22.04 LTS | Incluido |
| **Región** | NYC1 o más cercano | Incluido |
| **Almacenamiento** | 80GB SSD | Incluido |

> 💡 **Para 25 microservicios**, recomiendo mínimo 4GB RAM.  
> Opción económica: 2GB RAM ($12/mes) pero solo para pruebas ligeras.

### 2.2 Crear el Droplet

1. Dashboard → **Create** → **Droplets**
2. Seleccionar:
   - **Image**: Ubuntu 22.04 (LTS) x64
   - **Plan**: Basic → Regular → $24/mo (4GB/2CPU)
   - **Datacenter**: New York 1 (o el más cercano a ti)
   - **Authentication**: SSH Key (recomendado) o Password
   - **Hostname**: `cardealer-staging`

3. Click **Create Droplet**

### 2.3 Obtener IP del Servidor
```
Tu Droplet IP: XXX.XXX.XXX.XXX (la verás en el dashboard)
```

---

## 📋 PASO 3: Configurar el Servidor

### 3.1 Conectarse por SSH

```bash
# Desde PowerShell o Terminal
ssh root@TU_IP_DEL_DROPLET
```

### 3.2 Script de Configuración Inicial

Copia y ejecuta este script en el servidor:

```bash
#!/bin/bash
# setup-staging.sh

echo "🚀 Configurando servidor staging para CarDealer..."

# Actualizar sistema
apt update && apt upgrade -y

# Instalar Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# Instalar Docker Compose
apt install docker-compose-plugin -y

# Verificar instalación
docker --version
docker compose version

# Crear usuario para deploy (más seguro que root)
adduser --disabled-password --gecos "" deploy
usermod -aG docker deploy

# Crear directorio para la aplicación
mkdir -p /opt/cardealer
chown deploy:deploy /opt/cardealer

# Configurar firewall
ufw allow 22      # SSH
ufw allow 80      # HTTP
ufw allow 443     # HTTPS
ufw allow 18443   # Gateway
ufw --force enable

echo "✅ Servidor configurado!"
echo "📝 IP del servidor: $(curl -s ifconfig.me)"
```

---

## 📋 PASO 4: Configurar Container Registry

### Opción A: Docker Hub (Gratis, más fácil)

1. Crear cuenta en https://hub.docker.com/
2. Crear repositorio: `tuusuario/cardealer`

### Opción B: Digital Ocean Container Registry (Integrado)

1. Dashboard → **Container Registry** → **Create**
2. Nombre: `cardealer-registry`
3. Plan: Starter (gratis, 500MB) o Basic ($5/mes, 5GB)

### Opción C: GitHub Container Registry (Recomendado)

Ya viene integrado con GitHub, sin configuración extra.

---

## 📋 PASO 5: Configurar GitHub Actions para Deploy

### 5.1 Agregar Secrets en GitHub

Ir a: **Settings** → **Secrets and variables** → **Actions**

Agregar estos secrets:

| Secret Name | Valor |
|-------------|-------|
| `DOCKERHUB_USERNAME` | Tu usuario de Docker Hub |
| `DOCKERHUB_TOKEN` | Token de acceso de Docker Hub |
| `STAGING_HOST` | IP de tu Droplet |
| `STAGING_USER` | `deploy` |
| `STAGING_SSH_KEY` | Tu clave SSH privada |

### 5.2 Crear Workflow de Deploy

Crear archivo `.github/workflows/deploy-staging.yml`:

```yaml
name: Deploy to Staging

on:
  workflow_run:
    workflows: ["CI/CD Pipeline"]
    types:
      - completed
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' }}
    
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Login to Docker Hub
        uses: docker/login-action@v3
        with:
          username: ${{ secrets.DOCKERHUB_USERNAME }}
          password: ${{ secrets.DOCKERHUB_TOKEN }}

      - name: Build and push images
        run: |
          cd backend
          
          # Build y push de servicios principales
          services=(
            "errorservice:ErrorService"
            "authservice:AuthService"
            "notificationservice:NotificationService"
            "gateway:Gateway"
          )
          
          for svc in "${services[@]}"; do
            name="${svc%%:*}"
            path="${svc##*:}"
            
            docker build -t ${{ secrets.DOCKERHUB_USERNAME }}/cardealer-${name}:latest \
              -f ${path}/Dockerfile .
            docker push ${{ secrets.DOCKERHUB_USERNAME }}/cardealer-${name}:latest
          done

      - name: Deploy to Staging Server
        uses: appleboy/ssh-action@v1.0.0
        with:
          host: ${{ secrets.STAGING_HOST }}
          username: ${{ secrets.STAGING_USER }}
          key: ${{ secrets.STAGING_SSH_KEY }}
          script: |
            cd /opt/cardealer
            
            # Pull latest images
            docker compose pull
            
            # Restart services
            docker compose up -d
            
            # Cleanup old images
            docker image prune -f
            
            echo "✅ Deploy completado!"
```

---

## 📋 PASO 6: Archivo docker-compose para Staging

Crear `docker-compose.staging.yml` en el servidor:

```yaml
version: '3.8'

services:
  # Base de datos PostgreSQL
  postgres:
    image: postgres:16
    environment:
      POSTGRES_USER: cardealer
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      POSTGRES_DB: cardealer
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: always

  # Redis para cache
  redis:
    image: redis:7-alpine
    restart: always

  # RabbitMQ para mensajería
  rabbitmq:
    image: rabbitmq:3.12-management
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}
    restart: always

  # Gateway (punto de entrada)
  gateway:
    image: ${DOCKER_REGISTRY}/cardealer-gateway:latest
    ports:
      - "80:80"
      - "443:443"
    environment:
      ASPNETCORE_ENVIRONMENT: Staging
    depends_on:
      - authservice
      - errorservice
    restart: always

  # AuthService
  authservice:
    image: ${DOCKER_REGISTRY}/cardealer-authservice:latest
    environment:
      ASPNETCORE_ENVIRONMENT: Staging
      ConnectionStrings__DefaultConnection: "Host=postgres;Database=authservice;Username=cardealer;Password=${DB_PASSWORD}"
    depends_on:
      - postgres
      - redis
    restart: always

  # ErrorService
  errorservice:
    image: ${DOCKER_REGISTRY}/cardealer-errorservice:latest
    environment:
      ASPNETCORE_ENVIRONMENT: Staging
      ConnectionStrings__DefaultConnection: "Host=postgres;Database=errorservice;Username=cardealer;Password=${DB_PASSWORD}"
    depends_on:
      - postgres
    restart: always

  # NotificationService
  notificationservice:
    image: ${DOCKER_REGISTRY}/cardealer-notificationservice:latest
    environment:
      ASPNETCORE_ENVIRONMENT: Staging
      ConnectionStrings__DefaultConnection: "Host=postgres;Database=notificationservice;Username=cardealer;Password=${DB_PASSWORD}"
    depends_on:
      - postgres
      - rabbitmq
    restart: always

volumes:
  postgres_data:
```

---

## 📋 PASO 7: Variables de Entorno

Crear archivo `.env` en el servidor (`/opt/cardealer/.env`):

```bash
# Database
DB_PASSWORD=TuPasswordSeguro123!

# RabbitMQ
RABBITMQ_PASSWORD=RabbitPasswordSeguro!

# Docker Registry
DOCKER_REGISTRY=tuusuario

# JWT
JWT_SECRET=TuSecretoJWTMuyLargo123!

# SendGrid (emails)
SENDGRID_API_KEY=SG.xxx

# Twilio (SMS)
TWILIO_ACCOUNT_SID=ACxxx
TWILIO_AUTH_TOKEN=xxx
```

---

## 📋 PASO 8: Primer Deploy Manual

### En tu servidor staging:

```bash
# Conectar al servidor
ssh deploy@TU_IP_DEL_DROPLET

# Ir al directorio
cd /opt/cardealer

# Crear archivo .env (con tus variables)
nano .env

# Copiar docker-compose.staging.yml
nano docker-compose.yml

# Pull de imágenes
docker compose pull

# Iniciar servicios
docker compose up -d

# Verificar que están corriendo
docker compose ps

# Ver logs
docker compose logs -f
```

---

## 🎯 Resumen: Tu Checklist

### Esta Semana:
- [ ] Crear cuenta en Digital Ocean
- [ ] Crear Droplet (4GB RAM)
- [ ] Configurar servidor con script
- [ ] Crear cuenta Docker Hub

### Próxima Semana:
- [ ] Agregar secrets en GitHub
- [ ] Crear workflow de deploy
- [ ] Primer deploy manual
- [ ] Verificar que funciona

### Después:
- [ ] Configurar dominio (opcional)
- [ ] Agregar SSL con Let's Encrypt
- [ ] Configurar backups automáticos
- [ ] Monitoreo con Grafana

---

## 💰 Costos Estimados

| Servicio | Costo Mensual |
|----------|---------------|
| Droplet 4GB | $24 |
| Container Registry | $0-5 |
| Dominio (opcional) | $1-2 |
| **Total Staging** | **~$25-30/mes** |

---

## 🆘 Si Necesitas Ayuda

1. **Documentación Digital Ocean**: https://docs.digitalocean.com/
2. **Docker Docs**: https://docs.docker.com/
3. **GitHub Actions**: https://docs.github.com/en/actions

---

## ⚡ Comando Rápido para Verificar Estado

Una vez desplegado, desde tu máquina:

```powershell
# Verificar que el gateway responde
Invoke-WebRequest -Uri "http://TU_IP_DEL_DROPLET/health"

# O con curl
curl http://TU_IP_DEL_DROPLET/health
```

---

*¡Buena suerte con tu primer deploy! 🚀*
