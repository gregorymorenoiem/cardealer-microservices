import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';
import '../../widgets/auth/password_strength_indicator.dart';
import '../../widgets/buttons/gradient_button.dart';

/// Forgot Password Flow - Sprint 7 AE-008
/// Enhanced password recovery with email/phone selection
/// Features:
/// - Choose recovery method (email/phone)
/// - OTP verification (reuses AE-006 component pattern)
/// - New password setup with strength indicator
/// - Success confirmation

enum ForgotPasswordStep {
  selectMethod,
  enterContact,
  verifyCode,
  newPassword,
  success,
}

enum RecoveryMethod {
  email,
  phone,
}

class ForgotPasswordFlowPage extends StatefulWidget {
  const ForgotPasswordFlowPage({super.key});

  @override
  State<ForgotPasswordFlowPage> createState() => _ForgotPasswordFlowPageState();
}

class _ForgotPasswordFlowPageState extends State<ForgotPasswordFlowPage>
    with TickerProviderStateMixin {
  ForgotPasswordStep _currentStep = ForgotPasswordStep.selectMethod;
  RecoveryMethod _selectedMethod = RecoveryMethod.email;

  // Controllers
  final _contactController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();
  final List<TextEditingController> _codeControllers =
      List.generate(6, (_) => TextEditingController());
  final List<FocusNode> _codeFocusNodes = List.generate(6, (_) => FocusNode());

  // State
  bool _obscureConfirmPassword = true;
  bool _isLoading = false;
  int _resendCountdown = 0;
  String _maskedContact = '';

  // Animation
  late final AnimationController _animationController;
  late final Animation<double> _fadeAnimation;
  late final Animation<Offset> _slideAnimation;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 600),
      vsync: this,
    );

    _fadeAnimation = Tween<double>(
      begin: 0.0,
      end: 1.0,
    ).animate(CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeOut,
    ));

    _slideAnimation = Tween<Offset>(
      begin: const Offset(0.3, 0),
      end: Offset.zero,
    ).animate(CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeOutCubic,
    ));

    _animationController.forward();
  }

  @override
  void dispose() {
    _animationController.dispose();
    _contactController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    for (var controller in _codeControllers) {
      controller.dispose();
    }
    for (var focusNode in _codeFocusNodes) {
      focusNode.dispose();
    }
    super.dispose();
  }

  void _nextStep() {
    setState(() {
      switch (_currentStep) {
        case ForgotPasswordStep.selectMethod:
          _currentStep = ForgotPasswordStep.enterContact;
          break;
        case ForgotPasswordStep.enterContact:
          _handleSendCode();
          break;
        case ForgotPasswordStep.verifyCode:
          _handleVerifyCode();
          break;
        case ForgotPasswordStep.newPassword:
          _handleResetPassword();
          break;
        case ForgotPasswordStep.success:
          break;
      }
      _animationController.reset();
      _animationController.forward();
    });
  }

  void _previousStep() {
    if (_currentStep.index > 0) {
      setState(() {
        _currentStep = ForgotPasswordStep.values[_currentStep.index - 1];
        _animationController.reset();
        _animationController.forward();
      });
    }
  }

  Future<void> _handleSendCode() async {
    setState(() => _isLoading = true);

    // Simulate API call
    await Future.delayed(const Duration(seconds: 2));

    final contact = _contactController.text;
    if (_selectedMethod == RecoveryMethod.email) {
      // Mask email: a***@example.com
      final parts = contact.split('@');
      if (parts.length == 2) {
        _maskedContact = '${parts[0][0]}***@${parts[1]}';
      }
    } else {
      // Mask phone: ***-***-1234
      if (contact.length >= 4) {
        _maskedContact = '***-***-${contact.substring(contact.length - 4)}';
      }
    }

    setState(() {
      _isLoading = false;
      _currentStep = ForgotPasswordStep.verifyCode;
      _startResendTimer();
    });
  }

  Future<void> _handleVerifyCode() async {
    final code = _codeControllers.map((c) => c.text).join();

    if (code.length != 6) {
      _showError('Ingresa el código completo');
      return;
    }

    setState(() => _isLoading = true);

    // Simulate API verification
    await Future.delayed(const Duration(seconds: 1));

    // Mock: accept code "123456"
    if (code == '123456') {
      setState(() {
        _isLoading = false;
        _currentStep = ForgotPasswordStep.newPassword;
      });
    } else {
      setState(() => _isLoading = false);
      _showError('Código inválido');
      _clearCodeInputs();
    }
  }

  Future<void> _handleResetPassword() async {
    if (_passwordController.text != _confirmPasswordController.text) {
      _showError('Las contraseñas no coinciden');
      return;
    }

    if (_passwordController.text.length < 8) {
      _showError('La contraseña debe tener al menos 8 caracteres');
      return;
    }

    setState(() => _isLoading = true);

    // Simulate API call
    await Future.delayed(const Duration(seconds: 2));

    setState(() {
      _isLoading = false;
      _currentStep = ForgotPasswordStep.success;
    });
  }

  void _startResendTimer() {
    setState(() => _resendCountdown = 60);
    Future.doWhile(() async {
      await Future.delayed(const Duration(seconds: 1));
      if (mounted) {
        setState(() {
          if (_resendCountdown > 0) {
            _resendCountdown--;
          }
        });
        return _resendCountdown > 0;
      }
      return false;
    });
  }

  void _handleResend() {
    for (var controller in _codeControllers) {
      controller.clear();
    }
    _codeFocusNodes[0].requestFocus();
    _startResendTimer();

    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Código reenviado')),
    );
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Row(
          children: [
            const Icon(Icons.error_outline, color: Colors.white),
            const SizedBox(width: AppSpacing.sm),
            Expanded(child: Text(message)),
          ],
        ),
        backgroundColor: AppColors.error,
      ),
    );
  }

  void _clearCodeInputs() {
    for (var controller in _codeControllers) {
      controller.clear();
    }
    _codeFocusNodes[0].requestFocus();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back, color: AppColors.primary),
          onPressed: () {
            if (_currentStep == ForgotPasswordStep.selectMethod) {
              Navigator.pop(context);
            } else {
              _previousStep();
            }
          },
        ),
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: FadeTransition(
            opacity: _fadeAnimation,
            child: SlideTransition(
              position: _slideAnimation,
              child: _buildStepContent(),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildStepContent() {
    switch (_currentStep) {
      case ForgotPasswordStep.selectMethod:
        return _buildSelectMethodStep();
      case ForgotPasswordStep.enterContact:
        return _buildEnterContactStep();
      case ForgotPasswordStep.verifyCode:
        return _buildVerifyCodeStep();
      case ForgotPasswordStep.newPassword:
        return _buildNewPasswordStep();
      case ForgotPasswordStep.success:
        return _buildSuccessStep();
    }
  }

  Widget _buildSelectMethodStep() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        // Header
        Container(
          width: 80,
          height: 80,
          decoration: BoxDecoration(
            shape: BoxShape.circle,
            gradient: const LinearGradient(
              colors: [AppColors.primary, AppColors.accent],
            ),
            boxShadow: [
              BoxShadow(
                color: AppColors.primary.withValues(alpha: 0.3),
                blurRadius: 12,
                offset: const Offset(0, 4),
              ),
            ],
          ),
          child: const Icon(
            Icons.lock_reset,
            color: Colors.white,
            size: 40,
          ),
        ),
        const SizedBox(height: AppSpacing.lg),
        Text(
          '¿Olvidaste tu contraseña?',
          style: AppTypography.h2.copyWith(
            color: AppColors.textPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: AppSpacing.sm),
        Text(
          'Elige cómo quieres recuperar tu cuenta',
          style: AppTypography.bodyLarge.copyWith(
            color: AppColors.textSecondary,
          ),
        ),
        const SizedBox(height: AppSpacing.xl),

        // Recovery Methods
        _buildMethodCard(
          method: RecoveryMethod.email,
          icon: Icons.email_outlined,
          title: 'Recuperar por Email',
          description: 'Enviaremos un código de verificación a tu email',
        ),
        const SizedBox(height: AppSpacing.md),
        _buildMethodCard(
          method: RecoveryMethod.phone,
          icon: Icons.phone_outlined,
          title: 'Recuperar por Teléfono',
          description: 'Enviaremos un código SMS a tu número de teléfono',
        ),
        const SizedBox(height: AppSpacing.xl),

        GradientButton(
          onPressed: _nextStep,
          text: 'Continuar',
          icon: const Icon(Icons.arrow_forward),
        ),
      ],
    );
  }

  Widget _buildMethodCard({
    required RecoveryMethod method,
    required IconData icon,
    required String title,
    required String description,
  }) {
    final isSelected = _selectedMethod == method;

    return GestureDetector(
      onTap: () {
        setState(() {
          _selectedMethod = method;
        });
      },
      child: Container(
        padding: const EdgeInsets.all(AppSpacing.md),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: isSelected ? AppColors.primary : Colors.grey.shade300,
            width: isSelected ? 2 : 1,
          ),
          boxShadow: isSelected
              ? [
                  BoxShadow(
                    color: AppColors.primary.withValues(alpha: 0.2),
                    blurRadius: 8,
                    offset: const Offset(0, 2),
                  ),
                ]
              : null,
        ),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(AppSpacing.sm),
              decoration: BoxDecoration(
                color: isSelected
                    ? AppColors.primary.withValues(alpha: 0.1)
                    : Colors.grey.shade100,
                borderRadius: BorderRadius.circular(12),
              ),
              child: Icon(
                icon,
                color: isSelected ? AppColors.primary : Colors.grey.shade600,
                size: 28,
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
                      color: AppColors.textPrimary,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: AppSpacing.xs),
                  Text(
                    description,
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                ],
              ),
            ),
            if (isSelected)
              const Icon(
                Icons.check_circle,
                color: AppColors.primary,
                size: 24,
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildEnterContactStep() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text(
          _selectedMethod == RecoveryMethod.email
              ? 'Ingresa tu email'
              : 'Ingresa tu teléfono',
          style: AppTypography.h3.copyWith(
            color: AppColors.textPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: AppSpacing.sm),
        Text(
          'Te enviaremos un código de verificación',
          style: AppTypography.bodyLarge.copyWith(
            color: AppColors.textSecondary,
          ),
        ),
        const SizedBox(height: AppSpacing.xl),
        TextFormField(
          controller: _contactController,
          keyboardType: _selectedMethod == RecoveryMethod.email
              ? TextInputType.emailAddress
              : TextInputType.phone,
          decoration: InputDecoration(
            labelText:
                _selectedMethod == RecoveryMethod.email ? 'Email' : 'Teléfono',
            prefixIcon: Icon(
              _selectedMethod == RecoveryMethod.email
                  ? Icons.email
                  : Icons.phone,
            ),
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
            ),
          ),
        ),
        const SizedBox(height: AppSpacing.xl),
        GradientButton(
          onPressed: _isLoading ? null : _nextStep,
          text: _isLoading ? 'Enviando...' : 'Enviar código',
          icon: _isLoading ? null : const Icon(Icons.send),
        ),
      ],
    );
  }

  Widget _buildVerifyCodeStep() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text(
          'Verifica tu identidad',
          style: AppTypography.h3.copyWith(
            color: AppColors.textPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: AppSpacing.sm),
        Text(
          'Ingresa el código de 6 dígitos enviado a $_maskedContact',
          style: AppTypography.bodyLarge.copyWith(
            color: AppColors.textSecondary,
          ),
        ),
        const SizedBox(height: AppSpacing.xl),

        // OTP Input
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: List.generate(
            6,
            (index) => SizedBox(
              width: 48,
              child: TextFormField(
                controller: _codeControllers[index],
                focusNode: _codeFocusNodes[index],
                keyboardType: TextInputType.number,
                textAlign: TextAlign.center,
                maxLength: 1,
                style: AppTypography.h3.copyWith(
                  fontWeight: FontWeight.bold,
                ),
                decoration: InputDecoration(
                  counterText: '',
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                onChanged: (value) {
                  if (value.isNotEmpty && index < 5) {
                    _codeFocusNodes[index + 1].requestFocus();
                  }
                  if (index == 5 && value.isNotEmpty) {
                    _handleVerifyCode();
                  }
                },
              ),
            ),
          ),
        ),
        const SizedBox(height: AppSpacing.lg),

        // Resend
        if (_resendCountdown > 0)
          Text(
            'Reenviar código en $_resendCountdown segundos',
            textAlign: TextAlign.center,
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textSecondary,
            ),
          )
        else
          TextButton(
            onPressed: _handleResend,
            child: Text(
              'Reenviar código',
              style: AppTypography.bodyMedium.copyWith(
                color: AppColors.primary,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        const SizedBox(height: AppSpacing.xl),

        // Tip
        Container(
          padding: const EdgeInsets.all(AppSpacing.md),
          decoration: BoxDecoration(
            color: AppColors.info.withValues(alpha: 0.1),
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: AppColors.info.withValues(alpha: 0.3)),
          ),
          child: Row(
            children: [
              const Icon(Icons.info_outline, color: AppColors.info, size: 20),
              const SizedBox(width: AppSpacing.sm),
              Expanded(
                child: Text(
                  'Para pruebas, usa el código: 123456',
                  style: AppTypography.bodySmall.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildNewPasswordStep() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text(
          'Crea una nueva contraseña',
          style: AppTypography.h3.copyWith(
            color: AppColors.textPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: AppSpacing.sm),
        Text(
          'Asegúrate de que sea segura y diferente a la anterior',
          style: AppTypography.bodyLarge.copyWith(
            color: AppColors.textSecondary,
          ),
        ),
        const SizedBox(height: AppSpacing.xl),
        PasswordFieldWithStrength(
          controller: _passwordController,
        ),
        const SizedBox(height: AppSpacing.md),
        TextFormField(
          controller: _confirmPasswordController,
          obscureText: _obscureConfirmPassword,
          decoration: InputDecoration(
            labelText: 'Confirmar Contraseña',
            prefixIcon: const Icon(Icons.lock_outline),
            suffixIcon: IconButton(
              icon: Icon(
                _obscureConfirmPassword
                    ? Icons.visibility_outlined
                    : Icons.visibility_off_outlined,
              ),
              onPressed: () {
                setState(() {
                  _obscureConfirmPassword = !_obscureConfirmPassword;
                });
              },
            ),
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
            ),
          ),
        ),
        const SizedBox(height: AppSpacing.xl),
        GradientButton(
          onPressed: _isLoading ? null : _nextStep,
          text: _isLoading ? 'Guardando...' : 'Restablecer contraseña',
          icon: _isLoading ? null : const Icon(Icons.check),
        ),
      ],
    );
  }

  Widget _buildSuccessStep() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.center,
      children: [
        const SizedBox(height: AppSpacing.xl),
        Container(
          width: 100,
          height: 100,
          decoration: BoxDecoration(
            shape: BoxShape.circle,
            color: AppColors.success.withValues(alpha: 0.1),
          ),
          child: const Icon(
            Icons.check_circle,
            color: AppColors.success,
            size: 60,
          ),
        ),
        const SizedBox(height: AppSpacing.lg),
        Text(
          '¡Contraseña restablecida!',
          style: AppTypography.h2.copyWith(
            color: AppColors.textPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: AppSpacing.sm),
        Text(
          'Tu contraseña ha sido actualizada exitosamente',
          textAlign: TextAlign.center,
          style: AppTypography.bodyLarge.copyWith(
            color: AppColors.textSecondary,
          ),
        ),
        const SizedBox(height: AppSpacing.xl),
        GradientButton(
          onPressed: () {
            Navigator.popUntil(context, (route) => route.isFirst);
          },
          text: 'Ir a inicio de sesión',
          icon: const Icon(Icons.arrow_forward),
        ),
      ],
    );
  }
}
