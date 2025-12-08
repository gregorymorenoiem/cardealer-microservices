import '../entities/connectivity.dart';

/// Repository interface for connectivity operations
abstract class ConnectivityRepository {
  /// Get current connectivity status
  Future<ConnectivityState> getCurrentConnectivity();

  /// Stream of connectivity changes
  Stream<ConnectivityState> get connectivityStream;

  /// Check if device is currently online
  Future<bool> isOnline();

  /// Check internet connectivity by pinging a server
  Future<bool> hasInternetAccess();
}
