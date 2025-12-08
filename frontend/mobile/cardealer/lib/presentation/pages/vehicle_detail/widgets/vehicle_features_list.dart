import 'package:flutter/material.dart';
import '../../../../domain/entities/vehicle.dart';

/// List of vehicle features with checkmarks
class VehicleFeaturesList extends StatelessWidget {
  final Vehicle vehicle;

  const VehicleFeaturesList({
    super.key,
    required this.vehicle,
  });

  @override
  Widget build(BuildContext context) {
    // Build features list from vehicle properties
    final features = <String>[];
    
    // TODO: Add more features from vehicle entity when available
    // For now, add some common features based on condition and type
    features.addAll([
      'Aire acondicionado',
      'Sistema de audio',
      'Cierre centralizado',
      'Espejos eléctricos',
      'Dirección asistida',
      'Frenos ABS',
    ]);

    if (features.isEmpty) {
      return const SizedBox.shrink();
    }

    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Características',
            style: Theme.of(context).textTheme.titleLarge?.copyWith(
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 16),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: features.map((feature) {
              return Chip(
                avatar: const Icon(
                  Icons.check_circle,
                  size: 18,
                  color: Colors.green,
                ),
                label: Text(feature),
                backgroundColor: Colors.green[50],
                side: BorderSide.none,
              );
            }).toList(),
          ),
        ],
      ),
    );
  }
}
