#!/usr/bin/env python3
"""Download ISNet model via rembg"""

import subprocess
import sys

# Install rembg with CPU support
print("📦 Installing rembg[cpu]...")
subprocess.run([sys.executable, "-m", "pip", "install", "rembg[cpu]", "onnxruntime", "-q"], check=True)

print("✅ rembg installed")

# Import and download model
from rembg import new_session
import os

print("📥 Downloading ISNet model via rembg...")
session = new_session("isnet-general-use")
print("✅ ISNet model ready!")

# Check location
home = os.path.expanduser("~")
model_path = os.path.join(home, ".u2net", "isnet-general-use.onnx")
if os.path.exists(model_path):
    size = os.path.getsize(model_path) / (1024*1024)
    print(f"📁 Model location: {model_path} ({size:.1f} MB)")
