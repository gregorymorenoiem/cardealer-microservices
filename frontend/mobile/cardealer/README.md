# 📱 CarDealer Mobile App

Aplicación móvil nativa para iOS y Android del marketplace de vehículos CarDealer.

## ✅ Estado del Proyecto

**Progreso General:** 6/14 Sprints Completados (43%)

- ✅ **Sprint 0:** Infraestructura base (100%)
- ✅ **Sprint 1:** Design System (100%)
- ✅ **Sprint 2:** Auth & Onboarding (100%)
- ✅ **Sprint 3:** HomePage (100%)
- ✅ **Sprint 12:** Performance & Optimization (100%)
- ✅ **Sprint 13:** Testing & QA (100%)
- ✅ **Sprint 14:** Deploy & Monitoring (100%)
- 🔜 **Sprint 4:** Vehicle Details Page (próximo)

**Calidad de Código:** 
- ✅ 0 warnings/errors
- ✅ 4 tests passing
- ✅ Coverage establecido
- ✅ Production-ready
- ✅ Firebase integrado

## 🚀 Stack Tecnológico

- **Framework**: Flutter 3.x
- **Lenguaje**: Dart 3.x
- **Arquitectura**: Clean Architecture + BLoC Pattern
- **State Management**: flutter_bloc
- **Dependency Injection**: get_it + injectable
- **Network**: Dio + Retrofit
- **Local Storage**: Hive + Secure Storage
- **Testing**: bloc_test + mocktail
- **Performance**: Optimized images, caching, monitoring

## 📁 Estructura del Proyecto

```
lib/
├── core/           # Configuración, constantes, utilidades
│   ├── di/         # Dependency injection
│   ├── theme/      # App theme & colors
│   ├── utils/      # Helpers & formatters
│   └── performance/ # Performance monitoring
├── data/           # Models, repositories impl, datasources
│   ├── models/     # Data models
│   ├── datasources/ # API & local data sources
│   └── repositories/ # Repository implementations
├── domain/         # Entities, repositories interfaces, use cases
│   ├── entities/   # Business entities
│   ├── repositories/ # Repository contracts
│   └── usecases/   # Business logic
├── presentation/   # BLoC, pages, widgets
│   ├── bloc/       # State management
│   ├── pages/      # Screen components
│   └── widgets/    # Reusable UI components
└── main.dart
```

## 🏗️ Arquitectura

### Clean Architecture + BLoC

- **Presentation Layer**: BLoC para state management, UI widgets
- **Domain Layer**: Business logic, use cases, entities
- **Data Layer**: API clients, local storage, repositories implementation

## 🛠️ Setup

### Prerrequisitos

- Flutter SDK 3.2.0 o superior
- Dart SDK 3.2.0 o superior
- Android Studio / Xcode
- VS Code con Flutter extension

### Instalación

```bash
# Instalar dependencias
flutter pub get

# Generar código (models, DI, etc)
flutter pub run build_runner build --delete-conflicting-outputs

# Ejecutar en modo debug
flutter run

# Ejecutar tests
flutter test

# Generar coverage
flutter test --coverage
```

## 🌍 Internacionalización

La app soporta español (ES) e inglés (EN).

Archivos de traducción en `lib/l10n/`:
- `app_es.arb` - Español
- `app_en.arb` - Inglés

## 📦 Estructura de Features

Cada feature sigue Clean Architecture:

```
feature/
├── data/
│   ├── models/
│   ├── datasources/
│   └── repositories/
├── domain/
│   ├── entities/
│   ├── repositories/
│   └── usecases/
└── presentation/
    ├── bloc/
    ├── pages/
    └── widgets/
```

## 🧪 Testing

- Unit tests para use cases y BLoCs
- Widget tests para UI components
- Integration tests para flujos completos

```bash
# Run all tests
flutter test

# Run specific test file
flutter test test/presentation/bloc/auth_bloc_test.dart

# Run with coverage
flutter test --coverage
```

## 🚀 Build & Deploy

### Android

```bash
# Debug build
flutter build apk --debug

# Release build
flutter build apk --release
flutter build appbundle --release
```

### iOS

```bash
# Debug build
flutter build ios --debug

# Release build
flutter build ios --release
```

## 📝 Convenciones de Código

- Usar `flutter_lints` para linting
- Nombres de archivos en `snake_case`
- Nombres de clases en `PascalCase`
- Nombres de variables y funciones en `camelCase`
- Siempre agregar documentación para clases y funciones públicas

## 🔗 Enlaces

- [Documentación Flutter](https://flutter.dev/docs)
- [BLoC Library](https://bloclibrary.dev)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## 👥 Equipo

Desarrollado por el equipo de CarDealer

---

**Versión**: 1.0.0  
**Última actualización**: Diciembre 2025
