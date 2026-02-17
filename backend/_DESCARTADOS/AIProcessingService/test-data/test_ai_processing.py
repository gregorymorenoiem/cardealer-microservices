#!/usr/bin/env python3
"""
Test script for AIProcessingService using local images
Uploads images via MediaService and processes them with SAM2
"""

import os
import json
import requests
import time
from pathlib import Path

# Configuration
MEDIA_SERVICE_URL = "http://localhost:15020"
AI_PROCESSING_URL = "http://localhost:5070"
JWT_TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ0ZXN0LXVzZXItMDAxIiwibmFtZSI6IlRlc3QgVXNlciIsImVtYWlsIjoidGVzdEBva2xhLmNvbS5kbyIsInJvbGUiOiJBZG1pbiIsImlhdCI6MTc2OTQzMjY1NSwiZXhwIjoxNzY5NTE5MDU1LCJpc3MiOiJPS0xBIiwiYXVkIjoiT0tMQS1Vc2VycyJ9.jrNyaXr0fJThSGOKg3r0vBBu7VKZ5NFXNM7Wfyh_-zU"

DATASETS_PATH = Path(__file__).parent / "datasets"

def upload_image(image_path: str) -> dict:
    """Upload image to MediaService and return response"""
    print(f"\n📤 Uploading: {image_path}")
    
    with open(image_path, 'rb') as f:
        files = {'file': (os.path.basename(image_path), f, 'image/jpeg')}
        data = {'folder': 'ai-test-vehicles'}
        
        response = requests.post(
            f"{MEDIA_SERVICE_URL}/api/media/upload/image",
            files=files,
            data=data
        )
    
    if response.status_code == 200:
        result = response.json()
        print(f"   ✅ Uploaded: {result.get('publicId', 'N/A')}")
        print(f"   📎 Size: {result.get('size', 0) / 1024:.1f} KB")
        return result
    else:
        print(f"   ❌ Upload failed: {response.status_code} - {response.text}")
        return None


def process_image(image_url: str, vehicle_id: str, processing_type: str = "FullPipeline") -> dict:
    """Submit image for AI processing"""
    print(f"\n🤖 Processing: {processing_type}")
    print(f"   Vehicle ID: {vehicle_id}")
    
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {JWT_TOKEN}"
    }
    
    payload = {
        "VehicleId": vehicle_id,
        "ImageUrl": image_url,
        "Type": processing_type
    }
    
    response = requests.post(
        f"{AI_PROCESSING_URL}/api/aiprocessing/process",
        headers=headers,
        json=payload
    )
    
    if response.status_code == 200:
        result = response.json()
        print(f"   ✅ Job queued: {result.get('jobId', 'N/A')}")
        print(f"   ⏱️  Estimated: {result.get('estimatedSeconds', 30)}s")
        return result
    else:
        print(f"   ❌ Processing failed: {response.status_code} - {response.text}")
        return None


def check_job_status(job_id: str) -> dict:
    """Check the status of a processing job"""
    headers = {"Authorization": f"Bearer {JWT_TOKEN}"}
    
    response = requests.get(
        f"{AI_PROCESSING_URL}/api/aiprocessing/status/{job_id}",
        headers=headers
    )
    
    if response.status_code == 200:
        return response.json()
    return {"status": "unknown", "error": response.text}


def test_single_image(image_name: str):
    """Test a single image through the full pipeline"""
    image_path = DATASETS_PATH / image_name
    
    if not image_path.exists():
        print(f"❌ Image not found: {image_path}")
        return
    
    print(f"\n{'='*60}")
    print(f"🚗 TESTING: {image_name}")
    print(f"{'='*60}")
    
    # Step 1: Upload to MediaService
    upload_result = upload_image(str(image_path))
    if not upload_result:
        return
    
    image_url = upload_result.get('url')
    vehicle_id = f"test-{image_name.replace('.jpg', '').replace('_', '-')}"
    
    # Step 2: Submit for AI processing
    job_result = process_image(image_url, vehicle_id, "FullPipeline")
    if not job_result:
        return
    
    job_id = job_result.get('jobId')
    
    # Step 3: Wait and check status
    print(f"\n⏳ Waiting for processing...")
    time.sleep(5)  # Give it a few seconds to process
    
    status = check_job_status(job_id)
    print(f"   Status: {status.get('status', 'unknown')}")
    
    return {
        "image": image_name,
        "job_id": job_id,
        "status": status
    }


def test_all_images():
    """Test all images in the datasets folder"""
    print("\n" + "="*60)
    print("🧪 AI PROCESSING SERVICE - BATCH TEST")
    print("="*60)
    
    images = list(DATASETS_PATH.glob("*.jpg"))
    print(f"\n📁 Found {len(images)} images in datasets/")
    
    results = []
    
    for i, image_path in enumerate(images[:3], 1):  # Test first 3 for now
        print(f"\n[{i}/{min(3, len(images))}]")
        result = test_single_image(image_path.name)
        if result:
            results.append(result)
    
    print("\n" + "="*60)
    print("📊 SUMMARY")
    print("="*60)
    
    for r in results:
        print(f"  • {r['image']}: {r['status'].get('status', 'unknown')}")
    
    return results


if __name__ == "__main__":
    import sys
    
    if len(sys.argv) > 1:
        # Test specific image
        test_single_image(sys.argv[1])
    else:
        # Test batch
        test_all_images()
