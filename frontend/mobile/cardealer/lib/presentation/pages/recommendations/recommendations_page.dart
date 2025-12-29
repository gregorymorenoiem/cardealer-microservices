import 'package:flutter/material.dart';

/// PE-006: Recommendation Engine UI (Sprint 11)
/// Sistema de recomendaciones personalizadas basado en preferencias y historial
class RecommendationsPage extends StatefulWidget {
  const RecommendationsPage({super.key});

  @override
  State<RecommendationsPage> createState() => _RecommendationsPageState();
}

class _RecommendationsPageState extends State<RecommendationsPage> {
  // Selected preferences
  final Set<String> _selectedCategories = {'suv', 'sedan'};
  RangeValues _priceRange = const RangeValues(20000, 50000);
  String _selectedLocation = 'miami';

  // Mock recommendations
  final bool _isLoading = false;
  bool _isRefreshing = false;

  final List<Map<String, dynamic>> _recommendations = [
    {
      'title': 'Toyota RAV4 2024',
      'price': 35000,
      'image': 'https://picsum.photos/300/200',
      'reason': 'Te gustan los SUV y has visto modelos similares',
      'match': 95,
      'dealer': 'Toyota Miami',
      'year': 2024,
      'mileage': 5000,
    },
    {
      'title': 'Honda CR-V 2023',
      'price': 32000,
      'image': 'https://picsum.photos/301/200',
      'reason': 'En tu rango de precio preferido',
      'match': 92,
      'dealer': 'Honda Center',
      'year': 2023,
      'mileage': 15000,
    },
    {
      'title': 'Mazda CX-5 2024',
      'price': 38000,
      'image': 'https://picsum.photos/302/200',
      'reason': 'Basado en tus búsquedas recientes',
      'match': 88,
      'dealer': 'Mazda South',
      'year': 2024,
      'mileage': 8000,
    },
  ];

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Para Ti'),
        actions: [
          IconButton(
            icon: const Icon(Icons.tune),
            onPressed: _showPreferencesDialog,
            tooltip: 'Ajustar preferencias',
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _refreshRecommendations,
        child: _isLoading
            ? const Center(child: CircularProgressIndicator())
            : SingleChildScrollView(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Header with match score
                    Container(
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        gradient: LinearGradient(
                          colors: [
                            theme.colorScheme.primaryContainer,
                            theme.colorScheme.primaryContainer.withAlpha(50),
                          ],
                        ),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Row(
                        children: [
                          Container(
                            padding: const EdgeInsets.all(12),
                            decoration: BoxDecoration(
                              color: theme.colorScheme.primary,
                              shape: BoxShape.circle,
                            ),
                            child: const Icon(
                              Icons.auto_awesome,
                              color: Colors.white,
                              size: 28,
                            ),
                          ),
                          const SizedBox(width: 16),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  'Recomendaciones Personalizadas',
                                  style: theme.textTheme.titleMedium?.copyWith(
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                const SizedBox(height: 4),
                                Text(
                                  'Basadas en tus preferencias y actividad',
                                  style: theme.textTheme.bodySmall?.copyWith(
                                    color: Colors.grey.shade700,
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 24),

                    // Active filters chips
                    if (_selectedCategories.isNotEmpty) ...[
                      Row(
                        children: [
                          const Text(
                            'Filtros activos:',
                            style: TextStyle(
                              fontSize: 13,
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: SingleChildScrollView(
                              scrollDirection: Axis.horizontal,
                              child: Row(
                                children: [
                                  ..._selectedCategories.map((category) =>
                                      Padding(
                                        padding:
                                            const EdgeInsets.only(right: 8),
                                        child: Chip(
                                          label:
                                              Text(_getCategoryName(category)),
                                          onDeleted: () {
                                            setState(() {
                                              _selectedCategories
                                                  .remove(category);
                                            });
                                          },
                                          deleteIcon:
                                              const Icon(Icons.close, size: 16),
                                        ),
                                      )),
                                  ActionChip(
                                    label: const Text('Ajustar'),
                                    onPressed: _showPreferencesDialog,
                                    avatar: const Icon(Icons.tune, size: 16),
                                  ),
                                ],
                              ),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),
                    ],

                    // Recommendations list
                    Text(
                      '${_recommendations.length} vehículos recomendados',
                      style: theme.textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 16),

                    ListView.separated(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      itemCount: _recommendations.length,
                      separatorBuilder: (context, index) =>
                          const SizedBox(height: 16),
                      itemBuilder: (context, index) {
                        final vehicle = _recommendations[index];
                        return _RecommendationCard(
                          vehicle: vehicle,
                          onTap: () {
                            // Navigate to vehicle details
                          },
                          onFavorite: () {
                            ScaffoldMessenger.of(context).showSnackBar(
                              SnackBar(
                                  content: Text(
                                      '${vehicle['title']} añadido a favoritos')),
                            );
                          },
                        );
                      },
                    ),
                    const SizedBox(height: 16),

                    // Load more button
                    Center(
                      child: OutlinedButton.icon(
                        onPressed: _isRefreshing ? null : _loadMore,
                        icon: _isRefreshing
                            ? const SizedBox(
                                width: 16,
                                height: 16,
                                child:
                                    CircularProgressIndicator(strokeWidth: 2),
                              )
                            : const Icon(Icons.refresh),
                        label: Text(_isRefreshing
                            ? 'Cargando...'
                            : 'Ver más recomendaciones'),
                      ),
                    ),
                  ],
                ),
              ),
      ),
    );
  }

  void _showPreferencesDialog() {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.8,
        minChildSize: 0.5,
        maxChildSize: 0.95,
        expand: false,
        builder: (context, scrollController) {
          return StatefulBuilder(
            builder: (context, setModalState) {
              return SingleChildScrollView(
                controller: scrollController,
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        const Text(
                          'Ajustar Recomendaciones',
                          style: TextStyle(
                            fontSize: 20,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        IconButton(
                          icon: const Icon(Icons.close),
                          onPressed: () => Navigator.pop(context),
                        ),
                      ],
                    ),
                    const SizedBox(height: 24),

                    // Categories
                    const Text(
                      'Tipos de Vehículo',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 12),
                    Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: [
                        'suv',
                        'sedan',
                        'truck',
                        'coupe',
                        'van',
                        'convertible'
                      ].map((category) {
                        final isSelected =
                            _selectedCategories.contains(category);
                        return FilterChip(
                          label: Text(_getCategoryName(category)),
                          selected: isSelected,
                          onSelected: (selected) {
                            setModalState(() {
                              setState(() {
                                if (selected) {
                                  _selectedCategories.add(category);
                                } else {
                                  _selectedCategories.remove(category);
                                }
                              });
                            });
                          },
                        );
                      }).toList(),
                    ),
                    const SizedBox(height: 24),

                    // Price Range
                    const Text(
                      'Rango de Precio',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 12),
                    RangeSlider(
                      values: _priceRange,
                      min: 0,
                      max: 100000,
                      divisions: 20,
                      labels: RangeLabels(
                        '\$${(_priceRange.start / 1000).toStringAsFixed(0)}K',
                        '\$${(_priceRange.end / 1000).toStringAsFixed(0)}K',
                      ),
                      onChanged: (values) {
                        setModalState(() {
                          setState(() {
                            _priceRange = values;
                          });
                        });
                      },
                    ),
                    Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 16),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text('\$${_priceRange.start.toInt().toString()}'),
                          Text('\$${_priceRange.end.toInt().toString()}'),
                        ],
                      ),
                    ),
                    const SizedBox(height: 24),

                    // Location
                    const Text(
                      'Ubicación Preferida',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 12),
                    DropdownButtonFormField<String>(
                      initialValue: _selectedLocation,
                      decoration: const InputDecoration(
                        border: OutlineInputBorder(),
                        prefixIcon: Icon(Icons.location_on),
                      ),
                      items: const [
                        DropdownMenuItem(
                            value: 'miami', child: Text('Miami, FL')),
                        DropdownMenuItem(
                            value: 'orlando', child: Text('Orlando, FL')),
                        DropdownMenuItem(
                            value: 'tampa', child: Text('Tampa, FL')),
                        DropdownMenuItem(
                            value: 'all', child: Text('Todas las ubicaciones')),
                      ],
                      onChanged: (value) {
                        if (value != null) {
                          setModalState(() {
                            setState(() {
                              _selectedLocation = value;
                            });
                          });
                        }
                      },
                    ),
                    const SizedBox(height: 32),

                    // Apply button
                    SizedBox(
                      width: double.infinity,
                      child: FilledButton.icon(
                        onPressed: () {
                          Navigator.pop(context);
                          _refreshRecommendations();
                        },
                        icon: const Icon(Icons.check),
                        label: const Text('Aplicar Filtros'),
                      ),
                    ),
                  ],
                ),
              );
            },
          );
        },
      ),
    );
  }

  String _getCategoryName(String category) {
    const names = {
      'suv': 'SUV',
      'sedan': 'Sedán',
      'truck': 'Pickup',
      'coupe': 'Coupé',
      'van': 'Van',
      'convertible': 'Convertible',
    };
    return names[category] ?? category;
  }

  Future<void> _refreshRecommendations() async {
    setState(() {
      _isRefreshing = true;
    });

    // Simulate API call
    await Future.delayed(const Duration(seconds: 2));

    if (mounted) {
      setState(() {
        _isRefreshing = false;
      });

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Recomendaciones actualizadas')),
      );
    }
  }

  Future<void> _loadMore() async {
    setState(() {
      _isRefreshing = true;
    });

    // Simulate API call
    await Future.delayed(const Duration(seconds: 1));

    if (mounted) {
      setState(() {
        _isRefreshing = false;
      });
    }
  }
}

/// Recommendation card widget
class _RecommendationCard extends StatelessWidget {
  final Map<String, dynamic> vehicle;
  final VoidCallback onTap;
  final VoidCallback onFavorite;

  const _RecommendationCard({
    required this.vehicle,
    required this.onTap,
    required this.onFavorite,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final match = vehicle['match'] as int;

    return Card(
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: onTap,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Image with match badge
            Stack(
              children: [
                AspectRatio(
                  aspectRatio: 16 / 9,
                  child: Container(
                    color: Colors.grey.shade300,
                    child: const Icon(Icons.directions_car, size: 64),
                  ),
                ),
                Positioned(
                  top: 12,
                  right: 12,
                  child: Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                    decoration: BoxDecoration(
                      color: _getMatchColor(match),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.star, color: Colors.white, size: 16),
                        const SizedBox(width: 4),
                        Text(
                          '$match% Match',
                          style: const TextStyle(
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                            fontSize: 12,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                Positioned(
                  top: 12,
                  left: 12,
                  child: IconButton(
                    icon: const Icon(Icons.favorite_border),
                    onPressed: onFavorite,
                    style: IconButton.styleFrom(
                      backgroundColor: Colors.white,
                      foregroundColor: Colors.red,
                    ),
                  ),
                ),
              ],
            ),

            // Details
            Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Title and price
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Expanded(
                        child: Text(
                          vehicle['title'] as String,
                          style: theme.textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                      Text(
                        '\$${vehicle['price']}',
                        style: theme.textTheme.titleLarge?.copyWith(
                          color: theme.colorScheme.primary,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),

                  // Details row
                  Row(
                    children: [
                      Icon(Icons.calendar_today,
                          size: 14, color: Colors.grey.shade600),
                      const SizedBox(width: 4),
                      Text(
                        '${vehicle['year']}',
                        style: TextStyle(
                            fontSize: 13, color: Colors.grey.shade600),
                      ),
                      const SizedBox(width: 12),
                      Icon(Icons.speed, size: 14, color: Colors.grey.shade600),
                      const SizedBox(width: 4),
                      Text(
                        '${vehicle['mileage']} km',
                        style: TextStyle(
                            fontSize: 13, color: Colors.grey.shade600),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),

                  // Dealer
                  Row(
                    children: [
                      Icon(Icons.store, size: 14, color: Colors.grey.shade600),
                      const SizedBox(width: 4),
                      Text(
                        vehicle['dealer'] as String,
                        style: TextStyle(
                            fontSize: 13, color: Colors.grey.shade600),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),

                  // Reason badge
                  Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                    decoration: BoxDecoration(
                      color: Colors.blue.shade50,
                      borderRadius: BorderRadius.circular(8),
                      border: Border.all(color: Colors.blue.shade200),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(Icons.lightbulb_outline,
                            size: 14, color: Colors.blue.shade700),
                        const SizedBox(width: 6),
                        Flexible(
                          child: Text(
                            vehicle['reason'] as String,
                            style: TextStyle(
                              fontSize: 12,
                              color: Colors.blue.shade900,
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Color _getMatchColor(int match) {
    if (match >= 90) return Colors.green;
    if (match >= 80) return Colors.blue;
    if (match >= 70) return Colors.orange;
    return Colors.grey;
  }
}
