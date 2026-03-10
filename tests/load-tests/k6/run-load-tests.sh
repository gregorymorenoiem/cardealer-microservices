#!/bin/bash
# =============================================================================
# OKLA Load Test Runner
# =============================================================================
# Usage:
#   ./run-load-tests.sh [quick|full|stress|soak] [base_url]
#
# Examples:
#   ./run-load-tests.sh quick                           # Quick 2-min test on localhost
#   ./run-load-tests.sh full https://api.okla.com.do    # Full 10-min test on production
#   ./run-load-tests.sh stress http://localhost:8080     # Stress test on local
#   ./run-load-tests.sh soak https://api.okla.com.do    # 30-min soak test
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPORT_DIR="${SCRIPT_DIR}/reports"
TIMESTAMP=$(date '+%Y%m%d_%H%M%S')

# Defaults
MODE="${1:-quick}"
BASE_URL="${2:-http://localhost:8080}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║         🚗 OKLA Load Test Runner                           ║${NC}"
echo -e "${BLUE}╠══════════════════════════════════════════════════════════════╣${NC}"
echo -e "${BLUE}║  Mode:     ${YELLOW}${MODE}${NC}"
echo -e "${BLUE}║  Target:   ${YELLOW}${BASE_URL}${NC}"
echo -e "${BLUE}║  Reports:  ${YELLOW}${REPORT_DIR}${NC}"
echo -e "${BLUE}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Ensure report directory exists
mkdir -p "${REPORT_DIR}"

# Check k6 is installed
if ! command -v k6 &> /dev/null; then
    echo -e "${RED}❌ k6 is not installed. Install with: brew install k6${NC}"
    exit 1
fi

# Verify API is reachable
echo -e "${YELLOW}🔍 Checking API availability at ${BASE_URL}/health ...${NC}"
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "${BASE_URL}/health" 2>/dev/null || echo "000")

if [ "${HTTP_STATUS}" == "000" ]; then
    echo -e "${RED}❌ API at ${BASE_URL} is unreachable. Please check:${NC}"
    echo -e "   1. Docker containers are running: docker compose ps"
    echo -e "   2. Gateway is healthy: curl ${BASE_URL}/health"
    echo -e "   3. For production: verify DNS and SSL"
    exit 1
fi

echo -e "${GREEN}✅ API responded with status ${HTTP_STATUS}${NC}"
echo ""

# Run the appropriate test
case "${MODE}" in
    quick)
        echo -e "${YELLOW}🏃 Running Quick Load Test (2 minutes, 50 VUs)...${NC}"
        k6 run \
            --env BASE_URL="${BASE_URL}" \
            --env QUICK=true \
            --env ENV=staging \
            --out json="${REPORT_DIR}/load-test-quick-${TIMESTAMP}.json" \
            "${SCRIPT_DIR}/load-test.js"
        ;;
    full)
        echo -e "${YELLOW}🚀 Running Full Load Test (12 minutes, 500 VUs)...${NC}"
        echo -e "${YELLOW}   ⚠️  This will generate significant load. Ensure monitoring is active.${NC}"
        sleep 3
        k6 run \
            --env BASE_URL="${BASE_URL}" \
            --env QUICK=false \
            --env ENV=staging \
            --out json="${REPORT_DIR}/load-test-full-${TIMESTAMP}.json" \
            "${SCRIPT_DIR}/load-test.js"
        ;;
    stress)
        echo -e "${YELLOW}🔥 Running Stress Test (12 minutes, up to 1250 VUs)...${NC}"
        echo -e "${RED}   ⚠️  WARNING: This WILL push the system to its limits!${NC}"
        sleep 5
        k6 run \
            --env BASE_URL="${BASE_URL}" \
            --env ENV=staging \
            --out json="${REPORT_DIR}/stress-test-${TIMESTAMP}.json" \
            "${SCRIPT_DIR}/stress-test.js"
        ;;
    soak)
        echo -e "${YELLOW}🧪 Running Soak Test (34 minutes, 200 VUs sustained)...${NC}"
        echo -e "${YELLOW}   ⚠️  This is a long-running test for stability verification.${NC}"
        sleep 3
        k6 run \
            --env BASE_URL="${BASE_URL}" \
            --env ENV=staging \
            --out json="${REPORT_DIR}/soak-test-${TIMESTAMP}.json" \
            "${SCRIPT_DIR}/soak-test.js"
        ;;
    *)
        echo -e "${RED}❌ Unknown mode: ${MODE}${NC}"
        echo "   Usage: $0 [quick|full|stress|soak] [base_url]"
        exit 1
        ;;
esac

echo ""
echo -e "${GREEN}✅ Load test completed! Reports saved in: ${REPORT_DIR}${NC}"
echo -e "${BLUE}   View reports: ls -la ${REPORT_DIR}${NC}"
