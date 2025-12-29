import 'package:dartz/dartz.dart';
import '../../core/errors/failures.dart';
import '../../domain/entities/user.dart';
import '../../domain/repositories/auth_repository.dart';
import '../datasources/mock/mock_auth_datasource.dart';
import '../datasources/remote/auth_remote_datasource.dart';
import '../models/user_model.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Implementation of AuthRepository
/// Currently using mock data source
/// Will switch to remote data source when API is ready
class AuthRepositoryImpl implements AuthRepository {
  final MockAuthDataSource _mockDataSource;
  final AuthRemoteDataSource _remoteDataSource;
  final FlutterSecureStorage _secureStorage;

  // Flag to switch between mock and real API
  // TODO: Set to true when API is ready
  static const bool _useRealAPI = false;

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
      final response = _useRealAPI
          ? await _remoteDataSource.login(email: email, password: password)
          : await _mockDataSource.login(email: email, password: password);

      final user = UserModel.fromJson(response['user']);
      final token = response['token'] as String;

      // Save token securely
      await _secureStorage.write(key: 'auth_token', value: token);
      await _secureStorage.write(key: 'user_id', value: user.id);

      return Right(user);
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
      final response = _useRealAPI
          ? await _remoteDataSource.register(
              email: email,
              password: password,
              firstName: firstName,
              lastName: lastName,
              phoneNumber: phoneNumber,
              role: role.toShortString(),
              dealershipName: dealershipName,
            )
          : await _mockDataSource.register(
              email: email,
              password: password,
              firstName: firstName,
              lastName: lastName,
              phoneNumber: phoneNumber,
              role: role.toShortString(),
              dealershipName: dealershipName,
            );

      final user = UserModel.fromJson(response['user']);
      final token = response['token'] as String;

      // Save token securely
      await _secureStorage.write(key: 'auth_token', value: token);
      await _secureStorage.write(key: 'user_id', value: user.id);

      return Right(user);
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, User>> loginWithGoogle() async {
    try {
      // TODO: Implement real Google Sign In when ready
      final response = await _mockDataSource.loginWithGoogle();

      final user = UserModel.fromJson(response['user']);
      final token = response['token'] as String;

      await _secureStorage.write(key: 'auth_token', value: token);
      await _secureStorage.write(key: 'user_id', value: user.id);

      return Right(user);
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, User>> loginWithApple() async {
    try {
      // TODO: Implement real Apple Sign In when ready
      final response = await _mockDataSource.loginWithApple();

      final user = UserModel.fromJson(response['user']);
      final token = response['token'] as String;

      await _secureStorage.write(key: 'auth_token', value: token);
      await _secureStorage.write(key: 'user_id', value: user.id);

      return Right(user);
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> logout() async {
    try {
      if (_useRealAPI) {
        final token = await _secureStorage.read(key: 'auth_token');
        if (token != null) {
          await _remoteDataSource.logout(token);
        }
      } else {
        await _mockDataSource.logout();
      }

      // Clear local storage
      await _secureStorage.delete(key: 'auth_token');
      await _secureStorage.delete(key: 'user_id');

      return const Right(null);
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, User>> getCurrentUser() async {
    try {
      final response = _useRealAPI
          ? await _remoteDataSource.getCurrentUser(
              await _secureStorage.read(key: 'auth_token') ?? '',
            )
          : await _mockDataSource.getCurrentUser();

      final user = UserModel.fromJson(response);
      return Right(user);
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, void>> requestPasswordReset(String email) async {
    try {
      if (_useRealAPI) {
        await _remoteDataSource.requestPasswordReset(email);
      } else {
        await _mockDataSource.requestPasswordReset(email);
      }
      return const Right(null);
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
      if (_useRealAPI) {
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
      if (_useRealAPI) {
        await _remoteDataSource.verifyEmail(email: email, code: code);
      } else {
        await _mockDataSource.verifyEmail(email: email, code: code);
      }
      return const Right(null);
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
      final response = _useRealAPI
          ? await _remoteDataSource.updateProfile(
              token: await _secureStorage.read(key: 'auth_token') ?? '',
              firstName: firstName,
              lastName: lastName,
              phoneNumber: phoneNumber,
              avatarUrl: avatarUrl,
            )
          : await _mockDataSource.updateProfile(
              userId: userId,
              firstName: firstName,
              lastName: lastName,
              phoneNumber: phoneNumber,
              avatarUrl: avatarUrl,
            );

      final user = UserModel.fromJson(response);
      return Right(user);
    } catch (e) {
      return Left(ServerFailure(e.toString()));
    }
  }

  @override
  Future<Either<Failure, bool>> checkEmailAvailability(String email) async {
    try {
      final isAvailable = _useRealAPI
          ? await _remoteDataSource.checkEmailAvailability(email)
          : await _mockDataSource.checkEmailAvailability(email);
      return Right(isAvailable);
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
      if (_useRealAPI) {
        final token = await _secureStorage.read(key: 'auth_token');
        return Right(token != null && token.isNotEmpty);
      } else {
        return Right(_mockDataSource.isAuthenticated());
      }
    } catch (e) {
      return Left(CacheFailure(e.toString()));
    }
  }
}
