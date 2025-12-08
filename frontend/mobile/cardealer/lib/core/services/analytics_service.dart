import 'package:firebase_analytics/firebase_analytics.dart';

class AnalyticsService {
  static final FirebaseAnalytics _analytics = FirebaseAnalytics.instance;
  static FirebaseAnalyticsObserver get observer =>
      FirebaseAnalyticsObserver(analytics: _analytics);

  // Screen tracking
  static Future<void> logScreenView({
    required String screenName,
    String? screenClass,
  }) async {
    await _analytics.logScreenView(
      screenName: screenName,
      screenClass: screenClass ?? screenName,
    );
  }

  // User events
  static Future<void> logSignUp(String method) async {
    await _analytics.logSignUp(signUpMethod: method);
  }

  static Future<void> logLogin(String method) async {
    await _analytics.logLogin(loginMethod: method);
  }

  // Vehicle events
  static Future<void> logViewVehicle({
    required String vehicleId,
    required String brand,
    required String model,
    required double price,
  }) async {
    await _analytics.logEvent(
      name: 'view_vehicle',
      parameters: {
        'vehicle_id': vehicleId,
        'brand': brand,
        'model': model,
        'price': price,
        'currency': 'USD',
      },
    );
  }

  static Future<void> logSearch({
    required String searchTerm,
    Map<String, dynamic>? filters,
  }) async {
    await _analytics.logSearch(
      searchTerm: searchTerm,
      parameters: filters,
    );
  }

  static Future<void> logAddToFavorites(String vehicleId) async {
    await _analytics.logEvent(
      name: 'add_to_favorites',
      parameters: {
        'vehicle_id': vehicleId,
        'content_type': 'vehicle',
      },
    );
  }

  static Future<void> logRemoveFromFavorites(String vehicleId) async {
    await _analytics.logEvent(
      name: 'remove_from_favorites',
      parameters: {
        'vehicle_id': vehicleId,
        'content_type': 'vehicle',
      },
    );
  }

  static Future<void> logShare({
    required String contentType,
    required String itemId,
  }) async {
    await _analytics.logShare(
      contentType: contentType,
      itemId: itemId,
      method: 'app',
    );
  }

  // Transaction events
  static Future<void> logBeginCheckout({
    required String vehicleId,
    required double price,
  }) async {
    await _analytics.logBeginCheckout(
      value: price,
      currency: 'USD',
      items: [
        AnalyticsEventItem(
          itemId: vehicleId,
          itemName: 'Vehicle',
          price: price,
        ),
      ],
    );
  }

  static Future<void> logPurchase({
    required String transactionId,
    required double value,
    required String vehicleId,
  }) async {
    await _analytics.logPurchase(
      transactionId: transactionId,
      value: value,
      currency: 'USD',
      items: [
        AnalyticsEventItem(
          itemId: vehicleId,
          itemName: 'Vehicle',
          price: value,
        ),
      ],
    );
  }

  // User properties
  static Future<void> setUserId(String userId) async {
    await _analytics.setUserId(id: userId);
  }

  static Future<void> setUserType(String userType) async {
    await _analytics.setUserProperty(name: 'user_type', value: userType);
  }

  static Future<void> setUserProperty({
    required String name,
    required String value,
  }) async {
    await _analytics.setUserProperty(name: name, value: value);
  }

  // Engagement
  static Future<void> logAppOpen() async {
    await _analytics.logAppOpen();
  }

  static Future<void> logTutorialBegin() async {
    await _analytics.logTutorialBegin();
  }

  static Future<void> logTutorialComplete() async {
    await _analytics.logTutorialComplete();
  }

  // Custom events
  static Future<void> logEvent({
    required String name,
    Map<String, Object>? parameters,
  }) async {
    await _analytics.logEvent(
      name: name,
      parameters: parameters,
    );
  }
}
