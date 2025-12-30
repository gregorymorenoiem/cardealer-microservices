import 'package:dartz/dartz.dart';
import '../../core/config/api_config.dart';
import '../../core/errors/failures.dart';
import '../../domain/entities/user.dart';
import '../../domain/repositories/auth_repository.dart';
import '../datasources/mock/mock_auth_datasource.dart';
import '../datasources/remote/auth_remote_datasource.dart';
import '../datasources/remote/vehicle_remote_datasource.dart';
import '../models/user_model.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Implementation of AuthRepository
/// Uses mock data source in development, real API in production
class AuthRepositoryImpl implements AuthRepository {
  final MockAuthDataSource _mockDataSource;
  final AuthRemoteDataSource _remoteDataSource;
  final FlutterSecureStorage _secureStorage;

  /// Whether to use mock data (controlled by ApiConfig.enableMockData)
  bool get _useMock => ApiConfig.enableMockData;

  AuthRepositoryImpl({
    required MockAuthDataSource mockDataSource,
    required AuthRemoteDataSource remoteDataSource,
    required FlutterSecureStorage secureStorage,
  })  : _mockDataSource = mockDataSource,
        _remoteDataSource = remoteDataSource,
        _secureStorage = secureStorage;

  @override
  Future<Either<Failure, User>> login({
    required String email,
    required String password,
  }) async {
    try {
      if (_useMock) {
        final response = await _mockDataSource.login(email: email, password: password);
        final user = UserModel.fromJson(response['user'] as Map<String, dynamic>);
        final token = response['token'] as String;
        await _secureStorage.write(key: 'auth_token', value: token);
        await _secureStorage.write(key: 'user_id', value: user.id);
        return Right(user);
      }
      
      final authResponse = await _remoteDataSource.login(email: email, password: password);
      await _secureStorage.write(key: 'user_id', value: authResponse.user.id);
      return Right(authResponse.user);
    } on ApiException catch (e) {
      return Left(ServerFailure(e.message));
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, User>> register({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
    required String phoneNumber,
    required UserRole role,
    String? dealershipName,
  }) async {
    try {
      if (_useMock) {
        final response = await _mockDataSource.register(
          email: email,
          password: password,
          firstName: firstName,
          lastName: lastName,
          phoneNumber: phoneNumber,
          role: role.toShortString(),
          dealershipName: dealershipName,
        );
        final user = UserModel.fromJson(response['user'] as Map<String, dynamic>);
        final token = response['token'] as String;
        await _secureStorage.write(key: 'auth_token', value: token);
        await _secureStorage.write(key: 'user_id', value: user.id);
        return Right(user);
      }

      final authResponse = await _remoteDataSource.register(
        email: email,
        password: password,
        firstName: firstName,
        lastName: lastName,
        phoneNumber: phoneNumber,
        role: role.toShortString(),
        dealershipName: dealershipName,
      );
      await _secureStorage.write(key: 'user_id', value: authResponse.user.id);
      return Right(authResponse.user);
    } on ApiException catch (e) {
      return Left(ServerFailure(e.message));
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, User>> loginWithGoogle() async {
    try {
      // TODO: Implement real Google Sign In when ready
      final response = await _mockDataSource.loginWithGoogle();

      final user = UserModel.fromJson(response['user'] as Map<String, dynamic>);
      final token = response['token'] as String;

      await _secureStorage.write(key: 'auth_token', value: token);
      await _secureStorage.write(key: 'user_id', value: user.id);

      return Right(user);
    } on ApiException catch (e) {
      return Left(ServerFailure(e.message));
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, User>> loginWithApple() async {
    try {
      // TODO: Implement real Apple Sign In when ready
      final response = await _mockDataSource.loginWithApple();

      final user = UserModel.fromJson(response['user'] as Map<String, dynamic>);
      final token = response['token'] as String;

      await _secureStorage.write(key: 'auth_token', value: token);
      await _secureStorage.write(key: 'user_id', value: user.id);

      return Right(user);
    } on ApiException catch (e) {
      return Left(ServerFailure(e.message));
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> logout() async {
    try {
      if (!_useMock) {
        await _remoteDataSource.logout();
      } else {
        await _mockDataSource.logout();
      }

      // Clear local storage
      await _secureStorage.delete(key: 'auth_token');
      await _secureStorage.delete(key: 'user_id');

      return const Right(null);
    } on ApiException catch (e) {
      return Left(ServerFailure(e.message));
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, User>> getCurrentUser() async {
    try {
      if (_useMock) {
        final response = await _mockDataSource.getCurrentUser();
        final user = UserModel.fromJson(response as Map<String, dynamic>);
        return Right(user);
      }
      
      final user = await _remoteDataSource.getCurrentUser();
      return Right(user);
    } on ApiException catch (e) {
      return Left(ServerFailure(e.message));
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> requestPasswordReset(String email) async {
    try {
      if (!_useMock) {
        await _remoteDataSource.requestPasswordReset(email);
      } else {
        await _mockDataSource.requestPasswordReset(email);
      }
      return const Right(null);
    } on ApiException catch (e) {
      return Left(ServerFailure(e.message));
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> resetPassword({
    required String email,
    required String code,
    required String newPassword,
  }) async {
    try {
      if (!_useMock) {
        await _remoteDataSource.resetPassword(
          email: email,
          code: code,
          newPassword: newPassword,
        );
      } else {
        await _mockDataSource.resetPassword(
          email: email,
          code: code,
          newPassword: newPassword,
        );
      }
      return const Right(null);
    } on ApiException catch (e) {
      return Left(ServerFailure(e.message));
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> verifyEmail({
    required String email,
    required String code,
  }) async {
    try {
      if (!_useMock) {
        await _remoteDataSource.verifyEmail(email: email, code: code);
      } else {
        await _mockDataSource.verifyEmail(email: email, code: code);
      }
      return const Right(null);
    } on ApiException catch (e) {
      return Left(ServerFailure(e.message));
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, User>> updateProfile({
    required String userId,
    String? firstName,
    String? lastName,
    String? phoneNumber,
    String? avatarUrl,
  }) async {
    try {
      if (_useMock) {
        final response = await _mockDataSource.updateProfile(
          userId: userId,
          firstName: firstName,
          lastName: lastName,
          phoneNumber: phoneNumber,
          avatarUrl: avatarUrl,
        );
        final user = UserModel.fromJson(response as Map<String, dynamic>);
        return Right(user);
      }
      
      final user = await _remoteDataSource.updateProfile(
        firstName: firstName,
        lastName: lastName,
        phoneNumber: phoneNumber,
        avatarUrl: avatarUrl,
      );
      return Right(user);
    } on ApiException catch (e) {
      return Left(ServerFailure(e.message));
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, bool>> checkEmailAvailability(String email) async {
    try {
      final isAvailable = _useMock
          ? await _mockDataSource.checkEmailAvailability(email)
          : await _remoteDataSource.checkEmailAvailability(email);
      return Right(isAvailable);
    } on ApiException catch (e) {
      return Left(ServerFailure(e.message));
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, String?>> getStoredToken() async {
    try {
      final token = await _secureStorage.read(key: 'auth_token');
      return Right(token);
    } catch (e) {
      return Left(CacheFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, bool>> isAuthenticated() async {
    try {
      if (_useMock) {
        return Right(_mockDataSource.isAuthenticated());
      }
      final token = await _secureStorage.read(key: 'auth_token');
      return Right(token != null && token.isNotEmpty);
    } catch (e) {
      return Left(CacheFailure(e.toString()));
    }
  }
}
