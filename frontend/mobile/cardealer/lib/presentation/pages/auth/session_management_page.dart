import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';
import '../../../core/services/session_manager.dart';
import '../../widgets/buttons/gradient_button.dart';

/// Session Management UI - Sprint 7 AE-009
/// Shows active sessions and allows device management

class ActiveDevice {
  final String deviceId;
  final String deviceName;
  final String deviceType; // mobile, tablet, desktop, web
  final String location;
  final DateTime lastActive;
  final bool isCurrent;

  ActiveDevice({
    required this.deviceId,
    required this.deviceName,
    required this.deviceType,
    required this.location,
    required this.lastActive,
    required this.isCurrent,
  });
}

class SessionManagementPage extends StatefulWidget {
  const SessionManagementPage({super.key});

  @override
  State<SessionManagementPage> createState() => _SessionManagementPageState();
}

class _SessionManagementPageState extends State<SessionManagementPage> {
  final _sessionManager = SessionManager();
  bool _rememberMe = false;

  // Mock data - replace with real API call
  final List<ActiveDevice> _mockDevices = [
    ActiveDevice(
      deviceId: '1',
      deviceName: 'iPhone 13 Pro',
      deviceType: 'mobile',
      location: 'Santo Domingo, República Dominicana',
      lastActive: DateTime.now(),
      isCurrent: true,
    ),
    ActiveDevice(
      deviceId: '2',
      deviceName: 'MacBook Pro',
      deviceType: 'desktop',
      location: 'Santo Domingo, República Dominicana',
      lastActive: DateTime.now().subtract(const Duration(hours: 3)),
      isCurrent: false,
    ),
    ActiveDevice(
      deviceId: '3',
      deviceName: 'Chrome - Windows',
      deviceType: 'web',
      location: 'Santiago, República Dominicana',
      lastActive: DateTime.now().subtract(const Duration(days: 2)),
      isCurrent: false,
    ),
  ];

  @override
  void initState() {
    super.initState();
    _rememberMe = _sessionManager.rememberMe;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Gestión de Sesiones'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
      ),
      body: SafeArea(
        child: ListView(
          padding: const EdgeInsets.all(AppSpacing.lg),
          children: [
            // Session Info
            _buildSessionInfoCard(),
            const SizedBox(height: AppSpacing.lg),

            // Remember Me
            _buildRememberMeCard(),
            const SizedBox(height: AppSpacing.lg),

            // Active Devices
            Text(
              'Dispositivos Activos',
              style: AppTypography.h3.copyWith(
                color: AppColors.textPrimary,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: AppSpacing.sm),
            Text(
              'Administra dónde has iniciado sesión',
              style: AppTypography.bodyMedium.copyWith(
                color: AppColors.textSecondary,
              ),
            ),
            const SizedBox(height: AppSpacing.md),
            ..._mockDevices.map((device) => _buildDeviceCard(device)),

            const SizedBox(height: AppSpacing.lg),

            // Logout from all devices
            GradientButton(
              onPressed: _handleLogoutAllDevices,
              text: 'Cerrar sesión en todos los dispositivos',
              variant: GradientButtonVariant.outline,
              icon: const Icon(Icons.logout),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSessionInfoCard() {
    final timeUntilExpiry = _sessionManager.getTimeUntilExpiry();

    return Container(
      padding: const EdgeInsets.all(AppSpacing.lg),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [AppColors.primary, AppColors.accent],
        ),
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: AppColors.primary.withValues(alpha: 0.3),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Icon(Icons.timer_outlined, color: Colors.white, size: 28),
              const SizedBox(width: AppSpacing.sm),
              Text(
                'Sesión Activa',
                style: AppTypography.h4.copyWith(
                  color: Colors.white,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.md),
          if (timeUntilExpiry != null) ...[
            Text(
              'Tiempo restante: ${_formatDuration(timeUntilExpiry)}',
              style: AppTypography.bodyLarge.copyWith(
                color: Colors.white.withValues(alpha: 0.9),
              ),
            ),
            const SizedBox(height: AppSpacing.xs),
            LinearProgressIndicator(
              value: timeUntilExpiry.inMinutes / 30.0,
              backgroundColor: Colors.white.withValues(alpha: 0.3),
              valueColor: const AlwaysStoppedAnimation<Color>(Colors.white),
            ),
          ] else
            Text(
              'No hay sesión activa',
              style: AppTypography.bodyLarge.copyWith(
                color: Colors.white.withValues(alpha: 0.9),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildRememberMeCard() {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.lg),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.shade200,
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Icon(
                Icons.save_outlined,
                color: AppColors.primary,
                size: 24,
              ),
              const SizedBox(width: AppSpacing.sm),
              Text(
                'Recordar sesión',
                style: AppTypography.labelLarge.copyWith(
                  color: AppColors.textPrimary,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.sm),
          Text(
            'Mantén tu sesión activa por 30 días sin necesidad de iniciar sesión nuevamente',
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textSecondary,
            ),
          ),
          const SizedBox(height: AppSpacing.md),
          SwitchListTile(
            value: _rememberMe,
            onChanged: (value) {
              setState(() {
                _rememberMe = value;
              });
              // TODO: Update session manager
              // _sessionManager.updateRememberMe(value);
            },
            title: Text(
              _rememberMe ? 'Activado' : 'Desactivado',
              style: AppTypography.bodyMedium.copyWith(
                color: AppColors.textPrimary,
                fontWeight: FontWeight.w600,
              ),
            ),
            activeThumbColor: AppColors.primary,
            contentPadding: EdgeInsets.zero,
          ),
        ],
      ),
    );
  }

  Widget _buildDeviceCard(ActiveDevice device) {
    return Container(
      margin: const EdgeInsets.only(bottom: AppSpacing.md),
      padding: const EdgeInsets.all(AppSpacing.lg),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: device.isCurrent
            ? Border.all(color: AppColors.primary, width: 2)
            : null,
        boxShadow: [
          BoxShadow(
            color: Colors.grey.shade200,
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(AppSpacing.sm),
                decoration: BoxDecoration(
                  color: device.isCurrent
                      ? AppColors.primary.withValues(alpha: 0.1)
                      : Colors.grey.shade100,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Icon(
                  _getDeviceIcon(device.deviceType),
                  color: device.isCurrent
                      ? AppColors.primary
                      : Colors.grey.shade600,
                  size: 24,
                ),
              ),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Text(
                          device.deviceName,
                          style: AppTypography.labelLarge.copyWith(
                            color: AppColors.textPrimary,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        if (device.isCurrent) ...[
                          const SizedBox(width: AppSpacing.xs),
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: AppSpacing.xs,
                              vertical: 2,
                            ),
                            decoration: BoxDecoration(
                              color: AppColors.primary,
                              borderRadius: BorderRadius.circular(4),
                            ),
                            child: Text(
                              'Actual',
                              style: AppTypography.bodySmall.copyWith(
                                color: Colors.white,
                                fontSize: 10,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ),
                        ],
                      ],
                    ),
                    const SizedBox(height: AppSpacing.xs),
                    Row(
                      children: [
                        const Icon(
                          Icons.location_on_outlined,
                          size: 14,
                          color: AppColors.textSecondary,
                        ),
                        const SizedBox(width: 4),
                        Expanded(
                          child: Text(
                            device.location,
                            style: AppTypography.bodySmall.copyWith(
                              color: AppColors.textSecondary,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.sm),
          Divider(color: Colors.grey.shade300),
          const SizedBox(height: AppSpacing.sm),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Última actividad: ${_formatLastActive(device.lastActive)}',
                style: AppTypography.bodySmall.copyWith(
                  color: AppColors.textSecondary,
                ),
              ),
              if (!device.isCurrent)
                TextButton(
                  onPressed: () => _handleLogoutDevice(device),
                  child: Text(
                    'Cerrar sesión',
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.error,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
            ],
          ),
        ],
      ),
    );
  }

  IconData _getDeviceIcon(String deviceType) {
    switch (deviceType) {
      case 'mobile':
        return Icons.phone_iphone;
      case 'tablet':
        return Icons.tablet_mac;
      case 'desktop':
        return Icons.computer;
      case 'web':
        return Icons.language;
      default:
        return Icons.devices;
    }
  }

  String _formatDuration(Duration duration) {
    final minutes = duration.inMinutes;
    if (minutes < 60) {
      return '$minutes minutos';
    } else {
      final hours = duration.inHours;
      final remainingMinutes = minutes % 60;
      return '$hours h $remainingMinutes min';
    }
  }

  String _formatLastActive(DateTime dateTime) {
    final now = DateTime.now();
    final difference = now.difference(dateTime);

    if (difference.inMinutes < 1) {
      return 'Ahora';
    } else if (difference.inHours < 1) {
      return 'Hace ${difference.inMinutes} minutos';
    } else if (difference.inDays < 1) {
      return 'Hace ${difference.inHours} horas';
    } else {
      return 'Hace ${difference.inDays} días';
    }
  }

  void _handleLogoutDevice(ActiveDevice device) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Cerrar sesión'),
        content: Text(
          '¿Estás seguro de que quieres cerrar sesión en ${device.deviceName}?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          TextButton(
            onPressed: () {
              _sessionManager.logoutFromDevice(device.deviceId);
              Navigator.pop(context);
              setState(() {
                _mockDevices.removeWhere((d) => d.deviceId == device.deviceId);
              });
            },
            child: const Text('Cerrar sesión'),
          ),
        ],
      ),
    );
  }

  void _handleLogoutAllDevices() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Cerrar todas las sesiones'),
        content: const Text(
          '¿Estás seguro de que quieres cerrar sesión en todos los dispositivos? Tendrás que iniciar sesión nuevamente.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          TextButton(
            onPressed: () {
              _sessionManager.logoutFromAllDevices();
              Navigator.pop(context);
              Navigator.pop(context); // Return to previous screen
            },
            child: const Text('Cerrar todas'),
          ),
        ],
      ),
    );
  }
}
