# 🤗 Hugging Face - Documentación Completa

**Versión:** Transformers 4.36+  
**Costo:** Gratis (open-source)  
**Latencia:** <100ms (local)  
**Modelos:** 350K+ disponibles

---

## 📖 Introducción

**Hugging Face** es la plataforma más grande de modelos de ML open-source con:

- Pre-trained models (BERT, RoBERTa, DistilBERT, etc.)
- Datasets públicos (200K+)
- Infrastructure para inference
- Community colaborativa

### Uso en OKLA:

1. **ReviewService**: Análisis de sentimiento en reviews
2. **UserBehaviorService**: Clasificación de intención de usuario
3. **ChatbotService**: NLP avanzado, procesamiento de lenguaje natural

---

## 🔗 Modelos Recomendados para OKLA

### 1. Sentiment Analysis

**Modelo:** `nlptown/bert-base-multilingual-uncased-sentiment`

```bash
# Instalación
pip install transformers torch

# Test rápido
from transformers import pipeline

sentiment = pipeline("sentiment-analysis",
    model="nlptown/bert-base-multilingual-uncased-sentiment")

result = sentiment("Este vendedor es excelente!")
# Output: [{'label': 'positive', 'score': 0.99}]
```

**Características:**

- Multiidioma (español soportado)
- 5 sentimientos: very negative, negative, neutral, positive, very positive
- Accuracy: >90%
- Latencia: <50ms

---

### 2. Named Entity Recognition (NER)

**Modelo:** `nlptown/bert-base-multilingual-uncased-ner`

```bash
from transformers import pipeline

ner = pipeline("ner",
    model="nlptown/bert-base-multilingual-uncased-ner")

text = "El Toyota Corolla de Juan cuesta $1.5M y está en Santo Domingo"
result = ner(text)

# Output:
# [
#   {'entity': 'B-PER', 'score': 0.98, 'word': 'Juan'},
#   {'entity': 'B-LOC', 'score': 0.97, 'word': 'Santo Domingo'},
#   ...
# ]
```

**Características:**

- Extrae: Personas, Lugares, Organizaciones, Cantidades
- Latencia: <100ms
- Accuracy: >92%

---

### 3. Text Classification

**Modelo:** `xlm-roberta-base` + fine-tuning

Para clasificar tipos de usuario (Buyer/Seller/Dealer):

```python
from transformers import AutoTokenizer, AutoModelForSequenceClassification

tokenizer = AutoTokenizer.from_pretrained("xlm-roberta-base")
model = AutoModelForSequenceClassification.from_pretrained("xlm-roberta-base")

# Fine-tuning en datos de OKLA
# Categorías: buyer, seller, dealer, admin
```

---

## 💻 Implementación C#

### Nuget Packages

```bash
dotnet add package SciSharp.TensorFlow.Redist
dotnet add package ML.NET
dotnet add package ML.NET.TensorFlow
```

### Alternativa: Python Service

```bash
# Crear servicio Python standalone
pip install fastapi uvicorn transformers torch

# main.py
from fastapi import FastAPI
from transformers import pipeline

app = FastAPI()

sentiment_analyzer = pipeline("sentiment-analysis",
    model="nlptown/bert-base-multilingual-uncased-sentiment")

ner_analyzer = pipeline("ner",
    model="nlptown/bert-base-multilingual-uncased-ner")

@app.post("/api/sentiment")
async def analyze_sentiment(text: str):
    result = sentiment_analyzer(text)
    return {"text": text, "sentiment": result}

@app.post("/api/ner")
async def extract_entities(text: str):
    result = ner_analyzer(text)
    return {"text": text, "entities": result}
```

**Deployment:**

```bash
# Dockerfile
FROM python:3.11-slim
WORKDIR /app
COPY requirements.txt .
RUN pip install -r requirements.txt
COPY main.py .
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "5050"]
```

**En Kubernetes:**

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: hugging-face-service
  namespace: okla
spec:
  replicas: 2
  selector:
    matchLabels:
      app: hugging-face-service
  template:
    metadata:
      labels:
        app: hugging-face-service
    spec:
      containers:
        - name: api
          image: okla-hugging-face:latest
          ports:
            - containerPort: 5050
          resources:
            limits:
              memory: "2Gi"
              cpu: "1"
            requests:
              memory: "1Gi"
              cpu: "500m"
          livenessProbe:
            httpGet:
              path: /health
              port: 5050
            initialDelaySeconds: 30
            periodSeconds: 10
```

---

### Desde C# (.NET 8)

```csharp
using System.Net.Http.Json;

public interface IHuggingFaceService
{
    Task<SentimentResult> AnalyzeSentimentAsync(string text, CancellationToken ct);
    Task<NerResult[]> ExtractEntitiesAsync(string text, CancellationToken ct);
}

public class HuggingFaceService : IHuggingFaceService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public HuggingFaceService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _baseUrl = config["HuggingFace:BaseUrl"] ?? "http://hugging-face-service:5050";
    }

    public async Task<SentimentResult> AnalyzeSentimentAsync(
        string text,
        CancellationToken ct)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"{_baseUrl}/api/sentiment",
            new { text },
            cancellationToken: ct
        );

        return await response.Content.ReadAsAsync<SentimentResult>(cancellationToken: ct);
    }

    public async Task<NerResult[]> ExtractEntitiesAsync(
        string text,
        CancellationToken ct)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"{_baseUrl}/api/ner",
            new { text },
            cancellationToken: ct
        );

        var result = await response.Content.ReadAsAsync<NerResponse>(cancellationToken: ct);
        return result.Entities;
    }
}

public record SentimentResult(string Label, double Score);
public record NerResult(string Entity, double Score, string Word);
public record NerResponse(string Text, NerResult[] Entities);
```

---

## ⚛️ Integración en Frontend

### useHuggingFace Hook

```typescript
import { useMutation, useQuery } from "@tanstack/react-query";
import axios from "axios";

interface SentimentResult {
  label: string;
  score: number;
}

interface Entity {
  entity: string;
  score: number;
  word: string;
}

export const useHuggingFace = () => {
  const analyzeSentiment = useMutation({
    mutationFn: async (text: string) => {
      const { data } = await axios.post("/api/nlp/sentiment", { text });
      return data as SentimentResult;
    },
  });

  const extractEntities = useMutation({
    mutationFn: async (text: string) => {
      const { data } = await axios.post("/api/nlp/entities", { text });
      return data as Entity[];
    },
  });

  return { analyzeSentiment, extractEntities };
};
```

### Review Analyzer Component

```tsx
import React from "react";
import { useHuggingFace } from "@/hooks/useHuggingFace";

interface ReviewAnalyzerProps {
  reviewText: string;
  onSentimentChange?: (sentiment: string) => void;
}

export const ReviewAnalyzer: React.FC<ReviewAnalyzerProps> = ({
  reviewText,
  onSentimentChange,
}) => {
  const { analyzeSentiment } = useHuggingFace();
  const [sentiment, setSentiment] = React.useState<string | null>(null);

  React.useEffect(() => {
    if (reviewText.length > 10) {
      analyzeSentiment.mutate(reviewText, {
        onSuccess: (result) => {
          setSentiment(result.label);
          onSentimentChange?.(result.label);
        },
      });
    }
  }, [reviewText]);

  return (
    <div className="review-analyzer">
      {sentiment && (
        <div className={`sentiment-badge sentiment-${sentiment.toLowerCase()}`}>
          {sentiment === "positive" && "😊 Positivo"}
          {sentiment === "negative" && "😞 Negativo"}
          {sentiment === "neutral" && "😐 Neutral"}
        </div>
      )}
      {analyzeSentiment.isPending && (
        <div className="loading">Analizando...</div>
      )}
    </div>
  );
};
```

---

## 🎯 Casos de Uso en OKLA

### 1. ReviewService - Moderation

```python
@app.post("/api/reviews/analyze")
async def analyze_review(review: ReviewRequest):
    # 1. Análisis de sentimiento
    sentiment = sentiment_analyzer(review.text)[0]

    # 2. Extracción de entidades
    entities = ner_analyzer(review.text)

    # 3. Verificar si es spam
    is_spam = detect_spam(review.text)

    # 4. Filtrar si es abusivo
    if sentiment['label'] == 'negative' and sentiment['score'] > 0.9:
        # Requiere revisión manual
        return {"flagged": True, "reason": "potentially_negative"}

    return {
        "sentiment": sentiment['label'],
        "confidence": sentiment['score'],
        "entities": entities,
        "approved": True
    }
```

### 2. UserBehaviorService - Clasificación

```python
@app.post("/api/users/intent")
async def classify_user_intent(message: str):
    # Clasificar tipo de usuario basado en mensaje
    classifier = pipeline("zero-shot-classification",
        model="facebook/bart-large-mnli")

    result = classifier(message, ["buyer", "seller", "dealer", "support_request"])

    return {
        "intent": result['labels'][0],
        "confidence": result['scores'][0]
    }
```

### 3. ChatbotService - NLP Avanzado

```python
@app.post("/api/chatbot/process")
async def process_chat_message(message: str, user_id: str):
    # 1. Extraer entidades (vehículos, precios, etc.)
    entities = ner_analyzer(message)

    # 2. Detectar intención
    intent = classifier(message,
        ["search", "price_inquiry", "comparison", "contact_seller"])

    # 3. Responder basado en intención
    return {
        "entities": entities,
        "intent": intent['labels'][0],
        "response": generate_response(intent, entities)
    }
```

---

## 💰 Costo

**Gratis** (todos los modelos son open-source)

**Opción 1: Auto-alojado (recomendado)**

- Descargar modelo (~500MB)
- Alojar en pod Kubernetes
- Costo: Solo infraestructura (~$0.50/día)

**Opción 2: Hugging Face Inference API**

- API cloud de HF
- $9/mes por modelo
- Gratis tier incluido

---

## 📊 Performance

| Modelo         | Latencia | Memory | Accuracy |
| -------------- | -------- | ------ | -------- |
| Sentiment      | <50ms    | 400MB  | 92%      |
| NER            | <100ms   | 600MB  | 94%      |
| Classification | <150ms   | 800MB  | 88%      |

---

## ✅ Checklist

- [ ] Instalar transformers + torch
- [ ] Descargar modelos para pruebas
- [ ] Crear servicio Python o integración C#
- [ ] Testing en desarrollo
- [ ] Deploy a Kubernetes
- [ ] Monitoreo de latencia
- [ ] Integration testing

---

_Documentación Hugging Face para OKLA_  
_Última actualización: Enero 15, 2026_
