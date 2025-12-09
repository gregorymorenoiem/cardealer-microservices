import 'package:flutter/material.dart';
import 'package:timeline_tile/timeline_tile.dart';

/// Vehicle History Timeline
///
/// Features:
/// - Visual timeline of vehicle history
/// - Ownership records
/// - Service history
/// - Accident reports (if any)
/// - Clean title indicator
class VehicleHistoryTimeline extends StatelessWidget {
  final List<HistoryEvent> events;
  final bool hasCleanTitle;

  const VehicleHistoryTimeline({
    super.key,
    required this.events,
    this.hasCleanTitle = true,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Clean Title Badge
        if (hasCleanTitle)
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            margin: const EdgeInsets.only(bottom: 16),
            decoration: BoxDecoration(
              color: const Color(0xFF10B981).withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(8),
              border: Border.all(
                color: const Color(0xFF10B981),
                width: 1.5,
              ),
            ),
            child: const Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(
                  Icons.verified,
                  color: Color(0xFF10B981),
                  size: 20,
                ),
                SizedBox(width: 8),
                Text(
                  'Clean Title - No Accidents Reported',
                  style: TextStyle(
                    color: Color(0xFF065F46),
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ],
            ),
          ),

        // Timeline
        ListView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: events.length,
          itemBuilder: (context, index) {
            final event = events[index];
            final isFirst = index == 0;
            final isLast = index == events.length - 1;

            return TimelineTile(
              isFirst: isFirst,
              isLast: isLast,
              alignment: TimelineAlign.manual,
              lineXY: 0.0,
              indicatorStyle: IndicatorStyle(
                width: 40,
                height: 40,
                indicator: Container(
                  decoration: BoxDecoration(
                    color: event.type.color,
                    shape: BoxShape.circle,
                  ),
                  child: Icon(
                    event.type.icon,
                    color: Colors.white,
                    size: 20,
                  ),
                ),
              ),
              beforeLineStyle: LineStyle(
                color: Colors.grey.shade300,
                thickness: 2,
              ),
              endChild: Padding(
                padding: const EdgeInsets.only(left: 16, bottom: 24),
                child: _EventCard(event: event),
              ),
            );
          },
        ),
      ],
    );
  }
}

class _EventCard extends StatelessWidget {
  final HistoryEvent event;

  const _EventCard({required this.event});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: Colors.grey.shade200,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Date
          Text(
            event.date,
            style: TextStyle(
              color: Colors.grey.shade600,
              fontSize: 12,
              fontWeight: FontWeight.w500,
            ),
          ),

          const SizedBox(height: 8),

          // Title
          Text(
            event.title,
            style: const TextStyle(
              color: Color(0xFF1E293B),
              fontSize: 16,
              fontWeight: FontWeight.w600,
            ),
          ),

          if (event.description != null) ...[
            const SizedBox(height: 6),
            Text(
              event.description!,
              style: TextStyle(
                color: Colors.grey.shade700,
                fontSize: 14,
                height: 1.5,
              ),
            ),
          ],

          if (event.mileage != null) ...[
            const SizedBox(height: 12),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              decoration: BoxDecoration(
                color: Colors.grey.shade100,
                borderRadius: BorderRadius.circular(6),
              ),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(
                    Icons.speed,
                    size: 14,
                    color: Colors.grey.shade600,
                  ),
                  const SizedBox(width: 4),
                  Text(
                    '${event.mileage} mi',
                    style: TextStyle(
                      color: Colors.grey.shade700,
                      fontSize: 12,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }
}

/// History Event Model
class HistoryEvent {
  final String date;
  final String title;
  final String? description;
  final int? mileage;
  final EventType type;

  const HistoryEvent({
    required this.date,
    required this.title,
    this.description,
    this.mileage,
    required this.type,
  });
}

enum EventType {
  purchase,
  service,
  inspection,
  accident,
  registration,
  ownership;

  IconData get icon {
    switch (this) {
      case EventType.purchase:
        return Icons.shopping_cart;
      case EventType.service:
        return Icons.build;
      case EventType.inspection:
        return Icons.verified;
      case EventType.accident:
        return Icons.warning;
      case EventType.registration:
        return Icons.description;
      case EventType.ownership:
        return Icons.person;
    }
  }

  Color get color {
    switch (this) {
      case EventType.purchase:
        return const Color(0xFF3B82F6);
      case EventType.service:
        return const Color(0xFF10B981);
      case EventType.inspection:
        return const Color(0xFF6366F1);
      case EventType.accident:
        return const Color(0xFFEF4444);
      case EventType.registration:
        return const Color(0xFF8B5CF6);
      case EventType.ownership:
        return const Color(0xFF001F54);
    }
  }
}
