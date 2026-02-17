#!/usr/bin/env python3
"""
===============================================================================
OKLA Motors - Script de Descarga y Upload de Imágenes a AWS S3
===============================================================================

Este script:
1. Descarga imágenes de Picsum (Lorem Picsum) para cada vehículo
2. Las guarda localmente en data/vehicle_images/
3. Las sube a AWS S3 bucket: okla-images-2026
4. Actualiza la base de datos con las URLs de S3

Autor: Gregory Moreno
Fecha: Enero 2026

Uso:
    python3 scripts/download_and_upload_images.py

Requisitos:
    pip install boto3 requests psycopg2-binary
===============================================================================
"""

import os
import sys
import time
import uuid
import requests
import boto3
from botocore.exceptions import ClientError
import psycopg2
from concurrent.futures import ThreadPoolExecutor, as_completed
from pathlib import Path

# ===============================================================================
# CONFIGURACIÓN
# ===============================================================================

# AWS S3 - Use environment variables for sensitive data
S3_ACCESS_KEY = os.environ.get("AWS_ACCESS_KEY_ID", "")
S3_SECRET_KEY = os.environ.get("AWS_SECRET_ACCESS_KEY", "")
S3_BUCKET_NAME = os.environ.get("S3_BUCKET_NAME", "okla-images-2026")
S3_REGION = os.environ.get("S3_REGION", "us-east-2")

# PostgreSQL
DB_HOST = os.environ.get("DB_HOST", "localhost")
DB_PORT = int(os.environ.get("DB_PORT", "5433"))
DB_NAME = os.environ.get("DB_NAME", "vehiclessaleservice")
DB_USER = os.environ.get("DB_USER", "postgres")
DB_PASSWORD = os.environ.get("DB_PASSWORD", "password")

# Local storage
LOCAL_IMAGES_PATH = Path(__file__).parent.parent / "data" / "vehicle_images"
IMAGES_PER_VEHICLE = 5

# Image dimensions
IMAGE_DIMENSIONS = [
    (1280, 720),  # Imagen 1: Principal exterior
    (1280, 720),  # Imagen 2: Vista lateral
    (1280, 720),  # Imagen 3: Interior
    (800, 600),   # Imagen 4: Detalles
    (800, 600),   # Imagen 5: Motor
]

IMAGE_CAPTIONS = [
    "Vista exterior principal",
    "Vista lateral",
    "Interior del vehículo",
    "Detalle del vehículo",
    "Compartimento del motor"
]

IMAGE_TYPES = [0, 0, 1, 2, 3]  # 0=Exterior, 1=Interior, 2=Detail, 3=Engine

# ===============================================================================
# FUNCIONES
# ===============================================================================

def get_db_connection():
    """Establece conexión con PostgreSQL"""
    return psycopg2.connect(
        host=DB_HOST,
        port=DB_PORT,
        database=DB_NAME,
        user=DB_USER,
        password=DB_PASSWORD
    )

def get_s3_client():
    """Crea cliente de AWS S3"""
    return boto3.client(
        's3',
        aws_access_key_id=S3_ACCESS_KEY,
        aws_secret_access_key=S3_SECRET_KEY,
        region_name=S3_REGION
    )

def get_all_vehicles():
    """Obtiene todos los vehículos de la base de datos"""
    conn = get_db_connection()
    cur = conn.cursor()
    cur.execute('SELECT "Id", "DealerId", "Title" FROM vehicles ORDER BY "CreatedAt"')
    vehicles = cur.fetchall()
    cur.close()
    conn.close()
    return vehicles

def download_image(vehicle_id: str, image_index: int) -> tuple:
    """Descarga una imagen de Picsum y la guarda localmente"""
    width, height = IMAGE_DIMENSIONS[image_index]
    
    # Generar URL de Picsum con seed para consistencia
    seed = f"{vehicle_id}-{image_index + 1}"
    picsum_url = f"https://picsum.photos/seed/{seed}/{width}/{height}"
    
    # Crear directorio del vehículo
    vehicle_dir = LOCAL_IMAGES_PATH / vehicle_id
    vehicle_dir.mkdir(parents=True, exist_ok=True)
    
    # Nombre del archivo local
    filename = f"image_{image_index + 1}.jpg"
    local_path = vehicle_dir / filename
    
    try:
        response = requests.get(picsum_url, timeout=30, stream=True)
        response.raise_for_status()
        
        with open(local_path, 'wb') as f:
            for chunk in response.iter_content(chunk_size=8192):
                f.write(chunk)
        
        file_size = local_path.stat().st_size
        return (True, str(local_path), file_size, width, height)
    
    except Exception as e:
        return (False, str(e), 0, width, height)

def upload_to_s3(s3_client, local_path: str, vehicle_id: str, image_index: int) -> tuple:
    """Sube una imagen a S3 y retorna la URL pública"""
    s3_key = f"vehicles/{vehicle_id}/image_{image_index + 1}.jpg"
    
    try:
        # Sin ACL - el bucket tiene Block Public Access habilitado
        # Las imágenes serán servidas con CloudFront o presigned URLs
        s3_client.upload_file(
            local_path,
            S3_BUCKET_NAME,
            s3_key,
            ExtraArgs={
                'ContentType': 'image/jpeg'
            }
        )
        
        # Generar URL de S3 (será accesible si el bucket tiene policy pública o via CloudFront)
        s3_url = f"https://{S3_BUCKET_NAME}.s3.{S3_REGION}.amazonaws.com/{s3_key}"
        return (True, s3_url)
    
    except ClientError as e:
        return (False, str(e))

def update_database(vehicle_id: str, dealer_id: str, images_data: list):
    """Actualiza la base de datos con las URLs de S3"""
    conn = get_db_connection()
    cur = conn.cursor()
    
    # Primero eliminar imágenes existentes del vehículo
    cur.execute('DELETE FROM vehicle_images WHERE "VehicleId" = %s', (vehicle_id,))
    
    # Insertar nuevas imágenes
    for idx, img_data in enumerate(images_data):
        s3_url, file_size, width, height = img_data
        
        # Generar URL de thumbnail
        thumbnail_url = s3_url.replace('/image_', '/thumb_')
        
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
            s3_url,
            thumbnail_url,
            IMAGE_CAPTIONS[idx],
            IMAGE_TYPES[idx],
            idx,
            idx == 0,  # Primera imagen es primary
            file_size,
            'image/jpeg',
            width,
            height
        ))
    
    conn.commit()
    cur.close()
    conn.close()

def process_vehicle(s3_client, vehicle_id: str, dealer_id: str, vehicle_title: str, vehicle_num: int, total: int):
    """Procesa un vehículo: descarga imágenes, sube a S3, actualiza DB"""
    print(f"   [{vehicle_num}/{total}] {vehicle_title[:40]}...", end=" ", flush=True)
    
    images_data = []
    
    for i in range(IMAGES_PER_VEHICLE):
        # 1. Descargar imagen
        success, result, file_size, width, height = download_image(vehicle_id, i)
        if not success:
            print(f"❌ Error descarga: {result}")
            return False
        
        local_path = result
        
        # 2. Subir a S3
        success, s3_url = upload_to_s3(s3_client, local_path, vehicle_id, i)
        if not success:
            print(f"❌ Error S3: {s3_url}")
            return False
        
        images_data.append((s3_url, file_size, width, height))
    
    # 3. Actualizar base de datos
    update_database(vehicle_id, dealer_id, images_data)
    
    print(f"✅ {IMAGES_PER_VEHICLE} imágenes")
    return True

def main():
    print("=" * 70)
    print("🖼️  OKLA Motors - Descarga y Upload de Imágenes a AWS S3")
    print("=" * 70)
    print()
    
    # Crear directorio local si no existe
    LOCAL_IMAGES_PATH.mkdir(parents=True, exist_ok=True)
    print(f"📁 Directorio local: {LOCAL_IMAGES_PATH}")
    print(f"☁️  Bucket S3: s3://{S3_BUCKET_NAME}/vehicles/")
    print()
    
    # Verificar conexión a S3
    print("🔌 Verificando conexión a AWS S3...", end=" ")
    try:
        s3_client = get_s3_client()
        s3_client.head_bucket(Bucket=S3_BUCKET_NAME)
        print("✅")
    except ClientError as e:
        error_code = e.response['Error']['Code']
        if error_code == '404':
            print(f"❌ Bucket no existe: {S3_BUCKET_NAME}")
        elif error_code == '403':
            print(f"❌ Sin acceso al bucket: {S3_BUCKET_NAME}")
        else:
            print(f"❌ Error: {e}")
        sys.exit(1)
    
    # Verificar conexión a PostgreSQL
    print("🔌 Verificando conexión a PostgreSQL...", end=" ")
    try:
        conn = get_db_connection()
        conn.close()
        print("✅")
    except Exception as e:
        print(f"❌ Error: {e}")
        sys.exit(1)
    
    # Obtener vehículos
    print()
    print("📋 Obteniendo lista de vehículos...")
    vehicles = get_all_vehicles()
    print(f"   Encontrados: {len(vehicles)} vehículos")
    
    total_images = len(vehicles) * IMAGES_PER_VEHICLE
    print()
    print(f"📊 Plan de ejecución:")
    print(f"   • Vehículos: {len(vehicles)}")
    print(f"   • Imágenes por vehículo: {IMAGES_PER_VEHICLE}")
    print(f"   • Total imágenes: {total_images}")
    print(f"   • Almacenamiento estimado: ~{total_images * 0.5:.0f} MB")
    print()
    
    # Modo automático - sin confirmación
    print("🚀 Iniciando proceso automáticamente...")
    print()
    
    start_time = time.time()
    success_count = 0
    error_count = 0
    
    for idx, (vehicle_id, dealer_id, title) in enumerate(vehicles, 1):
        try:
            if process_vehicle(s3_client, vehicle_id, dealer_id, title, idx, len(vehicles)):
                success_count += 1
            else:
                error_count += 1
        except Exception as e:
            print(f"❌ Error: {e}")
            error_count += 1
        
        # Rate limiting para no sobrecargar Picsum
        if idx % 10 == 0:
            time.sleep(1)
    
    elapsed = time.time() - start_time
    
    print()
    print("=" * 70)
    print("📊 RESUMEN")
    print("=" * 70)
    print(f"   • Vehículos procesados: {success_count}")
    print(f"   • Errores: {error_count}")
    print(f"   • Imágenes subidas: {success_count * IMAGES_PER_VEHICLE}")
    print(f"   • Tiempo total: {elapsed:.1f} segundos")
    print(f"   • Velocidad: {(success_count * IMAGES_PER_VEHICLE) / elapsed:.1f} imágenes/seg")
    print()
    print(f"📁 Imágenes locales: {LOCAL_IMAGES_PATH}")
    print(f"☁️  Imágenes S3: https://{S3_BUCKET_NAME}.s3.{S3_REGION}.amazonaws.com/vehicles/")
    print()
    print("✅ Proceso completado!")

if __name__ == "__main__":
    main()
