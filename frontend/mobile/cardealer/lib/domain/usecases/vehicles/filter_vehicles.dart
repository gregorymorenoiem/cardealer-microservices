import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../entities/vehicle.dart';
import '../../entities/filter_criteria.dart';
import '../../repositories/vehicle_repository.dart';

/// Use case para filtrar vehículos con criterios avanzados
class FilterVehicles {
  final VehicleRepository repository;

  FilterVehicles(this.repository);

  /// Ejecuta el filtrado
  /// 
  /// [criteria] - Criterios de filtrado
  /// [sortBy] - Opción de ordenamiento (opcional)
  /// [page] - Número de página para paginación (opcional)
  /// [limit] - Límite de resultados por página (opcional)
  Future<Either<Failure, List<Vehicle>>> call({
    required FilterCriteria criteria,
    SortOption? sortBy,
    int? page,
    int? limit,
  }) async {
    return repository.filterVehicles(
      criteria: criteria,
      sortBy: sortBy,
      page: page,
      limit: limit,
    );
  }
}
