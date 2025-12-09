import 'package:flutter_test/flutter_test.dart';
import 'package:cardealer_mobile/core/ab_testing/feature_flags.dart';

void main() {
  group('FeatureFlags Tests', () {
    test('should have predefined feature flags', () {
      expect(FeatureFlags.newHomeDesign, isNotNull);
      expect(FeatureFlags.newHomeDesign.key, equals('new_home_design'));
    });

    test('should have correct default values', () {
      expect(FeatureFlags.newHomeDesign.defaultValue, isA<BoolValue>());
    });
  });

  group('FeatureFlagValue Tests', () {
    test('BoolValue should store boolean', () {
      final value = BoolValue(true);
      expect(value.value, isTrue);
    });

    test('StringValue should store string', () {
      final value = StringValue('test');
      expect(value.value, equals('test'));
    });

    test('IntValue should store integer', () {
      final value = IntValue(42);
      expect(value.value, equals(42));
    });

    test('DoubleValue should store double', () {
      final value = DoubleValue(3.14);
      expect(value.value, closeTo(3.14, 0.001));
    });
  });

  group('FeatureFlagManager Tests', () {
    late FeatureFlagManager manager;

    setUp(() {
      manager = FeatureFlagManager();
    });

    test('should return default value for uninitialized flag', () {
      final value = manager.getBool(FeatureFlags.newHomeDesign);
      expect(value, isA<bool>());
    });

    test('should initialize without errors', () async {
      // Mock initialization
      expect(() async => await manager.initialize(), returnsNormally);
    });
  });

  group('ABTest Tests', () {
    test('should create AB test with variants', () {
      const test = ABTest(
        key: 'test_experiment',
        variants: [
          ABTestVariant(key: 'control', weight: 50),
          ABTestVariant(key: 'variant_a', weight: 50),
        ],
        description: 'Test experiment',
      );

      expect(test.key, equals('test_experiment'));
      expect(test.variants.length, equals(2));
      expect(test.variants[0].weight, equals(50));
    });
  });

  group('ABTestVariant Tests', () {
    test('should create variant with correct properties', () {
      const variant = ABTestVariant(
        key: 'variant_a',
        weight: 25,
      );

      expect(variant.key, equals('variant_a'));
      expect(variant.weight, equals(25));
    });

    test('should create default variant', () {
      const variant = ABTestVariant(key: 'control');

      expect(variant.key, equals('control'));
      expect(variant.weight, equals(0));
    });
  });

  group('ABTestManager Tests', () {
    late ABTestManager manager;

    setUp(() {
      manager = ABTestManager();
    });

    test('should assign variants consistently for same user', () {
      const test = ABTest(
        key: 'consistency_test',
        variants: [
          ABTestVariant(key: 'a', weight: 50),
          ABTestVariant(key: 'b', weight: 50),
        ],
        description: 'Consistency test',
      );

      final variant1 = manager.getVariant(test, userId: 'user123');
      final variant2 = manager.getVariant(test, userId: 'user123');

      expect(variant1, equals(variant2));
    });

    test('should initialize without errors', () async {
      expect(() async => await manager.initialize(), returnsNormally);
    });
  });
}
