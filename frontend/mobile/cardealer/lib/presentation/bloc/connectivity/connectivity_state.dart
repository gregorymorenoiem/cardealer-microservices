import 'package:equatable/equatable.dart';
import '../../../domain/entities/connectivity.dart';

/// Base class for connectivity states
abstract class ConnectivityState extends Equatable {
  const ConnectivityState();

  @override
  List<Object?> get props => [];
}

/// Initial state
class ConnectivityInitial extends ConnectivityState {
  const ConnectivityInitial();
}

/// State when connectivity status is known
class ConnectivityStatusKnown extends ConnectivityState {
  final ConnectivityStatus status;
  final bool isOnline;
  final bool hasInternetAccess;
  final SyncStatus syncStatus;

  const ConnectivityStatusKnown({
    required this.status,
    required this.isOnline,
    required this.hasInternetAccess,
    required this.syncStatus,
  });

  /// Check if currently syncing
  bool get isSyncing => syncStatus.isSyncing;

  /// Check if there are pending operations
  bool get hasPendingOperations => syncStatus.pendingOperations > 0;

  /// Check if there are failed operations
  bool get hasFailedOperations => syncStatus.failedOperations > 0;

  /// Get status message
  String get statusMessage {
    if (!isOnline) return 'Sin conexión';
    if (!hasInternetAccess) return 'Sin acceso a internet';
    if (isSyncing) return 'Sincronizando...';
    if (hasPendingOperations) {
      return '${syncStatus.pendingOperations} operación(es) pendiente(s)';
    }
    if (hasFailedOperations) {
      return '${syncStatus.failedOperations} operación(es) fallida(s)';
    }
    return 'Conectado';
  }

  ConnectivityStatusKnown copyWith({
    ConnectivityStatus? status,
    bool? isOnline,
    bool? hasInternetAccess,
    SyncStatus? syncStatus,
  }) {
    return ConnectivityStatusKnown(
      status: status ?? this.status,
      isOnline: isOnline ?? this.isOnline,
      hasInternetAccess: hasInternetAccess ?? this.hasInternetAccess,
      syncStatus: syncStatus ?? this.syncStatus,
    );
  }

  @override
  List<Object?> get props => [status, isOnline, hasInternetAccess, syncStatus];
}

/// State when syncing
class ConnectivitySyncing extends ConnectivityState {
  final int pendingOperations;

  const ConnectivitySyncing(this.pendingOperations);

  @override
  List<Object?> get props => [pendingOperations];
}

/// State when sync completes successfully
class ConnectivitySyncSuccess extends ConnectivityState {
  const ConnectivitySyncSuccess();
}

/// State when sync fails
class ConnectivitySyncError extends ConnectivityState {
  final String message;

  const ConnectivitySyncError(this.message);

  @override
  List<Object?> get props => [message];
}
