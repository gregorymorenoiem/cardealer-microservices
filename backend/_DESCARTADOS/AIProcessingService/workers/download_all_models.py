#!/usr/bin/env python3
"""
Download All Models for Professional 7-Stage Vehicle Segmentation Pipeline
Total: ~5.5 GB

Models:
1. YOLOv8x-seg (137 MB) - Vehicle detection
2. GroundingDINO (1.5 GB) - Context understanding  
3. ShadowFormer (200 MB) - Shadow detection
4. SAM2-Hiera-Large (2.4 GB) - Main segmentation
5. CarParts-Seg (500 MB) - Parts segmentation
6. SegFormer-B5 (380 MB) - Semantic validation
7. ISNet-DIS (175 MB) - Edge refinement / Alpha matting
8. RealESRGAN (64 MB) - Image enhancement
"""

import os
import sys
import subprocess
from pathlib import Path

# Install required packages
def ensure_packages():
    packages = ["requests", "tqdm", "huggingface_hub"]
    for pkg in packages:
        try:
            __import__(pkg.replace("-", "_"))
        except ImportError:
            print(f"Installing {pkg}...")
            subprocess.check_call([sys.executable, "-m", "pip", "install", pkg, "-q"])

ensure_packages()

import requests
from tqdm import tqdm
from huggingface_hub import hf_hub_download

# Models directory
MODELS_DIR = Path("./models")
MODELS_DIR.mkdir(exist_ok=True)

# Model definitions
MODELS = {
    # 1. YOLOv8x-seg - Vehicle Detection
    "yolov8x-seg": {
        "filename": "yolov8x-seg.pt",
        "url": "https://github.com/ultralytics/assets/releases/download/v8.1.0/yolov8x-seg.pt",
        "size": "137 MB",
        "source": "direct",
        "stage": 1,
        "description": "Vehicle detection with instance segmentation"
    },
    
    # 2. GroundingDINO - Context Understanding
    "groundingdino": {
        "filename": "groundingdino_swint_ogc.pth",
        "repo": "ShilongLiu/GroundingDINO",
        "hf_filename": "groundingdino_swint_ogc.pth",
        "size": "1.5 GB",
        "source": "huggingface",
        "stage": 1,
        "description": "Open-vocabulary object detection for context"
    },
    
    # 3. ShadowFormer - Shadow Detection (ONNX)
    "shadowformer": {
        "filename": "shadowformer.onnx",
        "url": "https://github.com/PINTO0309/PINTO_model_zoo/raw/main/426_ShadowFormer/shadowformer_istd_384x384.onnx",
        "size": "200 MB",
        "source": "direct",
        "stage": 2,
        "description": "Shadow detection and removal"
    },
    
    # 4. SAM2-Hiera-Large - Main Segmentation
    "sam2-large": {
        "filename": "sam2_hiera_large.pt",
        "url": "https://dl.fbaipublicfiles.com/segment_anything_2/072824/sam2_hiera_large.pt",
        "size": "2.4 GB",
        "source": "direct",
        "stage": 3,
        "description": "Segment Anything 2 - Large model"
    },
    
    # 5. CarParts Segmentation
    "carparts": {
        "filename": "carparts_segformer.pt",
        "repo": "nvidia/segformer-b5-finetuned-cityscapes-1024-1024",
        "size": "500 MB",
        "source": "transformers",
        "stage": 4,
        "description": "Car parts segmentation (wheels, windows, body)"
    },
    
    # 6. SegFormer-B5 - Semantic Validation
    "segformer": {
        "filename": "segformer_b5_cityscapes.pt",
        "repo": "nvidia/segformer-b5-finetuned-cityscapes-1024-1024",
        "size": "380 MB",
        "source": "transformers",
        "stage": 5,
        "description": "Semantic segmentation for validation"
    },
    
    # 7. ISNet-DIS - Edge Refinement / Alpha Matting
    "isnet": {
        "filename": "isnet-general-use.pth",
        "url": "https://huggingface.co/briaai/RMBG-1.4/resolve/main/model.pth",
        "alt_urls": [
            "https://github.com/danielgatis/rembg/releases/download/v0.0.0/isnet-general-use.pth",
            "https://huggingface.co/skytnt/anime-seg/resolve/main/isnet_is.ckpt"
        ],
        "size": "175 MB",
        "source": "direct",
        "stage": 6,
        "description": "High-precision edge refinement"
    },
    
    # 8. RealESRGAN - Image Enhancement
    "realesrgan": {
        "filename": "RealESRGAN_x4plus.pth",
        "url": "https://github.com/xinntao/Real-ESRGAN/releases/download/v0.1.0/RealESRGAN_x4plus.pth",
        "size": "64 MB",
        "source": "direct",
        "stage": 7,
        "description": "Image upscaling and enhancement"
    }
}


def download_file(url: str, dest: Path, desc: str = "Downloading") -> bool:
    """Download file with progress bar"""
    try:
        print(f"   URL: {url[:80]}...")
        response = requests.get(url, stream=True, allow_redirects=True, timeout=60)
        response.raise_for_status()
        
        total_size = int(response.headers.get('content-length', 0))
        
        with open(dest, 'wb') as f:
            with tqdm(total=total_size, unit='B', unit_scale=True, desc=desc, ncols=80) as pbar:
                for chunk in response.iter_content(chunk_size=8192):
                    if chunk:
                        f.write(chunk)
                        pbar.update(len(chunk))
        
        # Verify file size
        if dest.stat().st_size < 1000:  # Less than 1KB = probably an error page
            print(f"   ⚠️ File too small, may be an error page")
            return False
            
        return True
    except Exception as e:
        print(f"   ❌ Error: {e}")
        return False


def download_from_huggingface(repo: str, filename: str, dest: Path) -> bool:
    """Download from Hugging Face Hub"""
    try:
        print(f"   Repo: {repo}")
        downloaded = hf_hub_download(
            repo_id=repo,
            filename=filename,
            local_dir=dest.parent,
            local_dir_use_symlinks=False
        )
        # Move to correct location if needed
        downloaded_path = Path(downloaded)
        if downloaded_path != dest:
            if dest.exists():
                dest.unlink()
            downloaded_path.rename(dest)
        return True
    except Exception as e:
        print(f"   ❌ Error: {e}")
        return False


def download_model(name: str, info: dict, force: bool = False) -> bool:
    """Download a single model"""
    dest = MODELS_DIR / info["filename"]
    
    # Check if already exists
    if dest.exists() and not force:
        size_mb = dest.stat().st_size / (1024 * 1024)
        print(f"   ✅ Already exists ({size_mb:.1f} MB)")
        return True
    
    source = info.get("source", "direct")
    
    if source == "direct":
        # Try main URL first
        url = info.get("url")
        if url and download_file(url, dest, info["filename"]):
            return True
        
        # Try alternative URLs
        for alt_url in info.get("alt_urls", []):
            print(f"   Trying alternative URL...")
            if download_file(alt_url, dest, info["filename"]):
                return True
        
        return False
        
    elif source == "huggingface":
        return download_from_huggingface(
            info["repo"], 
            info.get("hf_filename", info["filename"]), 
            dest
        )
        
    elif source == "transformers":
        # Will be loaded dynamically via transformers library
        print(f"   ℹ️ Will be loaded via transformers library at runtime")
        return True
    
    return False


def check_all_models() -> dict:
    """Check status of all models"""
    status = {}
    for name, info in MODELS.items():
        dest = MODELS_DIR / info["filename"]
        if dest.exists():
            size_mb = dest.stat().st_size / (1024 * 1024)
            status[name] = {"exists": True, "size_mb": size_mb}
        else:
            status[name] = {"exists": False, "size_mb": 0}
    return status


def main():
    print("=" * 70)
    print("🚗 Professional Vehicle Segmentation Pipeline - Model Downloader")
    print("=" * 70)
    print(f"\n📁 Models directory: {MODELS_DIR.absolute()}")
    
    # Check current status
    print("\n📊 Current Model Status:")
    print("-" * 70)
    
    status = check_all_models()
    total_size = 0
    missing_size = 0
    
    for name, info in MODELS.items():
        s = status[name]
        stage = info.get("stage", "?")
        if s["exists"]:
            print(f"   ✅ Stage {stage}: {name:<15} ({s['size_mb']:.1f} MB)")
            total_size += s["size_mb"]
        else:
            print(f"   ❌ Stage {stage}: {name:<15} (need {info['size']})")
            # Parse size string to MB
            size_str = info['size'].lower()
            if 'gb' in size_str:
                missing_size += float(size_str.replace('gb', '').strip()) * 1024
            elif 'mb' in size_str:
                missing_size += float(size_str.replace('mb', '').strip())
    
    print("-" * 70)
    print(f"   Downloaded: {total_size:.1f} MB")
    print(f"   Missing: ~{missing_size:.0f} MB")
    
    # Ask to proceed
    missing = [n for n, s in status.items() if not s["exists"]]
    
    if not missing:
        print("\n✅ All models already downloaded!")
        return
    
    print(f"\n📥 Will download {len(missing)} missing models...")
    print("   This may take several minutes depending on your connection.\n")
    
    # Download missing models
    for i, name in enumerate(missing, 1):
        info = MODELS[name]
        print(f"\n[{i}/{len(missing)}] 📥 {name} ({info['size']})")
        print(f"   {info['description']}")
        
        if download_model(name, info):
            dest = MODELS_DIR / info["filename"]
            if dest.exists():
                size_mb = dest.stat().st_size / (1024 * 1024)
                print(f"   ✅ Downloaded: {size_mb:.1f} MB")
        else:
            print(f"   ❌ Failed to download {name}")
    
    # Final status
    print("\n" + "=" * 70)
    print("📊 FINAL STATUS")
    print("=" * 70)
    
    final_status = check_all_models()
    success = sum(1 for s in final_status.values() if s["exists"])
    total = len(MODELS)
    
    for name, info in MODELS.items():
        s = final_status[name]
        stage = info.get("stage", "?")
        icon = "✅" if s["exists"] else "❌"
        size = f"{s['size_mb']:.1f} MB" if s["exists"] else "MISSING"
        print(f"   {icon} Stage {stage}: {name:<15} - {size}")
    
    print("-" * 70)
    print(f"   Total: {success}/{total} models ready")
    print("=" * 70)


if __name__ == "__main__":
    main()
