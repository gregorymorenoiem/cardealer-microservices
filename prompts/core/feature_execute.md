---
version: 2.0
lastUpdated: 2026-02-19
author: gregorymoreno
---

Context: #codebase
Spec a implementar: [pega la spec de CORE-E]

Reglas:
- Implementa estrictamente según la spec; declara supuestos al inicio
- Reutiliza patrones, utilidades y libs ya presentes en el repo
- No introduces dependencias nuevas sin mencionar alternativa interna

Entrega:
- Lista de archivos a crear/modificar (con rutas)
- Implementación por capas: FE → BE → infra
- Migraciones de base de datos si aplica (con script o migration file)
- Variables de entorno nuevas requeridas (.env.example actualizado)
- Tests a añadir con código completo
- Documentación inline para funciones públicas
- Checklist final antes de PR
