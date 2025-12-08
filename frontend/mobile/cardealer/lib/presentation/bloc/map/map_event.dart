import 'package:equatable/equatable.dart';
import '../../../domain/entities/location.dart';
import 'map_state.dart';

abstract class MapEvent extends Equatable {
  const MapEvent();

  @override
  List<Object?> get props => [];
}

// Initialize map
class InitializeMap extends MapEvent {
  final Location? initialLocation;
  final double? initialRadius;

  const InitializeMap({
    this.initialLocation,
    this.initialRadius,
  });

  @override
  List<Object?> get props => [initialLocation, initialRadius];
}

// Get current location
class GetCurrentLocationEvent extends MapEvent {}

// Update search area
class UpdateSearchArea extends MapEvent {
  final SearchArea searchArea;

  const UpdateSearchArea({required this.searchArea});

  @override
  List<Object> get props => [searchArea];
}

// Update radius
class UpdateRadius extends MapEvent {
  final double radiusKm;

  const UpdateRadius({required this.radiusKm});

  @override
  List<Object> get props => [radiusKm];
}

// Center map on location
class CenterMapOnLocation extends MapEvent {
  final Location location;
  final double? radius;

  const CenterMapOnLocation({
    required this.location,
    this.radius,
  });

  @override
  List<Object?> get props => [location, radius];
}

// Search vehicles
class SearchVehiclesInArea extends MapEvent {
  final List<String>? brands;
  final double? minPrice;
  final double? maxPrice;

  const SearchVehiclesInArea({
    this.brands,
    this.minPrice,
    this.maxPrice,
  });

  @override
  List<Object?> get props => [brands, minPrice, maxPrice];
}

// Marker selection
class SelectMarker extends MapEvent {
  final VehicleMarker marker;

  const SelectMarker({required this.marker});

  @override
  List<Object> get props => [marker];
}

class DeselectMarker extends MapEvent {}

// Map interactions
class UpdateZoomLevel extends MapEvent {
  final double zoomLevel;

  const UpdateZoomLevel({required this.zoomLevel});

  @override
  List<Object> get props => [zoomLevel];
}

class ChangeMapType extends MapEvent {
  final MapType mapType;

  const ChangeMapType({required this.mapType});

  @override
  List<Object> get props => [mapType];
}

// Place search
class SearchPlacesEvent extends MapEvent {
  final String query;

  const SearchPlacesEvent({required this.query});

  @override
  List<Object> get props => [query];
}

class ClearPlaceSearch extends MapEvent {}

// Refresh
class RefreshMap extends MapEvent {}
