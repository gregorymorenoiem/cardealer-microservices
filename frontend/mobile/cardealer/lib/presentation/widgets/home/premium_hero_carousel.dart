import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';
import '../../../core/responsive/responsive_utils.dart';
import '../../../domain/entities/vehicle.dart';
import '../../pages/vehicle_detail/vehicle_detail_page.dart';

/// Premium Hero Carousel with parallax effect and gradient overlays
class PremiumHeroCarousel extends StatefulWidget {
  final List<Vehicle> vehicles;
  final bool autoPlay;
  final Duration autoPlayDuration;
  final VoidCallback? onSeeAllTap;

  const PremiumHeroCarousel({
    super.key,
    required this.vehicles,
    this.autoPlay = true,
    this.autoPlayDuration = const Duration(seconds: 5),
    this.onSeeAllTap,
  });

  @override
  State<PremiumHeroCarousel> createState() => _PremiumHeroCarouselState();
}

class _PremiumHeroCarouselState extends State<PremiumHeroCarousel>
    with SingleTickerProviderStateMixin {
  late PageController _pageController;
  int _currentPage = 0;
  double _currentPageValue = 0.0;
  bool _isPaused = false;
  double _lastViewportFraction = 0.92;

  @override
  void initState() {
    super.initState();
    _pageController = PageController(viewportFraction: 0.92);
    _pageController.addListener(() {
      setState(() {
        _currentPageValue = _pageController.page ?? 0.0;
      });
    });

    if (widget.autoPlay && widget.vehicles.length > 1) {
      _startAutoPlay();
    }
  }

  /// Calculate viewportFraction based on screen width
  /// Mobile: Show 1 card (0.92)
  /// Tablet: Show 2 cards (0.48)
  /// Desktop: Show 3 cards (0.33)
  double _calculateViewportFraction(double width) {
    if (width >= 1024) {
      return 0.33; // Show 3 cards on desktop
    } else if (width >= 768) {
      return 0.45; // Show ~2.2 cards on large tablets
    } else if (width >= 600) {
      return 0.48; // Show 2 cards on tablets
    } else {
      return 0.92; // Show 1 card on mobile
    }
  }

  void _updatePageControllerIfNeeded(double width) {
    final newViewportFraction = _calculateViewportFraction(width);
    if (newViewportFraction != _lastViewportFraction) {
      _lastViewportFraction = newViewportFraction;
      final currentPage = _currentPage;
      _pageController.dispose();
      _pageController = PageController(
        viewportFraction: newViewportFraction,
        initialPage: currentPage,
      );
      _pageController.addListener(() {
        if (mounted) {
          setState(() {
            _currentPageValue = _pageController.page ?? 0.0;
          });
        }
      });
    }
  }

  void _startAutoPlay() {
    Future.delayed(widget.autoPlayDuration, () {
      if (mounted && !_isPaused && widget.vehicles.isNotEmpty) {
        final nextPage = (_currentPage + 1) % widget.vehicles.length;
        _pageController.animateToPage(
          nextPage,
          duration: const Duration(milliseconds: 600),
          curve: Curves.easeInOut,
        );
        setState(() {
          _currentPage = nextPage;
        });
        _startAutoPlay();
      }
    });
  }

  void _pauseAutoPlay() {
    setState(() {
      _isPaused = true;
    });
  }

  void _resumeAutoPlay() {
    setState(() {
      _isPaused = false;
    });
    if (widget.autoPlay) {
      _startAutoPlay();
    }
  }

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (widget.vehicles.isEmpty) {
      return const SizedBox.shrink();
    }

    final screenWidth = MediaQuery.of(context).size.width;
    final carouselHeight =
        context.isMobile ? 360.0 : (context.isTablet ? 420.0 : 480.0);

    // Update page controller if viewport fraction changed
    _updatePageControllerIfNeeded(screenWidth);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.symmetric(
            horizontal: AppSpacing.md,
            vertical: AppSpacing.sm,
          ),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Destacados',
                    style: AppTypography.h3.copyWith(
                      color: AppColors.textPrimary,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Text(
                    'Lo mejor para ti hoy',
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                ],
              ),
              if (widget.onSeeAllTap != null)
                TextButton(
                  onPressed: widget.onSeeAllTap,
                  child: Text(
                    'Ver todos',
                    style: AppTypography.bodyMedium.copyWith(
                      color: AppColors.primary,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
            ],
          ),
        ),
        const SizedBox(height: AppSpacing.sm),
        GestureDetector(
          onPanDown: (_) => _pauseAutoPlay(),
          onPanEnd: (_) => _resumeAutoPlay(),
          onPanCancel: _resumeAutoPlay,
          child: SizedBox(
            height: carouselHeight,
            child: PageView.builder(
              controller: _pageController,
              itemCount: widget.vehicles.length,
              onPageChanged: (index) {
                setState(() {
                  _currentPage = index;
                });
              },
              itemBuilder: (context, index) {
                final vehicle = widget.vehicles[index];
                return _ParallaxCard(
                  vehicle: vehicle,
                  pageValue: _currentPageValue,
                  index: index,
                );
              },
            ),
          ),
        ),
        const SizedBox(height: AppSpacing.md),
        // Page indicators
        Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: List.generate(
            widget.vehicles.length,
            (index) => AnimatedContainer(
              duration: const Duration(milliseconds: 300),
              width: _currentPage == index ? 32 : 8,
              height: 8,
              margin: const EdgeInsets.symmetric(horizontal: 4),
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(4),
                gradient: _currentPage == index
                    ? const LinearGradient(
                        colors: [AppColors.primary, AppColors.accent],
                      )
                    : null,
                color: _currentPage == index ? null : AppColors.border,
              ),
            ),
          ),
        ),
      ],
    );
  }
}

/// Parallax card with dynamic offset based on scroll position
class _ParallaxCard extends StatelessWidget {
  final Vehicle vehicle;
  final double pageValue;
  final int index;

  const _ParallaxCard({
    required this.vehicle,
    required this.pageValue,
    required this.index,
  });

  @override
  Widget build(BuildContext context) {
    // Calculate parallax offset
    final difference = pageValue - index;
    final scale = 1 - (difference.abs() * 0.1).clamp(0.0, 0.1);
    final opacity = (1 - (difference.abs() * 0.5)).clamp(0.0, 1.0);

    return Opacity(
      opacity: opacity,
      child: Transform.scale(
        scale: scale,
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: AppSpacing.sm),
          child: GestureDetector(
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
            child: ClipRRect(
              borderRadius: BorderRadius.circular(24),
              child: Stack(
                children: [
                  // Background image with parallax effect
                  Positioned.fill(
                    child: Transform.translate(
                      offset: Offset(difference * 50, 0),
                      child: _buildImage(),
                    ),
                  ),
                  // Gradient overlay
                  _buildGradientOverlay(),
                  // Content
                  _buildContent(context),
                  // Premium badge (optional - can be based on price or other criteria)
                  if (vehicle.price > 50000) _buildPremiumBadge(),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildImage() {
    return CachedNetworkImage(
      imageUrl: vehicle.images.isNotEmpty
          ? vehicle.images.first
          : 'https://via.placeholder.com/400x300',
      fit: BoxFit.cover,
      placeholder: (context, url) => Container(
        color: AppColors.surface,
        child: const Center(
          child: CircularProgressIndicator(),
        ),
      ),
      errorWidget: (context, url, error) => Container(
        color: AppColors.surface,
        child: const Icon(
          Icons.directions_car,
          size: 64,
          color: AppColors.textSecondary,
        ),
      ),
    );
  }

  Widget _buildGradientOverlay() {
    return Positioned.fill(
      child: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [
              Colors.transparent,
              Colors.black.withValues(alpha: 0.3),
              Colors.black.withValues(alpha: 0.8),
            ],
            stops: const [0.0, 0.5, 1.0],
          ),
        ),
      ),
    );
  }

  Widget _buildContent(BuildContext context) {
    return Positioned(
      left: AppSpacing.md,
      right: AppSpacing.md,
      bottom: AppSpacing.md,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisSize: MainAxisSize.min,
        children: [
          // Price
          Container(
            padding: const EdgeInsets.symmetric(
              horizontal: AppSpacing.sm,
              vertical: AppSpacing.xs,
            ),
            decoration: BoxDecoration(
              color: AppColors.accent,
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text(
              '\$${vehicle.price.toStringAsFixed(0)}',
              style: AppTypography.h3.copyWith(
                color: Colors.white,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          const SizedBox(height: AppSpacing.sm),
          // Title
          Text(
            '${vehicle.year} ${vehicle.make} ${vehicle.model}',
            style: AppTypography.h2.copyWith(
              color: Colors.white,
              fontWeight: FontWeight.bold,
              shadows: [
                Shadow(
                  color: Colors.black.withValues(alpha: 0.5),
                  blurRadius: 8,
                  offset: const Offset(0, 2),
                ),
              ],
            ),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),
          const SizedBox(height: AppSpacing.xs),
          // Specs
          Row(
            children: [
              _SpecChip(
                icon: Icons.speed,
                text: '${vehicle.mileage.toStringAsFixed(0)} km',
              ),
              const SizedBox(width: AppSpacing.sm),
              _SpecChip(
                icon: Icons.local_gas_station,
                text: vehicle.fuelType,
              ),
              const SizedBox(width: AppSpacing.sm),
              _SpecChip(
                icon: Icons.settings,
                text: vehicle.transmission,
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildPremiumBadge() {
    return Positioned(
      top: AppSpacing.md,
      right: AppSpacing.md,
      child: Container(
        padding: const EdgeInsets.symmetric(
          horizontal: AppSpacing.sm,
          vertical: AppSpacing.xs,
        ),
        decoration: BoxDecoration(
          gradient: const LinearGradient(
            colors: [AppColors.gold, Color(0xFFFFE55C)],
          ),
          borderRadius: BorderRadius.circular(20),
          boxShadow: [
            BoxShadow(
              color: AppColors.gold.withValues(alpha: 0.4),
              blurRadius: 8,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(
              Icons.star,
              size: 14,
              color: AppColors.textPrimary,
            ),
            const SizedBox(width: 4),
            Text(
              'PREMIUM',
              style: AppTypography.caption.copyWith(
                color: AppColors.textPrimary,
                fontWeight: FontWeight.bold,
                fontSize: 10,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Spec chip for vehicle specifications
class _SpecChip extends StatelessWidget {
  final IconData icon;
  final String text;

  const _SpecChip({
    required this.icon,
    required this.text,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.sm,
        vertical: 4,
      ),
      decoration: BoxDecoration(
        color: Colors.white.withValues(alpha: 0.25),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: Colors.white.withValues(alpha: 0.3),
          width: 1,
        ),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            icon,
            size: 14,
            color: Colors.white,
          ),
          const SizedBox(width: 4),
          Text(
            text,
            style: AppTypography.caption.copyWith(
              color: Colors.white,
              fontWeight: FontWeight.w600,
              fontSize: 11,
            ),
          ),
        ],
      ),
    );
  }
}
