import 'package:flutter_test/flutter_test.dart';
import 'package:cardealer_mobile/core/config/api_config.dart';

void main() {
  group('ApiConfig Tests', () {
    test('should return correct base URL for development', () {
      expect(ApiConfig.baseUrl, contains('localhost'));
    });

    test('should have default headers', () {
      expect(
          ApiConfig.defaultHeaders['Content-Type'], equals('application/json'));
      expect(ApiConfig.defaultHeaders['Accept'], equals('application/json'));
    });

    test('should return correct service URLs', () {
      final baseUrl = ApiConfig.baseUrl;
      expect(ApiConfig.authServiceUrl, equals('$baseUrl/api/auth'));
      expect(ApiConfig.userServiceUrl, equals('$baseUrl/api/users'));
      expect(ApiConfig.vehicleServiceUrl, equals('$baseUrl/api/vehicles'));
      expect(ApiConfig.paymentServiceUrl, equals('$baseUrl/api/payments'));
    });

    test('should set and clear credentials', () {
      ApiConfig.setCredentials('test-key', 'test-token');
      expect(ApiConfig.apiKey, equals('test-key'));
      expect(ApiConfig.refreshToken, equals('test-token'));

      ApiConfig.clearCredentials();
      expect(ApiConfig.apiKey, isNull);
      expect(ApiConfig.refreshToken, isNull);
    });

    test('should include authorization header when API key is set', () {
      ApiConfig.setCredentials('my-api-key', null);
      final headers = ApiConfig.defaultHeaders;
      expect(headers['Authorization'], equals('Bearer my-api-key'));

      ApiConfig.clearCredentials();
    });
  });
}
