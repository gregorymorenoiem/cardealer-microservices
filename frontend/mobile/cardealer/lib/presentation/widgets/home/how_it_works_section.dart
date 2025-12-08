import 'package:flutter/material.dart';

/// How It Works Section
/// Shows the process of buying/selling on the platform
class HowItWorksSection extends StatelessWidget {
  const HowItWorksSection({super.key});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'How It Works',
            style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
          ),
          const SizedBox(height: 16),
          const _StepCard(
            step: 1,
            title: 'Browse Vehicles',
            description: 'Explore thousands of verified vehicles',
            icon: Icons.search,
          ),
          const SizedBox(height: 12),
          const _StepCard(
            step: 2,
            title: 'Contact Seller',
            description: 'Connect directly with dealers or owners',
            icon: Icons.chat,
          ),
          const SizedBox(height: 12),
          const _StepCard(
            step: 3,
            title: 'Schedule Test Drive',
            description: 'Book an appointment to see the vehicle',
            icon: Icons.calendar_today,
          ),
          const SizedBox(height: 12),
          const _StepCard(
            step: 4,
            title: 'Complete Purchase',
            description: 'Secure payment and paperwork handling',
            icon: Icons.check_circle,
          ),
        ],
      ),
    );
  }
}

class _StepCard extends StatelessWidget {
  final int step;
  final String title;
  final String description;
  final IconData icon;

  const _StepCard({
    required this.step,
    required this.title,
    required this.description,
    required this.icon,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Step Number
        CircleAvatar(
          backgroundColor: Theme.of(context).primaryColor,
          radius: 20,
          child: Text(
            '$step',
            style: const TextStyle(
              color: Colors.white,
              fontWeight: FontWeight.bold,
              fontSize: 16,
            ),
          ),
        ),
        const SizedBox(width: 16),
        // Content
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Icon(
                    icon,
                    size: 20,
                    color: Theme.of(context).primaryColor,
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      title,
                      style: const TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ],
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
    );
  }
}
