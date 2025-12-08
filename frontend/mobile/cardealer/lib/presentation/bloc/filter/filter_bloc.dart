import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../domain/entities/filter_criteria.dart';
import '../../../domain/usecases/vehicles/filter_vehicles.dart';
import '../../../domain/usecases/vehicles/get_filter_suggestions.dart';
import 'filter_event.dart';
import 'filter_state.dart';

/// BLoC para manejo de filtros de búsqueda de vehículos
class FilterBloc extends Bloc<FilterEvent, FilterState> {
  final FilterVehicles filterVehicles;
  final GetFilterSuggestions getFilterSuggestions;

  FilterCriteria _currentCriteria = const FilterCriteria();
  SortOption _currentSortOption = SortOption.relevance;

  FilterBloc({
    required this.filterVehicles,
    required this.getFilterSuggestions,
  }) : super(const FilterInitial()) {
    on<LoadFilterSuggestions>(_onLoadFilterSuggestions);
    on<UpdatePriceRange>(_onUpdatePriceRange);
    on<UpdateYearRange>(_onUpdateYearRange);
    on<UpdateMakes>(_onUpdateMakes);
    on<UpdateModels>(_onUpdateModels);
    on<UpdateBodyTypes>(_onUpdateBodyTypes);
    on<UpdateFuelTypes>(_onUpdateFuelTypes);
    on<UpdateTransmissions>(_onUpdateTransmissions);
    on<UpdateMaxMileage>(_onUpdateMaxMileage);
    on<UpdateCondition>(_onUpdateCondition);
    on<UpdateColors>(_onUpdateColors);
    on<UpdateLocation>(_onUpdateLocation);
    on<UpdateSortOption>(_onUpdateSortOption);
    on<ClearFilters>(_onClearFilters);
    on<ApplyFilters>(_onApplyFilters);
  }

  Future<void> _onLoadFilterSuggestions(
    LoadFilterSuggestions event,
    Emitter<FilterState> emit,
  ) async {
    emit(const FilterLoading());

    final result = await getFilterSuggestions();

    result.fold(
      (failure) => emit(FilterError(failure.message)),
      (suggestions) => emit(FilterLoaded(
        criteria: _currentCriteria,
        sortOption: _currentSortOption,
        suggestions: suggestions,
        hasActiveFilters: _currentCriteria.hasActiveFilters,
        activeFilterCount: _currentCriteria.activeFilterCount,
      )),
    );
  }

  void _onUpdatePriceRange(
    UpdatePriceRange event,
    Emitter<FilterState> emit,
  ) {
    _currentCriteria = _currentCriteria.copyWith(
      minPrice: event.minPrice,
      maxPrice: event.maxPrice,
    );
    _emitLoadedState(emit);
  }

  void _onUpdateYearRange(
    UpdateYearRange event,
    Emitter<FilterState> emit,
  ) {
    _currentCriteria = _currentCriteria.copyWith(
      minYear: event.minYear,
      maxYear: event.maxYear,
    );
    _emitLoadedState(emit);
  }

  void _onUpdateMakes(
    UpdateMakes event,
    Emitter<FilterState> emit,
  ) {
    _currentCriteria = _currentCriteria.copyWith(makes: event.makes);
    _emitLoadedState(emit);
  }

  void _onUpdateModels(
    UpdateModels event,
    Emitter<FilterState> emit,
  ) {
    _currentCriteria = _currentCriteria.copyWith(models: event.models);
    _emitLoadedState(emit);
  }

  void _onUpdateBodyTypes(
    UpdateBodyTypes event,
    Emitter<FilterState> emit,
  ) {
    _currentCriteria = _currentCriteria.copyWith(bodyTypes: event.bodyTypes);
    _emitLoadedState(emit);
  }

  void _onUpdateFuelTypes(
    UpdateFuelTypes event,
    Emitter<FilterState> emit,
  ) {
    _currentCriteria = _currentCriteria.copyWith(fuelTypes: event.fuelTypes);
    _emitLoadedState(emit);
  }

  void _onUpdateTransmissions(
    UpdateTransmissions event,
    Emitter<FilterState> emit,
  ) {
    _currentCriteria = _currentCriteria.copyWith(transmissions: event.transmissions);
    _emitLoadedState(emit);
  }

  void _onUpdateMaxMileage(
    UpdateMaxMileage event,
    Emitter<FilterState> emit,
  ) {
    _currentCriteria = _currentCriteria.copyWith(maxMileage: event.maxMileage);
    _emitLoadedState(emit);
  }

  void _onUpdateCondition(
    UpdateCondition event,
    Emitter<FilterState> emit,
  ) {
    _currentCriteria = _currentCriteria.copyWith(condition: event.condition);
    _emitLoadedState(emit);
  }

  void _onUpdateColors(
    UpdateColors event,
    Emitter<FilterState> emit,
  ) {
    _currentCriteria = _currentCriteria.copyWith(colors: event.colors);
    _emitLoadedState(emit);
  }

  void _onUpdateLocation(
    UpdateLocation event,
    Emitter<FilterState> emit,
  ) {
    _currentCriteria = _currentCriteria.copyWith(
      location: event.location,
      maxDistance: event.maxDistance,
    );
    _emitLoadedState(emit);
  }

  void _onUpdateSortOption(
    UpdateSortOption event,
    Emitter<FilterState> emit,
  ) {
    _currentSortOption = event.sortOption;
    _emitLoadedState(emit);
  }

  void _onClearFilters(
    ClearFilters event,
    Emitter<FilterState> emit,
  ) {
    _currentCriteria = const FilterCriteria();
    _currentSortOption = SortOption.relevance;
    _emitLoadedState(emit);
  }

  Future<void> _onApplyFilters(
    ApplyFilters event,
    Emitter<FilterState> emit,
  ) async {
    emit(const FilterLoading());

    final result = await filterVehicles(
      criteria: _currentCriteria,
      sortBy: _currentSortOption,
    );

    result.fold(
      (failure) => emit(FilterError(failure.message)),
      (vehicles) => emit(FilterApplied(
        criteria: _currentCriteria,
        sortOption: _currentSortOption,
        results: vehicles,
        totalResults: vehicles.length,
        hasActiveFilters: _currentCriteria.hasActiveFilters,
        activeFilterCount: _currentCriteria.activeFilterCount,
      )),
    );
  }

  void _emitLoadedState(Emitter<FilterState> emit) {
    final currentState = state;
    if (currentState is FilterLoaded) {
      emit(currentState.copyWith(
        criteria: _currentCriteria,
        sortOption: _currentSortOption,
        hasActiveFilters: _currentCriteria.hasActiveFilters,
        activeFilterCount: _currentCriteria.activeFilterCount,
      ));
    } else {
      emit(FilterLoaded(
        criteria: _currentCriteria,
        sortOption: _currentSortOption,
        hasActiveFilters: _currentCriteria.hasActiveFilters,
        activeFilterCount: _currentCriteria.activeFilterCount,
      ));
    }
  }
}
