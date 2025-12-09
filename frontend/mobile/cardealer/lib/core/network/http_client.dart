import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import '../config/api_config.dart';

/// HTTP Client Factory
/// Creates and configures Dio instances for API communication
class HttpClientFactory {
  static Dio createClient({
    String? baseUrl,
    Map<String, String>? headers,
    Duration? connectTimeout,
    Duration? receiveTimeout,
  }) {
    final dio = Dio(
      BaseOptions(
        baseUrl: baseUrl ?? ApiConfig.baseUrl,
        connectTimeout: connectTimeout ?? ApiConfig.connectTimeout,
        receiveTimeout: receiveTimeout ?? ApiConfig.receiveTimeout,
        sendTimeout: ApiConfig.sendTimeout,
        headers: {
          ...ApiConfig.defaultHeaders,
          if (headers != null) ...headers,
        },
        validateStatus: (status) => status! < 500,
      ),
    );

    // Add interceptors
    if (ApiConfig.enableApiLogging && kDebugMode) {
      dio.interceptors.add(LogInterceptor(
        requestBody: true,
        responseBody: true,
        requestHeader: true,
        responseHeader: false,
        error: true,
        logPrint: (obj) => debugPrint('🌐 API: $obj'),
      ));
    }

    // Add auth interceptor
    dio.interceptors.add(_AuthInterceptor());

    // Add retry interceptor
    dio.interceptors.add(_RetryInterceptor());

    // Add error handling interceptor
    dio.interceptors.add(_ErrorInterceptor());

    return dio;
  }
}

/// Authentication Interceptor
class _AuthInterceptor extends Interceptor {
  _AuthInterceptor();

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    // Add auth token if available
    final token = ApiConfig.apiKey;
    if (token != null && token.isNotEmpty) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    handler.next(options);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    // Handle 401 Unauthorized - refresh token
    if (err.response?.statusCode == 401) {
      try {
        // TODO: Implement token refresh logic
        debugPrint('🔒 Token expired, needs refresh');
        // After refresh, retry original request
      } catch (e) {
        debugPrint('❌ Token refresh failed: $e');
      }
    }
    handler.next(err);
  }
}

/// Retry Interceptor
class _RetryInterceptor extends Interceptor {
  _RetryInterceptor();

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    if (_shouldRetry(err)) {
      final retries = err.requestOptions.extra['retries'] as int? ?? 0;
      if (retries < ApiConfig.maxRetries) {
        debugPrint('🔄 Retrying request (attempt ${retries + 1})');
        await Future.delayed(ApiConfig.retryDelay);

        err.requestOptions.extra['retries'] = retries + 1;
        try {
          final response = await Dio().fetch(err.requestOptions);
          handler.resolve(response);
          return;
        } catch (e) {
          // Continue to next error handler
        }
      }
    }
    handler.next(err);
  }

  bool _shouldRetry(DioException err) {
    // Retry on network errors and 5xx server errors
    return err.type == DioExceptionType.connectionTimeout ||
        err.type == DioExceptionType.sendTimeout ||
        err.type == DioExceptionType.receiveTimeout ||
        (err.response?.statusCode != null && err.response!.statusCode! >= 500);
  }
}

/// Error Handling Interceptor
class _ErrorInterceptor extends Interceptor {
  _ErrorInterceptor();

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    String errorMessage;

    switch (err.type) {
      case DioExceptionType.connectionTimeout:
        errorMessage =
            'Connection timeout. Please check your internet connection.';
        break;
      case DioExceptionType.sendTimeout:
        errorMessage = 'Send timeout. The request took too long.';
        break;
      case DioExceptionType.receiveTimeout:
        errorMessage = 'Receive timeout. The server took too long to respond.';
        break;
      case DioExceptionType.badResponse:
        errorMessage = _handleStatusCode(err.response?.statusCode);
        break;
      case DioExceptionType.cancel:
        errorMessage = 'Request cancelled';
        break;
      case DioExceptionType.unknown:
        if (err.error.toString().contains('SocketException')) {
          errorMessage = 'No internet connection';
        } else {
          errorMessage = 'Unexpected error occurred';
        }
        break;
      default:
        errorMessage = 'An error occurred';
    }

    debugPrint('❌ API Error: $errorMessage');
    handler.next(err.copyWith(
      message: errorMessage,
    ));
  }

  String _handleStatusCode(int? statusCode) {
    switch (statusCode) {
      case 400:
        return 'Bad request. Please check your input.';
      case 401:
        return 'Unauthorized. Please login again.';
      case 403:
        return 'Forbidden. You don\'t have permission.';
      case 404:
        return 'Resource not found.';
      case 409:
        return 'Conflict. The resource already exists.';
      case 422:
        return 'Validation error. Please check your input.';
      case 500:
        return 'Server error. Please try again later.';
      case 503:
        return 'Service unavailable. Please try again later.';
      default:
        return 'Error occurred (Status: $statusCode)';
    }
  }
}

/// API Response wrapper
class ApiResponse<T> {
  final bool success;
  final T? data;
  final String? message;
  final int? statusCode;
  final Map<String, dynamic>? errors;

  ApiResponse({
    required this.success,
    this.data,
    this.message,
    this.statusCode,
    this.errors,
  });

  factory ApiResponse.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic>) fromJson,
  ) {
    return ApiResponse<T>(
      success: json['success'] ?? true,
      data: json['data'] != null ? fromJson(json['data']) : null,
      message: json['message'],
      statusCode: json['statusCode'],
      errors: json['errors'],
    );
  }

  factory ApiResponse.fromList(
    Map<String, dynamic> json,
    T Function(List<dynamic>) fromList,
  ) {
    return ApiResponse<T>(
      success: json['success'] ?? true,
      data: json['data'] != null ? fromList(json['data']) : null,
      message: json['message'],
      statusCode: json['statusCode'],
      errors: json['errors'],
    );
  }

  factory ApiResponse.error(String message, {int? statusCode}) {
    return ApiResponse<T>(
      success: false,
      message: message,
      statusCode: statusCode,
    );
  }
}
