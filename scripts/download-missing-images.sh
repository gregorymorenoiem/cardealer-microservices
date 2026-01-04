#!/bin/bash
# Download missing images for VehiclesOnlyHomePage.tsx

DOWNLOAD_DIR="/tmp/missing-images"
mkdir -p "$DOWNLOAD_DIR/vehicles/sale"

# Missing photoIds for vehicles/sale
MISSING_PHOTOS=(
    "photo-1519641471654-76ce0107ad1b"
    "photo-1533473359331-0135ef1b58bf"
    "photo-1549317661-bd32c8ce0db2"
    "photo-1551830820-330a71b99659"
    "photo-1552519507-da3b142c6e3d"
    "photo-1558618666-fcd25c85cd64"
    "photo-1560958089-b8a1929cea89"
    "photo-1563720223185-11003d516935"
    "photo-1590362891991-f776e747a588"
    "photo-1596468138838-8c50b8d9b2c5"
    "photo-1612544448445-b8232cff3b6c"
    "photo-1619767886558-efdc259cde1a"
    "photo-1621007947382-bb3c3994e3fb"
)

echo "Downloading ${#MISSING_PHOTOS[@]} missing images..."

for photo in "${MISSING_PHOTOS[@]}"; do
    url="https://images.unsplash.com/${photo}?w=800&h=600&fit=crop"
    output="$DOWNLOAD_DIR/vehicles/sale/${photo}.jpg"
    echo "Downloading: $photo"
    curl -sL "$url" -o "$output"
done

echo ""
echo "Downloaded files:"
ls -la "$DOWNLOAD_DIR/vehicles/sale/"

echo ""
echo "Uploading to S3..."
aws s3 sync "$DOWNLOAD_DIR/vehicles/sale/" s3://okla-images-2026/frontend/assets/vehicles/sale/ --content-type "image/jpeg"

echo ""
echo "Verifying upload..."
aws s3 ls s3://okla-images-2026/frontend/assets/vehicles/sale/ | wc -l
