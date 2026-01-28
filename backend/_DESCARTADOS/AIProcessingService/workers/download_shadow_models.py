#!/usr/bin/env python3
"""
Download pretrained shadow detection models

Models available:
1. BDRAR - Bidirectional Feature Pyramid Network (ECCV 2018)
   - Best quality, requires ResNeXt backbone
   - Download: ~200MB total

2. Custom U-Net trained on SBU dataset
   - Lightweight, faster inference
   - Can be trained locally
"""

import os
import sys
import gdown
import logging
from pathlib import Path

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

# Google Drive file IDs
MODELS = {
    "bdrar": {
        "weights": "1Cw3nUmWEmnTnAVXPn3xZYhQ_uzmWmsr7",  # BDRAR trained weights
        "backbone": "1dnH-IHwmu9xFPlyndqI6MfF4LvH6JKNQ",  # ResNeXt-101 pretrained
        "description": "BDRAR Shadow Detection (ECCV 2018)"
    }
}

MODELS_DIR = Path("/models")


def download_from_gdrive(file_id: str, output_path: str, description: str = ""):
    """Download a file from Google Drive"""
    url = f"https://drive.google.com/uc?id={file_id}"
    
    if os.path.exists(output_path):
        logger.info(f"✅ Already exists: {output_path}")
        return True
    
    logger.info(f"⬇️ Downloading {description or file_id}...")
    try:
        gdown.download(url, output_path, quiet=False)
        if os.path.exists(output_path):
            size_mb = os.path.getsize(output_path) / (1024 * 1024)
            logger.info(f"✅ Downloaded: {output_path} ({size_mb:.1f} MB)")
            return True
        else:
            logger.error(f"❌ Download failed: {output_path}")
            return False
    except Exception as e:
        logger.error(f"❌ Error downloading: {e}")
        return False


def download_bdrar_model():
    """Download BDRAR model and backbone"""
    MODELS_DIR.mkdir(parents=True, exist_ok=True)
    
    # Download BDRAR weights
    success = download_from_gdrive(
        MODELS["bdrar"]["weights"],
        str(MODELS_DIR / "bdrar_shadow_detector.pth"),
        "BDRAR Shadow Detection weights"
    )
    
    if success:
        # Download ResNeXt backbone
        download_from_gdrive(
            MODELS["bdrar"]["backbone"],
            str(MODELS_DIR / "resnext_101_32x4d.pth"),
            "ResNeXt-101 backbone"
        )
    
    return success


def create_lightweight_detector():
    """
    Create a lightweight shadow detector using transfer learning
    from a pre-trained segmentation model (no additional download needed).
    
    Uses PyTorch's pretrained DeepLabV3 with custom head for shadow detection.
    """
    import torch
    import torch.nn as nn
    from torchvision.models.segmentation import deeplabv3_resnet50
    
    logger.info("🔧 Creating lightweight shadow detector from DeepLabV3...")
    
    # Load pretrained DeepLabV3
    model = deeplabv3_resnet50(pretrained=True)
    
    # Modify the classifier for binary output (shadow/non-shadow)
    model.classifier[4] = nn.Conv2d(256, 1, kernel_size=(1, 1))
    model.aux_classifier[4] = nn.Conv2d(256, 1, kernel_size=(1, 1))
    
    # Freeze backbone, only train classifier
    for name, param in model.named_parameters():
        if 'classifier' not in name:
            param.requires_grad = False
    
    # Save model structure
    output_path = MODELS_DIR / "shadow_detector_deeplabv3.pt"
    torch.save(model.state_dict(), output_path)
    logger.info(f"✅ Saved lightweight model to {output_path}")
    
    return model


def main():
    print("=" * 60)
    print("🌑 Shadow Detection Model Downloader")
    print("=" * 60)
    
    if len(sys.argv) > 1:
        model_name = sys.argv[1].lower()
    else:
        print("\nAvailable models:")
        print("  1. bdrar      - BDRAR (best quality, ~200MB)")
        print("  2. lightweight - DeepLabV3-based (uses pretrained weights)")
        print("\nUsage: python download_shadow_models.py [model_name]")
        print("Example: python download_shadow_models.py bdrar")
        return
    
    if model_name == "bdrar":
        download_bdrar_model()
    elif model_name == "lightweight":
        create_lightweight_detector()
    else:
        print(f"Unknown model: {model_name}")
        print("Available: bdrar, lightweight")


if __name__ == "__main__":
    main()
