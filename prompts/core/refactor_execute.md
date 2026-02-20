---
version: 2.0
lastUpdated: 2026-02-19
author: gregorymoreno
---

Context: #file:[RUTA DEL MÓDULO]
Módulo a refactorizar: [NOMBRE / RUTA]
Plan a seguir: [pega el plan del PR correspondiente de CORE-C]

Reglas:
- No cambies comportamiento observable (salvo bugs documentados en el plan)
- No renombres sin necesidad; si renombras, indica todos los sitios de uso
- Mantén compatibilidad hacia atrás en contratos públicos
- Usa patrones del repo; no introduzcas nuevas dependencias sin justificación

Entrega:
- Lista de archivos a crear/modificar con rutas exactas
- Cambios por archivo: qué se modifica y por qué
- Tests nuevos o actualizados (con código)
- Cómo verificar que no rompió nada (comando de test específico)
- Checklist de PR: qué revisar antes de hacer merge
