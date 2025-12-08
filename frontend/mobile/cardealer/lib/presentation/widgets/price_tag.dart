import 'package:flutter/material.dart';
import '../../core/theme/colors.dart';
import '../../core/theme/spacing.dart';
import '../../core/theme/typography.dart';
import '../../core/utils/formatters.dart';

enum PriceSize {
  small,
  medium,
  large,
  xlarge,
}

class PriceTag extends StatelessWidget {
  final double price;
  final PriceSize size;
  final bool showCurrency;
  final String? currency;
  final Color? color;
  final bool bold;
  final double? originalPrice; // For showing discount
  final String? period; // e.g., "/mes", "/día"

  const PriceTag({
    Key? key,
    required this.price,
    this.size = PriceSize.medium,
    this.showCurrency = true,
    this.currency,
    this.color,
    this.bold = false,
    this.originalPrice,
    this.period,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final priceColor = color ?? AppColors.textPrimary;
    final formattedPrice = Formatters.formatPrice(
      price,
      symbol: showCurrency ? (currency ?? '\$') : '',
    );

    if (originalPrice != null) {
      // Show with discount
      return _buildDiscountPrice(context, formattedPrice, priceColor);
    }

    return _buildSimplePrice(formattedPrice, priceColor);
  }

  Widget _buildSimplePrice(String formattedPrice, Color priceColor) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      crossAxisAlignment: CrossAxisAlignment.end,
      children: [
        Text(
          formattedPrice,
          style: _getTextStyle().copyWith(
            color: priceColor,
            fontWeight: bold ? FontWeight.w700 : FontWeight.w600,
          ),
        ),
        if (period != null) ...[
          SizedBox(width: AppSpacing.xs / 2),
          Text(
            period!,
            style: _getPeriodTextStyle().copyWith(
              color: priceColor.withOpacity(0.7),
            ),
          ),
        ],
      ],
    );
  }

  Widget _buildDiscountPrice(
    BuildContext context,
    String formattedPrice,
    Color priceColor,
  ) {
    final formattedOriginalPrice = Formatters.formatPrice(
      originalPrice!,
      symbol: showCurrency ? (currency ?? '\$') : '',
    );

    final discountPercent =
        ((originalPrice! - price) / originalPrice! * 100).round();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      mainAxisSize: MainAxisSize.min,
      children: [
        // Current price
        Row(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.end,
          children: [
            Text(
              formattedPrice,
              style: _getTextStyle().copyWith(
                color: AppColors.error,
                fontWeight: FontWeight.w700,
              ),
            ),
            if (period != null) ...[
              SizedBox(width: AppSpacing.xs / 2),
              Text(
                period!,
                style: _getPeriodTextStyle().copyWith(
                  color: AppColors.error.withOpacity(0.7),
                ),
              ),
            ],
          ],
        ),
        SizedBox(height: AppSpacing.xs / 2),
        // Original price + discount badge
        Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              formattedOriginalPrice,
              style: _getOriginalPriceTextStyle().copyWith(
                color: AppColors.textTertiary,
                decoration: TextDecoration.lineThrough,
              ),
            ),
            SizedBox(width: AppSpacing.xs),
            Container(
              padding: EdgeInsets.symmetric(
                horizontal: AppSpacing.xs,
                vertical: 2,
              ),
              decoration: BoxDecoration(
                color: AppColors.error.withOpacity(0.1),
                borderRadius: BorderRadius.circular(4),
              ),
              child: Text(
                '-$discountPercent%',
                style: AppTypography.caption.copyWith(
                  color: AppColors.error,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ),
          ],
        ),
      ],
    );
  }

  TextStyle _getTextStyle() {
    switch (size) {
      case PriceSize.small:
        return AppTypography.bodyMedium;
      case PriceSize.medium:
        return AppTypography.h6;
      case PriceSize.large:
        return AppTypography.h4;
      case PriceSize.xlarge:
        return AppTypography.h2;
    }
  }

  TextStyle _getPeriodTextStyle() {
    switch (size) {
      case PriceSize.small:
        return AppTypography.caption;
      case PriceSize.medium:
        return AppTypography.bodySmall;
      case PriceSize.large:
        return AppTypography.bodyMedium;
      case PriceSize.xlarge:
        return AppTypography.h6;
    }
  }

  TextStyle _getOriginalPriceTextStyle() {
    switch (size) {
      case PriceSize.small:
        return AppTypography.caption;
      case PriceSize.medium:
        return AppTypography.bodySmall;
      case PriceSize.large:
        return AppTypography.bodyMedium;
      case PriceSize.xlarge:
        return AppTypography.h6;
    }
  }
}

/// Price range widget (e.g., "$10,000 - $50,000")
class PriceRange extends StatelessWidget {
  final double minPrice;
  final double maxPrice;
  final PriceSize size;
  final String? currency;
  final Color? color;

  const PriceRange({
    Key? key,
    required this.minPrice,
    required this.maxPrice,
    this.size = PriceSize.medium,
    this.currency,
    this.color,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final priceColor = color ?? AppColors.textPrimary;
    final minFormatted = Formatters.formatPrice(
      minPrice,
      symbol: currency ?? '\$',
    );
    final maxFormatted = Formatters.formatPrice(
      maxPrice,
      symbol: currency ?? '\$',
    );

    return Text(
      '$minFormatted - $maxFormatted',
      style: _getTextStyle().copyWith(
        color: priceColor,
        fontWeight: FontWeight.w600,
      ),
    );
  }

  TextStyle _getTextStyle() {
    switch (size) {
      case PriceSize.small:
        return AppTypography.bodyMedium;
      case PriceSize.medium:
        return AppTypography.h6;
      case PriceSize.large:
        return AppTypography.h4;
      case PriceSize.xlarge:
        return AppTypography.h2;
    }
  }
}

/// Price with badge/label (e.g., "From $10,000", "Starting at $15,000")
class PriceLabelTag extends StatelessWidget {
  final String label;
  final double price;
  final PriceSize size;
  final String? currency;
  final Color? labelColor;
  final Color? priceColor;

  const PriceLabelTag({
    Key? key,
    required this.label,
    required this.price,
    this.size = PriceSize.medium,
    this.currency,
    this.labelColor,
    this.priceColor,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final formattedPrice = Formatters.formatPrice(
      price,
      symbol: currency ?? '\$',
    );

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(
          label,
          style: _getLabelTextStyle().copyWith(
            color: labelColor ?? AppColors.textSecondary,
          ),
        ),
        Text(
          formattedPrice,
          style: _getPriceTextStyle().copyWith(
            color: priceColor ?? AppColors.textPrimary,
            fontWeight: FontWeight.w700,
          ),
        ),
      ],
    );
  }

  TextStyle _getLabelTextStyle() {
    switch (size) {
      case PriceSize.small:
        return AppTypography.caption;
      case PriceSize.medium:
        return AppTypography.bodySmall;
      case PriceSize.large:
        return AppTypography.bodyMedium;
      case PriceSize.xlarge:
        return AppTypography.h6;
    }
  }

  TextStyle _getPriceTextStyle() {
    switch (size) {
      case PriceSize.small:
        return AppTypography.bodyMedium;
      case PriceSize.medium:
        return AppTypography.h6;
      case PriceSize.large:
        return AppTypography.h4;
      case PriceSize.xlarge:
        return AppTypography.h2;
    }
  }
}

/// Contact for price widget
class ContactForPrice extends StatelessWidget {
  final PriceSize size;
  final Color? color;
  final VoidCallback? onTap;

  const ContactForPrice({
    Key? key,
    this.size = PriceSize.medium,
    this.color,
    this.onTap,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final textColor = color ?? AppColors.primary;

    return GestureDetector(
      onTap: onTap,
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            Icons.phone_outlined,
            size: _getIconSize(),
            color: textColor,
          ),
          SizedBox(width: AppSpacing.xs),
          Text(
            'Consultar precio',
            style: _getTextStyle().copyWith(
              color: textColor,
              fontWeight: FontWeight.w600,
              decoration: onTap != null ? TextDecoration.underline : null,
            ),
          ),
        ],
      ),
    );
  }

  double _getIconSize() {
    switch (size) {
      case PriceSize.small:
        return 16;
      case PriceSize.medium:
        return 20;
      case PriceSize.large:
        return 24;
      case PriceSize.xlarge:
        return 28;
    }
  }

  TextStyle _getTextStyle() {
    switch (size) {
      case PriceSize.small:
        return AppTypography.bodySmall;
      case PriceSize.medium:
        return AppTypography.bodyMedium;
      case PriceSize.large:
        return AppTypography.h6;
      case PriceSize.xlarge:
        return AppTypography.h5;
    }
  }
}
