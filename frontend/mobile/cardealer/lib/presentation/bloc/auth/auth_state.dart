import 'package:equatable/equatable.dart';
import '../../../domain/entities/user.dart';

/// Base class for all authentication states
abstract class AuthState extends Equatable {
  const AuthState();

  @override
  List<Object?> get props => [];
}

/// Initial state - auth status not checked yet
class AuthInitial extends AuthState {
  const AuthInitial();
}

/// Loading state during authentication operations
class AuthLoading extends AuthState {
  const AuthLoading();
}

/// User is authenticated
class AuthAuthenticated extends AuthState {
  final User user;

  const AuthAuthenticated({required this.user});

  @override
  List<Object> get props => [user];
}

/// User is not authenticated
class AuthUnauthenticated extends AuthState {
  const AuthUnauthenticated();
}

/// Authentication error occurred
class AuthError extends AuthState {
  final String message;

  const AuthError({required this.message});

  @override
  List<Object> get props => [message];
}

/// Registration successful but needs email verification
class AuthRegistrationSuccess extends AuthState {
  final User user;
  final bool needsVerification;

  const AuthRegistrationSuccess({
    required this.user,
    this.needsVerification = true,
  });

  @override
  List<Object> get props => [user, needsVerification];
}
