import 'dart:async';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../domain/usecases/connectivity/check_connectivity.dart';
import '../../../domain/usecases/sync/sync_operations.dart' as usecases;
import '../../../domain/entities/connectivity.dart' as domain;
import 'connectivity_event.dart';
import 'connectivity_state.dart';

/// BLoC for managing connectivity and sync operations
class ConnectivityBloc extends Bloc<ConnectivityEvent, ConnectivityState> {
  final CheckConnectivity checkConnectivity;
  final WatchConnectivity watchConnectivity;
  final CheckInternetAccess checkInternetAccess;
  final usecases.GetSyncStatus getSyncStatus;
  final usecases.ProcessSyncQueue processSyncQueue;
  final usecases.WatchSyncStatus watchSyncStatus;
  final usecases.RetryFailedSync retryFailedSync;
  final usecases.QueueSyncOperation queueSyncOperation;

  StreamSubscription<domain.ConnectivityState>? _connectivitySubscription;
  StreamSubscription<domain.SyncStatus>? _syncStatusSubscription;

  ConnectivityBloc({
    required this.checkConnectivity,
    required this.watchConnectivity,
    required this.checkInternetAccess,
    required this.getSyncStatus,
    required this.processSyncQueue,
    required this.watchSyncStatus,
    required this.retryFailedSync,
    required this.queueSyncOperation,
  }) : super(const ConnectivityInitial()) {
    on<InitializeConnectivity>(_onInitializeConnectivity);
    on<ConnectivityChanged>(_onConnectivityChanged);
    on<CheckInternet>(_onCheckInternet);
    on<TriggerManualSync>(_onTriggerManualSync);
    on<RetryFailedSyncEvent>(_onRetryFailedSync);
    on<ClearFailedOperations>(_onClearFailedOperations);
  }

  /// Initialize connectivity monitoring
  Future<void> _onInitializeConnectivity(
    InitializeConnectivity event,
    Emitter<ConnectivityState> emit,
  ) async {
    // Get current connectivity
    final connectivityResult = await checkConnectivity();
    final internetResult = await checkInternetAccess();
    final syncStatusResult = await getSyncStatus();

    connectivityResult.fold(
      (failure) => emit(ConnectivitySyncError(failure.message)),
      (connectivityState) {
        internetResult.fold(
          (failure) => emit(ConnectivitySyncError(failure.message)),
          (hasInternet) {
            syncStatusResult.fold(
              (failure) => emit(ConnectivitySyncError(failure.message)),
              (syncStatus) {
                emit(ConnectivityStatusKnown(
                  status: connectivityState.status,
                  isOnline: connectivityState.isOnline,
                  hasInternetAccess: hasInternet,
                  syncStatus: syncStatus,
                ));

                // If online, try to sync
                if (connectivityState.isOnline && hasInternet) {
                  add(const TriggerManualSync());
                }
              },
            );
          },
        );
      },
    );

    // Listen to connectivity changes
    await _connectivitySubscription?.cancel();
    _connectivitySubscription = watchConnectivity().listen((connectivityState) {
      add(ConnectivityChanged(connectivityState));
    });

    // Listen to sync status changes
    await _syncStatusSubscription?.cancel();
    _syncStatusSubscription = watchSyncStatus().listen((syncStatus) {
      if (state is ConnectivityStatusKnown) {
        final currentState = state as ConnectivityStatusKnown;
        emit(currentState.copyWith(syncStatus: syncStatus));
      }
    });
  }

  /// Handle connectivity changes
  Future<void> _onConnectivityChanged(
    ConnectivityChanged event,
    Emitter<ConnectivityState> emit,
  ) async {
    if (state is! ConnectivityStatusKnown) return;

    final currentState = state as ConnectivityStatusKnown;
    final wasOffline = !currentState.isOnline;
    final isNowOnline = event.connectivityState.isOnline;

    // Check internet access
    final internetResult = await checkInternetAccess();
    final hasInternet = internetResult.fold((_) => false, (result) => result);

    emit(currentState.copyWith(
      status: event.connectivityState.status,
      isOnline: isNowOnline,
      hasInternetAccess: hasInternet,
    ));

    // If went from offline to online, trigger sync
    if (wasOffline && isNowOnline && hasInternet) {
      add(const TriggerManualSync());
    }
  }

  /// Check internet access
  Future<void> _onCheckInternet(
    CheckInternet event,
    Emitter<ConnectivityState> emit,
  ) async {
    if (state is! ConnectivityStatusKnown) return;

    final currentState = state as ConnectivityStatusKnown;
    final internetResult = await checkInternetAccess();

    internetResult.fold(
      (failure) => emit(ConnectivitySyncError(failure.message)),
      (hasInternet) {
        emit(currentState.copyWith(hasInternetAccess: hasInternet));
      },
    );
  }

  /// Trigger manual sync
  Future<void> _onTriggerManualSync(
    TriggerManualSync event,
    Emitter<ConnectivityState> emit,
  ) async {
    if (state is! ConnectivityStatusKnown) return;

    final currentState = state as ConnectivityStatusKnown;

    if (!currentState.isOnline || !currentState.hasInternetAccess) {
      emit(const ConnectivitySyncError('No hay conexión a internet'));
      emit(currentState); // Return to previous state
      return;
    }

    // Get current pending operations
    final syncStatusResult = await getSyncStatus();
    final pendingCount =
        syncStatusResult.fold((_) => 0, (status) => status.pendingOperations);

    if (pendingCount == 0) return;

    emit(ConnectivitySyncing(pendingCount));

    // Process sync queue
    final result = await processSyncQueue();

    result.fold(
      (failure) {
        emit(ConnectivitySyncError(failure.message));
        emit(currentState); // Return to previous state
      },
      (_) {
        emit(const ConnectivitySyncSuccess());

        // Update sync status
        getSyncStatus().then((statusResult) {
          statusResult.fold(
            (_) {},
            (syncStatus) {
              emit(currentState.copyWith(syncStatus: syncStatus));
            },
          );
        });
      },
    );
  }

  /// Retry failed sync operations
  Future<void> _onRetryFailedSync(
    RetryFailedSyncEvent event,
    Emitter<ConnectivityState> emit,
  ) async {
    if (state is! ConnectivityStatusKnown) return;

    final currentState = state as ConnectivityStatusKnown;

    if (!currentState.isOnline || !currentState.hasInternetAccess) {
      emit(const ConnectivitySyncError('No hay conexión a internet'));
      emit(currentState);
      return;
    }

    emit(ConnectivitySyncing(currentState.syncStatus.failedOperations));

    final result = await retryFailedSync();

    result.fold(
      (failure) {
        emit(ConnectivitySyncError(failure.message));
        emit(currentState);
      },
      (_) {
        emit(const ConnectivitySyncSuccess());

        // Update sync status
        getSyncStatus().then((statusResult) {
          statusResult.fold(
            (_) {},
            (syncStatus) {
              emit(currentState.copyWith(syncStatus: syncStatus));
            },
          );
        });
      },
    );
  }

  /// Clear failed operations
  Future<void> _onClearFailedOperations(
    ClearFailedOperations event,
    Emitter<ConnectivityState> emit,
  ) async {
    // TODO: Implement clear failed operations use case
    if (state is ConnectivityStatusKnown) {
      final currentState = state as ConnectivityStatusKnown;

      // Update sync status
      final syncStatusResult = await getSyncStatus();
      syncStatusResult.fold(
        (_) {},
        (syncStatus) {
          emit(currentState.copyWith(syncStatus: syncStatus));
        },
      );
    }
  }

  @override
  Future<void> close() {
    _connectivitySubscription?.cancel();
    _syncStatusSubscription?.cancel();
    return super.close();
  }
}
