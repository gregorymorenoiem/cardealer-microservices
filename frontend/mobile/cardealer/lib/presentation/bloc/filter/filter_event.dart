import 'package:equatable/equatable.dart';
import '../../../domain/entities/filter_criteria.dart';

/// Eventos del FilterBloc
abstract class FilterEvent extends Equatable {
  const FilterEvent();

  @override
  List<Object?> get props => [];
}

/// Evento para actualizar el rango de precio
class UpdatePriceRange extends FilterEvent {
  final double? minPrice;
  final double? maxPrice;

  const UpdatePriceRange({this.minPrice, this.maxPrice});

  @override
  List<Object?> get props => [minPrice, maxPrice];
}

/// Evento para actualizar el rango de año
class UpdateYearRange extends FilterEvent {
  final int? minYear;
  final int? maxYear;

  const UpdateYearRange({this.minYear, this.maxYear});

  @override
  List<Object?> get props => [minYear, maxYear];
}

/// Evento para actualizar las marcas seleccionadas
class UpdateMakes extends FilterEvent {
  final List<String> makes;

  const UpdateMakes(this.makes);

  @override
  List<Object> get props => [makes];
}

/// Evento para actualizar los modelos seleccionados
class UpdateModels extends FilterEvent {
  final List<String> models;

  const UpdateModels(this.models);

  @override
  List<Object> get props => [models];
}

/// Evento para actualizar los tipos de carrocería
class UpdateBodyTypes extends FilterEvent {
  final List<String> bodyTypes;

  const UpdateBodyTypes(this.bodyTypes);

  @override
  List<Object> get props => [bodyTypes];
}

/// Evento para actualizar los tipos de combustible
class UpdateFuelTypes extends FilterEvent {
  final List<String> fuelTypes;

  const UpdateFuelTypes(this.fuelTypes);

  @override
  List<Object> get props => [fuelTypes];
}

/// Evento para actualizar las transmisiones
class UpdateTransmissions extends FilterEvent {
  final List<String> transmissions;

  const UpdateTransmissions(this.transmissions);

  @override
  List<Object> get props => [transmissions];
}

/// Evento para actualizar el kilometraje máximo
class UpdateMaxMileage extends FilterEvent {
  final int? maxMileage;

  const UpdateMaxMileage(this.maxMileage);

  @override
  List<Object?> get props => [maxMileage];
}

/// Evento para actualizar la condición
class UpdateCondition extends FilterEvent {
  final String? condition;

  const UpdateCondition(this.condition);

  @override
  List<Object?> get props => [condition];
}

/// Evento para actualizar los colores
class UpdateColors extends FilterEvent {
  final List<String> colors;

  const UpdateColors(this.colors);

  @override
  List<Object> get props => [colors];
}

/// Evento para actualizar la ubicación
class UpdateLocation extends FilterEvent {
  final String? location;
  final double? maxDistance;

  const UpdateLocation({this.location, this.maxDistance});

  @override
  List<Object?> get props => [location, maxDistance];
}

/// Evento para limpiar todos los filtros
class ClearFilters extends FilterEvent {
  const ClearFilters();
}

/// Evento para aplicar los filtros
class ApplyFilters extends FilterEvent {
  const ApplyFilters();
}

/// Evento para cargar sugerencias de filtros
class LoadFilterSuggestions extends FilterEvent {
  const LoadFilterSuggestions();
}

/// Evento para actualizar la opción de ordenamiento
class UpdateSortOption extends FilterEvent {
  final SortOption sortOption;

  const UpdateSortOption(this.sortOption);

  @override
  List<Object> get props => [sortOption];
}
