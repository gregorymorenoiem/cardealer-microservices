"""
Video 360 Frame Extractor - Worker Python con OpenCV
Extrae N frames equidistantes de un video 360 de vehículo.

Uso:
    python video360_processor.py <video_path> <output_dir> [options_json]

Ejemplo:
    python video360_processor.py input.mp4 ./output '{"frame_count": 6}'
"""

import cv2
import numpy as np
import json
import sys
import os
from pathlib import Path
from typing import List, Dict, Any, Optional, Tuple
from dataclasses import dataclass, asdict
import logging

# Configurar logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


@dataclass
class ProcessingOptions:
    """Opciones de procesamiento"""
    frame_count: int = 6
    output_width: int = 1920
    output_height: int = 1080
    jpeg_quality: int = 90
    output_format: str = "jpg"
    smart_selection: bool = True
    auto_exposure: bool = True
    generate_thumbnails: bool = True
    thumbnail_width: int = 400
    start_offset: float = 0.0
    end_offset: float = 0.0


@dataclass
class VideoInfo:
    """Información del video"""
    duration_seconds: float
    total_frames: int
    fps: float
    width: int
    height: int
    codec: str
    file_size_bytes: int = 0


@dataclass
class ExtractedFrame:
    """Frame extraído"""
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
    quality_score: Optional[int] = None


@dataclass
class ProcessingResult:
    """Resultado del procesamiento"""
    success: bool
    error: Optional[str]
    video_info: Optional[VideoInfo]
    frames: List[ExtractedFrame]


# Nombres estándar para cada vista
STANDARD_VIEWS = {
    1: ("Frente", 0),
    2: ("Frente-Derecha", 60),
    3: ("Derecha", 120),
    4: ("Atrás-Derecha", 180),
    5: ("Atrás", 240),
    6: ("Izquierda", 300),
}


def get_video_info(video_path: str) -> Optional[VideoInfo]:
    """Obtiene información del video"""
    try:
        cap = cv2.VideoCapture(video_path)
        if not cap.isOpened():
            logger.error(f"No se pudo abrir el video: {video_path}")
            return None
        
        fps = cap.get(cv2.CAP_PROP_FPS)
        total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
        width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
        height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
        duration = total_frames / fps if fps > 0 else 0
        
        # Obtener codec
        fourcc = int(cap.get(cv2.CAP_PROP_FOURCC))
        codec = "".join([chr((fourcc >> 8 * i) & 0xFF) for i in range(4)])
        
        cap.release()
        
        file_size = os.path.getsize(video_path) if os.path.exists(video_path) else 0
        
        return VideoInfo(
            duration_seconds=duration,
            total_frames=total_frames,
            fps=fps,
            width=width,
            height=height,
            codec=codec,
            file_size_bytes=file_size
        )
    except Exception as e:
        logger.error(f"Error obteniendo info del video: {e}")
        return None


def calculate_sharpness(image: np.ndarray) -> float:
    """Calcula la nitidez de una imagen usando Laplacian variance"""
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY) if len(image.shape) == 3 else image
    return cv2.Laplacian(gray, cv2.CV_64F).var()


def calculate_quality_score(image: np.ndarray) -> int:
    """Calcula un puntaje de calidad (0-100)"""
    # Factores: nitidez, contraste, brillo
    sharpness = calculate_sharpness(image)
    
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY) if len(image.shape) == 3 else image
    contrast = gray.std()
    brightness = gray.mean()
    
    # Normalizar y combinar
    sharpness_score = min(100, sharpness / 10)  # Normalizar
    contrast_score = min(100, contrast / 0.6)
    brightness_score = 100 - abs(brightness - 127) / 1.27  # Ideal ~127
    
    # Promedio ponderado
    score = int(0.5 * sharpness_score + 0.3 * contrast_score + 0.2 * brightness_score)
    return max(0, min(100, score))


def auto_correct_exposure(image: np.ndarray) -> np.ndarray:
    """Corrige automáticamente la exposición de la imagen"""
    # Convertir a LAB
    lab = cv2.cvtColor(image, cv2.COLOR_BGR2LAB)
    l, a, b = cv2.split(lab)
    
    # Aplicar CLAHE al canal L
    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
    l = clahe.apply(l)
    
    # Reconstruir imagen
    lab = cv2.merge([l, a, b])
    result = cv2.cvtColor(lab, cv2.COLOR_LAB2BGR)
    
    return result


def select_best_frame_in_range(
    cap: cv2.VideoCapture,
    start_frame: int,
    end_frame: int,
    samples: int = 5
) -> Tuple[int, np.ndarray]:
    """
    Selecciona el mejor frame dentro de un rango basándose en la calidad.
    Muestrea 'samples' frames y elige el de mayor nitidez.
    """
    if end_frame <= start_frame:
        cap.set(cv2.CAP_PROP_POS_FRAMES, start_frame)
        ret, frame = cap.read()
        return start_frame, frame
    
    step = max(1, (end_frame - start_frame) // samples)
    best_frame = None
    best_score = -1
    best_frame_num = start_frame
    
    for i in range(samples):
        frame_num = start_frame + i * step
        cap.set(cv2.CAP_PROP_POS_FRAMES, frame_num)
        ret, frame = cap.read()
        
        if not ret or frame is None:
            continue
        
        score = calculate_sharpness(frame)
        
        if score > best_score:
            best_score = score
            best_frame = frame.copy()
            best_frame_num = frame_num
    
    if best_frame is None:
        cap.set(cv2.CAP_PROP_POS_FRAMES, start_frame)
        ret, frame = cap.read()
        return start_frame, frame
    
    return best_frame_num, best_frame


def resize_image(
    image: np.ndarray,
    width: int,
    height: int,
    maintain_aspect: bool = True
) -> np.ndarray:
    """Redimensiona una imagen manteniendo la calidad"""
    if maintain_aspect:
        h, w = image.shape[:2]
        aspect = w / h
        
        if width / height > aspect:
            # Ajustar por altura
            new_height = height
            new_width = int(height * aspect)
        else:
            # Ajustar por ancho
            new_width = width
            new_height = int(width / aspect)
        
        resized = cv2.resize(image, (new_width, new_height), interpolation=cv2.INTER_LANCZOS4)
        
        # Crear canvas del tamaño final y centrar la imagen
        canvas = np.zeros((height, width, 3), dtype=np.uint8)
        y_offset = (height - new_height) // 2
        x_offset = (width - new_width) // 2
        canvas[y_offset:y_offset+new_height, x_offset:x_offset+new_width] = resized
        
        return canvas
    else:
        return cv2.resize(image, (width, height), interpolation=cv2.INTER_LANCZOS4)


def save_image(
    image: np.ndarray,
    output_path: str,
    format: str = "jpg",
    quality: int = 90
) -> int:
    """Guarda una imagen y retorna el tamaño del archivo"""
    if format.lower() in ["jpg", "jpeg"]:
        params = [cv2.IMWRITE_JPEG_QUALITY, quality]
    elif format.lower() == "png":
        params = [cv2.IMWRITE_PNG_COMPRESSION, 9 - (quality // 11)]  # 0-9
    elif format.lower() == "webp":
        params = [cv2.IMWRITE_WEBP_QUALITY, quality]
    else:
        params = []
    
    cv2.imwrite(output_path, image, params)
    return os.path.getsize(output_path)


def extract_frames(
    video_path: str,
    output_dir: str,
    options: ProcessingOptions,
    progress_callback: Optional[callable] = None
) -> ProcessingResult:
    """
    Extrae frames equidistantes de un video 360.
    
    Args:
        video_path: Ruta al video de entrada
        output_dir: Directorio para guardar los frames
        options: Opciones de procesamiento
        progress_callback: Callback para reportar progreso (0-100)
    
    Returns:
        ProcessingResult con los frames extraídos
    """
    try:
        # Crear directorio de salida
        Path(output_dir).mkdir(parents=True, exist_ok=True)
        thumbnail_dir = Path(output_dir) / "thumbnails"
        if options.generate_thumbnails:
            thumbnail_dir.mkdir(exist_ok=True)
        
        # Obtener info del video
        video_info = get_video_info(video_path)
        if video_info is None:
            return ProcessingResult(
                success=False,
                error="No se pudo leer el video",
                video_info=None,
                frames=[]
            )
        
        logger.info(f"Video: {video_info.duration_seconds:.2f}s, "
                   f"{video_info.total_frames} frames, {video_info.fps:.2f} fps")
        
        # Abrir video
        cap = cv2.VideoCapture(video_path)
        if not cap.isOpened():
            return ProcessingResult(
                success=False,
                error="No se pudo abrir el video",
                video_info=video_info,
                frames=[]
            )
        
        # Calcular frames efectivos (excluyendo offsets)
        start_frame = int(options.start_offset * video_info.fps)
        end_frame = video_info.total_frames - int(options.end_offset * video_info.fps)
        effective_frames = end_frame - start_frame
        
        if effective_frames < options.frame_count:
            cap.release()
            return ProcessingResult(
                success=False,
                error="El video es muy corto para extraer los frames solicitados",
                video_info=video_info,
                frames=[]
            )
        
        # Calcular posiciones de extracción
        frame_interval = effective_frames / options.frame_count
        extracted_frames = []
        
        for i in range(options.frame_count):
            # Reportar progreso
            if progress_callback:
                progress = int((i / options.frame_count) * 100)
                progress_callback(progress)
            
            # Calcular rango de frames para esta posición
            target_frame = int(start_frame + i * frame_interval)
            range_start = target_frame
            range_end = min(int(start_frame + (i + 1) * frame_interval), end_frame)
            
            # Seleccionar el mejor frame en el rango
            if options.smart_selection:
                frame_num, frame = select_best_frame_in_range(
                    cap, range_start, range_end, samples=5
                )
            else:
                cap.set(cv2.CAP_PROP_POS_FRAMES, target_frame)
                ret, frame = cap.read()
                frame_num = target_frame
                if not ret or frame is None:
                    logger.warning(f"No se pudo leer el frame {target_frame}")
                    continue
            
            if frame is None:
                logger.warning(f"Frame nulo en posición {i+1}")
                continue
            
            # Corregir exposición si está habilitado
            if options.auto_exposure:
                frame = auto_correct_exposure(frame)
            
            # Redimensionar
            frame = resize_image(
                frame,
                options.output_width,
                options.output_height,
                maintain_aspect=True
            )
            
            # Calcular calidad
            quality_score = calculate_quality_score(frame)
            
            # Nombre de la vista
            seq_num = i + 1
            view_name, angle = STANDARD_VIEWS.get(seq_num, (f"Vista-{seq_num}", i * (360 // options.frame_count)))
            
            # Guardar imagen principal
            ext = options.output_format.lower()
            filename = f"frame_{seq_num:02d}_{view_name.replace(' ', '_').lower()}.{ext}"
            filepath = os.path.join(output_dir, filename)
            file_size = save_image(frame, filepath, ext, options.jpeg_quality)
            
            # Guardar thumbnail
            thumbnail_path = None
            if options.generate_thumbnails:
                thumb_height = int(options.thumbnail_width * options.output_height / options.output_width)
                thumbnail = cv2.resize(frame, (options.thumbnail_width, thumb_height), 
                                       interpolation=cv2.INTER_AREA)
                thumb_filename = f"thumb_{seq_num:02d}.{ext}"
                thumbnail_path = str(thumbnail_dir / thumb_filename)
                save_image(thumbnail, thumbnail_path, ext, 80)
            
            # Crear registro del frame
            timestamp = frame_num / video_info.fps if video_info.fps > 0 else 0
            
            extracted_frame = ExtractedFrame(
                sequence_number=seq_num,
                view_name=view_name,
                angle_degrees=angle,
                file_path=filepath,
                thumbnail_path=thumbnail_path,
                width=frame.shape[1],
                height=frame.shape[0],
                file_size_bytes=file_size,
                source_frame_number=frame_num,
                timestamp_seconds=timestamp,
                quality_score=quality_score
            )
            extracted_frames.append(extracted_frame)
            
            logger.info(f"Extraído frame {seq_num}/{options.frame_count}: "
                       f"{view_name} (calidad: {quality_score})")
            
            # Imprimir progreso para subprocess
            print(f"PROGRESS:{int((i + 1) / options.frame_count * 100)}")
        
        cap.release()
        
        if len(extracted_frames) == 0:
            return ProcessingResult(
                success=False,
                error="No se pudo extraer ningún frame",
                video_info=video_info,
                frames=[]
            )
        
        logger.info(f"Extracción completada: {len(extracted_frames)} frames")
        
        return ProcessingResult(
            success=True,
            error=None,
            video_info=video_info,
            frames=extracted_frames
        )
        
    except Exception as e:
        logger.exception(f"Error procesando video: {e}")
        return ProcessingResult(
            success=False,
            error=str(e),
            video_info=None,
            frames=[]
        )


def result_to_dict(result: ProcessingResult) -> Dict[str, Any]:
    """Convierte el resultado a diccionario para JSON"""
    return {
        "success": result.success,
        "error": result.error,
        "video_info": asdict(result.video_info) if result.video_info else None,
        "frames": [asdict(f) for f in result.frames]
    }


def main():
    """Punto de entrada principal"""
    if len(sys.argv) < 3:
        print("Uso: python video360_processor.py <video_path> <output_dir> [options_json]")
        sys.exit(1)
    
    video_path = sys.argv[1]
    output_dir = sys.argv[2]
    
    # Parsear opciones
    options = ProcessingOptions()
    if len(sys.argv) > 3:
        try:
            opts = json.loads(sys.argv[3])
            options = ProcessingOptions(
                frame_count=opts.get("frame_count", 6),
                output_width=opts.get("output_width", 1920),
                output_height=opts.get("output_height", 1080),
                jpeg_quality=opts.get("jpeg_quality", 90),
                output_format=opts.get("output_format", "jpg"),
                smart_selection=opts.get("smart_selection", True),
                auto_exposure=opts.get("auto_exposure", True),
                generate_thumbnails=opts.get("generate_thumbnails", True),
                thumbnail_width=opts.get("thumbnail_width", 400),
                start_offset=opts.get("start_offset", 0.0),
                end_offset=opts.get("end_offset", 0.0)
            )
        except json.JSONDecodeError as e:
            logger.error(f"Error parseando opciones JSON: {e}")
    
    logger.info(f"Procesando video: {video_path}")
    logger.info(f"Output: {output_dir}")
    logger.info(f"Opciones: {options}")
    
    # Procesar
    result = extract_frames(video_path, output_dir, options)
    
    # Guardar resultado como JSON
    result_path = os.path.join(output_dir, "result.json")
    with open(result_path, "w") as f:
        json.dump(result_to_dict(result), f, indent=2)
    
    # Imprimir resultado
    print(json.dumps(result_to_dict(result), indent=2))
    
    sys.exit(0 if result.success else 1)


if __name__ == "__main__":
    main()
