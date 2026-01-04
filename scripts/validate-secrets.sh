#!/bin/bash
# ===========================================================================
# validate-secrets.sh - Sprint 1 Secrets Validation Script (macOS version)
# ===========================================================================
# Este script verifica que todos los archivos de secrets existan y tengan
# contenido válido (no placeholders).
# ===========================================================================

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "\n${CYAN}========================================${NC}"
echo -e "${CYAN}SPRINT 1 - SECRETS VALIDATION${NC}"
echo -e "${CYAN}========================================${NC}\n"

SECRETS_PATH="./secrets"
PASSED=0
FAILED=0
WARNINGS=0

# Función para validar un secret
validate_secret() {
    local file=$1
    local min_length=$2
    local pattern=$3
    local critical=$4
    local description=$5
    
    local filepath="${SECRETS_PATH}/${file}"
    local status="UNKNOWN"
    local message=""
    local icon=""
    local color=""
    
    # Verificar si el archivo existe
    if [ ! -f "$filepath" ]; then
        if [ "$critical" = "true" ]; then
            ((FAILED++))
            status="MISSING"
            icon="❌"
            color=$RED
            message="Archivo crítico no encontrado"
        else
            ((WARNINGS++))
            status="MISSING"
            icon="⚠️ "
            color=$YELLOW
            message="Archivo opcional no encontrado"
        fi
    else
        # Leer contenido
        content=$(cat "$filepath" | tr -d '\n' | tr -d '\r')
        
        # Verificar placeholders
        if echo "$content" | grep -qiE "placeholder|CHANGE|TODO|XXX|example|configure_in_sprint"; then
            if [ "$critical" = "true" ]; then
                ((FAILED++))
                status="PLACEHOLDER"
                icon="❌"
                color=$RED
                message="Contiene placeholder - reemplazar con valor real"
            else
                ((WARNINGS++))
                status="PLACEHOLDER"
                icon="⚠️ "
                color=$YELLOW
                message="Contiene placeholder (opcional en dev)"
            fi
        # Verificar longitud mínima
        elif [ ${#content} -lt $min_length ]; then
            if [ "$critical" = "true" ]; then
                ((FAILED++))
                status="TOO_SHORT"
                icon="❌"
                color=$RED
                message="Contenido muy corto (mínimo ${min_length} caracteres)"
            else
                ((WARNINGS++))
                status="TOO_SHORT"
                icon="⚠️ "
                color=$YELLOW
                message="Contenido muy corto"
            fi
        # Verificar patrón
        elif ! echo "$content" | grep -qE "$pattern"; then
            if [ "$critical" = "true" ]; then
                ((FAILED++))
                status="INVALID_FORMAT"
                icon="❌"
                color=$RED
                message="Formato inválido"
            else
                ((WARNINGS++))
                status="INVALID_FORMAT"
                icon="⚠️ "
                color=$YELLOW
                message="Formato inválido (opcional)"
            fi
        else
            # Todo bien
            ((PASSED++))
            status="OK"
            icon="✅"
            color=$GREEN
            message="Válido"
        fi
    fi
    
    # Imprimir resultado
    echo -e "${color}${icon} ${description}${NC}"
    echo -e "   Archivo: ${file}"
    echo -e "   Estado: ${status} - ${message}"
    echo ""
}

echo -e "${YELLOW}Validando archivos de secrets...${NC}\n"

# Google Cloud Platform
validate_secret "google_maps_api_key.txt" 35 "^AIzaSy" "true" "Google Maps API Key"
validate_secret "google_client_id.txt" 50 "^[0-9]+-[a-zA-Z0-9]+\.apps\.googleusercontent\.com$" "true" "Google OAuth Client ID"
validate_secret "google_client_secret.txt" 20 "^GOCSPX-" "true" "Google OAuth Client Secret"

# Firebase
validate_secret "firebase_service_account.json" 100 "service_account" "true" "Firebase Service Account JSON"

# Stripe
validate_secret "stripe_secret_key.txt" 20 "^sk_(test|live)_" "true" "Stripe Secret Key"
validate_secret "stripe_webhook_secret.txt" 20 "^whsec_" "false" "Stripe Webhook Secret (opcional)"

# Email Services
validate_secret "resend_api_key.txt" 20 "^re_" "false" "Resend API Key"
validate_secret "sendgrid_api_key.txt" 20 "^SG\." "false" "SendGrid API Key (opcional)"

# Twilio
validate_secret "twilio_account_sid.txt" 30 "^AC" "false" "Twilio Account SID (opcional)"
validate_secret "twilio_auth_token.txt" 30 "^[a-f0-9]{32}$" "false" "Twilio Auth Token (opcional)"
validate_secret "twilio_phone_number.txt" 10 "^\+" "false" "Twilio Phone Number (opcional)"

# AWS S3
validate_secret "aws_access_key_id.txt" 16 "^AKIA" "true" "AWS Access Key ID"
validate_secret "aws_secret_access_key.txt" 30 "^[A-Za-z0-9+/]{40}$" "true" "AWS Secret Access Key"
validate_secret "aws_s3_bucket_name.txt" 3 "^[a-z0-9\-]+$" "true" "AWS S3 Bucket Name"
validate_secret "aws_region.txt" 9 "^[a-z]{2}-[a-z]+-[0-9]{1}$" "true" "AWS Region"

# JWT y Database
validate_secret "jwt_secret_key.txt" 32 "^[A-Za-z0-9+/=]+$" "true" "JWT Secret Key"
validate_secret "db_password.txt" 8 "^.{8,}$" "true" "PostgreSQL Password"

# Resumen
echo -e "${CYAN}========================================${NC}"
echo -e "${CYAN}RESUMEN DE VALIDACIÓN${NC}"
echo -e "${CYAN}========================================${NC}\n"

echo -e "${GREEN}✅ Passed: ${PASSED}${NC}"
echo -e "${YELLOW}⚠️  Warnings: ${WARNINGS}${NC}"
echo -e "${RED}❌ Failed: ${FAILED}${NC}\n"

if [ $FAILED -gt 0 ]; then
    echo -e "${RED}⛔ VALIDACIÓN FALLIDA${NC}"
    echo -e "Hay ${FAILED} archivo(s) crítico(s) con problemas."
    echo -e "Por favor, corrige los errores antes de continuar.\n"
    exit 1
elif [ $WARNINGS -gt 0 ]; then
    echo -e "${YELLOW}⚠️  VALIDACIÓN CON WARNINGS${NC}"
    echo -e "Hay ${WARNINGS} archivo(s) opcional(es) con problemas."
    echo -e "Los servicios críticos están OK, pero algunos opcionales faltan.\n"
    exit 0
else
    echo -e "${GREEN}🎉 VALIDACIÓN EXITOSA${NC}"
    echo -e "Todos los secrets están correctamente configurados!\n"
    exit 0
fi
