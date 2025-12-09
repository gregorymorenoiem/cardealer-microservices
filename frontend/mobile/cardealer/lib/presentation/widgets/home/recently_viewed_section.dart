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
                    'Recently Viewed',
                    style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Continue where you left off',
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
                      Text('Clear'),
                      SizedBox(width: 4),
                      Icon(Icons.clear_all, size: 16),
                    ],
                  ),
                ),
            ],
          ),
        ),
        const SizedBox(height: 16),
        // Horizontal Scroll with CompactVehicleCard
        SizedBox(
          height: responsive.cardHeight,
          child: ListView.builder(
            scrollDirection: Axis.horizontal,
            padding:
                EdgeInsets.symmetric(horizontal: responsive.horizontalPadding),
            itemCount: vehicles.length,
            itemBuilder: (context, index) {
              return Padding(
                padding: EdgeInsets.only(
                  right:
                      index < vehicles.length - 1 ? responsive.cardSpacing : 0,
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
        ),
      ],
    );
  }
}
