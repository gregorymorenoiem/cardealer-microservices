import 'package:dartz/dartz.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../../../core/error/failures.dart';

/// Use case to remove a vehicle from favorites
class RemoveFavorite {
  final SharedPreferences sharedPreferences;

  static const String _favoritesKey = 'favorite_vehicles';

  RemoveFavorite(this.sharedPreferences);

  Future<Either<Failure, bool>> call(String vehicleId) async {
    try {
      final favoriteIds = sharedPreferences.getStringList(_favoritesKey) ?? [];

      if (!favoriteIds.contains(vehicleId)) {
        return const Left(CacheFailure(message: 'Vehicle not in favorites'));
      }

      favoriteIds.remove(vehicleId);
      final success =
          await sharedPreferences.setStringList(_favoritesKey, favoriteIds);

      return Right(success);
    } catch (e) {
      return Left(
          CacheFailure(message: 'Failed to remove favorite: ${e.toString()}'));
    }
  }
}
