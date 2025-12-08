# 🔄 Mock Data Strategy - CarDealer Mobile

## 📋 Overview

Este documento explica la estrategia de **mock data** implementada en la aplicación móvil de CarDealer mientras el backend API no está disponible.

---

## 🎯 Objetivo

Permitir el desarrollo y testing completo de la aplicación móvil **sin depender del backend**, con la capacidad de **activar fácilmente** la comunicación real con el API cuando esté listo.

---

## 🏗️ Arquitectura

### Dual Data Source Pattern

```
┌─────────────────┐
│   Presentation  │ (BLoC/UI)
└────────┬────────┘
         │
┌────────▼────────┐
│   Repository    │ (Abstraction)
│  Implementation │
└────┬───────┬────┘
     │       │
     │       └──────────────┐
     │                      │
┌────▼─────────┐    ┌──────▼──────────┐
│ Mock Data    │    │ Remote Data     │
│ Source       │    │ Source (API)    │
│ (ACTIVE)     │    │ (INACTIVE)      │
└──────────────┘    └─────────────────┘
```

### Switch Mechanism

En cada repositorio existe un flag para cambiar entre mock y real API:

```dart
// auth_repository_impl.dart
class AuthRepositoryImpl implements AuthRepository {
  // Flag to switch between mock and real API
  // TODO: Set to true when API is ready
  static const bool _useRealAPI = false;  // ← CAMBIAR A true
  
  Future<Either<Failure, User>> login(...) async {
    final response = _useRealAPI
        ? await _remoteDataSource.login(...)  // Real API
        : await _mockDataSource.login(...);   // Mock data
  }
}
```

---

## 📂 Estructura de Archivos

### Mock Data Sources

```
lib/data/datasources/
├── mock/
│   ├── mock_auth_datasource.dart        ✅ (auth mock data)
│   ├── mock_vehicle_datasource.dart     🔜 (vehicle mock data)
│   ├── mock_dealer_datasource.dart      🔜 (dealer mock data)
│   └── mock_favorite_datasource.dart    🔜 (favorites mock data)
│
└── remote/
    ├── auth_remote_datasource.dart      ⏸️ (auth API - preparado)
    ├── vehicle_remote_datasource.dart   🔜 (vehicle API - pendiente)
    ├── dealer_remote_datasource.dart    🔜 (dealer API - pendiente)
    └── favorite_remote_datasource.dart  🔜 (favorites API - pendiente)
```

### Repository Implementations

```
lib/data/repositories/
├── auth_repository_impl.dart            ✅ (con switch mock/real)
├── vehicle_repository_impl.dart         🔜 (próximo sprint)
├── dealer_repository_impl.dart          🔜 (próximo sprint)
└── favorite_repository_impl.dart        🔜 (próximo sprint)
```

---

## 🔐 Authentication Mock Data

### Mock Users Database

El `MockAuthDataSource` incluye usuarios de prueba:

```dart
static final List<Map<String, dynamic>> _mockUsers = [
  {
    'id': '1',
    'email': 'demo@cardealer.com',
    'password': 'Demo123!',
    'firstName': 'Demo',
    'lastName': 'User',
    'role': 'individual',
  },
  {
    'id': '2',
    'email': 'dealer@cardealer.com',
    'password': 'Dealer123!',
    'firstName': 'John',
    'lastName': 'Dealer',
    'role': 'dealer',
    'dealershipName': 'Premium Auto Sales',
  },
];
```

### Mock API Delays

Todas las operaciones mock incluyen delays realistas:

```dart
static const _apiDelay = Duration(milliseconds: 1500);

Future<Map<String, dynamic>> login(...) async {
  await Future.delayed(_apiDelay);  // Simula latencia de red
  // ... mock logic
}
```

### Available Mock Operations

#### ✅ Implemented
- ✅ Login (email/password)
- ✅ Register (new user)
- ✅ Login with Google (simulated)
- ✅ Login with Apple (simulated)
- ✅ Logout
- ✅ Get current user
- ✅ Refresh token
- ✅ Request password reset
- ✅ Reset password (with code)
- ✅ Verify email (with code)
- ✅ Update user profile
- ✅ Check email availability

#### 🔜 Next Sprints
- Vehicle listing (mock data)
- Vehicle search/filter (mock data)
- Dealer operations (mock data)
- Favorites/wishlist (mock data)
- Messages (mock data)

---

## 🔄 How to Switch to Real API

### Step 1: Update Repository Flag

```dart
// lib/data/repositories/auth_repository_impl.dart
class AuthRepositoryImpl implements AuthRepository {
  static const bool _useRealAPI = true;  // ← CAMBIAR de false a true
  // ...
}
```

### Step 2: Complete Remote Data Source

```dart
// lib/data/datasources/remote/auth_remote_datasource.dart
Future<Map<String, dynamic>> login({
  required String email,
  required String password,
}) async {
  final response = await _dio.post(
    '$_baseUrl/auth/login',
    data: {
      'email': email,
      'password': password,
    },
  );
  return response.data;
}
```

### Step 3: Configure API Base URL

```dart
// lib/app_config.dart
class AppConfig {
  static String get apiBaseUrl {
    switch (flavor) {
      case Flavor.dev:
        return 'https://dev-api.cardealer.com';
      case Flavor.staging:
        return 'https://staging-api.cardealer.com';
      case Flavor.prod:
        return 'https://api.cardealer.com';
    }
  }
}
```

### Step 4: Test with Real API

```bash
# Run in dev flavor
flutter run --flavor dev --target lib/main_dev.dart

# Check logs
flutter logs
```

---

## 🧪 Testing with Mock Data

### Unit Tests

```dart
test('login should return user from mock data', () async {
  // Arrange
  final mockDataSource = MockAuthDataSource();
  
  // Act
  final result = await mockDataSource.login(
    email: 'demo@cardealer.com',
    password: 'Demo123!',
  );
  
  // Assert
  expect(result['user']['email'], 'demo@cardealer.com');
  expect(result['token'], isNotNull);
});
```

### Integration Tests

```dart
testWidgets('login flow with mock data', (tester) async {
  await tester.pumpWidget(MyApp());
  
  // Enter credentials
  await tester.enterText(emailField, 'demo@cardealer.com');
  await tester.enterText(passwordField, 'Demo123!');
  
  // Tap login
  await tester.tap(loginButton);
  await tester.pumpAndSettle();
  
  // Verify authenticated
  expect(find.text('Welcome, Demo'), findsOneWidget);
});
```

---

## 📊 Mock Data Features

### Session Management

```dart
// Simulated session storage
static String? _currentToken;
static Map<String, dynamic>? _currentUser;
```

### Token Generation

```dart
// Generate mock token
_currentToken = 'mock_token_${DateTime.now().millisecondsSinceEpoch}';
```

### Error Simulation

```dart
// Check if email already exists
if (_mockUsers.any((u) => u['email'] == email)) {
  throw Exception('Email already registered');
}
```

### Verification Codes

```dart
// Accept any 6-digit code for demo
if (code.length != 6) {
  throw Exception('Invalid verification code');
}
```

---

## 🎨 UI Development Benefits

### Immediate Feedback
- No API downtime blocks development
- Instant responses (1.5s simulated delay)
- Consistent test data

### Demo Capabilities
- Pre-loaded demo accounts
- Predictable behavior
- Easy client presentations

### Offline Development
- Work without internet
- No VPN requirements
- Local testing only

---

## 🔐 Security Notes

### Mock Data Security
- ⚠️ **Mock credentials should NEVER be used in production**
- ⚠️ **Mock tokens have no real security**
- ⚠️ **Clear mock flag before production release**

### Production Checklist
```dart
// ❌ NEVER ship with mock data enabled
static const bool _useRealAPI = false;  // ❌ BAD

// ✅ ALWAYS use real API in production
static const bool _useRealAPI = true;   // ✅ GOOD
```

---

## 📝 Mock Data Maintenance

### Adding New Mock Users

```dart
// lib/data/datasources/mock/mock_auth_datasource.dart
static final List<Map<String, dynamic>> _mockUsers = [
  // ... existing users
  {
    'id': '3',
    'email': 'newuser@cardealer.com',
    'password': 'NewUser123!',
    'firstName': 'New',
    'lastName': 'User',
    'role': 'individual',
  },
];
```

### Adding New Mock Operations

```dart
/// New mock operation
Future<Map<String, dynamic>> newOperation() async {
  await Future.delayed(_apiDelay);
  
  // Mock logic here
  
  return {
    'success': true,
    'data': mockData,
  };
}
```

---

## 🚀 Next Steps

### Sprint 2 (Current)
- ✅ Auth mock data complete
- 🔄 Login/Register UI (in progress)
- 🔜 Onboarding flow
- 🔜 Profile setup

### Sprint 3 (Vehicles)
- 🔜 Vehicle mock data source
- 🔜 Vehicle list mock data (50+ vehicles)
- 🔜 Search/filter mock data
- 🔜 Vehicle details mock data

### Sprint 4 (Dealer)
- 🔜 Dealer mock data source
- 🔜 Dealer dashboard mock data
- 🔜 Dealer CRM mock data
- 🔜 Analytics mock data

### Sprint 5 (Social)
- 🔜 Favorites mock data
- 🔜 Messages mock data
- 🔜 Notifications mock data

---

## 📞 Support

### Questions?
- Check this document first
- Review `mock_*_datasource.dart` files
- Look at repository implementations
- Check `auth_repository_impl.dart` for switch pattern

### Issues?
- Mock data not working? Check `_useRealAPI` flag
- Need new mock users? Add to `_mockUsers` list
- Need new mock operation? Follow existing patterns

---

## ✅ Summary

| Feature | Status | File |
|---------|--------|------|
| Mock Auth Data | ✅ Complete | `mock_auth_datasource.dart` |
| Real API (Auth) | ⏸️ Ready | `auth_remote_datasource.dart` |
| Switch Mechanism | ✅ Complete | `auth_repository_impl.dart` |
| Mock Vehicles | 🔜 Sprint 3 | - |
| Mock Dealers | 🔜 Sprint 4 | - |
| Mock Social | 🔜 Sprint 5 | - |

**Current Mode**: 🔄 **MOCK DATA ONLY** (Real API inactive)

---

**Last Updated**: December 2025  
**Author**: GitHub Copilot  
**Status**: ✅ Active Development with Mock Data
