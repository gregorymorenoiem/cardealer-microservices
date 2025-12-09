import 'package:flutter/material.dart';

/// Data class for sort options
class SortOption {
  final String id;
  final String title;
  final String subtitle;
  final IconData icon;

  SortOption({
    required this.id,
    required this.title,
    required this.subtitle,
    required this.icon,
  });
}

/// SE-006: Sort Options Bottom Sheet
/// Premium bottom sheet for sorting search results
class SortBottomSheet extends StatefulWidget {
  final String? currentSort;
  final Function(String sortOption) onSort;

  const SortBottomSheet({
    super.key,
    this.currentSort,
    required this.onSort,
  });

  @override
  State<SortBottomSheet> createState() => _SortBottomSheetState();
}

class _SortBottomSheetState extends State<SortBottomSheet>
    with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _slideAnimation;
  String? _selectedSort;

  final List<SortOption> _sortOptions = [
    SortOption(
      id: 'relevance',
      title: 'Relevance',
      subtitle: 'Best match for your search',
      icon: Icons.star_outline,
    ),
    SortOption(
      id: 'price_low',
      title: 'Price: Low to High',
      subtitle: 'Most affordable first',
      icon: Icons.arrow_upward,
    ),
    SortOption(
      id: 'price_high',
      title: 'Price: High to Low',
      subtitle: 'Premium vehicles first',
      icon: Icons.arrow_downward,
    ),
    SortOption(
      id: 'year_new',
      title: 'Year: Newest First',
      subtitle: 'Latest models',
      icon: Icons.new_releases_outlined,
    ),
    SortOption(
      id: 'year_old',
      title: 'Year: Oldest First',
      subtitle: 'Classic vehicles',
      icon: Icons.history,
    ),
    SortOption(
      id: 'mileage_low',
      title: 'Mileage: Lowest First',
      subtitle: 'Least driven',
      icon: Icons.speed,
    ),
    SortOption(
      id: 'mileage_high',
      title: 'Mileage: Highest First',
      subtitle: 'Most driven',
      icon: Icons.trending_up,
    ),
    SortOption(
      id: 'date_new',
      title: 'Newest Listings',
      subtitle: 'Recently added',
      icon: Icons.access_time,
    ),
  ];

  @override
  void initState() {
    super.initState();
    _selectedSort = widget.currentSort ?? 'relevance';

    _animationController = AnimationController(
      duration: const Duration(milliseconds: 300),
      vsync: this,
    );

    _slideAnimation = Tween<double>(
      begin: 1.0,
      end: 0.0,
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

  void _applySort() {
    if (_selectedSort != null) {
      widget.onSort(_selectedSort!);
      Navigator.pop(context);
    }
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _slideAnimation,
      builder: (context, child) {
        return Transform.translate(
          offset: Offset(0,
              MediaQuery.of(context).size.height * _slideAnimation.value * 0.1),
          child: Opacity(
            opacity: 1.0 - _slideAnimation.value,
            child: child,
          ),
        );
      },
      child: Container(
        decoration: const BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.vertical(
            top: Radius.circular(24),
          ),
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Handle bar
            Container(
              margin: const EdgeInsets.only(top: 12),
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                color: Colors.grey.shade300,
                borderRadius: BorderRadius.circular(2),
              ),
            ),

            // Header
            Padding(
              padding: const EdgeInsets.all(20),
              child: Row(
                children: [
                  Icon(
                    Icons.sort,
                    color: Theme.of(context).primaryColor,
                  ),
                  const SizedBox(width: 12),
                  const Text(
                    'Sort Results',
                    style: TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const Spacer(),
                  IconButton(
                    onPressed: () => Navigator.pop(context),
                    icon: const Icon(Icons.close),
                    tooltip: 'Close',
                  ),
                ],
              ),
            ),

            const Divider(height: 1),

            // Sort options list
            Flexible(
              child: ListView.builder(
                shrinkWrap: true,
                padding: const EdgeInsets.symmetric(vertical: 8),
                itemCount: _sortOptions.length,
                itemBuilder: (context, index) {
                  final option = _sortOptions[index];
                  final isSelected = _selectedSort == option.id;

                  return _SortOptionTile(
                    option: option,
                    isSelected: isSelected,
                    onTap: () {
                      setState(() {
                        _selectedSort = option.id;
                      });
                    },
                  );
                },
              ),
            ),

            // Apply button
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: Colors.white,
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.05),
                    blurRadius: 10,
                    offset: const Offset(0, -2),
                  ),
                ],
              ),
              child: SafeArea(
                child: SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: _applySort,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Theme.of(context).primaryColor,
                      foregroundColor: Colors.white,
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                      elevation: 0,
                    ),
                    child: const Text(
                      'Apply Sort',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _SortOptionTile extends StatelessWidget {
  final SortOption option;
  final bool isSelected;
  final VoidCallback onTap;

  const _SortOptionTile({
    required this.option,
    required this.isSelected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(
          horizontal: 20,
          vertical: 12,
        ),
        decoration: BoxDecoration(
          color: isSelected
              ? Theme.of(context).primaryColor.withValues(alpha: 0.08)
              : Colors.transparent,
        ),
        child: Row(
          children: [
            // Icon
            Container(
              padding: const EdgeInsets.all(10),
              decoration: BoxDecoration(
                color: isSelected
                    ? Theme.of(context).primaryColor.withValues(alpha: 0.15)
                    : Colors.grey.shade100,
                borderRadius: BorderRadius.circular(10),
              ),
              child: Icon(
                option.icon,
                color: isSelected
                    ? Theme.of(context).primaryColor
                    : Colors.grey.shade600,
                size: 24,
              ),
            ),

            const SizedBox(width: 16),

            // Title and subtitle
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    option.title,
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight:
                          isSelected ? FontWeight.w600 : FontWeight.w500,
                      color: isSelected
                          ? Theme.of(context).primaryColor
                          : Colors.black87,
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    option.subtitle,
                    style: TextStyle(
                      fontSize: 13,
                      color: Colors.grey.shade600,
                    ),
                  ),
                ],
              ),
            ),

            // Check icon
            if (isSelected)
              Icon(
                Icons.check_circle,
                color: Theme.of(context).primaryColor,
                size: 24,
              )
            else
              Icon(
                Icons.circle_outlined,
                color: Colors.grey.shade300,
                size: 24,
              ),
          ],
        ),
      ),
    );
  }
}
