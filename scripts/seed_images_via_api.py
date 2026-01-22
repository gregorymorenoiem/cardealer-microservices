#!/usr/bin/env python3
"""
OKLA Motors - Seed Vehicle Images via API
==========================================
Este script:
1. Registra/Login un usuario dealer via AuthService
2. Crea un Dealer via DealerManagementService  
3. Para cada vehículo:
   - Descarga imagen de Picsum
   - Inicializa upload en MediaService (obtiene presigned URL)
   - Sube directamente a S3 usando la presigned URL
   - Finaliza el upload en MediaService
   - Actualiza la base de datos de vehicles con la URL
"""

import os
import sys
import uuid
import requests
import psycopg2
from pathlib import Path
from datetime import datetime
import time

# ==============================================================================
# CONFIGURACIÓN
# ==============================================================================

# API URLs (Docker local)
AUTH_SERVICE_URL = "http://localhost:15085"
DEALER_SERVICE_URL = "http://localhost:15039"
MEDIA_SERVICE_URL = "http://localhost:15090"

# Credenciales del dealer de seeding
DEALER_EMAIL = "seeding_dealer@okla.com.do"
DEALER_PASSWORD = "SeedingDealer123!"
DEALER_USERNAME = "seeding_dealer"

# Datos del dealer
DEALER_DATA = {
    "businessName": "OKLA Seeding Dealer",
    "rnc": "123456789",
    "legalName": "OKLA Seeding Dealer SRL",
    "tradeName": "OKLA Seeding",
    "type": "Independent",
    "email": DEALER_EMAIL,
    "phone": "809-555-0001",
    "mobilePhone": "829-555-0001",
    "website": "https://okla.com.do",
    "address": "Calle Principal #1",
    "city": "Santo Domingo",
    "province": "Distrito Nacional",
    "zipCode": "10101",
    "description": "Dealer para seeding de datos de prueba",
    "establishedDate": "2020-01-01T00:00:00Z",
    "employeeCount": 10
}

# Base de datos
DB_HOST = "localhost"
DB_PORT = "5433"
DB_NAME = "vehiclessaleservice"
DB_USER = "postgres"
DB_PASSWORD = "password"

# Directorio local para backup de imágenes
LOCAL_IMAGES_DIR = Path(__file__).parent.parent / "data" / "vehicle_images"

# Configuración de imágenes
IMAGES_PER_VEHICLE = 5
IMAGE_CAPTIONS = [
    "Vista frontal del vehículo",
    "Vista lateral derecha",
    "Vista trasera",
    "Interior - Tablero",
    "Interior - Asientos"
]
IMAGE_TYPES = ["Exterior", "Exterior", "Exterior", "Interior", "Interior"]

# ==============================================================================
# FUNCIONES DE AUTENTICACIÓN
# ==============================================================================

def register_user():
    """Registra un nuevo usuario"""
    print(f"📝 Registrando usuario {DEALER_EMAIL}...")
    
    response = requests.post(
        f"{AUTH_SERVICE_URL}/api/auth/register",
        json={
            "userName": DEALER_USERNAME,
            "email": DEALER_EMAIL,
            "password": DEALER_PASSWORD
        },
        timeout=30
    )
    
    if response.status_code == 200:
        data = response.json()
        if data.get("success"):
            print(f"   ✅ Usuario registrado: {data['data']['userId']}")
            return data["data"]
        else:
            print(f"   ⚠️ Error: {data.get('error')}")
            return None
    else:
        print(f"   ❌ Error HTTP {response.status_code}: {response.text}")
        return None

def login_user():
    """Login del usuario y obtiene JWT"""
    print(f"🔐 Haciendo login como {DEALER_EMAIL}...")
    
    response = requests.post(
        f"{AUTH_SERVICE_URL}/api/auth/login",
        json={
            "email": DEALER_EMAIL,
            "password": DEALER_PASSWORD
        },
        timeout=30
    )
    
    if response.status_code == 200:
        data = response.json()
        if data.get("success"):
            token = data["data"]["accessToken"]
            user_id = data["data"]["userId"]
            print(f"   ✅ Login exitoso. UserId: {user_id}")
            return token, user_id
        else:
            error = data.get("error", "Unknown error")
            if "verify your email" in error.lower():
                print("   ⚠️ Email no verificado, verificando en DB...")
                verify_email_in_db()
                return login_user()  # Retry
            print(f"   ❌ Error: {error}")
            return None, None
    else:
        print(f"   ❌ Error HTTP {response.status_code}")
        return None, None

def verify_email_in_db():
    """Verifica el email directamente en la base de datos"""
    conn = psycopg2.connect(
        host=DB_HOST, port=DB_PORT, 
        dbname="authservice", user=DB_USER, password=DB_PASSWORD
    )
    cur = conn.cursor()
    cur.execute(
        'UPDATE "Users" SET "EmailConfirmed" = true WHERE "Email" = %s',
        (DEALER_EMAIL,)
    )
    conn.commit()
    cur.close()
    conn.close()
    print("   ✅ Email verificado en DB")

def get_or_create_dealer(token: str, user_id: str) -> str:
    """Obtiene o crea un dealer para el usuario"""
    headers = {"Authorization": f"Bearer {token}"}
    
    # Primero intentar obtener dealer existente
    print(f"🏢 Buscando dealer para usuario {user_id}...")
    response = requests.get(
        f"{DEALER_SERVICE_URL}/api/dealers/user/{user_id}",
        headers=headers,
        timeout=30
    )
    
    if response.status_code == 200:
        dealer = response.json()
        dealer_id = dealer.get("id")
        print(f"   ✅ Dealer encontrado: {dealer_id}")
        return dealer_id
    
    # Crear nuevo dealer
    print("   📝 Creando nuevo dealer...")
    dealer_request = {**DEALER_DATA, "userId": user_id}
    
    response = requests.post(
        f"{DEALER_SERVICE_URL}/api/dealers",
        json=dealer_request,
        headers=headers,
        timeout=30
    )
    
    if response.status_code in [200, 201]:
        dealer = response.json()
        dealer_id = dealer.get("id")
        print(f"   ✅ Dealer creado: {dealer_id}")
        
        # Actualizar usuario con dealerId en AuthService DB
        update_user_dealer_id(user_id, dealer_id)
        
        return dealer_id
    else:
        print(f"   ❌ Error creando dealer: {response.status_code} - {response.text}")
        return None

def update_user_dealer_id(user_id: str, dealer_id: str):
    """Actualiza el dealerId del usuario en AuthService"""
    conn = psycopg2.connect(
        host=DB_HOST, port=DB_PORT,
        dbname="authservice", user=DB_USER, password=DB_PASSWORD
    )
    cur = conn.cursor()
    cur.execute(
        'UPDATE "Users" SET "DealerId" = %s, "AccountType" = 2 WHERE "Id" = %s',
        (dealer_id, user_id)
    )
    conn.commit()
    cur.close()
    conn.close()
    print(f"   ✅ Usuario actualizado con DealerId")

# ==============================================================================
# FUNCIONES DE MEDIA
# ==============================================================================

def download_image(vehicle_id: str, index: int) -> tuple:
    """Descarga imagen de Picsum y la guarda localmente"""
    # Crear directorio del vehículo
    vehicle_dir = LOCAL_IMAGES_DIR / vehicle_id
    vehicle_dir.mkdir(parents=True, exist_ok=True)
    
    local_path = vehicle_dir / f"image_{index + 1}.jpg"
    
    # Si ya existe, usarla
    if local_path.exists():
        return True, str(local_path), local_path.stat().st_size
    
    # Descargar de Picsum con seed único
    seed = f"{vehicle_id[:8]}-{index}"
    picsum_url = f"https://picsum.photos/seed/{seed}/1280/720"
    
    try:
        response = requests.get(picsum_url, timeout=30, stream=True)
        if response.status_code == 200:
            with open(local_path, 'wb') as f:
                for chunk in response.iter_content(chunk_size=8192):
                    f.write(chunk)
            
            file_size = local_path.stat().st_size
            return True, str(local_path), file_size
        else:
            return False, f"HTTP {response.status_code}", 0
    except Exception as e:
        return False, str(e), 0

def init_upload(token: str, dealer_id: str, vehicle_id: str, filename: str, file_size: int) -> dict:
    """Inicializa upload en MediaService y obtiene presigned URL"""
    headers = {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}
    
    response = requests.post(
        f"{MEDIA_SERVICE_URL}/api/media/upload/init",
        json={
            "ownerId": vehicle_id,
            "context": "vehicle-images",
            "fileName": filename,
            "contentType": "image/jpeg",
            "fileSize": file_size
        },
        headers=headers,
        timeout=30
    )
    
    if response.status_code == 200:
        data = response.json()
        if data.get("success"):
            return data["data"]
    
    return None

def upload_to_s3(upload_url: str, local_path: str, headers: dict) -> bool:
    """Sube archivo directamente a S3 usando presigned URL"""
    with open(local_path, 'rb') as f:
        response = requests.put(
            upload_url,
            data=f,
            headers={"Content-Type": "image/jpeg", **headers},
            timeout=60
        )
    
    return response.status_code in [200, 204]

def finalize_upload(token: str, media_id: str) -> dict:
    """Finaliza el upload en MediaService"""
    headers = {"Authorization": f"Bearer {token}"}
    
    response = requests.post(
        f"{MEDIA_SERVICE_URL}/api/media/upload/finalize/{media_id}",
        headers=headers,
        timeout=30
    )
    
    if response.status_code == 200:
        data = response.json()
        if data.get("success"):
            return data["data"]
    
    return None

def get_media_url(token: str, media_id: str) -> str:
    """Obtiene la URL del media"""
    headers = {"Authorization": f"Bearer {token}"}
    
    response = requests.get(
        f"{MEDIA_SERVICE_URL}/api/media/{media_id}",
        headers=headers,
        timeout=30
    )
    
    if response.status_code == 200:
        data = response.json()
        if data.get("success"):
            return data["data"].get("url")
    
    return None

# ==============================================================================
# FUNCIONES DE BASE DE DATOS
# ==============================================================================

def get_db_connection():
    """Conecta a PostgreSQL"""
    return psycopg2.connect(
        host=DB_HOST, port=DB_PORT,
        dbname=DB_NAME, user=DB_USER, password=DB_PASSWORD
    )

def get_vehicles():
    """Obtiene lista de vehículos"""
    conn = get_db_connection()
    cur = conn.cursor()
    cur.execute('''
        SELECT "Id", "DealerId", "Year", 
               (SELECT "Name" FROM vehicle_makes WHERE "Id" = v."MakeId") as make,
               (SELECT "Name" FROM vehicle_models WHERE "Id" = v."ModelId") as model
        FROM vehicles v
        WHERE "Status" = 'Active'
        ORDER BY "CreatedAt"
    ''')
    vehicles = cur.fetchall()
    cur.close()
    conn.close()
    return vehicles

def update_vehicle_images(vehicle_id: str, dealer_id: str, images_data: list):
    """Actualiza la base de datos con las URLs de las imágenes"""
    conn = get_db_connection()
    cur = conn.cursor()
    
    # Eliminar imágenes existentes
    cur.execute('DELETE FROM vehicle_images WHERE "VehicleId" = %s', (vehicle_id,))
    
    # Insertar nuevas imágenes
    for idx, (url, file_size) in enumerate(images_data):
        thumbnail_url = url  # Por ahora usar la misma URL
        
        cur.execute('''
            INSERT INTO vehicle_images 
            ("Id", "DealerId", "VehicleId", "Url", "ThumbnailUrl", "Caption", 
             "ImageType", "SortOrder", "IsPrimary", "FileSize", "MimeType", 
             "Width", "Height", "CreatedAt")
            VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, NOW())
        ''', (
            str(uuid.uuid4()),
            dealer_id if dealer_id else '00000000-0000-0000-0000-000000000000',
            vehicle_id,
            url,
            thumbnail_url,
            IMAGE_CAPTIONS[idx],
            IMAGE_TYPES[idx],
            idx,
            idx == 0,
            file_size,
            'image/jpeg',
            1280,
            720
        ))
    
    conn.commit()
    cur.close()
    conn.close()

# ==============================================================================
# PROCESO PRINCIPAL
# ==============================================================================

def process_vehicle(token: str, dealer_id: str, vehicle_id: str, vehicle_dealer_id: str, 
                    title: str, idx: int, total: int) -> bool:
    """Procesa un vehículo: descarga imágenes y las sube via API"""
    print(f"   [{idx}/{total}] {title}...", end=" ", flush=True)
    
    images_data = []
    
    for i in range(IMAGES_PER_VEHICLE):
        # 1. Descargar imagen
        success, result, file_size = download_image(vehicle_id, i)
        if not success:
            print(f"❌ Error descargando imagen {i+1}")
            continue
        
        local_path = result
        filename = f"vehicle_{vehicle_id[:8]}_image_{i+1}.jpg"
        
        # 2. Inicializar upload en MediaService
        upload_info = init_upload(token, dealer_id, vehicle_id, filename, file_size)
        if not upload_info:
            print(f"❌ Error init upload imagen {i+1}")
            # Fallback: usar URL directa si MediaService falla
            images_data.append((f"file://{local_path}", file_size))
            continue
        
        media_id = upload_info.get("mediaId")
        upload_url = upload_info.get("uploadUrl")
        upload_headers = upload_info.get("uploadHeaders", {})
        
        # 3. Subir a S3
        if not upload_to_s3(upload_url, local_path, upload_headers):
            print(f"❌ Error S3 upload imagen {i+1}")
            images_data.append((f"file://{local_path}", file_size))
            continue
        
        # 4. Finalizar upload
        finalize_result = finalize_upload(token, media_id)
        if finalize_result:
            url = finalize_result.get("url") or get_media_url(token, media_id)
            if url:
                images_data.append((url, file_size))
            else:
                images_data.append((f"file://{local_path}", file_size))
        else:
            # Intentar obtener URL de todas formas
            url = get_media_url(token, media_id)
            if url:
                images_data.append((url, file_size))
            else:
                images_data.append((f"file://{local_path}", file_size))
    
    # 5. Actualizar base de datos
    if images_data:
        update_vehicle_images(
            vehicle_id, 
            vehicle_dealer_id or dealer_id,
            images_data
        )
        print(f"✅ {len(images_data)} imágenes")
        return True
    else:
        print("❌ Sin imágenes")
        return False

def main():
    print("=" * 70)
    print("🖼️  OKLA Motors - Seed Vehicle Images via API")
    print("=" * 70)
    
    # Crear directorio de imágenes
    LOCAL_IMAGES_DIR.mkdir(parents=True, exist_ok=True)
    print(f"\n📁 Directorio local: {LOCAL_IMAGES_DIR}")
    
    # 1. Registrar o Login usuario
    print("\n" + "=" * 40)
    print("1️⃣  AUTENTICACIÓN")
    print("=" * 40)
    
    token, user_id = login_user()
    
    if not token:
        print("   Usuario no existe, registrando...")
        register_result = register_user()
        if register_result:
            # Verificar email y hacer login
            verify_email_in_db()
            token, user_id = login_user()
    
    if not token:
        print("❌ No se pudo autenticar. Abortando.")
        sys.exit(1)
    
    # 2. Obtener o crear dealer
    print("\n" + "=" * 40)
    print("2️⃣  DEALER MANAGEMENT")
    print("=" * 40)
    
    dealer_id = get_or_create_dealer(token, user_id)
    
    if not dealer_id:
        print("⚠️ No se pudo crear dealer, usando ID genérico")
        dealer_id = "00000000-0000-0000-0000-000000000001"
    
    # Re-login para obtener token con dealerId actualizado
    print("\n🔄 Re-autenticando para obtener token actualizado...")
    token, _ = login_user()
    
    if not token:
        print("❌ Error en re-autenticación")
        sys.exit(1)
    
    # 3. Obtener vehículos
    print("\n" + "=" * 40)
    print("3️⃣  PROCESANDO VEHÍCULOS")
    print("=" * 40)
    
    vehicles = get_vehicles()
    total = len(vehicles)
    print(f"\n📋 Encontrados: {total} vehículos")
    print(f"📊 Total imágenes a procesar: {total * IMAGES_PER_VEHICLE}")
    
    # Confirmación
    confirm = input("\n¿Continuar? (s/n): ").strip().lower()
    if confirm != 's':
        print("Cancelado.")
        sys.exit(0)
    
    # Procesar vehículos
    print("\n🚀 Iniciando proceso...\n")
    
    success_count = 0
    error_count = 0
    start_time = time.time()
    
    for idx, (vid, v_dealer_id, year, make, model) in enumerate(vehicles, 1):
        title = f"{year} {make} {model}"
        
        try:
            if process_vehicle(token, dealer_id, vid, v_dealer_id, title, idx, total):
                success_count += 1
            else:
                error_count += 1
        except Exception as e:
            print(f"   [{idx}/{total}] {title}... ❌ Error: {e}")
            error_count += 1
        
        # Rate limiting para no sobrecargar APIs
        if idx % 10 == 0:
            time.sleep(0.5)
    
    # Resumen
    elapsed = time.time() - start_time
    print("\n" + "=" * 70)
    print("📊 RESUMEN")
    print("=" * 70)
    print(f"   ✅ Exitosos: {success_count}")
    print(f"   ❌ Errores: {error_count}")
    print(f"   ⏱️  Tiempo: {elapsed:.1f} segundos")
    print(f"   📁 Imágenes guardadas en: {LOCAL_IMAGES_DIR}")
    print("=" * 70)

if __name__ == "__main__":
    main()
