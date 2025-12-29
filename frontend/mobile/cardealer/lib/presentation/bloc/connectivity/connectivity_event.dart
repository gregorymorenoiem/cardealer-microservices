import 'package:equatable/equatable.dart';
import '../../../domain/entities/connectivity.dart';

/// Base class for connectivity events
abstract class ConnectivityEvent extends Equatable {
  const ConnectivityEvent();

  @override
  List<Object?> get props => [];
}

/// Event to initialize connectivity monitoring
class InitializeConnectivity extends ConnectivityEvent {
  const InitializeConnectivity();
}

/// Event when connectivity changes
class ConnectivityChanged extends ConnectivityEvent {
  final ConnectivityState connectivityState;

  const ConnectivityChanged(this.connectivityState);

  @override
  List<Object?> get props => [connectivityState];
}

/// Event to check internet access
class CheckInternet extends ConnectivityEvent {
  const CheckInternet();
}

/// Event to manually trigger sync
class TriggerManualSync extends ConnectivityEvent {
  const TriggerManualSync();
}

/// Event to retry failed sync operations
class RetryFailedSyncEvent extends ConnectivityEvent {
  const RetryFailedSyncEvent();
}

/// Event to clear failed operations
class ClearFailedOperations extends ConnectivityEvent {
  const ClearFailedOperations();
}
