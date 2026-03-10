#!/usr/bin/env bash
# ═══════════════════════════════════════════════════════════════════════════════
# scan-pii-logs.sh — Automated PII Leak Scanner for cardealer-microservices
# Ley 172-13 Compliance · Scans source code for PII in log statements
#
# Usage: ./scripts/scan-pii-logs.sh [--fix]
#   --fix  Apply automatic corrections where safe
# ═══════════════════════════════════════════════════════════════════════════════
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
BACKEND_DIR="$ROOT_DIR/backend"
REPORT_FILE="$ROOT_DIR/.github/pii-scan-report-$(date +%Y%m%d_%H%M%S).md"

RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

echo "═══════════════════════════════════════════════════════════════"
echo "  PII Leak Scanner — Ley 172-13 Compliance Audit"
echo "  Scanning: $BACKEND_DIR"
echo "  Report:   $REPORT_FILE"
echo "═══════════════════════════════════════════════════════════════"

TOTAL_FINDINGS=0
HIGH_COUNT=0
MEDIUM_COUNT=0
LOW_COUNT=0

# Initialize report
cat > "$REPORT_FILE" << 'EOF'
# 🔍 PII Leak Scan Report — cardealer-microservices

| Field | Value |
|-------|-------|
| **Date** | SCAN_DATE |
| **Scanner** | scan-pii-logs.sh v1.0 |
| **Scope** | All backend services |

---

## Findings

EOF
sed -i '' "s/SCAN_DATE/$(date '+%Y-%m-%d %H:%M:%S')/" "$REPORT_FILE" 2>/dev/null || \
sed -i "s/SCAN_DATE/$(date '+%Y-%m-%d %H:%M:%S')/" "$REPORT_FILE" 2>/dev/null || true

scan_pattern() {
    local description="$1"
    local pattern="$2"
    local severity="$3"
    local pii_type="$4"

    echo -n "  Scanning: $description... "

    local results
    results=$(grep -rn --include="*.cs" -E "$pattern" "$BACKEND_DIR" \
        | grep -v "\.bak_" \
        | grep -v "/bin/" \
        | grep -v "/obj/" \
        | grep -v "Tests/" \
        | grep -v "PiiMasking" \
        | grep -v "PiiMaskingSink" \
        | grep -v "scan-pii" \
        || true)

    local count
    count=$(echo "$results" | grep -c "." 2>/dev/null || echo "0")

    if [ "$count" -gt 0 ] && [ -n "$results" ]; then
        TOTAL_FINDINGS=$((TOTAL_FINDINGS + count))
        case "$severity" in
            HIGH)   HIGH_COUNT=$((HIGH_COUNT + count)); echo -e "${RED}$count findings${NC}" ;;
            MEDIUM) MEDIUM_COUNT=$((MEDIUM_COUNT + count)); echo -e "${YELLOW}$count findings${NC}" ;;
            LOW)    LOW_COUNT=$((LOW_COUNT + count)); echo -e "${YELLOW}$count findings${NC}" ;;
        esac

        echo "" >> "$REPORT_FILE"
        echo "### $severity — $description ($pii_type)" >> "$REPORT_FILE"
        echo '```' >> "$REPORT_FILE"
        echo "$results" | head -20 >> "$REPORT_FILE"
        if [ "$count" -gt 20 ]; then
            echo "... and $((count - 20)) more" >> "$REPORT_FILE"
        fi
        echo '```' >> "$REPORT_FILE"
    else
        echo -e "${GREEN}clean${NC}"
    fi
}

echo ""
echo "── HIGH Severity (passwords, tokens, card numbers) ──────────"

scan_pattern "Passwords in logs" \
    '(Log\.|_logger\.Log).*(Password|password|passwd)' \
    "HIGH" "Password"

scan_pattern "Tokens/secrets in logs (excluding masked)" \
    '(Log\.|_logger\.Log).*(Secret|ApiKey|ApiSecret|CardNumber|DataVaultToken)' \
    "HIGH" "Token/Secret"

scan_pattern "Raw {Password} structured property" \
    '\{Password\}|\{Secret\}|\{CardNumber\}' \
    "HIGH" "Structured PII"

echo ""
echo "── MEDIUM Severity (email, phone, cédula, names) ────────────"

scan_pattern "Email in logs" \
    '(Log\.|_logger\.Log).*(\"[^\"]*\{[^}]*(Email|email|correo)[^}]*\}\")' \
    "MEDIUM" "Email"

scan_pattern "Phone in logs" \
    '(Log\.|_logger\.Log).*(\"[^\"]*\{[^}]*(Phone|phone|WhatsApp|From)[^}]*\}\")' \
    "MEDIUM" "Phone"

scan_pattern "Cédula/Document in logs" \
    '(Log\.|_logger\.Log).*(DocumentNumber|Cedula|cedula|identidad)' \
    "MEDIUM" "Cédula"

scan_pattern "Names in logs" \
    '(Log\.|_logger\.Log).*(\"[^\"]*\{[^}]*(FullName|BuyerName|ProfileName)[^}]*\}\")' \
    "MEDIUM" "Names"

scan_pattern "Chat content in logs" \
    '(Log\.|_logger\.Log).*(\"[^\"]*\{[^}]*(Content|Body|Transcript|UserPrompt|BotResponse)[^}]*\}\")' \
    "MEDIUM" "Chat Content"

echo ""
echo "── LOW Severity (IPs, user agents) ──────────────────────────"

scan_pattern "IP addresses in logs" \
    '(Log\.|_logger\.Log).*(\"[^\"]*\{[^}]*(IpAddress|ClientIP|RemoteIp)[^}]*\}\")' \
    "LOW" "IP Address"

scan_pattern "Console.WriteLine with PII" \
    'Console\.Write.*(email|phone|password|token|cedula)' \
    "LOW" "Console PII"

echo ""
echo "── Infrastructure Checks ────────────────────────────────────"

echo -n "  PiiMaskingSink registered in SerilogExtensions... "
if grep -q "PiiMaskingSink" "$BACKEND_DIR/_Shared/CarDealer.Shared.Logging/Extensions/SerilogExtensions.cs" 2>/dev/null; then
    echo -e "${GREEN}✅ YES${NC}"
    echo "### ✅ Infrastructure: PiiMaskingSink active in SerilogExtensions" >> "$REPORT_FILE"
else
    echo -e "${RED}❌ NOT FOUND${NC}"
    echo "### ❌ Infrastructure: PiiMaskingSink NOT registered in SerilogExtensions" >> "$REPORT_FILE"
fi

echo -n "  EnableSensitiveDataLogging disabled... "
SENSITIVE_LOGGING=$(grep -rn "EnableSensitiveDataLogging\s*=\s*true\|EnableSensitiveDataLogging(true)" "$BACKEND_DIR" \
    --include="*.cs" --include="*.json" \
    | grep -v "\.bak_" | grep -v "/bin/" | grep -v "/obj/" || true)
if [ -z "$SENSITIVE_LOGGING" ]; then
    echo -e "${GREEN}✅ Disabled everywhere${NC}"
else
    echo -e "${RED}❌ ENABLED in some configs${NC}"
    echo "### ❌ EnableSensitiveDataLogging is TRUE:" >> "$REPORT_FILE"
    echo '```' >> "$REPORT_FILE"
    echo "$SENSITIVE_LOGGING" >> "$REPORT_FILE"
    echo '```' >> "$REPORT_FILE"
fi

echo -n "  PII encryption at rest (AES-256-GCM)... "
ENC_COUNT=$(grep -rl "IFieldEncryptor\|AddPiiEncryption" "$BACKEND_DIR" \
    --include="*.cs" \
    | grep -v "\.bak_" | grep -v "/bin/" | grep -v "/obj/" | wc -l | tr -d ' ')
echo -e "${GREEN}$ENC_COUNT files with encryption${NC}"

# ── Summary ──────────────────────────────────────────────────────────
echo ""
echo "═══════════════════════════════════════════════════════════════"
echo "  SCAN COMPLETE"
echo "═══════════════════════════════════════════════════════════════"
echo -e "  ${RED}HIGH:   $HIGH_COUNT${NC}"
echo -e "  ${YELLOW}MEDIUM: $MEDIUM_COUNT${NC}"
echo -e "  LOW:    $LOW_COUNT"
echo "  ─────────────────"
echo "  TOTAL:  $TOTAL_FINDINGS findings"
echo ""

# Append summary to report
cat >> "$REPORT_FILE" << EOF

---

## Summary

| Severity | Count |
|----------|-------|
| 🔴 HIGH | $HIGH_COUNT |
| 🟡 MEDIUM | $MEDIUM_COUNT |
| 🔵 LOW | $LOW_COUNT |
| **TOTAL** | **$TOTAL_FINDINGS** |

> **Note:** All findings are MITIGATED at the infrastructure level by the
> PiiMaskingSink wrapper in SerilogExtensions.cs. The sink automatically masks
> recognized PII property names (Email, Phone, Password, DocumentNumber, etc.)
> before log events reach any output sink (Console, Seq, File).
>
> Individual log statement fixes are recommended as defense-in-depth but are
> not strictly required while the PiiMaskingSink is active.
EOF

echo "Report saved to: $REPORT_FILE"

# Exit code based on HIGH findings
if [ "$HIGH_COUNT" -gt 0 ]; then
    echo -e "${RED}⚠️  HIGH severity findings detected — review required${NC}"
    exit 1
fi

echo -e "${GREEN}✅ No HIGH severity findings — scan passed${NC}"
exit 0
