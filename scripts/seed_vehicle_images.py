#!/usr/bin/env python3
"""
Script para poblar imágenes de vehículos usando el API de VehiclesSaleService.
Genera 5 imágenes de Picsum (Lorem Picsum) por cada vehículo publicado.

Autor: Gregory Moreno
Fecha: Enero 2026
"""

import requests
import json
import sys
from concurrent.futures import ThreadPoolExecutor
import time

# Configuración
API_BASE_URL = "http://localhost:15070"
IMAGES_PER_VEHICLE = 5
BATCH_SIZE = 50  # Procesar en lotes de 50 vehículos

# Dealer ID default (para entidades que requieren DealerId)
DEFAULT_DEALER_ID = "00000000-0000-0000-0000-000000000000"

def get_all_vehicle_ids():
    """Obtiene todos los IDs de vehículos activos."""
    vehicle_ids = []
    page = 1
    page_size = 100
    
    print("📋 Obteniendo lista de vehículos...")
    
    while True:
        try:
            response = requests.get(
                f"{API_BASE_URL}/api/vehicles",
                params={"page": page, "pageSize": page_size},
                timeout=30
            )
            response.raise_for_status()
            data = response.json()
            
            vehicles = data.get("vehicles", [])
            if not vehicles:
                break
                
            for v in vehicles:
                vehicle_ids.append(v.get("id"))
            
            print(f"   Página {page}: {len(vehicles)} vehículos (Total: {len(vehicle_ids)})")
            
            if len(vehicles) < page_size:
                break
                
            page += 1
            
        except requests.RequestException as e:
            print(f"❌ Error obteniendo vehículos en página {page}: {e}")
            break
    
    print(f"✅ Total vehículos encontrados: {len(vehicle_ids)}")
    return vehicle_ids

def generate_image_url(vehicle_id: str, image_index: int, image_type: str = "exterior") -> dict:
    """Genera una URL de imagen de Picsum con seed determinístico."""
    # Diferentes dimensiones según el tipo de imagen
    dimensions = {
        0: (1280, 720),  # Exterior - Principal (16:9 grande)
        1: (1280, 720),  # Exterior 2
        2: (1280, 720),  # Interior
        3: (800, 600),   # Detalles
        4: (800, 600),   # Motor
    }
    
    width, height = dimensions.get(image_index, (800, 600))
    
    # Crear seed único basado en vehicle_id + index
    seed = f"{vehicle_id}-{image_index}"
    
    # URL de Picsum con seed para reproducibilidad
    url = f"https://picsum.photos/seed/{seed}/{width}/{height}"
    thumbnail_url = f"https://picsum.photos/seed/{seed}/200/150"
    
    # Tipos de imagen
    image_types = ["Exterior", "Exterior", "Interior", "Detail", "Engine"]
    captions = [
        "Vista exterior principal",
        "Vista exterior lateral", 
        "Interior del vehículo",
        "Detalle del vehículo",
        "Compartimento del motor"
    ]
    
    return {
        "url": url,
        "thumbnailUrl": thumbnail_url,
        "caption": captions[image_index] if image_index < len(captions) else f"Imagen {image_index + 1}",
        "imageType": image_index if image_index <= 4 else 0,  # 0=Exterior, 1=Interior, 2=Detail, 3=Engine, 4=Document
        "sortOrder": image_index,
        "isPrimary": image_index == 0,
        "fileSize": 500000 + (image_index * 100000),  # Tamaño simulado
        "width": width,
        "height": height,
        "mimeType": "image/jpeg"
    }

def seed_images_for_batch(vehicle_ids: list) -> dict:
    """Envía un lote de imágenes usando el endpoint bulk."""
    vehicle_images = []
    
    for vehicle_id in vehicle_ids:
        images = []
        for i in range(IMAGES_PER_VEHICLE):
            images.append(generate_image_url(vehicle_id, i))
        
        vehicle_images.append({
            "vehicleId": vehicle_id,
            "images": images
        })
    
    payload = {
        "vehicleImages": vehicle_images
    }
    
    try:
        response = requests.post(
            f"{API_BASE_URL}/api/vehicles/bulk-images",
            json=payload,
            headers={"Content-Type": "application/json"},
            timeout=60
        )
        
        if response.status_code == 200:
            result = response.json()
            return {
                "success": True,
                "processed": result.get("totalProcessed", len(vehicle_ids)),
                "images": result.get("totalImagesAdded", len(vehicle_ids) * IMAGES_PER_VEHICLE)
            }
        else:
            return {
                "success": False,
                "error": f"HTTP {response.status_code}: {response.text[:200]}"
            }
            
    except requests.RequestException as e:
        return {
            "success": False,
            "error": str(e)
        }

def main():
    print("=" * 60)
    print("🖼️  OKLA Motors - Seeding de Imágenes de Vehículos")
    print("=" * 60)
    print()
    
    # 1. Obtener todos los vehículos
    vehicle_ids = get_all_vehicle_ids()
    
    if not vehicle_ids:
        print("❌ No se encontraron vehículos. Abortando.")
        sys.exit(1)
    
    print()
    print(f"📊 Plan de seeding:")
    print(f"   • Vehículos: {len(vehicle_ids)}")
    print(f"   • Imágenes por vehículo: {IMAGES_PER_VEHICLE}")
    print(f"   • Total imágenes a crear: {len(vehicle_ids) * IMAGES_PER_VEHICLE}")
    print(f"   • Tamaño de lote: {BATCH_SIZE}")
    print()
    
    # 2. Procesar en lotes
    total_processed = 0
    total_images = 0
    start_time = time.time()
    
    batches = [vehicle_ids[i:i+BATCH_SIZE] for i in range(0, len(vehicle_ids), BATCH_SIZE)]
    
    print(f"🚀 Iniciando seeding ({len(batches)} lotes)...")
    print()
    
    for batch_num, batch in enumerate(batches, 1):
        print(f"   Lote {batch_num}/{len(batches)}: procesando {len(batch)} vehículos...", end=" ", flush=True)
        
        result = seed_images_for_batch(batch)
        
        if result["success"]:
            total_processed += result["processed"]
            total_images += result["images"]
            print(f"✅ {result['images']} imágenes")
        else:
            print(f"❌ Error: {result['error'][:50]}")
    
    elapsed = time.time() - start_time
    
    print()
    print("=" * 60)
    print("📊 RESUMEN DEL SEEDING")
    print("=" * 60)
    print(f"   • Vehículos procesados: {total_processed}")
    print(f"   • Imágenes creadas: {total_images}")
    print(f"   • Tiempo total: {elapsed:.2f} segundos")
    print(f"   • Velocidad: {total_images/elapsed:.0f} imágenes/segundo")
    print()
    print("✅ Seeding completado!")

if __name__ == "__main__":
    main()
