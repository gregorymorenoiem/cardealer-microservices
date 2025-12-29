import 'package:dartz/dartz.dart';
import 'dart:math';
import '../../../core/error/failures.dart';
import '../../../domain/entities/location.dart';
import '../../../domain/repositories/location_repository.dart';

class MockLocationRepository implements LocationRepository {
  // Mock current location (Santo Domingo, Dominican Republic)
  final Location _defaultLocation = const Location(
    latitude: 18.486058,
    longitude: -69.931212,
    address: 'Avenida Abraham Lincoln',
    city: 'Santo Domingo',
    state: 'Distrito Nacional',
    country: 'República Dominicana',
    postalCode: '10101',
    formattedAddress:
        'Avenida Abraham Lincoln, Santo Domingo, República Dominicana',
  );

  // Mock vehicle markers with various locations
  final List<VehicleMarker> _mockMarkers = [
    // Santo Domingo area
    const VehicleMarker(
      vehicleId: '1',
      title: 'Toyota Corolla 2022',
      imageUrl: 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb',
      price: 17800,
      currency: 'USD',
      location: Location(
        latitude: 18.486058,
        longitude: -69.931212,
        city: 'Santo Domingo',
        state: 'Distrito Nacional',
        country: 'República Dominicana',
      ),
      isFeatured: true,
    ),
    const VehicleMarker(
      vehicleId: '2',
      title: 'Honda CR-V 2023',
      imageUrl: 'https://images.unsplash.com/photo-1581540222194-0def2dda95b8',
      price: 32500,
      currency: 'USD',
      location: Location(
        latitude: 18.475832,
        longitude: -69.940292,
        city: 'Santo Domingo',
        state: 'Distrito Nacional',
        country: 'República Dominicana',
      ),
      isFeatured: true,
    ),
    const VehicleMarker(
      vehicleId: '3',
      title: 'Nissan Sentra 2021',
      imageUrl: 'https://images.unsplash.com/photo-1605559424843-9e4c228bf1c2',
      price: 13500,
      currency: 'USD',
      location: Location(
        latitude: 18.492840,
        longitude: -69.936821,
        city: 'Santo Domingo',
        state: 'Distrito Nacional',
        country: 'República Dominicana',
      ),
    ),
    // Santiago area
    const VehicleMarker(
      vehicleId: '4',
      title: 'Mazda CX-5 2023',
      imageUrl: 'https://images.unsplash.com/photo-1552519507-da3b142c6e3d',
      price: 29000,
      currency: 'USD',
      location: Location(
        latitude: 19.450000,
        longitude: -70.683333,
        city: 'Santiago',
        state: 'Santiago',
        country: 'República Dominicana',
      ),
      isFeatured: true,
    ),
    const VehicleMarker(
      vehicleId: '5',
      title: 'Ford Explorer 2024',
      imageUrl: 'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf',
      price: 45000,
      currency: 'USD',
      location: Location(
        latitude: 19.458333,
        longitude: -70.666667,
        city: 'Santiago',
        state: 'Santiago',
        country: 'República Dominicana',
      ),
      isFeatured: true,
    ),
    // La Romana area
    const VehicleMarker(
      vehicleId: '6',
      title: 'Hyundai Tucson 2022',
      imageUrl: 'https://images.unsplash.com/photo-1583121274602-3e2820c69888',
      price: 25500,
      currency: 'USD',
      location: Location(
        latitude: 18.427186,
        longitude: -68.972716,
        city: 'La Romana',
        state: 'La Romana',
        country: 'República Dominicana',
      ),
    ),
    // Punta Cana area
    const VehicleMarker(
      vehicleId: '7',
      title: 'BMW X3 2023',
      imageUrl: 'https://images.unsplash.com/photo-1555215695-3004980ad54e',
      price: 48500,
      currency: 'USD',
      location: Location(
        latitude: 18.581320,
        longitude: -68.404670,
        city: 'Punta Cana',
        state: 'La Altagracia',
        country: 'República Dominicana',
      ),
      isFeatured: true,
    ),
  ];

  @override
  Future<Either<Failure, Location>> getCurrentLocation() async {
    await Future.delayed(const Duration(milliseconds: 500));
    return Right(_defaultLocation);
  }

  @override
  Future<Either<Failure, bool>> checkLocationPermission() async {
    await Future.delayed(const Duration(milliseconds: 100));
    // Always return true in mock
    return const Right(true);
  }

  @override
  Future<Either<Failure, bool>> requestLocationPermission() async {
    await Future.delayed(const Duration(milliseconds: 500));
    // Always grant in mock
    return const Right(true);
  }

  @override
  Future<Either<Failure, Location>> geocodeAddress(String address) async {
    await Future.delayed(const Duration(milliseconds: 400));

    // Simple mock geocoding
    if (address.toLowerCase().contains('santiago')) {
      return const Right(Location(
        latitude: 19.450000,
        longitude: -70.683333,
        city: 'Santiago',
        state: 'Santiago',
        country: 'República Dominicana',
        formattedAddress: 'Santiago de los Caballeros, República Dominicana',
      ));
    }

    if (address.toLowerCase().contains('la romana')) {
      return const Right(Location(
        latitude: 18.427186,
        longitude: -68.972716,
        city: 'La Romana',
        state: 'La Romana',
        country: 'República Dominicana',
        formattedAddress: 'La Romana, República Dominicana',
      ));
    }

    if (address.toLowerCase().contains('punta cana')) {
      return const Right(Location(
        latitude: 18.581320,
        longitude: -68.404670,
        city: 'Punta Cana',
        state: 'La Altagracia',
        country: 'República Dominicana',
        formattedAddress: 'Punta Cana, La Altagracia, República Dominicana',
      ));
    }

    // Default to Santo Domingo
    return Right(_defaultLocation);
  }

  @override
  Future<Either<Failure, Location>> reverseGeocode({
    required double latitude,
    required double longitude,
  }) async {
    await Future.delayed(const Duration(milliseconds: 400));

    // Find closest city based on coordinates
    final location = Location(latitude: latitude, longitude: longitude);

    if (location
            .distanceTo(const Location(latitude: 19.45, longitude: -70.68)) <
        50) {
      return const Right(Location(
        latitude: 19.450000,
        longitude: -70.683333,
        city: 'Santiago',
        state: 'Santiago',
        country: 'República Dominicana',
        formattedAddress: 'Santiago de los Caballeros, República Dominicana',
      ));
    }

    if (location
            .distanceTo(const Location(latitude: 18.43, longitude: -68.97)) <
        50) {
      return const Right(Location(
        latitude: 18.427186,
        longitude: -68.972716,
        city: 'La Romana',
        state: 'La Romana',
        country: 'República Dominicana',
        formattedAddress: 'La Romana, República Dominicana',
      ));
    }

    // Default to Santo Domingo
    return Right(Location(
      latitude: latitude,
      longitude: longitude,
      city: 'Santo Domingo',
      state: 'Distrito Nacional',
      country: 'República Dominicana',
      formattedAddress: 'Santo Domingo, República Dominicana',
    ));
  }

  @override
  Future<Either<Failure, List<Location>>> searchPlaces(String query) async {
    await Future.delayed(const Duration(milliseconds: 300));

    final places = <Location>[];

    if (query.toLowerCase().contains('santo')) {
      places.add(_defaultLocation);
      places.add(const Location(
        latitude: 18.475832,
        longitude: -69.940292,
        city: 'Santo Domingo',
        state: 'Distrito Nacional',
        country: 'República Dominicana',
        formattedAddress: 'Zona Colonial, Santo Domingo',
      ));
    }

    if (query.toLowerCase().contains('santiago')) {
      places.add(const Location(
        latitude: 19.450000,
        longitude: -70.683333,
        city: 'Santiago',
        state: 'Santiago',
        country: 'República Dominicana',
        formattedAddress: 'Santiago de los Caballeros',
      ));
    }

    if (query.toLowerCase().contains('punta') ||
        query.toLowerCase().contains('cana')) {
      places.add(const Location(
        latitude: 18.581320,
        longitude: -68.404670,
        city: 'Punta Cana',
        state: 'La Altagracia',
        country: 'República Dominicana',
        formattedAddress: 'Punta Cana, La Altagracia',
      ));
    }

    if (query.toLowerCase().contains('romana')) {
      places.add(const Location(
        latitude: 18.427186,
        longitude: -68.972716,
        city: 'La Romana',
        state: 'La Romana',
        country: 'República Dominicana',
        formattedAddress: 'La Romana',
      ));
    }

    if (places.isEmpty) {
      // Return default suggestions
      return Right([
        _defaultLocation,
        const Location(
          latitude: 19.450000,
          longitude: -70.683333,
          city: 'Santiago',
          state: 'Santiago',
          country: 'República Dominicana',
          formattedAddress: 'Santiago de los Caballeros',
        ),
      ]);
    }

    return Right(places);
  }

  @override
  Future<Either<Failure, List<VehicleMarker>>> getVehicleMarkers({
    required SearchArea searchArea,
    List<String>? brands,
    double? minPrice,
    double? maxPrice,
  }) async {
    await Future.delayed(const Duration(milliseconds: 600));

    // Filter markers within search area
    var filtered = _mockMarkers.where((marker) {
      return searchArea.contains(marker.location);
    }).toList();

    // Apply brand filter
    if (brands != null && brands.isNotEmpty) {
      filtered = filtered.where((marker) {
        return brands.any((brand) =>
            marker.title.toLowerCase().contains(brand.toLowerCase()));
      }).toList();
    }

    // Apply price filters
    if (minPrice != null) {
      filtered = filtered.where((marker) => marker.price >= minPrice).toList();
    }

    if (maxPrice != null) {
      filtered = filtered.where((marker) => marker.price <= maxPrice).toList();
    }

    return Right(filtered);
  }

  @override
  Future<Either<Failure, List<MarkerCluster>>> clusterMarkers({
    required List<VehicleMarker> markers,
    required double zoomLevel,
  }) async {
    await Future.delayed(const Duration(milliseconds: 200));

    if (markers.isEmpty) {
      return const Right([]);
    }

    // Simple clustering algorithm based on zoom level
    // Higher zoom = less clustering
    final clusterDistance = _getClusterDistance(zoomLevel);

    final clusters = <MarkerCluster>[];
    final processed = <String>{};

    for (final marker in markers) {
      if (processed.contains(marker.vehicleId)) continue;

      final clusterMarkers = <VehicleMarker>[marker];
      processed.add(marker.vehicleId);

      // Find nearby markers
      for (final other in markers) {
        if (processed.contains(other.vehicleId)) continue;

        final distance = marker.location.distanceTo(other.location);
        if (distance <= clusterDistance) {
          clusterMarkers.add(other);
          processed.add(other.vehicleId);
        }
      }

      // Create cluster
      if (clusterMarkers.length > 1) {
        final avgLat = clusterMarkers
                .map((m) => m.location.latitude)
                .reduce((a, b) => a + b) /
            clusterMarkers.length;
        final avgLng = clusterMarkers
                .map((m) => m.location.longitude)
                .reduce((a, b) => a + b) /
            clusterMarkers.length;

        final prices = clusterMarkers.map((m) => m.price).toList();

        clusters.add(MarkerCluster(
          center: Location(latitude: avgLat, longitude: avgLng),
          count: clusterMarkers.length,
          markers: clusterMarkers,
          minPrice: prices.reduce(min),
          maxPrice: prices.reduce(max),
        ));
      } else {
        // Single marker "cluster"
        clusters.add(MarkerCluster(
          center: marker.location,
          count: 1,
          markers: [marker],
          minPrice: marker.price,
          maxPrice: marker.price,
        ));
      }
    }

    return Right(clusters);
  }

  double _getClusterDistance(double zoomLevel) {
    // Zoom level typically 0-20
    // Higher zoom = closer view = smaller cluster distance
    if (zoomLevel >= 15) return 1; // 1 km
    if (zoomLevel >= 12) return 5; // 5 km
    if (zoomLevel >= 10) return 10; // 10 km
    if (zoomLevel >= 8) return 25; // 25 km
    return 50; // 50 km
  }
}
