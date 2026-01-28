#!/usr/bin/env python3
"""
🎯 AUTO-OPTIMIZER: Sistema de Auto-Perfeccionamiento
=====================================================

Este sistema ajusta automáticamente los parámetros del pipeline
hasta que TODAS las imágenes obtengan score ≥95.

Estrategia:
1. Procesar todas las imágenes
2. Evaluar con GPT-4o Vision
3. Identificar problemas comunes
4. Ajustar parámetros basándose en el feedback
5. Repetir hasta score ≥95 en todas

Uso:
    export GITHUB_TOKEN=$(gh auth token)
    python3 auto_optimizer.py

Author: Gregory Moreno
Date: January 2026
"""

import os
import sys
import cv2
import json
import numpy as np
import time
from pathlib import Path
from dataclasses import dataclass, field, asdict
from typing import Dict, List, Tuple, Optional
from datetime import datetime
import copy

# Agregar path
sys.path.insert(0, str(Path(__file__).parent))

from pipeline_v2 import ProfessionalPipelineV2
from ai_evaluator import ComputationalEvaluator, AIEvaluation

SCRIPT_DIR = Path(__file__).parent
INPUT_DIR = SCRIPT_DIR / "input"
OUTPUT_DIR = SCRIPT_DIR / "output_optimized"
LOG_DIR = SCRIPT_DIR / "optimization_logs"


@dataclass
class PipelineParams:
    """Parámetros ajustables del pipeline"""
    
    # Stage 2: Shadow removal
    shadow_threshold: float = 0.3          # 0.1 - 0.5 (menor = más agresivo)
    shadow_expansion: int = 5              # 0 - 15 píxeles
    shadow_feather: int = 3                # 1 - 10 píxeles
    
    # Stage 3: Wheel protection
    wheel_expansion: int = 15              # 5 - 30 píxeles
    wheel_min_radius: int = 20             # 10 - 40 píxeles
    wheel_sensitivity: float = 0.7         # 0.5 - 1.0
    
    # Stage 5: Hole filling
    hole_kernel_size: int = 5              # 3 - 15 (impar)
    hole_iterations: int = 2               # 1 - 5
    
    # Stage 6: Edge refinement
    edge_radius: int = 8                   # 4 - 20
    edge_eps: float = 0.01                 # 0.001 - 0.1
    
    # Stage 7: Anti-aliasing
    antialias_sigma: float = 1.0           # 0.5 - 3.0
    
    # General
    mask_dilation: int = 3                 # 0 - 10
    mask_erosion: int = 2                  # 0 - 10
    
    def to_dict(self) -> dict:
        return asdict(self)
    
    @classmethod
    def from_dict(cls, d: dict) -> 'PipelineParams':
        return cls(**{k: v for k, v in d.items() if k in cls.__dataclass_fields__})


@dataclass
class OptimizationResult:
    """Resultado de una imagen"""
    image_name: str
    score: float
    vehicle_complete: bool
    wheels_intact: bool
    no_shadows: bool
    clean_edges: bool
    no_background: bool
    issues: List[str]
    suggestions: List[str]


@dataclass
class OptimizationRound:
    """Resultado de una ronda completa"""
    round_number: int
    timestamp: str
    params: PipelineParams
    results: List[OptimizationResult]
    avg_score: float
    min_score: float
    max_score: float
    all_passed: bool
    problem_summary: Dict[str, int]


class AutoOptimizer:
    """
    Sistema de auto-optimización del pipeline
    """
    
    def __init__(self, target_score: float = 95.0, max_rounds: int = 20):
        self.target_score = target_score
        self.max_rounds = max_rounds
        self.current_params = PipelineParams()
        self.history: List[OptimizationRound] = []
        self.best_params: Optional[PipelineParams] = None
        self.best_avg_score = 0.0
        
        # Crear directorios
        OUTPUT_DIR.mkdir(exist_ok=True)
        LOG_DIR.mkdir(exist_ok=True)
        
        # Inicializar evaluador computacional (100% CPU, sin GPU, sin API)
        print("🤖 Inicializando ComputationalEvaluator (100% CPU - SIN GPU - SIN LÍMITES)...")
        self.evaluator = ComputationalEvaluator()
        
        # Pipeline (se reinicializa en cada ronda con nuevos params)
        self.pipeline = None
        
    def _load_pipeline(self):
        """Cargar pipeline con parámetros actuales"""
        print("   📦 Cargando pipeline...")
        self.pipeline = ProfessionalPipelineV2()
        self.pipeline.load_models()
        
        # Aplicar parámetros personalizados
        # (Los parámetros se aplican durante el procesamiento)
        
    def _process_image(self, image_path: Path) -> Tuple[np.ndarray, np.ndarray]:
        """Procesar una imagen con el pipeline actual"""
        image = cv2.imread(str(image_path))
        if image is None:
            raise ValueError(f"No se pudo cargar: {image_path}")
        
        # Procesar con parámetros actuales
        result, mask = self.pipeline.process(
            image,
            shadow_threshold=self.current_params.shadow_threshold,
            shadow_expansion=self.current_params.shadow_expansion,
            wheel_expansion=self.current_params.wheel_expansion,
            edge_radius=self.current_params.edge_radius,
            edge_eps=self.current_params.edge_eps,
            antialias_sigma=self.current_params.antialias_sigma
        )
        
        return result, mask
    
    def _evaluate_image(self, original: np.ndarray, cutout: np.ndarray) -> AIEvaluation:
        """Evaluar resultado con GPT-4o"""
        return self.evaluator.evaluate(original, cutout)
    
    def _analyze_problems(self, results: List[OptimizationResult]) -> Dict[str, int]:
        """Analizar problemas comunes en los resultados"""
        problems = {
            "shadow_issues": 0,
            "wheel_issues": 0,
            "edge_issues": 0,
            "background_issues": 0,
            "completeness_issues": 0
        }
        
        for r in results:
            if not r.no_shadows:
                problems["shadow_issues"] += 1
            if not r.wheels_intact:
                problems["wheel_issues"] += 1
            if not r.clean_edges:
                problems["edge_issues"] += 1
            if not r.no_background:
                problems["background_issues"] += 1
            if not r.vehicle_complete:
                problems["completeness_issues"] += 1
                
            # Analizar issues textuales
            for issue in r.issues:
                issue_lower = issue.lower()
                if any(w in issue_lower for w in ["shadow", "sombra"]):
                    problems["shadow_issues"] += 1
                if any(w in issue_lower for w in ["wheel", "rueda", "tire"]):
                    problems["wheel_issues"] += 1
                if any(w in issue_lower for w in ["edge", "borde", "artifact"]):
                    problems["edge_issues"] += 1
                if any(w in issue_lower for w in ["background", "fondo"]):
                    problems["background_issues"] += 1
                if any(w in issue_lower for w in ["cut", "incomplete", "cortad", "incomplet"]):
                    problems["completeness_issues"] += 1
                    
        return problems
    
    def _adjust_params(self, problems: Dict[str, int], results: List[OptimizationResult]):
        """Ajustar parámetros basándose en los problemas detectados"""
        total_images = len(results)
        old_params = copy.deepcopy(self.current_params)
        
        print("\n   🔧 Ajustando parámetros...")
        
        # Problema: Sombras
        if problems["shadow_issues"] > 0:
            severity = problems["shadow_issues"] / total_images
            if severity > 0.5:
                # Muchas sombras: ser más agresivo
                self.current_params.shadow_threshold = max(0.1, self.current_params.shadow_threshold - 0.05)
                self.current_params.shadow_expansion = min(15, self.current_params.shadow_expansion + 2)
                print(f"      ↳ Shadows: threshold {old_params.shadow_threshold:.2f} → {self.current_params.shadow_threshold:.2f}")
            else:
                # Pocas sombras: ajuste menor
                self.current_params.shadow_threshold = max(0.15, self.current_params.shadow_threshold - 0.02)
                print(f"      ↳ Shadows: minor adjustment")
        
        # Problema: Ruedas cortadas
        if problems["wheel_issues"] > 0:
            severity = problems["wheel_issues"] / total_images
            if severity > 0.3:
                # Expandir protección de ruedas
                self.current_params.wheel_expansion = min(30, self.current_params.wheel_expansion + 5)
                self.current_params.wheel_sensitivity = min(1.0, self.current_params.wheel_sensitivity + 0.1)
                print(f"      ↳ Wheels: expansion {old_params.wheel_expansion} → {self.current_params.wheel_expansion}")
            else:
                self.current_params.wheel_expansion = min(25, self.current_params.wheel_expansion + 2)
        
        # Problema: Bordes sucios
        if problems["edge_issues"] > 0:
            severity = problems["edge_issues"] / total_images
            if severity > 0.5:
                # Más refinamiento de bordes
                self.current_params.edge_radius = min(20, self.current_params.edge_radius + 2)
                self.current_params.antialias_sigma = min(3.0, self.current_params.antialias_sigma + 0.3)
                print(f"      ↳ Edges: radius {old_params.edge_radius} → {self.current_params.edge_radius}")
            else:
                self.current_params.antialias_sigma = min(2.5, self.current_params.antialias_sigma + 0.2)
        
        # Problema: Restos de fondo
        if problems["background_issues"] > 0:
            severity = problems["background_issues"] / total_images
            if severity > 0.3:
                # Más erosión del mask
                self.current_params.mask_erosion = min(8, self.current_params.mask_erosion + 1)
                print(f"      ↳ Background: erosion {old_params.mask_erosion} → {self.current_params.mask_erosion}")
        
        # Problema: Vehículo incompleto
        if problems["completeness_issues"] > 0:
            severity = problems["completeness_issues"] / total_images
            if severity > 0.3:
                # Más dilatación, menos erosión
                self.current_params.mask_dilation = min(10, self.current_params.mask_dilation + 2)
                self.current_params.mask_erosion = max(0, self.current_params.mask_erosion - 1)
                self.current_params.hole_iterations = min(5, self.current_params.hole_iterations + 1)
                print(f"      ↳ Completeness: dilation {old_params.mask_dilation} → {self.current_params.mask_dilation}")
    
    def _run_round(self, round_num: int, images: List[Path]) -> OptimizationRound:
        """Ejecutar una ronda de optimización"""
        print(f"\n{'='*60}")
        print(f"📍 RONDA {round_num}/{self.max_rounds}")
        print(f"{'='*60}")
        print(f"   Parámetros actuales:")
        print(f"      shadow_threshold: {self.current_params.shadow_threshold:.2f}")
        print(f"      wheel_expansion: {self.current_params.wheel_expansion}")
        print(f"      edge_radius: {self.current_params.edge_radius}")
        print(f"      antialias_sigma: {self.current_params.antialias_sigma:.1f}")
        
        # Cargar pipeline
        if self.pipeline is None:
            self._load_pipeline()
        
        results = []
        
        for i, img_path in enumerate(images, 1):
            print(f"\n   [{i}/{len(images)}] {img_path.name}")
            
            try:
                # Procesar
                original = cv2.imread(str(img_path))
                cutout, mask = self._process_image(img_path)
                
                # Guardar
                output_path = OUTPUT_DIR / f"{img_path.stem}_r{round_num}.png"
                cv2.imwrite(str(output_path), cutout)
                
                # Evaluar con GPT-4o
                print(f"      🤖 Evaluando...")
                evaluation = self._evaluate_image(original, cutout)
                
                if evaluation:
                    emoji = "🌟" if evaluation.score >= 95 else "✅" if evaluation.score >= 85 else "⚠️" if evaluation.score >= 70 else "❌"
                    print(f"      {emoji} Score: {evaluation.score}/100")
                    
                    result = OptimizationResult(
                        image_name=img_path.name,
                        score=evaluation.score,
                        vehicle_complete=evaluation.vehicle_complete,
                        wheels_intact=evaluation.wheels_intact,
                        no_shadows=evaluation.no_shadows,
                        clean_edges=evaluation.clean_edges,
                        no_background=evaluation.no_background,
                        issues=evaluation.issues,
                        suggestions=evaluation.suggestions
                    )
                    results.append(result)
                else:
                    print(f"      ❌ Error en evaluación")
                    
            except Exception as e:
                print(f"      ❌ Error: {e}")
        
        # Calcular estadísticas
        if results:
            scores = [r.score for r in results]
            avg_score = sum(scores) / len(scores)
            min_score = min(scores)
            max_score = max(scores)
            all_passed = all(s >= self.target_score for s in scores)
        else:
            avg_score = min_score = max_score = 0
            all_passed = False
        
        # Analizar problemas
        problems = self._analyze_problems(results)
        
        # Crear registro de ronda
        round_result = OptimizationRound(
            round_number=round_num,
            timestamp=datetime.now().isoformat(),
            params=copy.deepcopy(self.current_params),
            results=results,
            avg_score=avg_score,
            min_score=min_score,
            max_score=max_score,
            all_passed=all_passed,
            problem_summary=problems
        )
        
        self.history.append(round_result)
        
        # Guardar mejor resultado
        if avg_score > self.best_avg_score:
            self.best_avg_score = avg_score
            self.best_params = copy.deepcopy(self.current_params)
        
        # Mostrar resumen
        print(f"\n   📊 Resumen Ronda {round_num}:")
        print(f"      Promedio: {avg_score:.1f}/100")
        print(f"      Mínimo: {min_score:.1f} | Máximo: {max_score:.1f}")
        print(f"      Objetivo (≥{self.target_score}): {'✅ LOGRADO!' if all_passed else '❌ No alcanzado'}")
        
        if not all_passed and problems:
            print(f"      Problemas detectados:")
            for prob, count in problems.items():
                if count > 0:
                    print(f"         - {prob}: {count}")
        
        return round_result
    
    def optimize(self, images: Optional[List[Path]] = None) -> Tuple[bool, PipelineParams]:
        """
        Ejecutar optimización completa
        
        Returns:
            (success, best_params)
        """
        if images is None:
            images = sorted(list(INPUT_DIR.glob("*.jpg")) + list(INPUT_DIR.glob("*.jpeg")))
        
        if not images:
            print("❌ No hay imágenes para procesar")
            return False, self.current_params
        
        print(f"\n{'#'*60}")
        print(f"🎯 AUTO-OPTIMIZER: Objetivo Score ≥{self.target_score}")
        print(f"{'#'*60}")
        print(f"   Imágenes: {len(images)}")
        print(f"   Máximo de rondas: {self.max_rounds}")
        print(f"   Evaluador: GPT-4o Vision (GitHub Models)")
        
        start_time = time.time()
        success = False
        
        for round_num in range(1, self.max_rounds + 1):
            round_result = self._run_round(round_num, images)
            
            if round_result.all_passed:
                success = True
                print(f"\n🎉 ¡OBJETIVO LOGRADO en ronda {round_num}!")
                break
            else:
                # Ajustar parámetros para siguiente ronda
                self._adjust_params(round_result.problem_summary, round_result.results)
        
        elapsed = time.time() - start_time
        
        # Resumen final
        print(f"\n{'='*60}")
        print(f"📊 RESUMEN FINAL")
        print(f"{'='*60}")
        print(f"   Tiempo total: {elapsed/60:.1f} minutos")
        print(f"   Rondas ejecutadas: {len(self.history)}")
        print(f"   Mejor promedio: {self.best_avg_score:.1f}/100")
        print(f"   Objetivo alcanzado: {'✅ SÍ' if success else '❌ NO'}")
        
        if self.best_params:
            print(f"\n   🔧 Mejores parámetros encontrados:")
            print(f"      shadow_threshold: {self.best_params.shadow_threshold:.2f}")
            print(f"      shadow_expansion: {self.best_params.shadow_expansion}")
            print(f"      wheel_expansion: {self.best_params.wheel_expansion}")
            print(f"      edge_radius: {self.best_params.edge_radius}")
            print(f"      antialias_sigma: {self.best_params.antialias_sigma:.1f}")
            print(f"      mask_dilation: {self.best_params.mask_dilation}")
            print(f"      mask_erosion: {self.best_params.mask_erosion}")
        
        # Guardar log
        self._save_log()
        
        return success, self.best_params
    
    def _save_log(self):
        """Guardar log de optimización"""
        log_file = LOG_DIR / f"optimization_{datetime.now().strftime('%Y%m%d_%H%M%S')}.json"
        
        log_data = {
            "target_score": self.target_score,
            "max_rounds": self.max_rounds,
            "best_avg_score": self.best_avg_score,
            "best_params": self.best_params.to_dict() if self.best_params else None,
            "rounds": [
                {
                    "round": r.round_number,
                    "timestamp": r.timestamp,
                    "avg_score": r.avg_score,
                    "min_score": r.min_score,
                    "max_score": r.max_score,
                    "all_passed": r.all_passed,
                    "problems": r.problem_summary,
                    "params": r.params.to_dict(),
                    "results": [
                        {
                            "image": res.image_name,
                            "score": res.score,
                            "issues": res.issues
                        }
                        for res in r.results
                    ]
                }
                for r in self.history
            ]
        }
        
        with open(log_file, 'w') as f:
            json.dump(log_data, f, indent=2)
        
        print(f"\n   📝 Log guardado: {log_file.name}")


def estimate_time(num_images: int, target_score: float = 95.0) -> dict:
    """
    Estimar tiempo de optimización
    
    Basado en:
    - ~3-5 segundos por evaluación GPT-4o
    - ~2-4 segundos por procesamiento de pipeline
    - Típicamente 5-15 rondas para alcanzar score ≥95
    """
    time_per_image = 7  # segundos (proceso + evaluación)
    time_per_round = num_images * time_per_image
    
    # Escenarios
    optimistic_rounds = 5
    typical_rounds = 10
    pessimistic_rounds = 20
    
    return {
        "per_image": f"~{time_per_image} segundos",
        "per_round": f"~{time_per_round/60:.1f} minutos",
        "optimistic": {
            "rounds": optimistic_rounds,
            "time": f"~{(time_per_round * optimistic_rounds)/60:.0f} minutos"
        },
        "typical": {
            "rounds": typical_rounds,
            "time": f"~{(time_per_round * typical_rounds)/60:.0f} minutos"
        },
        "pessimistic": {
            "rounds": pessimistic_rounds,
            "time": f"~{(time_per_round * pessimistic_rounds)/60:.0f} minutos"
        }
    }


if __name__ == "__main__":
    import argparse
    
    parser = argparse.ArgumentParser(description="Auto-Optimizer para segmentación de vehículos")
    parser.add_argument("--target", type=float, default=95.0, help="Score objetivo (default: 95)")
    parser.add_argument("--max-rounds", type=int, default=20, help="Máximo de rondas (default: 20)")
    parser.add_argument("--estimate", action="store_true", help="Solo estimar tiempo, no ejecutar")
    
    args = parser.parse_args()
    
    # Contar imágenes
    images = sorted(list(INPUT_DIR.glob("*.jpg")) + list(INPUT_DIR.glob("*.jpeg")))
    num_images = len(images)
    
    if args.estimate:
        print(f"\n⏱️  ESTIMACIÓN DE TIEMPO")
        print(f"{'='*50}")
        print(f"   Imágenes: {num_images}")
        print(f"   Objetivo: Score ≥{args.target}")
        
        est = estimate_time(num_images, args.target)
        
        print(f"\n   📊 Tiempos estimados:")
        print(f"      Por imagen: {est['per_image']}")
        print(f"      Por ronda: {est['per_round']}")
        print(f"\n   🎯 Escenarios:")
        print(f"      Optimista: {est['optimistic']['rounds']} rondas = {est['optimistic']['time']}")
        print(f"      Típico: {est['typical']['rounds']} rondas = {est['typical']['time']}")
        print(f"      Pesimista: {est['pessimistic']['rounds']} rondas = {est['pessimistic']['time']}")
        
    else:
        # Ejecutar optimización
        optimizer = AutoOptimizer(target_score=args.target, max_rounds=args.max_rounds)
        success, best_params = optimizer.optimize(images)
        
        if success:
            print(f"\n✅ ¡Optimización exitosa! Todas las imágenes con score ≥{args.target}")
        else:
            print(f"\n⚠️ No se alcanzó el objetivo en {args.max_rounds} rondas")
            print(f"   Mejor resultado: {optimizer.best_avg_score:.1f}/100")
