#!/bin/bash
# =============================================================================
# Consolidate master .sln with all active .csproj files
# Excludes _DESCARTADOS (discarded services) and corrupted files
# =============================================================================
set -e

SLN_DIR="/Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices"
SLN_FILE="$SLN_DIR/cardealer.sln"

cd "$SLN_DIR"

echo "=== Consolidating master cardealer.sln ==="
echo "Current projects in .sln: $(grep -c '\.csproj' "$SLN_FILE" 2>/dev/null || echo 0)"

# Find all active .csproj files, excluding discarded/corrupted
PROJECTS=$(find backend -name '*.csproj' \
  -not -path '*_DESCARTADOS*' \
  -not -path '*_REMOVED*' \
  -not -name '*.!*' \
  | sort)

TOTAL=$(echo "$PROJECTS" | wc -l | tr -d ' ')
echo "Active .csproj files found: $TOTAL"

ADDED=0
SKIPPED=0
FAILED=0

for proj in $PROJECTS; do
  # Check if already in .sln
  if grep -q "$(basename "$proj")" "$SLN_FILE" 2>/dev/null; then
    ((SKIPPED++))
    continue
  fi
  
  if dotnet sln "$SLN_FILE" add "$proj" 2>/dev/null; then
    ((ADDED++))
  else
    echo "  FAILED: $proj"
    ((FAILED++))
  fi
done

FINAL=$(grep -c '\.csproj' "$SLN_FILE" 2>/dev/null || echo 0)
echo ""
echo "=== Summary ==="
echo "Added:   $ADDED"
echo "Skipped: $SKIPPED (already in .sln)"
echo "Failed:  $FAILED"
echo "Total projects in .sln: $FINAL"
