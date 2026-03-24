#!/usr/bin/env python3
"""
Market Intelligence Web Scraper
===============================

Scraper para recopilar datos de precios de vehículos de la competencia
en República Dominicana. Incluye SuperCarros, Facebook Marketplace, y otros.
"""

import asyncio
import aiohttp
import logging
import json
import time
from datetime import datetime, timedelta
from typing import Dict, List, Optional, Any
from dataclasses import dataclass, asdict
from pathlib import Path
import re

import pandas as pd
from bs4 import BeautifulSoup
from selenium import webdriver
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC

import psycopg2
import redis
from sqlalchemy import create_engine

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

@dataclass
class ScrapedVehicle:
    """Datos de vehículo scrapeado"""
    source: str
    make: str
    model: str
    year: int
    price: float
    currency: str
    mileage: Optional[int] = None
    location: Optional[str] = None
    condition: Optional[str] = None
    fuel_type: Optional[str] = None
    transmission: Optional[str] = None
    description: Optional[str] = None
    images: List[str] = None
    url: str = ""
    scraped_at: datetime = None
    
    def __post_init__(self):
        if self.images is None:
            self.images = []
        if self.scraped_at is None:
            self.scraped_at = datetime.now()

class MarketIntelligenceScraper:
    """Scraper principal para inteligencia de mercado"""
    
    def __init__(self):
        self.scraped_data = []
        self.session = aiohttp.ClientSession(
            timeout=aiohttp.ClientTimeout(total=30),
            headers={
                'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
            }
        )
        
        # Setup Selenium
        chrome_options = Options()
        chrome_options.add_argument('--headless')
        chrome_options.add_argument('--no-sandbox')
        chrome_options.add_argument('--disable-dev-shm-usage')
        chrome_options.add_argument('--disable-gpu')
        chrome_options.add_argument('--window-size=1920,1080')
        
        self.driver = webdriver.Chrome(options=chrome_options)
        self.wait = WebDriverWait(self.driver, 10)
        
        # Database connection
        self.db_engine = create_engine(
            f"postgresql://oklauser:{os.getenv('POSTGRES_PASSWORD')}@postgres:5432/pricingagent"
        )
        
        # Redis para cache
        self.redis_client = redis.Redis(host='redis', port=6379, decode_responses=True)
        
        logger.info("MarketIntelligenceScraper inicializado")

    async def scrape_all_sources(self) -> List[ScrapedVehicle]:
        """Scrape de todas las fuentes configuradas"""
        all_vehicles = []
        
        try:
            # 1. SuperCarros (principal competidor)
            supercarros_data = await self.scrape_supercarros()
            all_vehicles.extend(supercarros_data)
            
            # 2. Facebook Marketplace (si es accesible)
            facebook_data = await self.scrape_facebook_marketplace()
            all_vehicles.extend(facebook_data)
            
            # 3. Clasificados locales
            clasificados_data = await self.scrape_clasificados_locales()
            all_vehicles.extend(clasificados_data)
            
            # 4. Guardar en base de datos
            await self.save_to_database(all_vehicles)
            
            logger.info(f"Scraped {len(all_vehicles)} vehículos en total")
            return all_vehicles
            
        except Exception as e:
            logger.error(f"Error en scraping: {e}")
            return []

    async def scrape_supercarros(self) -> List[ScrapedVehicle]:
        """Scrape específico de SuperCarros.com"""
        logger.info("Scraping SuperCarros...")
        vehicles = []
        
        try:
            # URLs base para diferentes categorías
            base_urls = [
                "https://supercarros.com/autos-usados",
                "https://supercarros.com/suv-usados", 
                "https://supercarros.com/pickup-usados"
            ]
            
            for base_url in base_urls:
                page_vehicles = await self._scrape_supercarros_category(base_url)
                vehicles.extend(page_vehicles)
                
                # Respeto hacia el sitio - delay entre requests
                await asyncio.sleep(2)
            
            logger.info(f"SuperCarros: {len(vehicles)} vehículos scraped")
            return vehicles
            
        except Exception as e:
            logger.error(f"Error scraping SuperCarros: {e}")
            return []

    async def _scrape_supercarros_category(self, base_url: str) -> List[ScrapedVehicle]:
        """Scrape una categoría específica de SuperCarros"""
        vehicles = []
        
        try:
            # Usar Selenium para JavaScript heavy sites
            self.driver.get(base_url)
            await asyncio.sleep(3)
            
            # Scroll para cargar más contenido
            self.driver.execute_script("window.scrollTo(0, document.body.scrollHeight);")
            await asyncio.sleep(2)
            
            # Buscar listados de vehículos
            vehicle_elements = self.driver.find_elements(By.CLASS_NAME, "vehicle-card")
            
            for element in vehicle_elements[:50]:  # Limitar para no saturar
                try:
                    vehicle_data = self._parse_supercarros_vehicle(element)
                    if vehicle_data:
                        vehicles.append(vehicle_data)
                except Exception as e:
                    logger.warning(f"Error parsing vehicle element: {e}")
                    continue
            
            return vehicles
            
        except Exception as e:
            logger.error(f"Error scraping category {base_url}: {e}")
            return []

    def _parse_supercarros_vehicle(self, element) -> Optional[ScrapedVehicle]:
        """Parse un elemento de vehículo de SuperCarros"""
        try:
            # Extraer datos básicos
            title = element.find_element(By.CLASS_NAME, "vehicle-title").text
            price_text = element.find_element(By.CLASS_NAME, "vehicle-price").text
            
            # Parse título para obtener marca, modelo, año
            title_parts = title.split()
            if len(title_parts) < 3:
                return None
            
            # Heurística para parsear título
            year_match = re.search(r'\b(19|20)\d{2}\b', title)
            year = int(year_match.group()) if year_match else 2020
            
            # Extraer marca y modelo (simplificado)
            make = title_parts[0] if title_parts else "Unknown"
            model = title_parts[1] if len(title_parts) > 1 else "Unknown"
            
            # Parse precio
            price_cleaned = re.sub(r'[^\d.]', '', price_text)
            price = float(price_cleaned) if price_cleaned else 0
            
            # Determinar moneda
            currency = "USD" if "$" in price_text else "DOP"
            
            # Intentar obtener más detalles
            try:
                details = element.find_element(By.CLASS_NAME, "vehicle-details").text
                mileage_match = re.search(r'(\d+(?:,\d+)*)\s*km', details, re.IGNORECASE)
                mileage = int(mileage_match.group(1).replace(',', '')) if mileage_match else None
            except:
                mileage = None
            
            # URL del vehículo
            try:
                url = element.find_element(By.TAG_NAME, "a").get_attribute("href")
            except:
                url = ""
            
            return ScrapedVehicle(
                source="supercarros",
                make=make,
                model=model,
                year=year,
                price=price,
                currency=currency,
                mileage=mileage,
                location="República Dominicana",
                url=url
            )
            
        except Exception as e:
            logger.warning(f"Error parsing SuperCarros vehicle: {e}")
            return None

    async def scrape_facebook_marketplace(self) -> List[ScrapedVehicle]:
        """Scrape Facebook Marketplace (limitado por términos de servicio)"""
        logger.info("Scraping Facebook Marketplace...")
        
        # NOTA: Facebook Marketplace tiene restricciones estrictas de scraping
        # En producción, considerar usar Facebook Graph API con permisos apropiados
        
        try:
            # Simulación de datos para demostración
            # En producción, implementar con Graph API oficial
            facebook_vehicles = self._generate_facebook_sample_data()
            
            logger.info(f"Facebook Marketplace: {len(facebook_vehicles)} vehículos (simulados)")
            return facebook_vehicles
            
        except Exception as e:
            logger.error(f"Error scraping Facebook: {e}")
            return []

    def _generate_facebook_sample_data(self) -> List[ScrapedVehicle]:
        """Generar datos de muestra para Facebook Marketplace"""
        # Datos simulados basados en patrones reales del mercado dominicano
        sample_data = [
            ScrapedVehicle(
                source="facebook_marketplace",
                make="Toyota",
                model="Corolla",
                year=2018,
                price=850000,
                currency="DOP",
                mileage=75000,
                location="Santo Domingo"
            ),
            ScrapedVehicle(
                source="facebook_marketplace", 
                make="Honda",
                model="Civic",
                year=2019,
                price=950000,
                currency="DOP",
                mileage=60000,
                location="Santiago"
            )
            # Más datos simulados...
        ]
        return sample_data

    async def scrape_clasificados_locales(self) -> List[ScrapedVehicle]:
        """Scrape sitios de clasificados locales dominicanos"""
        logger.info("Scraping clasificados locales...")
        vehicles = []
        
        # Sitios de clasificados populares en RD
        sites = [
            "https://clasificados.com.do/vehiculos",
            "https://miclasificado.com.do/vehiculos"
            # Agregar más según disponibilidad
        ]
        
        for site in sites:
            try:
                site_vehicles = await self._scrape_generic_classifieds(site)
                vehicles.extend(site_vehicles)
                await asyncio.sleep(3)  # Rate limiting
            except Exception as e:
                logger.warning(f"Error scraping {site}: {e}")
                continue
        
        logger.info(f"Clasificados locales: {len(vehicles)} vehículos")
        return vehicles

    async def _scrape_generic_classifieds(self, site_url: str) -> List[ScrapedVehicle]:
        """Scraper genérico para sitios de clasificados"""
        vehicles = []
        
        try:
            async with self.session.get(site_url) as response:
                if response.status == 200:
                    html = await response.text()
                    soup = BeautifulSoup(html, 'html.parser')
                    
                    # Buscar patrones comunes en sitios de clasificados
                    vehicle_elements = soup.find_all(['div', 'article'], 
                                                   class_=re.compile(r'(vehicle|car|auto|listing)', re.I))
                    
                    for element in vehicle_elements[:20]:  # Límite por sitio
                        vehicle_data = self._parse_generic_vehicle(element, site_url)
                        if vehicle_data:
                            vehicles.append(vehicle_data)
            
            return vehicles
            
        except Exception as e:
            logger.error(f"Error scraping {site_url}: {e}")
            return []

    def _parse_generic_vehicle(self, element, source_url: str) -> Optional[ScrapedVehicle]:
        """Parse genérico para elementos de vehículo"""
        try:
            text_content = element.get_text()
            
            # Buscar patrones en el texto
            year_match = re.search(r'\b(19|20)\d{2}\b', text_content)
            price_match = re.search(r'[\$RD]*\s*(\d{1,3}(?:,\d{3})*(?:\.\d{2})?)', text_content)
            
            if not year_match or not price_match:
                return None
            
            year = int(year_match.group())
            price = float(price_match.group(1).replace(',', ''))
            
            # Heurística para marca y modelo
            words = text_content.split()
            make = "Unknown"
            model = "Unknown"
            
            # Buscar marcas conocidas
            known_makes = ['Toyota', 'Honda', 'Nissan', 'BMW', 'Mercedes', 'Hyundai', 'Kia']
            for word in words:
                if word in known_makes:
                    make = word
                    break
            
            return ScrapedVehicle(
                source=f"clasificados_{source_url.split('/')[2]}",
                make=make,
                model=model,
                year=year,
                price=price,
                currency="DOP",  # Asumir DOP para sitios locales
                location="República Dominicana"
            )
            
        except Exception as e:
            logger.warning(f"Error parsing generic vehicle: {e}")
            return None

    async def save_to_database(self, vehicles: List[ScrapedVehicle]):
        """Guardar datos scrapeados en base de datos"""
        if not vehicles:
            return
        
        try:
            # Convertir a DataFrame
            df = pd.DataFrame([asdict(vehicle) for vehicle in vehicles])
            
            # Limpiar datos
            df = df.dropna(subset=['make', 'model', 'year', 'price'])
            df = df[df['price'] > 0]
            df = df[df['year'] >= 2000]
            
            # Guardar en PostgreSQL
            table_name = 'scraped_vehicles'
            df.to_sql(table_name, self.db_engine, if_exists='append', index=False)
            
            # Actualizar cache de Redis
            cache_key = f"last_scrape_{datetime.now().strftime('%Y%m%d')}"
            self.redis_client.set(cache_key, len(vehicles), ex=86400)  # 24h TTL
            
            logger.info(f"Guardados {len(df)} vehículos en base de datos")
            
        except Exception as e:
            logger.error(f"Error guardando en base de datos: {e}")

    async def get_market_summary(self) -> Dict[str, Any]:
        """Obtener resumen del mercado desde datos scrapeados"""
        try:
            query = """
            SELECT 
                make,
                model,
                AVG(price) as avg_price,
                COUNT(*) as listings_count,
                MIN(price) as min_price,
                MAX(price) as max_price
            FROM scraped_vehicles 
            WHERE scraped_at >= NOW() - INTERVAL '7 days'
            GROUP BY make, model
            ORDER BY listings_count DESC
            LIMIT 50
            """
            
            df = pd.read_sql(query, self.db_engine)
            
            summary = {
                'total_listings': len(df),
                'top_makes': df.groupby('make')['listings_count'].sum().to_dict(),
                'price_ranges': {
                    'budget': df[df['avg_price'] < 800000]['make'].unique().tolist(),
                    'mid_range': df[(df['avg_price'] >= 800000) & (df['avg_price'] < 2000000)]['make'].unique().tolist(),
                    'luxury': df[df['avg_price'] >= 2000000]['make'].unique().tolist()
                },
                'last_updated': datetime.now().isoformat()
            }
            
            return summary
            
        except Exception as e:
            logger.error(f"Error generating market summary: {e}")
            return {}

    async def cleanup(self):
        """Cleanup de recursos"""
        try:
            await self.session.close()
            if self.driver:
                self.driver.quit()
        except Exception as e:
            logger.warning(f"Error en cleanup: {e}")

async def main():
    """Función principal del scraper"""
    scraper = MarketIntelligenceScraper()
    
    try:
        # Ejecutar scraping completo
        vehicles = await scraper.scrape_all_sources()
        
        # Generar resumen
        summary = await scraper.get_market_summary()
        
        logger.info(f"Scraping completado: {len(vehicles)} vehículos")
        logger.info(f"Resumen del mercado: {summary}")
        
    except Exception as e:
        logger.error(f"Error en scraping principal: {e}")
    finally:
        await scraper.cleanup()

if __name__ == "__main__":
    # Ejecutar scraper
    asyncio.run(main())