# HTTPS Local + Dominio Público — Setup & Metodología

> **Objetivo:** Replicar la arquitectura de producción (HTTPS + dominio público) localmente  
> en tu Mac M5, sin depender de infraestructura externa.

---

## Arquitectura

### Producción (DigitalOcean K8s)

```
Internet → DNS okla.com.do → nginx ingress (HTTPS, Let's Encrypt)
              ├─ /api/*  → Next.js (SSR rewrite) → Gateway:8080 → Microservices
              └─ /*      → Next.js (frontend-web:8080)
```

### Local (tu Mac M5)

```
Browser → https://okla.local (Caddy + mkcert, puerto 443)
              ├─ /api/*  → gateway:80 (Ocelot, Docker network)
              └─ /*      → host.docker.internal:3000 (Next.js pnpm dev)

Opcional — dominio público temporal:
Browser → https://xxxx.trycloudflare.com (Cloudflare Tunnel, gratis)
              └─ tunnel → caddy:80 → mismas rutas de arriba
```

### Comparación

| Componente     | Producción               | Local                               |
| -------------- | ------------------------ | ----------------------------------- |
| Reverse Proxy  | nginx ingress controller | Caddy 2 (Docker)                    |
| TLS            | Let's Encrypt (auto)     | mkcert (certs locales confiables)   |
| Dominio        | `okla.com.do` (DNS real) | `okla.local` (/etc/hosts)           |
| Público        | Siempre                  | `cloudflared` túnel bajo demanda    |
| Frontend       | Next.js en K8s pod       | `pnpm dev` en host (hot-reload)     |
| Gateway        | Ocelot en K8s pod        | Ocelot en Docker container          |
| Microservicios | K8s pods (ClusterIP)     | Docker containers (okla-net bridge) |

---

## Setup Único (5 minutos)

### Prerrequisitos

- macOS con Homebrew
- Docker Desktop instalado y corriendo
- Node.js 20+ y pnpm

### Script automático

```bash
# Desde la raíz del repo
./infra/setup-https-local.sh
```

Esto ejecuta los 5 pasos automáticamente. Si prefieres hacerlo manual:

### Pasos manuales

#### 1. Instalar herramientas

```bash
brew install mkcert cloudflared
```

#### 2. Instalar CA raíz (pide contraseña del sistema)

```bash
mkcert -install
# Instala la CA en macOS Keychain → los navegadores confían en los certs locales
```

#### 3. Generar certificados TLS

```bash
mkdir -p infra/caddy/certs
cd infra/caddy/certs

mkcert -cert-file okla.local.pem -key-file okla.local-key.pem \
    "okla.local" "*.okla.local" "localhost" "127.0.0.1" "::1"

# Copiar CA para Docker
cp "$(mkcert -CAROOT)/rootCA.pem" ./rootCA.pem
```

#### 4. Agregar dominio local

```bash
# Agrega okla.local → 127.0.0.1
echo "127.0.0.1    okla.local" | sudo tee -a /etc/hosts
```

#### 5. Configurar variables de entorno

```bash
cp .env.local.example .env
# Editar .env con tus valores reales (API keys, OAuth, etc.)
```

---

## Uso Diario

### Modo 1: HTTPS local (el 95% del tiempo)

La forma estándar de desarrollar. Caddy corre con la infra (no tiene profile).

```bash
# Terminal 1 — levantar infra (incluye Caddy)
docker compose up -d

# Terminal 2 — frontend
cd frontend/web-next && pnpm dev

# Terminal 3 — backend (si necesitas hot-reload de un servicio)
cd backend/AuthService
dotnet watch run --project AuthService.Api/AuthService.Api.csproj
```

Abrir: **https://okla.local**

La URL funciona igual que producción:

- `https://okla.local` → frontend Next.js
- `https://okla.local/api/auth/login` → gateway → authservice
- `https://okla.local/api/vehicles` → gateway → vehiclessaleservice

### Modo 2: Dominio público temporal (webhooks, OAuth, móvil)

Cuando necesitas que un servicio externo (Stripe, OAuth, etc.) te envíe callbacks,
o quieres probar desde un teléfono en la misma red.

```bash
# 1. Levantar infra + túnel
docker compose --profile tunnel up -d cloudflared

# 2. Obtener la URL pública
docker compose logs cloudflared | grep "trycloudflare.com"
# Output: https://random-words-random.trycloudflare.com

# 3. Usar esa URL para:
#    - OAuth redirect URI
#    - Stripe webhook URL
#    - Testing desde móvil

# 4. Cuando termines, apagar el túnel
docker compose --profile tunnel down
```

**Importante:** La URL cambia cada vez que levantas el túnel. Para OAuth,
actualiza la redirect URI en Google Console / Azure cada vez.

### Modo 3: Sin Caddy (debugging rápido)

Si no necesitas HTTPS, accede directamente a los puertos:

```bash
docker compose up -d
pnpm dev

# Frontend: http://localhost:3000
# Gateway:  http://localhost:18443
# Swagger:  http://localhost:15001/swagger (AuthService directo)
```

---

## Cómo Funciona Cada Componente

### Caddy (reverse proxy)

- **Imagen:** `caddy:2-alpine` (~15 MB)
- **Config:** `infra/caddy/Caddyfile`
- **Puertos:** 443 (HTTPS) + 80 (HTTP para túnel)
- **RAM:** ~64 MB
- **Rol:** Equivalente al nginx ingress de K8s

Caddy recibe todo el tráfico en `https://okla.local` y lo distribuye:

- `/api/*` → `gateway:80` (API, misma ruta que producción)
- `/*` → `host.docker.internal:3000` (frontend Next.js en el host)

La variable `FRONTEND_UPSTREAM` en `.env` controla a dónde va el tráfico del frontend:

- `host.docker.internal:3000` (default) → Next.js corriendo con `pnpm dev` en el host
- `frontend-next:3000` → Next.js corriendo dentro de Docker

### mkcert (certificados TLS)

- Crea una **CA raíz local** que se instala en macOS Keychain
- Genera certificados firmados por esa CA
- **Chrome, Firefox, Safari** confían automáticamente → candado verde ✅
- Los certs nunca se suben a git (están en `.gitignore`)

### cloudflared (túnel público)

- **Sin cuenta necesaria** — usa Cloudflare Quick Tunnel
- Crea un túnel cifrado: `*.trycloudflare.com` → tu `localhost:80`
- Cloudflare provee HTTPS automático al exterior
- **Gratis, temporal** — la URL cambia cada vez

---

## Troubleshooting

### "NET::ERR_CERT_AUTHORITY_INVALID" en Chrome

```bash
# La CA de mkcert no está instalada
mkcert -install
# Reiniciar Chrome completamente (Cmd+Q, no solo cerrar tab)
```

### "okla.local no se puede alcanzar"

```bash
# Verificar /etc/hosts
cat /etc/hosts | grep okla

# Si no existe:
echo "127.0.0.1    okla.local" | sudo tee -a /etc/hosts

# Verificar que Caddy está corriendo
docker compose ps caddy
```

### "502 Bad Gateway" en https://okla.local

```bash
# El frontend no está corriendo
pnpm dev  # en otra terminal

# O el gateway no está healthy
docker compose ps gateway
docker compose logs gateway --tail=20
```

### "502" en https://okla.local/api/*

```bash
# El gateway no puede conectar a los microservicios
# Verificar que el profile correcto está levantado
docker compose --profile core ps

# Ver logs del gateway
docker compose logs gateway --tail=50
```

### cloudflared no muestra URL

```bash
# Esperar 5 segundos y revisar logs completos
docker compose logs cloudflared

# Si Caddy no está healthy, cloudflared no arranca
docker compose ps caddy
```

### Puerto 443 ya ocupado

```bash
# Verificar qué proceso usa el puerto
sudo lsof -i :443

# Si es AirPlay Receiver (macOS):
# System Settings → General → AirDrop & Handoff → AirPlay Receiver → OFF
```

---

## Seguridad

- Los certificados en `infra/caddy/certs/` son **solo para desarrollo local**
- Están en `.gitignore` — nunca se suben al repo
- La CA raíz es local a tu máquina — no afecta otros equipos
- cloudflared tunnel es temporal y no requiere exponer puertos del firewall
- Las mismas cabeceras de seguridad de producción están en el Caddyfile (X-Frame-Options, HSTS, etc.)

---

## Referencia de Archivos

| Archivo                                | Propósito                                                   |
| -------------------------------------- | ----------------------------------------------------------- |
| `infra/setup-https-local.sh`           | Script de setup automático (ejecutar una vez)               |
| `infra/caddy/Caddyfile`                | Configuración de Caddy (rutas, TLS)                         |
| `infra/caddy/certs/okla.local.pem`     | Certificado TLS (generado por mkcert)                       |
| `infra/caddy/certs/okla.local-key.pem` | Llave privada TLS (generada por mkcert)                     |
| `infra/caddy/certs/rootCA.pem`         | CA raíz de mkcert (para Docker)                             |
| `.env.local.example`                   | Template con URLs HTTPS configuradas                        |
| `compose.yaml`                         | Servicios `caddy` (infra) + `cloudflared` (profile: tunnel) |
