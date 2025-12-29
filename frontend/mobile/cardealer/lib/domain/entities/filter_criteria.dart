import 'package:equatable/equatable.dart';

/// Entity que representa los criterios de filtrado para búsqueda de vehículos
class FilterCriteria extends Equatable {
  final double? minPrice;
  final double? maxPrice;
  final int? minYear;
  final int? maxYear;
  final List<String>? makes;
  final List<String>? models;
  final List<String>? bodyTypes;
  final List<String>? fuelTypes;
  final List<String>? transmissions;
  final String? location;
  final double? maxDistance; // km desde location
  final int? maxMileage;
  final String? condition; // 'new', 'used', 'certified'
  final List<String>? colors;
  final bool? hasWarranty;
  final bool? hasServiceHistory;

  const FilterCriteria({
    this.minPrice,
    this.maxPrice,
    this.minYear,
    this.maxYear,
    this.makes,
    this.models,
    this.bodyTypes,
    this.fuelTypes,
    this.transmissions,
    this.location,
    this.maxDistance,
    this.maxMileage,
    this.condition,
    this.colors,
    this.hasWarranty,
    this.hasServiceHistory,
  });

  /// Crea una copia con los valores modificados
  FilterCriteria copyWith({
    double? minPrice,
    double? maxPrice,
    int? minYear,
    int? maxYear,
    List<String>? makes,
    List<String>? models,
    List<String>? bodyTypes,
    List<String>? fuelTypes,
    List<String>? transmissions,
    String? location,
    double? maxDistance,
    int? maxMileage,
    String? condition,
    List<String>? colors,
    bool? hasWarranty,
    bool? hasServiceHistory,
  }) {
    return FilterCriteria(
      minPrice: minPrice ?? this.minPrice,
      maxPrice: maxPrice ?? this.maxPrice,
      minYear: minYear ?? this.minYear,
      maxYear: maxYear ?? this.maxYear,
      makes: makes ?? this.makes,
      models: models ?? this.models,
      bodyTypes: bodyTypes ?? this.bodyTypes,
      fuelTypes: fuelTypes ?? this.fuelTypes,
      transmissions: transmissions ?? this.transmissions,
      location: location ?? this.location,
      maxDistance: maxDistance ?? this.maxDistance,
      maxMileage: maxMileage ?? this.maxMileage,
      condition: condition ?? this.condition,
      colors: colors ?? this.colors,
      hasWarranty: hasWarranty ?? this.hasWarranty,
      hasServiceHistory: hasServiceHistory ?? this.hasServiceHistory,
    );
  }

  /// Limpia todos los filtros
  FilterCriteria clear() {
    return const FilterCriteria();
  }

  /// Verifica si hay algún filtro activo
  bool get hasActiveFilters {
    return minPrice != null ||
        maxPrice != null ||
        minYear != null ||
        maxYear != null ||
        (makes?.isNotEmpty ?? false) ||
        (models?.isNotEmpty ?? false) ||
        (bodyTypes?.isNotEmpty ?? false) ||
        (fuelTypes?.isNotEmpty ?? false) ||
        (transmissions?.isNotEmpty ?? false) ||
        location != null ||
        maxDistance != null ||
        maxMileage != null ||
        condition != null ||
        (colors?.isNotEmpty ?? false) ||
        hasWarranty != null ||
        hasServiceHistory != null;
  }

  /// Cuenta los filtros activos
  int get activeFilterCount {
    int count = 0;
    if (minPrice != null || maxPrice != null) count++;
    if (minYear != null || maxYear != null) count++;
    if (makes?.isNotEmpty ?? false) count++;
    if (models?.isNotEmpty ?? false) count++;
    if (bodyTypes?.isNotEmpty ?? false) count++;
    if (fuelTypes?.isNotEmpty ?? false) count++;
    if (transmissions?.isNotEmpty ?? false) count++;
    if (location != null) count++;
    if (maxDistance != null) count++;
    if (maxMileage != null) count++;
    if (condition != null) count++;
    if (colors?.isNotEmpty ?? false) count++;
    if (hasWarranty != null) count++;
    if (hasServiceHistory != null) count++;
    return count;
  }

  @override
  List<Object?> get props => [
        minPrice,
        maxPrice,
        minYear,
        maxYear,
        makes,
        models,
        bodyTypes,
        fuelTypes,
        transmissions,
        location,
        maxDistance,
        maxMileage,
        condition,
        colors,
        hasWarranty,
        hasServiceHistory,
      ];
}

/// Enum para ordenamiento
enum SortOption {
  relevance,
  priceAsc,
  priceDesc,
  yearDesc,
  yearAsc,
  mileageAsc,
  mileageDesc,
  dateDesc,
}

extension SortOptionExtension on SortOption {
  String get label {
    switch (this) {
      case SortOption.relevance:
        return 'Relevancia';
      case SortOption.priceAsc:
        return 'Precio: Menor a Mayor';
      case SortOption.priceDesc:
        return 'Precio: Mayor a Menor';
      case SortOption.yearDesc:
        return 'Año: Más Nuevo';
      case SortOption.yearAsc:
        return 'Año: Más Antiguo';
      case SortOption.mileageAsc:
        return 'Kilometraje: Menor';
      case SortOption.mileageDesc:
        return 'Kilometraje: Mayor';
      case SortOption.dateDesc:
        return 'Más Recientes';
    }
  }
}
