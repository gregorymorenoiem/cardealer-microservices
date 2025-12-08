import 'package:dartz/dartz.dart';
import '../../../core/errors/failures.dart';
import '../../entities/connectivity.dart';
import '../../repositories/connectivity_repository.dart';

/// Use case to check current connectivity status
class CheckConnectivity {
  final ConnectivityRepository repository;

  CheckConnectivity(this.repository);

  Future<Either<Failure, ConnectivityState>> call() async {
    try {
      final connectivity = await repository.getCurrentConnectivity();
      return Right(connectivity);
    } catch (e) {
      return Left(
          NetworkFailure('Error al verificar conectividad: ${e.toString()}'));
    }
  }
}

/// Use case to get connectivity stream
class WatchConnectivity {
  final ConnectivityRepository repository;

  WatchConnectivity(this.repository);

  Stream<ConnectivityState> call() {
    return repository.connectivityStream;
  }
}

/// Use case to check internet access
class CheckInternetAccess {
  final ConnectivityRepository repository;

  CheckInternetAccess(this.repository);

  Future<Either<Failure, bool>> call() async {
    try {
      final hasInternet = await repository.hasInternetAccess();
      return Right(hasInternet);
    } catch (e) {
      return Left(NetworkFailure(
          'Error al verificar acceso a internet: ${e.toString()}'));
    }
  }
}
