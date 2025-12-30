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

    test('should have description', () {
      expect(FeatureFlags.newHomeDesign.description, isNotEmpty);
    });

    test('all flags should be defined', () {
      expect(FeatureFlags.all, isNotEmpty);
      expect(FeatureFlags.all.length, greaterThan(5));
    });
  });

  group('FeatureFlagValue Tests', () {
    test('BoolValue should store boolean', () {
      const value = BoolValue(true);
      expect(value.value, isTrue);
    });

    test('StringValue should store string', () {
      const value = StringValue('test');
      expect(value.value, equals('test'));
    });

    test('IntValue should store integer', () {
      const value = IntValue(42);
      expect(value.value, equals(42));
    });

    test('DoubleValue should store double', () {
      const value = DoubleValue(3.14);
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

    test('should get int value', () {
      final value = manager.getInt(FeatureFlags.maxItemsInCart);
      expect(value, isA<int>());
    });

    test('should get double value', () {
      final value = manager.getDouble(FeatureFlags.minOrderAmount);
      expect(value, isA<double>());
    });
  });

  group('ABTestVariant Tests', () {
    test('should have control variant', () {
      expect(ABTestVariant.control, isNotNull);
      expect(ABTestVariant.control.name, equals('control'));
    });

    test('should have variantA', () {
      expect(ABTestVariant.variantA, isNotNull);
      expect(ABTestVariant.variantA.name, equals('variantA'));
    });

    test('should have all variants', () {
      expect(ABTestVariant.values.length, equals(4));
    });
  });

  group('ABTest Tests', () {
    test('should create AB test with variants', () {
      const test = ABTest(
        key: 'test_experiment',
        variants: [
          ABTestVariant.control,
          ABTestVariant.variantA,
        ],
      );

      expect(test.key, equals('test_experiment'));
      expect(test.variants.length, equals(2));
      expect(test.defaultVariant, equals(ABTestVariant.control));
    });

    test('should have default variant', () {
      const test = ABTest(
        key: 'test',
        variants: [ABTestVariant.control],
        defaultVariant: ABTestVariant.variantB,
      );

      expect(test.defaultVariant, equals(ABTestVariant.variantB));
    });
  });

  group('ABTests Registry Tests', () {
    test('should have predefined tests', () {
      expect(ABTests.homeLayout, isNotNull);
      expect(ABTests.checkoutFlow, isNotNull);
      expect(ABTests.pricingDisplay, isNotNull);
    });

    test('should have all tests listed', () {
      expect(ABTests.all, isNotEmpty);
      expect(ABTests.all.length, equals(3));
    });
  });

  group('ABTestManager Tests', () {
    late ABTestManager manager;

    setUp(() {
      manager = ABTestManager();
      manager.clearAllAssignments();
    });

    test('should return variant for test', () {
      final variant = manager.getVariant(ABTests.homeLayout);
      expect(variant, isA<ABTestVariant>());
    });

    test('should allow setting variant override', () {
      manager.setVariant(ABTests.homeLayout, ABTestVariant.variantA);
      final variant = manager.getVariant(ABTests.homeLayout);
      expect(variant, equals(ABTestVariant.variantA));
    });

    test('should check if in specific variant', () {
      manager.setVariant(ABTests.homeLayout, ABTestVariant.variantB);
      expect(manager.isInVariant(ABTests.homeLayout, ABTestVariant.variantB), isTrue);
      expect(manager.isInVariant(ABTests.homeLayout, ABTestVariant.control), isFalse);
    });

    test('should clear assignment', () {
      manager.setVariant(ABTests.homeLayout, ABTestVariant.variantA);
      manager.clearAssignment(ABTests.homeLayout);
      // After clearing, should get default or re-assigned variant
      final variant = manager.getVariant(ABTests.homeLayout);
      expect(variant, isA<ABTestVariant>());
    });

    test('should get all assignments', () {
      final assignments = manager.getAllAssignments();
      expect(assignments, isA<Map<String, String>>());
    });
  });
}
