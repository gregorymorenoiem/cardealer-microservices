# 🔧 Fix Summary: Login → 401 Error en `/api/auth/me`

**Fecha:** Febrero 19, 2026  
**Problema:** Usuario logueado exitosamente pero obtiene 401 cuando intenta acceder a `/api/auth/me`  
**Causa Raíz:** Cookies con `SameSite=Strict` y `Path=/api/auth` no se enviaban en requests cross-site

---

## 🔴 Síntoma

```
1. POST /api/auth/login → 200 ✅ (retorna access token + refresh token)
2. GET /api/auth/me → 401 ❌ (Unauthorized)
3. POST /api/auth/refresh-token → 400 ❌ (Bad Request - RefreshToken field is required)
```

**Console errors:**

```
GET https://okla.com.do/api/auth/me 401 (Unauthorized)
POST https://okla.com.do/api/auth/refresh-token 400 (Bad Request)
```

---

## 🔍 Análisis

### Cookies Originales (PROBLEMA)

```csharp
// AuthCookieHelper.cs - ANTES
var accessCookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = isProduction,
    SameSite = SameSiteMode.Strict,  // ❌ PROBLEMA 1
    Path = "/",
    Expires = expiresAt,
    IsEssential = true
};

var refreshCookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = isProduction,
    SameSite = SameSiteMode.Strict,  // ❌ PROBLEMA 1
    Path = "/api/auth",              // ❌ PROBLEMA 2 - Solo se envía a /api/auth/*
    Expires = DateTimeOffset.UtcNow.AddDays(7),
    IsEssential = true
};
```

### Por qué falla

| Problema                   | Impacto                                   | Síntoma                                           |
| -------------------------- | ----------------------------------------- | ------------------------------------------------- |
| `SameSite=Strict`          | Cookie NO se envía en requests cross-site | Browser → Gateway → AuthService; cookie se pierde |
| `Path=/api/auth` (refresh) | Cookie NO se envía a `/api/auth/me`       | GET /me → sin refresh token en cookie             |

**Flujo del error:**

```
Browser: POST login
    ↓
AuthService: Set-Cookie okla_refresh_token; Path=/api/auth; SameSite=Strict
    ↓
Browser: GET /api/auth/me
    ↓
cookie okla_refresh_token (Path=/api/auth) ¿se envía a /me?
    NO — porque /me != /api/auth/* exacto
    ↓
AuthService: 401 Unauthorized (sin token)
```

---

## ✅ Solución Aplicada

### Cambios en `AuthService.Api/Helpers/AuthCookieHelper.cs`

```csharp
// DESPUÉS
var accessCookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = isProduction,
    SameSite = SameSiteMode.Lax,     // ✅ FIX: Lax permite cookies en cross-site
    Path = "/",
    Expires = expiresAt,
    IsEssential = true
};

var refreshCookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = isProduction,
    SameSite = SameSiteMode.Lax,     // ✅ FIX: Lax
    Path = "/",                      // ✅ FIX: "/" no "/api/auth"
    Expires = DateTimeOffset.UtcNow.AddDays(7),
    IsEssential = true
};
```

### Cambios Equivalentes en `AuthService.Api/Controllers/AuthController.cs`

(Mismo fix en métodos privados `SetAuthCookies()` y `ClearAuthCookies()`)

---

## 📊 Comparación: Strict vs Lax

| Propiedad              | Strict                   | Lax                  |
| ---------------------- | ------------------------ | -------------------- |
| **Misma URL**          | ✅ Enviada               | ✅ Enviada           |
| **Cross-site (GET)**   | ❌ NO                    | ✅ SÍ                |
| **Cross-site (POST)**  | ❌ NO                    | ❌ NO (safe)         |
| **Cross-site (fetch)** | ❌ NO                    | ❌ NO                |
| **Uso recomendado**    | Máxima seguridad (OAuth) | Balance seguridad/UX |

**Para OKLA:** Lax es apropiado porque:

- El frontend (Next.js) hace requests legítimos a la API
- POST/PUT/DELETE aún están protegidos (SameSite=Lax bloquea en top-level POST)
- GET requests (como `/me`) pueden incluir la cookie

---

## 🚀 Despliegue

```bash
# 1. Cambios ya committed y pusheados
git log -1 --oneline
# fix(auth): change cookies from SameSite=Strict to Lax...

# 2. CI/CD automáticamente:
# - Rebuild Docker image de AuthService
# - Push a GHCR: ghcr.io/gregorymorenoiem/authservice:latest
# - Deploy a K8s con nueva imagen

# 3. Verificar rollout:
kubectl rollout status deployment/authservice -n okla --timeout=60s

# 4. Test:
# POST /api/auth/login → Get cookies
# GET  /api/auth/me    → ✅ 200 (con cookies)
# POST /api/auth/refresh-token → ✅ 200
```

---

## 🧪 Testing Manual

### Test 1: Cookies guardadas correctamente

```bash
# Login y guardar cookies
curl -s -c /tmp/cookies.txt -X POST https://okla.com.do/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@okla.local","password":"Admin123!@#"}'

# Ver cookies
cat /tmp/cookies.txt | grep okla_

# Esperado:
# okla_access_token   Path=/   SameSite=Lax   HttpOnly
# okla_refresh_token  Path=/   SameSite=Lax   HttpOnly
```

### Test 2: GET /me con cookies

```bash
curl -s -b /tmp/cookies.txt -X GET https://okla.com.do/api/auth/me
# Esperado: HTTP 200 ✅
```

### Test 3: Refresh token

```bash
curl -s -b /tmp/cookies.txt -X POST https://okla.com.do/api/auth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{}'
# Esperado: HTTP 200 con nuevo token ✅
```

---

## 📝 Notas de Seguridad

- ✅ **Cookies aún HttpOnly** → XSS attacks bloqueados (JS no puede acceder)
- ✅ **Secure en producción** → HTTPS only
- ✅ **SameSite=Lax** → CSRF attacks bloqueados (POST/PUT/DELETE requieren same-site)
- ✅ **Path=/** → Todas las rutas reciben la cookie (por diseño)

---

## 📋 Archivos Modificados

| Archivo                                                             | Cambio                                  |
| ------------------------------------------------------------------- | --------------------------------------- |
| `backend/AuthService/AuthService.Api/Helpers/AuthCookieHelper.cs`   | SameSite: Strict→Lax, Path: /api/auth→/ |
| `backend/AuthService/AuthService.Api/Controllers/AuthController.cs` | Mismo cambio en métodos privados        |

---

## ✨ Estado

- ✅ Fix código completado
- ✅ Cambios commiteados
- ⏳ CI/CD building...
- ⏳ Despliegue a DOKS...
- 📋 Próximas pruebas: Validar login flujo completo desde navegador
