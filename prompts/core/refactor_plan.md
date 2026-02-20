---
version: 2.0
lastUpdated: 2026-02-19
author: gregorymoreno
---

Context: #codebase
Diseña un plan de refactor por fases (PRs pequeños e independientes).
Criterios: mínimo riesgo, máximo impacto, cero breaking changes no anunciados.

Para cada PR incluye:
- Objetivo en una línea
- Archivos probables a modificar
- Tests que deben actualizarse o crearse
- Dependencia de otros PRs (si existe)
- Riesgo y estrategia de rollback
- Criterio de aceptación medible

Salida:
- Lista de PRs ordenados (PR1 → PR2 → ...)
- Checklist de validación por PR
- Estimación de esfuerzo por PR
- Diagrama de dependencias entre PRs (texto ASCII)
