import 'package:equatable/equatable.dart';

/// Enum representing network connectivity status
enum ConnectivityStatus {
  /// Device is connected to WiFi
  wifi,

  /// Device is connected to mobile data
  mobile,

  /// Device is offline (no connection)
  offline,
}

/// Entity representing the current connectivity state
class ConnectivityState extends Equatable {
  /// Current connectivity status
  final ConnectivityStatus status;

  /// Whether the device is currently online
  final bool isOnline;

  /// Last time connectivity was checked
  final DateTime lastChecked;

  const ConnectivityState({
    required this.status,
    required this.isOnline,
    required this.lastChecked,
  });

  /// Factory for offline state
  factory ConnectivityState.offline() {
    return ConnectivityState(
      status: ConnectivityStatus.offline,
      isOnline: false,
      lastChecked: DateTime.now(),
    );
  }

  /// Factory for online state (WiFi)
  factory ConnectivityState.wifi() {
    return ConnectivityState(
      status: ConnectivityStatus.wifi,
      isOnline: true,
      lastChecked: DateTime.now(),
    );
  }

  /// Factory for online state (Mobile)
  factory ConnectivityState.mobile() {
    return ConnectivityState(
      status: ConnectivityStatus.mobile,
      isOnline: true,
      lastChecked: DateTime.now(),
    );
  }

  /// Copy with method for immutability
  ConnectivityState copyWith({
    ConnectivityStatus? status,
    bool? isOnline,
    DateTime? lastChecked,
  }) {
    return ConnectivityState(
      status: status ?? this.status,
      isOnline: isOnline ?? this.isOnline,
      lastChecked: lastChecked ?? this.lastChecked,
    );
  }

  @override
  List<Object?> get props => [status, isOnline, lastChecked];

  @override
  String toString() {
    return 'ConnectivityState(status: $status, isOnline: $isOnline, lastChecked: $lastChecked)';
  }
}

/// Entity representing a sync operation waiting in queue
class SyncOperation extends Equatable {
  /// Unique identifier for this operation
  final String id;

  /// Type of operation (e.g., 'favorite', 'message', 'profile_update')
  final String operationType;

  /// Timestamp when operation was created
  final DateTime createdAt;

  /// JSON data for the operation
  final Map<String, dynamic> data;

  /// Number of retry attempts
  final int retryCount;

  /// Whether this operation failed
  final bool isFailed;

  /// Error message if failed
  final String? errorMessage;

  const SyncOperation({
    required this.id,
    required this.operationType,
    required this.createdAt,
    required this.data,
    this.retryCount = 0,
    this.isFailed = false,
    this.errorMessage,
  });

  /// Copy with method for immutability
  SyncOperation copyWith({
    String? id,
    String? operationType,
    DateTime? createdAt,
    Map<String, dynamic>? data,
    int? retryCount,
    bool? isFailed,
    String? errorMessage,
  }) {
    return SyncOperation(
      id: id ?? this.id,
      operationType: operationType ?? this.operationType,
      createdAt: createdAt ?? this.createdAt,
      data: data ?? this.data,
      retryCount: retryCount ?? this.retryCount,
      isFailed: isFailed ?? this.isFailed,
      errorMessage: errorMessage ?? this.errorMessage,
    );
  }

  @override
  List<Object?> get props => [
        id,
        operationType,
        createdAt,
        data,
        retryCount,
        isFailed,
        errorMessage,
      ];

  @override
  String toString() {
    return 'SyncOperation(id: $id, type: $operationType, retries: $retryCount, failed: $isFailed)';
  }
}

/// Entity representing sync status
class SyncStatus extends Equatable {
  /// Whether sync is currently in progress
  final bool isSyncing;

  /// Number of operations pending sync
  final int pendingOperations;

  /// Last successful sync timestamp
  final DateTime? lastSyncTime;

  /// Number of failed operations
  final int failedOperations;

  const SyncStatus({
    required this.isSyncing,
    required this.pendingOperations,
    this.lastSyncTime,
    this.failedOperations = 0,
  });

  /// Factory for idle state
  factory SyncStatus.idle() {
    return const SyncStatus(
      isSyncing: false,
      pendingOperations: 0,
      failedOperations: 0,
    );
  }

  /// Factory for syncing state
  factory SyncStatus.syncing(int pendingOps) {
    return SyncStatus(
      isSyncing: true,
      pendingOperations: pendingOps,
      failedOperations: 0,
    );
  }

  /// Copy with method for immutability
  SyncStatus copyWith({
    bool? isSyncing,
    int? pendingOperations,
    DateTime? lastSyncTime,
    int? failedOperations,
  }) {
    return SyncStatus(
      isSyncing: isSyncing ?? this.isSyncing,
      pendingOperations: pendingOperations ?? this.pendingOperations,
      lastSyncTime: lastSyncTime ?? this.lastSyncTime,
      failedOperations: failedOperations ?? this.failedOperations,
    );
  }

  @override
  List<Object?> get props => [
        isSyncing,
        pendingOperations,
        lastSyncTime,
        failedOperations,
      ];

  @override
  String toString() {
    return 'SyncStatus(isSyncing: $isSyncing, pending: $pendingOperations, failed: $failedOperations)';
  }
}
