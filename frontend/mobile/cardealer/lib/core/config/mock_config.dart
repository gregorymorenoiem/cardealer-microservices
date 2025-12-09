import 'package:flutter/foundation.dart';
import '../../data/datasources/mock/mock_auth_datasource.dart';
import '../../data/datasources/mock/mock_vehicle_datasource.dart';
import '../../data/datasources/mock/mock_payment_datasource.dart';
import 'api_config.dart';

/// Mock Configuration
/// Manages whether to use mock data or real API
class MockConfig {
  /// Check if mock data is enabled
  static bool get isEnabled => ApiConfig.enableMockData;

  /// Initialize mock data sources
  static void initializeMockServices() {
    if (!isEnabled) return;

    // Register mock datasources in service locator
    // This will be used instead of real API calls

    debugPrint('🎭 Mock Data Enabled - Using mock datasources');
    debugPrint('   - MockAuthDataSource: Ready');
    debugPrint('   - MockVehicleDataSource: 71+ vehicles available');
    debugPrint('   - MockPaymentDataSource: Ready');
    debugPrint('');
    debugPrint('💡 To disable mocks, change ApiConfig.enableMockData to false');
  }

  /// Get mock auth datasource
  static MockAuthDataSource getAuthDataSource() {
    if (!isEnabled) {
      throw Exception('Mock data is disabled. Use real API instead.');
    }
    return MockAuthDataSource();
  }

  /// Get mock vehicle datasource
  static MockVehicleDataSource getVehicleDataSource() {
    if (!isEnabled) {
      throw Exception('Mock data is disabled. Use real API instead.');
    }
    return MockVehicleDataSource();
  }

  /// Get mock payment datasource
  static MockPaymentDataSource getPaymentDataSource() {
    if (!isEnabled) {
      throw Exception('Mock data is disabled. Use real API instead.');
    }
    return MockPaymentDataSource();
  }

  /// Quick access to mock data status
  static String get status => '''
🎭 Mock Data Configuration:
   Status: ${isEnabled ? '✅ ENABLED' : '❌ DISABLED'}
   Auth: ${isEnabled ? 'Using MockAuthDataSource' : 'Using Real API'}
   Vehicles: ${isEnabled ? 'Using MockVehicleDataSource (71+ items)' : 'Using Real API'}
   Payments: ${isEnabled ? 'Using MockPaymentDataSource' : 'Using Real API'}
''';
}
