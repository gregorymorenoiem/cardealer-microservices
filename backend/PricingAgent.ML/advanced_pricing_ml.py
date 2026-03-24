#!/usr/bin/env python3
"""
OKLA Advanced Pricing ML Engine
===============================

Sistema completo de pricing inteligente con ML para el marketplace de vehículos OKLA.
Incluye predicción de precios, análisis de mercado y estrategias de pricing dinámico.

Autor: OKLA Development Team
Fecha: Marzo 2026
"""

import os
import json
import logging
import numpy as np
import pandas as pd
import joblib
import asyncio
import aiohttp
from datetime import datetime, timedelta
from typing import Dict, List, Optional, Tuple, Any
from dataclasses import dataclass, asdict
from pathlib import Path

# ML Libraries
import xgboost as xgb
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import LabelEncoder, StandardScaler
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score
from sklearn.ensemble import IsolationForest

# Web Scraping
import requests
from bs4 import BeautifulSoup
import asyncio
import aiohttp

# FastAPI para API
from fastapi import FastAPI, HTTPException, BackgroundTasks
from pydantic import BaseModel
import uvicorn

# Configuración
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
    features: List[str] = None
    
    def __post_init__(self):
        if self.features is None:
            self.features = []

@dataclass
class PricingPrediction:
    """Resultado de predicción de precio"""
    suggested_price_dop: float
    suggested_price_usd: float
    confidence_score: float
    price_range_min: float
    price_range_max: float
    market_position: str
    days_to_sell: int
    market_trend: str
    competitive_analysis: Dict[str, Any]
    pricing_factors: Dict[str, float]
    model_version: str
    
class AdvancedPricingEngine:
    """Motor de pricing avanzado con ML"""
    
    def __init__(self, model_path: str = "models/", data_path: str = "data/"):
        self.model_path = Path(model_path)
        self.data_path = Path(data_path)
        self.model_path.mkdir(exist_ok=True)
        self.data_path.mkdir(exist_ok=True)
        
        # Modelos ML
        self.xgb_model = None
        self.feature_pipeline = None
        self.label_encoders = {}
        self.scaler = StandardScaler()
        self.anomaly_detector = IsolationForest(contamination=0.1, random_state=42)
        
        # Cache para competidores
        self.competitor_cache = {}
        self.cache_expiry = timedelta(hours=6)
        
        # Configuración DOM (República Dominicana)
        self.usd_to_dop_rate = 58.5  # Se actualiza automáticamente
        self.provinces = [
            'Santo Domingo', 'Santiago', 'La Vega', 'San Pedro de Macorís',
            'Puerto Plata', 'La Romana', 'Moca', 'Higüey', 'Bonao', 'Baní'
        ]
        
        logger.info("AdvancedPricingEngine inicializado")

    async def initialize(self):
        """Inicializar modelos y datos"""
        try:
            await self._update_exchange_rate()
            await self._load_models()
            await self._prepare_training_data()
            logger.info("Inicialización completada exitosamente")
        except Exception as e:
            logger.error(f"Error en inicialización: {e}")
            raise

    async def _update_exchange_rate(self):
        """Actualizar tasa de cambio USD/DOP en tiempo real"""
        try:
            # API del Banco Central de RD o similar
            async with aiohttp.ClientSession() as session:
                url = "https://api.exchangerate-api.com/v4/latest/USD"
                async with session.get(url) as response:
                    if response.status == 200:
                        data = await response.json()
                        self.usd_to_dop_rate = data['rates'].get('DOP', 58.5)
                        logger.info(f"Tasa actualizada: 1 USD = {self.usd_to_dop_rate} DOP")
        except Exception as e:
            logger.warning(f"No se pudo actualizar tasa de cambio: {e}")

    def _prepare_features(self, vehicles_df: pd.DataFrame) -> pd.DataFrame:
        """Preparar features para el modelo ML"""
        df = vehicles_df.copy()
        
        # Feature Engineering
        df['vehicle_age'] = datetime.now().year - df['year']
        df['mileage_per_year'] = df['mileage'] / (df['vehicle_age'] + 1)
        df['is_luxury_brand'] = df['make'].isin(['BMW', 'Mercedes-Benz', 'Audi', 'Lexus', 'Acura'])
        df['is_reliable_brand'] = df['make'].isin(['Toyota', 'Honda', 'Nissan', 'Mazda', 'Hyundai'])
        
        # Seasonal factors
        current_month = datetime.now().month
        df['is_high_season'] = current_month in [11, 12, 1, 2]  # Nov-Feb (temporada alta RD)
        
        # Location factors
        df['is_major_city'] = df['province'].isin(['Santo Domingo', 'Santiago'])
        
        # Condition scoring
        condition_scores = {
            'Excelente': 1.0, 'Muy Bueno': 0.8, 'Bueno': 0.6, 
            'Regular': 0.4, 'Malo': 0.2
        }
        df['condition_score'] = df['condition'].map(condition_scores).fillna(0.6)
        
        # Encode categorical variables
        categorical_cols = ['make', 'model', 'fuel_type', 'transmission', 'province']
        
        for col in categorical_cols:
            if col not in self.label_encoders:
                self.label_encoders[col] = LabelEncoder()
                df[f'{col}_encoded'] = self.label_encoders[col].fit_transform(df[col].fillna('Unknown'))
            else:
                # Para nuevos datos, manejar categorías no vistas
                known_categories = self.label_encoders[col].classes_
                df[col] = df[col].fillna('Unknown')
                mask = df[col].isin(known_categories)
                df.loc[~mask, col] = 'Unknown'
                df[f'{col}_encoded'] = self.label_encoders[col].transform(df[col])
        
        # Features finales para el modelo
        feature_columns = [
            'year', 'mileage', 'vehicle_age', 'mileage_per_year', 'condition_score',
            'is_luxury_brand', 'is_reliable_brand', 'is_high_season', 'is_major_city'
        ] + [f'{col}_encoded' for col in categorical_cols]
        
        return df[feature_columns]

    async def _prepare_training_data(self):
        """Preparar datos de entrenamiento desde múltiples fuentes"""
        logger.info("Preparando datos de entrenamiento...")
        
        # 1. Datos históricos de OKLA (simulados)
        okla_data = self._generate_synthetic_data(n_samples=10000)
        
        # 2. Scraping de competidores (en paralelo)
        competitor_data = await self._scrape_competitor_data()
        
        # 3. Combinar y limpiar datos
        all_data = pd.concat([okla_data, competitor_data], ignore_index=True)
        all_data = self._clean_training_data(all_data)
        
        # 4. Guardar dataset
        dataset_path = self.data_path / "training_dataset.csv"
        all_data.to_csv(dataset_path, index=False)
        logger.info(f"Dataset guardado: {len(all_data)} registros en {dataset_path}")
        
        return all_data

    def _generate_synthetic_data(self, n_samples: int = 10000) -> pd.DataFrame:
        """Generar datos sintéticos realistas para República Dominicana"""
        np.random.seed(42)
        
        # Marcas populares en RD
        makes = ['Toyota', 'Honda', 'Nissan', 'Hyundai', 'Kia', 'Mitsubishi', 
                'Chevrolet', 'Ford', 'BMW', 'Mercedes-Benz', 'Audi', 'Suzuki']
        
        models_by_make = {
            'Toyota': ['Corolla', 'Camry', 'RAV4', 'Prado', 'Yaris', 'Hilux'],
            'Honda': ['Civic', 'Accord', 'CR-V', 'Fit', 'Pilot', 'HR-V'],
            'Nissan': ['Sentra', 'Altima', 'X-Trail', 'Pathfinder', 'Versa'],
            # ... más modelos
        }
        
        data = []
        for _ in range(n_samples):
            make = np.random.choice(makes)
            model = np.random.choice(models_by_make.get(make, ['Sedan', 'SUV', 'Hatchback']))
            year = np.random.randint(2015, 2024)
            age = 2024 - year
            
            # Mileage realista basado en edad
            base_mileage = age * np.random.uniform(8000, 25000)
            mileage = max(0, int(base_mileage + np.random.normal(0, 5000)))
            
            # Precio base por marca y modelo
            if make in ['BMW', 'Mercedes-Benz', 'Audi']:
                base_price = np.random.uniform(1500000, 4000000)  # DOP
            elif make in ['Toyota', 'Honda', 'Nissan']:
                base_price = np.random.uniform(800000, 2500000)
            else:
                base_price = np.random.uniform(600000, 2000000)
            
            # Ajustes por factores
            age_depreciation = (1 - age * 0.15) ** 0.7
            mileage_factor = max(0.3, 1 - (mileage / 200000) * 0.4)
            condition_factor = np.random.uniform(0.8, 1.1)
            
            final_price = base_price * age_depreciation * mileage_factor * condition_factor
            
            data.append({
                'make': make,
                'model': model,
                'year': year,
                'mileage': mileage,
                'fuel_type': np.random.choice(['Gasolina', 'Diésel', 'Híbrido']),
                'transmission': np.random.choice(['Manual', 'Automática']),
                'condition': np.random.choice(['Excelente', 'Muy Bueno', 'Bueno', 'Regular'], 
                                           p=[0.2, 0.4, 0.3, 0.1]),
                'province': np.random.choice(self.provinces),
                'sale_price': final_price,
                'source': 'synthetic'
            })
        
        return pd.DataFrame(data)

    async def _scrape_competitor_data(self) -> pd.DataFrame:
        """Scraping de datos de competidores (SuperCarros, etc.)"""
        logger.info("Scraping datos de competidores...")
        
        # TODO: Implementar scraping real de SuperCarros y otros
        # Por ahora retornamos datos simulados
        competitor_data = self._generate_synthetic_data(n_samples=2000)
        competitor_data['source'] = 'competitor'
        
        return competitor_data

    def _clean_training_data(self, df: pd.DataFrame) -> pd.DataFrame:
        """Limpiar y validar datos de entrenamiento"""
        logger.info("Limpiando datos de entrenamiento...")
        
        # Remover outliers extremos
        df = df[df['sale_price'].between(100000, 10000000)]  # Precios razonables
        df = df[df['year'].between(2000, 2024)]
        df = df[df['mileage'].between(0, 500000)]
        
        # Remover duplicados
        df = df.drop_duplicates(['make', 'model', 'year', 'mileage', 'sale_price'])
        
        # Detectar anomalías con Isolation Forest
        features_for_anomaly = ['year', 'mileage', 'sale_price']
        X_anomaly = df[features_for_anomaly].fillna(0)
        anomaly_labels = self.anomaly_detector.fit_predict(X_anomaly)
        df = df[anomaly_labels == 1]  # Mantener solo datos normales
        
        logger.info(f"Datos limpios: {len(df)} registros")
        return df

    async def train_model(self, retrain: bool = False):
        """Entrenar el modelo XGBoost"""
        model_file = self.model_path / "pricing_model.joblib"
        
        if model_file.exists() and not retrain:
            logger.info("Cargando modelo existente...")
            self.xgb_model = joblib.load(model_file)
            return
        
        logger.info("Entrenando nuevo modelo XGBoost...")
        
        # Cargar datos
        dataset_path = self.data_path / "training_dataset.csv"
        if not dataset_path.exists():
            await self._prepare_training_data()
        
        df = pd.read_csv(dataset_path)
        
        # Preparar features
        X = self._prepare_features(df)
        y = df['sale_price']
        
        # Split train/test
        X_train, X_test, y_train, y_test = train_test_split(
            X, y, test_size=0.2, random_state=42
        )
        
        # Entrenar modelo XGBoost
        self.xgb_model = xgb.XGBRegressor(
            n_estimators=200,
            max_depth=8,
            learning_rate=0.1,
            subsample=0.8,
            colsample_bytree=0.8,
            random_state=42,
            n_jobs=-1
        )
        
        self.xgb_model.fit(X_train, y_train)
        
        # Evaluación
        y_pred = self.xgb_model.predict(X_test)
        mae = mean_absolute_error(y_test, y_pred)
        rmse = np.sqrt(mean_squared_error(y_test, y_pred))
        r2 = r2_score(y_test, y_pred)
        
        logger.info(f"Métricas del modelo:")
        logger.info(f"  MAE: {mae:,.0f} DOP")
        logger.info(f"  RMSE: {rmse:,.0f} DOP")
        logger.info(f"  R²: {r2:.3f}")
        
        # Guardar modelo
        joblib.dump(self.xgb_model, model_file)
        logger.info(f"Modelo guardado en {model_file}")

    async def _load_models(self):
        """Cargar modelos pre-entrenados"""
        try:
            model_file = self.model_path / "pricing_model.joblib"
            if model_file.exists():
                self.xgb_model = joblib.load(model_file)
                logger.info("Modelo cargado exitosamente")
            else:
                logger.info("No se encontró modelo, entrenando nuevo modelo...")
                await self.train_model()
        except Exception as e:
            logger.error(f"Error cargando modelo: {e}")
            raise

    async def predict_price(self, vehicle: VehicleData) -> PricingPrediction:
        """Predecir precio de un vehículo con análisis completo"""
        if self.xgb_model is None:
            raise ValueError("Modelo no entrenado")
        
        start_time = datetime.now()
        
        # Convertir a DataFrame para procesamiento
        vehicle_df = pd.DataFrame([asdict(vehicle)])
        
        # Preparar features
        X = self._prepare_features(vehicle_df)
        
        # Predicción base
        base_prediction = self.xgb_model.predict(X)[0]
        
        # Análisis de mercado y ajustes
        market_analysis = await self._analyze_market_conditions(vehicle)
        competitive_analysis = await self._analyze_competition(vehicle)
        
        # Aplicar ajustes de mercado
        market_multiplier = market_analysis['trend_multiplier']
        competition_adjustment = competitive_analysis['price_adjustment']
        
        final_price_dop = base_prediction * market_multiplier * competition_adjustment
        final_price_usd = final_price_dop / self.usd_to_dop_rate
        
        # Calcular rango de confianza
        confidence_interval = 0.15  # ±15%
        price_range_min = final_price_dop * (1 - confidence_interval)
        price_range_max = final_price_dop * (1 + confidence_interval)
        
        # Determinar posición en el mercado
        market_position = self._determine_market_position(
            final_price_dop, competitive_analysis['competitor_prices']
        )
        
        # Estimar días para vender
        days_to_sell = self._estimate_days_to_sell(vehicle, final_price_dop, market_analysis)
        
        # Factores de pricing
        pricing_factors = self._analyze_pricing_factors(vehicle, market_analysis, competitive_analysis)
        
        # Calcular confianza del modelo
        confidence_score = self._calculate_confidence_score(
            vehicle, market_analysis, competitive_analysis
        )
        
        latency = (datetime.now() - start_time).total_seconds() * 1000
        
        return PricingPrediction(
            suggested_price_dop=round(final_price_dop, 0),
            suggested_price_usd=round(final_price_usd, 0),
            confidence_score=confidence_score,
            price_range_min=round(price_range_min, 0),
            price_range_max=round(price_range_max, 0),
            market_position=market_position,
            days_to_sell=days_to_sell,
            market_trend=market_analysis['trend'],
            competitive_analysis=competitive_analysis,
            pricing_factors=pricing_factors,
            model_version="v1.0.0"
        )

    async def _analyze_market_conditions(self, vehicle: VehicleData) -> Dict[str, Any]:
        """Analizar condiciones actuales del mercado"""
        # Simulación de análisis de mercado
        # En producción, esto consultaría APIs externas y data real
        
        current_month = datetime.now().month
        is_high_season = current_month in [11, 12, 1, 2]
        
        # Tendencias por tipo de vehículo
        if vehicle.make in ['Toyota', 'Honda', 'Nissan']:
            trend_multiplier = 1.05 if is_high_season else 1.0
            trend = "Estable al alza"
        elif vehicle.make in ['BMW', 'Mercedes-Benz', 'Audi']:
            trend_multiplier = 0.95 if not is_high_season else 1.1
            trend = "Volátil estacional"
        else:
            trend_multiplier = 1.0
            trend = "Estable"
        
        return {
            'trend': trend,
            'trend_multiplier': trend_multiplier,
            'is_high_season': is_high_season,
            'market_demand': 'Alto' if is_high_season else 'Moderado'
        }

    async def _analyze_competition(self, vehicle: VehicleData) -> Dict[str, Any]:
        """Analizar precios de la competencia"""
        # Cache key para evitar múltiples scraping
        cache_key = f"{vehicle.make}_{vehicle.model}_{vehicle.year}"
        
        if (cache_key in self.competitor_cache and 
            datetime.now() - self.competitor_cache[cache_key]['timestamp'] < self.cache_expiry):
            return self.competitor_cache[cache_key]['data']
        
        # Simulación de análisis competitivo
        # En producción, esto haría scraping real
        
        base_price = 1500000  # Simulado
        competitor_prices = [
            base_price * np.random.uniform(0.9, 1.1) for _ in range(5)
        ]
        
        avg_competitor_price = np.mean(competitor_prices)
        price_adjustment = 0.98  # Ser ligeramente más competitivo
        
        analysis = {
            'competitor_prices': competitor_prices,
            'avg_competitor_price': avg_competitor_price,
            'price_adjustment': price_adjustment,
            'competitors_found': len(competitor_prices)
        }
        
        # Actualizar cache
        self.competitor_cache[cache_key] = {
            'data': analysis,
            'timestamp': datetime.now()
        }
        
        return analysis

    def _determine_market_position(self, predicted_price: float, competitor_prices: List[float]) -> str:
        """Determinar posición en el mercado"""
        if not competitor_prices:
            return "Sin competencia directa"
        
        avg_competitor = np.mean(competitor_prices)
        ratio = predicted_price / avg_competitor
        
        if ratio < 0.9:
            return "Muy competitivo"
        elif ratio < 0.95:
            return "Competitivo"
        elif ratio < 1.05:
            return "Precio de mercado"
        elif ratio < 1.15:
            return "Premium"
        else:
            return "Muy por encima del mercado"

    def _estimate_days_to_sell(self, vehicle: VehicleData, price: float, market_analysis: Dict) -> int:
        """Estimar días para vender basado en precio y mercado"""
        base_days = 30
        
        # Ajustes por marca
        if vehicle.make in ['Toyota', 'Honda']:
            base_days -= 5
        elif vehicle.make in ['BMW', 'Mercedes-Benz']:
            base_days += 10
        
        # Ajustes por condición del mercado
        if market_analysis['is_high_season']:
            base_days -= 7
        
        # Ajustes por edad del vehículo
        age = datetime.now().year - vehicle.year
        if age > 10:
            base_days += 15
        elif age < 3:
            base_days -= 5
        
        return max(7, min(90, base_days))

    def _analyze_pricing_factors(self, vehicle: VehicleData, market_analysis: Dict, 
                                competitive_analysis: Dict) -> Dict[str, float]:
        """Analizar factores que afectan el pricing"""
        factors = {}
        
        # Factor de marca
        if vehicle.make in ['Toyota', 'Honda', 'Nissan']:
            factors['brand_reliability'] = 0.05
        elif vehicle.make in ['BMW', 'Mercedes-Benz', 'Audi']:
            factors['luxury_premium'] = 0.10
        
        # Factor de edad
        age = datetime.now().year - vehicle.year
        if age < 3:
            factors['low_depreciation'] = 0.08
        elif age > 10:
            factors['high_depreciation'] = -0.15
        
        # Factor de millaje
        if vehicle.mileage and vehicle.mileage < 50000:
            factors['low_mileage'] = 0.06
        elif vehicle.mileage and vehicle.mileage > 150000:
            factors['high_mileage'] = -0.10
        
        # Factor estacional
        if market_analysis['is_high_season']:
            factors['seasonal_demand'] = 0.03
        
        # Factor competitivo
        if competitive_analysis['competitors_found'] > 3:
            factors['high_competition'] = -0.02
        
        return factors

    def _calculate_confidence_score(self, vehicle: VehicleData, market_analysis: Dict, 
                                  competitive_analysis: Dict) -> float:
        """Calcular score de confianza de la predicción"""
        confidence = 0.8  # Base
        
        # Aumentar confianza si hay más datos
        if competitive_analysis['competitors_found'] > 2:
            confidence += 0.1
        
        # Reducir confianza para vehículos muy viejos o raros
        age = datetime.now().year - vehicle.year
        if age > 15:
            confidence -= 0.2
        
        if vehicle.make not in ['Toyota', 'Honda', 'Nissan', 'Hyundai', 'Kia']:
            confidence -= 0.1
        
        return max(0.3, min(0.95, confidence))

# FastAPI App para integración
app = FastAPI(title="OKLA Advanced Pricing API", version="1.0.0")
pricing_engine = AdvancedPricingEngine()

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

@app.on_event("startup")
async def startup_event():
    await pricing_engine.initialize()

@app.post("/api/pricing/predict")
async def predict_vehicle_price(request: PricingRequest):
    """Endpoint para predecir precio de vehículo"""
    try:
        vehicle = VehicleData(**request.dict())
        prediction = await pricing_engine.predict_price(vehicle)
        return asdict(prediction)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/pricing/train")
async def retrain_model(background_tasks: BackgroundTasks):
    """Endpoint para reentrenar el modelo"""
    background_tasks.add_task(pricing_engine.train_model, retrain=True)
    return {"message": "Reentrenamiento iniciado en background"}

@app.get("/api/pricing/health")
async def health_check():
    """Health check del servicio"""
    return {
        "status": "healthy",
        "model_loaded": pricing_engine.xgb_model is not None,
        "timestamp": datetime.now().isoformat()
    }

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8080)