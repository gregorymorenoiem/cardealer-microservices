import 'dart:async';

/// Mock data source for authentication
/// This simulates API calls with mock data
/// TODO: Replace with real API implementation when backend is ready
class MockAuthDataSource {
  // Simulated delay for API calls
  static const _apiDelay = Duration(milliseconds: 1500);

  // Mock user database
  static final List<Map<String, dynamic>> _mockUsers = [
    {
      'id': '1',
      'email': 'demo@cardealer.com',
      'password': 'Demo123!',
      'firstName': 'Demo',
      'lastName': 'User',
      'phoneNumber': '+1234567890',
      'role': 'individual',
      'avatarUrl': null,
      'isVerified': true,
      'createdAt': '2025-01-01T00:00:00Z',
    },
    {
      'id': '2',
      'email': 'dealer@cardealer.com',
      'password': 'Dealer123!',
      'firstName': 'John',
      'lastName': 'Dealer',
      'phoneNumber': '+1234567891',
      'role': 'dealer',
      'avatarUrl': null,
      'isVerified': true,
      'dealershipName': 'Premium Auto Sales',
      'createdAt': '2025-01-01T00:00:00Z',
    },
  ];

  // Simulated session storage
  static String? _currentToken;
  static Map<String, dynamic>? _currentUser;

  /// Login with email and password
  /// Returns user data and auth token
  Future<Map<String, dynamic>> login({
    required String email,
    required String password,
  }) async {
    await Future.delayed(_apiDelay);

    // Simulate API call
    final user = _mockUsers.firstWhere(
      (u) => u['email'] == email && u['password'] == password,
      orElse: () => throw Exception('Invalid credentials'),
    );

    // Generate mock token
    _currentToken = 'mock_token_${DateTime.now().millisecondsSinceEpoch}';
    _currentUser = Map<String, dynamic>.from(user);

    return {
      'user': user,
      'token': _currentToken,
      'expiresIn': 3600, // 1 hour
    };
  }

  /// Register new user
  /// Returns user data and auth token
  Future<Map<String, dynamic>> register({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
    required String phoneNumber,
    required String role, // 'individual', 'dealer'
    String? dealershipName,
  }) async {
    await Future.delayed(_apiDelay);

    // Check if email already exists
    if (_mockUsers.any((u) => u['email'] == email)) {
      throw Exception('Email already registered');
    }

    // Create new user
    final newUser = {
      'id': '${_mockUsers.length + 1}',
      'email': email,
      'password': password,
      'firstName': firstName,
      'lastName': lastName,
      'phoneNumber': phoneNumber,
      'role': role,
      'avatarUrl': null,
      'isVerified': false,
      'dealershipName': dealershipName,
      'createdAt': DateTime.now().toIso8601String(),
    };

    _mockUsers.add(newUser);

    // Generate mock token
    _currentToken = 'mock_token_${DateTime.now().millisecondsSinceEpoch}';
    _currentUser = Map<String, dynamic>.from(newUser);

    return {
      'user': newUser,
      'token': _currentToken,
      'expiresIn': 3600,
    };
  }

  /// Login with Google (OAuth)
  /// Simulates Google OAuth flow
  Future<Map<String, dynamic>> loginWithGoogle() async {
    await Future.delayed(_apiDelay);

    final googleUser = {
      'id': 'google_${DateTime.now().millisecondsSinceEpoch}',
      'email': 'google.user@gmail.com',
      'firstName': 'Google',
      'lastName': 'User',
      'phoneNumber': null,
      'role': 'individual',
      'avatarUrl': 'https://i.pravatar.cc/150?img=1',
      'isVerified': true,
      'createdAt': DateTime.now().toIso8601String(),
    };

    _currentToken = 'mock_token_${DateTime.now().millisecondsSinceEpoch}';
    _currentUser = Map<String, dynamic>.from(googleUser);

    return {
      'user': googleUser,
      'token': _currentToken,
      'expiresIn': 3600,
    };
  }

  /// Login with Apple (OAuth)
  /// Simulates Apple Sign In flow
  Future<Map<String, dynamic>> loginWithApple() async {
    await Future.delayed(_apiDelay);

    final appleUser = {
      'id': 'apple_${DateTime.now().millisecondsSinceEpoch}',
      'email': 'apple.user@icloud.com',
      'firstName': 'Apple',
      'lastName': 'User',
      'phoneNumber': null,
      'role': 'individual',
      'avatarUrl': 'https://i.pravatar.cc/150?img=2',
      'isVerified': true,
      'createdAt': DateTime.now().toIso8601String(),
    };

    _currentToken = 'mock_token_${DateTime.now().millisecondsSinceEpoch}';
    _currentUser = Map<String, dynamic>.from(appleUser);

    return {
      'user': appleUser,
      'token': _currentToken,
      'expiresIn': 3600,
    };
  }

  /// Logout current user
  Future<void> logout() async {
    await Future.delayed(const Duration(milliseconds: 500));
    _currentToken = null;
    _currentUser = null;
  }

  /// Get current user data
  Future<Map<String, dynamic>> getCurrentUser() async {
    await Future.delayed(const Duration(milliseconds: 300));

    if (_currentUser == null) {
      throw Exception('No authenticated user');
    }

    return _currentUser!;
  }

  /// Refresh authentication token
  Future<Map<String, dynamic>> refreshToken() async {
    await Future.delayed(const Duration(milliseconds: 500));

    if (_currentToken == null) {
      throw Exception('No token to refresh');
    }

    _currentToken = 'mock_token_${DateTime.now().millisecondsSinceEpoch}';

    return {
      'token': _currentToken,
      'expiresIn': 3600,
    };
  }

  /// Request password reset
  Future<void> requestPasswordReset(String email) async {
    await Future.delayed(_apiDelay);

    final userExists = _mockUsers.any((u) => u['email'] == email);
    if (!userExists) {
      throw Exception('User not found');
    }

    // Simulate email sent
    print('Password reset email sent to: $email');
  }

  /// Reset password with code
  Future<void> resetPassword({
    required String email,
    required String code,
    required String newPassword,
  }) async {
    await Future.delayed(_apiDelay);

    final userIndex = _mockUsers.indexWhere((u) => u['email'] == email);
    if (userIndex == -1) {
      throw Exception('User not found');
    }

    // Simulate code verification (accept any 6-digit code for demo)
    if (code.length != 6) {
      throw Exception('Invalid verification code');
    }

    // Update password
    _mockUsers[userIndex]['password'] = newPassword;
  }

  /// Verify email with code
  Future<void> verifyEmail({
    required String email,
    required String code,
  }) async {
    await Future.delayed(_apiDelay);

    final userIndex = _mockUsers.indexWhere((u) => u['email'] == email);
    if (userIndex == -1) {
      throw Exception('User not found');
    }

    // Simulate code verification (accept any 6-digit code for demo)
    if (code.length != 6) {
      throw Exception('Invalid verification code');
    }

    // Mark as verified
    _mockUsers[userIndex]['isVerified'] = true;

    if (_currentUser?['email'] == email) {
      _currentUser!['isVerified'] = true;
    }
  }

  /// Update user profile
  Future<Map<String, dynamic>> updateProfile({
    required String userId,
    String? firstName,
    String? lastName,
    String? phoneNumber,
    String? avatarUrl,
  }) async {
    await Future.delayed(_apiDelay);

    final userIndex = _mockUsers.indexWhere((u) => u['id'] == userId);
    if (userIndex == -1) {
      throw Exception('User not found');
    }

    // Update fields
    if (firstName != null) _mockUsers[userIndex]['firstName'] = firstName;
    if (lastName != null) _mockUsers[userIndex]['lastName'] = lastName;
    if (phoneNumber != null) _mockUsers[userIndex]['phoneNumber'] = phoneNumber;
    if (avatarUrl != null) _mockUsers[userIndex]['avatarUrl'] = avatarUrl;

    // Update current user if it's the same
    if (_currentUser?['id'] == userId) {
      _currentUser = _mockUsers[userIndex];
    }

    return _mockUsers[userIndex];
  }

  /// Check if email is available
  Future<bool> checkEmailAvailability(String email) async {
    await Future.delayed(const Duration(milliseconds: 500));
    return !_mockUsers.any((u) => u['email'] == email);
  }

  /// Get current auth token
  String? getToken() => _currentToken;

  /// Check if user is authenticated
  bool isAuthenticated() => _currentToken != null && _currentUser != null;
}
