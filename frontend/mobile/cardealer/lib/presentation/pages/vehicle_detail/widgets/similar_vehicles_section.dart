import 'package:flutter/material.dart';
import '../../../../domain/entities/vehicle.dart';
import '../../../widgets/vehicle_card.dart';

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

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Text(
              'Vehículos similares',
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
          const SizedBox(height: 16),
          SizedBox(
            height: 280,
            child: ListView.builder(
              scrollDirection: Axis.horizontal,
              padding: const EdgeInsets.symmetric(horizontal: 16),
              itemCount: vehicles.length,
              itemBuilder: (context, index) {
                final vehicle = vehicles[index];
                return Padding(
                  padding: EdgeInsets.only(
                    right: index < vehicles.length - 1 ? 16 : 0,
                  ),
                  child: SizedBox(
                    width: 200,
                    child: VehicleCard(
                      id: vehicle.id,
                      title: '${vehicle.make} ${vehicle.model}',
                      imageUrl: vehicle.images.isNotEmpty ? vehicle.images[0] : '',
                      price: vehicle.price,
                      year: vehicle.year.toString(),
                      mileage: vehicle.mileage.toString(),
                      location: vehicle.location,
                      isFeatured: vehicle.isFeatured,
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
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Vehículos similares',
            style: Theme.of(context).textTheme.titleLarge?.copyWith(
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 16),
          const Center(
            child: CircularProgressIndicator(),
          ),
        ],
      ),
    );
  }
}
