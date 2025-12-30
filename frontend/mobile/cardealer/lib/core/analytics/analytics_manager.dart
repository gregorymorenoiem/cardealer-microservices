library;

/// Enhanced analytics tracking
/// Provides comprehensive event tracking, screen tracking, and user behavior analytics
import 'package:firebase_analytics/firebase_analytics.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

/// Analytics event types
enum AnalyticsEventType {
  screenView,
  buttonClick,
  formSubmit,
  search,
  purchase,
  viewItem,
  addToCart,
  removeFromCart,
  beginCheckout,
  share,
  login,
  signup,
  error,
  custom,
}

/// Analytics event
class AnalyticsEvent {
  final String name;
  final AnalyticsEventType type;
  final Map<String, Object>? parameters;
  final DateTime timestamp;

  AnalyticsEvent({
    required this.name,
    required this.type,
    this.parameters,
    DateTime? timestamp,
  }) : timestamp = timestamp ?? DateTime.now();

  Map<String, dynamic> toJson() => {
        'name': name,
        'type': type.name,
        'parameters': parameters,
        'timestamp': timestamp.toIso8601String(),
      };
}

/// Enhanced analytics manager
class AnalyticsManager {
  static final AnalyticsManager _instance = AnalyticsManager._internal();
  factory AnalyticsManager() => _instance;
  AnalyticsManager._internal();

  FirebaseAnalytics? _analytics;
  FirebaseAnalyticsObserver? _observer;

  final List<AnalyticsEvent> _eventHistory = [];
  bool _initialized = false;
  String? _userId;
  final Map<String, dynamic> _userProperties = {};

  FirebaseAnalyticsObserver? get observer => _observer;
  List<AnalyticsEvent> get eventHistory => List.unmodifiable(_eventHistory);
  bool get isInitialized => _initialized;

  Future<void> initialize() async {
    if (_initialized) return;

    try {
      _analytics = FirebaseAnalytics.instance;
      _observer = FirebaseAnalyticsObserver(analytics: _analytics!);
      _initialized = true;

      if (kDebugMode) {
        debugPrint('Analytics initialized');
      }
    } catch (e) {
      debugPrint('Failed to initialize analytics: $e');
    }
  }

  Future<void> setUserId(String userId) async {
    _userId = userId;
    await _analytics?.setUserId(id: userId);

    if (kDebugMode) {
      debugPrint('Analytics user ID set: $userId');
    }
  }

  Future<void> setUserProperty(String name, String value) async {
    _userProperties[name] = value;
    await _analytics?.setUserProperty(name: name, value: value);

    if (kDebugMode) {
      debugPrint('Analytics user property: $name = $value');
    }
  }

  Future<void> setUserProperties(Map<String, String> properties) async {
    for (final entry in properties.entries) {
      await setUserProperty(entry.key, entry.value);
    }
  }

  Future<void> logEvent(
    String name, {
    Map<String, Object>? parameters,
    AnalyticsEventType type = AnalyticsEventType.custom,
  }) async {
    final event = AnalyticsEvent(
      name: name,
      type: type,
      parameters: parameters,
    );

    _eventHistory.add(event);

    if (_analytics != null) {
      await _analytics!.logEvent(
        name: name,
        parameters: parameters,
      );
    }

    if (kDebugMode) {
      debugPrint('Analytics event: $name ${parameters ?? ''}');
    }
  }

  // Screen tracking
  Future<void> logScreenView(String screenName, {String? screenClass}) async {
    await logEvent(
      'screen_view',
      parameters: {
        'screen_name': screenName,
        if (screenClass != null) 'screen_class': screenClass,
      },
      type: AnalyticsEventType.screenView,
    );
  }

  // User actions
  Future<void> logButtonClick(String buttonName, {String? screen}) async {
    await logEvent(
      'button_click',
      parameters: {
        'button_name': buttonName,
        if (screen != null) 'screen': screen,
      },
      type: AnalyticsEventType.buttonClick,
    );
  }

  Future<void> logSearch(String searchTerm, {String? category}) async {
    await logEvent(
      'search',
      parameters: {
        'search_term': searchTerm,
        if (category != null) 'category': category,
      },
      type: AnalyticsEventType.search,
    );
  }

  Future<void> logShare(
    String contentType,
    String itemId, {
    String? method,
  }) async {
    await logEvent(
      'share',
      parameters: {
        'content_type': contentType,
        'item_id': itemId,
        if (method != null) 'method': method,
      },
      type: AnalyticsEventType.share,
    );
  }

  // E-commerce events
  Future<void> logViewItem({
    required String itemId,
    required String itemName,
    required String itemCategory,
    required double price,
    String? brand,
  }) async {
    await logEvent(
      'view_item',
      parameters: {
        'item_id': itemId,
        'item_name': itemName,
        'item_category': itemCategory,
        'price': price,
        if (brand != null) 'brand': brand,
      },
      type: AnalyticsEventType.viewItem,
    );
  }

  Future<void> logAddToCart({
    required String itemId,
    required String itemName,
    required double price,
    int quantity = 1,
  }) async {
    await logEvent(
      'add_to_cart',
      parameters: {
        'item_id': itemId,
        'item_name': itemName,
        'price': price,
        'quantity': quantity,
      },
      type: AnalyticsEventType.addToCart,
    );
  }

  Future<void> logRemoveFromCart({
    required String itemId,
    required String itemName,
    required double price,
  }) async {
    await logEvent(
      'remove_from_cart',
      parameters: {
        'item_id': itemId,
        'item_name': itemName,
        'price': price,
      },
      type: AnalyticsEventType.removeFromCart,
    );
  }

  Future<void> logBeginCheckout({
    required double value,
    required String currency,
    int itemCount = 1,
  }) async {
    await logEvent(
      'begin_checkout',
      parameters: {
        'value': value,
        'currency': currency,
        'item_count': itemCount,
      },
      type: AnalyticsEventType.beginCheckout,
    );
  }

  Future<void> logPurchase({
    required String transactionId,
    required double value,
    required String currency,
    double? tax,
    double? shipping,
    List<Map<String, dynamic>>? items,
  }) async {
    await logEvent(
      'purchase',
      parameters: {
        'transaction_id': transactionId,
        'value': value,
        'currency': currency,
        if (tax != null) 'tax': tax,
        if (shipping != null) 'shipping': shipping,
        if (items != null) 'items': items,
      },
      type: AnalyticsEventType.purchase,
    );
  }

  // Authentication events
  Future<void> logLogin(String method) async {
    await logEvent(
      'login',
      parameters: {'method': method},
      type: AnalyticsEventType.login,
    );
  }

  Future<void> logSignup(String method) async {
    await logEvent(
      'sign_up',
      parameters: {'method': method},
      type: AnalyticsEventType.signup,
    );
  }

  // Error tracking
  Future<void> logError({
    required String errorMessage,
    String? errorCode,
    String? screen,
    bool fatal = false,
  }) async {
    await logEvent(
      'error',
      parameters: {
        'error_message': errorMessage,
        if (errorCode != null) 'error_code': errorCode,
        if (screen != null) 'screen': screen,
        'fatal': fatal,
      },
      type: AnalyticsEventType.error,
    );
  }

  // Form tracking
  Future<void> logFormStart(String formName) async {
    await logEvent('form_start', parameters: {'form_name': formName});
  }

  Future<void> logFormComplete(
    String formName, {
    Duration? duration,
  }) async {
    await logEvent(
      'form_complete',
      parameters: {
        'form_name': formName,
        if (duration != null) 'duration_seconds': duration.inSeconds,
      },
      type: AnalyticsEventType.formSubmit,
    );
  }

  void clearHistory() {
    _eventHistory.clear();
  }

  Map<String, dynamic> getAnalyticsSummary() {
    final eventsByType = <String, int>{};

    for (final event in _eventHistory) {
      final type = event.type.name;
      eventsByType[type] = (eventsByType[type] ?? 0) + 1;
    }

    return {
      'total_events': _eventHistory.length,
      'events_by_type': eventsByType,
      'user_id': _userId,
      'user_properties': _userProperties,
    };
  }
}

/// Screen tracking mixin
mixin AnalyticsScreenMixin<T extends StatefulWidget> on State<T> {
  String get screenName;
  String? get screenClass => null;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _trackScreen();
    });
  }

  void _trackScreen() {
    AnalyticsManager().logScreenView(
      screenName,
      screenClass: screenClass,
    );
  }
}

/// Analytics-aware navigator observer
class AnalyticsNavigatorObserver extends NavigatorObserver {
  @override
  void didPush(Route<dynamic> route, Route<dynamic>? previousRoute) {
    super.didPush(route, previousRoute);
    _logNavigation(route);
  }

  @override
  void didReplace({Route<dynamic>? newRoute, Route<dynamic>? oldRoute}) {
    super.didReplace(newRoute: newRoute, oldRoute: oldRoute);
    if (newRoute != null) {
      _logNavigation(newRoute);
    }
  }

  void _logNavigation(Route<dynamic> route) {
    if (route.settings.name != null) {
      AnalyticsManager().logScreenView(route.settings.name!);
    }
  }
}

/// Tracked button widget
class TrackedButton extends StatelessWidget {
  final String label;
  final VoidCallback? onPressed;
  final String? eventName;
  final Map<String, dynamic>? eventParameters;
  final Widget? child;

  const TrackedButton({
    super.key,
    required this.label,
    this.onPressed,
    this.eventName,
    this.eventParameters,
    this.child,
  });

  @override
  Widget build(BuildContext context) {
    return ElevatedButton(
      onPressed: onPressed == null
          ? null
          : () {
              AnalyticsManager().logEvent(
                eventName ?? 'button_$label',
                parameters: {
                  'button_label': label,
                  ...?eventParameters,
                },
                type: AnalyticsEventType.buttonClick,
              );
              onPressed!();
            },
      child: child ?? Text(label),
    );
  }
}

/// User journey tracker
class UserJourneyTracker {
  static final UserJourneyTracker _instance = UserJourneyTracker._internal();
  factory UserJourneyTracker() => _instance;
  UserJourneyTracker._internal();

  final List<String> _journey = [];
  DateTime? _journeyStart;

  void startJourney(String initialScreen) {
    _journey.clear();
    _journey.add(initialScreen);
    _journeyStart = DateTime.now();
  }

  void addStep(String screen) {
    _journey.add(screen);
  }

  void endJourney(String goal, {bool success = true}) {
    final duration = _journeyStart != null
        ? DateTime.now().difference(_journeyStart!)
        : null;

    AnalyticsManager().logEvent(
      'user_journey_complete',
      parameters: {
        'goal': goal,
        'success': success,
        'steps': _journey.length,
        'path': _journey.join(' > '),
        if (duration != null) 'duration_seconds': duration.inSeconds,
      },
    );

    _journey.clear();
    _journeyStart = null;
  }

  List<String> get currentJourney => List.unmodifiable(_journey);
}

/// Funnel tracking
class FunnelTracker {
  final String funnelName;
  final List<String> steps;
  final Map<String, DateTime> _stepTimestamps = {};
  int _currentStep = 0;

  FunnelTracker({
    required this.funnelName,
    required this.steps,
  });

  void start() {
    _currentStep = 0;
    _stepTimestamps.clear();
    _logStep(steps[0]);
  }

  void nextStep() {
    if (_currentStep < steps.length - 1) {
      _currentStep++;
      _logStep(steps[_currentStep]);
    }
  }

  void complete() {
    _logStep('${steps.last}_complete');

    AnalyticsManager().logEvent(
      'funnel_complete',
      parameters: {
        'funnel_name': funnelName,
        'steps_completed': _currentStep + 1,
        'total_steps': steps.length,
      },
    );
  }

  void abandon(String reason) {
    AnalyticsManager().logEvent(
      'funnel_abandon',
      parameters: {
        'funnel_name': funnelName,
        'last_step': steps[_currentStep],
        'step_number': _currentStep + 1,
        'reason': reason,
      },
    );
  }

  void _logStep(String step) {
    _stepTimestamps[step] = DateTime.now();

    AnalyticsManager().logEvent(
      'funnel_step',
      parameters: {
        'funnel_name': funnelName,
        'step_name': step,
        'step_number': _currentStep + 1,
      },
    );
  }
}
