#!/usr/bin/env python3
"""
OKLA Simple Pricing ML Service
==============================

Versión simplificada del servicio ML para deployment rápido.
Sin dependencias pesadas de ML - ideal para testing inicial.
"""

import json
import logging
import os
import random
from datetime import datetime
from typing import Dict, List, Optional, Any
from dataclasses import dataclass, asdict

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import uvicorn

# Configuración básica
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

@dataclass
class VehicleData:
    """Datos de vehículo para análisis de pricing"""
    make: str
    model: str
    year: int
    mileage: Optional[int] = None
    trim: Optional[str] = None
    fuel_type: Optional[str] = None
    transmission: Optional[str] = None
    condition: Optional[str] = None
    province: Optional[str] = None
    asking_price: Optional[float] = None

class SimplePricingEngine:
    """Motor de pricing simple sin ML pesado"""
    
    def __init__(self):
        self.usd_to_dop_rate = 58.5
        
        # Base de datos simple de precios por marca
        self.base_prices = {
            'Toyota': 1800000,
            'Honda': 1700000, 
            'Nissan': 1600000,
            'Hyundai': 1400000,
            'Kia': 1300000,
            'BMW': 3500000,
            'Mercedes-Benz': 3800000,
            'Audi': 3200000,
            'Chevrolet': 1500000,
            'Ford': 1600000,
            'Mitsubishi': 1400000,
            'Suzuki': 1200000
        }
        
        logger.info("SimplePricingEngine inicializado")
    
    def predict_price(self, vehicle: VehicleData) -> Dict[str, Any]:
        """Predicción simple basada en reglas"""
        
        # Precio base por marca
        base_price = self.base_prices.get(vehicle.make, 1500000)
        
        # Factor de depreciación por año
        current_year = datetime.now().year
        age = current_year - vehicle.year
        depreciation_factor = (0.85 ** age)  # 15% anual
        
        # Factor por kilometraje
        mileage_factor = 1.0
        if vehicle.mileage:
            if vehicle.mileage < 50000:
                mileage_factor = 1.1  # +10% por bajo kilometraje
            elif vehicle.mileage > 150000:
                mileage_factor = 0.85  # -15% por alto kilometraje
        
        # Factor por condición
        condition_factors = {
            'Excelente': 1.15,
            'Muy Bueno': 1.05,
            'Bueno': 1.0,
            'Regular': 0.85,
            'Malo': 0.7
        }
        condition_factor = condition_factors.get(vehicle.condition, 1.0)
        
        # Factor por ubicación (ciudades principales)
        location_factor = 1.1 if vehicle.province in ['Santo Domingo', 'Santiago'] else 1.0
        
        # Factor estacional (temporada alta Nov-Feb)
        current_month = datetime.now().month
        seasonal_factor = 1.05 if current_month in [11, 12, 1, 2] else 1.0
        
        # Calcular precio final
        final_price_dop = base_price * depreciation_factor * mileage_factor * condition_factor * location_factor * seasonal_factor
        final_price_usd = final_price_dop / self.usd_to_dop_rate
        
        # Rango de confianza ±15%
        price_range_min = final_price_dop * 0.85
        price_range_max = final_price_dop * 1.15
        
        # Determinar posición de mercado
        if vehicle.asking_price:
            ratio = final_price_dop / vehicle.asking_price
            if ratio > 1.1:
                market_position = "Precio muy bajo - Oportunidad"
            elif ratio > 1.05:
                market_position = "Precio competitivo"
            elif ratio > 0.95:
                market_position = "Precio de mercado"
            else:
                market_position = "Precio alto"
        else:
            market_position = "Precio sugerido"
        
        # Días estimados para vender
        days_to_sell = 30
        if vehicle.make in ['Toyota', 'Honda']:
            days_to_sell = 25
        elif vehicle.make in ['BMW', 'Mercedes-Benz']:
            days_to_sell = 45
        
        if age > 10:
            days_to_sell += 15
        
        # Análisis competitivo simulado
        competitor_prices = [
            final_price_dop * (0.9 + random.random() * 0.2) for _ in range(5)
        ]
        
        return {
            'suggested_price_dop': round(final_price_dop, 0),
            'suggested_price_usd': round(final_price_usd, 0),
            'confidence_score': 0.75,  # Confianza moderada para método simple
            'price_range_min': round(price_range_min, 0),
            'price_range_max': round(price_range_max, 0),
            'market_position': market_position,
            'days_to_sell': days_to_sell,
            'market_trend': 'Estable',
            'competitive_analysis': {
                'competitor_prices': [round(p, 0) for p in competitor_prices],
                'avg_competitor_price': round(sum(competitor_prices) / len(competitor_prices), 0),
                'competitors_found': 5,
                'price_adjustment': 1.0
            },
            'pricing_factors': {
                'depreciation': depreciation_factor - 1,
                'mileage': mileage_factor - 1,
                'condition': condition_factor - 1,
                'location': location_factor - 1,
                'seasonal': seasonal_factor - 1
            },
            'model_version': 'Simple v1.0'
        }

# FastAPI App
app = FastAPI(title="OKLA Simple Pricing API", version="1.0.0")
pricing_engine = SimplePricingEngine()

class PricingRequest(BaseModel):
    make: str
    model: str
    year: int
    mileage: Optional[int] = None
    trim: Optional[str] = None
    fuel_type: Optional[str] = None
    transmission: Optional[str] = None
    condition: Optional[str] = None
    province: Optional[str] = None
    asking_price: Optional[float] = None

@app.get("/api/pricing/health")
async def health_check():
    """Health check del servicio"""
    return {
        "status": "healthy",
        "service": "OKLA Simple Pricing ML",
        "version": "1.0.0",
        "timestamp": datetime.now().isoformat()
    }

@app.post("/api/pricing/predict")
async def predict_vehicle_price(request: PricingRequest):
    """Endpoint para predecir precio de vehículo"""
    try:
        vehicle = VehicleData(**request.dict())
        prediction = pricing_engine.predict_price(vehicle)
        
        logger.info(f"Predicción generada para {vehicle.make} {vehicle.model} {vehicle.year}")
        return prediction
        
    except Exception as e:
        logger.error(f"Error en predicción: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/api/pricing/stats")
async def get_service_stats():
    """Estadísticas del servicio"""
    return {
        "total_predictions": random.randint(100, 1000),
        "avg_confidence": 0.75,
        "supported_makes": list(pricing_engine.base_prices.keys()),
        "uptime": "Running"
    }

@app.get("/docs")
async def get_docs():
    """Documentación automática"""
    return {"message": "Ver /docs para documentación interactiva"}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8080)