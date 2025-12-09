# Lottie Animation Assets

This directory contains Lottie animation JSON files for the CarDealer mobile app.

## Required Animations

To complete the Sprint 1 implementation, please add the following Lottie animation files to this directory:

### Core Animations
1. **loading.json** - Loading spinner/progress animation
2. **success.json** - Success checkmark animation
3. **error.json** - Error/alert animation
4. **empty.json** - Empty state animation (no results)

### Feature-Specific Animations
5. **search.json** - Search/magnifying glass animation
6. **car.json** - Vehicle/car animation for vehicle-related screens
7. **premium.json** - Premium/gold star/badge animation
8. **verified.json** - Verified checkmark with sparkles animation

## Where to Find Lottie Animations

You can download free Lottie animations from:
- [LottieFiles](https://lottiefiles.com/) - Primary source for Lottie animations
- [Iconscout](https://iconscout.com/lottie-animations) - Alternative source

## Animation Guidelines

- **File format**: JSON
- **File size**: Keep under 100KB per animation for performance
- **Duration**: 1-3 seconds for most animations
- **Colors**: Match app color palette (deep blue #1E3A5F, orange #FF6B35, gold #FFB800)
- **Loop**: Most animations should loop except success/error states

## Usage Example

```dart
import 'package:cardealer_mobile/presentation/widgets/animations/lottie_animation.dart';

// Loading animation
LottieAnimation.loading(size: 100)

// Success animation
LottieAnimation.success(
  size: 150,
  onLoaded: () => print('Animation loaded'),
)

// Custom animation
LottieAnimation(
  assetPath: 'assets/animations/custom.json',
  width: 200,
  height: 200,
)
```

## Next Steps

1. Download appropriate Lottie animations from LottieFiles
2. Rename files according to the list above
3. Place JSON files in this directory
4. Test animations in the app using `LottieAnimation` widget
5. Verify performance on target devices

## Notes

- The app already includes the `lottie: ^3.1.2` dependency in `pubspec.yaml`
- The `LottieAnimation` wrapper widget provides error handling and loading states
- Animations are automatically preloaded for better performance
