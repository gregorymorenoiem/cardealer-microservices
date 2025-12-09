import 'package:flutter/material.dart';
import '../../../core/responsive/responsive_helper.dart';
import '../../../domain/entities/vehicle.dart';
import '../../pages/vehicle_detail/vehicle_detail_page.dart';
import '../vehicles/compact_vehicle_card.dart';

/// Recently Viewed Section - HR-008
/// Shows user's recently viewed vehicles using CompactVehicleCard
class RecentlyViewedSection extends StatelessWidget {
  final List<Vehicle> vehicles;
  final VoidCallback? onSeeAllTap;

  const RecentlyViewedSection({
    super.key,
    required this.vehicles,
    this.onSeeAllTap,
  });

  @override
  Widget build(BuildContext context) {
    if (vehicles.isEmpty) {
      return const SizedBox.shrink();
    }

    final responsive = context.responsive;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Section Header
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16.0),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Vistos Recientemente',
                    style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Continúa donde lo dejaste',
                    style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                          color: Colors.grey.shade600,
                          fontSize: 13,
                        ),
                  ),
                ],
              ),
              if (onSeeAllTap != null)
                TextButton(
                  onPressed: onSeeAllTap,
                  child: const Row(
                    children: [
                      Text('Limpiar'),
                      SizedBox(width: 4),
                      Icon(Icons.clear_all, size: 16),
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
          return Padding(
            padding: EdgeInsets.only(
              right: index < vehicles.length - 1 ? responsive.cardSpacing : 0,
            ),
            child: SizedBox(
              width: responsive.cardWidth,
              child: CompactVehicleCard(
                vehicle: vehicles[index],
                isFeatured: false,
                onTap: () {
                  Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (context) => VehicleDetailPage(
                        vehicleId: vehicles[index].id,
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
              isFeatured: false,
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
