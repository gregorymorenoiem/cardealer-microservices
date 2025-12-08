import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../../domain/usecases/vehicles/search_vehicles.dart';
import 'search_event.dart';
import 'search_state.dart';

/// BLoC para búsqueda de vehículos
class SearchBloc extends Bloc<SearchEvent, SearchState> {
  final SearchVehicles searchVehicles;
  final SharedPreferences sharedPreferences;

  static const String _recentSearchesKey = 'recent_searches';
  static const int _maxRecentSearches = 10;

  SearchBloc({
    required this.searchVehicles,
    required this.sharedPreferences,
  }) : super(const SearchInitial()) {
    on<SearchVehiclesEvent>(_onSearchVehicles);
    on<ClearSearch>(_onClearSearch);
    on<LoadRecentSearches>(_onLoadRecentSearches);
    on<AddToRecentSearches>(_onAddToRecentSearches);
    on<RemoveRecentSearch>(_onRemoveRecentSearch);
    on<ClearRecentSearches>(_onClearRecentSearches);
  }

  Future<void> _onSearchVehicles(
    SearchVehiclesEvent event,
    Emitter<SearchState> emit,
  ) async {
    if (event.query.trim().isEmpty) {
      emit(const SearchInitial());
      return;
    }

    emit(const SearchLoading());

    final result = await searchVehicles(
      query: event.query,
      limit: event.limit,
    );

    result.fold(
      (failure) => emit(SearchError(failure.message)),
      (vehicles) {
        if (vehicles.isEmpty) {
          emit(SearchEmpty(event.query));
        } else {
          emit(SearchLoaded(
            results: vehicles,
            query: event.query,
            totalResults: vehicles.length,
          ));
          // Agregar a búsquedas recientes
          add(AddToRecentSearches(event.query));
        }
      },
    );
  }

  void _onClearSearch(
    ClearSearch event,
    Emitter<SearchState> emit,
  ) {
    emit(const SearchInitial());
  }

  void _onLoadRecentSearches(
    LoadRecentSearches event,
    Emitter<SearchState> emit,
  ) {
    final recentSearches = _getRecentSearches();
    emit(SearchRecentLoaded(recentSearches));
  }

  Future<void> _onAddToRecentSearches(
    AddToRecentSearches event,
    Emitter<SearchState> emit,
  ) async {
    final recentSearches = _getRecentSearches();

    // Remover si ya existe
    recentSearches.remove(event.query);

    // Agregar al inicio
    recentSearches.insert(0, event.query);

    // Limitar a máximo de búsquedas recientes
    if (recentSearches.length > _maxRecentSearches) {
      recentSearches.removeRange(_maxRecentSearches, recentSearches.length);
    }

    await sharedPreferences.setStringList(_recentSearchesKey, recentSearches);
  }

  Future<void> _onRemoveRecentSearch(
    RemoveRecentSearch event,
    Emitter<SearchState> emit,
  ) async {
    final recentSearches = _getRecentSearches();
    recentSearches.remove(event.query);
    await sharedPreferences.setStringList(_recentSearchesKey, recentSearches);
    emit(SearchRecentLoaded(recentSearches));
  }

  Future<void> _onClearRecentSearches(
    ClearRecentSearches event,
    Emitter<SearchState> emit,
  ) async {
    await sharedPreferences.remove(_recentSearchesKey);
    emit(const SearchRecentLoaded([]));
  }

  List<String> _getRecentSearches() {
    return sharedPreferences.getStringList(_recentSearchesKey) ?? [];
  }
}
