import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../bloc/connectivity/connectivity_bloc.dart';
import '../bloc/connectivity/connectivity_event.dart';
import '../bloc/connectivity/connectivity_state.dart';
import '../../../core/theme/colors.dart';

/// Widget that shows sync status and allows manual sync
class SyncStatusWidget extends StatelessWidget {
  const SyncStatusWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<ConnectivityBloc, ConnectivityState>(
      builder: (context, state) {
        if (state is! ConnectivityStatusKnown) {
          return const SizedBox.shrink();
        }

        return Card(
          margin: const EdgeInsets.all(16),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Header
                Row(
                  children: [
                    Icon(
                      _getStatusIcon(state),
                      color: _getStatusColor(state),
                      size: 24,
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'Estado de Sincronización',
                            style: Theme.of(context)
                                .textTheme
                                .titleMedium
                                ?.copyWith(
                                  fontWeight: FontWeight.w600,
                                ),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            state.statusMessage,
                            style:
                                Theme.of(context).textTheme.bodySmall?.copyWith(
                                      color: AppColors.textSecondary,
                                    ),
                          ),
                        ],
                      ),
                    ),
                    if (state.isSyncing)
                      const SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      ),
                  ],
                ),

                // Pending operations info
                if (state.hasPendingOperations) ...[
                  const SizedBox(height: 16),
                  const Divider(),
                  const SizedBox(height: 16),
                  Row(
                    children: [
                      const Icon(
                        Icons.schedule,
                        size: 20,
                        color: AppColors.warning,
                      ),
                      const SizedBox(width: 8),
                      Text(
                        '${state.syncStatus.pendingOperations} operación(es) pendiente(s)',
                        style: Theme.of(context).textTheme.bodyMedium,
                      ),
                    ],
                  ),
                  if (state.isOnline && state.hasInternetAccess) ...[
                    const SizedBox(height: 12),
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton.icon(
                        onPressed: state.isSyncing
                            ? null
                            : () {
                                context.read<ConnectivityBloc>().add(
                                      const TriggerManualSync(),
                                    );
                              },
                        icon: const Icon(Icons.sync, size: 20),
                        label: const Text('Sincronizar Ahora'),
                      ),
                    ),
                  ],
                ],

                // Failed operations info
                if (state.hasFailedOperations) ...[
                  const SizedBox(height: 16),
                  const Divider(),
                  const SizedBox(height: 16),
                  Row(
                    children: [
                      const Icon(
                        Icons.error_outline,
                        size: 20,
                        color: AppColors.error,
                      ),
                      const SizedBox(width: 8),
                      Text(
                        '${state.syncStatus.failedOperations} operación(es) fallida(s)',
                        style: Theme.of(context).textTheme.bodyMedium,
                      ),
                    ],
                  ),
                  if (state.isOnline && state.hasInternetAccess) ...[
                    const SizedBox(height: 12),
                    Row(
                      children: [
                        Expanded(
                          child: OutlinedButton.icon(
                            onPressed: state.isSyncing
                                ? null
                                : () {
                                    context.read<ConnectivityBloc>().add(
                                          const RetryFailedSyncEvent(),
                                        );
                                  },
                            icon: const Icon(Icons.refresh, size: 20),
                            label: const Text('Reintentar'),
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: OutlinedButton.icon(
                            onPressed: state.isSyncing
                                ? null
                                : () {
                                    context.read<ConnectivityBloc>().add(
                                          const ClearFailedOperations(),
                                        );
                                  },
                            icon: const Icon(Icons.clear, size: 20),
                            label: const Text('Limpiar'),
                            style: OutlinedButton.styleFrom(
                              foregroundColor: AppColors.error,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ],
                ],

                // Last sync time
                if (state.syncStatus.lastSyncTime != null) ...[
                  const SizedBox(height: 16),
                  Text(
                    'Última sincronización: ${_formatLastSyncTime(state.syncStatus.lastSyncTime!)}',
                    style: Theme.of(context).textTheme.bodySmall?.copyWith(
                          color: AppColors.textTertiary,
                        ),
                  ),
                ],
              ],
            ),
          ),
        );
      },
    );
  }

  IconData _getStatusIcon(ConnectivityStatusKnown state) {
    if (!state.isOnline) return Icons.cloud_off;
    if (!state.hasInternetAccess) return Icons.signal_wifi_off;
    if (state.isSyncing) return Icons.sync;
    if (state.hasFailedOperations) return Icons.error_outline;
    if (state.hasPendingOperations) return Icons.schedule;
    return Icons.cloud_done;
  }

  Color _getStatusColor(ConnectivityStatusKnown state) {
    if (!state.isOnline || !state.hasInternetAccess) return AppColors.error;
    if (state.hasFailedOperations) return AppColors.error;
    if (state.hasPendingOperations) return AppColors.warning;
    if (state.isSyncing) return AppColors.primary;
    return AppColors.success;
  }

  String _formatLastSyncTime(DateTime dateTime) {
    final now = DateTime.now();
    final difference = now.difference(dateTime);

    if (difference.inMinutes < 1) return 'Hace un momento';
    if (difference.inMinutes < 60) return 'Hace ${difference.inMinutes} min';
    if (difference.inHours < 24) return 'Hace ${difference.inHours} h';
    return 'Hace ${difference.inDays} día(s)';
  }
}
