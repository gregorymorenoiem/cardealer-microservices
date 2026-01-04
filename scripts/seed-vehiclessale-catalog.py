#!/usr/bin/env python3
"""
🚗 Vehicle Catalog Seeder para VehiclesSaleService
===================================================
Este script descarga datos completos de vehículos incluyendo TRIMS con specs
de la NHTSA API y otras fuentes, y genera SQL para VehiclesSaleService.

El flujo del dealer es:
1. Selecciona Marca (Toyota)
2. Selecciona Modelo (Camry) 
3. Selecciona Año (2024)
4. Selecciona Trim (XLE) → Auto-fill specs
5. Dealer completa: Precio, Fotos, Mileage, VIN, Condición

Uso:
    python seed-vehiclessale-catalog.py --download    # Descargar datos
    python seed-vehiclessale-catalog.py --seed        # Generar SQL
    python seed-vehiclessale-catalog.py --all         # Todo
"""

import json
import argparse
import uuid
from pathlib import Path
from datetime import datetime
from typing import Optional, Dict, List

# Configuración
SCRIPT_DIR = Path(__file__).parent
DATA_DIR = SCRIPT_DIR / "vehicle-data"
OUTPUT_DIR = DATA_DIR / "vehiclessale"

NHTSA_BASE_URL = "https://vpic.nhtsa.dot.gov/api/vehicles"

# Crear directorios
DATA_DIR.mkdir(exist_ok=True)
OUTPUT_DIR.mkdir(exist_ok=True)


# ========================================
# DATOS DE TRIMS PRECARGADOS
# ========================================
# Estos son datos reales de trims populares con specs.
# La NHTSA API no proporciona trims, así que usamos datos curados.

TRIM_DATA: Dict[str, Dict[str, Dict[int, List[Dict]]]] = {
    "Toyota": {
        "Camry": {
            2024: [
                {"name": "LE", "engine": "2.5L I4", "hp": 203, "torque": 184, "trans": "Automatic", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 28, "mpg_hwy": 39, "msrp": 28400},
                {"name": "SE", "engine": "2.5L I4", "hp": 203, "torque": 184, "trans": "Automatic", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 28, "mpg_hwy": 39, "msrp": 29495},
                {"name": "XLE", "engine": "2.5L I4", "hp": 203, "torque": 184, "trans": "Automatic", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 28, "mpg_hwy": 39, "msrp": 31170},
                {"name": "XSE", "engine": "2.5L I4", "hp": 203, "torque": 184, "trans": "Automatic", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 28, "mpg_hwy": 39, "msrp": 32920},
                {"name": "TRD", "engine": "3.5L V6", "hp": 301, "torque": 267, "trans": "Automatic", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 22, "mpg_hwy": 33, "msrp": 36095},
                {"name": "Hybrid LE", "engine": "2.5L Hybrid", "hp": 225, "torque": 163, "trans": "CVT", "drive": "AWD", "fuel": "Hybrid", "mpg_city": 51, "mpg_hwy": 53, "msrp": 29845},
                {"name": "Hybrid SE", "engine": "2.5L Hybrid", "hp": 225, "torque": 163, "trans": "CVT", "drive": "AWD", "fuel": "Hybrid", "mpg_city": 46, "mpg_hwy": 50, "msrp": 31895},
                {"name": "Hybrid XLE", "engine": "2.5L Hybrid", "hp": 225, "torque": 163, "trans": "CVT", "drive": "AWD", "fuel": "Hybrid", "mpg_city": 51, "mpg_hwy": 53, "msrp": 33145},
                {"name": "Hybrid XSE", "engine": "2.5L Hybrid", "hp": 225, "torque": 163, "trans": "CVT", "drive": "AWD", "fuel": "Hybrid", "mpg_city": 46, "mpg_hwy": 50, "msrp": 34895},
            ],
            2023: [
                {"name": "LE", "engine": "2.5L I4", "hp": 203, "torque": 184, "trans": "Automatic", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 28, "mpg_hwy": 39, "msrp": 26420},
                {"name": "SE", "engine": "2.5L I4", "hp": 203, "torque": 184, "trans": "Automatic", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 28, "mpg_hwy": 39, "msrp": 27760},
                {"name": "XLE", "engine": "2.5L I4", "hp": 203, "torque": 184, "trans": "Automatic", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 28, "mpg_hwy": 39, "msrp": 29945},
                {"name": "XSE", "engine": "2.5L I4", "hp": 203, "torque": 184, "trans": "Automatic", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 28, "mpg_hwy": 39, "msrp": 31070},
                {"name": "TRD", "engine": "3.5L V6", "hp": 301, "torque": 267, "trans": "Automatic", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 22, "mpg_hwy": 32, "msrp": 33795},
            ],
        },
        "Corolla": {
            2024: [
                {"name": "LE", "engine": "1.8L I4", "hp": 139, "torque": 126, "trans": "CVT", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 30, "mpg_hwy": 38, "msrp": 22050},
                {"name": "SE", "engine": "2.0L I4", "hp": 169, "torque": 151, "trans": "CVT", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 31, "mpg_hwy": 40, "msrp": 24450},
                {"name": "XLE", "engine": "1.8L I4", "hp": 139, "torque": 126, "trans": "CVT", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 30, "mpg_hwy": 38, "msrp": 25550},
                {"name": "Hybrid LE", "engine": "1.8L Hybrid", "hp": 138, "torque": 105, "trans": "CVT", "drive": "FWD", "fuel": "Hybrid", "mpg_city": 53, "mpg_hwy": 46, "msrp": 24050},
                {"name": "Hybrid SE", "engine": "1.8L Hybrid", "hp": 138, "torque": 105, "trans": "CVT", "drive": "FWD", "fuel": "Hybrid", "mpg_city": 50, "mpg_hwy": 43, "msrp": 26700},
            ],
        },
        "RAV4": {
            2024: [
                {"name": "LE", "engine": "2.5L I4", "hp": 203, "torque": 184, "trans": "Automatic", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 27, "mpg_hwy": 35, "msrp": 30825},
                {"name": "XLE", "engine": "2.5L I4", "hp": 203, "torque": 184, "trans": "Automatic", "drive": "AWD", "fuel": "Gasoline", "mpg_city": 26, "mpg_hwy": 33, "msrp": 33325},
                {"name": "XLE Premium", "engine": "2.5L I4", "hp": 203, "torque": 184, "trans": "Automatic", "drive": "AWD", "fuel": "Gasoline", "mpg_city": 26, "mpg_hwy": 33, "msrp": 37325},
                {"name": "Adventure", "engine": "2.5L I4", "hp": 203, "torque": 184, "trans": "Automatic", "drive": "AWD", "fuel": "Gasoline", "mpg_city": 26, "mpg_hwy": 33, "msrp": 38795},
                {"name": "TRD Off-Road", "engine": "2.5L I4", "hp": 203, "torque": 184, "trans": "Automatic", "drive": "AWD", "fuel": "Gasoline", "mpg_city": 26, "mpg_hwy": 33, "msrp": 41490},
                {"name": "Hybrid SE", "engine": "2.5L Hybrid", "hp": 219, "torque": 163, "trans": "CVT", "drive": "AWD", "fuel": "Hybrid", "mpg_city": 41, "mpg_hwy": 38, "msrp": 33325},
                {"name": "Hybrid XLE", "engine": "2.5L Hybrid", "hp": 219, "torque": 163, "trans": "CVT", "drive": "AWD", "fuel": "Hybrid", "mpg_city": 41, "mpg_hwy": 38, "msrp": 36150},
                {"name": "Prime SE", "engine": "2.5L PHEV", "hp": 302, "torque": 199, "trans": "CVT", "drive": "AWD", "fuel": "PlugInHybrid", "mpg_city": 94, "mpg_hwy": 84, "msrp": 44340},
                {"name": "Prime XSE", "engine": "2.5L PHEV", "hp": 302, "torque": 199, "trans": "CVT", "drive": "AWD", "fuel": "PlugInHybrid", "mpg_city": 94, "mpg_hwy": 84, "msrp": 49770},
            ],
        },
        "Tacoma": {
            2024: [
                {"name": "SR", "engine": "2.4L Turbo I4", "hp": 228, "torque": 243, "trans": "Automatic", "drive": "RWD", "fuel": "Gasoline", "mpg_city": 21, "mpg_hwy": 26, "msrp": 35490},
                {"name": "SR5", "engine": "2.4L Turbo I4", "hp": 278, "torque": 317, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 19, "mpg_hwy": 24, "msrp": 38990},
                {"name": "TRD Sport", "engine": "2.4L Turbo I4", "hp": 278, "torque": 317, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 19, "mpg_hwy": 24, "msrp": 43745},
                {"name": "TRD Off-Road", "engine": "2.4L Turbo I4", "hp": 278, "torque": 317, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 19, "mpg_hwy": 24, "msrp": 45110},
                {"name": "Limited", "engine": "2.4L Turbo I4", "hp": 278, "torque": 317, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 19, "mpg_hwy": 24, "msrp": 51840},
                {"name": "TRD Pro", "engine": "2.4L Turbo I4", "hp": 278, "torque": 317, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 19, "mpg_hwy": 23, "msrp": 58490},
                {"name": "Trailhunter", "engine": "2.4L Turbo Hybrid", "hp": 326, "torque": 465, "trans": "Automatic", "drive": "4WD", "fuel": "Hybrid", "mpg_city": 23, "mpg_hwy": 24, "msrp": 63740},
            ],
        },
    },
    "Honda": {
        "Civic": {
            2024: [
                {"name": "LX", "engine": "2.0L I4", "hp": 158, "torque": 138, "trans": "CVT", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 31, "mpg_hwy": 40, "msrp": 24950},
                {"name": "Sport", "engine": "2.0L I4", "hp": 158, "torque": 138, "trans": "CVT", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 31, "mpg_hwy": 40, "msrp": 26600},
                {"name": "EX", "engine": "1.5L Turbo I4", "hp": 180, "torque": 177, "trans": "CVT", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 33, "mpg_hwy": 42, "msrp": 28300},
                {"name": "Touring", "engine": "1.5L Turbo I4", "hp": 180, "torque": 177, "trans": "CVT", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 33, "mpg_hwy": 42, "msrp": 30900},
                {"name": "Si", "engine": "1.5L Turbo I4", "hp": 200, "torque": 192, "trans": "Manual", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 27, "mpg_hwy": 37, "msrp": 30350},
                {"name": "Type R", "engine": "2.0L Turbo I4", "hp": 315, "torque": 310, "trans": "Manual", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 22, "mpg_hwy": 28, "msrp": 44845},
            ],
        },
        "Accord": {
            2024: [
                {"name": "LX", "engine": "1.5L Turbo I4", "hp": 192, "torque": 192, "trans": "CVT", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 29, "mpg_hwy": 37, "msrp": 28990},
                {"name": "EX", "engine": "1.5L Turbo I4", "hp": 192, "torque": 192, "trans": "CVT", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 29, "mpg_hwy": 37, "msrp": 32490},
                {"name": "EX-L", "engine": "1.5L Turbo I4", "hp": 192, "torque": 192, "trans": "CVT", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 29, "mpg_hwy": 37, "msrp": 34940},
                {"name": "Sport", "engine": "1.5L Turbo I4", "hp": 192, "torque": 192, "trans": "CVT", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 29, "mpg_hwy": 37, "msrp": 32990},
                {"name": "Sport-L", "engine": "1.5L Turbo I4", "hp": 192, "torque": 192, "trans": "CVT", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 29, "mpg_hwy": 37, "msrp": 35940},
                {"name": "Hybrid Sport", "engine": "2.0L Hybrid", "hp": 204, "torque": 247, "trans": "CVT", "drive": "FWD", "fuel": "Hybrid", "mpg_city": 51, "mpg_hwy": 44, "msrp": 33990},
                {"name": "Hybrid EX-L", "engine": "2.0L Hybrid", "hp": 204, "torque": 247, "trans": "CVT", "drive": "FWD", "fuel": "Hybrid", "mpg_city": 51, "mpg_hwy": 44, "msrp": 36940},
                {"name": "Hybrid Sport-L", "engine": "2.0L Hybrid", "hp": 204, "torque": 247, "trans": "CVT", "drive": "FWD", "fuel": "Hybrid", "mpg_city": 51, "mpg_hwy": 44, "msrp": 37940},
                {"name": "Hybrid Touring", "engine": "2.0L Hybrid", "hp": 204, "torque": 247, "trans": "CVT", "drive": "FWD", "fuel": "Hybrid", "mpg_city": 48, "mpg_hwy": 47, "msrp": 39890},
            ],
        },
        "CR-V": {
            2024: [
                {"name": "LX", "engine": "1.5L Turbo I4", "hp": 190, "torque": 179, "trans": "CVT", "drive": "FWD", "fuel": "Gasoline", "mpg_city": 28, "mpg_hwy": 34, "msrp": 30850},
                {"name": "EX", "engine": "1.5L Turbo I4", "hp": 190, "torque": 179, "trans": "CVT", "drive": "AWD", "fuel": "Gasoline", "mpg_city": 27, "mpg_hwy": 32, "msrp": 34555},
                {"name": "EX-L", "engine": "1.5L Turbo I4", "hp": 190, "torque": 179, "trans": "CVT", "drive": "AWD", "fuel": "Gasoline", "mpg_city": 27, "mpg_hwy": 32, "msrp": 36005},
                {"name": "Sport", "engine": "1.5L Turbo I4", "hp": 190, "torque": 179, "trans": "CVT", "drive": "AWD", "fuel": "Gasoline", "mpg_city": 27, "mpg_hwy": 32, "msrp": 35955},
                {"name": "Sport-L", "engine": "1.5L Turbo I4", "hp": 190, "torque": 179, "trans": "CVT", "drive": "AWD", "fuel": "Gasoline", "mpg_city": 27, "mpg_hwy": 32, "msrp": 37805},
                {"name": "Hybrid Sport", "engine": "2.0L Hybrid", "hp": 204, "torque": 247, "trans": "CVT", "drive": "AWD", "fuel": "Hybrid", "mpg_city": 43, "mpg_hwy": 36, "msrp": 34555},
                {"name": "Hybrid Sport-L", "engine": "2.0L Hybrid", "hp": 204, "torque": 247, "trans": "CVT", "drive": "AWD", "fuel": "Hybrid", "mpg_city": 43, "mpg_hwy": 36, "msrp": 38905},
                {"name": "Hybrid Sport Touring", "engine": "2.0L Hybrid", "hp": 204, "torque": 247, "trans": "CVT", "drive": "AWD", "fuel": "Hybrid", "mpg_city": 40, "mpg_hwy": 34, "msrp": 41305},
            ],
        },
    },
    "Ford": {
        "F-150": {
            2024: [
                {"name": "XL", "engine": "3.3L V6", "hp": 290, "torque": 265, "trans": "Automatic", "drive": "RWD", "fuel": "Gasoline", "mpg_city": 20, "mpg_hwy": 24, "msrp": 36920},
                {"name": "XLT", "engine": "2.7L EcoBoost V6", "hp": 325, "torque": 400, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 19, "mpg_hwy": 24, "msrp": 45835},
                {"name": "Lariat", "engine": "3.5L EcoBoost V6", "hp": 400, "torque": 500, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 17, "mpg_hwy": 23, "msrp": 57120},
                {"name": "King Ranch", "engine": "3.5L EcoBoost V6", "hp": 400, "torque": 500, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 17, "mpg_hwy": 23, "msrp": 66005},
                {"name": "Platinum", "engine": "3.5L EcoBoost V6", "hp": 400, "torque": 500, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 17, "mpg_hwy": 23, "msrp": 69405},
                {"name": "Limited", "engine": "3.5L EcoBoost V6", "hp": 400, "torque": 500, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 17, "mpg_hwy": 23, "msrp": 79005},
                {"name": "Raptor", "engine": "3.5L EcoBoost V6", "hp": 450, "torque": 510, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 15, "mpg_hwy": 18, "msrp": 76775},
                {"name": "Raptor R", "engine": "5.2L Supercharged V8", "hp": 720, "torque": 640, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 10, "mpg_hwy": 15, "msrp": 112590},
                {"name": "Lightning XLT", "engine": "Electric", "hp": 452, "torque": 775, "trans": "Automatic", "drive": "4WD", "fuel": "Electric", "mpg_city": 76, "mpg_hwy": 61, "msrp": 52990},
                {"name": "Lightning Lariat", "engine": "Electric", "hp": 580, "torque": 775, "trans": "Automatic", "drive": "4WD", "fuel": "Electric", "mpg_city": 76, "mpg_hwy": 61, "msrp": 70990},
            ],
        },
        "Mustang": {
            2024: [
                {"name": "EcoBoost", "engine": "2.3L Turbo I4", "hp": 315, "torque": 350, "trans": "Automatic", "drive": "RWD", "fuel": "Gasoline", "mpg_city": 22, "mpg_hwy": 32, "msrp": 32515},
                {"name": "EcoBoost Premium", "engine": "2.3L Turbo I4", "hp": 315, "torque": 350, "trans": "Automatic", "drive": "RWD", "fuel": "Gasoline", "mpg_city": 22, "mpg_hwy": 32, "msrp": 38020},
                {"name": "GT", "engine": "5.0L V8", "hp": 486, "torque": 418, "trans": "Automatic", "drive": "RWD", "fuel": "Gasoline", "mpg_city": 17, "mpg_hwy": 26, "msrp": 43180},
                {"name": "GT Premium", "engine": "5.0L V8", "hp": 486, "torque": 418, "trans": "Automatic", "drive": "RWD", "fuel": "Gasoline", "mpg_city": 17, "mpg_hwy": 26, "msrp": 48685},
                {"name": "Dark Horse", "engine": "5.0L V8", "hp": 500, "torque": 418, "trans": "Automatic", "drive": "RWD", "fuel": "Gasoline", "mpg_city": 15, "mpg_hwy": 24, "msrp": 59565},
                {"name": "Dark Horse Premium", "engine": "5.0L V8", "hp": 500, "torque": 418, "trans": "Automatic", "drive": "RWD", "fuel": "Gasoline", "mpg_city": 15, "mpg_hwy": 24, "msrp": 63165},
            ],
        },
    },
    "Tesla": {
        "Model 3": {
            2024: [
                {"name": "Standard Range", "engine": "Electric RWD", "hp": 272, "torque": 310, "trans": "Automatic", "drive": "RWD", "fuel": "Electric", "mpg_city": 138, "mpg_hwy": 126, "msrp": 38990},
                {"name": "Long Range AWD", "engine": "Dual Motor", "hp": 366, "torque": 376, "trans": "Automatic", "drive": "AWD", "fuel": "Electric", "mpg_city": 140, "mpg_hwy": 127, "msrp": 45990},
                {"name": "Performance", "engine": "Dual Motor", "hp": 510, "torque": 486, "trans": "Automatic", "drive": "AWD", "fuel": "Electric", "mpg_city": 133, "mpg_hwy": 125, "msrp": 52990},
            ],
        },
        "Model Y": {
            2024: [
                {"name": "Standard Range", "engine": "Electric RWD", "hp": 272, "torque": 310, "trans": "Automatic", "drive": "RWD", "fuel": "Electric", "mpg_city": 126, "mpg_hwy": 115, "msrp": 43990},
                {"name": "Long Range AWD", "engine": "Dual Motor", "hp": 384, "torque": 376, "trans": "Automatic", "drive": "AWD", "fuel": "Electric", "mpg_city": 127, "mpg_hwy": 117, "msrp": 48990},
                {"name": "Performance", "engine": "Dual Motor", "hp": 510, "torque": 486, "trans": "Automatic", "drive": "AWD", "fuel": "Electric", "mpg_city": 115, "mpg_hwy": 106, "msrp": 52990},
            ],
        },
    },
    "Chevrolet": {
        "Silverado 1500": {
            2024: [
                {"name": "WT", "engine": "2.7L Turbo I4", "hp": 310, "torque": 430, "trans": "Automatic", "drive": "RWD", "fuel": "Gasoline", "mpg_city": 20, "mpg_hwy": 23, "msrp": 37495},
                {"name": "Custom", "engine": "2.7L Turbo I4", "hp": 310, "torque": 430, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 19, "mpg_hwy": 22, "msrp": 42495},
                {"name": "LT", "engine": "5.3L V8", "hp": 355, "torque": 383, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 16, "mpg_hwy": 22, "msrp": 49095},
                {"name": "RST", "engine": "5.3L V8", "hp": 355, "torque": 383, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 16, "mpg_hwy": 22, "msrp": 53595},
                {"name": "LT Trail Boss", "engine": "5.3L V8", "hp": 355, "torque": 383, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 15, "mpg_hwy": 20, "msrp": 53095},
                {"name": "ZR2", "engine": "6.2L V8", "hp": 420, "torque": 460, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 14, "mpg_hwy": 18, "msrp": 74595},
                {"name": "High Country", "engine": "6.2L V8", "hp": 420, "torque": 460, "trans": "Automatic", "drive": "4WD", "fuel": "Gasoline", "mpg_city": 15, "mpg_hwy": 20, "msrp": 64895},
            ],
        },
    },
    "BMW": {
        "3 Series": {
            2024: [
                {"name": "330i", "engine": "2.0L Turbo I4", "hp": 255, "torque": 295, "trans": "Automatic", "drive": "RWD", "fuel": "Gasoline", "mpg_city": 26, "mpg_hwy": 36, "msrp": 44400},
                {"name": "330i xDrive", "engine": "2.0L Turbo I4", "hp": 255, "torque": 295, "trans": "Automatic", "drive": "AWD", "fuel": "Gasoline", "mpg_city": 25, "mpg_hwy": 34, "msrp": 46400},
                {"name": "330e", "engine": "2.0L PHEV", "hp": 288, "torque": 310, "trans": "Automatic", "drive": "RWD", "fuel": "PlugInHybrid", "mpg_city": 28, "mpg_hwy": 35, "msrp": 48900},
                {"name": "M340i", "engine": "3.0L Turbo I6", "hp": 382, "torque": 369, "trans": "Automatic", "drive": "RWD", "fuel": "Gasoline", "mpg_city": 24, "mpg_hwy": 33, "msrp": 57400},
                {"name": "M340i xDrive", "engine": "3.0L Turbo I6", "hp": 382, "torque": 369, "trans": "Automatic", "drive": "AWD", "fuel": "Gasoline", "mpg_city": 24, "mpg_hwy": 32, "msrp": 59400},
            ],
        },
    },
    "Mercedes-Benz": {
        "C-Class": {
            2024: [
                {"name": "C 300", "engine": "2.0L Turbo I4", "hp": 255, "torque": 295, "trans": "Automatic", "drive": "RWD", "fuel": "Gasoline", "mpg_city": 25, "mpg_hwy": 35, "msrp": 47550},
                {"name": "C 300 4MATIC", "engine": "2.0L Turbo I4", "hp": 255, "torque": 295, "trans": "Automatic", "drive": "AWD", "fuel": "Gasoline", "mpg_city": 24, "mpg_hwy": 33, "msrp": 49550},
                {"name": "AMG C 43 4MATIC", "engine": "2.0L Turbo I4", "hp": 402, "torque": 369, "trans": "Automatic", "drive": "AWD", "fuel": "Gasoline", "mpg_city": 22, "mpg_hwy": 30, "msrp": 60600},
                {"name": "AMG C 63 S E PERFORMANCE", "engine": "2.0L Turbo PHEV", "hp": 671, "torque": 752, "trans": "Automatic", "drive": "AWD", "fuel": "PlugInHybrid", "mpg_city": 16, "mpg_hwy": 22, "msrp": 89800},
            ],
        },
    },
}


# Marcas populares con información de país y logo
MAKES_INFO = {
    "Toyota": {"country": "Japan", "popular": True, "order": 1},
    "Honda": {"country": "Japan", "popular": True, "order": 2},
    "Ford": {"country": "USA", "popular": True, "order": 3},
    "Chevrolet": {"country": "USA", "popular": True, "order": 4},
    "Nissan": {"country": "Japan", "popular": True, "order": 5},
    "Jeep": {"country": "USA", "popular": True, "order": 6},
    "RAM": {"country": "USA", "popular": True, "order": 7},
    "GMC": {"country": "USA", "popular": True, "order": 8},
    "Hyundai": {"country": "South Korea", "popular": True, "order": 9},
    "Kia": {"country": "South Korea", "popular": True, "order": 10},
    "Tesla": {"country": "USA", "popular": True, "order": 11},
    "BMW": {"country": "Germany", "popular": True, "order": 12},
    "Mercedes-Benz": {"country": "Germany", "popular": True, "order": 13},
    "Audi": {"country": "Germany", "popular": True, "order": 14},
    "Lexus": {"country": "Japan", "popular": True, "order": 15},
    "Subaru": {"country": "Japan", "popular": False, "order": 16},
    "Volkswagen": {"country": "Germany", "popular": False, "order": 17},
    "Mazda": {"country": "Japan", "popular": False, "order": 18},
    "Dodge": {"country": "USA", "popular": False, "order": 19},
    "Porsche": {"country": "Germany", "popular": False, "order": 20},
}


def generate_uuid():
    return str(uuid.uuid4())


def slugify(text: str) -> str:
    return text.lower().replace(" ", "-").replace("'", "").replace("/", "-")


def generate_sql():
    """Genera SQL para insertar el catálogo en VehiclesSaleService."""
    
    sql_lines = [
        "-- ==============================================",
        "-- Vehicle Catalog Seed for VehiclesSaleService",
        f"-- Generated: {datetime.now().isoformat()}",
        "-- Flujo: Marca → Modelo → Año → Trim → Auto-fill specs",
        "-- ==============================================",
        "",
        "-- Limpiar datos existentes (opcional)",
        "-- TRUNCATE vehicle_trims, vehicle_models, vehicle_makes RESTART IDENTITY CASCADE;",
        "",
        "-- ========================================",
        "-- MARCAS",
        "-- ========================================",
        ""
    ]
    
    make_ids = {}
    model_ids = {}
    
    # Generar marcas
    for make_name, make_info in MAKES_INFO.items():
        make_id = generate_uuid()
        make_ids[make_name] = make_id
        
        sql_lines.append(
            f"INSERT INTO \"VehicleMakes\" (\"Id\", \"Name\", \"Slug\", \"Country\", \"IsPopular\", \"SortOrder\", \"IsActive\", \"CreatedAt\", \"UpdatedAt\") "
            f"VALUES ('{make_id}', '{make_name}', '{slugify(make_name)}', '{make_info['country']}', {str(make_info['popular']).lower()}, {make_info['order']}, true, NOW(), NOW()) "
            f"ON CONFLICT (\"Slug\") DO UPDATE SET \"Name\" = EXCLUDED.\"Name\", \"UpdatedAt\" = NOW();"
        )
    
    sql_lines.extend(["", "-- ========================================", "-- MODELOS", "-- ========================================", ""])
    
    # Generar modelos
    for make_name, models in TRIM_DATA.items():
        if make_name not in make_ids:
            continue
            
        for model_name in models.keys():
            model_id = generate_uuid()
            model_ids[(make_name, model_name)] = model_id
            
            years = list(models[model_name].keys())
            start_year = min(years)
            
            sql_lines.append(
                f"INSERT INTO \"VehicleModels\" (\"Id\", \"MakeId\", \"Name\", \"Slug\", \"VehicleType\", \"StartYear\", \"IsPopular\", \"IsActive\", \"CreatedAt\", \"UpdatedAt\") "
                f"VALUES ('{model_id}', '{make_ids[make_name]}', '{model_name}', '{slugify(model_name)}', 'Car', {start_year}, true, true, NOW(), NOW()) "
                f"ON CONFLICT (\"MakeId\", \"Slug\") DO UPDATE SET \"Name\" = EXCLUDED.\"Name\", \"UpdatedAt\" = NOW();"
            )
    
    sql_lines.extend(["", "-- ========================================", "-- TRIMS (con specs para auto-fill)", "-- ========================================", ""])
    
    # Generar trims
    trim_count = 0
    for make_name, models in TRIM_DATA.items():
        for model_name, years in models.items():
            if (make_name, model_name) not in model_ids:
                continue
                
            model_id = model_ids[(make_name, model_name)]
            
            for year, trims in years.items():
                for trim in trims:
                    trim_id = generate_uuid()
                    trim_count += 1
                    
                    # Mapear FuelType al enum
                    fuel_type_map = {
                        "Gasoline": "Gasoline",
                        "Hybrid": "Hybrid",
                        "Electric": "Electric",
                        "PlugInHybrid": "PlugInHybrid",
                        "Diesel": "Diesel"
                    }
                    fuel = fuel_type_map.get(trim.get("fuel", "Gasoline"), "Gasoline")
                    
                    # Mapear Transmission
                    trans = trim.get("trans", "Automatic")
                    if trans == "CVT":
                        trans = "CVT"
                    elif trans == "Manual":
                        trans = "Manual"
                    else:
                        trans = "Automatic"
                    
                    # Mapear DriveType
                    drive_map = {"FWD": "FWD", "AWD": "AWD", "RWD": "RWD", "4WD": "FourWD"}
                    drive = drive_map.get(trim.get("drive", "FWD"), "FWD")
                    
                    sql_lines.append(
                        f"INSERT INTO \"VehicleTrims\" ("
                        f"\"Id\", \"ModelId\", \"Name\", \"Slug\", \"Year\", \"EngineSize\", \"Horsepower\", \"Torque\", "
                        f"\"FuelType\", \"Transmission\", \"DriveType\", \"MpgCity\", \"MpgHighway\", \"BaseMSRP\", \"IsActive\", \"CreatedAt\", \"UpdatedAt\") "
                        f"VALUES ("
                        f"'{trim_id}', '{model_id}', '{trim['name']}', '{slugify(trim['name'])}', {year}, "
                        f"'{trim.get('engine', '')}', {trim.get('hp', 'NULL')}, {trim.get('torque', 'NULL')}, "
                        f"'{fuel}', '{trans}', '{drive}', "
                        f"{trim.get('mpg_city', 'NULL')}, {trim.get('mpg_hwy', 'NULL')}, {trim.get('msrp', 'NULL')}, "
                        f"true, NOW(), NOW()) "
                        f"ON CONFLICT (\"ModelId\", \"Year\", \"Slug\") DO UPDATE SET "
                        f"\"EngineSize\" = EXCLUDED.\"EngineSize\", \"Horsepower\" = EXCLUDED.\"Horsepower\", "
                        f"\"BaseMSRP\" = EXCLUDED.\"BaseMSRP\", \"UpdatedAt\" = NOW();"
                    )
    
    sql_lines.extend([
        "",
        "-- ========================================",
        "-- VERIFICACIÓN",
        "-- ========================================",
        "",
        "SELECT 'Makes: ' || COUNT(*) FROM \"VehicleMakes\";",
        "SELECT 'Models: ' || COUNT(*) FROM \"VehicleModels\";",
        "SELECT 'Trims: ' || COUNT(*) FROM \"VehicleTrims\";",
    ])
    
    # Guardar SQL
    sql_file = OUTPUT_DIR / "vehicle_catalog_vehiclessale.sql"
    with open(sql_file, "w", encoding="utf-8") as f:
        f.write("\n".join(sql_lines))
    
    print(f"✅ SQL guardado en: {sql_file}")
    print(f"   - {len(make_ids)} marcas")
    print(f"   - {len(model_ids)} modelos")
    print(f"   - {trim_count} trims con specs")
    
    return sql_file


def generate_json():
    """Genera JSON del catálogo para referencia."""
    
    catalog = {
        "generated_at": datetime.now().isoformat(),
        "makes": {},
        "stats": {
            "total_makes": 0,
            "total_models": 0,
            "total_trims": 0
        }
    }
    
    for make_name, make_info in MAKES_INFO.items():
        catalog["makes"][make_name] = {
            "country": make_info["country"],
            "popular": make_info["popular"],
            "models": {}
        }
        catalog["stats"]["total_makes"] += 1
        
        if make_name in TRIM_DATA:
            for model_name, years in TRIM_DATA[make_name].items():
                catalog["makes"][make_name]["models"][model_name] = {
                    "years": {}
                }
                catalog["stats"]["total_models"] += 1
                
                for year, trims in years.items():
                    catalog["makes"][make_name]["models"][model_name]["years"][str(year)] = trims
                    catalog["stats"]["total_trims"] += len(trims)
    
    json_file = OUTPUT_DIR / "vehicle_catalog_vehiclessale.json"
    with open(json_file, "w", encoding="utf-8") as f:
        json.dump(catalog, f, indent=2, ensure_ascii=False)
    
    print(f"✅ JSON guardado en: {json_file}")
    return json_file


def main():
    parser = argparse.ArgumentParser(description="Vehicle Catalog Seeder for VehiclesSaleService")
    parser.add_argument("--sql", action="store_true", help="Generar script SQL")
    parser.add_argument("--json", action="store_true", help="Generar JSON")
    parser.add_argument("--all", action="store_true", help="Generar todo")
    
    args = parser.parse_args()
    
    if args.all:
        args.sql = True
        args.json = True
    
    if not any([args.sql, args.json]):
        parser.print_help()
        print("\n💡 Ejemplo rápido:")
        print("   python seed-vehiclessale-catalog.py --all")
        return
    
    print("🚗 Vehicle Catalog Seeder for VehiclesSaleService")
    print("=" * 55)
    print("\n📊 Datos incluidos:")
    print(f"   - {len(MAKES_INFO)} marcas populares con info de país")
    print(f"   - Modelos populares con trims detallados")
    print(f"   - Specs: Motor, HP, Torque, MPG, MSRP")
    print()
    
    if args.json:
        generate_json()
    
    if args.sql:
        generate_sql()
    
    print("\n✅ Completado!")
    print("\n📋 Próximo paso: Ejecutar SQL en VehiclesSaleService DB:")
    print("   docker exec -i vehiclessaleservice-db psql -U postgres -d vehiclessaleservice < scripts/vehicle-data/vehiclessale/vehicle_catalog_vehiclessale.sql")


if __name__ == "__main__":
    main()
