# End-to-End Tests - Playwright

Este directorio contiene los tests E2E (End-to-End) de la aplicación CarDealer usando Playwright.

## 📁 Estructura

```
e2e/
├── auth.spec.ts              # Tests de autenticación (login, register, logout)
├── browse.spec.ts            # Tests de navegación de vehículos
├── vehicle-detail.spec.ts    # Tests de detalles de vehículo
└── search-filter.spec.ts     # Tests de búsqueda y filtros
```

## 🚀 Ejecutar Tests

### Todos los tests en todos los navegadores
```bash
npm run test:e2e
```

### Solo Chromium (más rápido)
```bash
npm run test:e2e:chromium
```

### Modo UI (interactivo)
```bash
npm run test:e2e:ui
```

### Modo headed (ver el navegador)
```bash
npm run test:e2e:headed
```

### Ver reporte HTML
```bash
npm run test:e2e:report
```

## 🎯 Cobertura de Tests

### auth.spec.ts (5 tests)
- ✅ Navegación a página de login
- ✅ Validación de formulario vacío
- ✅ Login con credenciales válidas
- ✅ Navegación a registro
- ✅ Logout exitoso

### browse.spec.ts (6 tests)
- ✅ Carga de página con grid de vehículos
- ✅ Toggle entre vista grid/list
- ✅ Filtrado por rango de precio
- ✅ Navegación a detalle de vehículo
- ✅ Búsqueda de vehículos
- ✅ Paginación de resultados

### vehicle-detail.spec.ts (7 tests)
- ✅ Visualización de detalles del vehículo
- ✅ Visualización de especificaciones
- ✅ Botón de contacto al vendedor
- ✅ Agregar a favoritos
- ✅ Navegación en galería de imágenes
- ✅ Vehículos similares
- ✅ Compartir vehículo

### search-filter.spec.ts (8 tests)
- ✅ Búsqueda básica
- ✅ Filtro por tipo de vehículo
- ✅ Filtro por marca/brand
- ✅ Filtro por rango de año
- ✅ Filtro por kilometraje
- ✅ Limpiar todos los filtros
- ✅ Guardar búsqueda
- ✅ Ordenar resultados

**Total: 26 tests E2E** cubriendo los flujos críticos de usuario.

## 🔧 Configuración

La configuración se encuentra en `playwright.config.ts`:

- **Base URL**: http://localhost:5173
- **Navegadores**: Chromium, Firefox, WebKit, Mobile Chrome, Mobile Safari
- **Timeouts**: 30s por test
- **Traces**: Capturados en primer reintento
- **Screenshots**: Solo en fallos
- **Videos**: Retenidos en fallos
- **Dev Server**: Inicia automáticamente con `npm run dev`

## 📝 Escribir Nuevos Tests

### Ejemplo básico

```typescript
import { test, expect } from '@playwright/test';

test.describe('My Feature', () => {
  test('should do something', async ({ page }) => {
    await page.goto('/');
    await page.click('button');
    await expect(page.locator('h1')).toContainText('Success');
  });
});
```

### Mejores prácticas

1. **Usar data-testid** para selectores estables:
   ```typescript
   await page.click('[data-testid="login-button"]');
   ```

2. **Esperar por elementos**:
   ```typescript
   await page.waitForSelector('[data-testid="vehicle-card"]');
   ```

3. **Verificar navegación**:
   ```typescript
   await expect(page).toHaveURL(/.*\/vehicles\/\d+/);
   ```

4. **Usar fixtures para setup**:
   ```typescript
   test.beforeEach(async ({ page }) => {
     await page.goto('/browse');
   });
   ```

5. **Tests independientes**: Cada test debe poder ejecutarse solo.

## 🐛 Debugging

### Debug un test específico
```bash
npx playwright test auth.spec.ts --debug
```

### Ver trace viewer
```bash
npx playwright show-trace trace.zip
```

### Pausar ejecución
```typescript
await page.pause(); // Pausa y abre inspector
```

## 📊 CI/CD

Los tests E2E se ejecutan automáticamente en:
- Pull Requests
- Push a main/develop
- Solo en Chromium en CI (más rápido)

Ver `.github/workflows/test.yml` para más detalles.

## 🔍 Tips

- **Tests lentos**: Usar `--project=chromium` para ejecutar solo en un navegador
- **Flaky tests**: Aumentar timeouts o agregar `waitForLoadState('networkidle')`
- **Debug en CI**: Los traces/screenshots se suben como artifacts
- **Mock APIs**: Usar `page.route()` para interceptar requests

## 📚 Recursos

- [Playwright Docs](https://playwright.dev)
- [Best Practices](https://playwright.dev/docs/best-practices)
- [API Reference](https://playwright.dev/docs/api/class-playwright)
