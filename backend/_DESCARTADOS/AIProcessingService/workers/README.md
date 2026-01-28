# 🚗 Self-Improving Vehicle Segmentation System

## Overview

This is a complete **self-improving AI system** for vehicle image segmentation (background removal). It automatically:

1. **Processes** images using a 7-stage professional pipeline
2. **Evaluates** results using **GPT-4o Vision** (FREE via GitHub Models) or CLIP
3. **Classifies** results: Excellent, Good, Needs Review, Rejected
4. **Stores** good results for future model training
5. **Retrains** models when enough data is collected

## 🆓 FREE AI Evaluation with GitHub Models

This system uses **GPT-4o Vision** via GitHub Models - completely **FREE** for GitHub users!

### Setup

```bash
# Authenticate with GitHub CLI
gh auth login

# Export token for the system
export GITHUB_TOKEN=$(gh auth token)
```

### What it evaluates:

- ✅ Vehicle completeness (no parts cut off)
- ✅ Wheels intact (not cropped)
- ✅ Shadows removed (clean background)
- ✅ Edge quality (smooth borders)
- ✅ Background removal (no artifacts)

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    SELF-IMPROVING VEHICLE SEGMENTATION                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  INPUT IMAGE                                                                │
│       │                                                                     │
│       ▼                                                                     │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │               PIPELINE V2 (7 STAGES)                                 │   │
│  ├─────────────────────────────────────────────────────────────────────┤   │
│  │  [1] YOLOv8x-seg    │ Vehicle detection + initial mask              │   │
│  │  [2] CLIPSeg        │ Shadow detection and removal                  │   │
│  │  [3] Hough Circles  │ Wheel protection (prevent cutting)            │   │
│  │  [4] SegFormer-B5   │ Semantic validation                           │   │
│  │  [5] Morphology     │ Hole filling                                  │   │
│  │  [6] Guided Filter  │ Edge refinement                               │   │
│  │  [7] Gaussian       │ Anti-aliasing                                 │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│       │                                                                     │
│       ▼                                                                     │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │               AI EVALUATOR (GPT-4o Vision via GitHub Models)         │   │
│  ├─────────────────────────────────────────────────────────────────────┤   │
│  │  • Quality Score: 0-100                                             │   │
│  │  • Checks: vehicle_complete, wheels_intact, no_shadows, clean_edges │   │
│  │  • Detailed issues and suggestions                                  │   │
│  │  • Classification: Excellent/Good/Needs Review/Rejected             │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│       │                                                                     │
│       ▼                                                                     │
│  ┌──────────────┬──────────────┬──────────────┬──────────────┐             │
│  │   EXCELLENT  │     GOOD     │ NEEDS REVIEW │   REJECTED   │             │
│  │   Score ≥90  │  Score 75-89 │  Score 60-74 │  Score <60   │             │
│  │   Auto-OK ✓  │   Auto-OK ✓  │   Manual ⚠️   │   Retry/Skip │             │
│  └──────┬───────┴──────┬───────┴──────────────┴──────────────┘             │
│         │              │                                                    │
│         ▼              ▼                                                    │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │               TRAINING DATA STORAGE (SQLite + Files)                 │   │
│  ├─────────────────────────────────────────────────────────────────────┤   │
│  │  learning_data/                                                      │   │
│  │  ├── excellent/     # High quality (ready for training)             │   │
│  │  ├── good/          # Good quality (ready for training)             │   │
│  │  ├── needs_review/  # Needs human review                            │   │
│  │  ├── rejected/      # Failed - analyze problems                     │   │
│  │  └── training_ready/# Prepared YOLO format                          │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│         │                                                                   │
│         ▼ (When 50+ samples ready)                                         │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │               MODEL FINE-TUNING                                      │   │
│  │  • Convert masks to YOLO format                                      │   │
│  │  • Fine-tune YOLOv8x-seg                                            │   │
│  │  • Update pipeline with improved model                               │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Files

| File                       | Description                                |
| -------------------------- | ------------------------------------------ |
| `pipeline_v2.py`           | 7-stage professional segmentation pipeline |
| `ai_evaluator.py`          | CLIP-based AI quality evaluator            |
| `self_improving_system.py` | Complete integrated system                 |
| `learning_system.db`       | SQLite database for tracking results       |
| `learning_data/`           | Classified results and training data       |

## Models Used (~4.5 GB total)

| Model          | Size   | Purpose                          |
| -------------- | ------ | -------------------------------- |
| YOLOv8x-seg    | 140 MB | Vehicle detection + initial mask |
| CLIPSeg        | 580 MB | Shadow detection                 |
| SegFormer-B5   | 180 MB | Semantic validation              |
| SAM2-Large     | 870 MB | High-precision mask refinement   |
| ISNet          | 175 MB | Salient object detection         |
| RealESRGAN     | 16 MB  | Upscaling (optional)             |
| GroundingDINO  | 690 MB | Text-guided detection            |
| CLIP-ViT-large | 1.7 GB | AI quality evaluation            |

## Usage

### Basic Usage

```python
from self_improving_system import SelfImprovingSystem

# Initialize system (loads all models)
system = SelfImprovingSystem(use_vision_llm=False)

# Process single image
result, record = system.process_image(Path("car.jpg"))

# Process directory
results = system.process_directory(Path("input/"))

# Check stats
report = system.get_improvement_report()
```

### Command Line

```bash
# Activate virtual environment
source venv/bin/activate

# Run system on all images in input/
python3 self_improving_system.py

# Just run pipeline (without learning)
python3 pipeline_v2.py

# Just run evaluator
python3 ai_evaluator.py
```

## Classification Thresholds

| Score | Classification  | Action                                          |
| ----- | --------------- | ----------------------------------------------- |
| ≥90   | 🌟 Excellent    | Auto-approve, use for training                  |
| 75-89 | ✅ Good         | Auto-approve, use for training                  |
| 60-74 | ⚠️ Needs Review | Save for human review                           |
| <60   | ❌ Rejected     | Analyze problems, retry with different settings |

## AI Evaluation Metrics

The CLIP evaluator checks:

1. **Vehicle Complete** - Is the entire vehicle visible?
2. **Wheels Intact** - Are all wheels fully included?
3. **No Shadows** - Are shadows properly removed?
4. **Clean Edges** - Are edges smooth without artifacts?

## Self-Improvement Loop

```
1. Process Image → Pipeline V2
         ↓
2. AI Evaluation → CLIP Score + Classification
         ↓
3. Store Results → SQLite + Files
         ↓
4. Check Threshold → 50+ approved samples?
         ↓ Yes
5. Prepare Training Data → YOLO format
         ↓
6. Fine-tune Model → YOLOv8 training
         ↓
7. Deploy Updated Model → Pipeline uses new model
         ↓
8. Repeat → Better results each cycle
```

## Results (Current Run)

```
Total Processed: 8
🌟 Excellent: 7 (87.5%)
✅ Good: 1 (12.5%)
⚠️ Needs Review: 0
❌ Rejected: 0

✅ Success Rate: 100%
📚 Training Ready: 8 samples
```

## Directory Structure

```
workers/
├── models/              # Downloaded AI models (~2.8 GB)
│   ├── yolov8x-seg.pt
│   ├── sam2_hiera_large.pt
│   ├── CLIPSeg/
│   ├── SegFormer/
│   └── ...
├── input/               # Input images
├── output_v2/           # Pipeline output (for testing)
├── learning_data/       # Classified results
│   ├── excellent/       # Score ≥90 (training ready)
│   ├── good/           # Score 75-89 (training ready)
│   ├── needs_review/   # Score 60-74 (needs human)
│   ├── rejected/       # Score <60 (problems)
│   └── training_ready/ # YOLO format for training
├── pipeline_v2.py       # Segmentation pipeline
├── ai_evaluator.py      # AI quality evaluator
├── self_improving_system.py  # Complete system
├── learning_system.db   # SQLite database
└── README.md           # This file
```

## Integration with AIProcessingService

This system can be integrated into the .NET AIProcessingService:

```csharp
// In AIProcessingService.Api/Controllers/ProcessingController.cs

[HttpPost("process")]
public async Task<IActionResult> ProcessImage(IFormFile image)
{
    // 1. Save uploaded image to temp path
    var tempPath = Path.GetTempFileName() + ".jpg";
    await using (var stream = System.IO.File.Create(tempPath))
    {
        await image.CopyToAsync(stream);
    }

    // 2. Call Python pipeline via subprocess
    var startInfo = new ProcessStartInfo
    {
        FileName = "python3",
        Arguments = $"workers/self_improving_system.py --image {tempPath}",
        WorkingDirectory = "/path/to/AIProcessingService",
        RedirectStandardOutput = true,
        UseShellExecute = false
    };

    using var process = Process.Start(startInfo);
    var output = await process.StandardOutput.ReadToEndAsync();

    // 3. Return processed image and score
    var result = JsonSerializer.Deserialize<ProcessingResult>(output);
    return Ok(result);
}
```

## Future Improvements

1. **Vision LLM Integration** - Use GPT-4V or Claude for detailed evaluation
2. **Automatic Retraining** - Trigger fine-tuning when threshold reached
3. **A/B Testing** - Compare old vs new models automatically
4. **Problem Analysis** - Identify common failure patterns
5. **Continuous Deployment** - Auto-deploy improved models

## Requirements

```
Python >= 3.11
torch >= 2.0
transformers >= 4.35
ultralytics >= 8.0
opencv-python >= 4.8
huggingface_hub
```

## Author

Gregory Moreno - OKLA Marketplace  
January 2026
