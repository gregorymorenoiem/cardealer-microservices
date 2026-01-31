# 📁 01-PUBLICO - Páginas Públicas

> **Descripción:** Páginas accesibles sin autenticación  
> **Total:** 10 documentos  
> **Prioridad:** 🔴 P0 - Core del sistema

---

## 📋 Documentos en Esta Sección

| #   | Archivo                                            | Descripción                                     | Prioridad |
| --- | -------------------------------------------------- | ----------------------------------------------- | --------- |
| 1   | [01-home.md](01-home.md)                           | Página principal con hero, búsqueda y secciones | P0        |
| 2   | [02-busqueda.md](02-busqueda.md)                   | Búsqueda avanzada de vehículos                  | P0        |
| 3   | [03-detalle-vehiculo.md](03-detalle-vehiculo.md)   | Página de detalle de vehículo                   | P0        |
| 4   | [04-help-center.md](04-help-center.md)             | Centro de ayuda y FAQ                           | P2        |
| 5   | [05-vehicle-360-page.md](05-vehicle-360-page.md)   | Visor 360° de vehículos                         | P1        |
| 6   | [06-comparador.md](06-comparador.md)               | Comparador de vehículos                         | P1        |
| 7   | [07-filtros-avanzados.md](07-filtros-avanzados.md) | Sistema de filtros avanzados                    | P0        |
| 8   | [08-search-completo.md](08-search-completo.md)     | Búsqueda completa con todos los features        | P0        |
| 9   | [09-vehicle-browse.md](09-vehicle-browse.md)       | Navegación de vehículos                         | P1        |
| 10  | [10-homepage-public.md](10-homepage-public.md)     | Homepage y perfiles públicos                    | P1        |

---

## 🎯 Orden de Implementación para IA

```
1. 01-home.md           → Página principal (landing)
2. 07-filtros-avanzados.md → Sistema de filtros (se usa en búsqueda)
3. 02-busqueda.md       → Búsqueda de vehículos
4. 08-search-completo.md → Búsqueda avanzada completa
5. 03-detalle-vehiculo.md → Detalle de vehículo
6. 05-vehicle-360-page.md → Vista 360°
7. 06-comparador.md     → Comparador
8. 09-vehicle-browse.md → Navegación
9. 10-homepage-public.md → Perfiles públicos
10. 04-help-center.md   → Centro de ayuda
```

---

## 🔗 Dependencias Externas

- **03-COMPONENTES/**: VehicleCard, SearchFilters, Gallery
- **05-API-INTEGRATION/**: vehicles-api, catalog-api
- **02-UX-DESIGN-SYSTEM/**: Design tokens, componentes base

---

## 📊 APIs Utilizadas

| Servicio            | Endpoints Principales                                  |
| ------------------- | ------------------------------------------------------ |
| VehiclesSaleService | GET /vehicles, GET /vehicles/:id, GET /vehicles/search |
| CatalogService      | GET /catalog/makes, GET /catalog/models                |
| Vehicle360Service   | GET /vehicle360/:id                                    |
| ComparisonService   | GET /comparisons, POST /comparisons                    |
