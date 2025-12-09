import 'package:flutter/material.dart';
import '../../../../core/responsive/responsive_helper.dart';
import '../../../../domain/entities/vehicle.dart';
import '../../../widgets/vehicles/compact_vehicle_card.dart';
import '../../vehicle_detail/vehicle_detail_page.dart';

/// Grid view for displaying favorite vehicles
class FavoritesGrid extends StatelessWidget {
  final List<Vehicle> vehicles;
  final Function(String) onRemove;

  const FavoritesGrid({
    super.key,
    required this.vehicles,
    required this.onRemove,
  });

  @override
  Widget build(BuildContext context) {
    final responsive = context.responsive;

    return GridView.builder(
      padding: EdgeInsets.all(responsive.horizontalPadding),
      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: responsive.gridColumns,
        childAspectRatio: 0.7,
        crossAxisSpacing: responsive.cardSpacing,
        mainAxisSpacing: responsive.cardSpacing,
      ),
      itemCount: vehicles.length,
      itemBuilder: (context, index) {
        final vehicle = vehicles[index];
        return GestureDetector(
          onLongPress: () => _showRemoveDialog(context, vehicle),
          child: CompactVehicleCard(
            vehicle: vehicle,
            isFavorite: true,
            isFeatured: vehicle.isFeatured,
            onTap: () {
              Navigator.of(context).push(
                MaterialPageRoute(
                  builder: (context) => VehicleDetailPage(
                    vehicleId: vehicle.id,
                  ),
                ),
              );
            },
            onFavorite: () {
              onRemove(vehicle.id);
            },
          ),
        );
      },
    );
  }

  void _showRemoveDialog(BuildContext context, Vehicle vehicle) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Eliminar de Favoritos'),
        content: Text(
          '¿Deseas eliminar "${vehicle.make} ${vehicle.model}" de favoritos?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Cancelar'),
          ),
          TextButton(
            onPressed: () {
              Navigator.of(context).pop();
              onRemove(vehicle.id);
            },
            child: const Text(
              'Eliminar',
              style: TextStyle(color: Colors.red),
            ),
          ),
        ],
      ),
    );
  }
}
