#!/usr/bin/env python3
"""
Self-Improving Vehicle Segmentation Pipeline
=============================================

Sistema de aprendizaje continuo que mejora automáticamente con el uso:

1. FEEDBACK LOOP: Usuarios aprueban/rechazan resultados
2. DATA COLLECTION: Almacena imágenes buenas para reentrenamiento
3. QUALITY METRICS: Detecta automáticamente resultados pobres
4. FINE-TUNING: Reentrena modelos periódicamente
5. A/B TESTING: Compara versiones del modelo

Author: Gregory Moreno
Date: January 2026
"""

import cv2
import numpy as np
import torch
import json
import sqlite3
import hashlib
import shutil
from pathlib import Path
from datetime import datetime, timedelta
from typing import Optional, Tuple, Dict, Any, List
from dataclasses import dataclass, field, asdict
from enum import Enum
import threading
import queue
from ultralytics import YOLO
from PIL import Image

# Paths
SCRIPT_DIR = Path(__file__).parent
DATA_DIR = SCRIPT_DIR / "training_data"
MODELS_DIR = SCRIPT_DIR / "models"
DB_PATH = SCRIPT_DIR / "pipeline_learning.db"

# Ensure directories exist
DATA_DIR.mkdir(parents=True, exist_ok=True)
(DATA_DIR / "approved").mkdir(exist_ok=True)
(DATA_DIR / "rejected").mkdir(exist_ok=True)
(DATA_DIR / "pending_review").mkdir(exist_ok=True)

DEVICE = "mps" if torch.backends.mps.is_available() else "cuda" if torch.cuda.is_available() else "cpu"


class FeedbackType(Enum):
    APPROVED = "approved"           # Usuario aprobó el resultado
    REJECTED = "rejected"           # Usuario rechazó completamente
    EDITED = "edited"               # Usuario editó manualmente la máscara
    AUTO_APPROVED = "auto_approved" # Sistema aprobó por alta confianza
    AUTO_REJECTED = "auto_rejected" # Sistema rechazó por baja calidad


class QualityLevel(Enum):
    EXCELLENT = "excellent"   # >95% calidad estimada
    GOOD = "good"             # 80-95%
    ACCEPTABLE = "acceptable" # 60-80%
    POOR = "poor"             # <60%


@dataclass
class ProcessingResult:
    """Resultado de procesamiento con metadatos para aprendizaje"""
    image_id: str
    input_path: str
    output_path: str
    mask_path: str
    
    # Métricas del modelo
    detection_confidence: float
    coverage_percent: float
    edge_smoothness: float
    shadow_detected: bool
    wheels_protected: bool
    
    # Métricas de calidad estimada
    quality_score: float
    quality_level: QualityLevel
    
    # Feedback
    feedback: Optional[FeedbackType] = None
    feedback_timestamp: Optional[str] = None
    user_notes: Optional[str] = None
    
    # Para reentrenamiento
    used_for_training: bool = False
    model_version: str = "v2.0"
    
    timestamp: str = field(default_factory=lambda: datetime.now().isoformat())


class LearningDatabase:
    """Base de datos SQLite para almacenar historial de procesamiento y feedback"""
    
    def __init__(self, db_path: Path = DB_PATH):
        self.db_path = db_path
        self._init_db()
        
    def _init_db(self):
        """Inicializar esquema de base de datos"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        # Tabla principal de resultados
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS processing_results (
                image_id TEXT PRIMARY KEY,
                input_path TEXT,
                output_path TEXT,
                mask_path TEXT,
                detection_confidence REAL,
                coverage_percent REAL,
                edge_smoothness REAL,
                shadow_detected INTEGER,
                wheels_protected INTEGER,
                quality_score REAL,
                quality_level TEXT,
                feedback TEXT,
                feedback_timestamp TEXT,
                user_notes TEXT,
                used_for_training INTEGER DEFAULT 0,
                model_version TEXT,
                timestamp TEXT
            )
        ''')
        
        # Tabla de métricas del modelo por versión
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS model_metrics (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                model_version TEXT,
                total_processed INTEGER,
                approved_count INTEGER,
                rejected_count INTEGER,
                avg_quality_score REAL,
                avg_confidence REAL,
                timestamp TEXT
            )
        ''')
        
        # Tabla de historial de reentrenamiento
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS training_history (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                model_version TEXT,
                training_samples INTEGER,
                validation_accuracy REAL,
                improvement_percent REAL,
                timestamp TEXT
            )
        ''')
        
        conn.commit()
        conn.close()
        
    def save_result(self, result: ProcessingResult):
        """Guardar resultado de procesamiento"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            INSERT OR REPLACE INTO processing_results 
            (image_id, input_path, output_path, mask_path, detection_confidence,
             coverage_percent, edge_smoothness, shadow_detected, wheels_protected,
             quality_score, quality_level, feedback, feedback_timestamp, user_notes,
             used_for_training, model_version, timestamp)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ''', (
            result.image_id, result.input_path, result.output_path, result.mask_path,
            result.detection_confidence, result.coverage_percent, result.edge_smoothness,
            1 if result.shadow_detected else 0, 1 if result.wheels_protected else 0,
            result.quality_score, result.quality_level.value,
            result.feedback.value if result.feedback else None,
            result.feedback_timestamp, result.user_notes,
            1 if result.used_for_training else 0, result.model_version, result.timestamp
        ))
        
        conn.commit()
        conn.close()
        
    def update_feedback(self, image_id: str, feedback: FeedbackType, notes: str = None):
        """Actualizar feedback de usuario"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            UPDATE processing_results 
            SET feedback = ?, feedback_timestamp = ?, user_notes = ?
            WHERE image_id = ?
        ''', (feedback.value, datetime.now().isoformat(), notes, image_id))
        
        conn.commit()
        conn.close()
        
    def get_training_candidates(self, min_quality: float = 0.7) -> List[Dict]:
        """Obtener imágenes aprobadas para reentrenamiento"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            SELECT * FROM processing_results
            WHERE (feedback = 'approved' OR feedback = 'auto_approved')
            AND used_for_training = 0
            AND quality_score >= ?
            ORDER BY quality_score DESC
            LIMIT 100
        ''', (min_quality,))
        
        columns = [desc[0] for desc in cursor.description]
        results = [dict(zip(columns, row)) for row in cursor.fetchall()]
        
        conn.close()
        return results
        
    def get_rejection_patterns(self) -> Dict[str, Any]:
        """Analizar patrones de rechazos para identificar debilidades"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        # Estadísticas de rechazos
        cursor.execute('''
            SELECT 
                COUNT(*) as total_rejected,
                AVG(detection_confidence) as avg_confidence,
                AVG(coverage_percent) as avg_coverage,
                AVG(quality_score) as avg_quality,
                SUM(CASE WHEN shadow_detected = 0 THEN 1 ELSE 0 END) as shadow_issues,
                SUM(CASE WHEN wheels_protected = 0 THEN 1 ELSE 0 END) as wheel_issues
            FROM processing_results
            WHERE feedback = 'rejected'
        ''')
        
        row = cursor.fetchone()
        conn.close()
        
        if row and row[0] > 0:
            return {
                'total_rejected': row[0],
                'avg_confidence': row[1],
                'avg_coverage': row[2],
                'avg_quality': row[3],
                'shadow_issues_percent': row[4] / row[0] * 100 if row[0] > 0 else 0,
                'wheel_issues_percent': row[5] / row[0] * 100 if row[0] > 0 else 0
            }
        return {}
        
    def get_model_performance(self, model_version: str = None) -> Dict[str, Any]:
        """Obtener métricas de rendimiento del modelo"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        query = '''
            SELECT 
                model_version,
                COUNT(*) as total,
                SUM(CASE WHEN feedback IN ('approved', 'auto_approved') THEN 1 ELSE 0 END) as approved,
                SUM(CASE WHEN feedback = 'rejected' THEN 1 ELSE 0 END) as rejected,
                AVG(quality_score) as avg_quality,
                AVG(detection_confidence) as avg_confidence
            FROM processing_results
            WHERE feedback IS NOT NULL
        '''
        
        if model_version:
            query += f" AND model_version = '{model_version}'"
            
        query += " GROUP BY model_version"
        
        cursor.execute(query)
        
        results = {}
        for row in cursor.fetchall():
            version = row[0]
            total = row[1]
            approved = row[2]
            results[version] = {
                'total': total,
                'approved': approved,
                'rejected': row[3],
                'approval_rate': approved / total * 100 if total > 0 else 0,
                'avg_quality': row[4],
                'avg_confidence': row[5]
            }
            
        conn.close()
        return results


class QualityEstimator:
    """Estima la calidad del resultado automáticamente"""
    
    def __init__(self):
        self.thresholds = {
            'min_coverage': 0.05,      # Mínimo 5% de la imagen
            'max_coverage': 0.85,      # Máximo 85% (no debería cubrir todo)
            'min_confidence': 0.5,     # Confianza mínima de detección
            'edge_smoothness': 0.7,    # Suavidad de bordes
        }
        
    def estimate_quality(self, 
                         image: np.ndarray, 
                         mask: np.ndarray,
                         detection_confidence: float) -> Tuple[float, QualityLevel, Dict]:
        """
        Estimar calidad del resultado basado en múltiples métricas
        Returns: (score 0-1, level, details)
        """
        h, w = mask.shape[:2]
        total_pixels = h * w
        
        # 1. Coverage check
        mask_binary = (mask > 128).astype(np.uint8) if len(mask.shape) == 2 else (mask[:,:,3] > 128).astype(np.uint8)
        coverage = mask_binary.sum() / total_pixels
        coverage_score = self._score_coverage(coverage)
        
        # 2. Edge quality
        edges = cv2.Canny(mask_binary * 255, 50, 150)
        edge_length = edges.sum() / 255
        contours, _ = cv2.findContours(mask_binary, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        
        edge_score = 1.0
        if contours:
            largest = max(contours, key=cv2.contourArea)
            perimeter = cv2.arcLength(largest, True)
            area = cv2.contourArea(largest)
            if perimeter > 0:
                circularity = 4 * np.pi * area / (perimeter ** 2)
                edge_score = min(1.0, circularity * 2)  # Más circular = mejor (para autos)
        
        # 3. Hole detection (ventanas no deberían crear huecos)
        filled = mask_binary.copy()
        if contours:
            cv2.drawContours(filled, [max(contours, key=cv2.contourArea)], -1, 1, -1)
        holes = filled.sum() - mask_binary.sum()
        hole_ratio = holes / (coverage * total_pixels) if coverage > 0 else 0
        hole_score = max(0, 1 - hole_ratio * 2)
        
        # 4. Detection confidence
        conf_score = min(1.0, detection_confidence / 0.9)
        
        # 5. Bottom region check (ruedas)
        bottom_region = mask_binary[int(h * 0.7):, :]
        bottom_coverage = bottom_region.sum() / bottom_region.size
        wheel_score = min(1.0, bottom_coverage / 0.1) if coverage > 0.1 else 1.0
        
        # Weighted final score
        weights = {
            'coverage': 0.15,
            'edges': 0.20,
            'holes': 0.15,
            'confidence': 0.30,
            'wheels': 0.20
        }
        
        final_score = (
            coverage_score * weights['coverage'] +
            edge_score * weights['edges'] +
            hole_score * weights['holes'] +
            conf_score * weights['confidence'] +
            wheel_score * weights['wheels']
        )
        
        # Determine level
        if final_score >= 0.90:
            level = QualityLevel.EXCELLENT
        elif final_score >= 0.75:
            level = QualityLevel.GOOD
        elif final_score >= 0.60:
            level = QualityLevel.ACCEPTABLE
        else:
            level = QualityLevel.POOR
            
        details = {
            'coverage': coverage,
            'coverage_score': coverage_score,
            'edge_score': edge_score,
            'hole_score': hole_score,
            'conf_score': conf_score,
            'wheel_score': wheel_score,
            'final_score': final_score
        }
        
        return final_score, level, details
        
    def _score_coverage(self, coverage: float) -> float:
        """Score basado en cobertura (ni muy poco ni demasiado)"""
        if coverage < self.thresholds['min_coverage']:
            return coverage / self.thresholds['min_coverage']
        elif coverage > self.thresholds['max_coverage']:
            return max(0, 1 - (coverage - self.thresholds['max_coverage']) * 5)
        else:
            # Rango óptimo: 10-50%
            if 0.10 <= coverage <= 0.50:
                return 1.0
            elif coverage < 0.10:
                return 0.8 + (coverage - 0.05) * 4
            else:
                return 1.0 - (coverage - 0.50) * 1.5


class ActiveLearningSelector:
    """Selecciona las mejores muestras para reentrenamiento"""
    
    def __init__(self, db: LearningDatabase):
        self.db = db
        
    def select_training_samples(self, max_samples: int = 100) -> List[Dict]:
        """
        Seleccionar muestras diversas y de alta calidad para reentrenamiento
        
        Estrategia:
        1. 60% mejores aprobados (alta calidad)
        2. 20% casos límite (para mejorar en casos difíciles)
        3. 20% rechazados editados (correcciones del usuario)
        """
        samples = []
        
        # 1. Mejores aprobados
        approved = self.db.get_training_candidates(min_quality=0.8)
        samples.extend(approved[:int(max_samples * 0.6)])
        
        # 2. Casos límite (quality 0.6-0.8, aprobados)
        conn = sqlite3.connect(self.db.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            SELECT * FROM processing_results
            WHERE feedback IN ('approved', 'auto_approved')
            AND quality_score BETWEEN 0.6 AND 0.8
            AND used_for_training = 0
            ORDER BY RANDOM()
            LIMIT ?
        ''', (int(max_samples * 0.2),))
        
        columns = [desc[0] for desc in cursor.description]
        edge_cases = [dict(zip(columns, row)) for row in cursor.fetchall()]
        samples.extend(edge_cases)
        
        # 3. Editados por usuario (correcciones valiosas)
        cursor.execute('''
            SELECT * FROM processing_results
            WHERE feedback = 'edited'
            AND used_for_training = 0
            ORDER BY timestamp DESC
            LIMIT ?
        ''', (int(max_samples * 0.2),))
        
        edited = [dict(zip(columns, row)) for row in cursor.fetchall()]
        samples.extend(edited)
        
        conn.close()
        
        return samples[:max_samples]


class ModelTrainer:
    """Reentrenador de modelos basado en feedback"""
    
    def __init__(self, db: LearningDatabase):
        self.db = db
        self.selector = ActiveLearningSelector(db)
        self.training_dir = DATA_DIR / "training"
        self.training_dir.mkdir(exist_ok=True)
        
    def prepare_training_data(self) -> Tuple[Path, int]:
        """Preparar datos para reentrenamiento"""
        samples = self.selector.select_training_samples()
        
        if len(samples) < 20:
            print(f"⚠️ Insuficientes muestras para reentrenamiento ({len(samples)}/20)")
            return None, 0
            
        # Crear estructura YOLO
        train_dir = self.training_dir / "images" / "train"
        labels_dir = self.training_dir / "labels" / "train"
        train_dir.mkdir(parents=True, exist_ok=True)
        labels_dir.mkdir(parents=True, exist_ok=True)
        
        prepared = 0
        for sample in samples:
            try:
                # Copiar imagen
                input_path = Path(sample['input_path'])
                if input_path.exists():
                    shutil.copy(input_path, train_dir / input_path.name)
                    
                    # Crear label YOLO desde máscara
                    mask_path = Path(sample['mask_path'])
                    if mask_path.exists():
                        mask = cv2.imread(str(mask_path), cv2.IMREAD_GRAYSCALE)
                        label = self._mask_to_yolo_label(mask, input_path.stem)
                        
                        label_file = labels_dir / f"{input_path.stem}.txt"
                        with open(label_file, 'w') as f:
                            f.write(label)
                            
                        prepared += 1
                        
            except Exception as e:
                print(f"Error preparando {sample['image_id']}: {e}")
                
        # Crear dataset.yaml
        yaml_content = f"""
train: {train_dir}
val: {train_dir}

nc: 1
names: ['vehicle']
"""
        with open(self.training_dir / "dataset.yaml", 'w') as f:
            f.write(yaml_content)
            
        return self.training_dir / "dataset.yaml", prepared
        
    def _mask_to_yolo_label(self, mask: np.ndarray, image_id: str) -> str:
        """Convertir máscara a formato de label YOLO segmentación"""
        h, w = mask.shape
        
        # Encontrar contornos
        binary = (mask > 128).astype(np.uint8)
        contours, _ = cv2.findContours(binary, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        
        if not contours:
            return ""
            
        # Usar el contorno más grande
        largest = max(contours, key=cv2.contourArea)
        
        # Simplificar contorno
        epsilon = 0.001 * cv2.arcLength(largest, True)
        approx = cv2.approxPolyDP(largest, epsilon, True)
        
        # Convertir a formato YOLO (normalizado 0-1)
        points = []
        for point in approx.reshape(-1, 2):
            x_norm = point[0] / w
            y_norm = point[1] / h
            points.extend([f"{x_norm:.6f}", f"{y_norm:.6f}"])
            
        # Clase 0 (vehicle) + puntos del polígono
        return f"0 {' '.join(points)}"
        
    def fine_tune_yolo(self, epochs: int = 10) -> Optional[Path]:
        """Fine-tune del modelo YOLO con nuevos datos"""
        dataset_path, num_samples = self.prepare_training_data()
        
        if dataset_path is None:
            return None
            
        print(f"\n🔄 Iniciando fine-tuning con {num_samples} muestras...")
        
        try:
            # Cargar modelo base
            base_model = MODELS_DIR / "yolov8x-seg.pt"
            model = YOLO(str(base_model))
            
            # Fine-tune
            results = model.train(
                data=str(dataset_path),
                epochs=epochs,
                imgsz=640,
                batch=4,
                device=DEVICE,
                project=str(MODELS_DIR / "fine_tuned"),
                name=f"yolo_ft_{datetime.now().strftime('%Y%m%d_%H%M%S')}",
                exist_ok=True,
                pretrained=True,
                freeze=10,  # Congelar primeras 10 capas
                lr0=0.0001,  # Learning rate bajo para fine-tuning
            )
            
            # Guardar nueva versión
            new_version = f"v2.{len(list((MODELS_DIR / 'fine_tuned').glob('yolo_ft_*')))}"
            new_model_path = MODELS_DIR / f"yolov8x-seg_{new_version}.pt"
            
            best_model = Path(results.save_dir) / "weights" / "best.pt"
            if best_model.exists():
                shutil.copy(best_model, new_model_path)
                print(f"✅ Nuevo modelo guardado: {new_model_path.name}")
                
                # Registrar en historial
                conn = sqlite3.connect(self.db.db_path)
                cursor = conn.cursor()
                cursor.execute('''
                    INSERT INTO training_history 
                    (model_version, training_samples, validation_accuracy, timestamp)
                    VALUES (?, ?, ?, ?)
                ''', (new_version, num_samples, 0.0, datetime.now().isoformat()))
                conn.commit()
                conn.close()
                
                return new_model_path
                
        except Exception as e:
            print(f"❌ Error en fine-tuning: {e}")
            
        return None


class SelfImprovingPipeline:
    """Pipeline principal con aprendizaje continuo"""
    
    def __init__(self):
        self.db = LearningDatabase()
        self.quality_estimator = QualityEstimator()
        self.trainer = ModelTrainer(self.db)
        
        # Cargar mejor modelo disponible
        self.model_path = self._get_best_model()
        self.model = YOLO(str(self.model_path))
        self.model_version = self._get_model_version()
        
        print(f"🚗 Self-Improving Pipeline initialized")
        print(f"   Model: {self.model_path.name}")
        print(f"   Version: {self.model_version}")
        
    def _get_best_model(self) -> Path:
        """Obtener el mejor modelo disponible"""
        # Buscar modelos fine-tuned
        ft_models = list(MODELS_DIR.glob("yolov8x-seg_v*.pt"))
        
        if ft_models:
            # Usar el más reciente
            return max(ft_models, key=lambda p: p.stat().st_mtime)
        
        # Fallback al modelo base
        return MODELS_DIR / "yolov8x-seg.pt"
        
    def _get_model_version(self) -> str:
        """Extraer versión del modelo"""
        name = self.model_path.stem
        if "_v" in name:
            return name.split("_v")[-1]
        return "2.0"
        
    def process(self, image_path: Path) -> ProcessingResult:
        """Procesar imagen y registrar para aprendizaje"""
        
        # Generar ID único
        image_id = hashlib.md5(
            f"{image_path.name}_{datetime.now().isoformat()}".encode()
        ).hexdigest()[:12]
        
        # Cargar imagen
        image = cv2.imread(str(image_path))
        h, w = image.shape[:2]
        
        # Detectar vehículo
        results = self.model(image, conf=0.3, verbose=False)[0]
        
        if results.masks is None:
            return ProcessingResult(
                image_id=image_id,
                input_path=str(image_path),
                output_path="",
                mask_path="",
                detection_confidence=0,
                coverage_percent=0,
                edge_smoothness=0,
                shadow_detected=False,
                wheels_protected=False,
                quality_score=0,
                quality_level=QualityLevel.POOR,
                model_version=self.model_version
            )
        
        # Obtener mejor máscara
        vehicle_classes = {2, 3, 5, 7}
        best_mask = None
        best_conf = 0
        
        for i, mask in enumerate(results.masks.data):
            cls = int(results.boxes.cls[i])
            conf = float(results.boxes.conf[i])
            if cls in vehicle_classes and conf > best_conf:
                best_conf = conf
                best_mask = mask.cpu().numpy()
        
        if best_mask is None:
            return self._create_empty_result(image_id, image_path)
            
        # Redimensionar máscara
        mask_resized = cv2.resize(
            best_mask.astype(np.float32),
            (w, h),
            interpolation=cv2.INTER_LINEAR
        )
        mask_uint8 = (mask_resized * 255).astype(np.uint8)
        
        # Estimar calidad
        quality_score, quality_level, quality_details = self.quality_estimator.estimate_quality(
            image, mask_uint8, best_conf
        )
        
        # Crear output
        output_dir = SCRIPT_DIR / "output_learning"
        output_dir.mkdir(exist_ok=True)
        
        # RGBA output
        rgba = cv2.cvtColor(image, cv2.COLOR_BGR2BGRA)
        rgba[:, :, 3] = mask_uint8
        
        output_path = output_dir / f"{image_id}_output.png"
        mask_path = output_dir / f"{image_id}_mask.png"
        
        cv2.imwrite(str(output_path), rgba)
        cv2.imwrite(str(mask_path), mask_uint8)
        
        # Crear resultado
        result = ProcessingResult(
            image_id=image_id,
            input_path=str(image_path),
            output_path=str(output_path),
            mask_path=str(mask_path),
            detection_confidence=best_conf,
            coverage_percent=quality_details['coverage'] * 100,
            edge_smoothness=quality_details['edge_score'],
            shadow_detected=True,  # TODO: Implementar detección real
            wheels_protected=quality_details['wheel_score'] > 0.7,
            quality_score=quality_score,
            quality_level=quality_level,
            model_version=self.model_version
        )
        
        # Auto-aprobar si calidad excelente
        if quality_level == QualityLevel.EXCELLENT:
            result.feedback = FeedbackType.AUTO_APPROVED
            result.feedback_timestamp = datetime.now().isoformat()
            
            # Copiar a directorio de aprobados
            shutil.copy(image_path, DATA_DIR / "approved" / image_path.name)
            shutil.copy(mask_path, DATA_DIR / "approved" / f"{image_id}_mask.png")
            
        elif quality_level == QualityLevel.POOR:
            result.feedback = FeedbackType.AUTO_REJECTED
            result.feedback_timestamp = datetime.now().isoformat()
        else:
            # Pendiente de revisión
            shutil.copy(image_path, DATA_DIR / "pending_review" / image_path.name)
            
        # Guardar en DB
        self.db.save_result(result)
        
        return result
        
    def _create_empty_result(self, image_id: str, image_path: Path) -> ProcessingResult:
        """Crear resultado vacío para imágenes sin detección"""
        return ProcessingResult(
            image_id=image_id,
            input_path=str(image_path),
            output_path="",
            mask_path="",
            detection_confidence=0,
            coverage_percent=0,
            edge_smoothness=0,
            shadow_detected=False,
            wheels_protected=False,
            quality_score=0,
            quality_level=QualityLevel.POOR,
            model_version=self.model_version
        )
        
    def submit_feedback(self, image_id: str, approved: bool, 
                        edited_mask_path: Optional[str] = None,
                        notes: str = None):
        """
        Recibir feedback del usuario
        
        Args:
            image_id: ID de la imagen procesada
            approved: True si aprobado, False si rechazado
            edited_mask_path: Ruta a máscara editada manualmente (opcional)
            notes: Notas del usuario
        """
        if edited_mask_path:
            feedback = FeedbackType.EDITED
            # Copiar máscara editada
            shutil.copy(edited_mask_path, DATA_DIR / "approved" / f"{image_id}_mask_edited.png")
        elif approved:
            feedback = FeedbackType.APPROVED
        else:
            feedback = FeedbackType.REJECTED
            
        self.db.update_feedback(image_id, feedback, notes)
        
        # Si aprobado, mover a carpeta de entrenamiento
        conn = sqlite3.connect(self.db.db_path)
        cursor = conn.cursor()
        cursor.execute('SELECT input_path, mask_path FROM processing_results WHERE image_id = ?', (image_id,))
        row = cursor.fetchone()
        conn.close()
        
        if row and approved:
            input_path = Path(row[0])
            mask_path = Path(row[1])
            
            if input_path.exists():
                dest = DATA_DIR / "approved" / input_path.name
                if not dest.exists():
                    shutil.copy(input_path, dest)
                    
            if mask_path.exists():
                dest = DATA_DIR / "approved" / mask_path.name
                if not dest.exists():
                    shutil.copy(mask_path, dest)
                    
        print(f"✅ Feedback registrado: {image_id} -> {feedback.value}")
        
    def trigger_retraining(self, min_samples: int = 50) -> bool:
        """
        Disparar reentrenamiento si hay suficientes muestras
        
        Returns: True si se realizó reentrenamiento
        """
        # Verificar si hay suficientes muestras nuevas
        candidates = self.db.get_training_candidates()
        
        if len(candidates) < min_samples:
            print(f"⏳ Esperando más muestras ({len(candidates)}/{min_samples})")
            return False
            
        print(f"\n🔄 Iniciando reentrenamiento con {len(candidates)} muestras...")
        
        new_model = self.trainer.fine_tune_yolo(epochs=10)
        
        if new_model:
            # Actualizar modelo activo
            self.model_path = new_model
            self.model = YOLO(str(new_model))
            self.model_version = self._get_model_version()
            
            # Marcar muestras como usadas
            conn = sqlite3.connect(self.db.db_path)
            cursor = conn.cursor()
            cursor.execute('''
                UPDATE processing_results 
                SET used_for_training = 1 
                WHERE feedback IN ('approved', 'auto_approved', 'edited')
                AND used_for_training = 0
            ''')
            conn.commit()
            conn.close()
            
            print(f"✅ Modelo actualizado a versión {self.model_version}")
            return True
            
        return False
        
    def get_performance_report(self) -> Dict[str, Any]:
        """Generar reporte de rendimiento"""
        performance = self.db.get_model_performance()
        rejection_patterns = self.db.get_rejection_patterns()
        
        # Calcular mejora entre versiones
        versions = sorted(performance.keys())
        improvement = None
        
        if len(versions) >= 2:
            prev = performance[versions[-2]]
            curr = performance[versions[-1]]
            improvement = {
                'approval_rate_change': curr['approval_rate'] - prev['approval_rate'],
                'quality_change': (curr['avg_quality'] or 0) - (prev['avg_quality'] or 0)
            }
            
        return {
            'current_version': self.model_version,
            'performance_by_version': performance,
            'rejection_analysis': rejection_patterns,
            'improvement': improvement,
            'pending_samples': len(list((DATA_DIR / "pending_review").glob("*"))),
            'approved_samples': len(list((DATA_DIR / "approved").glob("*.jpg")))
        }


def main():
    """Demo del pipeline auto-mejorable"""
    print("=" * 60)
    print("🚗 SELF-IMPROVING VEHICLE SEGMENTATION PIPELINE")
    print("=" * 60)
    
    # Crear pipeline
    pipeline = SelfImprovingPipeline()
    
    # Procesar imágenes de prueba
    input_dir = SCRIPT_DIR / "input"
    images = list(input_dir.glob("*.jpg"))
    
    print(f"\n📸 Procesando {len(images)} imágenes...")
    
    for img_path in images:
        result = pipeline.process(img_path)
        print(f"\n  {img_path.name}:")
        print(f"    ID: {result.image_id}")
        print(f"    Quality: {result.quality_score:.2f} ({result.quality_level.value})")
        print(f"    Confidence: {result.detection_confidence:.2f}")
        print(f"    Feedback: {result.feedback.value if result.feedback else 'pending'}")
        
    # Simular feedback
    print("\n📝 Simulando feedback de usuario...")
    
    # Mostrar reporte
    report = pipeline.get_performance_report()
    print(f"\n📊 REPORTE DE RENDIMIENTO:")
    print(f"   Versión actual: {report['current_version']}")
    print(f"   Muestras aprobadas: {report['approved_samples']}")
    print(f"   Muestras pendientes: {report['pending_samples']}")
    
    if report['performance_by_version']:
        for version, metrics in report['performance_by_version'].items():
            print(f"\n   Versión {version}:")
            print(f"     Total: {metrics['total']}")
            print(f"     Aprobación: {metrics['approval_rate']:.1f}%")
            print(f"     Calidad promedio: {metrics['avg_quality']:.2f}")


if __name__ == "__main__":
    main()
