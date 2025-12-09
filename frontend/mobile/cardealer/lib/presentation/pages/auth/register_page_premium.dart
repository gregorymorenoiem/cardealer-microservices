import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';
import '../../widgets/auth/password_strength_indicator.dart';
import '../../widgets/buttons/gradient_button.dart';

/// Register Flow Redesign - Sprint 7 AE-005
/// Multi-step registration con progress indicator
/// Features:
/// - 3 steps: Account Type → Basic Info → Security
/// - Progress indicator visual
/// - Inline validation
/// - Password strength integration
/// - Role selection (individual/dealer)

enum RegisterStep {
  accountType,
  basicInfo,
  security,
}

class RegisterPagePremium extends StatefulWidget {
  const RegisterPagePremium({super.key});

  @override
  State<RegisterPagePremium> createState() => _RegisterPagePremiumState();
}

class _RegisterPagePremiumState extends State<RegisterPagePremium>
    with TickerProviderStateMixin {
  RegisterStep _currentStep = RegisterStep.accountType;
  final _formKey = GlobalKey<FormState>();

  // Controllers
  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  final _phoneController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();

  // State
  String _selectedRole = 'individual';
  bool _termsAccepted = false;
  bool _obscureConfirmPassword = true;

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
    _nameController.dispose();
    _emailController.dispose();
    _phoneController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }

  void _nextStep() {
    if (_currentStep == RegisterStep.accountType) {
      setState(() {
        _currentStep = RegisterStep.basicInfo;
        _animationController.reset();
        _animationController.forward();
      });
    } else if (_currentStep == RegisterStep.basicInfo) {
      if (_formKey.currentState!.validate()) {
        setState(() {
          _currentStep = RegisterStep.security;
          _animationController.reset();
          _animationController.forward();
        });
      }
    } else if (_currentStep == RegisterStep.security) {
      if (_formKey.currentState!.validate() && _termsAccepted) {
        _handleRegister();
      }
    }
  }

  void _previousStep() {
    if (_currentStep == RegisterStep.basicInfo) {
      setState(() {
        _currentStep = RegisterStep.accountType;
        _animationController.reset();
        _animationController.forward();
      });
    } else if (_currentStep == RegisterStep.security) {
      setState(() {
        _currentStep = RegisterStep.basicInfo;
        _animationController.reset();
        _animationController.forward();
      });
    }
  }

  void _handleRegister() {
    // TODO: Implement registration logic
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Registro implementado próximamente'),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        leading: _currentStep != RegisterStep.accountType
            ? IconButton(
                icon: const Icon(Icons.arrow_back, color: AppColors.primary),
                onPressed: _previousStep,
              )
            : null,
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                // Progress Indicator
                _buildProgressIndicator(),
                const SizedBox(height: AppSpacing.xl),

                // Step Content
                FadeTransition(
                  opacity: _fadeAnimation,
                  child: SlideTransition(
                    position: _slideAnimation,
                    child: _buildStepContent(),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildProgressIndicator() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            _buildProgressDot(RegisterStep.accountType),
            Expanded(child: _buildProgressLine(RegisterStep.basicInfo)),
            _buildProgressDot(RegisterStep.basicInfo),
            Expanded(child: _buildProgressLine(RegisterStep.security)),
            _buildProgressDot(RegisterStep.security),
          ],
        ),
        const SizedBox(height: AppSpacing.md),
        Text(
          _getStepTitle(_currentStep),
          style: AppTypography.h2.copyWith(
            color: AppColors.textPrimary,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: AppSpacing.xs),
        Text(
          'Paso ${_getStepNumber(_currentStep)} de 3',
          style: AppTypography.bodyMedium.copyWith(
            color: AppColors.textSecondary,
          ),
        ),
      ],
    );
  }

  Widget _buildProgressDot(RegisterStep step) {
    final isActive = _getStepNumber(step) <= _getStepNumber(_currentStep);
    final isCompleted = _getStepNumber(step) < _getStepNumber(_currentStep);

    return Container(
      width: 40,
      height: 40,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: isActive ? AppColors.primary : Colors.grey.shade300,
        boxShadow: isActive
            ? [
                BoxShadow(
                  color: AppColors.primary.withValues(alpha: 0.3),
                  blurRadius: 8,
                  offset: const Offset(0, 2),
                ),
              ]
            : null,
      ),
      child: Center(
        child: isCompleted
            ? const Icon(
                Icons.check,
                color: Colors.white,
                size: 24,
              )
            : Text(
                '${_getStepNumber(step)}',
                style: AppTypography.labelLarge.copyWith(
                  color: isActive ? Colors.white : Colors.grey.shade600,
                  fontWeight: FontWeight.bold,
                ),
              ),
      ),
    );
  }

  Widget _buildProgressLine(RegisterStep step) {
    final isActive = _getStepNumber(step) <= _getStepNumber(_currentStep);

    return Container(
      height: 2,
      margin: const EdgeInsets.symmetric(horizontal: AppSpacing.xs),
      color: isActive ? AppColors.primary : Colors.grey.shade300,
    );
  }

  Widget _buildStepContent() {
    switch (_currentStep) {
      case RegisterStep.accountType:
        return _buildAccountTypeStep();
      case RegisterStep.basicInfo:
        return _buildBasicInfoStep();
      case RegisterStep.security:
        return _buildSecurityStep();
    }
  }

  Widget _buildAccountTypeStep() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text(
          '¿Qué tipo de cuenta necesitas?',
          style: AppTypography.bodyLarge.copyWith(
            color: AppColors.textSecondary,
          ),
        ),
        const SizedBox(height: AppSpacing.xl),
        _buildAccountTypeCard(
          role: 'individual',
          icon: Icons.person_outline,
          title: 'Cuenta Personal',
          description: 'Para comprar vehículos y gestionar tus favoritos',
          benefits: [
            'Búsqueda avanzada de vehículos',
            'Guarda tus favoritos',
            'Recibe alertas personalizadas',
          ],
        ),
        const SizedBox(height: AppSpacing.lg),
        _buildAccountTypeCard(
          role: 'dealer',
          icon: Icons.business_outlined,
          title: 'Cuenta de Concesionario',
          description: 'Para vender vehículos y gestionar tu inventario',
          benefits: [
            'Publica vehículos ilimitados',
            'Dashboard de ventas',
            'Estadísticas y reportes',
          ],
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

  Widget _buildAccountTypeCard({
    required String role,
    required IconData icon,
    required String title,
    required String description,
    required List<String> benefits,
  }) {
    final isSelected = _selectedRole == role;

    return GestureDetector(
      onTap: () {
        setState(() {
          _selectedRole = role;
        });
      },
      child: Container(
        padding: const EdgeInsets.all(AppSpacing.lg),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: isSelected ? AppColors.primary : Colors.grey.shade300,
            width: isSelected ? 2 : 1,
          ),
          boxShadow: isSelected
              ? [
                  BoxShadow(
                    color: AppColors.primary.withValues(alpha: 0.2),
                    blurRadius: 12,
                    offset: const Offset(0, 4),
                  ),
                ]
              : null,
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
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
                    color:
                        isSelected ? AppColors.primary : Colors.grey.shade600,
                    size: 32,
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
            const SizedBox(height: AppSpacing.md),
            const Divider(),
            const SizedBox(height: AppSpacing.sm),
            ...benefits.map((benefit) => Padding(
                  padding: const EdgeInsets.only(bottom: AppSpacing.xs),
                  child: Row(
                    children: [
                      Icon(
                        Icons.check,
                        size: 16,
                        color: isSelected
                            ? AppColors.primary
                            : Colors.grey.shade600,
                      ),
                      const SizedBox(width: AppSpacing.xs),
                      Expanded(
                        child: Text(
                          benefit,
                          style: AppTypography.bodySmall.copyWith(
                            color: AppColors.textSecondary,
                          ),
                        ),
                      ),
                    ],
                  ),
                )),
          ],
        ),
      ),
    );
  }

  Widget _buildBasicInfoStep() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        TextFormField(
          controller: _nameController,
          decoration: InputDecoration(
            labelText: _selectedRole == 'dealer'
                ? 'Nombre del Concesionario'
                : 'Nombre Completo',
            prefixIcon: Icon(
              _selectedRole == 'dealer' ? Icons.business : Icons.person,
            ),
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
            ),
          ),
          validator: (value) {
            if (value == null || value.isEmpty) {
              return 'Este campo es requerido';
            }
            return null;
          },
        ),
        const SizedBox(height: AppSpacing.md),
        TextFormField(
          controller: _emailController,
          keyboardType: TextInputType.emailAddress,
          decoration: InputDecoration(
            labelText: 'Email',
            prefixIcon: const Icon(Icons.email),
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
            ),
          ),
          validator: (value) {
            if (value == null || value.isEmpty) {
              return 'El email es requerido';
            }
            if (!value.contains('@')) {
              return 'Ingresa un email válido';
            }
            return null;
          },
        ),
        const SizedBox(height: AppSpacing.md),
        TextFormField(
          controller: _phoneController,
          keyboardType: TextInputType.phone,
          decoration: InputDecoration(
            labelText: 'Teléfono',
            prefixIcon: const Icon(Icons.phone),
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
            ),
          ),
          validator: (value) {
            if (value == null || value.isEmpty) {
              return 'El teléfono es requerido';
            }
            return null;
          },
        ),
        if (_selectedRole == 'dealer') ...[
          const SizedBox(height: AppSpacing.md),
          TextFormField(
            decoration: InputDecoration(
              labelText: 'Dirección del Concesionario',
              prefixIcon: const Icon(Icons.location_on),
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
          ),
        ],
        const SizedBox(height: AppSpacing.xl),
        GradientButton(
          onPressed: _nextStep,
          text: 'Continuar',
          icon: const Icon(Icons.arrow_forward),
        ),
      ],
    );
  }

  Widget _buildSecurityStep() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
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
          validator: (value) {
            if (value == null || value.isEmpty) {
              return 'Confirma tu contraseña';
            }
            if (value != _passwordController.text) {
              return 'Las contraseñas no coinciden';
            }
            return null;
          },
        ),
        const SizedBox(height: AppSpacing.lg),
        // Terms and Conditions
        Container(
          padding: const EdgeInsets.all(AppSpacing.md),
          decoration: BoxDecoration(
            color: Colors.grey.shade100,
            borderRadius: BorderRadius.circular(12),
          ),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Checkbox(
                value: _termsAccepted,
                onChanged: (value) {
                  setState(() {
                    _termsAccepted = value ?? false;
                  });
                },
                activeColor: AppColors.primary,
              ),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const SizedBox(height: 12),
                    Text.rich(
                      TextSpan(
                        text: 'Acepto los ',
                        style: AppTypography.bodySmall.copyWith(
                          color: AppColors.textSecondary,
                        ),
                        children: [
                          TextSpan(
                            text: 'Términos y Condiciones',
                            style: AppTypography.bodySmall.copyWith(
                              color: AppColors.primary,
                              fontWeight: FontWeight.bold,
                              decoration: TextDecoration.underline,
                            ),
                          ),
                          const TextSpan(text: ' y la '),
                          TextSpan(
                            text: 'Política de Privacidad',
                            style: AppTypography.bodySmall.copyWith(
                              color: AppColors.primary,
                              fontWeight: FontWeight.bold,
                              decoration: TextDecoration.underline,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
        if (!_termsAccepted) ...[
          const SizedBox(height: AppSpacing.sm),
          Text(
            'Debes aceptar los términos para continuar',
            style: AppTypography.bodySmall.copyWith(
              color: AppColors.error,
            ),
          ),
        ],
        const SizedBox(height: AppSpacing.xl),
        GradientButton(
          onPressed: _termsAccepted ? _nextStep : null,
          text: 'Crear Cuenta',
          icon: const Icon(Icons.check),
        ),
        const SizedBox(height: AppSpacing.md),
        Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(
              '¿Ya tienes cuenta? ',
              style: AppTypography.bodyMedium.copyWith(
                color: AppColors.textSecondary,
              ),
            ),
            TextButton(
              onPressed: () {
                Navigator.pop(context);
              },
              child: Text(
                'Inicia sesión',
                style: AppTypography.bodyMedium.copyWith(
                  color: AppColors.primary,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ],
        ),
      ],
    );
  }

  String _getStepTitle(RegisterStep step) {
    switch (step) {
      case RegisterStep.accountType:
        return 'Tipo de Cuenta';
      case RegisterStep.basicInfo:
        return 'Información Básica';
      case RegisterStep.security:
        return 'Seguridad';
    }
  }

  int _getStepNumber(RegisterStep step) {
    switch (step) {
      case RegisterStep.accountType:
        return 1;
      case RegisterStep.basicInfo:
        return 2;
      case RegisterStep.security:
        return 3;
    }
  }
}
