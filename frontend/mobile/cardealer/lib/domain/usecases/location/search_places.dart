import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/location.dart';
import '../../repositories/location_repository.dart';

class SearchPlaces implements UseCase<List<Location>, String> {
  final LocationRepository repository;

  SearchPlaces(this.repository);

  @override
  Future<Either<Failure, List<Location>>> call(String query) async {
    if (query.trim().isEmpty) {
      return const Left(ValidationFailure(
        message: 'La búsqueda no puede estar vacía',
      ));
    }

    return await repository.searchPlaces(query);
  }
}

class GeocodeAddress implements UseCase<Location, String> {
  final LocationRepository repository;

  GeocodeAddress(this.repository);

  @override
  Future<Either<Failure, Location>> call(String address) async {
    if (address.trim().isEmpty) {
      return const Left(ValidationFailure(
        message: 'La dirección no puede estar vacía',
      ));
    }

    return await repository.geocodeAddress(address);
  }
}
