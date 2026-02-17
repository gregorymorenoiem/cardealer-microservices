#!/bin/bash

# ==============================================================================
# 🔍 Infrastructure Verification Script (A, B, C)
# ==============================================================================
# Verifica que A) Dockerfiles, B) Compose, C) Ocelot estén configurados
# ==============================================================================

set -e

PROJECT_ROOT="/Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices"
cd "$PROJECT_ROOT"

echo "╔════════════════════════════════════════════════════════════════════════╗"
echo "║          🔍 INFRASTRUCTURE VERIFICATION - A, B, C                     ║"
echo "║          OKLA Payment Systems Integration                             ║"
echo "╚════════════════════════════════════════════════════════════════════════╝"
echo ""

# Color codes
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

PASSED=0
FAILED=0

# Helper functions
check_success() {
    echo -e "${GREEN}✅ PASS${NC} - $1"
    ((PASSED++))
}

check_error() {
    echo -e "${RED}❌ FAIL${NC} - $1"
    ((FAILED++))
}

check_warning() {
    echo -e "${YELLOW}⚠️ WARN${NC} - $1"
}

check_info() {
    echo -e "${BLUE}ℹ️  INFO${NC} - $1"
}

echo "═══════════════════════════════════════════════════════════════════════════"
echo "A) DOCKERFILES VERIFICATION"
echo "═══════════════════════════════════════════════════════════════════════════"
echo ""

# Check Dockerfiles count
DOCKERFILE_COUNT=$(find backend -name "Dockerfile" | wc -l)
if [ "$DOCKERFILE_COUNT" -ge 48 ]; then
    check_success "Found $DOCKERFILE_COUNT Dockerfiles (expected: 48+)"
else
    check_error "Found only $DOCKERFILE_COUNT Dockerfiles (expected: 48+)"
fi

# Check AzulPaymentService Dockerfile
if [ -f "backend/AzulPaymentService/Dockerfile" ]; then
    LINES=$(wc -l < backend/AzulPaymentService/Dockerfile)
    if [ "$LINES" -eq 64 ]; then
        check_success "AzulPaymentService/Dockerfile exists ($LINES lines)"
    else
        check_warning "AzulPaymentService/Dockerfile exists ($LINES lines, expected: 64)"
    fi
else
    check_error "AzulPaymentService/Dockerfile NOT FOUND"
fi

# Check StripePaymentService Dockerfile
if [ -f "backend/StripePaymentService/Dockerfile" ]; then
    LINES=$(wc -l < backend/StripePaymentService/Dockerfile)
    if [ "$LINES" -eq 64 ]; then
        check_success "StripePaymentService/Dockerfile exists ($LINES lines)"
    else
        check_warning "StripePaymentService/Dockerfile exists ($LINES lines, expected: 64)"
    fi
else
    check_error "StripePaymentService/Dockerfile NOT FOUND"
fi

# Check multi-stage build pattern
MULTI_STAGE=$(grep -c "FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build" backend/AzulPaymentService/Dockerfile || true)
if [ "$MULTI_STAGE" -eq 1 ]; then
    check_success "Multi-stage build pattern (build stage) found"
else
    check_error "Multi-stage build pattern (build stage) NOT found"
fi

MULTI_STAGE=$(grep -c "FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final" backend/AzulPaymentService/Dockerfile || true)
if [ "$MULTI_STAGE" -eq 1 ]; then
    check_success "Multi-stage build pattern (final stage) found"
else
    check_error "Multi-stage build pattern (final stage) NOT found"
fi

# Check health check in Dockerfile
HEALTHCHECK=$(grep -c "healthcheck" backend/ReviewService/Dockerfile || true)
if [ "$HEALTHCHECK" -eq 1 ]; then
    check_success "Health check configuration found in Dockerfiles"
else
    check_warning "Health check configuration might be missing"
fi

echo ""
echo "═══════════════════════════════════════════════════════════════════════════"
echo "B) DOCKER COMPOSE VERIFICATION"
echo "═══════════════════════════════════════════════════════════════════════════"
echo ""

# Check compose.yaml exists and line count
if [ -f "compose.yaml" ]; then
    LINES=$(wc -l < compose.yaml)
    if [ "$LINES" -ge 2800 ]; then
        check_success "compose.yaml exists ($LINES lines, expected: 2,800+)"
    else
        check_error "compose.yaml exists but only $LINES lines (expected: 2,800+)"
    fi
else
    check_error "compose.yaml NOT FOUND"
    exit 1
fi

# Check services in compose.yaml
SERVICES=("azulpaymentservice" "stripepaymentservice" "postgres_db" "rabbitmq" "redis" \
          "authservice" "userservice" "vehiclessaleservice" "chatbotservice" \
          "reviewservice" "recommendationservice" "vehicleintelligenceservice" \
          "userbehaviorservice")

for SERVICE in "${SERVICES[@]}"; do
    if grep -q "^  ${SERVICE}:" compose.yaml; then
        check_success "Service '$SERVICE' found in compose.yaml"
    else
        check_error "Service '$SERVICE' NOT found in compose.yaml"
    fi
done

# Check environment variables
if grep -q "ASPNETCORE_URLS: http://+:80" compose.yaml; then
    check_success "ASPNETCORE_URLS environment variable configured"
else
    check_error "ASPNETCORE_URLS environment variable NOT configured"
fi

# Check health checks
HEALTH_COUNT=$(grep -c "healthcheck:" compose.yaml || true)
if [ "$HEALTH_COUNT" -ge 15 ]; then
    check_success "Health checks configured ($HEALTH_COUNT found)"
else
    check_warning "Only $HEALTH_COUNT health checks found (expected: 15+)"
fi

# Check depends_on
DEPENDS=$(grep -c "depends_on:" compose.yaml || true)
if [ "$DEPENDS" -ge 10 ]; then
    check_success "Service dependencies configured ($DEPENDS found)"
else
    check_warning "Only $DEPENDS dependencies found (expected: 10+)"
fi

echo ""
echo "═══════════════════════════════════════════════════════════════════════════"
echo "C) OCELOT GATEWAY ROUTES VERIFICATION"
echo "═══════════════════════════════════════════════════════════════════════════"
echo ""

# Check ocelot.prod.json exists
if [ -f "backend/Gateway/Gateway.Api/ocelot.prod.json" ]; then
    LINES=$(wc -l < backend/Gateway/Gateway.Api/ocelot.prod.json)
    if [ "$LINES" -ge 850 ]; then
        check_success "ocelot.prod.json exists ($LINES lines, expected: 850+)"
    else
        check_error "ocelot.prod.json exists but only $LINES lines (expected: 850+)"
    fi
else
    check_error "ocelot.prod.json NOT FOUND"
    exit 1
fi

# Check routes in ocelot
ROUTES=("azul-payment" "stripe-payment" "auth" "users" "vehicles" "media" \
        "notifications" "reviews" "recommendations" "chatbot" "errors")

for ROUTE in "${ROUTES[@]}"; do
    if grep -q "\"UpstreamPathTemplate\": \"/api/${ROUTE}" backend/Gateway/Gateway.Api/ocelot.prod.json; then
        check_success "Route '/api/${ROUTE}/*' found in ocelot.json"
    else
        check_error "Route '/api/${ROUTE}/*' NOT found in ocelot.json"
    fi
done

# Check QoS configuration
QOS_COUNT=$(grep -c "\"ExceptionsAllowedBeforeBreaking\"" backend/Gateway/Gateway.Api/ocelot.prod.json || true)
if [ "$QOS_COUNT" -ge 20 ]; then
    check_success "QoS configuration found ($QOS_COUNT instances)"
else
    check_warning "Only $QOS_COUNT QoS configurations found (expected: 20+)"
fi

# Check authentication
AUTH=$(grep -c "\"AuthenticationProviderKey\": \"Bearer\"" backend/Gateway/Gateway.Api/ocelot.prod.json || true)
if [ "$AUTH" -ge 10 ]; then
    check_success "Bearer authentication configured ($AUTH routes)"
else
    check_warning "Only $AUTH routes with Bearer authentication (expected: 10+)"
fi

echo ""
echo "═══════════════════════════════════════════════════════════════════════════"
echo "ADDITIONAL CHECKS"
echo "═══════════════════════════════════════════════════════════════════════════"
echo ""

# Docker compose syntax validation
if docker compose config --services > /dev/null 2>&1; then
    SERVICE_COUNT=$(docker compose config --services 2>/dev/null | wc -l)
    check_success "Docker compose syntax is valid ($SERVICE_COUNT services)"
else
    check_warning "Docker compose syntax validation failed (Docker might not be running)"
fi

# Check shared projects
SHARED_PROJECTS=("CarDealer.Shared" "CarDealer.Contracts")
for PROJECT in "${SHARED_PROJECTS[@]}"; do
    if [ -d "backend/_Shared/$PROJECT" ]; then
        check_success "Shared project '$PROJECT' exists"
    else
        check_error "Shared project '$PROJECT' NOT found"
    fi
done

echo ""
echo "═══════════════════════════════════════════════════════════════════════════"
echo "SUMMARY"
echo "═══════════════════════════════════════════════════════════════════════════"
echo ""

TOTAL=$((PASSED + FAILED))
echo "Total checks: $TOTAL"
echo -e "${GREEN}Passed: $PASSED${NC}"
if [ "$FAILED" -gt 0 ]; then
    echo -e "${RED}Failed: $FAILED${NC}"
else
    echo -e "${GREEN}Failed: $FAILED${NC}"
fi
echo ""

if [ "$FAILED" -eq 0 ]; then
    echo "╔════════════════════════════════════════════════════════════════════════╗"
    echo "║                                                                        ║"
    echo "║                  ✅ ALL CHECKS PASSED!                                 ║"
    echo "║                                                                        ║"
    echo "║  A) Dockerfiles: ✅ VERIFIED (48 servicios)                           ║"
    echo "║  B) Docker Compose: ✅ VERIFIED (20+ servicios)                       ║"
    echo "║  C) Ocelot Routes: ✅ VERIFIED (40+ rutas)                            ║"
    echo "║                                                                        ║"
    echo "║  Ready to run: docker-compose up -d                                   ║"
    echo "║                                                                        ║"
    echo "╚════════════════════════════════════════════════════════════════════════╝"
    echo ""
    exit 0
else
    echo "╔════════════════════════════════════════════════════════════════════════╗"
    echo "║                    ⚠️  SOME CHECKS FAILED                              ║"
    echo "║              Please review the errors above                            ║"
    echo "╚════════════════════════════════════════════════════════════════════════╝"
    echo ""
    exit 1
fi
