import 'package:flutter/material.dart';
import '../../../core/responsive/responsive_helper.dart';
import '../../../domain/entities/vehicle.dart';
import '../../pages/vehicle_detail/vehicle_detail_page.dart';
import '../vehicles/compact_vehicle_card.dart';

/// Sponsored Listings Section - Premium ad section for paid vehicle listings
/// Features golden border, "SPONSORED" badge, and prominent placement
class SponsoredListingsSection extends StatefulWidget {
  final List<Vehicle> vehicles;
  final VoidCallback onSeeAllTap;

  const SponsoredListingsSection({
    super.key,
    required this.vehicles,
    required this.onSeeAllTap,
  });

  @override
  State<SponsoredListingsSection> createState() =>
      _SponsoredListingsSectionState();
}

class _SponsoredListingsSectionState extends State<SponsoredListingsSection>
    with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;
  late Animation<double> _shimmerAnimation;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 800),
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

    _shimmerAnimation = Tween<double>(
      begin: -1.0,
      end: 2.0,
    ).animate(CurvedAnimation(
      parent: _animationController,
      curve: Curves.easeInOut,
    ));

    _animationController.forward();

    // Repeat shimmer effect
    _animationController.addStatusListener((status) {
      if (status == AnimationStatus.completed) {
        Future.delayed(const Duration(seconds: 3), () {
          if (mounted) {
            _animationController.reset();
            _animationController.forward();
          }
        });
      }
    });
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
    final theme = Theme.of(context);

    return FadeTransition(
      opacity: _fadeAnimation,
      child: SlideTransition(
        position: _slideAnimation,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header with golden gradient background
            Container(
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  colors: [
                    Colors.amber.shade700,
                    Colors.amber.shade500,
                    Colors.amber.shade700,
                  ],
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                ),
                borderRadius: BorderRadius.circular(responsive.borderRadius),
                boxShadow: [
                  BoxShadow(
                    color: Colors.amber.withValues(alpha: 0.3),
                    blurRadius: 12,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              padding: EdgeInsets.symmetric(
                horizontal: responsive.horizontalPadding,
                vertical: responsive.cardSpacing,
              ),
              margin: EdgeInsets.symmetric(
                horizontal: responsive.horizontalPadding,
              ),
              child: Row(
                children: [
                  // Sponsor icon
                  Container(
                    padding: EdgeInsets.all(responsive.iconSize * 0.3),
                    decoration: BoxDecoration(
                      color: Colors.white.withValues(alpha: 0.2),
                      borderRadius:
                          BorderRadius.circular(responsive.borderRadius * 0.5),
                    ),
                    child: Icon(
                      Icons.stars_rounded,
                      color: Colors.white,
                      size: responsive.iconSize * 1.2,
                    ),
                  ),
                  SizedBox(width: responsive.cardSpacing),

                  // Title and subtitle
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            Flexible(
                              child: Text(
                                'Patrocinados',
                                style: theme.textTheme.titleLarge?.copyWith(
                                  fontWeight: FontWeight.bold,
                                  fontSize: responsive.titleFontSize,
                                  color: Colors.white,
                                  letterSpacing: 0.5,
                                ),
                                overflow: TextOverflow.ellipsis,
                              ),
                            ),
                            SizedBox(width: responsive.cardSpacing * 0.5),
                            // Shimmer effect badge
                            AnimatedBuilder(
                              animation: _shimmerAnimation,
                              builder: (context, child) {
                                return Container(
                                  padding: EdgeInsets.symmetric(
                                    horizontal: responsive.cardSpacing * 0.8,
                                    vertical: responsive.cardSpacing * 0.3,
                                  ),
                                  decoration: BoxDecoration(
                                    color: Colors.white.withValues(alpha: 0.3),
                                    borderRadius: BorderRadius.circular(
                                        responsive.borderRadius * 0.5),
                                  ),
                                  child: Text(
                                    'AD',
                                    style: TextStyle(
                                      fontSize: responsive.smallFontSize,
                                      fontWeight: FontWeight.bold,
                                      color: Colors.white,
                                    ),
                                  ),
                                );
                              },
                            ),
                          ],
                        ),
                        SizedBox(height: responsive.cardSpacing * 0.2),
                        Text(
                          'Destacados premium',
                          style: theme.textTheme.bodyMedium?.copyWith(
                            fontSize: responsive.bodyFontSize,
                            color: Colors.white.withValues(alpha: 0.9),
                          ),
                        ),
                      ],
                    ),
                  ),

                  // See all button
                  TextButton(
                    onPressed: widget.onSeeAllTap,
                    style: TextButton.styleFrom(
                      foregroundColor: Colors.white,
                      backgroundColor: Colors.white.withValues(alpha: 0.2),
                      padding: EdgeInsets.symmetric(
                        horizontal: responsive.cardSpacing * 1.5,
                        vertical: responsive.cardSpacing * 0.8,
                      ),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(
                            responsive.borderRadius * 0.7),
                      ),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Text(
                          'Ver Todo',
                          style: TextStyle(
                            fontSize: responsive.bodyFontSize,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        SizedBox(width: responsive.cardSpacing * 0.3),
                        Icon(
                          Icons.arrow_forward_rounded,
                          size: responsive.iconSize * 0.8,
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),

            SizedBox(height: responsive.cardSpacing * 1.5),

            // Mobile: Horizontal scroll, Tablet: Grid layout
            responsive.isMobile
                ? _buildHorizontalList(context, responsive)
                : _buildGridLayout(context, responsive),
          ],
        ),
      ),
    );
  }

  Widget _buildHorizontalList(
      BuildContext context, ResponsiveHelper responsive) {
    return SizedBox(
      height: responsive.cardHeight,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        padding: EdgeInsets.symmetric(
          horizontal: responsive.horizontalPadding,
        ),
        itemCount: widget.vehicles.length,
        physics: const BouncingScrollPhysics(),
        itemBuilder: (context, index) {
          return Padding(
            padding: EdgeInsets.only(
              right: index < widget.vehicles.length - 1
                  ? responsive.cardSpacing
                  : 0,
            ),
            child: _buildSponsoredCard(
                context, responsive, widget.vehicles[index]),
          );
        },
      ),
    );
  }

  Widget _buildGridLayout(BuildContext context, ResponsiveHelper responsive) {
    final columns = responsive.cardGridColumns;
    final screenWidth = MediaQuery.of(context).size.width;
    final padding = responsive.horizontalPadding;
    final spacing = responsive.cardSpacing;
    final availableWidth =
        screenWidth - (padding * 2) - (spacing * (columns - 1));
    final cardWidth = availableWidth / columns;
    final cardHeight = cardWidth * 0.85; // Aspect ratio for cards

    return Padding(
      padding: EdgeInsets.symmetric(horizontal: padding),
      child: Wrap(
        spacing: spacing,
        runSpacing: spacing,
        children: widget.vehicles.take(columns * 2).map((vehicle) {
          return SizedBox(
            width: cardWidth,
            height: cardHeight,
            child: _buildSponsoredCard(context, responsive, vehicle,
                customWidth: cardWidth),
          );
        }).toList(),
      ),
    );
  }

  Widget _buildSponsoredCard(
      BuildContext context, ResponsiveHelper responsive, Vehicle vehicle,
      {double? customWidth}) {
    return Container(
      width: customWidth ?? responsive.cardWidth,
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(responsive.borderRadius),
        border: Border.all(
          color: Colors.amber.shade600,
          width: 2.5,
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.amber.withValues(alpha: 0.2),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(responsive.borderRadius),
        child: Stack(
          children: [
            // Main card content
            CompactVehicleCard(
              vehicle: vehicle,
              onTap: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => VehicleDetailPage(
                      vehicleId: vehicle.id,
                    ),
                  ),
                );
              },
              onFavorite: () {
                // TODO: Toggle favorite
              },
              isFeatured: true,
            ),

            // Sponsored badge overlay (top-right)
            Positioned(
              top: responsive.cardSpacing * 0.8,
              right: responsive.cardSpacing * 0.8,
              child: Container(
                padding: EdgeInsets.symmetric(
                  horizontal: responsive.cardSpacing * 0.8,
                  vertical: responsive.cardSpacing * 0.4,
                ),
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    colors: [
                      Colors.amber.shade700,
                      Colors.amber.shade500,
                    ],
                  ),
                  borderRadius:
                      BorderRadius.circular(responsive.borderRadius * 0.5),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withValues(alpha: 0.3),
                      blurRadius: 4,
                      offset: const Offset(0, 2),
                    ),
                  ],
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      Icons.star_rounded,
                      color: Colors.white,
                      size: responsive.iconSize * 0.6,
                    ),
                    SizedBox(width: responsive.cardSpacing * 0.3),
                    Text(
                      'PATROCINADO',
                      style: TextStyle(
                        fontSize: responsive.smallFontSize * 0.85,
                        fontWeight: FontWeight.bold,
                        color: Colors.white,
                        letterSpacing: 0.5,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
