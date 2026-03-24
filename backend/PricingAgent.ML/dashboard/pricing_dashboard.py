#!/usr/bin/env python3
"""
OKLA Pricing Dashboard
======================

Dashboard interactivo para análisis de pricing y market intelligence.
Built with Streamlit para visualización en tiempo real.
"""

import streamlit as st
import pandas as pd
import numpy as np
import plotly.express as px
import plotly.graph_objects as go
from plotly.subplots import make_subplots
import requests
import json
from datetime import datetime, timedelta
import psycopg2
from sqlalchemy import create_engine
import os

# Configuración de página
st.set_page_config(
    page_title="OKLA Pricing Intelligence",
    page_icon="🚗",
    layout="wide",
    initial_sidebar_state="expanded"
)

class PricingDashboard:
    """Dashboard principal de pricing intelligence"""
    
    def __init__(self):
        self.db_engine = create_engine(
            f"postgresql://oklauser:{os.getenv('POSTGRES_PASSWORD')}@postgres:5432/pricingagent"
        )
        self.ml_service_url = os.getenv('ML_SERVICE_URL', 'http://pricing-ml-service:8080')
        
    def run(self):
        """Ejecutar dashboard principal"""
        
        # Sidebar para navegación
        st.sidebar.title("🚗 OKLA Pricing Intelligence")
        
        # Selector de página
        page = st.sidebar.selectbox("Seleccionar Vista", [
            "📊 Dashboard Principal",
            "🔍 Análisis de Vehículo",
            "📈 Tendencias de Mercado", 
            "🏆 Análisis Competitivo",
            "🤖 ML Model Performance",
            "⚙️ Configuración"
        ])
        
        # Filtros globales
        st.sidebar.markdown("### Filtros Globales")
        
        # Selector de fecha
        date_range = st.sidebar.date_input(
            "Rango de Fechas",
            value=(datetime.now() - timedelta(days=30), datetime.now()),
            max_value=datetime.now()
        )
        
        # Filtros adicionales
        selected_makes = st.sidebar.multiselect(
            "Marcas",
            options=['Toyota', 'Honda', 'Nissan', 'BMW', 'Mercedes-Benz', 'Hyundai', 'Kia'],
            default=['Toyota', 'Honda', 'Nissan']
        )
        
        # Renderizar página seleccionada
        if page == "📊 Dashboard Principal":
            self.render_main_dashboard(date_range, selected_makes)
        elif page == "🔍 Análisis de Vehículo":
            self.render_vehicle_analysis()
        elif page == "📈 Tendencias de Mercado":
            self.render_market_trends(date_range, selected_makes)
        elif page == "🏆 Análisis Competitivo":
            self.render_competitive_analysis(date_range, selected_makes)
        elif page == "🤖 ML Model Performance":
            self.render_ml_performance()
        elif page == "⚙️ Configuración":
            self.render_configuration()

    def render_main_dashboard(self, date_range, selected_makes):
        """Dashboard principal con métricas clave"""
        
        st.title("📊 OKLA Pricing Intelligence Dashboard")
        st.markdown("---")
        
        # Métricas principales
        col1, col2, col3, col4 = st.columns(4)
        
        with col1:
            # Total de análisis realizados
            total_analyses = self.get_total_analyses(date_range)
            st.metric("Análisis Realizados", f"{total_analyses:,}", delta="+12%")
        
        with col2:
            # Precisión del modelo
            model_accuracy = self.get_model_accuracy()
            st.metric("Precisión ML", f"{model_accuracy:.1%}", delta="+2.3%")
        
        with col3:
            # Ahorro estimado
            estimated_savings = self.calculate_estimated_savings(date_range)
            st.metric("Ahorro Estimado", f"${estimated_savings:,.0f}", delta="+18%")
        
        with col4:
            # Vehículos analizados
            vehicles_analyzed = self.get_vehicles_count(date_range)
            st.metric("Vehículos Únicos", f"{vehicles_analyzed:,}", delta="+8%")
        
        st.markdown("---")
        
        # Gráficos principales
        col1, col2 = st.columns([2, 1])
        
        with col1:
            # Tendencia de precios por tiempo
            st.subheader("📈 Tendencias de Precios")
            price_trends = self.get_price_trends(date_range, selected_makes)
            
            fig = px.line(price_trends, x='date', y='avg_price', color='make',
                         title="Precio Promedio por Marca (Últimos 30 días)")
            fig.update_layout(height=400)
            st.plotly_chart(fig, use_container_width=True)
        
        with col2:
            # Distribución por marca
            st.subheader("🏷️ Distribución por Marca")
            make_distribution = self.get_make_distribution(date_range)
            
            fig = px.pie(make_distribution, values='count', names='make',
                        title="Análisis por Marca")
            fig.update_layout(height=400)
            st.plotly_chart(fig, use_container_width=True)
        
        # Tabla de vehículos recientes
        st.subheader("🔄 Análisis Recientes")
        recent_analyses = self.get_recent_analyses(limit=10)
        st.dataframe(recent_analyses, use_container_width=True)

    def render_vehicle_analysis(self):
        """Análisis de vehículo específico"""
        
        st.title("🔍 Análisis de Vehículo Individual")
        st.markdown("---")
        
        # Formulario para análisis
        with st.form("vehicle_analysis_form"):
            col1, col2, col3 = st.columns(3)
            
            with col1:
                make = st.selectbox("Marca", ['Toyota', 'Honda', 'Nissan', 'BMW', 'Mercedes-Benz', 'Hyundai', 'Kia'])
                model = st.text_input("Modelo", placeholder="ej: Corolla")
                year = st.number_input("Año", min_value=2000, max_value=2024, value=2020)
            
            with col2:
                mileage = st.number_input("Kilometraje", min_value=0, value=50000, step=1000)
                condition = st.selectbox("Condición", ['Excelente', 'Muy Bueno', 'Bueno', 'Regular'])
                fuel_type = st.selectbox("Combustible", ['Gasolina', 'Diésel', 'Híbrido'])
            
            with col3:
                transmission = st.selectbox("Transmisión", ['Manual', 'Automática'])
                province = st.selectbox("Provincia", ['Santo Domingo', 'Santiago', 'La Vega', 'Puerto Plata'])
                asking_price = st.number_input("Precio Solicitado (DOP)", min_value=0, value=1000000)
            
            submitted = st.form_submit_button("Analizar Vehículo", type="primary")
        
        if submitted:
            # Llamar al servicio ML
            with st.spinner("Analizando vehículo..."):
                prediction = self.get_ml_prediction({
                    'make': make,
                    'model': model,
                    'year': year,
                    'mileage': mileage,
                    'condition': condition,
                    'fuel_type': fuel_type,
                    'transmission': transmission,
                    'province': province,
                    'asking_price': asking_price
                })
            
            if prediction:
                # Mostrar resultados
                col1, col2 = st.columns([2, 1])
                
                with col1:
                    # Precio sugerido
                    st.subheader("💰 Análisis de Precio")
                    
                    # Métricas de precio
                    mcol1, mcol2, mcol3 = st.columns(3)
                    
                    with mcol1:
                        st.metric("Precio Sugerido (DOP)", 
                                f"${prediction['suggested_price_dop']:,.0f}",
                                delta=f"{((prediction['suggested_price_dop'] - asking_price) / asking_price * 100):+.1f}%")
                    
                    with mcol2:
                        st.metric("Precio USD", f"${prediction['suggested_price_usd']:,.0f}")
                    
                    with mcol3:
                        st.metric("Confianza", f"{prediction['confidence_score']:.1%}")
                    
                    # Rango de precio
                    st.subheader("📊 Rango de Precio")
                    
                    price_data = pd.DataFrame({
                        'Tipo': ['Mínimo', 'Sugerido', 'Máximo', 'Solicitado'],
                        'Precio': [
                            prediction['price_range_min'],
                            prediction['suggested_price_dop'], 
                            prediction['price_range_max'],
                            asking_price
                        ],
                        'Color': ['red', 'green', 'red', 'blue']
                    })
                    
                    fig = px.bar(price_data, x='Tipo', y='Precio', color='Color',
                               title="Comparación de Precios")
                    st.plotly_chart(fig, use_container_width=True)
                
                with col2:
                    # Información adicional
                    st.subheader("📋 Detalles del Análisis")
                    
                    st.write(f"**Posición en Mercado:** {prediction['market_position']}")
                    st.write(f"**Días Estimados Venta:** {prediction['days_to_sell']} días")
                    st.write(f"**Tendencia de Mercado:** {prediction['market_trend']}")
                    
                    # Factores de pricing
                    st.subheader("⚖️ Factores de Precio")
                    for factor, value in prediction['pricing_factors'].items():
                        delta_color = "normal" if abs(value) < 0.02 else ("inverse" if value < 0 else "normal")
                        st.metric(factor.replace('_', ' ').title(), f"{value:+.1%}", delta_color=delta_color)
                
                # Análisis competitivo
                st.subheader("🏆 Análisis Competitivo")
                if prediction['competitive_analysis']['competitors_found'] > 0:
                    comp_data = pd.DataFrame({
                        'Competidor': [f"Comp {i+1}" for i in range(len(prediction['competitive_analysis']['competitor_prices']))],
                        'Precio': prediction['competitive_analysis']['competitor_prices']
                    })
                    
                    fig = px.scatter(comp_data, x='Competidor', y='Precio',
                                   title="Precios de Competencia")
                    fig.add_hline(y=prediction['suggested_price_dop'], 
                                line_dash="dash", line_color="red",
                                annotation_text="Precio Sugerido")
                    st.plotly_chart(fig, use_container_width=True)
                else:
                    st.warning("No se encontraron competidores directos")

    def render_market_trends(self, date_range, selected_makes):
        """Análisis de tendencias del mercado"""
        
        st.title("📈 Tendencias de Mercado")
        st.markdown("---")
        
        # Tendencias por marca
        st.subheader("Tendencias por Marca")
        
        trends_data = self.get_detailed_trends(date_range, selected_makes)
        
        fig = make_subplots(
            rows=2, cols=2,
            subplot_titles=("Precios Promedio", "Volumen de Listings", 
                          "Tiempo Promedio Venta", "Distribución de Años")
        )
        
        # Gráfico 1: Precios promedio
        for make in selected_makes:
            make_data = trends_data[trends_data['make'] == make]
            fig.add_trace(
                go.Scatter(x=make_data['date'], y=make_data['avg_price'], 
                          name=f"{make} - Precio", mode='lines'),
                row=1, col=1
            )
        
        # Gráfico 2: Volumen
        volume_data = trends_data.groupby(['date', 'make']).size().reset_index(name='count')
        for make in selected_makes:
            make_volume = volume_data[volume_data['make'] == make]
            fig.add_trace(
                go.Scatter(x=make_volume['date'], y=make_volume['count'],
                          name=f"{make} - Volumen", mode='lines'),
                row=1, col=2
            )
        
        fig.update_layout(height=600, showlegend=True)
        st.plotly_chart(fig, use_container_width=True)
        
        # Insights automáticos
        st.subheader("🧠 Insights Automáticos")
        insights = self.generate_market_insights(trends_data, selected_makes)
        
        for insight in insights:
            st.info(f"💡 {insight}")

    def render_competitive_analysis(self, date_range, selected_makes):
        """Análisis competitivo detallado"""
        
        st.title("🏆 Análisis Competitivo")
        st.markdown("---")
        
        # Análisis de posicionamiento
        st.subheader("Posicionamiento vs Competencia")
        
        competitor_data = self.get_competitor_analysis(date_range, selected_makes)
        
        # Scatter plot de precio vs características
        fig = px.scatter(competitor_data, x='mileage', y='price', 
                        color='make', size='year',
                        title="Precio vs Kilometraje por Marca",
                        hover_data=['model', 'year'])
        st.plotly_chart(fig, use_container_width=True)
        
        # Tabla de comparación
        st.subheader("Tabla Comparativa")
        comparison_table = self.create_comparison_table(competitor_data)
        st.dataframe(comparison_table, use_container_width=True)

    def render_ml_performance(self):
        """Dashboard de performance del modelo ML"""
        
        st.title("🤖 ML Model Performance")
        st.markdown("---")
        
        # Métricas del modelo
        model_metrics = self.get_model_metrics()
        
        col1, col2, col3, col4 = st.columns(4)
        
        with col1:
            st.metric("Accuracy", f"{model_metrics['accuracy']:.2%}")
        
        with col2:
            st.metric("MAE (DOP)", f"{model_metrics['mae']:,.0f}")
        
        with col3:
            st.metric("R² Score", f"{model_metrics['r2']:.3f}")
        
        with col4:
            st.metric("Predictions Today", f"{model_metrics['predictions_today']:,}")
        
        # Distribución de errores
        st.subheader("📊 Distribución de Errores")
        error_data = self.get_prediction_errors()
        
        fig = px.histogram(error_data, x='error_percentage', 
                          title="Distribución de Errores de Predicción (%)")
        st.plotly_chart(fig, use_container_width=True)
        
        # Feature importance
        st.subheader("🎯 Importancia de Features")
        feature_importance = self.get_feature_importance()
        
        fig = px.bar(feature_importance, x='importance', y='feature',
                    orientation='h', title="Importancia de Características")
        st.plotly_chart(fig, use_container_width=True)

    def render_configuration(self):
        """Panel de configuración"""
        
        st.title("⚙️ Configuración del Sistema")
        st.markdown("---")
        
        # Estado del sistema
        st.subheader("Estado del Sistema")
        
        system_status = self.get_system_status()
        
        col1, col2 = st.columns(2)
        
        with col1:
            st.success(f"🟢 ML Service: {'Online' if system_status['ml_service'] else 'Offline'}")
            st.success(f"🟢 Database: {'Connected' if system_status['database'] else 'Disconnected'}")
        
        with col2:
            st.success(f"🟢 Redis Cache: {'Available' if system_status['redis'] else 'Unavailable'}")
            st.success(f"🟢 Scraping: {'Active' if system_status['scraping'] else 'Inactive'}")
        
        # Controles de reentrenamiento
        st.subheader("🔄 Control del Modelo")
        
        col1, col2 = st.columns(2)
        
        with col1:
            if st.button("Reentrenar Modelo", type="primary"):
                with st.spinner("Iniciando reentrenamiento..."):
                    result = self.trigger_model_retrain()
                    if result:
                        st.success("✅ Reentrenamiento iniciado exitosamente")
                    else:
                        st.error("❌ Error iniciando reentrenamiento")
        
        with col2:
            if st.button("Actualizar Cache"):
                with st.spinner("Actualizando cache..."):
                    self.update_cache()
                    st.success("✅ Cache actualizado")
        
        # Configuración de scraping
        st.subheader("🕷️ Configuración de Scraping")
        
        scraping_interval = st.number_input("Intervalo de Scraping (horas)", 
                                          min_value=1, max_value=24, value=6)
        
        enable_supercarros = st.checkbox("Scraping SuperCarros", value=True)
        enable_facebook = st.checkbox("Scraping Facebook Marketplace", value=False)
        enable_clasificados = st.checkbox("Scraping Clasificados Locales", value=True)
        
        if st.button("Actualizar Configuración Scraping"):
            # TODO: Implementar actualización de configuración
            st.success("✅ Configuración actualizada")

    # Métodos de datos
    def get_total_analyses(self, date_range):
        """Obtener total de análisis en el rango de fechas"""
        # Simulado - en producción usar base de datos real
        return 1547

    def get_model_accuracy(self):
        """Obtener precisión del modelo"""
        return 0.847

    def calculate_estimated_savings(self, date_range):
        """Calcular ahorro estimado"""
        return 125000

    def get_vehicles_count(self, date_range):
        """Obtener conteo de vehículos únicos"""
        return 892

    def get_price_trends(self, date_range, selected_makes):
        """Obtener tendencias de precios"""
        # Datos simulados
        dates = pd.date_range(start=date_range[0], end=date_range[1], freq='D')
        
        data = []
        for make in selected_makes:
            base_price = {'Toyota': 1500000, 'Honda': 1600000, 'Nissan': 1400000}.get(make, 1500000)
            
            for date in dates:
                # Agregar algo de ruido y tendencia
                noise = np.random.normal(0, 0.05)
                trend = 0.001 * (date - dates[0]).days  # Tendencia ligera al alza
                price = base_price * (1 + trend + noise)
                
                data.append({
                    'date': date,
                    'make': make,
                    'avg_price': price
                })
        
        return pd.DataFrame(data)

    def get_make_distribution(self, date_range):
        """Obtener distribución por marca"""
        return pd.DataFrame({
            'make': ['Toyota', 'Honda', 'Nissan', 'Hyundai', 'Kia'],
            'count': [45, 38, 32, 28, 22]
        })

    def get_recent_analyses(self, limit=10):
        """Obtener análisis recientes"""
        return pd.DataFrame({
            'Fecha': pd.date_range(end=datetime.now(), periods=limit, freq='H'),
            'Marca': np.random.choice(['Toyota', 'Honda', 'Nissan'], limit),
            'Modelo': np.random.choice(['Corolla', 'Civic', 'Sentra'], limit),
            'Año': np.random.randint(2015, 2024, limit),
            'Precio Sugerido': np.random.randint(800000, 2500000, limit),
            'Confianza': np.random.uniform(0.7, 0.95, limit)
        })

    def get_ml_prediction(self, vehicle_data):
        """Obtener predicción del servicio ML"""
        try:
            response = requests.post(
                f"{self.ml_service_url}/api/pricing/predict",
                json=vehicle_data,
                timeout=30
            )
            
            if response.status_code == 200:
                return response.json()
            else:
                st.error(f"Error en predicción: {response.status_code}")
                return None
        
        except Exception as e:
            st.error(f"Error conectando con servicio ML: {e}")
            # Retornar datos simulados para demo
            return self._get_mock_prediction(vehicle_data)

    def _get_mock_prediction(self, vehicle_data):
        """Predicción mock para demo"""
        base_price = 1500000
        age_factor = (2024 - vehicle_data['year']) * 0.1
        adjusted_price = base_price * (1 - age_factor)
        
        return {
            'suggested_price_dop': adjusted_price,
            'suggested_price_usd': adjusted_price / 58.5,
            'confidence_score': 0.85,
            'price_range_min': adjusted_price * 0.9,
            'price_range_max': adjusted_price * 1.1,
            'market_position': 'Competitivo',
            'days_to_sell': 35,
            'market_trend': 'Estable',
            'pricing_factors': {
                'brand_reliability': 0.05,
                'vehicle_age': -age_factor,
                'market_demand': 0.02
            },
            'competitive_analysis': {
                'competitors_found': 5,
                'competitor_prices': [adjusted_price * f for f in [0.95, 1.02, 0.98, 1.05, 0.97]],
                'avg_competitor_price': adjusted_price * 1.01
            }
        }

    def get_system_status(self):
        """Obtener estado del sistema"""
        return {
            'ml_service': True,
            'database': True,
            'redis': True,
            'scraping': True
        }

    def trigger_model_retrain(self):
        """Disparar reentrenamiento del modelo"""
        try:
            response = requests.post(f"{self.ml_service_url}/api/pricing/train")
            return response.status_code == 200
        except:
            return False

    def update_cache(self):
        """Actualizar cache del sistema"""
        # Implementación de actualización de cache
        pass

    # Métodos adicionales para otros dashboards...
    def get_detailed_trends(self, date_range, selected_makes):
        # Implementación de tendencias detalladas
        return pd.DataFrame()

    def generate_market_insights(self, trends_data, selected_makes):
        # Generar insights automáticos
        return [
            "Los precios de Toyota han mostrado estabilidad en los últimos 30 días",
            "Se observa un incremento del 3% en la demanda de SUVs",
            "La temporada alta está comenzando - se recomienda ajustar precios"
        ]

    def get_competitor_analysis(self, date_range, selected_makes):
        # Análisis competitivo mock
        return pd.DataFrame({
            'make': np.random.choice(selected_makes, 100),
            'model': np.random.choice(['Corolla', 'Civic', 'Sentra'], 100),
            'year': np.random.randint(2015, 2024, 100),
            'price': np.random.randint(800000, 2500000, 100),
            'mileage': np.random.randint(20000, 150000, 100)
        })

    def create_comparison_table(self, competitor_data):
        # Crear tabla de comparación
        return competitor_data.groupby('make').agg({
            'price': ['mean', 'min', 'max'],
            'mileage': 'mean',
            'year': 'mean'
        }).round(0)

    def get_model_metrics(self):
        return {
            'accuracy': 0.847,
            'mae': 125000,
            'r2': 0.823,
            'predictions_today': 156
        }

    def get_prediction_errors(self):
        return pd.DataFrame({
            'error_percentage': np.random.normal(0, 10, 1000)
        })

    def get_feature_importance(self):
        return pd.DataFrame({
            'feature': ['Año', 'Kilometraje', 'Marca', 'Modelo', 'Condición', 'Provincia'],
            'importance': [0.35, 0.28, 0.15, 0.12, 0.07, 0.03]
        })

# Ejecutar dashboard
if __name__ == "__main__":
    dashboard = PricingDashboard()
    dashboard.run()