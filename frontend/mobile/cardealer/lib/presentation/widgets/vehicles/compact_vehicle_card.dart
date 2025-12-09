import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../../core/theme/colors.dart';
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
          vertical: responsive.cardSpacing * 0.25,
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
            // Photo Section: 55%
            Expanded(
              flex: 55,
              child: _buildPhotoSection(context, responsive),
            ),
            // Info Section: 45%
            Expanded(
              flex: 45,
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
                borderRadius:
                    BorderRadius.circular(responsive.borderRadius * 0.33),
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
      padding: EdgeInsets.symmetric(
        horizontal: responsive.horizontalPadding * 0.375,
        vertical: 2, // Minimal vertical padding to prevent overflow
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        mainAxisSize: MainAxisSize.max,
        children: [
          // Row 1: Price
          Flexible(
            child: Text(
              '\$${_formatPrice(vehicle.price)}',
              style: TextStyle(
                color: AppColors.primary,
                fontWeight: FontWeight.bold,
                fontSize: responsive.titleFontSize + 2,
              ),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
            ),
          ),

          // Row 2: Title (Year + Make + Model)
          Flexible(
            child: Text(
              '${vehicle.year} ${vehicle.make} ${vehicle.model}',
              style: TextStyle(
                fontWeight: FontWeight.w600,
                fontSize: responsive.bodyFontSize,
              ),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
            ),
          ),

          // Row 3: Metadata (Mileage + Distance)
          Flexible(
            child: Row(
              children: [
                // Mileage
                Icon(
                  Icons.speed,
                  size: responsive.iconSize * 0.7,
                  color: AppColors.textSecondary,
                ),
                SizedBox(width: 2),
                Text(
                  _formatMileage(vehicle.mileage),
                  style: TextStyle(
                    color: AppColors.textSecondary,
                    fontSize: responsive.smallFontSize,
                  ),
                ),
                SizedBox(width: 8),
                // Distance
                Icon(
                  Icons.location_on,
                  size: responsive.iconSize * 0.7,
                  color: AppColors.textSecondary,
                ),
                SizedBox(width: 2),
                Expanded(
                  child: Text(
                    vehicle.location,
                    style: TextStyle(
                      color: AppColors.textSecondary,
                      fontSize: responsive.smallFontSize,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
              ],
            ),
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
    final responsive = context.responsive;

    return Container(
      width: responsive.cardWidth, // Responsive width for horizontal scroll
      margin: EdgeInsets.only(right: responsive.cardSpacing),
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
