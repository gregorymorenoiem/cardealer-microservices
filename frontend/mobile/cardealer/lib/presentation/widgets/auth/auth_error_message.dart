import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';

/// Auth Error States Widget - Sprint 7 AE-010
/// Mensajes de error claros con opciones de recuperación
/// Features:
/// - Mensajes contextuales y claros
/// - Recovery options (retry, help, support)
/// - Help links relevantes
/// - Visual feedback mejorado

enum AuthErrorType {
  invalidCredentials,
  accountNotFound,
  emailNotVerified,
  accountLocked,
  networkError,
  serverError,
  sessionExpired,
  invalidCode,
  tooManyAttempts,
  unknown,
}

class AuthErrorMessage extends StatelessWidget {
  final AuthErrorType errorType;
  final String? customMessage;
  final VoidCallback? onRetry;
  final VoidCallback? onHelp;
  final VoidCallback? onContactSupport;
  final bool showIcon;

  const AuthErrorMessage({
    super.key,
    required this.errorType,
    this.customMessage,
    this.onRetry,
    this.onHelp,
    this.onContactSupport,
    this.showIcon = true,
  });

  @override
  Widget build(BuildContext context) {
    final config = AuthErrorMessage._getErrorConfig(errorType);

    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: config.backgroundColor,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: config.borderColor),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisSize: MainAxisSize.min,
        children: [
          // Header con icono
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              if (showIcon) ...[
                Icon(
                  config.icon,
                  color: config.iconColor,
                  size: 24,
                ),
                const SizedBox(width: AppSpacing.sm),
              ],
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      config.title,
                      style: AppTypography.labelLarge.copyWith(
                        color: config.textColor,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    const SizedBox(height: AppSpacing.xs),
                    Text(
                      customMessage ?? config.message,
                      style: AppTypography.bodyMedium.copyWith(
                        color: config.textColor,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          // Recovery options
          if (config.showRecoveryOptions) ...[
            const SizedBox(height: AppSpacing.md),
            AuthErrorMessage._buildRecoveryOptions(
              config,
              onRetry: onRetry,
              onHelp: onHelp,
              onContactSupport: onContactSupport,
            ),
          ],
        ],
      ),
    );
  }

  static Widget _buildRecoveryOptions(
    _AuthErrorConfig config, {
    VoidCallback? onRetry,
    VoidCallback? onHelp,
    VoidCallback? onContactSupport,
  }) {
    return Wrap(
      spacing: AppSpacing.sm,
      runSpacing: AppSpacing.xs,
      children: [
        if (onRetry != null && config.showRetry)
          _RecoveryButton(
            icon: Icons.refresh,
            label: 'Reintentar',
            onPressed: onRetry,
            color: config.iconColor,
          ),
        if (onHelp != null || config.helpText != null)
          _RecoveryButton(
            icon: Icons.help_outline,
            label: config.helpText ?? 'Ayuda',
            onPressed: onHelp ?? () {},
            color: AppColors.info,
          ),
        if (onContactSupport != null)
          _RecoveryButton(
            icon: Icons.support_agent,
            label: 'Contactar soporte',
            onPressed: onContactSupport,
            color: AppColors.primary,
          ),
      ],
    );
  }

  static _AuthErrorConfig _getErrorConfig(AuthErrorType type) {
    switch (type) {
      case AuthErrorType.invalidCredentials:
        return _AuthErrorConfig(
          title: 'Credenciales inválidas',
          message:
              'El email o contraseña son incorrectos. Por favor verifica e intenta de nuevo.',
          icon: Icons.error_outline,
          iconColor: AppColors.error,
          textColor: AppColors.error,
          backgroundColor: AppColors.error.withValues(alpha: 0.1),
          borderColor: AppColors.error.withValues(alpha: 0.3),
          showRetry: true,
          showRecoveryOptions: true,
          helpText: '¿Olvidaste tu contraseña?',
        );

      case AuthErrorType.accountNotFound:
        return _AuthErrorConfig(
          title: 'Cuenta no encontrada',
          message:
              'No encontramos una cuenta con este email. ¿Quieres crear una nueva cuenta?',
          icon: Icons.person_off_outlined,
          iconColor: AppColors.warning,
          textColor: AppColors.textPrimary,
          backgroundColor: AppColors.warning.withValues(alpha: 0.1),
          borderColor: AppColors.warning.withValues(alpha: 0.3),
          showRetry: false,
          showRecoveryOptions: true,
          helpText: 'Crear cuenta',
        );

      case AuthErrorType.emailNotVerified:
        return _AuthErrorConfig(
          title: 'Email no verificado',
          message:
              'Por favor verifica tu email antes de iniciar sesión. Revisa tu bandeja de entrada.',
          icon: Icons.email_outlined,
          iconColor: AppColors.warning,
          textColor: AppColors.textPrimary,
          backgroundColor: AppColors.warning.withValues(alpha: 0.1),
          borderColor: AppColors.warning.withValues(alpha: 0.3),
          showRetry: true,
          showRecoveryOptions: true,
          helpText: 'Reenviar email',
        );

      case AuthErrorType.accountLocked:
        return _AuthErrorConfig(
          title: 'Cuenta bloqueada',
          message:
              'Tu cuenta ha sido bloqueada temporalmente por seguridad. Intenta de nuevo en 30 minutos o contacta a soporte.',
          icon: Icons.lock_outline,
          iconColor: AppColors.error,
          textColor: AppColors.error,
          backgroundColor: AppColors.error.withValues(alpha: 0.1),
          borderColor: AppColors.error.withValues(alpha: 0.3),
          showRetry: false,
          showRecoveryOptions: true,
        );

      case AuthErrorType.networkError:
        return _AuthErrorConfig(
          title: 'Error de conexión',
          message:
              'No pudimos conectar con el servidor. Verifica tu conexión a internet e intenta de nuevo.',
          icon: Icons.wifi_off,
          iconColor: AppColors.warning,
          textColor: AppColors.textPrimary,
          backgroundColor: AppColors.warning.withValues(alpha: 0.1),
          borderColor: AppColors.warning.withValues(alpha: 0.3),
          showRetry: true,
          showRecoveryOptions: true,
        );

      case AuthErrorType.serverError:
        return _AuthErrorConfig(
          title: 'Error del servidor',
          message:
              'Algo salió mal en nuestros servidores. Estamos trabajando en solucionarlo. Por favor intenta más tarde.',
          icon: Icons.error_outline,
          iconColor: AppColors.error,
          textColor: AppColors.error,
          backgroundColor: AppColors.error.withValues(alpha: 0.1),
          borderColor: AppColors.error.withValues(alpha: 0.3),
          showRetry: true,
          showRecoveryOptions: true,
        );

      case AuthErrorType.sessionExpired:
        return _AuthErrorConfig(
          title: 'Sesión expirada',
          message:
              'Tu sesión ha expirado por seguridad. Por favor inicia sesión nuevamente.',
          icon: Icons.timer_off_outlined,
          iconColor: AppColors.warning,
          textColor: AppColors.textPrimary,
          backgroundColor: AppColors.warning.withValues(alpha: 0.1),
          borderColor: AppColors.warning.withValues(alpha: 0.3),
          showRetry: true,
          showRecoveryOptions: false,
        );

      case AuthErrorType.invalidCode:
        return _AuthErrorConfig(
          title: 'Código inválido',
          message:
              'El código que ingresaste es incorrecto o ha expirado. Solicita un nuevo código.',
          icon: Icons.error_outline,
          iconColor: AppColors.error,
          textColor: AppColors.error,
          backgroundColor: AppColors.error.withValues(alpha: 0.1),
          borderColor: AppColors.error.withValues(alpha: 0.3),
          showRetry: true,
          showRecoveryOptions: true,
          helpText: 'Reenviar código',
        );

      case AuthErrorType.tooManyAttempts:
        return _AuthErrorConfig(
          title: 'Demasiados intentos',
          message:
              'Has intentado demasiadas veces. Por seguridad, espera 15 minutos antes de intentar de nuevo.',
          icon: Icons.block,
          iconColor: AppColors.error,
          textColor: AppColors.error,
          backgroundColor: AppColors.error.withValues(alpha: 0.1),
          borderColor: AppColors.error.withValues(alpha: 0.3),
          showRetry: false,
          showRecoveryOptions: true,
        );

      case AuthErrorType.unknown:
        return _AuthErrorConfig(
          title: 'Error desconocido',
          message:
              'Ocurrió un error inesperado. Por favor intenta de nuevo o contacta a soporte si el problema persiste.',
          icon: Icons.help_outline,
          iconColor: AppColors.textSecondary,
          textColor: AppColors.textPrimary,
          backgroundColor: Colors.grey.shade100,
          borderColor: Colors.grey.shade300,
          showRetry: true,
          showRecoveryOptions: true,
        );
    }
  }
}

class _AuthErrorConfig {
  final String title;
  final String message;
  final IconData icon;
  final Color iconColor;
  final Color textColor;
  final Color backgroundColor;
  final Color borderColor;
  final bool showRetry;
  final bool showRecoveryOptions;
  final String? helpText;

  _AuthErrorConfig({
    required this.title,
    required this.message,
    required this.icon,
    required this.iconColor,
    required this.textColor,
    required this.backgroundColor,
    required this.borderColor,
    required this.showRetry,
    required this.showRecoveryOptions,
    this.helpText,
  });
}

class _RecoveryButton extends StatelessWidget {
  final IconData icon;
  final String label;
  final VoidCallback onPressed;
  final Color color;

  const _RecoveryButton({
    required this.icon,
    required this.label,
    required this.onPressed,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    return TextButton.icon(
      onPressed: onPressed,
      icon: Icon(icon, size: 18, color: color),
      label: Text(
        label,
        style: AppTypography.labelMedium.copyWith(
          color: color,
          fontWeight: FontWeight.w600,
        ),
      ),
      style: TextButton.styleFrom(
        padding: const EdgeInsets.symmetric(
          horizontal: AppSpacing.sm,
          vertical: AppSpacing.xs,
        ),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(8),
        ),
      ),
    );
  }
}

/// Snackbar helper para mostrar errores
class AuthErrorSnackbar {
  static void show(
    BuildContext context, {
    required AuthErrorType errorType,
    String? customMessage,
    Duration duration = const Duration(seconds: 4),
  }) {
    final config = _getSnackbarConfig(errorType);

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Row(
          children: [
            Icon(config.icon, color: Colors.white, size: 24),
            const SizedBox(width: AppSpacing.sm),
            Expanded(
              child: Text(
                customMessage ?? config.message,
                style: AppTypography.bodyMedium.copyWith(
                  color: Colors.white,
                ),
              ),
            ),
          ],
        ),
        backgroundColor: config.backgroundColor,
        duration: duration,
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
        action: config.showAction
            ? SnackBarAction(
                label: 'OK',
                textColor: Colors.white,
                onPressed: () {},
              )
            : null,
      ),
    );
  }

  static _SnackbarConfig _getSnackbarConfig(AuthErrorType type) {
    switch (type) {
      case AuthErrorType.invalidCredentials:
        return _SnackbarConfig(
          message: 'Credenciales inválidas',
          icon: Icons.error_outline,
          backgroundColor: AppColors.error,
          showAction: false,
        );
      case AuthErrorType.networkError:
        return _SnackbarConfig(
          message: 'Error de conexión',
          icon: Icons.wifi_off,
          backgroundColor: AppColors.warning,
          showAction: true,
        );
      default:
        return _SnackbarConfig(
          message: 'Ocurrió un error',
          icon: Icons.error_outline,
          backgroundColor: AppColors.error,
          showAction: false,
        );
    }
  }
}

class _SnackbarConfig {
  final String message;
  final IconData icon;
  final Color backgroundColor;
  final bool showAction;

  _SnackbarConfig({
    required this.message,
    required this.icon,
    required this.backgroundColor,
    required this.showAction,
  });
}
