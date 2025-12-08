import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';
import '../../../core/usecases/usecase.dart';
import '../../entities/location.dart';
import '../../repositories/location_repository.dart';

class GetCurrentLocation implements UseCase<Location, NoParams> {
  final LocationRepository repository;

  GetCurrentLocation(this.repository);

  @override
  Future<Either<Failure, Location>> call(NoParams params) async {
    // Check permission first
    final permissionResult = await repository.checkLocationPermission();

    return permissionResult.fold(
      (failure) => Left(failure),
      (hasPermission) async {
        if (!hasPermission) {
          // Request permission
          final requestResult = await repository.requestLocationPermission();

          return requestResult.fold(
            (failure) => Left(failure),
            (granted) async {
              if (!granted) {
                return const Left(PermissionFailure(
                  message: 'Permiso de ubicación denegado',
                ));
              }

              return await repository.getCurrentLocation();
            },
          );
        }

        return await repository.getCurrentLocation();
      },
    );
  }
}

class PermissionFailure extends Failure {
  const PermissionFailure({required super.message});
}
