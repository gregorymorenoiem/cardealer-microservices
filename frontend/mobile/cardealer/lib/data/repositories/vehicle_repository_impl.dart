import 'package:dartz/dartz.dart';
import '../../core/config/api_config.dart';
import '../../core/error/failures.dart';
import '../../core/network/network_info.dart';
import '../../domain/entities/vehicle.dart';
import '../../domain/entities/filter_criteria.dart';
import '../../domain/repositories/vehicle_repository.dart';
import '../datasources/mock/mock_vehicle_datasource.dart';
import '../datasources/remote/vehicle_remote_datasource.dart';

/// Implementation of vehicle repository with dual source pattern
/// Uses remote API when available, falls back to mock data
class VehicleRepositoryImpl implements VehicleRepository {
  final MockVehicleDataSource mockDataSource;
  final VehicleRemoteDataSource? remoteDataSource;
  final NetworkInfo networkInfo;

  /// Whether to use mock data (controlled by ApiConfig.enableMockData)
  bool get _useMock => ApiConfig.enableMockData || remoteDataSource == null;

  VehicleRepositoryImpl({
    required this.mockDataSource,
    this.remoteDataSource,
    required this.networkInfo,
  });

  @override
  Future<Either<Failure, List<Vehicle>>> getHeroCarouselVehicles() async {
    try {
      if (_useMock) {
        final vehicles = await mockDataSource.getHeroCarouselVehicles();
        return Right(vehicles);
      }
      final vehicles = await remoteDataSource!.getHeroCarouselVehicles();
      return Right(vehicles);
    } on ApiException catch (e) {
      return Left(ServerFailure(message: e.message));
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getFeaturedGridVehicles() async {
    try {
      if (_useMock) {
        final vehicles = await mockDataSource.getFeaturedGridVehicles();
        return Right(vehicles);
      }
      final vehicles = await remoteDataSource!.getFeaturedGridVehicles();
      return Right(vehicles);
    } on ApiException catch (e) {
      return Left(ServerFailure(message: e.message));
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getWeekFeaturedVehicles() async {
    try {
      if (_useMock) {
        final vehicles = await mockDataSource.getWeekFeaturedVehicles();
        return Right(vehicles);
      }
      final vehicles = await remoteDataSource!.getWeekFeaturedVehicles();
      return Right(vehicles);
    } on ApiException catch (e) {
      return Left(ServerFailure(message: e.message));
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getDailyDeals() async {
    try {
      if (_useMock) {
        final vehicles = await mockDataSource.getDailyDeals();
        return Right(vehicles);
      }
      final vehicles = await remoteDataSource!.getDailyDeals();
      return Right(vehicles);
    } on ApiException catch (e) {
      return Left(ServerFailure(message: e.message));
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getSUVsAndTrucks() async {
    try {
      if (_useMock) {
        final vehicles = await mockDataSource.getSUVsAndTrucks();
        return Right(vehicles);
      }
      final vehicles = await remoteDataSource!.getSUVsAndTrucks();
      return Right(vehicles);
    } on ApiException catch (e) {
      return Left(ServerFailure(message: e.message));
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getPremiumVehicles() async {
    try {
      if (_useMock) {
        final vehicles = await mockDataSource.getPremiumVehicles();
        return Right(vehicles);
      }
      final vehicles = await remoteDataSource!.getPremiumVehicles();
      return Right(vehicles);
    } on ApiException catch (e) {
      return Left(ServerFailure(message: e.message));
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getElectricAndHybrid() async {
    try {
      if (_useMock) {
        final vehicles = await mockDataSource.getElectricAndHybrid();
        return Right(vehicles);
      }
      final vehicles = await remoteDataSource!.getElectricAndHybrid();
      return Right(vehicles);
    } on ApiException catch (e) {
      return Left(ServerFailure(message: e.message));
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getAllVehicles() async {
    try {
      if (_useMock) {
        final vehicles = await mockDataSource.getAllVehicles();
        return Right(vehicles);
      }
      final response = await remoteDataSource!.getVehicles();
      return Right(response.items);
    } on ApiException catch (e) {
      return Left(ServerFailure(message: e.message));
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, Vehicle>> getVehicleById(String id) async {
    try {
      if (_useMock) {
        final vehicles = await mockDataSource.getAllVehicles();
        final vehicle = vehicles.firstWhere(
          (v) => v.id == id,
          orElse: () => throw Exception('Vehicle not found'),
        );
        return Right(vehicle);
      }
      final vehicle = await remoteDataSource!.getVehicleById(id);
      return Right(vehicle);
    } on ApiException catch (e) {
      return Left(ServerFailure(message: e.message));
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

  @override
  Future<Either<Failure, List<Vehicle>>> searchVehiclesByQuery({
    required String query,
    int? limit,
  }) async {
    try {
      final result = await mockDataSource.getAllVehicles();

      // Búsqueda por texto en múltiples campos
      final searchQuery = query.toLowerCase();
      var vehicles = result.where((v) {
        return v.make.toLowerCase().contains(searchQuery) ||
            v.model.toLowerCase().contains(searchQuery) ||
            v.title.toLowerCase().contains(searchQuery) ||
            v.description.toLowerCase().contains(searchQuery) ||
            v.bodyType.toLowerCase().contains(searchQuery) ||
            v.fuelType.toLowerCase().contains(searchQuery);
      }).toList();

      // Aplicar límite si se especifica
      if (limit != null && vehicles.length > limit) {
        vehicles = vehicles.take(limit).toList();
      }

      return Right(vehicles);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> filterVehicles({
    required FilterCriteria criteria,
    SortOption? sortBy,
    int? page,
    int? limit,
  }) async {
    try {
      var vehicles = await mockDataSource.getAllVehicles();

      // Aplicar filtros de precio
      if (criteria.minPrice != null) {
        vehicles =
            vehicles.where((v) => v.price >= criteria.minPrice!).toList();
      }
      if (criteria.maxPrice != null) {
        vehicles =
            vehicles.where((v) => v.price <= criteria.maxPrice!).toList();
      }

      // Aplicar filtros de año
      if (criteria.minYear != null) {
        vehicles = vehicles.where((v) => v.year >= criteria.minYear!).toList();
      }
      if (criteria.maxYear != null) {
        vehicles = vehicles.where((v) => v.year <= criteria.maxYear!).toList();
      }

      // Aplicar filtros de marca
      if (criteria.makes != null && criteria.makes!.isNotEmpty) {
        vehicles =
            vehicles.where((v) => criteria.makes!.contains(v.make)).toList();
      }

      // Aplicar filtros de modelo
      if (criteria.models != null && criteria.models!.isNotEmpty) {
        vehicles =
            vehicles.where((v) => criteria.models!.contains(v.model)).toList();
      }

      // Aplicar filtros de tipo de carrocería
      if (criteria.bodyTypes != null && criteria.bodyTypes!.isNotEmpty) {
        vehicles = vehicles
            .where((v) => criteria.bodyTypes!.contains(v.bodyType))
            .toList();
      }

      // Aplicar filtros de tipo de combustible
      if (criteria.fuelTypes != null && criteria.fuelTypes!.isNotEmpty) {
        vehicles = vehicles
            .where((v) => criteria.fuelTypes!.contains(v.fuelType))
            .toList();
      }

      // Aplicar filtros de transmisión
      if (criteria.transmissions != null &&
          criteria.transmissions!.isNotEmpty) {
        vehicles = vehicles
            .where((v) => criteria.transmissions!.contains(v.transmission))
            .toList();
      }

      // Aplicar filtro de kilometraje
      if (criteria.maxMileage != null) {
        vehicles =
            vehicles.where((v) => v.mileage <= criteria.maxMileage!).toList();
      }

      // Aplicar filtro de condición
      if (criteria.condition != null) {
        vehicles =
            vehicles.where((v) => v.condition == criteria.condition).toList();
      }

      // Aplicar filtro de color
      if (criteria.colors != null && criteria.colors!.isNotEmpty) {
        vehicles = vehicles
            .where((v) => v.color != null && criteria.colors!.contains(v.color))
            .toList();
      }

      // Aplicar ordenamiento
      if (sortBy != null) {
        switch (sortBy) {
          case SortOption.priceAsc:
            vehicles.sort((a, b) => a.price.compareTo(b.price));
            break;
          case SortOption.priceDesc:
            vehicles.sort((a, b) => b.price.compareTo(a.price));
            break;
          case SortOption.yearDesc:
            vehicles.sort((a, b) => b.year.compareTo(a.year));
            break;
          case SortOption.yearAsc:
            vehicles.sort((a, b) => a.year.compareTo(b.year));
            break;
          case SortOption.mileageAsc:
            vehicles.sort((a, b) => a.mileage.compareTo(b.mileage));
            break;
          case SortOption.mileageDesc:
            vehicles.sort((a, b) => b.mileage.compareTo(a.mileage));
            break;
          case SortOption.dateDesc:
            vehicles.sort((a, b) => b.createdAt.compareTo(a.createdAt));
            break;
          case SortOption.relevance:
            // Ordenar por featured y luego por fecha
            vehicles.sort((a, b) {
              if (a.isFeatured != b.isFeatured) {
                return b.isFeatured ? 1 : -1;
              }
              return b.createdAt.compareTo(a.createdAt);
            });
            break;
        }
      }

      // Aplicar paginación
      if (page != null && limit != null) {
        final startIndex = page * limit;
        if (startIndex < vehicles.length) {
          final endIndex = (startIndex + limit).clamp(0, vehicles.length);
          vehicles = vehicles.sublist(startIndex, endIndex);
        } else {
          vehicles = [];
        }
      } else if (limit != null) {
        vehicles = vehicles.take(limit).toList();
      }

      return Right(vehicles);
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, Map<String, List<String>>>>
      getFilterSuggestions() async {
    try {
      final vehicles = await mockDataSource.getAllVehicles();

      // Extraer valores únicos para cada categoría
      final makes = vehicles.map((v) => v.make).toSet().toList()..sort();
      final models = vehicles.map((v) => v.model).toSet().toList()..sort();
      final bodyTypes = vehicles.map((v) => v.bodyType).toSet().toList()
        ..sort();
      final fuelTypes = vehicles.map((v) => v.fuelType).toSet().toList()
        ..sort();
      final transmissions = vehicles.map((v) => v.transmission).toSet().toList()
        ..sort();
      final colors = vehicles
          .map((v) => v.color)
          .whereType<String>()
          .toSet()
          .toList()
        ..sort();
      final conditions = vehicles.map((v) => v.condition).toSet().toList()
        ..sort();

      return Right({
        'makes': makes,
        'models': models,
        'bodyTypes': bodyTypes,
        'fuelTypes': fuelTypes,
        'transmissions': transmissions,
        'colors': colors,
        'conditions': conditions,
      });
    } catch (e) {
      return Left(ServerFailure(message: e.toString()));
    }
  }

  @override
  Future<Either<Failure, String>> contactSeller({
    required String vehicleId,
    required String sellerId,
    required String message,
  }) async {
    try {
      // TODO: Implement API call to create conversation/message
      // For now, simulate success
      await Future.delayed(const Duration(milliseconds: 500));

      // Return conversation/message ID
      return Right('conversation_${DateTime.now().millisecondsSinceEpoch}');
    } catch (e) {
      return Left(ServerFailure(message: 'Failed to contact seller: $e'));
    }
  }

  @override
  Future<Either<Failure, List<Vehicle>>> getSimilarVehicles({
    required String currentVehicleId,
    String? make,
    String? model,
    double? priceMin,
    double? priceMax,
    int limit = 10,
  }) async {
    try {
      final result = await getAllVehicles();

      return result.fold(
        (failure) => Left(failure),
        (vehicles) {
          // Filter out current vehicle
          var similar =
              vehicles.where((v) => v.id != currentVehicleId).toList();

          // Filter by make if provided
          if (make != null && make.isNotEmpty) {
            similar = similar
                .where((v) => v.make.toLowerCase() == make.toLowerCase())
                .toList();
          }

          // Filter by model if provided
          if (model != null && model.isNotEmpty) {
            similar = similar
                .where((v) => v.model.toLowerCase() == model.toLowerCase())
                .toList();
          }

          // Filter by price range if provided
          if (priceMin != null) {
            similar = similar.where((v) => v.price >= priceMin).toList();
          }
          if (priceMax != null) {
            similar = similar.where((v) => v.price <= priceMax).toList();
          }

          // Sort by relevance: featured first, then by price similarity
          similar.sort((a, b) {
            // Featured vehicles first
            if (a.isFeatured && !b.isFeatured) return -1;
            if (!a.isFeatured && b.isFeatured) return 1;

            // Then by date (newest first)
            return b.createdAt.compareTo(a.createdAt);
          });

          // Limit results
          final limited = similar.take(limit).toList();

          return Right(limited);
        },
      );
    } catch (e) {
      return Left(ServerFailure(message: 'Failed to get similar vehicles: $e'));
    }
  }
}
