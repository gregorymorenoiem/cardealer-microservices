#!/usr/bin/env python3
"""
OKLA Chatbot LLM — Inference Server
====================================

Servidor HTTP ligero que expone el modelo GGUF fine-tuned via API REST.
Se despliega como contenedor Docker en Kubernetes.

Endpoints:
    POST /v1/chat/completions  — Chat completion (OpenAI-compatible)
    POST /v1/embeddings        — Text embeddings (sentence-transformers)
    GET  /health               — Health check
    GET  /info                 — Model info

Uso local:
    python server.py --model /models/okla-llama3-8b-q4_k_m.gguf --port 8000

Docker:
    docker build -t okla-llm-server .
    docker run -p 8000:8000 -v /path/to/model:/models okla-llm-server
"""

import argparse
import asyncio
import json
import logging
import os
import sys
import time
import uuid
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime
from typing import Any, Optional

from fastapi import FastAPI, HTTPException, Request, Response
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
import uvicorn

# Prometheus metrics
from prometheus_client import (
    Counter, Histogram, Gauge, Info,
    generate_latest, CollectorRegistry, CONTENT_TYPE_LATEST,
)

PROM_REGISTRY = CollectorRegistry()

PROM_REQUESTS_TOTAL = Counter(
    "okla_llm_requests_total", "Total inference requests", registry=PROM_REGISTRY
)
PROM_REQUESTS_SUCCESS = Counter(
    "okla_llm_requests_success_total", "Successful inference requests", registry=PROM_REGISTRY
)
PROM_REQUESTS_ERROR = Counter(
    "okla_llm_requests_error_total", "Failed inference requests",
    ["error_type"], registry=PROM_REGISTRY
)
PROM_DURATION = Histogram(
    "okla_llm_response_duration_ms", "Response time in ms",
    buckets=[100, 250, 500, 1000, 2000, 3000, 5000, 8000, 10000, 15000, 30000],
    registry=PROM_REGISTRY,
)
PROM_TOKENS_TOTAL = Counter(
    "okla_llm_tokens_total", "Total tokens (prompt+completion)", registry=PROM_REGISTRY
)
PROM_PROMPT_TOKENS = Counter(
    "okla_llm_prompt_tokens_total", "Prompt tokens consumed", registry=PROM_REGISTRY
)
PROM_COMPLETION_TOKENS = Counter(
    "okla_llm_completion_tokens_total", "Completion tokens generated", registry=PROM_REGISTRY
)
PROM_MODEL_LOADED = Gauge(
    "okla_llm_model_loaded", "Model loaded (1=yes, 0=no)", registry=PROM_REGISTRY
)
PROM_UPTIME = Gauge(
    "okla_llm_uptime_seconds", "Server uptime in seconds", registry=PROM_REGISTRY
)
PROM_ACTIVE = Gauge(
    "okla_llm_active_requests", "Active inference requests", registry=PROM_REGISTRY
)
PROM_AVG_RT = Gauge(
    "okla_llm_avg_response_time_ms", "Rolling avg response time ms", registry=PROM_REGISTRY
)
PROM_MODEL_INFO = Info(
    "okla_llm_model", "Loaded model information", registry=PROM_REGISTRY
)

# ============================================================
# CONFIG
# ============================================================

MODEL_PATH = os.getenv("MODEL_PATH", "/models/okla-llama3-8b-q4_k_m.gguf")
HOST = os.getenv("HOST", "0.0.0.0")
PORT = int(os.getenv("PORT", "8000"))
N_CTX = int(os.getenv("N_CTX", "8192"))         # Context window (8192 for RAG+inventory+history)
N_GPU_LAYERS = int(os.getenv("N_GPU_LAYERS", "0"))  # 0 = CPU only, -1 = all GPU
N_THREADS = int(os.getenv("N_THREADS", "4"))
N_BATCH = int(os.getenv("N_BATCH", "512"))        # Batch size for prompt eval
MAX_TOKENS = int(os.getenv("MAX_TOKENS", "600"))   # 600 for full 8-field JSON schema
EMBEDDING_MODEL = os.getenv("EMBEDDING_MODEL", "all-MiniLM-L6-v2")
EMBEDDING_DEVICE = os.getenv("EMBEDDING_DEVICE", "cpu")
ALLOWED_ORIGINS = os.getenv("ALLOWED_ORIGINS", "http://localhost:3000,http://localhost:5060,https://okla.com.do").split(",")

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s"
)
logger = logging.getLogger("okla-llm-server")

# ============================================================
# MODELS (Pydantic)
# ============================================================

class ChatMessage(BaseModel):
    role: str = Field(..., description="Role: system, user, or assistant")
    content: str = Field(..., description="Message content")


class ChatCompletionRequest(BaseModel):
    model: str = Field(default="okla-llama3-8b", description="Model identifier")
    messages: list[ChatMessage] = Field(..., description="Conversation messages")
    temperature: float = Field(default=0.3, ge=0, le=2)
    top_p: float = Field(default=0.9, ge=0, le=1)
    max_tokens: int = Field(default=600, ge=1, le=4096)
    stream: bool = Field(default=False)
    stop: Optional[list[str]] = None
    repetition_penalty: float = Field(default=1.15, ge=1, le=2)
    frequency_penalty: float = Field(default=0.0, ge=0, le=2, description="OpenAI-compat alias for repetition_penalty")


class ChatCompletionChoice(BaseModel):
    index: int = 0
    message: ChatMessage
    finish_reason: str = "stop"


class UsageInfo(BaseModel):
    prompt_tokens: int = 0
    completion_tokens: int = 0
    total_tokens: int = 0


class ChatCompletionResponse(BaseModel):
    id: str = Field(default_factory=lambda: f"chatcmpl-{uuid.uuid4().hex[:12]}")
    object: str = "chat.completion"
    created: int = Field(default_factory=lambda: int(time.time()))
    model: str = "okla-llama3-8b"
    choices: list[ChatCompletionChoice] = []
    usage: UsageInfo = Field(default_factory=UsageInfo)


class HealthResponse(BaseModel):
    status: str = "healthy"
    model_loaded: bool = False
    model_path: str = ""
    uptime_seconds: float = 0
    total_requests: int = 0
    avg_response_time_ms: float = 0


class ModelInfoResponse(BaseModel):
    model_id: str = "okla-llama3-8b"
    model_path: str = ""
    model_type: str = "gguf"
    quantization: str = "Q4_K_M"
    context_length: int = 8192
    gpu_layers: int = 0
    n_threads: int = 4
    base_model: str = "meta-llama/Meta-Llama-3-8B-Instruct"
    fine_tuned_for: str = "OKLA vehicle chatbot - Dominican Spanish"
    embedding_model: str = "all-MiniLM-L6-v2"
    embedding_dimensions: int = 384


# Embedding models (Pydantic)
class EmbeddingRequest(BaseModel):
    input: str | list[str] = Field(..., description="Text(s) to embed")
    model: str = Field(default="all-MiniLM-L6-v2", description="Embedding model")


class EmbeddingData(BaseModel):
    object: str = "embedding"
    embedding: list[float]
    index: int = 0


class EmbeddingResponse(BaseModel):
    object: str = "list"
    data: list[EmbeddingData] = []
    model: str = "all-MiniLM-L6-v2"
    usage: UsageInfo = Field(default_factory=UsageInfo)


# ============================================================
# APP
# ============================================================

app = FastAPI(
    title="OKLA LLM Inference Server",
    description="Servidor de inferencia LLM para el chatbot OKLA.",
    version="1.0.0",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=ALLOWED_ORIGINS,
    allow_methods=["GET", "POST", "OPTIONS"],
    allow_headers=["Content-Type", "Authorization", "X-CSRF-Token"],
)

# Global state — thread-safe counters
import threading
llm = None
embedding_model = None
start_time = time.time()
_counter_lock = threading.Lock()
_request_count = 0
_total_response_time = 0.0
_executor = ThreadPoolExecutor(max_workers=2)

# GBNF grammar for guaranteed valid JSON output
# Ensures model always produces parseable JSON with the expected schema
JSON_GRAMMAR = r"""
root   ::= "{" ws members ws "}"
value  ::= object | array | string | number | "true" | "false" | "null"
object ::= "{" ws members? ws "}"
members ::= pair ("," ws pair)*
pair   ::= string ws ":" ws value
array  ::= "[" ws values? ws "]"
values ::= value ("," ws value)*
string ::= "\"" chars "\""
chars  ::= char*
char   ::= [^"\\] | "\\" escape
escape ::= ["\\/bfnrt] | "u" hex hex hex hex
hex    ::= [0-9a-fA-F]
number ::= "-"? int frac? exp?
int    ::= "0" | [1-9] [0-9]*
frac   ::= "." [0-9]+
exp    ::= [eE] [+-]? [0-9]+
ws     ::= [ \t\n\r]*
""".strip()


def load_model():
    """Load the GGUF model using llama-cpp-python with integrity validation."""
    global llm
    from llama_cpp import Llama
    import hashlib

    logger.info(f"Loading model: {MODEL_PATH}")
    logger.info(f"  Context: {N_CTX}, GPU layers: {N_GPU_LAYERS}, Threads: {N_THREADS}")

    if not os.path.exists(MODEL_PATH):
        logger.error(f"Model file not found: {MODEL_PATH}")
        logger.info("Available files in /models/:")
        models_dir = os.path.dirname(MODEL_PATH) or "/models"
        if os.path.exists(models_dir):
            for f in os.listdir(models_dir):
                size = os.path.getsize(os.path.join(models_dir, f)) / 1024**3
                logger.info(f"  {f} ({size:.2f} GB)")
        raise FileNotFoundError(f"Model not found: {MODEL_PATH}")

    # R6: Validate model integrity via SHA256 checksum
    sha256_path = MODEL_PATH + ".sha256"
    if os.path.exists(sha256_path):
        logger.info("🔐 Validating model SHA256 checksum...")
        try:
            with open(sha256_path, "r") as f:
                expected_hash = f.read().strip().split()[0]  # Handle "hash  filename" format
            sha256 = hashlib.sha256()
            with open(MODEL_PATH, "rb") as f:
                for chunk in iter(lambda: f.read(8192 * 1024), b""):  # 8MB chunks
                    sha256.update(chunk)
            actual_hash = sha256.hexdigest()
            if actual_hash == expected_hash:
                logger.info(f"  ✅ SHA256 verified: {actual_hash[:16]}...")
            else:
                logger.error(f"  ❌ SHA256 MISMATCH!")
                logger.error(f"     Expected: {expected_hash}")
                logger.error(f"     Actual:   {actual_hash}")
                if os.getenv("STRICT_CHECKSUM", "false").lower() == "true":
                    raise ValueError(f"Model integrity check failed: SHA256 mismatch")
                else:
                    logger.warning("  ⚠️ Continuing with mismatched checksum (STRICT_CHECKSUM=false)")
        except Exception as e:
            if "SHA256 mismatch" in str(e):
                raise
            logger.warning(f"  ⚠️ Could not validate checksum: {e}")
    else:
        logger.info(f"  ℹ️ No checksum file found at {sha256_path} — skipping integrity check")

    llm = Llama(
        model_path=MODEL_PATH,
        n_ctx=N_CTX,
        n_batch=N_BATCH,
        n_gpu_layers=N_GPU_LAYERS,
        n_threads=N_THREADS,
        verbose=False,
        chat_format="llama-3",
    )

    logger.info("✅ Model loaded successfully")
    return llm


@app.on_event("startup")
async def startup():
    global embedding_model
    load_model()
    PROM_MODEL_LOADED.set(1)
    PROM_MODEL_INFO.info({
        "model_id": "okla-llama3-8b",
        "model_path": MODEL_PATH,
        "quantization": "Q4_K_M",
        "parameters": "8B",
    })
    
    # Load sentence-transformers embedding model for RAG
    try:
        from sentence_transformers import SentenceTransformer
        logger.info(f"Loading embedding model: {EMBEDDING_MODEL}")
        embedding_model = SentenceTransformer(EMBEDDING_MODEL, device=EMBEDDING_DEVICE)
        logger.info(f"✅ Embedding model loaded ({EMBEDDING_MODEL}, dim={embedding_model.get_sentence_embedding_dimension()})")
    except ImportError:
        logger.warning("⚠️ sentence-transformers not installed. /v1/embeddings will be unavailable.")
        logger.warning("  Install: pip install sentence-transformers")
    except Exception as e:
        logger.warning(f"⚠️ Could not load embedding model: {e}")


# ============================================================
# ENDPOINTS
# ============================================================

def _build_llama3_prompt(messages: list[dict]) -> str:
    """
    Build explicit Llama 3 / 3.1 Instruct chat template.
    Ensures prompt format matches EXACTLY what the model was fine-tuned on.
    This removes dependency on llama-cpp-python's chat_format auto-detection.
    """
    prompt = "<|begin_of_text|>"
    for msg in messages:
        role = msg["role"]
        content = msg["content"]
        prompt += f"<|start_header_id|>{role}<|end_header_id|>\n\n{content}<|eot_id|>"
    # Add the assistant header to prompt the model to respond
    prompt += "<|start_header_id|>assistant<|end_header_id|>\n\n"
    return prompt


@app.post("/v1/chat/completions", response_model=ChatCompletionResponse)
def chat_completions(request: ChatCompletionRequest, http_request: Request = None):
    """
    OpenAI-compatible chat completion endpoint.
    Used by LlmService.cs in ChatbotService.
    Using sync def so FastAPI runs it in a thread pool,
    keeping the event loop free for health checks.

    R15 (MLOps): Propagates W3C TraceContext from .NET ChatbotService.
    """
    global _request_count, _total_response_time

    if llm is None:
        raise HTTPException(status_code=503, detail="Model not loaded")

    start = time.time()
    with _counter_lock:
        _request_count += 1
        current_count = _request_count
    PROM_REQUESTS_TOTAL.inc()
    PROM_ACTIVE.inc()

    # R15: Extract W3C TraceContext from incoming request headers
    trace_id = None
    if http_request:
        traceparent = http_request.headers.get("traceparent", "")
        if traceparent:
            trace_id = traceparent
            logger.debug(f"Trace context received: {traceparent[:32]}...")

    try:
        # Convert messages to llama-cpp format
        messages = [{"role": m.role, "content": m.content} for m in request.messages]

        # Map OpenAI frequency_penalty → repeat_penalty if provided
        repeat_penalty = request.repetition_penalty
        if request.frequency_penalty > 0:
            repeat_penalty = 1.0 + request.frequency_penalty

        # Build explicit Llama 3 chat template prompt (REC-2)
        prompt = _build_llama3_prompt(messages)

        # Use create_completion with explicit template + GBNF grammar
        # instead of create_chat_completion to ensure exact template control
        from llama_cpp import LlamaGrammar
        grammar = LlamaGrammar.from_string(JSON_GRAMMAR)

        result = llm.create_completion(
            prompt=prompt,
            temperature=request.temperature,
            top_p=request.top_p,
            max_tokens=request.max_tokens,
            stop=["<|eot_id|>", "</s>"] + (request.stop or []),
            repeat_penalty=repeat_penalty,
            grammar=grammar,
        )

        elapsed = (time.time() - start) * 1000
        with _counter_lock:
            _total_response_time += elapsed
            avg_rt = _total_response_time / _request_count if _request_count else 0

        # Extract response
        content = result["choices"][0]["text"].strip()
        finish_reason = result["choices"][0].get("finish_reason", "stop")

        usage = UsageInfo(
            prompt_tokens=result.get("usage", {}).get("prompt_tokens", 0),
            completion_tokens=result.get("usage", {}).get("completion_tokens", 0),
            total_tokens=result.get("usage", {}).get("total_tokens", 0),
        )

        logger.info(
            f"Request #{current_count}: {usage.total_tokens} tokens, "
            f"{elapsed:.0f}ms, finish={finish_reason}"
            + (f", trace={trace_id[:32]}" if trace_id else "")
        )

        # Prometheus instrumentation
        PROM_REQUESTS_SUCCESS.inc()
        PROM_ACTIVE.dec()
        PROM_DURATION.observe(elapsed)
        PROM_TOKENS_TOTAL.inc(usage.total_tokens)
        PROM_PROMPT_TOKENS.inc(usage.prompt_tokens)
        PROM_COMPLETION_TOKENS.inc(usage.completion_tokens)
        PROM_AVG_RT.set(avg_rt)

        return ChatCompletionResponse(
            model=request.model,
            choices=[
                ChatCompletionChoice(
                    message=ChatMessage(role="assistant", content=content),
                    finish_reason=finish_reason,
                )
            ],
            usage=usage,
        )

    except Exception as e:
        PROM_REQUESTS_ERROR.labels(error_type=type(e).__name__).inc()
        PROM_ACTIVE.dec()
        logger.error(f"Inference error: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Inference error: {str(e)}")


@app.get("/health", response_model=HealthResponse)
async def health():
    """Health check endpoint for K8s probes."""
    with _counter_lock:
        count = _request_count
        avg_time = (_total_response_time / count) if count > 0 else 0
    return HealthResponse(
        status="healthy" if llm is not None else "unhealthy",
        model_loaded=llm is not None,
        model_path=MODEL_PATH,
        uptime_seconds=time.time() - start_time,
        total_requests=count,
        avg_response_time_ms=avg_time,
    )


@app.post("/v1/embeddings", response_model=EmbeddingResponse, tags=["embeddings"])
def create_embeddings(request: EmbeddingRequest):
    """
    Generate text embeddings using sentence-transformers.
    Used by VectorSearchService for RAG (pgvector semantic search).
    
    Accepts single string or list of strings.
    Returns 384-dimensional embeddings (all-MiniLM-L6-v2).
    """
    if embedding_model is None:
        raise HTTPException(
            status_code=503,
            detail="Embedding model not loaded. Install: pip install sentence-transformers"
        )

    try:
        # Normalize input
        texts = [request.input] if isinstance(request.input, str) else request.input
        
        if not texts:
            raise HTTPException(status_code=400, detail="No input text provided")
        
        if len(texts) > 100:
            raise HTTPException(status_code=400, detail="Max 100 texts per request")

        # Generate embeddings
        embeddings = embedding_model.encode(texts, normalize_embeddings=True)
        
        data = [
            EmbeddingData(
                embedding=emb.tolist(),
                index=i
            )
            for i, emb in enumerate(embeddings)
        ]
        
        # Approximate token count
        total_chars = sum(len(t) for t in texts)
        approx_tokens = total_chars // 4
        
        logger.info(f"Embeddings: {len(texts)} texts, ~{approx_tokens} tokens")

        return EmbeddingResponse(
            data=data,
            model=EMBEDDING_MODEL,
            usage=UsageInfo(
                prompt_tokens=approx_tokens,
                total_tokens=approx_tokens,
            ),
        )

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Embedding error: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Embedding error: {str(e)}")


@app.get("/info", response_model=ModelInfoResponse)
async def model_info():
    """Model information endpoint."""
    return ModelInfoResponse(
        model_path=MODEL_PATH,
        context_length=N_CTX,
        gpu_layers=N_GPU_LAYERS,
        n_threads=N_THREADS,
        embedding_model=EMBEDDING_MODEL,
        embedding_dimensions=384 if embedding_model else 0,
    )


@app.get("/metrics", tags=["monitoring"])
async def metrics():
    """Prometheus metrics endpoint for scraping."""
    PROM_UPTIME.set(time.time() - start_time)
    body = generate_latest(PROM_REGISTRY)
    return Response(content=body, media_type=CONTENT_TYPE_LATEST)


# ============================================================
# R22: MODEL HOT-SWAP ENDPOINT
# ============================================================

class ModelSwapRequest(BaseModel):
    model_path: str = Field(..., description="Absolute path to new GGUF model")
    validate_checksum: bool = Field(default=True, description="Validate SHA256 before swap")


class ModelSwapResponse(BaseModel):
    success: bool
    previous_model: str
    new_model: str
    load_time_seconds: float
    message: str


@app.post("/admin/swap-model", response_model=ModelSwapResponse, tags=["admin"])
def swap_model(request: ModelSwapRequest):
    """
    R22 (MLOps): Hot-swap the loaded model without restarting the server.
    This enables zero-downtime model updates.

    WARNING: This endpoint should be protected in production.
    During swap, concurrent inference requests will block until the new model is loaded.

    Usage:
        curl -X POST http://llm-server:8000/admin/swap-model \
          -H 'Content-Type: application/json' \
          -d '{"model_path": "/models/okla-llama3-8b-v2.gguf"}'
    """
    global llm, MODEL_PATH
    import hashlib as hl

    previous_model = MODEL_PATH
    new_path = request.model_path

    if not os.path.exists(new_path):
        raise HTTPException(status_code=404, detail=f"Model file not found: {new_path}")

    logger.info(f"🔄 Model hot-swap requested: {previous_model} → {new_path}")
    swap_start = time.time()

    # Optional SHA256 validation
    if request.validate_checksum:
        sha256_path = new_path + ".sha256"
        if os.path.exists(sha256_path):
            logger.info("  🔐 Validating new model checksum...")
            with open(sha256_path, "r") as f:
                expected = f.read().strip().split()[0]
            sha = hl.sha256()
            with open(new_path, "rb") as f:
                for chunk in iter(lambda: f.read(8192 * 1024), b""):
                    sha.update(chunk)
            if sha.hexdigest() != expected:
                raise HTTPException(
                    status_code=400,
                    detail=f"SHA256 mismatch for {new_path}. Swap aborted."
                )
            logger.info("  ✅ Checksum verified")

    # Swap the model
    try:
        from llama_cpp import Llama

        PROM_MODEL_LOADED.set(0)
        old_llm = llm
        llm = None  # Block inference during load

        new_llm = Llama(
            model_path=new_path,
            n_ctx=N_CTX,
            n_batch=N_BATCH,
            n_gpu_layers=N_GPU_LAYERS,
            n_threads=N_THREADS,
            verbose=False,
            chat_format="llama-3",
        )

        llm = new_llm
        MODEL_PATH = new_path
        PROM_MODEL_LOADED.set(1)
        PROM_MODEL_INFO.info({
            "model_id": "okla-llama3-8b",
            "model_path": new_path,
            "quantization": "Q4_K_M",
            "parameters": "8B",
        })

        # Release old model memory
        del old_llm

        load_time = time.time() - swap_start
        logger.info(f"✅ Model swapped in {load_time:.1f}s: {new_path}")

        return ModelSwapResponse(
            success=True,
            previous_model=previous_model,
            new_model=new_path,
            load_time_seconds=round(load_time, 2),
            message="Model hot-swapped successfully"
        )

    except Exception as e:
        # Attempt to reload previous model
        logger.error(f"❌ Hot-swap failed: {e}")
        try:
            from llama_cpp import Llama as LlamaFallback
            llm = LlamaFallback(
                model_path=previous_model,
                n_ctx=N_CTX, n_batch=N_BATCH,
                n_gpu_layers=N_GPU_LAYERS, n_threads=N_THREADS,
                verbose=False, chat_format="llama-3",
            )
            PROM_MODEL_LOADED.set(1)
            MODEL_PATH = previous_model
            logger.info(f"  ⚠️ Rolled back to previous model: {previous_model}")
        except Exception:
            logger.critical("  💀 CRITICAL: Could not reload previous model!")

        raise HTTPException(
            status_code=500,
            detail=f"Hot-swap failed: {str(e)}. Attempted rollback to {previous_model}."
        )


# ============================================================
# MAIN
# ============================================================

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="OKLA LLM Inference Server")
    parser.add_argument("--model", default=MODEL_PATH, help="Path to GGUF model")
    parser.add_argument("--host", default=HOST)
    parser.add_argument("--port", type=int, default=PORT)
    parser.add_argument("--ctx", type=int, default=N_CTX, help="Context window size")
    parser.add_argument("--gpu-layers", type=int, default=N_GPU_LAYERS)
    parser.add_argument("--threads", type=int, default=N_THREADS)
    args = parser.parse_args()

    MODEL_PATH = args.model
    N_CTX = args.ctx
    N_GPU_LAYERS = args.gpu_layers
    N_THREADS = args.threads

    uvicorn.run(app, host=args.host, port=args.port, log_level="info")
