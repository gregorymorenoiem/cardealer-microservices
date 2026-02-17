#!/usr/bin/env bash

# 🔍 SCRIPT DE AUDITORÍA DE DOCUMENTACIÓN DE API
# Fecha: Enero 30, 2026
# Propósito: Extraer endpoints reales de documentos y compararlos con Gateway

set -e

# Verificar que estamos usando bash 4+
if [ "${BASH_VERSINFO[0]}" -lt 4 ]; then
    echo "Error: Este script requiere bash 4 o superior."
    echo "En macOS, instala bash moderno con: brew install bash"
    echo "O usa una alternativa Python/Node.js"
    exit 1
fi

# Colores para output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}==================================================${NC}"
echo -e "${BLUE}  🔍 AUDITORÍA DE DOCUMENTACIÓN DE API${NC}"
echo -e "${BLUE}==================================================${NC}"
echo ""

# Directorios
API_DOCS_DIR="docs/frontend-rebuild/05-API-INTEGRATION"
GATEWAY_CONFIG="backend/Gateway/Gateway.Api/ocelot.prod.json"
OUTPUT_DIR="docs/frontend-rebuild/audit-reports"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
OUTPUT_FILE="$OUTPUT_DIR/audit-report-$TIMESTAMP.json"
OUTPUT_CSV="$OUTPUT_DIR/audit-report-$TIMESTAMP.csv"
OUTPUT_MD="$OUTPUT_DIR/audit-report-$TIMESTAMP.md"

# Crear directorio de reportes si no existe
mkdir -p "$OUTPUT_DIR"

echo -e "${GREEN}✓${NC} Directorios configurados"
echo ""

# Array para almacenar resultados
declare -A documented_endpoints
declare -A gateway_endpoints
total_documented=0
total_gateway=0

echo -e "${YELLOW}📖 Leyendo documentos de API...${NC}"
echo ""

# Función para extraer endpoints de un archivo
extract_endpoints() {
    local file=$1
    local filename=$(basename "$file")
    
    echo -e "${BLUE}  → Procesando: $filename${NC}"
    
    # Extraer líneas que contienen endpoints (GET, POST, PUT, DELETE seguido de /api/)
    local endpoints=$(grep -E "(GET|POST|PUT|DELETE|PATCH)\s+/api/" "$file" 2>/dev/null || true)
    
    if [ -n "$endpoints" ]; then
        local count=$(echo "$endpoints" | wc -l | tr -d ' ')
        echo -e "    ${GREEN}✓ $count endpoints encontrados${NC}"
        
        # Almacenar cada endpoint
        while IFS= read -r line; do
            # Extraer método y ruta
            local method=$(echo "$line" | grep -oE "GET|POST|PUT|DELETE|PATCH" | head -1)
            local route=$(echo "$line" | grep -oE "/api/[^[:space:];\"']+" | head -1)
            
            if [ -n "$method" ] && [ -n "$route" ]; then
                local key="$method $route"
                documented_endpoints["$key"]="$filename"
                ((total_documented++))
            fi
        done <<< "$endpoints"
    else
        echo -e "    ${YELLOW}⚠ Sin endpoints documentados${NC}"
    fi
    echo ""
}

# Procesar todos los archivos .md en el directorio de API
if [ -d "$API_DOCS_DIR" ]; then
    for file in "$API_DOCS_DIR"/*.md; do
        if [ -f "$file" ]; then
            extract_endpoints "$file"
        fi
    done
else
    echo -e "${RED}✗ Directorio no encontrado: $API_DOCS_DIR${NC}"
    exit 1
fi

echo -e "${YELLOW}📡 Leyendo configuración del Gateway...${NC}"
echo ""

# Extraer endpoints del Gateway (ocelot.prod.json)
if [ -f "$GATEWAY_CONFIG" ]; then
    # Usar jq si está disponible, sino usar grep
    if command -v jq &> /dev/null; then
        # Extraer UpstreamPathTemplate de cada ruta
        local gateway_routes=$(jq -r '.Routes[].UpstreamPathTemplate' "$GATEWAY_CONFIG" 2>/dev/null || true)
        
        if [ -n "$gateway_routes" ]; then
            while IFS= read -r route; do
                if [ -n "$route" ]; then
                    # Extraer método HTTP de UpstreamHttpMethod
                    gateway_endpoints["$route"]="gateway"
                    ((total_gateway++))
                fi
            done <<< "$gateway_routes"
            
            echo -e "${GREEN}✓ $total_gateway rutas encontradas en Gateway${NC}"
        fi
    else
        # Fallback: contar Routes en el JSON
        total_gateway=$(grep -c '"UpstreamPathTemplate"' "$GATEWAY_CONFIG" 2>/dev/null || echo "0")
        echo -e "${GREEN}✓ $total_gateway rutas encontradas en Gateway (conteo aproximado)${NC}"
    fi
else
    echo -e "${RED}✗ Archivo no encontrado: $GATEWAY_CONFIG${NC}"
    exit 1
fi

echo ""
echo -e "${BLUE}==================================================${NC}"
echo -e "${BLUE}  📊 RESULTADOS DE LA AUDITORÍA${NC}"
echo -e "${BLUE}==================================================${NC}"
echo ""

# Calcular porcentaje
if [ $total_gateway -gt 0 ]; then
    percentage=$(awk "BEGIN {printf \"%.1f\", ($total_documented/$total_gateway)*100}")
else
    percentage="0.0"
fi

echo -e "${GREEN}✓ Endpoints documentados:${NC} $total_documented"
echo -e "${YELLOW}⚠ Total de rutas en Gateway:${NC} $total_gateway"
echo -e "${BLUE}📈 Cobertura de documentación:${NC} $percentage%"
echo ""

# Generar reporte JSON
echo -e "${YELLOW}📄 Generando reporte JSON...${NC}"
cat > "$OUTPUT_FILE" << EOF
{
  "audit_date": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "summary": {
    "total_documented": $total_documented,
    "total_gateway_routes": $total_gateway,
    "coverage_percentage": $percentage
  },
  "documented_endpoints": {
EOF

# Agregar endpoints documentados al JSON
first=true
for endpoint in "${!documented_endpoints[@]}"; do
    if [ "$first" = true ]; then
        first=false
    else
        echo "," >> "$OUTPUT_FILE"
    fi
    echo -n "    \"$endpoint\": \"${documented_endpoints[$endpoint]}\"" >> "$OUTPUT_FILE"
done

cat >> "$OUTPUT_FILE" << EOF

  }
}
EOF

echo -e "${GREEN}✓ Reporte JSON generado: $OUTPUT_FILE${NC}"

# Generar reporte CSV
echo -e "${YELLOW}📄 Generando reporte CSV...${NC}"
cat > "$OUTPUT_CSV" << EOF
Método,Ruta,Archivo Documentado
EOF

for endpoint in "${!documented_endpoints[@]}"; do
    method=$(echo "$endpoint" | cut -d' ' -f1)
    route=$(echo "$endpoint" | cut -d' ' -f2-)
    file="${documented_endpoints[$endpoint]}"
    echo "$method,$route,$file" >> "$OUTPUT_CSV"
done

echo -e "${GREEN}✓ Reporte CSV generado: $OUTPUT_CSV${NC}"

# Generar reporte Markdown
echo -e "${YELLOW}📄 Generando reporte Markdown...${NC}"
cat > "$OUTPUT_MD" << EOF
# 📊 Reporte de Auditoría de Documentación de API

**Fecha:** $(date +"%B %d, %Y %H:%M:%S")  
**Generado por:** audit-api-documentation.sh

---

## 📈 Resumen Ejecutivo

| Métrica                      | Valor           |
|------------------------------|-----------------|
| **Endpoints Documentados**   | $total_documented |
| **Rutas en Gateway**         | $total_gateway  |
| **Cobertura de Documentación** | $percentage%   |

---

## 📋 Endpoints Documentados

| Método | Ruta | Archivo |
|--------|------|---------|
EOF

for endpoint in "${!documented_endpoints[@]}"; do
    method=$(echo "$endpoint" | cut -d' ' -f1)
    route=$(echo "$endpoint" | cut -d' ' -f2-)
    file="${documented_endpoints[$endpoint]}"
    echo "| \`$method\` | \`$route\` | $file |" >> "$OUTPUT_MD"
done

cat >> "$OUTPUT_MD" << EOF

---

## 📊 Desglose por Archivo

EOF

# Contar endpoints por archivo
declare -A file_counts
for endpoint in "${!documented_endpoints[@]}"; do
    file="${documented_endpoints[$endpoint]}"
    if [ -z "${file_counts[$file]}" ]; then
        file_counts[$file]=0
    fi
    ((file_counts[$file]++))
done

echo "| Archivo | Endpoints Documentados |" >> "$OUTPUT_MD"
echo "|---------|------------------------|" >> "$OUTPUT_MD"

for file in "${!file_counts[@]}"; do
    count="${file_counts[$file]}"
    echo "| $file | $count |" >> "$OUTPUT_MD"
done

cat >> "$OUTPUT_MD" << EOF

---

## 🎯 Próximos Pasos

### Servicios Pendientes de Documentar

Basado en el Gateway, los siguientes servicios necesitan documentación:

- **VehiclesService:** Endpoints de vehículos (búsqueda, filtrado, CRUD)
- **UserService:** Gestión de usuarios y perfiles
- **BillingService:** Pagos, suscripciones, planes
- **RoleService:** Roles y permisos
- **NotificationService:** Notificaciones push, email, SMS
- **Y más...**

### Recomendaciones

1. **Prioridad Alta:** Documentar servicios core (Vehicles, Users, Billing)
2. **Prioridad Media:** Documentar servicios de soporte (Notifications, Media)
3. **Prioridad Baja:** Documentar servicios administrativos

---

_Generado automáticamente por audit-api-documentation.sh_
EOF

echo -e "${GREEN}✓ Reporte Markdown generado: $OUTPUT_MD${NC}"
echo ""

echo -e "${BLUE}==================================================${NC}"
echo -e "${GREEN}✅ AUDITORÍA COMPLETADA${NC}"
echo -e "${BLUE}==================================================${NC}"
echo ""
echo -e "Reportes generados en: ${YELLOW}$OUTPUT_DIR${NC}"
echo ""
echo -e "  📄 JSON:     $OUTPUT_FILE"
echo -e "  📊 CSV:      $OUTPUT_CSV"
echo -e "  📝 Markdown: $OUTPUT_MD"
echo ""
echo -e "${BLUE}Para ver el reporte Markdown:${NC}"
echo -e "  cat $OUTPUT_MD"
echo ""
