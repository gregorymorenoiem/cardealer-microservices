import 'package:flutter/material.dart';
import '../../core/theme/colors.dart';
import '../../core/theme/spacing.dart';
import '../../core/theme/typography.dart';

/// Featured badge overlay
class FeaturedBadge extends StatelessWidget {
  final String text;
  final BadgeSize size;

  const FeaturedBadge({
    super.key,
    this.text = 'DESTACADO',
    this.size = BadgeSize.medium,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: _getPadding(),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [
            AppColors.featuredGradientStart,
            AppColors.featuredGradientEnd,
          ],
        ),
        borderRadius: BorderRadius.circular(_getBorderRadius()),
        boxShadow: [
          BoxShadow(
            color: AppColors.shadowMedium,
            blurRadius: 4,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Text(text, style: _getTextStyle()),
    );
  }

  EdgeInsets _getPadding() {
    switch (size) {
      case BadgeSize.small:
        return const EdgeInsets.symmetric(horizontal: 6, vertical: 2);
      case BadgeSize.medium:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.sm,
          vertical: AppSpacing.xs,
        );
      case BadgeSize.large:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.md,
          vertical: AppSpacing.sm,
        );
    }
  }

  double _getBorderRadius() {
    switch (size) {
      case BadgeSize.small:
        return AppSpacing.radiusXs;
      case BadgeSize.medium:
        return AppSpacing.radiusSm;
      case BadgeSize.large:
        return AppSpacing.radiusMd;
    }
  }

  TextStyle _getTextStyle() {
    switch (size) {
      case BadgeSize.small:
        return AppTypography.overline.copyWith(
          color: AppColors.textOnPrimary,
          fontSize: 8,
          fontWeight: FontWeight.w700,
        );
      case BadgeSize.medium:
        return AppTypography.labelSmall.copyWith(
          color: AppColors.textOnPrimary,
          fontWeight: FontWeight.w700,
        );
      case BadgeSize.large:
        return AppTypography.labelMedium.copyWith(
          color: AppColors.textOnPrimary,
          fontWeight: FontWeight.w700,
        );
    }
  }
}

enum BadgeSize { small, medium, large }
