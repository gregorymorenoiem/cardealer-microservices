import 'package:flutter/material.dart';

/// SE-004: Filter Bottom Sheet Redesign
/// Premium filters with range sliders and multi-select chips
class FilterBottomSheet extends StatefulWidget {
  final Function(Map<String, dynamic>) onApplyFilters;

  const FilterBottomSheet({
    super.key,
    required this.onApplyFilters,
  });

  @override
  State<FilterBottomSheet> createState() => _FilterBottomSheetState();
}

class _FilterBottomSheetState extends State<FilterBottomSheet> {
  // Filter values
  RangeValues _priceRange = const RangeValues(0, 100000);
  RangeValues _yearRange = const RangeValues(2010, 2025);
  RangeValues _mileageRange = const RangeValues(0, 200000);

  final List<String> _selectedMakes = [];
  final List<String> _selectedTypes = [];
  final List<String> _selectedFuelTypes = [];

  // Available options
  static const List<String> _makes = [
    'Toyota',
    'Honda',
    'Ford',
    'Chevrolet',
    'Nissan',
    'BMW',
    'Mercedes',
    'Audi',
    'Volkswagen',
    'Hyundai'
  ];

  static const List<String> _types = [
    'Sedan',
    'SUV',
    'Truck',
    'Coupe',
    'Hatchback',
    'Wagon',
    'Van',
    'Convertible'
  ];

  static const List<String> _fuelTypes = [
    'Gasoline',
    'Diesel',
    'Electric',
    'Hybrid',
    'Plug-in Hybrid'
  ];

  void _applyFilters() {
    final filters = {
      'priceMin': _priceRange.start,
      'priceMax': _priceRange.end,
      'yearMin': _yearRange.start.round(),
      'yearMax': _yearRange.end.round(),
      'mileageMin': _mileageRange.start,
      'mileageMax': _mileageRange.end,
      'makes': _selectedMakes,
      'types': _selectedTypes,
      'fuelTypes': _selectedFuelTypes,
    };
    widget.onApplyFilters(filters);
    Navigator.of(context).pop();
  }

  void _resetFilters() {
    setState(() {
      _priceRange = const RangeValues(0, 100000);
      _yearRange = const RangeValues(2010, 2025);
      _mileageRange = const RangeValues(0, 200000);
      _selectedMakes.clear();
      _selectedTypes.clear();
      _selectedFuelTypes.clear();
    });
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      height: MediaQuery.of(context).size.height * 0.85,
      decoration: const BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      child: Column(
        children: [
          // Header
          Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              border: Border(
                bottom: BorderSide(
                  color: Colors.grey.shade200,
                  width: 1,
                ),
              ),
            ),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Filters',
                  style: Theme.of(context).textTheme.titleLarge?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                ),
                Row(
                  children: [
                    TextButton(
                      onPressed: _resetFilters,
                      child: const Text('Reset'),
                    ),
                    IconButton(
                      onPressed: () => Navigator.of(context).pop(),
                      icon: const Icon(Icons.close),
                    ),
                  ],
                ),
              ],
            ),
          ),

          // Filters content
          Expanded(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Price Range
                  _FilterSection(
                    title: 'Price Range',
                    child: Column(
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              '\$${_priceRange.start.toStringAsFixed(0)}',
                              style:
                                  const TextStyle(fontWeight: FontWeight.w600),
                            ),
                            Text(
                              '\$${_priceRange.end.toStringAsFixed(0)}',
                              style:
                                  const TextStyle(fontWeight: FontWeight.w600),
                            ),
                          ],
                        ),
                        RangeSlider(
                          values: _priceRange,
                          min: 0,
                          max: 100000,
                          divisions: 100,
                          onChanged: (values) {
                            setState(() {
                              _priceRange = values;
                            });
                          },
                        ),
                      ],
                    ),
                  ),

                  const SizedBox(height: 24),

                  // Year Range
                  _FilterSection(
                    title: 'Year',
                    child: Column(
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              '${_yearRange.start.round()}',
                              style:
                                  const TextStyle(fontWeight: FontWeight.w600),
                            ),
                            Text(
                              '${_yearRange.end.round()}',
                              style:
                                  const TextStyle(fontWeight: FontWeight.w600),
                            ),
                          ],
                        ),
                        RangeSlider(
                          values: _yearRange,
                          min: 2000,
                          max: 2025,
                          divisions: 25,
                          onChanged: (values) {
                            setState(() {
                              _yearRange = values;
                            });
                          },
                        ),
                      ],
                    ),
                  ),

                  const SizedBox(height: 24),

                  // Mileage Range
                  _FilterSection(
                    title: 'Mileage',
                    child: Column(
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              '${(_mileageRange.start / 1000).toStringAsFixed(0)}k mi',
                              style:
                                  const TextStyle(fontWeight: FontWeight.w600),
                            ),
                            Text(
                              '${(_mileageRange.end / 1000).toStringAsFixed(0)}k mi',
                              style:
                                  const TextStyle(fontWeight: FontWeight.w600),
                            ),
                          ],
                        ),
                        RangeSlider(
                          values: _mileageRange,
                          min: 0,
                          max: 200000,
                          divisions: 40,
                          onChanged: (values) {
                            setState(() {
                              _mileageRange = values;
                            });
                          },
                        ),
                      ],
                    ),
                  ),

                  const SizedBox(height: 24),

                  // Makes (SE-005: Chips)
                  _FilterSection(
                    title: 'Make',
                    child: Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: _makes.map((make) {
                        final isSelected = _selectedMakes.contains(make);
                        return FilterChip(
                          label: Text(make),
                          selected: isSelected,
                          onSelected: (selected) {
                            setState(() {
                              if (selected) {
                                _selectedMakes.add(make);
                              } else {
                                _selectedMakes.remove(make);
                              }
                            });
                          },
                          showCheckmark: false,
                          selectedColor: Theme.of(context)
                              .primaryColor
                              .withValues(alpha: 0.2),
                          labelStyle: TextStyle(
                            color: isSelected
                                ? Theme.of(context).primaryColor
                                : Colors.grey.shade700,
                            fontWeight: isSelected
                                ? FontWeight.w600
                                : FontWeight.normal,
                          ),
                        );
                      }).toList(),
                    ),
                  ),

                  const SizedBox(height: 24),

                  // Body Type
                  _FilterSection(
                    title: 'Body Type',
                    child: Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: _types.map((type) {
                        final isSelected = _selectedTypes.contains(type);
                        return FilterChip(
                          label: Text(type),
                          selected: isSelected,
                          onSelected: (selected) {
                            setState(() {
                              if (selected) {
                                _selectedTypes.add(type);
                              } else {
                                _selectedTypes.remove(type);
                              }
                            });
                          },
                          showCheckmark: false,
                          selectedColor: Theme.of(context)
                              .primaryColor
                              .withValues(alpha: 0.2),
                          labelStyle: TextStyle(
                            color: isSelected
                                ? Theme.of(context).primaryColor
                                : Colors.grey.shade700,
                            fontWeight: isSelected
                                ? FontWeight.w600
                                : FontWeight.normal,
                          ),
                        );
                      }).toList(),
                    ),
                  ),

                  const SizedBox(height: 24),

                  // Fuel Type
                  _FilterSection(
                    title: 'Fuel Type',
                    child: Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: _fuelTypes.map((fuelType) {
                        final isSelected =
                            _selectedFuelTypes.contains(fuelType);
                        return FilterChip(
                          label: Text(fuelType),
                          selected: isSelected,
                          onSelected: (selected) {
                            setState(() {
                              if (selected) {
                                _selectedFuelTypes.add(fuelType);
                              } else {
                                _selectedFuelTypes.remove(fuelType);
                              }
                            });
                          },
                          showCheckmark: false,
                          selectedColor: Theme.of(context)
                              .primaryColor
                              .withValues(alpha: 0.2),
                          labelStyle: TextStyle(
                            color: isSelected
                                ? Theme.of(context).primaryColor
                                : Colors.grey.shade700,
                            fontWeight: isSelected
                                ? FontWeight.w600
                                : FontWeight.normal,
                          ),
                        );
                      }).toList(),
                    ),
                  ),

                  const SizedBox(height: 100), // Space for button
                ],
              ),
            ),
          ),

          // Apply button
          Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: Colors.white,
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withValues(alpha: 0.05),
                  blurRadius: 10,
                  offset: const Offset(0, -5),
                ),
              ],
            ),
            child: SafeArea(
              child: SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: _applyFilters,
                  style: ElevatedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                  child: const Text(
                    'Apply Filters',
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
    );
  }
}

class _FilterSection extends StatelessWidget {
  final String title;
  final Widget child;

  const _FilterSection({
    required this.title,
    required this.child,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: Theme.of(context).textTheme.titleMedium?.copyWith(
                fontWeight: FontWeight.bold,
              ),
        ),
        const SizedBox(height: 12),
        child,
      ],
    );
  }
}
