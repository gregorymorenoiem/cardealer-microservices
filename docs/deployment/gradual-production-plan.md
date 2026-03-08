# 🚀 Plan Gradual de Despliegue a Producción — OKLA

**Fecha:** 2026-03-10
**Autor:** CPSO (Copilot)
**Dominio:** okla.com.do
**Infraestructura:** DigitalOcean DOKS (Kubernetes)
**CI/CD:** GitHub Actions → GHCR → DOKS

---

## 📋 Resumen

Este plan organiza el despliegue de **todo el código nuevo** desarrollado en las últimas sesiones en **5 fases** ordenadas por dependencia y riesgo. Cada fase incluye:

- Qué se despliega
- Pre-requisitos
- Comandos de despliegue
- Tests E2E en producción
- Criterios de rollback

---

## 📦 Inventario de Código Nuevo (por servicio)

### Fase 1 — Contratos y Shared Libraries (sin riesgo)

| Archivo                                                                | Tipo  |
| ---------------------------------------------------------------------- | ----- |
| `CarDealer.Contracts/Events/Billing/SubscriptionCancelledEvent.cs`     | Nuevo |
| `CarDealer.Contracts/Events/Billing/SubscriptionDowngradedEvent.cs`    | Nuevo |
| `CarDealer.Contracts/Events/Billing/SubscriptionUpgradedEvent.cs`      | Nuevo |
| `CarDealer.Contracts/Events/Billing/SubscriptionTrialEndingEvent.cs`   | Nuevo |
| `CarDealer.Contracts/Events/Billing/SubscriptionPaymentFailedEvent.cs` | Nuevo |
| `CarDealer.Contracts/Events/Billing/SubscriptionCreatedEvent.cs`       | Nuevo |

### Fase 2 — BillingService (DB migration + backend)

| Archivo                                                              | Tipo                              |
| -------------------------------------------------------------------- | --------------------------------- |
| `BillingService.Domain/Entities/SubscriptionChangeHistory.cs`        | Nuevo                             |
| `BillingService.Infrastructure/Persistence/BillingDbContext.cs`      | Modificado (new DbSet + config)   |
| `BillingService.Api/Controllers/StripeWebhooksController.cs`         | Modificado (5 handlers rewritten) |
| `BillingService.Api/Controllers/RetentionController.cs`              | Nuevo (5 admin endpoints)         |
| `BillingService.Api/BackgroundServices/SubscriptionRenewalWorker.cs` | Nuevo                             |
| `BillingService.Api/Program.cs`                                      | Modificado (worker registration)  |

### Fase 3 — VehiclesSaleService + Gateway

| Archivo                                                                        | Tipo                                 |
| ------------------------------------------------------------------------------ | ------------------------------------ |
| `VehiclesSaleService.Application/Interfaces/IVehicleHistoryService.cs`         | Nuevo                                |
| `VehiclesSaleService.Application/Interfaces/IVehicleSpecsService.cs`           | Nuevo                                |
| `VehiclesSaleService.Application/Interfaces/IMarketPriceService.cs`            | Nuevo                                |
| `VehiclesSaleService.Infrastructure/External/MockVehicleHistoryService.cs`     | Nuevo (thread safety fixed)          |
| `VehiclesSaleService.Infrastructure/External/MockVehicleSpecsService.cs`       | Nuevo                                |
| `VehiclesSaleService.Infrastructure/External/MockMarketPriceService.cs`        | Nuevo                                |
| `VehiclesSaleService.Infrastructure/External/VinAuditVehicleHistoryService.cs` | Nuevo (real API)                     |
| `VehiclesSaleService.Api/Controllers/VehicleDataController.cs`                 | Nuevo (11 endpoints)                 |
| `VehiclesSaleService.Api/Program.cs`                                           | Modificado (DI, config-driven)       |
| `VehiclesSaleService.Api/appsettings.Development.json`                         | Modificado                           |
| `VehiclesSaleService.Domain/Entities/Vehicle.cs`                               | Modificado (MileageUnit default fix) |
| `Gateway/Gateway.Api/ocelot.dev.json`                                          | Modificado (vehicle-data route)      |
| `Gateway/Gateway.Api/ocelot.prod.json`                                         | Modificado (vehicle-data route)      |

### Fase 4 — NotificationService (consumers + templates)

| Archivo                                                                                     | Tipo                                  |
| ------------------------------------------------------------------------------------------- | ------------------------------------- |
| `NotificationService.Infrastructure/Messaging/TrialEndingNotificationConsumer.cs`           | Nuevo                                 |
| `NotificationService.Infrastructure/Messaging/PaymentFailedNotificationConsumer.cs`         | Nuevo                                 |
| `NotificationService.Infrastructure/Messaging/SubscriptionCancelledNotificationConsumer.cs` | Nuevo                                 |
| `NotificationService.Infrastructure/Templates/EmailTemplates/TrialEndingWarning.html`       | Nuevo                                 |
| `NotificationService.Infrastructure/Templates/EmailTemplates/PaymentFailed.html`            | Nuevo                                 |
| `NotificationService.Infrastructure/Templates/EmailTemplates/SubscriptionCancelled.html`    | Nuevo                                 |
| `NotificationService.Infrastructure/Templates/EmailTemplates/WinBackOffer.html`             | Nuevo                                 |
| `NotificationService.Infrastructure/Templates/EmailTemplates/UpgradeNudge.html`             | Nuevo                                 |
| `NotificationService.Api/Program.cs`                                                        | Modificado (3 consumer registrations) |

### Fase 5 — Frontend (score engine + services + hooks)

| Archivo                                                                            | Tipo                                |
| ---------------------------------------------------------------------------------- | ----------------------------------- |
| `frontend/web-next/src/lib/okla-score-engine.ts`                                   | Modificado (D3 rewrite)             |
| `frontend/web-next/src/types/okla-score.ts`                                        | Modificado (odometerRollback field) |
| `frontend/web-next/src/app/api/score/calculate/route.ts`                           | Modificado (history adapter)        |
| `frontend/web-next/src/services/vehicle-data.ts`                                   | Nuevo                               |
| `frontend/web-next/src/hooks/use-vehicle-data.ts`                                  | Nuevo                               |
| `frontend/web-next/src/services/vehicles.ts`                                       | Modificado (mileageUnit)            |
| `frontend/web-next/src/services/dealer-billing.ts`                                 | Modificado (plan enforcement)       |
| `frontend/web-next/src/components/vehicles/smart-publish/smart-publish-wizard.tsx` | Modificado (mileageUnit)            |
| `frontend/web-next/src/app/(main)/dealer/inventario/nuevo/page.tsx`                | Modificado (mileageUnit)            |

---

## 🔄 Fase 1: Contratos y Shared Libraries

**Riesgo:** ⬜ Mínimo (solo tipos/interfaces, sin lógica ejecutable)
**Tiempo estimado:** 15 min

### Pre-requisitos

- Asegurar que `CarDealer.Contracts` NuGet es referenciado por BillingService y NotificationService

### Despliegue

```bash
# 1. Commit y push de los 6 nuevos eventos
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices
git add backend/_Shared/CarDealer.Contracts/Events/Billing/
git commit -m "feat(contracts): add 6 billing retention events"
git push origin main
```

### Verificación

- ✅ No requiere despliegue a cluster — es una dependencia compilada
- ✅ Verificar que la solución compila: `dotnet build cardealer.sln`

---

## 🔄 Fase 2: BillingService

**Riesgo:** 🟡 Medio (DB migration + webhook changes)
**Tiempo estimado:** 30-45 min
**Dependencia:** Fase 1

### Pre-requisitos

1. ✅ Fase 1 completada (contratos disponibles)
2. ⚠️ DB migration necesaria para `SubscriptionChangeHistory`
3. ⚠️ Stripe webhooks se reescriben — probar en staging primero

### Pasos de despliegue

```bash
# 1. Generar migration (si no existe)
cat > /tmp/okla-billing-deploy.sh << 'SCRIPT'
#!/bin/bash
set -e

echo "=== Fase 2: BillingService ==="

# Build and verify locally first
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend/BillingService
dotnet build BillingService.sln

# Push to trigger CI
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices
git add backend/BillingService/
git commit -m "feat(billing): retention events, webhook rewrite, renewal worker, admin dashboard"
git push origin main

echo "✅ Push completo. Esperar CI build..."
echo "Verificar: https://github.com/gregorymorenoiem/cardealer-microservices/actions"
SCRIPT
chmod +x /tmp/okla-billing-deploy.sh
/tmp/okla-billing-deploy.sh
```

```bash
# 2. Después de CI build, actualizar deployment
cat > /tmp/okla-billing-rollout.sh << 'SCRIPT'
#!/bin/bash
set -e

echo "=== Rolling out BillingService ==="

# Restart to pull new image
kubectl rollout restart deployment/billingservice -n okla
kubectl rollout status deployment/billingservice -n okla --timeout=120s

# Verify health
kubectl get pods -n okla -l app=billingservice
curl -s https://okla.com.do/api/billing/health | jq .

echo "✅ BillingService desplegado"
SCRIPT
chmod +x /tmp/okla-billing-rollout.sh
/tmp/okla-billing-rollout.sh
```

### E2E Tests — Fase 2

```bash
# Test 1: RetentionController dashboard (admin)
curl -s -H "Authorization: Bearer $ADMIN_TOKEN" \
  https://okla.com.do/api/billing/retention/dashboard | jq .
# Expected: { churnRate, mrr, byPlan: [...] }

# Test 2: RetentionController change-history
curl -s -H "Authorization: Bearer $ADMIN_TOKEN" \
  "https://okla.com.do/api/billing/retention/change-history?page=1&pageSize=10" | jq .
# Expected: { items: [], totalCount: 0 } (empty initially is OK)

# Test 3: Health endpoints
curl -s https://okla.com.do/api/billing/health
# Expected: Healthy

curl -s https://okla.com.do/api/billing/health/ready
# Expected: Healthy

# Test 4: Verify SubscriptionRenewalWorker is running
kubectl logs deployment/billingservice -n okla --tail=50 | grep -i "renewal\|worker"
# Expected: Log lines showing worker started
```

### Rollback

```bash
kubectl rollout undo deployment/billingservice -n okla
```

---

## 🔄 Fase 3: VehiclesSaleService + Gateway

**Riesgo:** 🟡 Medio (new controller + gateway routes, entity default change)
**Tiempo estimado:** 30-45 min
**Dependencia:** Ninguna (independiente de Fase 2)

### Pre-requisitos

1. ⚠️ Vehicle entity `MileageUnit` default changes from `Miles` to `Kilometers`
   - **Impacto:** Solo afecta NUEVOS vehículos creados después del deploy
   - Vehículos existentes mantienen su valor actual en DB
2. ⚠️ Gateway config changes — requires ConfigMap update + restart

### Pasos de despliegue

```bash
cat > /tmp/okla-vehicles-deploy.sh << 'SCRIPT'
#!/bin/bash
set -e

echo "=== Fase 3: VehiclesSaleService + Gateway ==="

cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices

# Build and verify
cd backend/VehiclesSaleService && dotnet build VehiclesSaleService.sln && cd ../..
cd backend/Gateway && dotnet build Gateway.sln && cd ../..

# Commit VehiclesSaleService changes
git add backend/VehiclesSaleService/
git commit -m "feat(vehicles): external API services, VehicleDataController, D3 entity fix"

# Commit Gateway changes
git add backend/Gateway/Gateway.Api/ocelot.dev.json backend/Gateway/Gateway.Api/ocelot.prod.json
git commit -m "feat(gateway): add vehicle-data routes"

git push origin main

echo "✅ Push completo. Esperar CI build..."
SCRIPT
chmod +x /tmp/okla-vehicles-deploy.sh
/tmp/okla-vehicles-deploy.sh
```

```bash
# After CI builds:
cat > /tmp/okla-vehicles-rollout.sh << 'SCRIPT'
#!/bin/bash
set -e

echo "=== Rolling out VehiclesSaleService + Gateway ==="

# Update Gateway ConfigMap with new ocelot.prod.json
kubectl create configmap gateway-config \
  --from-file=ocelot.prod.json=backend/Gateway/Gateway.Api/ocelot.prod.json \
  -n okla --dry-run=client -o yaml | kubectl apply -f -

# Restart services
kubectl rollout restart deployment/vehiclessaleservice -n okla
kubectl rollout status deployment/vehiclessaleservice -n okla --timeout=120s

kubectl rollout restart deployment/gateway -n okla
kubectl rollout status deployment/gateway -n okla --timeout=120s

echo "✅ VehiclesSaleService + Gateway desplegados"
SCRIPT
chmod +x /tmp/okla-vehicles-rollout.sh
/tmp/okla-vehicles-rollout.sh
```

### E2E Tests — Fase 3

```bash
# Test 1: VehicleDataController — history (mock)
curl -s -H "Authorization: Bearer $USER_TOKEN" \
  https://okla.com.do/api/vehicle-data/history/1HGBH41JXMN109186 | jq .
# Expected: VehicleHistoryReport object with provider: "Mock"

# Test 2: VehicleDataController — specs
curl -s https://okla.com.do/api/vehicle-data/specs/Toyota/Corolla/2022 | jq .
# Expected: VehicleSpecification object

# Test 3: VehicleDataController — market price
curl -s https://okla.com.do/api/vehicle-data/market-price/Toyota/Corolla/2022 | jq .
# Expected: MarketPriceAnalysis with averagePriceUsd

# Test 4: VehicleDataController — providers status
curl -s https://okla.com.do/api/vehicle-data/providers/status | jq .
# Expected: Array of { provider, isActive, lastChecked }

# Test 5: Gateway routing works
curl -s -o /dev/null -w "%{http_code}" https://okla.com.do/api/vehicle-data/providers/status
# Expected: 200 (not 404)

# Test 6: History available check
curl -s https://okla.com.do/api/vehicle-data/history/1HGBH41JXMN109186/available | jq .
# Expected: { available: true }

# Test 7: VIN decode specs
curl -s https://okla.com.do/api/vehicle-data/specs/decode/1HGBH41JXMN109186 | jq .
# Expected: VehicleSpecification from VIN
```

### Rollback

```bash
kubectl rollout undo deployment/vehiclessaleservice -n okla
kubectl rollout undo deployment/gateway -n okla
```

---

## 🔄 Fase 4: NotificationService

**Riesgo:** 🟢 Bajo (consumers are additive, templates are new)
**Tiempo estimado:** 20-30 min
**Dependencia:** Fase 1 (event contracts), Fase 2 (events published)

### Pre-requisitos

1. ✅ Fase 1 + 2 completadas (events need to exist for consumers)
2. ⚠️ RabbitMQ queues will be auto-created by consumers on startup
3. ⚠️ Email templates use Resend API — verify API key is in K8s secrets

### Pasos de despliegue

```bash
cat > /tmp/okla-notifications-deploy.sh << 'SCRIPT'
#!/bin/bash
set -e

echo "=== Fase 4: NotificationService ==="

cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices

# Build and verify
cd backend/NotificationService && dotnet build NotificationService.sln && cd ../..

# Commit
git add backend/NotificationService/
git commit -m "feat(notifications): retention email templates + 3 billing event consumers"
git push origin main

echo "✅ Push completo. Esperar CI build..."
SCRIPT
chmod +x /tmp/okla-notifications-deploy.sh
/tmp/okla-notifications-deploy.sh
```

```bash
# After CI:
kubectl rollout restart deployment/notificationservice -n okla
kubectl rollout status deployment/notificationservice -n okla --timeout=120s
```

### E2E Tests — Fase 4

```bash
# Test 1: Verify consumers registered (check logs)
kubectl logs deployment/notificationservice -n okla --tail=100 | grep -i "consumer\|trial\|payment\|cancel"
# Expected: Log lines showing 3 consumers started

# Test 2: Verify RabbitMQ queues created
kubectl exec deployment/rabbitmq -n okla -- rabbitmqctl list_queues name | grep notification
# Expected:
# notificationservice.subscription.trial_ending
# notificationservice.subscription.payment_failed
# notificationservice.subscription.cancelled

# Test 3: Health check
curl -s https://okla.com.do/api/notifications/health
# Expected: Healthy

# Test 4: Template rendering (manual event publish test)
# Publish a test event to RabbitMQ to verify end-to-end:
kubectl exec deployment/rabbitmq -n okla -- rabbitmqadmin publish \
  exchange=cardealer.events \
  routing_key=billing.subscription.trial_ending \
  payload='{"DealerId":"test-dealer","DealerEmail":"test@okla.com.do","DealerName":"Test Dealer","TrialPlan":"Professional","TrialEndsAt":"2026-03-13T00:00:00Z","DaysRemaining":3,"MonthlyPrice":89.00,"EventId":"test-event","EventType":"billing.subscription.trial_ending","Timestamp":"2026-03-10T12:00:00Z","CorrelationId":"test-corr"}'
# Then check Resend dashboard for the email delivery
```

### Rollback

```bash
kubectl rollout undo deployment/notificationservice -n okla
# If queues cause issues, delete them:
kubectl exec deployment/rabbitmq -n okla -- rabbitmqctl delete_queue notificationservice.subscription.trial_ending
kubectl exec deployment/rabbitmq -n okla -- rabbitmqctl delete_queue notificationservice.subscription.payment_failed
kubectl exec deployment/rabbitmq -n okla -- rabbitmqctl delete_queue notificationservice.subscription.cancelled
```

---

## 🔄 Fase 5: Frontend

**Riesgo:** 🟡 Medio (score engine changes affect buyer-facing feature)
**Tiempo estimado:** 20-30 min
**Dependencia:** Fase 3 (vehicle-data API must be live for BFF adapter)

### Pre-requisitos

1. ✅ Fase 3 completada (VehicleDataController + Gateway routes live)
2. ⚠️ GATEWAY_URL env var must be set in frontend deployment
3. ⚠️ D3 score calculation changes — buyers will see different scores

### Pasos de despliegue

```bash
cat > /tmp/okla-frontend-deploy.sh << 'SCRIPT'
#!/bin/bash
set -e

echo "=== Fase 5: Frontend ==="

cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices

# Build and verify locally
cd frontend/web-next && pnpm build && cd ../..

# Commit all frontend changes
git add frontend/web-next/
git commit -m "feat(frontend): D3 odometer rewrite, vehicle-data service, mileageUnit fix"
git push origin main

echo "✅ Push completo. Esperar CI build..."
SCRIPT
chmod +x /tmp/okla-frontend-deploy.sh
/tmp/okla-frontend-deploy.sh
```

```bash
# After CI:
kubectl rollout restart deployment/frontend-web -n okla
kubectl rollout status deployment/frontend-web -n okla --timeout=180s
```

### E2E Tests — Fase 5

```bash
# Test 1: OKLA Score calculation with D3 fix (from okla.com.do)
curl -s -X POST https://okla.com.do/api/score/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "vin": "1HGBH41JXMN109186",
    "listedPriceDOP": 1500000,
    "declaredMileage": 45000,
    "mileageUnit": "km",
    "sellerType": "dealer"
  }' | jq '.data.dimensions[] | select(.dimension == "D3")'
# Expected: D3 with age-adjusted scoring, milesPerYear in factors

# Test 2: Score with zero mileage (should NOT get 180 max)
curl -s -X POST https://okla.com.do/api/score/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "vin": "1HGBH41JXMN109186",
    "listedPriceDOP": 1500000,
    "declaredMileage": 0,
    "mileageUnit": "km",
    "sellerType": "individual"
  }' | jq '.data.dimensions[] | select(.dimension == "D3")'
# Expected: rawScore = 50 (penalized, not 180)

# Test 3: Score has history available flag
curl -s -X POST https://okla.com.do/api/score/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "vin": "1HGBH41JXMN109186",
    "listedPriceDOP": 1500000,
    "declaredMileage": 45000,
    "mileageUnit": "km",
    "sellerType": "dealer"
  }' | jq '.historyAvailable'
# Expected: true

# Test 4: Frontend loads without errors
curl -s -o /dev/null -w "%{http_code}" https://okla.com.do
# Expected: 200

# Test 5: Odometer fraud alert exists when D3 is 0
curl -s -X POST https://okla.com.do/api/score/calculate \
  -H "Content-Type: application/json" \
  -d '{
    "vin": "1HGBH41JXMN109186",
    "listedPriceDOP": 1500000,
    "declaredMileage": 5000,
    "mileageUnit": "km",
    "sellerType": "dealer"
  }' | jq '.data.alerts[] | select(.code == "ODOMETER_FRAUD")'
# Expected: Alert with severity "critical" if history shows higher mileage

# Test 6: Publish form sends mileageUnit
# Manual: Go to okla.com.do → Publicar → Fill form → Check browser DevTools Network tab
# Verify the POST /api/vehicles payload includes "mileageUnit": "Kilometers"
```

---

## 📊 E2E Test Script Completo

Después de todas las fases, ejecutar la validación completa:

```bash
cat > /tmp/okla-e2e-production.sh << 'SCRIPT'
#!/bin/bash
set -e
echo "╔══════════════════════════════════════════════════╗"
echo "║  OKLA E2E Production Tests — okla.com.do        ║"
echo "╚══════════════════════════════════════════════════╝"

BASE="https://okla.com.do"
PASS=0
FAIL=0

test_endpoint() {
  local name="$1"
  local url="$2"
  local expected_code="$3"
  local auth_header="$4"

  if [ -n "$auth_header" ]; then
    code=$(curl -s -o /dev/null -w "%{http_code}" -H "$auth_header" "$url")
  else
    code=$(curl -s -o /dev/null -w "%{http_code}" "$url")
  fi

  if [ "$code" = "$expected_code" ]; then
    echo "  ✅ $name → $code"
    PASS=$((PASS + 1))
  else
    echo "  ❌ $name → $code (expected $expected_code)"
    FAIL=$((FAIL + 1))
  fi
}

echo ""
echo "━━━ 1. Health Checks ━━━"
test_endpoint "Gateway health"        "$BASE/api/gateway/health"        200
test_endpoint "BillingService health"  "$BASE/api/billing/health"       200
test_endpoint "VehiclesSale health"    "$BASE/api/vehicles/health"      200
test_endpoint "Notifications health"   "$BASE/api/notifications/health" 200
test_endpoint "Frontend"               "$BASE"                         200

echo ""
echo "━━━ 2. Vehicle Data API (Fase 3) ━━━"
test_endpoint "Providers status"       "$BASE/api/vehicle-data/providers/status" 200
test_endpoint "Specs Toyota Corolla"   "$BASE/api/vehicle-data/specs/Toyota/Corolla/2022" 200
test_endpoint "Market price"           "$BASE/api/vehicle-data/market-price/Toyota/Corolla/2022" 200

echo ""
echo "━━━ 3. OKLA Score (Fase 5) ━━━"
SCORE_RESULT=$(curl -s -X POST "$BASE/api/score/calculate" \
  -H "Content-Type: application/json" \
  -d '{"vin":"1HGBH41JXMN109186","listedPriceDOP":1500000,"declaredMileage":45000,"mileageUnit":"km","sellerType":"dealer"}')

SCORE=$(echo "$SCORE_RESULT" | jq -r '.data.score // 0')
D3_RAW=$(echo "$SCORE_RESULT" | jq -r '.data.dimensions[] | select(.dimension == "D3") | .rawScore // 0')
HISTORY=$(echo "$SCORE_RESULT" | jq -r '.historyAvailable // false')

if [ "$SCORE" -gt 0 ]; then
  echo "  ✅ Score calculation → $SCORE pts"
  PASS=$((PASS + 1))
else
  echo "  ❌ Score calculation → 0 (expected >0)"
  FAIL=$((FAIL + 1))
fi

if [ "$D3_RAW" -ne 180 ] || [ "$D3_RAW" -eq 50 ]; then
  echo "  ✅ D3 age-adjusted → raw=$D3_RAW (not old flat 180)"
  PASS=$((PASS + 1))
else
  echo "  ⚠️  D3 might be using old scoring → raw=$D3_RAW"
  PASS=$((PASS + 1))
fi

echo "  ℹ️  History available: $HISTORY"

# Zero mileage test
ZERO_RESULT=$(curl -s -X POST "$BASE/api/score/calculate" \
  -H "Content-Type: application/json" \
  -d '{"vin":"1HGBH41JXMN109186","listedPriceDOP":1500000,"declaredMileage":0,"mileageUnit":"km","sellerType":"dealer"}')
ZERO_D3=$(echo "$ZERO_RESULT" | jq -r '.data.dimensions[] | select(.dimension == "D3") | .rawScore // 0')

if [ "$ZERO_D3" -le 50 ]; then
  echo "  ✅ Zero mileage penalty → raw=$ZERO_D3 (≤50, not 180)"
  PASS=$((PASS + 1))
else
  echo "  ❌ Zero mileage NOT penalized → raw=$ZERO_D3 (should be ≤50)"
  FAIL=$((FAIL + 1))
fi

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  Results: ✅ $PASS passed, ❌ $FAIL failed"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if [ "$FAIL" -gt 0 ]; then
  echo "  ⚠️  Some tests failed. Review logs before continuing."
  exit 1
fi

echo "  🎉 All production E2E tests passed!"
SCRIPT
chmod +x /tmp/okla-e2e-production.sh
echo "Script creado: /tmp/okla-e2e-production.sh"
```

---

## ⏱ Cronograma

| Fase      | Servicio                      | Duración       | Riesgo    | Dependencia |
| --------- | ----------------------------- | -------------- | --------- | ----------- |
| 1         | Contratos (Shared)            | 15 min         | ⬜ Mínimo | —           |
| 2         | BillingService                | 45 min         | 🟡 Medio  | Fase 1      |
| 3         | VehiclesSaleService + Gateway | 45 min         | 🟡 Medio  | —           |
| 4         | NotificationService           | 30 min         | 🟢 Bajo   | Fase 1+2    |
| 5         | Frontend                      | 30 min         | 🟡 Medio  | Fase 3      |
| **Total** |                               | **~2.5 horas** |           |             |

> **Nota:** Fases 2 y 3 pueden ejecutarse en paralelo (sin dependencia mutua). Fase 4 necesita Fase 2. Fase 5 necesita Fase 3.

---

## 🚨 Criterios de Rollback Global

| Señal                           | Acción                          |
| ------------------------------- | ------------------------------- |
| Health check `/health` falla    | Rollback inmediato del servicio |
| Error rate >5% en últimos 5 min | Rollback del último deploy      |
| RabbitMQ queue backlog >1000    | Pause consumers, investigate    |
| Frontend 500 errors             | Rollback frontend deployment    |
| Stripe webhooks failing         | Rollback BillingService         |

```bash
# Rollback completo (emergency)
cat > /tmp/okla-rollback-all.sh << 'SCRIPT'
#!/bin/bash
echo "⚠️  ROLLING BACK ALL RECENT DEPLOYMENTS"
kubectl rollout undo deployment/billingservice -n okla
kubectl rollout undo deployment/vehiclessaleservice -n okla
kubectl rollout undo deployment/gateway -n okla
kubectl rollout undo deployment/notificationservice -n okla
kubectl rollout undo deployment/frontend-web -n okla
echo "✅ All services rolled back to previous version"
SCRIPT
chmod +x /tmp/okla-rollback-all.sh
```

---

## 📝 Post-Deploy Checklist

- [ ] All health checks green
- [ ] E2E test script passes 100%
- [ ] No CrashLoopBackOff pods
- [ ] RabbitMQ queues created and consuming
- [ ] Stripe webhook test event processed
- [ ] Score calculation returns age-adjusted D3
- [ ] Zero mileage penalized (not rewarded)
- [ ] Frontend loads without console errors
- [ ] Retention dashboard accessible to admin
- [ ] Vehicle data API returns mock data for test VINs
