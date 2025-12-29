import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/responsive/responsive_helper.dart';
import '../../bloc/filter/filter_bloc.dart';
import '../../bloc/filter/filter_event.dart';
import '../../bloc/filter/filter_state.dart';
import '../../bloc/search/search_bloc.dart';
import '../../bloc/search/search_event.dart';
import '../../bloc/search/search_state.dart';
import '../../../domain/entities/vehicle.dart';
import '../../widgets/vehicles/compact_vehicle_card.dart';
import '../../widgets/empty_state_widget.dart';
import '../vehicle_detail/vehicle_detail_page.dart';
import 'widgets/filter_bottom_sheet.dart';
import 'widgets/browse_search_bar.dart';
import 'widgets/sort_dropdown.dart';
import 'widgets/active_filters_chips.dart';

/// Página de navegación y filtrado de vehículos
class BrowsePage extends StatefulWidget {
  const BrowsePage({super.key});

  @override
  State<BrowsePage> createState() => _BrowsePageState();
}

class _BrowsePageState extends State<BrowsePage> {
  final ScrollController _scrollController = ScrollController();
  final TextEditingController _searchController = TextEditingController();

  @override
  void initState() {
    super.initState();
    // Cargar sugerencias de filtros y aplicar filtros iniciales
    context.read<FilterBloc>().add(const LoadFilterSuggestions());
    context.read<FilterBloc>().add(const ApplyFilters());
  }

  @override
  void dispose() {
    _scrollController.dispose();
    _searchController.dispose();
    super.dispose();
  }

  void _showFilterBottomSheet() {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => const FilterBottomSheet(),
    );
  }

  void _onSearch(String query) {
    if (query.trim().isNotEmpty) {
      context.read<SearchBloc>().add(SearchVehiclesEvent(query: query));
    } else {
      context.read<SearchBloc>().add(const ClearSearch());
      context.read<FilterBloc>().add(const ApplyFilters());
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Explorar Vehículos'),
        actions: [
          // Botón de filtros con badge
          BlocBuilder<FilterBloc, FilterState>(
            builder: (context, state) {
              final filterCount = state is FilterLoaded
                  ? state.activeFilterCount
                  : state is FilterApplied
                      ? state.activeFilterCount
                      : 0;

              return Stack(
                children: [
                  IconButton(
                    icon: const Icon(Icons.filter_list),
                    onPressed: _showFilterBottomSheet,
                  ),
                  if (filterCount > 0)
                    Positioned(
                      right: 8,
                      top: 8,
                      child: Container(
                        padding: const EdgeInsets.all(4),
                        decoration: const BoxDecoration(
                          color: Colors.red,
                          shape: BoxShape.circle,
                        ),
                        constraints: const BoxConstraints(
                          minWidth: 16,
                          minHeight: 16,
                        ),
                        child: Text(
                          '$filterCount',
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 10,
                            fontWeight: FontWeight.bold,
                          ),
                          textAlign: TextAlign.center,
                        ),
                      ),
                    ),
                ],
              );
            },
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: () async {
          context.read<FilterBloc>().add(const ApplyFilters());
        },
        child: Column(
          children: [
            // Barra de búsqueda
            Padding(
              padding: const EdgeInsets.all(16.0),
              child: BrowseSearchBar(
                controller: _searchController,
                onSearch: _onSearch,
                onClear: () {
                  _searchController.clear();
                  context.read<SearchBloc>().add(const ClearSearch());
                  context.read<FilterBloc>().add(const ApplyFilters());
                },
              ),
            ),

            // Filtros activos y ordenamiento
            BlocBuilder<FilterBloc, FilterState>(
              builder: (context, state) {
                if (state is FilterLoaded || state is FilterApplied) {
                  final hasFilters = state is FilterLoaded
                      ? state.hasActiveFilters
                      : (state as FilterApplied).hasActiveFilters;
                  final sortOption = state is FilterLoaded
                      ? state.sortOption
                      : (state as FilterApplied).sortOption;

                  return Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 16.0,
                      vertical: 8.0,
                    ),
                    child: Row(
                      children: [
                        // Chips de filtros activos
                        if (hasFilters)
                          Expanded(
                            child: ActiveFiltersChips(
                              criteria: state is FilterLoaded
                                  ? state.criteria
                                  : (state as FilterApplied).criteria,
                              onClearAll: () {
                                context
                                    .read<FilterBloc>()
                                    .add(const ClearFilters());
                                context
                                    .read<FilterBloc>()
                                    .add(const ApplyFilters());
                              },
                            ),
                          ),

                        // Dropdown de ordenamiento
                        SortDropdown(
                          value: sortOption,
                          onChanged: (value) {
                            if (value != null) {
                              context
                                  .read<FilterBloc>()
                                  .add(UpdateSortOption(value));
                              context
                                  .read<FilterBloc>()
                                  .add(const ApplyFilters());
                            }
                          },
                        ),
                      ],
                    ),
                  );
                }
                return const SizedBox.shrink();
              },
            ),

            // Resultados
            Expanded(
              child: BlocBuilder<SearchBloc, SearchState>(
                builder: (context, searchState) {
                  if (searchState is SearchLoading) {
                    return const Center(child: CircularProgressIndicator());
                  }

                  if (searchState is SearchLoaded) {
                    return _buildResultsList(searchState.results);
                  }

                  if (searchState is SearchEmpty) {
                    return EmptyStateWidget(
                      icon: Icons.search_off,
                      title: 'Sin resultados',
                      message:
                          'No encontramos vehículos para "${searchState.query}"',
                    );
                  }

                  // Mostrar resultados de filtros
                  return BlocBuilder<FilterBloc, FilterState>(
                    builder: (context, filterState) {
                      if (filterState is FilterLoading) {
                        return const Center(child: CircularProgressIndicator());
                      }

                      if (filterState is FilterApplied) {
                        if (filterState.results.isEmpty) {
                          return const EmptyStateWidget(
                            icon: Icons.filter_alt_off,
                            title: 'Sin resultados',
                            message:
                                'No encontramos vehículos con estos filtros',
                          );
                        }
                        return _buildResultsList(filterState.results);
                      }

                      if (filterState is FilterError) {
                        return EmptyStateWidget(
                          icon: Icons.error_outline,
                          title: 'Error',
                          message: filterState.message,
                        );
                      }

                      return const EmptyStateWidget(
                        icon: Icons.search,
                        title: 'Explora vehículos',
                        message:
                            'Usa los filtros para encontrar tu vehículo ideal',
                      );
                    },
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildResultsList(List<Vehicle> vehicles) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final responsive = context.responsive;

        // Use grid on tablet/desktop, list on mobile
        if (responsive.isTablet || responsive.isDesktop) {
          return GridView.builder(
            controller: _scrollController,
            padding: EdgeInsets.all(responsive.horizontalPadding),
            gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
              crossAxisCount: responsive.gridColumns,
              childAspectRatio: 0.7,
              crossAxisSpacing: responsive.cardSpacing,
              mainAxisSpacing: responsive.cardSpacing,
            ),
            itemCount: vehicles.length,
            itemBuilder: (context, index) {
              final vehicle = vehicles[index];
              return CompactVehicleCard(
                vehicle: vehicle,
                isFeatured: vehicle.isFeatured,
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
              );
            },
          );
        }

        // List view for mobile
        return ListView.builder(
          controller: _scrollController,
          padding: EdgeInsets.all(responsive.horizontalPadding),
          itemCount: vehicles.length,
          itemBuilder: (context, index) {
            final vehicle = vehicles[index];
            return Padding(
              padding: EdgeInsets.only(bottom: responsive.cardSpacing),
              child: CompactVehicleCard(
                vehicle: vehicle,
                isFeatured: vehicle.isFeatured,
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
              ),
            );
          },
        );
      },
    );
  }
}
