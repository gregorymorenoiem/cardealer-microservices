# Flutter Performance Configuration

## Build Configuration

### Android Release Build
```bash
flutter build apk --release --split-per-abi --obfuscate --split-debug-info=./debug-info
```

### iOS Release Build
```bash
flutter build ios --release --obfuscate --split-debug-info=./debug-info
```

## Performance Targets

- **App Size**: < 50 MB
- **Startup Time**: < 3 seconds
- **Memory Usage**: < 200 MB
- **Frame Rate**: 60 FPS

## Optimization Flags

### pubspec.yaml
```yaml
flutter:
  uses-material-design: true
  
  # Asset optimization
  assets:
    - assets/images/     # Usar WebP cuando sea posible
    - assets/icons/      # Usar SVG para iconos
  
  # Font subsetting
  fonts:
    - family: Inter
      fonts:
        - asset: assets/fonts/Inter-Regular.ttf
          weight: 400
```

### android/app/build.gradle
```gradle
android {
    buildTypes {
        release {
            minifyEnabled true
            shrinkResources true
            proguardFiles getDefaultProguardFile('proguard-android-optimize.txt'), 'proguard-rules.pro'
        }
    }
    
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

## Cache Configuration

### Image Cache
- **Max Size**: 100 MB
- **Max Age**: 7 days
- **Compression**: Automatic
- **Format**: WebP preferred

### Network Cache
- **Max Size**: 50 MB
- **Max Age**: 24 hours
- **Strategy**: Cache-first for images, network-first for API

## Performance Monitoring

### DevTools Commands
```bash
# Launch DevTools
flutter pub global activate devtools
flutter pub global run devtools

# Profile mode
flutter run --profile

# Analyze build size
flutter build apk --analyze-size
```

### Firebase Performance
- Enabled in production
- Custom traces for critical operations
- Network monitoring enabled

## Code Optimization

### Lazy Loading
- Routes with lazy loading
- Images with lazy loading
- Heavy dependencies with deferred imports

### Widget Optimization
- Use `const` constructors
- Implement `RepaintBoundary` for heavy widgets
- Use `ListView.builder` instead of `ListView`
- Implement `AutomaticKeepAliveClientMixin` when needed

### Memory Management
- Dispose controllers properly
- Cancel subscriptions
- Clear caches periodically
- Use weak references when appropriate

## Testing

### Performance Tests
```bash
# Run performance tests
flutter test test/performance/

# Profile memory usage
flutter run --profile --trace-systrace

# Measure frame rendering time
flutter run --profile --trace-skia
```

## Deployment Checklist

- [ ] Run `flutter analyze` with no issues
- [ ] Run performance tests
- [ ] Check app size < 50 MB
- [ ] Verify startup time < 3s
- [ ] Test on low-end devices
- [ ] Monitor memory usage
- [ ] Verify 60 FPS scrolling
- [ ] Test with slow network
- [ ] Verify cache is working
- [ ] Check Firebase Performance metrics
