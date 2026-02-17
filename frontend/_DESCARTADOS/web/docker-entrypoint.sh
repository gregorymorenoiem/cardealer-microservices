#!/bin/sh
# =============================================================================
# Docker Entrypoint - CarDealer Frontend
# =============================================================================
# This script allows runtime environment variable injection into the
# static frontend build. It replaces placeholders in index.html with
# actual values from environment variables.
# =============================================================================

set -e

# Runtime environment variables (can override build-time values)
# These are useful for Kubernetes ConfigMaps/Secrets

# Create a config.js file with runtime variables
cat > /usr/share/nginx/html/config.js << EOF
window.__RUNTIME_CONFIG__ = {
  API_URL: "${RUNTIME_API_URL:-}",
  APP_VERSION: "${RUNTIME_APP_VERSION:-}",
  SENTRY_DSN: "${RUNTIME_SENTRY_DSN:-}",
  GA_TRACKING_ID: "${RUNTIME_GA_TRACKING_ID:-}",
  ENVIRONMENT: "${NODE_ENV:-production}"
};
EOF

echo "Runtime configuration created"

# Execute the main command (nginx)
exec "$@"
