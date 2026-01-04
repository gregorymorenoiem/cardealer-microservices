#!/usr/bin/env python3
"""
🚗 Vehicle Catalog Seeder for CarDealer Microservices
=====================================================
Este script descarga datos de vehículos de NHTSA API y los prepara
para insertar en la base de datos de ProductService.

Fuentes de datos:
1. NHTSA vPIC API (oficial del gobierno de EE.UU.)
2. Kaggle datasets (opcional, requiere descarga manual)

Uso:
    python seed-vehicle-catalog.py --download    # Descargar datos
    python seed-vehicle-catalog.py --seed        # Insertar en DB
    python seed-vehicle-catalog.py --all         # Todo
"""

import json
import os
import sys
import time
import argparse
import requests
from pathlib import Path
from datetime import datetime
from typing import Optional

# Configuración
SCRIPT_DIR = Path(__file__).parent
DATA_DIR = SCRIPT_DIR / "vehicle-data"
OUTPUT_DIR = DATA_DIR / "processed"

NHTSA_BASE_URL = "https://vpic.nhtsa.dot.gov/api/vehicles"

# Crear directorios
DATA_DIR.mkdir(exist_ok=True)
OUTPUT_DIR.mkdir(exist_ok=True)


class NHTSAClient:
    """Cliente para NHTSA vPIC API"""
    
    def __init__(self):
        self.session = requests.Session()
        self.session.headers.update({
            "Accept": "application/json",
            "User-Agent": "CarDealer-Seeder/1.0"
        })
    
    def get_all_makes(self) -> list:
        """Obtiene todas las marcas de vehículos"""
        print("📥 Descargando lista de marcas...")
        url = f"{NHTSA_BASE_URL}/GetAllMakes?format=json"
        response = self.session.get(url, timeout=30)
        response.raise_for_status()
        data = response.json()
        makes = data.get("Results", [])
        print(f"   ✅ {len(makes)} marcas encontradas")
        return makes
    
    def get_makes_for_vehicle_type(self, vehicle_type: str = "car") -> list:
        """Obtiene marcas por tipo de vehículo"""
        print(f"📥 Descargando marcas para tipo: {vehicle_type}...")
        url = f"{NHTSA_BASE_URL}/GetMakesForVehicleType/{vehicle_type}?format=json"
        response = self.session.get(url, timeout=30)
        response.raise_for_status()
        data = response.json()
        makes = data.get("Results", [])
        print(f"   ✅ {len(makes)} marcas encontradas")
        return makes
    
    def get_models_for_make(self, make_name: str) -> list:
        """Obtiene modelos para una marca"""
        url = f"{NHTSA_BASE_URL}/GetModelsForMake/{make_name}?format=json"
        try:
            response = self.session.get(url, timeout=15)
            response.raise_for_status()
            data = response.json()
            return data.get("Results", [])
        except Exception as e:
            print(f"   ⚠️ Error obteniendo modelos para {make_name}: {e}")
            return []
    
    def get_models_for_make_year(self, make_name: str, year: int) -> list:
        """Obtiene modelos para una marca y año específico"""
        url = f"{NHTSA_BASE_URL}/GetModelsForMakeYear/make/{make_name}/modelyear/{year}?format=json"
        try:
            response = self.session.get(url, timeout=15)
            response.raise_for_status()
            data = response.json()
            return data.get("Results", [])
        except Exception as e:
            print(f"   ⚠️ Error obteniendo modelos para {make_name} {year}: {e}")
            return []
    
    def decode_vin(self, vin: str) -> dict:
        """Decodifica un VIN para obtener especificaciones completas"""
        url = f"{NHTSA_BASE_URL}/DecodeVin/{vin}?format=json"
        try:
            response = self.session.get(url, timeout=15)
            response.raise_for_status()
            data = response.json()
            # Convertir lista de resultados a diccionario
            results = {}
            for item in data.get("Results", []):
                variable = item.get("Variable", "")
                value = item.get("Value", "")
                if value and value.strip():
                    results[variable] = value
            return results
        except Exception as e:
            print(f"   ⚠️ Error decodificando VIN {vin}: {e}")
            return {}


# Marcas populares para priorizar (más comunes en EE.UU./LatAm)
PRIORITY_MAKES = [
    "Toyota", "Honda", "Ford", "Chevrolet", "Nissan", "Jeep", "RAM",
    "GMC", "Hyundai", "Kia", "Subaru", "Volkswagen", "BMW", "Mercedes-Benz",
    "Audi", "Lexus", "Mazda", "Dodge", "Cadillac", "Acura", "Infiniti",
    "Buick", "Chrysler", "Mitsubishi", "Volvo", "Lincoln", "Tesla",
    "Porsche", "Land Rover", "Jaguar", "Alfa Romeo", "Fiat", "Mini",
    "Genesis", "Maserati", "Ferrari", "Lamborghini", "Bentley", "Rolls-Royce"
]

# Años a descargar (últimos 30 años + 5 futuros)
YEARS_RANGE = range(1995, 2031)


def download_vehicle_catalog(priority_only: bool = True, years: Optional[list] = None):
    """
    Descarga el catálogo completo de vehículos de NHTSA.
    
    Args:
        priority_only: Solo descargar marcas prioritarias (más rápido)
        years: Lista de años específicos (default: últimos 10)
    """
    client = NHTSAClient()
    
    if years is None:
        current_year = datetime.now().year
        years = list(range(current_year - 10, current_year + 2))
    
    catalog = {
        "generated_at": datetime.now().isoformat(),
        "source": "NHTSA vPIC API",
        "makes": [],
        "models_by_make": {},
        "models_by_year": {}
    }
    
    # 1. Obtener marcas
    if priority_only:
        print(f"\n🎯 Usando {len(PRIORITY_MAKES)} marcas prioritarias")
        makes_to_process = [{"Make_Name": m, "Make_ID": None} for m in PRIORITY_MAKES]
    else:
        all_makes = client.get_all_makes()
        makes_to_process = all_makes
    
    catalog["makes"] = [m.get("Make_Name", m) if isinstance(m, dict) else m for m in makes_to_process]
    
    # 2. Obtener modelos por marca
    print(f"\n📥 Descargando modelos para {len(makes_to_process)} marcas...")
    
    for i, make_info in enumerate(makes_to_process):
        make_name = make_info.get("Make_Name") if isinstance(make_info, dict) else make_info
        print(f"   [{i+1}/{len(makes_to_process)}] {make_name}...", end=" ")
        
        models = client.get_models_for_make(make_name)
        
        if models:
            catalog["models_by_make"][make_name] = [
                {
                    "model_id": m.get("Model_ID"),
                    "model_name": m.get("Model_Name"),
                    "make_id": m.get("Make_ID"),
                    "make_name": m.get("Make_Name")
                }
                for m in models
            ]
            print(f"✅ {len(models)} modelos")
        else:
            catalog["models_by_make"][make_name] = []
            print("⚠️ sin modelos")
        
        # Rate limiting
        time.sleep(0.2)
    
    # 3. Obtener modelos por año (para marcas prioritarias)
    print(f"\n📅 Descargando modelos por año ({years[0]}-{years[-1]})...")
    
    for year in years:
        print(f"\n   Año {year}:")
        catalog["models_by_year"][str(year)] = {}
        
        for make_name in PRIORITY_MAKES[:20]:  # Top 20 marcas por año
            models = client.get_models_for_make_year(make_name, year)
            if models:
                catalog["models_by_year"][str(year)][make_name] = [
                    m.get("Model_Name") for m in models
                ]
            time.sleep(0.1)
    
    # Guardar catálogo
    output_file = OUTPUT_DIR / "vehicle_catalog_nhtsa.json"
    with open(output_file, "w", encoding="utf-8") as f:
        json.dump(catalog, f, indent=2, ensure_ascii=False)
    
    print(f"\n✅ Catálogo guardado en: {output_file}")
    
    # Estadísticas
    total_models = sum(len(m) for m in catalog["models_by_make"].values())
    print(f"\n📊 Estadísticas:")
    print(f"   - Marcas: {len(catalog['makes'])}")
    print(f"   - Modelos totales: {total_models}")
    print(f"   - Años con datos: {len(catalog['models_by_year'])}")
    
    return catalog


def generate_sql_seed(catalog_file: Path = None):
    """
    Genera scripts SQL para insertar el catálogo en la base de datos.
    """
    if catalog_file is None:
        catalog_file = OUTPUT_DIR / "vehicle_catalog_nhtsa.json"
    
    if not catalog_file.exists():
        print(f"❌ Archivo no encontrado: {catalog_file}")
        print("   Ejecuta primero: python seed-vehicle-catalog.py --download")
        return
    
    with open(catalog_file, "r", encoding="utf-8") as f:
        catalog = json.load(f)
    
    sql_lines = [
        "-- ==============================================",
        "-- Vehicle Catalog Seed Script",
        f"-- Generated: {datetime.now().isoformat()}",
        "-- Source: NHTSA vPIC API",
        "-- ==============================================",
        "",
        "-- Crear tablas si no existen",
        """
CREATE TABLE IF NOT EXISTS vehicle_makes (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    slug VARCHAR(100) NOT NULL UNIQUE,
    logo_url VARCHAR(500),
    country VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS vehicle_models (
    id SERIAL PRIMARY KEY,
    make_id INTEGER REFERENCES vehicle_makes(id),
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) NOT NULL,
    body_type VARCHAR(50),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(make_id, slug)
);

CREATE TABLE IF NOT EXISTS vehicle_years (
    id SERIAL PRIMARY KEY,
    model_id INTEGER REFERENCES vehicle_models(id),
    year INTEGER NOT NULL,
    is_active BOOLEAN DEFAULT true,
    UNIQUE(model_id, year)
);

CREATE TABLE IF NOT EXISTS vehicle_trims (
    id SERIAL PRIMARY KEY,
    year_id INTEGER REFERENCES vehicle_years(id),
    name VARCHAR(100) NOT NULL,
    engine VARCHAR(100),
    transmission VARCHAR(100),
    drivetrain VARCHAR(50),
    fuel_type VARCHAR(50),
    mpg_city INTEGER,
    mpg_highway INTEGER,
    horsepower INTEGER,
    torque INTEGER,
    msrp DECIMAL(10,2),
    is_active BOOLEAN DEFAULT true,
    UNIQUE(year_id, name)
);
""",
        "",
        "-- Insertar marcas",
    ]
    
    # Insertar marcas
    for make in catalog.get("makes", []):
        make_name = make.replace("'", "''")
        slug = make.lower().replace(" ", "-").replace("'", "")
        sql_lines.append(
            f"INSERT INTO vehicle_makes (name, slug) VALUES ('{make_name}', '{slug}') ON CONFLICT (slug) DO NOTHING;"
        )
    
    sql_lines.append("")
    sql_lines.append("-- Insertar modelos")
    
    # Insertar modelos
    for make_name, models in catalog.get("models_by_make", {}).items():
        make_slug = make_name.lower().replace(" ", "-").replace("'", "")
        for model in models:
            model_name = model.get("model_name", "").replace("'", "''")
            if model_name:
                model_slug = model_name.lower().replace(" ", "-").replace("'", "").replace("/", "-")
                sql_lines.append(
                    f"INSERT INTO vehicle_models (make_id, name, slug) "
                    f"SELECT id, '{model_name}', '{model_slug}' FROM vehicle_makes WHERE slug = '{make_slug}' "
                    f"ON CONFLICT (make_id, slug) DO NOTHING;"
                )
    
    sql_lines.append("")
    sql_lines.append("-- Insertar años disponibles")
    
    # Insertar años
    for year, makes_models in catalog.get("models_by_year", {}).items():
        for make_name, models in makes_models.items():
            make_slug = make_name.lower().replace(" ", "-").replace("'", "")
            for model_name in models:
                model_slug = model_name.lower().replace(" ", "-").replace("'", "").replace("/", "-")
                sql_lines.append(
                    f"INSERT INTO vehicle_years (model_id, year) "
                    f"SELECT vm.id, {year} FROM vehicle_models vm "
                    f"JOIN vehicle_makes mk ON vm.make_id = mk.id "
                    f"WHERE mk.slug = '{make_slug}' AND vm.slug = '{model_slug}' "
                    f"ON CONFLICT (model_id, year) DO NOTHING;"
                )
    
    # Guardar SQL
    sql_file = OUTPUT_DIR / "vehicle_catalog_seed.sql"
    with open(sql_file, "w", encoding="utf-8") as f:
        f.write("\n".join(sql_lines))
    
    print(f"✅ SQL seed guardado en: {sql_file}")
    print(f"   Total líneas SQL: {len(sql_lines)}")
    
    return sql_file


def generate_typescript_types():
    """
    Genera tipos TypeScript para el frontend basados en el catálogo.
    """
    catalog_file = OUTPUT_DIR / "vehicle_catalog_nhtsa.json"
    
    if not catalog_file.exists():
        print(f"❌ Primero ejecuta: python seed-vehicle-catalog.py --download")
        return
    
    with open(catalog_file, "r", encoding="utf-8") as f:
        catalog = json.load(f)
    
    ts_content = '''// Auto-generated vehicle types from NHTSA catalog
// Generated: ''' + datetime.now().isoformat() + '''

export interface VehicleMake {
  id: number;
  name: string;
  slug: string;
  logoUrl?: string;
  country?: string;
}

export interface VehicleModel {
  id: number;
  makeId: number;
  name: string;
  slug: string;
  bodyType?: string;
}

export interface VehicleYear {
  id: number;
  modelId: number;
  year: number;
}

export interface VehicleTrim {
  id: number;
  yearId: number;
  name: string;
  engine?: string;
  transmission?: string;
  drivetrain?: string;
  fuelType?: string;
  mpgCity?: number;
  mpgHighway?: number;
  horsepower?: number;
  torque?: number;
  msrp?: number;
}

// Available makes in the catalog
export const VEHICLE_MAKES = [
'''
    
    for make in sorted(catalog.get("makes", [])):
        ts_content += f'  "{make}",\n'
    
    ts_content += '] as const;\n\n'
    ts_content += 'export type VehicleMakeName = typeof VEHICLE_MAKES[number];\n'
    
    # Guardar archivo
    ts_file = OUTPUT_DIR / "vehicle-types.ts"
    with open(ts_file, "w", encoding="utf-8") as f:
        f.write(ts_content)
    
    print(f"✅ Tipos TypeScript guardados en: {ts_file}")
    
    return ts_file


def main():
    parser = argparse.ArgumentParser(description="Vehicle Catalog Seeder")
    parser.add_argument("--download", action="store_true", help="Descargar catálogo de NHTSA")
    parser.add_argument("--sql", action="store_true", help="Generar script SQL")
    parser.add_argument("--types", action="store_true", help="Generar tipos TypeScript")
    parser.add_argument("--all", action="store_true", help="Ejecutar todo")
    parser.add_argument("--full", action="store_true", help="Descargar TODAS las marcas (lento)")
    
    args = parser.parse_args()
    
    if args.all:
        args.download = True
        args.sql = True
        args.types = True
    
    if not any([args.download, args.sql, args.types]):
        parser.print_help()
        print("\n💡 Ejemplo rápido:")
        print("   python seed-vehicle-catalog.py --all")
        return
    
    print("🚗 Vehicle Catalog Seeder")
    print("=" * 50)
    
    if args.download:
        download_vehicle_catalog(priority_only=not args.full)
    
    if args.sql:
        generate_sql_seed()
    
    if args.types:
        generate_typescript_types()
    
    print("\n✅ Completado!")


if __name__ == "__main__":
    main()
