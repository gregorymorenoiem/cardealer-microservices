enum EnvironmentType {
  dev,
  staging,
  production,
}

class Environment {
  static late EnvironmentType _environment;
  static late String _apiBaseUrl;
  static late bool _enableLogging;

  static EnvironmentType get environment => _environment;
  static String get apiBaseUrl => _apiBaseUrl;
  static bool get enableLogging => _enableLogging;

  static bool get isDev => _environment == EnvironmentType.dev;
  static bool get isStaging => _environment == EnvironmentType.staging;
  static bool get isProduction => _environment == EnvironmentType.production;

  static void init({
    required EnvironmentType environment,
    required String apiBaseUrl,
    bool enableLogging = false,
  }) {
    _environment = environment;
    _apiBaseUrl = apiBaseUrl;
    _enableLogging = enableLogging;
  }

  static String get environmentName {
    switch (_environment) {
      case EnvironmentType.dev:
        return 'Development';
      case EnvironmentType.staging:
        return 'Staging';
      case EnvironmentType.production:
        return 'Production';
    }
  }
}
