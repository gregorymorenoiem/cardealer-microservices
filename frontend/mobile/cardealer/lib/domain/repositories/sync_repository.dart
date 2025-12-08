import '../entities/connectivity.dart';

/// Repository interface for sync operations
abstract class SyncRepository {
  /// Add an operation to the sync queue
  Future<void> queueOperation(SyncOperation operation);

  /// Get all pending sync operations
  Future<List<SyncOperation>> getPendingOperations();

  /// Get sync status
  Future<SyncStatus> getSyncStatus();

  /// Process sync queue (send operations to server)
  Future<void> processSyncQueue();

  /// Remove an operation from queue (after successful sync)
  Future<void> removeOperation(String operationId);

  /// Mark an operation as failed
  Future<void> markOperationAsFailed(String operationId, String error);

  /// Clear all failed operations
  Future<void> clearFailedOperations();

  /// Retry failed operations
  Future<void> retryFailedOperations();

  /// Stream of sync status changes
  Stream<SyncStatus> get syncStatusStream;
}
