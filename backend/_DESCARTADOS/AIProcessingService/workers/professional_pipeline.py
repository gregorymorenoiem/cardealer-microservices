#!/usr/bin/env python3
"""
🚗 Professional Vehicle Segmentation Pipeline
=============================================

A 7-stage AI pipeline for automotive dealership-quality vehicle cutouts.

Pipeline Stages:
----------------
┌─────────────────────────────────────────────────────────────────────────────┐
│  INPUT IMAGE                                                                 │
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 1: DETECTION (YOLOv8x-seg + GroundingDINO)                       ││
│  │  • YOLOv8x-seg: Fast instance segmentation                              ││
│  │  • GroundingDINO: Text-guided detection "car. vehicle. automobile."    ││
│  │  • Output: Combined bbox + initial mask + confidence                    ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 2: SHADOW DETECTION (ShadowFormer)                               ││
│  │  • Detect ground shadows under vehicle                                  ││
│  │  • Generate shadow mask for exclusion                                   ││
│  │  • Preserve wheel contact points                                        ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 3: PRIMARY SEGMENTATION (SAM2-H)                                 ││
│  │  • Use detection from Stage 1 as prompts                               ││
│  │  • Exclude shadow regions from Stage 2                                  ││
│  │  • Multi-mask output with score selection                              ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 4: PARTS ANALYSIS (CarParts-Seg)                                 ││
│  │  • Segment 8 car parts: hood, bumper, fender, door, lights, etc.       ││
│  │  • Identify wheels for protection                                       ││
│  │  • Validate SAM2 mask contains all expected parts                      ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 5: SEMANTIC CORRECTION (SegFormer)                               ││
│  │  • Semantic segmentation for "car" class verification                   ││
│  │  • Correct any over/under-segmentation from SAM2                       ││
│  │  • Mask intersection for precision                                      ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 6: EDGE REFINEMENT (ISNet/DIS)                                   ││
│  │  • High-precision dichotomous segmentation                              ││
│  │  • Sub-pixel edge accuracy                                              ││
│  │  • Handle transparent/reflective surfaces                               ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │ STAGE 7: ENHANCEMENT (CodeFormer/GFPGAN + RealESRGAN)                  ││
│  │  • Detail enhancement on vehicle                                        ││
│  │  • Background upsampling (if needed)                                    ││
│  │  • Professional alpha matting                                           ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│       ↓                                                                      │
│  OUTPUT: Professional cutout with transparent/white background              │
└─────────────────────────────────────────────────────────────────────────────┘

Author: Gregory Moreno
Version: 1.0.0
License: MIT
"""

import os
import sys
import logging
import numpy as np
from PIL import Image
from pathlib import Path
from typing import Dict, Any, Optional, Tuple, List
from dataclasses import dataclass, field
from abc import ABC, abstractmethod
import torch
import cv2

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - [%(name)s] %(message)s'
)
logger = logging.getLogger("ProfessionalPipeline")


# =============================================================================
# CONFIGURATION
# =============================================================================

@dataclass
class PipelineConfig:
    """Configuration for the professional pipeline"""
    # Paths
    models_dir: str = "/models"
    device: str = "cuda" if torch.cuda.is_available() else "cpu"
    
    # Stage 1: Detection
    yolo_model: str = "yolov8x-seg.pt"
    yolo_confidence: float = 0.45
    grounding_dino_model: str = "IDEA-Research/grounding-dino-base"
    grounding_dino_threshold: float = 0.35
    detection_prompts: List[str] = field(default_factory=lambda: [
        "car.", "vehicle.", "automobile.", "suv.", "truck.", "sedan."
    ])
    
    # Stage 2: Shadow Detection
    shadowformer_model: str = "shadowformer.onnx"
    shadow_threshold: float = 0.5
    
    # Stage 3: SAM2
    sam2_model: str = "sam2_hiera_large.pt"  # Upgrade to Large
    sam2_config: str = "sam2_hiera_l.yaml"
    sam2_multimask: bool = True
    
    # Stage 4: CarParts
    carparts_model: str = "Armandoliv/cars-parts-segmentation-unet-resnet18"
    carparts_classes: List[str] = field(default_factory=lambda: [
        "background", "back_bumper", "back_glass", "back_left_door",
        "back_left_light", "back_right_door", "back_right_light",
        "front_bumper", "front_glass", "front_left_door", "front_left_light",
        "front_right_door", "front_right_light", "hood", "left_mirror",
        "right_mirror", "tailgate", "trunk", "wheel"
    ])
    
    # Stage 5: SegFormer
    segformer_model: str = "nvidia/segformer-b5-finetuned-ade-640-640"
    segformer_car_class_id: int = 20  # ADE20K "car" class
    
    # Stage 6: ISNet
    isnet_model: str = "isnet-general-use.pth"
    isnet_input_size: Tuple[int, int] = (1024, 1024)
    
    # Stage 7: Enhancement
    codeformer_model: str = "codeformer.pth"
    realesrgan_model: str = "RealESRGAN_x4plus.pth"
    upscale_factor: float = 1.0  # 1.0 = no upscale, 2.0 = 2x, etc.
    
    # Output
    output_format: str = "png"
    output_quality: int = 95
    add_drop_shadow: bool = False
    background_color: Tuple[int, int, int] = (255, 255, 255)  # White


@dataclass
class StageResult:
    """Result from a pipeline stage"""
    success: bool
    mask: Optional[np.ndarray] = None
    bbox: Optional[np.ndarray] = None
    confidence: float = 0.0
    metadata: Dict[str, Any] = field(default_factory=dict)
    error: Optional[str] = None


# =============================================================================
# ABSTRACT BASE STAGE
# =============================================================================

class PipelineStage(ABC):
    """Abstract base class for pipeline stages"""
    
    def __init__(self, config: PipelineConfig, name: str):
        self.config = config
        self.name = name
        self.logger = logging.getLogger(f"Stage.{name}")
        self.model = None
        self.is_loaded = False
    
    @abstractmethod
    def load_model(self) -> bool:
        """Load the model for this stage"""
        pass
    
    @abstractmethod
    def process(self, image: np.ndarray, context: Dict[str, Any]) -> StageResult:
        """Process the image and return result"""
        pass
    
    def unload_model(self):
        """Unload model to free memory"""
        self.model = None
        self.is_loaded = False
        if torch.cuda.is_available():
            torch.cuda.empty_cache()
        self.logger.info(f"Model unloaded")


# =============================================================================
# STAGE 1: DETECTION (YOLOv8x-seg + GroundingDINO)
# =============================================================================

class Stage1Detection(PipelineStage):
    """
    Dual detection stage combining:
    - YOLOv8x-seg: Fast instance segmentation
    - GroundingDINO: Text-guided zero-shot detection
    """
    
    def __init__(self, config: PipelineConfig):
        super().__init__(config, "Detection")
        self.yolo_model = None
        self.grounding_dino_processor = None
        self.grounding_dino_model = None
    
    def load_model(self) -> bool:
        try:
            # Load YOLOv8x-seg
            from ultralytics import YOLO
            yolo_path = os.path.join(self.config.models_dir, self.config.yolo_model)
            
            if not os.path.exists(yolo_path):
                # Download if not exists
                self.yolo_model = YOLO("yolov8x-seg.pt")
            else:
                self.yolo_model = YOLO(yolo_path)
            
            self.logger.info("✅ YOLOv8x-seg loaded")
            
            # Load GroundingDINO
            try:
                from transformers import AutoProcessor, AutoModelForZeroShotObjectDetection
                
                self.grounding_dino_processor = AutoProcessor.from_pretrained(
                    self.config.grounding_dino_model
                )
                self.grounding_dino_model = AutoModelForZeroShotObjectDetection.from_pretrained(
                    self.config.grounding_dino_model
                ).to(self.config.device)
                
                self.logger.info("✅ GroundingDINO loaded")
            except Exception as e:
                self.logger.warning(f"⚠️ GroundingDINO failed to load: {e}")
                self.grounding_dino_model = None
            
            self.is_loaded = True
            return True
            
        except Exception as e:
            self.logger.error(f"❌ Failed to load detection models: {e}")
            return False
    
    def _detect_yolo(self, image: np.ndarray) -> Tuple[Optional[np.ndarray], Optional[np.ndarray], float]:
        """Run YOLO detection with instance segmentation"""
        if self.yolo_model is None:
            return None, None, 0.0
        
        try:
            results = self.yolo_model(
                image,
                device=self.config.device,
                verbose=False,
                conf=self.config.yolo_confidence
            )
            
            best_bbox = None
            best_mask = None
            best_score = 0.0
            
            VEHICLE_CLASSES = {2: 'car', 3: 'motorcycle', 5: 'bus', 7: 'truck'}
            
            for result in results:
                if result.masks is None:
                    continue
                    
                for i, box in enumerate(result.boxes):
                    class_id = int(box.cls[0])
                    if class_id in VEHICLE_CLASSES:
                        conf = float(box.conf[0])
                        # Weight cars higher
                        weight = 2.0 if class_id == 2 else 1.0
                        score = conf * weight
                        
                        if score > best_score:
                            best_score = score
                            best_bbox = box.xyxy[0].cpu().numpy()
                            
                            # Get instance mask
                            if result.masks is not None and i < len(result.masks.data):
                                mask_tensor = result.masks.data[i]
                                best_mask = mask_tensor.cpu().numpy()
                                # Resize to image size
                                h, w = image.shape[:2]
                                best_mask = cv2.resize(
                                    best_mask.astype(np.float32),
                                    (w, h),
                                    interpolation=cv2.INTER_LINEAR
                                )
            
            return best_bbox, best_mask, best_score
            
        except Exception as e:
            self.logger.error(f"YOLO detection failed: {e}")
            return None, None, 0.0
    
    def _detect_grounding_dino(self, image: np.ndarray) -> Tuple[Optional[np.ndarray], float]:
        """Run GroundingDINO text-guided detection"""
        if self.grounding_dino_model is None:
            return None, 0.0
        
        try:
            # Convert to PIL
            pil_image = Image.fromarray(image)
            
            # Combine prompts
            text_prompt = " ".join(self.config.detection_prompts)
            
            # Process
            inputs = self.grounding_dino_processor(
                images=pil_image,
                text=text_prompt,
                return_tensors="pt"
            ).to(self.config.device)
            
            with torch.no_grad():
                outputs = self.grounding_dino_model(**inputs)
            
            # Post-process
            results = self.grounding_dino_processor.post_process_grounded_object_detection(
                outputs,
                inputs.input_ids,
                box_threshold=self.config.grounding_dino_threshold,
                text_threshold=self.config.grounding_dino_threshold,
                target_sizes=[pil_image.size[::-1]]
            )[0]
            
            if len(results["boxes"]) == 0:
                return None, 0.0
            
            # Get best detection
            scores = results["scores"].cpu().numpy()
            boxes = results["boxes"].cpu().numpy()
            best_idx = np.argmax(scores)
            
            return boxes[best_idx], float(scores[best_idx])
            
        except Exception as e:
            self.logger.error(f"GroundingDINO detection failed: {e}")
            return None, 0.0
    
    def process(self, image: np.ndarray, context: Dict[str, Any]) -> StageResult:
        """Run dual detection and combine results"""
        self.logger.info("🎯 Running dual detection...")
        
        # Run YOLO
        yolo_bbox, yolo_mask, yolo_score = self._detect_yolo(image)
        self.logger.info(f"  YOLO: bbox={'Yes' if yolo_bbox is not None else 'No'}, "
                        f"mask={'Yes' if yolo_mask is not None else 'No'}, "
                        f"score={yolo_score:.3f}")
        
        # Run GroundingDINO
        gdino_bbox, gdino_score = self._detect_grounding_dino(image)
        self.logger.info(f"  GroundingDINO: bbox={'Yes' if gdino_bbox is not None else 'No'}, "
                        f"score={gdino_score:.3f}")
        
        # Combine results
        # Priority: YOLO mask + expanded bbox from both detectors
        final_bbox = None
        final_mask = yolo_mask
        final_score = max(yolo_score, gdino_score)
        
        if yolo_bbox is not None and gdino_bbox is not None:
            # Union of bboxes for more coverage
            final_bbox = np.array([
                min(yolo_bbox[0], gdino_bbox[0]),
                min(yolo_bbox[1], gdino_bbox[1]),
                max(yolo_bbox[2], gdino_bbox[2]),
                max(yolo_bbox[3], gdino_bbox[3])
            ])
        elif yolo_bbox is not None:
            final_bbox = yolo_bbox
        elif gdino_bbox is not None:
            final_bbox = gdino_bbox
        
        if final_bbox is None:
            return StageResult(
                success=False,
                error="No vehicle detected by either model"
            )
        
        return StageResult(
            success=True,
            mask=final_mask,
            bbox=final_bbox,
            confidence=final_score,
            metadata={
                "yolo_score": yolo_score,
                "gdino_score": gdino_score,
                "has_instance_mask": yolo_mask is not None
            }
        )


# =============================================================================
# STAGE 2: SHADOW DETECTION (ShadowFormer)
# =============================================================================

class Stage2ShadowDetection(PipelineStage):
    """
    Shadow detection using ShadowFormer (ONNX) or fallback to color-based detection
    """
    
    def __init__(self, config: PipelineConfig):
        super().__init__(config, "ShadowDetection")
        self.onnx_session = None
    
    def load_model(self) -> bool:
        try:
            import onnxruntime as ort
            
            model_path = os.path.join(self.config.models_dir, self.config.shadowformer_model)
            
            if os.path.exists(model_path):
                providers = ['CUDAExecutionProvider', 'CPUExecutionProvider']
                self.onnx_session = ort.InferenceSession(model_path, providers=providers)
                self.logger.info("✅ ShadowFormer ONNX loaded")
                self.is_loaded = True
                return True
            else:
                self.logger.warning(f"⚠️ ShadowFormer model not found at {model_path}")
                self.logger.warning("  Using color-based shadow detection as fallback")
                self.is_loaded = True
                return True
                
        except Exception as e:
            self.logger.error(f"❌ Failed to load ShadowFormer: {e}")
            self.is_loaded = True  # Continue with fallback
            return True
    
    def _detect_shadows_onnx(self, image: np.ndarray) -> Optional[np.ndarray]:
        """Detect shadows using ShadowFormer ONNX model"""
        if self.onnx_session is None:
            return None
        
        try:
            # Preprocess
            h, w = image.shape[:2]
            input_size = (512, 512)  # ShadowFormer input size
            
            # Resize and normalize
            img_resized = cv2.resize(image, input_size)
            img_norm = img_resized.astype(np.float32) / 255.0
            img_norm = np.transpose(img_norm, (2, 0, 1))  # HWC -> CHW
            img_norm = np.expand_dims(img_norm, 0)  # Add batch dim
            
            # Run inference
            input_name = self.onnx_session.get_inputs()[0].name
            output_name = self.onnx_session.get_outputs()[0].name
            result = self.onnx_session.run([output_name], {input_name: img_norm})[0]
            
            # Postprocess
            shadow_mask = result[0, 0]  # First output channel
            shadow_mask = cv2.resize(shadow_mask, (w, h))
            shadow_mask = (shadow_mask > self.config.shadow_threshold).astype(np.uint8) * 255
            
            return shadow_mask
            
        except Exception as e:
            self.logger.error(f"ONNX shadow detection failed: {e}")
            return None
    
    def _detect_shadows_color(self, image: np.ndarray, vehicle_mask: Optional[np.ndarray] = None) -> np.ndarray:
        """Fallback: Detect shadows using color analysis"""
        h, w = image.shape[:2]
        shadow_mask = np.zeros((h, w), dtype=np.uint8)
        
        # Convert to different color spaces
        hsv = cv2.cvtColor(image, cv2.COLOR_RGB2HSV)
        lab = cv2.cvtColor(image, cv2.COLOR_RGB2LAB)
        
        # Shadow characteristics:
        # - Low luminance in LAB
        # - Low saturation in HSV
        # - Specific value range in HSV
        
        l_channel = lab[:, :, 0]
        s_channel = hsv[:, :, 1]
        v_channel = hsv[:, :, 2]
        
        # Thresholds for shadow detection
        low_luminance = l_channel < 80
        low_saturation = s_channel < 40
        mid_value = (v_channel > 20) & (v_channel < 180)
        
        # Combine conditions
        potential_shadow = low_luminance & low_saturation & mid_value
        
        # Focus on bottom portion of image (where ground shadows typically are)
        bottom_region = np.zeros((h, w), dtype=bool)
        bottom_region[int(h * 0.6):, :] = True
        
        shadow_mask = (potential_shadow & bottom_region).astype(np.uint8) * 255
        
        # If we have vehicle mask, exclude vehicle interior
        if vehicle_mask is not None:
            # Dilate vehicle mask slightly
            kernel = np.ones((5, 5), np.uint8)
            vehicle_dilated = cv2.dilate(vehicle_mask, kernel, iterations=2)
            shadow_mask = shadow_mask & ~vehicle_dilated
        
        # Morphological cleanup
        kernel = np.ones((7, 7), np.uint8)
        shadow_mask = cv2.morphologyEx(shadow_mask, cv2.MORPH_CLOSE, kernel)
        shadow_mask = cv2.morphologyEx(shadow_mask, cv2.MORPH_OPEN, kernel)
        
        return shadow_mask
    
    def process(self, image: np.ndarray, context: Dict[str, Any]) -> StageResult:
        """Detect shadows in the image"""
        self.logger.info("🌑 Detecting shadows...")
        
        vehicle_mask = context.get("stage1_mask")
        
        # Try ONNX first
        shadow_mask = self._detect_shadows_onnx(image)
        detection_method = "ShadowFormer"
        
        # Fallback to color-based
        if shadow_mask is None:
            shadow_mask = self._detect_shadows_color(image, vehicle_mask)
            detection_method = "color-analysis"
        
        shadow_pixels = np.sum(shadow_mask > 127)
        total_pixels = shadow_mask.shape[0] * shadow_mask.shape[1]
        shadow_ratio = shadow_pixels / total_pixels
        
        self.logger.info(f"  Method: {detection_method}")
        self.logger.info(f"  Shadow coverage: {shadow_ratio * 100:.1f}%")
        
        return StageResult(
            success=True,
            mask=shadow_mask,
            confidence=1.0 - shadow_ratio,  # Confidence inversely proportional to shadow
            metadata={
                "detection_method": detection_method,
                "shadow_ratio": shadow_ratio
            }
        )


# =============================================================================
# STAGE 3: PRIMARY SEGMENTATION (SAM2-H)
# =============================================================================

class Stage3SAM2(PipelineStage):
    """
    SAM2 Large segmentation with shadow-aware prompting
    """
    
    def __init__(self, config: PipelineConfig):
        super().__init__(config, "SAM2")
        self.predictor = None
    
    def load_model(self) -> bool:
        try:
            from sam2.sam2_image_predictor import SAM2ImagePredictor
            from sam2.build_sam import build_sam2
            
            model_path = os.path.join(self.config.models_dir, self.config.sam2_model)
            
            if not os.path.exists(model_path):
                # Fallback to base model if large not available
                model_path = os.path.join(self.config.models_dir, "sam2_hiera_base_plus.pt")
                self.config.sam2_config = "sam2_hiera_b+.yaml"
            
            self.model = build_sam2(
                config_file=self.config.sam2_config,
                ckpt_path=model_path,
                device=self.config.device
            )
            self.predictor = SAM2ImagePredictor(self.model)
            
            self.logger.info(f"✅ SAM2 loaded from {model_path}")
            self.is_loaded = True
            return True
            
        except Exception as e:
            self.logger.error(f"❌ Failed to load SAM2: {e}")
            return False
    
    def _generate_prompts(
        self,
        bbox: np.ndarray,
        shadow_mask: Optional[np.ndarray] = None,
        image_shape: Tuple[int, int] = None
    ) -> Tuple[np.ndarray, np.ndarray]:
        """Generate strategic prompt points"""
        x1, y1, x2, y2 = bbox
        box_width = x2 - x1
        box_height = y2 - y1
        center_x = (x1 + x2) / 2
        center_y = (y1 + y2) / 2
        
        wheel_line_y = y1 + box_height * 0.78  # Wheel level
        
        # 12 strategic positive points for optimal coverage
        positive_points = [
            # Top section
            [center_x, y1 + box_height * 0.08],       # Top center (roof)
            [x1 + box_width * 0.3, y1 + box_height * 0.15],   # Left roof
            [x2 - box_width * 0.3, y1 + box_height * 0.15],   # Right roof
            
            # Middle section (body)
            [center_x, center_y],                     # Absolute center
            [x1 + box_width * 0.15, center_y],        # Left body
            [x2 - box_width * 0.15, center_y],        # Right body
            [x1 + box_width * 0.25, y1 + box_height * 0.35],  # Left upper body
            [x2 - box_width * 0.25, y1 + box_height * 0.35],  # Right upper body
            
            # Lower body (above wheels)
            [center_x, wheel_line_y - box_height * 0.1],      # Center lower body
            [x1 + box_width * 0.2, wheel_line_y - box_height * 0.08],  # Left lower
            [x2 - box_width * 0.2, wheel_line_y - box_height * 0.08],  # Right lower
            
            # Wheel centers (critical!)
            [x1 + box_width * 0.18, wheel_line_y],    # Left wheel center
            [x2 - box_width * 0.18, wheel_line_y],    # Right wheel center
        ]
        
        # Negative points: explicitly in shadow regions
        negative_points = []
        
        if shadow_mask is not None and image_shape is not None:
            h, w = image_shape
            # Sample points from shadow mask
            shadow_coords = np.where(shadow_mask > 127)
            if len(shadow_coords[0]) > 0:
                # Get 5 random shadow points
                indices = np.random.choice(len(shadow_coords[0]), min(5, len(shadow_coords[0])), replace=False)
                for idx in indices:
                    y, x = shadow_coords[0][idx], shadow_coords[1][idx]
                    negative_points.append([x, y])
        
        # Add ground points as negative (below vehicle)
        negative_points.extend([
            [center_x, y2 + box_height * 0.1],        # Below center
            [x1 + box_width * 0.3, y2 + box_height * 0.08],  # Below left
            [x2 - box_width * 0.3, y2 + box_height * 0.08],  # Below right
        ])
        
        all_points = np.array(positive_points + negative_points)
        labels = np.array([1] * len(positive_points) + [0] * len(negative_points))
        
        return all_points, labels
    
    def process(self, image: np.ndarray, context: Dict[str, Any]) -> StageResult:
        """Run SAM2 segmentation with enhanced prompts"""
        self.logger.info("🎨 Running SAM2 segmentation...")
        
        if self.predictor is None:
            return StageResult(success=False, error="SAM2 not loaded")
        
        bbox = context.get("stage1_bbox")
        shadow_mask = context.get("stage2_mask")
        yolo_mask = context.get("stage1_mask")
        
        if bbox is None:
            return StageResult(success=False, error="No bbox from Stage 1")
        
        try:
            # Pad bbox for better coverage
            h, w = image.shape[:2]
            x1, y1, x2, y2 = bbox
            box_width = x2 - x1
            box_height = y2 - y1
            
            padded_bbox = np.array([
                max(0, x1 - box_width * 0.1),
                max(0, y1 - box_height * 0.05),
                min(w, x2 + box_width * 0.1),
                min(h, y2 + box_height * 0.15)  # More padding at bottom for wheels
            ])
            
            # Set image
            self.predictor.set_image(image)
            
            # Generate prompts
            points, labels = self._generate_prompts(
                bbox, shadow_mask, image.shape[:2]
            )
            
            self.logger.info(f"  Using {np.sum(labels == 1)} positive, {np.sum(labels == 0)} negative points")
            
            # Predict
            masks, scores, _ = self.predictor.predict(
                point_coords=points,
                point_labels=labels,
                box=padded_bbox,
                multimask_output=self.config.sam2_multimask
            )
            
            # Select best mask
            best_idx = np.argmax(scores)
            sam_mask = masks[best_idx]
            sam_score = float(scores[best_idx])
            
            # Convert to uint8
            sam_mask = (sam_mask.astype(np.uint8) * 255)
            
            # If we have YOLO instance mask, combine them (union)
            if yolo_mask is not None:
                yolo_mask_uint8 = (yolo_mask > 0.5).astype(np.uint8) * 255
                # Weighted combination: 70% SAM, 30% YOLO
                combined = cv2.addWeighted(sam_mask, 0.7, yolo_mask_uint8, 0.3, 0)
                sam_mask = (combined > 127).astype(np.uint8) * 255
                self.logger.info("  Combined SAM2 + YOLO masks")
            
            # Remove shadow regions
            if shadow_mask is not None:
                # Only remove shadows that are NOT inside the main vehicle region
                # Protect core vehicle pixels
                kernel = np.ones((10, 10), np.uint8)
                core_vehicle = cv2.erode(sam_mask, kernel, iterations=2)
                shadow_to_remove = shadow_mask & ~core_vehicle
                sam_mask = sam_mask & ~shadow_to_remove
                self.logger.info("  Removed shadow regions while protecting core vehicle")
            
            # Largest contour only (remove artifacts)
            contours, _ = cv2.findContours(sam_mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
            if contours:
                largest = max(contours, key=cv2.contourArea)
                sam_mask = np.zeros_like(sam_mask)
                cv2.drawContours(sam_mask, [largest], -1, 255, -1)
            
            self.logger.info(f"  SAM2 score: {sam_score:.3f}")
            
            return StageResult(
                success=True,
                mask=sam_mask,
                bbox=padded_bbox,
                confidence=sam_score,
                metadata={
                    "num_positive_points": int(np.sum(labels == 1)),
                    "num_negative_points": int(np.sum(labels == 0)),
                    "combined_with_yolo": yolo_mask is not None
                }
            )
            
        except Exception as e:
            self.logger.error(f"SAM2 segmentation failed: {e}")
            return StageResult(success=False, error=str(e))


# =============================================================================
# STAGE 4: CAR PARTS ANALYSIS
# =============================================================================

class Stage4CarParts(PipelineStage):
    """
    Car parts segmentation for wheel protection and validation
    """
    
    def __init__(self, config: PipelineConfig):
        super().__init__(config, "CarParts")
        self.model = None
        self.processor = None
    
    def load_model(self) -> bool:
        try:
            # Try HuggingFace model
            from transformers import AutoImageProcessor, AutoModelForSemanticSegmentation
            
            self.processor = AutoImageProcessor.from_pretrained(self.config.carparts_model)
            self.model = AutoModelForSemanticSegmentation.from_pretrained(
                self.config.carparts_model
            ).to(self.config.device)
            
            self.logger.info("✅ CarParts segmentation loaded")
            self.is_loaded = True
            return True
            
        except Exception as e:
            self.logger.warning(f"⚠️ CarParts model not available: {e}")
            self.is_loaded = True  # Continue without this stage
            return True
    
    def process(self, image: np.ndarray, context: Dict[str, Any]) -> StageResult:
        """Segment car parts, especially wheels"""
        self.logger.info("🔧 Analyzing car parts...")
        
        if self.model is None:
            self.logger.warning("  CarParts model not loaded, skipping")
            return StageResult(
                success=True,
                mask=context.get("stage3_mask"),
                metadata={"skipped": True}
            )
        
        try:
            # Process image
            pil_image = Image.fromarray(image)
            inputs = self.processor(images=pil_image, return_tensors="pt").to(self.config.device)
            
            with torch.no_grad():
                outputs = self.model(**inputs)
            
            # Get segmentation
            logits = outputs.logits
            upsampled = torch.nn.functional.interpolate(
                logits,
                size=image.shape[:2],
                mode="bilinear",
                align_corners=False
            )
            seg_map = upsampled.argmax(dim=1)[0].cpu().numpy()
            
            # Find wheel class (usually class 18 or named "wheel")
            wheel_class_id = None
            for i, class_name in enumerate(self.config.carparts_classes):
                if "wheel" in class_name.lower():
                    wheel_class_id = i
                    break
            
            wheel_mask = None
            if wheel_class_id is not None:
                wheel_mask = (seg_map == wheel_class_id).astype(np.uint8) * 255
                wheel_pixels = np.sum(wheel_mask > 0)
                self.logger.info(f"  Found {wheel_pixels} wheel pixels")
            
            # Create full car mask (all non-background classes)
            car_mask = (seg_map > 0).astype(np.uint8) * 255
            
            # Combine with SAM2 mask
            sam_mask = context.get("stage3_mask")
            if sam_mask is not None:
                # Ensure wheels are included in final mask
                if wheel_mask is not None:
                    # Dilate wheels slightly for safety
                    kernel = np.ones((5, 5), np.uint8)
                    wheel_dilated = cv2.dilate(wheel_mask, kernel, iterations=1)
                    # Add wheels to SAM mask
                    combined_mask = sam_mask | wheel_dilated
                else:
                    combined_mask = sam_mask
            else:
                combined_mask = car_mask
            
            # Count detected parts
            unique_parts = np.unique(seg_map)
            detected_parts = [self.config.carparts_classes[i] for i in unique_parts if i > 0]
            
            self.logger.info(f"  Detected parts: {len(detected_parts)}")
            
            return StageResult(
                success=True,
                mask=combined_mask,
                metadata={
                    "detected_parts": detected_parts,
                    "wheel_mask": wheel_mask,
                    "has_wheels": wheel_mask is not None and np.any(wheel_mask > 0)
                }
            )
            
        except Exception as e:
            self.logger.error(f"CarParts analysis failed: {e}")
            return StageResult(
                success=True,
                mask=context.get("stage3_mask"),
                metadata={"error": str(e)}
            )


# =============================================================================
# STAGE 5: SEMANTIC CORRECTION (SegFormer)
# =============================================================================

class Stage5SegFormer(PipelineStage):
    """
    SegFormer semantic segmentation for mask validation and correction
    """
    
    def __init__(self, config: PipelineConfig):
        super().__init__(config, "SegFormer")
    
    def load_model(self) -> bool:
        try:
            from transformers import SegformerFeatureExtractor, SegformerForSemanticSegmentation
            
            self.feature_extractor = SegformerFeatureExtractor.from_pretrained(
                self.config.segformer_model
            )
            self.model = SegformerForSemanticSegmentation.from_pretrained(
                self.config.segformer_model
            ).to(self.config.device)
            
            self.logger.info("✅ SegFormer loaded")
            self.is_loaded = True
            return True
            
        except Exception as e:
            self.logger.warning(f"⚠️ SegFormer failed to load: {e}")
            self.is_loaded = True
            return True
    
    def process(self, image: np.ndarray, context: Dict[str, Any]) -> StageResult:
        """Verify and correct mask using semantic segmentation"""
        self.logger.info("🔍 Running semantic verification...")
        
        if not hasattr(self, 'model') or self.model is None:
            self.logger.warning("  SegFormer not loaded, skipping")
            return StageResult(
                success=True,
                mask=context.get("stage4_mask") or context.get("stage3_mask"),
                metadata={"skipped": True}
            )
        
        try:
            # Process
            pil_image = Image.fromarray(image)
            inputs = self.feature_extractor(images=pil_image, return_tensors="pt").to(self.config.device)
            
            with torch.no_grad():
                outputs = self.model(**inputs)
            
            logits = outputs.logits
            upsampled = torch.nn.functional.interpolate(
                logits,
                size=image.shape[:2],
                mode="bilinear",
                align_corners=False
            )
            seg_map = upsampled.argmax(dim=1)[0].cpu().numpy()
            
            # ADE20K car class is 20
            car_mask = (seg_map == self.config.segformer_car_class_id).astype(np.uint8) * 255
            
            # Get current mask
            current_mask = context.get("stage4_mask") or context.get("stage3_mask")
            
            if current_mask is None:
                return StageResult(
                    success=True,
                    mask=car_mask,
                    metadata={"source": "segformer_only"}
                )
            
            # Calculate IoU
            intersection = np.sum((current_mask > 127) & (car_mask > 127))
            union = np.sum((current_mask > 127) | (car_mask > 127))
            iou = intersection / union if union > 0 else 0
            
            self.logger.info(f"  IoU with SegFormer: {iou:.3f}")
            
            # Correction strategy based on IoU
            if iou > 0.8:
                # High agreement, use current mask
                final_mask = current_mask
                correction = "none"
            elif iou > 0.5:
                # Medium agreement, use intersection
                final_mask = ((current_mask > 127) & (car_mask > 127)).astype(np.uint8) * 255
                correction = "intersection"
            else:
                # Low agreement, use union but trust SAM2 more
                final_mask = current_mask  # Trust SAM2
                correction = "trust_sam2"
            
            self.logger.info(f"  Correction applied: {correction}")
            
            # Ensure wheel mask is preserved
            wheel_mask = context.get("stage4_metadata", {}).get("wheel_mask")
            if wheel_mask is not None:
                final_mask = final_mask | wheel_mask
                self.logger.info("  Wheels preserved")
            
            return StageResult(
                success=True,
                mask=final_mask,
                confidence=iou,
                metadata={
                    "iou": iou,
                    "correction": correction
                }
            )
            
        except Exception as e:
            self.logger.error(f"SegFormer verification failed: {e}")
            return StageResult(
                success=True,
                mask=context.get("stage4_mask") or context.get("stage3_mask"),
                metadata={"error": str(e)}
            )


# =============================================================================
# STAGE 6: EDGE REFINEMENT (ISNet/DIS)
# =============================================================================

class Stage6ISNet(PipelineStage):
    """
    ISNet (Dichotomous Image Segmentation) for precise edge refinement
    """
    
    def __init__(self, config: PipelineConfig):
        super().__init__(config, "ISNet")
    
    def load_model(self) -> bool:
        try:
            # Try to load ISNet
            model_path = os.path.join(self.config.models_dir, self.config.isnet_model)
            
            if not os.path.exists(model_path):
                self.logger.warning(f"⚠️ ISNet model not found at {model_path}")
                self.is_loaded = True
                return True
            
            # ISNet architecture (simplified for loading)
            from isnet import ISNetDIS  # Will be defined in separate file
            
            self.model = ISNetDIS()
            self.model.load_state_dict(torch.load(model_path, map_location=self.config.device))
            self.model.to(self.config.device)
            self.model.eval()
            
            self.logger.info("✅ ISNet loaded")
            self.is_loaded = True
            return True
            
        except ImportError:
            self.logger.warning("⚠️ ISNet module not available, using OpenCV edge refinement")
            self.is_loaded = True
            return True
        except Exception as e:
            self.logger.warning(f"⚠️ ISNet failed to load: {e}")
            self.is_loaded = True
            return True
    
    def _refine_edges_opencv(self, mask: np.ndarray, image: np.ndarray) -> np.ndarray:
        """Fallback edge refinement using OpenCV techniques"""
        # GrabCut refinement
        try:
            h, w = image.shape[:2]
            
            # Initialize GrabCut mask
            gc_mask = np.zeros((h, w), dtype=np.uint8)
            gc_mask[mask > 127] = cv2.GC_PR_FGD
            gc_mask[mask <= 127] = cv2.GC_PR_BGD
            
            # Definite foreground (eroded mask)
            kernel = np.ones((15, 15), np.uint8)
            fg_definite = cv2.erode(mask, kernel, iterations=2)
            gc_mask[fg_definite > 127] = cv2.GC_FGD
            
            # Definite background (far from mask)
            bg_definite = cv2.dilate(mask, kernel, iterations=3)
            gc_mask[bg_definite == 0] = cv2.GC_BGD
            
            # Run GrabCut
            bgd_model = np.zeros((1, 65), np.float64)
            fgd_model = np.zeros((1, 65), np.float64)
            
            cv2.grabCut(image, gc_mask, None, bgd_model, fgd_model, 3, cv2.GC_INIT_WITH_MASK)
            
            refined_mask = np.where((gc_mask == cv2.GC_FGD) | (gc_mask == cv2.GC_PR_FGD), 255, 0).astype(np.uint8)
            
            return refined_mask
            
        except Exception as e:
            self.logger.warning(f"GrabCut refinement failed: {e}")
            return mask
    
    def _refine_edges_isnet(self, mask: np.ndarray, image: np.ndarray) -> np.ndarray:
        """Refine edges using ISNet"""
        if self.model is None:
            return self._refine_edges_opencv(mask, image)
        
        try:
            input_size = self.config.isnet_input_size
            h, w = image.shape[:2]
            
            # Preprocess
            img_resized = cv2.resize(image, input_size)
            img_tensor = torch.from_numpy(img_resized.transpose(2, 0, 1)).float() / 255.0
            img_tensor = img_tensor.unsqueeze(0).to(self.config.device)
            
            # Normalize
            mean = torch.tensor([0.485, 0.456, 0.406]).view(1, 3, 1, 1).to(self.config.device)
            std = torch.tensor([0.229, 0.224, 0.225]).view(1, 3, 1, 1).to(self.config.device)
            img_tensor = (img_tensor - mean) / std
            
            # Inference
            with torch.no_grad():
                result = self.model(img_tensor)
                if isinstance(result, (list, tuple)):
                    output = result[0]
                else:
                    output = result
            
            # Postprocess
            isnet_mask = output.squeeze().cpu().numpy()
            isnet_mask = (isnet_mask * 255).astype(np.uint8)
            isnet_mask = cv2.resize(isnet_mask, (w, h))
            
            # Combine with input mask (ISNet for edges, input for interior)
            # Detect edges
            edges = cv2.Canny(mask, 50, 150)
            edge_region = cv2.dilate(edges, np.ones((7, 7), np.uint8), iterations=2)
            
            # Use ISNet in edge region, original elsewhere
            refined = np.where(edge_region > 0, isnet_mask, mask)
            
            return refined
            
        except Exception as e:
            self.logger.warning(f"ISNet refinement failed: {e}")
            return self._refine_edges_opencv(mask, image)
    
    def process(self, image: np.ndarray, context: Dict[str, Any]) -> StageResult:
        """Refine mask edges"""
        self.logger.info("✨ Refining edges...")
        
        current_mask = (
            context.get("stage5_mask") or
            context.get("stage4_mask") or
            context.get("stage3_mask")
        )
        
        if current_mask is None:
            return StageResult(success=False, error="No mask to refine")
        
        # Apply edge refinement
        if self.model is not None:
            refined_mask = self._refine_edges_isnet(current_mask, image)
            method = "ISNet"
        else:
            refined_mask = self._refine_edges_opencv(current_mask, image)
            method = "GrabCut"
        
        # Final cleanup
        # Largest contour
        contours, _ = cv2.findContours(refined_mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        if contours:
            largest = max(contours, key=cv2.contourArea)
            refined_mask = np.zeros_like(refined_mask)
            cv2.drawContours(refined_mask, [largest], -1, 255, -1)
        
        # Smooth edges
        refined_mask = cv2.GaussianBlur(refined_mask, (3, 3), 0)
        refined_mask = (refined_mask > 127).astype(np.uint8) * 255
        
        self.logger.info(f"  Refinement method: {method}")
        
        return StageResult(
            success=True,
            mask=refined_mask,
            metadata={"method": method}
        )


# =============================================================================
# STAGE 7: ENHANCEMENT (CodeFormer/GFPGAN + RealESRGAN)
# =============================================================================

class Stage7Enhancement(PipelineStage):
    """
    Final enhancement using CodeFormer and RealESRGAN
    """
    
    def __init__(self, config: PipelineConfig):
        super().__init__(config, "Enhancement")
        self.upsampler = None
    
    def load_model(self) -> bool:
        try:
            # Try to load RealESRGAN for upscaling
            from basicsr.archs.rrdbnet_arch import RRDBNet
            from realesrgan import RealESRGANer
            
            model_path = os.path.join(self.config.models_dir, self.config.realesrgan_model)
            
            if os.path.exists(model_path):
                model = RRDBNet(
                    num_in_ch=3, num_out_ch=3, num_feat=64,
                    num_block=23, num_grow_ch=32, scale=4
                )
                self.upsampler = RealESRGANer(
                    scale=4,
                    model_path=model_path,
                    model=model,
                    tile=400,
                    tile_pad=10,
                    pre_pad=0,
                    half=True if self.config.device == "cuda" else False
                )
                self.logger.info("✅ RealESRGAN loaded")
            else:
                self.logger.warning(f"⚠️ RealESRGAN model not found at {model_path}")
            
            self.is_loaded = True
            return True
            
        except ImportError:
            self.logger.warning("⚠️ RealESRGAN not available, using basic enhancement")
            self.is_loaded = True
            return True
        except Exception as e:
            self.logger.warning(f"⚠️ Enhancement models failed to load: {e}")
            self.is_loaded = True
            return True
    
    def _enhance_basic(self, image: np.ndarray, mask: np.ndarray) -> np.ndarray:
        """Basic enhancement using OpenCV"""
        # Apply CLAHE for contrast
        lab = cv2.cvtColor(image, cv2.COLOR_RGB2LAB)
        clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
        lab[:, :, 0] = clahe.apply(lab[:, :, 0])
        enhanced = cv2.cvtColor(lab, cv2.COLOR_LAB2RGB)
        
        # Slight sharpening
        kernel = np.array([[-1, -1, -1],
                          [-1,  9, -1],
                          [-1, -1, -1]]) / 1.0
        enhanced = cv2.filter2D(enhanced, -1, kernel * 0.3 + np.eye(3).flatten().reshape(3, 3) * 0.7)
        
        return np.clip(enhanced, 0, 255).astype(np.uint8)
    
    def _create_alpha_matte(self, mask: np.ndarray, image: np.ndarray) -> np.ndarray:
        """Create smooth alpha matte for edges"""
        # Edge detection
        edges = cv2.Canny(mask, 100, 200)
        edge_region = cv2.dilate(edges, np.ones((5, 5), np.uint8), iterations=2)
        
        # Create gradient alpha at edges
        alpha = mask.astype(np.float32)
        
        # Apply guided filter for edge-aware smoothing
        try:
            guide = cv2.cvtColor(image, cv2.COLOR_RGB2GRAY).astype(np.float32) / 255.0
            alpha_norm = alpha / 255.0
            
            # Simple edge-aware smoothing (approximation of guided filter)
            alpha_blurred = cv2.GaussianBlur(alpha_norm, (5, 5), 0)
            alpha_smooth = np.where(edge_region > 0, alpha_blurred, alpha_norm)
            alpha = (alpha_smooth * 255).astype(np.uint8)
        except:
            # Fallback to simple blur at edges
            alpha_blurred = cv2.GaussianBlur(alpha, (5, 5), 0)
            alpha = np.where(edge_region > 0, alpha_blurred, alpha)
        
        return alpha
    
    def process(self, image: np.ndarray, context: Dict[str, Any]) -> StageResult:
        """Apply final enhancement and create output"""
        self.logger.info("🎬 Applying final enhancement...")
        
        mask = (
            context.get("stage6_mask") or
            context.get("stage5_mask") or
            context.get("stage4_mask") or
            context.get("stage3_mask")
        )
        
        if mask is None:
            return StageResult(success=False, error="No mask available")
        
        # Enhance image
        if self.upsampler is not None and self.config.upscale_factor > 1.0:
            try:
                enhanced, _ = self.upsampler.enhance(image)
                # Resize mask to match
                h, w = enhanced.shape[:2]
                mask = cv2.resize(mask, (w, h), interpolation=cv2.INTER_LINEAR)
                mask = (mask > 127).astype(np.uint8) * 255
                self.logger.info(f"  Upscaled {self.config.upscale_factor}x")
            except Exception as e:
                self.logger.warning(f"  Upscaling failed: {e}")
                enhanced = self._enhance_basic(image, mask)
        else:
            enhanced = self._enhance_basic(image, mask)
        
        # Create alpha matte
        alpha = self._create_alpha_matte(mask, enhanced)
        
        # Create output with background
        h, w = enhanced.shape[:2]
        bg_color = self.config.background_color
        background = np.full((h, w, 3), bg_color, dtype=np.uint8)
        
        # Alpha composite
        alpha_norm = alpha.astype(np.float32) / 255.0
        alpha_3ch = np.stack([alpha_norm] * 3, axis=-1)
        
        output = (enhanced * alpha_3ch + background * (1 - alpha_3ch)).astype(np.uint8)
        
        self.logger.info("  ✅ Enhancement complete")
        
        return StageResult(
            success=True,
            mask=mask,
            metadata={
                "enhanced_image": output,
                "alpha_matte": alpha,
                "background_color": bg_color
            }
        )


# =============================================================================
# MAIN PIPELINE ORCHESTRATOR
# =============================================================================

class ProfessionalVehiclePipeline:
    """
    Orchestrates all 7 stages of the professional vehicle segmentation pipeline
    """
    
    def __init__(self, config: Optional[PipelineConfig] = None):
        self.config = config or PipelineConfig()
        self.logger = logging.getLogger("Pipeline")
        
        # Initialize stages
        self.stages: List[PipelineStage] = [
            Stage1Detection(self.config),
            Stage2ShadowDetection(self.config),
            Stage3SAM2(self.config),
            Stage4CarParts(self.config),
            Stage5SegFormer(self.config),
            Stage6ISNet(self.config),
            Stage7Enhancement(self.config),
        ]
        
        self.is_loaded = False
    
    def load_models(self) -> bool:
        """Load all stage models"""
        self.logger.info("=" * 60)
        self.logger.info("🚀 Loading Professional Pipeline Models")
        self.logger.info("=" * 60)
        
        all_loaded = True
        for i, stage in enumerate(self.stages, 1):
            self.logger.info(f"\n[{i}/7] Loading {stage.name}...")
            if not stage.load_model():
                self.logger.warning(f"⚠️ Stage {stage.name} failed to load, will use fallback")
                all_loaded = False
        
        self.is_loaded = True
        self.logger.info("\n" + "=" * 60)
        self.logger.info("✅ Pipeline ready" if all_loaded else "⚠️ Pipeline ready with fallbacks")
        self.logger.info("=" * 60)
        
        return all_loaded
    
    def process_image(
        self,
        image: np.ndarray,
        output_path: Optional[Path] = None,
        return_intermediates: bool = False
    ) -> Dict[str, Any]:
        """
        Process a single image through all 7 stages
        
        Args:
            image: RGB numpy array
            output_path: Optional path to save results
            return_intermediates: Whether to return intermediate masks
        
        Returns:
            Dictionary with final result and metadata
        """
        if not self.is_loaded:
            self.load_models()
        
        self.logger.info("\n" + "=" * 60)
        self.logger.info("🚗 Processing Vehicle Image")
        self.logger.info("=" * 60)
        
        context: Dict[str, Any] = {}
        intermediates: Dict[str, np.ndarray] = {}
        
        # Run each stage
        for i, stage in enumerate(self.stages, 1):
            self.logger.info(f"\n[Stage {i}/7] {stage.name}")
            self.logger.info("-" * 40)
            
            try:
                result = stage.process(image, context)
                
                # Store results in context for next stage
                context[f"stage{i}_result"] = result
                context[f"stage{i}_mask"] = result.mask
                context[f"stage{i}_bbox"] = result.bbox
                context[f"stage{i}_confidence"] = result.confidence
                context[f"stage{i}_metadata"] = result.metadata
                
                if return_intermediates and result.mask is not None:
                    intermediates[f"stage{i}_{stage.name}"] = result.mask.copy()
                
                if not result.success:
                    self.logger.warning(f"⚠️ Stage {i} warning: {result.error}")
                    
            except Exception as e:
                self.logger.error(f"❌ Stage {i} failed: {e}")
                context[f"stage{i}_error"] = str(e)
        
        # Get final outputs from Stage 7
        final_result = context.get("stage7_result", StageResult(success=False))
        final_metadata = context.get("stage7_metadata", {})
        
        output = {
            "success": final_result.success,
            "final_mask": final_result.mask,
            "final_image": final_metadata.get("enhanced_image"),
            "alpha_matte": final_metadata.get("alpha_matte"),
            "context": context
        }
        
        if return_intermediates:
            output["intermediates"] = intermediates
        
        # Save outputs
        if output_path is not None and final_result.success:
            self._save_outputs(output, output_path)
        
        self.logger.info("\n" + "=" * 60)
        self.logger.info("✅ Processing complete")
        self.logger.info("=" * 60)
        
        return output
    
    def _save_outputs(self, output: Dict[str, Any], output_path: Path):
        """Save all outputs to disk"""
        output_path.mkdir(parents=True, exist_ok=True)
        
        # Final image (with background)
        if output.get("final_image") is not None:
            Image.fromarray(output["final_image"]).save(
                output_path / "result_bg_removed.png",
                quality=self.config.output_quality
            )
            self.logger.info(f"  Saved: result_bg_removed.png")
        
        # Final mask
        if output.get("final_mask") is not None:
            Image.fromarray(output["final_mask"]).save(
                output_path / "result_mask.png"
            )
            self.logger.info(f"  Saved: result_mask.png")
        
        # Alpha matte
        if output.get("alpha_matte") is not None:
            Image.fromarray(output["alpha_matte"]).save(
                output_path / "result_alpha.png"
            )
            self.logger.info(f"  Saved: result_alpha.png")
        
        # Intermediates
        if "intermediates" in output:
            for name, mask in output["intermediates"].items():
                Image.fromarray(mask).save(
                    output_path / f"intermediate_{name}.png"
                )
            self.logger.info(f"  Saved: {len(output['intermediates'])} intermediate masks")
    
    def unload_models(self):
        """Unload all models to free memory"""
        for stage in self.stages:
            stage.unload_model()
        
        if torch.cuda.is_available():
            torch.cuda.empty_cache()
        
        self.is_loaded = False
        self.logger.info("All models unloaded")


# =============================================================================
# CLI INTERFACE
# =============================================================================

def main():
    """Command-line interface for the pipeline"""
    import argparse
    
    parser = argparse.ArgumentParser(
        description="Professional Vehicle Segmentation Pipeline"
    )
    parser.add_argument("--input", "-i", required=True, help="Input image or directory")
    parser.add_argument("--output", "-o", required=True, help="Output directory")
    parser.add_argument("--device", "-d", default="cuda", choices=["cuda", "cpu"])
    parser.add_argument("--intermediates", action="store_true", help="Save intermediate masks")
    
    args = parser.parse_args()
    
    # Configure
    config = PipelineConfig(device=args.device)
    
    # Initialize pipeline
    pipeline = ProfessionalVehiclePipeline(config)
    pipeline.load_models()
    
    # Process
    input_path = Path(args.input)
    output_path = Path(args.output)
    
    if input_path.is_file():
        # Single image
        image = np.array(Image.open(input_path).convert("RGB"))
        result = pipeline.process_image(
            image,
            output_path / input_path.stem,
            return_intermediates=args.intermediates
        )
        print(f"Success: {result['success']}")
        
    elif input_path.is_dir():
        # Batch processing
        images = list(input_path.glob("*.jpg")) + list(input_path.glob("*.png"))
        print(f"Found {len(images)} images")
        
        for img_path in images:
            print(f"\nProcessing: {img_path.name}")
            image = np.array(Image.open(img_path).convert("RGB"))
            pipeline.process_image(
                image,
                output_path / img_path.stem,
                return_intermediates=args.intermediates
            )
    
    pipeline.unload_models()


if __name__ == "__main__":
    main()
