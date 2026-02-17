# 🧪 Scripts de Testing - OKLA

Scripts automatizados para testing y validación de sprints.

---

## 📋 Scripts Disponibles

### `test-sprint.sh` - Testing Completo de Sprint

Script automatizado que verifica **TODOS** los componentes de un sprint antes de marcarlo como completado.

#### 🎯 Qué Verifica

1. **Backend Testing**

   - ✅ Estructura del microservicio existe
   - ✅ Dockerfile presente
   - ✅ Health Check configurado
   - ✅ Clean Architecture correcta

2. **Docker Build**

   - ✅ Imagen Docker compila sin errores
   - ✅ Dependencias resueltas

3. **Docker Compose**

   - ✅ Servicio configurado en compose.yaml
   - ✅ Servicio inicia correctamente
   - ✅ Logs sin errores críticos

4. **Frontend Integration**

   - ✅ Rutas agregadas en App.tsx
   - ✅ Links en Navbar.tsx
   - ✅ Componentes usan MainLayout
   - ✅ ProtectedRoute aplicado (si aplica)

5. **Gateway Configuration**

   - ✅ Rutas en ocelot.prod.json
   - ✅ Puerto 8080 (NO 80)
   - ✅ DownstreamHostAndPorts configurado

6. **Kubernetes Manifests**
   - ✅ Deployment en k8s/deployments.yaml
   - ✅ Service en k8s/services.yaml
   - ✅ containerPort 8080

---

## 🚀 Uso

### Sintaxis

```bash
./scripts/test-sprint.sh [nombre-del-servicio]
```

### Ejemplos

```bash
# Testing de AlertService
./scripts/test-sprint.sh alertservice

# Testing de ComparisonService
./scripts/test-sprint.sh comparisonservice

# Testing de MaintenanceService
./scripts/test-sprint.sh maintenanceservice
```

**Nota:** El script es case-insensitive y acepta el nombre con o sin sufijo "Service".

---

## 📊 Output del Script

### Éxito ✅

```
╔═══════════════════════════════════════════════════════════════╗
║        OKLA Sprint Testing - alertservice                     ║
╚═══════════════════════════════════════════════════════════════╝

[1/6] Backend Testing...
✓ Servicio encontrado: backend/AlertService
✓ Dockerfile encontrado
✓ Backend structure OK

[2/6] Docker Build Testing...
✓ Docker build exitoso

[3/6] Docker Compose Testing...
✓ Servicio en compose.yaml
✓ Servicio iniciado
✓ Servicio corriendo

[4/6] Frontend Integration Testing...
✓ Rutas encontradas en App.tsx
✓ Links encontrados en Navbar.tsx
✓ AlertsPage.tsx usa MainLayout

[5/6] Gateway Configuration Testing...
✓ Rutas encontradas en ocelot.prod.json
✓ Puerto 8080 configurado correctamente

[6/6] Kubernetes Manifests Testing...
✓ Deployment encontrado en k8s/deployments.yaml
✓ containerPort 8080 configurado
✓ Service encontrado en k8s/services.yaml

╔═══════════════════════════════════════════════════════════════╗
║                    RESUMEN DEL TESTING                        ║
╚═══════════════════════════════════════════════════════════════╝

✓ Checks completados

SIGUIENTE PASOS MANUALES:
1. Probar endpoints manualmente:
   curl http://localhost:18443/api/alerts/health

2. Si hay frontend, verificar en navegador:
   http://localhost:5173/alerts

3. Verificar logs sin errores:
   docker-compose logs -f alertservice

4. Si todo OK, marcar sprint como completado ✅
```

### Warnings ⚠️

El script mostrará warnings amarillos para cosas opcionales o no configuradas:

```
⚠ Warning: Health Check podría no estar configurado
⚠ No se encontraron rutas relacionadas en App.tsx
⚠ Rutas no encontradas en Gateway
```

### Errores ❌

Si algo crítico falla, el script se detiene y muestra el error:

```
✗ Docker build falló
✗ Servicio no está corriendo
✗ Puerto 80 encontrado - DEBE ser 8080 en producción
```

---

## 🔧 Configuración

### Variables de Entorno

```bash
# URL del API Gateway (default: http://localhost:18443)
export API_URL=https://api.okla.com.do
```

### Requisitos

- Docker instalado
- Docker Compose instalado
- Bash shell (macOS/Linux)

---

## 📝 Checklist Manual Post-Script

Después de que el script pase ✅, verificar manualmente:

### Backend

```bash
# 1. Health Check
curl http://localhost:PORT/health

# 2. Endpoint principal (ejemplo: GET)
curl http://localhost:PORT/api/resource

# 3. Endpoint con auth (ejemplo: POST)
curl -X POST http://localhost:PORT/api/resource \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"field": "value"}'

# 4. Logs sin errores
docker-compose logs -f servicename | grep -i error
```

### Frontend

```bash
# 1. Iniciar dev server
cd frontend/web
npm run dev

# 2. Abrir en navegador
open http://localhost:5173/ruta-correspondiente

# 3. Verificar:
# - Página se renderiza sin errores de consola
# - API calls funcionan (Network tab)
# - Botones/formularios funcionan
# - Responsive en mobile/tablet/desktop
```

### Integración E2E

```bash
# 1. Levantar stack completo
docker-compose up -d

# 2. Verificar todos los servicios
docker-compose ps

# 3. Testing de flujo completo:
# - Usuario se registra → Login → Usa feature → Logout
```

---

## 🎯 Cuándo Usar Este Script

### ✅ USAR en estos casos:

- Antes de marcar un sprint como completado
- Antes de hacer merge a `development`
- Antes de deploy a producción
- Después de cambios mayores en un servicio
- Al revisar PRs de otros desarrolladores

### ❌ NO USAR en estos casos:

- Cambios menores en un solo archivo
- Refactoring sin cambios funcionales
- Updates de documentación solamente

---

## 🐛 Troubleshooting

### Error: "Servicio no encontrado"

```bash
# Verifica que el servicio existe en backend/
ls -la backend/ | grep -i servicio

# El script busca patrones como:
# - AlertService/
# - alertservice/
# - Alert/
```

### Error: "Docker build falló"

```bash
# Ver logs completos del build
cd backend/MiServicio/MiServicio.Api
docker build -t test:latest .

# Revisar errores de compilación
```

### Error: "Servicio no está corriendo"

```bash
# Ver logs del contenedor
docker-compose logs servicename

# Posibles causas:
# - Puerto ya en uso
# - Dependencias (postgres, redis) no disponibles
# - Error de configuración en appsettings.json
```

### Warning: "Rutas no encontradas en App.tsx"

```bash
# Verificar manualmente si hay UI:
ls -la frontend/web/src/pages/ | grep -i servicio

# Si hay UI, agregar rutas:
# 1. Importar en App.tsx
# 2. Agregar <Route path="/ruta" element={<Component />} />
```

---

## 🔄 Integración con CI/CD

Este script puede integrarse en GitHub Actions:

```yaml
# .github/workflows/sprint-validation.yml
name: Sprint Validation

on:
  pull_request:
    branches: [development, main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Run Sprint Testing
        run: |
          chmod +x scripts/test-sprint.sh
          ./scripts/test-sprint.sh ${{ github.event.pull_request.title }}
```

---

## 📚 Referencias

- [copilot-instructions.md](../.github/copilot-instructions.md) - Workflow completo
- [SPRINT_1_NAVIGATION_INTEGRATION.md](../docs/SPRINT_1_NAVIGATION_INTEGRATION.md) - Ejemplo Sprint 1

---

**Última actualización:** Enero 8, 2026
