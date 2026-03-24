# Dockerfile para ML Pricing Service
FROM python:3.11-slim

WORKDIR /app

# Instalar dependencias del sistema
RUN apt-get update && apt-get install -y \
    gcc \
    g++ \
    curl \
    wget \
    && rm -rf /var/lib/apt/lists/*

# Copiar requirements
COPY requirements.txt .

# Instalar dependencias Python
RUN pip install --no-cache-dir -r requirements.txt

# Copiar código fuente
COPY . .

# Crear directorios necesarios
RUN mkdir -p /app/models /app/data /app/logs

# Variables de entorno
ENV PYTHONPATH=/app
ENV PYTHONUNBUFFERED=1

# Puerto
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/api/pricing/health || exit 1

# Comando de inicio
CMD ["python", "-m", "uvicorn", "advanced_pricing_ml:app", "--host", "0.0.0.0", "--port", "8080", "--workers", "2"]