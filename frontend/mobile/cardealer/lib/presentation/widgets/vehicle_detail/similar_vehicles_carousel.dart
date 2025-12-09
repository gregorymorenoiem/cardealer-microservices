import 'package:flutter/material.dart';

/// Similar Vehicles Carousel
///
/// Features:
/// - Horizontal scrolling cards
/// - "More like this" recommendations
/// - Quick favorite toggle
/// - Navigate to vehicle detail
class SimilarVehiclesCarousel extends StatelessWidget {
  final List<SimilarVehicle> vehicles;
  final Function(String vehicleId) onVehicleTap;
  final Function(String vehicleId) onFavoriteTap;

  const SimilarVehiclesCarousel({
    super.key,
    required this.vehicles,
    required this.onVehicleTap,
    required this.onFavoriteTap,
  });

  @override
  Widget build(BuildContext context) {
    if (vehicles.isEmpty) {
      return const SizedBox.shrink();
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Padding(
          padding: EdgeInsets.symmetric(horizontal: 20),
          child: Row(
            children: [
              Icon(
                Icons.recommend,
                color: Color(0xFF001F54),
                size: 24,
              ),
              SizedBox(width: 12),
              Text(
                'Similar Vehicles',
                style: TextStyle(
                  color: Color(0xFF1E293B),
                  fontSize: 20,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ],
          ),
        ),
        const SizedBox(height: 16),
        SizedBox(
          height: 280,
          child: ListView.builder(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 20),
            itemCount: vehicles.length,
            itemBuilder: (context, index) {
              final vehicle = vehicles[index];
              return Padding(
                padding: EdgeInsets.only(
                  right: index < vehicles.length - 1 ? 16 : 0,
                ),
                child: _VehicleCard(
                  vehicle: vehicle,
                  onTap: () => onVehicleTap(vehicle.id),
                  onFavorite: () => onFavoriteTap(vehicle.id),
                ),
              );
            },
          ),
        ),
      ],
    );
  }
}

class _VehicleCard extends StatelessWidget {
  final SimilarVehicle vehicle;
  final VoidCallback onTap;
  final VoidCallback onFavorite;

  const _VehicleCard({
    required this.vehicle,
    required this.onTap,
    required this.onFavorite,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 220,
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: Colors.grey.shade200,
          ),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.05),
              blurRadius: 8,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Image
            Stack(
              children: [
                ClipRRect(
                  borderRadius: const BorderRadius.vertical(
                    top: Radius.circular(12),
                  ),
                  child: Image.network(
                    vehicle.imageUrl,
                    width: double.infinity,
                    height: 140,
                    fit: BoxFit.cover,
                    errorBuilder: (context, error, stackTrace) {
                      return Container(
                        width: double.infinity,
                        height: 140,
                        color: Colors.grey.shade200,
                        child: Icon(
                          Icons.directions_car,
                          size: 48,
                          color: Colors.grey.shade400,
                        ),
                      );
                    },
                  ),
                ),

                // Favorite Button
                Positioned(
                  top: 8,
                  right: 8,
                  child: GestureDetector(
                    onTap: onFavorite,
                    child: Container(
                      padding: const EdgeInsets.all(8),
                      decoration: BoxDecoration(
                        color: Colors.white,
                        shape: BoxShape.circle,
                        boxShadow: [
                          BoxShadow(
                            color: Colors.black.withValues(alpha: 0.1),
                            blurRadius: 4,
                          ),
                        ],
                      ),
                      child: Icon(
                        vehicle.isFavorite
                            ? Icons.favorite
                            : Icons.favorite_border,
                        color: vehicle.isFavorite
                            ? const Color(0xFFEF4444)
                            : Colors.grey.shade600,
                        size: 18,
                      ),
                    ),
                  ),
                ),
              ],
            ),

            // Details
            Padding(
              padding: const EdgeInsets.all(12),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Title
                  Text(
                    vehicle.title,
                    style: const TextStyle(
                      color: Color(0xFF1E293B),
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                    ),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),

                  const SizedBox(height: 8),

                  // Specs
                  Row(
                    children: [
                      Icon(
                        Icons.speed,
                        size: 12,
                        color: Colors.grey.shade600,
                      ),
                      const SizedBox(width: 4),
                      Text(
                        vehicle.mileage,
                        style: TextStyle(
                          color: Colors.grey.shade600,
                          fontSize: 11,
                        ),
                      ),
                      const SizedBox(width: 8),
                      Icon(
                        Icons.local_gas_station,
                        size: 12,
                        color: Colors.grey.shade600,
                      ),
                      const SizedBox(width: 4),
                      Expanded(
                        child: Text(
                          vehicle.fuelType,
                          style: TextStyle(
                            color: Colors.grey.shade600,
                            fontSize: 11,
                          ),
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                    ],
                  ),

                  const SizedBox(height: 8),

                  // Price
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text(
                        vehicle.price,
                        style: const TextStyle(
                          color: Color(0xFF001F54),
                          fontSize: 18,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                      if (vehicle.discount != null)
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 6,
                            vertical: 2,
                          ),
                          decoration: BoxDecoration(
                            color: const Color(0xFFEF4444),
                            borderRadius: BorderRadius.circular(4),
                          ),
                          child: Text(
                            vehicle.discount!,
                            style: const TextStyle(
                              color: Colors.white,
                              fontSize: 10,
                              fontWeight: FontWeight.w700,
                            ),
                          ),
                        ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Similar Vehicle Model
class SimilarVehicle {
  final String id;
  final String title;
  final String price;
  final String imageUrl;
  final String mileage;
  final String fuelType;
  final String? discount;
  final bool isFavorite;

  const SimilarVehicle({
    required this.id,
    required this.title,
    required this.price,
    required this.imageUrl,
    required this.mileage,
    required this.fuelType,
    this.discount,
    this.isFavorite = false,
  });
}
