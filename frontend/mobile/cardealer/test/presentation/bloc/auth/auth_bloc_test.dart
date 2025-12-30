import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:dartz/dartz.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:cardealer_mobile/core/errors/failures.dart';
import 'package:cardealer_mobile/domain/entities/user.dart';
import 'package:cardealer_mobile/domain/usecases/auth/login_usecase.dart';
import 'package:cardealer_mobile/domain/usecases/auth/register_usecase.dart';
import 'package:cardealer_mobile/domain/usecases/auth/login_with_google_usecase.dart';
import 'package:cardealer_mobile/domain/usecases/auth/login_with_apple_usecase.dart';
import 'package:cardealer_mobile/domain/usecases/auth/logout_usecase.dart';
import 'package:cardealer_mobile/domain/usecases/auth/get_current_user_usecase.dart';
import 'package:cardealer_mobile/domain/usecases/auth/check_auth_status_usecase.dart';
import 'package:cardealer_mobile/presentation/bloc/auth/auth_bloc.dart';
import 'package:cardealer_mobile/presentation/bloc/auth/auth_event.dart';
import 'package:cardealer_mobile/presentation/bloc/auth/auth_state.dart';

// Mock classes using mocktail - no code generation needed
class MockLoginUseCase extends Mock implements LoginUseCase {}
class MockRegisterUseCase extends Mock implements RegisterUseCase {}
class MockLoginWithGoogleUseCase extends Mock implements LoginWithGoogleUseCase {}
class MockLoginWithAppleUseCase extends Mock implements LoginWithAppleUseCase {}
class MockLogoutUseCase extends Mock implements LogoutUseCase {}
class MockGetCurrentUserUseCase extends Mock implements GetCurrentUserUseCase {}
class MockCheckAuthStatusUseCase extends Mock implements CheckAuthStatusUseCase {}

void main() {
  late AuthBloc authBloc;
  late MockLoginUseCase mockLoginUseCase;
  late MockRegisterUseCase mockRegisterUseCase;
  late MockLoginWithGoogleUseCase mockLoginWithGoogleUseCase;
  late MockLoginWithAppleUseCase mockLoginWithAppleUseCase;
  late MockLogoutUseCase mockLogoutUseCase;
  late MockGetCurrentUserUseCase mockGetCurrentUserUseCase;
  late MockCheckAuthStatusUseCase mockCheckAuthStatusUseCase;

  // Register fallback values for mocktail
  setUpAll(() {
    registerFallbackValue(UserRole.individual);
  });

  setUp(() {
    mockLoginUseCase = MockLoginUseCase();
    mockRegisterUseCase = MockRegisterUseCase();
    mockLoginWithGoogleUseCase = MockLoginWithGoogleUseCase();
    mockLoginWithAppleUseCase = MockLoginWithAppleUseCase();
    mockLogoutUseCase = MockLogoutUseCase();
    mockGetCurrentUserUseCase = MockGetCurrentUserUseCase();
    mockCheckAuthStatusUseCase = MockCheckAuthStatusUseCase();

    authBloc = AuthBloc(
      loginUseCase: mockLoginUseCase,
      registerUseCase: mockRegisterUseCase,
      loginWithGoogleUseCase: mockLoginWithGoogleUseCase,
      loginWithAppleUseCase: mockLoginWithAppleUseCase,
      logoutUseCase: mockLogoutUseCase,
      getCurrentUserUseCase: mockGetCurrentUserUseCase,
      checkAuthStatusUseCase: mockCheckAuthStatusUseCase,
    );
  });

  tearDown(() {
    authBloc.close();
  });

  // Test data
  final testUser = User(
    id: 'user_123',
    email: 'test@example.com',
    firstName: 'John',
    lastName: 'Doe',
    phoneNumber: '+1234567890',
    role: UserRole.individual,
    avatarUrl: 'https://example.com/avatar.jpg',
    isVerified: true,
    createdAt: DateTime(2024, 1, 1),
  );

  group('AuthBloc', () {
    test('initial state should be AuthInitial', () {
      expect(authBloc.state, isA<AuthInitial>());
    });
  });

  group('AuthBloc - Login', () {
    blocTest<AuthBloc, AuthState>(
      'emits [AuthLoading, AuthAuthenticated] when login is successful',
      build: () {
        when(() => mockLoginUseCase(
              email: any(named: 'email'),
              password: any(named: 'password'),
            )).thenAnswer((_) async => Right(testUser));
        return authBloc;
      },
      act: (bloc) => bloc.add(const AuthLoginRequested(
        email: 'test@example.com',
        password: 'password123',
      )),
      expect: () => [
        isA<AuthLoading>(),
        isA<AuthAuthenticated>().having(
          (state) => state.user.email,
          'user email',
          'test@example.com',
        ),
      ],
    );

    blocTest<AuthBloc, AuthState>(
      'emits [AuthLoading, AuthError] when login fails with invalid credentials',
      build: () {
        when(() => mockLoginUseCase(
              email: any(named: 'email'),
              password: any(named: 'password'),
            )).thenAnswer(
                (_) async => const Left(AuthFailure('Invalid credentials')));
        return authBloc;
      },
      act: (bloc) => bloc.add(const AuthLoginRequested(
        email: 'wrong@example.com',
        password: 'wrongpassword',
      )),
      expect: () => [
        isA<AuthLoading>(),
        isA<AuthError>().having(
          (state) => state.message,
          'error message',
          'Invalid credentials',
        ),
      ],
    );

    blocTest<AuthBloc, AuthState>(
      'emits [AuthLoading, AuthError] when login fails with network error',
      build: () {
        when(() => mockLoginUseCase(
              email: any(named: 'email'),
              password: any(named: 'password'),
            )).thenAnswer(
                (_) async => const Left(NetworkFailure('No internet connection')));
        return authBloc;
      },
      act: (bloc) => bloc.add(const AuthLoginRequested(
        email: 'test@example.com',
        password: 'password123',
      )),
      expect: () => [
        isA<AuthLoading>(),
        isA<AuthError>(),
      ],
    );
  });

  group('AuthBloc - Registration', () {
    blocTest<AuthBloc, AuthState>(
      'emits [AuthLoading, AuthRegistrationSuccess] when registration is successful',
      build: () {
        when(() => mockRegisterUseCase(
              email: any(named: 'email'),
              password: any(named: 'password'),
              firstName: any(named: 'firstName'),
              lastName: any(named: 'lastName'),
              phoneNumber: any(named: 'phoneNumber'),
              role: any(named: 'role'),
              dealershipName: any(named: 'dealershipName'),
            )).thenAnswer((_) async => Right(testUser));
        return authBloc;
      },
      act: (bloc) => bloc.add(const AuthRegisterRequested(
        email: 'newuser@example.com',
        password: 'Password123!',
        firstName: 'John',
        lastName: 'Doe',
        phoneNumber: '+1234567890',
        role: 'individual',
      )),
      expect: () => [
        isA<AuthLoading>(),
        isA<AuthRegistrationSuccess>(),
      ],
    );

    blocTest<AuthBloc, AuthState>(
      'emits [AuthLoading, AuthError] when email already exists',
      build: () {
        when(() => mockRegisterUseCase(
              email: any(named: 'email'),
              password: any(named: 'password'),
              firstName: any(named: 'firstName'),
              lastName: any(named: 'lastName'),
              phoneNumber: any(named: 'phoneNumber'),
              role: any(named: 'role'),
              dealershipName: any(named: 'dealershipName'),
            )).thenAnswer(
                (_) async => const Left(AuthFailure('Email already registered')));
        return authBloc;
      },
      act: (bloc) => bloc.add(const AuthRegisterRequested(
        email: 'existing@example.com',
        password: 'Password123!',
        firstName: 'John',
        lastName: 'Doe',
        phoneNumber: '+1234567890',
        role: 'individual',
      )),
      expect: () => [
        isA<AuthLoading>(),
        isA<AuthError>(),
      ],
    );
  });

  group('AuthBloc - Social Login', () {
    blocTest<AuthBloc, AuthState>(
      'emits [AuthLoading, AuthAuthenticated] when Google login is successful',
      build: () {
        when(() => mockLoginWithGoogleUseCase.call())
            .thenAnswer((_) async => Right(testUser));
        return authBloc;
      },
      act: (bloc) => bloc.add(const AuthGoogleLoginRequested()),
      expect: () => [
        isA<AuthLoading>(),
        isA<AuthAuthenticated>(),
      ],
    );

    blocTest<AuthBloc, AuthState>(
      'emits [AuthLoading, AuthError] when Google login fails',
      build: () {
        when(() => mockLoginWithGoogleUseCase.call()).thenAnswer(
            (_) async => const Left(AuthFailure('Google sign in cancelled')));
        return authBloc;
      },
      act: (bloc) => bloc.add(const AuthGoogleLoginRequested()),
      expect: () => [
        isA<AuthLoading>(),
        isA<AuthError>(),
      ],
    );

    blocTest<AuthBloc, AuthState>(
      'emits [AuthLoading, AuthAuthenticated] when Apple login is successful',
      build: () {
        when(() => mockLoginWithAppleUseCase.call())
            .thenAnswer((_) async => Right(testUser));
        return authBloc;
      },
      act: (bloc) => bloc.add(const AuthAppleLoginRequested()),
      expect: () => [
        isA<AuthLoading>(),
        isA<AuthAuthenticated>(),
      ],
    );

    blocTest<AuthBloc, AuthState>(
      'emits [AuthLoading, AuthError] when Apple login fails',
      build: () {
        when(() => mockLoginWithAppleUseCase.call()).thenAnswer(
            (_) async => const Left(AuthFailure('Apple sign in cancelled')));
        return authBloc;
      },
      act: (bloc) => bloc.add(const AuthAppleLoginRequested()),
      expect: () => [
        isA<AuthLoading>(),
        isA<AuthError>(),
      ],
    );
  });

  group('AuthBloc - Logout', () {
    blocTest<AuthBloc, AuthState>(
      'emits [AuthLoading, AuthUnauthenticated] when logout is successful',
      build: () {
        when(() => mockLogoutUseCase.call())
            .thenAnswer((_) async => const Right(null));
        return authBloc;
      },
      act: (bloc) => bloc.add(const AuthLogoutRequested()),
      expect: () => [
        isA<AuthLoading>(),
        isA<AuthUnauthenticated>(),
      ],
      verify: (_) {
        verify(() => mockLogoutUseCase.call()).called(1);
      },
    );
  });

  group('AuthBloc - Check Auth Status', () {
    blocTest<AuthBloc, AuthState>(
      'emits [AuthLoading, AuthUnauthenticated] when user is not authenticated',
      build: () {
        when(() => mockCheckAuthStatusUseCase.call())
            .thenAnswer((_) async => const Right(false));
        return authBloc;
      },
      act: (bloc) => bloc.add(const AuthCheckRequested()),
      expect: () => [
        isA<AuthLoading>(),
        isA<AuthUnauthenticated>(),
      ],
    );

    blocTest<AuthBloc, AuthState>(
      'emits [AuthLoading, AuthUnauthenticated] when check fails',
      build: () {
        when(() => mockCheckAuthStatusUseCase.call()).thenAnswer(
            (_) async => const Left(AuthFailure('Token expired')));
        return authBloc;
      },
      act: (bloc) => bloc.add(const AuthCheckRequested()),
      expect: () => [
        isA<AuthLoading>(),
        isA<AuthUnauthenticated>(),
      ],
    );
  });
}
