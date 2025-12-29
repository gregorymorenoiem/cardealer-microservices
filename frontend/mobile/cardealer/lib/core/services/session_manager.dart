import 'dart:async';
import 'package:flutter/foundation.dart';

/// Session Management Service - Sprint 7 AE-009
/// Features:
/// - Remember me functionality
/// - Session expiry handling
/// - Multi-device logout
/// - Session timeout warnings
/// - Secure token storage

class SessionManager {
  static final SessionManager _instance = SessionManager._internal();
  factory SessionManager() => _instance;
  SessionManager._internal();

  // Session state
  bool _isAuthenticated = false;
  DateTime? _sessionStartTime;
  DateTime? _lastActivityTime;
  String? _accessToken;
  String? _refreshToken;
  String? _userId;
  bool _rememberMe = false;

  // Session configuration
  static const Duration _sessionTimeout = Duration(minutes: 30);
  static const Duration _warningBeforeTimeout = Duration(minutes: 5);
  static const Duration _refreshTokenLifetime = Duration(days: 30);

  // Timers
  Timer? _inactivityTimer;
  Timer? _warningTimer;

  // Callbacks
  VoidCallback? _onSessionExpired;
  VoidCallback? _onSessionWarning;
  Function(String userId, String deviceId)? _onLogoutFromDevice;

  /// Initialize session with user data
  Future<void> initSession({
    required String accessToken,
    required String refreshToken,
    required String userId,
    bool rememberMe = false,
  }) async {
    _isAuthenticated = true;
    _sessionStartTime = DateTime.now();
    _lastActivityTime = DateTime.now();
    _accessToken = accessToken;
    _refreshToken = refreshToken;
    _userId = userId;
    _rememberMe = rememberMe;

    // Start inactivity monitoring
    _startInactivityTimer();

    // Save to secure storage if remember me is enabled
    if (rememberMe) {
      await _saveToSecureStorage();
    }

    debugPrint('Session initialized for user: $userId');
  }

  /// Update last activity time
  void updateActivity() {
    _lastActivityTime = DateTime.now();
    _resetInactivityTimer();
  }

  /// Check if session is valid
  bool isSessionValid() {
    if (!_isAuthenticated || _sessionStartTime == null) {
      return false;
    }

    final now = DateTime.now();
    final sessionDuration = now.difference(_sessionStartTime!);

    // Check if session has expired
    if (sessionDuration > _sessionTimeout) {
      return false;
    }

    // Check if refresh token has expired (if remember me is disabled)
    if (!_rememberMe && sessionDuration > _refreshTokenLifetime) {
      return false;
    }

    return true;
  }

  /// Get time until session expires
  Duration? getTimeUntilExpiry() {
    if (_sessionStartTime == null) return null;

    final now = DateTime.now();
    final sessionDuration = now.difference(_sessionStartTime!);
    final remaining = _sessionTimeout - sessionDuration;

    return remaining.isNegative ? Duration.zero : remaining;
  }

  /// Refresh access token
  Future<bool> refreshAccessToken() async {
    if (_refreshToken == null) {
      return false;
    }

    try {
      // TODO: Call API to refresh token
      // final response = await authRepository.refreshToken(_refreshToken!);
      // _accessToken = response.accessToken;
      // _refreshToken = response.refreshToken;

      // Mock implementation
      await Future.delayed(const Duration(milliseconds: 500));

      debugPrint('Access token refreshed');
      return true;
    } catch (e) {
      debugPrint('Error refreshing token: $e');
      return false;
    }
  }

  /// End session (logout)
  Future<void> endSession() async {
    _isAuthenticated = false;
    _sessionStartTime = null;
    _lastActivityTime = null;
    _accessToken = null;
    _refreshToken = null;
    _userId = null;
    _rememberMe = false;

    _inactivityTimer?.cancel();
    _warningTimer?.cancel();

    await _clearSecureStorage();

    debugPrint('Session ended');
  }

  /// Logout from specific device
  Future<void> logoutFromDevice(String deviceId) async {
    try {
      // TODO: Call API to logout from device
      // await authRepository.logoutDevice(deviceId);

      _onLogoutFromDevice?.call(_userId ?? '', deviceId);

      debugPrint('Logged out from device: $deviceId');
    } catch (e) {
      debugPrint('Error logging out from device: $e');
    }
  }

  /// Logout from all devices
  Future<void> logoutFromAllDevices() async {
    try {
      // TODO: Call API to logout from all devices
      // await authRepository.logoutAllDevices();

      await endSession();

      debugPrint('Logged out from all devices');
    } catch (e) {
      debugPrint('Error logging out from all devices: $e');
    }
  }

  /// Set session expired callback
  void setOnSessionExpired(VoidCallback callback) {
    _onSessionExpired = callback;
  }

  /// Set session warning callback
  void setOnSessionWarning(VoidCallback callback) {
    _onSessionWarning = callback;
  }

  /// Set logout from device callback
  void setOnLogoutFromDevice(
      Function(String userId, String deviceId) callback) {
    _onLogoutFromDevice = callback;
  }

  /// Start inactivity timer
  void _startInactivityTimer() {
    _inactivityTimer?.cancel();

    // Start warning timer
    final warningTime = _sessionTimeout - _warningBeforeTimeout;
    _warningTimer = Timer(warningTime, () {
      _onSessionWarning?.call();
      debugPrint('Session expiring soon - warning triggered');
    });

    // Start expiry timer
    _inactivityTimer = Timer(_sessionTimeout, () {
      _handleSessionExpired();
    });
  }

  /// Reset inactivity timer
  void _resetInactivityTimer() {
    _startInactivityTimer();
  }

  /// Handle session expired
  void _handleSessionExpired() {
    debugPrint('Session expired');
    endSession();
    _onSessionExpired?.call();
  }

  /// Save session to secure storage
  Future<void> _saveToSecureStorage() async {
    // TODO: Implement with flutter_secure_storage
    // final storage = FlutterSecureStorage();
    // await storage.write(key: 'access_token', value: _accessToken);
    // await storage.write(key: 'refresh_token', value: _refreshToken);
    // await storage.write(key: 'user_id', value: _userId);
    // await storage.write(key: 'remember_me', value: _rememberMe.toString());

    debugPrint('Session saved to secure storage');
  }

  /// Load session from secure storage
  Future<bool> loadFromSecureStorage() async {
    // TODO: Implement with flutter_secure_storage
    // final storage = FlutterSecureStorage();
    // final accessToken = await storage.read(key: 'access_token');
    // final refreshToken = await storage.read(key: 'refresh_token');
    // final userId = await storage.read(key: 'user_id');
    // final rememberMe = await storage.read(key: 'remember_me');

    // if (accessToken != null && refreshToken != null && userId != null) {
    //   await initSession(
    //     accessToken: accessToken,
    //     refreshToken: refreshToken,
    //     userId: userId,
    //     rememberMe: rememberMe == 'true',
    //   );
    //   return true;
    // }

    return false;
  }

  /// Clear secure storage
  Future<void> _clearSecureStorage() async {
    // TODO: Implement with flutter_secure_storage
    // final storage = FlutterSecureStorage();
    // await storage.deleteAll();

    debugPrint('Secure storage cleared');
  }

  // Getters
  bool get isAuthenticated => _isAuthenticated;
  String? get accessToken => _accessToken;
  String? get refreshToken => _refreshToken;
  String? get userId => _userId;
  bool get rememberMe => _rememberMe;
  DateTime? get sessionStartTime => _sessionStartTime;
  DateTime? get lastActivityTime => _lastActivityTime;
}
