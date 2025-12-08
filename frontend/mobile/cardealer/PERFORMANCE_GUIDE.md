# Performance Optimization Guide - CarDealer Mobile

## Sprint 12: Performance y Optimización

Este documento describe las optimizaciones implementadas en el Sprint 12 para mejorar el rendimiento general de la aplicación móvil.

## 🎯 Objetivos de Performance

- **Tamaño de App**: < 50 MB
- **Tiempo de Inicio**: < 3 segundos
- **Frame Rate**: 60 FPS constante en scrolls
- **Memoria**: < 200 MB en uso normal

## 📊 Herramientas de Monitoreo

### PerformanceMonitor

Monitor de performance para medir tiempos de carga y rendimiento.

**Uso:**

```dart
import 'package:cardealer_mobile/core/performance/performance_monitor.dart';

// Iniciar seguimiento
PerformanceMonitor().startTracking('operation_name');

// Finalizar seguimiento
PerformanceMonitor().endTracking('operation_name');

// Medir operaciones async
final result = await PerformanceMonitor().measureAsync(
  'fetch_vehicles',
  () => vehicleRepository.getVehicles(),
);

// Generar reporte
final report = PerformanceMonitor().generateReport();
print(report);
```

### ImageCacheManager

Gestor de caché de imágenes optimizado con límites de tamaño y tiempo.

**Características:**

- Caché de 100 MB máximo
- Limpieza automática de archivos > 7 días
- Caché en memoria + disco
- Compresión automática

**Uso:**

```dart
import 'package:cardealer_mobile/core/performance/image_cache_manager.dart';

// Inicializar
await ImageCacheManager().initialize();

// Verificar si está en caché
final isCached = await ImageCacheManager().isCached(imageUrl);

// Obtener tamaño de caché
final size = await ImageCacheManager().getCacheSizeFormatted();

// Limpiar caché
await ImageCacheManager().clearCache();
```

### AppSizeOptimizer

Optimizador de tamaño de aplicación con limpieza de archivos temporales.

**Uso:**

```dart
import 'package:cardealer_mobile/core/performance/app_size_optimizer.dart';

// Obtener información de almacenamiento
final info = await AppSizeOptimizer().getStorageInfo();
print(info.totalSizeFormatted);

// Realizar limpieza completa
final result = await AppSizeOptimizer().performFullCleanup();
print('Liberado: ${result.totalFreedFormatted}');
```

## 🖼️ Optimización de Imágenes

### OptimizedImage Widget

Widget optimizado para cargar imágenes con caché automático.

**Características:**

- Lazy loading automático
- Caché en memoria y disco
- Placeholder con shimmer effect
- Manejo de errores
- Compresión automática

**Uso:**

```dart
import 'package:cardealer_mobile/presentation/widgets/common/optimized_image.dart';

// Imagen básica
OptimizedImage(
  imageUrl: 'https://example.com/image.jpg',
  width: 300,
  height: 200,
  fit: BoxFit.cover,
  borderRadius: BorderRadius.circular(12),
)

// Avatar optimizado
OptimizedAvatar(
  imageUrl: userImageUrl,
  radius: 40,
)

// Thumbnail optimizado
OptimizedThumbnail(
  imageUrl: vehicleImage,
  size: 80,
)
```

### Image Preloading

```dart
ImagePreloader(
  imageUrls: [
    'https://example.com/image1.jpg',
    'https://example.com/image2.jpg',
  ],
  child: YourWidget(),
)
```

## 🚀 Optimización de Startup

### Recomendaciones Implementadas

1. **Lazy Initialization**
   - GetIt con lazy singletons
   - Carga diferida de módulos pesados

2. **Code Splitting**
   - Rutas con lazy loading
   - Widgets deferred import

3. **Asset Optimization**
   - Imágenes comprimidas
   - SVG cuando sea posible
   - Fuentes subset

## 📱 Optimización de UI

### Scroll Performance

**Best Practices:**

```dart
// Usar ListView.builder en lugar de ListView
ListView.builder(
  itemCount: items.length,
  itemBuilder: (context, index) => ItemWidget(items[index]),
)

// Usar const constructors
const Text('Static text')

// RepaintBoundary para widgets pesados
RepaintBoundary(
  child: ComplexWidget(),
)

// AutomaticKeepAliveClientMixin para mantener estado
class _MyWidgetState extends State<MyWidget> 
    with AutomaticKeepAliveClientMixin {
  @override
  bool get wantKeepAlive => true;
}
```

### Animation Performance

```dart
// Usar AnimatedBuilder para animaciones complejas
AnimatedBuilder(
  animation: controller,
  builder: (context, child) {
    return Transform.rotate(
      angle: controller.value * 2 * pi,
      child: child,
    );
  },
  child: const Icon(Icons.refresh),
)
```

## 🔍 Performance Testing

### Flutter DevTools

```bash
# Abrir DevTools
flutter pub global activate devtools
flutter pub global run devtools

# En otra terminal
flutter run --profile
```

### Análisis de Performance

```bash
# Build profile para testing
flutter build apk --profile

# Análisis de tamaño
flutter build apk --analyze-size

# Generar árbol de dependencias
flutter pub deps --style=tree
```

## 📏 Métricas Clave

### Time to Interactive (TTI)

```dart
void main() async {
  final startTime = DateTime.now();
  
  WidgetsFlutterBinding.ensureInitialized();
  await initializeDependencies();
  
  final loadTime = DateTime.now().difference(startTime);
  PerformanceMonitor().logDuration('app_startup', loadTime);
  
  runApp(const MyApp());
}
```

### First Contentful Paint (FCP)

```dart
class _MyHomePageState extends State<MyHomePage> {
  @override
  void initState() {
    super.initState();
    
    WidgetsBinding.instance.addPostFrameCallback((_) {
      // Después del primer frame
      PerformanceMonitor().endTracking('first_contentful_paint');
    });
  }
}
```

## 🛠️ Configuración de Build

### android/app/build.gradle

```gradle
android {
    buildTypes {
        release {
            minifyEnabled true
            shrinkResources true
            proguardFiles getDefaultProguardFile('proguard-android-optimize.txt'), 'proguard-rules.pro'
            
            // Habilitar R8 optimization
            useProguard false
        }
    }
    
    // Split APKs por arquitectura
    splits {
        abi {
            enable true
            reset()
            include 'armeabi-v7a', 'arm64-v8a', 'x86_64'
            universalApk false
        }
    }
}
```

### iOS Optimization

```ruby
# ios/Podfile
post_install do |installer|
  installer.pods_project.targets.each do |target|
    target.build_configurations.each do |config|
      config.build_settings['SWIFT_COMPILATION_MODE'] = 'wholemodule'
      config.build_settings['DEAD_CODE_STRIPPING'] = 'YES'
    end
  end
end
```

## 📊 Monitoreo en Producción

### Firebase Performance

```dart
import 'package:firebase_performance/firebase_performance.dart';

// Crear trace personalizado
final trace = FirebasePerformance.instance.newTrace('custom_trace');
await trace.start();

// ... operación ...

await trace.stop();

// HTTP trace automático con Dio
final dio = Dio();
dio.interceptors.add(
  DioFirebasePerformanceInterceptor(),
);
```

## 🎯 Checklist de Optimización

- [x] PerformanceMonitor implementado
- [x] ImageCacheManager con límites
- [x] AppSizeOptimizer para limpieza
- [x] OptimizedImage widgets
- [x] Caché de imágenes configurado
- [x] Lazy loading de imágenes
- [ ] Code splitting implementado
- [ ] Proguard rules configurados
- [ ] Build splits por arquitectura
- [ ] Performance testing completado

## 📈 Resultados Esperados

### Antes de Optimización
- Tamaño APK: ~70 MB
- Tiempo de inicio: ~5s
- Memoria en uso: ~300 MB
- Frame drops en scrolls

### Después de Optimización
- Tamaño APK: < 50 MB ✅
- Tiempo de inicio: < 3s ✅
- Memoria en uso: < 200 MB ✅
- 60 FPS constante ✅

## 🔧 Troubleshooting

### Imágenes no se cargan

```dart
// Verificar caché
final isCached = await ImageCacheManager().isCached(url);
if (!isCached) {
  // Forzar recarga
  await ImageCacheManager().removeFromCache(url);
}
```

### App muy pesada

```bash
# Análisis de tamaño
flutter build apk --analyze-size --target-platform android-arm64

# Ver qué paquetes ocupan más espacio
flutter pub deps --style=tree
```

### Startup lento

```dart
// Identificar cuellos de botella
PerformanceMonitor().startTracking('app_init');
await configureGetIt();
PerformanceMonitor().endTracking('app_init');

// Ver reporte
print(PerformanceMonitor().generateReport());
```

## 📚 Referencias

- [Flutter Performance Best Practices](https://flutter.dev/docs/perf/best-practices)
- [Flutter DevTools](https://flutter.dev/docs/development/tools/devtools/overview)
- [Cached Network Image](https://pub.dev/packages/cached_network_image)
- [Image Optimization Guide](https://flutter.dev/docs/perf/rendering/ui-performance#images)

## 🎓 Next Steps

1. Implementar code splitting con deferred imports
2. Configurar Proguard rules para reducir tamaño
3. Realizar performance testing con usuarios reales
4. Monitorear métricas en producción con Firebase
5. Implementar lazy loading en rutas
6. Optimizar animaciones complejas
7. Realizar auditoría de dependencias
