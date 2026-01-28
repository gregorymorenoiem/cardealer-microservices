#!/usr/bin/env python3
"""
ISNet (Dichotomous Image Segmentation) Model Implementation

Based on: https://github.com/xuebinqin/DIS
Paper: Highly Accurate Dichotomous Image Segmentation (ECCV 2022)

This module provides high-precision edge segmentation for vehicle cutouts.
"""

import torch
import torch.nn as nn
import torch.nn.functional as F
from typing import List, Tuple


# =============================================================================
# BUILDING BLOCKS
# =============================================================================

class REBNCONV(nn.Module):
    """Residual conv block with batch norm and ReLU"""
    
    def __init__(self, in_ch: int = 3, out_ch: int = 3, dirate: int = 1):
        super(REBNCONV, self).__init__()
        
        self.conv_s1 = nn.Conv2d(
            in_ch, out_ch, 3,
            padding=1 * dirate,
            dilation=1 * dirate
        )
        self.bn_s1 = nn.BatchNorm2d(out_ch)
        self.relu_s1 = nn.ReLU(inplace=True)
    
    def forward(self, x):
        return self.relu_s1(self.bn_s1(self.conv_s1(x)))


def _upsample_like(src: torch.Tensor, tar: torch.Tensor) -> torch.Tensor:
    """Upsample src to match tar size"""
    return F.interpolate(src, size=tar.shape[2:], mode='bilinear', align_corners=False)


# =============================================================================
# RSU BLOCKS (Residual U-blocks)
# =============================================================================

class RSU7(nn.Module):
    """RSU-7 block with 7 encoder levels"""
    
    def __init__(self, in_ch: int = 3, mid_ch: int = 12, out_ch: int = 3):
        super(RSU7, self).__init__()
        
        self.rebnconvin = REBNCONV(in_ch, out_ch, dirate=1)
        
        self.rebnconv1 = REBNCONV(out_ch, mid_ch, dirate=1)
        self.pool1 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv2 = REBNCONV(mid_ch, mid_ch, dirate=1)
        self.pool2 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv3 = REBNCONV(mid_ch, mid_ch, dirate=1)
        self.pool3 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv4 = REBNCONV(mid_ch, mid_ch, dirate=1)
        self.pool4 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv5 = REBNCONV(mid_ch, mid_ch, dirate=1)
        self.pool5 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv6 = REBNCONV(mid_ch, mid_ch, dirate=1)
        
        self.rebnconv7 = REBNCONV(mid_ch, mid_ch, dirate=2)
        
        self.rebnconv6d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv5d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv4d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv3d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv2d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv1d = REBNCONV(mid_ch * 2, out_ch, dirate=1)
    
    def forward(self, x):
        hx = x
        hxin = self.rebnconvin(hx)
        
        hx1 = self.rebnconv1(hxin)
        hx = self.pool1(hx1)
        
        hx2 = self.rebnconv2(hx)
        hx = self.pool2(hx2)
        
        hx3 = self.rebnconv3(hx)
        hx = self.pool3(hx3)
        
        hx4 = self.rebnconv4(hx)
        hx = self.pool4(hx4)
        
        hx5 = self.rebnconv5(hx)
        hx = self.pool5(hx5)
        
        hx6 = self.rebnconv6(hx)
        
        hx7 = self.rebnconv7(hx6)
        
        hx6d = self.rebnconv6d(torch.cat((hx7, hx6), 1))
        hx6dup = _upsample_like(hx6d, hx5)
        
        hx5d = self.rebnconv5d(torch.cat((hx6dup, hx5), 1))
        hx5dup = _upsample_like(hx5d, hx4)
        
        hx4d = self.rebnconv4d(torch.cat((hx5dup, hx4), 1))
        hx4dup = _upsample_like(hx4d, hx3)
        
        hx3d = self.rebnconv3d(torch.cat((hx4dup, hx3), 1))
        hx3dup = _upsample_like(hx3d, hx2)
        
        hx2d = self.rebnconv2d(torch.cat((hx3dup, hx2), 1))
        hx2dup = _upsample_like(hx2d, hx1)
        
        hx1d = self.rebnconv1d(torch.cat((hx2dup, hx1), 1))
        
        return hx1d + hxin


class RSU6(nn.Module):
    """RSU-6 block"""
    
    def __init__(self, in_ch: int = 3, mid_ch: int = 12, out_ch: int = 3):
        super(RSU6, self).__init__()
        
        self.rebnconvin = REBNCONV(in_ch, out_ch, dirate=1)
        
        self.rebnconv1 = REBNCONV(out_ch, mid_ch, dirate=1)
        self.pool1 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv2 = REBNCONV(mid_ch, mid_ch, dirate=1)
        self.pool2 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv3 = REBNCONV(mid_ch, mid_ch, dirate=1)
        self.pool3 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv4 = REBNCONV(mid_ch, mid_ch, dirate=1)
        self.pool4 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv5 = REBNCONV(mid_ch, mid_ch, dirate=1)
        
        self.rebnconv6 = REBNCONV(mid_ch, mid_ch, dirate=2)
        
        self.rebnconv5d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv4d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv3d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv2d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv1d = REBNCONV(mid_ch * 2, out_ch, dirate=1)
    
    def forward(self, x):
        hx = x
        hxin = self.rebnconvin(hx)
        
        hx1 = self.rebnconv1(hxin)
        hx = self.pool1(hx1)
        
        hx2 = self.rebnconv2(hx)
        hx = self.pool2(hx2)
        
        hx3 = self.rebnconv3(hx)
        hx = self.pool3(hx3)
        
        hx4 = self.rebnconv4(hx)
        hx = self.pool4(hx4)
        
        hx5 = self.rebnconv5(hx)
        
        hx6 = self.rebnconv6(hx5)
        
        hx5d = self.rebnconv5d(torch.cat((hx6, hx5), 1))
        hx5dup = _upsample_like(hx5d, hx4)
        
        hx4d = self.rebnconv4d(torch.cat((hx5dup, hx4), 1))
        hx4dup = _upsample_like(hx4d, hx3)
        
        hx3d = self.rebnconv3d(torch.cat((hx4dup, hx3), 1))
        hx3dup = _upsample_like(hx3d, hx2)
        
        hx2d = self.rebnconv2d(torch.cat((hx3dup, hx2), 1))
        hx2dup = _upsample_like(hx2d, hx1)
        
        hx1d = self.rebnconv1d(torch.cat((hx2dup, hx1), 1))
        
        return hx1d + hxin


class RSU5(nn.Module):
    """RSU-5 block"""
    
    def __init__(self, in_ch: int = 3, mid_ch: int = 12, out_ch: int = 3):
        super(RSU5, self).__init__()
        
        self.rebnconvin = REBNCONV(in_ch, out_ch, dirate=1)
        
        self.rebnconv1 = REBNCONV(out_ch, mid_ch, dirate=1)
        self.pool1 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv2 = REBNCONV(mid_ch, mid_ch, dirate=1)
        self.pool2 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv3 = REBNCONV(mid_ch, mid_ch, dirate=1)
        self.pool3 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv4 = REBNCONV(mid_ch, mid_ch, dirate=1)
        
        self.rebnconv5 = REBNCONV(mid_ch, mid_ch, dirate=2)
        
        self.rebnconv4d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv3d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv2d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv1d = REBNCONV(mid_ch * 2, out_ch, dirate=1)
    
    def forward(self, x):
        hx = x
        hxin = self.rebnconvin(hx)
        
        hx1 = self.rebnconv1(hxin)
        hx = self.pool1(hx1)
        
        hx2 = self.rebnconv2(hx)
        hx = self.pool2(hx2)
        
        hx3 = self.rebnconv3(hx)
        hx = self.pool3(hx3)
        
        hx4 = self.rebnconv4(hx)
        
        hx5 = self.rebnconv5(hx4)
        
        hx4d = self.rebnconv4d(torch.cat((hx5, hx4), 1))
        hx4dup = _upsample_like(hx4d, hx3)
        
        hx3d = self.rebnconv3d(torch.cat((hx4dup, hx3), 1))
        hx3dup = _upsample_like(hx3d, hx2)
        
        hx2d = self.rebnconv2d(torch.cat((hx3dup, hx2), 1))
        hx2dup = _upsample_like(hx2d, hx1)
        
        hx1d = self.rebnconv1d(torch.cat((hx2dup, hx1), 1))
        
        return hx1d + hxin


class RSU4(nn.Module):
    """RSU-4 block"""
    
    def __init__(self, in_ch: int = 3, mid_ch: int = 12, out_ch: int = 3):
        super(RSU4, self).__init__()
        
        self.rebnconvin = REBNCONV(in_ch, out_ch, dirate=1)
        
        self.rebnconv1 = REBNCONV(out_ch, mid_ch, dirate=1)
        self.pool1 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv2 = REBNCONV(mid_ch, mid_ch, dirate=1)
        self.pool2 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.rebnconv3 = REBNCONV(mid_ch, mid_ch, dirate=1)
        
        self.rebnconv4 = REBNCONV(mid_ch, mid_ch, dirate=2)
        
        self.rebnconv3d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv2d = REBNCONV(mid_ch * 2, mid_ch, dirate=1)
        self.rebnconv1d = REBNCONV(mid_ch * 2, out_ch, dirate=1)
    
    def forward(self, x):
        hx = x
        hxin = self.rebnconvin(hx)
        
        hx1 = self.rebnconv1(hxin)
        hx = self.pool1(hx1)
        
        hx2 = self.rebnconv2(hx)
        hx = self.pool2(hx2)
        
        hx3 = self.rebnconv3(hx)
        
        hx4 = self.rebnconv4(hx3)
        
        hx3d = self.rebnconv3d(torch.cat((hx4, hx3), 1))
        hx3dup = _upsample_like(hx3d, hx2)
        
        hx2d = self.rebnconv2d(torch.cat((hx3dup, hx2), 1))
        hx2dup = _upsample_like(hx2d, hx1)
        
        hx1d = self.rebnconv1d(torch.cat((hx2dup, hx1), 1))
        
        return hx1d + hxin


class RSU4F(nn.Module):
    """RSU-4F block (dilated, no pooling)"""
    
    def __init__(self, in_ch: int = 3, mid_ch: int = 12, out_ch: int = 3):
        super(RSU4F, self).__init__()
        
        self.rebnconvin = REBNCONV(in_ch, out_ch, dirate=1)
        
        self.rebnconv1 = REBNCONV(out_ch, mid_ch, dirate=1)
        self.rebnconv2 = REBNCONV(mid_ch, mid_ch, dirate=2)
        self.rebnconv3 = REBNCONV(mid_ch, mid_ch, dirate=4)
        
        self.rebnconv4 = REBNCONV(mid_ch, mid_ch, dirate=8)
        
        self.rebnconv3d = REBNCONV(mid_ch * 2, mid_ch, dirate=4)
        self.rebnconv2d = REBNCONV(mid_ch * 2, mid_ch, dirate=2)
        self.rebnconv1d = REBNCONV(mid_ch * 2, out_ch, dirate=1)
    
    def forward(self, x):
        hx = x
        hxin = self.rebnconvin(hx)
        
        hx1 = self.rebnconv1(hxin)
        hx2 = self.rebnconv2(hx1)
        hx3 = self.rebnconv3(hx2)
        
        hx4 = self.rebnconv4(hx3)
        
        hx3d = self.rebnconv3d(torch.cat((hx4, hx3), 1))
        hx2d = self.rebnconv2d(torch.cat((hx3d, hx2), 1))
        hx1d = self.rebnconv1d(torch.cat((hx2d, hx1), 1))
        
        return hx1d + hxin


# =============================================================================
# ISNet-DIS MAIN MODEL
# =============================================================================

class ISNetDIS(nn.Module):
    """
    ISNet for Dichotomous Image Segmentation
    
    Produces high-precision binary masks with clean edges.
    Ideal for vehicle cutouts where edge quality is critical.
    """
    
    def __init__(self, in_ch: int = 3, out_ch: int = 1):
        super(ISNetDIS, self).__init__()
        
        # Encoder
        self.stage1 = RSU7(in_ch, 32, 64)
        self.pool12 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.stage2 = RSU6(64, 32, 128)
        self.pool23 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.stage3 = RSU5(128, 64, 256)
        self.pool34 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.stage4 = RSU4(256, 128, 512)
        self.pool45 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.stage5 = RSU4F(512, 256, 512)
        self.pool56 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.stage6 = RSU4F(512, 256, 512)
        
        # Decoder
        self.stage5d = RSU4F(1024, 256, 512)
        self.stage4d = RSU4(1024, 128, 256)
        self.stage3d = RSU5(512, 64, 128)
        self.stage2d = RSU6(256, 32, 64)
        self.stage1d = RSU7(128, 16, 64)
        
        # Side outputs
        self.side1 = nn.Conv2d(64, out_ch, 3, padding=1)
        self.side2 = nn.Conv2d(64, out_ch, 3, padding=1)
        self.side3 = nn.Conv2d(128, out_ch, 3, padding=1)
        self.side4 = nn.Conv2d(256, out_ch, 3, padding=1)
        self.side5 = nn.Conv2d(512, out_ch, 3, padding=1)
        self.side6 = nn.Conv2d(512, out_ch, 3, padding=1)
    
    def forward(self, x: torch.Tensor) -> List[torch.Tensor]:
        hx = x
        
        # Encoder
        hx1 = self.stage1(hx)
        hx = self.pool12(hx1)
        
        hx2 = self.stage2(hx)
        hx = self.pool23(hx2)
        
        hx3 = self.stage3(hx)
        hx = self.pool34(hx3)
        
        hx4 = self.stage4(hx)
        hx = self.pool45(hx4)
        
        hx5 = self.stage5(hx)
        hx = self.pool56(hx5)
        
        hx6 = self.stage6(hx)
        hx6up = _upsample_like(hx6, hx5)
        
        # Decoder
        hx5d = self.stage5d(torch.cat((hx6up, hx5), 1))
        hx5dup = _upsample_like(hx5d, hx4)
        
        hx4d = self.stage4d(torch.cat((hx5dup, hx4), 1))
        hx4dup = _upsample_like(hx4d, hx3)
        
        hx3d = self.stage3d(torch.cat((hx4dup, hx3), 1))
        hx3dup = _upsample_like(hx3d, hx2)
        
        hx2d = self.stage2d(torch.cat((hx3dup, hx2), 1))
        hx2dup = _upsample_like(hx2d, hx1)
        
        hx1d = self.stage1d(torch.cat((hx2dup, hx1), 1))
        
        # Side outputs (multi-scale supervision)
        d1 = self.side1(hx1d)
        
        d2 = self.side2(hx2d)
        d2 = _upsample_like(d2, d1)
        
        d3 = self.side3(hx3d)
        d3 = _upsample_like(d3, d1)
        
        d4 = self.side4(hx4d)
        d4 = _upsample_like(d4, d1)
        
        d5 = self.side5(hx5d)
        d5 = _upsample_like(d5, d1)
        
        d6 = self.side6(hx6)
        d6 = _upsample_like(d6, d1)
        
        # Return list of side outputs (for deep supervision during training)
        # During inference, use d1 (finest scale)
        return [
            torch.sigmoid(d1),
            torch.sigmoid(d2),
            torch.sigmoid(d3),
            torch.sigmoid(d4),
            torch.sigmoid(d5),
            torch.sigmoid(d6)
        ]


class ISNetGTEncoder(nn.Module):
    """
    Ground Truth Encoder for ISNet
    Used during training to encode GT masks for intermediate supervision
    """
    
    def __init__(self, in_ch: int = 1, out_ch: int = 1):
        super(ISNetGTEncoder, self).__init__()
        
        self.stage1 = RSU7(in_ch, 16, 64)
        self.pool12 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.stage2 = RSU6(64, 16, 64)
        self.pool23 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.stage3 = RSU5(64, 16, 64)
        self.pool34 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.stage4 = RSU4(64, 16, 64)
        self.pool45 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.stage5 = RSU4F(64, 16, 64)
        self.pool56 = nn.MaxPool2d(2, stride=2, ceil_mode=True)
        
        self.stage6 = RSU4F(64, 16, 64)
        
        self.side1 = nn.Conv2d(64, out_ch, 3, padding=1)
        self.side2 = nn.Conv2d(64, out_ch, 3, padding=1)
        self.side3 = nn.Conv2d(64, out_ch, 3, padding=1)
        self.side4 = nn.Conv2d(64, out_ch, 3, padding=1)
        self.side5 = nn.Conv2d(64, out_ch, 3, padding=1)
        self.side6 = nn.Conv2d(64, out_ch, 3, padding=1)
    
    def forward(self, x: torch.Tensor) -> List[torch.Tensor]:
        hx = x
        
        hx1 = self.stage1(hx)
        hx = self.pool12(hx1)
        
        hx2 = self.stage2(hx)
        hx = self.pool23(hx2)
        
        hx3 = self.stage3(hx)
        hx = self.pool34(hx3)
        
        hx4 = self.stage4(hx)
        hx = self.pool45(hx4)
        
        hx5 = self.stage5(hx)
        hx = self.pool56(hx5)
        
        hx6 = self.stage6(hx)
        
        d1 = self.side1(hx1)
        d1 = _upsample_like(d1, x)
        
        d2 = self.side2(hx2)
        d2 = _upsample_like(d2, x)
        
        d3 = self.side3(hx3)
        d3 = _upsample_like(d3, x)
        
        d4 = self.side4(hx4)
        d4 = _upsample_like(d4, x)
        
        d5 = self.side5(hx5)
        d5 = _upsample_like(d5, x)
        
        d6 = self.side6(hx6)
        d6 = _upsample_like(d6, x)
        
        return [d1, d2, d3, d4, d5, d6]


# =============================================================================
# UTILITY FUNCTIONS
# =============================================================================

def load_isnet(model_path: str, device: str = "cuda") -> ISNetDIS:
    """
    Load pretrained ISNet model
    
    Args:
        model_path: Path to the .pth weights file
        device: Target device (cuda/cpu)
    
    Returns:
        Loaded ISNetDIS model
    """
    model = ISNetDIS()
    
    # Load weights
    state_dict = torch.load(model_path, map_location=device)
    
    # Handle potential key mismatches
    if "model" in state_dict:
        state_dict = state_dict["model"]
    
    model.load_state_dict(state_dict)
    model.to(device)
    model.eval()
    
    return model


def predict_isnet(
    model: ISNetDIS,
    image: torch.Tensor,
    input_size: Tuple[int, int] = (1024, 1024)
) -> torch.Tensor:
    """
    Run ISNet prediction on an image
    
    Args:
        model: Loaded ISNetDIS model
        image: Input tensor [B, C, H, W]
        input_size: Size to resize input to
    
    Returns:
        Binary mask tensor [B, 1, H, W]
    """
    original_size = image.shape[2:]
    
    # Resize to model input size
    if image.shape[2:] != input_size:
        image = F.interpolate(image, size=input_size, mode='bilinear', align_corners=False)
    
    # Normalize
    mean = torch.tensor([0.485, 0.456, 0.406]).view(1, 3, 1, 1).to(image.device)
    std = torch.tensor([0.229, 0.224, 0.225]).view(1, 3, 1, 1).to(image.device)
    image = (image - mean) / std
    
    # Inference
    with torch.no_grad():
        outputs = model(image)
        mask = outputs[0]  # Use finest scale output
    
    # Resize back to original size
    if mask.shape[2:] != original_size:
        mask = F.interpolate(mask, size=original_size, mode='bilinear', align_corners=False)
    
    return mask


# =============================================================================
# STANDALONE INFERENCE
# =============================================================================

if __name__ == "__main__":
    import argparse
    from PIL import Image
    import numpy as np
    
    parser = argparse.ArgumentParser(description="ISNet Inference")
    parser.add_argument("--image", "-i", required=True, help="Input image path")
    parser.add_argument("--model", "-m", required=True, help="Model weights path")
    parser.add_argument("--output", "-o", required=True, help="Output mask path")
    parser.add_argument("--device", "-d", default="cuda")
    
    args = parser.parse_args()
    
    # Load model
    print(f"Loading model from {args.model}...")
    model = load_isnet(args.model, args.device)
    
    # Load image
    print(f"Processing {args.image}...")
    image = Image.open(args.image).convert("RGB")
    image_np = np.array(image)
    
    # Convert to tensor
    image_tensor = torch.from_numpy(image_np).float().permute(2, 0, 1).unsqueeze(0) / 255.0
    image_tensor = image_tensor.to(args.device)
    
    # Predict
    mask = predict_isnet(model, image_tensor)
    
    # Save
    mask_np = (mask.squeeze().cpu().numpy() * 255).astype(np.uint8)
    Image.fromarray(mask_np).save(args.output)
    print(f"Saved mask to {args.output}")
