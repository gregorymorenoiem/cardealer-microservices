import 'dart:ui';
import 'package:flutter/material.dart';
import '../../../domain/entities/vehicle.dart';
import '../../pages/vehicle_detail/vehicle_detail_page.dart';
import '../vehicles/compact_vehicle_card.dart';

class PremiumFeaturedGrid extends StatelessWidget {
  final List<Vehicle> vehicles;
  final String title;
  final String subtitle;
  final VoidCallback? onSeeAllTap;

  const PremiumFeaturedGrid({
    super.key,
    required this.vehicles,
    this.title = 'Featured Premium Vehicles',
    this.subtitle = 'Hand-picked luxury and high-performance vehicles',
    this.onSeeAllTap,
  });

  @override
  Widget build(BuildContext context) {
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
                          'See All',
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

          // Grid
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16.0),
            child: GridView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 2,
                childAspectRatio: 0.78, // Optimized for 180dp compact cards
                mainAxisSpacing: 12,
                crossAxisSpacing: 12,
              ),
              itemCount: vehicles.length,
              itemBuilder: (context, index) {
                final vehicle = vehicles[index];
                return CompactVehicleCard(
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
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}
