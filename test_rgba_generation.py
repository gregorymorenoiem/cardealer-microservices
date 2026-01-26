#!/usr/bin/env python3
"""
Test script to verify RGBA image generation with transparent background
"""
import numpy as np
from PIL import Image
import os

# Create test directories
os.makedirs('/tmp/rgba-test', exist_ok=True)

# Create a simple test image (RGB): white car on dark background
h, w = 480, 640
image = np.zeros((h, w, 3), dtype=np.uint8)

# Draw a "car" shape (white rectangle in the middle)
car_x1, car_x2 = 150, 500
car_y1, car_y2 = 150, 350
image[car_y1:car_y2, car_x1:car_x2] = 255  # White car

print(f"✅ Created test image: {h}x{w} with white car rectangle")
print(f"   Car region: x=[{car_x1}:{car_x2}], y=[{car_y1}:{car_y2}]")

# Create a binary mask (1 = car, 0 = background)
mask = np.zeros((h, w), dtype=np.uint8)
mask[car_y1:car_y2, car_x1:car_x2] = 1  # 1 where car is

print(f"\n✅ Created binary mask: {h}x{w}")
print(f"   Mask mean: {mask.mean():.3f} (1.0 = all car, 0.0 = all background)")
print(f"   Mask pixels=1: {(mask == 1).sum()} out of {h*w}")

# ========== Method 1: RGBA with transparent background ==========
print("\n📝 METHOD 1: Creating RGBA image (transparent background)...")
mask_binary = (mask > 0.5).astype(np.uint8)
h_img, w_img = image.shape[:2]
rgba_array = np.zeros((h_img, w_img, 4), dtype=np.uint8)
rgba_array[:, :, 0:3] = image  # Copy RGB
rgba_array[:, :, 3] = mask_binary * 255  # Alpha channel: 255 = opaque, 0 = transparent

print(f"   Alpha channel stats:")
print(f"   - Min: {rgba_array[:,:,3].min()}")
print(f"   - Max: {rgba_array[:,:,3].max()}")
print(f"   - Mean: {rgba_array[:,:,3].mean():.1f}")

rgba_img = Image.fromarray(rgba_array, 'RGBA')
rgba_path = '/tmp/rgba-test/car_with_transparent_bg.png'
rgba_img.save(rgba_path)
print(f"✅ Saved RGBA image (transparent bg): {rgba_path}")
print(f"   Mode: {rgba_img.mode}, Size: {rgba_img.size}")

# ========== Method 2: Grayscale mask ==========
print("\n📝 METHOD 2: Creating Grayscale mask image...")
mask_uint8 = mask * 255
mask_img = Image.fromarray(mask_uint8, 'L')
mask_path = '/tmp/rgba-test/car_mask_grayscale.png'
mask_img.save(mask_path)
print(f"✅ Saved Grayscale mask: {mask_path}")
print(f"   Mode: {mask_img.mode}, Size: {mask_img.size}")

# ========== Verification ==========
print("\n🔍 VERIFICATION - Opening saved files and comparing:")

# Open and check RGBA file
rgba_check = Image.open(rgba_path)
rgba_array_check = np.array(rgba_check)
print(f"\nRGBA file {rgba_path}:")
print(f"  Mode: {rgba_check.mode} (should be RGBA)")
print(f"  Shape: {rgba_array_check.shape}")
print(f"  Alpha channel min: {rgba_array_check[:,:,3].min()}, max: {rgba_array_check[:,:,3].max()}")
print(f"  Alpha pixels == 255: {(rgba_array_check[:,:,3] == 255).sum()}")
print(f"  Alpha pixels == 0: {(rgba_array_check[:,:,3] == 0).sum()}")

# Open and check Grayscale file
mask_check = Image.open(mask_path)
mask_array_check = np.array(mask_check)
print(f"\nGrayscale mask file {mask_path}:")
print(f"  Mode: {mask_check.mode} (should be L for grayscale)")
print(f"  Shape: {mask_array_check.shape}")
print(f"  Values min: {mask_array_check.min()}, max: {mask_array_check.max()}")
print(f"  Pixels == 255: {(mask_array_check == 255).sum()}")
print(f"  Pixels == 0: {(mask_array_check == 0).sum()}")

# ========== Visual difference check ==========
print("\n✨ Visual Difference Check:")
print("  RGBA file: Should show WHITE CAR on TRANSPARENT background")
print("             (In Preview: white car on gray checkerboard pattern)")
print("  Mask file: Should show WHITE CAR on BLACK background")
print("             (In Preview: white car on solid black)")
print("\n  These files should look DIFFERENT when opened side-by-side!")

print("\n" + "="*70)
print("✅ Test complete! Files are ready for visual comparison.")
print("="*70)
