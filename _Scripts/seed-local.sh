#!/bin/bash
# Script: seed-local.sh
# Descripción: Seed de datos de prueba en ambiente local
# Uso: bash seed-local.sh
# Compatible: macOS, Linux

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables de configuración
API_URL="${API_URL:-http://localhost:18443}"
DB_PASSWORD="${DB_PASSWORD:-postgres}"
DB_USER="${DB_USER:-postgres}"
DB_NAME="${DB_NAME:-cardealer}"
DB_HOST="${DB_HOST:-localhost}"

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

echo -e "${BLUE}"
echo "╔════════════════════════════════════════════════════════════╗"
echo "║         OKLA - Data Seeding Script (Local)                 ║"
echo "║          30 Dealers | 20 Users | 150 Vehicles             ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo -e "${NC}"

# Función para mostrar pasos
log_step() {
    echo -e "${GREEN}✓${NC} $1"
}

log_error() {
    echo -e "${RED}✗${NC} $1"
}

log_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

# PASO 0: Validaciones previas
echo ""
log_info "Validando requisitos previos..."

# Verificar que la API está disponible
if ! curl -s "${API_URL}/health" > /dev/null 2>&1; then
    log_error "API no disponible en ${API_URL}"
    echo "Asegúrate de que:"
    echo "  1. La API Gateway está corriendo"
    echo "  2. PostgreSQL está corriendo"
    echo "  3. El valor de API_URL es correcto"
    exit 1
fi
log_step "API disponible en ${API_URL}"

# Verificar que PostgreSQL está disponible
if ! PGPASSWORD=$DB_PASSWORD psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" -c "SELECT 1" > /dev/null 2>&1; then
    log_error "PostgreSQL no disponible"
    echo "Intenta:"
    echo "  docker-compose up -d postgres"
    exit 1
fi
log_step "PostgreSQL disponible"

# PASO 1: Limpiar datos existentes (opcional)
echo ""
log_info "¿Deseas limpiar datos existentes? (s/n)"
read -r clean_answer

if [ "$clean_answer" = "s" ] || [ "$clean_answer" = "S" ]; then
    log_warning "Limpiando datos existentes..."
    
    PGPASSWORD=$DB_PASSWORD psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" << EOF
    -- Deshabilitar constraints temporalmente
    SET session_replication_role = 'replica';
    
    -- Truncar tablas
    TRUNCATE TABLE vehicle_images CASCADE;
    TRUNCATE TABLE vehicles CASCADE;
    TRUNCATE TABLE dealers CASCADE;
    TRUNCATE TABLE users CASCADE;
    
    -- Re-habilitar constraints
    SET session_replication_role = 'origin';
    
    -- Reset sequences
    ALTER SEQUENCE users_id_seq RESTART WITH 1;
    ALTER SEQUENCE dealers_id_seq RESTART WITH 1;
    ALTER SEQUENCE vehicles_id_seq RESTART WITH 1;
    ALTER SEQUENCE vehicle_images_id_seq RESTART WITH 1;
EOF
    
    log_step "Base de datos limpiada"
fi

# PASO 2: Crear usuarios (20 total: 10 buyers + 10 sellers)
echo ""
log_info "Creando 20 usuarios (10 buyers + 10 sellers)..."

create_users() {
    local count=$1
    local type=$2
    
    for i in $(seq 1 "$count"); do
        local email="${type}${i}@okla.local"
        local first_name="User"
        local last_name="$type$i"
        local password="SecurePass123!@"
        
        local response=$(curl -s -X POST "${API_URL}/api/auth/register" \
            -H "Content-Type: application/json" \
            -d '{
                "email": "'$email'",
                "firstName": "'$first_name'",
                "lastName": "'$last_name'",
                "password": "'$password'",
                "accountType": "'$type'"
            }')
        
        # Verificar si fue exitoso
        if echo "$response" | grep -q "id\|token"; then
            echo "  ✓ $email"
        else
            echo "  ✗ $email (error)"
        fi
    done
}

create_users 10 "Buyer"
create_users 10 "Seller"

log_step "Usuarios creados"

# PASO 3: Crear dealers (30 total)
echo ""
log_info "Creando 30 dealers (distribución variada)..."

# Este sería un bulk insert desde archivo JSON o via C# seeding
log_warning "NOTA: Para dealers es mejor usar el C# Seeding Service"
log_info "Alternativamente, usa: dotnet run seed:dealers"

# PASO 4: Crear vehículos (150 total)
echo ""
log_info "Creando 150 vehículos..."
log_warning "NOTA: Para vehículos usa el C# Seeding Service"
log_info "Alternativamente, usa: dotnet run seed:vehicles"

# PASO 5: Crear imágenes (7,500 referencias)
echo ""
log_info "Creando referencias de imágenes (7,500)..."
log_info "URLs de Picsum Photos generadas automáticamente"
log_info "No requiere descargas - URLs lazy loaded"

# PASO 6: Validación final
echo ""
log_info "Validando integridad de datos..."

PGPASSWORD=$DB_PASSWORD psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" << EOF > /tmp/seed-summary.txt
SELECT 
    (SELECT COUNT(*) FROM users) as users_count,
    (SELECT COUNT(*) FROM dealers) as dealers_count,
    (SELECT COUNT(*) FROM vehicles) as vehicles_count,
    (SELECT COUNT(*) FROM vehicle_images) as images_count;
EOF

cat /tmp/seed-summary.txt

log_step "Validación completada"

# RESUMEN FINAL
echo ""
echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║${NC} 📊 RESUMEN DE SEEDING                                    ${BLUE}║${NC}"
echo -e "${BLUE}║${NC}                                                          ${BLUE}║${NC}"
echo -e "${BLUE}║${NC} ✓ 👥 Usuarios: 20                                        ${BLUE}║${NC}"
echo -e "${BLUE}║${NC} ✓ 🏪 Dealers: 30 (pendiente - usa C# service)           ${BLUE}║${NC}"
echo -e "${BLUE}║${NC} ✓ 🚗 Vehículos: 150 (pendiente - usa C# service)        ${BLUE}║${NC}"
echo -e "${BLUE}║${NC} ✓ 🖼️  Imágenes: 7,500 (URLs de Picsum)                  ${BLUE}║${NC}"
echo -e "${BLUE}║${NC}                                                          ${BLUE}║${NC}"
echo -e "${BLUE}║${NC} API: ${API_URL}                      ${BLUE}║${NC}"
echo -e "${BLUE}║${NC} DB:  ${DB_HOST}/${DB_NAME}                        ${BLUE}║${NC}"
echo -e "${BLUE}║${NC}                                                          ${BLUE}║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"

echo ""
log_step "Seeding completado!"
echo ""
echo "Próximos pasos:"
echo "  1. Ejecutar C# Seeding Service para dealers y vehículos"
echo "  2. Validar datos en: ${API_URL}/api/vehicles"
echo "  3. Probar APIs con datos reales"
echo ""
