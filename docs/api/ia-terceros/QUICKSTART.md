# 🚀 QuickStart: Setup de APIs de IA en 30 Minutos

**Tiempo:** 30 minutos  
**Requisitos:** Cuentas en OpenAI + Google Cloud  
**Resultado:** Todas las APIs configuradas y lista

---

## ⏱️ Timeline

```
0-5 min:   Crear cuentas
5-10 min:  Generar API keys
10-15 min: Crear secrets Kubernetes
15-25 min: Testing basic
25-30 min: Verificación final
```

---

## 1️⃣ Crear Cuentas (5 minutos)

### OpenAI

```bash
# Ir a https://platform.openai.com
# Click "Sign up"
# Email + Contraseña
# Verificar email
# Listo!
```

**Verificar acceso:**

```bash
curl https://api.openai.com/v1/models \
  -H "Authorization: Bearer test"
# → Debe retornar lista de modelos
```

---

### Google Cloud

```bash
# 1. Ir a https://console.cloud.google.com
# 2. Click "Create Project"
# 3. Nombre: "okla-ai"
# 4. Crear

# 2. En terminal:
gcloud init
gcloud auth login
gcloud config set project okla-ai
```

---

## 2️⃣ Generar API Keys (5 minutos)

### OpenAI API Key

```bash
# 1. Ir a https://platform.openai.com/account/api-keys
# 2. Click "Create new secret key"
# 3. Copy y guardar en lugar seguro
# 4. Guardar en variable:

export OPENAI_API_KEY="sk-proj-xxxxxxxxxxxx"
```

---

### Google Cloud Service Account

```bash
# 1. Crear service account
gcloud iam service-accounts create vertex-ai-okla \
  --display-name="Vertex AI for OKLA"

# 2. Otorgar permisos
gcloud projects add-iam-policy-binding okla-ai \
  --member="serviceAccount:vertex-ai-okla@okla-ai.iam.gserviceaccount.com" \
  --role="roles/aiplatform.admin"

gcloud projects add-iam-policy-binding okla-ai \
  --member="serviceAccount:vertex-ai-okla@okla-ai.iam.gserviceaccount.com" \
  --role="roles/bigquery.admin"

# 3. Crear key
gcloud iam service-accounts keys create /tmp/gcp-key.json \
  --iam-account=vertex-ai-okla@okla-ai.iam.gserviceaccount.com

# 4. Guardar en variable
export GOOGLE_APPLICATION_CREDENTIALS="/tmp/gcp-key.json"
```

---

### Verificar Google Vertex AI

```bash
# Habilitar APIs
gcloud services enable aiplatform.googleapis.com
gcloud services enable bigquery.googleapis.com

# Verificar autenticación
gcloud auth application-default print-access-token
# → Debe imprimir un token largo
```

---

## 3️⃣ Crear Secrets en Kubernetes (5 minutos)

```bash
# Crear namespace si no existe
kubectl create namespace okla

# Crear secret con OpenAI key
kubectl create secret generic openai-credentials \
  --from-literal=api_key=$OPENAI_API_KEY \
  -n okla

# Crear secret con Google Cloud key
kubectl create secret generic google-cloud-credentials \
  --from-file=/tmp/gcp-key.json \
  -n okla

# Verificar
kubectl get secrets -n okla
# → Debe mostrar: openai-credentials, google-cloud-credentials
```

---

## 4️⃣ Testing Básico (10 minutos)

### Test OpenAI en Terminal

```bash
# Variable de conveniencia
KEY=$OPENAI_API_KEY

# Test Chat Completions
curl https://api.openai.com/v1/chat/completions \
  -H "Authorization: Bearer $KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "gpt-4o-mini",
    "messages": [{"role": "user", "content": "Hola"}],
    "max_tokens": 50
  }'

# Respuesta esperada:
# {
#   "choices": [
#     {"message": {"content": "Hola! ¿Cómo estás?..."}}
#   ],
#   "usage": {"total_tokens": 25}
# }
```

---

### Test Google Vertex AI en Python

```bash
# Instalar cliente
pip install google-cloud-aiplatform

# Crear script test.py
cat > test.py << 'EOF'
from google.cloud import aiplatform

# Configurar
project_id = "okla-ai"
location = "us-central1"

# Inicializar
aiplatform.init(project=project_id, location=location)

# Test: obtener lista de modelos
response = aiplatform.gapic.PredictionServiceClient().predict(
    endpoint="projects/okla-ai/locations/us-central1/endpoints/test"
)

print("✅ Google Cloud configurado correctamente")
EOF

python test.py
```

---

## 5️⃣ En el Código (.NET)

### Program.cs

```csharp
using OpenAI;
using Google.Cloud.AIPlatform.V1;

var builder = WebApplication.CreateBuilder(args);

// OpenAI
var openaiKey = builder.Configuration["OpenAI:ApiKey"];
builder.Services.AddSingleton(new OpenAIClient(openaiKey));

// Google Cloud
var googleCredentials = builder.Configuration["Google:CredentialsPath"];
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", googleCredentials);

var app = builder.Build();
```

### appsettings.json

```json
{
  "OpenAI": {
    "ApiKey": "${OPENAI_API_KEY}"
  },
  "Google": {
    "ProjectId": "okla-ai",
    "Location": "us-central1",
    "CredentialsPath": "/run/secrets/google-cloud-credentials"
  }
}
```

### Deployment.yaml

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: chatbot-service
  namespace: okla
spec:
  template:
    spec:
      containers:
        - name: api
          image: cardealer-chatbot:latest
          env:
            - name: OPENAI_API_KEY
              valueFrom:
                secretKeyRef:
                  name: openai-credentials
                  key: api_key
            - name: GOOGLE_APPLICATION_CREDENTIALS
              value: /var/secrets/google/key.json
          volumeMounts:
            - name: google-cloud-key
              mountPath: /var/secrets/google
      volumes:
        - name: google-cloud-key
          secret:
            secretName: google-cloud-credentials
```

---

## 6️⃣ Verificación Final (5 minutos)

### Checklist

```bash
# 1. ¿OpenAI API key válida?
curl https://api.openai.com/v1/models \
  -H "Authorization: Bearer $OPENAI_API_KEY" | grep "gpt-4o-mini"
# → Debe mostrar: "id": "gpt-4o-mini"

# 2. ¿Google credentials válidas?
gcloud auth application-default print-access-token
# → Debe imprimir token

# 3. ¿Secrets en Kubernetes?
kubectl get secrets -n okla | grep -E "openai|google"
# → Debe mostrar 2 secrets

# 4. ¿Pod puede acceder a secrets?
kubectl get pods -n okla
# → Pods deben estar en estado "Running"

# 5. ¿Logs del pod sin errores?
kubectl logs -f deployment/chatbot-service -n okla
# → No debe haber errores de auth
```

---

## ⚡ Troubleshooting Rápido

### Error: "401 Unauthorized" OpenAI

```
Causa: API key inválida
Solución:
1. Verificar copia correcta de https://platform.openai.com/account/api-keys
2. No debe tener espacios adicionales
3. Regenerar key si es necesario
```

### Error: "GOOGLE_APPLICATION_CREDENTIALS not found"

```
Causa: Variable de entorno no configurada
Solución:
1. export GOOGLE_APPLICATION_CREDENTIALS="/ruta/a/key.json"
2. Verificar que archivo existe: ls $GOOGLE_APPLICATION_CREDENTIALS
3. En K8s: verificar volumeMounts en deployment
```

### Error: "401 Cannot list models"

```
Causa: Proyecto GCP no tiene APIs habilitadas
Solución:
gcloud services enable aiplatform.googleapis.com
gcloud services enable bigquery.googleapis.com
```

---

## 📚 Documentos Relacionados

- [OPENAI_API.md](OPENAI_API.md) - Implementación detallada
- [GOOGLE_VERTEX_AI.md](GOOGLE_VERTEX_AI.md) - Setup completo en GCP
- [README.md](README.md) - Visión general

---

## ✅ Listo!

Si completaste todos los pasos sin errores:

```
✅ OpenAI API configurada
✅ Google Vertex AI configurada
✅ Secrets en Kubernetes creados
✅ Código puede usar ambas APIs
✅ Listo para implementación
```

Próximo paso: Leer [OPENAI_API.md](OPENAI_API.md) para implementar ChatbotService.

---

_QuickStart de configuración de APIs de IA_  
_Tiempo total: ~30 minutos_  
_Dificultad: Intermedia_
