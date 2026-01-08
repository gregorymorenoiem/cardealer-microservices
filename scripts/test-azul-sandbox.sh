#!/bin/bash

##############################################################################
# Test de Integración AZUL Sandbox
# Verifica que la integración AZUL esté funcionando correctamente
# 
# Uso: ./test-azul-sandbox.sh
# Prerequisitos: Docker, curl, jq
##############################################################################

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuración
BILLING_SERVICE_URL="http://localhost:15107"
TEST_ORDER_NUMBER="AUTO-TEST-$(date +%s)"

# Contadores
TESTS_PASSED=0
TESTS_FAILED=0

# Funciones helper
print_header() {
    echo -e "${BLUE}============================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}============================================${NC}"
}

print_test() {
    echo -e "${YELLOW}▶ Test $1: $2${NC}"
}

print_pass() {
    echo -e "${GREEN}✅ PASS${NC}: $1"
    ((TESTS_PASSED++))
}

print_fail() {
    echo -e "${RED}❌ FAIL${NC}: $1"
    ((TESTS_FAILED++))
}

print_info() {
    echo -e "${BLUE}ℹ${NC}  $1"
}

##############################################################################
# Tests
##############################################################################

print_header "🧪 AZUL Sandbox Integration Tests"
echo ""

# Test 1: Health Check
print_test "1" "Health Check del BillingService"
HEALTH_RESPONSE=$(curl -s $BILLING_SERVICE_URL/health)
if [ "$HEALTH_RESPONSE" = "Healthy" ]; then
    print_pass "BillingService está healthy"
else
    print_fail "BillingService no responde correctamente. Response: $HEALTH_RESPONSE"
fi
echo ""

# Test 2: Payment Initiation
print_test "2" "Iniciar Payment Request"
PAYMENT_RESPONSE=$(curl -s -X POST $BILLING_SERVICE_URL/api/payment/azul/initiate \
  -H "Content-Type: application/json" \
  -d "{
    \"amount\": 1000.00,
    \"itbis\": 180.00,
    \"orderNumber\": \"$TEST_ORDER_NUMBER\"
  }")

if echo "$PAYMENT_RESPONSE" | jq -e '.paymentPageUrl' > /dev/null 2>&1; then
    print_pass "Payment request creado correctamente"
    
    # Extraer valores para validaciones adicionales
    PAYMENT_URL=$(echo "$PAYMENT_RESPONSE" | jq -r '.paymentPageUrl')
    MERCHANT_ID=$(echo "$PAYMENT_RESPONSE" | jq -r '.formFields.MerchantId')
    AMOUNT=$(echo "$PAYMENT_RESPONSE" | jq -r '.formFields.Amount')
    ITBIS=$(echo "$PAYMENT_RESPONSE" | jq -r '.formFields.ITBIS')
    AUTH_HASH=$(echo "$PAYMENT_RESPONSE" | jq -r '.formFields.AuthHash')
    
    print_info "PaymentPageUrl: $PAYMENT_URL"
    print_info "MerchantId: $MERCHANT_ID"
    print_info "Amount: $AMOUNT"
    print_info "ITBIS: $ITBIS"
    print_info "AuthHash: ${AUTH_HASH:0:20}..."
else
    print_fail "No se pudo crear payment request. Response: $PAYMENT_RESPONSE"
fi
echo ""

# Test 3: Validar Payment Page URL
print_test "3" "Validar Payment Page URL"
if [[ "$PAYMENT_URL" == "https://pruebas.azul.com.do/PaymentPage/" ]]; then
    print_pass "URL del Payment Page es correcta (ambiente Test)"
else
    print_fail "URL del Payment Page incorrecta: $PAYMENT_URL"
fi
echo ""

# Test 4: Validar formateo de montos
print_test "4" "Validar formateo de Amount"
if [ "$AMOUNT" = "100000" ]; then
    print_pass "Amount formateado correctamente (1000.00 → 100000)"
else
    print_fail "Amount mal formateado. Esperado: 100000, Recibido: $AMOUNT"
fi
echo ""

print_test "5" "Validar formateo de ITBIS"
if [ "$ITBIS" = "18000" ]; then
    print_pass "ITBIS formateado correctamente (180.00 → 18000)"
else
    print_fail "ITBIS mal formateado. Esperado: 18000, Recibido: $ITBIS"
fi
echo ""

# Test 6: Validar MerchantId configurado
print_test "6" "Validar MerchantId"
if [ -n "$MERCHANT_ID" ] && [ "$MERCHANT_ID" != "null" ] && [ "$MERCHANT_ID" != "" ]; then
    print_pass "MerchantId configurado: $MERCHANT_ID"
else
    print_fail "MerchantId no configurado o vacío"
fi
echo ""

# Test 7: Validar AuthHash generado
print_test "7" "Validar AuthHash"
if [ -n "$AUTH_HASH" ] && [ ${#AUTH_HASH} -eq 128 ]; then
    print_pass "AuthHash generado correctamente (128 caracteres SHA-512)"
else
    print_fail "AuthHash inválido. Longitud: ${#AUTH_HASH} (esperado: 128)"
fi
echo ""

# Test 8: Verificar conexión a PostgreSQL
print_test "8" "Verificar conexión a PostgreSQL"
DB_CHECK=$(docker exec postgres_db psql -U postgres -d billingservice -c "SELECT 1" 2>&1)
if echo "$DB_CHECK" | grep -q "1"; then
    print_pass "Conexión a PostgreSQL exitosa"
else
    print_fail "No se pudo conectar a PostgreSQL"
fi
echo ""

# Test 9: Verificar tabla azul_transactions
print_test "9" "Verificar tabla azul_transactions"
TABLE_CHECK=$(docker exec postgres_db psql -U postgres -d billingservice \
  -c "SELECT table_name FROM information_schema.tables WHERE table_name='azul_transactions'" 2>&1)
if echo "$TABLE_CHECK" | grep -q "azul_transactions"; then
    print_pass "Tabla azul_transactions existe"
    
    # Contar registros
    COUNT=$(docker exec postgres_db psql -U postgres -d billingservice \
      -t -c "SELECT COUNT(*) FROM azul_transactions" 2>&1 | xargs)
    print_info "Total de transacciones en DB: $COUNT"
else
    print_fail "Tabla azul_transactions no existe"
fi
echo ""

# Test 10: Verificar índices
print_test "10" "Verificar índices de performance"
INDEXES=$(docker exec postgres_db psql -U postgres -d billingservice \
  -t -c "SELECT COUNT(*) FROM pg_indexes WHERE tablename='azul_transactions'" 2>&1 | xargs)
if [ "$INDEXES" -ge 5 ]; then
    print_pass "Índices de performance creados ($INDEXES índices)"
else
    print_fail "Faltan índices. Encontrados: $INDEXES (esperados: 5+)"
fi
echo ""

# Test 11: Verificar logs de BillingService
print_test "11" "Verificar logs del servicio"
LOGS=$(docker logs billingservice --tail 100 2>&1)
if echo "$LOGS" | grep -qi "azul"; then
    print_pass "Logs muestran configuración AZUL"
else
    print_fail "No se encontraron referencias a AZUL en logs"
fi
echo ""

# Test 12: Verificar que no hay errores críticos
print_test "12" "Verificar ausencia de errores críticos"
ERRORS=$(docker logs billingservice --tail 100 2>&1 | grep -i "error\|exception\|fail" | wc -l)
if [ "$ERRORS" -eq 0 ]; then
    print_pass "No hay errores críticos en logs"
else
    print_fail "Se encontraron $ERRORS errores en logs"
fi
echo ""

##############################################################################
# Resumen
##############################################################################

print_header "📊 Resumen de Tests"
echo ""

TOTAL_TESTS=$((TESTS_PASSED + TESTS_FAILED))
SUCCESS_RATE=$(awk "BEGIN {printf \"%.1f\", ($TESTS_PASSED/$TOTAL_TESTS)*100}")

echo -e "Total de tests ejecutados: ${BLUE}$TOTAL_TESTS${NC}"
echo -e "Tests pasados: ${GREEN}$TESTS_PASSED${NC}"
echo -e "Tests fallidos: ${RED}$TESTS_FAILED${NC}"
echo -e "Tasa de éxito: ${BLUE}$SUCCESS_RATE%${NC}"
echo ""

if [ $TESTS_FAILED -eq 0 ]; then
    echo -e "${GREEN}🎉 ¡Todos los tests pasaron!${NC}"
    echo ""
    echo -e "${BLUE}📋 Próximos pasos:${NC}"
    echo "  1. Probar manualmente con tarjetas de test AZUL"
    echo "     - Visa Aprobada: 4265880000000007"
    echo "     - Visa Declinada: 4005520000000137"
    echo "  2. Verificar callbacks en logs después de completar pago"
    echo "  3. Validar persistencia en base de datos"
    echo "  4. Probar con Frontend cuando esté listo"
    echo ""
    exit 0
else
    echo -e "${RED}❌ Algunos tests fallaron${NC}"
    echo ""
    echo -e "${YELLOW}🔧 Acciones recomendadas:${NC}"
    echo "  1. Revisar logs de BillingService: docker logs billingservice"
    echo "  2. Verificar configuración en appsettings.json"
    echo "  3. Confirmar que credenciales de AZUL están configuradas"
    echo "  4. Verificar que la migration se aplicó correctamente"
    echo ""
    exit 1
fi
