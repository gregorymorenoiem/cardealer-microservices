import 'package:flutter/material.dart';
import '../../../core/responsive/responsive_helper.dart';
import '../../../domain/entities/vehicle.dart';
import '../../pages/vehicle_detail/vehicle_detail_page.dart';
import '../vehicles/compact_vehicle_card.dart';

/// Horizontal Vehicle Section - Premium version with animations
/// Shows vehicles in horizontal scrollable list with enhanced visual effects
class HorizontalVehicleSection extends StatefulWidget {
  final String title;
  final String subtitle;
  final List<Vehicle> vehicles;
  final VoidCallback onSeeAllTap;
  final bool showBadge;
  final String badgeText;

  const HorizontalVehicleSection({
    super.key,
    required this.title,
    required this.subtitle,
    required this.vehicles,
    required this.onSeeAllTap,
    this.showBadge = false,
    this.badgeText = '',
  });

  @override
  State<HorizontalVehicleSection> createState() =>
      _HorizontalVehicleSectionState();
}

class _HorizontalVehicleSectionState extends State<HorizontalVehicleSection>
    with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 600),
      vsync: this,
    );

    _fadeAnimation = Tween<double>(
      begin: 0.0,
      end: 1.0,
    ).animate(CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeOut,
    ));

    _slideAnimation = Tween<Offset>(
      begin: const Offset(0.2, 0),
      end: Offset.zero,
    ).animate(CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeOutCubic,
    ));

    _animationController.forward();
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (widget.vehicles.isEmpty) {
      return const SizedBox.shrink();
    }

    final responsive = context.responsive;

    return FadeTransition(
      opacity: _fadeAnimation,
      child: SlideTransition(
        position: _slideAnimation,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Section Header
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16.0),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          widget.title,
                          style:
                              Theme.of(context).textTheme.titleLarge?.copyWith(
                                    fontWeight: FontWeight.bold,
                                  ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          widget.subtitle,
                          style:
                              Theme.of(context).textTheme.bodyMedium?.copyWith(
                                    color: Colors.grey.shade600,
                                  ),
                        ),
                      ],
                    ),
                  ),
                  TextButton(
                    onPressed: widget.onSeeAllTap,
                    child: const Text('See All'),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 12),
            // Mobile: Horizontal scroll, Tablet: Grid
            responsive.isMobile
                ? _buildHorizontalList(context, responsive)
                : _buildGrid(context, responsive),
          ],
        ),
      ),
    );
  }

  /// Horizontal scroll list for mobile
  Widget _buildHorizontalList(
      BuildContext context, ResponsiveHelper responsive) {
    return SizedBox(
      height: responsive.cardHeight,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        padding: EdgeInsets.symmetric(horizontal: responsive.horizontalPadding),
        itemCount: widget.vehicles.length,
        itemBuilder: (context, index) {
          return HorizontalCompactVehicleCard(
            vehicle: widget.vehicles[index],
            badgeText: widget.showBadge ? widget.badgeText : null,
            isFeatured: widget.showBadge,
            onTap: () {
              Navigator.of(context).push(
                MaterialPageRoute(
                  builder: (context) => VehicleDetailPage(
                    vehicleId: widget.vehicles[index].id,
                  ),
                ),
              );
            },
            onFavorite: () {
              // TODO: Implement favorite toggle
            },
          );
        },
      ),
    );
  }

  /// Grid layout for tablets
  Widget _buildGrid(BuildContext context, ResponsiveHelper responsive) {
    final columns = responsive.cardGridColumns;
    final screenWidth = MediaQuery.of(context).size.width;
    final padding = responsive.horizontalPadding;
    final spacing = responsive.cardSpacing;
    final availableWidth =
        screenWidth - (padding * 2) - (spacing * (columns - 1));
    final cardWidth = availableWidth / columns;
    final cardHeight = cardWidth * 0.75; // 4:3 aspect ratio

    return Padding(
      padding: EdgeInsets.symmetric(horizontal: padding),
      child: Wrap(
        spacing: spacing,
        runSpacing: spacing,
        children: widget.vehicles.take(columns * 2).map((vehicle) {
          return SizedBox(
            width: cardWidth,
            height: cardHeight,
            child: GestureDetector(
              onTap: () {
                Navigator.of(context).push(
                  MaterialPageRoute(
                    builder: (context) => VehicleDetailPage(
                      vehicleId: vehicle.id,
                    ),
                  ),
                );
              },
              child: HorizontalCompactVehicleCard(
                vehicle: vehicle,
                badgeText: widget.showBadge ? widget.badgeText : null,
                isFeatured: widget.showBadge,
                onTap: () {
                  Navigator.of(context).push(
                    MaterialPageRoute(
                      builder: (context) => VehicleDetailPage(
                        vehicleId: vehicle.id,
                      ),
                    ),
                  );
                },
                onFavorite: () {
                  // TODO: Implement favorite toggle
                },
              ),
            ),
          );
        }).toList(),
      ),
    );
  }
}
