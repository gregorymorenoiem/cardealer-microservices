# 🚀 Tutorial: Cómo Agregar un Microservicio al CI/CD

## 📋 Índice
1. [Prerrequisitos](#prerrequisitos)
2. [Paso a Paso: Agregar Nuevo Microservicio](#paso-a-paso)
3. [Ejemplos Prácticos](#ejemplos-prácticos)
4. [Configuración Avanzada](#configuración-avanzada)
5. [Verificación y Troubleshooting](#verificación-y-troubleshooting)
6. [Checklist de Validación](#checklist-de-validación)

---

## 🎯 Prerrequisitos

Antes de agregar un nuevo microservicio al CI/CD, asegúrate de tener:

### **1. Estructura del Microservicio**

```
backend/
└── TuServicio/
    ├── TuServicio.sln                    # ✅ Solución .NET
    ├── TuServicio.Api/
    │   ├── TuServicio.Api.csproj
    │   ├── Program.cs
    │   └── appsettings.json
    ├── TuServicio.Domain/
    │   └── TuServicio.Domain.csproj
    ├── TuServicio.Application/
    │   └── TuServicio.Application.csproj
    ├── TuServicio.Infrastructure/
    │   └── TuServicio.Infrastructure.csproj
    ├── TuServicio.Tests/                 # ✅ Proyecto de tests
    │   └── TuServicio.Tests.csproj
    └── Dockerfile                         # ✅ Dockerfile
```

### **2. Dockerfile Funcional**

Tu servicio debe tener un Dockerfile que compile correctamente:

```dockerfile
# Ejemplo básico
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TuServicio/TuServicio.Api/TuServicio.Api.csproj", "TuServicio/TuServicio.Api/"]
RUN dotnet restore "TuServicio/TuServicio.Api/TuServicio.Api.csproj"
COPY . .
WORKDIR "/src/TuServicio/TuServicio.Api"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TuServicio.Api.dll"]
```

### **3. Tests Pasando al 100%**

```bash
cd backend/TuServicio
dotnet test TuServicio.Tests/TuServicio.Tests.csproj --verbosity normal

# Resultado esperado:
# Test Run Successful.
# Total tests: X
#      Passed: X
```

---

## 📝 Paso a Paso: Agregar Nuevo Microservicio

### **Método 1: Copia Rápida (RECOMENDADO)** ⚡

#### **Paso 1.1: Copiar Template** (30 segundos)

```bash
# Desde la raíz del repositorio
cd .github/workflows

# Copiar el template de ProductService
cp productservice.yml tuservicio.yml
```

#### **Paso 1.2: Editar el Archivo** (2 minutos)

Abre `.github/workflows/tuservicio.yml` y reemplaza:

```yaml
name: TuServicio CI/CD  # ⬅️ Cambiar nombre

on:
  push:
    branches: [ main, develop ]
    paths:
      - 'backend/TuServicio/**'              # ⬅️ Path del servicio
      - '.github/workflows/tuservicio.yml'   # ⬅️ Path del workflow
      - '.github/workflows/_reusable-dotnet-service.yml'
  pull_request:
    branches: [ main, develop ]
    paths:
      - 'backend/TuServicio/**'              # ⬅️ Path del servicio
  workflow_dispatch:

jobs:
  ci-cd:
    name: 🎯 TuServicio Pipeline  # ⬅️ Emoji + nombre
    uses: ./.github/workflows/_reusable-dotnet-service.yml
    with:
      service-name: tuservicio        # ⬅️ Nombre en minúsculas (para Docker)
      service-path: backend/TuServicio  # ⬅️ Path al servicio
      dotnet-version: '8.0.x'
      run-docker-build: true
      run-docker-push: true
      dockerfile-path: Dockerfile     # ⬅️ Path relativo al servicio
      solution-file: TuServicio.sln   # ⬅️ Nombre del .sln
    permissions:
      contents: read
      packages: write
```

#### **Paso 1.3: Commit y Push** (1 minuto)

```bash
git add .github/workflows/tuservicio.yml
git commit -m "ci: add TuServicio CI/CD pipeline"
git push origin main
```

**¡Listo!** Tu servicio ahora tiene CI/CD completo. 🎉

---

### **Método 2: Crear desde Cero** (Opción Manual) 📝

Si prefieres entender cada línea, aquí está el proceso completo:

#### **Paso 2.1: Crear Archivo Nuevo**

```bash
touch .github/workflows/tuservicio.yml
```

#### **Paso 2.2: Contenido Completo del Archivo**

```yaml
# ==============================================================================
# TUSERVICIO CI/CD PIPELINE
# ==============================================================================
# Este workflow implementa CI/CD completo para TuServicio:
# - Build y compilación
# - Ejecución de tests
# - Análisis de código
# - Build de imagen Docker
# - Push a GitHub Container Registry (solo en main)
# ==============================================================================

name: TuServicio CI/CD

# Triggers: ¿Cuándo se ejecuta este workflow?
on:
  # 1. Push a main/develop con cambios en TuServicio
  push:
    branches: [ main, develop ]
    paths:
      - 'backend/TuServicio/**'
      - '.github/workflows/tuservicio.yml'
      - '.github/workflows/_reusable-dotnet-service.yml'
  
  # 2. Pull Request con cambios en TuServicio
  pull_request:
    branches: [ main, develop ]
    paths:
      - 'backend/TuServicio/**'
  
  # 3. Ejecución manual desde GitHub UI
  workflow_dispatch:

jobs:
  # Job principal que llama al workflow reutilizable
  ci-cd:
    name: 🎯 TuServicio Pipeline
    
    # Usa el workflow reutilizable
    uses: ./.github/workflows/_reusable-dotnet-service.yml
    
    # Configuración específica de TuServicio
    with:
      # Nombre del servicio (usado para Docker image tag)
      # Formato: minúsculas, sin espacios
      service-name: tuservicio
      
      # Path al directorio del servicio
      # Debe contener el .sln y Dockerfile
      service-path: backend/TuServicio
      
      # Versión de .NET SDK
      dotnet-version: '8.0.x'
      
      # ¿Construir imagen Docker?
      run-docker-build: true
      
      # ¿Push a registry? (solo en main)
      run-docker-push: true
      
      # Path al Dockerfile (relativo a service-path)
      dockerfile-path: Dockerfile
      
      # Nombre del archivo .sln (opcional)
      solution-file: TuServicio.sln
      
      # Filtro de tests (opcional)
      # Ejemplos:
      # - "Category=Unit" → Solo tests unitarios
      # - "Category!=Integration" → Excluir tests de integración
      test-filter: ''
      
      # ¿Saltear tests? (no recomendado)
      skip-tests: false
    
    # Permisos requeridos
    permissions:
      contents: read      # Leer código
      packages: write     # Push a GitHub Container Registry
```

---

## 💡 Ejemplos Prácticos

### **Ejemplo 1: InventoryService** (Servicio de Inventario)

```yaml
name: InventoryService CI/CD

on:
  push:
    branches: [ main, develop ]
    paths:
      - 'backend/InventoryService/**'
      - '.github/workflows/inventoryservice.yml'
  pull_request:
    branches: [ main, develop ]
    paths:
      - 'backend/InventoryService/**'
  workflow_dispatch:

jobs:
  ci-cd:
    name: 📦 InventoryService Pipeline
    uses: ./.github/workflows/_reusable-dotnet-service.yml
    with:
      service-name: inventoryservice
      service-path: backend/InventoryService
      dotnet-version: '8.0.x'
      run-docker-build: true
      run-docker-push: true
    permissions:
      contents: read
      packages: write
```

---

### **Ejemplo 2: PaymentService** (Servicio de Pagos con Tests Específicos)

```yaml
name: PaymentService CI/CD

on:
  push:
    branches: [ main, develop ]
    paths:
      - 'backend/PaymentService/**'
      - '.github/workflows/paymentservice.yml'
  pull_request:
    branches: [ main, develop ]
    paths:
      - 'backend/PaymentService/**'
  workflow_dispatch:

jobs:
  ci-cd:
    name: 💳 PaymentService Pipeline
    uses: ./.github/workflows/_reusable-dotnet-service.yml
    with:
      service-name: paymentservice
      service-path: backend/PaymentService
      dotnet-version: '8.0.x'
      run-docker-build: true
      run-docker-push: true
      # Solo ejecutar tests unitarios (excluir integración con APIs externas)
      test-filter: 'Category!=Integration&Category!=ExternalAPI'
    permissions:
      contents: read
      packages: write
```

---

### **Ejemplo 3: ReportingService** (.NET 7, Dockerfile personalizado)

```yaml
name: ReportingService CI/CD

on:
  push:
    branches: [ main, develop ]
    paths:
      - 'backend/ReportingService/**'
      - '.github/workflows/reportingservice.yml'
  pull_request:
    branches: [ main, develop ]
    paths:
      - 'backend/ReportingService/**'
  workflow_dispatch:

jobs:
  ci-cd:
    name: 📊 ReportingService Pipeline
    uses: ./.github/workflows/_reusable-dotnet-service.yml
    with:
      service-name: reportingservice
      service-path: backend/ReportingService
      dotnet-version: '7.0.x'  # ⬅️ .NET 7
      run-docker-build: true
      run-docker-push: true
      dockerfile-path: Dockerfile.custom  # ⬅️ Dockerfile personalizado
      solution-file: ReportingService.sln
    permissions:
      contents: read
      packages: write
```

---

### **Ejemplo 4: LegacyService** (Sin Docker, solo Build/Test)

```yaml
name: LegacyService CI/CD

on:
  push:
    branches: [ main, develop ]
    paths:
      - 'backend/LegacyService/**'
      - '.github/workflows/legacyservice.yml'
  pull_request:
    branches: [ main, develop ]
    paths:
      - 'backend/LegacyService/**'
  workflow_dispatch:

jobs:
  ci-cd:
    name: 🔧 LegacyService Pipeline
    uses: ./.github/workflows/_reusable-dotnet-service.yml
    with:
      service-name: legacyservice
      service-path: backend/LegacyService
      dotnet-version: '8.0.x'
      run-docker-build: false  # ⬅️ No Docker
      run-docker-push: false   # ⬅️ No push
    permissions:
      contents: read
      packages: write
```

---

## ⚙️ Configuración Avanzada

### **Opción 1: Agregar al Monorepo Orchestrator** (Detección Inteligente)

Para que tu servicio se ejecute automáticamente cuando hay cambios, agrégalo a `monorepo-cicd.yml`:

#### **Paso A: Editar `.github/workflows/monorepo-cicd.yml`**

```yaml
# 1. Agregar output en detect-changes job
detect-changes:
  outputs:
    # ... otros servicios
    tu-servicio: ${{ steps.filter.outputs.tu-servicio }}  # ⬅️ AGREGAR

# 2. Agregar filtro de paths
- name: 🔍 Detect service changes
  uses: dorny/paths-filter@v3
  id: filter
  with:
    filters: |
      # ... otros servicios
      tu-servicio:                    # ⬅️ AGREGAR
        - 'backend/TuServicio/**'

# 3. Agregar job
tu-servicio:
  name: 🎯 TuServicio
  needs: detect-changes
  if: needs.detect-changes.outputs.tu-servicio == 'true' || needs.detect-changes.outputs.shared == 'true'
  uses: ./.github/workflows/_reusable-dotnet-service.yml
  with:
    service-name: tuservicio
    service-path: backend/TuServicio
    dotnet-version: '8.0.x'
    run-docker-build: true
    run-docker-push: true
  permissions:
    contents: read
    packages: write

# 4. Agregar a pipeline-status job
pipeline-status:
  needs: [..., tu-servicio]  # ⬅️ AGREGAR
  steps:
    - name: 📊 Generate summary
      run: |
        echo "| TuServicio | ${{ needs.tu-servicio.result }} |" >> $GITHUB_STEP_SUMMARY
```

---

### **Opción 2: Customizar el Workflow Reutilizable**

Si necesitas comportamiento específico, puedes crear un workflow customizado:

```yaml
name: TuServicio CI/CD (Custom)

on:
  push:
    branches: [ main, develop ]
    paths:
      - 'backend/TuServicio/**'

jobs:
  # Primero ejecuta el pipeline estándar
  standard-pipeline:
    uses: ./.github/workflows/_reusable-dotnet-service.yml
    with:
      service-name: tuservicio
      service-path: backend/TuServicio
    permissions:
      contents: read
      packages: write
  
  # Luego agrega pasos personalizados
  custom-steps:
    runs-on: ubuntu-latest
    needs: standard-pipeline
    steps:
      - name: 🔧 Custom validation
        run: |
          echo "Ejecutando validaciones específicas de TuServicio"
          # Tus comandos personalizados aquí
      
      - name: 📧 Send notification
        run: |
          curl -X POST https://api.slack.com/webhooks/... \
            -d '{"text":"TuServicio deployed!"}'
```

---

## ✅ Verificación y Troubleshooting

### **Paso 1: Verificar en GitHub Actions**

Después de hacer push, ve a:
```
https://github.com/gmorenotrade/cardealer-microservices/actions
```

Deberías ver:
- ✅ **TuServicio CI/CD** (ejecutándose o completado)
- ✅ **Monorepo CI/CD** (si lo agregaste al orchestrator)

---

### **Paso 2: Verificar Logs**

Haz clic en el workflow y revisa cada job:

```
🔨 Build & Test
  ├── ✅ Checkout code
  ├── ✅ Setup .NET
  ├── ✅ Restore dependencies
  ├── ✅ Build solution
  └── ✅ Run tests (10/10 passed)

🔍 Code Quality
  └── ✅ Run code analysis

🐳 Docker Build
  ├── ✅ Set up Docker Buildx
  ├── ✅ Log in to Container Registry
  ├── ✅ Extract metadata
  └── ✅ Build Docker image

🚀 Push Docker Image (solo en main)
  └── ✅ Build and Push
```

---

### **Paso 3: Verificar Imagen Docker**

Si el push fue exitoso, verifica la imagen en GitHub Container Registry:

```
https://github.com/gmorenotrade/cardealer-microservices/pkgs/container/tuservicio
```

Tags esperados:
- `latest` (si push a main)
- `main-abc1234` (SHA del commit)
- `develop` (si push a develop)

---

### **Problemas Comunes y Soluciones**

#### **❌ Error: "Solution file not found"**

**Causa:** El workflow no encuentra el archivo `.sln`

**Solución:**
```yaml
with:
  service-path: backend/TuServicio  # Verificar que sea correcto
  solution-file: TuServicio.sln     # Agregar explícitamente
```

---

#### **❌ Error: "Tests failed"**

**Causa:** Tests no pasan en CI

**Solución:**
```bash
# Ejecutar tests localmente primero
cd backend/TuServicio
dotnet test --verbosity normal

# Si fallan, arreglar antes de push
# Si necesitas saltear tests temporalmente:
```

```yaml
with:
  skip-tests: true  # ⚠️ No recomendado
```

---

#### **❌ Error: "Docker build failed"**

**Causa:** Dockerfile tiene errores o rutas incorrectas

**Solución:**
```bash
# Probar build localmente
cd backend
docker build -f TuServicio/Dockerfile -t tuservicio:test .

# Verificar paths en Dockerfile
COPY ["TuServicio/TuServicio.Api/TuServicio.Api.csproj", "TuServicio/TuServicio.Api/"]
#      ^^^^^^^^^ Path relativo desde /backend
```

---

#### **❌ Error: "Permission denied" al push de Docker**

**Causa:** Faltan permisos en el workflow

**Solución:**
```yaml
jobs:
  ci-cd:
    permissions:
      contents: read    # ⬅️ AGREGAR
      packages: write   # ⬅️ AGREGAR
```

---

#### **❌ Workflow no se ejecuta**

**Causa:** Path triggers incorrectos

**Solución:**
```yaml
on:
  push:
    paths:
      - 'backend/TuServicio/**'  # Verificar mayúsculas/minúsculas
      - '.github/workflows/tuservicio.yml'  # Incluir el propio workflow
```

---

## 📋 Checklist de Validación

Antes de considerar completa la implementación, verifica:

### **Pre-Deploy**
```
[ ] El servicio compila sin errores (dotnet build)
[ ] Los tests pasan al 100% (dotnet test)
[ ] El Dockerfile construye correctamente (docker build)
[ ] El servicio tiene estructura Clean Architecture
[ ] Existe carpeta Tests/ con tests unitarios
[ ] appsettings.Development.json y Production.json existen
```

### **Workflow Configuration**
```
[ ] Archivo .github/workflows/tuservicio.yml creado
[ ] service-name en minúsculas y sin espacios
[ ] service-path apunta al directorio correcto
[ ] paths incluye el path del servicio
[ ] paths incluye el path del workflow mismo
[ ] permissions incluye contents: read y packages: write
```

### **Post-Deploy**
```
[ ] GitHub Actions muestra el workflow ejecutándose
[ ] Job "Build & Test" completa exitosamente
[ ] Job "Docker Build" completa exitosamente
[ ] Imagen Docker aparece en GitHub Container Registry
[ ] Tags de Docker son correctos (latest, main-SHA, etc.)
[ ] No hay errores en los logs
```

### **Opcionales**
```
[ ] Agregado a monorepo-cicd.yml (orchestrator)
[ ] Documentación actualizada en README
[ ] docker-compose.yml actualizado con el servicio
[ ] Health checks configurados
[ ] Variables de entorno documentadas
```

---

## 🎯 Script de Automatización

Para migrar TODOS los servicios de golpe:

```bash
#!/bin/bash
# migrate-all-services.sh

# Lista de servicios a migrar
SERVICES=(
  "AdminService:adminservice:🔧"
  "UserService:userservice:👤"
  "RoleService:roleservice:🔑"
  "ContactService:contactservice:📞"
  "AuditService:auditservice:📝"
  "ConfigurationService:configurationservice:⚙️"
  "SchedulerService:schedulerservice:⏰"
  "HealthCheckService:healthcheckservice:💊"
  "SearchService:searchservice:🔍"
  "FeatureToggleService:featuretoggleservice:🎚️"
  "IdempotencyService:idempotencyservice:🔁"
  "RateLimitingService:ratelimitingservice:⏱️"
  "BackupDRService:backupdrservice:💾"
  "CacheService:cacheservice:⚡"
  "MessageBusService:messagebusservice:📨"
  "LoggingService:loggingservice:📋"
  "TracingService:tracingservice:🔬"
  "ServiceDiscovery:servicediscovery:🗺️"
  "ApiDocsService:apidocsservice:📚"
  "MediaService:mediaservice:🎬"
  "FileStorageService:filestorageservice:📁"
)

for SERVICE_INFO in "${SERVICES[@]}"; do
  # Parse service info: "ServiceName:servicename:emoji"
  IFS=':' read -r SERVICE_NAME SERVICE_LOWER EMOJI <<< "$SERVICE_INFO"
  
  echo "🚀 Creating workflow for $SERVICE_NAME..."
  
  cat > .github/workflows/${SERVICE_LOWER}.yml << EOF
name: ${SERVICE_NAME} CI/CD

on:
  push:
    branches: [ main, develop ]
    paths:
      - 'backend/${SERVICE_NAME}/**'
      - '.github/workflows/${SERVICE_LOWER}.yml'
      - '.github/workflows/_reusable-dotnet-service.yml'
  pull_request:
    branches: [ main, develop ]
    paths:
      - 'backend/${SERVICE_NAME}/**'
  workflow_dispatch:

jobs:
  ci-cd:
    name: ${EMOJI} ${SERVICE_NAME} Pipeline
    uses: ./.github/workflows/_reusable-dotnet-service.yml
    with:
      service-name: ${SERVICE_LOWER}
      service-path: backend/${SERVICE_NAME}
      dotnet-version: '8.0.x'
      run-docker-build: true
      run-docker-push: true
    permissions:
      contents: read
      packages: write
EOF

  echo "  ✅ Created .github/workflows/${SERVICE_LOWER}.yml"
done

echo ""
echo "🎉 ¡Migración completada!"
echo ""
echo "📝 Siguiente paso:"
echo "   git add .github/workflows/"
echo "   git commit -m 'ci: migrate all services to reusable workflows'"
echo "   git push origin main"
```

**Ejecutar:**
```bash
chmod +x migrate-all-services.sh
./migrate-all-services.sh
```

---

## 📚 Recursos Adicionales

### **Documentación Oficial**
- [GitHub Actions Reusable Workflows](https://docs.github.com/en/actions/using-workflows/reusing-workflows)
- [Docker Build Push Action](https://github.com/docker/build-push-action)
- [.NET CI/CD Best Practices](https://docs.microsoft.com/en-us/dotnet/devops/)

### **Archivos de Referencia**
- `.github/workflows/_reusable-dotnet-service.yml` - Template principal
- `.github/workflows/productservice.yml` - Ejemplo de servicio completo
- `.github/CICD_ARCHITECTURE.md` - Arquitectura completa
- `.github/WORKFLOWS_COEXISTENCE.md` - Convivencia de workflows

---

## 🎉 Resumen Ejecutivo

Para agregar un nuevo microservicio al CI/CD:

```bash
# 1. Copiar template (30 segundos)
cp .github/workflows/productservice.yml .github/workflows/tuservicio.yml

# 2. Buscar y reemplazar (1 minuto)
# - ProductService → TuServicio
# - productservice → tuservicio
# - 🛍️ → tu emoji favorito

# 3. Commit y push (30 segundos)
git add .github/workflows/tuservicio.yml
git commit -m "ci: add TuServicio CI/CD pipeline"
git push

# ¡Listo! Tu servicio ahora tiene CI/CD completo 🎉
```

**Tiempo total:** 2-3 minutos por servicio

---

**Creado:** December 5, 2025  
**Versión:** 2.0.0  
**Autor:** DevOps Team  
**Última actualización:** December 5, 2025
