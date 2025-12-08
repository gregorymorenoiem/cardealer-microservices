import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../../domain/entities/vehicle.dart';

/// Grid showing vehicle specifications
class VehicleSpecsGrid extends StatelessWidget {
  final Vehicle vehicle;

  const VehicleSpecsGrid({
    super.key,
    required this.vehicle,
  });

  @override
  Widget build(BuildContext context) {
    final specs = [
      _SpecItem(
        icon: Icons.event,
        label: 'Año',
        value: vehicle.year.toString(),
      ),
      _SpecItem(
        icon: Icons.speed,
        label: 'Kilometraje',
        value: _formatMileage(vehicle.mileage),
      ),
      _SpecItem(
        icon: Icons.category,
        label: 'Tipo',
        value: vehicle.bodyType,
      ),
      _SpecItem(
        icon: Icons.local_gas_station,
        label: 'Combustible',
        value: vehicle.fuelType,
      ),
      _SpecItem(
        icon: Icons.settings,
        label: 'Transmisión',
        value: vehicle.transmission,
      ),
      _SpecItem(
        icon: Icons.verified,
        label: 'Condición',
        value: vehicle.condition,
      ),
      if (vehicle.color != null)
        _SpecItem(
          icon: Icons.palette,
          label: 'Color',
          value: vehicle.color!,
        ),
      if (vehicle.engineSize != null)
        _SpecItem(
          icon: Icons.build,
          label: 'Motor',
          value: vehicle.engineSize!,
        ),
    ];

    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Especificaciones',
            style: Theme.of(context).textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.w600,
                ),
          ),
          const SizedBox(height: 16),
          GridView.builder(
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
              crossAxisCount: 2,
              childAspectRatio: 2.5,
              crossAxisSpacing: 16,
              mainAxisSpacing: 16,
            ),
            itemCount: specs.length,
            itemBuilder: (context, index) {
              final spec = specs[index];
              return _buildSpecItem(context, spec);
            },
          ),
        ],
      ),
    );
  }

  Widget _buildSpecItem(BuildContext context, _SpecItem spec) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.grey[100],
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        children: [
          Icon(
            spec.icon,
            size: 24,
            color: Theme.of(context).colorScheme.primary,
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text(
                  spec.label,
                  style: Theme.of(context).textTheme.bodySmall?.copyWith(
                        color: Colors.grey[600],
                      ),
                ),
                const SizedBox(height: 2),
                Text(
                  spec.value,
                  style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                        fontWeight: FontWeight.w600,
                      ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  String _formatMileage(int mileage) {
    final formatter = NumberFormat('#,###', 'es_MX');
    return '${formatter.format(mileage)} km';
  }
}

class _SpecItem {
  final IconData icon;
  final String label;
  final String value;

  _SpecItem({
    required this.icon,
    required this.label,
    required this.value,
  });
}
