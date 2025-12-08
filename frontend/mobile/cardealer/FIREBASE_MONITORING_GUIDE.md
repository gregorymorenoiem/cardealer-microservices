# Firebase Configuration Guide

## Configuración de Firebase para CarDealer Mobile

### 1. Crear Proyecto en Firebase

1. Ir a [Firebase Console](https://console.firebase.google.com/)
2. Crear nuevo proyecto: **CarDealer**
3. Habilitar Google Analytics (opcional pero recomendado)

### 2. Configurar Apps por Flavor

Necesitamos crear 3 apps en Firebase (una por flavor):

#### App 1: DEV
- **Android:** `com.cardealer.mobile.dev`
- **iOS:** `com.cardealer.mobile.dev`
- **Nombre:** CarDealer DEV

#### App 2: STAGING
- **Android:** `com.cardealer.mobile.stg`
- **iOS:** `com.cardealer.mobile.stg`
- **Nombre:** CarDealer STG

#### App 3: PROD
- **Android:** `com.cardealer.mobile`
- **iOS:** `com.cardealer.mobile`
- **Nombre:** CarDealer

### 3. Descargar Archivos de Configuración

#### Android

Para cada flavor, descargar `google-services.json`:

```
android/app/src/dev/google-services.json
android/app/src/staging/google-services.json
android/app/src/prod/google-services.json
```

Estructura de directorios:
```
android/
└── app/
    └── src/
        ├── dev/
        │   └── google-services.json
        ├── staging/
        │   └── google-services.json
        └── prod/
            └── google-services.json
```

#### iOS

Para cada flavor, descargar `GoogleService-Info.plist`:

```
ios/config/dev/GoogleService-Info.plist
ios/config/staging/GoogleService-Info.plist
ios/config/prod/GoogleService-Info.plist
```

Estructura de directorios:
```
ios/
├── config/
│   ├── dev/
│   │   └── GoogleService-Info.plist
│   ├── staging/
│   │   └── GoogleService-Info.plist
│   └── prod/
│       └── GoogleService-Info.plist
└── Runner/
```

### 4. Configurar Gradle (Android)

#### android/build.gradle.kts

Ya está configurado con:

```kotlin
buildscript {
    dependencies {
        classpath("com.google.gms:google-services:4.4.0")
        classpath("com.google.firebase:firebase-crashlytics-gradle:2.9.9")
    }
}
```

#### android/app/build.gradle.kts

Ya está configurado con:

```kotlin
plugins {
    id("com.google.gms.google-services")
    id("com.google.firebase.crashlytics")
}
```

### 5. Configurar Xcode (iOS)

1. Abrir `ios/Runner.xcworkspace` en Xcode
2. Para cada scheme (dev, staging, prod):
   - Seleccionar Runner target
   - Build Phases
   - Agregar "Run Script" phase:

```bash
# Type a script or drag a script file
"${PODS_ROOT}/FirebaseCrashlytics/run"
```

3. Configurar GoogleService-Info.plist por flavor:
   - Agregar Build Phase "Copy Files"
   - Copiar el plist correcto según el flavor

### 6. Habilitar Servicios en Firebase Console

#### Firebase Analytics ✅
- Automáticamente habilitado al crear proyecto
- Configurar conversions y eventos personalizados

#### Firebase Crashlytics ✅
1. En Firebase Console > Crashlytics
2. Enable Crashlytics
3. Ver crashes en tiempo real

#### Firebase Remote Config (Opcional)
- Configurar feature flags
- A/B testing
- Dynamic configuration

#### Firebase Cloud Messaging (Push Notifications)
1. En Firebase Console > Cloud Messaging
2. Configurar Server Key
3. Para iOS: Subir APNs certificate

### 7. Inicializar Firebase en la App

El código de inicialización ya está en `lib/main.dart`:

```dart
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_crashlytics/firebase_crashlytics.dart';
import 'package:firebase_analytics/firebase_analytics.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  
  // Inicializar Firebase
  await Firebase.initializeApp();
  
  // Configurar Crashlytics
  FlutterError.onError = FirebaseCrashlytics.instance.recordFlutterFatalError;
  PlatformDispatcher.instance.onError = (error, stack) {
    FirebaseCrashlytics.instance.recordError(error, stack, fatal: true);
    return true;
  };
  
  runApp(const MyApp());
}
```

### 8. Uso de Firebase Services

#### Analytics

```dart
import 'package:firebase_analytics/firebase_analytics.dart';

class AnalyticsService {
  static final FirebaseAnalytics _analytics = FirebaseAnalytics.instance;
  
  // Log custom event
  static Future<void> logEvent({
    required String name,
    Map<String, Object>? parameters,
  }) async {
    await _analytics.logEvent(
      name: name,
      parameters: parameters,
    );
  }
  
  // Log screen view
  static Future<void> logScreenView({
    required String screenName,
    String? screenClass,
  }) async {
    await _analytics.logScreenView(
      screenName: screenName,
      screenClass: screenClass,
    );
  }
  
  // Set user properties
  static Future<void> setUserId(String userId) async {
    await _analytics.setUserId(id: userId);
  }
  
  static Future<void> setUserProperty({
    required String name,
    required String value,
  }) async {
    await _analytics.setUserProperty(name: name, value: value);
  }
}

// Uso:
AnalyticsService.logEvent(
  name: 'view_vehicle',
  parameters: {
    'vehicle_id': '12345',
    'brand': 'Toyota',
    'model': 'Camry',
  },
);

AnalyticsService.logScreenView(screenName: 'HomePage');
```

#### Crashlytics

```dart
import 'package:firebase_crashlytics/firebase_crashlytics.dart';

class CrashlyticsService {
  static final FirebaseCrashlytics _crashlytics = FirebaseCrashlytics.instance;
  
  // Log error
  static Future<void> recordError(
    dynamic exception,
    StackTrace? stack, {
    dynamic reason,
    bool fatal = false,
  }) async {
    await _crashlytics.recordError(
      exception,
      stack,
      reason: reason,
      fatal: fatal,
    );
  }
  
  // Log message
  static Future<void> log(String message) async {
    await _crashlytics.log(message);
  }
  
  // Set user identifier
  static Future<void> setUserId(String userId) async {
    await _crashlytics.setUserId(userId);
  }
  
  // Set custom key
  static Future<void> setCustomKey(String key, Object value) async {
    await _crashlytics.setCustomKey(key, value);
  }
}

// Uso:
try {
  // código que puede fallar
} catch (e, stack) {
  CrashlyticsService.recordError(
    e,
    stack,
    reason: 'Error al cargar vehículos',
    fatal: false,
  );
}
```

#### Remote Config

```dart
import 'package:firebase_remote_config/firebase_remote_config.dart';

class RemoteConfigService {
  static final FirebaseRemoteConfig _remoteConfig = FirebaseRemoteConfig.instance;
  
  static Future<void> initialize() async {
    await _remoteConfig.setConfigSettings(RemoteConfigSettings(
      fetchTimeout: const Duration(minutes: 1),
      minimumFetchInterval: const Duration(hours: 1),
    ));
    
    // Set default values
    await _remoteConfig.setDefaults({
      'featured_vehicles_count': 6,
      'enable_chat': true,
      'maintenance_mode': false,
    });
    
    await _remoteConfig.fetchAndActivate();
  }
  
  static int getFeaturedVehiclesCount() {
    return _remoteConfig.getInt('featured_vehicles_count');
  }
  
  static bool isChatEnabled() {
    return _remoteConfig.getBool('enable_chat');
  }
  
  static bool isMaintenanceMode() {
    return _remoteConfig.getBool('maintenance_mode');
  }
}

// En main.dart:
await RemoteConfigService.initialize();
```

### 9. Eventos Analytics Recomendados

```dart
// User events
logEvent(name: 'sign_up', parameters: {'method': 'email'});
logEvent(name: 'login', parameters: {'method': 'email'});

// Vehicle events
logEvent(name: 'view_vehicle', parameters: {
  'vehicle_id': id,
  'brand': brand,
  'model': model,
  'price': price,
});

logEvent(name: 'search', parameters: {
  'search_term': query,
  'filters': filters,
});

logEvent(name: 'add_to_favorites', parameters: {
  'vehicle_id': id,
});

// Transaction events
logEvent(name: 'begin_checkout', parameters: {
  'vehicle_id': id,
  'price': price,
});

logEvent(name: 'purchase', parameters: {
  'transaction_id': id,
  'value': price,
  'currency': 'USD',
});

// Engagement
logEvent(name: 'share', parameters: {
  'content_type': 'vehicle',
  'item_id': vehicleId,
});
```

### 10. Testing Firebase

#### Verificar Analytics
```bash
# Habilitar debug mode
# Android
adb shell setprop debug.firebase.analytics.app com.cardealer.mobile.dev

# iOS
Xcode > Edit Scheme > Arguments > -FIRDebugEnabled
```

Ver eventos en tiempo real:
- Firebase Console > Analytics > DebugView

#### Verificar Crashlytics

```dart
// Forzar un crash para testing
ElevatedButton(
  onPressed: () {
    FirebaseCrashlytics.instance.crash();
  },
  child: Text('Test Crash'),
);

// Log error no fatal
FirebaseCrashlytics.instance.recordError(
  Exception('Test exception'),
  StackTrace.current,
  reason: 'Testing Crashlytics',
);
```

### 11. Performance Monitoring (Opcional)

Agregar al pubspec.yaml:
```yaml
firebase_performance: ^0.9.3+6
```

Uso:
```dart
import 'package:firebase_performance/firebase_performance.dart';

final trace = FirebasePerformance.instance.newTrace('load_vehicles');
await trace.start();

// ... código a medir

trace.incrementMetric('vehicles_count', vehiclesList.length);
await trace.stop();
```

### 12. Security Rules

#### Firestore (si se usa)
```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /vehicles/{vehicleId} {
      allow read: if true;
      allow write: if request.auth != null;
    }
  }
}
```

### 13. Monitoring Dashboard

Crear dashboard personalizado en Firebase Console:

- **Crashlytics:** Monitor crash-free rate (target: >99%)
- **Analytics:** Track DAU/MAU, retention
- **Performance:** Monitor app start time, network requests
- **Remote Config:** A/B tests activos

### 14. Troubleshooting

#### "FirebaseApp not initialized"
```dart
// Asegurar que Firebase.initializeApp() se llama antes de runApp()
await Firebase.initializeApp();
```

#### "google-services.json not found"
```bash
# Verificar ubicación
ls android/app/src/dev/google-services.json
ls android/app/src/staging/google-services.json
ls android/app/src/prod/google-services.json
```

#### iOS build failed
```bash
cd ios
pod install --repo-update
```

#### Analytics no reporta eventos
- Verificar que el app está en foreground
- Esperar 24 horas para data en production
- Usar DebugView para testing

## Checklist de Configuración

- [ ] Proyecto Firebase creado
- [ ] 3 apps configuradas (dev, staging, prod)
- [ ] google-services.json descargados y ubicados
- [ ] GoogleService-Info.plist descargados y ubicados
- [ ] Crashlytics habilitado
- [ ] Analytics configurado
- [ ] Remote Config inicializado
- [ ] Test crash exitoso
- [ ] Eventos analytics logueándose
- [ ] DebugView funcionando
- [ ] Dashboard monitoring configurado
