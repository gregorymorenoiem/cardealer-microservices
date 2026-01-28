# 🚗 Test Data for AIProcessingService

Este directorio contiene datos de prueba para los AI Workers.

## ✅ Test Results (January 26, 2026)

**All 16 images processed successfully on CPU!**

| Metric           | Value                             |
| ---------------- | --------------------------------- |
| Total Jobs       | 23                                |
| Success Rate     | 100% (last 16 images)             |
| Average Time     | 3.28 seconds                      |
| Total Processing | 75.4 seconds                      |
| Slowest          | 5.0s (Mercedes AMG - large image) |
| Fastest          | 2.5s (cached model)               |

### Quick Test

```bash
# Run batch test with all 16 images
./test_batch.sh 16

# Run with just 3 images for quick test
./test_batch.sh 3
```

## 📁 Estructura

```
test-data/
├── photos/          # Imágenes individuales (9 fotos de Unsplash)
├── datasets/        # Dataset completo para training/testing (16 imágenes)
├── videos/          # Videos para 360° spin (requiere descarga manual)
├── README.md        # Este archivo
├── run-tests.sh     # Script de pruebas automatizadas
└── generate-token.py # Generador de JWT para testing
```

## 📸 Dataset Principal (datasets/) - 16 imágenes

Imágenes de alta calidad de Unsplash (Licencia Libre):

| Archivo                 | Tipo     | Descripción         | Tamaño |
| ----------------------- | -------- | ------------------- | ------ |
| `sedan_black_1.jpg`     | Sedan    | Vista frontal negro | 56K    |
| `sports_red_1.jpg`      | Sports   | Deportivo rojo      | 51K    |
| `suv_white_1.jpg`       | SUV      | SUV blanco          | 115K   |
| `corvette_yellow_1.jpg` | Sports   | Corvette amarillo   | 77K    |
| `pickup_truck_1.jpg`    | Truck    | Pickup              | 109K   |
| `tesla_model_s.jpg`     | Electric | Tesla Model S       | 93K    |
| `bmw_m4_blue.jpg`       | Sports   | BMW M4 azul         | 77K    |
| `ferrari_red.jpg`       | Supercar | Ferrari rojo        | 58K    |
| `interior_luxury.jpg`   | Interior | Interior de lujo    | 91K    |
| `car_side_view.jpg`     | Side     | Vista lateral       | 137K   |
| `bmw_front.jpg`         | Front    | BMW vista frontal   | 809K   |
| `audi_r8.jpg`           | Supercar | Audi R8             | 67K    |
| `mercedes_amg.jpg`      | Sports   | Mercedes AMG        | 90K    |
| `jeep_wrangler.jpg`     | SUV      | Jeep Wrangler       | 58K    |
| `porsche_911.jpg`       | Sports   | Porsche 911         | 48K    |
| `lamborghini.jpg`       | Supercar | Lamborghini         | 79K    |

**Total:** 16 imágenes, ~2MB

## 📸 Fotos Adicionales (photos/) - 9 imágenes

| Archivo            | Descripción          | Uso                           |
| ------------------ | -------------------- | ----------------------------- |
| `car_front_1.jpg`  | Vista frontal sedan  | CLIP classification           |
| `car_front_2.jpg`  | Vista frontal sports | CLIP classification           |
| `car_side_1.jpg`   | Vista lateral        | SAM2 segmentation             |
| `car_rear_1.jpg`   | Vista trasera        | CLIP angle detection          |
| `suv_1.jpg`        | SUV exterior         | CLIP vehicle type             |
| `interior_1.jpg`   | Interior dashboard   | CLIP interior detection       |
| `sports_car_1.jpg` | Deportivo            | SAM2 + background replacement |
| `luxury_car_1.jpg` | Auto de lujo         | Full pipeline test            |
| `truck_1.jpg`      | Camioneta            | YOLO plate detection          |

## 🎬 Videos para 360° Spin

Los videos de Pexels requieren descarga manual. Aquí están las fuentes recomendadas:

### Opción 1: Pexels (Gratis)

```bash
# Visita estas URLs y descarga manualmente:
# https://www.pexels.com/search/videos/car%20spinning/
# https://www.pexels.com/search/videos/car%20360/
# https://www.pexels.com/search/videos/car%20turntable/
```

### Opción 2: Pixabay (Gratis)

```bash
# https://pixabay.com/videos/search/car%20spinning/
# https://pixabay.com/videos/search/car%20360/
```

### Opción 3: Dataset Académico - Stanford Cars

```bash
# Para clasificación de vehículos:
# https://ai.stanford.edu/~jkrause/cars/car_dataset.html
# 16,185 imágenes de 196 clases de autos
```

### Opción 4: CompCars Dataset

```bash
# Para detección y segmentación:
# http://mmlab.ie.cuhk.edu.hk/datasets/comp_cars/
# 136,726 imágenes de autos con anotaciones
```

## 🧪 Scripts de Testing

### Test CLIP (Clasificación)

```bash
# Clasificar tipo de imagen
curl -X POST http://localhost:5070/api/aiprocessing/analyze \
  -H "Content-Type: application/json" \
  -d '{
    "vehicleId": "test-vehicle-001",
    "imageUrl": "file:///app/test-data/photos/car_front_1.jpg"
  }'
```

### Test SAM2 (Segmentación)

```bash
# Remover fondo
curl -X POST http://localhost:5070/api/aiprocessing/process \
  -H "Content-Type: application/json" \
  -d '{
    "vehicleId": "test-vehicle-001",
    "userId": "test-user",
    "imageUrl": "https://example.com/car.jpg",
    "processingType": "BackgroundRemoval",
    "options": {}
  }'
```

### Test YOLO (Detección de placas)

```bash
# Detectar y difuminar placas
curl -X POST http://localhost:5070/api/aiprocessing/process \
  -H "Content-Type: application/json" \
  -d '{
    "vehicleId": "test-vehicle-001",
    "userId": "test-user",
    "imageUrl": "https://example.com/car-with-plate.jpg",
    "processingType": "PlateBlurring",
    "options": {}
  }'
```

## 📊 Datasets Públicos para AI Training

### 🏆 Datasets Profesionales (Kaggle, HuggingFace, etc.)

| Dataset           | Imágenes  | Descripción         | URL                                                                       |
| ----------------- | --------- | ------------------- | ------------------------------------------------------------------------- |
| **Stanford Cars** | 16,185    | 196 clases de autos | [ai.stanford.edu](https://ai.stanford.edu/~jkrause/cars/car_dataset.html) |
| **CompCars**      | 136,726   | Autos con atributos | [CUHK](http://mmlab.ie.cuhk.edu.hk/datasets/comp_cars/)                   |
| **Vehicle-1M**    | 1,000,000 | Re-identificación   | [PKU](https://www.pkuml.org/resources/pku-vehicle.html)                   |
| **CCPD**          | 250,000   | Placas chinas       | [GitHub](https://github.com/detectRecog/CCPD)                             |
| **Cityscapes**    | 25,000    | Segmentación urbana | [cityscapes-dataset.com](https://www.cityscapes-dataset.com/)             |

### 📦 Kaggle (Requiere cuenta gratuita)

```bash
# Instalar Kaggle CLI
pip install kaggle

# Descargar Stanford Cars Dataset
kaggle datasets download -d jessicali9530/stanford-cars-dataset

# Descargar Vehicle Classification
kaggle datasets download -d brsdincer/vehicle-classification-dataset

# Descargar Car Images Dataset
kaggle datasets download -d pankaj1111/car-images-dataset
```

### 🤗 HuggingFace Datasets

```bash
# Instalar datasets
pip install datasets

# Python script para descargar
python << 'EOF'
from datasets import load_dataset
dataset = load_dataset("keremberke/car-detection", split="train")
print(f"Loaded {len(dataset)} images")
EOF
```

### 🔥 Roboflow Universe (Gratis con cuenta)

| Dataset       | Imágenes | Formato | URL                                                                   |
| ------------- | -------- | ------- | --------------------------------------------------------------------- |
| Car Detection | 10,000+  | YOLO    | [roboflow.com](https://universe.roboflow.com/car-detection-datasets)  |
| License Plate | 5,000+   | YOLO    | [roboflow.com](https://universe.roboflow.com/license-plate-detection) |
| Vehicle Types | 8,000+   | COCO    | [roboflow.com](https://universe.roboflow.com/vehicle-type)            |

```bash
# Descargar desde Roboflow (requiere API key gratuita)
pip install roboflow
python << 'EOF'
from roboflow import Roboflow
rf = Roboflow(api_key="YOUR_API_KEY")
project = rf.workspace().project("car-detection")
dataset = project.version(1).download("yolov8")
EOF
```

### Para Segmentación de Vehículos

1. **COCO Dataset** - http://cocodataset.org/
   - 328K imágenes con segmentación
   - Incluye categoría "car"

2. **Cityscapes** - https://www.cityscapes-dataset.com/
   - 25K imágenes de conducción urbana
   - Segmentación pixel-perfect

### Para Detección de Placas

1. **CCPD** (Chinese City Parking Dataset)
   - https://github.com/detectRecog/CCPD
   - 250K imágenes con placas anotadas

2. **OpenALPR Benchmark**
   - https://github.com/openalpr/benchmarks

### Para 360° Spin

1. **ObjectNet3D** - http://cvgl.stanford.edu/projects/objectnet3d/
   - Objetos 3D desde múltiples ángulos

2. **ShapeNet** - https://shapenet.org/
   - Modelos 3D de vehículos

## 🔧 Cómo Agregar Tus Propias Imágenes

1. Coloca las fotos en `photos/`
2. Coloca los videos en `videos/`
3. Usa URLs públicas o copia al contenedor:
   ```bash
   docker cp photos/my-car.jpg aiprocessingservice:/app/test-data/
   ```

## ⚠️ Notas

- Las imágenes de Unsplash son de alta calidad (1200px)
- Para producción, usa imágenes de al menos 1080p
- Los videos 360° deben tener 24-36 frames para un spin completo
- YOLO funciona mejor con placas claramente visibles
