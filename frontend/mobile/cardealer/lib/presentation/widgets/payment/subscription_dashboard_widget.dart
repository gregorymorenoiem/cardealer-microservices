import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../../domain/entities/payment.dart';

/// Widget displaying subscription dashboard with usage stats
class SubscriptionDashboardWidget extends StatelessWidget {
  final Subscription subscription;

  const SubscriptionDashboardWidget({
    required this.subscription,
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    final dateFormat = DateFormat('MMM d, y');

    return Card(
      elevation: 3,
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text(
                  'Current Subscription',
                  style: TextStyle(
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                _buildStatusBadge(subscription.isActive),
              ],
            ),

            const SizedBox(height: 16),

            // Plan info
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: Colors.blue.shade50,
                borderRadius: BorderRadius.circular(12),
              ),
              child: Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          subscription.plan.name,
                          style: const TextStyle(
                            fontSize: 20,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          subscription.plan
                              .getFormattedPrice(subscription.billingPeriod),
                          style: TextStyle(
                            fontSize: 16,
                            color: Colors.grey[700],
                          ),
                        ),
                      ],
                    ),
                  ),
                  Icon(
                    Icons.workspace_premium,
                    size: 48,
                    color: Colors.blue.shade700,
                  ),
                ],
              ),
            ),

            const SizedBox(height: 16),

            // Billing info
            _buildInfoRow(
              Icons.calendar_today,
              'Next Billing',
              subscription.nextBillingDate != null
                  ? dateFormat.format(subscription.nextBillingDate!)
                  : 'N/A',
            ),

            if (subscription.isExpiringSoon)
              Padding(
                padding: const EdgeInsets.only(top: 8),
                child: Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: Colors.orange.shade50,
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: Colors.orange.shade200),
                  ),
                  child: Row(
                    children: [
                      Icon(Icons.warning_amber,
                          color: Colors.orange.shade700, size: 18),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          'Your subscription renews in ${subscription.daysUntilNextBilling} days',
                          style: TextStyle(
                            color: Colors.orange.shade700,
                            fontSize: 12,
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),

            const SizedBox(height: 16),

            // Usage stats
            if (subscription.usageStats != null) ...[
              const Divider(),
              const SizedBox(height: 16),
              const Text(
                'Usage This Period',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 12),
              _buildUsageBar(
                context,
                'Listings',
                subscription.usageStats!.currentListings,
                subscription.usageStats!.listingsLimit,
              ),
              const SizedBox(height: 12),
              _buildUsageBar(
                context,
                'Featured Listings',
                subscription.usageStats!.currentFeaturedListings,
                subscription.usageStats!.featuredListingsLimit,
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildStatusBadge(bool isActive) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: isActive ? Colors.green : Colors.red,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Text(
        isActive ? 'ACTIVE' : 'INACTIVE',
        style: const TextStyle(
          color: Colors.white,
          fontSize: 11,
          fontWeight: FontWeight.bold,
        ),
      ),
    );
  }

  Widget _buildInfoRow(IconData icon, String label, String value) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        children: [
          Icon(icon, size: 18, color: Colors.grey[600]),
          const SizedBox(width: 8),
          Text(
            '$label: ',
            style: TextStyle(
              color: Colors.grey[600],
              fontSize: 14,
            ),
          ),
          Text(
            value,
            style: const TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 14,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildUsageBar(
    BuildContext context,
    String label,
    int current,
    int limit,
  ) {
    final percentage = limit > 0 ? (current / limit) : 0.0;
    final isNearLimit = percentage >= 0.8;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              label,
              style: const TextStyle(
                fontSize: 14,
                fontWeight: FontWeight.w500,
              ),
            ),
            Text(
              '$current / ${limit == -1 ? "∞" : limit}',
              style: TextStyle(
                fontSize: 13,
                color: isNearLimit ? Colors.orange : Colors.grey[600],
                fontWeight: isNearLimit ? FontWeight.bold : FontWeight.normal,
              ),
            ),
          ],
        ),
        const SizedBox(height: 6),
        ClipRRect(
          borderRadius: BorderRadius.circular(4),
          child: LinearProgressIndicator(
            value: limit == -1 ? 0 : percentage.clamp(0.0, 1.0),
            backgroundColor: Colors.grey[200],
            valueColor: AlwaysStoppedAnimation<Color>(
              isNearLimit ? Colors.orange : Theme.of(context).primaryColor,
            ),
            minHeight: 8,
          ),
        ),
      ],
    );
  }
}
