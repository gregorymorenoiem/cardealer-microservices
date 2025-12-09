/// API Configuration
/// Central configuration for all API endpoints and settings
class ApiConfig {
  // Singleton pattern
  static final ApiConfig _instance = ApiConfig._internal();
  factory ApiConfig() => _instance;
  ApiConfig._internal();

  // Backend service URLs
  static const String _localBaseUrl = 'http://localhost:5000';
  static const String _stagingBaseUrl = 'https://staging-api.cardealer.com';
  static const String _productionBaseUrl = 'https://api.cardealer.com';

  // Current environment
  static const String environment = String.fromEnvironment(
    'ENVIRONMENT',
    defaultValue: 'development',
  );

  // Get base URL based on environment
  static String get baseUrl {
    switch (environment) {
      case 'production':
        return _productionBaseUrl;
      case 'staging':
        return _stagingBaseUrl;
      case 'development':
      default:
        return _localBaseUrl;
    }
  }

  // Service endpoints
  static String get authServiceUrl => '$baseUrl/api/auth';
  static String get userServiceUrl => '$baseUrl/api/users';
  static String get vehicleServiceUrl => '$baseUrl/api/vehicles';
  static String get paymentServiceUrl => '$baseUrl/api/payments';
  static String get notificationServiceUrl => '$baseUrl/api/notifications';
  static String get configServiceUrl => '$baseUrl/api/config';

  // API timeouts
  static const Duration connectTimeout = Duration(seconds: 30);
  static const Duration receiveTimeout = Duration(seconds: 30);
  static const Duration sendTimeout = Duration(seconds: 30);

  // Retry configuration
  static const int maxRetries = 3;
  static const Duration retryDelay = Duration(seconds: 2);

  // Cache configuration
  static const Duration cacheValidDuration = Duration(minutes: 5);
  static const int maxCacheSize = 50; // MB

  // Feature flags
  static const bool enableApiLogging = bool.fromEnvironment(
    'ENABLE_API_LOGGING',
    defaultValue: true,
  );
  static const bool enableMockData = bool.fromEnvironment(
    'ENABLE_MOCK_DATA',
    defaultValue: true, // Default to mock during development
  );
  static const bool enableOfflineMode = true;
  static const bool enableCaching = true;

  // API Keys (should be loaded from secure storage or environment)
  static String? _apiKey;
  static String? _refreshToken;

  static String? get apiKey => _apiKey;
  static String? get refreshToken => _refreshToken;

  static void setCredentials(String? key, String? token) {
    _apiKey = key;
    _refreshToken = token;
  }

  static void clearCredentials() {
    _apiKey = null;
    _refreshToken = null;
  }

  // Headers
  static Map<String, String> get defaultHeaders => {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        if (_apiKey != null) 'Authorization': 'Bearer $_apiKey',
      };

  // Debug info
  static String get debugInfo => '''
API Configuration:
- Environment: $environment
- Base URL: $baseUrl
- Mock Data: $enableMockData
- Offline Mode: $enableOfflineMode
- Caching: $enableCaching
- API Logging: $enableApiLogging
''';
}
