# 🔴 Post-Mortem: Deploy Febrero 2026 — ¿Por qué fallaron 6 cosas si hubo 15 auditorías?

**Proyecto:** OKLA (CarDealer Microservices)  
**Fecha del deploy:** Febrero 17–18, 2026  
**Autor:** Análisis post-mortem automatizado  
**Clasificación:** Análisis de brechas entre auditorías y operaciones reales

---

## 📋 Resumen Ejecutivo

El deploy a Digital Ocean Kubernetes (DOKS) presentó **6 fallos críticos** que requirieron intervención manual durante varias horas. A pesar de contar con **15 auditorías realizadas** por especialistas diversos, **ninguna** de las 15 detectó ni previno estos problemas.

**La causa raíz no es que las auditorías fallaron.** La causa raíz es que **nunca se auditó la capa de infraestructura y deployment.** Todas las auditorías cubrieron código de aplicación, arquitectura, IA, frontend y procesos de negocio — pero ninguna cubrió Dockerfiles, CI/CD, Kubernetes manifests, ni el pipeline de deployment.

---

## 🔥 Los 6 Fallos del Deploy

### Fallo 1: AuthService — Readiness Probe Timeout (200 segundos)

| Detalle | Valor |
|---------|-------|
| **Síntoma** | Pod `authservice` nunca pasaba readiness check, K8s lo mataba y reiniciaba en loop |
| **Causa raíz** | `ExternalServiceHealthCheck` intentaba conectar a Consul en `localhost:8500` (no desplegado). Timeout de 200s bloqueaba el thread pool, matando también `/health/ready` |
| **Fix** | Excluir checks con tag `"external"` del endpoint `/health` en `Program.cs` |
| **Archivo** | `AuthService.Api/Program.cs` línea 331 |

**¿Alguna auditoría lo cubría?**
- ⚠️ **Parcialmente.** La auditoría de Observabilidad (#6) listó los health endpoints de cada servicio, pero **NO ejecutó** los health checks para ver si funcionaban. Solo verificó que existían en el código.
- ❌ Nadie verificó que Consul no estaba desplegado en K8s.
- ❌ Nadie hizo un `kubectl exec` → `curl /health` para validar tiempos de respuesta.

---

### Fallo 2: Registry Credentials Expiradas (ImagePullBackOff)

| Detalle | Valor |
|---------|-------|
| **Síntoma** | Pods nuevos no podían bajar imágenes de GHCR — `ImagePullBackOff` con HTTP 403 |
| **Causa raíz** | El K8s secret `registry-credentials` contenía un token `ghs_*` efímero de GitHub Actions que expiró después del workflow |
| **Fix** | Recrear secret con token `gho_*` OAuth de `gh auth token` |
| **Archivo** | Secret K8s `registry-credentials` |

**¿Alguna auditoría lo cubría?**
- ❌ **Ninguna.** Cero auditorías revisaron secrets de Kubernetes, políticas de rotación de tokens, ni el flujo de autenticación al container registry.

---

### Fallo 3: IDeadLetterQueue — DI Crash en 6 Servicios

| Detalle | Valor |
|---------|-------|
| **Síntoma** | 6 servicios crasheaban al iniciar: `Unable to resolve service for type 'IDeadLetterQueue'` |
| **Causa raíz** | `AddPostgreSqlDeadLetterQueue()` registra `ISharedDeadLetterQueue` (de CarDealer.Shared), pero `DeadLetterQueueProcessor` depende de `IDeadLetterQueue` (interfaz local). Mismatch de interfaces en DI container |
| **Fix** | Agregar `AddSingleton<IDeadLetterQueue, InMemoryDeadLetterQueue>()` a los 6 servicios |
| **Archivos** | `Program.cs` de Auth, Error, Role, Audit, Notification, Media services |

**¿Alguna auditoría lo cubría?**
- ❌ **Ninguna.** Cero auditorías revisaron el wiring de Dependency Injection en `Program.cs`.
- ❌ La auditoría de Observabilidad (#6) evaluó patrones de arquitectura pero NO ejecutó los servicios para verificar que arrancan.
- ❌ La auditoría de Testing (#6) reportó cobertura de 52/100 — ningún test de integración validaba el startup del DI container.

---

### Fallo 4: Docker Build Cache Envenenado

| Detalle | Valor |
|---------|-------|
| **Síntoma** | CI/CD reportaba builds exitosos, pero las imágenes Docker tenían el mismo digest (código viejo). Los pods seguían corriendo código anterior |
| **Causa raíz** | `_reusable-dotnet-service.yml` usa `cache-from: type=local` con `restore-keys` pattern. Buildx reutilizaba ALL cached layers incluyendo `COPY . .` y `dotnet publish`, produciendo imágenes idénticas a las anteriores |
| **Fix** | Eliminar todos los caches `Linux-buildx-*` via `gh cache delete` |
| **Archivo** | `.github/workflows/_reusable-dotnet-service.yml` líneas 187-196 |

**¿Alguna auditoría lo cubría?**
- ❌ **Ninguna.** Cero auditorías revisaron los workflows de GitHub Actions, la estrategia de cache de Docker, ni la configuración de buildx.
- ❌ Ni siquiera la auditoría MLOps (#10), que creó un workflow CI/CD para el chatbot, revisó los workflows existentes de los otros 13 servicios.

---

### Fallo 5: RabbitMQ Queue PRECONDITION_FAILED

| Detalle | Valor |
|---------|-------|
| **Síntoma** | NotificationService crasheaba con `PRECONDITION_FAILED` al declarar queues |
| **Causa raíz** | Las queues existentes en RabbitMQ fueron creadas sin `x-dead-letter-exchange`. El código nuevo las declara CON `x-dead-letter-exchange: notification-exchange.dlx`. RabbitMQ no permite cambiar argumentos de una queue existente |
| **Fix** | Eliminar las 6 queues existentes via `rabbitmqctl delete_queue` para que el código las recreara con los argumentos correctos |
| **Archivo** | Queue topology en RabbitMQ (runtime, no código) |

**¿Alguna auditoría lo cubría?**
- ❌ **Ninguna.** Cero auditorías revisaron la topología de RabbitMQ (exchanges, queues, bindings, argumentos).
- ⚠️ La auditoría de Observabilidad (#6) mencionó "ErrorService DLQ" y "RabbitMQ health check ausente" pero NO auditó la configuración real de las queues ni sus argumentos.
- ❌ No existe proceso de migración de queues (análogo a EF Core migrations para DB).

---

### Fallo 6: Frontend Image Name Mismatch

| Detalle | Valor |
|---------|-------|
| **Síntoma** | `okla.com.do` mostraba la página vieja (Vite/SPA con "cardealer.do") en vez del Next.js nuevo |
| **Causa raíz** | K8s `deployments.yaml` referenciaba `cardealer-web:latest` (imagen vieja), pero el CI/CD workflow construye y pushea `frontend-web:latest` (imagen nueva). Mismatch en el nombre de imagen |
| **Fix** | `kubectl set image` + actualizar `deployments.yaml` línea 50 |
| **Archivo** | `k8s/deployments.yaml` línea 50 |

**¿Alguna auditoría lo cubría?**
- ❌ **Ninguna.** Cero auditorías compararon los nombres de imagen en `deployments.yaml` contra los nombres en los workflows de CI/CD.
- ❌ La auditoría de Gateway (#5) revisó rutas Ocelot pero NO los manifests de K8s.
- ❌ No existe validación automatizada que compare `deployments.yaml` ↔ `_reusable-frontend.yml` ↔ GHCR registry.

---

## 📊 Matriz: Auditorías vs Fallos

| Auditoría | Scope | F1: Health | F2: Registry | F3: DI | F4: Cache | F5: RabbitMQ | F6: Image |
|-----------|-------|:----------:|:------------:|:------:|:---------:|:------------:|:---------:|
| #1 Model Architect | ChatbotService arch | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| #2 AI Researcher | LLM pipeline | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| #3 Frontend Auditor | React/TSX code | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| #4 Roles & Security | RBAC flows | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| #5 Gateway Auditor | Ocelot routes | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| #6 Standards & Observ. | Code quality | ⚠️ Parcial | ❌ | ❌ | ❌ | ⚠️ Parcial | ❌ |
| #7 Business Coverage | Processes | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| #8 API Documentation | Endpoint docs | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| #9 Conversational AI | Chatbot dialogue | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| #10 MLOps Engineer | ML operations | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |

**Resultado: 0 de 6 fallos fueron detectados por alguna auditoría.**

---

## 🔍 Análisis: ¿Por Qué Pasó Esto?

### 1. Sesgo hacia código de aplicación, ceguera hacia infraestructura

Las 15 auditorías se distribuyeron así:

```
Código de Aplicación (.cs, .tsx):  ██████████████ 10 auditorías (67%)
IA / Machine Learning:             ████           4 auditorías (27%)
Infraestructura (Docker/K8s/CI):   ░              0 auditorías (0%)  ← BRECHA
```

**El 100% del esfuerzo de auditoría se concentró en "¿el código está bien escrito?"** pero nadie preguntó **"¿el código llega correctamente a producción?"**

### 2. Auditorías estáticas vs verificación dinámica

Todas las auditorías fueron **análisis estático de archivos**:
- Leer código → opinar sobre patrones
- Leer configs → verificar completitud
- Contar tests → medir cobertura

**Ninguna auditoría ejecutó el sistema:**
- ❌ Nadie corrió `docker build` para verificar que la imagen se construye
- ❌ Nadie corrió `docker compose up` para verificar que los servicios arrancan
- ❌ Nadie desplegó en un cluster de prueba
- ❌ Nadie verificó que los health checks responden < 10s
- ❌ Nadie verificó que las queues de RabbitMQ son compatibles entre versiones

### 3. Falta de un especialista de infraestructura

| Capa del sistema | Especialista asignado | Estado |
|------------------|-----------------------|--------|
| Chatbot / IA | 4 especialistas | ✅ Ultra-cubierto |
| Frontend React | 1 especialista | ✅ Cubierto |
| Backend .NET (código) | 3 especialistas | ✅ Cubierto |
| Business processes | 1 especialista | ✅ Cubierto |
| **Dockerfiles** | **Ninguno** | 🔴 **NO CUBIERTO** |
| **CI/CD Workflows** | **Ninguno** | 🔴 **NO CUBIERTO** |
| **K8s Manifests** | **Ninguno** | 🔴 **NO CUBIERTO** |
| **Messaging (RabbitMQ)** | **Ninguno** | 🔴 **NO CUBIERTO** |
| **DI Wiring (Program.cs)** | **Ninguno** | 🔴 **NO CUBIERTO** |
| **Secrets & Credentials** | **Ninguno** | 🔴 **NO CUBIERTO** |

### 4. La falsa sensación de seguridad

Con puntuaciones de **9.2/10**, **9.3/10**, **9.0/10** en múltiples auditorías, se creó una percepción de que el sistema estaba "listo para producción". Pero estas puntuaciones solo reflejan la calidad del **código de aplicación**, no la madurez del **pipeline de deployment**.

Es como auditar que un avión tiene excelentes motores, alas y aviónica — pero nadie revisó si la pista de aterrizaje existe.

---

## 🎯 Los 6 Fallos Categorizados

| Fallo | Categoría | ¿Se detecta con test unitario? | ¿Se detecta con análisis estático? | ¿Cómo se detecta? |
|-------|-----------|:------------------------------:|:----------------------------------:|---------------------|
| F1: Health check timeout | Infra + Config | ❌ | ❌ | Smoke test en cluster real |
| F2: Registry credentials | Secrets/DevOps | ❌ | ❌ | Checklist de rotación de secrets |
| F3: DI mismatch | Wiring/Startup | ✅ Con integration test | ⚠️ Difícil | `WebApplicationFactory` test |
| F4: Build cache poison | CI/CD | ❌ | ❌ | Verificar image digest post-build |
| F5: RabbitMQ args | Messaging infra | ❌ | ❌ | Queue migration strategy |
| F6: Image name mismatch | K8s ↔ CI/CD | ❌ | ✅ Con linter de K8s | Cross-reference YAML validation |

**Solo 1 de 6 fallos (F3) se podría haber detectado con tests unitarios convencionales.** Los otros 5 requieren validación de infraestructura que no estaba en el scope de ninguna auditoría.

---

## ✅ Recomendaciones: Auditorías Faltantes

### 🔴 P0 — Auditoría de Infraestructura y Deployment (URGENTE)

Esta es la auditoría que faltó y habría prevenido los 6 fallos:

| Área | Qué auditar |
|------|-------------|
| **Dockerfiles** | Multi-stage builds, base images, cacheo de layers, puertos, healthchecks |
| **CI/CD Workflows** | Build triggers, cache strategy, image naming, push conditions, deploy gates |
| **K8s Manifests** | Image names vs CI/CD, resource limits, probes, secrets, PVCs |
| **DI Startup** | Integration tests con `WebApplicationFactory` que validen que el DI container resuelve todos los servicios |
| **RabbitMQ Topology** | Queue arguments, exchange bindings, DLX config, migration strategy |
| **Secrets Management** | Rotación de tokens, tipos de tokens (ephemeral vs long-lived), expiración |
| **Smoke Tests** | Script que haga `curl /health` a cada servicio después del deploy |
| **Image Consistency** | Validar que `deployments.yaml` referencia las mismas imágenes que CI/CD construye |

### 🟡 P1 — Validación Automatizada en CI/CD

```yaml
# Propuesta: Job de validación pre-deploy
validate-manifests:
  steps:
    # 1. Verificar que image names en K8s coinciden con CI/CD
    - name: Cross-reference image names
      run: |
        for svc in frontend-web gateway authservice ...; do
          K8S_IMAGE=$(grep "image:.*$svc" k8s/deployments.yaml)
          CI_IMAGE=$(grep "tags:.*$svc" .github/workflows/*.yml)
          # Compare and fail if mismatch
        done
    
    # 2. Verificar DI startup de cada servicio
    - name: DI smoke test
      run: dotnet test --filter "Category=Startup"
    
    # 3. Verificar que Dockerfiles buildean
    - name: Docker build validation
      run: docker build --target runner -t test ./backend/AuthService
```

### 🟢 P2 — Startup Integration Tests

```csharp
// Test que habría detectado Fallo #3 (IDeadLetterQueue DI crash)
[Fact]
[Trait("Category", "Startup")]
public async Task Application_Starts_Successfully()
{
    await using var app = new WebApplicationFactory<Program>();
    using var client = app.CreateClient();
    var response = await client.GetAsync("/health");
    response.EnsureSuccessStatusCode();
}
```

---

## 📈 Lección Aprendida

> **"Un sistema no está listo para producción cuando el código está bien escrito.
> Está listo cuando el código llega a producción de forma confiable y repetible."**

Las auditorías existentes aseguran que el software es de calidad. Lo que faltó es asegurar que el **camino del código al usuario** (build → push → deploy → run) funciona sin fricción.

### Distribución ideal de auditorías para un proyecto con 86 microservicios:

```
Actual:                              Ideal:
Código: ██████████████ 67%           Código: ████████ 40%
IA/ML:  ████           27%           IA/ML:  ████     20%
Infra:  ░               0%    →      Infra:  ████     20%  ← NUEVA
DevOps:                  0%           DevOps: ███      15%  ← NUEVA
E2E:                     0%           E2E:    █         5%  ← NUEVA
```

---

## 📊 Resumen Final

| Métrica | Valor |
|---------|-------|
| Total de auditorías realizadas | 15 |
| Auditorías que cubrían infraestructura | **0** |
| Fallos en el deploy | **6** |
| Fallos prevenibles con auditoría de infra | **6 (100%)** |
| Tiempo total de resolución | ~6 horas |
| Costo de oportunidad | Alto (downtime en staging) |
| Lección | Auditar código ≠ Auditar deployment |

---

_Post-mortem generado el 18 de febrero de 2026_  
_Proyecto OKLA — Deploy Staging DOKS_
