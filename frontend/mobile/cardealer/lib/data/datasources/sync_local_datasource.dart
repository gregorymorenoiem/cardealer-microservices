import 'dart:async';
import 'dart:convert';
import 'package:hive_flutter/hive_flutter.dart';
import '../../domain/entities/connectivity.dart';

/// Local data source for sync operations using Hive
class SyncLocalDataSource {
  static const String _syncQueueBoxName = 'sync_queue';
  static const String _syncStatusKey = 'sync_status';

  final StreamController<SyncStatus> _syncStatusController;

  SyncLocalDataSource()
      : _syncStatusController = StreamController<SyncStatus>.broadcast();

  /// Initialize Hive boxes
  Future<void> initialize() async {
    await Hive.openBox<Map>(_syncQueueBoxName);
  }

  /// Get sync queue box
  Box<Map> get _syncQueueBox => Hive.box<Map>(_syncQueueBoxName);

  /// Add operation to sync queue
  Future<void> queueOperation(SyncOperation operation) async {
    final operationMap = _operationToMap(operation);
    await _syncQueueBox.put(operation.id, operationMap);
    await _updateSyncStatus();
  }

  /// Get all pending operations
  Future<List<SyncOperation>> getPendingOperations() async {
    final operations = <SyncOperation>[];

    for (var key in _syncQueueBox.keys) {
      final value = _syncQueueBox.get(key);
      if (value != null) {
        final operation = _mapToOperation(Map<String, dynamic>.from(value));
        if (!operation.isFailed) {
          operations.add(operation);
        }
      }
    }

    // Sort by creation time (oldest first)
    operations.sort((a, b) => a.createdAt.compareTo(b.createdAt));
    return operations;
  }

  /// Get all failed operations
  Future<List<SyncOperation>> getFailedOperations() async {
    final operations = <SyncOperation>[];

    for (var key in _syncQueueBox.keys) {
      final value = _syncQueueBox.get(key);
      if (value != null) {
        final operation = _mapToOperation(Map<String, dynamic>.from(value));
        if (operation.isFailed) {
          operations.add(operation);
        }
      }
    }

    return operations;
  }

  /// Get sync status
  Future<SyncStatus> getSyncStatus() async {
    final pending = await getPendingOperations();
    final failed = await getFailedOperations();

    final statusMap = _syncQueueBox.get(_syncStatusKey);
    DateTime? lastSyncTime;

    if (statusMap != null) {
      final statusData = Map<String, dynamic>.from(statusMap);
      if (statusData['lastSyncTime'] != null) {
        lastSyncTime = DateTime.parse(statusData['lastSyncTime'] as String);
      }
    }

    return SyncStatus(
      isSyncing: false,
      pendingOperations: pending.length,
      failedOperations: failed.length,
      lastSyncTime: lastSyncTime,
    );
  }

  /// Remove operation from queue
  Future<void> removeOperation(String operationId) async {
    await _syncQueueBox.delete(operationId);
    await _updateSyncStatus();
  }

  /// Mark operation as failed
  Future<void> markOperationAsFailed(String operationId, String error) async {
    final value = _syncQueueBox.get(operationId);
    if (value != null) {
      final operation = _mapToOperation(Map<String, dynamic>.from(value));
      final updatedOperation = operation.copyWith(
        isFailed: true,
        errorMessage: error,
        retryCount: operation.retryCount + 1,
      );
      await _syncQueueBox.put(operationId, _operationToMap(updatedOperation));
      await _updateSyncStatus();
    }
  }

  /// Clear all failed operations
  Future<void> clearFailedOperations() async {
    final failed = await getFailedOperations();
    for (var operation in failed) {
      await _syncQueueBox.delete(operation.id);
    }
    await _updateSyncStatus();
  }

  /// Mark operation for retry
  Future<void> retryOperation(String operationId) async {
    final value = _syncQueueBox.get(operationId);
    if (value != null) {
      final operation = _mapToOperation(Map<String, dynamic>.from(value));
      final updatedOperation = operation.copyWith(
        isFailed: false,
        errorMessage: null,
      );
      await _syncQueueBox.put(operationId, _operationToMap(updatedOperation));
      await _updateSyncStatus();
    }
  }

  /// Update last sync time
  Future<void> updateLastSyncTime() async {
    await _syncQueueBox.put(_syncStatusKey, {
      'lastSyncTime': DateTime.now().toIso8601String(),
    });
    await _updateSyncStatus();
  }

  /// Stream of sync status changes
  Stream<SyncStatus> get syncStatusStream => _syncStatusController.stream;

  /// Update sync status and notify listeners
  Future<void> _updateSyncStatus() async {
    final status = await getSyncStatus();
    _syncStatusController.add(status);
  }

  /// Convert SyncOperation to Map
  Map<String, dynamic> _operationToMap(SyncOperation operation) {
    return {
      'id': operation.id,
      'operationType': operation.operationType,
      'createdAt': operation.createdAt.toIso8601String(),
      'data': jsonEncode(operation.data),
      'retryCount': operation.retryCount,
      'isFailed': operation.isFailed,
      'errorMessage': operation.errorMessage,
    };
  }

  /// Convert Map to SyncOperation
  SyncOperation _mapToOperation(Map<String, dynamic> map) {
    return SyncOperation(
      id: map['id'] as String,
      operationType: map['operationType'] as String,
      createdAt: DateTime.parse(map['createdAt'] as String),
      data: jsonDecode(map['data'] as String) as Map<String, dynamic>,
      retryCount: map['retryCount'] as int? ?? 0,
      isFailed: map['isFailed'] as bool? ?? false,
      errorMessage: map['errorMessage'] as String?,
    );
  }

  /// Dispose resources
  void dispose() {
    _syncStatusController.close();
  }
}
