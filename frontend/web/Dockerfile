# =============================================================================
# DOCKERFILE - CarDealer Frontend Web
# =============================================================================
# Multi-stage build for optimized production image
#
# Build: docker build -t cardealer-web .
# Run:   docker run -p 80:80 cardealer-web
#
# Build with args:
#   docker build \
#     --build-arg VITE_API_URL=https://api.cardealer.do \
#     --build-arg VITE_SENTRY_DSN=your_dsn \
#     -t cardealer-web .
# =============================================================================

# -----------------------------------------------------------------------------
# Stage 1: Build
# -----------------------------------------------------------------------------
FROM node:20-alpine AS builder

WORKDIR /app

# Install dependencies first (better caching)
COPY package*.json ./
RUN npm install --legacy-peer-deps

# Copy source code
COPY . .

# Build-time arguments (injected as VITE_* env vars)
ARG VITE_API_URL=http://localhost:8080
ARG VITE_APP_NAME=CarDealer
ARG VITE_APP_VERSION=1.0.0
ARG VITE_BASE_URL=https://cardealer.do
ARG VITE_SENTRY_DSN=
ARG VITE_GA_TRACKING_ID=
ARG VITE_GOOGLE_MAPS_KEY=
ARG VITE_RECAPTCHA_SITE_KEY=

# Set environment variables for build
ENV VITE_API_URL=${VITE_API_URL}
ENV VITE_APP_NAME=${VITE_APP_NAME}
ENV VITE_APP_VERSION=${VITE_APP_VERSION}
ENV VITE_BASE_URL=${VITE_BASE_URL}
ENV VITE_SENTRY_DSN=${VITE_SENTRY_DSN}
ENV VITE_GA_TRACKING_ID=${VITE_GA_TRACKING_ID}
ENV VITE_GOOGLE_MAPS_KEY=${VITE_GOOGLE_MAPS_KEY}
ENV VITE_RECAPTCHA_SITE_KEY=${VITE_RECAPTCHA_SITE_KEY}
ENV VITE_USE_MOCK_AUTH=false
ENV VITE_ENABLE_MSW=false

# Build the application
RUN npm run build

# -----------------------------------------------------------------------------
# Stage 2: Production
# -----------------------------------------------------------------------------
FROM nginx:alpine AS production

# Install envsubst for runtime environment variable substitution
RUN apk add --no-cache gettext

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf
COPY nginx-default.conf /etc/nginx/conf.d/default.conf

# Copy built assets from builder stage
COPY --from=builder /app/dist /usr/share/nginx/html

# Copy entrypoint script for runtime env substitution
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Security: Run as non-root user
RUN addgroup -g 1001 -S nginx-user && \
    adduser -S -D -H -u 1001 -h /var/cache/nginx -s /sbin/nologin -G nginx-user nginx-user && \
    chown -R nginx-user:nginx-user /var/cache/nginx /var/log/nginx /etc/nginx/conf.d /usr/share/nginx/html && \
    touch /var/run/nginx.pid && chown nginx-user:nginx-user /var/run/nginx.pid

# Expose port 8080 (non-privileged)
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget -qO- http://localhost:8080/health || exit 1

# Run as non-root
USER nginx-user

# Start nginx
ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
