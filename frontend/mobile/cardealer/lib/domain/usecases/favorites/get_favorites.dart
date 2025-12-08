import 'package:dartz/dartz.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../../core/error/failures.dart';
import '../../entities/vehicle.dart';
import '../../repositories/vehicle_repository.dart';

/// Use case to retrieve all favorited vehicles
class GetFavorites {
  final VehicleRepository vehicleRepository;
  final SharedPreferences sharedPreferences;

  static const String _favoritesKey = 'favorite_vehicles';

  GetFavorites(this.vehicleRepository, this.sharedPreferences);

  Future<Either<Failure, List<Vehicle>>> call() async {
    try {
      // Get favorite IDs from SharedPreferences
      final favoriteIds = sharedPreferences.getStringList(_favoritesKey) ?? [];

      if (favoriteIds.isEmpty) {
        return const Right([]);
      }

      // Fetch vehicles from repository
      final result = await vehicleRepository.getAllVehicles();

      return result.fold(
        (failure) => Left(failure),
        (vehicles) {
          // Filter vehicles that are in favorites
          final favorites = vehicles
              .where((vehicle) => favoriteIds.contains(vehicle.id))
              .toList();

          // Sort by most recently added (reverse order of IDs list)
          favorites.sort((a, b) {
            final indexA = favoriteIds.indexOf(a.id);
            final indexB = favoriteIds.indexOf(b.id);
            return indexB.compareTo(indexA);
          });

          return Right(favorites);
        },
      );
    } catch (e) {
      return Left(
          CacheFailure(message: 'Failed to load favorites: ${e.toString()}'));
    }
  }
}
