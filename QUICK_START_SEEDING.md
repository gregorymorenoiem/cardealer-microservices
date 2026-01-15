# ⚡ QUICK START - DATA SEEDING

**TL;DR:** Copia y pega para llenar tu BD en 2 minutos

---

## 🔥 3 PASOS

```bash
# 1. Clonar y entrar al directorio
cd cardealer-microservices

# 2. Ejecutar seeding (elige UNO):
# Opción A (macOS/Linux):
bash _Scripts/seed-local.sh

# Opción B (Windows):
.\_Scripts\seed-local.ps1

# Opción C (C# - desde el repo):
dotnet run --project backend/VehiclesSaleService/VehiclesSaleService.Api
```

## ✅ QUÉ SE GENERA

```
20 Usuarios (10 buyers + 10 sellers)
30 Dealers (mix de tipos)
150 Vehículos (Toyota, Honda, Nissan, etc.)
7,500 Imágenes (50 por vehículo)
```

## 📊 VALIDAR

```bash
# Contar registros
psql -h localhost -U postgres -d cardealer -c \
  "SELECT 'vehicles: '||COUNT(*) FROM vehicles; SELECT 'dealers: '||COUNT(*) FROM dealers;"

# Listar vehículos en API
curl http://localhost:18443/api/vehicles | jq '.data | length'
```

## 🗑️ LIMPIAR

```bash
psql -h localhost -U postgres -d cardealer -f _Scripts/clean-db.sql
```

---

## 📚 DOCUMENTACIÓN COMPLETA

- **Plan Estratégico:** `docs/DATA_SEEDING_STRATEGY.md`
- **Guía de Uso:** `docs/SEEDING_USAGE_GUIDE.md`
- **Resumen:** `DATA_SEEDING_IMPLEMENTATION_SUMMARY.md`

---

**¡Listo! Tu BD está llena de datos.** 🎉
