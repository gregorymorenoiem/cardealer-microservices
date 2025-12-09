import 'package:flutter/material.dart';
import '../../../../core/responsive/responsive_helper.dart';
import '../../../../domain/entities/vehicle.dart';

/// Seller information card
class SellerInfoCard extends StatelessWidget {
  final Vehicle vehicle;

  const SellerInfoCard({
    super.key,
    required this.vehicle,
  });

  @override
  Widget build(BuildContext context) {
    // TODO: Get actual seller info from vehicle entity
    // For now using placeholder values
    const sellerName = 'Vendedor';
    const rating = 4.5;
    const reviewCount = 23;
    // Determine if seller is a dealer based on vehicle data
    final isDealer = vehicle.dealerId != null;
    final responsive = context.responsive;

    return Padding(
      padding: EdgeInsets.all(responsive.horizontalPadding),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Vendedor',
            style: Theme.of(context).textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.w600,
                  fontSize: responsive.titleFontSize + 2,
                ),
          ),
          SizedBox(height: responsive.cardSpacing),
          Container(
            padding: EdgeInsets.all(responsive.cardSpacing),
            decoration: BoxDecoration(
              color: Colors.grey[100],
              borderRadius: BorderRadius.circular(responsive.borderRadius),
            ),
            child: Row(
              children: [
                // Avatar
                CircleAvatar(
                  radius: 30,
                  backgroundColor: Theme.of(context).colorScheme.primary,
                  child: Text(
                    sellerName[0].toUpperCase(),
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 24,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
                const SizedBox(width: 16),

                // Info
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Text(
                            sellerName,
                            style: Theme.of(context)
                                .textTheme
                                .titleMedium
                                ?.copyWith(
                                  fontWeight: FontWeight.w600,
                                ),
                          ),
                          if (isDealer) ...[
                            const SizedBox(width: 8),
                            Container(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 6,
                                vertical: 2,
                              ),
                              decoration: BoxDecoration(
                                color: Colors.blue[100],
                                borderRadius: BorderRadius.circular(4),
                              ),
                              child: const Text(
                                'Dealer',
                                style: TextStyle(
                                  color: Colors.blue,
                                  fontSize: 10,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                            ),
                          ],
                        ],
                      ),
                      const SizedBox(height: 4),
                      Row(
                        children: [
                          Icon(
                            Icons.star,
                            size: 16,
                            color: Colors.amber[700],
                          ),
                          const SizedBox(width: 4),
                          Text(
                            '$rating',
                            style: Theme.of(context)
                                .textTheme
                                .bodyMedium
                                ?.copyWith(
                                  fontWeight: FontWeight.w600,
                                ),
                          ),
                          Text(
                            ' ($reviewCount reseñas)',
                            style: Theme.of(context)
                                .textTheme
                                .bodyMedium
                                ?.copyWith(
                                  color: Colors.grey[600],
                                ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),

                // View profile button
                IconButton(
                  onPressed: () {
                    // TODO: Navigate to seller profile
                  },
                  icon: const Icon(Icons.arrow_forward_ios, size: 20),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
