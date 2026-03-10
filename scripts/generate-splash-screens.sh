#!/bin/bash
# Generate Apple PWA splash screens for OKLA
# These are simple branded placeholders — replace with designed assets later

SPLASH_DIR="frontend/web-next/public/splash"
mkdir -p "$SPLASH_DIR"

# Brand color: #00A870 (OKLA green)
# Common iPhone sizes
declare -a SIZES=(
  "640x1136"
  "750x1334"
  "828x1792"
  "1125x2436"
  "1170x2532"
  "1179x2556"
  "1242x2688"
  "1284x2778"
  "1290x2796"
  "1536x2048"
  "1668x2388"
  "2048x2732"
)

for SIZE in "${SIZES[@]}"; do
  FILE="$SPLASH_DIR/apple-splash-${SIZE}.png"
  if [ ! -f "$FILE" ]; then
    # Create a simple SVG and convert, or just create a minimal placeholder
    W=$(echo "$SIZE" | cut -dx -f1)
    H=$(echo "$SIZE" | cut -dx -f2)

    # Create SVG splash screen
    cat > "/tmp/splash-${SIZE}.svg" <<EOF
<svg xmlns="http://www.w3.org/2000/svg" width="${W}" height="${H}" viewBox="0 0 ${W} ${H}">
  <rect width="${W}" height="${H}" fill="#ffffff"/>
  <text x="50%" y="45%" text-anchor="middle" font-family="system-ui, -apple-system, sans-serif" font-size="$(( W / 8 ))" font-weight="bold" fill="#00A870">OKLA</text>
  <text x="50%" y="55%" text-anchor="middle" font-family="system-ui, -apple-system, sans-serif" font-size="$(( W / 16 ))" fill="#666666">Marketplace de Vehículos</text>
</svg>
EOF

    # Try to convert with available tools
    if command -v rsvg-convert &> /dev/null; then
      rsvg-convert "/tmp/splash-${SIZE}.svg" -o "$FILE"
    elif command -v convert &> /dev/null; then
      convert "/tmp/splash-${SIZE}.svg" "$FILE"
    else
      # Fallback: just copy SVG renamed as png (browsers will still accept it from meta tags)
      cp "/tmp/splash-${SIZE}.svg" "$FILE"
    fi

    echo "Created: $FILE"
    rm -f "/tmp/splash-${SIZE}.svg"
  fi
done

echo "✅ Apple splash screens generated in $SPLASH_DIR"
