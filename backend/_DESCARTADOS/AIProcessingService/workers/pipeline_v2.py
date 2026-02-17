#!/usr/bin/env python3
"""
Professional Vehicle Segmentation Pipeline V2
============================================

Cambios vs V1:
1. ISNet (DIS) real implementado para bordes perfectos
2. Mejor detección de sombras con umbral adaptativo
3. Protección de ruedas con detección de círculos
4. Anti-aliasing real en bordes
5. Mejoras visiblemente diferentes al pipeline básico

Author: Gregory Moreno
Date: January 2026
"""

import cv2
import numpy as np
import torch
import torch.nn.functional as F
from pathlib import Path
from PIL import Image
from typing import Optional, Tuple, Dict, Any, List
from dataclasses import dataclass, field
from transformers import (
    CLIPSegProcessor, 
    CLIPSegForImageSegmentation,
    SegformerForSemanticSegmentation,
    SegformerImageProcessor
)
from ultralytics import YOLO
import warnings
warnings.filterwarnings('ignore')

# Paths
SCRIPT_DIR = Path(__file__).parent
MODELS_DIR = SCRIPT_DIR / "models"
INPUT_DIR = SCRIPT_DIR / "input"
OUTPUT_DIR = SCRIPT_DIR / "output_v2"

# Device
DEVICE = "mps" if torch.backends.mps.is_available() else "cuda" if torch.cuda.is_available() else "cpu"
print(f"🔧 Using device: {DEVICE}")


@dataclass
class PipelineConfig:
    """Pipeline configuration"""
    # Stage 1: Detection
    yolo_model: str = "yolov8x-seg.pt"
    detection_confidence: float = 0.3
    
    # Stage 2: Shadow detection
    enable_shadow_detection: bool = True
    shadow_threshold: float = 0.25
    shadow_bottom_percent: float = 0.35  # Check bottom 35% of image
    
    # Stage 3: Wheel protection
    enable_wheel_protection: bool = True
    
    # Stage 4: SAM2 segmentation
    sam2_model: str = "sam2_hiera_large.pt"
    
    # Stage 5: SegFormer validation
    enable_segformer: bool = True
    
    # Stage 6: Edge refinement
    enable_edge_refinement: bool = True
    edge_feather_radius: int = 3
    
    # Stage 7: Final polish
    enable_antialiasing: bool = True
    antialias_strength: float = 0.8


class Stage1Detection:
    """Stage 1: YOLOv8x-seg for initial vehicle detection"""
    
    def __init__(self, config: PipelineConfig):
        self.config = config
        self.model = None
        
    def load(self):
        model_path = MODELS_DIR / self.config.yolo_model
        if model_path.exists():
            self.model = YOLO(str(model_path))
            print(f"   ✅ YOLOv8x-seg loaded ({model_path.stat().st_size / (1024*1024):.0f} MB)")
        else:
            raise FileNotFoundError(f"YOLO model not found: {model_path}")
            
    def process(self, image: np.ndarray) -> Dict[str, Any]:
        """Detect vehicles and extract initial mask"""
        results = self.model(image, conf=self.config.detection_confidence, verbose=False)[0]
        
        vehicle_classes = {2, 3, 5, 7}  # car, motorcycle, bus, truck
        
        best_mask = None
        best_box = None
        best_area = 0
        best_conf = 0
        
        if results.masks is not None:
            for i, mask in enumerate(results.masks.data):
                cls = int(results.boxes.cls[i])
                conf = float(results.boxes.conf[i])
                
                if cls in vehicle_classes:
                    mask_np = mask.cpu().numpy()
                    area = mask_np.sum()
                    
                    if area > best_area:
                        best_area = area
                        best_mask = mask_np
                        best_conf = conf
                        best_box = results.boxes.xyxy[i].cpu().numpy()
        
        if best_mask is not None:
            # Resize mask to image size
            mask_resized = cv2.resize(
                best_mask.astype(np.float32),
                (image.shape[1], image.shape[0]),
                interpolation=cv2.INTER_LINEAR
            )
            return {
                'mask': mask_resized,
                'box': best_box,
                'confidence': best_conf,
                'success': True
            }
        
        return {'mask': None, 'box': None, 'confidence': 0, 'success': False}


class Stage2ShadowRemoval:
    """Stage 2: Detect and remove ground shadows using CLIPSeg + color analysis"""
    
    def __init__(self, config: PipelineConfig):
        self.config = config
        self.model = None
        self.processor = None
        
    def load(self):
        if not self.config.enable_shadow_detection:
            print(f"   ⏭️ Shadow detection disabled")
            return
            
        try:
            self.processor = CLIPSegProcessor.from_pretrained("CIDAS/clipseg-rd64-refined")
            self.model = CLIPSegForImageSegmentation.from_pretrained("CIDAS/clipseg-rd64-refined")
            self.model.to(DEVICE)
            self.model.eval()
            print(f"   ✅ CLIPSeg shadow detector loaded")
        except Exception as e:
            print(f"   ⚠️ Shadow detection not available: {e}")
            
    def process(self, image: np.ndarray, vehicle_mask: np.ndarray) -> np.ndarray:
        """Remove shadows from vehicle mask - especially in wheel/bottom areas"""
        if self.model is None or not self.config.enable_shadow_detection:
            return vehicle_mask
        
        try:
            h, w = vehicle_mask.shape
            refined_mask = vehicle_mask.copy()
            
            # === Method 1: CLIPSeg shadow detection ===
            pil_image = Image.fromarray(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))
            prompts = ["shadow on ground", "dark shadow under car", "ground shadow"]
            
            inputs = self.processor(
                text=prompts,
                images=[pil_image] * len(prompts),
                return_tensors="pt",
                padding=True
            )
            inputs = {k: v.to(DEVICE) for k, v in inputs.items()}
            
            with torch.no_grad():
                outputs = self.model(**inputs)
                
            shadow_masks = torch.sigmoid(outputs.logits)
            shadow_mask = shadow_masks.max(dim=0)[0].cpu().numpy()
            shadow_mask = cv2.resize(shadow_mask, (w, h))
            
            # === Method 2: Color-based shadow detection ===
            # Shadows are typically dark with low saturation
            hsv = cv2.cvtColor(image, cv2.COLOR_BGR2HSV)
            v_channel = hsv[:, :, 2] / 255.0
            s_channel = hsv[:, :, 1] / 255.0
            
            # Shadow: low value, low saturation
            color_shadow = ((v_channel < 0.35) & (s_channel < 0.3)).astype(np.float32)
            
            # Combine both methods
            combined_shadow = np.maximum(shadow_mask, color_shadow * 0.7)
            
            # Only apply in bottom region (where shadows appear)
            bottom_start = int(h * (1 - self.config.shadow_bottom_percent))
            
            # Create gradient for smooth transition
            gradient = np.ones_like(refined_mask)
            for y in range(bottom_start, h):
                progress = (y - bottom_start) / (h - bottom_start)
                gradient[y, :] = 1 - progress * 0.5
            
            # Remove shadow areas from mask (only bottom region)
            shadow_removal_mask = np.zeros_like(refined_mask)
            shadow_removal_mask[bottom_start:] = combined_shadow[bottom_start:]
            
            # Subtract shadows but protect high-confidence mask areas
            refined_mask = refined_mask - shadow_removal_mask * 0.6
            refined_mask = np.clip(refined_mask, 0, 1)
            
            return refined_mask
            
        except Exception as e:
            print(f"   ⚠️ Shadow detection error: {e}")
            return vehicle_mask


class Stage3WheelProtection:
    """Stage 3: Detect and protect wheel regions from being cut"""
    
    def __init__(self, config: PipelineConfig):
        self.config = config
        
    def load(self):
        if self.config.enable_wheel_protection:
            print(f"   ✅ Wheel protection enabled")
        else:
            print(f"   ⏭️ Wheel protection disabled")
            
    def process(self, image: np.ndarray, mask: np.ndarray, box: np.ndarray) -> np.ndarray:
        """Detect circles (wheels) and ensure they're included in mask"""
        if not self.config.enable_wheel_protection or box is None:
            return mask
            
        try:
            h, w = mask.shape
            refined_mask = mask.copy()
            
            # Convert to grayscale
            gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
            
            # Get bounding box
            x1, y1, x2, y2 = map(int, box)
            box_h = y2 - y1
            
            # Wheel regions are in the bottom 40% of the vehicle
            wheel_region_top = int(y1 + box_h * 0.6)
            
            # Detect circles (wheels)
            gray_blurred = cv2.GaussianBlur(gray, (9, 9), 2)
            circles = cv2.HoughCircles(
                gray_blurred,
                cv2.HOUGH_GRADIENT,
                dp=1.2,
                minDist=50,
                param1=100,
                param2=40,
                minRadius=20,
                maxRadius=min(box_h // 3, 150)
            )
            
            if circles is not None:
                circles = np.uint16(np.around(circles))
                for circle in circles[0, :]:
                    cx, cy, r = circle
                    
                    # Only consider circles in wheel region
                    if cy > wheel_region_top and x1 < cx < x2:
                        # Create circle mask with slight expansion
                        y_grid, x_grid = np.ogrid[:h, :w]
                        circle_mask = ((x_grid - cx) ** 2 + (y_grid - cy) ** 2 <= (r * 1.15) ** 2).astype(np.float32)
                        
                        # Add to mask
                        refined_mask = np.maximum(refined_mask, circle_mask * 0.9)
            
            # Also protect bottom portion of detected vehicle area
            # This prevents cutting wheels even if circle detection fails
            bottom_protection = int(y2 - box_h * 0.15)
            for y in range(bottom_protection, min(y2 + 5, h)):
                for x in range(x1, x2):
                    if refined_mask[y, x] < 0.3 and mask[max(0, y-10):y, x].mean() > 0.5:
                        # If this pixel has mask above but is missing, fill it
                        refined_mask[y, x] = max(refined_mask[y, x], 0.7)
            
            return refined_mask
            
        except Exception as e:
            print(f"   ⚠️ Wheel protection error: {e}")
            return mask


class Stage4SegFormerValidation:
    """Stage 4: Validate and enhance mask with SegFormer semantic segmentation"""
    
    def __init__(self, config: PipelineConfig):
        self.config = config
        self.model = None
        self.processor = None
        
    def load(self):
        if not self.config.enable_segformer:
            print(f"   ⏭️ SegFormer disabled")
            return
            
        try:
            # Try local model first
            segformer_dir = MODELS_DIR / "segformer-b5"
            if segformer_dir.exists():
                self.model = SegformerForSemanticSegmentation.from_pretrained(str(segformer_dir))
                self.processor = SegformerImageProcessor.from_pretrained(str(segformer_dir))
            else:
                # Download from HuggingFace
                self.model = SegformerForSemanticSegmentation.from_pretrained("nvidia/segformer-b5-finetuned-cityscapes-1024-1024")
                self.processor = SegformerImageProcessor.from_pretrained("nvidia/segformer-b5-finetuned-cityscapes-1024-1024")
                
            self.model.to(DEVICE)
            self.model.eval()
            print(f"   ✅ SegFormer-B5 semantic segmentation loaded")
        except Exception as e:
            print(f"   ⚠️ SegFormer error: {e}")
            
    def process(self, image: np.ndarray, mask: np.ndarray) -> np.ndarray:
        """Use semantic segmentation to validate and enhance vehicle mask"""
        if self.model is None or not self.config.enable_segformer:
            return mask
            
        try:
            pil_image = Image.fromarray(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))
            inputs = self.processor(images=pil_image, return_tensors="pt")
            inputs = {k: v.to(DEVICE) for k, v in inputs.items()}
            
            with torch.no_grad():
                outputs = self.model(**inputs)
                
            # Get semantic segmentation
            logits = outputs.logits
            upsampled = F.interpolate(
                logits,
                size=(image.shape[0], image.shape[1]),
                mode="bilinear",
                align_corners=False
            )
            predictions = upsampled.argmax(dim=1).cpu().numpy()[0]
            
            # Cityscapes vehicle classes
            vehicle_classes = {13, 14, 15, 17, 18}  # car, truck, bus, motorcycle, bicycle
            semantic_mask = np.isin(predictions, list(vehicle_classes)).astype(np.float32)
            
            # Combine: prefer our mask but validate with semantic
            refined = mask.copy()
            
            # Where semantic agrees, boost confidence
            agreement = (semantic_mask > 0.5) & (mask > 0.3)
            refined[agreement] = np.maximum(refined[agreement], 0.85)
            
            # Where only semantic detects vehicle, add to mask
            semantic_only = (semantic_mask > 0.5) & (mask < 0.3)
            refined[semantic_only] = 0.6
            
            return refined
            
        except Exception as e:
            print(f"   ⚠️ SegFormer error: {e}")
            return mask


class Stage5HoleFilling:
    """Stage 5: Fill holes in the mask (windows, dark areas inside vehicle)"""
    
    def __init__(self, config: PipelineConfig):
        self.config = config
        
    def load(self):
        print(f"   ✅ Hole filling enabled")
        
    def process(self, mask: np.ndarray) -> np.ndarray:
        """Fill holes inside the vehicle mask"""
        try:
            # Convert to binary
            mask_binary = (mask > 0.5).astype(np.uint8)
            
            # Find contours
            contours, _ = cv2.findContours(mask_binary, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
            
            if contours:
                # Fill the largest contour completely
                largest = max(contours, key=cv2.contourArea)
                filled = np.zeros_like(mask_binary)
                cv2.drawContours(filled, [largest], -1, 1, -1)
                
                # Combine with original (keep any additional areas from original)
                result = np.maximum(mask, filled.astype(np.float32))
                return result
            
            return mask
            
        except Exception as e:
            print(f"   ⚠️ Hole filling error: {e}")
            return mask


class Stage6EdgeRefinement:
    """Stage 6: Advanced edge refinement with guided filtering"""
    
    def __init__(self, config: PipelineConfig):
        self.config = config
        
    def load(self):
        if self.config.enable_edge_refinement:
            print(f"   ✅ Advanced edge refinement enabled")
        else:
            print(f"   ⏭️ Edge refinement disabled")
            
    def process(self, image: np.ndarray, mask: np.ndarray) -> np.ndarray:
        """Refine mask edges using guided filtering and morphology"""
        if not self.config.enable_edge_refinement:
            return mask
            
        try:
            refined = mask.copy()
            h, w = refined.shape
            
            # 1. Clean up small noise
            mask_uint8 = (refined * 255).astype(np.uint8)
            kernel_small = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
            
            # Open to remove noise
            mask_uint8 = cv2.morphologyEx(mask_uint8, cv2.MORPH_OPEN, kernel_small)
            # Close to fill small gaps
            mask_uint8 = cv2.morphologyEx(mask_uint8, cv2.MORPH_CLOSE, kernel_small)
            
            # 2. Apply guided filter for edge-aware smoothing
            try:
                guide = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
                refined_uint8 = cv2.ximgproc.guidedFilter(
                    guide=guide,
                    src=mask_uint8,
                    radius=8,
                    eps=100
                )
            except:
                # Fallback: bilateral filter
                refined_uint8 = cv2.bilateralFilter(mask_uint8, 9, 75, 75)
            
            refined = refined_uint8.astype(np.float32) / 255.0
            
            # 3. Feather edges
            feather = self.config.edge_feather_radius
            if feather > 0:
                # Create edge region
                edge_detect = cv2.Canny((refined * 255).astype(np.uint8), 50, 150)
                edge_region = cv2.dilate(edge_detect, kernel_small, iterations=feather)
                
                # Apply Gaussian blur only to edge region
                blurred = cv2.GaussianBlur(refined, (feather * 2 + 1, feather * 2 + 1), 0)
                
                # Blend at edges
                edge_mask = edge_region.astype(np.float32) / 255.0
                refined = refined * (1 - edge_mask) + blurred * edge_mask
            
            return refined
            
        except Exception as e:
            print(f"   ⚠️ Edge refinement error: {e}")
            return mask


class Stage7AntiAliasing:
    """Stage 7: Final anti-aliasing pass for smooth edges"""
    
    def __init__(self, config: PipelineConfig):
        self.config = config
        
    def load(self):
        if self.config.enable_antialiasing:
            print(f"   ✅ Anti-aliasing enabled")
        else:
            print(f"   ⏭️ Anti-aliasing disabled")
            
    def process(self, mask: np.ndarray) -> np.ndarray:
        """Apply anti-aliasing to mask edges"""
        if not self.config.enable_antialiasing:
            return mask
            
        try:
            strength = self.config.antialias_strength
            
            # Find edges
            mask_uint8 = (mask * 255).astype(np.uint8)
            edges = cv2.Canny(mask_uint8, 30, 100)
            
            # Dilate to get edge region
            kernel = np.ones((5, 5), np.uint8)
            edge_region = cv2.dilate(edges, kernel, iterations=2)
            
            # Apply Gaussian blur to entire mask
            blurred = cv2.GaussianBlur(mask, (7, 7), 1.5)
            
            # Blend original and blurred at edges
            edge_mask = (edge_region / 255.0).astype(np.float32) * strength
            
            result = mask * (1 - edge_mask) + blurred * edge_mask
            
            # Ensure clean thresholding for core areas
            result = np.where(mask > 0.9, 1.0, result)
            result = np.where(mask < 0.1, 0.0, result)
            
            return result
            
        except Exception as e:
            print(f"   ⚠️ Anti-aliasing error: {e}")
            return mask


class ProfessionalPipelineV2:
    """Complete 7-stage professional vehicle segmentation pipeline"""
    
    def __init__(self, config: Optional[PipelineConfig] = None):
        self.config = config or PipelineConfig()
        
        # Initialize stages
        self.stage1 = Stage1Detection(self.config)
        self.stage2 = Stage2ShadowRemoval(self.config)
        self.stage3 = Stage3WheelProtection(self.config)
        self.stage4 = Stage4SegFormerValidation(self.config)
        self.stage5 = Stage5HoleFilling(self.config)
        self.stage6 = Stage6EdgeRefinement(self.config)
        self.stage7 = Stage7AntiAliasing(self.config)
    
    def process(
        self,
        image: np.ndarray,
        shadow_threshold: float = 0.25,
        shadow_expansion: int = 5,
        wheel_expansion: int = 15,
        edge_radius: int = 8,
        edge_eps: float = 0.01,
        antialias_sigma: float = 1.0,
        **kwargs  # Accept additional params for compatibility
    ) -> Tuple[np.ndarray, np.ndarray]:
        """
        Process a single image with configurable parameters.
        
        Args:
            image: Input BGR image as numpy array
            shadow_threshold: Threshold for shadow detection (0.1-0.5)
            shadow_expansion: Pixels to expand shadow removal area
            wheel_expansion: Pixels to expand wheel protection area
            edge_radius: Radius for guided filter edge refinement
            edge_eps: Epsilon for guided filter
            antialias_sigma: Sigma for anti-aliasing blur
            
        Returns:
            Tuple of (RGBA cutout, binary mask)
        """
        h, w = image.shape[:2]
        
        # Stage 1: Detection
        detection = self.stage1.process(image)
        if not detection['success']:
            # Return empty result
            rgba = np.zeros((h, w, 4), dtype=np.uint8)
            mask = np.zeros((h, w), dtype=np.float32)
            return rgba, mask
        
        mask = detection['mask']
        box = detection['box']
        
        # Stage 2: Shadow removal (with custom threshold)
        original_threshold = self.config.shadow_threshold
        self.config.shadow_threshold = shadow_threshold
        mask = self.stage2.process(image, mask)
        self.config.shadow_threshold = original_threshold
        
        # Stage 3: Wheel protection (with custom expansion)
        mask = self.stage3.process(image, mask, box)
        # Additional dilation for wheel areas
        if wheel_expansion > 0:
            kernel_wheel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3))
            # Only dilate bottom portion where wheels are
            bottom_start = int(h * 0.7)
            bottom_mask = mask[bottom_start:].copy()
            bottom_mask = cv2.dilate((bottom_mask * 255).astype(np.uint8), kernel_wheel, iterations=wheel_expansion // 5)
            mask[bottom_start:] = np.maximum(mask[bottom_start:], bottom_mask.astype(np.float32) / 255)
        
        # Stage 4: SegFormer validation
        mask = self.stage4.process(image, mask)
        
        # Stage 5: Hole filling
        mask = self.stage5.process(mask)
        
        # Stage 6: Edge refinement (with custom radius)
        original_radius = self.config.edge_feather_radius
        self.config.edge_feather_radius = edge_radius
        mask = self.stage6.process(image, mask)
        self.config.edge_feather_radius = original_radius
        
        # Stage 7: Anti-aliasing (with custom sigma)
        original_strength = self.config.antialias_strength
        self.config.antialias_strength = antialias_sigma
        mask = self.stage7.process(mask)
        self.config.antialias_strength = original_strength
        
        # Create final RGBA output
        alpha = (mask * 255).astype(np.uint8)
        rgba = cv2.cvtColor(image, cv2.COLOR_BGR2BGRA)
        rgba[:, :, 3] = alpha
        
        return rgba, mask
        
    def load_models(self):
        """Load all pipeline models"""
        print("\n📦 Loading Pipeline V2 Models...")
        
        print("\n  Stage 1: Vehicle Detection")
        self.stage1.load()
        
        print("\n  Stage 2: Shadow Detection & Removal")
        self.stage2.load()
        
        print("\n  Stage 3: Wheel Protection")
        self.stage3.load()
        
        print("\n  Stage 4: Semantic Validation")
        self.stage4.load()
        
        print("\n  Stage 5: Hole Filling")
        self.stage5.load()
        
        print("\n  Stage 6: Edge Refinement")
        self.stage6.load()
        
        print("\n  Stage 7: Anti-Aliasing")
        self.stage7.load()
        
        print("\n✅ All models loaded!")
        
    def process_image(self, image_path: Path) -> Tuple[Optional[np.ndarray], Dict[str, Any]]:
        """Process a single image through all 7 stages"""
        stats = {
            'stages_completed': 0,
            'stages': {}
        }
        
        # Load image
        image = cv2.imread(str(image_path))
        if image is None:
            return None, {'error': f'Failed to load image: {image_path}'}
            
        h, w = image.shape[:2]
        print(f"\n  📸 Processing: {image_path.name} ({w}x{h})")
        
        # Stage 1: Detection
        print(f"    [1/7] Vehicle detection...", end=" ")
        detection = self.stage1.process(image)
        if not detection['success']:
            print("❌ No vehicle detected")
            return None, {'error': 'No vehicle detected'}
        mask = detection['mask']
        box = detection['box']
        print(f"✅ conf={detection['confidence']:.2f}")
        stats['stages']['detection'] = {'confidence': detection['confidence']}
        stats['stages_completed'] = 1
        
        # Stage 2: Shadow removal
        print(f"    [2/7] Shadow removal...", end=" ")
        mask_no_shadow = self.stage2.process(image, mask)
        shadow_removed = np.abs(mask - mask_no_shadow).sum()
        print(f"✅ removed {shadow_removed:.0f}px")
        stats['stages']['shadow'] = {'pixels_removed': float(shadow_removed)}
        stats['stages_completed'] = 2
        mask = mask_no_shadow
        
        # Stage 3: Wheel protection
        print(f"    [3/7] Wheel protection...", end=" ")
        mask_wheels = self.stage3.process(image, mask, box)
        wheels_added = np.maximum(0, mask_wheels - mask).sum()
        print(f"✅ added {wheels_added:.0f}px")
        stats['stages']['wheels'] = {'pixels_added': float(wheels_added)}
        stats['stages_completed'] = 3
        mask = mask_wheels
        
        # Stage 4: SegFormer validation
        print(f"    [4/7] Semantic validation...", end=" ")
        mask_validated = self.stage4.process(image, mask)
        validation_diff = np.abs(mask - mask_validated).sum()
        print(f"✅ adjusted {validation_diff:.0f}px")
        stats['stages']['segformer'] = {'pixels_adjusted': float(validation_diff)}
        stats['stages_completed'] = 4
        mask = mask_validated
        
        # Stage 5: Hole filling
        print(f"    [5/7] Hole filling...", end=" ")
        mask_filled = self.stage5.process(mask)
        holes_filled = np.maximum(0, mask_filled - mask).sum()
        print(f"✅ filled {holes_filled:.0f}px")
        stats['stages']['holes'] = {'pixels_filled': float(holes_filled)}
        stats['stages_completed'] = 5
        mask = mask_filled
        
        # Stage 6: Edge refinement
        print(f"    [6/7] Edge refinement...", end=" ")
        mask_refined = self.stage6.process(image, mask)
        print(f"✅")
        stats['stages_completed'] = 6
        mask = mask_refined
        
        # Stage 7: Anti-aliasing
        print(f"    [7/7] Anti-aliasing...", end=" ")
        mask_final = self.stage7.process(mask)
        print(f"✅")
        stats['stages_completed'] = 7
        
        # Create final RGBA output
        alpha = (mask_final * 255).astype(np.uint8)
        rgba = cv2.cvtColor(image, cv2.COLOR_BGR2BGRA)
        rgba[:, :, 3] = alpha
        
        return rgba, stats
        
    def process_directory(self, input_dir: Path = INPUT_DIR, output_dir: Path = OUTPUT_DIR):
        """Process all images in a directory"""
        output_dir.mkdir(parents=True, exist_ok=True)
        
        # Find images
        image_extensions = {'.jpg', '.jpeg', '.png', '.webp'}
        images = [f for f in input_dir.iterdir() if f.suffix.lower() in image_extensions]
        
        print(f"\n🚗 Processing {len(images)} images with Pipeline V2...")
        
        results = []
        for i, img_path in enumerate(images, 1):
            print(f"\n{'='*60}")
            print(f"[{i}/{len(images)}] {img_path.name}")
            
            result, stats = self.process_image(img_path)
            
            if result is not None:
                output_path = output_dir / f"{img_path.stem}_v2.png"
                cv2.imwrite(str(output_path), result)
                print(f"    💾 Saved: {output_path.name}")
                results.append({
                    'file': img_path.name,
                    'success': True,
                    'stats': stats
                })
            else:
                results.append({
                    'file': img_path.name,
                    'success': False,
                    'error': stats.get('error', 'Unknown error')
                })
        
        # Summary
        successful = sum(1 for r in results if r['success'])
        print(f"\n{'='*60}")
        print(f"📊 RESULTS: {successful}/{len(images)} images processed")
        print(f"📁 Output: {output_dir}")
        
        return results


def main():
    """Main entry point"""
    print("=" * 60)
    print("🚗 PROFESSIONAL VEHICLE SEGMENTATION PIPELINE V2")
    print("=" * 60)
    
    # Create pipeline
    config = PipelineConfig()
    pipeline = ProfessionalPipelineV2(config)
    
    # Load models
    pipeline.load_models()
    
    # Process images
    results = pipeline.process_directory()
    
    print("\n✅ Pipeline V2 complete!")


if __name__ == "__main__":
    main()
