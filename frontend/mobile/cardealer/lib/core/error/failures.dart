import 'package:equatable/equatable.dart';

/// Base failure class
abstract class Failure extends Equatable {
  final String message;

  const Failure({required this.message});

  @override
  List<Object?> get props => [message];
}

/// Server failure (API errors, network issues)
class ServerFailure extends Failure {
  const ServerFailure({required super.message});
}

/// Cache failure (local storage issues)
class CacheFailure extends Failure {
  const CacheFailure({required super.message});
}

/// Validation failure (input validation errors)
class ValidationFailure extends Failure {
  const ValidationFailure({required super.message});
}

/// Authentication failure (auth errors)
class AuthFailure extends Failure {
  const AuthFailure({required super.message});
}

/// Network failure (no internet connection)
class NetworkFailure extends Failure {
  const NetworkFailure({required super.message});
}

/// Not found failure (resource not found)
class NotFoundFailure extends Failure {
  const NotFoundFailure({required super.message});
}
