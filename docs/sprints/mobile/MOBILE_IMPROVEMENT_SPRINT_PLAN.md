# 📱 Mobile App Improvement Sprint Plan

## Análisis del Frontend Mobile (Flutter)

**Fecha de Auditoría:** Enero 2025  
**Versión Flutter SDK:** >=3.2.0 <4.0.0  
**Versión Dart:** 3.2.x+  
**Target Platforms:** Android (SDK 24-36), iOS 12+

---

## 📊 Resumen Ejecutivo

### Puntuación General: **8.2/10** ⭐⭐⭐⭐

| Categoría | Puntuación | Estado |
|-----------|------------|--------|
| Arquitectura | 9.0/10 | ✅ Excelente |
| State Management | 8.5/10 | ✅ Muy Bueno |
| Dependencias | 7.0/10 | ⚠️ Actualización Requerida |
| Testing | 5.5/10 | 🔴 Mejorable |
| API Integration | 4.0/10 | 🔴 Incompleta |
| UI/UX | 8.5/10 | ✅ Muy Bueno |
| Build Config | 9.0/10 | ✅ Excelente |

---

## ✅ Aspectos Positivos Identificados

### 1. **Arquitectura Clean Architecture** (9/10)
```
lib/
├── core/          # Utilidades, DI, Network, Theme
├── data/          # DataSources, Models, Repositories Impl
├── domain/        # Entities, Repository Interfaces, UseCases
└── presentation/  # BLoCs, Pages, Widgets
```
- Separación de capas excelente
- UseCases bien definidos por módulo
- Inyección de dependencias con GetIt + Injectable

### 2. **State Management con BLoC** (8.5/10)
- 12 BLoCs bien estructurados: auth, dealer, favorites, filter, map, messaging, payment, profile, search, vehicles, vehicle_detail, connectivity
- Uso correcto de Events/States con Equatable
- Pattern Result (dartz Either) para manejo de errores

### 3. **Android Build Configuration** (9/10)
```kotlin
compileSdk = 36        // ✅ Última versión
targetSdk = 36         // ✅ Última versión
minSdk = 24            // ✅ Android 7.0+ (95%+ devices)
Java 17                // ✅ Versión recomendada
Kotlin DSL             // ✅ Build moderno
```

### 4. **Material 3 Design** (8.5/10)
- Theme system completo con light/dark mode
- ColorScheme bien definido
- Typography system configurado

### 5. **Flavors Configuration** (8/10)
- 3 ambientes: dev, staging, prod
- AppConfig singleton para configuración
- Separación de entry points

---

## ⚠️ Issues Críticos Identificados

### 1. **Firebase SDK 2.x → 3.x (BREAKING CHANGE)** 🔴

| Paquete | Actual | Última | Gap |
|---------|--------|--------|-----|
| firebase_core | ^2.24.2 | 3.8.0 | ⚠️ Major |
| firebase_analytics | ^10.8.0 | 11.4.0 | ⚠️ Major |
| firebase_crashlytics | ^3.4.9 | 4.2.0 | ⚠️ Major |
| firebase_remote_config | ^4.3.8 | 5.2.0 | ⚠️ Major |
| firebase_messaging | ^14.7.10 | 15.2.0 | ⚠️ Major |

**Impacto:** Firebase 3.x requiere Dart 3.4.0+ y cambios en inicialización.

### 2. **GetIt 7.x → 8.x (BREAKING CHANGE)** 🔴

| Paquete | Actual | Última | Gap |
|---------|--------|--------|-----|
| get_it | ^7.6.4 | 8.0.2 | ⚠️ Major |
| injectable | ^2.3.2 | 2.5.0 | Minor |
| injectable_generator | ^2.4.1 | 2.6.2 | Minor |

**Impacto:** Cambios en API de registro de singletons.

### 3. **Payment SDK Desactualizado** 🟡

| Paquete | Actual | Última | Gap |
|---------|--------|--------|-----|
| flutter_stripe | ^10.1.0 | 11.4.0 | ⚠️ Major |

**Impacto:** Nuevas features PCI DSS, mejoras en 3D Secure.

### 4. **API Integration Incompleta** 🔴

```dart
// vehicle_remote_datasource.dart
class VehicleRemoteDataSource {
  Future<List<dynamic>> getHeroCarouselVehicles() async {
    throw UnimplementedError('API not ready yet');
  }
  // TODOS los métodos sin implementar
}

// api_client.dart
class ApiClient {
  // TODO: Implement Dio client with interceptors
}
```

**Impacto:** Aplicación funciona solo con datos mock.

### 5. **Testing Coverage Bajo** 🟡

```
test/
├── core/
│   ├── ab_testing/
│   ├── accessibility/
│   ├── config/
│   └── network/
└── widget_test.dart   // Solo 1 test básico
```

**Impacto:** Sin tests de BLoCs, UseCases, Repositories.

---

## 📦 Análisis Completo de Dependencias

### State Management & Architecture

| Paquete | Actual | Última | Estado | Prioridad |
|---------|--------|--------|--------|-----------|
| flutter_bloc | ^8.1.3 | 8.1.6 | ✅ Minor | 🟢 Baja |
| equatable | ^2.0.5 | 2.0.7 | ✅ Patch | 🟢 Baja |
| dartz | ^0.10.1 | 0.10.1 | ✅ OK | - |
| get_it | ^7.6.4 | 8.0.2 | 🔴 Major | 🔴 Alta |
| injectable | ^2.3.2 | 2.5.0 | 🟡 Minor | 🟡 Media |

### Networking

| Paquete | Actual | Última | Estado | Prioridad |
|---------|--------|--------|--------|-----------|
| dio | ^5.4.0 | 5.7.0 | 🟡 Minor | 🟡 Media |
| retrofit | ^4.0.3 | 4.4.1 | 🟡 Minor | 🟡 Media |
| json_annotation | ^4.8.1 | 4.9.0 | ✅ Patch | 🟢 Baja |
| pretty_dio_logger | ^1.3.1 | 1.4.0 | ✅ Minor | 🟢 Baja |

### Storage

| Paquete | Actual | Última | Estado | Prioridad |
|---------|--------|--------|--------|-----------|
| hive | ^2.2.3 | 2.2.3 | ✅ OK | - |
| hive_flutter | ^1.1.0 | 1.1.0 | ✅ OK | - |
| flutter_secure_storage | ^9.0.0 | 9.2.2 | 🟡 Patch | 🟢 Baja |
| shared_preferences | ^2.2.2 | 2.3.5 | 🟡 Minor | 🟢 Baja |

### Firebase Suite

| Paquete | Actual | Última | Estado | Prioridad |
|---------|--------|--------|--------|-----------|
| firebase_core | ^2.24.2 | 3.8.0 | 🔴 Major | 🔴 Alta |
| firebase_analytics | ^10.8.0 | 11.4.0 | 🔴 Major | 🔴 Alta |
| firebase_crashlytics | ^3.4.9 | 4.2.0 | 🔴 Major | 🔴 Alta |
| firebase_remote_config | ^4.3.8 | 5.2.0 | 🔴 Major | 🔴 Alta |
| firebase_messaging | ^14.7.10 | 15.2.0 | 🔴 Major | 🔴 Alta |

### UI Components

| Paquete | Actual | Última | Estado | Prioridad |
|---------|--------|--------|--------|-----------|
| cached_network_image | ^3.3.0 | 3.4.1 | ✅ Minor | 🟢 Baja |
| flutter_svg | ^2.0.9 | 2.0.16 | ✅ Patch | 🟢 Baja |
| shimmer | ^3.0.0 | 3.0.0 | ✅ OK | - |
| lottie | ^3.1.2 | 3.3.1 | 🟡 Minor | 🟢 Baja |
| fl_chart | ^0.68.0 | 0.70.2 | 🟡 Minor | 🟡 Media |

### Platform Features

| Paquete | Actual | Última | Estado | Prioridad |
|---------|--------|--------|--------|-----------|
| google_maps_flutter | ^2.14.0 | 2.14.0 | ✅ OK | - |
| flutter_stripe | ^10.1.0 | 11.4.0 | 🔴 Major | 🔴 Alta |
| video_player | ^2.10.1 | 2.10.1 | ✅ OK | - |
| local_auth | ^3.0.0 | 3.0.0 | ✅ OK | - |
| image_picker | ^1.2.1 | 1.2.1 | ✅ OK | - |

### Utils

| Paquete | Actual | Última | Estado | Prioridad |
|---------|--------|--------|--------|-----------|
| intl | ^0.20.2 | 0.20.2 | ✅ OK | - |
| connectivity_plus | ^5.0.2 | 6.1.4 | 🔴 Major | 🟡 Media |
| url_launcher | ^6.2.2 | 6.3.1 | 🟡 Minor | 🟢 Baja |
| permission_handler | ^11.3.0 | 11.3.1 | ✅ Patch | 🟢 Baja |
| share_plus | ^12.0.1 | 12.0.1 | ✅ OK | - |

### Dev Dependencies

| Paquete | Actual | Última | Estado | Prioridad |
|---------|--------|--------|--------|-----------|
| build_runner | ^2.4.6 | 2.4.15 | 🟡 Patch | 🟢 Baja |
| retrofit_generator | ^8.0.4 | 9.1.6 | 🔴 Major | 🟡 Media |
| json_serializable | ^6.7.1 | 6.9.4 | 🟡 Minor | 🟢 Baja |
| bloc_test | ^9.1.5 | 9.2.7 | 🟡 Minor | 🟡 Media |
| mocktail | ^1.0.1 | 1.0.4 | ✅ Patch | 🟢 Baja |
| widgetbook | ^3.8.0 | 3.10.1 | 🟡 Minor | 🟡 Media |

---

## 📋 frontend/shared Analysis

El directorio `frontend/shared` contiene **tipos TypeScript** para compartir entre web y mobile:

```
shared/
├── src/
│   ├── types/index.ts    # AccountType, User, Vehicle, etc.
│   └── index.ts
└── README.md
```

### Observaciones:
- ✅ Tipos bien definidos para AccountType, DealerPlan, User, Vehicle
- ✅ AuthStore con Zustand para web
- ⚠️ **No aplicable directamente a Flutter** - Flutter usa Dart, no TypeScript
- 💡 **Recomendación:** Crear equivalentes Dart en `lib/domain/entities/`

---

## 🎯 Sprint Plan - Mejoras Mobile

### Configuración Claude Opus 4.5

| Parámetro | Valor |
|-----------|-------|
| **Context Window** | 128,000 tokens |
| **Output Limit** | 16,000 tokens |
| **Token Multiplier** | 1x |
| **Effective Input** | ~112,000 tokens |

### Cálculo de Tokens por Tarea

| Tipo de Tarea | Tokens Input | Tokens Output | Total |
|---------------|--------------|---------------|-------|
| Actualización simple de paquete | ~3,000 | ~2,000 | ~5,000 |
| Breaking change (Firebase) | ~15,000 | ~8,000 | ~23,000 |
| Implementar API Client | ~10,000 | ~6,000 | ~16,000 |
| Crear Tests BLoC | ~8,000 | ~5,000 | ~13,000 |
| Refactoring módulo | ~12,000 | ~6,000 | ~18,000 |

---

## 🚀 Sprint 1: Firebase SDK 3.x Migration (CRITICAL)

**Duración:** 3 días  
**Prioridad:** 🔴 CRÍTICA  
**Tokens Estimados:** ~85,000

### Tareas

| # | Tarea | Tokens | Archivos |
|---|-------|--------|----------|
| 1.1 | Actualizar flutter SDK constraint a >=3.4.0 | 3,000 | pubspec.yaml |
| 1.2 | Migrar firebase_core 2.x → 3.x | 15,000 | pubspec.yaml, main.dart, injection.dart |
| 1.3 | Migrar firebase_analytics 10.x → 11.x | 12,000 | analytics files, event tracking |
| 1.4 | Migrar firebase_crashlytics 3.x → 4.x | 12,000 | crash reporting, error handlers |
| 1.5 | Migrar firebase_remote_config 4.x → 5.x | 10,000 | remote config, feature flags |
| 1.6 | Migrar firebase_messaging 14.x → 15.x | 18,000 | push notifications, handlers |
| 1.7 | Actualizar Firebase Android config | 8,000 | android/build.gradle, google-services |
| 1.8 | Actualizar Firebase iOS config | 7,000 | ios/Podfile, GoogleService-Info.plist |

### Breaking Changes Firebase 3.x
```dart
// ANTES (2.x)
await Firebase.initializeApp();

// DESPUÉS (3.x)
await Firebase.initializeApp(
  options: DefaultFirebaseOptions.currentPlatform,
);
```

**Sesiones Claude:** 5-6 sesiones

---

## 🚀 Sprint 2: DI & State Management Updates

**Duración:** 2 días  
**Prioridad:** 🔴 Alta  
**Tokens Estimados:** ~65,000

### Tareas

| # | Tarea | Tokens | Archivos |
|---|-------|--------|----------|
| 2.1 | Migrar get_it 7.x → 8.x | 18,000 | injection.dart, todos los registros |
| 2.2 | Actualizar injectable 2.3 → 2.5 | 8,000 | annotations, generators |
| 2.3 | Regenerar código con build_runner | 5,000 | *.g.dart files |
| 2.4 | Actualizar flutter_bloc 8.1.3 → 8.1.6 | 6,000 | Todos los BLoCs |
| 2.5 | Actualizar equatable 2.0.5 → 2.0.7 | 4,000 | States, Events, Entities |
| 2.6 | Actualizar bloc_test 9.1.5 → 9.2.7 | 6,000 | Test files |
| 2.7 | Validar inyección de dependencias | 10,000 | Integration tests |
| 2.8 | Actualizar injectable_generator | 8,000 | Dev dependency, rebuild |

### Breaking Changes GetIt 8.x
```dart
// ANTES (7.x)
getIt.registerLazySingleton<Service>(() => ServiceImpl());

// DESPUÉS (8.x) - Mismo API pero mejor type safety
getIt.registerLazySingleton<Service>(() => ServiceImpl());
// Nuevo: dispose callbacks mejorados
```

**Sesiones Claude:** 4-5 sesiones

---

## 🚀 Sprint 3: Networking & API Implementation

**Duración:** 4 días  
**Prioridad:** 🔴 CRÍTICA  
**Tokens Estimados:** ~95,000

### Tareas

| # | Tarea | Tokens | Archivos |
|---|-------|--------|----------|
| 3.1 | Actualizar dio 5.4 → 5.7 | 8,000 | pubspec.yaml, api_client |
| 3.2 | Implementar ApiClient con Dio | 16,000 | api_client.dart (nuevo) |
| 3.3 | Agregar interceptores (auth, logging, retry) | 14,000 | interceptors/*.dart |
| 3.4 | Actualizar retrofit 4.0 → 4.4 | 6,000 | retrofit clients |
| 3.5 | Actualizar retrofit_generator 8.x → 9.x | 8,000 | *.g.dart regeneration |
| 3.6 | Implementar VehicleRemoteDataSource | 18,000 | vehicle_remote_datasource.dart |
| 3.7 | Implementar AuthRemoteDataSource | 15,000 | auth_remote_datasource.dart |
| 3.8 | Agregar NetworkInfo con connectivity_plus 6.x | 10,000 | network_info.dart, connectivity |

### Código a Implementar
```dart
// api_client.dart
class ApiClient {
  late final Dio _dio;
  
  ApiClient({required AppConfig config}) {
    _dio = Dio(BaseOptions(
      baseUrl: config.apiBaseUrl,
      connectTimeout: const Duration(seconds: 30),
      receiveTimeout: const Duration(seconds: 30),
    ));
    
    _dio.interceptors.addAll([
      AuthInterceptor(),
      LoggingInterceptor(),
      RetryInterceptor(),
    ]);
  }
}
```

**Sesiones Claude:** 6-7 sesiones

---

## 🚀 Sprint 4: Payment & Maps Updates

**Duración:** 2 días  
**Prioridad:** 🟡 Media  
**Tokens Estimados:** ~48,000

### Tareas

| # | Tarea | Tokens | Archivos |
|---|-------|--------|----------|
| 4.1 | Migrar flutter_stripe 10.x → 11.x | 18,000 | payment_bloc, stripe_service |
| 4.2 | Actualizar configuración Stripe Android | 8,000 | android/build.gradle |
| 4.3 | Actualizar configuración Stripe iOS | 6,000 | ios/Podfile |
| 4.4 | Verificar google_maps_flutter (ya OK) | 4,000 | map files |
| 4.5 | Actualizar fl_chart 0.68 → 0.70 | 8,000 | chart widgets |
| 4.6 | Test payment flow end-to-end | 4,000 | Payment tests |

### Breaking Changes Stripe 11.x
```dart
// Nuevas APIs PCI DSS compliant
// Mejoras en CardField widget
// Soporte mejorado para 3D Secure 2
```

**Sesiones Claude:** 3-4 sesiones

---

## 🚀 Sprint 5: Testing Infrastructure

**Duración:** 4 días  
**Prioridad:** 🟡 Media  
**Tokens Estimados:** ~90,000

### Tareas

| # | Tarea | Tokens | Archivos |
|---|-------|--------|----------|
| 5.1 | Crear tests para VehiclesBloc | 12,000 | vehicles_bloc_test.dart |
| 5.2 | Crear tests para AuthBloc | 14,000 | auth_bloc_test.dart |
| 5.3 | Crear tests para SearchBloc | 10,000 | search_bloc_test.dart |
| 5.4 | Crear tests para PaymentBloc | 12,000 | payment_bloc_test.dart |
| 5.5 | Crear tests para UseCases | 16,000 | usecases/*.test.dart |
| 5.6 | Crear tests para Repositories | 14,000 | repositories/*.test.dart |
| 5.7 | Configurar test coverage reporting | 6,000 | test scripts, CI config |
| 5.8 | Actualizar mocktail 1.0.1 → 1.0.4 | 6,000 | mock files |

### Estructura de Tests Propuesta
```
test/
├── core/
├── data/
│   ├── datasources/
│   └── repositories/
├── domain/
│   └── usecases/
├── presentation/
│   ├── bloc/
│   │   ├── auth_bloc_test.dart
│   │   ├── vehicles_bloc_test.dart
│   │   └── ...
│   └── pages/
└── helpers/
    ├── mocks.dart
    └── test_fixtures.dart
```

**Sesiones Claude:** 6-7 sesiones

---

## 🚀 Sprint 6: UI & Utility Updates

**Duración:** 2 días  
**Prioridad:** 🟢 Baja  
**Tokens Estimados:** ~42,000

### Tareas

| # | Tarea | Tokens | Archivos |
|---|-------|--------|----------|
| 6.1 | Actualizar cached_network_image 3.3 → 3.4 | 5,000 | Image widgets |
| 6.2 | Actualizar lottie 3.1 → 3.3 | 5,000 | Animation files |
| 6.3 | Actualizar flutter_svg 2.0.9 → 2.0.16 | 4,000 | SVG components |
| 6.4 | Actualizar url_launcher 6.2 → 6.3 | 4,000 | URL launching |
| 6.5 | Actualizar shared_preferences 2.2 → 2.3 | 5,000 | Storage |
| 6.6 | Actualizar flutter_secure_storage 9.0 → 9.2 | 6,000 | Secure storage |
| 6.7 | Actualizar intl dependencies | 4,000 | Localization |
| 6.8 | Actualizar widgetbook 3.8 → 3.10 | 9,000 | UI catalog |

**Sesiones Claude:** 3-4 sesiones

---

## 🚀 Sprint 7: DevTools & Build Optimization

**Duración:** 2 días  
**Prioridad:** 🟢 Baja  
**Tokens Estimados:** ~38,000

### Tareas

| # | Tarea | Tokens | Archivos |
|---|-------|--------|----------|
| 7.1 | Actualizar build_runner 2.4.6 → 2.4.15 | 5,000 | Dev dependency |
| 7.2 | Actualizar json_serializable 6.7 → 6.9 | 6,000 | Models, generators |
| 7.3 | Optimizar build_runner configuration | 8,000 | build.yaml |
| 7.4 | Configurar flavor builds para CI/CD | 10,000 | Fastlane, GitHub Actions |
| 7.5 | Actualizar flutter_launcher_icons 0.13 → 0.14 | 4,000 | Icon generation |
| 7.6 | Optimizar ProGuard rules | 5,000 | proguard-rules.pro |

**Sesiones Claude:** 2-3 sesiones

---

## 📊 Resumen Total de Tokens

| Sprint | Descripción | Tokens | Sesiones |
|--------|-------------|--------|----------|
| 1 | Firebase SDK 3.x Migration | ~85,000 | 5-6 |
| 2 | DI & State Management | ~65,000 | 4-5 |
| 3 | Networking & API Implementation | ~95,000 | 6-7 |
| 4 | Payment & Maps Updates | ~48,000 | 3-4 |
| 5 | Testing Infrastructure | ~90,000 | 6-7 |
| 6 | UI & Utility Updates | ~42,000 | 3-4 |
| 7 | DevTools & Build Optimization | ~38,000 | 2-3 |

### **Total Estimado: ~463,000 tokens | 25-30 sesiones Claude Opus 4.5**

---

## 🔄 pubspec.yaml Actualizado (Target)

```yaml
environment:
  sdk: '>=3.4.0 <4.0.0'  # Requerido para Firebase 3.x

dependencies:
  flutter:
    sdk: flutter
  
  # State Management
  flutter_bloc: ^8.1.6
  equatable: ^2.0.7
  dartz: ^0.10.1
  
  # Dependency Injection
  get_it: ^8.0.2
  injectable: ^2.5.0
  
  # Network
  dio: ^5.7.0
  retrofit: ^4.4.1
  json_annotation: ^4.9.0
  pretty_dio_logger: ^1.4.0
  
  # Local Storage
  hive: ^2.2.3
  hive_flutter: ^1.1.0
  flutter_secure_storage: ^9.2.2
  shared_preferences: ^2.3.5
  
  # Firebase
  firebase_core: ^3.8.0
  firebase_analytics: ^11.4.0
  firebase_crashlytics: ^4.2.0
  firebase_remote_config: ^5.2.0
  firebase_messaging: ^15.2.0
  
  # Payment
  flutter_stripe: ^11.4.0
  
  # UI
  cached_network_image: ^3.4.1
  flutter_svg: ^2.0.16
  lottie: ^3.3.1
  fl_chart: ^0.70.2
  
  # Utils
  connectivity_plus: ^6.1.4
  url_launcher: ^6.3.1
  
dev_dependencies:
  build_runner: ^2.4.15
  injectable_generator: ^2.6.2
  retrofit_generator: ^9.1.6
  json_serializable: ^6.9.4
  bloc_test: ^9.2.7
  mocktail: ^1.0.4
  widgetbook: ^3.10.1
```

---

## ⚡ Quick Wins (Inmediato)

Estas actualizaciones son patch/minor y pueden hacerse sin riesgo:

```yaml
# Cambios seguros sin breaking changes
flutter_bloc: ^8.1.6           # 8.1.3 → 8.1.6
equatable: ^2.0.7              # 2.0.5 → 2.0.7
cached_network_image: ^3.4.1   # 3.3.0 → 3.4.1
flutter_svg: ^2.0.16           # 2.0.9 → 2.0.16
lottie: ^3.3.1                 # 3.1.2 → 3.3.1
url_launcher: ^6.3.1           # 6.2.2 → 6.3.1
mocktail: ^1.0.4               # 1.0.1 → 1.0.4
```

**Tokens Estimados Quick Wins:** ~12,000 | **1 sesión**

---

## 📅 Cronograma Sugerido

| Semana | Sprint | Foco |
|--------|--------|------|
| 1 | Sprint 1 | Firebase 3.x (CRÍTICO) |
| 2 | Sprint 2 | GetIt 8.x + State |
| 2-3 | Sprint 3 | API Implementation |
| 3 | Sprint 4 | Payment + Maps |
| 4 | Sprint 5 | Testing |
| 4-5 | Sprint 6-7 | UI + DevTools |

**Duración Total Estimada:** 4-5 semanas

---

## 🎯 Prioridad de Ejecución

1. **🔴 CRÍTICO (Sprint 1-3):** Firebase + GetIt + API = Sin esto la app no puede ir a producción
2. **🟡 IMPORTANTE (Sprint 4-5):** Payments + Testing = Monetización y calidad
3. **🟢 MEJORA (Sprint 6-7):** UI polish + DevTools = Mantenibilidad

---

## 📝 Notas Finales

### Fortalezas del Proyecto Mobile
- ✅ Arquitectura Clean Architecture bien implementada
- ✅ BLoC pattern correcto con eventos/estados
- ✅ Build configuration Android moderna (SDK 36)
- ✅ Material 3 design system
- ✅ Flavors para múltiples ambientes

### Áreas de Mejora
- 🔴 Completar integración con API backend
- 🔴 Actualizar Firebase SDK (breaking change)
- 🟡 Incrementar cobertura de tests
- 🟡 Actualizar DI framework
- 🟢 Mantener dependencias actualizadas

### Riesgos
| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|--------------|---------|------------|
| Firebase 3.x breaking changes | Alta | Alto | Migración gradual, testing extensivo |
| GetIt 8.x incompatibilidades | Media | Medio | Revisar changelog, tests unitarios |
| API backend no lista | Alta | Alto | Mantener mocks, implementar adaptadores |

---

*Documento generado: Enero 2025*
*Versión: 1.0*
