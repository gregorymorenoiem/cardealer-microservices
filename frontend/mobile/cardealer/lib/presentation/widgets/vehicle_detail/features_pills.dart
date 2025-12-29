import 'package:flutter/material.dart';

/// Features Pills with Color Categories
///
/// Features:
/// - Colorful pills by category
/// - Descriptive icons
/// - Expandable section
/// - Category grouping (Safety, Comfort, Technology, etc.)
class FeaturesPills extends StatefulWidget {
  final List<VehicleFeature> features;
  final int initialVisibleCount;
  final bool expandable;

  const FeaturesPills({
    super.key,
    required this.features,
    this.initialVisibleCount = 8,
    this.expandable = true,
  });

  @override
  State<FeaturesPills> createState() => _FeaturesPillsState();
}

class _FeaturesPillsState extends State<FeaturesPills> {
  bool _isExpanded = false;

  @override
  Widget build(BuildContext context) {
    final displayFeatures = _isExpanded || !widget.expandable
        ? widget.features
        : widget.features.take(widget.initialVisibleCount).toList();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Wrap(
          spacing: 8,
          runSpacing: 8,
          children: displayFeatures.map((feature) {
            return _FeaturePill(feature: feature);
          }).toList(),
        ),

        // Expand/Collapse
        if (widget.expandable &&
            widget.features.length > widget.initialVisibleCount)
          Padding(
            padding: const EdgeInsets.only(top: 12),
            child: TextButton.icon(
              onPressed: () {
                setState(() {
                  _isExpanded = !_isExpanded;
                });
              },
              icon: Icon(_isExpanded ? Icons.expand_less : Icons.expand_more),
              label: Text(
                _isExpanded
                    ? 'Show Less'
                    : 'Show ${widget.features.length - widget.initialVisibleCount} More',
              ),
              style: TextButton.styleFrom(
                foregroundColor: const Color(0xFF001F54),
              ),
            ),
          ),
      ],
    );
  }
}

class _FeaturePill extends StatelessWidget {
  final VehicleFeature feature;

  const _FeaturePill({required this.feature});

  @override
  Widget build(BuildContext context) {
    final colors = feature.category.colors;

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: colors.background,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(
          color: colors.border,
          width: 1,
        ),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            feature.icon,
            size: 16,
            color: colors.icon,
          ),
          const SizedBox(width: 6),
          Text(
            feature.name,
            style: TextStyle(
              color: colors.text,
              fontSize: 13,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }
}

/// Vehicle Feature Model
class VehicleFeature {
  final String name;
  final IconData icon;
  final FeatureCategory category;

  const VehicleFeature({
    required this.name,
    required this.icon,
    required this.category,
  });
}

/// Feature Categories with Colors
enum FeatureCategory {
  safety,
  comfort,
  technology,
  performance,
  entertainment,
  exterior,
  interior;

  FeatureCategoryColors get colors {
    switch (this) {
      case FeatureCategory.safety:
        return const FeatureCategoryColors(
          background: Color(0xFFDCFCE7),
          border: Color(0xFF86EFAC),
          icon: Color(0xFF10B981),
          text: Color(0xFF065F46),
        );
      case FeatureCategory.comfort:
        return const FeatureCategoryColors(
          background: Color(0xFFDEEDFF),
          border: Color(0xFF93C5FD),
          icon: Color(0xFF3B82F6),
          text: Color(0xFF1E40AF),
        );
      case FeatureCategory.technology:
        return const FeatureCategoryColors(
          background: Color(0xFFE0E7FF),
          border: Color(0xFFA5B4FC),
          icon: Color(0xFF6366F1),
          text: Color(0xFF4338CA),
        );
      case FeatureCategory.performance:
        return const FeatureCategoryColors(
          background: Color(0xFFFEE2E2),
          border: Color(0xFFFCA5A5),
          icon: Color(0xFFEF4444),
          text: Color(0xFF991B1B),
        );
      case FeatureCategory.entertainment:
        return const FeatureCategoryColors(
          background: Color(0xFFFCE7F3),
          border: Color(0xFFF9A8D4),
          icon: Color(0xFFEC4899),
          text: Color(0xFF9F1239),
        );
      case FeatureCategory.exterior:
        return const FeatureCategoryColors(
          background: Color(0xFFFEF3C7),
          border: Color(0xFFFCD34D),
          icon: Color(0xFFF59E0B),
          text: Color(0xFF92400E),
        );
      case FeatureCategory.interior:
        return const FeatureCategoryColors(
          background: Color(0xFFF3E8FF),
          border: Color(0xFFD8B4FE),
          icon: Color(0xFFA855F7),
          text: Color(0xFF6B21A8),
        );
    }
  }
}

class FeatureCategoryColors {
  final Color background;
  final Color border;
  final Color icon;
  final Color text;

  const FeatureCategoryColors({
    required this.background,
    required this.border,
    required this.icon,
    required this.text,
  });
}
