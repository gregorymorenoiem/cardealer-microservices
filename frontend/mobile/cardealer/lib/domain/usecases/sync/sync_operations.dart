import 'package:dartz/dartz.dart';
import '../../../core/errors/failures.dart';
import '../../entities/connectivity.dart';
import '../../repositories/sync_repository.dart';

/// Use case to queue an operation for later sync
class QueueSyncOperation {
  final SyncRepository repository;

  QueueSyncOperation(this.repository);

  Future<Either<Failure, void>> call(SyncOperation operation) async {
    try {
      await repository.queueOperation(operation);
      return const Right(null);
    } catch (e) {
      return Left(CacheFailure(
          'Error al agregar operación a la cola: ${e.toString()}'));
    }
  }
}

/// Use case to process sync queue
class ProcessSyncQueue {
  final SyncRepository repository;

  ProcessSyncQueue(this.repository);

  Future<Either<Failure, void>> call() async {
    try {
      await repository.processSyncQueue();
      return const Right(null);
    } catch (e) {
      return Left(NetworkFailure(
          'Error al procesar cola de sincronización: ${e.toString()}'));
    }
  }
}

/// Use case to get sync status
class GetSyncStatus {
  final SyncRepository repository;

  GetSyncStatus(this.repository);

  Future<Either<Failure, SyncStatus>> call() async {
    try {
      final status = await repository.getSyncStatus();
      return Right(status);
    } catch (e) {
      return Left(CacheFailure(
          'Error al obtener estado de sincronización: ${e.toString()}'));
    }
  }
}

/// Use case to watch sync status changes
class WatchSyncStatus {
  final SyncRepository repository;

  WatchSyncStatus(this.repository);

  Stream<SyncStatus> call() {
    return repository.syncStatusStream;
  }
}

/// Use case to retry failed operations
class RetryFailedSync {
  final SyncRepository repository;

  RetryFailedSync(this.repository);

  Future<Either<Failure, void>> call() async {
    try {
      await repository.retryFailedOperations();
      return const Right(null);
    } catch (e) {
      return Left(NetworkFailure(
          'Error al reintentar operaciones fallidas: ${e.toString()}'));
    }
  }
}
