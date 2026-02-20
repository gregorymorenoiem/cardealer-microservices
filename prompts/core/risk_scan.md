---
version: 2.0
lastUpdated: 2026-02-19
author: gregorymoreno
---

Context: #codebase
Escanea el repo para detectar los 10 mayores riesgos y deudas técnicas.
Prioriza: seguridad (auth, secretos, inyección), CI/CD roto, errores silenciados,
queries sin índice, dependencias con CVE, performance evidente, configuración expuesta.

Salida:
- Top 10 riesgos: Severidad | Archivo:línea | Descripción en 1 línea | Acción inmediata
- Veredicto final: ¿vale la pena un premium request de auditoría completa? Sí/No + razón
