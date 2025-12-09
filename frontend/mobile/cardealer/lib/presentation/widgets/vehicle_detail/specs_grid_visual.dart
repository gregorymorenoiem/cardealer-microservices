import 'package:flutter/material.dart';

/// Visual Specs Grid with Icons
///
/// Features:
/// - Icon for each specification
/// - Responsive grid layout
/// - Expand/collapse functionality
/// - Category grouping
class SpecsGridVisual extends StatefulWidget {
  final List<VehicleSpec> specs;
  final int initialVisibleCount;
  final bool expandable;

  const SpecsGridVisual({
    super.key,
    required this.specs,
    this.initialVisibleCount = 6,
    this.expandable = true,
  });

  @override
  State<SpecsGridVisual> createState() => _SpecsGridVisualState();
}

class _SpecsGridVisualState extends State<SpecsGridVisual> {
  bool _isExpanded = false;

  @override
  Widget build(BuildContext context) {
    final displaySpecs = _isExpanded || !widget.expandable
        ? widget.specs
        : widget.specs.take(widget.initialVisibleCount).toList();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        GridView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: 2,
            childAspectRatio: 2.5,
            crossAxisSpacing: 12,
            mainAxisSpacing: 12,
          ),
          itemCount: displaySpecs.length,
          itemBuilder: (context, index) {
            final spec = displaySpecs[index];
            return _SpecCard(spec: spec);
          },
        ),

        // Expand/Collapse Button
        if (widget.expandable &&
            widget.specs.length > widget.initialVisibleCount)
          Padding(
            padding: const EdgeInsets.only(top: 16),
            child: Center(
              child: TextButton.icon(
                onPressed: () {
                  setState(() {
                    _isExpanded = !_isExpanded;
                  });
                },
                icon: Icon(
                  _isExpanded ? Icons.expand_less : Icons.expand_more,
                ),
                label: Text(
                  _isExpanded ? 'Show Less' : 'Show All Specs',
                ),
                style: TextButton.styleFrom(
                  foregroundColor: const Color(0xFF001F54),
                ),
              ),
            ),
          ),
      ],
    );
  }
}

class _SpecCard extends StatelessWidget {
  final VehicleSpec spec;

  const _SpecCard({required this.spec});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: const Color(0xFFF8FAFC),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: const Color(0xFFE2E8F0),
        ),
      ),
      child: Row(
        children: [
          // Icon
          Container(
            width: 40,
            height: 40,
            decoration: BoxDecoration(
              color: const Color(0xFF001F54).withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Icon(
              spec.icon,
              color: const Color(0xFF001F54),
              size: 20,
            ),
          ),

          const SizedBox(width: 12),

          // Label and Value
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text(
                  spec.label,
                  style: TextStyle(
                    color: Colors.grey.shade600,
                    fontSize: 11,
                    fontWeight: FontWeight.w500,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                const SizedBox(height: 2),
                Text(
                  spec.value,
                  style: const TextStyle(
                    color: Color(0xFF1E293B),
                    fontSize: 14,
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
}

/// Vehicle Specification Model
class VehicleSpec {
  final String label;
  final String value;
  final IconData icon;
  final String? category;

  const VehicleSpec({
    required this.label,
    required this.value,
    required this.icon,
    this.category,
  });

  // Common spec icons
  static const IconData mileageIcon = Icons.speed;
  static const IconData engineIcon = Icons.settings;
  static const IconData transmissionIcon = Icons.sync;
  static const IconData fuelIcon = Icons.local_gas_station;
  static const IconData yearIcon = Icons.calendar_today;
  static const IconData colorIcon = Icons.palette;
  static const IconData drivetrainIcon = Icons.drive_eta;
  static const IconData seatsIcon = Icons.event_seat;
  static const IconData doorsIcon = Icons.door_front_door;
  static const IconData mpgIcon = Icons.eco;
}
