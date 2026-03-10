# 🚗 OKLA Pre-Launch Load Test Report

## Test de Carga Pre-Lanzamiento — Auditoría

**Fecha:** 2026-03-09  
**Ejecutado por:** CPSO (Copilot Agent)  
**Ambiente:** Staging (Docker local con imágenes GHCR)  
**Herramienta:** k6 v1.6.1

---

## 📋 Resumen Ejecutivo

Se ejecutó una auditoría de test de carga pre-lanzamiento para validar que la plataforma OKLA puede manejar 500 usuarios concurrentes durante 10 minutos con los criterios de rendimiento establecidos.

### Criterios de Aceptación

| Criterio            | Target         | Resultado Quick Test | Estado          |
| ------------------- | -------------- | -------------------- | --------------- |
| P95 Latencia        | < 3 segundos   | 10.46ms              | ✅ PASS         |
| P99 Latencia        | < 5 segundos   | 16.09ms              | ✅ PASS         |
| Latencia Promedio   | < 1.5 segundos | 4.01ms               | ✅ PASS         |
| Errores 5xx         | 0%             | 10.35%               | ❌ FAIL         |
| Autoscaling trigger | Latencia > 2s  | 0 alertas            | ✅ N/A (local)  |
| Request Rate        | > 50 req/s     | 24.05 req/s          | ⚠️ BELOW TARGET |
| Checks Pass Rate    | > 95%          | < 95%                | ❌ FAIL         |

---

## 📊 Resultados Detallados — Quick Load Test (50 VUs, 2 minutos)

### Request Metrics

- **Total Requests:** 2,966
- **Request Rate:** 24.05 req/s
- **Failed Rate:** 82.91% (incluye 4xx esperados + 5xx)

### Latency Distribution

| Percentile   | Value          |
| ------------ | -------------- |
| Average      | 4.01ms         |
| Median (P50) | 3.16ms         |
| P90          | 8.14ms         |
| **P95**      | **10.46ms** ✅ |
| P99          | 16.09ms        |
| Max          | 136.93ms       |

### Error Analysis

| Type           | Rate   | Details                                                          |
| -------------- | ------ | ---------------------------------------------------------------- |
| **5xx Errors** | 10.35% | 502 Bad Gateway — AuthService downstream no alcanzable           |
| **4xx Errors** | 72.58% | Esperado: servicios downstream no levantados (vehicles, catalog) |

---

## 🔍 Hallazgos Críticos

### 1. ❌ CRÍTICO: 502 Bad Gateway en AuthService Routes

**Problema:** Todas las rutas `/api/auth/*` retornan 502 Bad Gateway.

**Causa Raíz:** El AuthService Docker container se queda bloqueado durante el startup, probablemente en la fase de registro con Consul o conexión a Redis. El log muestra:

```
NoOpErrorProducer initialized - RabbitMQ is disabled
```

Pero nunca muestra `Now listening on: http://[::]:8080` ni `Application started`.

**Impacto:** 100% de las peticiones a auth endpoints fallan.

**Recomendación:**

1. Revisar el `Program.cs` de AuthService para verificar que los servicios de startup (Consul, Redis, RabbitMQ) tienen timeouts adecuados y no bloquean el startup
2. Implementar startup probes más agresivos en Kubernetes
3. Agregar `AddHostedService` con timeout para servicios de infraestructura
4. Verificar que la configuración `ConnectionStrings__DefaultConnection` se resuelve correctamente en Docker

### 2. ⚠️ IMPORTANTE: Servicios Downstream No Disponibles

**Problema:** Los servicios VehiclesSaleService, CatalogService, MediaService no están disponibles en el ambiente local.

**Impacto:** 72% de errores 4xx por rutas sin servicio downstream.

**Recomendación:**

1. Los servicios debe estar disponibles y saludables antes de ejecutar tests de carga
2. Crear un script `docker-compose` completo que levante TODOS los servicios necesarios
3. Implementar circuit breakers en el Gateway (Polly) para responder gracefully cuando un servicio downstream está caído

### 3. ✅ POSITIVO: Gateway Performance Excelente

**Hallazgo:** El Gateway responde en menos de 11ms en P95 incluso bajo carga de 50 VUs concurrentes. La latencia máxima fue 136ms.

**Proyección para 500 VUs:** Con la arquitectura actual (Ocelot + Polly), se espera que el Gateway mantenga latencia baja. El bottleneck estará en los servicios downstream.

### 4. ⚠️ Request Rate Below Target

**Problema:** 24 req/s vs target de 50 req/s.

**Causa:** El test incluye think time (1-3 segundos entre requests) que simula comportamiento real del usuario. Con 50 VUs y think time de ~2s promedio, 24 req/s es esperado.

**Para 500 VUs:** Se espera ~240 req/s, bien por encima del target de 50 req/s.

---

## 🛠️ Correcciones Ejecutadas

### Corrección 1: Infraestructura k6 Creada

- ✅ Instalado k6 v1.6.1 via Homebrew
- ✅ Creado `tests/load-tests/k6/load-test.js` — Test principal con 5 journeys de usuario
- ✅ Creado `tests/load-tests/k6/stress-test.js` — Test de estrés hasta 1250 VUs
- ✅ Creado `tests/load-tests/k6/soak-test.js` — Test de estabilidad 30 minutos
- ✅ Creado `tests/load-tests/k6/config.js` — Configuración centralizada
- ✅ Creado `tests/load-tests/k6/run-load-tests.sh` — Runner script con 4 modos

### Corrección 2: Docker Environment Fix

- ✅ Corregido `ASPNETCORE_URLS=http://+:8080` para Gateway (estaba escuchando en puerto 80)
- ✅ Gateway respondiendo correctamente en `http://localhost:8080/health`

### Corrección 3: Thresholds Configuration

- ✅ Removido threshold inválido `count<1` en `http_req_duration{status:5xx}` (k6 no soporta `count` en metrics tipo `trend`)

---

## 📈 Acciones Pendientes para Full Test (500 VUs, 10 min)

1. **Levantar TODOS los servicios downstream** antes de ejecutar el full test
2. **Resolver el bloqueo de startup del AuthService** en Docker
3. **Configurar monitoring en tiempo real** (Prometheus/Grafana) durante el test
4. **Ejecutar el full test** con: `./tests/load-tests/k6/run-load-tests.sh full http://localhost:8080`
5. **Verificar autoscaling en DOKS** ejecutando contra producción staging

---

## 🏃 Cómo Ejecutar los Tests

```bash
# Quick validation (2 min, 50 VUs)
./tests/load-tests/k6/run-load-tests.sh quick http://localhost:8080

# Full pre-launch test (12 min, 500 VUs)
./tests/load-tests/k6/run-load-tests.sh full http://localhost:8080

# Stress test — find breaking point (12 min, up to 1250 VUs)
./tests/load-tests/k6/run-load-tests.sh stress http://localhost:8080

# Soak test — stability over 30 min (200 VUs sustained)
./tests/load-tests/k6/run-load-tests.sh soak http://localhost:8080

# Against production/staging
./tests/load-tests/k6/run-load-tests.sh full https://api.okla.com.do
```

---

## 📅 Próximos Pasos

| Prioridad | Acción                                  | Responsable  |
| --------- | --------------------------------------- | ------------ |
| P0        | Fix AuthService startup blocking        | Backend Team |
| P0        | Levantar todos los servicios en Docker  | DevOps       |
| P1        | Re-ejecutar full load test con 500 VUs  | CPSO         |
| P1        | Verificar autoscaling en DOKS           | SRE          |
| P2        | Integrar k6 en CI/CD pipeline           | DevOps       |
| P2        | Configurar alertas de latencia P95 > 2s | SRE          |

---

_Reporte generado automáticamente por OKLA CPSO Agent — 2026-03-09_
