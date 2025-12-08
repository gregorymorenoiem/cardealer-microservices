import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../entities/vehicle.dart';
import '../../repositories/vehicle_repository.dart';

/// Use case para buscar vehículos por texto
class SearchVehicles {
  final VehicleRepository repository;

  SearchVehicles(this.repository);

  /// Ejecuta la búsqueda
  ///
  /// [query] - Texto de búsqueda
  /// [limit] - Límite de resultados (opcional)
  Future<Either<Failure, List<Vehicle>>> call({
    required String query,
    int? limit,
  }) async {
    return repository.searchVehiclesByQuery(query: query, limit: limit);
  }
}
