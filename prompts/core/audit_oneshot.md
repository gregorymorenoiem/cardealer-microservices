Eres un equipo de 4 especialistas (Frontend, Backend, Arquitectura, Infra/DevOps/Sec).
Contexto: #codebase
Reglas: no preguntas a mitad; declara supuestos si falta info; evidencia con ruta + línea.

Revisa explícitamente: README/docs, dependencias y versiones, entrypoints, Docker,
workflows CI/CD, variables de entorno, manejo de errores, autenticación/autorización,
queries a base de datos, surface de API expuesta, logs y observabilidad.

Entrega:
1. Tabla: Severidad | Área | Hallazgo | Evidencia (ruta:línea) | Riesgo real | Fix recomendado | Esfuerzo (h) | Prioridad
2. Severidades usadas: CRÍTICO / ALTO / MEDIO / BAJO / INFORMATIVO
3. Plan: 24h (críticos) / 7 días (altos) / 30 días (medios)
4. Top 10 quick wins (< 30 min cada uno)
5. Si hay 3+ críticos: snippet de parche mínimo viable para cada uno
6. Dependencias desactualizadas con CVE conocido: lista separada
7. Deuda técnica acumulada: resumen en 5 líneas

Salida: Markdown con tabla y secciones. Prioriza evidencia con links a archivos del repo.
