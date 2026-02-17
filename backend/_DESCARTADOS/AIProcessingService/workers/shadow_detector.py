#!/usr/bin/env python3
"""
Shadow Detection Module for Vehicle Image Processing

Integrates a deep learning model for accurate shadow detection.
Uses a simplified U-Net architecture with pre-trained weights or
falls back to intelligent color-based detection.

This module works alongside SAM2 and YOLO to improve vehicle segmentation
by identifying and removing floor shadows that are often incorrectly
included in the vehicle mask.
"""

import os
import logging
import numpy as np
import torch
import torch.nn as nn
import torch.nn.functional as F
import cv2
from typing import Tuple, Optional
from pathlib import Path

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)


# ============================================================================
# SIMPLE U-NET SHADOW DETECTOR
# A lightweight network trained on shadow detection datasets
# ============================================================================

class ConvBlock(nn.Module):
    """Double convolution block with batch normalization"""
    def __init__(self, in_ch: int, out_ch: int):
        super().__init__()
        self.conv = nn.Sequential(
            nn.Conv2d(in_ch, out_ch, 3, padding=1, bias=False),
            nn.BatchNorm2d(out_ch),
            nn.ReLU(inplace=True),
            nn.Conv2d(out_ch, out_ch, 3, padding=1, bias=False),
            nn.BatchNorm2d(out_ch),
            nn.ReLU(inplace=True)
        )
    
    def forward(self, x):
        return self.conv(x)


class ShadowUNet(nn.Module):
    """
    Lightweight U-Net for shadow detection.
    Input: RGB image (3, H, W)
    Output: Shadow probability map (1, H, W)
    """
    def __init__(self, in_channels: int = 3, base_channels: int = 32):
        super().__init__()
        
        # Encoder (downsampling path)
        self.enc1 = ConvBlock(in_channels, base_channels)
        self.enc2 = ConvBlock(base_channels, base_channels * 2)
        self.enc3 = ConvBlock(base_channels * 2, base_channels * 4)
        self.enc4 = ConvBlock(base_channels * 4, base_channels * 8)
        
        # Bottleneck
        self.bottleneck = ConvBlock(base_channels * 8, base_channels * 16)
        
        # Decoder (upsampling path)
        self.up4 = nn.ConvTranspose2d(base_channels * 16, base_channels * 8, 2, stride=2)
        self.dec4 = ConvBlock(base_channels * 16, base_channels * 8)
        
        self.up3 = nn.ConvTranspose2d(base_channels * 8, base_channels * 4, 2, stride=2)
        self.dec3 = ConvBlock(base_channels * 8, base_channels * 4)
        
        self.up2 = nn.ConvTranspose2d(base_channels * 4, base_channels * 2, 2, stride=2)
        self.dec2 = ConvBlock(base_channels * 4, base_channels * 2)
        
        self.up1 = nn.ConvTranspose2d(base_channels * 2, base_channels, 2, stride=2)
        self.dec1 = ConvBlock(base_channels * 2, base_channels)
        
        # Output
        self.out_conv = nn.Conv2d(base_channels, 1, 1)
        
        self.pool = nn.MaxPool2d(2)
    
    def forward(self, x):
        # Encoder
        e1 = self.enc1(x)
        e2 = self.enc2(self.pool(e1))
        e3 = self.enc3(self.pool(e2))
        e4 = self.enc4(self.pool(e3))
        
        # Bottleneck
        b = self.bottleneck(self.pool(e4))
        
        # Decoder with skip connections
        d4 = self.dec4(torch.cat([self.up4(b), e4], dim=1))
        d3 = self.dec3(torch.cat([self.up3(d4), e3], dim=1))
        d2 = self.dec2(torch.cat([self.up2(d3), e2], dim=1))
        d1 = self.dec1(torch.cat([self.up1(d2), e1], dim=1))
        
        return torch.sigmoid(self.out_conv(d1))


# ============================================================================
# SHADOW DETECTOR CLASS
# Main interface for shadow detection in vehicle images
# ============================================================================

class ShadowDetector:
    """
    Shadow detection for vehicle images.
    
    Uses a combination of:
    1. Deep learning model (when available)
    2. Color-based analysis (LAB + HSV colorspace)
    3. Edge and texture features
    
    Optimized for studio photography with controlled lighting.
    """
    
    # Standard image size for model inference
    MODEL_SIZE = (256, 256)
    
    # Pre-trained weights location
    WEIGHTS_PATH = "/models/shadow_detector.pt"
    
    def __init__(self, device: str = "cpu", use_deep_model: bool = True):
        """
        Initialize shadow detector.
        
        Args:
            device: "cpu" or "cuda"
            use_deep_model: Whether to try loading the neural network
        """
        self.device = device
        self.model = None
        self.use_deep_model = use_deep_model
        
        if use_deep_model:
            self._load_model()
    
    def _load_model(self):
        """Load pre-trained shadow detection model"""
        if os.path.exists(self.WEIGHTS_PATH):
            try:
                self.model = ShadowUNet(in_channels=3, base_channels=32)
                state_dict = torch.load(self.WEIGHTS_PATH, map_location=self.device)
                self.model.load_state_dict(state_dict)
                self.model.to(self.device)
                self.model.eval()
                logger.info(f"✅ Shadow detection model loaded from {self.WEIGHTS_PATH}")
            except Exception as e:
                logger.warning(f"⚠️ Failed to load shadow model: {e}")
                self.model = None
        else:
            logger.info(f"📝 No pre-trained shadow model at {self.WEIGHTS_PATH}, using color-based detection")
    
    def detect_shadows(
        self, 
        image: np.ndarray, 
        mask: Optional[np.ndarray] = None,
        threshold: float = 0.5
    ) -> Tuple[np.ndarray, dict]:
        """
        Detect shadows in an image.
        
        Args:
            image: RGB image (H, W, 3), uint8
            mask: Optional binary mask to restrict detection (H, W), uint8
            threshold: Probability threshold for shadow classification
        
        Returns:
            shadow_mask: Binary shadow mask (H, W), uint8 (0 or 255)
            metadata: Dict with detection info
        """
        h, w = image.shape[:2]
        
        # Try deep model first
        if self.model is not None:
            try:
                shadow_prob = self._deep_detect(image)
                shadow_mask = (shadow_prob > threshold).astype(np.uint8) * 255
                method = "deep_learning"
            except Exception as e:
                logger.warning(f"Deep model failed: {e}, falling back to color-based")
                shadow_mask, shadow_prob = self._color_detect(image)
                method = "color_based"
        else:
            shadow_mask, shadow_prob = self._color_detect(image)
            method = "color_based"
        
        # Apply mask constraint if provided
        if mask is not None:
            shadow_mask = cv2.bitwise_and(shadow_mask, mask)
        
        metadata = {
            "method": method,
            "shadow_pixels": int(np.sum(shadow_mask > 0)),
            "shadow_ratio": float(np.sum(shadow_mask > 0) / (h * w)),
            "threshold": threshold
        }
        
        return shadow_mask, metadata
    
    def _deep_detect(self, image: np.ndarray) -> np.ndarray:
        """Use neural network for shadow detection"""
        h, w = image.shape[:2]
        
        # Preprocess
        img_resized = cv2.resize(image, self.MODEL_SIZE)
        img_tensor = torch.from_numpy(img_resized).permute(2, 0, 1).float() / 255.0
        img_tensor = img_tensor.unsqueeze(0).to(self.device)
        
        # Normalize using ImageNet stats
        mean = torch.tensor([0.485, 0.456, 0.406]).view(1, 3, 1, 1).to(self.device)
        std = torch.tensor([0.229, 0.224, 0.225]).view(1, 3, 1, 1).to(self.device)
        img_tensor = (img_tensor - mean) / std
        
        # Inference
        with torch.no_grad():
            shadow_prob = self.model(img_tensor)
        
        # Resize back to original size
        shadow_prob = F.interpolate(shadow_prob, size=(h, w), mode='bilinear', align_corners=False)
        shadow_prob = shadow_prob.squeeze().cpu().numpy()
        
        return shadow_prob
    
    def _color_detect(self, image: np.ndarray) -> Tuple[np.ndarray, np.ndarray]:
        """
        Color-based shadow detection using LAB and HSV analysis.
        
        Shadows typically have:
        - Lower luminance (L channel in LAB)
        - Lower saturation (S channel in HSV)
        - Similar hue to surrounding area (not a distinct color)
        """
        h, w = image.shape[:2]
        
        # Convert to LAB and HSV
        lab = cv2.cvtColor(image, cv2.COLOR_RGB2LAB)
        hsv = cv2.cvtColor(image, cv2.COLOR_RGB2HSV)
        
        L = lab[:, :, 0].astype(np.float32)
        S_hsv = hsv[:, :, 1].astype(np.float32)
        V = hsv[:, :, 2].astype(np.float32)
        
        # Shadow probability based on multiple features:
        # 1. Low luminance in LAB
        # 2. Low saturation in HSV
        # 3. Not too dark (to avoid tires)
        # 4. Not too bright (definitely not shadow)
        
        # Normalize channels
        L_norm = L / 255.0
        S_norm = S_hsv / 255.0
        V_norm = V / 255.0
        
        # Shadow probability formula:
        # - Lower L = more likely shadow (but not below 0.15 to protect tires)
        # - Lower S = more likely shadow (grayish)
        # - Mid-range V = more likely shadow (not pure black, not bright)
        
        # L contribution: shadows have medium-low luminance
        L_prob = np.clip(0.6 - L_norm, 0, 1) * 1.5  # Max at L=0.15
        
        # S contribution: shadows are desaturated
        S_prob = np.clip(0.4 - S_norm, 0, 1) * 2.0  # Max at S=0
        
        # V contribution: shadows are not too dark (tires) or too bright
        V_prob = np.exp(-((V_norm - 0.4) ** 2) / 0.1)  # Gaussian centered at 0.4
        
        # Combine probabilities
        shadow_prob = (L_prob * 0.4 + S_prob * 0.4 + V_prob * 0.2)
        shadow_prob = np.clip(shadow_prob, 0, 1)
        
        # Apply Gaussian blur to smooth
        shadow_prob = cv2.GaussianBlur(shadow_prob, (5, 5), 0)
        
        # Threshold for binary mask
        shadow_mask = (shadow_prob > 0.4).astype(np.uint8) * 255
        
        return shadow_mask, shadow_prob
    
    def remove_shadows_from_mask(
        self,
        vehicle_mask: np.ndarray,
        image: np.ndarray,
        protect_wheels: bool = True,
        bottom_region_ratio: float = 0.30
    ) -> Tuple[np.ndarray, int]:
        """
        Remove shadow regions from a vehicle mask.
        
        This is the main function used after SAM2 segmentation to clean up
        floor shadows that were incorrectly included in the mask.
        
        Args:
            vehicle_mask: Binary mask from SAM2 (H, W), uint8
            image: Original RGB image (H, W, 3)
            protect_wheels: Whether to protect dark wheel areas from removal
            bottom_region_ratio: Only process bottom X% of mask (where shadows are)
        
        Returns:
            cleaned_mask: Mask with shadows removed
            removed_count: Number of shadow pixels removed
        """
        h, w = vehicle_mask.shape[:2]
        cleaned_mask = vehicle_mask.copy()
        
        # Find the bounding box of the current mask
        mask_rows = np.any(vehicle_mask > 0, axis=1)
        if not np.any(mask_rows):
            return vehicle_mask, 0
        
        mask_row_indices = np.where(mask_rows)[0]
        mask_top = mask_row_indices.min()
        mask_bottom = mask_row_indices.max()
        mask_height = mask_bottom - mask_top
        
        # Only process the bottom region
        shadow_zone_start = mask_bottom - int(mask_height * bottom_region_ratio)
        
        # Create zone mask (only bottom region)
        zone_mask = np.zeros_like(vehicle_mask)
        zone_mask[shadow_zone_start:, :] = 255
        
        # Detect shadows
        shadow_mask, metadata = self.detect_shadows(image)
        
        # Shadows to remove: in zone AND in vehicle mask AND detected as shadow
        shadows_in_zone = cv2.bitwise_and(shadow_mask, zone_mask)
        shadows_in_vehicle = cv2.bitwise_and(shadows_in_zone, vehicle_mask)
        
        if protect_wheels:
            # Protect very dark areas (likely tires)
            hsv = cv2.cvtColor(image, cv2.COLOR_RGB2HSV)
            V = hsv[:, :, 2]
            
            # Tires are typically very dark (V < 60)
            # Also protect areas with some saturation (colored parts)
            tire_protection = (V < 60) | (hsv[:, :, 1] > 50)
            tire_protection_mask = tire_protection.astype(np.uint8) * 255
            
            # Don't remove pixels that look like tires
            shadows_in_vehicle = cv2.bitwise_and(
                shadows_in_vehicle,
                cv2.bitwise_not(tire_protection_mask)
            )
        
        # Additional protection: don't remove pixels that are connected to large car regions
        # (using connected components)
        if shadows_in_vehicle.sum() > 0:
            # Find connected components in the shadow region
            num_labels, labels = cv2.connectedComponents(shadows_in_vehicle)
            
            # Only remove small disconnected shadow blobs
            for label_id in range(1, num_labels):
                component = (labels == label_id).astype(np.uint8) * 255
                component_area = np.sum(component > 0)
                
                # If the component is too large (>5% of mask area), it might be car
                mask_area = np.sum(vehicle_mask > 0)
                if component_area > mask_area * 0.05:
                    # Don't remove this - might be part of car
                    shadows_in_vehicle[labels == label_id] = 0
        
        # Count pixels to remove
        removed_count = int(np.sum(shadows_in_vehicle > 0))
        
        # Remove shadows from mask
        if removed_count > 0:
            cleaned_mask = cv2.bitwise_and(
                cleaned_mask, 
                cv2.bitwise_not(shadows_in_vehicle)
            )
            logger.info(f"  🌑 Shadow detector removed {removed_count} pixels from bottom {int(bottom_region_ratio*100)}%")
        
        return cleaned_mask, removed_count


# ============================================================================
# TRAINING FUNCTION (for generating pre-trained weights)
# ============================================================================

def train_shadow_model(
    dataset_path: str,
    output_path: str = "/models/shadow_detector.pt",
    epochs: int = 50,
    batch_size: int = 8,
    learning_rate: float = 1e-4
):
    """
    Train the shadow detection model on a shadow dataset.
    
    Expected dataset structure:
        dataset_path/
            images/
                img001.jpg
                img002.jpg
                ...
            masks/
                img001.png  (shadow regions = white)
                img002.png
                ...
    
    Popular shadow datasets:
    - SBU Shadow Dataset
    - UCF Shadow Dataset  
    - ISTD Dataset
    """
    from torch.utils.data import Dataset, DataLoader
    from torchvision import transforms
    from PIL import Image
    
    logger.info("🎓 Starting shadow model training...")
    
    class ShadowDataset(Dataset):
        def __init__(self, root_dir, transform=None):
            self.root_dir = Path(root_dir)
            self.image_dir = self.root_dir / "images"
            self.mask_dir = self.root_dir / "masks"
            self.images = sorted(list(self.image_dir.glob("*.jpg")) + list(self.image_dir.glob("*.png")))
            self.transform = transform
        
        def __len__(self):
            return len(self.images)
        
        def __getitem__(self, idx):
            img_path = self.images[idx]
            mask_path = self.mask_dir / f"{img_path.stem}.png"
            
            image = Image.open(img_path).convert("RGB")
            mask = Image.open(mask_path).convert("L")
            
            if self.transform:
                image = self.transform(image)
                mask = transforms.ToTensor()(mask)
            
            return image, mask
    
    # Transforms
    transform = transforms.Compose([
        transforms.Resize((256, 256)),
        transforms.ToTensor(),
        transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225])
    ])
    
    # Dataset and loader
    dataset = ShadowDataset(dataset_path, transform=transform)
    loader = DataLoader(dataset, batch_size=batch_size, shuffle=True, num_workers=2)
    
    # Model
    device = "cuda" if torch.cuda.is_available() else "cpu"
    model = ShadowUNet(in_channels=3, base_channels=32).to(device)
    
    # Loss and optimizer
    criterion = nn.BCELoss()
    optimizer = torch.optim.Adam(model.parameters(), lr=learning_rate)
    
    # Training loop
    for epoch in range(epochs):
        model.train()
        total_loss = 0
        
        for images, masks in loader:
            images = images.to(device)
            masks = masks.to(device)
            
            optimizer.zero_grad()
            outputs = model(images)
            loss = criterion(outputs, masks)
            loss.backward()
            optimizer.step()
            
            total_loss += loss.item()
        
        avg_loss = total_loss / len(loader)
        logger.info(f"Epoch {epoch+1}/{epochs}, Loss: {avg_loss:.4f}")
    
    # Save model
    torch.save(model.state_dict(), output_path)
    logger.info(f"✅ Model saved to {output_path}")


# ============================================================================
# STANDALONE TESTING
# ============================================================================

if __name__ == "__main__":
    import sys
    
    print("=" * 60)
    print("🌑 Shadow Detector Test")
    print("=" * 60)
    
    # Create detector
    detector = ShadowDetector(device="cpu", use_deep_model=True)
    
    # Test with a simple synthetic image
    test_image = np.zeros((256, 256, 3), dtype=np.uint8)
    
    # Create a synthetic shadow (gray area)
    test_image[150:200, 50:200] = [100, 100, 100]  # Gray shadow
    test_image[50:150, 50:200] = [255, 100, 100]   # Red car body
    test_image[160:190, 60:80] = [30, 30, 30]      # Dark wheel
    test_image[160:190, 170:190] = [30, 30, 30]    # Dark wheel
    
    # Create a mask (entire car + shadow)
    test_mask = np.zeros((256, 256), dtype=np.uint8)
    test_mask[50:200, 50:200] = 255
    
    # Detect and remove shadows
    cleaned_mask, removed = detector.remove_shadows_from_mask(
        test_mask, 
        test_image,
        protect_wheels=True,
        bottom_region_ratio=0.5
    )
    
    print(f"Original mask pixels: {np.sum(test_mask > 0)}")
    print(f"Cleaned mask pixels: {np.sum(cleaned_mask > 0)}")
    print(f"Shadow pixels removed: {removed}")
    print("=" * 60)
