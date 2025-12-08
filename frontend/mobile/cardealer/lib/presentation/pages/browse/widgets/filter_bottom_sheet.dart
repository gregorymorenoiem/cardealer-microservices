import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../../core/theme/colors.dart';
import '../../../../core/theme/spacing.dart';
import '../../../bloc/filter/filter_bloc.dart';
import '../../../bloc/filter/filter_event.dart';
import '../../../bloc/filter/filter_state.dart';

/// Bottom sheet con opciones de filtrado
class FilterBottomSheet extends StatefulWidget {
  const FilterBottomSheet({super.key});

  @override
  State<FilterBottomSheet> createState() => _FilterBottomSheetState();
}

class _FilterBottomSheetState extends State<FilterBottomSheet> {
  RangeValues _priceRange = const RangeValues(0, 100000);
  RangeValues _yearRange = RangeValues(2010, DateTime.now().year.toDouble());

  @override
  Widget build(BuildContext context) {
    return DraggableScrollableSheet(
      initialChildSize: 0.9,
      minChildSize: 0.5,
      maxChildSize: 0.9,
      builder: (context, scrollController) {
        return Container(
          decoration: const BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
          ),
          child: Column(
            children: [
              // Header
              Container(
                padding: const EdgeInsets.all(AppSpacing.md),
                decoration: const BoxDecoration(
                  border: Border(
                    bottom: BorderSide(color: AppColors.border),
                  ),
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Filtros',
                      style: TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    Row(
                      children: [
                        TextButton(
                          onPressed: () {
                            context
                                .read<FilterBloc>()
                                .add(const ClearFilters());
                          },
                          child: const Text('Limpiar'),
                        ),
                        IconButton(
                          icon: const Icon(Icons.close),
                          onPressed: () => Navigator.pop(context),
                        ),
                      ],
                    ),
                  ],
                ),
              ),

              // Content
              Expanded(
                child: BlocBuilder<FilterBloc, FilterState>(
                  builder: (context, state) {
                    if (state is FilterLoaded) {
                      return ListView(
                        controller: scrollController,
                        padding: const EdgeInsets.all(AppSpacing.md),
                        children: [
                          // Rango de precio
                          _buildSection(
                            'Precio',
                            Column(
                              children: [
                                Text(
                                  '\$${_priceRange.start.toInt()} - \$${_priceRange.end.toInt()}',
                                  style: const TextStyle(
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                RangeSlider(
                                  values: _priceRange,
                                  min: 0,
                                  max: 100000,
                                  divisions: 100,
                                  labels: RangeLabels(
                                    '\$${_priceRange.start.toInt()}',
                                    '\$${_priceRange.end.toInt()}',
                                  ),
                                  onChanged: (values) {
                                    setState(() => _priceRange = values);
                                  },
                                  onChangeEnd: (values) {
                                    context.read<FilterBloc>().add(
                                          UpdatePriceRange(
                                            minPrice: values.start,
                                            maxPrice: values.end,
                                          ),
                                        );
                                  },
                                ),
                              ],
                            ),
                          ),

                          // Rango de año
                          _buildSection(
                            'Año',
                            Column(
                              children: [
                                Text(
                                  '${_yearRange.start.toInt()} - ${_yearRange.end.toInt()}',
                                  style: const TextStyle(
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                RangeSlider(
                                  values: _yearRange,
                                  min: 2000,
                                  max: DateTime.now().year.toDouble(),
                                  divisions: DateTime.now().year - 2000,
                                  labels: RangeLabels(
                                    '${_yearRange.start.toInt()}',
                                    '${_yearRange.end.toInt()}',
                                  ),
                                  onChanged: (values) {
                                    setState(() => _yearRange = values);
                                  },
                                  onChangeEnd: (values) {
                                    context.read<FilterBloc>().add(
                                          UpdateYearRange(
                                            minYear: values.start.toInt(),
                                            maxYear: values.end.toInt(),
                                          ),
                                        );
                                  },
                                ),
                              ],
                            ),
                          ),

                          // Marcas (si hay sugerencias)
                          if (state.suggestions != null &&
                              state.suggestions!.containsKey('makes'))
                            _buildSection(
                              'Marcas',
                              Wrap(
                                spacing: 8,
                                children: state.suggestions!['makes']!
                                    .take(10)
                                    .map((make) => ChoiceChip(
                                          label: Text(make),
                                          selected: state.criteria.makes
                                                  ?.contains(make) ??
                                              false,
                                          onSelected: (selected) {
                                            final makes = List<String>.from(
                                                state.criteria.makes ?? []);
                                            if (selected) {
                                              makes.add(make);
                                            } else {
                                              makes.remove(make);
                                            }
                                            context
                                                .read<FilterBloc>()
                                                .add(UpdateMakes(makes));
                                          },
                                        ))
                                    .toList(),
                              ),
                            ),

                          // Tipos de carrocería
                          if (state.suggestions != null &&
                              state.suggestions!.containsKey('bodyTypes'))
                            _buildSection(
                              'Tipo de Carrocería',
                              Wrap(
                                spacing: 8,
                                children: state.suggestions!['bodyTypes']!
                                    .map((type) => ChoiceChip(
                                          label: Text(type),
                                          selected: state.criteria.bodyTypes
                                                  ?.contains(type) ??
                                              false,
                                          onSelected: (selected) {
                                            final types = List<String>.from(
                                                state.criteria.bodyTypes ?? []);
                                            if (selected) {
                                              types.add(type);
                                            } else {
                                              types.remove(type);
                                            }
                                            context
                                                .read<FilterBloc>()
                                                .add(UpdateBodyTypes(types));
                                          },
                                        ))
                                    .toList(),
                              ),
                            ),

                          // Tipos de combustible
                          if (state.suggestions != null &&
                              state.suggestions!.containsKey('fuelTypes'))
                            _buildSection(
                              'Combustible',
                              Wrap(
                                spacing: 8,
                                children: state.suggestions!['fuelTypes']!
                                    .map((fuel) => ChoiceChip(
                                          label: Text(fuel),
                                          selected: state.criteria.fuelTypes
                                                  ?.contains(fuel) ??
                                              false,
                                          onSelected: (selected) {
                                            final fuels = List<String>.from(
                                                state.criteria.fuelTypes ?? []);
                                            if (selected) {
                                              fuels.add(fuel);
                                            } else {
                                              fuels.remove(fuel);
                                            }
                                            context
                                                .read<FilterBloc>()
                                                .add(UpdateFuelTypes(fuels));
                                          },
                                        ))
                                    .toList(),
                              ),
                            ),

                          const SizedBox(height: 80),
                        ],
                      );
                    }
                    return const Center(child: CircularProgressIndicator());
                  },
                ),
              ),

              // Footer con botón Aplicar
              Container(
                padding: const EdgeInsets.all(AppSpacing.md),
                decoration: BoxDecoration(
                  color: Colors.white,
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withValues(alpha: 0.1),
                      blurRadius: 10,
                      offset: const Offset(0, -5),
                    ),
                  ],
                ),
                child: SafeArea(
                  child: SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: () {
                        context.read<FilterBloc>().add(const ApplyFilters());
                        Navigator.pop(context);
                      },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.primary,
                        padding: const EdgeInsets.symmetric(
                          vertical: AppSpacing.md,
                        ),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                        ),
                      ),
                      child: const Text(
                        'Aplicar Filtros',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                          color: Colors.white,
                        ),
                      ),
                    ),
                  ),
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildSection(String title, Widget content) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: const TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: AppSpacing.sm),
        content,
        const SizedBox(height: AppSpacing.lg),
      ],
    );
  }
}
