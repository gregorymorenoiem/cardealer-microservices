import 'package:dartz/dartz.dart';
import '../../../core/error/failures.dart';

/// Use case for toggling vehicle favorite status
/// Manages local favorites storage
class ToggleFavorite {
  Future<Either<Failure, bool>> call(String vehicleId) async {
    try {
      // TODO: Implement SharedPreferences storage
      // For now, return success
      return const Right(true);
    } catch (e) {
      return Left(CacheFailure(message: 'Failed to toggle favorite: $e'));
    }
  }
}
