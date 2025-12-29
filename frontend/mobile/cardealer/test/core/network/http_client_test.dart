import 'package:flutter_test/flutter_test.dart';
import 'package:cardealer_mobile/core/network/http_client.dart';
import 'package:dio/dio.dart';

void main() {
  group('HttpClientFactory Tests', () {
    test('should create Dio instance with default configuration', () {
      final dio = HttpClientFactory.createClient();

      expect(dio, isNotNull);
      expect(dio, isA<Dio>());
      expect(dio.options.baseUrl, isNotEmpty);
    });

    test('should add interceptors', () {
      final dio = HttpClientFactory.createClient();

      // Should have at least 3 interceptors (Auth, Retry, Error)
      expect(dio.interceptors.length, greaterThanOrEqualTo(3));
    });

    test('should set custom headers', () {
      final customHeaders = {'X-Custom-Header': 'test-value'};
      final dio = HttpClientFactory.createClient(headers: customHeaders);

      expect(dio.options.headers['X-Custom-Header'], equals('test-value'));
    });

    test('should use custom base URL', () {
      const customBaseUrl = 'https://custom-api.test.com';
      final dio = HttpClientFactory.createClient(baseUrl: customBaseUrl);

      expect(dio.options.baseUrl, equals(customBaseUrl));
    });

    test('should set timeout configurations', () {
      final dio = HttpClientFactory.createClient();

      expect(dio.options.connectTimeout, isNotNull);
      expect(dio.options.receiveTimeout, isNotNull);
      expect(dio.options.sendTimeout, isNotNull);
    });
  });

  group('ApiResponse Tests', () {
    test('should create success response from JSON', () {
      final json = {
        'success': true,
        'data': {'id': 1, 'name': 'Test'},
        'message': 'Success',
        'statusCode': 200,
      };

      final response = ApiResponse.fromJson(
        json,
        (data) => data,
      );

      expect(response.success, isTrue);
      expect(response.data, isNotNull);
      expect(response.message, equals('Success'));
      expect(response.statusCode, equals(200));
    });

    test('should create error response', () {
      final response = ApiResponse.error('Test error', statusCode: 400);

      expect(response.success, isFalse);
      expect(response.message, equals('Test error'));
      expect(response.statusCode, equals(400));
      expect(response.data, isNull);
    });

    test('should handle list data in response', () {
      final json = {
        'success': true,
        'data': [
          {'id': 1, 'name': 'Item 1'},
          {'id': 2, 'name': 'Item 2'},
        ],
      };

      final response = ApiResponse.fromList(
        json,
        (data) => data,
      );

      expect(response.success, isTrue);
      expect(response.data, isNotNull);
      expect(response.data, isList);
    });
  });
}
