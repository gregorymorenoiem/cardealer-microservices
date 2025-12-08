import 'package:dartz/dartz.dart';
import '../../entities/vehicle.dart';
import '../../repositories/vehicle_repository.dart';
import '../../../core/error/failures.dart';

/// Use case for fetching detailed information about a vehicle
class GetVehicleDetail {
  final VehicleRepository repository;

  GetVehicleDetail(this.repository);

  Future<Either<Failure, Vehicle>> call(String vehicleId) async {
    return await repository.getVehicleById(vehicleId);
  }
}
