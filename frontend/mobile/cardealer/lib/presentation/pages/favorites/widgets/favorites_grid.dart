import 'package:flutter/material.dart';
import '../../../../core/responsive/responsive_utils.dart';
import '../../../../domain/entities/vehicle.dart';
import '../../../widgets/vehicle_card_grid.dart';

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
    return GridView.builder(
      padding: const EdgeInsets.all(16),
      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: context.gridColumns,
        childAspectRatio: 0.7,
        crossAxisSpacing: 16,
        mainAxisSpacing: 16,
      ),
      itemCount: vehicles.length,
      itemBuilder: (context, index) {
        final vehicle = vehicles[index];
        return GestureDetector(
          onLongPress: () => _showRemoveDialog(context, vehicle),
          child: VehicleCardGrid(
            id: vehicle.id,
            title: '${vehicle.make} ${vehicle.model}',
            imageUrl: vehicle.images.isNotEmpty ? vehicle.images.first : '',
            price: vehicle.price,
            year: vehicle.year.toString(),
            isFeatured: vehicle.isFeatured,
            isFavorited: true,
            onTap: () {
              Navigator.of(context).pushNamed(
                '/vehicle-detail',
                arguments: vehicle.id,
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
