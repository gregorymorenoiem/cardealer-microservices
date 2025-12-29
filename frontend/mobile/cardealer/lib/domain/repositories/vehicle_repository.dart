import 'package:dartz/dartz.dart';
import '../../core/error/failures.dart';
import '../entities/vehicle.dart';
import '../entities/filter_criteria.dart';

/// Repository interface for vehicle operations
abstract class VehicleRepository {
  /// Get vehicles for hero carousel section
  Future<Either<Failure, List<Vehicle>>> getHeroCarouselVehicles();

  /// Get vehicles for featured grid section
  Future<Either<Failure, List<Vehicle>>> getFeaturedGridVehicles();

  /// Get week's featured vehicles
  Future<Either<Failure, List<Vehicle>>> getWeekFeaturedVehicles();

  /// Get daily deal vehicles
  Future<Either<Failure, List<Vehicle>>> getDailyDeals();

  /// Get SUVs and trucks
  Future<Either<Failure, List<Vehicle>>> getSUVsAndTrucks();

  /// Get premium vehicles
  Future<Either<Failure, List<Vehicle>>> getPremiumVehicles();

  /// Get electric and hybrid vehicles
  Future<Either<Failure, List<Vehicle>>> getElectricAndHybrid();

  /// Get all vehicles
  Future<Either<Failure, List<Vehicle>>> getAllVehicles();

  /// Get vehicle by ID
  Future<Either<Failure, Vehicle>> getVehicleById(String id);

  /// Search vehicles
  Future<Either<Failure, List<Vehicle>>> searchVehicles({
    String? make,
    String? model,
    double? minPrice,
    double? maxPrice,
    String? bodyType,
    String? fuelType,
    String? condition,
  });

  /// Search vehicles by query text
  Future<Either<Failure, List<Vehicle>>> searchVehiclesByQuery({
    required String query,
    int? limit,
  });

  /// Filter vehicles with advanced criteria
  Future<Either<Failure, List<Vehicle>>> filterVehicles({
    required FilterCriteria criteria,
    SortOption? sortBy,
    int? page,
    int? limit,
  });

  /// Get filter suggestions (makes, models, etc)
  Future<Either<Failure, Map<String, List<String>>>> getFilterSuggestions();

  /// Contact seller about a vehicle
  Future<Either<Failure, String>> contactSeller({
    required String vehicleId,
    required String sellerId,
    required String message,
  });

  /// Get similar vehicles based on criteria
  Future<Either<Failure, List<Vehicle>>> getSimilarVehicles({
    required String currentVehicleId,
    String? make,
    String? model,
    double? priceMin,
    double? priceMax,
    int limit = 10,
  });
}
