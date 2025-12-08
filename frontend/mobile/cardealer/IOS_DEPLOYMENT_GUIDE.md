# iOS Deployment Configuration

## Configuración de Build Settings

### 1. Configuración de Schemes y Targets

El proyecto usa **xcconfig files** para manejar diferentes configuraciones:

```
ios/Flutter/
├── Debug-dev.xcconfig
├── Debug-staging.xcconfig
├── Debug-prod.xcconfig
├── Release-dev.xcconfig
├── Release-staging.xcconfig
└── Release-prod.xcconfig
```

### 2. Crear Schemes en Xcode

1. Abrir `ios/Runner.xcworkspace` en Xcode
2. Product > Scheme > Manage Schemes
3. Crear 3 schemes:
   - **dev**: Development (com.cardealer.mobile.dev)
   - **staging**: Staging (com.cardealer.mobile.stg)
   - **prod**: Production (com.cardealer.mobile)

### 3. Configurar Signing

#### Automatic Signing (Desarrollo)
1. En Xcode: Runner target > Signing & Capabilities
2. Enable "Automatically manage signing"
3. Seleccionar tu Team

#### Manual Signing (Production)
1. Crear certificados en Apple Developer Portal
2. Descargar provisioning profiles
3. En Xcode: Signing & Capabilities
4. Disable automatic signing
5. Seleccionar manualmente los profiles

### 4. Bundle Identifiers por Flavor

| Flavor | Bundle ID | Display Name |
|--------|-----------|--------------|
| dev | com.cardealer.mobile.dev | CarDealer DEV |
| staging | com.cardealer.mobile.stg | CarDealer STG |
| prod | com.cardealer.mobile | CarDealer |

## Build Commands

### Desarrollo
```bash
# Debug build para dev
flutter build ios --debug --flavor dev -t lib/main_dev.dart

# Profile build
flutter build ios --profile --flavor dev -t lib/main_dev.dart
```

### Staging
```bash
flutter build ios --release --flavor staging -t lib/main_staging.dart
```

### Production
```bash
# Release build
flutter build ios --release --flavor prod -t lib/main_prod.dart

# Build para TestFlight
flutter build ipa --release --flavor prod -t lib/main_prod.dart
```

## App Store Submission

### 1. Requisitos Previos

- ✅ Apple Developer Account ($99/año)
- ✅ App Store Connect configurado
- ✅ Certificados y provisioning profiles
- ✅ App icons en todas las resoluciones
- ✅ Screenshots para todos los device sizes
- ✅ Privacy policy URL
- ✅ Support URL

### 2. Información Requerida

**App Information:**
- App Name: CarDealer
- Subtitle: Marketplace de Vehículos
- Category: Shopping
- Content Rights: Own all rights

**Version Information:**
- Version: 1.0.0
- Build: 1
- What's New: Initial release

**Privacy:**
- Privacy Policy URL: https://cardealer.com/privacy
- Uses Location: Yes (para búsquedas cercanas)
- Uses Camera: Yes (para fotos de vehículos)
- Uses Photo Library: Yes (selección de imágenes)

### 3. Upload a TestFlight

```bash
# Build IPA
flutter build ipa --release --flavor prod -t lib/main_prod.dart

# El archivo se genera en:
# build/ios/ipa/cardealer_mobile.ipa

# Subir con Transporter app o Xcode
# Xcode > Window > Organizer > Distribute App
```

### 4. TestFlight Testing

1. En App Store Connect > TestFlight
2. Agregar Internal Testers (hasta 100)
3. Agregar External Testers (hasta 10,000)
4. Completar "What to Test"
5. Submit for Review (External)

### 5. App Store Review

**Checklist antes de enviar:**
- [ ] App funciona en todos los iOS devices soportados (iOS 12.0+)
- [ ] No hay crashes ni bugs críticos
- [ ] Todas las features funcionan correctamente
- [ ] Screenshots y videos actualizados
- [ ] App description completa
- [ ] Keywords optimizados (100 caracteres máx)
- [ ] Privacy policy accesible
- [ ] Support email configurado
- [ ] Age rating correcto
- [ ] IDFA usage declaration (si aplica)

## Troubleshooting

### Problemas Comunes

**1. Signing Error**
```
Error: No profiles for 'com.cardealer.mobile' were found
```
**Solución:** Crear provisioning profile en Apple Developer Portal

**2. Architecture Error**
```
Error: Unable to find a specification for Runner (2.0)
```
**Solución:** 
```bash
cd ios
pod repo update
pod install
```

**3. Missing Info.plist Keys**
```
Error: Missing purpose string for NSCameraUsageDescription
```
**Solución:** Agregar description en Info.plist (ya configurado)

### Comandos Útiles

```bash
# Limpiar build cache
flutter clean
cd ios
rm -rf Pods Podfile.lock
pod install

# Verificar signing
cd ios
xcodebuild -workspace Runner.xcworkspace \
  -scheme prod \
  -configuration Release \
  -showBuildSettings

# Validar archive antes de upload
xcodebuild -exportArchive \
  -archivePath build/Runner.xcarchive \
  -exportPath build/ipa \
  -exportOptionsPlist ExportOptions.plist
```

## App Store Optimization (ASO)

### Keywords Sugeridos
```
coches, autos, vehículos, comprar coche, vender coche, 
marketplace coches, concesionario, dealer, cardealer,
segunda mano, usados, nuevos
```

### App Description Template
```
CarDealer es el marketplace líder de vehículos donde puedes:

✅ Buscar entre miles de coches
✅ Filtrar por marca, modelo, precio
✅ Contactar directamente con dealers
✅ Ver fotos HD y detalles completos
✅ Guardar tus favoritos
✅ Recibir notificaciones de nuevos coches

¿Eres dealer? 
Publica tus vehículos y llega a miles de compradores.

Descarga gratis y encuentra tu próximo coche hoy.
```

## Monitoring Post-Launch

### Métricas Clave
- Downloads
- DAU/MAU
- Retention (Day 1, 7, 30)
- Crash-free rate
- App Store rating
- Review sentiment

### Firebase Crashlytics
Ver configuración en: FIREBASE_SETUP.md

### App Store Connect Analytics
- Sales and Trends
- App Analytics
- TestFlight
- Ratings and Reviews

## Versioning Strategy

```yaml
# pubspec.yaml
version: 1.0.0+1
         ^     ^
         |     |
         |     +-- Build number (auto-increment por CI)
         +-------- Version name (semver)
```

**Semantic Versioning:**
- Major.Minor.Patch
- 1.0.0: Initial release
- 1.1.0: New features
- 1.0.1: Bug fixes
- 2.0.0: Breaking changes

**Build Number:**
- Incrementar automáticamente en CI/CD
- Único para cada build
- Requerido por App Store
