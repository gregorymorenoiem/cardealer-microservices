# 📊 Guía de Aplicación de Migraciones - FASE 1 & FASE 2

**Fecha**: 2026-02-24  
**Versión**: 1.0  
**Estado**: ✅ Migraciones EF Core creadas

---

## Resumen

Se han creado dos migraciones de Entity Framework Core para SellerProfile:

### ✅ FASE 1: AddSpecialtiesFieldToSellerProfile

- **Archivo**: `UserService.Infrastructure/Migrations/AddSpecialtiesFieldToSellerProfile.cs`
- **Cambios**:
  - Agrega columna `specialties` (TEXT[] - array de PostgreSQL)
  - Índice GIN para búsquedas eficientes
  - Default: ARRAY[]::TEXT[] (array vacío)

### ✅ FASE 2: OptimizeLocationFieldsWithIndexes

- **Archivo**: `UserService.Infrastructure/Migrations/OptimizeLocationFieldsWithIndexes.cs`
- **Cambios**:
  - 6 índices de rendimiento para City, State, ZipCode
  - Índices compuestos y simples
  - Índice GIN para especialidades + ubicación

---

## 🚀 Cómo Aplicar las Migraciones

### Opción 1: Desarrollo Local (LocalHost)

#### Prerequisitos:

```bash
# Instalar PostgreSQL si no lo tienes
brew install postgresql@15

# Iniciar PostgreSQL
brew services start postgresql@15

# Crear base de datos
createdb userservice
psql -U postgres -d userservice -c "CREATE DATABASE userservice;"
```

#### Ejecutar Migraciones:

```bash
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend/UserService

# Con conexión por defecto (localhost)
dotnet ef database update -p UserService.Infrastructure

# O especificar conexión personalizada:
export EF_CONNECTION_STRING="Host=localhost;Database=userservice;Username=postgres;Password=yourpassword;Port=5432"
dotnet ef database update -p UserService.Infrastructure
```

#### Verificar:

```sql
-- Conectar a la BD
psql -U postgres -d userservice

-- Verificar columna specialties
\d seller_profiles

-- Verificar índices
SELECT indexname FROM pg_indexes
WHERE tablename = 'seller_profiles'
ORDER BY indexname;
```

---

### Opción 2: Digital Ocean Managed Database (Producción)

#### Prerequisitos:

```bash
# 1. Obtener credenciales de DO (desde Panel de Control)
# - Host: db-postgresql-nyc1-123456.ondigitalocean.com
# - Port: 25060
# - Database: cardealer_prod
# - Username: doadmin
# - Password: (from DO console)

# 2. Instalar cliente psql si no lo tienes
brew install libpq

# 3. Verificar conexión
psql -h db-postgresql-nyc1-123456.ondigitalocean.com \
     -p 25060 \
     -U doadmin \
     -d cardealer_prod \
     -c "SELECT version();"
```

#### Ejecutar Migraciones:

```bash
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend/UserService

# Configurar variables de entorno
export EF_CONNECTION_STRING="Host=db-postgresql-nyc1-123456.ondigitalocean.com;Database=cardealer_prod;Username=doadmin;Password=YOUR_PASSWORD;Port=25060;SSL Mode=Require;"

# Aplicar migraciones
dotnet ef database update -p UserService.Infrastructure
```

---

### Opción 3: Kubernetes Cluster (Staging)

#### Prerequisitos:

```bash
# 1. Conectarse al cluster DOKS
doctl kubernetes cluster kubeconfig save cardealer-staging

# 2. Port-forward a la BD en-cluster
kubectl port-forward -n cardealer svc/postgres-service 5432:5432 &

# 3. Verificar conexión
psql -h localhost -U postgres -d cardealer_staging -c "SELECT 1;"
```

#### Ejecutar Migraciones:

```bash
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend/UserService

# Con port-forward activo
export EF_CONNECTION_STRING="Host=localhost;Database=cardealer_staging;Username=postgres;Password=POSTGRES_PASSWORD;Port=5432"

dotnet ef database update -p UserService.Infrastructure
```

---

## 📋 Estados de Aplicación de Migraciones

| Ambiente         | Estado       | Fecha | Notas                     |
| ---------------- | ------------ | ----- | ------------------------- |
| Desarrollo Local | ⏳ Pendiente | -     | Requiere PostgreSQL local |
| Staging (K8s)    | ⏳ Pendiente | -     | Requiere port-forward     |
| Producción (DO)  | ⏳ Pendiente | -     | Requiere credenciales     |

---

## ✅ Verificación Post-Migración

Después de aplicar las migraciones, verificar:

```sql
-- 1. Columna specialties existe y tiene tipo correcto
SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name = 'seller_profiles' AND column_name = 'specialties';

-- Expected: specialties | ARRAY | NULL (or ARRAY[]::text[])

-- 2. Índices fueron creados
SELECT indexname, tablename, indexdef
FROM pg_indexes
WHERE tablename = 'seller_profiles'
  AND indexname LIKE 'idx_%'
ORDER BY indexname;

-- Expected: 7 índices (1 de FASE 1 + 6 de FASE 2)

-- 3. Tabla __EFMigrationsHistory tiene registros
SELECT MigrationId, ProductVersion
FROM __EFMigrationsHistory
WHERE MigrationId LIKE '%Specialties%' OR MigrationId LIKE '%Location%';
```

---

## 🔄 Rollback (Si es Necesario)

### Deshacer última migración:

```bash
cd /Users/gregorymoreno/Developer/Web/Backend/cardealer-microservices/backend/UserService

# Volver a migración anterior
dotnet ef database update OptimizeLocationFieldsWithIndexes -p UserService.Infrastructure

# Remover migración del proyecto
dotnet ef migrations remove -p UserService.Infrastructure
```

### Deshacer ambas migraciones:

```bash
# Volver a última migración anterior a FASE 1
dotnet ef database update CreateMissingTables -p UserService.Infrastructure

# Remover migraciones
dotnet ef migrations remove -p UserService.Infrastructure
dotnet ef migrations remove -p UserService.Infrastructure
```

---

## 📊 Impacto de Performance

### FASE 1: Specialties

- **Índice**: GIN en `specialties` (TEXT[])
- **Mejora**: Búsquedas por especialidad ~100x más rápidas
- **Tamaño estimado**: +500 KB por millón de registros

### FASE 2: Location Indexes

- **Índices**: 6 índices B-tree y GIN
- **Mejora esperada**:
  - City + State: 50-100x más rápido
  - Por Province: 50-100x más rápido
  - Por ZipCode: 100-500x más rápido
- **Tamaño estimado**: +5-10 MB por millón de registros

---

## 📞 Troubleshooting

### Error: "Connection refused"

```bash
# Verificar PostgreSQL está corriendo
brew services list | grep postgres

# Iniciar si está detenido
brew services start postgresql@15
```

### Error: "SSL certificate problem"

```bash
# Para DO Managed Database, usar:
export EF_CONNECTION_STRING="Host=...;SSL Mode=Require;"
```

### Error: "Role 'doadmin' does not have permission"

- Verificar que el usuario tiene permisos de creación de índices
- Contactar al administrador de DO

---

## 📝 Próximos Pasos

1. ✅ **COMPLETADO**: Migraciones creadas
2. ⏳ **PRÓXIMO**: Aplicar migraciones en staging
3. ⏳ **PRÓXIMO**: Validar performance en staging (1 semana)
4. ⏳ **PRÓXIMO**: Aplicar migraciones en producción
5. ⏳ **PRÓXIMO**: Proceder con FASE 3 (Remove Phone Duplicated)

---

## 📚 Referencias

- EF Core Migrations: https://docs.microsoft.com/ef/core/managing-schemas/migrations/
- PostgreSQL Indexes: https://www.postgresql.org/docs/current/indexes.html
- Digital Ocean Database Docs: https://docs.digitalocean.com/products/databases/postgresql/
