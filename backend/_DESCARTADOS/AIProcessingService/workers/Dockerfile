# ============================================================
# Dockerfile - Auto-Learning Background Removal System
# ============================================================
# Sistema de aprendizaje automático para remoción de fondo
# con YOLO + SAM + Evaluación computacional/Ollama
# ============================================================

FROM python:3.11-slim

# Metadatos
LABEL maintainer="OKLA Team"
LABEL description="Auto-Learning Background Removal System"
LABEL version="1.0"

# Variables de entorno
ENV PYTHONUNBUFFERED=1
ENV PYTHONDONTWRITEBYTECODE=1
ENV PIP_NO_CACHE_DIR=1
ENV PIP_DISABLE_PIP_VERSION_CHECK=1

# Directorio de trabajo
WORKDIR /app

# Instalar dependencias del sistema
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    libgl1 \
    libglib2.0-0 \
    libsm6 \
    libxext6 \
    libxrender1 \
    libgomp1 \
    wget \
    curl \
    git \
    && rm -rf /var/lib/apt/lists/*

# Copiar requirements primero (para cache de Docker)
COPY requirements.docker.txt .

# Instalar dependencias Python
RUN pip install --upgrade pip && \
    pip install -r requirements.docker.txt

# Copiar código fuente
COPY auto_learning_system.py .
COPY remove_background_v7.py .

# Crear directorios necesarios
RUN mkdir -p /app/input \
             /app/output \
             /app/checkpoints \
             /app/models \
             /app/data

# Variables de entorno para el sistema
ENV INPUT_DIR=/app/input
ENV OUTPUT_DIR=/app/output
ENV CHECKPOINT_DIR=/app/checkpoints
ENV MODELS_DIR=/app/models
ENV DB_PATH=/app/data/autolearn.db

# Puerto para API (futuro)
EXPOSE 8000

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD python -c "import torch; print('OK')" || exit 1

# Comando por defecto
ENTRYPOINT ["python", "auto_learning_system.py"]
CMD ["--mode", "batch", "--input", "/app/input", "--output", "/app/output"]
