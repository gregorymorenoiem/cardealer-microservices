# Firebase Configuration Guide

## ⚠️ IMPORTANTE: Archivos de Configuración Requeridos

Para que Firebase funcione, necesitas agregar los archivos de configuración de Firebase:

### Android
1. Ir a [Firebase Console](https://console.firebase.google.com/)
2. Crear proyecto o usar existente
3. Agregar app Android con package name: `com.cardealer.mobile`
4. Descargar `google-services.json`
5. Colocar en: `android/app/google-services.json`

**Para flavors (dev/staging/prod):**
- `android/app/src/dev/google-services.json` (package: com.cardealer.mobile.dev)
- `android/app/src/staging/google-services.json` (package: com.cardealer.mobile.stg)  
- `android/app/src/prod/google-services.json` (package: com.cardealer.mobile)

### iOS
1. En Firebase Console, agregar app iOS
2. Bundle ID: `com.cardealer.mobile`
3. Descargar `GoogleService-Info.plist`
4. En Xcode: Drag & drop a `Runner/` (asegúrate de marcar "Copy items if needed")

**Para flavors (dev/staging/prod):**
- Crear 3 configs en Firebase (dev, staging, prod)
- Usar diferentes GoogleService-Info.plist por scheme
- Configurar en Build Phases de Xcode

## Configuración de Android

### 1. Agregar plugin de Google Services

Editar `android/build.gradle.kts`:

```kotlin
buildscript {
    dependencies {
        classpath("com.google.gms:google-services:4.4.0")
        classpath("com.google.firebase:firebase-crashlytics-gradle:2.9.9")
    }
}
```

Editar `android/app/build.gradle.kts`:

```kotlin
plugins {
    id("com.android.application")
    id("kotlin-android")
    id("dev.flutter.flutter-gradle-plugin")
    id("com.google.gms.google-services") // Agregar esta línea
    id("com.google.firebase.crashlytics") // Agregar esta línea
}
```

## Configuración de iOS

### 1. Editar Podfile

Ya está configurado en `ios/Podfile`. Descomentar las líneas de Firebase:

```ruby
# Firebase
pod 'Firebase/Analytics'
pod 'Firebase/Crashlytics'
pod 'Firebase/RemoteConfig'
pod 'Firebase/Messaging'
```

### 2. Instalar pods

```bash
cd ios
pod install
cd ..
```

## Uso en la App

### Inicialización

El servicio ya está implementado en `lib/core/services/firebase_service.dart`.

Para inicializar Firebase, agregar en `main.dart`:

```dart
import 'core/services/firebase_service.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  
  // Inicializar AppConfig
  AppConfig.initialize(Flavor.dev); // o staging/prod
  
  // Inicializar Firebase
  await FirebaseService.initialize();
  
  // Inicializar DI
  configureDependencies();
  
  runApp(CarDealerApp());
}
```

### Analytics

```dart
// Log evento personalizado
await FirebaseService.logEvent(
  name: 'vehicle_viewed',
  parameters: {
    'vehicle_id': '123',
    'make': 'Toyota',
    'model': 'Corolla',
  },
);

// Log vista de pantalla
await FirebaseService.logScreenView(
  screenName: 'VehicleDetailPage',
);
```

### Crashlytics

```dart
// Log error no fatal
try {
  // código que puede fallar
} catch (e, stack) {
  await FirebaseService.logError(
    e,
    stack,
    reason: 'Failed to load vehicles',
    information: {'user_id': '123'},
  );
}

// Establecer user ID
await FirebaseService.setUserId('user_123');

// Limpiar user ID (logout)
await FirebaseService.clearUserId();
```

### Remote Config

```dart
// Verificar mantenimiento
if (FirebaseService.isMaintenanceMode) {
  // Mostrar pantalla de mantenimiento
}

// Verificar actualización forzada
if (FirebaseService.requiresForceUpdate) {
  // Mostrar diálogo de actualización
}

// Obtener valores custom
final maxUploadSize = FirebaseService.getRemoteConfigValue<int>(
  'max_image_upload_size_mb',
  10, // valor por defecto
);
```

## Configuración de Remote Config

En Firebase Console > Remote Config, crear estos parámetros:

| Key | Type | Default Value | Descripción |
|-----|------|---------------|-------------|
| `min_app_version` | String | "1.0.0" | Versión mínima requerida |
| `force_update` | Boolean | false | Forzar actualización |
| `maintenance_mode` | Boolean | false | Modo mantenimiento |
| `featured_ratio` | Number | 0.4 | % de vehículos destacados |
| `max_image_upload_size_mb` | Number | 10 | Tamaño máximo de imagen |
| `enable_notifications` | Boolean | true | Habilitar notificaciones |

## Testing

### Verificar inicialización

En modo debug, deberías ver en console:
```
✅ Firebase inicializado correctamente
Environment: dev
Analytics enabled: false
```

### Verificar eventos en Firebase Console

1. Ir a Analytics > Events
2. Habilitar DebugView
3. Ejecutar app en modo debug
4. Ver eventos en tiempo real

### Comando para habilitar Debug View

```bash
# Android
adb shell setprop debug.firebase.analytics.app com.cardealer.mobile.dev

# iOS (en Xcode)
# Edit Scheme > Run > Arguments > Add: -FIRAnalyticsDebugEnabled
```

## Troubleshooting

### Error: "Default FirebaseApp is not initialized"
- Verificar que `google-services.json` o `GoogleService-Info.plist` existen
- Verificar que `Firebase.initializeApp()` se llama antes de usar Firebase
- Verificar package name / bundle ID coinciden

### Analytics no aparece en consola
- Puede tomar hasta 24 horas en producción
- Usar DebugView para testing inmediato
- Verificar que Analytics está habilitado en AppConfig

### Crashlytics no reporta crashes
- Verificar que crashlytics está habilitado para el environment
- En iOS, crashes solo se suben cuando la app se reinicia
- Usar `FirebaseCrashlytics.instance.crash()` para test

## Archivos Modificados

✅ `pubspec.yaml` - Dependencias Firebase agregadas
✅ `lib/core/services/firebase_service.dart` - Servicio creado
⚠️ `android/build.gradle.kts` - Requiere agregar plugins manualmente
⚠️ `android/app/build.gradle.kts` - Requiere agregar plugins manualmente
⚠️ `ios/Podfile` - Descomentar pods de Firebase

## Próximos Pasos

1. ✅ Crear proyecto en Firebase Console
2. ✅ Descargar archivos de configuración
3. ✅ Agregar plugins de Gradle (Android)
4. ✅ Instalar pods (iOS)
5. ✅ Inicializar en main.dart
6. ✅ Testear con DebugView
