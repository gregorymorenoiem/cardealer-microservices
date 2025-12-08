import 'package:dartz/dartz.dart';
import '../../core/error/failures.dart';
import '../../core/network/network_info.dart';
import '../../domain/entities/vehicle.dart';
import '../../domain/repositories/vehicle_repository.dart';
import '../datasources/mock/mock_vehicle_datasource.dart';

/// Implementation of vehicle repository with dual source pattern
class VehicleRepositoryImpl implements VehicleRepository {
  final MockVehicleDataSource mockDataSource;
  final NetworkInfo networkInfo;

  // Feature flag to switch between mock and real API
  static const bool _useRealAPI = false;

  VehicleRepositoryImpl({
    required this.mockDataSource,
    required this.networkInfo,
  });

  @override
  Future<Either<Failure, List<Vehicle>>> getHeroCarouselVehicles() async {
    try {
      final vehicles = await mockDataSource.getHeroCarouselVehicles();
      return Right(vehicles);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getFeaturedGridVehicles() async {
    try {
      final vehicles = await mockDataSource.getFeaturedGridVehicles();
      return Right(vehicles);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getWeekFeaturedVehicles() async {
    try {
      final vehicles = await mockDataSource.getWeekFeaturedVehicles();
      return Right(vehicles);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getDailyDeals() async {
    try {
      final vehicles = await mockDataSource.getDailyDeals();
      return Right(vehicles);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getSUVsAndTrucks() async {
    try {
      final vehicles = await mockDataSource.getSUVsAndTrucks();
      return Right(vehicles);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getPremiumVehicles() async {
    try {
      final vehicles = await mockDataSource.getPremiumVehicles();
      return Right(vehicles);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getElectricAndHybrid() async {
    try {
      final vehicles = await mockDataSource.getElectricAndHybrid();
      return Right(vehicles);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getAllVehicles() async {
    try {
      final vehicles = await mockDataSource.getAllVehicles();
      return Right(vehicles);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, Vehicle>> getVehicleById(String id) async {
    try {
      final vehicles = await mockDataSource.getAllVehicles();
      final vehicle = vehicles.firstWhere(
        (v) => v.id == id,
        orElse: () => throw Exception('Vehicle not found'),
      );
      return Right(vehicle);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> searchVehicles({
    String? make,
    String? model,
    double? minPrice,
    double? maxPrice,
    String? bodyType,
    String? fuelType,
    String? condition,
  }) async {
    try {
      var vehicles = await mockDataSource.getAllVehicles();

      if (make != null) {
        vehicles = vehicles
            .where((v) => v.make.toLowerCase().contains(make.toLowerCase()))
            .toList();
      }

      if (model != null) {
        vehicles = vehicles
            .where((v) => v.model.toLowerCase().contains(model.toLowerCase()))
            .toList();
      }

      if (minPrice != null) {
        vehicles = vehicles.where((v) => v.price >= minPrice).toList();
      }

      if (maxPrice != null) {
        vehicles = vehicles.where((v) => v.price <= maxPrice).toList();
      }

      if (bodyType != null) {
        vehicles = vehicles.where((v) => v.bodyType == bodyType).toList();
      }

      if (fuelType != null) {
        vehicles = vehicles.where((v) => v.fuelType == fuelType).toList();
      }

      if (condition != null) {
        vehicles = vehicles.where((v) => v.condition == condition).toList();
      }

      return Right(vehicles);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }
}
