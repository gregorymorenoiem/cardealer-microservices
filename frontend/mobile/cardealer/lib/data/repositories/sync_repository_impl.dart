import '../../domain/entities/connectivity.dart';
import '../../domain/repositories/sync_repository.dart';
import '../datasources/sync_local_datasource.dart';

/// Implementation of SyncRepository
class SyncRepositoryImpl implements SyncRepository {
  final SyncLocalDataSource localDataSource;

  SyncRepositoryImpl(this.localDataSource);

  @override
  Future<void> queueOperation(SyncOperation operation) {
    return localDataSource.queueOperation(operation);
  }

  @override
  Future<List<SyncOperation>> getPendingOperations() {
    return localDataSource.getPendingOperations();
  }

  @override
  Future<SyncStatus> getSyncStatus() {
    return localDataSource.getSyncStatus();
  }

  @override
  Future<void> processSyncQueue() async {
    final operations = await localDataSource.getPendingOperations();

    for (var operation in operations) {
      try {
        // TODO: Send operation to backend API based on operationType
        // For now, just simulate processing
        await Future.delayed(const Duration(milliseconds: 500));

        // Remove from queue after successful processing
        await localDataSource.removeOperation(operation.id);
      } catch (e) {
        // Mark as failed if processing fails
        await localDataSource.markOperationAsFailed(
          operation.id,
          e.toString(),
        );
      }
    }

    // Update last sync time
    await localDataSource.updateLastSyncTime();
  }

  @override
  Future<void> removeOperation(String operationId) {
    return localDataSource.removeOperation(operationId);
  }

  @override
  Future<void> markOperationAsFailed(String operationId, String error) {
    return localDataSource.markOperationAsFailed(operationId, error);
  }

  @override
  Future<void> clearFailedOperations() {
    return localDataSource.clearFailedOperations();
  }

  @override
  Future<void> retryFailedOperations() async {
    final failedOps = await localDataSource.getFailedOperations();

    for (var operation in failedOps) {
      await localDataSource.retryOperation(operation.id);
    }

    // Process the queue again
    await processSyncQueue();
  }

  @override
  Stream<SyncStatus> get syncStatusStream {
    return localDataSource.syncStatusStream;
  }
}
