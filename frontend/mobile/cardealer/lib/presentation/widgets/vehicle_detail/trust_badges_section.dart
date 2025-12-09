import 'package:flutter/material.dart';

/// Trust Badges Section
///
/// Features:
/// - "Verified by CarDealer" badge
/// - "Clean History" indicator
/// - Warranty badges
/// - Security indicators
class TrustBadgesSection extends StatelessWidget {
  final bool isVerified;
  final bool hasCleanHistory;
  final bool hasWarranty;
  final bool hasInspection;
  final bool hasMoneyBackGuarantee;
  final int? warrantyMonths;

  const TrustBadgesSection({
    super.key,
    this.isVerified = false,
    this.hasCleanHistory = false,
    this.hasWarranty = false,
    this.hasInspection = false,
    this.hasMoneyBackGuarantee = false,
    this.warrantyMonths,
  });

  @override
  Widget build(BuildContext context) {
    final badges = _buildBadgesList();

    if (badges.isEmpty) {
      return const SizedBox.shrink();
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Row(
          children: [
            Icon(
              Icons.shield_outlined,
              color: Color(0xFF001F54),
              size: 24,
            ),
            SizedBox(width: 12),
            Text(
              'Trust & Safety',
              style: TextStyle(
                color: Color(0xFF1E293B),
                fontSize: 20,
                fontWeight: FontWeight.w700,
              ),
            ),
          ],
        ),
        const SizedBox(height: 16),
        Wrap(
          spacing: 12,
          runSpacing: 12,
          children: badges,
        ),
      ],
    );
  }

  List<Widget> _buildBadgesList() {
    final badges = <Widget>[];

    if (isVerified) {
      badges.add(
        const _TrustBadge(
          icon: Icons.verified,
          label: 'Verified by CarDealer',
          description: 'This vehicle has been verified by our team',
          color: Color(0xFF10B981),
        ),
      );
    }

    if (hasCleanHistory) {
      badges.add(
        const _TrustBadge(
          icon: Icons.history,
          label: 'Clean History',
          description: 'No accidents reported',
          color: Color(0xFF3B82F6),
        ),
      );
    }

    if (hasWarranty) {
      badges.add(
        _TrustBadge(
          icon: Icons.security,
          label: warrantyMonths != null
              ? '$warrantyMonths-Month Warranty'
              : 'Warranty Included',
          description: 'Comprehensive coverage included',
          color: const Color(0xFF6366F1),
        ),
      );
    }

    if (hasInspection) {
      badges.add(
        const _TrustBadge(
          icon: Icons.check_circle,
          label: '150-Point Inspection',
          description: 'Thoroughly inspected and certified',
          color: Color(0xFF8B5CF6),
        ),
      );
    }

    if (hasMoneyBackGuarantee) {
      badges.add(
        const _TrustBadge(
          icon: Icons.currency_exchange,
          label: 'Money-Back Guarantee',
          description: '7-day return policy',
          color: Color(0xFFF59E0B),
        ),
      );
    }

    return badges;
  }
}

class _TrustBadge extends StatelessWidget {
  final IconData icon;
  final String label;
  final String description;
  final Color color;

  const _TrustBadge({
    required this.icon,
    required this.label,
    required this.description,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.05),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: color.withValues(alpha: 0.2),
          width: 1.5,
        ),
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: color.withValues(alpha: 0.1),
              shape: BoxShape.circle,
            ),
            child: Icon(
              icon,
              color: color,
              size: 24,
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  label,
                  style: TextStyle(
                    color: color,
                    fontSize: 15,
                    fontWeight: FontWeight.w700,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  description,
                  style: TextStyle(
                    color: Colors.grey.shade700,
                    fontSize: 12,
                    height: 1.4,
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
