import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';

/// Social Login Buttons Widget - Sprint 7 AE-002
/// Botones premium para Google, Apple y Facebook Sign-In
/// Features:
/// - Diseño consistente con branding de cada plataforma
/// - Animaciones de hover y tap
/// - Estados de loading
/// - Feedback visual mejorado

enum SocialLoginProvider {
  google,
  apple,
  facebook,
}

enum SocialLoginButtonLayout {
  stacked,
  row,
}

class SocialLoginButtons extends StatelessWidget {
  final VoidCallback onGooglePressed;
  final VoidCallback onApplePressed;
  final VoidCallback onFacebookPressed;
  final bool isLoading;
  final SocialLoginButtonLayout layout;

  const SocialLoginButtons({
    super.key,
    required this.onGooglePressed,
    required this.onApplePressed,
    required this.onFacebookPressed,
    this.isLoading = false,
    this.layout = SocialLoginButtonLayout.stacked,
  });

  @override
  Widget build(BuildContext context) {
    if (layout == SocialLoginButtonLayout.stacked) {
      return Column(
        children: [
          SocialLoginButton(
            provider: SocialLoginProvider.google,
            onPressed: isLoading ? null : onGooglePressed,
          ),
          const SizedBox(height: AppSpacing.md),
          SocialLoginButton(
            provider: SocialLoginProvider.apple,
            onPressed: isLoading ? null : onApplePressed,
          ),
          const SizedBox(height: AppSpacing.md),
          SocialLoginButton(
            provider: SocialLoginProvider.facebook,
            onPressed: isLoading ? null : onFacebookPressed,
          ),
        ],
      );
    }

    // Row layout
    return Row(
      children: [
        Expanded(
          child: SocialLoginButton(
            provider: SocialLoginProvider.google,
            onPressed: isLoading ? null : onGooglePressed,
            compact: true,
          ),
        ),
        const SizedBox(width: AppSpacing.sm),
        Expanded(
          child: SocialLoginButton(
            provider: SocialLoginProvider.apple,
            onPressed: isLoading ? null : onApplePressed,
            compact: true,
          ),
        ),
        const SizedBox(width: AppSpacing.sm),
        Expanded(
          child: SocialLoginButton(
            provider: SocialLoginProvider.facebook,
            onPressed: isLoading ? null : onFacebookPressed,
            compact: true,
          ),
        ),
      ],
    );
  }
}

/// Individual Social Login Button
class SocialLoginButton extends StatefulWidget {
  final SocialLoginProvider provider;
  final VoidCallback? onPressed;
  final bool compact;

  const SocialLoginButton({
    super.key,
    required this.provider,
    required this.onPressed,
    this.compact = false,
  });

  @override
  State<SocialLoginButton> createState() => _SocialLoginButtonState();
}

class _SocialLoginButtonState extends State<SocialLoginButton>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _scaleAnimation;
  bool _isPressed = false;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 100),
    );
    _scaleAnimation = Tween<double>(begin: 1.0, end: 0.95).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeInOut),
    );
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _handleTapDown(TapDownDetails details) {
    setState(() => _isPressed = true);
    _controller.forward();
  }

  void _handleTapUp(TapUpDetails details) {
    setState(() => _isPressed = false);
    _controller.reverse();
  }

  void _handleTapCancel() {
    setState(() => _isPressed = false);
    _controller.reverse();
  }

  @override
  Widget build(BuildContext context) {
    final config = _getProviderConfig(widget.provider);
    final isEnabled = widget.onPressed != null;

    return GestureDetector(
      onTapDown: isEnabled ? _handleTapDown : null,
      onTapUp: isEnabled ? _handleTapUp : null,
      onTapCancel: isEnabled ? _handleTapCancel : null,
      child: ScaleTransition(
        scale: _scaleAnimation,
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 200),
          width: double.infinity,
          height: widget.compact ? 48 : 56,
          child: ElevatedButton(
            onPressed: widget.onPressed,
            style: ElevatedButton.styleFrom(
              backgroundColor: config.backgroundColor,
              foregroundColor: config.textColor,
              elevation: _isPressed ? 1 : 2,
              shadowColor: Colors.black.withValues(alpha: 0.1),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
                side: config.borderColor != null
                    ? BorderSide(color: config.borderColor!)
                    : BorderSide.none,
              ),
              disabledBackgroundColor: Colors.grey.shade200,
              disabledForegroundColor: Colors.grey.shade500,
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(
                  config.icon,
                  color: isEnabled ? config.iconColor : Colors.grey.shade500,
                  size: widget.compact ? 20 : 24,
                ),
                if (!widget.compact) ...[
                  const SizedBox(width: AppSpacing.sm),
                  Flexible(
                    child: Text(
                      config.label,
                      style: AppTypography.labelLarge.copyWith(
                        color:
                            isEnabled ? config.textColor : Colors.grey.shade500,
                        fontWeight: FontWeight.w600,
                        fontSize: widget.compact ? 14 : 16,
                      ),
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }

  _SocialLoginConfig _getProviderConfig(SocialLoginProvider provider) {
    switch (provider) {
      case SocialLoginProvider.google:
        return _SocialLoginConfig(
          icon: Icons.g_mobiledata,
          label: 'Continuar con Google',
          backgroundColor: Colors.white,
          textColor: AppColors.textPrimary,
          iconColor: const Color(0xFFDB4437), // Google Red
          borderColor: AppColors.border,
        );
      case SocialLoginProvider.apple:
        return _SocialLoginConfig(
          icon: Icons.apple,
          label: 'Continuar con Apple',
          backgroundColor: Colors.black,
          textColor: Colors.white,
          iconColor: Colors.white,
        );
      case SocialLoginProvider.facebook:
        return _SocialLoginConfig(
          icon: Icons.facebook,
          label: 'Continuar con Facebook',
          backgroundColor: const Color(0xFF1877F2), // Facebook Blue
          textColor: Colors.white,
          iconColor: Colors.white,
        );
    }
  }
}

class _SocialLoginConfig {
  final IconData icon;
  final String label;
  final Color backgroundColor;
  final Color textColor;
  final Color iconColor;
  final Color? borderColor;

  _SocialLoginConfig({
    required this.icon,
    required this.label,
    required this.backgroundColor,
    required this.textColor,
    required this.iconColor,
    this.borderColor,
  });
}

/// Divider con texto "O continúa con"
class SocialLoginDivider extends StatelessWidget {
  final String text;

  const SocialLoginDivider({
    super.key,
    this.text = 'O continúa con',
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        const Expanded(child: Divider()),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: AppSpacing.md),
          child: Text(
            text,
            style: AppTypography.labelMedium.copyWith(
              color: AppColors.textSecondary,
            ),
          ),
        ),
        const Expanded(child: Divider()),
      ],
    );
  }
}
