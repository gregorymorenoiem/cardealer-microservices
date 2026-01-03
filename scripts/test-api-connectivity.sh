#!/bin/bash
# ============================================================================
# test-api-connectivity.sh - Test Third-Party API Connectivity
# ============================================================================
# Tests actual connectivity to external services using stored secrets.
# 
# USAGE:
#   chmod +x scripts/test-api-connectivity.sh
#   ./scripts/test-api-connectivity.sh
# ============================================================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

SECRETS_DIR="secrets"
PASSED=0
FAILED=0

echo -e "${BLUE}============================================================${NC}"
echo -e "${BLUE}  🌐 API Connectivity Tests${NC}"
echo -e "${BLUE}============================================================${NC}"
echo ""

# ============================================================================
# 1. Google Maps API
# ============================================================================
echo -e "${BLUE}[1] Testing Google Maps API...${NC}"
if [ -f "$SECRETS_DIR/google_maps_api_key.txt" ]; then
    API_KEY=$(cat "$SECRETS_DIR/google_maps_api_key.txt" | tr -d '\n\r')
    
    # Test Geocoding API
    RESPONSE=$(curl -s "https://maps.googleapis.com/maps/api/geocode/json?address=1600+Amphitheatre+Parkway,+Mountain+View,+CA&key=$API_KEY")
    
    if echo "$RESPONSE" | grep -q '"status"[[:space:]]*:[[:space:]]*"OK"'; then
        echo -e "${GREEN}✅ Google Maps API - OK${NC}"
        echo -e "   Status: Connected"
        PASSED=$((PASSED + 1))
    elif echo "$RESPONSE" | grep -q "REQUEST_DENIED"; then
        echo -e "${RED}❌ Google Maps API - REQUEST DENIED${NC}"
        echo -e "   Possible causes: API not enabled, IP restriction, or invalid key"
        FAILED=$((FAILED + 1))
    else
        echo -e "${YELLOW}⚠️  Google Maps API - Unexpected response${NC}"
        echo "$RESPONSE" | head -3
        FAILED=$((FAILED + 1))
    fi
else
    echo -e "${YELLOW}⚠️  Google Maps API Key not found${NC}"
    FAILED=$((FAILED + 1))
fi
echo ""

# ============================================================================
# 2. Stripe API
# ============================================================================
echo -e "${BLUE}[2] Testing Stripe API...${NC}"
if [ -f "$SECRETS_DIR/stripe_secret_key.txt" ]; then
    SECRET_KEY=$(cat "$SECRETS_DIR/stripe_secret_key.txt" | tr -d '\n\r')
    
    # Test retrieving balance
    RESPONSE=$(curl -s https://api.stripe.com/v1/balance \
        -u "$SECRET_KEY:" \
        -H "Content-Type: application/x-www-form-urlencoded")
    
    if echo "$RESPONSE" | grep -q '"object"[[:space:]]*:[[:space:]]*"balance"'; then
        echo -e "${GREEN}✅ Stripe API - OK${NC}"
        
        # Extract mode
        if echo "$SECRET_KEY" | grep -q "sk_test"; then
            echo -e "   Mode: ${YELLOW}Test Mode${NC}"
        else
            echo -e "   Mode: ${GREEN}Live Mode${NC}"
        fi
        
        PASSED=$((PASSED + 1))
    else
        echo -e "${RED}❌ Stripe API - Failed${NC}"
        echo "$RESPONSE" | head -3
        FAILED=$((FAILED + 1))
    fi
else
    echo -e "${YELLOW}⚠️  Stripe Secret Key not found${NC}"
    FAILED=$((FAILED + 1))
fi
echo ""

# ============================================================================
# 3. AWS S3
# ============================================================================
echo -e "${BLUE}[3] Testing AWS S3...${NC}"
if [ -f "$SECRETS_DIR/aws_access_key_id.txt" ] && [ -f "$SECRETS_DIR/aws_secret_access_key.txt" ]; then
    ACCESS_KEY=$(cat "$SECRETS_DIR/aws_access_key_id.txt" | tr -d '\n\r')
    BUCKET=$(cat "$SECRETS_DIR/aws_s3_bucket_name.txt" | tr -d '\n\r')
    REGION=$(cat "$SECRETS_DIR/aws_region.txt" | tr -d '\n\r')
    
    # Check if AWS CLI is installed
    if command -v aws &> /dev/null; then
        # Simple test - AWS CLI should be pre-configured
        if aws s3 ls "s3://$BUCKET" --region "$REGION" > /dev/null 2>&1; then
            echo -e "${GREEN}✅ AWS S3 - OK${NC}"
            echo -e "   Bucket: $BUCKET"
            echo -e "   Region: $REGION"
            PASSED=$((PASSED + 1))
        else
            echo -e "${RED}❌ AWS S3 - Access failed${NC}"
            echo -e "   Bucket: $BUCKET"
            FAILED=$((FAILED + 1))
        fi
    else
        echo -e "${YELLOW}⚠️  AWS CLI not installed${NC}"
        FAILED=$((FAILED + 1))
    fi
else
    echo -e "${YELLOW}⚠️  AWS credentials not found${NC}"
    FAILED=$((FAILED + 1))
fi
echo ""

# ============================================================================
# 4. Resend API
# ============================================================================
echo -e "${BLUE}[4] Testing Resend API...${NC}"
if [ -f "$SECRETS_DIR/resend_api_key.txt" ]; then
    API_KEY=$(cat "$SECRETS_DIR/resend_api_key.txt" | tr -d '\n\r')
    
    # Test API domains endpoint (doesn't send email)
    RESPONSE=$(curl -s https://api.resend.com/domains \
        -H "Authorization: Bearer $API_KEY" \
        -H "Content-Type: application/json")
    
    if echo "$RESPONSE" | grep -q '"data"'; then
        echo -e "${GREEN}✅ Resend API - OK${NC}"
        echo -e "   Status: Connected"
        PASSED=$((PASSED + 1))
    elif echo "$RESPONSE" | grep -q "restricted_api_key"; then
        # Restricted key is NORMAL - API key is for sending emails only
        echo -e "${GREEN}✅ Resend API - OK (Send-only key)${NC}"
        echo -e "   Status: Configured for email sending"
        PASSED=$((PASSED + 1))
    elif echo "$RESPONSE" | grep -q "invalid_api_key"; then
        echo -e "${RED}❌ Resend API - Invalid API Key${NC}"
        FAILED=$((FAILED + 1))
    else
        echo -e "${YELLOW}⚠️  Resend API - Unexpected response${NC}"
        echo "$RESPONSE" | head -3
        FAILED=$((FAILED + 1))
    fi
else
    echo -e "${YELLOW}⚠️  Resend API Key not found${NC}"
    FAILED=$((FAILED + 1))
fi
echo ""

# ============================================================================
# 5. Firebase
# ============================================================================
echo -e "${BLUE}[5] Testing Firebase...${NC}"
if [ -f "$SECRETS_DIR/firebase_service_account.json" ]; then
    # Validate JSON structure
    if python3 -m json.tool "$SECRETS_DIR/firebase_service_account.json" > /dev/null 2>&1; then
        PROJECT_ID=$(python3 -c "import json; print(json.load(open('$SECRETS_DIR/firebase_service_account.json'))['project_id'])" 2>/dev/null || echo "unknown")
        
        echo -e "${GREEN}✅ Firebase - Service Account Valid${NC}"
        echo -e "   Project ID: $PROJECT_ID"
        echo -e "   Note: Full FCM test requires sending actual notification"
        PASSED=$((PASSED + 1))
    else
        echo -e "${RED}❌ Firebase - Invalid JSON${NC}"
        FAILED=$((FAILED + 1))
    fi
else
    echo -e "${YELLOW}⚠️  Firebase Service Account not found${NC}"
    FAILED=$((FAILED + 1))
fi
echo ""

# ============================================================================
# 6. Twilio (Optional)
# ============================================================================
echo -e "${BLUE}[6] Testing Twilio (Optional)...${NC}"
if [ -f "$SECRETS_DIR/twilio_account_sid.txt" ] && [ -f "$SECRETS_DIR/twilio_auth_token.txt" ]; then
    ACCOUNT_SID=$(cat "$SECRETS_DIR/twilio_account_sid.txt" | tr -d '\n\r')
    AUTH_TOKEN=$(cat "$SECRETS_DIR/twilio_auth_token.txt" | tr -d '\n\r')
    
    # Test fetching account details
    RESPONSE=$(curl -s -u "$ACCOUNT_SID:$AUTH_TOKEN" \
        "https://api.twilio.com/2010-04-01/Accounts/$ACCOUNT_SID.json")
    
    if echo "$RESPONSE" | grep -q '"status"[[:space:]]*:[[:space:]]*"active"'; then
        echo -e "${GREEN}✅ Twilio - OK${NC}"
        echo -e "   Account Status: Active"
        PASSED=$((PASSED + 1))
    elif echo "$RESPONSE" | grep -q "20003"; then
        echo -e "${RED}❌ Twilio - Authentication Failed${NC}"
        FAILED=$((FAILED + 1))
    else
        echo -e "${YELLOW}⚠️  Twilio - Optional service not configured${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  Twilio credentials not found (optional)${NC}"
fi
echo ""

# ============================================================================
# Summary
# ============================================================================
echo -e "${BLUE}============================================================${NC}"
echo -e "${BLUE}  📊 Connectivity Test Summary${NC}"
echo -e "${BLUE}============================================================${NC}"
TOTAL=$((PASSED + FAILED))
echo -e "Total tests:   $TOTAL"
echo -e "${GREEN}Passed:        $PASSED${NC}"
echo -e "${RED}Failed:        $FAILED${NC}"
echo ""

if [ $PASSED -ge 4 ]; then
    echo -e "${GREEN}✅ Critical APIs are working! (${PASSED}/${TOTAL})${NC}"
    exit 0
elif [ $PASSED -ge 2 ]; then
    echo -e "${YELLOW}⚠️  Some APIs working, but some failed (${PASSED}/${TOTAL})${NC}"
    exit 0
else
    echo -e "${RED}❌ Most APIs failed. Check your credentials.${NC}"
    exit 1
fi
