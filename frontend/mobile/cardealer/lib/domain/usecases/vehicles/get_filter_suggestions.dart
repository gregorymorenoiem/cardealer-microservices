import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../repositories/vehicle_repository.dart';

/// Use case para obtener sugerencias de filtros
class GetFilterSuggestions {
  final VehicleRepository repository;

  GetFilterSuggestions(this.repository);

  /// Obtiene sugerencias para filtros (marcas, modelos, tipos de carrocería, etc)
  /// 
  /// Retorna un mapa con claves como 'makes', 'models', 'bodyTypes', etc
  /// y valores como listas de strings con las opciones disponibles
  Future<Either<Failure, Map<String, List<String>>>> call() async {
    return repository.getFilterSuggestions();
  }
}
