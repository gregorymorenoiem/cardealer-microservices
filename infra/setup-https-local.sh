#!/bin/bash
# ============================================================================
# OKLA — Setup HTTPS Local + Dominio Público
# ============================================================================
# Ejecutar una sola vez: ./infra/setup-https-local.sh
# Requiere: brew, Docker Desktop
# ============================================================================

set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
CERTS_DIR="$REPO_ROOT/infra/caddy/certs"

echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}  OKLA — Setup HTTPS Local + Dominio Público                  ${NC}"
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo ""

# ── 1. Verificar/instalar dependencias ─────────────────────────────────────
echo -e "${YELLOW}[1/5] Verificando dependencias...${NC}"

if ! command -v mkcert &> /dev/null; then
    echo "  → Instalando mkcert..."
    brew install mkcert
fi

if ! command -v cloudflared &> /dev/null; then
    echo "  → Instalando cloudflared..."
    brew install cloudflared
fi

echo -e "  ${GREEN}✓ mkcert $(mkcert --version)${NC}"
echo -e "  ${GREEN}✓ cloudflared $(cloudflared --version 2>&1 | head -1)${NC}"

# ── 2. Instalar CA raíz en el sistema ──────────────────────────────────────
echo ""
echo -e "${YELLOW}[2/5] Instalando CA raíz de mkcert (requiere contraseña)...${NC}"
mkcert -install
echo -e "  ${GREEN}✓ CA instalada en Keychain${NC}"

# ── 3. Generar certificados TLS ────────────────────────────────────────────
echo ""
echo -e "${YELLOW}[3/5] Generando certificados TLS para okla.local...${NC}"

mkdir -p "$CERTS_DIR"

if [ -f "$CERTS_DIR/okla.local.pem" ]; then
    echo -e "  ${GREEN}✓ Certificados ya existen — omitiendo${NC}"
else
    cd "$CERTS_DIR"
    mkcert -cert-file okla.local.pem -key-file okla.local-key.pem \
        "okla.local" "*.okla.local" "localhost" "127.0.0.1" "::1"
    echo -e "  ${GREEN}✓ Certificados generados${NC}"
fi

# Copiar CA raíz para Docker
cp "$(mkcert -CAROOT)/rootCA.pem" "$CERTS_DIR/rootCA.pem"
echo -e "  ${GREEN}✓ CA raíz copiada para Docker${NC}"

# ── 4. Configurar /etc/hosts ──────────────────────────────────────────────
echo ""
echo -e "${YELLOW}[4/5] Configurando /etc/hosts...${NC}"

if grep -q "okla.local" /etc/hosts; then
    echo -e "  ${GREEN}✓ okla.local ya existe en /etc/hosts${NC}"
else
    echo "  → Agregando okla.local a /etc/hosts (requiere contraseña)..."
    echo "127.0.0.1    okla.local" | sudo tee -a /etc/hosts > /dev/null
    echo -e "  ${GREEN}✓ okla.local agregado${NC}"
fi

# ── 5. Configurar .env ────────────────────────────────────────────────────
echo ""
echo -e "${YELLOW}[5/5] Verificando .env...${NC}"

if [ -f "$REPO_ROOT/.env" ]; then
    echo -e "  ${GREEN}✓ .env ya existe${NC}"

    # Verificar si tiene las URLs HTTPS
    if grep -q "NEXT_PUBLIC_API_URL=https://okla.local" "$REPO_ROOT/.env"; then
        echo -e "  ${GREEN}✓ URLs HTTPS ya configuradas${NC}"
    else
        echo -e "  ${YELLOW}⚠ .env existe pero no tiene URLs HTTPS.${NC}"
        echo -e "  ${YELLOW}  Edita manualmente o ejecuta:${NC}"
        echo -e "  ${YELLOW}  cp .env.local.example .env${NC}"
    fi
else
    echo "  → Copiando .env.local.example → .env"
    cp "$REPO_ROOT/.env.local.example" "$REPO_ROOT/.env"
    echo -e "  ${GREEN}✓ .env creado — editar con valores reales${NC}"
fi

# ── Resumen ────────────────────────────────────────────────────────────────
echo ""
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}  ✅ Setup completo${NC}"
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "  ${GREEN}HTTPS local:${NC}"
echo -e "    1. docker compose up -d       (levanta infra + Caddy)"
echo -e "    2. pnpm dev                   (Next.js en host)"
echo -e "    3. Abrir: ${GREEN}https://okla.local${NC}"
echo ""
echo -e "  ${GREEN}Dominio público temporal:${NC}"
echo -e "    1. docker compose --profile tunnel up -d cloudflared"
echo -e "    2. docker compose logs cloudflared | grep trycloudflare.com"
echo -e "    3. Copiar la URL ${GREEN}https://xxxx.trycloudflare.com${NC}"
echo ""
echo -e "  ${GREEN}UIs de desarrollo:${NC}"
echo -e "    - Swagger:   https://okla.local/api/auth/swagger"
echo -e "    - Seq (logs): http://localhost:5341"
echo -e "    - Jaeger:     http://localhost:16686"
echo -e "    - RabbitMQ:   http://localhost:15672"
echo ""
