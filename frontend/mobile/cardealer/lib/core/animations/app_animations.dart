/// Core animation definitions for the CarDealer app
///
/// This file contains all standard animations, transitions, and
/// micro-interactions used throughout the app for consistency.
library;

import 'package:flutter/material.dart';

/// Standard animation durations for consistency
class AnimationDurations {
  static const Duration fastest = Duration(milliseconds: 150);
  static const Duration fast = Duration(milliseconds: 250);
  static const Duration normal = Duration(milliseconds: 350);
  static const Duration slow = Duration(milliseconds: 500);
  static const Duration slowest = Duration(milliseconds: 700);
}

/// Standard animation curves
class AnimationCurves {
  static const Curve easeIn = Curves.easeIn;
  static const Curve easeOut = Curves.easeOut;
  static const Curve easeInOut = Curves.easeInOut;
  static const Curve bounce = Curves.bounceOut;
  static const Curve elastic = Curves.elasticOut;
  static const Curve standard = Curves.fastOutSlowIn;
}

/// Custom page transitions
class AppPageTransitions {
  /// Fade transition
  static PageRouteBuilder fadeTransition(Widget page) {
    return PageRouteBuilder(
      pageBuilder: (context, animation, secondaryAnimation) => page,
      transitionsBuilder: (context, animation, secondaryAnimation, child) {
        return FadeTransition(
          opacity: animation,
          child: child,
        );
      },
      transitionDuration: AnimationDurations.normal,
    );
  }

  /// Slide from right transition (default iOS-like)
  static PageRouteBuilder slideFromRight(Widget page) {
    return PageRouteBuilder(
      pageBuilder: (context, animation, secondaryAnimation) => page,
      transitionsBuilder: (context, animation, secondaryAnimation, child) {
        const begin = Offset(1.0, 0.0);
        const end = Offset.zero;
        const curve = Curves.fastOutSlowIn;

        var tween = Tween(begin: begin, end: end).chain(
          CurveTween(curve: curve),
        );

        return SlideTransition(
          position: animation.drive(tween),
          child: child,
        );
      },
      transitionDuration: AnimationDurations.normal,
    );
  }

  /// Slide from bottom transition (modal-like)
  static PageRouteBuilder slideFromBottom(Widget page) {
    return PageRouteBuilder(
      pageBuilder: (context, animation, secondaryAnimation) => page,
      transitionsBuilder: (context, animation, secondaryAnimation, child) {
        const begin = Offset(0.0, 1.0);
        const end = Offset.zero;
        const curve = Curves.fastOutSlowIn;

        var tween = Tween(begin: begin, end: end).chain(
          CurveTween(curve: curve),
        );

        return SlideTransition(
          position: animation.drive(tween),
          child: child,
        );
      },
      transitionDuration: AnimationDurations.normal,
    );
  }

  /// Scale transition (zoom in)
  static PageRouteBuilder scaleTransition(Widget page) {
    return PageRouteBuilder(
      pageBuilder: (context, animation, secondaryAnimation) => page,
      transitionsBuilder: (context, animation, secondaryAnimation, child) {
        const curve = Curves.fastOutSlowIn;

        var tween = Tween(begin: 0.8, end: 1.0).chain(
          CurveTween(curve: curve),
        );

        return FadeTransition(
          opacity: animation,
          child: ScaleTransition(
            scale: animation.drive(tween),
            child: child,
          ),
        );
      },
      transitionDuration: AnimationDurations.normal,
    );
  }

  /// Combined fade + slide transition
  static PageRouteBuilder fadeAndSlide(Widget page) {
    return PageRouteBuilder(
      pageBuilder: (context, animation, secondaryAnimation) => page,
      transitionsBuilder: (context, animation, secondaryAnimation, child) {
        const begin = Offset(0.0, 0.05);
        const end = Offset.zero;
        const curve = Curves.fastOutSlowIn;

        var slideTween = Tween(begin: begin, end: end).chain(
          CurveTween(curve: curve),
        );

        return FadeTransition(
          opacity: animation,
          child: SlideTransition(
            position: animation.drive(slideTween),
            child: child,
          ),
        );
      },
      transitionDuration: AnimationDurations.normal,
    );
  }
}

/// Micro-interaction animations
class MicroAnimations {
  /// Pulse animation for highlighting elements
  static Animation<double> pulse(AnimationController controller) {
    return Tween<double>(begin: 1.0, end: 1.15).animate(
      CurvedAnimation(
        parent: controller,
        curve: Curves.easeInOut,
      ),
    );
  }

  /// Shake animation for errors
  static Animation<double> shake(AnimationController controller) {
    return TweenSequence<double>([
      TweenSequenceItem(tween: Tween(begin: 0.0, end: 10.0), weight: 1),
      TweenSequenceItem(tween: Tween(begin: 10.0, end: -10.0), weight: 1),
      TweenSequenceItem(tween: Tween(begin: -10.0, end: 10.0), weight: 1),
      TweenSequenceItem(tween: Tween(begin: 10.0, end: -10.0), weight: 1),
      TweenSequenceItem(tween: Tween(begin: -10.0, end: 0.0), weight: 1),
    ]).animate(controller);
  }

  /// Bounce animation for success
  static Animation<double> bounce(AnimationController controller) {
    return Tween<double>(begin: 0.8, end: 1.0).animate(
      CurvedAnimation(
        parent: controller,
        curve: Curves.bounceOut,
      ),
    );
  }

  /// Fade in animation
  static Animation<double> fadeIn(AnimationController controller) {
    return Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: controller,
        curve: Curves.easeIn,
      ),
    );
  }

  /// Slide up animation
  static Animation<Offset> slideUp(AnimationController controller) {
    return Tween<Offset>(
      begin: const Offset(0.0, 0.3),
      end: Offset.zero,
    ).animate(
      CurvedAnimation(
        parent: controller,
        curve: Curves.fastOutSlowIn,
      ),
    );
  }
}

/// Staggered animation helpers
class StaggeredAnimations {
  /// Creates staggered animations for list items
  static Animation<double> createStaggeredAnimation({
    required AnimationController controller,
    required int index,
    required int totalItems,
    Duration? duration,
  }) {
    final itemDuration = duration ?? AnimationDurations.normal;
    final totalDuration = controller.duration!;
    final delayPerItem = (totalDuration.inMilliseconds / totalItems).round();
    final startTime =
        (index * delayPerItem).clamp(0, totalDuration.inMilliseconds);

    return Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: controller,
        curve: Interval(
          startTime / totalDuration.inMilliseconds,
          ((startTime + itemDuration.inMilliseconds) /
                  totalDuration.inMilliseconds)
              .clamp(0.0, 1.0),
          curve: Curves.fastOutSlowIn,
        ),
      ),
    );
  }
}

/// Hero animation tags for consistency
class HeroTags {
  static String vehicleImage(String vehicleId) => 'vehicle_image_$vehicleId';
  static String vehiclePrice(String vehicleId) => 'vehicle_price_$vehicleId';
  static String vehicleTitle(String vehicleId) => 'vehicle_title_$vehicleId';
  static String dealerLogo(String dealerId) => 'dealer_logo_$dealerId';
  static String profileAvatar(String userId) => 'profile_avatar_$userId';
  static String favoriteIcon(String vehicleId) => 'favorite_icon_$vehicleId';
}

/// Animated widgets builders
class AnimatedBuilders {
  /// Builds a fade-in list item
  static Widget fadeInListItem({
    required Widget child,
    required Animation<double> animation,
  }) {
    return FadeTransition(
      opacity: animation,
      child: SlideTransition(
        position: Tween<Offset>(
          begin: const Offset(0.0, 0.1),
          end: Offset.zero,
        ).animate(animation),
        child: child,
      ),
    );
  }

  /// Builds a scale-in item
  static Widget scaleInItem({
    required Widget child,
    required Animation<double> animation,
  }) {
    return ScaleTransition(
      scale: Tween<double>(begin: 0.8, end: 1.0).animate(
        CurvedAnimation(
          parent: animation,
          curve: Curves.fastOutSlowIn,
        ),
      ),
      child: FadeTransition(
        opacity: animation,
        child: child,
      ),
    );
  }
}

/// Loading animation widgets
class LoadingAnimations {
  /// Creates a pulsing loading indicator
  static Widget pulsingDot({Color? color}) {
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0.5, end: 1.0),
      duration: AnimationDurations.slow,
      curve: Curves.easeInOut,
      builder: (context, value, child) {
        return Opacity(
          opacity: value,
          child: Transform.scale(
            scale: value,
            child: Container(
              width: 8,
              height: 8,
              decoration: BoxDecoration(
                color: color ?? Theme.of(context).primaryColor,
                shape: BoxShape.circle,
              ),
            ),
          ),
        );
      },
      onEnd: () {
        // Animation repeats automatically via TweenAnimationBuilder rebuild
      },
    );
  }

  /// Creates a shimmer loading effect
  static Widget shimmerLoading({
    required double width,
    required double height,
    BorderRadius? borderRadius,
  }) {
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: -2.0, end: 2.0),
      duration: AnimationDurations.slowest,
      builder: (context, value, child) {
        return Container(
          width: width,
          height: height,
          decoration: BoxDecoration(
            borderRadius: borderRadius ?? BorderRadius.circular(4),
            gradient: LinearGradient(
              begin: Alignment.centerLeft,
              end: Alignment.centerRight,
              colors: [
                Colors.grey[300]!,
                Colors.grey[100]!,
                Colors.grey[300]!,
              ],
              stops: [
                (value - 1.0).clamp(0.0, 1.0),
                value.clamp(0.0, 1.0),
                (value + 1.0).clamp(0.0, 1.0),
              ],
            ),
          ),
        );
      },
      onEnd: () {
        // Repeats automatically
      },
    );
  }
}
