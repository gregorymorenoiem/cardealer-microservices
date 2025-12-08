# 🚀 Cómo Ejecutar el Proyecto Mobile

## Prerrequisitos

### 1. Instalar Flutter

**Windows:**
```powershell
# Descargar Flutter SDK desde: https://flutter.dev/docs/get-started/install/windows
# Agregar al PATH: C:\flutter\bin
```

**Verificar instalación:**
```bash
flutter doctor
```

### 2. Configurar Editor

**VS Code (Recomendado):**
- Instalar extensión "Flutter"
- Instalar extensión "Dart"

**Android Studio:**
- Instalar Flutter plugin
- Instalar Dart plugin

### 3. Configurar Dispositivo

**Android:**
- Habilitar "Depuración USB" en el dispositivo
- O usar Android Emulator

**iOS (Mac solamente):**
- Xcode instalado
- iOS Simulator
- Dispositivo físico con cuenta de desarrollador

---

## 🎬 Inicio Rápido

### Método 1: Script Automático (Recomendado)

```powershell
# Desde la raíz del proyecto cardealer
.\setup-mobile.ps1
```

Este script automáticamente:
1. ✅ Verifica Flutter instalado
2. ✅ Navega al directorio mobile
3. ✅ Limpia builds previos
4. ✅ Instala dependencias
5. ✅ Genera código necesario
6. ✅ Verifica dispositivos conectados

### Método 2: Manual

```bash
# 1. Navegar al directorio mobile
cd mobile

# 2. Limpiar builds previos (opcional)
flutter clean

# 3. Instalar dependencias
flutter pub get

# 4. Generar código (DI, models, etc.)
flutter pub run build_runner build --delete-conflicting-outputs

# 5. Ver dispositivos disponibles
flutter devices

# 6. Ejecutar app
flutter run
```

---

## 📱 Ejecutar en Diferentes Dispositivos

### Android Emulator

```bash
# Listar emuladores disponibles
flutter emulators

# Iniciar emulador específico
flutter emulators --launch Pixel_5_API_31

# Ejecutar app
flutter run
```

### iOS Simulator (Mac only)

```bash
# Abrir simulator
open -a Simulator

# Ejecutar app
flutter run
```

### Dispositivo Físico

```bash
# Ver dispositivos conectados
flutter devices

# Ejecutar en dispositivo específico
flutter run -d <device-id>
```

---

## 🔧 Comandos Útiles

### Desarrollo

```bash
# Ejecutar en modo debug (hot reload habilitado)
flutter run

# Ejecutar en modo release (optimizado)
flutter run --release

# Ejecutar en modo profile (para profiling)
flutter run --profile

# Hot reload (presionar 'r' en la terminal)
# Hot restart (presionar 'R' en la terminal)
```

### Testing

```bash
# Ejecutar todos los tests
flutter test

# Ejecutar test específico
flutter test test/widget_test.dart

# Ejecutar con coverage
flutter test --coverage

# Ver coverage en HTML
genhtml coverage/lcov.info -o coverage/html
```

### Build

```bash
# Android APK (debug)
flutter build apk --debug

# Android APK (release)
flutter build apk --release

# Android App Bundle (para Play Store)
flutter build appbundle --release

# iOS (Mac only)
flutter build ios --release
```

### Análisis

```bash
# Analizar código
flutter analyze

# Formatear código
flutter format lib/

# Verificar dependencias obsoletas
flutter pub outdated
```

### Generación de Código

```bash
# Generar código (primera vez)
flutter pub run build_runner build

# Generar código (limpiar conflictos)
flutter pub run build_runner build --delete-conflicting-outputs

# Watch mode (regenera automáticamente)
flutter pub run build_runner watch
```

---

## 🐛 Troubleshooting

### Problema: "Flutter not found"

```bash
# Verificar PATH
echo $env:PATH  # PowerShell
echo $PATH      # Bash

# Agregar Flutter al PATH (Windows)
setx PATH "%PATH%;C:\flutter\bin"
```

### Problema: "No devices found"

```bash
# Android: Verificar ADB
adb devices

# Reiniciar ADB server
adb kill-server
adb start-server

# iOS: Verificar Xcode
xcode-select --install
```

### Problema: "Pub get failed"

```bash
# Limpiar cache
flutter clean
flutter pub cache repair

# Reinstalar dependencias
flutter pub get
```

### Problema: "Build failed"

```bash
# Limpiar todo
flutter clean

# Regenerar código
flutter pub run build_runner clean
flutter pub run build_runner build --delete-conflicting-outputs

# Rebuild
flutter run
```

### Problema: Errores de compilación en Android

```bash
# Navegar a android/
cd android

# Limpiar Gradle
./gradlew clean

# Rebuild
./gradlew build

# Volver a root y ejecutar
cd ..
flutter run
```

---

## 📝 VS Code Launch Configuration

Crear `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "CarDealer Mobile (Debug)",
      "request": "launch",
      "type": "dart",
      "program": "lib/main.dart"
    },
    {
      "name": "CarDealer Mobile (Profile)",
      "request": "launch",
      "type": "dart",
      "flutterMode": "profile",
      "program": "lib/main.dart"
    },
    {
      "name": "CarDealer Mobile (Release)",
      "request": "launch",
      "type": "dart",
      "flutterMode": "release",
      "program": "lib/main.dart"
    }
  ]
}
```

Luego presionar **F5** para ejecutar.

---

## 🔥 Hot Reload

Durante el desarrollo, usa hot reload para ver cambios instantáneamente:

1. Hacer cambios en el código
2. Presionar `r` en la terminal (hot reload)
3. O presionar `R` (hot restart - reinicio completo)
4. O guardar el archivo en VS Code (auto hot reload)

---

## 📊 Performance Profiling

```bash
# Ejecutar en modo profile
flutter run --profile

# Abrir DevTools
flutter pub global activate devtools
flutter pub global run devtools

# O desde VS Code
# View > Command Palette > "Flutter: Open DevTools"
```

---

## 🌐 Cambiar Idioma

El idioma se detecta automáticamente del sistema. Para probar diferentes idiomas:

**Android:**
- Settings > System > Languages & input > Languages

**iOS:**
- Settings > General > Language & Region

O modificar el código temporalmente en `main.dart`:

```dart
MaterialApp(
  locale: const Locale('es'),  // Forzar español
  // locale: const Locale('en'),  // Forzar inglés
  ...
)
```

---

## 📱 Conectar Backend

Por defecto, la app apunta a `http://localhost:5000` (desarrollo).

Para cambiar:

1. Editar `lib/core/constants/api_constants.dart`
2. Cambiar `baseUrl` según el ambiente:

```dart
// Desarrollo local
static const String baseUrl = 'http://localhost:5000';

// Staging
static const String baseUrl = 'https://staging-api.cardealer.com';

// Producción
static const String baseUrl = 'https://api.cardealer.com';
```

---

## 📞 Ayuda

Si tienes problemas:

1. Revisar `flutter doctor` para verificar setup
2. Verificar logs con `flutter logs`
3. Buscar en [Stack Overflow](https://stackoverflow.com/questions/tagged/flutter)
4. Consultar [Flutter Documentation](https://flutter.dev/docs)

---

**Happy Coding! 🚀**
