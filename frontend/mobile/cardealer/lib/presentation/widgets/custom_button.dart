import 'package:flutter/material.dart';
import '../../core/theme/colors.dart';
import '../../core/theme/spacing.dart';
import '../../core/theme/typography.dart';

/// Custom button component
/// Variants: primary, secondary, outline, text
class CustomButton extends StatelessWidget {
  final String text;
  final VoidCallback? onPressed;
  final ButtonVariant variant;
  final ButtonSize size;
  final bool isLoading;
  final bool isFullWidth;
  final IconData? icon;
  final Widget? child;

  const CustomButton({
    super.key,
    required this.text,
    this.onPressed,
    this.variant = ButtonVariant.primary,
    this.size = ButtonSize.medium,
    this.isLoading = false,
    this.isFullWidth = false,
    this.icon,
    this.child,
  });

  const CustomButton.primary({
    super.key,
    required this.text,
    this.onPressed,
    this.size = ButtonSize.medium,
    this.isLoading = false,
    this.isFullWidth = false,
    this.icon,
    this.child,
  }) : variant = ButtonVariant.primary;

  const CustomButton.secondary({
    super.key,
    required this.text,
    this.onPressed,
    this.size = ButtonSize.medium,
    this.isLoading = false,
    this.isFullWidth = false,
    this.icon,
    this.child,
  }) : variant = ButtonVariant.secondary;

  const CustomButton.outline({
    super.key,
    required this.text,
    this.onPressed,
    this.size = ButtonSize.medium,
    this.isLoading = false,
    this.isFullWidth = false,
    this.icon,
    this.child,
  }) : variant = ButtonVariant.outline;

  const CustomButton.text({
    super.key,
    required this.text,
    this.onPressed,
    this.size = ButtonSize.medium,
    this.isLoading = false,
    this.isFullWidth = false,
    this.icon,
    this.child,
  }) : variant = ButtonVariant.text;

  @override
  Widget build(BuildContext context) {
    final double height = _getHeight();
    final EdgeInsets padding = _getPadding();
    final TextStyle textStyle = _getTextStyle();

    Widget buttonChild = child ??
        LayoutBuilder(
          builder: (context, constraints) {
            return Row(
              mainAxisSize: isFullWidth ? MainAxisSize.max : MainAxisSize.min,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                if (isLoading)
                  SizedBox(
                    width: 16,
                    height: 16,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      valueColor:
                          AlwaysStoppedAnimation<Color>(_getLoadingColor()),
                    ),
                  )
                else if (icon != null)
                  Icon(icon, size: _getIconSize()),
                if ((isLoading || icon != null) && text.isNotEmpty)
                  SizedBox(width: AppSpacing.sm - 1),
                if (text.isNotEmpty)
                  Flexible(
                    child: Text(
                      text,
                      style: textStyle,
                      overflow: TextOverflow.ellipsis,
                      maxLines: 1,
                    ),
                  ),
              ],
            );
          },
        );

    Widget button;
    switch (variant) {
      case ButtonVariant.primary:
        button = ElevatedButton(
          onPressed: isLoading ? null : onPressed,
          style: ElevatedButton.styleFrom(
            backgroundColor: AppColors.primary,
            foregroundColor: AppColors.textOnPrimary,
            minimumSize: Size(isFullWidth ? double.infinity : 0, height),
            padding: padding,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(AppSpacing.radiusMd),
            ),
          ),
          child: buttonChild,
        );
        break;
      case ButtonVariant.secondary:
        button = ElevatedButton(
          onPressed: isLoading ? null : onPressed,
          style: ElevatedButton.styleFrom(
            backgroundColor: AppColors.secondary,
            foregroundColor: AppColors.textOnPrimary,
            minimumSize: Size(isFullWidth ? double.infinity : 0, height),
            padding: padding,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(AppSpacing.radiusMd),
            ),
          ),
          child: buttonChild,
        );
        break;
      case ButtonVariant.outline:
        button = OutlinedButton(
          onPressed: isLoading ? null : onPressed,
          style: OutlinedButton.styleFrom(
            foregroundColor: AppColors.primary,
            side: const BorderSide(
              color: AppColors.primary,
              width: AppSpacing.borderWidthMedium,
            ),
            minimumSize: Size(isFullWidth ? double.infinity : 0, height),
            padding: padding,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(AppSpacing.radiusMd),
            ),
          ),
          child: buttonChild,
        );
        break;
      case ButtonVariant.text:
        button = TextButton(
          onPressed: isLoading ? null : onPressed,
          style: TextButton.styleFrom(
            foregroundColor: AppColors.primary,
            minimumSize: Size(isFullWidth ? double.infinity : 0, height),
            padding: padding,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(AppSpacing.radiusSm),
            ),
          ),
          child: buttonChild,
        );
        break;
    }

    if (isFullWidth) {
      return SizedBox(width: double.infinity, child: button);
    }

    return button;
  }

  double _getHeight() {
    switch (size) {
      case ButtonSize.small:
        return AppSpacing.buttonHeightSm;
      case ButtonSize.medium:
        return AppSpacing.buttonHeightMd;
      case ButtonSize.large:
        return AppSpacing.buttonHeightLg;
    }
  }

  EdgeInsets _getPadding() {
    switch (size) {
      case ButtonSize.small:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.paddingMd,
          vertical: AppSpacing.paddingSm,
        );
      case ButtonSize.medium:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.paddingLg,
          vertical: AppSpacing.paddingMd,
        );
      case ButtonSize.large:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.paddingXl,
          vertical: AppSpacing.paddingMd,
        );
    }
  }

  TextStyle _getTextStyle() {
    switch (size) {
      case ButtonSize.small:
        return AppTypography.buttonSmall;
      case ButtonSize.medium:
        return AppTypography.button;
      case ButtonSize.large:
        return AppTypography.buttonLarge;
    }
  }

  double _getIconSize() {
    switch (size) {
      case ButtonSize.small:
        return AppSpacing.iconSm;
      case ButtonSize.medium:
        return AppSpacing.iconMd;
      case ButtonSize.large:
        return AppSpacing.iconMd;
    }
  }

  Color _getLoadingColor() {
    switch (variant) {
      case ButtonVariant.primary:
      case ButtonVariant.secondary:
        return AppColors.textOnPrimary;
      case ButtonVariant.outline:
      case ButtonVariant.text:
        return AppColors.primary;
    }
  }
}

enum ButtonVariant { primary, secondary, outline, text }

enum ButtonSize { small, medium, large }
