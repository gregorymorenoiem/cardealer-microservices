import 'package:flutter/material.dart';

/// Premium Price Section with Market Comparison
///
/// Features:
/// - Large highlighted price
/// - Market comparison badge
/// - "Good Price" indicator
/// - Original price (if discounted)
/// - Price per month estimation
class PremiumPriceSection extends StatelessWidget {
  final double price;
  final double? originalPrice;
  final double? marketAverage;
  final double? monthlyPayment;
  final String currency;
  final bool isGoodDeal;

  const PremiumPriceSection({
    super.key,
    required this.price,
    this.originalPrice,
    this.marketAverage,
    this.monthlyPayment,
    this.currency = '\$',
    this.isGoodDeal = false,
  });

  String _formatPrice(double value) {
    return '$currency${value.toStringAsFixed(0).replaceAllMapped(
          RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'),
          (Match m) => '${m[1]},',
        )}';
  }

  double? get _savingsPercentage => originalPrice != null
      ? ((originalPrice! - price) / originalPrice! * 100)
      : null;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            Color(0xFF001F54),
            Color(0xFF001235),
          ],
        ),
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.1),
            blurRadius: 20,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Good Deal Badge
          if (isGoodDeal)
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
              decoration: BoxDecoration(
                color: const Color(0xFF10B981),
                borderRadius: BorderRadius.circular(20),
              ),
              child: const Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.check_circle, color: Colors.white, size: 16),
                  SizedBox(width: 6),
                  Text(
                    'Great Price',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 12,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),

          const SizedBox(height: 12),

          // Main Price
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                _formatPrice(price),
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 36,
                  fontWeight: FontWeight.bold,
                  letterSpacing: -0.5,
                ),
              ),

              // Original Price (if discounted)
              if (originalPrice != null) ...[
                const SizedBox(width: 12),
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      _formatPrice(originalPrice!),
                      style: TextStyle(
                        color: Colors.white.withValues(alpha: 0.5),
                        fontSize: 16,
                        decoration: TextDecoration.lineThrough,
                      ),
                    ),
                    if (_savingsPercentage != null)
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 6,
                          vertical: 2,
                        ),
                        decoration: BoxDecoration(
                          color: const Color(0xFFFF6B35),
                          borderRadius: BorderRadius.circular(4),
                        ),
                        child: Text(
                          '-${_savingsPercentage!.toStringAsFixed(0)}%',
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 10,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                  ],
                ),
              ],
            ],
          ),

          const SizedBox(height: 8),

          // Market Comparison
          if (marketAverage != null)
            Text(
              price < marketAverage!
                  ? '\$${(marketAverage! - price).toStringAsFixed(0)} below market average'
                  : 'Market average: ${_formatPrice(marketAverage!)}',
              style: TextStyle(
                color: price < marketAverage!
                    ? const Color(0xFF10B981)
                    : Colors.white.withValues(alpha: 0.7),
                fontSize: 14,
                fontWeight: FontWeight.w500,
              ),
            ),

          // Monthly Payment
          if (monthlyPayment != null) ...[
            const SizedBox(height: 16),
            const Divider(color: Colors.white24),
            const SizedBox(height: 16),
            Row(
              children: [
                const Icon(
                  Icons.calendar_today,
                  color: Colors.white70,
                  size: 16,
                ),
                const SizedBox(width: 8),
                Text(
                  'From ${_formatPrice(monthlyPayment!)}/mo',
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const SizedBox(width: 8),
                Text(
                  '(estimated)',
                  style: TextStyle(
                    color: Colors.white.withValues(alpha: 0.5),
                    fontSize: 12,
                  ),
                ),
              ],
            ),
          ],
        ],
      ),
    );
  }
}
