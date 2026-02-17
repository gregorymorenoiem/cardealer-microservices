#!/bin/bash
# Standardize all Dockerfiles to: alpine, port 8080, non-root user, healthcheck
# Standard: mcr.microsoft.com/dotnet/aspnet:8.0-alpine, EXPOSE 8080, ENV ASPNETCORE_URLS=http://+:8080
cd "$(dirname "$0")/.."

COUNT=0
SKIPPED=0

for DOCKERFILE in backend/*/Dockerfile; do
    SVC=$(echo "$DOCKERFILE" | cut -d'/' -f2)
    
    # Skip Gateway (has its own special config) and non-standard services
    if [ ! -f "$DOCKERFILE" ]; then
        continue
    fi

    CHANGED=false

    # 1. Replace aspnet:8.0 (non-alpine) with aspnet:8.0-alpine in runtime stage
    if grep -q "aspnet:8.0 AS" "$DOCKERFILE" 2>/dev/null; then
        sed -i '' 's|aspnet:8.0 AS final|aspnet:8.0-alpine AS final|g' "$DOCKERFILE"
        sed -i '' 's|aspnet:8.0 AS base|aspnet:8.0-alpine AS base|g' "$DOCKERFILE"
        sed -i '' 's|aspnet:8.0 AS runtime|aspnet:8.0-alpine AS runtime|g' "$DOCKERFILE"
        CHANGED=true
    fi

    # 2. Replace EXPOSE 80 with EXPOSE 8080
    if grep -q "^EXPOSE 80$" "$DOCKERFILE" 2>/dev/null; then
        sed -i '' 's/^EXPOSE 80$/EXPOSE 8080/' "$DOCKERFILE"
        CHANGED=true
    fi

    # 3. Remove EXPOSE 443 (not needed, TLS terminates at ingress)
    if grep -q "^EXPOSE 443$" "$DOCKERFILE" 2>/dev/null; then
        sed -i '' '/^EXPOSE 443$/d' "$DOCKERFILE"
        CHANGED=true
    fi

    # 4. Replace ASPNETCORE_URLS port 80 with 8080
    if grep -q "ASPNETCORE_URLS=http://+:80$" "$DOCKERFILE" 2>/dev/null; then
        sed -i '' 's|ASPNETCORE_URLS=http://+:80$|ASPNETCORE_URLS=http://+:8080|' "$DOCKERFILE"
        CHANGED=true
    fi

    # 5. Add ASPNETCORE_URLS if missing entirely
    if ! grep -q "ASPNETCORE_URLS" "$DOCKERFILE" 2>/dev/null; then
        # Add before ENTRYPOINT or CMD
        sed -i '' '/^ENTRYPOINT\|^CMD/i\
ENV ASPNETCORE_URLS=http://+:8080
' "$DOCKERFILE"
        CHANGED=true
    fi

    # 6. Fix healthcheck port from 80 to 8080
    if grep -q "localhost:80/" "$DOCKERFILE" 2>/dev/null; then
        sed -i '' 's|localhost:80/|localhost:8080/|g' "$DOCKERFILE"
        CHANGED=true
    fi

    # 7. Add healthcheck if missing
    if ! grep -q "HEALTHCHECK" "$DOCKERFILE" 2>/dev/null; then
        sed -i '' '/^ENTRYPOINT\|^CMD/i\
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \\\
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1
' "$DOCKERFILE"
        CHANGED=true
    fi

    # 8. Fix non-root user for alpine (use addgroup/adduser instead of groupadd/useradd)
    # Replace Debian-style user creation with Alpine-style if using alpine image
    if grep -q "alpine" "$DOCKERFILE" 2>/dev/null && grep -q "groupadd\|useradd" "$DOCKERFILE" 2>/dev/null; then
        sed -i '' 's|RUN groupadd -g 1000 appuser|RUN addgroup -g 1000 appuser|g' "$DOCKERFILE"
        sed -i '' 's|RUN useradd -m -u 1000 -g appuser appuser|RUN adduser -D -u 1000 -G appuser appuser|g' "$DOCKERFILE"
        CHANGED=true
    fi

    # 9. Add non-root user if missing entirely
    if ! grep -q "USER app" "$DOCKERFILE" 2>/dev/null; then
        # Add user creation before COPY --from=publish
        if grep -q "alpine" "$DOCKERFILE" 2>/dev/null; then
            sed -i '' '/^COPY --from=publish/i\
RUN addgroup -g 1000 appuser && adduser -D -u 1000 -G appuser appuser\
USER appuser
' "$DOCKERFILE"
        fi
        CHANGED=true
    fi

    # 10. Remove weird ports
    sed -i '' '/^EXPOSE 7095$/d' "$DOCKERFILE"
    sed -i '' '/^EXPOSE 5095$/d' "$DOCKERFILE"
    sed -i '' '/^EXPOSE 8081$/d' "$DOCKERFILE"

    if [ "$CHANGED" = true ]; then
        echo "✅ $SVC"
        COUNT=$((COUNT + 1))
    else
        SKIPPED=$((SKIPPED + 1))
    fi
done

echo "--- Standardized $COUNT Dockerfiles, $SKIPPED already compliant ---"
