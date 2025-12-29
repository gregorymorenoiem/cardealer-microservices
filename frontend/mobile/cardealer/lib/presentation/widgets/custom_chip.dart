import 'package:flutter/material.dart';
import '../../core/theme/colors.dart';
import '../../core/theme/spacing.dart';
import '../../core/theme/typography.dart';

enum ChipSize {
  small,
  medium,
  large,
}

enum ChipVariant {
  filled,
  outlined,
  soft,
}

class CustomChip extends StatelessWidget {
  final String label;
  final ChipSize size;
  final ChipVariant variant;
  final Color? color;
  final IconData? icon;
  final VoidCallback? onTap;
  final VoidCallback? onDelete;
  final bool selected;

  const CustomChip({
    super.key,
    required this.label,
    this.size = ChipSize.medium,
    this.variant = ChipVariant.filled,
    this.color,
    this.icon,
    this.onTap,
    this.onDelete,
    this.selected = false,
  });

  @override
  Widget build(BuildContext context) {
    final chipColor = color ?? AppColors.primary;

    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: _getPadding(),
        decoration: _getDecoration(chipColor),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            if (icon != null) ...[
              Icon(
                icon,
                size: _getIconSize(),
                color: _getContentColor(chipColor),
              ),
              const SizedBox(width: AppSpacing.xs),
            ],
            Text(
              label,
              style: _getTextStyle().copyWith(
                color: _getContentColor(chipColor),
              ),
            ),
            if (onDelete != null) ...[
              const SizedBox(width: AppSpacing.xs),
              GestureDetector(
                onTap: onDelete,
                child: Icon(
                  Icons.close,
                  size: _getIconSize(),
                  color: _getContentColor(chipColor),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  EdgeInsets _getPadding() {
    switch (size) {
      case ChipSize.small:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.sm,
          vertical: AppSpacing.xs / 2,
        );
      case ChipSize.medium:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.md,
          vertical: AppSpacing.xs,
        );
      case ChipSize.large:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.lg,
          vertical: AppSpacing.sm,
        );
    }
  }

  double _getIconSize() {
    switch (size) {
      case ChipSize.small:
        return 14;
      case ChipSize.medium:
        return 16;
      case ChipSize.large:
        return 18;
    }
  }

  TextStyle _getTextStyle() {
    switch (size) {
      case ChipSize.small:
        return AppTypography.caption;
      case ChipSize.medium:
        return AppTypography.bodySmall;
      case ChipSize.large:
        return AppTypography.bodyMedium;
    }
  }

  BoxDecoration _getDecoration(Color chipColor) {
    switch (variant) {
      case ChipVariant.filled:
        return BoxDecoration(
          color: selected ? chipColor : chipColor.withValues(alpha: 0.9),
          borderRadius: BorderRadius.circular(100),
        );
      case ChipVariant.outlined:
        return BoxDecoration(
          color:
              selected ? chipColor.withValues(alpha: 0.1) : Colors.transparent,
          border: Border.all(
            color: chipColor,
            width: selected ? 2 : 1,
          ),
          borderRadius: BorderRadius.circular(100),
        );
      case ChipVariant.soft:
        return BoxDecoration(
          color: chipColor.withValues(alpha: 0.15),
          borderRadius: BorderRadius.circular(100),
        );
    }
  }

  Color _getContentColor(Color chipColor) {
    switch (variant) {
      case ChipVariant.filled:
        return Colors.white;
      case ChipVariant.outlined:
      case ChipVariant.soft:
        return chipColor;
    }
  }
}

/// Badge widget for notifications, counts, status
class CustomBadge extends StatelessWidget {
  final String? label;
  final int? count;
  final Color? color;
  final BadgeSize size;
  final Widget? child;

  const CustomBadge({
    super.key,
    this.label,
    this.count,
    this.color,
    this.size = BadgeSize.medium,
    this.child,
  });

  @override
  Widget build(BuildContext context) {
    final badgeColor = color ?? AppColors.error;
    final displayText =
        count != null ? (count! > 99 ? '99+' : '$count') : label ?? '';

    if (child != null) {
      // Badge with child (positioned)
      return Stack(
        clipBehavior: Clip.none,
        children: [
          child!,
          if (displayText.isNotEmpty)
            Positioned(
              right: -4,
              top: -4,
              child: _buildBadge(badgeColor, displayText),
            ),
        ],
      );
    }

    // Standalone badge
    return _buildBadge(badgeColor, displayText);
  }

  Widget _buildBadge(Color badgeColor, String text) {
    return Container(
      padding: _getPadding(),
      decoration: BoxDecoration(
        color: badgeColor,
        shape: size == BadgeSize.small && text.length <= 2
            ? BoxShape.circle
            : BoxShape.rectangle,
        borderRadius: size == BadgeSize.small && text.length <= 2
            ? null
            : BorderRadius.circular(100),
      ),
      constraints: BoxConstraints(
        minWidth: _getMinSize(),
        minHeight: _getMinSize(),
      ),
      child: Center(
        child: Text(
          text,
          style: _getTextStyle().copyWith(
            color: Colors.white,
          ),
          textAlign: TextAlign.center,
        ),
      ),
    );
  }

  EdgeInsets _getPadding() {
    switch (size) {
      case BadgeSize.small:
        return const EdgeInsets.all(2);
      case BadgeSize.medium:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.xs,
          vertical: 2,
        );
      case BadgeSize.large:
        return const EdgeInsets.symmetric(
          horizontal: AppSpacing.sm,
          vertical: AppSpacing.xs / 2,
        );
    }
  }

  double _getMinSize() {
    switch (size) {
      case BadgeSize.small:
        return 16;
      case BadgeSize.medium:
        return 20;
      case BadgeSize.large:
        return 24;
    }
  }

  TextStyle _getTextStyle() {
    switch (size) {
      case BadgeSize.small:
        return AppTypography.caption.copyWith(fontSize: 10);
      case BadgeSize.medium:
        return AppTypography.caption;
      case BadgeSize.large:
        return AppTypography.bodySmall;
    }
  }
}

enum BadgeSize {
  small,
  medium,
  large,
}

/// Tag widget for categories, filters
class CustomTag extends StatelessWidget {
  final String label;
  final Color? backgroundColor;
  final Color? textColor;
  final bool removable;
  final VoidCallback? onRemove;

  const CustomTag({
    super.key,
    required this.label,
    this.backgroundColor,
    this.textColor,
    this.removable = false,
    this.onRemove,
  });

  @override
  Widget build(BuildContext context) {
    final bgColor = backgroundColor ?? AppColors.surfaceVariant;
    final txColor = textColor ?? AppColors.textPrimary;

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.sm,
        vertical: AppSpacing.xs / 2,
      ),
      decoration: BoxDecoration(
        color: bgColor,
        borderRadius: BorderRadius.circular(4),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            label,
            style: AppTypography.caption.copyWith(
              color: txColor,
              fontWeight: FontWeight.w600,
            ),
          ),
          if (removable && onRemove != null) ...[
            const SizedBox(width: AppSpacing.xs / 2),
            GestureDetector(
              onTap: onRemove,
              child: Icon(
                Icons.close,
                size: 14,
                color: txColor,
              ),
            ),
          ],
        ],
      ),
    );
  }
}
