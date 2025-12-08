import 'package:dartz/dartz.dart';
import '../../../core/errors/failures.dart';
import '../../entities/user.dart';
import '../../repositories/auth_repository.dart';

/// Use case for Apple Sign In
class LoginWithAppleUseCase {
  final AuthRepository _repository;

  LoginWithAppleUseCase(this._repository);

  Future<Either<Failure, User>> call() async {
    return await _repository.loginWithApple();
  }
}
