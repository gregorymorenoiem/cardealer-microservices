library;

/// A/B Testing and Feature Flags
/// Enables controlled feature rollouts and experimentation
import 'package:firebase_remote_config/firebase_remote_config.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

/// Feature flag value types
abstract class FeatureFlagValue {
  const FeatureFlagValue();
}

class BoolValue extends FeatureFlagValue {
  final bool value;
  const BoolValue(this.value);
}

class StringValue extends FeatureFlagValue {
  final String value;
  const StringValue(this.value);
}

class IntValue extends FeatureFlagValue {
  final int value;
  const IntValue(this.value);
}

class DoubleValue extends FeatureFlagValue {
  final double value;
  const DoubleValue(this.value);
}

/// Feature flag configuration
class FeatureFlag {
  final String key;
  final FeatureFlagValue defaultValue;
  final String description;

  const FeatureFlag({
    required this.key,
    required this.defaultValue,
    required this.description,
  });
}

/// Feature flags registry
class FeatureFlags {
  // UI Features
  static const newHomeDesign = FeatureFlag(
    key: 'new_home_design',
    defaultValue: BoolValue(false),
    description: 'Enable new home screen design',
  );

  static const darkModeEnabled = FeatureFlag(
    key: 'dark_mode_enabled',
    defaultValue: BoolValue(true),
    description: 'Enable dark mode support',
  );

  static const showRecommendations = FeatureFlag(
    key: 'show_recommendations',
    defaultValue: BoolValue(true),
    description: 'Show personalized recommendations',
  );

  // Features
  static const enableVoiceSearch = FeatureFlag(
    key: 'enable_voice_search',
    defaultValue: BoolValue(false),
    description: 'Enable voice search feature',
  );

  static const enableAR = FeatureFlag(
    key: 'enable_ar',
    defaultValue: BoolValue(false),
    description: 'Enable AR vehicle preview',
  );

  static const enableVideoChat = FeatureFlag(
    key: 'enable_video_chat',
    defaultValue: BoolValue(false),
    description: 'Enable video chat with dealers',
  );

  // Business Logic
  static const minOrderAmount = FeatureFlag(
    key: 'min_order_amount',
    defaultValue: DoubleValue(1000.0),
    description: 'Minimum order amount',
  );

  static const maxItemsInCart = FeatureFlag(
    key: 'max_items_in_cart',
    defaultValue: IntValue(10),
    description: 'Maximum items allowed in cart',
  );

  static const freeShippingThreshold = FeatureFlag(
    key: 'free_shipping_threshold',
    defaultValue: DoubleValue(5000.0),
    description: 'Free shipping threshold amount',
  );

  // Performance
  static const enableCaching = FeatureFlag(
    key: 'enable_caching',
    defaultValue: BoolValue(true),
    description: 'Enable client-side caching',
  );

  static const cacheExpirationHours = FeatureFlag(
    key: 'cache_expiration_hours',
    defaultValue: IntValue(24),
    description: 'Cache expiration in hours',
  );

  static const maxConcurrentRequests = FeatureFlag(
    key: 'max_concurrent_requests',
    defaultValue: IntValue(5),
    description: 'Max concurrent API requests',
  );

  // All flags list for iteration
  static const all = [
    newHomeDesign,
    darkModeEnabled,
    showRecommendations,
    enableVoiceSearch,
    enableAR,
    enableVideoChat,
    minOrderAmount,
    maxItemsInCart,
    freeShippingThreshold,
    enableCaching,
    cacheExpirationHours,
    maxConcurrentRequests,
  ];
}

/// Feature flag manager
class FeatureFlagManager {
  static final FeatureFlagManager _instance = FeatureFlagManager._internal();
  factory FeatureFlagManager() => _instance;
  FeatureFlagManager._internal();

  FirebaseRemoteConfig? _remoteConfig;
  final Map<String, FeatureFlagValue> _localOverrides = {};
  bool _initialized = false;

  bool get isInitialized => _initialized;

  Future<void> initialize() async {
    if (_initialized) return;

    try {
      _remoteConfig = FirebaseRemoteConfig.instance;

      await _remoteConfig!.setConfigSettings(
        RemoteConfigSettings(
          fetchTimeout: const Duration(seconds: 10),
          minimumFetchInterval: const Duration(hours: 1),
        ),
      );

      // Set defaults
      final defaults = <String, dynamic>{};
      for (final flag in FeatureFlags.all) {
        defaults[flag.key] = _getDefaultValue(flag.defaultValue);
      }
      await _remoteConfig!.setDefaults(defaults);

      // Fetch and activate
      await _remoteConfig!.fetchAndActivate();

      _initialized = true;

      if (kDebugMode) {
        debugPrint('Feature flags initialized');
        debugPrint(
            'All flags: ${FeatureFlags.all.map((f) => f.key).join(", ")}');
      }
    } catch (e) {
      debugPrint('Failed to initialize feature flags: $e');
      _initialized = true; // Use defaults
    }
  }

  dynamic _getDefaultValue(FeatureFlagValue value) {
    if (value is BoolValue) return value.value;
    if (value is StringValue) return value.value;
    if (value is IntValue) return value.value;
    if (value is DoubleValue) return value.value;
    return null;
  }

  bool getBool(FeatureFlag flag) {
    if (_localOverrides.containsKey(flag.key)) {
      final override = _localOverrides[flag.key];
      if (override is BoolValue) return override.value;
    }

    if (_remoteConfig != null && _initialized) {
      return _remoteConfig!.getBool(flag.key);
    }

    return (flag.defaultValue as BoolValue).value;
  }

  String getString(FeatureFlag flag) {
    if (_localOverrides.containsKey(flag.key)) {
      final override = _localOverrides[flag.key];
      if (override is StringValue) return override.value;
    }

    if (_remoteConfig != null && _initialized) {
      return _remoteConfig!.getString(flag.key);
    }

    return (flag.defaultValue as StringValue).value;
  }

  int getInt(FeatureFlag flag) {
    if (_localOverrides.containsKey(flag.key)) {
      final override = _localOverrides[flag.key];
      if (override is IntValue) return override.value;
    }

    if (_remoteConfig != null && _initialized) {
      return _remoteConfig!.getInt(flag.key);
    }

    return (flag.defaultValue as IntValue).value;
  }

  double getDouble(FeatureFlag flag) {
    if (_localOverrides.containsKey(flag.key)) {
      final override = _localOverrides[flag.key];
      if (override is DoubleValue) return override.value;
    }

    if (_remoteConfig != null && _initialized) {
      return _remoteConfig!.getDouble(flag.key);
    }

    return (flag.defaultValue as DoubleValue).value;
  }

  void setLocalOverride(String key, FeatureFlagValue value) {
    _localOverrides[key] = value;
    if (kDebugMode) {
      debugPrint('Feature flag override: $key = $value');
    }
  }

  void clearLocalOverride(String key) {
    _localOverrides.remove(key);
  }

  void clearAllOverrides() {
    _localOverrides.clear();
  }

  Future<void> refresh() async {
    if (_remoteConfig != null) {
      await _remoteConfig!.fetchAndActivate();
      if (kDebugMode) {
        debugPrint('Feature flags refreshed');
      }
    }
  }

  Map<String, dynamic> getAllFlags() {
    final flags = <String, dynamic>{};

    for (final flag in FeatureFlags.all) {
      if (flag.defaultValue is BoolValue) {
        flags[flag.key] = getBool(flag);
      } else if (flag.defaultValue is StringValue) {
        flags[flag.key] = getString(flag);
      } else if (flag.defaultValue is IntValue) {
        flags[flag.key] = getInt(flag);
      } else if (flag.defaultValue is DoubleValue) {
        flags[flag.key] = getDouble(flag);
      }
    }

    return flags;
  }
}

/// A/B Test variant
enum ABTestVariant {
  control,
  variantA,
  variantB,
  variantC,
}

/// A/B Test configuration
class ABTest {
  final String key;
  final List<ABTestVariant> variants;
  final ABTestVariant defaultVariant;

  const ABTest({
    required this.key,
    required this.variants,
    this.defaultVariant = ABTestVariant.control,
  });
}

/// A/B Tests registry
class ABTests {
  static const homeLayout = ABTest(
    key: 'home_layout_test',
    variants: [
      ABTestVariant.control,
      ABTestVariant.variantA,
      ABTestVariant.variantB,
    ],
  );

  static const checkoutFlow = ABTest(
    key: 'checkout_flow_test',
    variants: [
      ABTestVariant.control,
      ABTestVariant.variantA,
    ],
  );

  static const pricingDisplay = ABTest(
    key: 'pricing_display_test',
    variants: [
      ABTestVariant.control,
      ABTestVariant.variantA,
      ABTestVariant.variantB,
    ],
  );

  static const all = [
    homeLayout,
    checkoutFlow,
    pricingDisplay,
  ];
}

/// A/B Test manager
class ABTestManager {
  static final ABTestManager _instance = ABTestManager._internal();
  factory ABTestManager() => _instance;
  ABTestManager._internal();

  final Map<String, ABTestVariant> _assignments = {};

  ABTestVariant getVariant(ABTest test) {
    if (_assignments.containsKey(test.key)) {
      return _assignments[test.key]!;
    }

    // Get variant from remote config
    final variantName =
        FeatureFlagManager()._remoteConfig?.getString(test.key) ??
            test.defaultVariant.name;

    final variant = ABTestVariant.values.firstWhere(
      (v) => v.name == variantName,
      orElse: () => test.defaultVariant,
    );

    _assignments[test.key] = variant;
    return variant;
  }

  void setVariant(ABTest test, ABTestVariant variant) {
    _assignments[test.key] = variant;
    if (kDebugMode) {
      debugPrint('A/B Test override: ${test.key} = ${variant.name}');
    }
  }

  void clearAssignment(ABTest test) {
    _assignments.remove(test.key);
  }

  void clearAllAssignments() {
    _assignments.clear();
  }

  bool isInVariant(ABTest test, ABTestVariant variant) {
    return getVariant(test) == variant;
  }

  Map<String, String> getAllAssignments() {
    final assignments = <String, String>{};

    for (final test in ABTests.all) {
      assignments[test.key] = getVariant(test).name;
    }

    return assignments;
  }
}

/// Feature flag widget
class FeatureFlagWidget extends StatelessWidget {
  final FeatureFlag flag;
  final Widget Function(BuildContext, bool) builder;

  const FeatureFlagWidget({
    super.key,
    required this.flag,
    required this.builder,
  });

  @override
  Widget build(BuildContext context) {
    final isEnabled = FeatureFlagManager().getBool(flag);
    return builder(context, isEnabled);
  }
}

/// A/B Test widget
class ABTestWidget extends StatelessWidget {
  final ABTest test;
  final Map<ABTestVariant, Widget> variants;
  final Widget? fallback;

  const ABTestWidget({
    super.key,
    required this.test,
    required this.variants,
    this.fallback,
  });

  @override
  Widget build(BuildContext context) {
    final variant = ABTestManager().getVariant(test);
    return variants[variant] ?? fallback ?? const SizedBox.shrink();
  }
}
