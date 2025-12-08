import 'package:dartz/dartz.dart';
import '../error/failures.dart';

/// Base class for all use cases
/// T represents the return type, Params represents the parameters
abstract class UseCase<T, Params> {
  Future<Either<Failure, T>> call(Params params);
}

/// No parameters use case
class NoParams {}
