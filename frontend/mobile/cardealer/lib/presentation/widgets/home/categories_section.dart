import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';

/// Category data model
class VehicleCategory {
  final String id;
  final String name;
  final IconData icon;
  final int count;
  final Color? color;

  const VehicleCategory({
    required this.id,
    required this.name,
    required this.icon,
    this.count = 0,
    this.color,
  });
}

/// Categories section with horizontal scroll and selection animation
class CategoriesSection extends StatefulWidget {
  final List<VehicleCategory> categories;
  final Function(VehicleCategory)? onCategoryTap;
  final String? selectedCategoryId;

  const CategoriesSection({
    super.key,
    required this.categories,
    this.onCategoryTap,
    this.selectedCategoryId,
  });

  @override
  State<CategoriesSection> createState() => _CategoriesSectionState();
}

class _CategoriesSectionState extends State<CategoriesSection> {
  String? _selectedId;

  @override
  void initState() {
    super.initState();
    _selectedId = widget.selectedCategoryId;
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: AppSpacing.md),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Categorías',
                style: AppTypography.h3.copyWith(
                  color: AppColors.textPrimary,
                  fontWeight: FontWeight.bold,
                ),
              ),
              TextButton(
                onPressed: () {
                  // TODO: Navigate to all categories
                },
                child: Text(
                  'Ver todas',
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
        SizedBox(
          height: 110,
          child: ListView.builder(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: AppSpacing.md),
            itemCount: widget.categories.length,
            itemBuilder: (context, index) {
              final category = widget.categories[index];
              final isSelected = _selectedId == category.id;

              return Padding(
                padding: const EdgeInsets.only(right: AppSpacing.md),
                child: _CategoryCard(
                  category: category,
                  isSelected: isSelected,
                  onTap: () {
                    setState(() {
                      _selectedId = isSelected ? null : category.id;
                    });
                    widget.onCategoryTap?.call(category);
                  },
                ),
              );
            },
          ),
        ),
      ],
    );
  }
}

/// Category card with animation
class _CategoryCard extends StatefulWidget {
  final VehicleCategory category;
  final bool isSelected;
  final VoidCallback? onTap;

  const _CategoryCard({
    required this.category,
    required this.isSelected,
    this.onTap,
  });

  @override
  State<_CategoryCard> createState() => _CategoryCardState();
}

class _CategoryCardState extends State<_CategoryCard>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _scaleAnimation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 200),
      vsync: this,
    );

    _scaleAnimation = Tween<double>(begin: 1.0, end: 0.95).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeInOut),
    );
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _handleTapDown(TapDownDetails details) {
    _controller.forward();
  }

  void _handleTapUp(TapUpDetails details) {
    _controller.reverse();
    widget.onTap?.call();
  }

  void _handleTapCancel() {
    _controller.reverse();
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTapDown: _handleTapDown,
      onTapUp: _handleTapUp,
      onTapCancel: _handleTapCancel,
      child: AnimatedBuilder(
        animation: _scaleAnimation,
        builder: (context, child) {
          return Transform.scale(
            scale: _scaleAnimation.value,
            child: AnimatedContainer(
              duration: const Duration(milliseconds: 300),
              width: 90,
              decoration: BoxDecoration(
                gradient: widget.isSelected
                    ? const LinearGradient(
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                        colors: [
                          AppColors.primary,
                          AppColors.primaryDark,
                        ],
                      )
                    : null,
                color: widget.isSelected ? null : AppColors.surface,
                borderRadius: BorderRadius.circular(16),
                border: Border.all(
                  color:
                      widget.isSelected ? AppColors.primary : AppColors.border,
                  width: widget.isSelected ? 2 : 1,
                ),
                boxShadow: widget.isSelected
                    ? [
                        BoxShadow(
                          color: AppColors.primary.withValues(alpha: 0.3),
                          blurRadius: 12,
                          offset: const Offset(0, 4),
                        ),
                      ]
                    : null,
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  // Icon with background
                  Container(
                    width: 48,
                    height: 48,
                    decoration: BoxDecoration(
                      color: widget.isSelected
                          ? Colors.white.withValues(alpha: 0.2)
                          : widget.category.color?.withValues(alpha: 0.1) ??
                              AppColors.primary.withValues(alpha: 0.1),
                      shape: BoxShape.circle,
                    ),
                    child: Icon(
                      widget.category.icon,
                      color: widget.isSelected
                          ? Colors.white
                          : widget.category.color ?? AppColors.primary,
                      size: 28,
                    ),
                  ),
                  const SizedBox(height: AppSpacing.xs),
                  // Category name
                  Text(
                    widget.category.name,
                    style: AppTypography.caption.copyWith(
                      color: widget.isSelected
                          ? Colors.white
                          : AppColors.textPrimary,
                      fontWeight: FontWeight.w600,
                    ),
                    textAlign: TextAlign.center,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  // Count badge
                  if (widget.category.count > 0) ...[
                    const SizedBox(height: 2),
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 6,
                        vertical: 2,
                      ),
                      decoration: BoxDecoration(
                        color: widget.isSelected
                            ? AppColors.gold
                            : AppColors.accent.withValues(alpha: 0.2),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: Text(
                        '${widget.category.count}',
                        style: AppTypography.caption.copyWith(
                          color: widget.isSelected
                              ? AppColors.textPrimary
                              : AppColors.accent,
                          fontSize: 10,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                  ],
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}

/// Default categories list
List<VehicleCategory> getDefaultCategories() {
  return const [
    VehicleCategory(
      id: 'sedan',
      name: 'Sedán',
      icon: Icons.directions_car,
      count: 245,
      color: AppColors.primary,
    ),
    VehicleCategory(
      id: 'suv',
      name: 'SUV',
      icon: Icons.airport_shuttle,
      count: 198,
      color: AppColors.accent,
    ),
    VehicleCategory(
      id: 'pickup',
      name: 'Pickup',
      icon: Icons.local_shipping,
      count: 156,
      color: Color(0xFF8B5CF6),
    ),
    VehicleCategory(
      id: 'luxury',
      name: 'Lujo',
      icon: Icons.star,
      count: 89,
      color: AppColors.gold,
    ),
    VehicleCategory(
      id: 'electric',
      name: 'Eléctrico',
      icon: Icons.electric_car,
      count: 67,
      color: Color(0xFF10B981),
    ),
    VehicleCategory(
      id: 'sport',
      name: 'Deportivo',
      icon: Icons.sports_score,
      count: 54,
      color: Color(0xFFEF4444),
    ),
    VehicleCategory(
      id: 'van',
      name: 'Van',
      icon: Icons.rv_hookup,
      count: 43,
      color: Color(0xFF06B6D4),
    ),
    VehicleCategory(
      id: 'coupe',
      name: 'Coupé',
      icon: Icons.time_to_leave,
      count: 38,
      color: Color(0xFFF59E0B),
    ),
  ];
}
