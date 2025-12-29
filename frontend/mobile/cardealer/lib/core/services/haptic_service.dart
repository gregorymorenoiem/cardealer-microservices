import 'package:flutter/services.dart';

/// Haptic feedback service for tactile user interactions
/// Provides consistent haptic feedback across the app for better UX
class HapticService {
  HapticService._();

  /// Light haptic feedback for subtle interactions
  /// Use for: hover states, small actions, minor confirmations
  static Future<void> light() async {
    await HapticFeedback.lightImpact();
  }

  /// Medium haptic feedback for standard interactions
  /// Use for: button taps, list item selections, toggle switches
  static Future<void> medium() async {
    await HapticFeedback.mediumImpact();
  }

  /// Heavy haptic feedback for significant interactions
  /// Use for: major actions, confirmations, important decisions
  static Future<void> heavy() async {
    await HapticFeedback.heavyImpact();
  }

  /// Selection haptic feedback for scrolling/sliding interactions
  /// Use for: pickers, sliders, scrolling through options
  static Future<void> selection() async {
    await HapticFeedback.selectionClick();
  }

  /// Success haptic feedback pattern
  /// Use for: successful operations, confirmations, positive feedback
  static Future<void> success() async {
    await HapticFeedback.mediumImpact();
    await Future.delayed(const Duration(milliseconds: 100));
    await HapticFeedback.lightImpact();
  }

  /// Error haptic feedback pattern
  /// Use for: errors, failed operations, validation failures
  static Future<void> error() async {
    await HapticFeedback.heavyImpact();
    await Future.delayed(const Duration(milliseconds: 100));
    await HapticFeedback.heavyImpact();
  }

  /// Warning haptic feedback pattern
  /// Use for: warnings, cautions, important notifications
  static Future<void> warning() async {
    await HapticFeedback.mediumImpact();
    await Future.delayed(const Duration(milliseconds: 80));
    await HapticFeedback.mediumImpact();
  }

  /// Vibration feedback (longer duration)
  /// Use for: notifications, alerts, background events
  static Future<void> vibrate() async {
    await HapticFeedback.vibrate();
  }

  /// Button press haptic feedback
  /// Standard feedback for button interactions
  static Future<void> buttonPress() async {
    await medium();
  }

  /// Premium action haptic feedback
  /// Special feedback for premium/gold features
  static Future<void> premiumAction() async {
    await selection();
    await Future.delayed(const Duration(milliseconds: 50));
    await medium();
  }

  /// Swipe action haptic feedback
  /// Feedback for swipe gestures (delete, archive, etc.)
  static Future<void> swipe() async {
    await light();
  }

  /// Long press haptic feedback
  /// Feedback for long press interactions
  static Future<void> longPress() async {
    await heavy();
  }

  /// Refresh haptic feedback
  /// Feedback for pull-to-refresh actions
  static Future<void> refresh() async {
    await light();
    await Future.delayed(const Duration(milliseconds: 50));
    await medium();
  }
}
