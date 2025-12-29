import 'package:equatable/equatable.dart';
import '../../../domain/entities/filter_criteria.dart';
import '../../../domain/entities/vehicle.dart';

/// Estados del FilterBloc
abstract class FilterState extends Equatable {
  const FilterState();

  @override
  List<Object?> get props => [];
}

/// Estado inicial
class FilterInitial extends FilterState {
  const FilterInitial();
}

/// Estado de carga
class FilterLoading extends FilterState {
  const FilterLoading();
}

/// Estado con filtros actuales
class FilterLoaded extends FilterState {
  final FilterCriteria criteria;
  final SortOption sortOption;
  final Map<String, List<String>>? suggestions;
  final bool hasActiveFilters;
  final int activeFilterCount;

  const FilterLoaded({
    required this.criteria,
    this.sortOption = SortOption.relevance,
    this.suggestions,
    this.hasActiveFilters = false,
    this.activeFilterCount = 0,
  });

  FilterLoaded copyWith({
    FilterCriteria? criteria,
    SortOption? sortOption,
    Map<String, List<String>>? suggestions,
    bool? hasActiveFilters,
    int? activeFilterCount,
  }) {
    return FilterLoaded(
      criteria: criteria ?? this.criteria,
      sortOption: sortOption ?? this.sortOption,
      suggestions: suggestions ?? this.suggestions,
      hasActiveFilters: hasActiveFilters ?? this.hasActiveFilters,
      activeFilterCount: activeFilterCount ?? this.activeFilterCount,
    );
  }

  @override
  List<Object?> get props => [
        criteria,
        sortOption,
        suggestions,
        hasActiveFilters,
        activeFilterCount,
      ];
}

/// Estado con resultados filtrados
class FilterApplied extends FilterState {
  final FilterCriteria criteria;
  final SortOption sortOption;
  final List<Vehicle> results;
  final int totalResults;
  final bool hasActiveFilters;
  final int activeFilterCount;

  const FilterApplied({
    required this.criteria,
    required this.sortOption,
    required this.results,
    required this.totalResults,
    required this.hasActiveFilters,
    required this.activeFilterCount,
  });

  @override
  List<Object> get props => [
        criteria,
        sortOption,
        results,
        totalResults,
        hasActiveFilters,
        activeFilterCount,
      ];
}

/// Estado de error
class FilterError extends FilterState {
  final String message;

  const FilterError(this.message);

  @override
  List<Object> get props => [message];
}
