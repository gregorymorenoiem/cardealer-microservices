import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';

/// Password Strength Indicator Widget - Sprint 7 AE-007
/// Indicador visual de seguridad de contraseña
/// Features:
/// - Meter visual con colores
/// - Tips de seguridad en tiempo real
/// - Validación de requisitos
/// - Animaciones suaves

enum PasswordStrength {
  weak,
  fair,
  good,
  strong,
}

class PasswordStrengthIndicator extends StatefulWidget {
  final String password;
  final bool showTips;

  const PasswordStrengthIndicator({
    super.key,
    required this.password,
    this.showTips = true,
  });

  @override
  State<PasswordStrengthIndicator> createState() =>
      _PasswordStrengthIndicatorState();
}

class _PasswordStrengthIndicatorState extends State<PasswordStrengthIndicator>
    with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _progressAnimation;
  PasswordStrength _currentStrength = PasswordStrength.weak;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 300),
    );

    _progressAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: _animationController,
        curve: Curves.easeOut,
      ),
    );
  }

  @override
  void didUpdateWidget(PasswordStrengthIndicator oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.password != oldWidget.password) {
      _updateStrength();
    }
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  void _updateStrength() {
    final newStrength = _calculateStrength(widget.password);
    if (newStrength != _currentStrength) {
      setState(() {
        _currentStrength = newStrength;
      });
      _animationController.reset();
      _animationController.forward();
    }
  }

  PasswordStrength _calculateStrength(String password) {
    if (password.isEmpty) return PasswordStrength.weak;

    int score = 0;

    // Length
    if (password.length >= 8) score++;
    if (password.length >= 12) score++;

    // Uppercase
    if (password.contains(RegExp(r'[A-Z]'))) score++;

    // Lowercase
    if (password.contains(RegExp(r'[a-z]'))) score++;

    // Numbers
    if (password.contains(RegExp(r'[0-9]'))) score++;

    // Special characters
    if (password.contains(RegExp(r'[!@#$%^&*(),.?":{}|<>]'))) score++;

    if (score <= 2) return PasswordStrength.weak;
    if (score <= 4) return PasswordStrength.fair;
    if (score <= 5) return PasswordStrength.good;
    return PasswordStrength.strong;
  }

  @override
  Widget build(BuildContext context) {
    if (widget.password.isEmpty) {
      return const SizedBox.shrink();
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _buildStrengthMeter(),
        if (widget.showTips) ...[
          const SizedBox(height: AppSpacing.md),
          _buildRequirements(),
        ],
      ],
    );
  }

  Widget _buildStrengthMeter() {
    final config = _getStrengthConfig(_currentStrength);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Text(
              'Seguridad: ',
              style: AppTypography.labelSmall.copyWith(
                color: AppColors.textSecondary,
              ),
            ),
            Text(
              config.label,
              style: AppTypography.labelSmall.copyWith(
                color: config.color,
                fontWeight: FontWeight.w600,
              ),
            ),
          ],
        ),
        const SizedBox(height: AppSpacing.xs),
        ClipRRect(
          borderRadius: BorderRadius.circular(4),
          child: SizedBox(
            height: 8,
            child: Stack(
              children: [
                // Background
                Container(
                  color: Colors.grey.shade200,
                ),
                // Progress
                AnimatedBuilder(
                  animation: _progressAnimation,
                  builder: (context, child) {
                    return FractionallySizedBox(
                      alignment: Alignment.centerLeft,
                      widthFactor: config.progress * _progressAnimation.value,
                      child: Container(
                        decoration: BoxDecoration(
                          gradient: LinearGradient(
                            colors: [
                              config.color,
                              config.color.withValues(alpha: 0.7),
                            ],
                          ),
                        ),
                      ),
                    );
                  },
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildRequirements() {
    final password = widget.password;
    final hasMinLength = password.length >= 8;
    final hasUppercase = password.contains(RegExp(r'[A-Z]'));
    final hasLowercase = password.contains(RegExp(r'[a-z]'));
    final hasNumber = password.contains(RegExp(r'[0-9]'));
    final hasSpecial = password.contains(RegExp(r'[!@#$%^&*(),.?":{}|<>]'));

    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.backgroundSecondary,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.border),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Tu contraseña debe tener:',
            style: AppTypography.labelSmall.copyWith(
              color: AppColors.textSecondary,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: AppSpacing.sm),
          _buildRequirement('Mínimo 8 caracteres', hasMinLength),
          _buildRequirement('Una letra mayúscula (A-Z)', hasUppercase),
          _buildRequirement('Una letra minúscula (a-z)', hasLowercase),
          _buildRequirement('Un número (0-9)', hasNumber),
          _buildRequirement('Un carácter especial (!@#\$%...)', hasSpecial),
        ],
      ),
    );
  }

  Widget _buildRequirement(String text, bool isMet) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.xs),
      child: Row(
        children: [
          AnimatedSwitcher(
            duration: const Duration(milliseconds: 200),
            child: Icon(
              isMet ? Icons.check_circle : Icons.circle_outlined,
              key: ValueKey(isMet),
              size: 16,
              color: isMet ? AppColors.success : AppColors.textSecondary,
            ),
          ),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Text(
              text,
              style: AppTypography.bodySmall.copyWith(
                color: isMet ? AppColors.textPrimary : AppColors.textSecondary,
                decoration: isMet ? TextDecoration.lineThrough : null,
              ),
            ),
          ),
        ],
      ),
    );
  }

  _PasswordStrengthConfig _getStrengthConfig(PasswordStrength strength) {
    switch (strength) {
      case PasswordStrength.weak:
        return _PasswordStrengthConfig(
          label: 'Débil',
          color: AppColors.error,
          progress: 0.25,
        );
      case PasswordStrength.fair:
        return _PasswordStrengthConfig(
          label: 'Regular',
          color: AppColors.warning,
          progress: 0.5,
        );
      case PasswordStrength.good:
        return _PasswordStrengthConfig(
          label: 'Buena',
          color: AppColors.info,
          progress: 0.75,
        );
      case PasswordStrength.strong:
        return _PasswordStrengthConfig(
          label: 'Fuerte',
          color: AppColors.success,
          progress: 1.0,
        );
    }
  }
}

class _PasswordStrengthConfig {
  final String label;
  final Color color;
  final double progress;

  _PasswordStrengthConfig({
    required this.label,
    required this.color,
    required this.progress,
  });
}

/// Password Field con indicador de fuerza integrado
class PasswordFieldWithStrength extends StatefulWidget {
  final TextEditingController controller;
  final String labelText;
  final String? hintText;
  final FormFieldValidator<String>? validator;
  final bool showStrengthIndicator;
  final bool showRequirements;

  const PasswordFieldWithStrength({
    super.key,
    required this.controller,
    this.labelText = 'Contraseña',
    this.hintText = '••••••••',
    this.validator,
    this.showStrengthIndicator = true,
    this.showRequirements = true,
  });

  @override
  State<PasswordFieldWithStrength> createState() =>
      _PasswordFieldWithStrengthState();
}

class _PasswordFieldWithStrengthState extends State<PasswordFieldWithStrength> {
  bool _obscureText = true;
  String _password = '';

  @override
  void initState() {
    super.initState();
    widget.controller.addListener(_onPasswordChanged);
  }

  @override
  void dispose() {
    widget.controller.removeListener(_onPasswordChanged);
    super.dispose();
  }

  void _onPasswordChanged() {
    setState(() {
      _password = widget.controller.text;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        TextFormField(
          controller: widget.controller,
          obscureText: _obscureText,
          decoration: InputDecoration(
            labelText: widget.labelText,
            hintText: widget.hintText,
            prefixIcon: const Icon(
              Icons.lock_outlined,
              color: AppColors.primary,
            ),
            suffixIcon: IconButton(
              icon: Icon(
                _obscureText ? Icons.visibility_off : Icons.visibility,
                color: AppColors.textSecondary,
              ),
              onPressed: () {
                setState(() {
                  _obscureText = !_obscureText;
                });
              },
            ),
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: AppColors.border),
            ),
            enabledBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: AppColors.border),
            ),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: AppColors.primary, width: 2),
            ),
            errorBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: AppColors.error),
            ),
            filled: true,
            fillColor: AppColors.backgroundSecondary,
          ),
          validator: widget.validator,
        ),
        if (widget.showStrengthIndicator) ...[
          const SizedBox(height: AppSpacing.sm),
          PasswordStrengthIndicator(
            password: _password,
            showTips: widget.showRequirements,
          ),
        ],
      ],
    );
  }
}
