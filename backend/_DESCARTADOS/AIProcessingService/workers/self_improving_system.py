#!/usr/bin/env python3
"""
Complete Self-Improving Vehicle Segmentation System
====================================================

Sistema completo que:
1. Procesa imágenes con pipeline V2
2. Evalúa automáticamente con IA (CLIP)
3. Clasifica resultados (aprobar/rechazar/revisar)
4. Almacena para reentrenamiento
5. Reentrena el modelo cuando hay suficientes datos

Flujo:
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   INPUT     │ ──► │  PIPELINE   │ ──► │  AI EVAL    │
│   Image     │     │  V2 Process │     │  CLIP Score │
└─────────────┘     └─────────────┘     └─────────────┘
                                              │
                    ┌─────────────────────────┼─────────────────────────┐
                    │                         │                         │
                    ▼                         ▼                         ▼
            ┌─────────────┐           ┌─────────────┐           ┌─────────────┐
            │  EXCELLENT  │           │  NEEDS      │           │  REJECTED   │
            │  Score ≥90  │           │  REVIEW     │           │  Score <60  │
            │  Auto-OK ✓  │           │  60≤Score<90│           │  Retry/Skip │
            └─────────────┘           └─────────────┘           └─────────────┘
                    │                         │                         │
                    ▼                         ▼                         ▼
            ┌─────────────┐           ┌─────────────┐           ┌─────────────┐
            │  TRAINING   │           │  HUMAN      │           │  PROBLEM    │
            │  DATA STORE │           │  REVIEW     │           │  ANALYSIS   │
            └─────────────┘           └─────────────┘           └─────────────┘
                    │                         │                         
                    └────────────┬────────────┘                         
                                 ▼                                      
                         ┌─────────────┐                               
                         │  FINE-TUNE  │                               
                         │  MODEL      │                               
                         └─────────────┘                               

Author: Gregory Moreno
Date: January 2026
"""

import cv2
import numpy as np
import torch
import sqlite3
import shutil
import json
from pathlib import Path
from datetime import datetime
from typing import Optional, Tuple, Dict, Any, List
from dataclasses import dataclass, asdict
from enum import Enum

# Import our modules
from pipeline_v2 import ProfessionalPipelineV2, PipelineConfig
from ai_evaluator import HybridEvaluator, AIEvaluation, EvaluationResult

# Paths
SCRIPT_DIR = Path(__file__).parent
DATA_DIR = SCRIPT_DIR / "learning_data"
MODELS_DIR = SCRIPT_DIR / "models"
DB_PATH = SCRIPT_DIR / "learning_system.db"

# Create directories
(DATA_DIR / "excellent").mkdir(parents=True, exist_ok=True)
(DATA_DIR / "good").mkdir(parents=True, exist_ok=True)
(DATA_DIR / "needs_review").mkdir(parents=True, exist_ok=True)
(DATA_DIR / "rejected").mkdir(parents=True, exist_ok=True)
(DATA_DIR / "training_ready").mkdir(parents=True, exist_ok=True)

DEVICE = "mps" if torch.backends.mps.is_available() else "cuda" if torch.cuda.is_available() else "cpu"


@dataclass
class ProcessingRecord:
    """Registro completo de procesamiento"""
    id: str
    timestamp: str
    
    # Input
    input_path: str
    input_hash: str
    
    # Output
    output_path: str
    mask_path: str
    
    # Pipeline metrics
    detection_confidence: float
    coverage_percent: float
    processing_time_ms: float
    
    # AI Evaluation
    ai_score: float
    ai_result: str
    ai_vehicle_complete: bool
    ai_wheels_intact: bool
    ai_no_shadows: bool
    ai_clean_edges: bool
    ai_issues: List[str]
    
    # Status
    auto_approved: bool
    human_reviewed: bool
    final_status: str  # excellent, good, needs_review, rejected
    
    # Training
    used_for_training: bool
    training_batch: Optional[str]


class LearningDatabase:
    """SQLite database for learning system"""
    
    def __init__(self, db_path: Path = DB_PATH):
        self.db_path = db_path
        self._init_db()
        
    def _init_db(self):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS processing_records (
                id TEXT PRIMARY KEY,
                timestamp TEXT,
                input_path TEXT,
                input_hash TEXT,
                output_path TEXT,
                mask_path TEXT,
                detection_confidence REAL,
                coverage_percent REAL,
                processing_time_ms REAL,
                ai_score REAL,
                ai_result TEXT,
                ai_vehicle_complete INTEGER,
                ai_wheels_intact INTEGER,
                ai_no_shadows INTEGER,
                ai_clean_edges INTEGER,
                ai_issues TEXT,
                auto_approved INTEGER,
                human_reviewed INTEGER,
                final_status TEXT,
                used_for_training INTEGER DEFAULT 0,
                training_batch TEXT
            )
        ''')
        
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS training_runs (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                timestamp TEXT,
                samples_used INTEGER,
                model_version TEXT,
                validation_score REAL,
                improvement_percent REAL
            )
        ''')
        
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS system_stats (
                date TEXT PRIMARY KEY,
                total_processed INTEGER,
                excellent_count INTEGER,
                good_count INTEGER,
                rejected_count INTEGER,
                avg_score REAL
            )
        ''')
        
        conn.commit()
        conn.close()
        
    def save_record(self, record: ProcessingRecord):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            INSERT OR REPLACE INTO processing_records VALUES (
                ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?
            )
        ''', (
            record.id,
            record.timestamp,
            record.input_path,
            record.input_hash,
            record.output_path,
            record.mask_path,
            record.detection_confidence,
            record.coverage_percent,
            record.processing_time_ms,
            record.ai_score,
            record.ai_result,
            1 if record.ai_vehicle_complete else 0,
            1 if record.ai_wheels_intact else 0,
            1 if record.ai_no_shadows else 0,
            1 if record.ai_clean_edges else 0,
            json.dumps(record.ai_issues),
            1 if record.auto_approved else 0,
            1 if record.human_reviewed else 0,
            record.final_status,
            1 if record.used_for_training else 0,
            record.training_batch
        ))
        
        conn.commit()
        conn.close()
        
    def get_training_candidates(self, limit: int = 100) -> List[Dict]:
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            SELECT * FROM processing_records
            WHERE (final_status = 'excellent' OR final_status = 'good')
            AND used_for_training = 0
            ORDER BY ai_score DESC
            LIMIT ?
        ''', (limit,))
        
        columns = [desc[0] for desc in cursor.description]
        results = [dict(zip(columns, row)) for row in cursor.fetchall()]
        
        conn.close()
        return results
        
    def get_problem_analysis(self) -> Dict[str, Any]:
        """Analyze rejected images to find patterns"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            SELECT 
                COUNT(*) as total,
                AVG(ai_score) as avg_score,
                SUM(CASE WHEN ai_wheels_intact = 0 THEN 1 ELSE 0 END) as wheel_issues,
                SUM(CASE WHEN ai_no_shadows = 0 THEN 1 ELSE 0 END) as shadow_issues,
                SUM(CASE WHEN ai_clean_edges = 0 THEN 1 ELSE 0 END) as edge_issues,
                SUM(CASE WHEN ai_vehicle_complete = 0 THEN 1 ELSE 0 END) as incomplete_issues
            FROM processing_records
            WHERE final_status = 'rejected' OR final_status = 'needs_review'
        ''')
        
        row = cursor.fetchone()
        conn.close()
        
        if row and row[0] > 0:
            total = row[0]
            return {
                'total_problems': total,
                'avg_score': row[1],
                'wheel_issues_pct': (row[2] / total * 100) if total > 0 else 0,
                'shadow_issues_pct': (row[3] / total * 100) if total > 0 else 0,
                'edge_issues_pct': (row[4] / total * 100) if total > 0 else 0,
                'incomplete_pct': (row[5] / total * 100) if total > 0 else 0
            }
        return {}
        
    def get_stats(self) -> Dict[str, Any]:
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            SELECT 
                COUNT(*) as total,
                SUM(CASE WHEN final_status = 'excellent' THEN 1 ELSE 0 END) as excellent,
                SUM(CASE WHEN final_status = 'good' THEN 1 ELSE 0 END) as good,
                SUM(CASE WHEN final_status = 'needs_review' THEN 1 ELSE 0 END) as needs_review,
                SUM(CASE WHEN final_status = 'rejected' THEN 1 ELSE 0 END) as rejected,
                AVG(ai_score) as avg_score,
                AVG(processing_time_ms) as avg_time
            FROM processing_records
        ''')
        
        row = cursor.fetchone()
        conn.close()
        
        if row:
            total = row[0] or 0
            return {
                'total_processed': total,
                'excellent': row[1] or 0,
                'good': row[2] or 0,
                'needs_review': row[3] or 0,
                'rejected': row[4] or 0,
                'avg_score': row[5] or 0,
                'avg_time_ms': row[6] or 0,
                'success_rate': ((row[1] or 0) + (row[2] or 0)) / total * 100 if total > 0 else 0
            }
        return {}


class SelfImprovingSystem:
    """
    Sistema completo de segmentación auto-mejorable
    """
    
    def __init__(self, use_vision_llm: bool = False):
        print("🚗 Initializing Self-Improving Vehicle Segmentation System")
        print("=" * 60)
        
        # Database
        self.db = LearningDatabase()
        
        # Pipeline
        print("\n📦 Loading segmentation pipeline...")
        self.pipeline = ProfessionalPipelineV2()
        self.pipeline.load_models()
        
        # AI Evaluator
        print("\n🤖 Loading AI evaluator...")
        self.evaluator = HybridEvaluator(use_vision_llm=use_vision_llm)
        self.evaluator.load_models()
        
        # Configuration
        self.auto_approve_threshold = 90  # Score >= 90 = auto approve
        self.reject_threshold = 60        # Score < 60 = reject
        self.retraining_threshold = 50    # Retrain when 50+ approved samples
        
        print("\n✅ System ready!")
        print(f"   Auto-approve threshold: {self.auto_approve_threshold}")
        print(f"   Reject threshold: {self.reject_threshold}")
        print(f"   Retraining threshold: {self.retraining_threshold} samples")
        
    def process_image(self, image_path: Path) -> Tuple[Optional[np.ndarray], ProcessingRecord]:
        """
        Process single image through complete pipeline
        
        Returns:
            (output_image, record)
        """
        import hashlib
        import time
        
        # Generate ID
        with open(image_path, 'rb') as f:
            input_hash = hashlib.md5(f.read()).hexdigest()[:8]
        record_id = f"{image_path.stem}_{input_hash}_{datetime.now().strftime('%H%M%S')}"
        
        print(f"\n{'─' * 50}")
        print(f"🚗 Processing: {image_path.name}")
        
        # Load image
        image = cv2.imread(str(image_path))
        if image is None:
            print(f"   ❌ Failed to load image")
            return None, None
            
        h, w = image.shape[:2]
        
        # Process with pipeline
        start_time = time.time()
        result, stats = self.pipeline.process_image(image_path)
        processing_time = (time.time() - start_time) * 1000
        
        if result is None:
            print(f"   ❌ Pipeline failed")
            return None, None
            
        # Calculate coverage
        alpha = result[:, :, 3]
        coverage = (alpha > 128).sum() / (h * w) * 100
        
        # AI Evaluation
        print(f"   🤖 AI Evaluation...", end=" ")
        evaluation = self.evaluator.evaluate(image, result)
        print(f"Score: {evaluation.score:.0f}/100")
        
        # Determine status
        if evaluation.score >= self.auto_approve_threshold:
            status = "excellent"
            auto_approved = True
            dest_dir = DATA_DIR / "excellent"
        elif evaluation.score >= 75:
            status = "good"
            auto_approved = True
            dest_dir = DATA_DIR / "good"
        elif evaluation.score >= self.reject_threshold:
            status = "needs_review"
            auto_approved = False
            dest_dir = DATA_DIR / "needs_review"
        else:
            status = "rejected"
            auto_approved = False
            dest_dir = DATA_DIR / "rejected"
            
        # Save output
        output_path = dest_dir / f"{record_id}_output.png"
        mask_path = dest_dir / f"{record_id}_mask.png"
        
        cv2.imwrite(str(output_path), result)
        cv2.imwrite(str(mask_path), alpha)
        
        # Also copy input for training
        input_copy = dest_dir / f"{record_id}_input.jpg"
        shutil.copy(image_path, input_copy)
        
        # Create record
        record = ProcessingRecord(
            id=record_id,
            timestamp=datetime.now().isoformat(),
            input_path=str(image_path),
            input_hash=input_hash,
            output_path=str(output_path),
            mask_path=str(mask_path),
            detection_confidence=stats.get('stages', {}).get('detection', {}).get('confidence', 0),
            coverage_percent=coverage,
            processing_time_ms=processing_time,
            ai_score=float(evaluation.score),  # Ensure float for SQLite
            ai_result=str(evaluation.result.value),
            ai_vehicle_complete=bool(evaluation.vehicle_complete),
            ai_wheels_intact=bool(evaluation.wheels_intact),
            ai_no_shadows=bool(evaluation.no_shadows),
            ai_clean_edges=bool(evaluation.clean_edges),
            ai_issues=evaluation.issues,
            auto_approved=auto_approved,
            human_reviewed=False,
            final_status=status,
            used_for_training=False,
            training_batch=None
        )
        
        # Save to database
        self.db.save_record(record)
        
        # Print result
        status_emoji = {
            'excellent': '🌟',
            'good': '✅',
            'needs_review': '⚠️',
            'rejected': '❌'
        }
        print(f"   {status_emoji.get(status, '?')} Status: {status.upper()}")
        print(f"   📊 Coverage: {coverage:.1f}% | Time: {processing_time:.0f}ms")
        
        if evaluation.issues:
            print(f"   ⚠️ Issues: {', '.join(evaluation.issues[:2])}")
            
        return result, record
        
    def process_directory(self, input_dir: Path) -> Dict[str, Any]:
        """Process all images in directory"""
        images = list(input_dir.glob("*.jpg")) + list(input_dir.glob("*.png"))
        
        print(f"\n🚗 Processing {len(images)} images...")
        
        results = {
            'total': len(images),
            'excellent': 0,
            'good': 0,
            'needs_review': 0,
            'rejected': 0,
            'failed': 0
        }
        
        for img_path in images:
            output, record = self.process_image(img_path)
            
            if record:
                results[record.final_status] = results.get(record.final_status, 0) + 1
            else:
                results['failed'] += 1
                
        return results
        
    def check_retraining_needed(self) -> bool:
        """Check if we have enough samples for retraining"""
        candidates = self.db.get_training_candidates()
        return len(candidates) >= self.retraining_threshold
        
    def prepare_training_data(self) -> Tuple[Path, int]:
        """Prepare data for model fine-tuning"""
        candidates = self.db.get_training_candidates(limit=100)
        
        if len(candidates) < 20:
            print(f"⚠️ Not enough samples ({len(candidates)}/20 minimum)")
            return None, 0
            
        training_dir = DATA_DIR / "training_ready"
        images_dir = training_dir / "images"
        labels_dir = training_dir / "labels"
        
        images_dir.mkdir(exist_ok=True)
        labels_dir.mkdir(exist_ok=True)
        
        prepared = 0
        for sample in candidates:
            try:
                input_path = Path(sample['input_path'])
                mask_path = Path(sample['mask_path'])
                
                if input_path.exists() and mask_path.exists():
                    # Copy image
                    shutil.copy(input_path, images_dir / input_path.name)
                    
                    # Create YOLO format label
                    mask = cv2.imread(str(mask_path), cv2.IMREAD_GRAYSCALE)
                    label = self._mask_to_yolo(mask)
                    
                    if label:
                        label_file = labels_dir / f"{input_path.stem}.txt"
                        with open(label_file, 'w') as f:
                            f.write(label)
                        prepared += 1
                        
            except Exception as e:
                print(f"   ⚠️ Error preparing {sample['id']}: {e}")
                
        print(f"✅ Prepared {prepared} samples for training")
        return training_dir, prepared
        
    def _mask_to_yolo(self, mask: np.ndarray) -> str:
        """Convert mask to YOLO segmentation format"""
        h, w = mask.shape
        binary = (mask > 128).astype(np.uint8)
        
        contours, _ = cv2.findContours(binary, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        
        if not contours:
            return ""
            
        largest = max(contours, key=cv2.contourArea)
        epsilon = 0.002 * cv2.arcLength(largest, True)
        approx = cv2.approxPolyDP(largest, epsilon, True)
        
        if len(approx) < 4:
            return ""
            
        points = []
        for pt in approx.reshape(-1, 2):
            points.extend([f"{pt[0]/w:.6f}", f"{pt[1]/h:.6f}"])
            
        return f"0 {' '.join(points)}"
        
    def get_improvement_report(self) -> Dict[str, Any]:
        """Generate improvement report"""
        stats = self.db.get_stats()
        problems = self.db.get_problem_analysis()
        candidates = self.db.get_training_candidates()
        
        return {
            'overall_stats': stats,
            'problem_analysis': problems,
            'training_ready': len(candidates),
            'recommendations': self._generate_recommendations(problems)
        }
        
    def _generate_recommendations(self, problems: Dict) -> List[str]:
        """Generate recommendations based on problem analysis"""
        recommendations = []
        
        if problems.get('wheel_issues_pct', 0) > 30:
            recommendations.append("🔧 PRIORITY: Improve wheel detection - enable Hough circle detection")
            
        if problems.get('shadow_issues_pct', 0) > 30:
            recommendations.append("🔧 PRIORITY: Improve shadow removal - lower shadow threshold")
            
        if problems.get('edge_issues_pct', 0) > 20:
            recommendations.append("💡 Consider: Increase edge feathering radius")
            
        if problems.get('incomplete_pct', 0) > 20:
            recommendations.append("💡 Consider: Lower detection confidence threshold")
            
        if not recommendations:
            recommendations.append("✅ Pipeline performing well - no major issues detected")
            
        return recommendations


def main():
    """Demo del sistema completo"""
    print("=" * 60)
    print("🚗 SELF-IMPROVING VEHICLE SEGMENTATION SYSTEM")
    print("=" * 60)
    
    # Initialize system
    system = SelfImprovingSystem(use_vision_llm=False)
    
    # Process test images
    input_dir = SCRIPT_DIR / "input"
    results = system.process_directory(input_dir)
    
    # Print results
    print(f"\n{'=' * 60}")
    print("📊 PROCESSING RESULTS")
    print(f"{'=' * 60}")
    print(f"   Total: {results['total']}")
    print(f"   🌟 Excellent: {results['excellent']}")
    print(f"   ✅ Good: {results['good']}")
    print(f"   ⚠️ Needs Review: {results['needs_review']}")
    print(f"   ❌ Rejected: {results['rejected']}")
    
    success_rate = (results['excellent'] + results['good']) / results['total'] * 100 if results['total'] > 0 else 0
    print(f"\n   📈 Success Rate: {success_rate:.1f}%")
    
    # Get improvement report
    report = system.get_improvement_report()
    
    print(f"\n{'=' * 60}")
    print("🔍 IMPROVEMENT ANALYSIS")
    print(f"{'=' * 60}")
    
    if report['problem_analysis']:
        problems = report['problem_analysis']
        print(f"   Problem Images: {problems.get('total_problems', 0)}")
        print(f"   Wheel Issues: {problems.get('wheel_issues_pct', 0):.1f}%")
        print(f"   Shadow Issues: {problems.get('shadow_issues_pct', 0):.1f}%")
        print(f"   Edge Issues: {problems.get('edge_issues_pct', 0):.1f}%")
        
    print(f"\n   Training Ready: {report['training_ready']} samples")
    
    print(f"\n📝 RECOMMENDATIONS:")
    for rec in report['recommendations']:
        print(f"   {rec}")
        
    # Check if retraining is needed
    if system.check_retraining_needed():
        print(f"\n🔄 RETRAINING AVAILABLE")
        print(f"   {report['training_ready']} samples ready for fine-tuning")
        print(f"   Run: system.prepare_training_data()")


if __name__ == "__main__":
    main()
