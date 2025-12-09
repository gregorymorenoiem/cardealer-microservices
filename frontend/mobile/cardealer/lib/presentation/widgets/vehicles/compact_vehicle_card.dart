import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/responsive/responsive_helper.dart';
import '../../../domain/entities/vehicle.dart';

/// Compact Vehicle Card - Optimized for monetization
/// Specifications:
/// - Total height: 180dp (down from 280dp)
/// - Photo ratio: 70% (126dp)
/// - Info ratio: 30% (54dp)
/// - Max 5 info fields
/// - Badge overlay on photo
/// - Swipeable gallery indicator
///
/// Based on: Cars.com, AutoTrader, CarGurus best practices
class CompactVehicleCard extends StatelessWidget {
  final Vehicle vehicle;
  final VoidCallback? onTap;
  final VoidCallback? onFavorite;
  final bool isFavorite;
  final bool isSponsored;
  final bool isFeatured;
  final String? badgeText;
  final Color? badgeColor;

  const CompactVehicleCard({
    super.key,
    required this.vehicle,
    this.onTap,
    this.onFavorite,
    this.isFavorite = false,
    this.isSponsored = false,
    this.isFeatured = false,
    this.badgeText,
    this.badgeColor,
  });

  @override
  Widget build(BuildContext context) {
    final responsive = context.responsive;

    return GestureDetector(
      onTap: onTap,
      child: Container(
        height: responsive.cardHeight, // Responsive height
        margin: EdgeInsets.symmetric(
          horizontal: responsive.horizontalPadding,
          vertical: AppSpacing.xs,
        ),
        decoration: BoxDecoration(
          color: Theme.of(context).cardColor,
          borderRadius: BorderRadius.circular(12),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.08),
              blurRadius: 8,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Column(
          children: [
            // Photo Section: 70%
            Expanded(
              flex: 7,
              child: _buildPhotoSection(context, responsive),
            ),
            // Info Section: 30%
            Expanded(
              flex: 3,
              child: _buildInfoSection(context, responsive),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildPhotoSection(BuildContext context, ResponsiveHelper responsive) {
    return Stack(
      children: [
        // Main Photo
        ClipRRect(
          borderRadius: BorderRadius.vertical(
              top: Radius.circular(responsive.borderRadius)),
          child: CachedNetworkImage(
            imageUrl: vehicle.images.isNotEmpty
                ? vehicle.images.first
                : 'https://via.placeholder.com/400x300?text=No+Image',
            fit: BoxFit.cover,
            width: double.infinity,
            height: double.infinity,
            memCacheHeight: 400, // Optimize memory
            maxHeightDiskCache: 800, // Optimize storage
            fadeInDuration: const Duration(milliseconds: 150),
            placeholder: (context, url) => Container(
              color: AppColors.surfaceVariant,
              child: const Center(
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
            ),
            errorWidget: (context, url, error) => Container(
              color: AppColors.surfaceVariant,
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(
                    Icons.directions_car,
                    size: 48,
                    color: AppColors.textTertiary,
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'Image not available',
                    style: TextStyle(
                      color: AppColors.textTertiary,
                      fontSize: 12,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),

        // Gradient overlay for better text visibility
        Positioned(
          left: 0,
          right: 0,
          bottom: 0,
          child: Container(
            height: 40,
            decoration: BoxDecoration(
              gradient: LinearGradient(
                begin: Alignment.topCenter,
                end: Alignment.bottomCenter,
                colors: [
                  Colors.transparent,
                  Colors.black.withValues(alpha: 0.3),
                ],
              ),
            ),
          ),
        ),

        // Badge Overlay (Top-Left)
        if (badgeText != null || isSponsored || isFeatured)
          Positioned(
            top: responsive.horizontalPadding * 0.5,
            left: responsive.horizontalPadding * 0.5,
            child: Container(
              padding: EdgeInsets.symmetric(
                horizontal: responsive.horizontalPadding * 0.5,
                vertical: 4,
              ),
              decoration: BoxDecoration(
                color: _getBadgeColor(),
                borderRadius: BorderRadius.circular(responsive.borderRadius * 0.33),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.2),
                    blurRadius: 4,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: Text(
                _getBadgeText(),
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 10,
                  fontWeight: FontWeight.bold,
                  letterSpacing: 0.5,
                ),
              ),
            ),
          ),

        // Favorite Button (Top-Right)
        Positioned(
          top: 8,
          right: 8,
          child: GestureDetector(
            onTap: onFavorite,
            child: Container(
              padding: const EdgeInsets.all(6),
              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: 0.9),
                shape: BoxShape.circle,
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.15),
                    blurRadius: 4,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: Icon(
                isFavorite ? Icons.favorite : Icons.favorite_border,
                size: 20,
                color: isFavorite ? AppColors.accent : AppColors.textSecondary,
              ),
            ),
          ),
        ),

        // Swipeable Gallery Indicator (Bottom)
        if (vehicle.images.length > 1)
          Positioned(
            bottom: 8,
            left: 0,
            right: 0,
            child: Center(
              child: Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 8,
                  vertical: 4,
                ),
                decoration: BoxDecoration(
                  color: Colors.black.withValues(alpha: 0.5),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(
                      Icons.collections,
                      size: 12,
                      color: Colors.white,
                    ),
                    const SizedBox(width: 4),
                    Text(
                      '${vehicle.images.length}',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 11,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
      ],
    );
  }

  Widget _buildInfoSection(BuildContext context, ResponsiveHelper responsive) {
    return Padding(
      padding: EdgeInsets.all(responsive.horizontalPadding * 0.5),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          // Row 1: Price + Quick CTA
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              // Price (Large, Bold)
              Flexible(
                child: Text(
                  '\$${_formatPrice(vehicle.price)}',
                  style: TextStyle(
                    color: AppColors.primary,
                    fontWeight: FontWeight.bold,
                    fontSize: responsive.titleFontSize + 6,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
              // Quick Chat CTA
              Container(
                padding: EdgeInsets.symmetric(
                  horizontal: responsive.horizontalPadding * 0.5,
                  vertical: 4,
                ),
                decoration: BoxDecoration(
                  color: AppColors.primary.withValues(alpha: 0.1),
                  borderRadius:
                      BorderRadius.circular(responsive.borderRadius * 0.6),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      Icons.chat_bubble_outline,
                      size: responsive.iconSize * 0.875,
                      color: AppColors.primary,
                    ),
                    SizedBox(width: 4),
                    Text(
                      'Chat',
                      style: TextStyle(
                        color: AppColors.primary,
                        fontSize: responsive.smallFontSize,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),

          SizedBox(height: 2),

          // Row 2: Title (Year + Make + Model)
          Text(
            '${vehicle.year} ${vehicle.make} ${vehicle.model}',
            style: TextStyle(
              fontWeight: FontWeight.w600,
              fontSize: responsive.titleFontSize,
            ),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),

          SizedBox(height: 2),

          // Row 3: Metadata (Mileage + Distance)
          Row(
            children: [
              // Mileage
              Icon(
                Icons.speed,
                size: responsive.iconSize * 0.875,
                color: AppColors.textSecondary,
              ),
              SizedBox(width: 4),
              Text(
                _formatMileage(vehicle.mileage),
                style: TextStyle(
                  color: AppColors.textSecondary,
                  fontSize: responsive.bodyFontSize,
                ),
              ),
              SizedBox(width: 12),
              // Distance
              Icon(
                Icons.location_on,
                size: responsive.iconSize * 0.875,
                color: AppColors.textSecondary,
              ),
              SizedBox(width: 4),
              Expanded(
                child: Text(
                  vehicle.location,
                  style: TextStyle(
                    color: AppColors.textSecondary,
                    fontSize: responsive.bodyFontSize,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  String _getBadgeText() {
    if (badgeText != null) return badgeText!;
    if (isSponsored) return 'SPONSORED';
    if (isFeatured) return 'FEATURED';
    return '';
  }

  Color _getBadgeColor() {
    if (badgeColor != null) return badgeColor!;
    if (isSponsored) return AppColors.gold;
    if (isFeatured) return AppColors.primary;
    return AppColors.accent;
  }

  String _formatPrice(double price) {
    if (price >= 1000000) {
      return '${(price / 1000000).toStringAsFixed(1)}M';
    } else if (price >= 1000) {
      return '${(price / 1000).toStringAsFixed(0)}K';
    }
    return price.toStringAsFixed(0);
  }

  String _formatMileage(int mileage) {
    if (mileage >= 1000) {
      return '${(mileage / 1000).toStringAsFixed(1)}k mi';
    }
    return '$mileage mi';
  }
}

/// Horizontal Compact Card (for scrollable lists)
class HorizontalCompactVehicleCard extends StatelessWidget {
  final Vehicle vehicle;
  final VoidCallback? onTap;
  final VoidCallback? onFavorite;
  final bool isFavorite;
  final bool isSponsored;
  final bool isFeatured;
  final String? badgeText;

  const HorizontalCompactVehicleCard({
    super.key,
    required this.vehicle,
    this.onTap,
    this.onFavorite,
    this.isFavorite = false,
    this.isSponsored = false,
    this.isFeatured = false,
    this.badgeText,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 280, // Fixed width for horizontal scroll
      margin: const EdgeInsets.only(right: AppSpacing.md),
      child: CompactVehicleCard(
        vehicle: vehicle,
        onTap: onTap,
        onFavorite: onFavorite,
        isFavorite: isFavorite,
        isSponsored: isSponsored,
        isFeatured: isFeatured,
        badgeText: badgeText,
      ),
    );
  }
}
