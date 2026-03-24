FROM python:3.11-slim

WORKDIR /app

# Instalar dependencias del sistema
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Instalar Streamlit y dependencias básicas
RUN pip install --no-cache-dir \
    streamlit==1.28.1 \
    pandas==2.0.3 \
    numpy==1.24.3 \
    plotly==5.17.0 \
    requests==2.31.0 \
    psycopg2-binary==2.9.7 \
    sqlalchemy==2.0.21

# Copiar dashboard
COPY pricing_dashboard.py .

# Crear directorio de datos
RUN mkdir -p /app/data

# Puerto
EXPOSE 8501

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8501/_stcore/health || exit 1

# Comando de inicio
CMD ["streamlit", "run", "pricing_dashboard.py", "--server.port=8501", "--server.address=0.0.0.0"]