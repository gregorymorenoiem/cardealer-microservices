import 'dart:async';
import 'dart:io';
import 'package:connectivity_plus/connectivity_plus.dart';
import '../../domain/entities/connectivity.dart';

/// Data source for connectivity operations
class ConnectivityDataSource {
  final Connectivity _connectivity;
  final StreamController<ConnectivityState> _connectivityController;

  ConnectivityDataSource()
      : _connectivity = Connectivity(),
        _connectivityController =
            StreamController<ConnectivityState>.broadcast() {
    _initConnectivityListener();
  }

  /// Initialize connectivity listener
  void _initConnectivityListener() {
    _connectivity.onConnectivityChanged.listen((ConnectivityResult result) {
      final state = _mapConnectivityResult(result);
      _connectivityController.add(state);
    });
  }

  /// Get current connectivity status
  Future<ConnectivityState> getCurrentConnectivity() async {
    try {
      final result = await _connectivity.checkConnectivity();
      return _mapConnectivityResult(result);
    } catch (e) {
      // If check fails, assume offline
      return ConnectivityState.offline();
    }
  }

  /// Stream of connectivity changes
  Stream<ConnectivityState> get connectivityStream =>
      _connectivityController.stream;

  /// Check if device is online (basic check)
  Future<bool> isOnline() async {
    final state = await getCurrentConnectivity();
    return state.isOnline;
  }

  /// Check internet access by attempting to reach a server
  Future<bool> hasInternetAccess() async {
    try {
      // Try to lookup Google DNS
      final result = await InternetAddress.lookup('google.com');
      return result.isNotEmpty && result[0].rawAddress.isNotEmpty;
    } on SocketException catch (_) {
      return false;
    } catch (_) {
      return false;
    }
  }

  /// Map connectivity_plus result to domain entity
  ConnectivityState _mapConnectivityResult(ConnectivityResult result) {
    switch (result) {
      case ConnectivityResult.wifi:
        return ConnectivityState.wifi();
      case ConnectivityResult.mobile:
        return ConnectivityState.mobile();
      case ConnectivityResult.ethernet:
        return ConnectivityState.wifi(); // Treat ethernet as WiFi
      case ConnectivityResult.vpn:
        return ConnectivityState.wifi(); // VPN means online
      case ConnectivityResult.bluetooth:
      case ConnectivityResult.other:
      case ConnectivityResult.none:
        return ConnectivityState.offline();
    }
  }

  /// Dispose resources
  void dispose() {
    _connectivityController.close();
  }
}
