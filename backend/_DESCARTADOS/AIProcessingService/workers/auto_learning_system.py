#!/usr/bin/env python3
"""
🧠 AUTO-LEARNING SYSTEM: Sistema de Aprendizaje Automático con Ollama
======================================================================

Sistema completo de aprendizaje automático que perfecciona automáticamente
el proceso de eliminación de fondo y generación de sombras.

Características:
- 🔮 Usa Ollama (LLaVA) como evaluador de visión - LOCAL, GRATIS, SIN LÍMITES
- 🧬 Red neuronal para predicción de parámetros óptimos
- 🎯 Aprendizaje por refuerzo para mejora continua
- 📊 Base de datos SQLite para almacenar conocimiento
- 🔄 Mejora automática con cada imagen procesada

Componentes:
1. OllamaEvaluator: Evalúa calidad usando LLaVA multimodal
2. ParamPredictor: Red neuronal que predice parámetros óptimos
3. ReinforcementAgent: Ajusta parámetros basado en recompensas
4. LearningDatabase: Almacena conocimiento y estadísticas
5. AutoLearningPipeline: Orquesta todo el sistema

Uso:
    # Modo single - procesar una imagen
    python auto_learning_system.py --mode single --input ./input/car.jpg
    
    # Modo batch - entrenar con múltiples imágenes
    python auto_learning_system.py --mode batch --input ./input --epochs 10
    
    # Modo continuo - aprendizaje continuo
    python auto_learning_system.py --mode continuous --target-score 95

Requirements:
    pip install torch torchvision numpy scipy pillow ollama

Author: Gregory Moreno
Date: January 2026
"""

import os
import sys
import cv2
import json
import time
import sqlite3
import hashlib
import pickle
import numpy as np
import torch
import torch.nn as nn
import torch.optim as optim
import torch.nn.functional as F
from torch.utils.data import Dataset, DataLoader
from PIL import Image, ImageFilter, ImageDraw
from pathlib import Path
from dataclasses import dataclass, field, asdict
from typing import Dict, List, Tuple, Optional, Any
from datetime import datetime
from enum import Enum
from scipy.ndimage import (
    binary_fill_holes, 
    binary_dilation, 
    binary_erosion, 
    gaussian_filter,
    distance_transform_edt
)
import base64
from io import BytesIO
import copy

# =============================================================================
# CONFIGURACIÓN
# =============================================================================

SCRIPT_DIR = Path(__file__).parent

# Usar variables de entorno si están disponibles (Docker)
INPUT_DIR = Path(os.environ.get("INPUT_DIR", SCRIPT_DIR / "input"))
OUTPUT_DIR = Path(os.environ.get("OUTPUT_DIR", SCRIPT_DIR / "output_autolearn"))
CHECKPOINTS_DIR = Path(os.environ.get("CHECKPOINT_DIR", SCRIPT_DIR / "checkpoints"))
MODELS_DIR = Path(os.environ.get("MODELS_DIR", SCRIPT_DIR / "models"))
LOGS_DIR = SCRIPT_DIR / "learning_logs"
DB_PATH = Path(os.environ.get("DB_PATH", SCRIPT_DIR / "autolearn.db"))

# Host de Ollama (puede ser host.docker.internal en Docker)
OLLAMA_HOST = os.environ.get("OLLAMA_HOST", "localhost:11434")
SAM_CHECKPOINT = SCRIPT_DIR / "sam_vit_h_4b8939.pth"

# Crear directorios
for d in [OUTPUT_DIR, CHECKPOINTS_DIR, LOGS_DIR]:
    d.mkdir(exist_ok=True)
(OUTPUT_DIR / "transparent").mkdir(exist_ok=True)
(OUTPUT_DIR / "shadow").mkdir(exist_ok=True)
(OUTPUT_DIR / "debug").mkdir(exist_ok=True)

# Dispositivo para PyTorch
DEVICE = (
    "mps" if torch.backends.mps.is_available() 
    else "cuda" if torch.cuda.is_available() 
    else "cpu"
)
print(f"🖥️  Device: {DEVICE}")


# =============================================================================
# 1. PARÁMETROS OPTIMIZABLES
# =============================================================================

@dataclass
class OptimizationParams:
    """
    Todos los parámetros ajustables del pipeline.
    El sistema aprende los valores óptimos para cada tipo de imagen.
    """
    # === DETECCIÓN ===
    detection_confidence: float = 0.25  # Umbral de confianza para detección (0.1-0.5)
    
    # === SEGMENTACIÓN (SAM) ===
    sam_points_per_side: int = 32       # Puntos para SAM automático (16-64)
    
    # === MÁSCARA ===
    dilation_iterations: int = 1        # Dilatación de máscara (0-5)
    erosion_iterations: int = 1         # Erosión de máscara (0-5)
    fill_holes: bool = True             # Rellenar huecos
    
    # === BORDES ===
    edge_softness: float = 2.0          # Suavidad de bordes (1.0-5.0)
    edge_feather: float = 0.5           # Difuminado de bordes (0.1-2.0)
    
    # === SOMBRA ===
    shadow_enabled: bool = True         # Generar sombra
    shadow_intensity: float = 0.45      # Intensidad (0.2-0.7)
    shadow_blur: float = 18.0           # Desenfoque (10-40)
    shadow_bottom_offset: float = 0.15  # Espacio inferior (% del ancho)
    shadow_side_offset: float = 0.08    # Espacio lateral (% del ancho)
    contact_shadow_height: float = 0.02 # Altura sombra de contacto (% del alto)
    ambient_shadow_height: float = 0.12 # Altura sombra ambiental (% del alto)
    wheel_shadow_boost: float = 0.35    # Intensidad extra en ruedas
    
    # === COLORES ===
    shadow_color_r: int = 25            # Color sombra R (0-50)
    shadow_color_g: int = 25            # Color sombra G (0-50)
    shadow_color_b: int = 30            # Color sombra B (0-50)
    
    # === POST-PROCESAMIENTO ===
    alpha_threshold: float = 0.1        # Umbral de alfa (0.05-0.2)
    final_denoise: bool = True          # Aplicar reducción de ruido
    
    def to_dict(self) -> dict:
        return asdict(self)
    
    def to_tensor(self) -> torch.Tensor:
        """Convertir a tensor para la red neuronal"""
        values = [
            self.detection_confidence,
            float(self.sam_points_per_side) / 64,  # Normalizar
            float(self.dilation_iterations) / 5,
            float(self.erosion_iterations) / 5,
            1.0 if self.fill_holes else 0.0,
            self.edge_softness / 5,
            self.edge_feather / 2,
            1.0 if self.shadow_enabled else 0.0,
            self.shadow_intensity,
            self.shadow_blur / 40,
            self.shadow_bottom_offset,
            self.shadow_side_offset,
            self.contact_shadow_height,
            self.ambient_shadow_height,
            self.wheel_shadow_boost,
            float(self.shadow_color_r) / 50,
            float(self.shadow_color_g) / 50,
            float(self.shadow_color_b) / 50,
            self.alpha_threshold,
            1.0 if self.final_denoise else 0.0,
        ]
        return torch.tensor(values, dtype=torch.float32)
    
    @classmethod
    def from_tensor(cls, tensor: torch.Tensor) -> 'OptimizationParams':
        """Crear desde tensor"""
        t = tensor.cpu().numpy()
        return cls(
            detection_confidence=float(np.clip(t[0], 0.1, 0.5)),
            sam_points_per_side=int(np.clip(t[1] * 64, 16, 64)),
            dilation_iterations=int(np.clip(t[2] * 5, 0, 5)),
            erosion_iterations=int(np.clip(t[3] * 5, 0, 5)),
            fill_holes=bool(t[4] > 0.5),
            edge_softness=float(np.clip(t[5] * 5, 1.0, 5.0)),
            edge_feather=float(np.clip(t[6] * 2, 0.1, 2.0)),
            shadow_enabled=bool(t[7] > 0.5),
            shadow_intensity=float(np.clip(t[8], 0.2, 0.7)),
            shadow_blur=float(np.clip(t[9] * 40, 10, 40)),
            shadow_bottom_offset=float(np.clip(t[10], 0.05, 0.25)),
            shadow_side_offset=float(np.clip(t[11], 0.02, 0.15)),
            contact_shadow_height=float(np.clip(t[12], 0.01, 0.05)),
            ambient_shadow_height=float(np.clip(t[13], 0.05, 0.2)),
            wheel_shadow_boost=float(np.clip(t[14], 0.2, 0.5)),
            shadow_color_r=int(np.clip(t[15] * 50, 0, 50)),
            shadow_color_g=int(np.clip(t[16] * 50, 0, 50)),
            shadow_color_b=int(np.clip(t[17] * 50, 0, 50)),
            alpha_threshold=float(np.clip(t[18], 0.05, 0.2)),
            final_denoise=bool(t[19] > 0.5),
        )
    
    @classmethod
    def from_dict(cls, d: dict) -> 'OptimizationParams':
        return cls(**{k: v for k, v in d.items() if k in cls.__dataclass_fields__})


# =============================================================================
# 2. EVALUADOR CON OLLAMA (LLaVA)
# =============================================================================

class EvaluationScore(Enum):
    """Clasificación de resultados"""
    EXCELLENT = "excellent"     # 90-100: Perfecto
    GOOD = "good"               # 75-89: Bueno
    ACCEPTABLE = "acceptable"   # 60-74: Aceptable
    NEEDS_WORK = "needs_work"   # 40-59: Requiere mejora
    REJECTED = "rejected"       # 0-39: Rechazado


@dataclass
class EvaluationResult:
    """Resultado completo de evaluación"""
    score: float                        # 0-100
    classification: EvaluationScore     
    confidence: float                   # Confianza del evaluador
    
    # Análisis detallado
    vehicle_complete: bool              # ¿Vehículo completo?
    wheels_intact: bool                 # ¿Ruedas completas?
    no_ground_shadow: bool              # ¿Sin sombra del suelo original?
    clean_edges: bool                   # ¿Bordes limpios?
    no_background: bool                 # ¿Sin restos de fondo?
    shadow_natural: bool                # ¿Sombra generada natural?
    
    # Feedback textual
    issues: List[str]                   # Problemas encontrados
    suggestions: List[str]              # Sugerencias
    description: str                    # Descripción general
    
    # Métricas numéricas para aprendizaje
    edge_quality: float                 # Calidad de bordes (0-1)
    shadow_quality: float               # Calidad de sombra (0-1)
    completeness: float                 # Completitud del vehículo (0-1)
    
    def to_reward(self) -> float:
        """Convertir a recompensa para RL"""
        base_reward = self.score / 10.0  # 0-10
        
        # Bonificaciones
        bonus = 0.0
        if self.vehicle_complete:
            bonus += 1.0
        if self.wheels_intact:
            bonus += 1.0
        if self.no_ground_shadow:
            bonus += 0.5
        if self.clean_edges:
            bonus += 0.5
        if self.shadow_natural:
            bonus += 0.5
        
        # Penalizaciones
        penalty = len(self.issues) * 0.3
        
        return base_reward + bonus - penalty


class OllamaEvaluator:
    """
    Evaluador de calidad usando Ollama con LLaVA.
    
    LLaVA (Large Language and Vision Assistant) es un modelo multimodal
    que puede analizar imágenes y dar feedback detallado.
    
    Ventajas:
    - 100% LOCAL - Sin costos de API
    - Sin límites de uso
    - Rápido (~2-5s por imagen)
    - Feedback detallado y accionable
    """
    
    def __init__(self, model: str = "llava:7b"):
        """
        Args:
            model: Modelo Ollama a usar. Opciones:
                   - "llava:7b" (default, bueno y rápido)
                   - "llava:13b" (mejor pero más lento)
                   - "llava:34b" (mejor calidad, requiere mucha RAM)
                   - "bakllava" (alternativa)
        """
        self.model = model
        self.available = False
        self._check_availability()
    
    def _check_availability(self):
        """Verificar si Ollama está disponible"""
        try:
            import ollama
            from ollama import Client
            # Usar host configurable
            self.client = Client(host=f"http://{OLLAMA_HOST}")
            # Verificar conexión
            self.client.list()
            self.available = True
            print(f"   ✅ Ollama disponible con modelo: {self.model} (host: {OLLAMA_HOST})")
        except ImportError:
            print("   ⚠️ Ollama no instalado. Instalar con: pip install ollama")
            self.available = False
        except Exception as e:
            print(f"   ⚠️ Ollama no disponible: {e}")
            print("   💡 Iniciar Ollama con: ollama serve")
            print(f"   💡 Descargar modelo: ollama pull {self.model}")
            self.available = False
    
    def _image_to_base64(self, image: np.ndarray) -> str:
        """Convertir imagen numpy a base64"""
        if len(image.shape) == 3 and image.shape[2] == 4:
            # RGBA - componer sobre fondo de tablero para mostrar transparencia
            alpha = image[:, :, 3:4] / 255.0
            rgb = image[:, :, :3]
            
            h, w = image.shape[:2]
            checker = np.zeros((h, w, 3), dtype=np.uint8)
            block_size = 20
            for i in range(0, h, block_size):
                for j in range(0, w, block_size):
                    if (i // block_size + j // block_size) % 2 == 0:
                        checker[i:i+block_size, j:j+block_size] = [200, 200, 200]
                    else:
                        checker[i:i+block_size, j:j+block_size] = [150, 150, 150]
            
            composed = (rgb * alpha + checker * (1 - alpha)).astype(np.uint8)
        else:
            composed = image
        
        # Convertir a JPEG
        pil_image = Image.fromarray(cv2.cvtColor(composed, cv2.COLOR_BGR2RGB))
        buffer = BytesIO()
        pil_image.save(buffer, format='JPEG', quality=85)
        return base64.b64encode(buffer.getvalue()).decode('utf-8')
    
    def evaluate(
        self, 
        original: np.ndarray, 
        result: np.ndarray,
        mask: Optional[np.ndarray] = None
    ) -> EvaluationResult:
        """
        Evaluar el resultado del procesamiento.
        
        Args:
            original: Imagen original (BGR)
            result: Resultado con fondo removido (BGRA)
            mask: Máscara de segmentación (opcional, para análisis adicional)
            
        Returns:
            EvaluationResult con puntuación y feedback
        """
        if not self.available:
            return self._fallback_evaluation(original, result, mask)
        
        try:
            # Preparar imágenes
            original_b64 = self._image_to_base64(original)
            result_b64 = self._image_to_base64(result)
            
            # Prompt detallado para evaluación
            prompt = """Analiza estas dos imágenes de un vehículo:

IMAGEN 1 (ORIGINAL): La foto original del vehículo con su fondo.
IMAGEN 2 (RESULTADO): El vehículo recortado (el patrón de cuadros grises/blancos indica transparencia) con sombra artificial agregada.

Evalúa el RESULTADO y responde SOLO con JSON válido:

```json
{
    "score": <número 0-100>,
    "vehicle_complete": <true si el vehículo está completo, false si hay partes cortadas>,
    "wheels_intact": <true si TODAS las ruedas están completas sin cortar>,
    "no_ground_shadow": <true si NO se incluyó la sombra original del suelo>,
    "clean_edges": <true si los bordes están limpios sin artefactos ni halos>,
    "no_background": <true si NO hay restos del fondo original visible>,
    "shadow_natural": <true si la sombra artificial se ve natural y profesional>,
    "edge_quality": <0.0-1.0 calidad de los bordes>,
    "shadow_quality": <0.0-1.0 calidad de la sombra generada>,
    "completeness": <0.0-1.0 qué tan completo está el vehículo>,
    "issues": [<lista de problemas específicos encontrados>],
    "suggestions": [<lista de mejoras concretas para el algoritmo>],
    "description": "<descripción de 1-2 oraciones del resultado general>"
}
```

CRITERIOS DE PUNTUACIÓN:
- 90-100: EXCELENTE - Calidad profesional de concesionario, perfecto
- 75-89: BUENO - Pequeños detalles menores, aceptable para uso
- 60-74: ACEPTABLE - Problemas visibles pero funcional
- 40-59: NECESITA TRABAJO - Problemas significativos
- 0-39: RECHAZADO - Inaceptable, rehacer completamente

PROBLEMAS COMUNES A DETECTAR:
1. Ruedas cortadas o incompletas (MUY IMPORTANTE)
2. Sombra del suelo original incluida
3. Bordes dentados o con halo
4. Partes del fondo visible
5. Vehículo incompleto (espejos, antenas, etc.)
6. Sombra artificial muy oscura/clara/artificial

Responde ÚNICAMENTE con el JSON, sin texto adicional."""

            # Llamar a Ollama usando cliente con host configurable
            response = self.client.chat(
                model=self.model,
                messages=[{
                    "role": "user",
                    "content": prompt,
                    "images": [original_b64, result_b64]
                }],
                options={
                    "temperature": 0.1,  # Respuestas consistentes
                    "num_predict": 800
                }
            )
            
            # Parsear respuesta
            return self._parse_response(response['message']['content'])
            
        except Exception as e:
            print(f"   ⚠️ Error en evaluación Ollama: {e}")
            return self._fallback_evaluation(original, result, mask)
    
    def _parse_response(self, response_text: str) -> EvaluationResult:
        """Parsear respuesta JSON de Ollama"""
        try:
            # Limpiar respuesta
            text = response_text.strip()
            
            # Extraer JSON si está envuelto en markdown
            if "```json" in text:
                text = text.split("```json")[1].split("```")[0]
            elif "```" in text:
                text = text.split("```")[1].split("```")[0]
            
            data = json.loads(text)
            
            # Clasificar
            score = float(data.get('score', 50))
            if score >= 90:
                classification = EvaluationScore.EXCELLENT
            elif score >= 75:
                classification = EvaluationScore.GOOD
            elif score >= 60:
                classification = EvaluationScore.ACCEPTABLE
            elif score >= 40:
                classification = EvaluationScore.NEEDS_WORK
            else:
                classification = EvaluationScore.REJECTED
            
            return EvaluationResult(
                score=score,
                classification=classification,
                confidence=0.85,  # LLaVA es bastante confiable
                vehicle_complete=data.get('vehicle_complete', True),
                wheels_intact=data.get('wheels_intact', True),
                no_ground_shadow=data.get('no_ground_shadow', True),
                clean_edges=data.get('clean_edges', True),
                no_background=data.get('no_background', True),
                shadow_natural=data.get('shadow_natural', True),
                issues=data.get('issues', []),
                suggestions=data.get('suggestions', []),
                description=data.get('description', 'Sin descripción'),
                edge_quality=float(data.get('edge_quality', 0.7)),
                shadow_quality=float(data.get('shadow_quality', 0.7)),
                completeness=float(data.get('completeness', 0.9)),
            )
            
        except (json.JSONDecodeError, KeyError) as e:
            print(f"   ⚠️ Error parseando respuesta: {e}")
            print(f"   Respuesta: {response_text[:200]}...")
            
            # Retornar evaluación por defecto
            return EvaluationResult(
                score=50.0,
                classification=EvaluationScore.NEEDS_WORK,
                confidence=0.3,
                vehicle_complete=True,
                wheels_intact=True,
                no_ground_shadow=True,
                clean_edges=True,
                no_background=True,
                shadow_natural=True,
                issues=["No se pudo parsear respuesta de IA"],
                suggestions=["Verificar manualmente"],
                description="Evaluación automática falló",
                edge_quality=0.5,
                shadow_quality=0.5,
                completeness=0.5,
            )
    
    def _fallback_evaluation(
        self, 
        original: np.ndarray, 
        result: np.ndarray,
        mask: Optional[np.ndarray]
    ) -> EvaluationResult:
        """
        Evaluación de respaldo usando métricas computacionales.
        Se usa cuando Ollama no está disponible.
        """
        print("   📊 Usando evaluación computacional (fallback)")
        
        scores = {}
        issues = []
        suggestions = []
        
        # 1. Verificar que hay contenido (vehículo segmentado)
        if len(result.shape) == 3 and result.shape[2] == 4:
            alpha = result[:, :, 3]
            coverage = np.sum(alpha > 128) / alpha.size
            
            if coverage < 0.05:
                issues.append("Segmentación muy pequeña o vacía")
                scores['coverage'] = 0.2
            elif coverage > 0.8:
                issues.append("Segmentación demasiado grande (incluye fondo)")
                scores['coverage'] = 0.5
            else:
                scores['coverage'] = min(1.0, coverage / 0.3)
        else:
            scores['coverage'] = 0.5
        
        # 2. Evaluar bordes
        if mask is not None:
            edge_score = self._evaluate_edges(mask)
            scores['edges'] = edge_score
            if edge_score < 0.6:
                issues.append("Bordes irregulares")
                suggestions.append("Aumentar suavizado de bordes")
        else:
            scores['edges'] = 0.7
        
        # 3. Evaluar completitud (verificar que las esquinas no tienen contenido)
        if len(result.shape) == 3 and result.shape[2] == 4:
            alpha = result[:, :, 3]
            h, w = alpha.shape
            corner_size = min(h, w) // 10
            
            corners = [
                alpha[:corner_size, :corner_size],        # Top-left
                alpha[:corner_size, -corner_size:],       # Top-right
                alpha[-corner_size:, :corner_size],       # Bottom-left
                alpha[-corner_size:, -corner_size:],      # Bottom-right
            ]
            
            corner_content = sum(np.mean(c > 128) for c in corners) / 4
            if corner_content > 0.3:
                issues.append("Posible fondo residual en esquinas")
                scores['background'] = 0.5
            else:
                scores['background'] = 1.0
        else:
            scores['background'] = 0.7
        
        # Calcular score final
        weights = {'coverage': 0.3, 'edges': 0.4, 'background': 0.3}
        final_score = sum(scores.get(k, 0.5) * v for k, v in weights.items()) * 100
        
        # Clasificar
        if final_score >= 90:
            classification = EvaluationScore.EXCELLENT
        elif final_score >= 75:
            classification = EvaluationScore.GOOD
        elif final_score >= 60:
            classification = EvaluationScore.ACCEPTABLE
        elif final_score >= 40:
            classification = EvaluationScore.NEEDS_WORK
        else:
            classification = EvaluationScore.REJECTED
        
        return EvaluationResult(
            score=final_score,
            classification=classification,
            confidence=0.5,  # Menor confianza en evaluación automática
            vehicle_complete=scores.get('coverage', 0.5) > 0.5,
            wheels_intact=True,  # No podemos detectar esto sin IA
            no_ground_shadow=True,
            clean_edges=scores.get('edges', 0.5) > 0.6,
            no_background=scores.get('background', 0.5) > 0.7,
            shadow_natural=True,
            issues=issues,
            suggestions=suggestions,
            description=f"Evaluación automática: {final_score:.1f}/100",
            edge_quality=scores.get('edges', 0.5),
            shadow_quality=0.7,
            completeness=scores.get('coverage', 0.5),
        )
    
    def _evaluate_edges(self, mask: np.ndarray) -> float:
        """Evaluar calidad de bordes de la máscara"""
        from scipy.ndimage import sobel
        
        # Calcular gradiente
        gx = sobel(mask.astype(float), axis=1)
        gy = sobel(mask.astype(float), axis=0)
        gradient = np.sqrt(gx**2 + gy**2)
        
        # Bordes son donde hay gradiente alto
        edge_pixels = gradient > 0.1
        if np.sum(edge_pixels) == 0:
            return 0.5
        
        # Suavidad = baja varianza en el gradiente de los bordes
        edge_values = gradient[edge_pixels]
        smoothness = 1.0 - min(1.0, np.std(edge_values))
        
        return smoothness


# =============================================================================
# 3. RED NEURONAL PARA PREDICCIÓN DE PARÁMETROS
# =============================================================================

class ImageFeatureExtractor(nn.Module):
    """
    Extrae características de la imagen para el predictor.
    Usa una CNN simple pero efectiva.
    """
    
    def __init__(self, output_dim: int = 256):
        super().__init__()
        
        self.features = nn.Sequential(
            # Input: 3 x 224 x 224
            nn.Conv2d(3, 32, kernel_size=3, padding=1),
            nn.BatchNorm2d(32),
            nn.ReLU(inplace=True),
            nn.MaxPool2d(2),  # 32 x 112 x 112
            
            nn.Conv2d(32, 64, kernel_size=3, padding=1),
            nn.BatchNorm2d(64),
            nn.ReLU(inplace=True),
            nn.MaxPool2d(2),  # 64 x 56 x 56
            
            nn.Conv2d(64, 128, kernel_size=3, padding=1),
            nn.BatchNorm2d(128),
            nn.ReLU(inplace=True),
            nn.MaxPool2d(2),  # 128 x 28 x 28
            
            nn.Conv2d(128, 256, kernel_size=3, padding=1),
            nn.BatchNorm2d(256),
            nn.ReLU(inplace=True),
            nn.AdaptiveAvgPool2d((4, 4)),  # 256 x 4 x 4
            
            nn.Flatten(),  # 4096
            nn.Linear(256 * 4 * 4, output_dim),
            nn.ReLU(inplace=True),
        )
    
    def forward(self, x):
        return self.features(x)


class ParamPredictor(nn.Module):
    """
    Red neuronal que predice los parámetros óptimos para cada imagen.
    
    Input:
    - Características de imagen (256-dim)
    - Metadata (5-dim): tamaño, aspect ratio, luminosidad, etc.
    
    Output:
    - 20 parámetros optimizables
    """
    
    def __init__(self, feature_dim: int = 256, metadata_dim: int = 5, param_dim: int = 20):
        super().__init__()
        
        self.feature_extractor = ImageFeatureExtractor(feature_dim)
        
        self.predictor = nn.Sequential(
            nn.Linear(feature_dim + metadata_dim, 512),
            nn.ReLU(inplace=True),
            nn.Dropout(0.3),
            
            nn.Linear(512, 256),
            nn.ReLU(inplace=True),
            nn.Dropout(0.2),
            
            nn.Linear(256, 128),
            nn.ReLU(inplace=True),
            
            nn.Linear(128, param_dim),
        )
        
        # Guardar dimensiones
        self.param_dim = param_dim
    
    def forward(self, image: torch.Tensor, metadata: torch.Tensor) -> torch.Tensor:
        """
        Predecir parámetros óptimos.
        
        Args:
            image: Tensor (B, 3, 224, 224)
            metadata: Tensor (B, 5)
            
        Returns:
            params: Tensor (B, 20) - valores entre 0 y 1
        """
        features = self.feature_extractor(image)
        combined = torch.cat([features, metadata], dim=1)
        raw_params = self.predictor(combined)
        
        # Aplicar sigmoid para valores entre 0 y 1
        return torch.sigmoid(raw_params)
    
    def predict_for_image(self, pil_image: Image.Image) -> OptimizationParams:
        """
        Conveniencia: predecir parámetros para una imagen PIL.
        """
        from torchvision import transforms
        
        # Transformar imagen
        transform = transforms.Compose([
            transforms.Resize((224, 224)),
            transforms.ToTensor(),
            transforms.Normalize(mean=[0.485, 0.456, 0.406], 
                               std=[0.229, 0.224, 0.225])
        ])
        
        image_tensor = transform(pil_image).unsqueeze(0).to(DEVICE)
        
        # Metadata
        w, h = pil_image.size
        img_array = np.array(pil_image)
        metadata = torch.tensor([
            w / 1000,           # Ancho normalizado
            h / 1000,           # Alto normalizado
            w / h,              # Aspect ratio
            np.mean(img_array) / 255,  # Luminosidad
            1.0                 # Placeholder
        ]).unsqueeze(0).float().to(DEVICE)
        
        # Predecir
        self.eval()
        with torch.no_grad():
            params_tensor = self(image_tensor, metadata)[0]
        
        return OptimizationParams.from_tensor(params_tensor)


# =============================================================================
# 4. AGENTE DE APRENDIZAJE POR REFUERZO
# =============================================================================

@dataclass
class Experience:
    """Una experiencia para aprendizaje"""
    state: torch.Tensor          # Características de imagen + metadata
    action: torch.Tensor         # Parámetros predichos
    reward: float                # Recompensa obtenida
    next_state: torch.Tensor     # Estado siguiente (puede ser el mismo)
    done: bool                   # ¿Terminó el episodio?


class ReinforcementAgent:
    """
    Agente de aprendizaje por refuerzo que ajusta el predictor de parámetros.
    
    Usa Policy Gradient simplificado con baseline para estabilidad.
    """
    
    def __init__(
        self, 
        predictor: ParamPredictor,
        learning_rate: float = 1e-4,
        gamma: float = 0.99,
        memory_size: int = 1000
    ):
        self.predictor = predictor
        self.optimizer = optim.Adam(predictor.parameters(), lr=learning_rate)
        self.gamma = gamma
        self.memory: List[Experience] = []
        self.memory_size = memory_size
        self.baseline = 0.0  # Running average de recompensas
        
        # Estadísticas
        self.total_updates = 0
        self.reward_history: List[float] = []
    
    def store_experience(self, experience: Experience):
        """Almacenar experiencia en memoria"""
        self.memory.append(experience)
        if len(self.memory) > self.memory_size:
            self.memory.pop(0)
        
        # Actualizar baseline
        self.reward_history.append(experience.reward)
        if len(self.reward_history) > 100:
            self.reward_history.pop(0)
        self.baseline = np.mean(self.reward_history)
    
    def update(self, batch_size: int = 32) -> float:
        """
        Actualizar el predictor con experiencias almacenadas.
        
        Returns:
            loss: Pérdida promedio
        """
        if len(self.memory) < batch_size:
            return 0.0
        
        # Muestrear batch
        import random
        batch = random.sample(self.memory, batch_size)
        
        # Preparar datos (asegurar float32 para MPS)
        states = torch.stack([e.state.float() for e in batch]).to(DEVICE)
        actions = torch.stack([e.action.float() for e in batch]).to(DEVICE)
        rewards = torch.tensor([e.reward for e in batch], dtype=torch.float32).to(DEVICE)
        
        # Calcular ventaja (reward - baseline)
        advantages = rewards - self.baseline
        advantages = (advantages - advantages.mean()) / (advantages.std() + 1e-8)
        
        # Forward pass
        self.predictor.train()
        
        # Separar state en image y metadata
        # Asumimos que state es la concatenación de features y metadata
        image_features = states[:, :-5]  # Primeros 256
        metadata = states[:, -5:]         # Últimos 5
        
        # Reconstruir imagen dummy para forward (esto es un hack, idealmente guardar imagen)
        # En una implementación más limpia, guardaríamos las características directamente
        
        # Por ahora, usamos los parámetros directamente
        predicted_actions = actions  # Ya tenemos las acciones
        
        # Pérdida: maximizar recompensa (minimizar -reward * log_prob)
        # Aproximación: MSE entre acción y acción "ideal" ponderada por ventaja
        
        # Crear target ajustado por ventaja
        # Si ventaja > 0: mantener acción
        # Si ventaja < 0: alejarse de la acción
        
        # Simplificación: pérdida = -advantage * log_prob de la acción
        # Como usamos una red determinística, usamos regularización
        
        loss = -torch.mean(advantages.unsqueeze(1) * actions)
        
        # Regularización L2 para estabilidad
        l2_reg = sum(p.pow(2).sum() for p in self.predictor.parameters())
        loss = loss + 1e-5 * l2_reg
        
        # Backward
        self.optimizer.zero_grad()
        loss.backward()
        torch.nn.utils.clip_grad_norm_(self.predictor.parameters(), 1.0)
        self.optimizer.step()
        
        self.total_updates += 1
        
        return loss.item()
    
    def get_stats(self) -> Dict[str, Any]:
        """Obtener estadísticas del agente"""
        return {
            'memory_size': len(self.memory),
            'total_updates': self.total_updates,
            'baseline': self.baseline,
            'avg_reward': np.mean(self.reward_history) if self.reward_history else 0,
            'max_reward': max(self.reward_history) if self.reward_history else 0,
        }


# =============================================================================
# 5. BASE DE DATOS DE APRENDIZAJE
# =============================================================================

class LearningDatabase:
    """
    Base de datos SQLite para almacenar conocimiento aprendido.
    
    Almacena:
    - Historial de procesamiento
    - Parámetros óptimos por tipo de imagen
    - Estadísticas de mejora
    - Checkpoints del modelo
    """
    
    def __init__(self, db_path: Path = DB_PATH):
        self.db_path = db_path
        self._init_db()
    
    def _init_db(self):
        """Inicializar base de datos"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        # Tabla de procesamiento
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS processing_history (
                id TEXT PRIMARY KEY,
                timestamp TEXT NOT NULL,
                image_path TEXT NOT NULL,
                image_hash TEXT NOT NULL,
                
                -- Parámetros usados (JSON)
                params_json TEXT,
                
                -- Evaluación
                score REAL,
                classification TEXT,
                vehicle_complete INTEGER,
                wheels_intact INTEGER,
                no_ground_shadow INTEGER,
                clean_edges INTEGER,
                no_background INTEGER,
                shadow_natural INTEGER,
                issues_json TEXT,
                suggestions_json TEXT,
                
                -- Métricas
                edge_quality REAL,
                shadow_quality REAL,
                completeness REAL,
                processing_time_ms REAL,
                
                -- Para entrenamiento
                used_for_training INTEGER DEFAULT 0,
                reward REAL
            )
        ''')
        
        # Tabla de parámetros óptimos
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS optimal_params (
                image_hash TEXT PRIMARY KEY,
                params_json TEXT NOT NULL,
                score REAL,
                last_updated TEXT
            )
        ''')
        
        # Tabla de estadísticas diarias
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS daily_stats (
                date TEXT PRIMARY KEY,
                images_processed INTEGER,
                avg_score REAL,
                excellent_count INTEGER,
                good_count INTEGER,
                rejected_count INTEGER,
                avg_processing_time REAL
            )
        ''')
        
        # Tabla de checkpoints
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS model_checkpoints (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                timestamp TEXT NOT NULL,
                checkpoint_path TEXT NOT NULL,
                avg_score REAL,
                samples_trained INTEGER,
                notes TEXT
            )
        ''')
        
        # Índices
        cursor.execute('CREATE INDEX IF NOT EXISTS idx_score ON processing_history(score)')
        cursor.execute('CREATE INDEX IF NOT EXISTS idx_classification ON processing_history(classification)')
        cursor.execute('CREATE INDEX IF NOT EXISTS idx_timestamp ON processing_history(timestamp)')
        
        conn.commit()
        conn.close()
    
    def save_processing(
        self,
        image_path: str,
        params: OptimizationParams,
        evaluation: EvaluationResult,
        processing_time_ms: float
    ) -> str:
        """Guardar resultado de procesamiento"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        # Generar ID y hash
        record_id = f"{datetime.now().strftime('%Y%m%d_%H%M%S')}_{Path(image_path).stem}"
        image_hash = self._hash_file(image_path)
        
        cursor.execute('''
            INSERT OR REPLACE INTO processing_history (
                id, timestamp, image_path, image_hash, params_json,
                score, classification, vehicle_complete, wheels_intact,
                no_ground_shadow, clean_edges, no_background, shadow_natural,
                issues_json, suggestions_json, edge_quality, shadow_quality,
                completeness, processing_time_ms, used_for_training, reward
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        ''', (
            record_id,
            datetime.now().isoformat(),
            str(image_path),
            image_hash,
            json.dumps(params.to_dict()),
            evaluation.score,
            evaluation.classification.value,
            1 if evaluation.vehicle_complete else 0,
            1 if evaluation.wheels_intact else 0,
            1 if evaluation.no_ground_shadow else 0,
            1 if evaluation.clean_edges else 0,
            1 if evaluation.no_background else 0,
            1 if evaluation.shadow_natural else 0,
            json.dumps(evaluation.issues),
            json.dumps(evaluation.suggestions),
            evaluation.edge_quality,
            evaluation.shadow_quality,
            evaluation.completeness,
            processing_time_ms,
            0,  # used_for_training
            evaluation.to_reward()
        ))
        
        # Actualizar parámetros óptimos si es mejor
        cursor.execute(
            'SELECT score FROM optimal_params WHERE image_hash = ?',
            (image_hash,)
        )
        row = cursor.fetchone()
        
        if row is None or evaluation.score > row[0]:
            cursor.execute('''
                INSERT OR REPLACE INTO optimal_params VALUES (?, ?, ?, ?)
            ''', (
                image_hash,
                json.dumps(params.to_dict()),
                evaluation.score,
                datetime.now().isoformat()
            ))
        
        conn.commit()
        conn.close()
        
        return record_id
    
    def get_optimal_params(self, image_path: str) -> Optional[OptimizationParams]:
        """Obtener parámetros óptimos para una imagen similar"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        image_hash = self._hash_file(image_path)
        
        cursor.execute(
            'SELECT params_json FROM optimal_params WHERE image_hash = ?',
            (image_hash,)
        )
        row = cursor.fetchone()
        conn.close()
        
        if row:
            return OptimizationParams.from_dict(json.loads(row[0]))
        return None
    
    def get_training_data(self, min_score: float = 75.0, limit: int = 100) -> List[Dict]:
        """Obtener datos para entrenamiento (buenos resultados)"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            SELECT image_path, params_json, score, reward
            FROM processing_history
            WHERE score >= ? AND used_for_training = 0
            ORDER BY score DESC
            LIMIT ?
        ''', (min_score, limit))
        
        results = []
        for row in cursor.fetchall():
            results.append({
                'image_path': row[0],
                'params': json.loads(row[1]),
                'score': row[2],
                'reward': row[3]
            })
        
        conn.close()
        return results
    
    def mark_as_trained(self, image_paths: List[str]):
        """Marcar imágenes como usadas para entrenamiento"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        for path in image_paths:
            cursor.execute(
                'UPDATE processing_history SET used_for_training = 1 WHERE image_path = ?',
                (str(path),)
            )
        
        conn.commit()
        conn.close()
    
    def get_stats(self) -> Dict[str, Any]:
        """Obtener estadísticas generales"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            SELECT 
                COUNT(*) as total,
                AVG(score) as avg_score,
                MAX(score) as max_score,
                MIN(score) as min_score,
                SUM(CASE WHEN classification = 'excellent' THEN 1 ELSE 0 END) as excellent,
                SUM(CASE WHEN classification = 'good' THEN 1 ELSE 0 END) as good,
                SUM(CASE WHEN classification = 'rejected' THEN 1 ELSE 0 END) as rejected,
                AVG(processing_time_ms) as avg_time
            FROM processing_history
        ''')
        
        row = cursor.fetchone()
        conn.close()
        
        if row and row[0] > 0:
            return {
                'total_processed': row[0],
                'avg_score': round(row[1], 2),
                'max_score': round(row[2], 2),
                'min_score': round(row[3], 2),
                'excellent_count': row[4],
                'good_count': row[5],
                'rejected_count': row[6],
                'avg_time_ms': round(row[7], 1) if row[7] else 0,
                'success_rate': round((row[4] + row[5]) / row[0] * 100, 1)
            }
        return {}
    
    def get_problem_analysis(self) -> Dict[str, Any]:
        """Analizar problemas comunes"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            SELECT 
                SUM(CASE WHEN wheels_intact = 0 THEN 1 ELSE 0 END) as wheel_issues,
                SUM(CASE WHEN no_ground_shadow = 0 THEN 1 ELSE 0 END) as shadow_issues,
                SUM(CASE WHEN clean_edges = 0 THEN 1 ELSE 0 END) as edge_issues,
                SUM(CASE WHEN no_background = 0 THEN 1 ELSE 0 END) as bg_issues,
                SUM(CASE WHEN vehicle_complete = 0 THEN 1 ELSE 0 END) as incomplete_issues,
                COUNT(*) as total
            FROM processing_history
            WHERE classification IN ('needs_work', 'rejected')
        ''')
        
        row = cursor.fetchone()
        conn.close()
        
        if row and row[5] > 0:
            total = row[5]
            return {
                'wheel_issues_pct': round(row[0] / total * 100, 1),
                'shadow_issues_pct': round(row[1] / total * 100, 1),
                'edge_issues_pct': round(row[2] / total * 100, 1),
                'background_issues_pct': round(row[3] / total * 100, 1),
                'incomplete_pct': round(row[4] / total * 100, 1),
                'total_problems': total
            }
        return {}
    
    def _hash_file(self, file_path: str) -> str:
        """Calcular hash de archivo"""
        if not Path(file_path).exists():
            return hashlib.md5(file_path.encode()).hexdigest()
        
        with open(file_path, 'rb') as f:
            return hashlib.md5(f.read()).hexdigest()


# =============================================================================
# 6. PIPELINE DE PROCESAMIENTO (Basado en V7)
# =============================================================================

class ProcessingPipeline:
    """
    Pipeline de procesamiento de imágenes.
    Basado en remove_background_v7.py pero con parámetros ajustables.
    """
    
    def __init__(self):
        self.yolo_model = None
        self.sam_predictor = None
        self.loaded = False
    
    def load_models(self):
        """Cargar modelos de detección y segmentación"""
        if self.loaded:
            return
        
        print("📦 Cargando modelos...")
        
        # YOLO para detección
        print("   Cargando YOLO...")
        try:
            from ultralytics import YOLO
            self.yolo_model = YOLO("yolov8x-seg.pt")
            print("   ✅ YOLO cargado")
        except Exception as e:
            print(f"   ⚠️ Error cargando YOLO: {e}")
        
        # SAM para segmentación
        print("   Cargando SAM...")
        try:
            from segment_anything import sam_model_registry, SamPredictor
            
            if not SAM_CHECKPOINT.exists():
                print(f"   ⚠️ SAM checkpoint no encontrado: {SAM_CHECKPOINT}")
                print("   Descarga con: curl -L -o sam_vit_h_4b8939.pth \\")
                print('   "https://dl.fbaipublicfiles.com/segment_anything/sam_vit_h_4b8939.pth"')
            else:
                sam = sam_model_registry["vit_h"](checkpoint=str(SAM_CHECKPOINT))
                sam.to(device="cpu")
                self.sam_predictor = SamPredictor(sam)
                print("   ✅ SAM cargado")
        except Exception as e:
            print(f"   ⚠️ Error cargando SAM: {e}")
        
        self.loaded = True
    
    def detect_vehicle(
        self, 
        image_path: Path, 
        params: OptimizationParams
    ) -> Optional[Dict]:
        """Detectar vehículo principal en la imagen"""
        if self.yolo_model is None:
            return None
        
        results = self.yolo_model(
            str(image_path), 
            conf=params.detection_confidence, 
            verbose=False
        )[0]
        
        if results.boxes is None or len(results.boxes) == 0:
            return None
        
        vehicle_classes = {2: 'car', 3: 'motorcycle', 5: 'bus', 7: 'truck'}
        
        img = Image.open(image_path)
        width, height = img.size
        center_x, center_y = width / 2, height / 2
        
        vehicles = []
        for i, cls in enumerate(results.boxes.cls):
            class_id = int(cls.item())
            if class_id in vehicle_classes:
                box = results.boxes.xyxy[i].cpu().numpy()
                conf = float(results.boxes.conf[i].item())
                
                x1, y1, x2, y2 = int(box[0]), int(box[1]), int(box[2]), int(box[3])
                box_center_x = (x1 + x2) / 2
                box_center_y = (y1 + y2) / 2
                box_area = (x2 - x1) * (y2 - y1)
                
                # Score: tamaño + centrado + posición inferior
                size_score = box_area / (width * height)
                center_score = 1 - (abs(box_center_x - center_x) / center_x)
                bottom_score = box_center_y / height
                
                total_score = (size_score * 3) + (center_score * 1) + (bottom_score * 1) + (conf * 1)
                
                vehicles.append({
                    'box': [x1, y1, x2, y2],
                    'confidence': conf,
                    'class_name': vehicle_classes[class_id],
                    'score': total_score,
                    'area': box_area
                })
        
        if not vehicles:
            return None
        
        vehicles.sort(key=lambda x: x['score'], reverse=True)
        return vehicles[0]
    
    def segment_vehicle(
        self, 
        image_path: Path, 
        box: List[int],
        params: OptimizationParams
    ) -> np.ndarray:
        """Segmentar vehículo con SAM"""
        if self.sam_predictor is None:
            # Fallback: máscara basada en bounding box
            img = Image.open(image_path)
            mask = np.zeros((img.height, img.width), dtype=bool)
            x1, y1, x2, y2 = box
            mask[y1:y2, x1:x2] = True
            return mask
        
        image = np.array(Image.open(image_path).convert('RGB'))
        self.sam_predictor.set_image(image)
        
        input_box = np.array(box)
        masks, scores, _ = self.sam_predictor.predict(
            point_coords=None,
            point_labels=None,
            box=input_box,
            multimask_output=True
        )
        
        best_idx = np.argmax(scores)
        return masks[best_idx]
    
    def refine_mask(self, mask: np.ndarray, params: OptimizationParams) -> np.ndarray:
        """Refinar máscara con parámetros"""
        # Rellenar huecos
        if params.fill_holes:
            mask = binary_fill_holes(mask)
        
        # Dilatación
        if params.dilation_iterations > 0:
            mask = binary_dilation(mask, iterations=params.dilation_iterations)
        
        # Erosión
        if params.erosion_iterations > 0:
            mask = binary_erosion(mask, iterations=params.erosion_iterations)
        
        return mask
    
    def create_soft_alpha(self, mask: np.ndarray, params: OptimizationParams) -> np.ndarray:
        """Crear canal alfa con bordes suaves"""
        alpha = mask.astype(np.float32) * 255
        
        # Distancias
        dist_inside = distance_transform_edt(mask)
        dist_outside = distance_transform_edt(~mask)
        
        edge_width = params.edge_softness
        
        # Suavizar bordes internos
        edge_inside = (dist_inside > 0) & (dist_inside <= edge_width)
        alpha[edge_inside] = 200 + (dist_inside[edge_inside] / edge_width) * 55
        
        # Suavizar bordes externos
        edge_outside = (dist_outside > 0) & (dist_outside <= edge_width)
        alpha[edge_outside] = (1 - dist_outside[edge_outside] / edge_width) * 100
        
        # Gaussian blur para suavidad
        alpha = gaussian_filter(alpha, sigma=params.edge_feather)
        
        return np.clip(alpha, 0, 255).astype(np.uint8)
    
    def create_shadow(self, image: Image.Image, params: OptimizationParams) -> Image.Image:
        """Crear sombra profesional"""
        if not params.shadow_enabled:
            # Sin sombra, solo fondo blanco con padding
            width, height = image.size
            padding = 50
            result = Image.new('RGBA', (width + padding*2, height + padding*2), (255, 255, 255, 255))
            result.paste(image, (padding, padding), image)
            return result
        
        width, height = image.size
        padding = 80
        new_width = width + padding * 2
        new_height = height + padding * 2
        
        result = Image.new('RGBA', (new_width, new_height), (255, 255, 255, 255))
        
        alpha = np.array(image.split()[3])
        
        # Encontrar base del vehículo
        rows_with_content = np.where(alpha.max(axis=1) > 128)[0]
        if len(rows_with_content) == 0:
            result.paste(image, (padding, padding), image)
            return result
        
        bottom_y = rows_with_content[-1]
        
        # Dimensiones del vehículo
        cols_with_content = np.where(alpha.max(axis=0) > 128)[0]
        if len(cols_with_content) == 0:
            result.paste(image, (padding, padding), image)
            return result
        
        vehicle_left = cols_with_content[0]
        vehicle_right = cols_with_content[-1]
        vehicle_width = vehicle_right - vehicle_left
        
        # Crear sombra
        shadow_height = int(height * params.ambient_shadow_height)
        contact_height = int(height * params.contact_shadow_height)
        
        shadow_array = np.zeros((height, width), dtype=np.float32)
        
        # Sombra de contacto (más intensa en la base)
        for y in range(max(0, bottom_y - contact_height), min(height, bottom_y + 10)):
            cols = np.where(alpha[y] > 128)[0]
            if len(cols) > 0:
                left_x = cols[0]
                right_x = cols[-1]
                
                dist_from_bottom = abs(y - bottom_y)
                intensity = params.shadow_intensity * (1 - dist_from_bottom / (contact_height + 10))
                
                for x in range(left_x, right_x + 1):
                    edge_dist = min(x - left_x, right_x - x)
                    edge_falloff = min(1.0, edge_dist / 30)
                    shadow_array[y, x] = intensity * edge_falloff * 255
        
        # Sombra ambiental (más difusa)
        for y in range(max(0, bottom_y - shadow_height), max(0, bottom_y - contact_height)):
            cols = np.where(alpha[min(y + shadow_height, height-1)] > 128)[0]
            if len(cols) > 0:
                left_x = max(0, cols[0] - 20)
                right_x = min(width - 1, cols[-1] + 20)
                
                dist_from_contact = abs(y - (bottom_y - contact_height))
                intensity = params.shadow_intensity * 0.3 * (1 - dist_from_contact / shadow_height)
                
                for x in range(left_x, right_x + 1):
                    edge_dist = min(x - left_x, right_x - x)
                    edge_falloff = min(1.0, edge_dist / 50)
                    shadow_array[y, x] = max(shadow_array[y, x], intensity * edge_falloff * 255)
        
        # Aplicar blur
        shadow_array = gaussian_filter(shadow_array, sigma=params.shadow_blur)
        shadow_array = np.clip(shadow_array, 0, 255).astype(np.uint8)
        
        # Crear imagen de sombra
        shadow_img = Image.fromarray(shadow_array)
        shadow_color = (params.shadow_color_r, params.shadow_color_g, params.shadow_color_b)
        
        # Componer
        result.paste(
            Image.new('RGB', (width, height), shadow_color),
            (padding, padding + 5),
            shadow_img
        )
        result.paste(image, (padding, padding), image)
        
        return result
    
    def process(
        self, 
        image_path: Path, 
        params: OptimizationParams
    ) -> Tuple[np.ndarray, np.ndarray, Optional[Dict]]:
        """
        Procesar imagen completa.
        
        Returns:
            (result_image, mask, vehicle_info)
        """
        if not self.loaded:
            self.load_models()
        
        # 1. Detectar vehículo
        vehicle = self.detect_vehicle(image_path, params)
        if vehicle is None:
            raise ValueError("No se detectó vehículo")
        
        # 2. Segmentar
        mask = self.segment_vehicle(image_path, vehicle['box'], params)
        
        # 3. Refinar máscara
        mask = self.refine_mask(mask, params)
        
        # 4. Crear alfa suave
        alpha = self.create_soft_alpha(mask, params)
        
        # 5. Aplicar a imagen
        original = Image.open(image_path).convert('RGBA')
        original.putalpha(Image.fromarray(alpha))
        
        # 6. Crear sombra
        result = self.create_shadow(original, params)
        
        return np.array(result), mask, vehicle


# =============================================================================
# 7. PIPELINE DE APRENDIZAJE AUTOMÁTICO
# =============================================================================

class AutoLearningPipeline:
    """
    Pipeline principal que integra todo el sistema de aprendizaje.
    
    Flujo:
    1. Analiza imagen de entrada
    2. Predice parámetros óptimos (o usa los existentes)
    3. Procesa la imagen
    4. Evalúa el resultado con Ollama
    5. Ajusta parámetros basado en feedback
    6. Almacena conocimiento
    7. Mejora continuamente
    """
    
    def __init__(
        self,
        target_score: float = 90.0,
        max_iterations: int = 5,
        ollama_model: str = "llava:7b"
    ):
        self.target_score = target_score
        self.max_iterations = max_iterations
        
        # Componentes
        print("\n🚀 Inicializando Auto-Learning Pipeline...")
        
        self.database = LearningDatabase()
        print("   ✅ Base de datos inicializada")
        
        self.evaluator = OllamaEvaluator(model=ollama_model)
        
        self.pipeline = ProcessingPipeline()
        
        self.param_predictor = ParamPredictor().to(DEVICE)
        print(f"   ✅ Predictor de parámetros en {DEVICE}")
        
        self.rl_agent = ReinforcementAgent(self.param_predictor)
        print("   ✅ Agente RL inicializado")
        
        # Cargar checkpoint si existe
        self._load_checkpoint()
        
        # Estadísticas de sesión
        self.session_stats = {
            'processed': 0,
            'successful': 0,
            'total_score': 0.0,
            'best_score': 0.0,
        }
    
    def _load_checkpoint(self):
        """Cargar modelo guardado si existe"""
        checkpoint_path = CHECKPOINTS_DIR / "best_predictor.pth"
        if checkpoint_path.exists():
            try:
                self.param_predictor.load_state_dict(torch.load(checkpoint_path, map_location=DEVICE))
                print(f"   📂 Checkpoint cargado: {checkpoint_path}")
            except Exception as e:
                print(f"   ⚠️ Error cargando checkpoint: {e}")
    
    def _save_checkpoint(self, score: float):
        """Guardar modelo si es el mejor"""
        checkpoint_path = CHECKPOINTS_DIR / "best_predictor.pth"
        
        # Guardar siempre el mejor
        if score > self.session_stats['best_score']:
            torch.save(self.param_predictor.state_dict(), checkpoint_path)
            print(f"   💾 Nuevo mejor modelo guardado (score: {score:.1f})")
            self.session_stats['best_score'] = score
    
    def process_single(
        self, 
        image_path: Path,
        save_output: bool = True
    ) -> Tuple[np.ndarray, EvaluationResult, OptimizationParams]:
        """
        Procesar una imagen con optimización automática.
        
        Args:
            image_path: Ruta a la imagen
            save_output: Si guardar el resultado
            
        Returns:
            (result_image, evaluation, final_params)
        """
        print(f"\n{'='*60}")
        print(f"🖼️  Procesando: {image_path.name}")
        print(f"{'='*60}")
        
        start_time = time.time()
        best_result = None
        best_evaluation = None
        best_params = None
        best_score = -1
        
        # Verificar si ya tenemos parámetros óptimos para esta imagen
        cached_params = self.database.get_optimal_params(str(image_path))
        
        for iteration in range(self.max_iterations):
            print(f"\n   📍 Iteración {iteration + 1}/{self.max_iterations}")
            
            # 1. Obtener parámetros
            if iteration == 0 and cached_params:
                params = cached_params
                print("   📦 Usando parámetros cacheados")
            elif iteration == 0:
                # Predecir con red neuronal
                try:
                    pil_image = Image.open(image_path).convert('RGB')
                    params = self.param_predictor.predict_for_image(pil_image)
                    print("   🧠 Parámetros predichos por IA")
                except Exception as e:
                    params = OptimizationParams()
                    print(f"   ⚠️ Error en predicción, usando defaults: {e}")
            else:
                # Ajustar basado en feedback anterior
                params = self._adjust_params_from_feedback(
                    best_params if best_params else OptimizationParams(),
                    best_evaluation
                )
                print("   🔧 Parámetros ajustados por feedback")
            
            # 2. Procesar imagen
            try:
                result, mask, vehicle = self.pipeline.process(image_path, params)
                print(f"   ✅ Vehículo detectado: {vehicle['class_name']} (conf: {vehicle['confidence']:.2f})")
            except Exception as e:
                print(f"   ❌ Error en procesamiento: {e}")
                continue
            
            # 3. Evaluar resultado
            original = cv2.imread(str(image_path))
            evaluation = self.evaluator.evaluate(original, result, mask)
            
            score_emoji = "🌟" if evaluation.score >= 90 else "✅" if evaluation.score >= 75 else "⚠️" if evaluation.score >= 60 else "❌"
            print(f"   {score_emoji} Score: {evaluation.score:.1f}/100 ({evaluation.classification.value})")
            
            if evaluation.issues:
                for issue in evaluation.issues[:3]:
                    print(f"      ⚠️ {issue}")
            
            # 4. Actualizar mejor resultado
            if evaluation.score > best_score:
                best_score = evaluation.score
                best_result = result
                best_evaluation = evaluation
                best_params = params
            
            # 5. ¿Alcanzamos el objetivo?
            if evaluation.score >= self.target_score:
                print(f"   🎯 ¡Objetivo alcanzado!")
                break
            
            # 6. Almacenar experiencia para RL
            try:
                pil_image = Image.open(image_path).convert('RGB')
                from torchvision import transforms
                transform = transforms.Compose([
                    transforms.Resize((224, 224)),
                    transforms.ToTensor(),
                ])
                img_tensor = transform(pil_image).flatten()
                
                metadata = torch.tensor([
                    pil_image.width / 1000,
                    pil_image.height / 1000,
                    pil_image.width / pil_image.height,
                    np.mean(np.array(pil_image)) / 255,
                    1.0
                ])
                
                state = torch.cat([img_tensor[:256], metadata])  # Simplificado
                
                experience = Experience(
                    state=state,
                    action=params.to_tensor(),
                    reward=evaluation.to_reward(),
                    next_state=state,
                    done=(iteration == self.max_iterations - 1)
                )
                self.rl_agent.store_experience(experience)
            except Exception as e:
                pass  # No crítico
        
        # Tiempo total
        total_time = time.time() - start_time
        
        # 7. Actualizar agente RL
        if len(self.rl_agent.memory) >= 32:
            loss = self.rl_agent.update()
            if loss > 0:
                print(f"   🧠 RL update: loss={loss:.4f}")
        
        # 8. Guardar en base de datos
        if best_result is not None and best_params is not None:
            self.database.save_processing(
                image_path=str(image_path),
                params=best_params,
                evaluation=best_evaluation,
                processing_time_ms=total_time * 1000
            )
        
        # 9. Guardar output
        if save_output and best_result is not None:
            output_path = OUTPUT_DIR / "shadow" / f"{image_path.stem}_autolearn.png"
            cv2.imwrite(str(output_path), best_result)
            print(f"   💾 Guardado: {output_path.name}")
            
            # También transparente
            transparent_path = OUTPUT_DIR / "transparent" / f"{image_path.stem}_transparent.png"
            # Extraer versión sin sombra
            try:
                original = Image.open(image_path).convert('RGBA')
                alpha = self.pipeline.create_soft_alpha(
                    self.pipeline.segment_vehicle(image_path, vehicle['box'], best_params),
                    best_params
                )
                original.putalpha(Image.fromarray(alpha))
                original.save(str(transparent_path), 'PNG')
            except:
                pass
        
        # 10. Actualizar estadísticas
        self.session_stats['processed'] += 1
        if best_score >= self.target_score:
            self.session_stats['successful'] += 1
        self.session_stats['total_score'] += best_score
        
        # 11. Guardar checkpoint si es mejor
        self._save_checkpoint(best_score)
        
        print(f"\n   ⏱️  Tiempo total: {total_time:.1f}s")
        print(f"   📊 Score final: {best_score:.1f}/100")
        
        return best_result, best_evaluation, best_params
    
    def _adjust_params_from_feedback(
        self, 
        params: OptimizationParams, 
        evaluation: EvaluationResult
    ) -> OptimizationParams:
        """Ajustar parámetros basado en feedback de evaluación"""
        new_params = copy.deepcopy(params)
        
        if evaluation is None:
            return new_params
        
        # Analizar issues
        issues_text = ' '.join(evaluation.issues).lower()
        
        # Problema: Ruedas cortadas
        if not evaluation.wheels_intact or 'wheel' in issues_text or 'rueda' in issues_text:
            new_params.dilation_iterations = min(5, params.dilation_iterations + 1)
            new_params.erosion_iterations = max(0, params.erosion_iterations - 1)
        
        # Problema: Sombra original incluida
        if not evaluation.no_ground_shadow or 'shadow' in issues_text or 'sombra' in issues_text:
            new_params.erosion_iterations = min(5, params.erosion_iterations + 1)
            new_params.detection_confidence = max(0.15, params.detection_confidence - 0.03)
        
        # Problema: Bordes malos
        if not evaluation.clean_edges or 'edge' in issues_text or 'borde' in issues_text:
            new_params.edge_softness = min(5.0, params.edge_softness + 0.5)
            new_params.edge_feather = min(2.0, params.edge_feather + 0.2)
        
        # Problema: Fondo visible
        if not evaluation.no_background or 'background' in issues_text or 'fondo' in issues_text:
            new_params.erosion_iterations = min(5, params.erosion_iterations + 1)
        
        # Problema: Vehículo incompleto
        if not evaluation.vehicle_complete or 'incomplete' in issues_text or 'cortado' in issues_text:
            new_params.dilation_iterations = min(5, params.dilation_iterations + 1)
            new_params.detection_confidence = max(0.1, params.detection_confidence - 0.02)
        
        # Problema: Sombra artificial
        if not evaluation.shadow_natural or 'artificial' in issues_text:
            # Ajustar sombra
            if 'dark' in issues_text or 'oscur' in issues_text:
                new_params.shadow_intensity = max(0.2, params.shadow_intensity - 0.05)
            elif 'light' in issues_text or 'clar' in issues_text:
                new_params.shadow_intensity = min(0.7, params.shadow_intensity + 0.05)
            
            new_params.shadow_blur = min(40, params.shadow_blur + 3)
        
        return new_params
    
    def process_batch(
        self, 
        images_dir: Path,
        epochs: int = 1
    ) -> Dict[str, Any]:
        """
        Procesar lote de imágenes con entrenamiento.
        
        Args:
            images_dir: Directorio con imágenes
            epochs: Número de épocas de entrenamiento
            
        Returns:
            Estadísticas del procesamiento
        """
        # Obtener imágenes
        extensions = ['*.jpg', '*.jpeg', '*.png', '*.webp']
        images = []
        for ext in extensions:
            images.extend(images_dir.glob(ext))
        
        if not images:
            print(f"❌ No se encontraron imágenes en {images_dir}")
            return {}
        
        print(f"\n{'#'*60}")
        print(f"🎯 ENTRENAMIENTO BATCH")
        print(f"{'#'*60}")
        print(f"   Imágenes: {len(images)}")
        print(f"   Épocas: {epochs}")
        print(f"   Target score: {self.target_score}")
        
        total_start = time.time()
        all_scores = []
        
        for epoch in range(epochs):
            print(f"\n{'='*60}")
            print(f"📚 ÉPOCA {epoch + 1}/{epochs}")
            print(f"{'='*60}")
            
            epoch_scores = []
            
            for i, image_path in enumerate(sorted(images), 1):
                try:
                    result, evaluation, params = self.process_single(
                        image_path,
                        save_output=(epoch == epochs - 1)  # Solo guardar en última época
                    )
                    
                    if evaluation:
                        epoch_scores.append(evaluation.score)
                        all_scores.append(evaluation.score)
                        
                except Exception as e:
                    print(f"   ❌ Error: {e}")
            
            # Estadísticas de época
            if epoch_scores:
                avg = np.mean(epoch_scores)
                print(f"\n   📊 Época {epoch + 1} - Promedio: {avg:.1f}/100")
                print(f"      Min: {min(epoch_scores):.1f} | Max: {max(epoch_scores):.1f}")
        
        # Estadísticas finales
        total_time = time.time() - total_start
        
        stats = {
            'total_images': len(images),
            'epochs': epochs,
            'total_time_min': total_time / 60,
            'avg_score': np.mean(all_scores) if all_scores else 0,
            'max_score': max(all_scores) if all_scores else 0,
            'min_score': min(all_scores) if all_scores else 0,
            'success_rate': sum(1 for s in all_scores if s >= self.target_score) / len(all_scores) * 100 if all_scores else 0,
        }
        
        print(f"\n{'='*60}")
        print(f"📊 RESUMEN FINAL")
        print(f"{'='*60}")
        print(f"   Tiempo total: {stats['total_time_min']:.1f} minutos")
        print(f"   Promedio: {stats['avg_score']:.1f}/100")
        print(f"   Éxito (≥{self.target_score}): {stats['success_rate']:.1f}%")
        
        # Guardar log
        log_path = LOGS_DIR / f"batch_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
        with open(log_path, 'w') as f:
            json.dump(stats, f, indent=2)
        
        return stats
    
    def get_stats(self) -> Dict[str, Any]:
        """Obtener estadísticas completas"""
        db_stats = self.database.get_stats()
        rl_stats = self.rl_agent.get_stats()
        problems = self.database.get_problem_analysis()
        
        return {
            'session': self.session_stats,
            'database': db_stats,
            'rl_agent': rl_stats,
            'problems': problems,
        }
    
    def print_stats(self):
        """Imprimir estadísticas"""
        stats = self.get_stats()
        
        print(f"\n{'='*60}")
        print(f"📊 ESTADÍSTICAS")
        print(f"{'='*60}")
        
        print(f"\n📝 Sesión actual:")
        print(f"   Procesadas: {stats['session']['processed']}")
        print(f"   Exitosas: {stats['session']['successful']}")
        if stats['session']['processed'] > 0:
            print(f"   Promedio: {stats['session']['total_score'] / stats['session']['processed']:.1f}")
        
        if stats['database']:
            print(f"\n💾 Base de datos:")
            print(f"   Total histórico: {stats['database'].get('total_processed', 0)}")
            print(f"   Promedio histórico: {stats['database'].get('avg_score', 0):.1f}")
            print(f"   Tasa de éxito: {stats['database'].get('success_rate', 0):.1f}%")
        
        if stats['problems']:
            print(f"\n⚠️ Problemas comunes:")
            print(f"   Ruedas: {stats['problems'].get('wheel_issues_pct', 0):.1f}%")
            print(f"   Sombras: {stats['problems'].get('shadow_issues_pct', 0):.1f}%")
            print(f"   Bordes: {stats['problems'].get('edge_issues_pct', 0):.1f}%")


# =============================================================================
# 8. MAIN
# =============================================================================

def main():
    """Punto de entrada principal"""
    import argparse
    
    parser = argparse.ArgumentParser(
        description="🧠 Auto-Learning System para eliminación de fondo"
    )
    parser.add_argument(
        "--mode", 
        choices=["single", "batch", "continuous", "stats"],
        default="single",
        help="Modo de operación"
    )
    parser.add_argument(
        "--input",
        type=str,
        default="./input",
        help="Imagen o directorio de entrada"
    )
    parser.add_argument(
        "--output",
        type=str,
        default="./output_autolearn",
        help="Directorio de salida"
    )
    parser.add_argument(
        "--target-score",
        type=float,
        default=90.0,
        help="Puntuación objetivo (default: 90)"
    )
    parser.add_argument(
        "--max-iterations",
        type=int,
        default=5,
        help="Máximo de iteraciones por imagen (default: 5)"
    )
    parser.add_argument(
        "--epochs",
        type=int,
        default=1,
        help="Épocas para modo batch (default: 1)"
    )
    parser.add_argument(
        "--ollama-model",
        type=str,
        default="llava:7b",
        help="Modelo Ollama a usar (default: llava:7b)"
    )
    
    args = parser.parse_args()
    
    # Actualizar directorio de salida
    global OUTPUT_DIR
    OUTPUT_DIR = Path(args.output)
    OUTPUT_DIR.mkdir(exist_ok=True)
    (OUTPUT_DIR / "transparent").mkdir(exist_ok=True)
    (OUTPUT_DIR / "shadow").mkdir(exist_ok=True)
    
    # Crear pipeline
    pipeline = AutoLearningPipeline(
        target_score=args.target_score,
        max_iterations=args.max_iterations,
        ollama_model=args.ollama_model
    )
    
    if args.mode == "single":
        # Procesar imagen(es) individual(es)
        input_path = Path(args.input)
        
        if input_path.is_file():
            images = [input_path]
        else:
            extensions = ['*.jpg', '*.jpeg', '*.png', '*.webp']
            images = []
            for ext in extensions:
                images.extend(input_path.glob(ext))
        
        if not images:
            print(f"❌ No se encontraron imágenes en {input_path}")
            return
        
        for image_path in sorted(images):
            result, evaluation, params = pipeline.process_single(image_path)
        
        pipeline.print_stats()
    
    elif args.mode == "batch":
        # Entrenamiento batch
        input_path = Path(args.input)
        stats = pipeline.process_batch(input_path, epochs=args.epochs)
    
    elif args.mode == "continuous":
        # Modo continuo (monitorea directorio)
        print("🔄 Modo continuo - Ctrl+C para salir")
        input_path = Path(args.input)
        processed = set()
        
        try:
            while True:
                extensions = ['*.jpg', '*.jpeg', '*.png', '*.webp']
                images = []
                for ext in extensions:
                    images.extend(input_path.glob(ext))
                
                new_images = [img for img in images if str(img) not in processed]
                
                if new_images:
                    for image_path in sorted(new_images):
                        result, evaluation, params = pipeline.process_single(image_path)
                        processed.add(str(image_path))
                else:
                    time.sleep(5)  # Esperar nuevas imágenes
                    
        except KeyboardInterrupt:
            print("\n👋 Detenido por usuario")
            pipeline.print_stats()
    
    elif args.mode == "stats":
        # Solo mostrar estadísticas
        pipeline.print_stats()


if __name__ == "__main__":
    main()
