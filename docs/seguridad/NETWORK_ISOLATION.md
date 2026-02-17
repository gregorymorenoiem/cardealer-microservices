# 🔒 Network Isolation — Aislamiento de Red en Kubernetes

**Fecha de implementación:** Febrero 2026
**Proyecto:** OKLA (CarDealer Microservices)
**Infraestructura:** Digital Ocean Kubernetes (DOKS)

---

## ¿Qué es Network Isolation?

**Network Isolation** es el principio de seguridad que restringe qué servicios pueden comunicarse entre sí dentro de un cluster de Kubernetes. Sin esta restricción, cualquier pod puede hablar con cualquier otro pod — lo cual es un riesgo si un atacante compromete un servicio.

OKLA implementa un modelo **zero-trust**: cada servicio debe declarar explícitamente de quién acepta tráfico.

---

## Arquitectura de red

### Modelo zero-trust implementado

```
                           ┌─── INTERNET ───┐
                           │                 │
                           │   Solo HTTPS    │
                           │   puerto 443    │
                           │                 │
                           └────────┬────────┘
                                    │
                           ┌────────▼────────┐
                           │   Ingress       │
                           │   Controller    │
                           │   (nginx)       │
                           └────────┬────────┘
                                    │
                                    │ Solo frontend-web
                                    ▼
                           ┌─────────────────┐
                           │  frontend-web   │──── ÚNICA entrada
                           │  (Next.js)      │     desde internet
                           └────────┬────────┘
                                    │
                                    │ NetworkPolicy:
                                    │ solo frontend→gateway
                                    ▼
                           ┌─────────────────┐
                           │    gateway      │
                           │   (Ocelot)      │
                           └────────┬────────┘
                                    │
                    NetworkPolicy: solo gateway→servicios
                    ┌───────────────┼───────────────┐
                    ▼               ▼               ▼
             ┌────────────┐  ┌────────────┐  ┌────────────┐
             │ AuthService│  │VehiclesSvc │  │ KYCService │  ...
             └─────┬──────┘  └────────────┘  └─────┬──────┘
                   │                                │
                   │ NetworkPolicy:                 │
                   │ solo auth→postgres             │
                   ▼                                ▼
             ┌────────────┐                  ┌────────────┐
             │  postgres  │                  │   redis    │
             │   (5432)   │                  │   (6379)   │
             └────────────┘                  └────────────┘
```

---

## NetworkPolicies implementadas

### 1. Default Deny — Bloquear todo por defecto

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: default-deny-all
  namespace: okla
spec:
  podSelector: {} # Aplica a TODOS los pods
  policyTypes:
    - Ingress
    - Egress
  ingress: [] # Denegar todo ingreso
  egress: [] # Denegar todo egreso
```

> **Efecto:** Ningún pod puede comunicarse con ningún otro pod hasta que una NetworkPolicy lo permita explícitamente.

### 2. Frontend — Solo acepta de Ingress

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: frontend-web-policy
  namespace: okla
spec:
  podSelector:
    matchLabels:
      app: frontend-web
  ingress:
    - from:
        - namespaceSelector:
            matchLabels:
              kubernetes.io/metadata.name: ingress-nginx
      ports:
        - port: 8080
  egress:
    - to:
        - podSelector:
            matchLabels:
              app: gateway # Solo puede hablar con Gateway
      ports:
        - port: 8080
    - to: # DNS resolution
        - namespaceSelector:
            matchLabels:
              kubernetes.io/metadata.name: kube-system
      ports:
        - port: 53
          protocol: UDP
```

### 3. Gateway — Solo acepta de frontend-web

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: gateway-policy
  namespace: okla
spec:
  podSelector:
    matchLabels:
      app: gateway
  ingress:
    - from:
        - podSelector:
            matchLabels:
              app: frontend-web # SOLO frontend-web
      ports:
        - port: 8080
  egress:
    - to:
        - podSelector:
            matchLabels:
              tier: backend # Puede hablar con todos los microservicios
      ports:
        - port: 8080
    - to:
        - namespaceSelector:
            matchLabels:
              kubernetes.io/metadata.name: kube-system
      ports:
        - port: 53
          protocol: UDP
```

### 4. Microservicios — Solo aceptan de Gateway

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: backend-services-policy
  namespace: okla
spec:
  podSelector:
    matchLabels:
      tier: backend
  ingress:
    - from:
        - podSelector:
            matchLabels:
              app: gateway # Solo Gateway puede invocar servicios
      ports:
        - port: 8080
  egress:
    - to:
        - podSelector:
            matchLabels:
              app: postgres
      ports:
        - port: 5432
    - to:
        - podSelector:
            matchLabels:
              app: redis
      ports:
        - port: 6379
    - to:
        - podSelector:
            matchLabels:
              app: rabbitmq
      ports:
        - port: 5672
    - to:
        - podSelector:
            matchLabels:
              tier: backend # Comunicación inter-servicios vía Gateway
      ports:
        - port: 8080
    - to:
        - namespaceSelector:
            matchLabels:
              kubernetes.io/metadata.name: kube-system
      ports:
        - port: 53
          protocol: UDP
```

### 5. Bases de datos — Solo aceptan de servicios backend

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: postgres-policy
  namespace: okla
spec:
  podSelector:
    matchLabels:
      app: postgres
  ingress:
    - from:
        - podSelector:
            matchLabels:
              tier: backend
      ports:
        - port: 5432
  egress: [] # Postgres NO inicia conexiones
```

---

## Matriz de comunicación

| Origen → Destino                  | ¿Permitido? | Puerto   | Justificación            |
| --------------------------------- | ----------- | -------- | ------------------------ |
| Internet → frontend-web           | ✅          | 443→8080 | Única entrada            |
| Internet → gateway                | ❌          | —        | Sin Ingress              |
| Internet → authservice            | ❌          | —        | Sin Ingress              |
| frontend-web → gateway            | ✅          | 8080     | BFF proxy                |
| frontend-web → authservice        | ❌          | —        | Debe ir via Gateway      |
| gateway → authservice             | ✅          | 8080     | Routing legítimo         |
| gateway → vehiclessaleservice     | ✅          | 8080     | Routing legítimo         |
| authservice → postgres            | ✅          | 5432     | Base de datos            |
| authservice → rabbitmq            | ✅          | 5672     | Domain events            |
| authservice → redis               | ✅          | 6379     | Cache/sesiones           |
| vehiclessaleservice → authservice | ❌          | —        | Comunicación via Gateway |
| postgres → cualquier cosa         | ❌          | —        | Solo acepta, no inicia   |

---

## Tipos de servicio Kubernetes

| Servicio            | Tipo K8s            | ¿IP pública? | Acceso            |
| ------------------- | ------------------- | ------------ | ----------------- |
| frontend-web        | ClusterIP + Ingress | Sí (via LB)  | `okla.com.do`     |
| gateway             | ClusterIP           | No           | Solo interno      |
| authservice         | ClusterIP           | No           | Solo via Gateway  |
| vehiclessaleservice | ClusterIP           | No           | Solo via Gateway  |
| postgres            | ClusterIP           | No           | Solo backend pods |
| redis               | ClusterIP           | No           | Solo backend pods |
| rabbitmq            | ClusterIP           | No           | Solo backend pods |

---

## ¿Por qué NO usar LoadBalancer para cada servicio?

En muchos tutoriales se ve que cada servicio tiene un `type: LoadBalancer` con IP pública. Esto es **inseguro** porque:

1. **Superficie de ataque amplia** — cada IP pública es un punto de entrada
2. **Costos** — cada LB en Digital Ocean cuesta ~$12/mes
3. **Sin control centralizado** — no hay un punto único para autenticación, rate limiting, logging
4. **Descubribilidad** — un atacante puede escanear IPs y encontrar servicios

Con el modelo OKLA (1 solo Ingress → 1 LB → frontend-web), la superficie de ataque se reduce a **un solo punto de entrada**.

---

## Verificación

### Comandos para verificar las policies

```bash
# Ver todas las NetworkPolicies
kubectl get networkpolicies -n okla

# Ver detalle de una policy
kubectl describe networkpolicy gateway-policy -n okla

# Verificar que Gateway NO tiene IP externa
kubectl get svc gateway -n okla
# Debe mostrar: TYPE=ClusterIP, EXTERNAL-IP=<none>

# Intentar acceder a Gateway desde internet (debe fallar)
curl -v http://gateway.okla.svc.cluster.local:8080/health
# Solo funciona desde DENTRO del cluster

# Verificar que solo frontend-web tiene Ingress
kubectl get ingress -n okla
```

---

## Relación con las otras capas de seguridad

```
┌─────────────────────────────────────────────────────────┐
│  Capa 3: Server Actions                                │
│  → Oculta endpoints y datos en el browser              │
│  → Docs: SERVER_ACTIONS_SECURITY.md                    │
├─────────────────────────────────────────────────────────┤
│  Capa 2: BFF Pattern                                   │
│  → Gateway sin IP pública                              │
│  → Docs: BFF_PATTERN.md                                │
├─────────────────────────────────────────────────────────┤
│  Capa 1: Network Isolation  ← ESTE DOCUMENTO           │
│  → Zero-trust entre pods                               │
│  → Docs: NETWORK_ISOLATION.md                          │
└─────────────────────────────────────────────────────────┘
```

Cada capa protege contra un tipo diferente de ataque:

| Ataque                          | Capa 1 (Network) | Capa 2 (BFF) | Capa 3 (Actions) |
| ------------------------------- | ---------------- | ------------ | ---------------- |
| Escaneo de puertos externo      | ✅ Bloquea       | ✅ Sin IP    | N/A              |
| Acceso directo a microservicio  | ✅ Solo Gateway  | ✅ Sin IP    | N/A              |
| Inspección de Network tab       | N/A              | Parcial      | ✅ Oculta todo   |
| Pod comprometido escala lateral | ✅ NetworkPolicy | N/A          | N/A              |
| Extensión maliciosa del browser | N/A              | N/A          | ✅ Datos opacos  |

---

_Documentación de seguridad — OKLA — Febrero 2026_
