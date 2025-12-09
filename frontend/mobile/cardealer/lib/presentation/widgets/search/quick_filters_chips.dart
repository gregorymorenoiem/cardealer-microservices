import 'package:flutter/material.dart';

/// Data class for quick filters
class QuickFilter {
  final String id;
  final String label;
  final IconData icon;
  final String filterType;
  final String filterValue;

  QuickFilter({
    required this.id,
    required this.label,
    required this.icon,
    required this.filterType,
    required this.filterValue,
  });
}

/// SE-005: Quick Filters Chips
/// Horizontal scrollable quick access filters
class QuickFiltersChips extends StatelessWidget {
  final Function(String filterType, String filterValue) onFilterSelected;
  final Set<String> activeFilters;

  const QuickFiltersChips({
    super.key,
    required this.onFilterSelected,
    this.activeFilters = const {},
  });

  @override
  Widget build(BuildContext context) {
    final quickFilters = [
      QuickFilter(
        id: 'price_under_20k',
        label: 'Under \$20K',
        icon: Icons.attach_money,
        filterType: 'price',
        filterValue: 'under_20000',
      ),
      QuickFilter(
        id: 'electric',
        label: 'Electric',
        icon: Icons.electric_car,
        filterType: 'fuel_type',
        filterValue: 'electric',
      ),
      QuickFilter(
        id: 'low_mileage',
        label: 'Low Mileage',
        icon: Icons.speed,
        filterType: 'mileage',
        filterValue: 'under_50000',
      ),
      QuickFilter(
        id: 'new_arrival',
        label: 'New Arrivals',
        icon: Icons.fiber_new,
        filterType: 'date',
        filterValue: 'last_7_days',
      ),
      QuickFilter(
        id: 'suv',
        label: 'SUV',
        icon: Icons.directions_car,
        filterType: 'body_type',
        filterValue: 'suv',
      ),
      QuickFilter(
        id: 'sedan',
        label: 'Sedan',
        icon: Icons.directions_car_outlined,
        filterType: 'body_type',
        filterValue: 'sedan',
      ),
      QuickFilter(
        id: 'truck',
        label: 'Truck',
        icon: Icons.local_shipping,
        filterType: 'body_type',
        filterValue: 'truck',
      ),
      QuickFilter(
        id: 'certified',
        label: 'Certified',
        icon: Icons.verified,
        filterType: 'certification',
        filterValue: 'certified_pre_owned',
      ),
      QuickFilter(
        id: 'luxury',
        label: 'Luxury',
        icon: Icons.stars,
        filterType: 'category',
        filterValue: 'luxury',
      ),
      QuickFilter(
        id: 'recent_year',
        label: '2020 or Newer',
        icon: Icons.calendar_today,
        filterType: 'year',
        filterValue: '2020_or_newer',
      ),
    ];

    return Container(
      height: 50,
      margin: const EdgeInsets.symmetric(vertical: 8),
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: 16),
        itemCount: quickFilters.length,
        separatorBuilder: (context, index) => const SizedBox(width: 8),
        itemBuilder: (context, index) {
          final filter = quickFilters[index];
          final isActive = activeFilters.contains(filter.id);

          return _QuickFilterChip(
            filter: filter,
            isActive: isActive,
            onTap: () =>
                onFilterSelected(filter.filterType, filter.filterValue),
          );
        },
      ),
    );
  }
}

class _QuickFilterChip extends StatefulWidget {
  final QuickFilter filter;
  final bool isActive;
  final VoidCallback onTap;

  const _QuickFilterChip({
    required this.filter,
    required this.isActive,
    required this.onTap,
  });

  @override
  State<_QuickFilterChip> createState() => _QuickFilterChipState();
}

class _QuickFilterChipState extends State<_QuickFilterChip>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _scaleAnimation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 150),
      vsync: this,
    );

    _scaleAnimation = Tween<double>(
      begin: 1.0,
      end: 0.95,
    ).animate(CurvedAnimation(
      parent: _controller,
      curve: Curves.easeInOut,
    ));
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _onTapDown(TapDownDetails details) {
    _controller.forward();
  }

  void _onTapUp(TapUpDetails details) {
    _controller.reverse();
    widget.onTap();
  }

  void _onTapCancel() {
    _controller.reverse();
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTapDown: _onTapDown,
      onTapUp: _onTapUp,
      onTapCancel: _onTapCancel,
      child: ScaleTransition(
        scale: _scaleAnimation,
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 200),
          padding: const EdgeInsets.symmetric(
            horizontal: 16,
            vertical: 8,
          ),
          decoration: BoxDecoration(
            color:
                widget.isActive ? Theme.of(context).primaryColor : Colors.white,
            borderRadius: BorderRadius.circular(25),
            border: Border.all(
              color: widget.isActive
                  ? Theme.of(context).primaryColor
                  : Colors.grey.shade300,
              width: 1.5,
            ),
            boxShadow: widget.isActive
                ? [
                    BoxShadow(
                      color:
                          Theme.of(context).primaryColor.withValues(alpha: 0.3),
                      blurRadius: 8,
                      offset: const Offset(0, 2),
                    ),
                  ]
                : [],
          ),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(
                widget.filter.icon,
                size: 18,
                color: widget.isActive ? Colors.white : Colors.grey.shade700,
              ),
              const SizedBox(width: 6),
              Text(
                widget.filter.label,
                style: TextStyle(
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                  color: widget.isActive ? Colors.white : Colors.grey.shade800,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
