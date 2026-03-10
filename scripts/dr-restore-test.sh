#!/bin/bash
# ═══════════════════════════════════════════════════════════════════════════════
# DR Restore Test Script — OKLA (CarDealer Microservices)
# ═══════════════════════════════════════════════════════════════════════════════
# Purpose: Monthly automated test to verify backup integrity and restore process
# Usage:   ./scripts/dr-restore-test.sh [--database <name>] [--all]
# Requires: s3cmd, psql, pg_restore, kubectl (configured for DOKS)
# ═══════════════════════════════════════════════════════════════════════════════
set -euo pipefail

# ── Configuration ────────────────────────────────────────────────────────────
DO_SPACES_BUCKET="${DO_SPACES_BUCKET:-okla-dr-backups}"
DO_SPACES_REGION="${DO_SPACES_REGION:-sfo3}"
DO_SPACES_HOST="${DO_SPACES_REGION}.digitaloceanspaces.com"
RESTORE_DIR="/tmp/okla-dr-test-$(date +%Y%m%d_%H%M%S)"
ALL_DATABASES="authservice userservice roleservice vehiclessaleservice mediaservice billingservice notificationservice errorservice reviewservice adminservice auditservice contactservice"
TEST_DB_SUFFIX="_dr_test"
START_TIME=$(date +%s)
PASS=0
FAIL=0
SKIP=0

# ── Color output ─────────────────────────────────────────────────────────────
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_ok()   { echo -e "${GREEN}✅ $1${NC}"; }
log_fail() { echo -e "${RED}❌ $1${NC}"; }
log_warn() { echo -e "${YELLOW}⚠️  $1${NC}"; }
log_info() { echo -e "ℹ️  $1"; }

# ── Parse arguments ──────────────────────────────────────────────────────────
TARGET_DB=""
TEST_ALL=false

while [[ $# -gt 0 ]]; do
  case $1 in
    --database) TARGET_DB="$2"; shift 2 ;;
    --all) TEST_ALL=true; shift ;;
    --help)
      echo "Usage: $0 [--database <name>] [--all]"
      echo "  --database <name>  Test restore for a specific database"
      echo "  --all              Test restore for all 12 databases"
      echo "  (default)          Test restore for authservice only"
      exit 0
      ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

if [ "$TEST_ALL" = true ]; then
  DATABASES="$ALL_DATABASES"
elif [ -n "$TARGET_DB" ]; then
  DATABASES="$TARGET_DB"
else
  DATABASES="authservice" # Default: test one critical service
fi

# ── Pre-flight checks ───────────────────────────────────────────────────────
echo "════════════════════════════════════════════════════════════════"
echo "🧪 DR Restore Test — $(date '+%Y-%m-%d %H:%M:%S')"
echo "   Databases to test: $(echo $DATABASES | wc -w | tr -d ' ')"
echo "   Backup bucket: s3://$DO_SPACES_BUCKET"
echo "   Restore dir: $RESTORE_DIR"
echo "════════════════════════════════════════════════════════════════"

# Check required tools
for CMD in s3cmd psql pg_restore; do
  if ! command -v $CMD &> /dev/null; then
    log_fail "$CMD not found. Install it first."
    exit 1
  fi
done

# Get DB connection from K8s secret (or env vars)
if [ -z "${POSTGRES_HOST:-}" ]; then
  if command -v kubectl &> /dev/null; then
    log_info "Reading DB credentials from K8s secret..."
    POSTGRES_HOST=$(kubectl get secret database-secrets -n okla -o jsonpath='{.data.POSTGRES_HOST}' | base64 -d)
    POSTGRES_PORT=$(kubectl get secret database-secrets -n okla -o jsonpath='{.data.POSTGRES_PORT}' | base64 -d)
    POSTGRES_USER=$(kubectl get secret database-secrets -n okla -o jsonpath='{.data.POSTGRES_USER}' | base64 -d)
    export PGPASSWORD=$(kubectl get secret database-secrets -n okla -o jsonpath='{.data.POSTGRES_PASSWORD}' | base64 -d)
  else
    log_fail "Set POSTGRES_HOST, POSTGRES_PORT, POSTGRES_USER, PGPASSWORD env vars"
    exit 1
  fi
fi

mkdir -p "$RESTORE_DIR"

# ── Test each database ───────────────────────────────────────────────────────
for DB in $DATABASES; do
  echo ""
  echo "────────────────────────────────────────────────────────────────"
  echo "🧪 Testing: $DB"
  echo "────────────────────────────────────────────────────────────────"

  TEST_DB="${DB}${TEST_DB_SUFFIX}"
  DB_START=$(date +%s)

  # Step 1: Find latest backup in DO Spaces
  log_info "Step 1: Finding latest backup for $DB..."
  LATEST_BACKUP=$(s3cmd ls "s3://${DO_SPACES_BUCKET}/postgres/${DB}/" \
    --host="$DO_SPACES_HOST" \
    --host-bucket="%(bucket)s.${DO_SPACES_HOST}" \
    2>/dev/null | sort | tail -1 | awk '{print $4}')

  if [ -z "$LATEST_BACKUP" ]; then
    log_fail "No backup found for $DB in DO Spaces!"
    FAIL=$((FAIL + 1))
    continue
  fi

  BACKUP_DATE=$(echo "$LATEST_BACKUP" | grep -oP '\d{8}' | head -1)
  log_ok "Found backup: $LATEST_BACKUP (date: $BACKUP_DATE)"

  # Check backup age
  BACKUP_AGE_DAYS=$(( ($(date +%s) - $(date -d "${BACKUP_DATE:0:4}-${BACKUP_DATE:4:2}-${BACKUP_DATE:6:2}" +%s 2>/dev/null || echo $(date +%s))) / 86400 ))
  if [ "$BACKUP_AGE_DAYS" -gt 2 ]; then
    log_warn "Backup is $BACKUP_AGE_DAYS days old (expected < 2 for daily backups)"
  fi

  # Step 2: Download backup
  log_info "Step 2: Downloading backup..."
  LOCAL_DUMP="$RESTORE_DIR/${DB}_test.dump"
  if s3cmd get "$LATEST_BACKUP" "$LOCAL_DUMP" \
    --host="$DO_SPACES_HOST" \
    --host-bucket="%(bucket)s.${DO_SPACES_HOST}" \
    --force 2>/dev/null; then
    DUMP_SIZE=$(ls -lh "$LOCAL_DUMP" | awk '{print $5}')
    log_ok "Downloaded: $DUMP_SIZE"
  else
    log_fail "Download failed for $DB"
    FAIL=$((FAIL + 1))
    continue
  fi

  # Step 3: Verify dump integrity (pg_restore --list)
  log_info "Step 3: Verifying dump integrity..."
  TABLE_COUNT=$(pg_restore --list "$LOCAL_DUMP" 2>/dev/null | grep -c "TABLE " || echo "0")
  if [ "$TABLE_COUNT" -gt 0 ]; then
    log_ok "Dump valid: $TABLE_COUNT tables found"
  else
    log_fail "Dump appears empty or corrupted (0 tables)"
    FAIL=$((FAIL + 1))
    continue
  fi

  # Step 4: Create test database and restore
  log_info "Step 4: Creating test database '$TEST_DB' and restoring..."
  psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d defaultdb \
    -c "DROP DATABASE IF EXISTS $TEST_DB;" 2>/dev/null
  psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d defaultdb \
    -c "CREATE DATABASE $TEST_DB;" 2>/dev/null

  RESTORE_START=$(date +%s)
  if pg_restore -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" \
    -d "$TEST_DB" --no-owner --no-privileges --clean --if-exists \
    "$LOCAL_DUMP" 2>/tmp/restore_err_${DB}.log; then
    RESTORE_SECS=$(( $(date +%s) - RESTORE_START ))
    log_ok "Restore completed in ${RESTORE_SECS}s"
  else
    RESTORE_SECS=$(( $(date +%s) - RESTORE_START ))
    # pg_restore may return non-zero for warnings — check if data exists
    RESTORED_TABLES=$(psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d "$TEST_DB" \
      -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public';" 2>/dev/null | tr -d ' ')
    if [ "${RESTORED_TABLES:-0}" -gt 0 ]; then
      log_warn "Restore completed with warnings (${RESTORE_SECS}s) — $RESTORED_TABLES tables restored"
    else
      log_fail "Restore failed — see /tmp/restore_err_${DB}.log"
      FAIL=$((FAIL + 1))
      # Cleanup test DB
      psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d defaultdb \
        -c "DROP DATABASE IF EXISTS $TEST_DB;" 2>/dev/null
      continue
    fi
  fi

  # Step 5: Verify restored data
  log_info "Step 5: Verifying restored data..."
  ROW_COUNT=$(psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d "$TEST_DB" \
    -t -c "SELECT SUM(n_live_tup) FROM pg_stat_user_tables;" 2>/dev/null | tr -d ' ')
  RESTORED_TABLES=$(psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d "$TEST_DB" \
    -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public';" 2>/dev/null | tr -d ' ')

  log_ok "Restored: $RESTORED_TABLES tables, ~${ROW_COUNT:-0} total rows"

  # Step 6: Cleanup test database
  log_info "Step 6: Cleaning up test database..."
  psql -h "$POSTGRES_HOST" -p "$POSTGRES_PORT" -U "$POSTGRES_USER" -d defaultdb \
    -c "DROP DATABASE IF EXISTS $TEST_DB;" 2>/dev/null
  log_ok "Test database dropped"

  DB_ELAPSED=$(( $(date +%s) - DB_START ))
  log_ok "$DB — PASSED (${DB_ELAPSED}s total)"
  PASS=$((PASS + 1))
done

# ── Summary ──────────────────────────────────────────────────────────────────
TOTAL_ELAPSED=$(( $(date +%s) - START_TIME ))
TOTAL_MIN=$((TOTAL_ELAPSED / 60))
TOTAL_SEC=$((TOTAL_ELAPSED % 60))

echo ""
echo "════════════════════════════════════════════════════════════════"
echo "📊 DR Restore Test Summary"
echo "   ✅ Passed: $PASS"
echo "   ❌ Failed: $FAIL"
echo "   ⏭️  Skipped: $SKIP"
echo "   ⏱️  Total time: ${TOTAL_MIN}m ${TOTAL_SEC}s"
echo "   📁 Restore dir: $RESTORE_DIR"
echo "════════════════════════════════════════════════════════════════"

# Cleanup restore directory
rm -rf "$RESTORE_DIR"

if [ "$FAIL" -gt 0 ]; then
  log_fail "DR TEST FAILED — $FAIL databases could not be restored!"
  exit 1
else
  log_ok "DR TEST PASSED — All $PASS databases restored and verified successfully"
  echo ""
  echo "Estimated RTO for full restore: ~$((TOTAL_ELAPSED * 12 / $(echo $DATABASES | wc -w))) seconds"
fi
