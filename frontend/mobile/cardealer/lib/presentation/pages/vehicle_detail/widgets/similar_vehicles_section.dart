import 'package:flutter/material.dart';
import '../../../../domain/entities/vehicle.dart';
import '../../../../core/responsive/responsive_helper.dart';
import '../../../widgets/vehicles/compact_vehicle_card.dart';
import '../vehicle_detail_page.dart';

/// Similar vehicles horizontal section
class SimilarVehiclesSection extends StatelessWidget {
  final List<Vehicle> vehicles;
  final bool isLoading;

  const SimilarVehiclesSection({
    super.key,
    required this.vehicles,
    this.isLoading = false,
  });

  @override
  Widget build(BuildContext context) {
    if (isLoading) {
      return _buildLoading(context);
    }

    if (vehicles.isEmpty) {
      return const SizedBox.shrink();
    }

    final responsive = context.responsive;

    return Padding(
      padding: EdgeInsets.symmetric(vertical: responsive.cardSpacing),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: EdgeInsets.symmetric(horizontal: responsive.horizontalPadding),
            child: Text(
              'Vehículos similares',
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.w600,
                    fontSize: responsive.titleFontSize + 2,
                  ),
            ),
          ),
          SizedBox(height: responsive.cardSpacing),
          SizedBox(
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
                      isFeatured: vehicle.isFeatured,
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
                        // TODO: Toggle favorite
                      },
                    ),
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildLoading(BuildContext context) {
    final responsive = context.responsive;
    
    return Padding(
      padding: EdgeInsets.all(responsive.horizontalPadding),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Vehículos similares',
            style: Theme.of(context).textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.w600,
                  fontSize: responsive.titleFontSize + 2,
                ),
          ),
          SizedBox(height: responsive.cardSpacing),
          const Center(
            child: CircularProgressIndicator(),
          ),
        ],
      ),
    );
  }
}
