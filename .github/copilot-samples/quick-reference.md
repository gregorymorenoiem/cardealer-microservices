# 🚀 Quick Reference - CarDealer Microservices

## Comandos Frecuentes

### Backend (.NET)

```powershell
# Restaurar dependencias
cd backend
dotnet restore

# Build completo
dotnet build CarDealer.sln

# Build un servicio específico
dotnet build AuthService/AuthService.sln

# Ejecutar un servicio
dotnet run --project AuthService/AuthService.Api

# Ejecutar con watch (hot reload)
dotnet watch --project AuthService/AuthService.Api run

# Ejecutar tests
dotnet test

# Tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Crear migración EF
dotnet ef migrations add [MigrationName] -p AuthService.Infrastructure -s AuthService.Api

# Aplicar migración
dotnet ef database update -p AuthService.Infrastructure -s AuthService.Api

# Limpiar solución
dotnet clean
```

### Frontend Web (React)

```powershell
cd frontend/web

# Instalar dependencias
npm install

# Desarrollo
npm run dev

# Build producción
npm run build

# Preview build
npm run preview

# Lint
npm run lint

# Tests
npm run test

# Tests con UI
npm run test:ui

# Cobertura
npm run test:coverage
```

### Frontend Mobile (Flutter)

```powershell
cd frontend/mobile/cardealer

# Dependencias
flutter pub get

# Ejecutar en emulador
flutter run

# Ejecutar con flavor
flutter run --flavor dev -t lib/main_dev.dart
flutter run --flavor staging -t lib/main_staging.dart
flutter run --flavor prod -t lib/main_prod.dart

# Build APK
flutter build apk --release

# Build iOS
flutter build ios --release

# Tests
flutter test

# Cobertura
flutter test --coverage

# Generar iconos
flutter pub run flutter_launcher_icons

# Limpiar
flutter clean
```

### Docker

```powershell
# Levantar todo
docker-compose up -d

# Solo infraestructura (DB, Redis, RabbitMQ)
docker-compose up -d postgres redis rabbitmq consul

# Levantar servicio específico
docker-compose up -d authservice

# Ver logs
docker logs -f authservice

# Logs de múltiples servicios
docker-compose logs -f authservice errorservice gateway

# Rebuild imagen
docker-compose build authservice

# Rebuild y levantar
docker-compose up -d --build authservice

# Bajar todo
docker-compose down

# Bajar con volúmenes (BORRA DATOS)
docker-compose down -v

# Ver estado
docker-compose ps

# Entrar a contenedor
docker exec -it authservice sh

# Ver uso de recursos
docker stats
```

---

## 📍 Endpoints Principales

### Gateway (Puerto 8080)

| Servicio | Endpoint Base | Swagger |
|----------|---------------|---------|
| Auth | `/api/auth` | `/auth-service/swagger` |
| Users | `/api/users` | `/user-service/swagger` |
| Roles | `/api/roles` | `/role-service/swagger` |
| Products | `/api/products` | `/product-service/swagger` |
| Media | `/api/media` | `/media-service/swagger` |
| Errors | `/api/errors` | `/error-service/swagger` |
| Notifications | `/api/notifications` | `/notification-service/swagger` |

### Puertos Directos (Development)

| Servicio | Puerto | Health |
|----------|--------|--------|
| Gateway | 18443 | `/health` |
| AuthService | 15085 | `/health` |
| UserService | 15100 | `/health` |
| RoleService | 15101 | `/health` |
| VehiclesSaleService | 15070 | `/health` |
| VehiclesRentService | 15071 | `/health` |
| PropertiesSaleService | 15072 | `/health` |
| PropertiesRentService | 15073 | `/health` |
| ErrorService | 15083 | `/health` |
| NotificationService | 15084 | `/health` |
| MediaService | 15090 | `/health` |

### Infraestructura

| Servicio | Puerto | UI |
|----------|--------|-----|
| PostgreSQL | 5432 | - |
| Redis | 6379 | - |
| RabbitMQ | 5672 | 15672 |
| Consul | 8500 | 8500 |
| Prometheus | 9090 | 9090 |
| Grafana | 3000 | 3000 |
| Jaeger | 16686 | 16686 |

---

## 🔐 Auth Endpoints

```http
# Registro
POST /api/auth/register
Content-Type: application/json
{
  "email": "user@example.com",
  "password": "Password123!",
  "fullName": "John Doe"
}

# Login
POST /api/auth/login
Content-Type: application/json
{
  "email": "user@example.com",
  "password": "Password123!"
}

# Refresh Token
POST /api/auth/refresh
Content-Type: application/json
{
  "refreshToken": "..."
}

# Logout
POST /api/auth/logout
Authorization: Bearer {token}

# Get Current User
GET /api/auth/me
Authorization: Bearer {token}

# Change Password
PUT /api/auth/password
Authorization: Bearer {token}
{
  "currentPassword": "...",
  "newPassword": "..."
}

# Enable 2FA
POST /api/auth/2fa/enable
Authorization: Bearer {token}

# Verify 2FA
POST /api/auth/2fa/verify
Authorization: Bearer {token}
{
  "code": "123456"
}
```

---

## 📦 Product Endpoints

```http
# Listar productos (paginado)
GET /api/products?page=1&pageSize=10&search=toyota

# Obtener producto
GET /api/products/{id}

# Crear producto
POST /api/products
Authorization: Bearer {token}
Content-Type: application/json
{
  "name": "Toyota Camry 2023",
  "description": "Excellent condition",
  "price": 25000,
  "categoryId": "...",
  "customFieldsJson": "{\"make\":\"Toyota\",\"model\":\"Camry\",\"year\":2023}"
}

# Actualizar producto
PUT /api/products/{id}
Authorization: Bearer {token}

# Eliminar producto
DELETE /api/products/{id}
Authorization: Bearer {token}

# Subir imágenes
POST /api/products/{id}/images
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

---

## 🔧 Variables de Entorno

### Backend

```env
# General
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:80

# Database
ConnectionStrings__DefaultConnection=Host=localhost;Database=authservice;Username=postgres;Password=password

# Redis
ConnectionStrings__Redis=localhost:6379

# RabbitMQ
RabbitMQ__Host=localhost
RabbitMQ__Port=5672
RabbitMQ__Username=guest
RabbitMQ__Password=guest

# JWT
Jwt__SecretKey=your-super-secret-key-min-32-chars
Jwt__Issuer=CarDealer
Jwt__Audience=CarDealerApp
Jwt__ExpirationMinutes=60

# OpenTelemetry
OpenTelemetry__Exporter__Otlp__Endpoint=http://localhost:4317

# Consul
Consul__Address=http://localhost:8500
```

### Frontend Web

```env
VITE_API_URL=http://localhost:8080/api
VITE_APP_NAME=CarDealer
VITE_GOOGLE_MAPS_KEY=your-key
```

### Frontend Mobile

```dart
// lib/app_config.dart
class AppConfig {
  static const String apiUrl = 'http://10.0.2.2:8080/api'; // Android emulator
  // static const String apiUrl = 'http://localhost:8080/api'; // iOS simulator
}
```

---

## 📊 Health Check All Services

```powershell
# Script para verificar todos los servicios
$services = @(
    @{Name="Gateway"; Url="http://localhost:8080/health"},
    @{Name="AuthService"; Url="http://localhost:15085/health"},
    @{Name="UserService"; Url="http://localhost:15086/health"},
    @{Name="ErrorService"; Url="http://localhost:15083/health"}
)

foreach ($service in $services) {
    try {
        $response = Invoke-RestMethod -Uri $service.Url -TimeoutSec 5
        Write-Host "✅ $($service.Name): OK" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ $($service.Name): FAILED" -ForegroundColor Red
    }
}
```

---

## 🐛 Debug Tips

### Ver logs de un servicio

```powershell
# Docker
docker logs -f authservice --tail 100

# dotnet
dotnet run --project AuthService/AuthService.Api | Select-String -Pattern "error|exception"
```

### Conectar a PostgreSQL

```powershell
# Via Docker
docker exec -it authservice-db psql -U postgres -d authservice

# Comandos útiles
\dt          # Listar tablas
\d+ users    # Describir tabla
SELECT * FROM "Users" LIMIT 10;
```

### Conectar a Redis

```powershell
docker exec -it redis redis-cli

# Comandos útiles
KEYS *
GET key_name
FLUSHALL    # Limpiar todo (dev only!)
```

### RabbitMQ Management

1. Abrir http://localhost:15672
2. Login: guest/guest
3. Ver queues, exchanges, connections

---

## 📝 Checklist Nuevo Feature

- [ ] Crear Branch: `feature/nombre-feature`
- [ ] Definir DTOs en `.Application/DTOs`
- [ ] Crear Command/Query en `.Application/Features`
- [ ] Crear Validator
- [ ] Crear Handler
- [ ] Actualizar Controller
- [ ] Agregar tests unitarios
- [ ] Agregar tests de integración
- [ ] Verificar Swagger docs
- [ ] Actualizar README si es necesario
- [ ] PR con descripción clara

---

## 🔗 Links Útiles

- [Swagger UI](http://localhost:8080/swagger)
- [RabbitMQ Management](http://localhost:15672)
- [Consul UI](http://localhost:8500)
- [Grafana](http://localhost:3000)
- [Jaeger Tracing](http://localhost:16686)
- [Prometheus](http://localhost:9090)
