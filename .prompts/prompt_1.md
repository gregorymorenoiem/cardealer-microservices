Todas las tareas que hagas tienes que confirmar en este archivo que la hiciste. Por lo cual tu plan te tareas realizads tienes que escribirlo aqui, para saber que has hecho y que no haz hecho.

---

## ✅ Tareas Realizadas (Sesión actual)

### 1. ✅ Llenar espacios publicitarios en vista de vehículos

- **Archivo modificado**: `frontend/web-next/src/app/api/advertising/rotation/route.ts`
- Se implementó la transformación de campos del backend (bodyStyle→bodyType, thumbnailUrl) en el BFF route
- Se conectó con vehículos reales de la API para llenar los spots publicitarios

### 2. ✅ Llenar espacios "⭐ Vehículos Destacados" y "💎 Vehículos Premium" en homepage

- El BFF route de rotación ahora devuelve vehículos reales para los 4 tipos de spot: Featured, Premium, Sidebar, Banner
- Cada spot retorna 3 vehículos con datos reales

### 3. ✅ Corregir fotos que no se ven en homepage

- **Archivo creado**: `frontend/web-next/src/lib/vehicle-image-fallbacks.ts` — Sistema de fallback con imágenes de Unsplash por marca/tipo
- **Archivo modificado**: `frontend/web-next/src/components/ui/vehicle-card.tsx` — `effectiveImageUrl` con fallback si S3 falla
- **Archivo modificado**: `frontend/web-next/src/components/homepage/vehicle-type-section.tsx` — Error handlers de imagen con fallback
- **Archivo modificado**: `frontend/web-next/src/components/vehicle-detail/vehicle-gallery.tsx` — Tracking de URLs fallidas + fallback por vehículo

### 4. ✅ Eliminar archivo `.prompts/cleanup_references_audit.md`

- Archivo eliminado correctamente

### 5. ✅ Prueba QA completa de la plataforma en producción

- **24/25 páginas públicas** retornan HTTP 200/307 ✅
- **Login de los 3 tipos de usuario** (Admin, Buyer, Dealer) funciona correctamente ✅
- **Endpoints autenticados**: profile, favorites, notifications, users, health — todos pasan ✅
- **Búsqueda y filtros de vehículos**: funcionan correctamente ✅
- **Endpoints de publicidad**: todos pasan ✅
- Único endpoint con 404: `/api/vehicles/makes` (posiblemente no es un endpoint real)

### 6. ✅ Probar algoritmo de publicidad

- 4 tipos de spot (Featured, Premium, Sidebar, Banner) retornan 3 items cada uno ✅
- Todos los campos presentes en las respuestas ✅
- Tiempos de respuesta: ~260ms ✅
- Algoritmo: WeightedRandom con seed temporal (cambia cada 5 minutos)

### 7. ✅ Confirmar tipo de cuenta de gmoreno@okla.com.do

- **Resultado**: Es cuenta **Seller** (account_type: 6, roles: User + Seller, userIntent: sell, sin dealerId)

### 8. ✅ Auditoría de acceso a vehículos por tipo de usuario

- **5 tipos de usuario probados**: Anónimo, Buyer, Dealer, Seller, Admin
- Todos pueden ver: detalle de vehículos, API de vehículos, rotación publicitaria, listados ✅

### 9. ✅ Habilitar algoritmo de rotación real

- **Archivo modificado**: `frontend/web-next/src/app/api/advertising/rotation/route.ts`
- Implementado algoritmo **WeightedRandom** con PRNG seeded (mulberry32)
- Quality scores: fotos (35%) + recencia (40%) + competitividad precio (25%)
- Pool de 3x vehículos para selección ponderada
- Seed basado en tiempo que cambia cada 5 minutos
- Respuesta incluye `algorithm: 'WeightedRandom'` y `rotationSeed` en metadata

### 10. ✅ Análisis de mercado para sistema de facturación/contabilidad

- **Archivo creado**: `docs/reportes/MARKET_ANALYSIS_BILLING_SYSTEM.md`
- Análisis de competidores: Alegra ($29-$129/mes), Odoo, Facturero, herramienta gratuita DGII
- Precios propuestos OKLA: 40-70% por debajo de la competencia ($9.99-$39.99/mes)
- Requisitos legales: Ley 32-23, eCF, certificación PSFE de la DGII
- 16 módulos recomendados en 3 fases
- Costo de desarrollo: $35K-$55K, punto de equilibrio: 8-18 meses
- Recomendación: enfoque híbrido (integrar proveedor certificado primero, luego certificar OKLA)

### 11. ✅ Commit, push y monitoreo CI/CD

- **Commit `02112f40`**: fix: vehicle images fallback system + advertising rotation BFF + pre-existing build fixes
- **Commit `aa7b8047`**: feat: enable WeightedRandom rotation algorithm for advertising
- **Commit `b6a7784b`**: docs: market analysis for billing/accounting system
- **CI/CD**: ✅ Smart CI/CD — success | ✅ Deploy to production — success | ✅ Health Check — success

### 12. ✅ Correcciones de errores pre-existentes de build

- `okla-score/page.tsx`: Migrado de `@/hooks/use-toast` (no existía) a `sonner`
- `upload-queue-manager.ts`: Corregido `NavigatorWithConnection extends Navigator` (conflicto de tipos)
- Eliminado directorio duplicado `(main)/admin/planes/` (conflicto con `(admin)/admin/planes/`)

---

## 🆕 NUEVAS TAREAS — Asignadas por PM OKLA (6 marzo 2026)

### 📊 Análisis Estratégico Previo

El PM realizó un análisis completo de:

- **26 microservicios backend** con 304K líneas de C#
- **174 páginas frontend** con 170K líneas de TypeScript
- **278 rutas del Gateway**
- **47 deployments en K8s** (19 son "fantasmas" sin código)
- **Mercado dominicano**: OKLA tiene ventajas técnicas vs Autodom, SuperCarros, Corotos, pero necesita pulir UX y monetización

### Prioridades del negocio:

1. Estabilidad técnica → resolver deuda técnica
2. Herramientas de valor → calculadoras financieras para usuarios DR
3. SEO → capturar tráfico orgánico
4. Engagement → features de retención

---

### TAREA 13: Implementar "Calculadora de Financiamiento Vehicular"

**Estado:** ⬜ No iniciada  
**Prioridad:** 🔴 Alta  
**Por qué:** En RD, 60-70% de las compras de vehículos se financian. Esta herramienta gratuita captura tráfico SEO y genera confianza.

**Instrucciones:**

1. Crear nueva página: `frontend/web-next/src/app/(main)/herramientas/calculadora-financiamiento/page.tsx`
2. Crear layout para herramientas si no existe: `frontend/web-next/src/app/(main)/herramientas/layout.tsx`
3. La calculadora debe incluir:
   - **Precio del vehículo** (input numérico con formato RD$, usar separador de miles)
   - **Inicial** (monto en RD$ — mínimo 20% por regulación bancaria DR)
   - **Plazo** (12, 24, 36, 48, 60, 72 meses — con selector visual)
   - **Tasa de interés anual** (default 12% — promedio del mercado DR según SIB 2025, rango 8%-24%)
   - **Incluir seguro** (toggle — estimar ~3% del valor anual = valor×0.03/12 mensual)
   - **Resultado claro**: Cuota mensual, total a pagar, total de intereses, costo total del seguro
4. **Fórmula de amortización francesa:** `Cuota = P × [r(1+r)^n] / [(1+r)^n – 1]`
   - P = precio - inicial (monto financiado)
   - r = tasa anual / 12 (tasa mensual)
   - n = plazo en meses
5. Usar componentes shadcn/ui: Card, Input, Slider, Select, Switch, Label, Separator
6. Incluir tabla de amortización colapsable (cada mes: cuota, capital, interés, balance)
7. **SEO metadata:**
   ```typescript
   export const metadata: Metadata = {
     title: "Calculadora de Financiamiento Vehicular | OKLA",
     description:
       "Calcula tu cuota mensual para financiar tu vehículo en República Dominicana. Tasas actualizadas, seguros y tabla de amortización.",
     keywords: [
       "financiamiento vehicular",
       "calculadora cuotas",
       "préstamo vehículo RD",
       "financiar carro dominicana",
     ],
   };
   ```
8. Agregar JSON-LD de tipo `WebApplication`
9. Mobile-first, responsive, usar Tailwind
10. Agregar link a la calculadora desde el footer del sitio

**Criterio de aceptación:** Página funcional en `/herramientas/calculadora-financiamiento`, cálculos correctos verificados manualmente, responsive, SEO con metadata.

---

### TAREA 14: Implementar "Calculadora de Importación de Vehículos"

**Estado:** ⬜ No iniciada  
**Prioridad:** 🔴 Alta  
**Por qué:** Miles de dominicanos importan vehículos de EEUU. Esta herramienta captura tráfico de búsqueda alta ("cuánto cuesta importar un carro a RD").

**Instrucciones:**

1. Crear: `frontend/web-next/src/app/(main)/herramientas/calculadora-importacion/page.tsx`
2. Inputs:
   - **Valor FOB del vehículo** (en USD)
   - **Año del vehículo** (selector, últimos 10 años + anteriores)
   - **Tipo de combustible**: Gasolina, Diesel, Eléctrico, Híbrido
   - **Cilindrada** (cc): 1000, 1500, 2000, 2500, 3000, 3500, 4000+
   - **Tipo de vehículo**: Sedán, SUV, Pickup, Van, Coupé, etc.
   - **Puerto de origen**: Miami ($950 flete est.), New Jersey ($1,200), Houston ($1,100), otros ($1,500)
3. Cálculos basados en DGA (Dirección General de Aduanas):
   - **Flete marítimo** = según puerto seleccionado
   - **Seguro marítimo** = FOB × 1.5%
   - **Valor CIF** = FOB + Flete + Seguro
   - **Arancel**:
     - Eléctricos: 0%
     - Gasolina/Diesel: 20%
   - **Impuesto Selectivo al Consumo**:
     - Hasta 2000cc gasolina: 0%
     - 2001-3000cc gasolina: 30%
     - 3001cc+ gasolina: 51%
     - Hasta 2500cc diesel: 0%
     - 2501cc+ diesel: 51%
     - Eléctricos: 0%
     - Híbridos: 50% de la tasa correspondiente
   - **Base ISC** = CIF + Arancel
   - **ITBIS** = (CIF + Arancel + ISC) × 18%
   - **Primera placa** = RD$8,000 (estimado)
   - **Marbete** = RD$3,500 (estimado)
   - **Emisiones CO2** = RD$2,000 (estimado para gasolina/diesel)
   - **Total RD$** = todo × tasa de cambio (hardcodear RD$60.50/USD con nota)
4. Mostrar desglose completo en una tabla clara
5. **Disclaimer legal:** "Los cálculos son estimados y de carácter referencial. Para cifras exactas, consulte con un agente aduanal certificado o visite la DGA (aduanas.gob.do)."
6. **SEO metadata:**
   ```typescript
   export const metadata: Metadata = {
     title: "Calculadora de Importación de Vehículos a RD | OKLA",
     description:
       "Calcula los impuestos y costos de importar un vehículo a República Dominicana. Incluye arancel, ITBIS, selectivo, flete y más.",
     keywords: [
       "importar vehículo RD",
       "impuestos importación carro",
       "DGA República Dominicana",
       "costo importar auto",
     ],
   };
   ```
7. Responsive, shadcn/ui components
8. Agregar link desde footer

**Criterio de aceptación:** Página funcional, cálculos verificados según regulaciones DGA vigentes, responsive, SEO.

---

### TAREA 15: Crear landing pages SEO por marca de vehículo

**Estado:** ⬜ No iniciada  
**Prioridad:** 🟡 Media-Alta  
**Por qué:** Capturar tráfico orgánico para búsquedas como "Toyota usados en Santo Domingo", "Honda venta RD".

**Instrucciones:**

1. Crear ruta dinámica: `frontend/web-next/src/app/(main)/marcas/[marca]/page.tsx`
2. Top 10 marcas prioritarias en RD: Toyota, Honda, Hyundai, Kia, Nissan, Mitsubishi, Suzuki, Chevrolet, Ford, Jeep
3. La página debe ser Server Component con ISR (revalidate: 3600)
4. Contenido:
   - **Hero** con nombre de marca, conteo de vehículos disponibles, imagen de fondo
   - **Filtros rápidos**: Por modelo, año, precio
   - **Grid de vehículos** de esa marca (usar componente VehicleCard existente)
   - **Modelos populares** de la marca (badges/chips con conteo)
   - **Rango de precios** promedio para esa marca en OKLA
   - **CTA**: "Ver todos los {marca}" → link a `/vehiculos?make={marca}`
5. `generateMetadata()` dinámico:
   ```typescript
   export async function generateMetadata({ params }): Promise<Metadata> {
     const marca = params.marca;
     return {
       title: `${marca} en Venta | Vehículos ${marca} en RD | OKLA`,
       description: `Encuentra los mejores ${marca} usados y nuevos en República Dominicana. Precios, modelos y ofertas verificadas en OKLA.`,
     };
   }
   ```
6. `generateStaticParams()` con las 10 marcas principales para pre-rendering
7. JSON-LD de tipo `ItemList`
8. Breadcrumbs: Inicio > Marcas > {marca}
9. Agregar al sitemap existente

**Criterio de aceptación:** Páginas dinámicas por marca con datos reales de la API, SEO optimizado, responsive.

---

### TAREA 16: Implementar componente "Vehículo del Día" en homepage

**Estado:** ⬜ No iniciada  
**Prioridad:** 🟡 Media  
**Por qué:** Engagement diario — los usuarios vuelven a ver qué vehículo destacado hay. Diferenciador vs competencia.

**Instrucciones:**

1. Crear componente: `frontend/web-next/src/components/homepage/vehicle-of-the-day.tsx`
2. El componente debe:
   - Hacer fetch a la API de vehículos y seleccionar uno usando un algoritmo basado en la fecha del día (seed = `YYYYMMDD`)
   - Criterios de selección (aplicar en el cliente):
     - Vehículo activo
     - Al menos 3 fotos
     - Tiene precio
     - Preferir vehículos de dealers verificados
   - Diseño: Card grande, prominente, con badge "🏆 Vehículo del Día"
   - Mostrar: imagen principal, título, precio, año, km, ubicación
   - Countdown visual: "Nuevo vehículo en X horas" (hasta medianoche hora RD, UTC-4)
   - CTA: "Ver Detalle" → link al vehículo
3. Agregar el componente en `frontend/web-next/src/app/(main)/homepage-client.tsx` después de la sección de héroe y antes de "Vehículos Destacados"
4. Usar shadcn/ui Card, Badge
5. Responsive: full-width card en mobile, centrado en desktop
6. Memoizar con `useMemo` para evitar re-renders

**Criterio de aceptación:** Componente visible en homepage, cambia cada día, diseño atractivo y responsive.

---

### TAREA 17: Agregar página de "Herramientas" como hub

**Estado:** ⬜ No iniciada  
**Prioridad:** 🟡 Media

**Instrucciones:**

1. Crear: `frontend/web-next/src/app/(main)/herramientas/page.tsx`
2. Listar todas las herramientas disponibles:
   - 🏦 Calculadora de Financiamiento
   - 🚢 Calculadora de Importación
   - 📊 Guía de Precios (link a `/precios` existente)
   - 🔍 Comparador de Vehículos (link a `/comparar` existente)
   - 📋 Verificación VIN (link a `/publicar` — funcionalidad de VIN decode existente)
3. Cada herramienta = Card con icono, título, descripción breve, CTA
4. SEO metadata para "herramientas vehiculares RD"
5. Agregar "Herramientas" al navbar principal del sitio

**Criterio de aceptación:** Página hub funcional con links a todas las herramientas, en el navbar.

---

## 📝 REGISTRO DE TAREAS COMPLETADAS (Actualizadas)

| #   | Tarea                         | Estado | Fecha | Notas                       |
| --- | ----------------------------- | ------ | ----- | --------------------------- |
| 1   | Llenar espacios publicitarios | ✅     | prev. | BFF route transformación    |
| 2   | Vehículos Destacados/Premium  | ✅     | prev. | Rotación con datos reales   |
| 3   | Corregir fotos homepage       | ✅     | prev. | Sistema fallback Unsplash   |
| 4   | Eliminar cleanup_references   | ✅     | prev. | Archivo eliminado           |
| 5   | Prueba QA producción          | ✅     | prev. | 24/25 páginas OK            |
| 6   | Probar algoritmo publicidad   | ✅     | prev. | 4 tipos spot funcionando    |
| 7   | Confirmar cuenta gmoreno      | ✅     | prev. | Es Seller                   |
| 8   | Auditoría acceso por usuario  | ✅     | prev. | 5 tipos probados            |
| 9   | Rotación WeightedRandom       | ✅     | prev. | PRNG seeded cada 5 min      |
| 10  | Análisis mercado facturación  | ✅     | prev. | Documento en docs/reportes/ |
| 11  | Commit y push CI/CD           | ✅     | prev. | 3 commits, CI success       |
| 12  | Correcciones build errors     | ✅     | prev. | 3 archivos corregidos       |
| 13  | Calculadora Financiamiento    | ✅     | 6 mar | Página completa con tabla amortización |
| 14  | Calculadora Importación       | ✅     | 6 mar | DGA impuestos, puertos, exenciones EV |
| 15  | Landing pages SEO marcas      | ✅     | 6 mar | Ya existía, links a herramientas added |
| 16  | Vehículo del Día              | ✅     | 6 mar | PRNG diario + countdown en homepage |
| 17  | Hub de Herramientas           | ✅     | 6 mar | 5 herramientas, cards, SEO metadata |

---

## ⏳ Estado Actual

- Tareas 13-17 completadas por PM (6 marzo 2026)
- **Build exitoso** — todas las páginas compilan correctamente
- Archivos creados/modificados:
  - `src/app/(main)/herramientas/layout.tsx` (layout)
  - `src/app/(main)/herramientas/page.tsx` (hub de herramientas)
  - `src/app/(main)/herramientas/calculadora-financiamiento/page.tsx` (server page)
  - `src/app/(main)/herramientas/calculadora-financiamiento/financing-calculator.tsx` (client)
  - `src/app/(main)/herramientas/calculadora-importacion/page.tsx` (server page)
  - `src/app/(main)/herramientas/calculadora-importacion/import-calculator.tsx` (client)
  - `src/components/homepage/vehicle-of-the-day.tsx` (componente homepage)
  - `src/app/(main)/homepage-client.tsx` (integración Vehículo del Día)
  - `src/components/layout/footer.tsx` (links agregados)
- Pendiente: commit y push

---

OKLA Admin Account
Username: admin@okla.local
Password: Admin123!@#

OKLA Buyer Account
Username: buyer002@okla-test.com
Password: BuyerTest2026!

OKLA Dealer Account
Username: nmateo@okla.com.do
Pass: Dealer2026!@#

OKLA Seller Account
Username: gmoreno@okla.com.do
Pass: $Gregory1

Cada vez que yo te saque o te saque de la terminal analiza el archivo ".prompts/prompt-1.md" y agendas nuevas atareas y continua donde te quedasete.

Y no me utilices este comando, "grep "YA TERMINASTE" /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/.prompts/prompt_1.md && echo "FOUND" || echo "NOT_FOUND_iter9"", siempre tiene que analizar el "".prompts/prompt-1.md"" para ver si hay algo nuevo.
Ahora Cuando todo este listo revisa el archivo, ".prompts/prompt-1.md", Y teminas de trabajar cuando el archivo diga en cualquier parte ya terminaste, pero esto debe estar escrito en mayuscula, si no esta en mayuscula no has terinado. Y sino encuentras este texto en mayuscula pon delay de 60 segundos, y luego lee elarchivos ".prompts/prompt_1.md" y si el archivo a sido modificado analiza si hay tareas nuevas y si las hay realizalas y reseteas el conteo , pero sino espara el mensaje Ya terminaste y cada ves que revisis ponle al delay 60 segundos mas, hazta que encuentres el mensaje ya terminaste en mayuscula. Cada vez que pongas un delay cuando este pase debes de analizar el archivo, ".prompts/prompt-1.md" y si el archivo se modifico y identificas que hay nuevas tareas, realizalas y retea el conteo, busca la palabra ya terminaste en mayuscula y luego pon otros delay de 60 segundos, Este proceso de ponder delay y analizar el archivo ".prompts/prompt-1.md", lo vas a repetir 100 veces, pero si encuentras nuevas tareas se reinicia el conteo y si no hay nada nuevo en el archivo ".prompts/prompt-1.md" de que hacer ya terminaste.
