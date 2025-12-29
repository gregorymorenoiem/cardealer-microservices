import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:cardealer_mobile/presentation/widgets/custom_button.dart';
import 'package:cardealer_mobile/presentation/widgets/custom_text_field.dart';
import 'package:cardealer_mobile/core/theme/app_theme.dart';

void main() {
  group('Widget Tests', () {
    testWidgets('CustomButton should render correctly',
        (WidgetTester tester) async {
      bool pressed = false;

      await tester.pumpWidget(
        MaterialApp(
          theme: AppTheme.lightTheme,
          home: Scaffold(
            body: CustomButton(
              text: 'Test Button',
              onPressed: () => pressed = true,
            ),
          ),
        ),
      );

      expect(find.text('Test Button'), findsOneWidget);

      await tester.tap(find.text('Test Button'));
      await tester.pump();

      expect(pressed, isTrue);
    });

    testWidgets('CustomTextField should accept input',
        (WidgetTester tester) async {
      final controller = TextEditingController();

      await tester.pumpWidget(
        MaterialApp(
          theme: AppTheme.lightTheme,
          home: Scaffold(
            body: CustomTextField(
              hintText: 'Test Field',
              controller: controller,
            ),
          ),
        ),
      );

      await tester.enterText(find.byType(TextField), 'Test Input');
      expect(controller.text, 'Test Input');

      controller.dispose();
    });

    testWidgets('AppTheme should provide correct colors',
        (WidgetTester tester) async {
      await tester.pumpWidget(
        MaterialApp(
          theme: AppTheme.lightTheme,
          home: const Scaffold(
            body: Text('Theme Test'),
          ),
        ),
      );

      final context = tester.element(find.text('Theme Test'));
      final theme = Theme.of(context);

      expect(theme.primaryColor, isNotNull);
      expect(theme.colorScheme.primary, isNotNull);
    });
  });

  group('Performance Tests', () {
    testWidgets('App should render within acceptable time',
        (WidgetTester tester) async {
      final stopwatch = Stopwatch()..start();

      await tester.pumpWidget(
        MaterialApp(
          theme: AppTheme.lightTheme,
          home: const Scaffold(
            body: Center(child: Text('Performance Test')),
          ),
        ),
      );

      stopwatch.stop();

      // Should render in less than 100ms
      expect(stopwatch.elapsedMilliseconds, lessThan(100));
    });
  });
}
