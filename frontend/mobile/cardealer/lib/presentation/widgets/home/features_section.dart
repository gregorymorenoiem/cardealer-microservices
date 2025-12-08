import 'package:flutter/material.dart';

/// Features Section
/// Shows key platform features
class FeaturesSection extends StatelessWidget {
  const FeaturesSection({super.key});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Why Choose CarDealer?',
            style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
          ),
          const SizedBox(height: 16),
          _FeatureCard(
            icon: Icons.verified_user,
            title: 'Verified Listings',
            description: 'All vehicles are verified by our expert team',
            color: Colors.blue.shade600,
          ),
          const SizedBox(height: 12),
          _FeatureCard(
            icon: Icons.price_check,
            title: 'Best Prices',
            description: 'Competitive pricing and great deals',
            color: Colors.green.shade600,
          ),
          const SizedBox(height: 12),
          _FeatureCard(
            icon: Icons.support_agent,
            title: '24/7 Support',
            description: 'Customer support whenever you need it',
            color: Colors.orange.shade600,
          ),
          const SizedBox(height: 12),
          _FeatureCard(
            icon: Icons.shield,
            title: 'Secure Transactions',
            description: 'Your data and payments are always protected',
            color: Colors.purple.shade600,
          ),
        ],
      ),
    );
  }
}

class _FeatureCard extends StatelessWidget {
  final IconData icon;
  final String title;
  final String description;
  final Color color;

  const _FeatureCard({
    required this.icon,
    required this.title,
    required this.description,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: color.withValues(alpha: 0.3)),
      ),
      child: Row(
        children: [
          CircleAvatar(
            backgroundColor: color,
            radius: 24,
            child: Icon(icon, color: Colors.white, size: 24),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: const TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  description,
                  style: TextStyle(
                    fontSize: 13,
                    color: Colors.grey.shade600,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
