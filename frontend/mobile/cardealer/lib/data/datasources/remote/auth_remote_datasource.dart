import '../../../core/config/api_config.dart';
import '../../../core/network/api_client.dart';
import '../../models/user_model.dart';
import 'vehicle_remote_datasource.dart';

/// Remote data source for authentication
/// Communicates with the real API backend
class AuthRemoteDataSource {
  final ApiClient _apiClient;
  final String _baseUrl;

  AuthRemoteDataSource({
    required ApiClient apiClient,
    String? baseUrl,
  })  : _apiClient = apiClient,
        _baseUrl = baseUrl ?? ApiConfig.authServiceUrl;

  /// Login with email and password
  Future<AuthResponse> login({
    required String email,
    required String password,
  }) async {
    final response = await _apiClient.post<AuthResponse>(
      '$_baseUrl/login',
      data: {
        'email': email,
        'password': password,
      },
      fromJson: (json) => AuthResponse.fromJson(json),
    );

    if (response.success && response.data != null) {
      // Save tokens
      await _apiClient.saveTokens(
        accessToken: response.data!.accessToken,
        refreshToken: response.data!.refreshToken,
      );
      return response.data!;
    }
    throw ApiException(response.message ?? 'Login failed');
  }

  /// Register new user
  Future<AuthResponse> register({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
    required String phoneNumber,
    required String role,
    String? dealershipName,
  }) async {
    final response = await _apiClient.post<AuthResponse>(
      '$_baseUrl/register',
      data: {
        'email': email,
        'password': password,
        'firstName': firstName,
        'lastName': lastName,
        'phoneNumber': phoneNumber,
        'role': role,
        if (dealershipName != null) 'dealershipName': dealershipName,
      },
      fromJson: (json) => AuthResponse.fromJson(json),
    );

    if (response.success && response.data != null) {
      await _apiClient.saveTokens(
        accessToken: response.data!.accessToken,
        refreshToken: response.data!.refreshToken,
      );
      return response.data!;
    }
    throw ApiException(response.message ?? 'Registration failed');
  }

  /// Login with Google OAuth
  Future<AuthResponse> loginWithGoogle(String googleToken) async {
    final response = await _apiClient.post<AuthResponse>(
      '$_baseUrl/google',
      data: {'token': googleToken},
      fromJson: (json) => AuthResponse.fromJson(json),
    );

    if (response.success && response.data != null) {
      await _apiClient.saveTokens(
        accessToken: response.data!.accessToken,
        refreshToken: response.data!.refreshToken,
      );
      return response.data!;
    }
    throw ApiException(response.message ?? 'Google login failed');
  }

  /// Login with Apple OAuth
  Future<AuthResponse> loginWithApple(String appleToken) async {
    final response = await _apiClient.post<AuthResponse>(
      '$_baseUrl/apple',
      data: {'token': appleToken},
      fromJson: (json) => AuthResponse.fromJson(json),
    );

    if (response.success && response.data != null) {
      await _apiClient.saveTokens(
        accessToken: response.data!.accessToken,
        refreshToken: response.data!.refreshToken,
      );
      return response.data!;
    }
    throw ApiException(response.message ?? 'Apple login failed');
  }

  /// Logout
  Future<void> logout() async {
    final response = await _apiClient.post('$_baseUrl/logout');
    
    // Clear tokens regardless of API response
    await _apiClient.clearTokens();
    
    if (!response.success) {
      // Log but don't throw - we still want to clear local state
      // throw ApiException(response.message ?? 'Logout failed');
    }
  }

  /// Get current user
  Future<UserModel> getCurrentUser() async {
    final response = await _apiClient.get<UserModel>(
      '$_baseUrl/me',
      fromJson: (json) => UserModel.fromJson(json),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Failed to get current user');
  }

  /// Refresh token
  Future<AuthResponse> refreshToken(String refreshToken) async {
    final response = await _apiClient.post<AuthResponse>(
      '$_baseUrl/refresh',
      data: {'refreshToken': refreshToken},
      fromJson: (json) => AuthResponse.fromJson(json),
    );

    if (response.success && response.data != null) {
      await _apiClient.saveTokens(
        accessToken: response.data!.accessToken,
        refreshToken: response.data!.refreshToken,
      );
      return response.data!;
    }
    throw ApiException(response.message ?? 'Token refresh failed');
  }

  /// Request password reset
  Future<void> requestPasswordReset(String email) async {
    final response = await _apiClient.post(
      '$_baseUrl/password-reset/request',
      data: {'email': email},
    );

    if (!response.success) {
      throw ApiException(response.message ?? 'Password reset request failed');
    }
  }

  /// Reset password with code
  Future<void> resetPassword({
    required String email,
    required String code,
    required String newPassword,
  }) async {
    final response = await _apiClient.post(
      '$_baseUrl/password-reset/confirm',
      data: {
        'email': email,
        'code': code,
        'newPassword': newPassword,
      },
    );

    if (!response.success) {
      throw ApiException(response.message ?? 'Password reset failed');
    }
  }

  /// Change password (authenticated)
  Future<void> changePassword({
    required String currentPassword,
    required String newPassword,
  }) async {
    final response = await _apiClient.post(
      '$_baseUrl/password/change',
      data: {
        'currentPassword': currentPassword,
        'newPassword': newPassword,
      },
    );

    if (!response.success) {
      throw ApiException(response.message ?? 'Password change failed');
    }
  }

  /// Verify email
  Future<void> verifyEmail({
    required String email,
    required String code,
  }) async {
    final response = await _apiClient.post(
      '$_baseUrl/verify-email',
      data: {
        'email': email,
        'code': code,
      },
    );

    if (!response.success) {
      throw ApiException(response.message ?? 'Email verification failed');
    }
  }

  /// Resend verification email
  Future<void> resendVerificationEmail(String email) async {
    final response = await _apiClient.post(
      '$_baseUrl/verify-email/resend',
      data: {'email': email},
    );

    if (!response.success) {
      throw ApiException(response.message ?? 'Failed to resend verification');
    }
  }

  /// Update user profile
  Future<UserModel> updateProfile({
    String? firstName,
    String? lastName,
    String? phoneNumber,
    String? avatarUrl,
  }) async {
    final data = <String, dynamic>{};
    if (firstName != null) data['firstName'] = firstName;
    if (lastName != null) data['lastName'] = lastName;
    if (phoneNumber != null) data['phoneNumber'] = phoneNumber;
    if (avatarUrl != null) data['avatarUrl'] = avatarUrl;

    final response = await _apiClient.patch<UserModel>(
      '${ApiConfig.userServiceUrl}/profile',
      data: data,
      fromJson: (json) => UserModel.fromJson(json),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? 'Profile update failed');
  }

  /// Upload avatar
  Future<String> uploadAvatar(String filePath) async {
    final response = await _apiClient.uploadFile<Map<String, dynamic>>(
      '${ApiConfig.userServiceUrl}/profile/avatar',
      filePath: filePath,
      fieldName: 'avatar',
      fromJson: (json) => json as Map<String, dynamic>,
    );

    if (response.success && response.data != null) {
      return response.data!['url'] as String;
    }
    throw ApiException(response.message ?? 'Avatar upload failed');
  }

  /// Check email availability
  Future<bool> checkEmailAvailability(String email) async {
    final response = await _apiClient.get<Map<String, dynamic>>(
      '$_baseUrl/check-email',
      queryParameters: {'email': email},
      fromJson: (json) => json as Map<String, dynamic>,
    );

    if (response.success && response.data != null) {
      return response.data!['available'] as bool? ?? false;
    }
    throw ApiException(response.message ?? 'Email check failed');
  }

  /// Delete account
  Future<void> deleteAccount({required String password}) async {
    final response = await _apiClient.delete(
      '${ApiConfig.userServiceUrl}/account',
      data: {'password': password},
    );

    if (response.success) {
      await _apiClient.clearTokens();
    } else {
      throw ApiException(response.message ?? 'Account deletion failed');
    }
  }

  /// Enable 2FA
  Future<TwoFactorSetup> enable2FA() async {
    final response = await _apiClient.post<TwoFactorSetup>(
      '$_baseUrl/2fa/enable',
      fromJson: (json) => TwoFactorSetup.fromJson(json),
    );

    if (response.success && response.data != null) {
      return response.data!;
    }
    throw ApiException(response.message ?? '2FA setup failed');
  }

  /// Verify 2FA code
  Future<void> verify2FA(String code) async {
    final response = await _apiClient.post(
      '$_baseUrl/2fa/verify',
      data: {'code': code},
    );

    if (!response.success) {
      throw ApiException(response.message ?? '2FA verification failed');
    }
  }

  /// Disable 2FA
  Future<void> disable2FA(String code) async {
    final response = await _apiClient.post(
      '$_baseUrl/2fa/disable',
      data: {'code': code},
    );

    if (!response.success) {
      throw ApiException(response.message ?? 'Failed to disable 2FA');
    }
  }
}

/// Authentication response containing tokens and user
class AuthResponse {
  final String accessToken;
  final String refreshToken;
  final UserModel user;
  final DateTime expiresAt;

  AuthResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.user,
    required this.expiresAt,
  });

  factory AuthResponse.fromJson(Map<String, dynamic> json) {
    return AuthResponse(
      accessToken: json['accessToken'] ?? json['token'] ?? '',
      refreshToken: json['refreshToken'] ?? '',
      user: UserModel.fromJson(json['user'] ?? {}),
      expiresAt: json['expiresAt'] != null
          ? DateTime.parse(json['expiresAt'])
          : DateTime.now().add(const Duration(hours: 1)),
    );
  }
}

/// Two-factor authentication setup response
class TwoFactorSetup {
  final String secret;
  final String qrCodeUrl;
  final List<String> backupCodes;

  TwoFactorSetup({
    required this.secret,
    required this.qrCodeUrl,
    required this.backupCodes,
  });

  factory TwoFactorSetup.fromJson(Map<String, dynamic> json) {
    return TwoFactorSetup(
      secret: json['secret'] ?? '',
      qrCodeUrl: json['qrCodeUrl'] ?? '',
      backupCodes: (json['backupCodes'] as List<dynamic>?)
              ?.map((e) => e.toString())
              .toList() ??
          [],
    );
  }
}
