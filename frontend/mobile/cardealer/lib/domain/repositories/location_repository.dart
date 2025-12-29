import 'package:dartz/dartz.dart';
import '../../core/error/failures.dart';
import '../entities/location.dart';

/// Repository interface for location and geocoding operations
abstract class LocationRepository {
  /// Get current device location
  Future<Either<Failure, Location>> getCurrentLocation();

  /// Check if location permission is granted
  Future<Either<Failure, bool>> checkLocationPermission();

  /// Request location permission
  Future<Either<Failure, bool>> requestLocationPermission();

  /// Convert address to coordinates (geocoding)
  Future<Either<Failure, Location>> geocodeAddress(String address);

  /// Convert coordinates to address (reverse geocoding)
  Future<Either<Failure, Location>> reverseGeocode({
    required double latitude,
    required double longitude,
  });

  /// Search for places/addresses with autocomplete
  Future<Either<Failure, List<Location>>> searchPlaces(String query);

  /// Get vehicle markers within a search area
  Future<Either<Failure, List<VehicleMarker>>> getVehicleMarkers({
    required SearchArea searchArea,
    List<String>? brands,
    double? minPrice,
    double? maxPrice,
  });

  /// Cluster markers for map performance
  Future<Either<Failure, List<MarkerCluster>>> clusterMarkers({
    required List<VehicleMarker> markers,
    required double zoomLevel,
  });
}
