import 'package:firebase_crashlytics/firebase_crashlytics.dart';
import 'package:flutter/foundation.dart';

class CrashlyticsService {
  static final FirebaseCrashlytics _crashlytics = FirebaseCrashlytics.instance;

  /// Initialize Crashlytics
  static Future<void> initialize() async {
    // Enable crashlytics collection
    await _crashlytics.setCrashlyticsCollectionEnabled(!kDebugMode);

    // Set up Flutter error handling
    FlutterError.onError = _crashlytics.recordFlutterFatalError;

    // Set up platform error handling
    PlatformDispatcher.instance.onError = (error, stack) {
      _crashlytics.recordError(error, stack, fatal: true);
      return true;
    };
  }

  /// Record a non-fatal error
  static Future<void> recordError(
    dynamic exception,
    StackTrace? stack, {
    dynamic reason,
    bool fatal = false,
    Iterable<Object> information = const [],
  }) async {
    await _crashlytics.recordError(
      exception,
      stack,
      reason: reason,
      fatal: fatal,
      information: information,
    );
  }

  /// Record a fatal error
  static Future<void> recordFatalError(
    dynamic exception,
    StackTrace stack, {
    dynamic reason,
  }) async {
    await recordError(
      exception,
      stack,
      reason: reason,
      fatal: true,
    );
  }

  /// Log a message
  static Future<void> log(String message) async {
    await _crashlytics.log(message);
  }

  /// Set user identifier
  static Future<void> setUserId(String userId) async {
    await _crashlytics.setUserIdentifier(userId);
  }

  /// Clear user identifier
  static Future<void> clearUserId() async {
    await _crashlytics.setUserIdentifier('');
  }

  /// Set custom key
  static Future<void> setCustomKey(String key, Object value) async {
    await _crashlytics.setCustomKey(key, value);
  }

  /// Set multiple custom keys
  static Future<void> setCustomKeys(Map<String, Object> keys) async {
    for (final entry in keys.entries) {
      await setCustomKey(entry.key, entry.value);
    }
  }

  /// Check if crashlytics is enabled
  static bool isCrashlyticsCollectionEnabled() {
    return _crashlytics.isCrashlyticsCollectionEnabled;
  }

  /// Force a crash (for testing only)
  static void crash() {
    _crashlytics.crash();
  }

  /// Check for unhandled errors (for testing)
  static Future<void> checkForUnsentReports() async {
    final unsentReports = await _crashlytics.checkForUnsentReports();
    if (unsentReports) {
      // Send unsent reports
      await _crashlytics.sendUnsentReports();
    }
  }

  /// Delete unsent reports
  static Future<void> deleteUnsentReports() async {
    await _crashlytics.deleteUnsentReports();
  }

  /// Record breadcrumb for debugging
  static Future<void> recordBreadcrumb(String message) async {
    await log('[BREADCRUMB] $message');
  }

  /// Record user action
  static Future<void> recordUserAction(String action) async {
    await log('[USER_ACTION] $action');
  }

  /// Record API call
  static Future<void> recordApiCall(
    String method,
    String endpoint, {
    int? statusCode,
    String? error,
  }) async {
    final message = '[API] $method $endpoint'
        '${statusCode != null ? ' - $statusCode' : ''}'
        '${error != null ? ' - Error: $error' : ''}';
    await log(message);
  }

  /// Record screen view
  static Future<void> recordScreenView(String screenName) async {
    await log('[SCREEN] $screenName');
  }
}

/// Extension for easier error handling
extension ErrorHandling on Object {
  Future<void> reportError([StackTrace? stack]) async {
    await CrashlyticsService.recordError(
      this,
      stack ?? StackTrace.current,
      fatal: false,
    );
  }
}
