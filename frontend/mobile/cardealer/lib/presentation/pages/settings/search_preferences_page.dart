import 'package:flutter/material.dart';

/// PE-007: Search Preferences (Sprint 11)
/// Configuración de búsquedas por defecto y filtros guardados
class SearchPreferencesPage extends StatefulWidget {
  const SearchPreferencesPage({super.key});

  @override
  State<SearchPreferencesPage> createState() => _SearchPreferencesPageState();
}

class _SearchPreferencesPageState extends State<SearchPreferencesPage> {
  // Default filters
  String _defaultBrand = 'all';
  RangeValues _defaultPriceRange = const RangeValues(10000, 80000);
  int _defaultMinYear = 2020;
  int _defaultMaxMileage = 100000;

  // Preferred locations
  final List<String> _preferredLocations = ['Miami, FL', 'Orlando, FL'];

  // Sort preference
  String _defaultSort = 'newest';

  // Auto-apply filters
  bool _autoApplyFilters = true;

  // Save searches
  bool _saveSearchHistory = true;

  // Price range presets
  final Map<String, RangeValues> _pricePresets = {
    'budget': const RangeValues(0, 20000),
    'midrange': const RangeValues(20000, 50000),
    'luxury': const RangeValues(50000, 150000),
  };
  String _selectedPreset = 'midrange';

  bool _isSaving = false;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Preferencias de Búsqueda'),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Info card
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: theme.colorScheme.primaryContainer.withAlpha(50),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: theme.colorScheme.primary.withAlpha(50),
                ),
              ),
              child: Row(
                children: [
                  Icon(Icons.info_outline, color: theme.colorScheme.primary),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Text(
                      'Configura filtros predeterminados para acelerar tus búsquedas',
                      style: TextStyle(
                        color: theme.colorScheme.onSurface,
                        fontSize: 13,
                      ),
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 24),

            // Price Range Presets
            const _SectionHeader(
              icon: Icons.attach_money,
              title: 'Presupuesto Predeterminado',
              subtitle: 'Elige tu rango de precio habitual',
            ),
            const SizedBox(height: 16),

            Card(
              child: RadioGroup<String>(
                groupValue: _selectedPreset,
                onChanged: (value) {
                  setState(() {
                    _selectedPreset = value!;
                    if (value != 'custom') {
                      _defaultPriceRange = _pricePresets[value]!;
                    }
                  });
                },
                child: Column(
                  children: [
                    ListTile(
                      leading: const Radio<String>(
                        value: 'budget',
                      ),
                      title: const Text('Económico'),
                      subtitle: const Text('\$0 - \$20,000'),
                      trailing: const Icon(Icons.savings),
                      onTap: () {
                        setState(() {
                          _selectedPreset = 'budget';
                          _defaultPriceRange = _pricePresets['budget']!;
                        });
                      },
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Radio<String>(
                        value: 'midrange',
                      ),
                      title: const Text('Rango Medio'),
                      subtitle: const Text('\$20,000 - \$50,000'),
                      trailing: const Icon(Icons.trending_up),
                      onTap: () {
                        setState(() {
                          _selectedPreset = 'midrange';
                          _defaultPriceRange = _pricePresets['midrange']!;
                        });
                      },
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Radio<String>(
                        value: 'luxury',
                      ),
                      title: const Text('Lujo'),
                      subtitle: const Text('\$50,000 - \$150,000+'),
                      trailing: const Icon(Icons.diamond),
                      onTap: () {
                        setState(() {
                          _selectedPreset = 'luxury';
                          _defaultPriceRange = _pricePresets['luxury']!;
                        });
                      },
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Radio<String>(
                        value: 'custom',
                      ),
                      title: const Text('Personalizado'),
                      subtitle: Text(
                        '\$${_defaultPriceRange.start.toInt()} - \$${_defaultPriceRange.end.toInt()}',
                      ),
                      trailing: const Icon(Icons.tune),
                      onTap: () {
                        setState(() {
                          _selectedPreset = 'custom';
                        });
                      },
                    ),
                    if (_selectedPreset == 'custom') ...[
                      const Divider(height: 1),
                      Padding(
                        padding: const EdgeInsets.all(16),
                        child: RangeSlider(
                          values: _defaultPriceRange,
                          min: 0,
                          max: 200000,
                          divisions: 40,
                          labels: RangeLabels(
                            '\$${(_defaultPriceRange.start / 1000).toStringAsFixed(0)}K',
                            '\$${(_defaultPriceRange.end / 1000).toStringAsFixed(0)}K',
                          ),
                          onChanged: (values) {
                            setState(() {
                              _defaultPriceRange = values;
                            });
                          },
                        ),
                      ),
                    ],
                  ],
                ),
              ),
            ),
            const SizedBox(height: 32),

            // Default Filters
            const _SectionHeader(
              icon: Icons.filter_alt,
              title: 'Filtros Predeterminados',
              subtitle: 'Filtros aplicados automáticamente',
            ),
            const SizedBox(height: 16),

            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  children: [
                    // Brand
                    DropdownButtonFormField<String>(
                      initialValue: _defaultBrand,
                      decoration: const InputDecoration(
                        labelText: 'Marca preferida',
                        border: OutlineInputBorder(),
                        prefixIcon: Icon(Icons.directions_car),
                      ),
                      items: const [
                        DropdownMenuItem(
                            value: 'all', child: Text('Todas las marcas')),
                        DropdownMenuItem(
                            value: 'toyota', child: Text('Toyota')),
                        DropdownMenuItem(value: 'honda', child: Text('Honda')),
                        DropdownMenuItem(value: 'ford', child: Text('Ford')),
                        DropdownMenuItem(value: 'bmw', child: Text('BMW')),
                      ],
                      onChanged: (value) {
                        setState(() {
                          _defaultBrand = value!;
                        });
                      },
                    ),
                    const SizedBox(height: 16),

                    // Min Year
                    TextFormField(
                      initialValue: _defaultMinYear.toString(),
                      decoration: const InputDecoration(
                        labelText: 'Año mínimo',
                        border: OutlineInputBorder(),
                        prefixIcon: Icon(Icons.calendar_today),
                      ),
                      keyboardType: TextInputType.number,
                      onChanged: (value) {
                        final year = int.tryParse(value);
                        if (year != null) {
                          setState(() {
                            _defaultMinYear = year;
                          });
                        }
                      },
                    ),
                    const SizedBox(height: 16),

                    // Max Mileage
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            const Text('Kilometraje máximo'),
                            Text(
                              '${_defaultMaxMileage ~/ 1000}K km',
                              style: theme.textTheme.titleSmall?.copyWith(
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ],
                        ),
                        Slider(
                          value: _defaultMaxMileage.toDouble(),
                          min: 0,
                          max: 200000,
                          divisions: 20,
                          label: '${_defaultMaxMileage ~/ 1000}K km',
                          onChanged: (value) {
                            setState(() {
                              _defaultMaxMileage = value.toInt();
                            });
                          },
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 32),

            // Preferred Locations
            const _SectionHeader(
              icon: Icons.location_on,
              title: 'Ubicaciones Preferidas',
              subtitle: 'Guarda hasta 5 ubicaciones',
            ),
            const SizedBox(height: 16),

            Card(
              child: Column(
                children: [
                  ..._preferredLocations.asMap().entries.map((entry) {
                    return ListTile(
                      leading: const Icon(Icons.location_on),
                      title: Text(entry.value),
                      trailing: IconButton(
                        icon: const Icon(Icons.delete_outline),
                        onPressed: () {
                          setState(() {
                            _preferredLocations.removeAt(entry.key);
                          });
                        },
                      ),
                    );
                  }),
                  if (_preferredLocations.length < 5)
                    ListTile(
                      leading: const Icon(Icons.add),
                      title: const Text('Añadir ubicación'),
                      onTap: _addLocation,
                    ),
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Sort Preference
            const _SectionHeader(
              icon: Icons.sort,
              title: 'Ordenar por Defecto',
              subtitle: 'Orden predeterminado de resultados',
            ),
            const SizedBox(height: 16),

            Card(
              child: RadioGroup<String>(
                groupValue: _defaultSort,
                onChanged: (value) {
                  setState(() {
                    _defaultSort = value!;
                  });
                },
                child: Column(
                  children: [
                    ListTile(
                      leading: const Radio<String>(
                        value: 'newest',
                      ),
                      title: const Text('Más recientes'),
                      trailing: const Icon(Icons.new_releases),
                      onTap: () {
                        setState(() {
                          _defaultSort = 'newest';
                        });
                      },
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Radio<String>(
                        value: 'price_low',
                      ),
                      title: const Text('Precio: Menor a mayor'),
                      trailing: const Icon(Icons.arrow_downward),
                      onTap: () {
                        setState(() {
                          _defaultSort = 'price_low';
                        });
                      },
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Radio<String>(
                        value: 'price_high',
                      ),
                      title: const Text('Precio: Mayor a menor'),
                      trailing: const Icon(Icons.arrow_upward),
                      onTap: () {
                        setState(() {
                          _defaultSort = 'price_high';
                        });
                      },
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Radio<String>(
                        value: 'mileage',
                      ),
                      title: const Text('Menor kilometraje'),
                      trailing: const Icon(Icons.speed),
                      onTap: () {
                        setState(() {
                          _defaultSort = 'mileage';
                        });
                      },
                    ),
                    const Divider(height: 1),
                    ListTile(
                      leading: const Radio<String>(
                        value: 'rating',
                      ),
                      title: const Text('Mejor calificados'),
                      trailing: const Icon(Icons.star),
                      onTap: () {
                        setState(() {
                          _defaultSort = 'rating';
                        });
                      },
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 32),

            // Additional Options
            const _SectionHeader(
              icon: Icons.settings,
              title: 'Opciones Adicionales',
              subtitle: 'Configuración de búsqueda',
            ),
            const SizedBox(height: 16),

            Card(
              child: Column(
                children: [
                  SwitchListTile(
                    value: _autoApplyFilters,
                    onChanged: (value) {
                      setState(() {
                        _autoApplyFilters = value;
                      });
                    },
                    title: const Text('Aplicar Filtros Automáticamente'),
                    subtitle: const Text(
                        'Usa filtros predeterminados en cada búsqueda'),
                    secondary: const Icon(Icons.auto_awesome),
                  ),
                  const Divider(height: 1),
                  SwitchListTile(
                    value: _saveSearchHistory,
                    onChanged: (value) {
                      setState(() {
                        _saveSearchHistory = value;
                      });
                    },
                    title: const Text('Guardar Historial'),
                    subtitle: const Text('Guarda tus búsquedas recientes'),
                    secondary: const Icon(Icons.history),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 32),

            // Save Button
            SizedBox(
              width: double.infinity,
              child: FilledButton.icon(
                onPressed: _isSaving ? null : _savePreferences,
                icon: const Icon(Icons.save),
                label: _isSaving
                    ? const Text('Guardando...')
                    : const Text('Guardar Preferencias'),
              ),
            ),
            const SizedBox(height: 16),

            // Clear Button
            SizedBox(
              width: double.infinity,
              child: OutlinedButton.icon(
                onPressed: _clearHistory,
                icon: const Icon(Icons.delete_sweep),
                label: const Text('Borrar Historial de Búsquedas'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _addLocation() {
    final controller = TextEditingController();

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Añadir Ubicación'),
        content: TextField(
          controller: controller,
          decoration: const InputDecoration(
            labelText: 'Ciudad, Estado',
            hintText: 'Ej: Tampa, FL',
            border: OutlineInputBorder(),
            prefixIcon: Icon(Icons.location_city),
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () {
              if (controller.text.isNotEmpty) {
                setState(() {
                  _preferredLocations.add(controller.text);
                });
                Navigator.pop(context);
              }
            },
            child: const Text('Añadir'),
          ),
        ],
      ),
    );
  }

  Future<void> _savePreferences() async {
    setState(() {
      _isSaving = true;
    });

    // Simulate API call
    await Future.delayed(const Duration(seconds: 2));

    if (mounted) {
      setState(() {
        _isSaving = false;
      });

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Preferencias guardadas correctamente'),
          backgroundColor: Colors.green,
        ),
      );
    }
  }

  void _clearHistory() {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Borrar Historial'),
        content: const Text(
          '¿Estás seguro de que deseas borrar todo tu historial de búsquedas?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            onPressed: () {
              Navigator.pop(context);
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('Historial borrado')),
              );
            },
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            child: const Text('Borrar'),
          ),
        ],
      ),
    );
  }
}

/// Section header widget
class _SectionHeader extends StatelessWidget {
  final IconData icon;
  final String title;
  final String subtitle;

  const _SectionHeader({
    required this.icon,
    required this.title,
    required this.subtitle,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: Theme.of(context).colorScheme.primaryContainer,
            borderRadius: BorderRadius.circular(8),
          ),
          child: Icon(icon, color: Theme.of(context).colorScheme.primary),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: const TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                subtitle,
                style: TextStyle(
                  fontSize: 13,
                  color: Colors.grey.shade600,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
