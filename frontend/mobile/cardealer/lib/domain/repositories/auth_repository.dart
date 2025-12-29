import 'package:dartz/dartz.dart';
import '../entities/user.dart';
import '../../core/errors/failures.dart';

/// Repository interface for authentication operations
abstract class AuthRepository {
  /// Login with email and password
  Future<Either<Failure, User>> login({
    required String email,
    required String password,
  });

  /// Register new user
  Future<Either<Failure, User>> register({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
    required String phoneNumber,
    required UserRole role,
    String? dealershipName,
  });

  /// Login with Google OAuth
  Future<Either<Failure, User>> loginWithGoogle();

  /// Login with Apple OAuth
  Future<Either<Failure, User>> loginWithApple();

  /// Logout current user
  Future<Either<Failure, void>> logout();

  /// Get current authenticated user
  Future<Either<Failure, User>> getCurrentUser();

  /// Request password reset email
  Future<Either<Failure, void>> requestPasswordReset(String email);

  /// Reset password with verification code
  Future<Either<Failure, void>> resetPassword({
    required String email,
    required String code,
    required String newPassword,
  });

  /// Verify email with code
  Future<Either<Failure, void>> verifyEmail({
    required String email,
    required String code,
  });

  /// Update user profile
  Future<Either<Failure, User>> updateProfile({
    required String userId,
    String? firstName,
    String? lastName,
    String? phoneNumber,
    String? avatarUrl,
  });

  /// Check if email is available for registration
  Future<Either<Failure, bool>> checkEmailAvailability(String email);

  /// Get stored authentication token
  Future<Either<Failure, String?>> getStoredToken();

  /// Check if user is authenticated
  Future<Either<Failure, bool>> isAuthenticated();
}
