import 'package:flutter/foundation.dart';

enum Flavor {
  dev,
  staging,
  prod,
}

class AppConfig {
  final Flavor flavor;
  final String appName;
  final String apiBaseUrl;
  final bool enableLogging;
  final bool enableAnalytics;

  AppConfig._({
    required this.flavor,
    required this.appName,
    required this.apiBaseUrl,
    required this.enableLogging,
    required this.enableAnalytics,
  });

  static AppConfig? _instance;

  static AppConfig get instance {
    if (_instance == null) {
      throw Exception(
          'AppConfig no ha sido inicializado. Llama a AppConfig.initialize() primero.');
    }
    return _instance!;
  }

  static void initialize(Flavor flavor) {
    _instance = AppConfig._fromFlavor(flavor);
  }

  factory AppConfig._fromFlavor(Flavor flavor) {
    switch (flavor) {
      case Flavor.dev:
        return AppConfig._(
          flavor: Flavor.dev,
          appName: 'CarDealer DEV',
          apiBaseUrl: 'http://localhost:5000',
          enableLogging: true,
          enableAnalytics: false,
        );
      case Flavor.staging:
        return AppConfig._(
          flavor: Flavor.staging,
          appName: 'CarDealer STG',
          apiBaseUrl: 'https://api-staging.cardealer.com',
          enableLogging: true,
          enableAnalytics: true,
        );
      case Flavor.prod:
        return AppConfig._(
          flavor: Flavor.prod,
          appName: 'CarDealer',
          apiBaseUrl: 'https://api.cardealer.com',
          enableLogging: false,
          enableAnalytics: true,
        );
    }
  }

  bool get isDev => flavor == Flavor.dev;
  bool get isStaging => flavor == Flavor.staging;
  bool get isProd => flavor == Flavor.prod;

  @override
  String toString() {
    return 'AppConfig{flavor: $flavor, appName: $appName, apiBaseUrl: $apiBaseUrl}';
  }
}
