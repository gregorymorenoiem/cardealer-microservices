# Flujo de QA Local — OKLA Dev Environment

> **Fecha de implementación:** Marzo 2026  
> **Contexto:** Documentación de todas las piezas que se configuraron para tener un ambente local de desarrollo con paridad de producción, auditoría de agentes IA y flujo CI/CD local completo.

---

## ¿Qué se implementó?

### 1. Cloudflared Tunnel — Dominio público temporal

Se agregó `cloudflared` al `compose.yaml` bajo el profile `tunnel`. Permite exponer el stack local (Caddy:80) mediante un dominio público temporal `*.trycloudflare.com` sin necesidad de cuenta ni configuración DNS.

**Caso de uso:**

- Testing de agentes IA desde auditorías externas
- OAuth callbacks (Google, Microsoft) desde servicios externos
- Testing desde dispositivos móviles en otra red
- Webhooks de terceros (PayPal, WhatsApp)

**Comando para levantar:**

```bash
# Levantar el tunnel
docker compose --profile tunnel up -d cloudflared

# Ver la URL pública
docker compose logs cloudflared | grep trycloudflare.com
# Output: https://xxxx-xxxx.trycloudflare.com
```

**Arquitectura:**

```
Internet → https://xxxx.trycloudflare.com
              └─ cloudflared container (túnel cifrado)
                    └─ caddy:80 (reverse proxy local)
                          ├─ /api/*  → gateway:80 (Ocelot)
                          └─ /*      → host.docker.internal:3000 (Next.js)
```

**Limitaciones knownas:**

- La URL cambia cada vez que se levanta el túnel (Quick Tunnel gratuita)
- Latencia extra: 15-35× más lenta que localhost (normal para un túnel)
- No persistente: solo para sesiones de testing

---

### 2. CORS Fix — Variable `NEXT_PUBLIC_API_URL`

**Problema encontrado:** Al acceder al frontend via el tunnel externo, el navegador bloqueaba las llamadas API con:

```
Access to XMLHttpRequest at 'http://localhost:18443/api/...'
from origin 'https://xxxx.trycloudflare.com' has been blocked by CORS
```

**Causa raíz:** `NEXT_PUBLIC_API_URL=http://localhost:18443` en `.env.local` hace que el cliente del browser use la URL absoluta `http://localhost:18443`, que no existe desde una URL externa.

**Solución implementada:** Cuando se accede via tunnel o desde un dispositivo externo, iniciar el frontend con `NEXT_PUBLIC_API_URL=` (vacío), lo que activa el modo BFF (Backend for Frontend):

```
NEXT_PUBLIC_API_URL vacío → browser usa URLs relativas (/api/*)
→ Next.js rewrites proxy a gateway → sin CORS
```

**Cómo iniciarlo para el tunnel:**

```bash
# Para uso con tunnel / dominio externo (NEXT_PUBLIC_API_URL vacío = modo BFF)
cd frontend/web-next
NEXT_PUBLIC_API_URL= pnpm dev

# Para uso local normal (localhost → acceso directo al gateway)
cd frontend/web-next
pnpm dev  # usa NEXT_PUBLIC_API_URL=http://localhost:18443 del .env.local
```

**Por qué funciona:**

- Con URL vacía, el browser hace `GET /api/search-agent/health` (relativo)
- La petición va a `https://xxxx.trycloudflare.com/api/search-agent/health`
- Caddy la recibe y la proxea a `gateway:80`
- Caddy ya tiene el dominio del tunnel como origen → sin CORS
- Misma arquitectura que en producción (`okla.com.do/api/*` → gateway interno)

---

### 3. Docker Compose — Fixes implementados

Durante la sesión de audit se corrigieron varios servicios en `compose.yaml`:

| Servicio                 | Fix aplicado                                                                                                                                        |
| ------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| `vehiclessaleservice`    | Added `OKLA_PII_ENCRYPTION_KEY` + volumenes shared faltantes (`Audit`, `Resilience`, `Caching`, `ErrorService.Domain`, `ErrorService.Shared`)       |
| `contactservice`         | Added `OKLA_PII_ENCRYPTION_KEY`                                                                                                                     |
| `chatbotservice`         | Added `OKLA_PII_ENCRYPTION_KEY`                                                                                                                     |
| `dealeranalyticsservice` | Added volumenes shared faltantes (`Logging`, `ErrorHandling`, `Observability`, `Audit`, `Resilience`, `ErrorService.Domain`, `ErrorService.Shared`) |

**Llave PII compartida:**

```
OKLA_PII_ENCRYPTION_KEY=CMx0ZJgjwLb3GdHw6laG0ICy09Zu9nKcNUtdzRJNfSQ=
```

> Esta llave es solo para el ambiente local de desarrollo. En producción se usa un K8s Secret.

---

### 4. ChatbotService en Host (no Docker)

El `ChatbotService` corre en el **host** (no en Docker) en el puerto `5060`. Esto es por diseño — el `ocelot.dev.json` lo rutea a `host.docker.internal:5060`.

**Cómo arrancarlo:**

```bash
# Obtener la llave PII del compose.yaml
export OKLA_PII_ENCRYPTION_KEY=CMx0ZJgjwLb3GdHw6laG0ICy09Zu9nKcNUtdzRJNfSQ=

# Arrancar el servicio
cd backend/ChatbotService
dotnet watch run --project ChatbotService.Api/ChatbotService.Api.csproj

# Verificar
curl http://localhost:5060/health
# → {"status":"Healthy"}
```

**Por qué falla silenciosamente si falta la variable:**  
El `try/catch` en `Program.cs` captura la `InvalidOperationException` y la escribe solo en Seq (sin sink de Console). Síntoma: proceso termina con exit code 0, sin output.

---

### 5. Auditoria de Agentes IA

Se usa `monitor_prompt6.py` para auditar los 10 agentes IA via OpenClaw + Chrome.

**Último resultado: 8/8 PASS** (Marzo 26, 2026)

**Cómo correr la auditoria:**

```bash
# Activar venv primero
source .venv/bin/activate

# Opción A: Auditoría local completa (recomendado — sin latencia de tunnel)
python3 .prompts/monitor_prompt6.py --audit-only --url http://localhost:3000

# Opción B: Con tunnel (para testing desde afuera)
# 1. Obtener URL del tunnel activo
TUNNEL_URL=$(docker compose logs cloudflared 2>&1 | grep -o 'https://[a-z-]*\.trycloudflare\.com' | tail -1)
echo "Tunnel: $TUNNEL_URL"

# 2. Iniciar frontend en modo BFF (NEXT_PUBLIC_API_URL vacío)
NEXT_PUBLIC_API_URL= pnpm --prefix frontend/web-next dev &

# 3. Correr la auditoria
python3 .prompts/monitor_prompt6.py --audit-only --url "$TUNNEL_URL"
```

**Reportes guardados en:** `audit-reports/`

---

## Flujo completo — Desde código hasta auditoria pasada

```
1. docker compose up -d                          # Infra (2.4 GB)
2. docker compose --profile core --profile vehicles --profile ai up -d
3. OKLA_PII_ENCRYPTION_KEY=... dotnet watch run  # ChatbotService host:5060
4. pnpm dev (frontend/web-next)                  # Frontend host:3000
5. [Opcional] docker compose --profile tunnel up -d cloudflared
6. source .venv/bin/activate
7. python3 .prompts/monitor_prompt6.py --audit-only
```

---

## Servicios solo disponibles en producción (sin imagen local)

Los siguientes servicios están en `ocelot.dev.json` como `host.docker.internal:XXXX` pero NO tienen imagen local disponible. En las auditorias aparecen como N/A:

| Servicio           | Puerto host | Estado                   |
| ------------------ | ----------- | ------------------------ |
| AzulPaymentService | 5001        | No disponible localmente |
| CRMService         | 5062        | No disponible localmente |
| ReportsService     | 15110       | No disponible localmente |
| AuditService       | 15112       | No disponible localmente |
| StaffService       | 15200       | No disponible localmente |

---

## Variables de entorno clave para ambiente local

```bash
# Para el backend (añadir al .env o export antes de dotnet run)
OKLA_PII_ENCRYPTION_KEY=CMx0ZJgjwLb3GdHw6laG0ICy09Zu9nKcNUtdzRJNfSQ=
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=localhost;Port=5433;Database=...;Username=postgres;Password=password

# Para el frontend (override de .env.local)
NEXT_PUBLIC_API_URL=              # Vacío = modo BFF (tunnel/externo)
NEXT_PUBLIC_API_URL=http://localhost:18443  # Directo (solo localhost)
INTERNAL_API_URL=http://localhost:18443    # SSR → Gateway
```

---

## Troubleshooting

| Problema                             | Causa probable                               | Solución                                             |
| ------------------------------------ | -------------------------------------------- | ---------------------------------------------------- |
| Service exits with code 0, no output | `OKLA_PII_ENCRYPTION_KEY` faltante           | `export OKLA_PII_ENCRYPTION_KEY=...`                 |
| CORS en tunnel/externo               | `NEXT_PUBLIC_API_URL=http://localhost:18443` | Iniciar con `NEXT_PUBLIC_API_URL= pnpm dev`          |
| 530 en tunnel                        | Docker Desktop no corre                      | `open -a "Docker Desktop"`                           |
| SearchAgent no filtra NLP            | AWS Bedrock IAM no autorizado                | Agregar `bedrock:InvokeModel` al user `okla-backend` |
| Container unhealthy — 127.0.0.1:5432 | Imagen vieja con appsettings hardcoded       | `docker compose up -d --build <servicio>`            |
| `dotnet watch` no recarga            | Proyecto falta archivo en watch              | Agregar `<Watch Include="..."/>` al .csproj          |
