"""
Video 360 FastAPI Service
API HTTP para procesar videos 360 y extraer frames.
"""

from fastapi import FastAPI, File, UploadFile, Form, HTTPException, BackgroundTasks
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Optional, List
import shutil
import tempfile
import os
import uuid
from pathlib import Path

from video360_processor import (
    extract_frames,
    get_video_info,
    ProcessingOptions,
    result_to_dict,
    VideoInfo
)

app = FastAPI(
    title="Video 360 Processor API",
    description="API para procesar videos 360 de vehículos y extraer frames para viewer interactivo",
    version="1.0.0"
)

# CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Directorio temporal para archivos
TEMP_DIR = Path(os.environ.get("TEMP_DIR", "/tmp/video360"))
OUTPUT_DIR = Path(os.environ.get("OUTPUT_DIR", "/tmp/video360/output"))

TEMP_DIR.mkdir(parents=True, exist_ok=True)
OUTPUT_DIR.mkdir(parents=True, exist_ok=True)


class ProcessingRequest(BaseModel):
    """Request para procesamiento"""
    frame_count: int = 6
    output_width: int = 1920
    output_height: int = 1080
    jpeg_quality: int = 90
    output_format: str = "jpg"
    smart_selection: bool = True
    auto_exposure: bool = True
    generate_thumbnails: bool = True
    thumbnail_width: int = 400


class VideoInfoResponse(BaseModel):
    """Response de información del video"""
    duration_seconds: float
    total_frames: int
    fps: float
    width: int
    height: int
    codec: str


class FrameResponse(BaseModel):
    """Response de un frame extraído"""
    sequence_number: int
    view_name: str
    angle_degrees: int
    file_path: str
    thumbnail_path: Optional[str]
    width: int
    height: int
    file_size_bytes: int
    source_frame_number: int
    timestamp_seconds: float
    quality_score: Optional[int]


class ProcessingResponse(BaseModel):
    """Response del procesamiento"""
    success: bool
    error: Optional[str]
    video_info: Optional[VideoInfoResponse]
    frames: List[FrameResponse]
    job_id: str


# Almacén de jobs en memoria (en producción usar Redis)
jobs = {}


@app.get("/")
async def root():
    """Health check"""
    return {"status": "ok", "service": "video360-processor"}


@app.get("/health")
async def health():
    """Health check endpoint"""
    return {"status": "healthy", "service": "video360-processor"}


@app.post("/api/process")
async def process_video(
    video: UploadFile = File(...),
    frame_count: int = Form(6),
    output_width: int = Form(1920),
    output_height: int = Form(1080),
    jpeg_quality: int = Form(90),
    output_format: str = Form("jpg"),
    smart_selection: bool = Form(True),
    auto_exposure: bool = Form(True),
    generate_thumbnails: bool = Form(True),
    thumbnail_width: int = Form(400)
) -> JSONResponse:
    """
    Procesa un video 360 y extrae frames equidistantes.
    
    - **video**: Archivo de video (mp4, mov, avi, webm, mkv)
    - **frame_count**: Número de frames a extraer (4-12, default 6)
    - **output_width**: Ancho de las imágenes de salida
    - **output_height**: Alto de las imágenes de salida
    - **jpeg_quality**: Calidad JPEG (1-100)
    - **output_format**: Formato de salida (jpg, png, webp)
    - **smart_selection**: Usar selección inteligente de frames
    - **auto_exposure**: Corregir exposición automáticamente
    - **generate_thumbnails**: Generar miniaturas
    - **thumbnail_width**: Ancho de las miniaturas
    """
    # Validar archivo
    if not video.filename:
        raise HTTPException(status_code=400, detail="No se proporcionó archivo de video")
    
    allowed_extensions = [".mp4", ".mov", ".avi", ".webm", ".mkv"]
    ext = Path(video.filename).suffix.lower()
    if ext not in allowed_extensions:
        raise HTTPException(
            status_code=400, 
            detail=f"Formato no soportado. Use: {', '.join(allowed_extensions)}"
        )
    
    # Generar job ID
    job_id = str(uuid.uuid4())
    
    # Guardar video temporalmente
    temp_video_path = TEMP_DIR / f"{job_id}{ext}"
    output_path = OUTPUT_DIR / job_id
    output_path.mkdir(parents=True, exist_ok=True)
    
    try:
        # Guardar archivo
        with open(temp_video_path, "wb") as buffer:
            shutil.copyfileobj(video.file, buffer)
        
        # Crear opciones
        options = ProcessingOptions(
            frame_count=frame_count,
            output_width=output_width,
            output_height=output_height,
            jpeg_quality=jpeg_quality,
            output_format=output_format,
            smart_selection=smart_selection,
            auto_exposure=auto_exposure,
            generate_thumbnails=generate_thumbnails,
            thumbnail_width=thumbnail_width
        )
        
        # Procesar
        result = extract_frames(str(temp_video_path), str(output_path), options)
        
        # Convertir a respuesta
        response = result_to_dict(result)
        response["job_id"] = job_id
        
        # Limpiar video temporal
        if temp_video_path.exists():
            temp_video_path.unlink()
        
        return JSONResponse(content=response)
        
    except Exception as e:
        # Limpiar en caso de error
        if temp_video_path.exists():
            temp_video_path.unlink()
        
        raise HTTPException(status_code=500, detail=str(e))


@app.get("/api/info")
async def get_info(path: str) -> JSONResponse:
    """Obtiene información de un video sin procesarlo"""
    if not os.path.exists(path):
        raise HTTPException(status_code=404, detail="Video no encontrado")
    
    info = get_video_info(path)
    if info is None:
        raise HTTPException(status_code=400, detail="No se pudo leer el video")
    
    return JSONResponse(content={
        "duration_seconds": info.duration_seconds,
        "total_frames": info.total_frames,
        "fps": info.fps,
        "width": info.width,
        "height": info.height,
        "codec": info.codec,
        "file_size_bytes": info.file_size_bytes
    })


@app.get("/api/jobs/{job_id}")
async def get_job(job_id: str) -> JSONResponse:
    """Obtiene el resultado de un job de procesamiento"""
    output_path = OUTPUT_DIR / job_id / "result.json"
    
    if not output_path.exists():
        raise HTTPException(status_code=404, detail="Job no encontrado")
    
    import json
    with open(output_path) as f:
        result = json.load(f)
    
    result["job_id"] = job_id
    return JSONResponse(content=result)


@app.delete("/api/jobs/{job_id}")
async def delete_job(job_id: str) -> JSONResponse:
    """Elimina los archivos de un job"""
    output_path = OUTPUT_DIR / job_id
    
    if not output_path.exists():
        raise HTTPException(status_code=404, detail="Job no encontrado")
    
    shutil.rmtree(output_path)
    
    return JSONResponse(content={"message": "Job eliminado", "job_id": job_id})


if __name__ == "__main__":
    import uvicorn
    port = int(os.environ.get("PORT", 8000))
    uvicorn.run(app, host="0.0.0.0", port=port)
