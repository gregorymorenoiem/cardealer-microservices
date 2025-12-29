import 'package:dio/dio.dart';

/// Remote data source for authentication
/// This will communicate with the real API
/// Currently inactive - using mock data instead
class AuthRemoteDataSource {
  // ignore: unused_field
  final Dio _dio;
  // ignore: unused_field
  final String _baseUrl;

  AuthRemoteDataSource({
    required Dio dio,
    required String baseUrl,
  })  : _dio = dio,
        _baseUrl = baseUrl;

  // TODO: Implement when API is ready
  // These methods are prepared but not active yet

  /// Login with email and password
  Future<Map<String, dynamic>> login({
    required String email,
    required String password,
  }) async {
    // TODO: Activate when API is ready
    // final response = await _dio.post(
    //   '$_baseUrl/auth/login',
    //   data: {
    //     'email': email,
    //     'password': password,
    //   },
    // );
    // return response.data;
    throw UnimplementedError('API not ready - using mock data');
  }

  /// Register new user
  Future<Map<String, dynamic>> register({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
    required String phoneNumber,
    required String role,
    String? dealershipName,
  }) async {
    // TODO: Activate when API is ready
    // final response = await _dio.post(
    //   '$_baseUrl/auth/register',
    //   data: {
    //     'email': email,
    //     'password': password,
    //     'firstName': firstName,
    //     'lastName': lastName,
    //     'phoneNumber': phoneNumber,
    //     'role': role,
    //     'dealershipName': dealershipName,
    //   },
    // );
    // return response.data;
    throw UnimplementedError('API not ready - using mock data');
  }

  /// Login with Google OAuth
  Future<Map<String, dynamic>> loginWithGoogle(String googleToken) async {
    // TODO: Activate when API is ready
    // final response = await _dio.post(
    //   '$_baseUrl/auth/google',
    //   data: {'token': googleToken},
    // );
    // return response.data;
    throw UnimplementedError('API not ready - using mock data');
  }

  /// Login with Apple OAuth
  Future<Map<String, dynamic>> loginWithApple(String appleToken) async {
    // TODO: Activate when API is ready
    // final response = await _dio.post(
    //   '$_baseUrl/auth/apple',
    //   data: {'token': appleToken},
    // );
    // return response.data;
    throw UnimplementedError('API not ready - using mock data');
  }

  /// Logout
  Future<void> logout(String token) async {
    // TODO: Activate when API is ready
    // await _dio.post(
    //   '$_baseUrl/auth/logout',
    //   options: Options(
    //     headers: {'Authorization': 'Bearer $token'},
    //   ),
    // );
    throw UnimplementedError('API not ready - using mock data');
  }

  /// Get current user
  Future<Map<String, dynamic>> getCurrentUser(String token) async {
    // TODO: Activate when API is ready
    // final response = await _dio.get(
    //   '$_baseUrl/auth/me',
    //   options: Options(
    //     headers: {'Authorization': 'Bearer $token'},
    //   ),
    // );
    // return response.data;
    throw UnimplementedError('API not ready - using mock data');
  }

  /// Refresh token
  Future<Map<String, dynamic>> refreshToken(String refreshToken) async {
    // TODO: Activate when API is ready
    // final response = await _dio.post(
    //   '$_baseUrl/auth/refresh',
    //   data: {'refreshToken': refreshToken},
    // );
    // return response.data;
    throw UnimplementedError('API not ready - using mock data');
  }

  /// Request password reset
  Future<void> requestPasswordReset(String email) async {
    // TODO: Activate when API is ready
    // await _dio.post(
    //   '$_baseUrl/auth/password-reset/request',
    //   data: {'email': email},
    // );
    throw UnimplementedError('API not ready - using mock data');
  }

  /// Reset password
  Future<void> resetPassword({
    required String email,
    required String code,
    required String newPassword,
  }) async {
    // TODO: Activate when API is ready
    // await _dio.post(
    //   '$_baseUrl/auth/password-reset/confirm',
    //   data: {
    //     'email': email,
    //     'code': code,
    //     'newPassword': newPassword,
    //   },
    // );
    throw UnimplementedError('API not ready - using mock data');
  }

  /// Verify email
  Future<void> verifyEmail({
    required String email,
    required String code,
  }) async {
    // TODO: Activate when API is ready
    // await _dio.post(
    //   '$_baseUrl/auth/verify-email',
    //   data: {
    //     'email': email,
    //     'code': code,
    //   },
    // );
    throw UnimplementedError('API not ready - using mock data');
  }

  /// Update user profile
  Future<Map<String, dynamic>> updateProfile({
    required String token,
    String? firstName,
    String? lastName,
    String? phoneNumber,
    String? avatarUrl,
  }) async {
    // TODO: Activate when API is ready
    // final response = await _dio.patch(
    //   '$_baseUrl/users/profile',
    //   data: {
    //     'firstName': firstName,
    //     'lastName': lastName,
    //     'phoneNumber': phoneNumber,
    //     'avatarUrl': avatarUrl,
    //   },
    //   options: Options(
    //     headers: {'Authorization': 'Bearer $token'},
    //   ),
    // );
    // return response.data;
    throw UnimplementedError('API not ready - using mock data');
  }

  /// Check email availability
  Future<bool> checkEmailAvailability(String email) async {
    // TODO: Activate when API is ready
    // final response = await _dio.get(
    //   '$_baseUrl/auth/check-email',
    //   queryParameters: {'email': email},
    // );
    // return response.data['available'] as bool;
    throw UnimplementedError('API not ready - using mock data');
  }
}
