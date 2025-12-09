import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../bloc/search/search_bloc.dart';
import '../../bloc/search/search_event.dart';
import '../../bloc/search/search_state.dart';
import '../../widgets/search/search_header.dart';
import '../../widgets/search/search_suggestions.dart';
import '../../widgets/search/recent_searches.dart';
import '../../widgets/search/search_results_view.dart';
import '../../widgets/search/no_results_state.dart';
import '../../widgets/search/filter_bottom_sheet.dart';
import '../../widgets/search/sort_bottom_sheet.dart';
import '../../widgets/search/quick_filters_chips.dart';
import '../../widgets/search/search_analytics.dart';

/// Search Page - Sprint 4: Search Experience
/// Complete search experience with filters, map, and multiple views
class SearchPage extends StatefulWidget {
  const SearchPage({super.key});

  @override
  State<SearchPage> createState() => _SearchPageState();
}

class _SearchPageState extends State<SearchPage> {
  final TextEditingController _searchController = TextEditingController();
  final FocusNode _searchFocusNode = FocusNode();
  final Set<String> _activeQuickFilters = {};
  String _currentSort = 'relevance';

  @override
  void initState() {
    super.initState();
    // Initialize analytics
    SearchAnalytics().initialize();
    // Load recent searches on init
    context.read<SearchBloc>().add(const LoadRecentSearches());
  }

  @override
  void dispose() {
    _searchController.dispose();
    _searchFocusNode.dispose();
    super.dispose();
  }

  void _performSearch(String query) {
    if (query.trim().isNotEmpty) {
      _searchFocusNode.unfocus();
      // Track search in analytics
      SearchAnalytics().trackSearch(query);
      context.read<SearchBloc>().add(SearchVehiclesEvent(query: query));
    }
  }

  void _clearSearch() {
    _searchController.clear();
    _searchFocusNode.unfocus();
    context.read<SearchBloc>().add(const ClearSearch());
    context.read<SearchBloc>().add(const LoadRecentSearches());
  }

  void _showFilters() async {
    // Track filter usage
    SearchAnalytics().trackFilterUsage('advanced_filters');

    await showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => FilterBottomSheet(
        onApplyFilters: (filters) {
          // Apply filters to search
          final query = _searchController.text;
          if (query.isNotEmpty) {
            context.read<SearchBloc>().add(SearchVehiclesEvent(query: query));
          }
          Navigator.pop(context);
        },
      ),
    );
  }

  void _showSortOptions() async {
    await showModalBottomSheet(
      context: context,
      backgroundColor: Colors.transparent,
      builder: (context) => SortBottomSheet(
        currentSort: _currentSort,
        onSort: (sortOption) {
          setState(() {
            _currentSort = sortOption;
          });
          // Re-apply search with new sort
          final query = _searchController.text;
          if (query.isNotEmpty) {
            context.read<SearchBloc>().add(SearchVehiclesEvent(query: query));
          }
        },
      ),
    );
  }

  void _onQuickFilterSelected(String filterType, String filterValue) {
    setState(() {
      final filterId = '${filterType}_$filterValue';
      if (_activeQuickFilters.contains(filterId)) {
        _activeQuickFilters.remove(filterId);
      } else {
        _activeQuickFilters.add(filterId);
      }
    });

    // Track filter usage
    SearchAnalytics().trackFilterUsage(filterType);

    // Re-apply search with filters
    final query = _searchController.text;
    if (query.isNotEmpty) {
      context.read<SearchBloc>().add(SearchVehiclesEvent(query: query));
    }
  }

  String _getSortLabel(String sortOption) {
    switch (sortOption) {
      case 'relevance':
        return 'Relevancia';
      case 'price_low':
        return 'Precio: Menor a Mayor';
      case 'price_high':
        return 'Precio: Mayor a Menor';
      case 'year_new':
        return 'Más Reciente';
      case 'year_old':
        return 'Más Antiguo';
      case 'mileage_low':
        return 'Menor Kilometraje';
      case 'mileage_high':
        return 'Mayor Kilometraje';
      case 'date_new':
        return 'Nuevos Listados';
      default:
        return 'Ordenar';
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Column(
          children: [
            // SE-001: Search Header
            SearchHeader(
              controller: _searchController,
              focusNode: _searchFocusNode,
              onSearch: _performSearch,
              onClear: _clearSearch,
              onBack: () => Navigator.of(context).pop(),
              onFilterTap: _showFilters,
              onVoiceSearch: _performSearch,
            ),

            // Main content area
            Expanded(
              child: BlocBuilder<SearchBloc, SearchState>(
                builder: (context, state) {
                  if (state is SearchInitial || state is SearchRecentLoaded) {
                    // Show recent searches and suggestions
                    return SingleChildScrollView(
                      child: Column(
                        children: [
                          // SE-011: Trending Searches
                          TrendingSearchesWidget(
                            onSearchTap: (query) {
                              _searchController.text = query;
                              _performSearch(query);
                            },
                          ),

                          // SE-005: Quick Filters
                          QuickFiltersChips(
                            onFilterSelected: _onQuickFilterSelected,
                            activeFilters: _activeQuickFilters,
                          ),

                          // Recent searches
                          if (state is SearchRecentLoaded)
                            RecentSearches(
                              searches: state.recentSearches,
                              onSearchTap: (query) {
                                _searchController.text = query;
                                _performSearch(query);
                              },
                              onRemove: (query) {
                                context
                                    .read<SearchBloc>()
                                    .add(RemoveRecentSearch(query));
                              },
                              onClearAll: () {
                                context
                                    .read<SearchBloc>()
                                    .add(const ClearRecentSearches());
                              },
                            ),

                          // Search suggestions
                          SearchSuggestions(
                            query: _searchController.text,
                            onSuggestionTap: (suggestion) {
                              _searchController.text = suggestion;
                              _performSearch(suggestion);
                            },
                          ),
                        ],
                      ),
                    );
                  }

                  if (state is SearchLoading) {
                    return const Center(
                      child: CircularProgressIndicator(),
                    );
                  }

                  if (state is SearchEmpty) {
                    // SE-009: No Results State
                    // Track zero results
                    SearchAnalytics().trackZeroResults(state.query);

                    return NoResultsState(
                      query: state.query,
                      onModifyFilters: _showFilters,
                      onClearSearch: _clearSearch,
                    );
                  }

                  if (state is SearchLoaded) {
                    // SE-007: Results View with toggle
                    return Column(
                      children: [
                        // Sort button bar
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 16,
                            vertical: 8,
                          ),
                          decoration: BoxDecoration(
                            color: Colors.white,
                            boxShadow: [
                              BoxShadow(
                                color: Colors.black.withValues(alpha: 0.05),
                                blurRadius: 4,
                                offset: const Offset(0, 2),
                              ),
                            ],
                          ),
                          child: Row(
                            children: [
                              Text(
                                '${state.totalResults} results',
                                style: const TextStyle(
                                  fontSize: 14,
                                  fontWeight: FontWeight.w500,
                                ),
                              ),
                              const Spacer(),
                              // SE-006: Sort button
                              TextButton.icon(
                                onPressed: _showSortOptions,
                                icon: const Icon(Icons.sort, size: 20),
                                label: Text(_getSortLabel(_currentSort)),
                                style: TextButton.styleFrom(
                                  foregroundColor:
                                      Theme.of(context).primaryColor,
                                ),
                              ),
                            ],
                          ),
                        ),
                        Expanded(
                          child: SearchResultsView(
                            results: state.results,
                            query: state.query,
                            totalResults: state.totalResults,
                          ),
                        ),
                      ],
                    );
                  }

                  if (state is SearchError) {
                    return Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(
                            Icons.error_outline,
                            size: 64,
                            color: Colors.red.shade300,
                          ),
                          const SizedBox(height: 16),
                          Text(
                            'Error',
                            style: Theme.of(context).textTheme.headlineSmall,
                          ),
                          const SizedBox(height: 8),
                          Text(
                            state.message,
                            textAlign: TextAlign.center,
                            style: Theme.of(context).textTheme.bodyMedium,
                          ),
                          const SizedBox(height: 24),
                          ElevatedButton(
                            onPressed: _clearSearch,
                            child: const Text('Intentar de Nuevo'),
                          ),
                        ],
                      ),
                    );
                  }

                  return const SizedBox.shrink();
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}
