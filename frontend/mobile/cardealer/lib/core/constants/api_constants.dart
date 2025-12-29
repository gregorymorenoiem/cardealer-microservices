/// API constants and endpoints
class ApiConstants {
  // Private constructor
  ApiConstants._();

  // Base URLs
  static const String baseUrlDev = 'http://localhost:5000';
  static const String baseUrlStaging = 'https://staging-api.cardealer.com';
  static const String baseUrlProd = 'https://api.cardealer.com';

  // Current environment (change based on build configuration)
  static const String baseUrl = baseUrlDev;

  // API version
  static const String apiVersion = 'v1';

  // Timeout durations
  static const Duration connectTimeout = Duration(seconds: 30);
  static const Duration receiveTimeout = Duration(seconds: 30);
  static const Duration sendTimeout = Duration(seconds: 30);

  // Endpoints - Auth
  static const String login = '/api/$apiVersion/auth/login';
  static const String register = '/api/$apiVersion/auth/register';
  static const String logout = '/api/$apiVersion/auth/logout';
  static const String refreshToken = '/api/$apiVersion/auth/refresh';
  static const String verifyEmail = '/api/$apiVersion/auth/verify-email';
  static const String forgotPassword = '/api/$apiVersion/auth/forgot-password';
  static const String resetPassword = '/api/$apiVersion/auth/reset-password';

  // Endpoints - User
  static const String profile = '/api/$apiVersion/user/profile';
  static const String updateProfile = '/api/$apiVersion/user/profile';
  static const String changePassword = '/api/$apiVersion/user/change-password';

  // Endpoints - Vehicles
  static const String vehicles = '/api/$apiVersion/vehicles';
  static const String vehicleById = '/api/$apiVersion/vehicles/{id}';
  static const String featuredVehicles = '/api/$apiVersion/vehicles/featured';
  static const String searchVehicles = '/api/$apiVersion/vehicles/search';
  static const String filterVehicles = '/api/$apiVersion/vehicles/filter';
  static const String vehiclesByCategory =
      '/api/$apiVersion/vehicles/category/{category}';

  // Endpoints - Favorites
  static const String favorites = '/api/$apiVersion/favorites';
  static const String addFavorite = '/api/$apiVersion/favorites/{vehicleId}';
  static const String removeFavorite = '/api/$apiVersion/favorites/{vehicleId}';

  // Endpoints - Dealer
  static const String dealerDashboard = '/api/$apiVersion/dealer/dashboard';
  static const String dealerListings = '/api/$apiVersion/dealer/listings';
  static const String dealerStats = '/api/$apiVersion/dealer/stats';
  static const String dealerCrm = '/api/$apiVersion/dealer/crm';
  static const String dealerAnalytics = '/api/$apiVersion/dealer/analytics';

  // Endpoints - Messages
  static const String messages = '/api/$apiVersion/messages';
  static const String sendMessage = '/api/$apiVersion/messages';
  static const String conversation =
      '/api/$apiVersion/messages/conversation/{id}';

  // Endpoints - Subscriptions
  static const String subscriptions = '/api/$apiVersion/subscriptions';
  static const String subscribeToPlan =
      '/api/$apiVersion/subscriptions/subscribe';
  static const String cancelSubscription =
      '/api/$apiVersion/subscriptions/cancel';

  // Headers
  static const String headerContentType = 'Content-Type';
  static const String headerAuthorization = 'Authorization';
  static const String headerAcceptLanguage = 'Accept-Language';

  // Content types
  static const String contentTypeJson = 'application/json';
  static const String contentTypeFormData = 'multipart/form-data';

  // Storage keys
  static const String storageKeyAccessToken = 'access_token';
  static const String storageKeyRefreshToken = 'refresh_token';
  static const String storageKeyUser = 'user';
  static const String storageKeyLanguage = 'language';
}
