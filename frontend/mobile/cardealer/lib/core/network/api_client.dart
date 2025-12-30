import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../config/api_config.dart';
import 'http_client.dart';

/// API Client for making HTTP requests
/// Provides a high-level interface for all API operations
class ApiClient {
  late final Dio _dio;
  final FlutterSecureStorage _secureStorage;
  
  // Token keys
  static const String _accessTokenKey = 'access_token';
  static const String _refreshTokenKey = 'refresh_token';
  
  ApiClient({
    required FlutterSecureStorage secureStorage,
    Dio? dio,
  }) : _secureStorage = secureStorage {
    _dio = dio ?? _createDio();
  }

  /// Get the underlying Dio instance for direct access if needed
  Dio get dio => _dio;

  Dio _createDio() {
    final dio = Dio(
      BaseOptions(
        baseUrl: ApiConfig.baseUrl,
        connectTimeout: ApiConfig.connectTimeout,
        receiveTimeout: ApiConfig.receiveTimeout,
        sendTimeout: ApiConfig.sendTimeout,
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
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

    // Add auth interceptor with token refresh
    dio.interceptors.add(AuthInterceptor(
      dio: dio,
      secureStorage: _secureStorage,
    ));

    // Add retry interceptor
    dio.interceptors.add(RetryInterceptor());

    return dio;
  }

  // ===== Generic HTTP Methods =====

  /// GET request
  Future<ApiResponse<T>> get<T>(
    String path, {
    Map<String, dynamic>? queryParameters,
    T Function(dynamic json)? fromJson,
    Options? options,
    CancelToken? cancelToken,
  }) async {
    try {
      final response = await _dio.get(
        path,
        queryParameters: queryParameters,
        options: options,
        cancelToken: cancelToken,
      );
      return _handleResponse<T>(response, fromJson);
    } on DioException catch (e) {
      return _handleError<T>(e);
    }
  }

  /// POST request
  Future<ApiResponse<T>> post<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    T Function(dynamic json)? fromJson,
    Options? options,
    CancelToken? cancelToken,
  }) async {
    try {
      final response = await _dio.post(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
        cancelToken: cancelToken,
      );
      return _handleResponse<T>(response, fromJson);
    } on DioException catch (e) {
      return _handleError<T>(e);
    }
  }

  /// PUT request
  Future<ApiResponse<T>> put<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    T Function(dynamic json)? fromJson,
    Options? options,
    CancelToken? cancelToken,
  }) async {
    try {
      final response = await _dio.put(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
        cancelToken: cancelToken,
      );
      return _handleResponse<T>(response, fromJson);
    } on DioException catch (e) {
      return _handleError<T>(e);
    }
  }

  /// PATCH request
  Future<ApiResponse<T>> patch<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    T Function(dynamic json)? fromJson,
    Options? options,
    CancelToken? cancelToken,
  }) async {
    try {
      final response = await _dio.patch(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
        cancelToken: cancelToken,
      );
      return _handleResponse<T>(response, fromJson);
    } on DioException catch (e) {
      return _handleError<T>(e);
    }
  }

  /// DELETE request
  Future<ApiResponse<T>> delete<T>(
    String path, {
    dynamic data,
    Map<String, dynamic>? queryParameters,
    T Function(dynamic json)? fromJson,
    Options? options,
    CancelToken? cancelToken,
  }) async {
    try {
      final response = await _dio.delete(
        path,
        data: data,
        queryParameters: queryParameters,
        options: options,
        cancelToken: cancelToken,
      );
      return _handleResponse<T>(response, fromJson);
    } on DioException catch (e) {
      return _handleError<T>(e);
    }
  }

  /// Multipart file upload
  Future<ApiResponse<T>> uploadFile<T>(
    String path, {
    required String filePath,
    required String fieldName,
    Map<String, dynamic>? additionalFields,
    T Function(dynamic json)? fromJson,
    void Function(int sent, int total)? onProgress,
    CancelToken? cancelToken,
  }) async {
    try {
      final formData = FormData.fromMap({
        fieldName: await MultipartFile.fromFile(filePath),
        if (additionalFields != null) ...additionalFields,
      });

      final response = await _dio.post(
        path,
        data: formData,
        onSendProgress: onProgress,
        cancelToken: cancelToken,
        options: Options(
          headers: {'Content-Type': 'multipart/form-data'},
        ),
      );
      return _handleResponse<T>(response, fromJson);
    } on DioException catch (e) {
      return _handleError<T>(e);
    }
  }

  /// Download file
  Future<ApiResponse<String>> downloadFile(
    String url,
    String savePath, {
    void Function(int received, int total)? onProgress,
    CancelToken? cancelToken,
  }) async {
    try {
      await _dio.download(
        url,
        savePath,
        onReceiveProgress: onProgress,
        cancelToken: cancelToken,
      );
      return ApiResponse<String>(
        success: true,
        data: savePath,
        statusCode: 200,
      );
    } on DioException catch (e) {
      return _handleError<String>(e);
    }
  }

  // ===== Response Handling =====

  ApiResponse<T> _handleResponse<T>(
    Response response,
    T Function(dynamic json)? fromJson,
  ) {
    final statusCode = response.statusCode ?? 0;
    
    if (statusCode >= 200 && statusCode < 300) {
      T? data;
      if (fromJson != null && response.data != null) {
        // Check if response has a 'data' wrapper
        final responseData = response.data is Map && response.data['data'] != null
            ? response.data['data']
            : response.data;
        data = fromJson(responseData);
      }
      
      return ApiResponse<T>(
        success: true,
        data: data,
        statusCode: statusCode,
        message: response.data is Map ? response.data['message'] : null,
      );
    } else {
      return ApiResponse<T>(
        success: false,
        statusCode: statusCode,
        message: _getErrorMessage(response),
        errors: response.data is Map ? response.data['errors'] : null,
      );
    }
  }

  ApiResponse<T> _handleError<T>(DioException e) {
    String message;
    
    switch (e.type) {
      case DioExceptionType.connectionTimeout:
        message = 'Connection timeout. Please check your internet connection.';
        break;
      case DioExceptionType.sendTimeout:
        message = 'Request timeout. Please try again.';
        break;
      case DioExceptionType.receiveTimeout:
        message = 'Server response timeout. Please try again.';
        break;
      case DioExceptionType.badResponse:
        message = _getErrorMessage(e.response);
        break;
      case DioExceptionType.cancel:
        message = 'Request was cancelled.';
        break;
      default:
        if (e.error.toString().contains('SocketException')) {
          message = 'No internet connection.';
        } else {
          message = 'An unexpected error occurred.';
        }
    }

    return ApiResponse<T>(
      success: false,
      statusCode: e.response?.statusCode,
      message: message,
    );
  }

  String _getErrorMessage(Response? response) {
    if (response?.data is Map) {
      return response!.data['message'] ?? 
             response.data['error'] ?? 
             'Request failed';
    }
    
    switch (response?.statusCode) {
      case 400:
        return 'Invalid request. Please check your input.';
      case 401:
        return 'Session expired. Please login again.';
      case 403:
        return 'You don\'t have permission to perform this action.';
      case 404:
        return 'Resource not found.';
      case 409:
        return 'Resource already exists.';
      case 422:
        return 'Validation error. Please check your input.';
      case 429:
        return 'Too many requests. Please wait and try again.';
      case 500:
        return 'Server error. Please try again later.';
      case 503:
        return 'Service unavailable. Please try again later.';
      default:
        return 'Request failed (${response?.statusCode})';
    }
  }

  // ===== Token Management =====

  /// Save authentication tokens
  Future<void> saveTokens({
    required String accessToken,
    required String refreshToken,
  }) async {
    await _secureStorage.write(key: _accessTokenKey, value: accessToken);
    await _secureStorage.write(key: _refreshTokenKey, value: refreshToken);
    ApiConfig.setCredentials(accessToken, refreshToken);
  }

  /// Get stored access token
  Future<String?> getAccessToken() async {
    return await _secureStorage.read(key: _accessTokenKey);
  }

  /// Get stored refresh token
  Future<String?> getRefreshToken() async {
    return await _secureStorage.read(key: _refreshTokenKey);
  }

  /// Clear stored tokens
  Future<void> clearTokens() async {
    await _secureStorage.delete(key: _accessTokenKey);
    await _secureStorage.delete(key: _refreshTokenKey);
    ApiConfig.clearCredentials();
  }

  /// Check if user is authenticated
  Future<bool> isAuthenticated() async {
    final token = await getAccessToken();
    return token != null && token.isNotEmpty;
  }
}

/// Authentication Interceptor with Token Refresh
class AuthInterceptor extends Interceptor {
  final Dio _dio;
  final FlutterSecureStorage _secureStorage;
  bool _isRefreshing = false;
  final List<RequestOptions> _pendingRequests = [];
  
  static const String _accessTokenKey = 'access_token';
  static const String _refreshTokenKey = 'refresh_token';

  AuthInterceptor({
    required Dio dio,
    required FlutterSecureStorage secureStorage,
  })  : _dio = dio,
        _secureStorage = secureStorage;

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    // Skip auth for login/register endpoints
    if (_isAuthEndpoint(options.path)) {
      handler.next(options);
      return;
    }

    final token = await _secureStorage.read(key: _accessTokenKey);
    if (token != null && token.isNotEmpty) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    handler.next(options);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    if (err.response?.statusCode == 401 && !_isAuthEndpoint(err.requestOptions.path)) {
      // Token expired, try to refresh
      if (!_isRefreshing) {
        _isRefreshing = true;
        
        try {
          final newToken = await _refreshToken();
          if (newToken != null) {
            // Retry original request with new token
            err.requestOptions.headers['Authorization'] = 'Bearer $newToken';
            final response = await _dio.fetch(err.requestOptions);
            handler.resolve(response);
            
            // Retry pending requests
            for (final request in _pendingRequests) {
              request.headers['Authorization'] = 'Bearer $newToken';
              _dio.fetch(request);
            }
            _pendingRequests.clear();
            return;
          }
        } catch (e) {
          debugPrint('❌ Token refresh failed: $e');
          // Clear tokens on refresh failure
          await _secureStorage.delete(key: _accessTokenKey);
          await _secureStorage.delete(key: _refreshTokenKey);
        } finally {
          _isRefreshing = false;
        }
      } else {
        // Another refresh is in progress, queue this request
        _pendingRequests.add(err.requestOptions);
        return;
      }
    }
    
    handler.next(err);
  }

  Future<String?> _refreshToken() async {
    final refreshToken = await _secureStorage.read(key: _refreshTokenKey);
    if (refreshToken == null) return null;

    try {
      // Create a new Dio instance to avoid interceptor loop
      final refreshDio = Dio(BaseOptions(
        baseUrl: ApiConfig.baseUrl,
        headers: {'Content-Type': 'application/json'},
      ));

      final response = await refreshDio.post(
        '${ApiConfig.authServiceUrl}/refresh',
        data: {'refreshToken': refreshToken},
      );

      if (response.statusCode == 200 && response.data != null) {
        final newAccessToken = response.data['accessToken'];
        final newRefreshToken = response.data['refreshToken'] ?? refreshToken;
        
        await _secureStorage.write(key: _accessTokenKey, value: newAccessToken);
        await _secureStorage.write(key: _refreshTokenKey, value: newRefreshToken);
        ApiConfig.setCredentials(newAccessToken, newRefreshToken);
        
        debugPrint('🔄 Token refreshed successfully');
        return newAccessToken;
      }
    } catch (e) {
      debugPrint('❌ Token refresh error: $e');
    }
    
    return null;
  }

  bool _isAuthEndpoint(String path) {
    return path.contains('/login') ||
           path.contains('/register') ||
           path.contains('/refresh') ||
           path.contains('/forgot-password');
  }
}

/// Retry Interceptor for handling transient failures
class RetryInterceptor extends Interceptor {
  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    if (_shouldRetry(err)) {
      final retries = err.requestOptions.extra['retries'] as int? ?? 0;
      if (retries < ApiConfig.maxRetries) {
        debugPrint('🔄 Retrying request (attempt ${retries + 1}/${ApiConfig.maxRetries})');
        await Future.delayed(ApiConfig.retryDelay * (retries + 1));

        err.requestOptions.extra['retries'] = retries + 1;
        try {
          final response = await Dio(BaseOptions(
            baseUrl: err.requestOptions.baseUrl,
            headers: err.requestOptions.headers,
          )).fetch(err.requestOptions);
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
    // Only retry on network errors and 5xx server errors
    return err.type == DioExceptionType.connectionTimeout ||
           err.type == DioExceptionType.sendTimeout ||
           err.type == DioExceptionType.receiveTimeout ||
           (err.response?.statusCode != null && err.response!.statusCode! >= 500);
  }
}
