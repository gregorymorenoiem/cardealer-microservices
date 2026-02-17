# LLM Models Directory

Place your trained GGUF model file here.

## Expected file

```
models/
└── okla-llama3-8b-q4_k_m.gguf  (~4.7 GB)
```

## How to get the model

1. Run the training notebook: `docs/chatbot-llm/FASE_3_TRAINING/okla_finetune_llama3.ipynb`
2. The notebook exports the model as GGUF Q4_K_M quantization
3. Copy the `.gguf` file to this directory

## Alternative: Download from HuggingFace

After training and uploading to HuggingFace Hub:

```bash
huggingface-cli download gregorymorenoiem/okla-llama3-8b-chatbot \
  okla-llama3-8b-chatbot.Q4_K_M.gguf \
  --local-dir .
mv okla-llama3-8b-chatbot.Q4_K_M.gguf okla-llama3-8b-q4_k_m.gguf
```

> ⚠️ This file is gitignored — do NOT commit the .gguf model (4.7 GB).
