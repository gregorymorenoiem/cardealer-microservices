#!/bin/bash
#
# Download ALL images from Unsplash and upload to S3
# Uses simple filenames: {photoId}.jpg (no size suffix)
#
# S3 Structure:
#   frontend/assets/vehicles/sale/{photoId}.jpg
#   frontend/assets/vehicles/rent/{photoId}.jpg
#   frontend/assets/properties/sale/{photoId}.jpg
#   frontend/assets/lodging/{photoId}.jpg
#

set -e

# Configuration
S3_BUCKET="okla-images-2026"
S3_REGION="us-east-2"
LOCAL_DIR="/tmp/okla-images"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}======================================${NC}"
echo -e "${GREEN}  OKLA Images - Download & Upload    ${NC}"
echo -e "${GREEN}======================================${NC}"

# Create local directories
mkdir -p "$LOCAL_DIR/vehicles/sale"
mkdir -p "$LOCAL_DIR/vehicles/rent"
mkdir -p "$LOCAL_DIR/properties/sale"
mkdir -p "$LOCAL_DIR/lodging"

# Function to download image
download_image() {
    local photo_id=$1
    local category=$2  # vehicles, properties, lodging
    local type=$3      # sale, rent, or empty for lodging
    local target_dir
    
    if [ -n "$type" ]; then
        target_dir="$LOCAL_DIR/$category/$type"
    else
        target_dir="$LOCAL_DIR/$category"
    fi
    
    local filename="${photo_id}.jpg"
    local filepath="$target_dir/$filename"
    
    # Skip if already exists
    if [ -f "$filepath" ]; then
        echo -e "  ${YELLOW}⏭️  Skipping (exists): $filename${NC}"
        return 0
    fi
    
    # Download from Unsplash with high quality
    local url="https://images.unsplash.com/${photo_id}?w=1200&q=85&fit=crop"
    
    echo -e "  📥 Downloading: $photo_id"
    if curl -sSL -o "$filepath" "$url" 2>/dev/null; then
        # Verify it's a valid image
        if file "$filepath" | grep -q "JPEG\|PNG\|image"; then
            echo -e "  ${GREEN}✅ Downloaded: $filename${NC}"
        else
            echo -e "  ${RED}❌ Invalid image: $filename${NC}"
            rm -f "$filepath"
            return 1
        fi
    else
        echo -e "  ${RED}❌ Failed: $photo_id${NC}"
        return 1
    fi
}

echo ""
echo -e "${GREEN}📦 Downloading VEHICLES/SALE images...${NC}"
# From HomePage.tsx - vehiculosListings
download_image "photo-1618843479313-40f8afb4b4d8" "vehicles" "sale"
download_image "photo-1555215695-3004980ad54e" "vehicles" "sale"
download_image "photo-1503376780353-7e6692767b70" "vehicles" "sale"
download_image "photo-1606664515524-ed2f786a0bd6" "vehicles" "sale"
download_image "photo-1617788138017-80ad40651399" "vehicles" "sale"
download_image "photo-1606016159991-dfe4f2746ad5" "vehicles" "sale"
# Category image for "Vehículos"
download_image "photo-1492144534655-ae79c964c9d7" "vehicles" "sale"
# From VehiclesOnlyHomePage and other pages
download_image "photo-1494905998402-395d579af36f" "vehicles" "sale"
download_image "photo-1494976388531-d1058494cdd8" "vehicles" "sale"
download_image "photo-1580273916550-e323be2ae537" "vehicles" "sale"
download_image "photo-1612825173281-9a193378527e" "vehicles" "sale"
download_image "photo-1617814076367-b759c7d7e738" "vehicles" "sale"
download_image "photo-1603584173870-7f23fdae1b7a" "vehicles" "sale"
download_image "photo-1561580125-028ee3bd62eb" "vehicles" "sale"
download_image "photo-1583121274602-3e2820c69888" "vehicles" "sale"
# From mockVehicles and other data
download_image "photo-1547744152-14d985cb937f" "vehicles" "sale"
download_image "photo-1544636331-e26879cd4d9b" "vehicles" "sale"
download_image "photo-1621135802920-133df287f89c" "vehicles" "sale"
download_image "photo-1625231334168-5bf4c59cbb21" "vehicles" "sale"
download_image "photo-1605559424843-9e4c228bf1c2" "vehicles" "sale"
download_image "photo-1580587771525-78b9dba3b914" "vehicles" "sale"

echo ""
echo -e "${GREEN}📦 Downloading VEHICLES/RENT images...${NC}"
# From HomePage.tsx - rentaVehiculosListings
download_image "photo-1549317661-bd32c8ce0db2" "vehicles" "rent"
download_image "photo-1563720223185-11003d516935" "vehicles" "rent"
download_image "photo-1619767886558-efdc259cde1a" "vehicles" "rent"
download_image "photo-1533473359331-0135ef1b58bf" "vehicles" "rent"
download_image "photo-1560958089-b8a1929cea89" "vehicles" "rent"
download_image "photo-1551830820-330a71b99659" "vehicles" "rent"
# Category image for "Renta de Vehículos"
download_image "photo-1449965408869-eaa3f722e40d" "vehicles" "rent"
# Additional vehicle rent images
download_image "photo-1552519507-da3b142c6e3d" "vehicles" "rent"
download_image "photo-1519641471654-76ce0107ad1b" "vehicles" "rent"

echo ""
echo -e "${GREEN}📦 Downloading PROPERTIES/SALE images...${NC}"
# From HomePage.tsx - propiedadesListings
download_image "photo-1600607687939-ce8a6c25118c" "properties" "sale"
download_image "photo-1600596542815-ffad4c1539a9" "properties" "sale"
download_image "photo-1502672260266-1c1ef2d93688" "properties" "sale"
download_image "photo-1600585154340-be6161a56a0c" "properties" "sale"
download_image "photo-1600607688969-a5bfcd646154" "properties" "sale"
download_image "photo-1613490493576-7fde63acd811" "properties" "sale"
# Additional property images from other sources
download_image "photo-1600566752355-35792bedcfea" "properties" "sale"
download_image "photo-1600566753086-00f18fb6b3ea" "properties" "sale"
download_image "photo-1600566753190-17f0baa2a6c3" "properties" "sale"
download_image "photo-1600607687644-c7171b42498f" "properties" "sale"
download_image "photo-1500382017468-9049fed747ef" "properties" "sale"
download_image "photo-1441986300917-64674bd600d8" "properties" "sale"
download_image "photo-1568605114967-8130f3a36994" "properties" "sale"
download_image "photo-1512917774080-9991f1c4c750" "properties" "sale"
download_image "photo-1600047509807-ba8f99d2cdde" "properties" "sale"
download_image "photo-1600585154526-990dced4db0d" "properties" "sale"
download_image "photo-1600607687644-aac4c3eac7f4" "properties" "sale"

echo ""
echo -e "${GREEN}📦 Downloading LODGING images...${NC}"
# From HomePage.tsx - hospedajeListings
download_image "photo-1582719478250-c89cae4dc85b" "lodging" ""
download_image "photo-1560448204-e02f11c3d0e2" "lodging" ""
download_image "photo-1602002418082-a4443e081dd1" "lodging" ""
download_image "photo-1587061949409-02df41d5e562" "lodging" ""
download_image "photo-1578683010236-d716f9a3f461" "lodging" ""
download_image "photo-1499793983690-e29da59ef1c2" "lodging" ""
# Category image for "Hospedaje"
download_image "photo-1566073771259-6a8506099945" "lodging" ""
# Additional lodging images
download_image "photo-1554995207-c18c203602cb" "lodging" ""
download_image "photo-1560185893-a55cbc8c57e8" "lodging" ""
download_image "photo-1522708323590-d24dbb6b0267" "lodging" ""
download_image "photo-1617531653332-bd46c24f2068" "lodging" ""
download_image "photo-1616455579100-2ceaa4eb2d37" "lodging" ""
download_image "photo-1609521263047-f8f205293f24" "lodging" ""
download_image "photo-1611566026373-c6c8da0ea861" "lodging" ""
download_image "photo-1611651337392-8504609d8c86" "lodging" ""
download_image "photo-1584345604476-8ec5e12e42dd" "lodging" ""
download_image "photo-1590362891991-f776e747a588" "lodging" ""
download_image "photo-1568844293986-8c2bfd4b01e5" "lodging" ""
download_image "photo-1571607388263-1044f9ea01dd" "lodging" ""
download_image "photo-1545324418-cc1a3fa10c00" "lodging" ""
download_image "photo-1549399542-7e3f8b79c341" "lodging" ""
download_image "photo-1549399542-7e8ee8c6e7a0" "lodging" ""
download_image "photo-1596468138838-8c50b8d9b2c5" "lodging" ""
download_image "photo-1558618666-fcd25c85cd64" "lodging" ""
download_image "photo-1621007947382-bb3c3994e3fb" "lodging" ""
download_image "photo-1583267746897-c94e54e8e922" "lodging" ""
download_image "photo-1583267746897-e8b02e1d291f" "lodging" ""
download_image "photo-1612544448445-b8232cff3b6c" "lodging" ""

# Additional miscellaneous images that appear in the frontend
echo ""
echo -e "${GREEN}📦 Downloading additional images...${NC}"
download_image "photo-1560179707-f14e90ef3623" "properties" "sale"
download_image "photo-1486406146926-c627a92ad1ab" "properties" "sale"
download_image "photo-1536700503339-1e4b06520771" "vehicles" "sale"

echo ""
echo -e "${GREEN}======================================${NC}"
echo -e "${GREEN}📊 Download Summary:${NC}"
echo -e "${GREEN}======================================${NC}"
echo "Vehicles/Sale: $(ls -1 $LOCAL_DIR/vehicles/sale/*.jpg 2>/dev/null | wc -l) images"
echo "Vehicles/Rent: $(ls -1 $LOCAL_DIR/vehicles/rent/*.jpg 2>/dev/null | wc -l) images"
echo "Properties/Sale: $(ls -1 $LOCAL_DIR/properties/sale/*.jpg 2>/dev/null | wc -l) images"
echo "Lodging: $(ls -1 $LOCAL_DIR/lodging/*.jpg 2>/dev/null | wc -l) images"
echo ""

# Upload to S3
echo -e "${GREEN}======================================${NC}"
echo -e "${GREEN}☁️  Uploading to S3...${NC}"
echo -e "${GREEN}======================================${NC}"

# First, remove all old files from S3 (clean slate)
echo -e "${YELLOW}🗑️  Cleaning old S3 files...${NC}"
aws s3 rm "s3://${S3_BUCKET}/frontend/assets/" --recursive --quiet 2>/dev/null || true

# Upload each category
echo ""
echo "📤 Uploading vehicles/sale..."
aws s3 sync "$LOCAL_DIR/vehicles/sale/" "s3://${S3_BUCKET}/frontend/assets/vehicles/sale/" \
    --content-type "image/jpeg" \
    --cache-control "max-age=31536000" \
    --acl public-read \
    --quiet

echo "📤 Uploading vehicles/rent..."
aws s3 sync "$LOCAL_DIR/vehicles/rent/" "s3://${S3_BUCKET}/frontend/assets/vehicles/rent/" \
    --content-type "image/jpeg" \
    --cache-control "max-age=31536000" \
    --acl public-read \
    --quiet

echo "📤 Uploading properties/sale..."
aws s3 sync "$LOCAL_DIR/properties/sale/" "s3://${S3_BUCKET}/frontend/assets/properties/sale/" \
    --content-type "image/jpeg" \
    --cache-control "max-age=31536000" \
    --acl public-read \
    --quiet

echo "📤 Uploading lodging..."
aws s3 sync "$LOCAL_DIR/lodging/" "s3://${S3_BUCKET}/frontend/assets/lodging/" \
    --content-type "image/jpeg" \
    --cache-control "max-age=31536000" \
    --acl public-read \
    --quiet

echo ""
echo -e "${GREEN}======================================${NC}"
echo -e "${GREEN}✅ Upload Complete!${NC}"
echo -e "${GREEN}======================================${NC}"

# Verify uploads
echo ""
echo "🔍 Verifying S3 uploads..."
TOTAL_UPLOADED=$(aws s3 ls "s3://${S3_BUCKET}/frontend/assets/" --recursive | grep "\.jpg" | wc -l)
echo -e "${GREEN}Total images in S3: $TOTAL_UPLOADED${NC}"

echo ""
echo "📂 S3 Structure:"
aws s3 ls "s3://${S3_BUCKET}/frontend/assets/" --recursive | grep "\.jpg" | head -20
echo "..."

echo ""
echo -e "${GREEN}🎉 Done! All images uploaded successfully.${NC}"
echo ""
echo "Test URLs:"
echo "  https://${S3_BUCKET}.s3.${S3_REGION}.amazonaws.com/frontend/assets/vehicles/sale/photo-1618843479313-40f8afb4b4d8.jpg"
echo "  https://${S3_BUCKET}.s3.${S3_REGION}.amazonaws.com/frontend/assets/lodging/photo-1582719478250-c89cae4dc85b.jpg"
