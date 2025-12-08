import 'package:dartz/dartz.dart';
import '../error/failures.dart';

/// Base class for all use cases
/// Type represents the return type, Params represents the parameters
abstract class UseCase<Type, Params> {
  Future<Either<Failure, Type>> call(Params params);
}

/// No parameters use case
class NoParams {}
