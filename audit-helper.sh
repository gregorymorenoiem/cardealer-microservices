#!/bin/bash

# 🔍 OKLA Audit Automation Script
# Automatiza el proceso de auditoría incremental para GitHub Copilot

PROJECT_DIR="/Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices"
AUDIT_PROMPT_FILE="$PROJECT_DIR/AUDIT_PROMPT.md"
REPORTS_DIR="$PROJECT_DIR/audit-reports"

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Crear directorio de reportes si no existe
mkdir -p "$REPORTS_DIR"

function show_help() {
    echo -e "${BLUE}🔍 OKLA Audit Automation Script${NC}"
    echo ""
    echo "Comandos disponibles:"
    echo -e "${GREEN}  ./audit-helper.sh status${NC}     - Ver estado actual de la auditoría"
    echo -e "${GREEN}  ./audit-helper.sh next${NC}       - Mostrar próximo servicio a auditar"
    echo -e "${GREEN}  ./audit-helper.sh complete${NC}   - Marcar servicio actual como completado"
    echo -e "${GREEN}  ./audit-helper.sh list${NC}       - Listar todos los servicios pendientes"
    echo -e "${GREEN}  ./audit-helper.sh reports${NC}    - Ver reportes generados"
    echo -e "${GREEN}  ./audit-helper.sh progress${NC}   - Ver progreso general"
    echo ""
}

function get_current_service() {
    grep "^\*\*SERVICIO:\*\*" "$AUDIT_PROMPT_FILE" | sed 's/\*\*SERVICIO:\*\* //' | tr -d '\r\n '
}

function get_progress() {
    grep "^\*\*PROGRESO:\*\*" "$AUDIT_PROMPT_FILE" | sed 's/\*\*PROGRESO:\*\* //' | tr -d '\r\n'
}

function show_status() {
    echo -e "${BLUE}📊 ESTADO ACTUAL DE LA AUDITORÍA${NC}"
    echo ""
    
    current_service=$(get_current_service)
    progress=$(get_progress)
    
    echo -e "${YELLOW}🎯 Servicio Actual:${NC} $current_service"
    echo -e "${YELLOW}📈 Progreso:${NC} $progress"
    
    if [ -f "$REPORTS_DIR/AUDIT_REPORT_$current_service.md" ]; then
        echo -e "${GREEN}✅ Reporte existe para $current_service${NC}"
    else
        echo -e "${RED}❌ Reporte pendiente para $current_service${NC}"
    fi
    
    echo ""
    echo -e "${BLUE}📁 Ruta del servicio:${NC}"
    echo "  $PROJECT_DIR/backend/$current_service"
    echo ""
}

function show_next() {
    current_service=$(get_current_service)
    echo -e "${GREEN}🎯 PRÓXIMO SERVICIO A AUDITAR:${NC} $current_service"
    echo ""
    echo -e "${YELLOW}📋 Para GitHub Copilot:${NC}"
    echo "1. Lee el archivo AUDIT_PROMPT.md"
    echo "2. Audita el servicio: $current_service"
    echo "3. Genera reporte siguiendo el formato especificado"
    echo "4. Guarda como: AUDIT_REPORT_$current_service.md"
    echo "5. Ejecuta: ./audit-helper.sh complete"
    echo ""
}

function complete_audit() {
    current_service=$(get_current_service)
    
    # Verificar si existe el reporte
    if [ ! -f "$REPORTS_DIR/AUDIT_REPORT_$current_service.md" ]; then
        echo -e "${RED}❌ Error: No se encuentra el reporte AUDIT_REPORT_$current_service.md${NC}"
        echo "Por favor genera el reporte antes de marcar como completado."
        exit 1
    fi
    
    echo -e "${GREEN}✅ Marcando $current_service como completado...${NC}"
    
    # Lista de servicios en orden
    services=("AdminService" "AuthService" "ContactService" "ErrorService" "Gateway" "MediaService" "NotificationService" "UserService" "VehiclesSaleService" "BillingService" "AuditService" "ChatbotService" "CRMService" "ComparisonService" "KYCService" "ReportsService" "RoleService" "ReviewService" "RecommendationService" "DealerAnalyticsService" "AIProcessingService" "AnalyticsAgent" "ListingAgent" "ModerationAgent" "PricingAgent" "RecoAgent" "SearchAgent" "SupportAgent" "VehicleIntelligenceService" "web-next" "_Shared" "_Tests")
    
    # Encontrar índice actual
    current_index=-1
    for i in "${!services[@]}"; do
        if [[ "${services[$i]}" == "$current_service" ]]; then
            current_index=$i
            break
        fi
    done
    
    if [ $current_index -eq -1 ]; then
        echo -e "${RED}❌ Error: Servicio $current_service no encontrado en la lista${NC}"
        exit 1
    fi
    
    # Calcular siguiente servicio y progreso
    next_index=$((current_index + 1))
    total_services=${#services[@]}
    completed_services=$((current_index + 1))
    progress_percent=$(echo "scale=1; $completed_services * 100 / $total_services" | bc)
    
    if [ $next_index -lt $total_services ]; then
        next_service="${services[$next_index]}"
        
        # Actualizar AUDIT_PROMPT.md
        current_date=$(date "+%Y-%m-%d %H:%M AST")
        
        # Crear archivo temporal con las actualizaciones
        sed "s/\*\*SERVICIO:\*\* $current_service/\*\*SERVICIO:\*\* $next_service/g" "$AUDIT_PROMPT_FILE" | \
        sed "s/\*\*PROGRESO:\*\*.*%/\*\*PROGRESO:\*\* ${progress_percent}% ($completed_services\/$total_services completados)/g" | \
        sed "s/\*\*ÚLTIMO UPDATE:\*\*.*/\*\*ÚLTIMO UPDATE:\*\* $current_date/g" > "$AUDIT_PROMPT_FILE.tmp"
        
        mv "$AUDIT_PROMPT_FILE.tmp" "$AUDIT_PROMPT_FILE"
        
        echo -e "${GREEN}✅ $current_service completado!${NC}"
        echo -e "${YELLOW}🎯 Siguiente servicio: $next_service${NC}"
        echo -e "${BLUE}📈 Progreso: ${progress_percent}% ($completed_services/$total_services)${NC}"
        
    else
        echo -e "${GREEN}🎉 ¡AUDITORÍA COMPLETA! Todos los servicios han sido auditados.${NC}"
        
        # Actualizar para marcar como completado
        current_date=$(date "+%Y-%m-%d %H:%M AST")
        sed "s/\*\*PROGRESO:\*\*.*%/\*\*PROGRESO:\*\* 100% - ¡COMPLETADO!/g" "$AUDIT_PROMPT_FILE" | \
        sed "s/\*\*ÚLTIMO UPDATE:\*\*.*/\*\*ÚLTIMO UPDATE:\*\* $current_date/g" > "$AUDIT_PROMPT_FILE.tmp"
        mv "$AUDIT_PROMPT_FILE.tmp" "$AUDIT_PROMPT_FILE"
    fi
}

function list_services() {
    echo -e "${BLUE}📋 SERVICIOS EN COLA DE AUDITORÍA${NC}"
    echo ""
    
    services=("AdminService" "AuthService" "ContactService" "ErrorService" "Gateway" "MediaService" "NotificationService" "UserService" "VehiclesSaleService" "BillingService" "AuditService" "ChatbotService" "CRMService" "ComparisonService" "KYCService" "ReportsService" "RoleService" "ReviewService" "RecommendationService" "DealerAnalyticsService" "AIProcessingService" "AnalyticsAgent" "ListingAgent" "ModerationAgent" "PricingAgent" "RecoAgent" "SearchAgent" "SupportAgent" "VehicleIntelligenceService" "web-next" "_Shared" "_Tests")
    
    current_service=$(get_current_service)
    found_current=false
    
    for service in "${services[@]}"; do
        if [[ "$service" == "$current_service" ]]; then
            found_current=true
            echo -e "${YELLOW}🎯 $service${NC} ← ACTUAL"
        elif [[ "$found_current" == false ]]; then
            if [ -f "$REPORTS_DIR/AUDIT_REPORT_$service.md" ]; then
                echo -e "${GREEN}✅ $service${NC}"
            else
                echo -e "${RED}❌ $service${NC} ← PERDIDO"
            fi
        else
            echo -e "${BLUE}⏳ $service${NC}"
        fi
    done
    echo ""
}

function show_reports() {
    echo -e "${BLUE}📊 REPORTES GENERADOS${NC}"
    echo ""
    
    if [ -d "$REPORTS_DIR" ] && [ "$(ls -A $REPORTS_DIR)" ]; then
        for report in "$REPORTS_DIR"/*.md; do
            if [ -f "$report" ]; then
                filename=$(basename "$report")
                service_name=$(echo "$filename" | sed 's/AUDIT_REPORT_//' | sed 's/.md//')
                size=$(du -h "$report" | cut -f1)
                date=$(date -r "$report" "+%Y-%m-%d %H:%M")
                
                echo -e "${GREEN}✅ $service_name${NC}"
                echo "   📄 Archivo: $filename"
                echo "   📊 Tamaño: $size"
                echo "   📅 Fecha: $date"
                echo ""
            fi
        done
    else
        echo -e "${YELLOW}⚠️ No se han generado reportes aún${NC}"
    fi
}

function show_progress() {
    echo -e "${BLUE}📈 PROGRESO GENERAL DE LA AUDITORÍA${NC}"
    echo ""
    
    services=("AdminService" "AuthService" "ContactService" "ErrorService" "Gateway" "MediaService" "NotificationService" "UserService" "VehiclesSaleService" "BillingService" "AuditService" "ChatbotService" "CRMService" "ComparisonService" "KYCService" "ReportsService" "RoleService" "ReviewService" "RecommendationService" "DealerAnalyticsService" "AIProcessingService" "AnalyticsAgent" "ListingAgent" "ModerationAgent" "PricingAgent" "RecoAgent" "SearchAgent" "SupportAgent" "VehicleIntelligenceService" "web-next" "_Shared" "_Tests")
    
    total_services=${#services[@]}
    completed_count=0
    
    for service in "${services[@]}"; do
        if [ -f "$REPORTS_DIR/AUDIT_REPORT_$service.md" ]; then
            completed_count=$((completed_count + 1))
        fi
    done
    
    progress_percent=$(echo "scale=1; $completed_count * 100 / $total_services" | bc)
    remaining=$((total_services - completed_count))
    
    echo -e "${GREEN}✅ Completados:${NC} $completed_count/$total_services ($progress_percent%)"
    echo -e "${YELLOW}⏳ Restantes:${NC} $remaining"
    echo ""
    
    # Barra de progreso visual
    filled=$(echo "$completed_count * 50 / $total_services" | bc)
    bar=""
    for ((i=1; i<=50; i++)); do
        if [ $i -le $filled ]; then
            bar+="█"
        else
            bar+="░"
        fi
    done
    
    echo -e "${BLUE}[$bar] ${progress_percent}%${NC}"
    echo ""
}

# Main script logic
case "$1" in
    "status")
        show_status
        ;;
    "next")
        show_next
        ;;
    "complete")
        complete_audit
        ;;
    "list")
        list_services
        ;;
    "reports")
        show_reports
        ;;
    "progress")
        show_progress
        ;;
    "help"|"-h"|"--help"|"")
        show_help
        ;;
    *)
        echo -e "${RED}❌ Comando desconocido: $1${NC}"
        echo ""
        show_help
        exit 1
        ;;
esac