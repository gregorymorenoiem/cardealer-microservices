import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/typography.dart';
import '../../../core/theme/spacing.dart';

/// Premium badge with gold gradient to highlight premium features
/// Used for premium listings, featured vehicles, and exclusive content
class PremiumBadge extends StatelessWidget {
  final String text;
  final PremiumBadgeSize size;
  final bool showIcon;
  final EdgeInsets? padding;

  const PremiumBadge({
    super.key,
    this.text = 'PREMIUM',
    this.size = PremiumBadgeSize.medium,
    this.showIcon = true,
    this.padding,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: padding ?? _getDefaultPadding(),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [
            AppColors.gold,
            AppColors.goldDark,
          ],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(_getBorderRadius()),
        boxShadow: [
          BoxShadow(
            color: AppColors.gold.withValues(alpha: 0.4),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          if (showIcon) ...[
            Icon(
              Icons.star,
              size: _getIconSize(),
              color: AppColors.textPrimary,
            ),
            const SizedBox(width: AppSpacing.xxs),
          ],
          Text(
            text,
            style: _getTextStyle(),
          ),
        ],
      ),
    );
  }

  EdgeInsets _getDefaultPadding() {
    switch (size) {
      case PremiumBadgeSize.small:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.xs,
          vertical: AppSpacing.xxs,
        );
      case PremiumBadgeSize.medium:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.sm,
          vertical: AppSpacing.xs,
        );
      case PremiumBadgeSize.large:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.md,
          vertical: AppSpacing.sm,
        );
    }
  }

  double _getBorderRadius() {
    switch (size) {
      case PremiumBadgeSize.small:
        return 4.0;
      case PremiumBadgeSize.medium:
        return 6.0;
      case PremiumBadgeSize.large:
        return 8.0;
    }
  }

  double _getIconSize() {
    switch (size) {
      case PremiumBadgeSize.small:
        return 12.0;
      case PremiumBadgeSize.medium:
        return 14.0;
      case PremiumBadgeSize.large:
        return 16.0;
    }
  }

  TextStyle _getTextStyle() {
    final baseStyle = size == PremiumBadgeSize.large
        ? AppTypography.labelLarge
        : size == PremiumBadgeSize.small
            ? AppTypography.labelSmall
            : AppTypography.labelMedium;

    return baseStyle.copyWith(
      color: AppColors.textPrimary,
      fontWeight: FontWeight.w700, // Bold
      letterSpacing: 0.5,
    );
  }
}

enum PremiumBadgeSize {
  small,
  medium,
  large,
}
