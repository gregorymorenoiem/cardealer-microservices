#!/usr/bin/env python3
"""
OKLA LLM Training Script — Automated Fine-Tuning Pipeline v2.0 (Dual-Mode)
===========================================================================

Wraps the Colab notebook logic into a reproducible CLI script.
Supports: QLoRA fine-tuning → Merge → GGUF export → Evaluation.

Dual-Mode: SingleVehicle (SV) + DealerInventory (DI)
Context Window: 8192 tokens | QLoRA r=64, alpha=128

Usage:
    python train.py --dataset /datasets/train.jsonl --output /output
    python train.py --dry-run  # Validate environment only
    python train.py --help
"""

import argparse
import hashlib
import json
import logging
import os
import sys
from datetime import datetime

logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
logger = logging.getLogger("okla-trainer")


def get_config():
    """Build training config from environment variables with defaults."""
    # Auto-detect GPU for optimal batch/precision
    gpu_name = ""
    try:
        import torch
        if torch.cuda.is_available():
            gpu_name = torch.cuda.get_device_name(0).upper()
    except Exception:
        pass

    is_a100 = "A100" in gpu_name
    default_batch = "8" if is_a100 else "2"
    default_grad_accum = "4" if is_a100 else "8"

    return {
        "base_model": os.getenv("BASE_MODEL", "unsloth/Meta-Llama-3.1-8B-Instruct-bnb-4bit"),
        "lora_r": int(os.getenv("LORA_R", "64")),
        "lora_alpha": int(os.getenv("LORA_ALPHA", "128")),
        "lora_dropout": float(os.getenv("LORA_DROPOUT", "0.0")),
        "learning_rate": float(os.getenv("LEARNING_RATE", "2e-4")),
        "num_epochs": int(os.getenv("NUM_EPOCHS", "3")),
        "batch_size": int(os.getenv("BATCH_SIZE", default_batch)),
        "gradient_accumulation": int(os.getenv("GRADIENT_ACCUMULATION", default_grad_accum)),
        "warmup_steps": int(os.getenv("WARMUP_STEPS", "100")),
        "weight_decay": float(os.getenv("WEIGHT_DECAY", "0.01")),
        "max_seq_length": int(os.getenv("MAX_SEQ_LENGTH", "8192")),
        "seed": int(os.getenv("SEED", "42")),
        "quantization_method": os.getenv("QUANTIZATION_METHOD", "q4_k_m"),
        "gpu_detected": gpu_name or "none",
    }


def validate_environment():
    """Check that all required packages are installed."""
    logger.info("🔍 Validating training environment...")
    required = ["torch", "transformers", "peft", "trl", "datasets", "accelerate"]
    missing = []
    for pkg in required:
        try:
            __import__(pkg)
            logger.info(f"  ✅ {pkg}")
        except ImportError:
            missing.append(pkg)
            logger.error(f"  ❌ {pkg} — NOT INSTALLED")

    if missing:
        logger.error(f"Missing packages: {', '.join(missing)}")
        sys.exit(1)

    # Check GPU
    try:
        import torch
        if torch.cuda.is_available():
            gpu_name = torch.cuda.get_device_name(0)
            gpu_mem = torch.cuda.get_device_properties(0).total_mem / 1024**3
            logger.info(f"  🎮 GPU: {gpu_name} ({gpu_mem:.1f} GB)")
        else:
            logger.warning("  ⚠️ No GPU detected — training will be extremely slow")
    except Exception:
        logger.warning("  ⚠️ Could not detect GPU")

    logger.info("✅ Environment validated\n")


def compute_file_hash(filepath):
    """Compute SHA256 hash of a file."""
    sha256 = hashlib.sha256()
    with open(filepath, "rb") as f:
        for chunk in iter(lambda: f.read(8192 * 1024), b""):
            sha256.update(chunk)
    return sha256.hexdigest()


def train(dataset_path, output_dir, config):
    """Execute the full training pipeline."""
    logger.info("=" * 60)
    logger.info("OKLA LLM Fine-Tuning Pipeline")
    logger.info("=" * 60)
    logger.info(f"Dataset: {dataset_path}")
    logger.info(f"Output:  {output_dir}")
    logger.info(f"Config:  {json.dumps(config, indent=2)}")
    logger.info("=" * 60)

    os.makedirs(output_dir, exist_ok=True)
    start_time = datetime.now()

    # ── Stage 1: Load Base Model with QLoRA ─────────────────────────
    logger.info("\n📦 Stage 1/5: Loading base model with QLoRA...")
    from unsloth import FastLanguageModel

    model, tokenizer = FastLanguageModel.from_pretrained(
        model_name=config["base_model"],
        max_seq_length=config["max_seq_length"],
        load_in_4bit=True,
    )

    model = FastLanguageModel.get_peft_model(
        model,
        r=config["lora_r"],
        lora_alpha=config["lora_alpha"],
        lora_dropout=config["lora_dropout"],
        target_modules=["q_proj", "k_proj", "v_proj", "o_proj",
                        "gate_proj", "up_proj", "down_proj"],
    )
    logger.info("✅ Model loaded with QLoRA adapters")

    # ── Stage 2: Load Dataset ───────────────────────────────────────
    logger.info("\n📊 Stage 2/5: Loading dataset...")
    from datasets import load_dataset

    dataset = load_dataset("json", data_files=dataset_path, split="train")
    logger.info(f"  Loaded {len(dataset)} training examples")

    # ── Stage 3: Train ──────────────────────────────────────────────
    logger.info("\n🏋️ Stage 3/5: Training...")
    from trl import SFTTrainer
    from transformers import TrainingArguments

    training_args = TrainingArguments(
        output_dir=os.path.join(output_dir, "checkpoints"),
        per_device_train_batch_size=config["batch_size"],
        gradient_accumulation_steps=config["gradient_accumulation"],
        num_train_epochs=config["num_epochs"],
        learning_rate=config["learning_rate"],
        warmup_steps=config["warmup_steps"],
        weight_decay=config["weight_decay"],
        seed=config["seed"],
        logging_steps=10,
        save_strategy="epoch",
        fp16=True,
        report_to="none",
    )

    trainer = SFTTrainer(
        model=model,
        tokenizer=tokenizer,
        train_dataset=dataset,
        args=training_args,
        dataset_text_field="text",
        max_seq_length=config["max_seq_length"],
    )

    result = trainer.train()
    logger.info(f"✅ Training completed: loss={result.training_loss:.4f}")

    # ── Stage 4: Export to GGUF ─────────────────────────────────────
    logger.info(f"\n💾 Stage 4/5: Exporting to GGUF ({config['quantization_method']})...")
    gguf_path = os.path.join(output_dir, f"okla-llama3-8b-{config['quantization_method']}.gguf")

    model.save_pretrained_gguf(
        output_dir,
        tokenizer,
        quantization_method=config["quantization_method"],
    )
    logger.info(f"✅ GGUF exported: {gguf_path}")

    # ── Stage 5: Generate Checksums & Manifest ──────────────────────
    logger.info("\n📋 Stage 5/5: Generating manifest...")
    if os.path.exists(gguf_path):
        model_hash = compute_file_hash(gguf_path)
        model_size = os.path.getsize(gguf_path) / 1024**3

        # Write SHA256 companion file
        with open(gguf_path + ".sha256", "w") as f:
            f.write(f"{model_hash}  {os.path.basename(gguf_path)}\n")

        manifest = {
            "training_completed": datetime.now().isoformat(),
            "training_duration_seconds": (datetime.now() - start_time).total_seconds(),
            "base_model": config["base_model"],
            "output_file": os.path.basename(gguf_path),
            "output_size_gb": round(model_size, 2),
            "sha256": model_hash,
            "config": config,
            "training_loss": result.training_loss,
            "dataset": dataset_path,
            "dataset_examples": len(dataset),
        }

        manifest_path = os.path.join(output_dir, "training_manifest.json")
        with open(manifest_path, "w") as f:
            json.dump(manifest, f, indent=2)

        logger.info(f"  SHA256: {model_hash[:16]}...")
        logger.info(f"  Size:   {model_size:.2f} GB")
        logger.info(f"  Manifest: {manifest_path}")

    duration = (datetime.now() - start_time).total_seconds()
    logger.info(f"\n{'=' * 60}")
    logger.info(f"✅ TRAINING COMPLETE in {duration/60:.1f} minutes")
    logger.info(f"{'=' * 60}")


def main():
    parser = argparse.ArgumentParser(description="OKLA LLM Fine-Tuning Pipeline")
    parser.add_argument("--dataset", type=str, help="Path to training JSONL file")
    parser.add_argument("--output", type=str, default="/output", help="Output directory")
    parser.add_argument("--dry-run", action="store_true", help="Validate environment only")
    args = parser.parse_args()

    validate_environment()

    if args.dry_run:
        logger.info("🏁 Dry run complete — environment is ready for training")
        return

    if not args.dataset:
        parser.error("--dataset is required (or use --dry-run)")

    if not os.path.exists(args.dataset):
        logger.error(f"Dataset not found: {args.dataset}")
        sys.exit(1)

    config = get_config()
    train(args.dataset, args.output, config)


if __name__ == "__main__":
    main()
