#!/usr/bin/env bash
# ============================================================================
# OKLA LLM Model Downloader
# ============================================================================
# Downloads the GGUF model required by the LLM inference server.
#
# Usage:
#   ./download-model.sh                     # Download default model
#   ./download-model.sh --output /my/path   # Custom output directory
#   ./download-model.sh --url <URL>         # Custom download URL
#
# Environment variables (override defaults):
#   MODEL_URL        - Direct URL to the GGUF file
#   MODEL_DIR        - Directory to save the model (default: ./models)
#   MODEL_FILENAME   - Filename for the saved model
#   HF_TOKEN         - HuggingFace token for private/gated repos
# ============================================================================
set -euo pipefail

# ── Defaults ──────────────────────────────────────────────────────────────────
DEFAULT_MODEL_URL="https://huggingface.co/unsloth/Meta-Llama-3.1-8B-Instruct-GGUF/resolve/main/Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf"
DEFAULT_MODEL_DIR="$(cd "$(dirname "$0")" && pwd)/models"
DEFAULT_MODEL_FILENAME="okla-llama3-8b-q4_k_m.gguf"
EXPECTED_MIN_SIZE_MB=4000  # Minimum expected file size (~4.5 GB for Q4_K_M)

# ── Colors ────────────────────────────────────────────────────────────────────
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# ── Parse arguments ──────────────────────────────────────────────────────────
MODEL_URL="${MODEL_URL:-$DEFAULT_MODEL_URL}"
MODEL_DIR="${MODEL_DIR:-$DEFAULT_MODEL_DIR}"
MODEL_FILENAME="${MODEL_FILENAME:-$DEFAULT_MODEL_FILENAME}"

while [[ $# -gt 0 ]]; do
    case "$1" in
        --url)
            MODEL_URL="$2"
            shift 2
            ;;
        --output)
            MODEL_DIR="$2"
            shift 2
            ;;
        --filename)
            MODEL_FILENAME="$2"
            shift 2
            ;;
        --help|-h)
            echo "Usage: $0 [--url <URL>] [--output <DIR>] [--filename <NAME>]"
            echo ""
            echo "Options:"
            echo "  --url       Direct download URL for the GGUF model"
            echo "  --output    Directory to save the model (default: ./models)"
            echo "  --filename  Output filename (default: okla-llama3-8b-q4_k_m.gguf)"
            echo ""
            echo "Environment variables:"
            echo "  MODEL_URL        Override download URL"
            echo "  MODEL_DIR        Override output directory"
            echo "  MODEL_FILENAME   Override output filename"
            echo "  HF_TOKEN         HuggingFace API token for gated models"
            exit 0
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            exit 1
            ;;
    esac
done

MODEL_PATH="${MODEL_DIR}/${MODEL_FILENAME}"

# ── Functions ─────────────────────────────────────────────────────────────────
log_info()  { echo -e "${BLUE}[INFO]${NC}  $1"; }
log_ok()    { echo -e "${GREEN}[OK]${NC}    $1"; }
log_warn()  { echo -e "${YELLOW}[WARN]${NC}  $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

check_dependencies() {
    local missing=()
    for cmd in curl sha256sum; do
        if ! command -v "$cmd" &>/dev/null; then
            missing+=("$cmd")
        fi
    done
    # sha256sum fallback on macOS
    if ! command -v sha256sum &>/dev/null && command -v shasum &>/dev/null; then
        sha256sum() { shasum -a 256 "$@"; }
        export -f sha256sum 2>/dev/null || true
    fi
    if ! command -v curl &>/dev/null; then
        log_error "curl is required but not installed."
        exit 1
    fi
}

get_file_size_mb() {
    local file="$1"
    if [[ "$(uname)" == "Darwin" ]]; then
        stat -f%z "$file" 2>/dev/null | awk '{printf "%.0f", $1/1048576}'
    else
        stat --printf="%s" "$file" 2>/dev/null | awk '{printf "%.0f", $1/1048576}'
    fi
}

# ── Main ──────────────────────────────────────────────────────────────────────
echo ""
echo -e "${BLUE}╔══════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║        OKLA LLM Model Downloader                       ║${NC}"
echo -e "${BLUE}╚══════════════════════════════════════════════════════════╝${NC}"
echo ""

check_dependencies

# Check if model already exists
if [[ -f "$MODEL_PATH" ]]; then
    existing_size=$(get_file_size_mb "$MODEL_PATH")
    if [[ "$existing_size" -ge "$EXPECTED_MIN_SIZE_MB" ]]; then
        log_ok "Model already exists: ${MODEL_PATH} (${existing_size} MB)"
        echo ""
        echo -e "${GREEN}Model is ready. No download needed.${NC}"
        echo "To force re-download, delete the file and run again."
        exit 0
    else
        log_warn "Existing file is too small (${existing_size} MB < ${EXPECTED_MIN_SIZE_MB} MB). Re-downloading..."
        rm -f "$MODEL_PATH"
    fi
fi

# Create output directory
mkdir -p "$MODEL_DIR"

log_info "Download URL:  ${MODEL_URL}"
log_info "Output path:   ${MODEL_PATH}"
echo ""

# Build curl args
CURL_ARGS=(
    --location
    --progress-bar
    --fail
    --output "$MODEL_PATH"
)

# Add HuggingFace token if provided (for gated models like Llama)
if [[ -n "${HF_TOKEN:-}" ]]; then
    log_info "Using HuggingFace token for authentication"
    CURL_ARGS+=(--header "Authorization: Bearer ${HF_TOKEN}")
fi

# Download
log_info "Downloading model... (this may take 10-30 minutes depending on connection)"
echo ""

if curl "${CURL_ARGS[@]}" "$MODEL_URL"; then
    echo ""
    downloaded_size=$(get_file_size_mb "$MODEL_PATH")
    
    if [[ "$downloaded_size" -ge "$EXPECTED_MIN_SIZE_MB" ]]; then
        log_ok "Download complete! (${downloaded_size} MB)"
    else
        log_error "Downloaded file is too small (${downloaded_size} MB). Expected at least ${EXPECTED_MIN_SIZE_MB} MB."
        log_error "The download may have been interrupted or the URL may be incorrect."
        rm -f "$MODEL_PATH"
        exit 1
    fi
else
    echo ""
    log_error "Download failed! Check the URL and your internet connection."
    log_error "If the model is gated, set HF_TOKEN environment variable."
    rm -f "$MODEL_PATH"
    exit 1
fi

# Generate checksum
if command -v sha256sum &>/dev/null; then
    CHECKSUM=$(sha256sum "$MODEL_PATH" | cut -d' ' -f1)
    echo "$CHECKSUM  $MODEL_FILENAME" > "${MODEL_DIR}/${MODEL_FILENAME}.sha256"
    log_ok "Checksum saved: ${MODEL_DIR}/${MODEL_FILENAME}.sha256"
elif command -v shasum &>/dev/null; then
    CHECKSUM=$(shasum -a 256 "$MODEL_PATH" | cut -d' ' -f1)
    echo "$CHECKSUM  $MODEL_FILENAME" > "${MODEL_DIR}/${MODEL_FILENAME}.sha256"
    log_ok "Checksum saved: ${MODEL_DIR}/${MODEL_FILENAME}.sha256"
fi

echo ""
echo -e "${GREEN}╔══════════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║  ✅ Model downloaded successfully!                      ║${NC}"
echo -e "${GREEN}║                                                         ║${NC}"
echo -e "${GREEN}║  To start the server:                                   ║${NC}"
echo -e "${GREEN}║    python server.py --model ${MODEL_PATH}               ║${NC}"
echo -e "${GREEN}║                                                         ║${NC}"
echo -e "${GREEN}║  Or with Docker:                                        ║${NC}"
echo -e "${GREEN}║    docker run -v \$(pwd)/models:/models okla-llm-server  ║${NC}"
echo -e "${GREEN}╚══════════════════════════════════════════════════════════╝${NC}"
echo ""
