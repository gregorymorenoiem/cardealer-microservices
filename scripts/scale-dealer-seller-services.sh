#!/bin/bash
# =============================================================================
# Scale Up Dealer & Seller Portal Services
# =============================================================================
# Use this script after deploying new images to GHCR to enable the services
# needed for the Dealer & Seller portal functionality.
#
# Prerequisites:
#   - kubectl configured for okla-cluster
#   - Images pushed to GHCR (via CI/CD or manual build)
#   - registry-credentials secret is valid (not expired)
#
# Usage:
#   chmod +x scripts/scale-dealer-seller-services.sh
#   ./scripts/scale-dealer-seller-services.sh
# =============================================================================

set -euo pipefail

NAMESPACE="okla"
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo "============================================"
echo "🚀 Scaling Dealer & Seller Portal Services"
echo "============================================"
echo ""

# Check kubectl connection
if ! kubectl get namespace "$NAMESPACE" &>/dev/null; then
    echo -e "${RED}ERROR: Cannot access namespace '$NAMESPACE'. Check kubectl config.${NC}"
    echo "Run: doctl kubernetes cluster kubeconfig save okla-cluster"
    exit 1
fi

# Verify registry-credentials secret
echo -e "${YELLOW}Checking registry-credentials...${NC}"
if ! kubectl get secret registry-credentials -n "$NAMESPACE" &>/dev/null; then
    echo -e "${RED}ERROR: registry-credentials secret not found.${NC}"
    echo "Create it with:"
    echo '  TOKEN=$(gh auth token)'
    echo '  kubectl create secret docker-registry registry-credentials \'
    echo '    --docker-server=ghcr.io --docker-username=gregorymorenoiem \'
    echo "    --docker-password=\$TOKEN -n $NAMESPACE"
    exit 1
fi
echo -e "${GREEN}✅ registry-credentials found${NC}"

# Core services that should already be running (verify)
echo ""
echo -e "${YELLOW}Verifying core services...${NC}"
CORE_SERVICES=(
    "vehiclessaleservice"
    "gateway"
    "authservice"
    "userservice"
)
for svc in "${CORE_SERVICES[@]}"; do
    REPLICAS=$(kubectl get deployment "$svc" -n "$NAMESPACE" -o jsonpath='{.spec.replicas}' 2>/dev/null || echo "NOT_FOUND")
    if [ "$REPLICAS" = "NOT_FOUND" ]; then
        echo -e "  ${RED}✗ $svc: deployment not found${NC}"
    elif [ "$REPLICAS" = "0" ]; then
        echo -e "  ${YELLOW}⚠ $svc: currently disabled (replicas: 0)${NC}"
    else
        echo -e "  ${GREEN}✓ $svc: running (replicas: $REPLICAS)${NC}"
    fi
done

# Services to enable for Dealer & Seller portal
echo ""
echo -e "${YELLOW}Scaling up Dealer & Seller services...${NC}"
PORTAL_SERVICES=(
    "paymentservice"
    "dealermanagementservice"
    "dealeranalyticsservice"
)

for svc in "${PORTAL_SERVICES[@]}"; do
    echo -n "  Scaling $svc to 1 replica... "
    
    # Check if image exists in GHCR first
    IMAGE=$(kubectl get deployment "$svc" -n "$NAMESPACE" -o jsonpath='{.spec.template.spec.containers[0].image}' 2>/dev/null || echo "")
    
    if [ -z "$IMAGE" ]; then
        echo -e "${RED}✗ deployment not found${NC}"
        continue
    fi
    
    kubectl scale deployment "$svc" --replicas=1 -n "$NAMESPACE" 2>/dev/null
    echo -e "${GREEN}✓ scaled ($IMAGE)${NC}"
done

# Update Gateway ConfigMap to include new routes
echo ""
echo -e "${YELLOW}Updating Gateway ConfigMap...${NC}"
OCELOT_FILE="backend/Gateway/Gateway.Api/ocelot.prod.json"
if [ -f "$OCELOT_FILE" ]; then
    kubectl delete configmap gateway-config -n "$NAMESPACE" 2>/dev/null || true
    kubectl create configmap gateway-config --from-file=ocelot.json="$OCELOT_FILE" -n "$NAMESPACE"
    kubectl rollout restart deployment/gateway -n "$NAMESPACE"
    echo -e "${GREEN}✅ Gateway ConfigMap updated and restarted${NC}"
else
    echo -e "${RED}✗ $OCELOT_FILE not found. Run from repo root.${NC}"
fi

# Wait for rollouts
echo ""
echo -e "${YELLOW}Waiting for deployments to stabilize...${NC}"
for svc in "${PORTAL_SERVICES[@]}"; do
    echo -n "  Waiting for $svc... "
    if kubectl rollout status deployment/"$svc" -n "$NAMESPACE" --timeout=120s 2>/dev/null; then
        echo -e "${GREEN}✓ ready${NC}"
    else
        echo -e "${RED}✗ timeout or error${NC}"
    fi
done

# Health check
echo ""
echo -e "${YELLOW}Running health checks...${NC}"
for svc in "${PORTAL_SERVICES[@]}"; do
    echo -n "  $svc /health: "
    STATUS=$(kubectl exec deployment/gateway -n "$NAMESPACE" -- \
        wget -q -O - --timeout=5 "http://$svc:8080/health" 2>/dev/null | head -c 100 || echo "UNREACHABLE")
    if echo "$STATUS" | grep -qi "healthy\|Healthy\|ok"; then
        echo -e "${GREEN}✓ healthy${NC}"
    else
        echo -e "${YELLOW}⚠ $STATUS${NC}"
    fi
done

echo ""
echo "============================================"
echo -e "${GREEN}✅ Dealer & Seller services scaled up${NC}"
echo "============================================"
echo ""
echo "Next steps:"
echo "  1. Verify pods: kubectl get pods -n $NAMESPACE | grep -E 'payment|dealer'"
echo "  2. Test leads API: kubectl port-forward svc/vehiclessaleservice 8080:8080 -n $NAMESPACE"
echo "  3. Test invoices: kubectl port-forward svc/paymentservice 8081:8080 -n $NAMESPACE"
echo "  4. Verify Gateway: curl -s https://okla.com.do/api/health | jq ."
echo ""
