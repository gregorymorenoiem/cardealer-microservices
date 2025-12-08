import '../../domain/entities/connectivity.dart';
import '../../domain/repositories/connectivity_repository.dart';
import '../datasources/connectivity_datasource.dart';

/// Implementation of ConnectivityRepository
class ConnectivityRepositoryImpl implements ConnectivityRepository {
  final ConnectivityDataSource dataSource;

  ConnectivityRepositoryImpl(this.dataSource);

  @override
  Future<ConnectivityState> getCurrentConnectivity() {
    return dataSource.getCurrentConnectivity();
  }

  @override
  Stream<ConnectivityState> get connectivityStream {
    return dataSource.connectivityStream;
  }

  @override
  Future<bool> isOnline() {
    return dataSource.isOnline();
  }

  @override
  Future<bool> hasInternetAccess() {
    return dataSource.hasInternetAccess();
  }
}
