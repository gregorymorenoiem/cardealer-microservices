---
version: 2.0
lastUpdated: 2026-02-19
author: gregorymoreno
---

Context: #file:[RUTA DEL MÓDULO A TESTEAR]
Framework: detecta el framework de tests del repo y adáptate.

Genera una suite de tests completa para este módulo:
- Happy path
- Edge cases (null, empty, extremes)
- Casos de error (excepciones, retornos erróneos)
- Mocks de dependencias externas
- Tests de integración si hay side effects

Convenciones:
- Nombra describe con el módulo/función
- Nombra it con "should [comportamiento] when [condición]"
- Objetivo cobertura: >=80% ramas
