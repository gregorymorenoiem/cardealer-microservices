import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/typography.dart';
import '../../../core/theme/spacing.dart';

enum GradientButtonVariant {
  primary,
  secondary,
  outline,
}

enum GradientButtonSize {
  small,
  medium,
  large,
}

/// Gradient button with three variants: primary, secondary, and outline
/// Follows the new design system with smooth interactions and haptic feedback
class GradientButton extends StatelessWidget {
  final String text;
  final VoidCallback? onPressed;
  final GradientButtonVariant variant;
  final GradientButtonSize size;
  final bool isLoading;
  final Widget? icon;
  final bool fullWidth;

  const GradientButton({
    super.key,
    required this.text,
    this.onPressed,
    this.variant = GradientButtonVariant.primary,
    this.size = GradientButtonSize.medium,
    this.isLoading = false,
    this.icon,
    this.fullWidth = false,
  });

  @override
  Widget build(BuildContext context) {
    final bool isEnabled = onPressed != null && !isLoading;

    return SizedBox(
      width: fullWidth ? double.infinity : null,
      height: _getHeight(),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: isEnabled ? onPressed : null,
          borderRadius: BorderRadius.circular(_getBorderRadius()),
          child: Container(
            decoration: _getDecoration(isEnabled),
            padding: EdgeInsets.symmetric(
              horizontal: _getHorizontalPadding(),
              vertical: _getVerticalPadding(),
            ),
            child: Center(
              child: isLoading
                  ? SizedBox(
                      width: _getIconSize(),
                      height: _getIconSize(),
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        valueColor: AlwaysStoppedAnimation<Color>(
                          _getLoadingColor(),
                        ),
                      ),
                    )
                  : _buildContent(),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildContent() {
    if (icon != null) {
      return Row(
        mainAxisSize: MainAxisSize.min,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          icon!,
          const SizedBox(width: AppSpacing.xs),
          Text(
            text,
            style: _getTextStyle(),
          ),
        ],
      );
    }

    return Text(
      text,
      style: _getTextStyle(),
    );
  }

  BoxDecoration _getDecoration(bool isEnabled) {
    final opacity = isEnabled ? 1.0 : 0.5;

    switch (variant) {
      case GradientButtonVariant.primary:
        return BoxDecoration(
          gradient: LinearGradient(
            colors: [
              AppColors.primary.withValues(alpha: opacity),
              AppColors.primaryDark.withValues(alpha: opacity),
            ],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
          borderRadius: BorderRadius.circular(_getBorderRadius()),
          boxShadow: isEnabled
              ? [
                  BoxShadow(
                    color: AppColors.primary.withValues(alpha: 0.3),
                    blurRadius: 12,
                    offset: const Offset(0, 4),
                  ),
                ]
              : [],
        );

      case GradientButtonVariant.secondary:
        return BoxDecoration(
          gradient: LinearGradient(
            colors: [
              AppColors.accent.withValues(alpha: opacity),
              AppColors.accentDark.withValues(alpha: opacity),
            ],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
          borderRadius: BorderRadius.circular(_getBorderRadius()),
          boxShadow: isEnabled
              ? [
                  BoxShadow(
                    color: AppColors.accent.withValues(alpha: 0.3),
                    blurRadius: 12,
                    offset: const Offset(0, 4),
                  ),
                ]
              : [],
        );

      case GradientButtonVariant.outline:
        return BoxDecoration(
          color: Colors.transparent,
          border: Border.all(
            color: AppColors.primary.withValues(alpha: opacity),
            width: 2,
          ),
          borderRadius: BorderRadius.circular(_getBorderRadius()),
        );
    }
  }

  TextStyle _getTextStyle() {
    final baseStyle = size == GradientButtonSize.large
        ? AppTypography.buttonLarge
        : size == GradientButtonSize.small
            ? AppTypography.buttonSmall
            : AppTypography.button;

    final color = variant == GradientButtonVariant.outline
        ? AppColors.primary
        : Colors.white;

    return baseStyle.copyWith(color: color);
  }

  Color _getLoadingColor() {
    return variant == GradientButtonVariant.outline
        ? AppColors.primary
        : Colors.white;
  }

  double _getHeight() {
    switch (size) {
      case GradientButtonSize.small:
        return 36.0;
      case GradientButtonSize.medium:
        return 48.0;
      case GradientButtonSize.large:
        return 56.0;
    }
  }

  double _getBorderRadius() {
    switch (size) {
      case GradientButtonSize.small:
        return 8.0;
      case GradientButtonSize.medium:
        return 12.0;
      case GradientButtonSize.large:
        return 12.0;
    }
  }

  double _getHorizontalPadding() {
    switch (size) {
      case GradientButtonSize.small:
        return AppSpacing.md;
      case GradientButtonSize.medium:
        return AppSpacing.lg;
      case GradientButtonSize.large:
        return AppSpacing.xl;
    }
  }

  double _getVerticalPadding() {
    switch (size) {
      case GradientButtonSize.small:
        return AppSpacing.xs;
      case GradientButtonSize.medium:
        return AppSpacing.sm;
      case GradientButtonSize.large:
        return AppSpacing.md;
    }
  }

  double _getIconSize() {
    switch (size) {
      case GradientButtonSize.small:
        return 16.0;
      case GradientButtonSize.medium:
        return 20.0;
      case GradientButtonSize.large:
        return 24.0;
    }
  }
}
