# Android Deployment Guide

## 🔑 Configuración de Signing

### 1. Generar Keystore

```bash
# Generar keystore para release
keytool -genkey -v -keystore ~/upload-keystore.jks \
  -storetype JKS -keyalg RSA -keysize 2048 -validity 10000 \
  -alias upload

# Información requerida:
# - Password del keystore (guardar seguro)
# - Password del key (puede ser igual)
# - First and Last Name: CarDealer
# - Organizational Unit: Mobile
# - Organization: CarDealer
# - City: Tu ciudad
# - State: Tu estado
# - Country Code: ES/MX/etc
```

### 2. Configurar key.properties

Crear archivo `android/key.properties`:

```properties
storePassword=TU_STORE_PASSWORD
keyPassword=TU_KEY_PASSWORD
keyAlias=upload
storeFile=/ruta/absoluta/a/upload-keystore.jks
```

**⚠️ IMPORTANTE:** 
- Agregar `key.properties` a `.gitignore`
- Guardar passwords en 1Password/LastPass
- Hacer backup del keystore (sin él no puedes actualizar la app)

### 3. Configuración de Flavors

El proyecto ya está configurado con 3 flavors:

| Flavor | App ID | Nombre |
|--------|--------|--------|
| dev | com.cardealer.mobile.dev | CarDealer DEV |
| staging | com.cardealer.mobile.stg | CarDealer STG |
| prod | com.cardealer.mobile | CarDealer |

## 🏗️ Build Commands

### Desarrollo
```bash
# Debug build
flutter build apk --debug --flavor dev -t lib/main_dev.dart

# Debug para dispositivo específico
flutter run --flavor dev -t lib/main_dev.dart
```

### Staging
```bash
# Release build para testing
flutter build apk --release --flavor staging -t lib/main_staging.dart

# App Bundle para testing interno
flutter build appbundle --release --flavor staging -t lib/main_staging.dart
```

### Production
```bash
# APK para distribución directa
flutter build apk --release --flavor prod -t lib/main_prod.dart

# App Bundle para Play Store (RECOMENDADO)
flutter build appbundle --release --flavor prod -t lib/main_prod.dart

# Split por ABI (APKs más pequeños)
flutter build apk --release --flavor prod -t lib/main_prod.dart --split-per-abi
```

### Ubicación de Builds

```
build/app/outputs/
├── bundle/
│   ├── devRelease/
│   ├── stagingRelease/
│   └── prodRelease/
│       └── app-prod-release.aab
└── apk/
    ├── dev/
    ├── staging/
    └── prod/
        └── release/
            └── app-prod-release.apk
```

## 📦 Google Play Store

### 1. Requisitos Previos

- ✅ Google Play Console Account ($25 one-time)
- ✅ App signing configurado
- ✅ Privacy policy URL
- ✅ Store listing completo
- ✅ Content rating
- ✅ Target audience seleccionado

### 2. Primera Publicación

**App Information:**
```
App name: CarDealer
Short description (80 chars):
  Compra y vende coches fácilmente. Marketplace líder de vehículos.

Full description (4000 chars):
  [Ver template abajo]

Category: Shopping
Tags: marketplace, coches, autos, vehículos

Contact details:
  Email: support@cardealer.com
  Phone: +1234567890
  Website: https://cardealer.com

Privacy Policy: https://cardealer.com/privacy
```

**Store Listing Assets:**
- Icon: 512x512 (PNG, 32-bit)
- Feature Graphic: 1024x500
- Screenshots:
  - Phone: Mínimo 2, máximo 8 (16:9 o 9:16)
  - 7" Tablet: Opcional
  - 10" Tablet: Opcional
- Promotional video: YouTube URL (opcional)

### 3. Content Rating

**Questionnaire IARC:**
- Violence: None
- Sexual content: None
- Language: None
- Controlled substances: None
- Gambling: None
- User interaction: Yes (chat, user-generated content)

**Expected Rating:** PEGI 3 / Everyone

### 4. Target Audience

- Age groups: 18+ (compra/venta requiere mayoría de edad)
- Children: No directed to children under 13

### 5. Releases

**Tracks disponibles:**
- **Internal testing:** Hasta 100 testers
- **Closed testing:** Testing privado
- **Open testing:** Beta público
- **Production:** Release público

**Rollout Strategy:**
```
1. Internal testing (5-10 usuarios) → 1 week
2. Closed testing (50-100 usuarios) → 2 weeks
3. Open testing (500-1000 usuarios) → 2 weeks
4. Production staged rollout:
   - 5% → 1 day
   - 10% → 1 day
   - 25% → 2 days
   - 50% → 2 days
   - 100% → Full release
```

### 6. Upload a Play Console

```bash
# 1. Build app bundle
flutter build appbundle --release --flavor prod -t lib/main_prod.dart

# 2. En Play Console:
#    - Release > Production > Create new release
#    - Upload AAB: build/app/outputs/bundle/prodRelease/app-prod-release.aab
#    - Add release notes
#    - Review and rollout

# 3. App Review:
#    - Típicamente 1-3 días
#    - Check console para updates
```

## 🔒 App Signing

### Play App Signing (Recomendado)

Google gestiona el signing key de producción:

1. En Play Console: Setup > App integrity
2. Enable "Use Play App Signing"
3. Upload tu upload certificate
4. Google firma automáticamente cada release

**Ventajas:**
- Google gestiona signing key
- No pierdes acceso si pierdes keystore
- Optimization automática del APK
- Support para múltiples signing configs

### Manual Signing (Legacy)

Si no usas Play App Signing, TÚ gestionas el keystore de producción.

⚠️ **Si pierdes el keystore, NO puedes actualizar la app**

## 📊 Store Listing Template

```markdown
# CarDealer - Marketplace de Vehículos

🚗 Compra y vende coches de forma fácil y segura

## ¿Qué es CarDealer?

CarDealer es el marketplace líder donde puedes encontrar el coche 
perfecto o vender el tuyo rápidamente.

## Características Principales

✅ **Búsqueda Avanzada**
   Filtra por marca, modelo, año, precio, kilometraje y más

✅ **Miles de Vehículos**
   Coches nuevos, seminuevos y de segunda mano

✅ **Fotos HD**
   Galería completa de cada vehículo

✅ **Contacto Directo**
   Habla directamente con dealers verificados

✅ **Favoritos**
   Guarda coches para verlos después

✅ **Notificaciones**
   Recibe alertas de nuevos coches que te interesan

✅ **Perfil de Dealer**
   ¿Vendes coches? Publica tu inventario

## Para Compradores

- Busca entre miles de vehículos
- Compara precios y características
- Contacta dealers verificados
- Guarda tus búsquedas favoritas
- Recibe notificaciones de nuevos coches

## Para Dealers

- Publica tu inventario completo
- Gestiona tus anuncios fácilmente
- Recibe consultas de compradores
- Analytics de tus publicaciones
- Panel de control optimizado

## Seguridad

- Dealers verificados
- Sistema de reviews
- Reportar anuncios sospechosos
- Datos encriptados

## Gratis y Sin Comisiones

Descarga gratis y empieza a buscar tu próximo coche hoy mismo.

---

🌟 Únete a miles de usuarios que ya encontraron su coche ideal

📧 Soporte: support@cardealer.com
🌐 Web: https://cardealer.com
```

## 🚀 CI/CD con GitHub Actions

Ver: `.github/workflows/android-release.yml`

```yaml
name: Android Release

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
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
      
      - name: Decode keystore
        run: |
          echo "${{ secrets.KEYSTORE_BASE64 }}" | base64 --decode > android/app/upload-keystore.jks
      
      - name: Create key.properties
        run: |
          echo "storePassword=${{ secrets.STORE_PASSWORD }}" > android/key.properties
          echo "keyPassword=${{ secrets.KEY_PASSWORD }}" >> android/key.properties
          echo "keyAlias=upload" >> android/key.properties
          echo "storeFile=upload-keystore.jks" >> android/key.properties
      
      - run: flutter pub get
      - run: flutter build appbundle --release --flavor prod -t lib/main_prod.dart
      
      - name: Upload to Play Store
        uses: r0adkll/upload-google-play@v1
        with:
          serviceAccountJsonPlainText: ${{ secrets.SERVICE_ACCOUNT_JSON }}
          packageName: com.cardealer.mobile
          releaseFiles: build/app/outputs/bundle/prodRelease/app-prod-release.aab
          track: internal
          status: completed
```

## 📈 Monitoring Post-Release

### Play Console Metrics

**Vitals:**
- Crash rate (target: < 1%)
- ANR rate (target: < 0.5%)
- Excessive wakeups
- Stuck wake locks

**User Acquisition:**
- Installs by source
- Store listing conversions
- Retained installers

**Engagement:**
- Daily active users
- Monthly active users
- Average session length
- Screens per session

### Firebase Crashlytics

```dart
// Ya configurado en el código
await FirebaseCrashlytics.instance.setCrashlyticsCollectionEnabled(true);

// Reportar errores custom
try {
  // código
} catch (e, stack) {
  await FirebaseCrashlytics.instance.recordError(e, stack);
}
```

## 🐛 Troubleshooting

### Problemas Comunes

**1. "App not installed" al hacer update**
```
Error: INSTALL_FAILED_UPDATE_INCOMPATIBLE
```
**Causa:** Signing key diferente
**Solución:** Usar mismo keystore para debug/release

**2. ProGuard elimina código necesario**
```
Error: ClassNotFoundException en runtime
```
**Solución:** Actualizar proguard-rules.pro con keep rules

**3. Multidex error**
```
Error: Cannot fit requested classes in a single dex file
```
**Solución:** Ya configurado `multiDexEnabled = true`

**4. Play Store rechaza APK**
```
Error: You uploaded a debuggable APK
```
**Solución:** Build con `--release` flag

### Comandos Útiles

```bash
# Verificar firma de APK
keytool -printcert -jarfile app-prod-release.apk

# Inspeccionar AAB
bundletool build-apks --bundle=app-prod-release.aab \
  --output=app.apks --mode=universal

# Analizar tamaño de APK
flutter build apk --release --analyze-size

# Ver dependencias
flutter pub deps --style=tree

# Limpiar builds anteriores
flutter clean
cd android && ./gradlew clean
```

## 📋 Pre-Launch Checklist

- [ ] Keystore generado y respaldado
- [ ] key.properties configurado (no en git)
- [ ] ProGuard rules actualizadas
- [ ] Todos los assets (iconos, screenshots)
- [ ] Privacy policy publicada
- [ ] Content rating completado
- [ ] Store listing completo
- [ ] Testing en múltiples devices
- [ ] Crash-free rate > 99%
- [ ] Performance monitoreada
- [ ] Deep links testeados
- [ ] Push notifications funcionando
- [ ] Analytics configurado

## 🎯 Post-Launch Strategy

**Week 1:**
- Monitor crash rate cada día
- Review user ratings/reviews
- Fix critical bugs ASAP
- Responder a reviews

**Week 2-4:**
- Analizar user behavior
- Identificar friction points
- Planear hotfixes
- Preparar v1.1.0

**Long-term:**
- A/B testing features
- Optimize ASO keywords
- Seasonal campaigns
- Feature releases mensually
