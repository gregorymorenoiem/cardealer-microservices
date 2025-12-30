library;

/// Offline mode management
/// Handles network status, caching strategy, and sync operations
import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:flutter/material.dart';
import 'package:rxdart/rxdart.dart';

/// Network status manager
class NetworkStatusManager {
  static final NetworkStatusManager _instance =
      NetworkStatusManager._internal();
  factory NetworkStatusManager() => _instance;
  NetworkStatusManager._internal();

  final _connectivityController = BehaviorSubject<List<ConnectivityResult>>.seeded(
    [ConnectivityResult.none],
  );

  Stream<List<ConnectivityResult>> get connectivityStream =>
      _connectivityController.stream;
  List<ConnectivityResult> get currentStatus => _connectivityController.value;

  bool get isOnline =>
      currentStatus.contains(ConnectivityResult.wifi) ||
      currentStatus.contains(ConnectivityResult.mobile) ||
      currentStatus.contains(ConnectivityResult.ethernet);

  void initialize() {
    Connectivity().onConnectivityChanged.listen((result) {
      _connectivityController.add(result);
    });

    // Check initial status
    _checkInitialConnectivity();
  }

  Future<void> _checkInitialConnectivity() async {
    final result = await Connectivity().checkConnectivity();
    _connectivityController.add(result);
  }

  void dispose() {
    _connectivityController.close();
  }
}

/// Offline indicator widget
class OfflineIndicator extends StatelessWidget {
  final Widget child;
  final Widget? offlineWidget;
  final Color? backgroundColor;
  final Duration animationDuration;

  const OfflineIndicator({
    super.key,
    required this.child,
    this.offlineWidget,
    this.backgroundColor,
    this.animationDuration = const Duration(milliseconds: 300),
  });

  @override
  Widget build(BuildContext context) {
    return StreamBuilder<List<ConnectivityResult>>(
      stream: NetworkStatusManager().connectivityStream,
      builder: (context, snapshot) {
        final isOnline = NetworkStatusManager().isOnline;

        return Stack(
          children: [
            child,
            if (!isOnline)
              Positioned(
                top: 0,
                left: 0,
                right: 0,
                child: AnimatedContainer(
                  duration: animationDuration,
                  color: backgroundColor ?? Colors.red.shade700,
                  padding: const EdgeInsets.symmetric(
                    horizontal: 16,
                    vertical: 8,
                  ),
                  child: offlineWidget ??
                      const Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(
                            Icons.cloud_off,
                            color: Colors.white,
                            size: 16,
                          ),
                          SizedBox(width: 8),
                          Text(
                            'Sin conexión a internet',
                            style: TextStyle(
                              color: Colors.white,
                              fontSize: 14,
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                        ],
                      ),
                ),
              ),
          ],
        );
      },
    );
  }
}

/// Offline banner widget
class OfflineBanner extends StatelessWidget {
  final String? message;
  final IconData? icon;
  final VoidCallback? onRetry;

  const OfflineBanner({
    super.key,
    this.message,
    this.icon,
    this.onRetry,
  });

  @override
  Widget build(BuildContext context) {
    return StreamBuilder<List<ConnectivityResult>>(
      stream: NetworkStatusManager().connectivityStream,
      builder: (context, snapshot) {
        final isOnline = NetworkStatusManager().isOnline;

        if (isOnline) return const SizedBox.shrink();

        return MaterialBanner(
          backgroundColor: Colors.orange.shade100,
          leading: Icon(
            icon ?? Icons.wifi_off,
            color: Colors.orange.shade900,
          ),
          content: Text(
            message ?? 'Estás trabajando sin conexión',
            style: TextStyle(
              color: Colors.orange.shade900,
              fontWeight: FontWeight.w500,
            ),
          ),
          actions: [
            if (onRetry != null)
              TextButton(
                onPressed: onRetry,
                child: const Text('REINTENTAR'),
              ),
          ],
        );
      },
    );
  }
}

/// Sync status enum
enum SyncStatus {
  idle,
  syncing,
  success,
  failed,
}

/// Offline sync manager
class OfflineSyncManager {
  static final OfflineSyncManager _instance = OfflineSyncManager._internal();
  factory OfflineSyncManager() => _instance;
  OfflineSyncManager._internal();

  final _syncStatusController =
      BehaviorSubject<SyncStatus>.seeded(SyncStatus.idle);
  final _pendingOperations = <OfflineOperation>[];

  Stream<SyncStatus> get syncStatusStream => _syncStatusController.stream;
  SyncStatus get currentStatus => _syncStatusController.value;
  List<OfflineOperation> get pendingOperations =>
      List.unmodifiable(_pendingOperations);
  int get pendingCount => _pendingOperations.length;

  /// Add operation to queue
  void addOperation(OfflineOperation operation) {
    _pendingOperations.add(operation);
  }

  /// Remove operation from queue
  void removeOperation(String id) {
    _pendingOperations.removeWhere((op) => op.id == id);
  }

  /// Clear all pending operations
  void clearOperations() {
    _pendingOperations.clear();
  }

  /// Start sync process
  Future<void> sync() async {
    if (_pendingOperations.isEmpty) return;

    _syncStatusController.add(SyncStatus.syncing);

    try {
      for (final operation in _pendingOperations.toList()) {
        await operation.execute();
        _pendingOperations.remove(operation);
      }

      _syncStatusController.add(SyncStatus.success);

      // Reset to idle after delay
      await Future.delayed(const Duration(seconds: 2));
      _syncStatusController.add(SyncStatus.idle);
    } catch (e) {
      _syncStatusController.add(SyncStatus.failed);

      // Reset to idle after delay
      await Future.delayed(const Duration(seconds: 2));
      _syncStatusController.add(SyncStatus.idle);
    }
  }

  /// Auto sync when connection is restored
  void enableAutoSync() {
    NetworkStatusManager().connectivityStream.listen((result) {
      if (NetworkStatusManager().isOnline && _pendingOperations.isNotEmpty) {
        sync();
      }
    });
  }

  void dispose() {
    _syncStatusController.close();
  }
}

/// Offline operation model
class OfflineOperation {
  final String id;
  final String type;
  final Map<String, dynamic> data;
  final DateTime timestamp;
  final Future<void> Function() execute;

  OfflineOperation({
    required this.id,
    required this.type,
    required this.data,
    required this.execute,
    DateTime? timestamp,
  }) : timestamp = timestamp ?? DateTime.now();

  Map<String, dynamic> toJson() => {
        'id': id,
        'type': type,
        'data': data,
        'timestamp': timestamp.toIso8601String(),
      };

  factory OfflineOperation.fromJson(
    Map<String, dynamic> json,
    Future<void> Function() execute,
  ) =>
      OfflineOperation(
        id: json['id'] as String,
        type: json['type'] as String,
        data: json['data'] as Map<String, dynamic>,
        timestamp: DateTime.parse(json['timestamp'] as String),
        execute: execute,
      );
}

/// Sync indicator widget
class SyncIndicator extends StatelessWidget {
  final bool showWhenIdle;

  const SyncIndicator({
    super.key,
    this.showWhenIdle = false,
  });

  @override
  Widget build(BuildContext context) {
    return StreamBuilder<SyncStatus>(
      stream: OfflineSyncManager().syncStatusStream,
      builder: (context, snapshot) {
        final status = snapshot.data ?? SyncStatus.idle;

        if (status == SyncStatus.idle && !showWhenIdle) {
          return const SizedBox.shrink();
        }

        return AnimatedContainer(
          duration: const Duration(milliseconds: 300),
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
          decoration: BoxDecoration(
            color: _getBackgroundColor(status),
            borderRadius: BorderRadius.circular(16),
          ),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              if (status == SyncStatus.syncing)
                const SizedBox(
                  width: 12,
                  height: 12,
                  child: CircularProgressIndicator(
                    strokeWidth: 2,
                    valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
                  ),
                )
              else
                Icon(
                  _getIcon(status),
                  size: 16,
                  color: Colors.white,
                ),
              const SizedBox(width: 6),
              Text(
                _getMessage(status),
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 12,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Color _getBackgroundColor(SyncStatus status) {
    switch (status) {
      case SyncStatus.idle:
        return Colors.grey.shade600;
      case SyncStatus.syncing:
        return Colors.blue.shade600;
      case SyncStatus.success:
        return Colors.green.shade600;
      case SyncStatus.failed:
        return Colors.red.shade600;
    }
  }

  IconData _getIcon(SyncStatus status) {
    switch (status) {
      case SyncStatus.idle:
        return Icons.cloud_done;
      case SyncStatus.syncing:
        return Icons.sync;
      case SyncStatus.success:
        return Icons.check_circle;
      case SyncStatus.failed:
        return Icons.error;
    }
  }

  String _getMessage(SyncStatus status) {
    switch (status) {
      case SyncStatus.idle:
        return 'Sincronizado';
      case SyncStatus.syncing:
        return 'Sincronizando...';
      case SyncStatus.success:
        return 'Sincronizado';
      case SyncStatus.failed:
        return 'Error al sincronizar';
    }
  }
}

/// Pending operations widget
class PendingOperationsIndicator extends StatelessWidget {
  final VoidCallback? onTap;

  const PendingOperationsIndicator({
    super.key,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return StreamBuilder<SyncStatus>(
      stream: OfflineSyncManager().syncStatusStream,
      builder: (context, snapshot) {
        final pendingCount = OfflineSyncManager().pendingCount;

        if (pendingCount == 0) return const SizedBox.shrink();

        return Material(
          color: Colors.transparent,
          child: InkWell(
            onTap: onTap,
            borderRadius: BorderRadius.circular(20),
            child: Container(
              padding: const EdgeInsets.symmetric(
                horizontal: 12,
                vertical: 6,
              ),
              decoration: BoxDecoration(
                color: Colors.orange.shade100,
                borderRadius: BorderRadius.circular(20),
                border: Border.all(
                  color: Colors.orange.shade300,
                  width: 1,
                ),
              ),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(
                    Icons.cloud_upload,
                    size: 16,
                    color: Colors.orange.shade900,
                  ),
                  const SizedBox(width: 6),
                  Text(
                    '$pendingCount pendiente${pendingCount > 1 ? 's' : ''}',
                    style: TextStyle(
                      color: Colors.orange.shade900,
                      fontSize: 12,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ),
          ),
        );
      },
    );
  }
}
