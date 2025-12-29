# CarDealer Mobile - Deployment Scripts

Scripts para automatizar el proceso de build y deployment.

## Scripts Disponibles

### 1. build.sh (Linux/macOS)
Script bash para builds en sistemas Unix.

```bash
# Hacer ejecutable
chmod +x scripts/build.sh

# Ejecutar
./scripts/build.sh
```

### 2. build.ps1 (Windows)
Script PowerShell para builds en Windows.

```powershell
# Ejecutar
.\scripts\build.ps1

# Con parámetros
.\scripts\build.ps1 -Action android -Flavor prod
.\scripts\build.ps1 -Action ios -Flavor dev
.\scripts\build.ps1 -Action clean
```

### 3. deploy-android.sh
Script para deployment a Play Store (requiere configuración).

### 4. deploy-ios.sh
Script para deployment a App Store (requiere configuración).

## Uso del Menú Interactivo

Ambos scripts (`build.sh` y `build.ps1`) ofrecen un menú interactivo:

```
1.  Build Android DEV (APK)
2.  Build Android STAGING (APK)
3.  Build Android PROD (APK)
4.  Build Android PROD (AAB - Play Store)
5.  Build iOS DEV
6.  Build iOS STAGING
7.  Build iOS PROD
8.  Build iOS PROD (IPA - App Store)
9.  Build TODO Android
10. Build TODO iOS
11. Build TODO (Android + iOS)
12. Clean build
0.  Salir
```

## Builds por Flavor

### Development (dev)
```bash
# Android
flutter build apk --release --flavor dev -t lib/main_dev.dart

# iOS
flutter build ios --release --flavor dev -t lib/main_dev.dart
```

### Staging (staging)
```bash
# Android
flutter build apk --release --flavor staging -t lib/main_staging.dart

# iOS
flutter build ios --release --flavor staging -t lib/main_staging.dart
```

### Production (prod)
```bash
# Android APK
flutter build apk --release --flavor prod -t lib/main_prod.dart

# Android AAB (Play Store)
flutter build appbundle --release --flavor prod -t lib/main_prod.dart

# iOS
flutter build ios --release --flavor prod -t lib/main_prod.dart

# iOS IPA (App Store)
flutter build ipa --release --flavor prod -t lib/main_prod.dart
```

## Configuración de Entornos

Cada flavor tiene su propio entry point:

- `lib/main_dev.dart` - Development
- `lib/main_staging.dart` - Staging
- `lib/main_prod.dart` - Production

### Crear Entry Points

```dart
// lib/main_dev.dart
import 'package:flutter/material.dart';
import 'core/config/environment.dart';
import 'main.dart' as app;

void main() {
  Environment.init(
    environment: EnvironmentType.dev,
    apiBaseUrl: 'https://api-dev.cardealer.com',
  );
  
  app.main();
}
```

```dart
// lib/main_staging.dart
import 'package:flutter/material.dart';
import 'core/config/environment.dart';
import 'main.dart' as app;

void main() {
  Environment.init(
    environment: EnvironmentType.staging,
    apiBaseUrl: 'https://api-staging.cardealer.com',
  );
  
  app.main();
}
```

```dart
// lib/main_prod.dart
import 'package:flutter/material.dart';
import 'core/config/environment.dart';
import 'main.dart' as app;

void main() {
  Environment.init(
    environment: EnvironmentType.production,
    apiBaseUrl: 'https://api.cardealer.com',
  );
  
  app.main();
}
```

## CI/CD Integration

### GitHub Actions

```yaml
# .github/workflows/build.yml
name: Build

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build-android:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-java@v3
        with:
          distribution: 'zulu'
          java-version: '17'
      - uses: subosito/flutter-action@v2
        with:
          flutter-version: '3.16.0'
      
      - run: flutter pub get
      - run: flutter analyze
      - run: flutter test
      - run: flutter build apk --release --flavor prod -t lib/main_prod.dart
      
      - uses: actions/upload-artifact@v3
        with:
          name: android-apk
          path: build/app/outputs/apk/prod/release/

  build-ios:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v3
      - uses: subosito/flutter-action@v2
        with:
          flutter-version: '3.16.0'
      
      - run: flutter pub get
      - run: flutter analyze
      - run: flutter test
      - run: flutter build ios --release --flavor prod -t lib/main_prod.dart --no-codesign
      
      - uses: actions/upload-artifact@v3
        with:
          name: ios-build
          path: build/ios/iphoneos/
```

## Troubleshooting

### Problema: "Command not found: flutter"
**Solución:** Asegúrate de que Flutter está en tu PATH

```bash
# Verificar
which flutter
flutter --version

# Agregar a PATH si es necesario
export PATH="$PATH:[FLUTTER_DIRECTORY]/flutter/bin"
```

### Problema: "Build failed" en Android
**Solución:** Verifica la configuración de signing

```bash
# Verificar que existe key.properties
ls -la android/key.properties

# Verificar gradlew permissions
chmod +x android/gradlew
```

### Problema: "Build failed" en iOS
**Solución:** Actualizar pods

```bash
cd ios
pod repo update
pod install
cd ..
```

## Best Practices

1. **Siempre hacer clean antes de release builds**
   ```bash
   flutter clean
   flutter pub get
   ```

2. **Verificar analyze y tests antes de build**
   ```bash
   flutter analyze
   flutter test
   ```

3. **Usar flavors apropiados**
   - `dev` para desarrollo local
   - `staging` para testing interno
   - `prod` para releases públicos

4. **Incrementar version en pubspec.yaml**
   ```yaml
   version: 1.0.1+2  # Cambiar antes de cada release
   ```

5. **Documentar cambios en CHANGELOG.md**

6. **Testing en dispositivos reales**
   - Android: Múltiples fabricantes (Samsung, Xiaomi, etc)
   - iOS: iPhone y iPad de diferentes generaciones

## Automatización Completa

Para automatizar completamente el proceso, considera:

1. **Fastlane** para deployment
2. **GitHub Actions** para CI/CD
3. **Firebase App Distribution** para beta testing
4. **CodeMagic** o **Codemagic** como alternativa

Ver guías específicas:
- `FASTLANE_SETUP.md`
- `.github/workflows/`
