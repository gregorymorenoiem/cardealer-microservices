import 'package:flutter_test/flutter_test.dart';
import 'package:cardealer_mobile/core/accessibility/accessibility_utils.dart';
import 'package:flutter/material.dart';

void main() {
  group('ContrastChecker Tests', () {
    test('should calculate correct luminance for black', () {
      final luminance = ContrastChecker.calculateLuminance(Colors.black);
      expect(luminance, closeTo(0.0, 0.01));
    });

    test('should calculate correct luminance for white', () {
      final luminance = ContrastChecker.calculateLuminance(Colors.white);
      expect(luminance, closeTo(1.0, 0.01));
    });

    test('should calculate contrast ratio correctly', () {
      final contrast = ContrastChecker.calculateContrast(
        Colors.black,
        Colors.white,
      );
      expect(contrast, greaterThan(20)); // High contrast
    });

    test('should meet WCAG AA for normal text with sufficient contrast', () {
      final meetsWCAG = ContrastChecker.meetsWCAG(
        Colors.black,
        Colors.white,
        largeText: false,
      );
      expect(meetsWCAG, isTrue);
    });

    test('should fail WCAG for low contrast colors', () {
      final meetsWCAG = ContrastChecker.meetsWCAG(
        Colors.grey[300]!,
        Colors.grey[200]!,
        largeText: false,
      );
      expect(meetsWCAG, isFalse);
    });

    test('should ensure contrast returns accessible color', () {
      final accessibleColor = ContrastChecker.ensureContrast(
        Colors.grey,
        Colors.grey,
        largeText: false,
      );
      // Should return either black or white for better contrast
      expect(
        accessibleColor == Colors.black || accessibleColor == Colors.white,
        isTrue,
      );
    });
  });

  group('TextScaleHelper Tests', () {
    test('should clamp scale to min value', () {
      final clamped = TextScaleHelper.clamp(0.5);
      expect(clamped, equals(A11yConstants.minTextScale));
    });

    test('should clamp scale to max value', () {
      final clamped = TextScaleHelper.clamp(5.0);
      expect(clamped, equals(A11yConstants.maxTextScale));
    });

    test('should not clamp normal scale', () {
      final clamped = TextScaleHelper.clamp(1.5);
      expect(clamped, equals(1.5));
    });
  });

  group('A11yAnnouncer Tests', () {
    test('should create announcer instance', () {
      expect(A11yAnnouncer(), isNotNull);
    });
  });

  group('TouchTargetWrapper Tests', () {
    testWidgets('should wrap child widget', (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: TouchTargetWrapper(
              child: Container(
                width: 20,
                height: 20,
                color: Colors.red,
              ),
            ),
          ),
        ),
      );

      final container = find.byType(Container);
      expect(container, findsOneWidget);
    });
  });

  group('AccessibleIconButton Tests', () {
    testWidgets('should create accessible icon button',
        (WidgetTester tester) async {
      var pressed = false;

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: AccessibleIconButton(
              icon: Icons.home,
              label: 'Home Button',
              onPressed: () => pressed = true,
            ),
          ),
        ),
      );

      final button = find.byType(IconButton);
      expect(button, findsOneWidget);

      await tester.tap(button);
      await tester.pumpAndSettle();

      expect(pressed, isTrue);
    });
  });

  group('AccessibleTextButton Tests', () {
    testWidgets('should create accessible text button',
        (WidgetTester tester) async {
      var pressed = false;

      await tester.pumpWidget(
        MaterialApp(
          home: Scaffold(
            body: AccessibleTextButton(
              text: 'Click Me',
              onPressed: () => pressed = true,
            ),
          ),
        ),
      );

      final button = find.byType(TextButton);
      expect(button, findsOneWidget);

      await tester.tap(button);
      await tester.pumpAndSettle();

      expect(pressed, isTrue);
    });
  });
}
