/// Network information interface
abstract class NetworkInfo {
  Future<bool> get isConnected;
}

/// Implementation using connectivity package (mock for now)
class NetworkInfoImpl implements NetworkInfo {
  @override
  Future<bool> get isConnected async {
    // TODO: Implement with connectivity_plus package when adding real API
    return true;
  }
}
