import 'package:dartz/dartz.dart';
import '../../entities/vehicle.dart';
import '../../repositories/vehicle_repository.dart';
import '../../../core/error/failures.dart';

/// Use case for fetching similar vehicles
/// Based on make, model, and price range
class GetSimilarVehicles {
  final VehicleRepository repository;

  GetSimilarVehicles(this.repository);

  Future<Either<Failure, List<Vehicle>>> call({
    required String currentVehicleId,
    String? make,
    String? model,
    double? priceMin,
    double? priceMax,
    int limit = 10,
  }) async {
    return await repository.getSimilarVehicles(
      currentVehicleId: currentVehicleId,
      make: make,
      model: model,
      priceMin: priceMin,
      priceMax: priceMax,
      limit: limit,
    );
  }
}
