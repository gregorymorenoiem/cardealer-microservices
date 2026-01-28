#!/usr/bin/env python3
"""
AI-Powered Quality Evaluator for Vehicle Segmentation
======================================================

Sistema de evaluación automática usando modelos de IA:

1. CLIP: Evaluación rápida local (sin costo)
2. Vision LLM (GPT-4V/Claude): Evaluación detallada (con API)
3. Aesthetic Score: Calidad visual general
4. Anomaly Detection: Detectar problemas específicos

Author: Gregory Moreno
Date: January 2026
"""

import cv2
import numpy as np
import torch
import torch.nn.functional as F
from PIL import Image
from pathlib import Path
from typing import Dict, Any, List, Tuple, Optional
from dataclasses import dataclass
from enum import Enum
import base64
import json
import io
import os

# Transformers
from transformers import (
    CLIPProcessor, 
    CLIPModel,
    BlipProcessor,
    BlipForConditionalGeneration,
    AutoProcessor,
    AutoModel
)

SCRIPT_DIR = Path(__file__).parent
MODELS_DIR = SCRIPT_DIR / "models"
DEVICE = "mps" if torch.backends.mps.is_available() else "cuda" if torch.cuda.is_available() else "cpu"


class EvaluationResult(Enum):
    EXCELLENT = "excellent"     # 90-100: Perfecto, listo para usar
    GOOD = "good"               # 75-89: Bueno, aceptable
    ACCEPTABLE = "acceptable"   # 60-74: Aceptable con reservas
    NEEDS_REVIEW = "needs_review"  # 40-59: Requiere revisión manual
    REJECTED = "rejected"       # 0-39: Rechazado, rehacer


@dataclass
class AIEvaluation:
    """Resultado de evaluación por IA"""
    score: float                    # 0-100
    result: EvaluationResult
    confidence: float               # Confianza de la evaluación
    
    # Análisis detallado
    vehicle_complete: bool          # ¿Vehículo completo?
    wheels_intact: bool             # ¿Ruedas completas?
    no_shadows: bool                # ¿Sin sombras del suelo?
    clean_edges: bool               # ¿Bordes limpios?
    no_background: bool             # ¿Sin restos de fondo?
    
    # Feedback textual
    issues: List[str]               # Lista de problemas detectados
    suggestions: List[str]          # Sugerencias de mejora
    description: str                # Descripción general
    
    # Para reentrenamiento
    should_retrain: bool            # ¿Usar para reentrenamiento?
    problem_areas: List[str]        # Áreas problemáticas específicas


class CLIPEvaluator:
    """
    Evaluador basado en CLIP (Contrastive Language-Image Pre-training)
    
    Ventajas:
    - Local, sin costo de API
    - Rápido (~100ms por imagen)
    - Bueno para comparaciones relativas
    """
    
    def __init__(self):
        self.model = None
        self.processor = None
        self.loaded = False
        
        # Prompts para evaluación de calidad
        self.quality_prompts = {
            'good': [
                "a perfect vehicle cutout with transparent background",
                "a clean car photo with no background, professional quality",
                "a vehicle perfectly isolated from background, high quality",
                "a car with complete wheels and clean edges, no shadows",
                "professional product photo of a car on transparent background"
            ],
            'bad': [
                "a poorly cut vehicle with jagged edges",
                "a car cutout with shadows included",
                "a vehicle photo with missing wheels or parts",
                "a bad photoshop job with visible artifacts",
                "a car cutout with background remnants visible"
            ]
        }
        
        # Prompts específicos para problemas
        self.issue_prompts = {
            'wheels_cut': "a car with cut or missing wheels",
            'shadow_included': "a car with ground shadow included",
            'jagged_edges': "a photo with rough jagged edges",
            'background_remnants': "a cutout with background pieces visible",
            'incomplete_vehicle': "an incomplete or partial vehicle",
            'good_cutout': "a perfect professional vehicle cutout"
        }
        
    def load(self):
        """Cargar modelo CLIP"""
        if self.loaded:
            return
            
        print("   Loading CLIP model...")
        try:
            self.processor = CLIPProcessor.from_pretrained("openai/clip-vit-large-patch14")
            self.model = CLIPModel.from_pretrained("openai/clip-vit-large-patch14")
            self.model.to(DEVICE)
            self.model.eval()
            self.loaded = True
            print("   ✅ CLIP loaded successfully")
        except Exception as e:
            print(f"   ❌ Failed to load CLIP: {e}")
            
    def _image_to_pil(self, image: np.ndarray) -> Image.Image:
        """Convertir numpy array a PIL Image"""
        if len(image.shape) == 3 and image.shape[2] == 4:
            # RGBA - crear fondo de tablero de ajedrez para visualizar transparencia
            alpha = image[:, :, 3:4] / 255.0
            rgb = image[:, :, :3]
            
            # Crear patrón de tablero
            h, w = image.shape[:2]
            checker = np.zeros((h, w, 3), dtype=np.uint8)
            block_size = 20
            for i in range(0, h, block_size):
                for j in range(0, w, block_size):
                    if (i // block_size + j // block_size) % 2 == 0:
                        checker[i:i+block_size, j:j+block_size] = [200, 200, 200]
                    else:
                        checker[i:i+block_size, j:j+block_size] = [150, 150, 150]
            
            # Componer
            composed = (rgb * alpha + checker * (1 - alpha)).astype(np.uint8)
            return Image.fromarray(cv2.cvtColor(composed, cv2.COLOR_BGR2RGB))
        else:
            return Image.fromarray(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))
            
    def evaluate_quality(self, cutout_image: np.ndarray) -> Tuple[float, Dict[str, float]]:
        """
        Evaluar calidad general del recorte
        
        Returns:
            (score 0-100, detailed_scores)
        """
        if not self.loaded:
            self.load()
            
        if not self.loaded:
            return 50.0, {}
            
        pil_image = self._image_to_pil(cutout_image)
        
        # Calcular similitud con prompts buenos vs malos
        all_prompts = self.quality_prompts['good'] + self.quality_prompts['bad']
        
        inputs = self.processor(
            text=all_prompts,
            images=pil_image,
            return_tensors="pt",
            padding=True
        )
        inputs = {k: v.to(DEVICE) for k, v in inputs.items()}
        
        with torch.no_grad():
            outputs = self.model(**inputs)
            
        # Calcular probabilidades
        logits = outputs.logits_per_image[0]
        probs = F.softmax(logits, dim=0).cpu().numpy()
        
        n_good = len(self.quality_prompts['good'])
        good_score = probs[:n_good].sum()
        bad_score = probs[n_good:].sum()
        
        # Score final (0-100)
        quality_score = good_score / (good_score + bad_score) * 100
        
        return quality_score, {
            'good_similarity': float(good_score),
            'bad_similarity': float(bad_score)
        }
        
    def detect_issues(self, cutout_image: np.ndarray) -> Dict[str, float]:
        """
        Detectar problemas específicos en el recorte
        
        Returns:
            Dict con probabilidad de cada problema (0-1)
        """
        if not self.loaded:
            self.load()
            
        if not self.loaded:
            return {}
            
        pil_image = self._image_to_pil(cutout_image)
        
        prompts = list(self.issue_prompts.values())
        
        inputs = self.processor(
            text=prompts,
            images=pil_image,
            return_tensors="pt",
            padding=True
        )
        inputs = {k: v.to(DEVICE) for k, v in inputs.items()}
        
        with torch.no_grad():
            outputs = self.model(**inputs)
            
        logits = outputs.logits_per_image[0]
        probs = F.softmax(logits, dim=0).cpu().numpy()
        
        issue_scores = {}
        for i, (issue_name, _) in enumerate(self.issue_prompts.items()):
            issue_scores[issue_name] = float(probs[i])
            
        return issue_scores


class BLIPEvaluator:
    """
    Evaluador basado en BLIP (Bootstrapping Language-Image Pre-training)
    
    Ventajas:
    - Puede generar descripciones de lo que ve
    - Bueno para identificar problemas específicos
    - Respuestas más detalladas que CLIP
    """
    
    def __init__(self):
        self.model = None
        self.processor = None
        self.loaded = False
        
    def load(self):
        """Cargar modelo BLIP"""
        if self.loaded:
            return
            
        print("   Loading BLIP model...")
        try:
            self.processor = BlipProcessor.from_pretrained("Salesforce/blip-image-captioning-large")
            self.model = BlipForConditionalGeneration.from_pretrained("Salesforce/blip-image-captioning-large")
            self.model.to(DEVICE)
            self.model.eval()
            self.loaded = True
            print("   ✅ BLIP loaded successfully")
        except Exception as e:
            print(f"   ❌ Failed to load BLIP: {e}")
            
    def describe_image(self, image: np.ndarray) -> str:
        """Generar descripción de la imagen"""
        if not self.loaded:
            self.load()
            
        if not self.loaded:
            return "Unable to analyze image"
            
        # Convertir a PIL
        if len(image.shape) == 3 and image.shape[2] == 4:
            pil_image = Image.fromarray(cv2.cvtColor(image[:, :, :3], cv2.COLOR_BGR2RGB))
        else:
            pil_image = Image.fromarray(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))
            
        inputs = self.processor(images=pil_image, return_tensors="pt")
        inputs = {k: v.to(DEVICE) for k, v in inputs.items()}
        
        with torch.no_grad():
            output = self.model.generate(**inputs, max_length=50)
            
        caption = self.processor.decode(output[0], skip_special_tokens=True)
        return caption
        
    def answer_question(self, image: np.ndarray, question: str) -> str:
        """Responder pregunta sobre la imagen (VQA)"""
        if not self.loaded:
            self.load()
            
        if not self.loaded:
            return "Unable to analyze"
            
        # Convertir a PIL
        if len(image.shape) == 3 and image.shape[2] == 4:
            pil_image = Image.fromarray(cv2.cvtColor(image[:, :, :3], cv2.COLOR_BGR2RGB))
        else:
            pil_image = Image.fromarray(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))
            
        inputs = self.processor(images=pil_image, text=question, return_tensors="pt")
        inputs = {k: v.to(DEVICE) for k, v in inputs.items()}
        
        with torch.no_grad():
            output = self.model.generate(**inputs, max_length=30)
            
        answer = self.processor.decode(output[0], skip_special_tokens=True)
        return answer


class VisionLLMEvaluator:
    """
    Evaluador usando Vision LLM (GPT-4V, Claude 3, GitHub Models, o Ollama)
    
    Providers disponibles:
    - "ollama": Ollama local (LLaVA) - GRATIS, SIN LÍMITES ⭐
    - "github": GitHub Models (GPT-4o) - Usa GITHUB_TOKEN (50/día límite)
    - "openai": OpenAI GPT-4V - Usa OPENAI_API_KEY
    - "anthropic": Anthropic Claude - Usa ANTHROPIC_API_KEY
    
    Ventajas:
    - Evaluación más inteligente y detallada
    - Puede dar feedback específico y actionable
    - Entiende contexto (marketplace de autos)
    
    Desventajas:
    - Requiere API key (excepto Ollama)
    - Costo por imagen (~$0.01-0.03) (excepto Ollama)
    """
    
    def __init__(self, provider: str = "ollama"):
        """
        Args:
            provider: "ollama" (local, SIN LÍMITES), "github", "openai", o "anthropic"
        """
        self.provider = provider
        self.api_key = None
        self.model_name = None
        
        # Cargar configuración según provider
        if provider == "ollama":
            self.api_key = "local"  # No necesita API key
            self.model_name = "llava:7b"  # LLaVA multimodal local
        elif provider == "github":
            self.api_key = os.environ.get("GITHUB_TOKEN")
            self.model_name = "gpt-4o"  # GPT-4o via GitHub Models
        elif provider == "openai":
            self.api_key = os.environ.get("OPENAI_API_KEY")
            self.model_name = "gpt-4o"
        elif provider == "anthropic":
            self.api_key = os.environ.get("ANTHROPIC_API_KEY")
            self.model_name = "claude-sonnet-4-20250514"
            
    def _encode_image(self, image: np.ndarray) -> str:
        """Convertir imagen a base64"""
        # Si tiene alpha, componer sobre fondo de tablero
        if len(image.shape) == 3 and image.shape[2] == 4:
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
            image_to_encode = composed
        else:
            image_to_encode = image
            
        # Encode
        _, buffer = cv2.imencode('.jpg', image_to_encode, [cv2.IMWRITE_JPEG_QUALITY, 90])
        return base64.b64encode(buffer).decode('utf-8')
        
    def evaluate(self, original_image: np.ndarray, cutout_image: np.ndarray) -> Optional[AIEvaluation]:
        """
        Evaluar recorte usando Vision LLM
        
        Args:
            original_image: Imagen original
            cutout_image: Imagen recortada (RGBA)
            
        Returns:
            AIEvaluation o None si falla
        """
        if not self.api_key:
            print(f"   ⚠️ No API key for {self.provider}")
            return None
            
        original_b64 = self._encode_image(original_image)
        cutout_b64 = self._encode_image(cutout_image)
        
        prompt = """Eres un experto en calidad de imágenes para un marketplace de vehículos.

Analiza estas dos imágenes:
1. ORIGINAL: La foto original del vehículo
2. CUTOUT: El vehículo recortado (el fondo de cuadros grises indica transparencia)

Evalúa el CUTOUT y responde en JSON exactamente así:
{
    "score": <número 0-100>,
    "vehicle_complete": <true/false - ¿el vehículo está completo?>,
    "wheels_intact": <true/false - ¿las ruedas están completas, sin cortar?>,
    "no_shadows": <true/false - ¿no hay sombras del suelo incluidas?>,
    "clean_edges": <true/false - ¿los bordes están limpios sin artefactos?>,
    "no_background": <true/false - ¿no hay restos de fondo visible?>,
    "issues": [<lista de problemas encontrados>],
    "suggestions": [<lista de sugerencias para mejorar>],
    "description": "<descripción breve de la calidad general>"
}

Criterios de puntuación:
- 90-100: Perfecto, calidad profesional
- 75-89: Bueno, pequeños detalles menores
- 60-74: Aceptable pero mejorable
- 40-59: Problemas visibles, requiere revisión
- 0-39: Inaceptable, rechazar

Sé estricto pero justo. Responde SOLO con el JSON, sin texto adicional."""

        try:
            if self.provider == "ollama":
                return self._call_ollama(prompt, original_b64, cutout_b64)
            elif self.provider == "github":
                return self._call_github_models(prompt, original_b64, cutout_b64)
            elif self.provider == "openai":
                return self._call_openai(prompt, original_b64, cutout_b64)
            elif self.provider == "anthropic":
                return self._call_anthropic(prompt, original_b64, cutout_b64)
        except Exception as e:
            print(f"   ❌ Vision LLM error: {e}")
            return None
    
    def _call_ollama(self, prompt: str, original_b64: str, cutout_b64: str) -> Optional[AIEvaluation]:
        """Llamar a LLaVA via Ollama - LOCAL, GRATIS, SIN LÍMITES"""
        import ollama
        
        response = ollama.chat(
            model=self.model_name,  # llava:7b
            messages=[
                {
                    "role": "user",
                    "content": prompt,
                    "images": [original_b64, cutout_b64]
                }
            ],
            options={
                "temperature": 0.1,
                "num_predict": 500
            }
        )
        
        result_text = response['message']['content']
        return self._parse_response(result_text)
            
    def _call_github_models(self, prompt: str, original_b64: str, cutout_b64: str) -> Optional[AIEvaluation]:
        """Llamar a GPT-4o via GitHub Models (Azure endpoint) - GRATIS"""
        from openai import OpenAI
        
        client = OpenAI(
            base_url="https://models.inference.ai.azure.com",
            api_key=self.api_key
        )
        
        response = client.chat.completions.create(
            model="gpt-4o",  # GPT-4o disponible en GitHub Models (gratis)
            messages=[
                {
                    "role": "user",
                    "content": [
                        {"type": "text", "text": prompt},
                        {
                            "type": "image_url",
                            "image_url": {
                                "url": f"data:image/jpeg;base64,{original_b64}",
                                "detail": "low"
                            }
                        },
                        {
                            "type": "image_url", 
                            "image_url": {
                                "url": f"data:image/jpeg;base64,{cutout_b64}",
                                "detail": "high"
                            }
                        }
                    ]
                }
            ],
            max_tokens=500,
            temperature=0.1
        )
        
        result_text = response.choices[0].message.content
        return self._parse_response(result_text)
            
    def _call_openai(self, prompt: str, original_b64: str, cutout_b64: str) -> Optional[AIEvaluation]:
        """Llamar a GPT-4 Vision"""
        import openai
        
        client = openai.OpenAI(api_key=self.api_key)
        
        response = client.chat.completions.create(
            model="gpt-4o",
            messages=[
                {
                    "role": "user",
                    "content": [
                        {"type": "text", "text": prompt},
                        {
                            "type": "image_url",
                            "image_url": {
                                "url": f"data:image/jpeg;base64,{original_b64}",
                                "detail": "low"
                            }
                        },
                        {
                            "type": "image_url", 
                            "image_url": {
                                "url": f"data:image/jpeg;base64,{cutout_b64}",
                                "detail": "high"
                            }
                        }
                    ]
                }
            ],
            max_tokens=500,
            temperature=0.1
        )
        
        result_text = response.choices[0].message.content
        return self._parse_response(result_text)
        
    def _call_anthropic(self, prompt: str, original_b64: str, cutout_b64: str) -> Optional[AIEvaluation]:
        """Llamar a Claude Vision"""
        import anthropic
        
        client = anthropic.Anthropic(api_key=self.api_key)
        
        response = client.messages.create(
            model="claude-sonnet-4-20250514",
            max_tokens=500,
            messages=[
                {
                    "role": "user",
                    "content": [
                        {
                            "type": "image",
                            "source": {
                                "type": "base64",
                                "media_type": "image/jpeg",
                                "data": original_b64
                            }
                        },
                        {
                            "type": "image",
                            "source": {
                                "type": "base64",
                                "media_type": "image/jpeg",
                                "data": cutout_b64
                            }
                        },
                        {
                            "type": "text",
                            "text": prompt
                        }
                    ]
                }
            ]
        )
        
        result_text = response.content[0].text
        return self._parse_response(result_text)
        
    def _parse_response(self, response_text: str) -> Optional[AIEvaluation]:
        """Parsear respuesta JSON a AIEvaluation"""
        try:
            # Limpiar respuesta
            text = response_text.strip()
            if text.startswith("```"):
                text = text.split("```")[1]
                if text.startswith("json"):
                    text = text[4:]
            text = text.strip()
            
            data = json.loads(text)
            
            score = float(data.get('score', 50))
            
            # Determinar resultado
            if score >= 90:
                result = EvaluationResult.EXCELLENT
            elif score >= 75:
                result = EvaluationResult.GOOD
            elif score >= 60:
                result = EvaluationResult.ACCEPTABLE
            elif score >= 40:
                result = EvaluationResult.NEEDS_REVIEW
            else:
                result = EvaluationResult.REJECTED
                
            return AIEvaluation(
                score=score,
                result=result,
                confidence=0.9,  # Alta confianza para Vision LLM
                vehicle_complete=data.get('vehicle_complete', True),
                wheels_intact=data.get('wheels_intact', True),
                no_shadows=data.get('no_shadows', True),
                clean_edges=data.get('clean_edges', True),
                no_background=data.get('no_background', True),
                issues=data.get('issues', []),
                suggestions=data.get('suggestions', []),
                description=data.get('description', ''),
                should_retrain=score >= 75,
                problem_areas=data.get('issues', [])
            )
            
        except Exception as e:
            print(f"   ⚠️ Failed to parse response: {e}")
            return None


class HybridEvaluator:
    """
    Evaluador híbrido que combina múltiples métodos
    
    Estrategia:
    1. CLIP: Evaluación rápida inicial (siempre)
    2. Análisis de máscara: Métricas técnicas
    3. Vision LLM: Solo para casos ambiguos o muestreo (GitHub Models / Claude)
    """
    
    def __init__(self, use_vision_llm: bool = False, vision_provider: str = "github"):
        self.clip = CLIPEvaluator()
        self.blip = BLIPEvaluator()
        self.vision_llm = VisionLLMEvaluator(vision_provider) if use_vision_llm else None
        
        # Contadores para muestreo
        self.eval_count = 0
        self.llm_sample_rate = 0.1  # Usar LLM en 10% de las evaluaciones
        
    def load_models(self):
        """Cargar todos los modelos"""
        print("\n📦 Loading AI Evaluator models...")
        self.clip.load()
        # BLIP es más pesado, cargar solo si se necesita
        
    def evaluate(self, 
                 original_image: np.ndarray, 
                 cutout_image: np.ndarray,
                 mask: Optional[np.ndarray] = None,
                 force_llm: bool = False) -> AIEvaluation:
        """
        Evaluación completa del recorte
        
        Args:
            original_image: Imagen original
            cutout_image: Imagen recortada (RGBA)
            mask: Máscara de segmentación (opcional)
            force_llm: Forzar uso de Vision LLM
            
        Returns:
            AIEvaluation con todos los detalles
        """
        self.eval_count += 1
        issues = []
        suggestions = []
        
        # === 1. Evaluación CLIP (siempre) ===
        clip_score, clip_details = self.clip.evaluate_quality(cutout_image)
        clip_issues = self.clip.detect_issues(cutout_image)
        
        # === 2. Análisis técnico de máscara ===
        if mask is None and len(cutout_image.shape) == 3 and cutout_image.shape[2] == 4:
            mask = cutout_image[:, :, 3]
            
        mask_analysis = self._analyze_mask(mask) if mask is not None else {}
        
        # === 3. Detectar problemas específicos ===
        
        # Ruedas cortadas
        wheels_ok = True
        if mask is not None:
            wheels_ok = mask_analysis.get('wheels_coverage', 1.0) > 0.05
        if clip_issues.get('wheels_cut', 0) > 0.3:
            wheels_ok = False
            issues.append("Ruedas posiblemente cortadas o incompletas")
            suggestions.append("Ajustar detección para incluir ruedas completas")
            
        # Sombras incluidas
        no_shadows = True
        if clip_issues.get('shadow_included', 0) > 0.3:
            no_shadows = False
            issues.append("Sombra del suelo incluida en el recorte")
            suggestions.append("Mejorar detección de sombras en zona inferior")
            
        # Bordes irregulares
        clean_edges = True
        if clip_issues.get('jagged_edges', 0) > 0.25:
            clean_edges = False
            issues.append("Bordes irregulares o con artefactos")
            suggestions.append("Aplicar más suavizado en bordes")
            
        # Fondo visible
        no_background = True
        if clip_issues.get('background_remnants', 0) > 0.25:
            no_background = False
            issues.append("Restos de fondo visibles")
            suggestions.append("Revisar umbral de segmentación")
            
        # Vehículo incompleto
        vehicle_complete = True
        if clip_issues.get('incomplete_vehicle', 0) > 0.3:
            vehicle_complete = False
            issues.append("Vehículo incompleto o parcialmente cortado")
            suggestions.append("Verificar bounding box de detección")
            
        # === 4. Calcular score final ===
        
        # Pesos para cada factor
        weights = {
            'clip': 0.40,
            'mask_coverage': 0.15,
            'mask_edges': 0.15,
            'no_issues': 0.30
        }
        
        # Score de problemas (penalización por cada issue)
        issue_penalty = len(issues) * 10
        issue_score = max(0, 100 - issue_penalty)
        
        # Score de máscara
        mask_score = 70  # Default
        if mask_analysis:
            coverage = mask_analysis.get('coverage', 0.2)
            # Cobertura ideal: 10-50% de la imagen
            if 0.10 <= coverage <= 0.50:
                mask_score = 100
            elif coverage < 0.05:
                mask_score = 30
            else:
                mask_score = 70
                
        # Score final ponderado
        final_score = (
            clip_score * weights['clip'] +
            mask_score * weights['mask_coverage'] +
            (100 if clean_edges else 50) * weights['mask_edges'] +
            issue_score * weights['no_issues']
        )
        
        # === 5. Vision LLM (opcional) ===
        llm_evaluation = None
        use_llm = force_llm or (
            self.vision_llm is not None and 
            (self.eval_count % int(1 / self.llm_sample_rate) == 0 or  # Muestreo
             40 <= final_score <= 75)  # Casos ambiguos
        )
        
        if use_llm and self.vision_llm:
            print("   🤖 Using Vision LLM for detailed evaluation...")
            llm_evaluation = self.vision_llm.evaluate(original_image, cutout_image)
            
            if llm_evaluation:
                # Promediar con evaluación LLM (más peso a LLM)
                final_score = final_score * 0.3 + llm_evaluation.score * 0.7
                issues = llm_evaluation.issues
                suggestions = llm_evaluation.suggestions
                
        # === 6. Determinar resultado final ===
        if final_score >= 90:
            result = EvaluationResult.EXCELLENT
        elif final_score >= 75:
            result = EvaluationResult.GOOD
        elif final_score >= 60:
            result = EvaluationResult.ACCEPTABLE
        elif final_score >= 40:
            result = EvaluationResult.NEEDS_REVIEW
        else:
            result = EvaluationResult.REJECTED
            
        # Descripción
        description = f"Score: {final_score:.0f}/100. "
        if result == EvaluationResult.EXCELLENT:
            description += "Recorte de alta calidad, listo para usar."
        elif result == EvaluationResult.GOOD:
            description += "Buen recorte con detalles menores."
        elif result == EvaluationResult.ACCEPTABLE:
            description += "Aceptable pero mejorable."
        elif result == EvaluationResult.NEEDS_REVIEW:
            description += "Requiere revisión manual."
        else:
            description += "Calidad insuficiente, rechazado."
            
        return AIEvaluation(
            score=final_score,
            result=result,
            confidence=0.85 if llm_evaluation else 0.70,
            vehicle_complete=vehicle_complete,
            wheels_intact=wheels_ok,
            no_shadows=no_shadows,
            clean_edges=clean_edges,
            no_background=no_background,
            issues=issues,
            suggestions=suggestions,
            description=description,
            should_retrain=result in [EvaluationResult.EXCELLENT, EvaluationResult.GOOD],
            problem_areas=[i for i in issues if 'rueda' in i.lower() or 'sombra' in i.lower()]
        )
        
    def _analyze_mask(self, mask: np.ndarray) -> Dict[str, float]:
        """Análisis técnico de la máscara"""
        h, w = mask.shape[:2]
        total = h * w
        
        # Binarizar si es necesario
        binary = (mask > 128).astype(np.uint8)
        
        # Cobertura
        coverage = binary.sum() / total
        
        # Cobertura en zona de ruedas (bottom 30%)
        wheel_region = binary[int(h * 0.7):, :]
        wheels_coverage = wheel_region.sum() / wheel_region.size if wheel_region.size > 0 else 0
        
        # Conteo de contornos (idealmente 1 para un vehículo)
        contours, _ = cv2.findContours(binary, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        
        # Circularidad del contorno principal
        circularity = 0
        if contours:
            largest = max(contours, key=cv2.contourArea)
            area = cv2.contourArea(largest)
            perimeter = cv2.arcLength(largest, True)
            if perimeter > 0:
                circularity = 4 * np.pi * area / (perimeter ** 2)
                
        return {
            'coverage': coverage,
            'wheels_coverage': wheels_coverage,
            'num_contours': len(contours),
            'circularity': circularity
        }


class ComputationalEvaluator:
    """
    Evaluador 100% CPU sin IA externa
    
    Usa solo métricas computacionales de OpenCV para evaluar:
    - Análisis de contornos y formas
    - Calidad de bordes (gradientes)
    - Detección de huecos/artefactos
    - Cobertura y proporción del vehículo
    - Análisis de ruedas (regiones circulares en zona inferior)
    - Detección de sombras (análisis de gradiente vertical)
    
    Ventajas:
    - 100% local, sin API ni GPU
    - Muy rápido (~10ms por imagen)
    - Determinístico y reproducible
    - Funciona en cualquier máquina
    """
    
    def __init__(self):
        self.loaded = True  # No requiere carga de modelos
        
    def load(self):
        """No requiere carga - compatibilidad con otros evaluadores"""
        pass
        
    def evaluate(self, original_image: np.ndarray, cutout_image: np.ndarray, 
                 mask: Optional[np.ndarray] = None) -> AIEvaluation:
        """
        Evaluar calidad del recorte usando métricas computacionales
        
        Args:
            original_image: Imagen original BGR
            cutout_image: Imagen recortada BGRA (con alpha)
            mask: Máscara opcional (se extrae del alpha si no se provee)
            
        Returns:
            AIEvaluation con puntuación y detalles
        """
        issues = []
        suggestions = []
        
        # Extraer máscara del alpha channel
        if mask is None and len(cutout_image.shape) == 3 and cutout_image.shape[2] == 4:
            mask = cutout_image[:, :, 3]
        elif mask is None:
            # Si no hay alpha, crear máscara desde no-negro
            gray = cv2.cvtColor(cutout_image[:, :, :3] if len(cutout_image.shape) == 3 else cutout_image, cv2.COLOR_BGR2GRAY)
            mask = (gray > 5).astype(np.uint8) * 255
            
        h, w = mask.shape[:2]
        total_pixels = h * w
        
        # 1. ANÁLISIS DE COBERTURA (15 puntos max)
        binary_mask = (mask > 128).astype(np.uint8)
        coverage = binary_mask.sum() / total_pixels
        
        coverage_score = 0
        if 0.15 <= coverage <= 0.60:  # Cobertura ideal: 15-60%
            coverage_score = 15
        elif 0.10 <= coverage <= 0.70:
            coverage_score = 12
        elif 0.05 <= coverage <= 0.80:
            coverage_score = 8
        else:
            coverage_score = 3
            issues.append(f"Cobertura anormal: {coverage*100:.1f}% (ideal: 15-60%)")
            suggestions.append("Revisar dimensiones del bounding box")
            
        # 2. ANÁLISIS DE CONTORNOS (20 puntos max)
        contours, _ = cv2.findContours(binary_mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        
        contour_score = 0
        num_contours = len(contours)
        
        if num_contours == 1:
            contour_score = 20  # Perfecto: un solo contorno (el vehículo)
        elif num_contours <= 3:
            contour_score = 15
        elif num_contours <= 5:
            contour_score = 10
            issues.append(f"Múltiples contornos detectados: {num_contours}")
        else:
            contour_score = 5
            issues.append(f"Demasiados fragmentos: {num_contours} contornos")
            suggestions.append("Mejorar conectividad de la máscara")
            
        # 3. CALIDAD DE BORDES (20 puntos max)
        edge_score = self._analyze_edge_quality(mask)
        if edge_score < 12:
            issues.append("Bordes irregulares o con artefactos")
            suggestions.append("Aumentar suavizado de bordes (edge refinement)")
            
        # 4. ANÁLISIS DE RUEDAS (20 puntos max) 
        wheel_score, wheels_ok = self._analyze_wheels(mask)
        if not wheels_ok:
            issues.append("Ruedas posiblemente incompletas o cortadas")
            suggestions.append("Ajustar detección en zona inferior de la imagen")
            
        # 5. DETECCIÓN DE SOMBRAS (15 puntos max)
        shadow_score, no_shadows = self._detect_shadows(mask, cutout_image)
        if not no_shadows:
            issues.append("Posible sombra del suelo incluida")
            suggestions.append("Mejorar detección de sombras en pipeline")
            
        # 6. HUECOS INTERNOS (10 puntos max)
        holes_score, has_holes = self._detect_holes(binary_mask)
        if has_holes:
            issues.append("Huecos internos en la máscara")
            suggestions.append("Aplicar hole filling más agresivo")
            
        # CALCULAR SCORE FINAL
        total_score = (
            coverage_score +    # 15
            contour_score +     # 20  
            edge_score +        # 20
            wheel_score +       # 20
            shadow_score +      # 15
            holes_score         # 10
        )  # Total: 100
        
        # Determinar resultado
        if total_score >= 90:
            result = EvaluationResult.EXCELLENT
            description = "Recorte de alta calidad, listo para producción."
        elif total_score >= 75:
            result = EvaluationResult.GOOD
            description = "Buen recorte con detalles menores."
        elif total_score >= 60:
            result = EvaluationResult.ACCEPTABLE
            description = "Aceptable pero mejorable."
        elif total_score >= 40:
            result = EvaluationResult.NEEDS_REVIEW
            description = "Requiere revisión y mejoras."
        else:
            result = EvaluationResult.REJECTED
            description = "Calidad insuficiente, rechazado."
            
        return AIEvaluation(
            score=total_score,
            result=result,
            confidence=0.85,  # Alta confianza para métricas determinísticas
            vehicle_complete=num_contours <= 2 and coverage >= 0.10,
            wheels_intact=wheels_ok,
            no_shadows=no_shadows,
            clean_edges=edge_score >= 15,
            no_background=num_contours == 1 and coverage <= 0.70,
            issues=issues,
            suggestions=suggestions,
            description=description,
            should_retrain=result in [EvaluationResult.EXCELLENT, EvaluationResult.GOOD],
            problem_areas=[i for i in issues if 'rueda' in i.lower() or 'sombra' in i.lower()]
        )
        
    def _analyze_edge_quality(self, mask: np.ndarray) -> int:
        """Analizar calidad de bordes usando gradientes"""
        # Detectar bordes de la máscara
        edges = cv2.Canny(mask, 50, 150)
        
        # Calcular suavidad de bordes usando gradientes
        sobel_x = cv2.Sobel(mask, cv2.CV_64F, 1, 0, ksize=3)
        sobel_y = cv2.Sobel(mask, cv2.CV_64F, 0, 1, ksize=3)
        gradient_magnitude = np.sqrt(sobel_x**2 + sobel_y**2)
        
        # Bordes suaves = transiciones graduales (valores de gradiente moderados)
        edge_pixels = gradient_magnitude[edges > 0]
        
        if len(edge_pixels) == 0:
            return 10  # No hay bordes claros
            
        # Coeficiente de variación: menor = más uniforme = mejor
        mean_grad = np.mean(edge_pixels)
        std_grad = np.std(edge_pixels)
        cv = std_grad / (mean_grad + 1e-6)
        
        # Mapear CV a score (CV bajo = bordes suaves = bueno)
        if cv < 0.3:
            return 20
        elif cv < 0.5:
            return 16
        elif cv < 0.7:
            return 12
        elif cv < 1.0:
            return 8
        else:
            return 5
            
    def _analyze_wheels(self, mask: np.ndarray) -> Tuple[int, bool]:
        """Analizar si las ruedas están completas"""
        h, w = mask.shape[:2]
        
        # Zona de ruedas: bottom 35% de la imagen
        wheel_region = mask[int(h * 0.65):, :]
        wheel_h, wheel_w = wheel_region.shape[:2]
        
        # Dividir en izquierda y derecha
        left_wheel = wheel_region[:, :wheel_w//3]
        right_wheel = wheel_region[:, 2*wheel_w//3:]
        
        # Calcular cobertura en cada zona
        left_coverage = (left_wheel > 128).sum() / left_wheel.size
        right_coverage = (right_wheel > 128).sum() / right_wheel.size
        
        # Buscar regiones aproximadamente circulares (ruedas)
        wheel_binary = (wheel_region > 128).astype(np.uint8)
        contours, _ = cv2.findContours(wheel_binary, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        
        has_wheel_shapes = 0
        for cnt in contours:
            area = cv2.contourArea(cnt)
            if area < 100:  # Muy pequeño
                continue
            perimeter = cv2.arcLength(cnt, True)
            if perimeter == 0:
                continue
            circularity = 4 * np.pi * area / (perimeter ** 2)
            # Ruedas tienen circularidad moderada-alta
            if 0.3 < circularity < 1.0:
                has_wheel_shapes += 1
                
        # Evaluar
        wheels_ok = True
        score = 10  # Base
        
        # Bonus por cobertura en zonas de ruedas
        if left_coverage > 0.05 and right_coverage > 0.05:
            score += 5
        elif left_coverage > 0.02 or right_coverage > 0.02:
            score += 2
        else:
            wheels_ok = False
            
        # Bonus por formas circulares detectadas
        if has_wheel_shapes >= 2:
            score += 5
        elif has_wheel_shapes >= 1:
            score += 3
        else:
            wheels_ok = False
            score -= 2
            
        return min(20, max(0, score)), wheels_ok
        
    def _detect_shadows(self, mask: np.ndarray, cutout: np.ndarray) -> Tuple[int, bool]:
        """Detectar sombras del suelo incluidas"""
        h, w = mask.shape[:2]
        
        # Zona de sombra potencial: bottom 20%
        shadow_region = mask[int(h * 0.80):, :]
        
        # Analizar gradiente vertical en zona inferior
        # Sombras tienden a tener valores grises intermedios
        binary = (mask > 128).astype(np.uint8)
        semi_transparent = ((mask > 50) & (mask < 200)).astype(np.uint8)
        
        # Ratio de semi-transparencia en zona inferior
        bottom_region = semi_transparent[int(h * 0.75):, :]
        semi_ratio = bottom_region.sum() / (bottom_region.size + 1e-6)
        
        # Analizar color en zona inferior del cutout
        if len(cutout.shape) == 3 and cutout.shape[2] >= 3:
            bottom_cutout = cutout[int(h * 0.80):, :, :3]
            bottom_mask = mask[int(h * 0.80):, :]
            
            # Donde hay máscara, ¿es muy oscuro? (sombra)
            masked_pixels = bottom_cutout[bottom_mask > 128]
            if len(masked_pixels) > 100:
                avg_brightness = np.mean(masked_pixels)
                if avg_brightness < 50:  # Muy oscuro = posible sombra
                    return 8, False
                    
        # Score basado en ratio de semi-transparencia
        if semi_ratio < 0.05:
            return 15, True  # Sin sombras
        elif semi_ratio < 0.15:
            return 12, True  # Mínimo
        elif semi_ratio < 0.30:
            return 8, False  # Posible sombra
        else:
            return 5, False  # Definitivamente sombra
            
    def _detect_holes(self, binary_mask: np.ndarray) -> Tuple[int, bool]:
        """Detectar huecos internos en la máscara"""
        # Flood fill desde las esquinas para encontrar el exterior
        h, w = binary_mask.shape[:2]
        
        # Invertir máscara
        inverted = 255 - binary_mask * 255
        
        # Contar regiones en el inverso
        contours, _ = cv2.findContours(inverted.astype(np.uint8), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        
        # Filtrar contornos que no tocan el borde (son huecos internos)
        holes = 0
        total_hole_area = 0
        
        for cnt in contours:
            x, y, cw, ch = cv2.boundingRect(cnt)
            # Si no toca ningún borde, es un hueco interno
            if x > 0 and y > 0 and (x + cw) < w and (y + ch) < h:
                area = cv2.contourArea(cnt)
                if area > 50:  # Ignorar huecos muy pequeños
                    holes += 1
                    total_hole_area += area
                    
        hole_ratio = total_hole_area / (h * w)
        
        if holes == 0:
            return 10, False  # Sin huecos
        elif holes <= 2 and hole_ratio < 0.01:
            return 8, False  # Pocos huecos pequeños
        elif holes <= 5 and hole_ratio < 0.03:
            return 5, True  # Algunos huecos
        else:
            return 2, True  # Muchos huecos


def main():
    """Demo del evaluador AI"""
    print("=" * 60)
    print("🤖 AI-POWERED QUALITY EVALUATOR")
    print("=" * 60)
    
    # Crear evaluador
    evaluator = HybridEvaluator(
        use_vision_llm=False  # Cambiar a True si tienes API key
    )
    evaluator.load_models()
    
    # Evaluar imágenes procesadas
    input_dir = SCRIPT_DIR / "input"
    output_dir = SCRIPT_DIR / "output_v2"
    
    images = [
        "luxury_car_1",
        "sports_car_1",
        "suv_1", 
        "truck_1",
        "car_front_1",
        "car_side_1"
    ]
    
    print(f"\n📊 Evaluating {len(images)} images...\n")
    
    results = []
    for name in images:
        original_path = input_dir / f"{name}.jpg"
        cutout_path = output_dir / f"{name}_v2.png"
        
        if not original_path.exists() or not cutout_path.exists():
            continue
            
        original = cv2.imread(str(original_path))
        cutout = cv2.imread(str(cutout_path), cv2.IMREAD_UNCHANGED)
        
        print(f"{'─' * 50}")
        print(f"🚗 {name}")
        
        evaluation = evaluator.evaluate(original, cutout)
        
        print(f"   Score: {evaluation.score:.0f}/100 ({evaluation.result.value})")
        print(f"   ✓ Vehicle complete: {evaluation.vehicle_complete}")
        print(f"   ✓ Wheels intact: {evaluation.wheels_intact}")
        print(f"   ✓ No shadows: {evaluation.no_shadows}")
        print(f"   ✓ Clean edges: {evaluation.clean_edges}")
        
        if evaluation.issues:
            print(f"   ⚠️ Issues:")
            for issue in evaluation.issues:
                print(f"      - {issue}")
                
        if evaluation.suggestions:
            print(f"   💡 Suggestions:")
            for sug in evaluation.suggestions:
                print(f"      - {sug}")
                
        results.append({
            'name': name,
            'score': evaluation.score,
            'result': evaluation.result.value,
            'should_retrain': evaluation.should_retrain
        })
        
    # Resumen
    print(f"\n{'=' * 50}")
    print("📈 SUMMARY")
    print(f"{'=' * 50}")
    
    avg_score = sum(r['score'] for r in results) / len(results) if results else 0
    excellent = sum(1 for r in results if r['result'] == 'excellent')
    good = sum(1 for r in results if r['result'] == 'good')
    trainable = sum(1 for r in results if r['should_retrain'])
    
    print(f"   Average Score: {avg_score:.0f}/100")
    print(f"   Excellent: {excellent}/{len(results)}")
    print(f"   Good: {good}/{len(results)}")
    print(f"   Usable for training: {trainable}/{len(results)}")


if __name__ == "__main__":
    main()
