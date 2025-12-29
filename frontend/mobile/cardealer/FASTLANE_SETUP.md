# Fastlane Setup Guide

## Instalación

### Instalar Fastlane

**macOS:**
```bash
# Via Homebrew (recomendado)
brew install fastlane

# O via RubyGems
sudo gem install fastlane
```

**Windows:**
```powershell
# Fastlane requiere Ruby
# Instalar Ruby desde https://rubyinstaller.org/
# Luego instalar fastlane
gem install fastlane
```

### Verificar instalación
```bash
fastlane --version
```

## Configuración Android

### 1. Navegar a la carpeta
```bash
cd mobile/android
```

### 2. Inicializar Fastlane (opcional, ya está configurado)
```bash
fastlane init
```

### 3. Configurar Google Play Service Account

1. Ir a [Google Play Console](https://play.google.com/console)
2. Setup > API Access
3. Crear nuevo Service Account en Google Cloud
4. Generar y descargar JSON key
5. Colocar en: `mobile/android/fastlane/google-play-service-account.json`
6. Actualizar `Appfile`:
```ruby
json_key_file("fastlane/google-play-service-account.json")
package_name("com.cardealer.mobile")
```

### 4. Configurar Firebase App Distribution

Instalar plugin:
```bash
fastlane add_plugin firebase_app_distribution
```

Configurar variables de entorno (agregar a GitHub Secrets):
```
FIREBASE_APP_ID_ANDROID_DEV=1:123456789:android:abc123def456
FIREBASE_APP_ID_ANDROID_STAGING=1:123456789:android:xyz789uvw012
FIREBASE_TOKEN=your-firebase-ci-token
```

Para obtener Firebase token:
```bash
firebase login:ci
```

### 5. Usar lanes de Android

```bash
# Build y deploy DEV a Firebase
fastlane dev

# Build y deploy STAGING a Firebase
fastlane staging

# Build y upload PROD a Google Play Internal
fastlane prod

# Promover a beta
fastlane beta

# Promover a producción
fastlane release
```

## Configuración iOS

### 1. Navegar a la carpeta
```bash
cd mobile/ios
```

### 2. Inicializar Fastlane (opcional, ya está configurado)
```bash
fastlane init
```

### 3. Configurar App Store Connect

Actualizar `Appfile`:
```ruby
app_identifier("com.cardealer.mobile")
apple_id("your-apple-id@example.com")
itc_team_id("YOUR_ITC_TEAM_ID") # Team ID de App Store Connect
team_id("YOUR_TEAM_ID") # Team ID de Apple Developer
```

Para encontrar Team IDs:
- App Store Connect Team ID: App Store Connect > Users and Access > Keys
- Developer Team ID: developer.apple.com > Membership

### 4. Configurar Match (Code Signing)

```bash
fastlane match init
```

Configurar en `Matchfile`:
```ruby
git_url("git@github.com:your-org/certificates.git")
storage_mode("git")
type("appstore") # o "adhoc", "development"
app_identifier(["com.cardealer.mobile", "com.cardealer.mobile.dev", "com.cardealer.mobile.stg"])
username("your-apple-id@example.com")
```

Generar certificados:
```bash
# Development
fastlane match development

# Ad-hoc (para Firebase App Distribution)
fastlane match adhoc

# App Store
fastlane match appstore
```

### 5. Configurar Firebase App Distribution iOS

Instalar plugin:
```bash
fastlane add_plugin firebase_app_distribution
```

Variables de entorno:
```
FIREBASE_APP_ID_IOS_DEV=1:123456789:ios:abc123def456
FIREBASE_APP_ID_IOS_STAGING=1:123456789:ios:xyz789uvw012
FIREBASE_TOKEN=your-firebase-ci-token
```

### 6. Usar lanes de iOS

```bash
# Build y deploy DEV a Firebase
fastlane dev

# Build y deploy STAGING a Firebase
fastlane staging

# Build y upload PROD a TestFlight
fastlane prod

# Generar screenshots
fastlane screenshots

# Ejecutar tests
fastlane test

# Release completo a App Store
fastlane release
```

## Integración con GitHub Actions

El workflow ya está configurado en `.github/workflows/flutter-ci-cd.yml`.

### Secrets de GitHub requeridos:

**Android:**
- `GOOGLE_PLAY_SERVICE_ACCOUNT_JSON` - Contenido del JSON key
- `FIREBASE_APP_ID_ANDROID_DEV`
- `FIREBASE_APP_ID_ANDROID_STAGING`
- `FIREBASE_TOKEN`
- `ANDROID_KEYSTORE` - Keystore en base64
- `ANDROID_KEYSTORE_PASSWORD`
- `ANDROID_KEY_ALIAS`
- `ANDROID_KEY_PASSWORD`

**iOS:**
- `APPLE_ID` - Apple ID email
- `APPLE_APP_SPECIFIC_PASSWORD` - App-specific password
- `MATCH_PASSWORD` - Password para certificates repo
- `FIREBASE_APP_ID_IOS_DEV`
- `FIREBASE_APP_ID_IOS_STAGING`
- `FIREBASE_TOKEN`

**Firebase (general):**
- `FIREBASE_CONFIG_DEV` - Contenido de google-services.json (dev)
- `FIREBASE_CONFIG_STAGING` - Contenido de google-services.json (staging)
- `FIREBASE_CONFIG_PROD` - Contenido de google-services.json (prod)
- `FIREBASE_CONFIG_IOS_DEV` - Contenido de GoogleService-Info.plist (dev)
- `FIREBASE_CONFIG_IOS_STAGING` - Contenido de GoogleService-Info.plist (staging)
- `FIREBASE_CONFIG_IOS_PROD` - Contenido de GoogleService-Info.plist (prod)

## Signing Android

### Crear keystore para producción

```bash
keytool -genkey -v -keystore cardealer-release-key.jks -keyalg RSA -keysize 2048 -validity 10000 -alias cardealer
```

### Configurar en `android/key.properties`

```properties
storePassword=your-keystore-password
keyPassword=your-key-password
keyAlias=cardealer
storeFile=/path/to/cardealer-release-key.jks
```

### Actualizar `android/app/build.gradle.kts`

```kotlin
val keystoreProperties = Properties()
val keystorePropertiesFile = rootProject.file("key.properties")
if (keystorePropertiesFile.exists()) {
    keystoreProperties.load(FileInputStream(keystorePropertiesFile))
}

android {
    // ...
    
    signingConfigs {
        create("release") {
            keyAlias = keystoreProperties["keyAlias"] as String
            keyPassword = keystoreProperties["keyPassword"] as String
            storeFile = file(keystoreProperties["storeFile"] as String)
            storePassword = keystoreProperties["storePassword"] as String
        }
    }
    
    buildTypes {
        release {
            signingConfig = signingConfigs.getByName("release")
            // ...
        }
    }
}
```

## Comandos Útiles

### Android
```bash
# Listar todos los lanes disponibles
fastlane list

# Ver metadata de la app
fastlane supply init

# Validar configuración
fastlane validate
```

### iOS
```bash
# Listar certificates y profiles
fastlane match status

# Renovar certificates (si expiraron)
fastlane match nuke development
fastlane match nuke distribution

# Incrementar versión
fastlane run increment_version_number bump_type:patch
fastlane run increment_build_number
```

## Troubleshooting

### Error: "Could not find action, lane or variable"
- Verificar que estás en el directorio correcto (android/ o ios/)
- Verificar sintaxis en Fastfile

### Error: "No code signing identity found"
- Ejecutar `fastlane match` para generar/descargar certificados
- Verificar que Team ID es correcto

### Error: "Unable to authenticate with Google"
- Verificar que service account JSON es válido
- Verificar permisos del service account en Google Play Console

### Error: "Firebase token invalid"
- Generar nuevo token: `firebase login:ci`
- Actualizar en secrets de GitHub

## Testing Local

Antes de commitear, probar localmente:

```bash
# Android
cd mobile/android
fastlane dev

# iOS (requiere macOS)
cd mobile/ios
fastlane dev
```

## Próximos Pasos

1. ✅ Configurar Service Account de Google Play
2. ✅ Generar keystore de Android
3. ✅ Configurar App Store Connect
4. ✅ Configurar Match para iOS certificates
5. ✅ Obtener Firebase CI token
6. ✅ Agregar secrets a GitHub
7. ✅ Probar builds localmente
8. ✅ Probar CI/CD en GitHub Actions
