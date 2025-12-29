import 'package:dartz/dartz.dart';
import '../../repositories/vehicle_repository.dart';
import '../../../core/error/failures.dart';

/// Use case for contacting a vehicle seller
/// Creates a conversation thread or sends initial message
class ContactSeller {
  final VehicleRepository repository;

  ContactSeller(this.repository);

  Future<Either<Failure, String>> call({
    required String vehicleId,
    required String sellerId,
    required String message,
  }) async {
    return await repository.contactSeller(
      vehicleId: vehicleId,
      sellerId: sellerId,
      message: message,
    );
  }
}
