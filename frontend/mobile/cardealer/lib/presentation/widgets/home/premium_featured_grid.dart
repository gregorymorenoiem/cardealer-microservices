import 'package:flutter/material.dart';
import '../../../core/responsive/responsive_helper.dart';
import '../../../domain/entities/vehicle.dart';
import '../../pages/vehicle_detail/vehicle_detail_page.dart';
import '../vehicles/compact_vehicle_card.dart';

/// Premium Featured Section - Horizontal scroll of featured vehicles
/// Converted from grid to horizontal for better mobile UX
class PremiumFeaturedGrid extends StatelessWidget {
  final List<Vehicle> vehicles;
  final String title;
  final String subtitle;
  final VoidCallback? onSeeAllTap;

  const PremiumFeaturedGrid({
    super.key,
    required this.vehicles,
    this.title = 'Vehículos Premium Destacados',
    this.subtitle = 'Vehículos de lujo y alto rendimiento seleccionados',
    this.onSeeAllTap,
  });

  @override
  Widget build(BuildContext context) {
    final responsive = context.responsive;

    return Container(
      padding: const EdgeInsets.symmetric(vertical: 20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [
            Colors.blue.shade50.withValues(alpha: 0.3),
            Colors.white,
          ],
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16.0),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        title,
                        style: const TextStyle(
                          fontSize: 22,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        subtitle,
                        style: TextStyle(
                          fontSize: 13,
                          color: Colors.grey.shade600,
                        ),
                      ),
                    ],
                  ),
                ),
                if (onSeeAllTap != null)
                  TextButton(
                    onPressed: onSeeAllTap,
                    style: TextButton.styleFrom(
                      foregroundColor: Colors.blue.shade700,
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Text(
                          'Ver Todo',
                          style: TextStyle(
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        const SizedBox(width: 4),
                        Icon(
                          Icons.arrow_forward_ios,
                          size: 14,
                          color: Colors.blue.shade700,
                        ),
                      ],
                    ),
                  ),
              ],
            ),
          ),
          const SizedBox(height: 16),

          // Mobile: Horizontal Scroll, Tablet: Grid
          responsive.isMobile
              ? _buildHorizontalList(context, responsive)
              : _buildGridLayout(context, responsive),
        ],
      ),
    );
  }

  Widget _buildHorizontalList(
      BuildContext context, ResponsiveHelper responsive) {
    return SizedBox(
      height: responsive.cardHeight,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        padding: EdgeInsets.symmetric(horizontal: responsive.horizontalPadding),
        itemCount: vehicles.length,
        itemBuilder: (context, index) {
          final vehicle = vehicles[index];
          return Padding(
            padding: EdgeInsets.only(
              right: index < vehicles.length - 1 ? responsive.cardSpacing : 0,
            ),
            child: SizedBox(
              width: responsive.cardWidth,
              child: CompactVehicleCard(
                vehicle: vehicle,
                isFeatured: true,
                onTap: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (context) => VehicleDetailPage(
                        vehicleId: vehicle.id,
                      ),
                    ),
                  );
                },
                onFavorite: () {
                  // TODO: Handle favorite toggle
                },
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildGridLayout(BuildContext context, ResponsiveHelper responsive) {
    final columns = responsive.cardGridColumns;
    final screenWidth = MediaQuery.of(context).size.width;
    final padding = responsive.horizontalPadding;
    final spacing = responsive.cardSpacing;
    final availableWidth =
        screenWidth - (padding * 2) - (spacing * (columns - 1));
    final cardWidth = availableWidth / columns;
    final cardHeight = cardWidth * 0.85; // Aspect ratio for cards

    return Padding(
      padding: EdgeInsets.symmetric(horizontal: padding),
      child: Wrap(
        spacing: spacing,
        runSpacing: spacing,
        children: vehicles.take(columns * 2).map((vehicle) {
          return SizedBox(
            width: cardWidth,
            height: cardHeight,
            child: CompactVehicleCard(
              vehicle: vehicle,
              isFeatured: true,
              onTap: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => VehicleDetailPage(
                      vehicleId: vehicle.id,
                    ),
                  ),
                );
              },
              onFavorite: () {
                // TODO: Handle favorite toggle
              },
            ),
          );
        }).toList(),
      ),
    );
  }
}
