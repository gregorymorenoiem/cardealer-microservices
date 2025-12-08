import 'package:flutter/material.dart';
import '../../../../domain/entities/vehicle.dart';

/// Expandable vehicle description section
class VehicleDescription extends StatefulWidget {
  final Vehicle vehicle;

  const VehicleDescription({
    super.key,
    required this.vehicle,
  });

  @override
  State<VehicleDescription> createState() => _VehicleDescriptionState();
}

class _VehicleDescriptionState extends State<VehicleDescription> {
  bool _isExpanded = false;
  static const int _maxLines = 3;

  @override
  Widget build(BuildContext context) {
    final description = widget.vehicle.description;

    if (description.isEmpty) {
      return const SizedBox.shrink();
    }

    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Descripción',
            style: Theme.of(context).textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.w600,
                ),
          ),
          const SizedBox(height: 12),
          AnimatedCrossFade(
            firstChild: Text(
              description,
              style: Theme.of(context).textTheme.bodyMedium,
              maxLines: _maxLines,
              overflow: TextOverflow.ellipsis,
            ),
            secondChild: Text(
              description,
              style: Theme.of(context).textTheme.bodyMedium,
            ),
            crossFadeState: _isExpanded
                ? CrossFadeState.showSecond
                : CrossFadeState.showFirst,
            duration: const Duration(milliseconds: 200),
          ),
          if (_shouldShowToggle(description))
            TextButton(
              onPressed: () {
                setState(() {
                  _isExpanded = !_isExpanded;
                });
              },
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(_isExpanded ? 'Ver menos' : 'Ver más'),
                  Icon(
                    _isExpanded
                        ? Icons.keyboard_arrow_up
                        : Icons.keyboard_arrow_down,
                    size: 20,
                  ),
                ],
              ),
            ),
        ],
      ),
    );
  }

  bool _shouldShowToggle(String text) {
    // Simple heuristic: show toggle if text is longer than ~150 characters
    // or has more than 3 lines
    return text.length > 150 || '\n'.allMatches(text).length >= _maxLines;
  }
}
