# 📋 OKLA — Sprint 18 Proposal (Draft)

**Fecha de elaboración:** 2026-03-06
**Autor:** CPSO (Copilot)
**Estado:** BORRADOR — Pendiente de aprobación y ejecución de Sprint 16/17 Developer
**Basado en:** 7 auditorías técnicas realizadas (Backend, Frontend, DB, CI/CD, K8s, Shared Libs, SEO)

---

## 🔧 Sprint 18 — Desarrollador Senior

**Período:** [Después de completar Sprint 17] → +7 días
**Módulos asignados:** Gateway CI/CD, LlmServer, VehiclesSaleService DB, K8s deployments
**⚠️ Áreas bloqueadas para el CPSO:** Gateway, LlmServer, VehiclesSaleService, k8s/deployments.yaml

### Tarea 18.1: 🔴 SEGURIDAD — Fix LlmServer Dockerfile (non-root user)

**Objetivo:** Eliminar el riesgo de escalación de privilegios en el container de LlmServer
**Archivos:** `backend/LlmServer/Dockerfile`
**Instrucciones:**

1. Agregar creación de usuario non-root:

```dockerfile
RUN addgroup --system --gid 1000 appuser && \
    adduser --system --uid 1000 --gid 1000 appuser
```

2. Cambiar a usuario non-root antes del ENTRYPOINT:

```dockerfile
USER appuser
```

3. Asegurar que los directorios de trabajo tengan permisos correctos
4. Verificar que el health check siga funcionando
   **Criterios:** Container corre como UID 1000. `kubectl exec` confirma `whoami` = appuser
   **QA:** Build local, test health endpoint, deploy a staging

### Tarea 18.2: 🔴 CI/CD — Hacer Trivy blocking para vulnerabilidades CRITICAL

**Objetivo:** Prevenir deployment de imágenes con CVEs críticos conocidos
**Archivos:** `.github/workflows/smart-cicd.yml` (o el workflow principal)
**Instrucciones:**

1. Remover `continue-on-error: true` del step de Trivy scan
2. Agregar `exit-code: '1'` y `severity: 'CRITICAL'` a la configuración de Trivy
3. Mantener WARNING para HIGH (no blocking)
   **Criterios:** Pipeline falla si hay CVE CRITICAL. Pipeline pasa con solo HIGH/MEDIUM
   **QA:** Trigger build, verificar que scan funciona. Test con imagen con CVE conocido

### Tarea 18.3: 🟠 PERFORMANCE — Fix paginación en memoria en VehiclesSaleService

**Objetivo:** Mover Skip/Take a nivel de DB para evitar cargar todos los vehículos en RAM
**Archivos:** `backend/VehiclesSaleService/VehiclesSaleService.Infrastructure/Repositories/VehicleRepository.cs`
**Instrucciones:**

1. En `GetBySellerIdAsync`: Mover `.OrderBy().Skip().Take()` ANTES de `.ToListAsync()`
2. En `SearchAsync`: Mismo patrón — paginación en query, no en memoria
3. Agregar `AsNoTracking()` a todas las queries de lectura
4. Agregar parámetros `pageSize/pageNumber` a métodos que no los tengan
   **Criterios:** Query SQL generada debe incluir `LIMIT/OFFSET`. Verificar con logging de EF Core
   **QA:** Test con seller con 100+ vehículos. Comparar uso de memoria antes/después

### Tarea 18.4: 🟠 K8s — Fix securityContext en frontend-web, gateway, chatbotservice

**Objetivo:** Hardening de seguridad en los 3 workloads más expuestos
**Archivos:** `k8s/deployments.yaml`
**Instrucciones:**

1. Para `frontend-web`: Agregar `securityContext: { runAsNonRoot: true, runAsUser: 1001, readOnlyRootFilesystem: true, allowPrivilegeEscalation: false, capabilities: { drop: [ALL] } }`
2. Para `gateway`: Agregar `runAsNonRoot: true, runAsUser: 1000`
3. Para `chatbotservice`: Agregar securityContext completo (mismo que authservice)
4. Agregar tmpfs volume mount si readOnlyRootFilesystem causa problemas con logs/temp
   **Criterios:** `kubectl describe pod` muestra securityContext. Pods arrancan sin crash
   **QA:** Deploy a staging, verificar health checks, smoke test de funcionalidad

### Tarea 18.5: 🟡 K8s — Eliminar HPAs duplicados y consolidar

**Objetivo:** Resolver conflictos entre hpa.yaml y scaling.yaml
**Archivos:** `k8s/hpa.yaml`, `k8s/scaling.yaml`
**Instrucciones:**

1. Identificar HPAs duplicados (gateway-hpa, vehiclessaleservice-hpa)
2. Mantener la versión de `hpa.yaml` (más conservadora: minReplicas=2)
3. Remover las entradas duplicadas de `scaling.yaml`
4. Documentar en comentario YAML la razón de los parámetros elegidos
   **Criterios:** `kubectl get hpa` muestra un solo HPA por servicio. No hay warnings de conflicto
   **QA:** `kubectl apply --dry-run=server` para verificar antes de aplicar

---

## 🎯 Sprint 18 — CPSO (Copilot)

**Período:** [Simultáneo con Sprint 18 Developer]
**Módulos asignados:** \_Shared/HealthChecks, \_Shared/Contracts, docs/, ChatbotService CI
**⚠️ Áreas bloqueadas para el Desarrollador:** \_Shared/HealthChecks, \_Shared/Contracts, ChatbotService CI config

### Tarea 18.6: Fix `/health` default para excluir tag "external"

**Objetivo:** Alinear la librería con la regla crítica del proyecto
**Archivos:** `backend/_Shared/CarDealer.Shared.HealthChecks/Extensions/HealthCheckEndpointExtensions.cs`
**Instrucciones:**

1. Modificar `MapStandardHealthChecks` para que `/health` excluya checks con tag "external"
2. Mantener `/health/ready` incluyendo solo checks con tag "ready"
3. Mantener `/health/live` sin predicate (siempre healthy)
4. Agregar `/health/all` endpoint que incluya TODO (para debugging)

### Tarea 18.7: Actualizar `ServiceName` enum en Contracts

**Objetivo:** El enum está desactualizado (9 servicios, existen 20+)
**Archivos:** `backend/_Shared/CarDealer.Contracts/Enums/ServiceName.cs`
**Instrucciones:**

1. Agregar todos los servicios faltantes: BillingService, KYCService, VehiclesSaleService, ReportsService, ReviewService, CRMService, DealerAnalyticsService, AIProcessingService, RecommendationService, VehicleIntelligenceService, ChatbotService, ComparisonService, FeatureToggleService, SearchAgent, SupportAgent, RecoAgent
2. Mantener valores numéricos existentes sin cambiar (backward compatibility)
3. Agregar nuevos valores con números secuenciales después del último existente

### Tarea 18.8: Deduplicar CI de ChatbotService

**Objetivo:** ChatbotService se construye en dos pipelines simultáneamente
**Archivos:** `.github/workflows/smart-cicd.yml` o pipeline dedicada de chatbot
**Instrucciones:**

1. Identificar qué pipeline es redundante
2. Remover la entrada de ChatbotService de uno de los dos workflows
3. Mantener el workflow más completo (con tests, scan, etc.)

### Tarea 18.9: Crear documento de estándares actualizado (copilot-instructions v3)

**Objetivo:** El copilot-instructions.md tiene discrepancias con la realidad del código
**Archivos:** `.github/copilot-instructions.md`
**Instrucciones (actualizaciones necesarias):**

1. Cambiar "React Native" a "Flutter/Dart" en sección mobile
2. Agregar "Zustand fue eliminado — solo usar TanStack Query y React Context" en frontend
3. Actualizar lista de shared libraries (actualmente hay 15, no las que se listan)
4. Agregar regla sobre `Directory.Packages.props` (cuando se implemente)
5. Agregar sección sobre ServiceDiscovery (Consul vs K8s DNS)

### Tarea 18.10: Reporte integral de estado técnico del proyecto

**Objetivo:** Consolidar los 7 reportes de auditoría en un reporte ejecutivo
**Archivos:** `docs/reportes/OKLA_TECHNICAL_STATE_2026-Q1.md`
**Contenido:**

1. Resumen ejecutivo del estado técnico
2. Scorecard por área (Backend, Frontend, DB, CI/CD, K8s, Shared Libs, SEO)
3. Top 20 issues priorizados por impacto/esfuerzo
4. Roadmap técnico Q2 2026
5. Proyección de deuda técnica si no se abordan los issues

---

## Priorización Global Sprint 18

| #   | Tarea                     | Tipo          | Impacto | Esfuerzo | Score |
| --- | ------------------------- | ------------- | ------- | -------- | ----- |
| 1   | 18.1 LlmServer non-root   | Seguridad     | Alto    | Bajo     | 🔴 P0 |
| 2   | 18.4 K8s securityContext  | Seguridad     | Alto    | Bajo     | 🔴 P0 |
| 3   | 18.2 Trivy blocking       | Seguridad     | Alto    | Bajo     | 🔴 P0 |
| 4   | 18.3 DB paginación        | Performance   | Alto    | Medio    | 🟠 P1 |
| 5   | 18.6 /health external     | Correctness   | Medio   | Bajo     | 🟠 P1 |
| 6   | 18.7 ServiceName enum     | Correctness   | Bajo    | Bajo     | 🟡 P2 |
| 7   | 18.5 HPAs duplicados      | Operacional   | Medio   | Bajo     | 🟡 P2 |
| 8   | 18.8 ChatbotService CI    | DevEx         | Bajo    | Bajo     | 🟡 P2 |
| 9   | 18.9 copilot-instructions | Documentación | Medio   | Medio    | 🟡 P2 |
| 10  | 18.10 Reporte Q1          | Documentación | Medio   | Alto     | 🔵 P3 |
