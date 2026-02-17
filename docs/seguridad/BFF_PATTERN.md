# 🌐 BFF Pattern — Backend for Frontend

**Fecha de implementación:** Febrero 2026
**Proyecto:** OKLA (CarDealer Microservices)

---

## ¿Qué es el BFF Pattern?

**Backend for Frontend (BFF)** es un patrón de arquitectura donde el frontend tiene un backend dedicado que actúa como intermediario entre el navegador y los microservicios.

En OKLA, el servidor Next.js actúa como BFF: **el API Gateway (Ocelot) no tiene IP pública**. Todo el tráfico API fluye a través de Next.js.

---

## Arquitectura

### Antes (Gateway público)

```
┌──────────┐     HTTPS      ┌──────────┐
│ Browser  │ ──────────────▶ │ Gateway  │  ← IP pública (api.okla.com.do)
│          │ ◀────────────── │ (Ocelot) │  ← Expuesto al internet
└──────────┘                 └──────────┘
                                  │
                         ┌────────┼────────┐
                         ▼        ▼        ▼
                    AuthSvc  UserSvc  VehicleSvc
```

**Problemas:**

- Atacantes podían probar endpoints directamente en `api.okla.com.do`
- La estructura de la API era descubrible vía Network tab
- Surface area de ataque más amplio

### Después (BFF — Gateway interno)

```
┌──────────┐     HTTPS      ┌──────────────┐    HTTP interno    ┌──────────┐
│ Browser  │ ──────────────▶ │  Next.js     │ ─────────────────▶ │ Gateway  │
│          │ ◀────────────── │  (BFF)       │ ◀───────────────── │ (8080)   │
└──────────┘                 │ okla.com.do  │                    │ ClusterIP│
                             └──────────────┘                    └──────────┘
                                                                      │
                                                             ┌────────┼────────┐
                                                             ▼        ▼        ▼
                                                        AuthSvc  UserSvc  VehicleSvc
```

**Beneficios:**

- Gateway **NO tiene IP pública** — solo accesible dentro del cluster K8s
- `api.okla.com.do` **ya no existe** como subdominio
- El browser solo ve `okla.com.do/api/*` → Next.js proxea internamente

---

## Implementación

### next.config.ts — Rewrites

```typescript
async rewrites() {
  return {
    afterFiles: [
      {
        source: '/api/:path*',
        destination: `${process.env.INTERNAL_API_URL || 'http://gateway:8080'}/api/:path*`,
      },
    ],
  };
}
```

### Variables de entorno

```env
# Producción
NEXT_PUBLIC_API_URL=                          # Vacío — browser usa URLs relativas (/api/*)
INTERNAL_API_URL=http://gateway:8080          # Solo server-side — NO es NEXT_PUBLIC_

# Desarrollo local
NEXT_PUBLIC_API_URL=http://localhost:18443     # Gateway local
```

### src/lib/api-url.ts

```typescript
// Server-side: usa Gateway por red interna
export function getInternalApiUrl(): string {
  return (
    process.env.INTERNAL_API_URL ||
    process.env.NEXT_PUBLIC_API_URL ||
    "http://localhost:18443"
  );
}

// Client-side: usa URL relativa (Next.js rewrites proxea)
export function getClientApiUrl(): string {
  return process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:18443";
}

// Auto-detect: server → internal, client → relative
export function getApiBaseUrl(): string {
  if (typeof window === "undefined") return getInternalApiUrl();
  return getClientApiUrl();
}
```

### Kubernetes — Sin Ingress para Gateway

```yaml
# Gateway es ClusterIP (no LoadBalancer, no Ingress)
apiVersion: v1
kind: Service
metadata:
  name: gateway
  namespace: okla
spec:
  type: ClusterIP # ← Solo accesible dentro del cluster
  ports:
    - port: 8080
  selector:
    app: gateway
```

### NetworkPolicy — Solo frontend-web puede hablar con Gateway

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: gateway-network-policy
  namespace: okla
spec:
  podSelector:
    matchLabels:
      app: gateway
  ingress:
    - from:
        - podSelector:
            matchLabels:
              app: frontend-web # ← SOLO frontend-web
      ports:
        - port: 8080
```

---

## Flujo de una petición en producción

```
1. Browser: GET https://okla.com.do/api/vehicles
2. DNS: okla.com.do → 146.190.199.0 (Load Balancer)
3. Ingress: route to frontend-web:8080
4. Next.js: rewrite /api/vehicles → http://gateway:8080/api/vehicles
5. Gateway: route to vehiclessaleservice:8080
6. VehiclesSaleService: query DB, return JSON
7. Response bubbles back: Service → Gateway → Next.js → Browser
```

---

## Relación con Server Actions

El BFF Pattern hace que el **Gateway sea invisible desde internet**. Los Server Actions van un paso más allá: hacen que las **peticiones al Gateway también sean invisibles en el browser**.

| Sin BFF ni Actions                          | Con BFF                                 | Con BFF + Server Actions         |
| ------------------------------------------- | --------------------------------------- | -------------------------------- |
| Browser ve `api.okla.com.do/api/auth/login` | Browser ve `okla.com.do/api/auth/login` | Browser ve `POST /login` (opaco) |
| Gateway tiene IP pública                    | Gateway es interno                      | Gateway es interno               |
| Endpoint descubrible                        | Endpoint visible en Network             | Endpoint invisible               |

---

_Documentación de seguridad — OKLA — Febrero 2026_
