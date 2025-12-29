import 'package:flutter/material.dart';
import 'package:lottie/lottie.dart';

/// Lottie animation wrapper with preloading and error handling
/// Provides consistent animation loading across the app
class LottieAnimation extends StatelessWidget {
  final String assetPath;
  final double? width;
  final double? height;
  final BoxFit? fit;
  final bool repeat;
  final bool reverse;
  final bool animate;
  final VoidCallback? onLoaded;
  final Widget? errorWidget;
  final Widget? loadingWidget;

  const LottieAnimation({
    super.key,
    required this.assetPath,
    this.width,
    this.height,
    this.fit,
    this.repeat = true,
    this.reverse = false,
    this.animate = true,
    this.onLoaded,
    this.errorWidget,
    this.loadingWidget,
  });

  /// Creates a loading animation
  factory LottieAnimation.loading({
    double? size = 100,
  }) {
    return LottieAnimation(
      assetPath: 'assets/animations/loading.json',
      width: size,
      height: size,
    );
  }

  /// Creates a success animation (checkmark)
  factory LottieAnimation.success({
    double? size = 100,
    VoidCallback? onLoaded,
  }) {
    return LottieAnimation(
      assetPath: 'assets/animations/success.json',
      width: size,
      height: size,
      repeat: false,
      onLoaded: onLoaded,
    );
  }

  /// Creates an error animation (cross or alert)
  factory LottieAnimation.error({
    double? size = 100,
    VoidCallback? onLoaded,
  }) {
    return LottieAnimation(
      assetPath: 'assets/animations/error.json',
      width: size,
      height: size,
      repeat: false,
      onLoaded: onLoaded,
    );
  }

  /// Creates an empty state animation
  factory LottieAnimation.empty({
    double? size = 200,
  }) {
    return LottieAnimation(
      assetPath: 'assets/animations/empty.json',
      width: size,
      height: size,
    );
  }

  /// Creates a search animation
  factory LottieAnimation.search({
    double? size = 150,
  }) {
    return LottieAnimation(
      assetPath: 'assets/animations/search.json',
      width: size,
      height: size,
    );
  }

  /// Creates a car animation (for vehicle-related screens)
  factory LottieAnimation.car({
    double? size = 200,
  }) {
    return LottieAnimation(
      assetPath: 'assets/animations/car.json',
      width: size,
      height: size,
    );
  }

  /// Creates a premium/gold animation (for premium features)
  factory LottieAnimation.premium({
    double? size = 150,
  }) {
    return LottieAnimation(
      assetPath: 'assets/animations/premium.json',
      width: size,
      height: size,
    );
  }

  /// Creates a verified animation (checkmark with sparkles)
  factory LottieAnimation.verified({
    double? size = 80,
  }) {
    return LottieAnimation(
      assetPath: 'assets/animations/verified.json',
      width: size,
      height: size,
      repeat: false,
    );
  }

  @override
  Widget build(BuildContext context) {
    return Lottie.asset(
      assetPath,
      width: width,
      height: height,
      fit: fit ?? BoxFit.contain,
      repeat: repeat,
      reverse: reverse,
      animate: animate,
      onLoaded: (composition) {
        onLoaded?.call();
      },
      errorBuilder: (context, error, stackTrace) {
        return errorWidget ??
            Center(
              child: Icon(
                Icons.error_outline,
                size: width ?? height ?? 50,
                color: Colors.grey,
              ),
            );
      },
      frameBuilder: (context, child, composition) {
        if (composition == null) {
          return loadingWidget ??
              Center(
                child: SizedBox(
                  width: width ?? height ?? 50,
                  height: height ?? width ?? 50,
                  child: const CircularProgressIndicator(
                    strokeWidth: 2,
                  ),
                ),
              );
        }
        return child;
      },
    );
  }
}
