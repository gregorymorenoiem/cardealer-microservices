import 'package:equatable/equatable.dart';

/// Represents a geographic location with coordinates
class Location extends Equatable {
  final double latitude;
  final double longitude;
  final String? address;
  final String? city;
  final String? state;
  final String? country;
  final String? postalCode;
  final String? formattedAddress;

  const Location({
    required this.latitude,
    required this.longitude,
    this.address,
    this.city,
    this.state,
    this.country,
    this.postalCode,
    this.formattedAddress,
  });

  @override
  List<Object?> get props => [
        latitude,
        longitude,
        address,
        city,
        state,
        country,
        postalCode,
        formattedAddress,
      ];

  /// Calculate distance to another location in kilometers
  double distanceTo(Location other) {
    const double earthRadius = 6371; // km

    final double lat1Rad = latitude * (3.141592653589793 / 180);
    final double lat2Rad = other.latitude * (3.141592653589793 / 180);
    final double deltaLat =
        (other.latitude - latitude) * (3.141592653589793 / 180);
    final double deltaLng =
        (other.longitude - longitude) * (3.141592653589793 / 180);

    final double a = (deltaLat / 2).abs() * (deltaLat / 2).abs() +
        lat1Rad.abs() *
            lat2Rad.abs() *
            (deltaLng / 2).abs() *
            (deltaLng / 2).abs();

    final double c = 2 * (a.abs().clamp(0, 1));

    return earthRadius * c;
  }

  /// Get display text for location
  String get displayText {
    if (formattedAddress != null && formattedAddress!.isNotEmpty) {
      return formattedAddress!;
    }

    final parts = <String>[];
    if (city != null && city!.isNotEmpty) parts.add(city!);
    if (state != null && state!.isNotEmpty) parts.add(state!);
    if (country != null && country!.isNotEmpty) parts.add(country!);

    return parts.isNotEmpty ? parts.join(', ') : 'Ubicación';
  }

  /// Get short display text (city, state)
  String get shortDisplay {
    final parts = <String>[];
    if (city != null && city!.isNotEmpty) parts.add(city!);
    if (state != null && state!.isNotEmpty) parts.add(state!);
    return parts.isNotEmpty ? parts.join(', ') : 'Ubicación';
  }

  Location copyWith({
    double? latitude,
    double? longitude,
    String? address,
    String? city,
    String? state,
    String? country,
    String? postalCode,
    String? formattedAddress,
  }) {
    return Location(
      latitude: latitude ?? this.latitude,
      longitude: longitude ?? this.longitude,
      address: address ?? this.address,
      city: city ?? this.city,
      state: state ?? this.state,
      country: country ?? this.country,
      postalCode: postalCode ?? this.postalCode,
      formattedAddress: formattedAddress ?? this.formattedAddress,
    );
  }
}

/// Represents a vehicle marker on the map
class VehicleMarker extends Equatable {
  final String vehicleId;
  final String title;
  final String? imageUrl;
  final double price;
  final String currency;
  final Location location;
  final bool isFeatured;
  final bool isFavorite;

  const VehicleMarker({
    required this.vehicleId,
    required this.title,
    this.imageUrl,
    required this.price,
    required this.currency,
    required this.location,
    this.isFeatured = false,
    this.isFavorite = false,
  });

  @override
  List<Object?> get props => [
        vehicleId,
        title,
        imageUrl,
        price,
        currency,
        location,
        isFeatured,
        isFavorite,
      ];

  String get priceFormatted => '\$$currency ${price.toStringAsFixed(0)}';

  VehicleMarker copyWith({
    String? vehicleId,
    String? title,
    String? imageUrl,
    double? price,
    String? currency,
    Location? location,
    bool? isFeatured,
    bool? isFavorite,
  }) {
    return VehicleMarker(
      vehicleId: vehicleId ?? this.vehicleId,
      title: title ?? this.title,
      imageUrl: imageUrl ?? this.imageUrl,
      price: price ?? this.price,
      currency: currency ?? this.currency,
      location: location ?? this.location,
      isFeatured: isFeatured ?? this.isFeatured,
      isFavorite: isFavorite ?? this.isFavorite,
    );
  }
}

/// Represents a geographic search area
class SearchArea extends Equatable {
  final Location center;
  final double radiusKm;

  const SearchArea({
    required this.center,
    required this.radiusKm,
  });

  @override
  List<Object> get props => [center, radiusKm];

  /// Check if a location is within this search area
  bool contains(Location location) {
    return center.distanceTo(location) <= radiusKm;
  }

  SearchArea copyWith({
    Location? center,
    double? radiusKm,
  }) {
    return SearchArea(
      center: center ?? this.center,
      radiusKm: radiusKm ?? this.radiusKm,
    );
  }
}

/// Represents a cluster of vehicle markers
class MarkerCluster extends Equatable {
  final Location center;
  final int count;
  final List<VehicleMarker> markers;
  final double minPrice;
  final double maxPrice;

  const MarkerCluster({
    required this.center,
    required this.count,
    required this.markers,
    required this.minPrice,
    required this.maxPrice,
  });

  @override
  List<Object> get props => [center, count, markers, minPrice, maxPrice];

  String get priceRange {
    if (minPrice == maxPrice) {
      return '\$${minPrice.toStringAsFixed(0)}';
    }
    return '\$${minPrice.toStringAsFixed(0)} - \$${maxPrice.toStringAsFixed(0)}';
  }
}
