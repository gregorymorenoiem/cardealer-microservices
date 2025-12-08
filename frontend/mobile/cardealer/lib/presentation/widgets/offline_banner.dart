import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../bloc/connectivity/connectivity_bloc.dart';
import '../bloc/connectivity/connectivity_state.dart';
import '../../../core/theme/colors.dart';

/// Widget that displays offline banner when device is offline
class OfflineBanner extends StatelessWidget {
  const OfflineBanner({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<ConnectivityBloc, ConnectivityState>(
      builder: (context, state) {
        if (state is! ConnectivityStatusKnown) {
          return const SizedBox.shrink();
        }

        // Don't show banner if online
        if (state.isOnline && state.hasInternetAccess) {
          return const SizedBox.shrink();
        }

        return Material(
          elevation: 4,
          color: AppColors.warning,
          child: Container(
            width: double.infinity,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            child: Row(
              children: [
                const Icon(
                  Icons.cloud_off,
                  color: Colors.white,
                  size: 20,
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    state.isOnline
                        ? 'Sin acceso a internet'
                        : 'Sin conexión - Algunas funciones limitadas',
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 14,
                      fontWeight: FontWeight.w500,
                    ),
                  ),
                ),
                if (state.hasPendingOperations) ...[
                  const SizedBox(width: 8),
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 8,
                      vertical: 4,
                    ),
                    decoration: BoxDecoration(
                      color: Colors.white.withValues(alpha: 0.2),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Text(
                      '${state.syncStatus.pendingOperations} pendiente(s)',
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 12,
                        fontWeight: FontWeight.w600,
                      ),
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
}
