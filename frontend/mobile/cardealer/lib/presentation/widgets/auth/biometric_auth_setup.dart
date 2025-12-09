import 'package:flutter/material.dart';
import 'package:local_auth/local_auth.dart';
import '../../../core/services/biometric_auth_service.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';

/// Biometric Auth Setup Widget - Sprint 7 AE-003
/// Widget para configurar autenticación biométrica
/// Features:
/// - Detección de tipo de biometría (Face ID/Touch ID/Fingerprint)
/// - Setup flow con pasos claros
/// - Fallback a password explicado
/// - Animaciones y visual feedback

class BiometricAuthSetup extends StatefulWidget {
  final VoidCallback onEnabled;
  final VoidCallback onSkipped;

  const BiometricAuthSetup({
    super.key,
    required this.onEnabled,
    required this.onSkipped,
  });

  @override
  State<BiometricAuthSetup> createState() => _BiometricAuthSetupState();
}

class _BiometricAuthSetupState extends State<BiometricAuthSetup>
    with SingleTickerProviderStateMixin {
  final _biometricService = BiometricAuthService();
  late AnimationController _controller;
  late Animation<double> _scaleAnimation;

  bool _isAvailable = false;
  bool _isLoading = true;
  String _biometricName = 'Biometría';
  IconData _biometricIcon = Icons.fingerprint;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1500),
    )..repeat(reverse: true);

    _scaleAnimation = Tween<double>(begin: 1.0, end: 1.1).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeInOut),
    );

    _checkBiometricAvailability();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  Future<void> _checkBiometricAvailability() async {
    final isAvailable = await _biometricService.isBiometricAvailable();
    final types = await _biometricService.getAvailableBiometrics();

    setState(() {
      _isAvailable = isAvailable;
      _isLoading = false;
      _updateBiometricInfo(types);
    });
  }

  void _updateBiometricInfo(List<BiometricType> types) {
    if (types.contains(BiometricType.face)) {
      _biometricName = 'Face ID';
      _biometricIcon = Icons.face;
    } else if (types.contains(BiometricType.fingerprint)) {
      _biometricName = 'Touch ID';
      _biometricIcon = Icons.fingerprint;
    } else if (types.contains(BiometricType.iris)) {
      _biometricName = 'Iris Scan';
      _biometricIcon = Icons.remove_red_eye;
    }
  }

  Future<void> _handleEnable() async {
    final result = await _biometricService.authenticate(
      reason: 'Verifica tu identidad para habilitar $_biometricName',
      useErrorDialogs: true,
      stickyAuth: true,
    );

    if (!mounted) return;

    if (result.success) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Row(
            children: [
              const Icon(Icons.check_circle, color: Colors.white),
              const SizedBox(width: 12),
              Text('$_biometricName habilitado correctamente'),
            ],
          ),
          backgroundColor: AppColors.success,
          behavior: SnackBarBehavior.floating,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
        ),
      );
      widget.onEnabled();
    } else {
      String errorMessage =
          result.errorMessage ?? 'Error al habilitar $_biometricName';

      if (result.errorType == BiometricErrorType.notEnrolled) {
        errorMessage =
            'No tienes $_biometricName configurado. Ve a Configuración para configurarlo.';
      }

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Row(
            children: [
              const Icon(Icons.error_outline, color: Colors.white),
              const SizedBox(width: 12),
              Expanded(child: Text(errorMessage)),
            ],
          ),
          backgroundColor: AppColors.error,
          behavior: SnackBarBehavior.floating,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
          ),
          duration: const Duration(seconds: 4),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.xl),
      decoration: const BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          // Handle bar
          Container(
            width: 40,
            height: 4,
            decoration: BoxDecoration(
              color: Colors.grey.shade300,
              borderRadius: BorderRadius.circular(2),
            ),
          ),
          const SizedBox(height: AppSpacing.xl),
          if (_isLoading) _buildLoading() else _buildContent(),
        ],
      ),
    );
  }

  Widget _buildLoading() {
    return const Column(
      children: [
        CircularProgressIndicator(),
        SizedBox(height: AppSpacing.lg),
        Text('Verificando disponibilidad...'),
        SizedBox(height: AppSpacing.xxl),
      ],
    );
  }

  Widget _buildContent() {
    if (!_isAvailable) {
      return _buildNotAvailable();
    }

    return Column(
      children: [
        // Icono animado
        ScaleTransition(
          scale: _scaleAnimation,
          child: Container(
            width: 100,
            height: 100,
            decoration: BoxDecoration(
              gradient: const LinearGradient(
                colors: [AppColors.primary, AppColors.accent],
              ),
              shape: BoxShape.circle,
              boxShadow: [
                BoxShadow(
                  color: AppColors.primary.withValues(alpha: 0.3),
                  blurRadius: 20,
                  offset: const Offset(0, 8),
                ),
              ],
            ),
            child: Icon(
              _biometricIcon,
              size: 50,
              color: Colors.white,
            ),
          ),
        ),
        const SizedBox(height: AppSpacing.xl),
        // Título
        Text(
          'Habilitar $_biometricName',
          style: AppTypography.h2.copyWith(
            fontWeight: FontWeight.bold,
          ),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: AppSpacing.md),
        // Descripción
        Text(
          'Accede más rápido y de forma segura usando $_biometricName',
          style: AppTypography.bodyLarge.copyWith(
            color: AppColors.textSecondary,
          ),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: AppSpacing.xl),
        // Beneficios
        _buildBenefit(
          icon: Icons.flash_on,
          title: 'Acceso rápido',
          description: 'Inicia sesión en segundos',
        ),
        const SizedBox(height: AppSpacing.md),
        _buildBenefit(
          icon: Icons.security,
          title: 'Más seguro',
          description: 'Protección adicional para tu cuenta',
        ),
        const SizedBox(height: AppSpacing.md),
        _buildBenefit(
          icon: Icons.lock_outline,
          title: 'Fallback seguro',
          description: 'Siempre podrás usar tu contraseña',
        ),
        const SizedBox(height: AppSpacing.xxl),
        // Botones
        SizedBox(
          width: double.infinity,
          height: 56,
          child: ElevatedButton(
            onPressed: _handleEnable,
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
            child: Text(
              'Habilitar $_biometricName',
              style: AppTypography.labelLarge.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ),
        const SizedBox(height: AppSpacing.md),
        TextButton(
          onPressed: widget.onSkipped,
          child: Text(
            'Tal vez luego',
            style: AppTypography.labelLarge.copyWith(
              color: AppColors.textSecondary,
            ),
          ),
        ),
        const SizedBox(height: AppSpacing.lg),
      ],
    );
  }

  Widget _buildNotAvailable() {
    return Column(
      children: [
        const Icon(
          Icons.info_outline,
          size: 80,
          color: AppColors.warning,
        ),
        const SizedBox(height: AppSpacing.lg),
        Text(
          'Biometría no disponible',
          style: AppTypography.h2.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: AppSpacing.md),
        Text(
          'Tu dispositivo no tiene biometría configurada o no es compatible.',
          style: AppTypography.bodyLarge.copyWith(
            color: AppColors.textSecondary,
          ),
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: AppSpacing.xl),
        SizedBox(
          width: double.infinity,
          height: 56,
          child: ElevatedButton(
            onPressed: widget.onSkipped,
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
            child: const Text(
              'Continuar',
              style: TextStyle(
                color: Colors.white,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ),
        const SizedBox(height: AppSpacing.lg),
      ],
    );
  }

  Widget _buildBenefit({
    required IconData icon,
    required String title,
    required String description,
  }) {
    return Row(
      children: [
        Container(
          width: 48,
          height: 48,
          decoration: BoxDecoration(
            color: AppColors.primary.withValues(alpha: 0.1),
            borderRadius: BorderRadius.circular(12),
          ),
          child: Icon(
            icon,
            color: AppColors.primary,
            size: 24,
          ),
        ),
        const SizedBox(width: AppSpacing.md),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: AppTypography.labelLarge.copyWith(
                  fontWeight: FontWeight.w600,
                ),
              ),
              Text(
                description,
                style: AppTypography.bodyMedium.copyWith(
                  color: AppColors.textSecondary,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
