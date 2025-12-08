import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../domain/entities/user.dart';
import '../../../domain/usecases/auth/login_usecase.dart';
import '../../../domain/usecases/auth/register_usecase.dart';
import '../../../domain/usecases/auth/login_with_google_usecase.dart';
import '../../../domain/usecases/auth/login_with_apple_usecase.dart';
import '../../../domain/usecases/auth/logout_usecase.dart';
import '../../../domain/usecases/auth/get_current_user_usecase.dart';
import '../../../domain/usecases/auth/check_auth_status_usecase.dart';
import 'auth_event.dart';
import 'auth_state.dart';

/// BLoC for managing authentication state
/// Handles login, registration, social auth, and logout
class AuthBloc extends Bloc<AuthEvent, AuthState> {
  final LoginUseCase _loginUseCase;
  final RegisterUseCase _registerUseCase;
  final LoginWithGoogleUseCase _loginWithGoogleUseCase;
  final LoginWithAppleUseCase _loginWithAppleUseCase;
  final LogoutUseCase _logoutUseCase;
  final GetCurrentUserUseCase _getCurrentUserUseCase;
  final CheckAuthStatusUseCase _checkAuthStatusUseCase;

  AuthBloc({
    required LoginUseCase loginUseCase,
    required RegisterUseCase registerUseCase,
    required LoginWithGoogleUseCase loginWithGoogleUseCase,
    required LoginWithAppleUseCase loginWithAppleUseCase,
    required LogoutUseCase logoutUseCase,
    required GetCurrentUserUseCase getCurrentUserUseCase,
    required CheckAuthStatusUseCase checkAuthStatusUseCase,
  })  : _loginUseCase = loginUseCase,
        _registerUseCase = registerUseCase,
        _loginWithGoogleUseCase = loginWithGoogleUseCase,
        _loginWithAppleUseCase = loginWithAppleUseCase,
        _logoutUseCase = logoutUseCase,
        _getCurrentUserUseCase = getCurrentUserUseCase,
        _checkAuthStatusUseCase = checkAuthStatusUseCase,
        super(const AuthInitial()) {
    on<AuthCheckRequested>(_onAuthCheckRequested);
    on<AuthLoginRequested>(_onAuthLoginRequested);
    on<AuthRegisterRequested>(_onAuthRegisterRequested);
    on<AuthGoogleLoginRequested>(_onAuthGoogleLoginRequested);
    on<AuthAppleLoginRequested>(_onAuthAppleLoginRequested);
    on<AuthLogoutRequested>(_onAuthLogoutRequested);
    on<AuthUserRefreshed>(_onAuthUserRefreshed);
  }

  /// Check if user is authenticated on app start
  Future<void> _onAuthCheckRequested(
    AuthCheckRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(const AuthLoading());

    final result = await _checkAuthStatusUseCase();

    result.fold(
      (failure) {
        emit(const AuthUnauthenticated());
      },
      (isAuthenticated) async {
        if (isAuthenticated) {
          // Get current user data
          final userResult = await _getCurrentUserUseCase();
          userResult.fold(
            (failure) {
              emit(const AuthUnauthenticated());
            },
            (user) {
              emit(AuthAuthenticated(user: user));
            },
          );
        } else {
          emit(const AuthUnauthenticated());
        }
      },
    );
  }

  /// Handle email/password login
  Future<void> _onAuthLoginRequested(
    AuthLoginRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(const AuthLoading());

    final result = await _loginUseCase(
      email: event.email,
      password: event.password,
    );

    result.fold(
      (failure) {
        emit(AuthError(message: failure.message));
      },
      (user) {
        emit(AuthAuthenticated(user: user));
      },
    );
  }

  /// Handle user registration
  Future<void> _onAuthRegisterRequested(
    AuthRegisterRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(const AuthLoading());

    final result = await _registerUseCase(
      email: event.email,
      password: event.password,
      firstName: event.firstName,
      lastName: event.lastName,
      phoneNumber: event.phoneNumber,
      role: event.role.toUserRole(),
      dealershipName: event.dealershipName,
    );

    result.fold(
      (failure) {
        emit(AuthError(message: failure.message));
      },
      (user) {
        emit(AuthRegistrationSuccess(
          user: user,
          needsVerification: !user.isVerified,
        ));
      },
    );
  }

  /// Handle Google Sign In
  Future<void> _onAuthGoogleLoginRequested(
    AuthGoogleLoginRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(const AuthLoading());

    final result = await _loginWithGoogleUseCase();

    result.fold(
      (failure) {
        emit(AuthError(message: failure.message));
      },
      (user) {
        emit(AuthAuthenticated(user: user));
      },
    );
  }

  /// Handle Apple Sign In
  Future<void> _onAuthAppleLoginRequested(
    AuthAppleLoginRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(const AuthLoading());

    final result = await _loginWithAppleUseCase();

    result.fold(
      (failure) {
        emit(AuthError(message: failure.message));
      },
      (user) {
        emit(AuthAuthenticated(user: user));
      },
    );
  }

  /// Handle logout
  Future<void> _onAuthLogoutRequested(
    AuthLogoutRequested event,
    Emitter<AuthState> emit,
  ) async {
    emit(const AuthLoading());

    final result = await _logoutUseCase();

    result.fold(
      (failure) {
        emit(AuthError(message: failure.message));
      },
      (_) {
        emit(const AuthUnauthenticated());
      },
    );
  }

  /// Refresh current user data
  Future<void> _onAuthUserRefreshed(
    AuthUserRefreshed event,
    Emitter<AuthState> emit,
  ) async {
    if (state is AuthAuthenticated) {
      final result = await _getCurrentUserUseCase();

      result.fold(
        (failure) {
          emit(AuthError(message: failure.message));
        },
        (user) {
          emit(AuthAuthenticated(user: user));
        },
      );
    }
  }
}
