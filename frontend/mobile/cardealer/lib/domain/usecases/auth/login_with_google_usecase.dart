import 'package:dartz/dartz.dart';
import '../../../core/errors/failures.dart';
import '../../entities/user.dart';
import '../../repositories/auth_repository.dart';

/// Use case for Google OAuth login
class LoginWithGoogleUseCase {
  final AuthRepository _repository;

  LoginWithGoogleUseCase(this._repository);

  Future<Either<Failure, User>> call() async {
    return await _repository.loginWithGoogle();
  }
}
