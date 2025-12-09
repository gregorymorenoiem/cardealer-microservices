import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../../core/responsive/responsive_helper.dart';
import '../../../../domain/entities/vehicle.dart';

/// Vehicle price and title header
class VehicleInfoHeader extends StatelessWidget {
  final Vehicle vehicle;

  const VehicleInfoHeader({
    super.key,
    required this.vehicle,
  });

  @override
  Widget build(BuildContext context) {
    final formatter = NumberFormat.currency(
      locale: 'es_MX',
      symbol: '\$',
      decimalDigits: 0,
    );

    final responsive = context.responsive;
    
    return Padding(
      padding: EdgeInsets.all(responsive.horizontalPadding),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Price
          Text(
            formatter.format(vehicle.price),
            style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                  color: Theme.of(context).colorScheme.primary,
                  fontWeight: FontWeight.bold,
                  fontSize: responsive.titleFontSize + 8,
                ),
          ),
          SizedBox(height: responsive.cardSpacing * 0.6),

          // Title
          Text(
            '${vehicle.make} ${vehicle.model} ${vehicle.year}',
            style: Theme.of(context).textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.w600,
                  fontSize: responsive.titleFontSize + 2,
                ),
          ),
          SizedBox(height: responsive.cardSpacing * 0.3),

          // Location and date
          Row(
            children: [
              Icon(Icons.location_on, size: responsive.iconSize * 0.8, color: Colors.grey),
              SizedBox(width: responsive.cardSpacing * 0.3),
              Flexible(
                child: Text(
                  vehicle.location,
                  style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                        color: Colors.grey[600],
                        fontSize: responsive.bodyFontSize,
                      ),
                  overflow: TextOverflow.ellipsis,
                ),
              ),
              SizedBox(width: responsive.cardSpacing),
              Icon(Icons.calendar_today, size: responsive.iconSize * 0.8, color: Colors.grey),
              SizedBox(width: responsive.cardSpacing * 0.3),
              Text(
                _formatDate(vehicle.createdAt),
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      color: Colors.grey[600],
                      fontSize: responsive.bodyFontSize,
                    ),
              ),
            ],
          ),

          // Featured badge
          if (vehicle.isFeatured)
            Padding(
              padding: EdgeInsets.only(top: responsive.cardSpacing * 0.6),
              child: Container(
                padding: EdgeInsets.symmetric(
                  horizontal: responsive.cardSpacing * 0.6,
                  vertical: responsive.cardSpacing * 0.3,
                ),
                decoration: BoxDecoration(
                  color: Colors.orange[100],
                  borderRadius: BorderRadius.circular(responsive.borderRadius * 0.33),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(Icons.star, size: responsive.iconSize * 0.8, color: Colors.orange),
                    SizedBox(width: responsive.cardSpacing * 0.3),
                    Text(
                      'Destacado',
                      style: TextStyle(
                        color: Colors.orange,
                        fontSize: responsive.smallFontSize,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
              ),
            ),
        ],
      ),
    );
  }

  String _formatDate(DateTime date) {
    final now = DateTime.now();
    final difference = now.difference(date);

    if (difference.inDays == 0) {
      return 'Hoy';
    } else if (difference.inDays == 1) {
      return 'Ayer';
    } else if (difference.inDays < 7) {
      return 'Hace ${difference.inDays} días';
    } else {
      return DateFormat('dd MMM yyyy', 'es_MX').format(date);
    }
  }
}
