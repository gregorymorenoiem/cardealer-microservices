import 'package:equatable/equatable.dart';
import '../../../domain/entities/location.dart';

abstract class MapState extends Equatable {
  const MapState();

  @override
  List<Object?> get props => [];
}

// Initial
class MapInitial extends MapState {}

// Loading states
class MapLoading extends MapState {}

class MapLocationLoading extends MapState {}

// Main map state
class MapLoaded extends MapState {
  final Location? currentLocation;
  final SearchArea searchArea;
  final List<VehicleMarker> markers;
  final List<MarkerCluster> clusters;
  final VehicleMarker? selectedMarker;
  final double zoomLevel;
  final MapType mapType;

  const MapLoaded({
    this.currentLocation,
    required this.searchArea,
    required this.markers,
    required this.clusters,
    this.selectedMarker,
    this.zoomLevel = 12.0,
    this.mapType = MapType.normal,
  });

  @override
  List<Object?> get props => [
        currentLocation,
        searchArea,
        markers,
        clusters,
        selectedMarker,
        zoomLevel,
        mapType,
      ];

  MapLoaded copyWith({
    Location? currentLocation,
    SearchArea? searchArea,
    List<VehicleMarker>? markers,
    List<MarkerCluster>? clusters,
    VehicleMarker? selectedMarker,
    bool clearSelection = false,
    double? zoomLevel,
    MapType? mapType,
  }) {
    return MapLoaded(
      currentLocation: currentLocation ?? this.currentLocation,
      searchArea: searchArea ?? this.searchArea,
      markers: markers ?? this.markers,
      clusters: clusters ?? this.clusters,
      selectedMarker:
          clearSelection ? null : (selectedMarker ?? this.selectedMarker),
      zoomLevel: zoomLevel ?? this.zoomLevel,
      mapType: mapType ?? this.mapType,
    );
  }
}

// Place search states
class PlaceSearchLoading extends MapState {}

class PlaceSearchLoaded extends MapState {
  final List<Location> places;
  final String query;

  const PlaceSearchLoaded({
    required this.places,
    required this.query,
  });

  @override
  List<Object> get props => [places, query];
}

// Error states
class MapError extends MapState {
  final String message;

  const MapError({required this.message});

  @override
  List<Object> get props => [message];
}

class MapLocationError extends MapState {
  final String message;

  const MapLocationError({required this.message});

  @override
  List<Object> get props => [message];
}

class PlaceSearchError extends MapState {
  final String message;

  const PlaceSearchError({required this.message});

  @override
  List<Object> get props => [message];
}

// Map type enum
enum MapType {
  normal,
  satellite,
  hybrid,
  terrain,
}
