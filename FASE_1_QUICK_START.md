# 🚀 GUÍA RÁPIDA: Compilar, Probar y Deployr FASE 1

**Última Actualización:** 24 de febrero de 2026  
**Propósito:** Validación local antes de merge

---

## ✅ Pre-requisitos

```bash
# Verificar versiones
dotnet --version                    # >= 8.0
dotnet --list-runtimes             # .NET 8
sqlcmd -v                           # Para migrations (opcional)
```

---

## 📦 PASO 1: Compilar Localmente

### 1.1 Navegar al proyecto

```bash
cd backend/UserService

# Verificar archivos
ls -la UserService.Domain/Entities/SellerProfile.cs      # Entity ✓
ls -la UserService.Application/DTOs/SellerDtos.cs         # DTOs ✓
ls -la UserService.Application/Validators/SellerProfileValidators.cs  # Validators ✓
ls -la UserService.Application/UseCases/Sellers/         # Handlers ✓
```

### 1.2 Compilar solución

```bash
# Compilación de debug
dotnet build -c Debug

# Esperado: Build succeeded
# ✓ UserService.Domain
# ✓ UserService.Application
# ✓ UserService.Infrastructure
# ✓ UserService.Api
```

### 1.3 Compilación de release

```bash
# Para simular producción
dotnet build -c Release

# Esperado: Build succeeded, 0 warnings
```

---

## 🧪 PASO 2: Ejecutar Tests Localmente

### 2.1 Ejecutar todos los tests de Sellers

```bash
cd UserService.Tests

# Ejecutar solo tests de specialties
dotnet test --filter "SellerProfileSpecialties" -v normal

# Output esperado:
# Test Session start: 2026-02-24 XX:XX:XX
# Target framework: .NET 8.0
# Resolving project dependencies
# Restored /path/to/UserService.Tests.csproj
# ...
# ========== Test Execution Summary ==========
# Total Tests: 11
# Passed: 11
# Failed: 0
# Duration: ~2-5 seconds
```

### 2.2 Tests individuales

```bash
# Test específico
dotnet test --filter "CreateSellerProfile_WithSpecialties_ShouldPersistSpecialties" -v normal

# Todos los CreateSellerProfile tests
dotnet test --filter "CreateSellerProfile" -v normal

# Todos los UpdateSellerProfile tests
dotnet test --filter "UpdateSellerProfile" -v normal

# Error cases
dotnet test --filter "ProfileNotFound" -v normal
```

### 2.3 Con cobertura de código

```bash
# Instalar herramienta de cobertura
dotnet tool install -g dotnet-reportgenerator-globaltool

# Ejecutar tests con cobertura
dotnet test /p:CollectCoverage=true \
            /p:CoverageFileName=coverage.xml \
            /p:Exclude="[*Tests*]*"

# Generar reporte HTML
reportgenerator -reports:"./coverage.xml" \
                -targetdir:"./coverage-report" \
                -reporttypes:Html

# Abrir reporte
open ./coverage-report/index.html
```

---

## 🗄️ PASO 3: Aplicar Migration a BD Local

### 3.1 Opción A: PostgreSQL CLI (SQL puro)

```bash
# Conectar a BD local
psql -U postgres -d cardealer_db -h localhost

# En psql prompt:
\i backend/UserService/Migrations/20260224_AddSpecialtiesFieldToSellerProfile.sql

# Verificar
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name='seller_profiles'
AND column_name='specialties';

# Esperado:
# column_name |   data_type
# ────────────┼───────────────
# specialties | text[]
```

### 3.2 Opción B: Entity Framework CLI (recomendado)

```bash
# Navegar al proyecto API
cd UserService.Api

# Aplicar migrations pending
dotnet ef database update

# Verificar
dotnet ef migrations list

# Esperado:
# 20260224_AddSpecialtiesFieldToSellerProfile
```

### 3.3 Verificar migration

```bash
# Conectar a BD
psql -U postgres -d cardealer_db

# Verificar columna
\d seller_profiles

# Esperado:
# Column      |       Type        | Collation | Nullable | Default
# ─────────────────────────────────────────────────────────────────
# specialties | text[]            |           | not null | '{}'::text[]

# Verificar índice
SELECT indexname FROM pg_indexes
WHERE tablename='seller_profiles'
AND indexname LIKE '%specialties%';

# Esperado:
# indexname
# ─────────────────────────────────────
# idx_seller_profiles_specialties
```

---

## 🔌 PASO 4: Pruebas de Integración Locales

### 4.1 Startup del servicio

```bash
# Navegar al API
cd UserService.Api

# Ejecutar en modo desarrollo
dotnet run --configuration Debug

# Esperado:
# info: Microsoft.AspNetCore.Hosting.Diagnostics
#       Now listening on: https://localhost:5001
#       Now listening on: http://localhost:5000
#       Application started...
```

### 4.2 Probar endpoint de creación

```bash
# Terminal new
curl -X POST https://localhost:5001/api/sellers \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "00000000-0000-0000-0000-000000000001",
    "fullName": "Juan García",
    "phone": "+1-809-555-0123",
    "email": "juan@example.com",
    "city": "Santo Domingo",
    "country": "DO",
    "specialties": ["Sedanes", "SUVs", "Camionetas"]
  }'

# Esperado:
# {
#   "id": "...",
#   "userId": "...",
#   "fullName": "Juan García",
#   "phone": "+1-809-555-0123",
#   "email": "juan@example.com",
#   "city": "Santo Domingo",
#   "specialties": ["Sedanes", "SUVs", "Camionetas"],  ✨ NUEVO
#   "createdAt": "2026-02-24T..."
# }
```

### 4.3 Probar endpoint de lectura

```bash
# GET profile
curl -X GET https://localhost:5001/api/sellers/{sellerId} \
  -H "Authorization: Bearer YOUR_TOKEN"

# Esperado:
# {
#   ...
#   "specialties": ["Sedanes", "SUVs", "Camionetas"],  ✨ PERSISTE
#   ...
# }
```

### 4.4 Probar endpoint de actualización

```bash
# PUT profile
curl -X PUT https://localhost:5001/api/sellers/{sellerId} \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "specialties": ["Minivanes", "Camionetas", "Motos"]
  }'

# Esperado:
# {
#   ...
#   "specialties": ["Minivanes", "Camionetas", "Motos"],  ✨ ACTUALIZADO
#   "updatedAt": "2026-02-24T..."
# }
```

---

## 📋 PASO 5: Validación Checklist

### ✅ Código

- [ ] Compilación sin errores
- [ ] Compilación sin warnings
- [ ] Tests pasan (11/11)
- [ ] Cobertura de tests >= 80%

### ✅ Base de Datos

- [ ] Migration aplicada exitosamente
- [ ] Columna `specialties` existe
- [ ] Índice `idx_seller_profiles_specialties` existe
- [ ] Dados existentes no se corrompieron

### ✅ API

- [ ] POST /api/sellers con specialties funciona
- [ ] GET /api/sellers/{id} retorna specialties
- [ ] PUT /api/sellers/{id} actualiza specialties
- [ ] Validación de max 10 especialidades funciona
- [ ] Validación de max 100 chars funciona

### ✅ Compatibilidad

- [ ] Vendedores sin specialties = array vacío
- [ ] Versión anterior de API sigue funcionando
- [ ] Rollback script probado

---

## 🐛 PASO 6: Troubleshooting

### Problema: "dotnet: command not found"

```bash
# Solución
brew install dotnet-sdk          # macOS
sudo apt-get install dotnet-sdk-8.0  # Linux
choco install dotnet-sdk         # Windows
```

### Problema: "Cannot connect to database"

```bash
# Verificar BD
pg_isready -h localhost -p 5432

# Si no existe, crear
createdb cardealer_db
psql cardealer_db < schema.sql   # Si tienes backup
```

### Problema: "Tests failing with timeout"

```bash
# Aumentar timeout
dotnet test --logger "console;verbosity=normal" \
            --configuration Debug \
            --no-build
```

### Problema: "Migration not applied"

```bash
# Reset BD (cuidado en producción)
dotnet ef database drop --force
dotnet ef database update

# O manualmente
psql cardealer_db < migration.sql
```

### Problema: "Compilation errors after pull"

```bash
# Clean y rebuild
dotnet clean
rm -rf bin obj
dotnet restore
dotnet build
```

---

## 🎯 PASO 7: Code Review Checklist

Antes de hacer merge, verifica:

```bash
# 1. Archivos cambiados
git diff --name-only

# Esperado:
# UserService/UserService.Domain/Entities/SellerProfile.cs
# UserService/UserService.Application/DTOs/SellerDtos.cs
# UserService/UserService.Application/Validators/SellerProfileValidators.cs
# UserService/UserService.Application/UseCases/Sellers/CreateSellerProfileCommand.cs
# UserService/UserService.Application/UseCases/Sellers/UpdateSellerProfileCommand.cs
# UserService/UserService.Application/UseCases/Sellers/GetSellerProfileQuery.cs
# UserService/UserService.Tests/UseCases/Sellers/SellerProfileSpecialtiesTests.cs
# UserService/Migrations/20260224_AddSpecialtiesFieldToSellerProfile.sql

# 2. Líneas agregadas
git diff --stat

# Esperado: ~50-60 líneas de código

# 3. Sin archivos accidentales
git status

# Esperado: Solo archivos esperados, sin build artifacts
```

---

## 🚀 PASO 8: Deployment a Staging

```bash
# 1. Compilar imagen Docker
docker build -f UserService/Dockerfile -t userservice:fase-1 .

# 2. Tag para registry
docker tag userservice:fase-1 ghcr.io/gregorymorenoiem/userservice:fase-1

# 3. Push a registry
docker push ghcr.io/gregorymorenoiem/userservice:fase-1

# 4. Deploy a K8s staging
kubectl set image deployment/userservice \
  userservice=ghcr.io/gregorymorenoiem/userservice:fase-1 \
  -n staging

# 5. Verificar
kubectl rollout status deployment/userservice -n staging
kubectl logs -n staging -l app=userservice --tail=50

# 6. Probar en staging
curl -X GET https://staging-api.okla.local/api/sellers/{id} \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## ✅ Confirmación de Éxito

Si ves esto, ¡**FASE 1 está lista para producción**! ✨

```
✅ Compilación: SIN ERRORES
✅ Tests: 11/11 PASANDO
✅ Migration: APLICADA
✅ API: RESPONDIENDO CORRECTAMENTE
✅ Especialidades: PERSISTIDAS EN BD
✅ Índice: CREADO Y OPTIMIZADO
✅ Backward Compatibility: VERIFICADA
✅ Documentación: COMPLETA

ESTADO: 🟢 LISTO PARA MERGE → STAGING → PRODUCCIÓN
```

---

## 📞 Si algo falla...

1. **Revisar logs:**

   ```bash
   dotnet run --configuration Debug
   # Busca stack trace
   ```

2. **Ejecutar con verbose:**

   ```bash
   dotnet test -v diagnostic
   ```

3. **Hacer rollback:**

   ```bash
   git revert HEAD
   # O si no está commiteado
   git checkout -- .
   ```

4. **Contactar al equipo con:**
   - Error exacto
   - Output completo
   - Versión de .NET
   - OS (macOS/Linux/Windows)
   - Steps para reproducir

---

**¡Listo para empezar!** 🎯

Si completaste todos los pasos sin errores, procede con:

1. ✅ Git commit
2. ✅ Code review
3. ✅ Merge a main
4. ✅ CI/CD automático de GitHub Actions
5. ✅ Deployment a producción
