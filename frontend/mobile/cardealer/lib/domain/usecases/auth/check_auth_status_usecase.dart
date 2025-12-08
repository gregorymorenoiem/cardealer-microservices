import 'package:dartz/dartz.dart';
import '../../../core/errors/failures.dart';
import '../../repositories/auth_repository.dart';

/// Use case for checking authentication status
class CheckAuthStatusUseCase {
  final AuthRepository _repository;

  CheckAuthStatusUseCase(this._repository);

  Future<Either<Failure, bool>> call() async {
    return await _repository.isAuthenticated();
  }
}
