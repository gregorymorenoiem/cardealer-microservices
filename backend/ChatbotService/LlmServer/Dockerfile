FROM python:3.11-slim

LABEL maintainer="OKLA Team"
LABEL description="OKLA Chatbot LLM Inference Server"

WORKDIR /app

# Install build dependencies for llama-cpp-python (CPU only)
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    cmake \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Install Python dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy server code
COPY server.py .

# Create model directory
RUN mkdir -p /models

# Environment variables
ENV MODEL_PATH=/models/okla-llama3-8b-q4_k_m.gguf
ENV HOST=0.0.0.0
ENV PORT=8000
ENV N_CTX=4096
ENV N_GPU_LAYERS=0
ENV N_THREADS=4
ENV MAX_TOKENS=600

EXPOSE 8000

HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
    CMD curl -f http://localhost:8000/health || exit 1

CMD ["python", "server.py"]
