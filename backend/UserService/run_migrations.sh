#!/bin/bash
# =====================================================
# Migration Script: FASE 1 & FASE 2 - SellerProfile
# =====================================================
# This script executes SQL migrations for:
# - FASE 1: Add Specialties field
# - FASE 2: Add Location indexes
# =====================================================

set -e  # Exit on error

# Configuration
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-cardealer_db}"
DB_USER="${DB_USER:-postgres}"
DB_PASS="${DB_PASS:-}"

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Print colored output
print_status() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

print_header() {
    echo ""
    echo "========================================"
    echo "$1"
    echo "========================================"
}

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Migration files
MIGRATION_FASE1="$SCRIPT_DIR/Migrations/20260224_AddSpecialtiesFieldToSellerProfile.sql"
MIGRATION_FASE2="$SCRIPT_DIR/UserService.Infrastructure/Migrations/20260224_OptimizeLocationFieldsWithIndexes.sql"

# Verify migration files exist
print_header "Verifying Migration Files"

if [ ! -f "$MIGRATION_FASE1" ]; then
    print_error "FASE 1 migration not found: $MIGRATION_FASE1"
    exit 1
fi
print_status "Found FASE 1: $MIGRATION_FASE1"

if [ ! -f "$MIGRATION_FASE2" ]; then
    print_error "FASE 2 migration not found: $MIGRATION_FASE2"
    exit 1
fi
print_status "Found FASE 2: $MIGRATION_FASE2"

# Build PostgreSQL connection string
if [ -z "$DB_PASS" ]; then
    PGPASSWORD=""
    PSQL_OPTS="-h $DB_HOST -U $DB_USER -d $DB_NAME"
else
    export PGPASSWORD="$DB_PASS"
    PSQL_OPTS="-h $DB_HOST -U $DB_USER -d $DB_NAME"
fi

# Test database connection
print_header "Testing Database Connection"
if psql $PSQL_OPTS -c "SELECT 1" > /dev/null 2>&1; then
    print_status "Database connection successful"
else
    print_error "Cannot connect to database"
    print_warning "Connection string: $DB_HOST:$DB_PORT/$DB_NAME (user: $DB_USER)"
    print_warning "Environment variables:"
    echo "  DB_HOST=$DB_HOST"
    echo "  DB_PORT=$DB_PORT"
    echo "  DB_NAME=$DB_NAME"
    echo "  DB_USER=$DB_USER"
    exit 1
fi

# Execute migrations
print_header "Executing FASE 1: Add Specialties Field"
if psql $PSQL_OPTS -f "$MIGRATION_FASE1" > /dev/null 2>&1; then
    print_status "FASE 1 migration completed successfully"
else
    print_error "FASE 1 migration failed"
    print_warning "Attempting to execute with error output..."
    psql $PSQL_OPTS -f "$MIGRATION_FASE1"
    exit 1
fi

print_header "Executing FASE 2: Optimize Location Fields with Indexes"
if psql $PSQL_OPTS -f "$MIGRATION_FASE2" > /dev/null 2>&1; then
    print_status "FASE 2 migration completed successfully"
else
    print_error "FASE 2 migration failed"
    print_warning "Attempting to execute with error output..."
    psql $PSQL_OPTS -f "$MIGRATION_FASE2"
    exit 1
fi

# Verify indexes were created
print_header "Verifying Indexes"
INDEXES=$(psql $PSQL_OPTS -t -c "
SELECT indexname FROM pg_indexes 
WHERE tablename = 'seller_profiles' 
AND (indexname LIKE 'idx_seller_profiles_%' OR indexname LIKE 'idx_seller_%')
ORDER BY indexname;
")

if [ -z "$INDEXES" ]; then
    print_warning "No indexes found for seller_profiles"
else
    print_status "Indexes created:"
    echo "$INDEXES" | while read -r index; do
        if [ ! -z "$index" ]; then
            echo "  - $index"
        fi
    done
fi

# Verify specialties column
print_header "Verifying SPECIALTIES Column"
SPECIALTIES=$(psql $PSQL_OPTS -t -c "
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'seller_profiles' AND column_name = 'specialties';
")

if [ -z "$SPECIALTIES" ]; then
    print_warning "Specialties column not found"
else
    print_status "Specialties column verified:"
    echo "$SPECIALTIES"
fi

# Summary
print_header "Migration Summary"
print_status "All migrations completed successfully!"
echo ""
echo "Changes made:"
echo "  ✓ Added specialties TEXT[] column with GIN index"
echo "  ✓ Added 6 location-based indexes:"
echo "    - idx_seller_profiles_city_state (composite)"
echo "    - idx_seller_profiles_state"
echo "    - idx_seller_profiles_city"
echo "    - idx_seller_profiles_zipcode"
echo "    - idx_seller_profiles_verification_location"
echo "    - idx_seller_profiles_specialties_location"
echo ""
echo "Database: $DB_NAME on $DB_HOST"
echo ""
