# ☸️ OKLA — Auditoría de Manifiestos Kubernetes

**Fecha:** 2026-03-06
**Autor:** CPSO (Copilot)
**Scope:** Todos los manifiestos K8s en `k8s/` (26 archivos, ~6,000+ líneas)

---

## 📊 Resumen Ejecutivo

| Área             | Estado           | Hallazgos Críticos                                     |
| ---------------- | ---------------- | ------------------------------------------------------ |
| Security Context | ⚠️ Inconsistente | 3 servicios sin hardening (frontend, gateway, chatbot) |
| Image Tags       | 🔴 Crítico       | 100% de deployments usan `:latest`                     |
| Autoscaling      | ⚠️ Conflictos    | HPAs duplicados con config diferente                   |
| Network Policies | ⚠️ Conflictos    | Políticas duplicadas entre 2 archivos                  |
| Rate Limiting    | 🔴 Ausente       | Sin rate limiting en Ingress                           |
| Secrets          | ⚠️ Compartidos   | Una sola credencial DB para todos los servicios        |

---

## 1. Hallazgos Críticos 🔴

### C1. frontend-web y gateway sin `runAsNonRoot`

Los dos workloads más expuestos al exterior carecen de hardening de seguridad:

- **frontend-web**: sin `runAsNonRoot`, `readOnlyRootFilesystem: false`
- **gateway**: sin `runAsNonRoot`
- **chatbotservice**: sin `securityContext` completo (tiene acceso a API keys de Claude y WhatsApp)

### C2. Todas las imágenes usan `:latest`

Cada deployment usa `image: ghcr.io/gregorymorenoiem/*:latest`. Esto hace rollbacks imposibles y deployments impredecibles. Violación directa de las instrucciones del proyecto.

### C3. HPAs duplicados y conflictivos

Dos archivos definen HPAs para los mismos servicios con **configs diferentes**:

- `hpa.yaml`: gateway-hpa con min=2, max=8, CPU 50%
- `scaling.yaml`: gateway-hpa con min=1, max=3, CPU 65%

### C4. NetworkPolicies en conflicto

`rbac-netpol.yaml` contiene políticas que **contradicen** las de `network-policies.yaml`:

- `allow-ingress-to-gateway` permite ingress-nginx → gateway directamente
- Esto **rompe el patrón BFF** (solo frontend debería llegar a gateway)

### C5. BillingService sin autoscaling

Dice "gestionado por KEDA" pero **no existe ScaledObject para BillingService**. Servicio de pagos sin escalamiento automático.

### C6. Sin rate limiting en Ingress

El ingress no tiene anotaciones de rate limiting. Frontend expuesto a DDoS y brute-force sin protección.

---

## 2. Warnings ⚠️

| #   | Warning                                     | Impacto                                              |
| --- | ------------------------------------------- | ---------------------------------------------------- |
| W1  | `imagePullPolicy` no explícito              | Con `:latest`, K8s usa `Always` = pulls innecesarios |
| W2  | Secrets comparten un solo user DB           | Compromiso de un servicio = acceso a todas las DBs   |
| W3  | Redis password en CLI args                  | Visible en `kubectl describe pod`                    |
| W4  | Infra pods sin securityContext              | Redis, RabbitMQ sin hardening                        |
| W5  | Prometheus usa `:latest`                    | Puede romper monitoring inesperadamente              |
| W6  | 3 servicios activos sin PDB                 | auditservice, reviewservice, chatbotservice          |
| W7  | contactservice: HPA/PDB sin deployment      | Recursos huérfanos                                   |
| W8  | Replicas base = 1 para servicios "critical" | Si HPA falla, no hay HA                              |
| W9  | Sin service mesh / mTLS                     | Tráfico entre servicios es HTTP plano                |

---

## 3. Aspectos Positivos ✅

| Aspecto                             | Estado                                        |
| ----------------------------------- | --------------------------------------------- |
| Namespace isolation                 | ✅ Todo en namespace `okla`                   |
| RBAC                                | ✅ ServiceAccounts + Roles dedicados          |
| Resource Limits                     | ✅ Requests + Limits en todos los deployments |
| LimitRange + ResourceQuota          | ✅ Defaults y límites globales                |
| Probes (Startup/Liveness/Readiness) | ✅ En cada deployment                         |
| TLS (cert-manager + Let's Encrypt)  | ✅ Configurado                                |
| BFF Pattern                         | ✅ Solo frontend expuesto, gateway interno    |
| 100% ClusterIP                      | ✅ Sin NodePort/LoadBalancer filtrados        |
| KEDA event-driven scaling           | ✅ Para consumidores de colas                 |
| Topology/Anti-affinity              | ✅ Hard/soft por tier                         |
| Monitoring stack                    | ✅ Prometheus + Grafana                       |
| Cluster Autoscaler                  | ✅ DO DOKS 2-6 nodos                          |

---

## 4. Matriz de Servicios Activos

| Servicio            | Replicas | Resources | Probes | runAsNonRoot | PDB | HPA/KEDA |
| ------------------- | -------- | --------- | ------ | ------------ | --- | -------- |
| frontend-web        | 1        | ✅        | ✅     | ❌           | ✅  | HPA 2-8  |
| gateway             | 1        | ✅        | ✅     | ❌           | ✅  | HPA 2-8  |
| authservice         | 1        | ✅        | ✅     | ✅           | ✅  | HPA 2-8  |
| vehiclessaleservice | 1        | ✅        | ✅     | ✅           | ✅  | HPA 2-8  |
| billingservice      | 1        | ✅        | ✅     | ✅           | ✅  | ❌ None  |
| chatbotservice      | 1        | ✅        | ✅     | ❌           | ❌  | HPA 1-4  |
| adminservice        | 1        | ✅        | ✅     | ✅           | ✅  | HPA 1-3  |
| kycservice          | 1        | ✅        | ✅     | ✅           | ✅  | HPA 1-4  |
| auditservice        | 1        | ✅        | ✅     | ✅           | ❌  | KEDA     |
| reviewservice       | 1        | ✅        | ✅     | ✅           | ❌  | None     |

---

## 5. Top 5 Recomendaciones (Sprint 18/19)

| #   | Recomendación                                             | Impacto                                | Esfuerzo |
| --- | --------------------------------------------------------- | -------------------------------------- | -------- |
| 1   | **Fix securityContext** en frontend, gateway, chatbot     | Cierra brecha de seguridad crítica     | Bajo     |
| 2   | **Eliminar recursos duplicados** (HPAs, NetworkPolicies)  | Evita conflictos de configuración      | Bajo     |
| 3   | **Tags inmutables** (commit SHA) en todos los deployments | Deployments reproducibles, rollbacks   | Medio    |
| 4   | **Rate limiting en Ingress** + secrets por servicio       | Protección DDoS + aislamiento de datos | Medio    |
| 5   | **BillingService autoscaler** + PDBs faltantes            | HA para servicio de pagos crítico      | Bajo     |

---

## 6. Integración con Sprint 18

**Developer Tasks:**

- 18.4: Fix securityContext en frontend-web, gateway, chatbot deployments
- 18.5: Eliminar HPAs duplicados en scaling.yaml, consolidar en hpa.yaml

**CPSO Tasks:**

- 18.9: Remover NetworkPolicies de rbac-netpol.yaml (pertenecen solo a network-policies.yaml)
- 18.10: Agregar rate limiting annotations al Ingress
