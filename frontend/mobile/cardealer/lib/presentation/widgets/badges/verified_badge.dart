import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/typography.dart';
import '../../../core/theme/spacing.dart';

/// Verified badge with check icon for verified dealers and listings
/// Provides trust and credibility to users
class VerifiedBadge extends StatelessWidget {
  final String text;
  final VerifiedBadgeSize size;
  final VerifiedBadgeVariant variant;
  final EdgeInsets? padding;

  const VerifiedBadge({
    super.key,
    this.text = 'VERIFIED',
    this.size = VerifiedBadgeSize.medium,
    this.variant = VerifiedBadgeVariant.filled,
    this.padding,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: padding ?? _getDefaultPadding(),
      decoration: _getDecoration(),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            Icons.verified,
            size: _getIconSize(),
            color: _getIconColor(),
          ),
          const SizedBox(width: AppSpacing.xxs),
          Text(
            text,
            style: _getTextStyle(),
          ),
        ],
      ),
    );
  }

  BoxDecoration _getDecoration() {
    switch (variant) {
      case VerifiedBadgeVariant.filled:
        return BoxDecoration(
          color: AppColors.badgeVerified,
          borderRadius: BorderRadius.circular(_getBorderRadius()),
          boxShadow: [
            BoxShadow(
              color: AppColors.badgeVerified.withValues(alpha: 0.3),
              blurRadius: 6,
              offset: const Offset(0, 2),
            ),
          ],
        );
      case VerifiedBadgeVariant.outlined:
        return BoxDecoration(
          color: Colors.transparent,
          border: Border.all(
            color: AppColors.badgeVerified,
            width: 1.5,
          ),
          borderRadius: BorderRadius.circular(_getBorderRadius()),
        );
      case VerifiedBadgeVariant.subtle:
        return BoxDecoration(
          color: AppColors.badgeVerified.withValues(alpha: 0.1),
          borderRadius: BorderRadius.circular(_getBorderRadius()),
        );
    }
  }

  EdgeInsets _getDefaultPadding() {
    switch (size) {
      case VerifiedBadgeSize.small:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.xs,
          vertical: AppSpacing.xxs,
        );
      case VerifiedBadgeSize.medium:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.sm,
          vertical: AppSpacing.xs,
        );
      case VerifiedBadgeSize.large:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.md,
          vertical: AppSpacing.sm,
        );
    }
  }

  double _getBorderRadius() {
    switch (size) {
      case VerifiedBadgeSize.small:
        return 4.0;
      case VerifiedBadgeSize.medium:
        return 6.0;
      case VerifiedBadgeSize.large:
        return 8.0;
    }
  }

  double _getIconSize() {
    switch (size) {
      case VerifiedBadgeSize.small:
        return 12.0;
      case VerifiedBadgeSize.medium:
        return 14.0;
      case VerifiedBadgeSize.large:
        return 16.0;
    }
  }

  Color _getIconColor() {
    switch (variant) {
      case VerifiedBadgeVariant.filled:
        return Colors.white;
      case VerifiedBadgeVariant.outlined:
      case VerifiedBadgeVariant.subtle:
        return AppColors.badgeVerified;
    }
  }

  TextStyle _getTextStyle() {
    final baseStyle = size == VerifiedBadgeSize.large
        ? AppTypography.labelLarge
        : size == VerifiedBadgeSize.small
            ? AppTypography.labelSmall
            : AppTypography.labelMedium;

    final color = variant == VerifiedBadgeVariant.filled
        ? Colors.white
        : AppColors.badgeVerified;

    return baseStyle.copyWith(
      color: color,
      fontWeight: FontWeight.w700, // Bold
      letterSpacing: 0.5,
    );
  }
}

enum VerifiedBadgeSize {
  small,
  medium,
  large,
}

enum VerifiedBadgeVariant {
  filled,
  outlined,
  subtle,
}
