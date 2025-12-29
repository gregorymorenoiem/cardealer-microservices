# iOS Scheme Configuration for Flavors

Para configurar los schemes de iOS para cada flavor (dev, staging, prod), necesitas:

## 1. Abrir el proyecto en Xcode
```bash
open ios/Runner.xcworkspace
```

## 2. Duplicar schemes
1. En Xcode, ir a Product > Scheme > Manage Schemes
2. Duplicar "Runner" scheme 3 veces
3. Renombrar a: "dev", "staging", "prod"

## 3. Configurar cada scheme

### Scheme "dev"
- Edit Scheme > Build > Pre-actions > Add Run Script:
```bash
echo "APP_DISPLAY_NAME=CarDealer DEV" > ${SRCROOT}/Flutter/Generated.xcconfig
echo "PRODUCT_BUNDLE_IDENTIFIER=com.cardealer.mobile.dev" >> ${SRCROOT}/Flutter/Generated.xcconfig
```

### Scheme "staging"
- Edit Scheme > Build > Pre-actions > Add Run Script:
```bash
echo "APP_DISPLAY_NAME=CarDealer STG" > ${SRCROOT}/Flutter/Generated.xcconfig
echo "PRODUCT_BUNDLE_IDENTIFIER=com.cardealer.mobile.stg" >> ${SRCROOT}/Flutter/Generated.xcconfig
```

### Scheme "prod"
- Edit Scheme > Build > Pre-actions > Add Run Script:
```bash
echo "APP_DISPLAY_NAME=CarDealer" > ${SRCROOT}/Flutter/Generated.xcconfig
echo "PRODUCT_BUNDLE_IDENTIFIER=com.cardealer.mobile" >> ${SRCROOT}/Flutter/Generated.xcconfig
```

## 4. Comandos Flutter para cada flavor

### Android
```bash
# Dev
flutter run --flavor dev -t lib/main_dev.dart

# Staging
flutter run --flavor staging -t lib/main_staging.dart

# Prod
flutter run --flavor prod -t lib/main_prod.dart
```

### iOS (después de configurar schemes en Xcode)
```bash
# Dev
flutter run --flavor dev -t lib/main_dev.dart

# Staging  
flutter run --flavor staging -t lib/main_staging.dart

# Prod
flutter run --flavor prod -t lib/main_prod.dart
```

## 5. Build commands

### Android APK
```bash
flutter build apk --flavor prod -t lib/main_prod.dart --release
```

### Android App Bundle
```bash
flutter build appbundle --flavor prod -t lib/main_prod.dart --release
```

### iOS
```bash
flutter build ios --flavor prod -t lib/main_prod.dart --release
```

## Nota
La configuración completa de iOS flavors requiere Xcode y solo puede hacerse completamente en macOS.
Para desarrollo en Windows, la configuración de Android está lista para usar.
