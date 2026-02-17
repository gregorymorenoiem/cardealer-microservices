#!/usr/bin/env python3
"""
Simplified Model Downloader for Professional Pipeline
Downloads essential models for vehicle segmentation
"""

import os
import sys
from pathlib import Path

# Check for required packages
try:
    import requests
    from tqdm import tqdm
except ImportError:
    print("Installing required packages...")
    os.system(f"{sys.executable} -m pip install requests tqdm -q")
    import requests
    from tqdm import tqdm


def download_file(url: str, dest: Path, desc: str = "Downloading") -> bool:
    """Download file with progress bar"""
    try:
        response = requests.get(url, stream=True, allow_redirects=True, timeout=30)
        response.raise_for_status()
        
        total_size = int(response.headers.get('content-length', 0))
        dest.parent.mkdir(parents=True, exist_ok=True)
        
        with open(dest, 'wb') as f:
            with tqdm(total=total_size, unit='B', unit_scale=True, desc=desc, ncols=80) as pbar:
                for chunk in response.iter_content(chunk_size=8192):
                    if chunk:
                        f.write(chunk)
                        pbar.update(len(chunk))
        return True
    except Exception as e:
        print(f"❌ Error: {e}")
        return False


def main():
    models_dir = Path(os.getenv("MODELS_DIR", "./models"))
    models_dir.mkdir(parents=True, exist_ok=True)
    
    print("=" * 60)
    print("🚗 Professional Pipeline - Essential Models Downloader")
    print("=" * 60)
    print(f"📁 Models directory: {models_dir.absolute()}")
    print()
    
    # Essential models
    models = {
        "yolov8x-seg.pt": {
            "url": "https://github.com/ultralytics/assets/releases/download/v8.1.0/yolov8x-seg.pt",
            "desc": "YOLOv8x-seg (137 MB)",
            "size": "137 MB"
        },
        "sam2_hiera_base_plus.pt": {
            "url": "https://dl.fbaipublicfiles.com/segment_anything_2/072824/sam2_hiera_base_plus.pt",
            "desc": "SAM2-Base+ (323 MB)",
            "size": "323 MB"
        },
        "isnet-general-use.pth": {
            "url": "https://github.com/xuebinqin/DIS/releases/download/v1.0/isnet-general-use.pth",
            "desc": "ISNet-DIS (175 MB)",
            "size": "175 MB"
        }
    }
    
    print("📋 Models to download:")
    for name, info in models.items():
        dest = models_dir / name
        status = "✅ EXISTS" if dest.exists() else f"📥 {info['size']}"
        print(f"   • {name}: {status}")
    print()
    
    # Download missing models
    for name, info in models.items():
        dest = models_dir / name
        
        if dest.exists():
            print(f"✅ {name} already exists, skipping")
            continue
        
        print(f"\n📥 Downloading {info['desc']}...")
        if download_file(info["url"], dest, name):
            print(f"✅ Saved to {dest}")
        else:
            print(f"❌ Failed to download {name}")
    
    print("\n" + "=" * 60)
    print("✅ Download complete!")
    print("=" * 60)
    
    # Verify
    print("\n📊 Model Status:")
    for name in models:
        dest = models_dir / name
        if dest.exists():
            size_mb = dest.stat().st_size / (1024 * 1024)
            print(f"   ✅ {name}: {size_mb:.1f} MB")
        else:
            print(f"   ❌ {name}: MISSING")


if __name__ == "__main__":
    main()
