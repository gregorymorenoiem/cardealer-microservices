# 🚀 Sprint 11 - Comandos Rápidos

Referencia rápida de comandos para ejecutar los tests del Sprint 11.

---

## 📱 Frontend Tests

### Unit Tests (Vitest)

```bash
# Ejecutar todos los tests
cd frontend/web
npm test

# Ejecutar tests en modo watch
npm test -- --watch

# Ejecutar tests con cobertura
npm run test:coverage

# Ejecutar tests en modo UI (interfaz visual)
npm run test:ui

# Ejecutar un archivo específico
npm test BrowsePage

# Ejecutar tests y salir (CI mode)
npm test -- --run
```

### E2E Tests (Playwright)

```bash
# Ejecutar todos los E2E tests
cd frontend/web
npm run test:e2e

# Solo Chromium (más rápido)
npm run test:e2e:chromium

# Modo UI interactivo
npm run test:e2e:ui

# Ver el navegador durante la ejecución
npm run test:e2e:headed

# Ejecutar un archivo específico
npx playwright test auth.spec.ts

# Ejecutar un test específico
npx playwright test -g "should login"

# Ver reporte HTML
npm run test:e2e:report

# Debug mode (pausa ejecución)
npx playwright test --debug

# Regenerar screenshots/snapshots
npx playwright test --update-snapshots
```

---

## 🔧 Backend Tests

### .NET Tests

```bash
# Ejecutar todos los tests de un servicio
cd backend/AuthService
dotnet test

# Con verbosidad detallada
dotnet test --verbosity detailed

# Solo tests que contienen una palabra
dotnet test --filter "Login"

# Con cobertura (si está configurado)
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar tests en todos los microservicios
cd backend
for dir in */; do
  if [ -d "$dir" ]; then
    cd "$dir"
    if ls *.Tests/*.csproj 1> /dev/null 2>&1; then
      echo "Testing $dir..."
      dotnet test
    fi
    cd ..
  fi
done
```

---

## 🐳 Integration Tests (Docker)

```bash
# Levantar servicios para integration tests
docker-compose up -d redis rabbitmq
docker-compose up -d authservice-db authservice
docker-compose up -d errorservice-db errorservice

# Verificar health de servicios
curl http://localhost:15085/health  # AuthService
curl http://localhost:15083/health  # ErrorService

# Ver logs
docker logs -f authservice

# Bajar servicios
docker-compose down
```

---

## 🔍 Linting & Type Checking

```bash
cd frontend/web

# ESLint
npm run lint

# ESLint con auto-fix
npm run lint -- --fix

# TypeScript type checking
npx tsc --noEmit

# Prettier (si está configurado)
npx prettier --write src/**/*.{ts,tsx}
```

---

## 🔁 CI/CD Local

### Simular CI Workflow

```bash
# Simular PR checks
cd frontend/web
npm ci  # Install exactas versiones
npm run lint
npx tsc --noEmit
npm test -- --run

# Backend build check
cd ../../backend/AuthService
dotnet restore
dotnet build --no-restore

# Docker build check
cd ../..
docker-compose build authservice
```

---

## 📊 Coverage Reports

### Frontend Coverage

```bash
cd frontend/web
npm run test:coverage

# Ver reporte HTML
open coverage/index.html  # macOS
xdg-open coverage/index.html  # Linux
start coverage/index.html  # Windows
```

### Backend Coverage

```bash
cd backend/AuthService
dotnet test --collect:"XPlat Code Coverage"

# Instalar reportgenerator (una vez)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generar reporte HTML
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# Ver reporte
open coveragereport/index.html
```

---

## 🔧 Troubleshooting

### Limpiar Caches

```bash
# Frontend
cd frontend/web
rm -rf node_modules
npm cache clean --force
npm install

# Backend
cd backend/AuthService
dotnet clean
dotnet restore

# Docker
docker-compose down
docker system prune -f
docker-compose up -d
```

### Reinstalar Playwright

```bash
cd frontend/web
npm uninstall @playwright/test
npm install -D @playwright/test
npx playwright install chromium
```

### Fix Port Conflicts

```bash
# Ver qué está usando puerto 5173 (Vite)
lsof -i :5173

# Matar proceso
kill -9 <PID>

# Ver todos los puertos de microservicios
lsof -i :15085,15083,15084,15090
```

---

## 🎯 Verificación Rápida (Script Todo-en-Uno)

```bash
# Ejecutar script de verificación completa
./scripts/verify-sprint11.sh
```

Este script ejecuta:
- ✅ Instalación de dependencias
- ✅ Tests unitarios
- ✅ Lint
- ✅ Type checking
- ✅ Verificación de Playwright

---

## 📁 Archivos Importantes

| Archivo | Descripción |
|---------|-------------|
| `frontend/web/vitest.config.ts` | Configuración de Vitest |
| `frontend/web/playwright.config.ts` | Configuración de Playwright |
| `frontend/web/src/test/setup.ts` | Setup global de tests unitarios |
| `frontend/web/e2e/*.spec.ts` | Tests E2E |
| `.github/workflows/test.yml` | CI/CD principal |
| `.github/workflows/pr-checks.yml` | Quick checks para PRs |

---

## 🆘 Ayuda

### Documentación
- [Vitest Docs](https://vitest.dev)
- [React Testing Library](https://testing-library.com/react)
- [Playwright Docs](https://playwright.dev)
- [xUnit Docs](https://xunit.net)

### Tests Fallando?

1. **Check logs:** `npm test -- --reporter=verbose`
2. **Update snapshots:** `npx playwright test --update-snapshots`
3. **Clear cache:** `npm cache clean --force && rm -rf node_modules && npm install`
4. **Check mocks:** Ver `src/test/setup.ts`
5. **Debug test:** `npx playwright test --debug auth.spec.ts`

---

_Happy Testing! 🧪✨_
