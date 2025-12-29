import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/usecases/usecase.dart';
import '../../../domain/entities/location.dart';
import '../../../domain/repositories/location_repository.dart';
import '../../../domain/usecases/location/get_current_location.dart';
import '../../../domain/usecases/location/search_places.dart';
import '../../../domain/usecases/location/search_vehicles_by_location.dart';
import 'map_event.dart';
import 'map_state.dart';

class MapBloc extends Bloc<MapEvent, MapState> {
  final GetCurrentLocation getCurrentLocation;
  final SearchVehiclesByLocation searchVehiclesByLocation;
  final SearchPlaces searchPlaces;
  final LocationRepository locationRepository;

  MapBloc({
    required this.getCurrentLocation,
    required this.searchVehiclesByLocation,
    required this.searchPlaces,
    required this.locationRepository,
  }) : super(MapInitial()) {
    on<InitializeMap>(_onInitializeMap);
    on<GetCurrentLocationEvent>(_onGetCurrentLocation);
    on<UpdateSearchArea>(_onUpdateSearchArea);
    on<UpdateRadius>(_onUpdateRadius);
    on<CenterMapOnLocation>(_onCenterMapOnLocation);
    on<SearchVehiclesInArea>(_onSearchVehiclesInArea);
    on<SelectMarker>(_onSelectMarker);
    on<DeselectMarker>(_onDeselectMarker);
    on<UpdateZoomLevel>(_onUpdateZoomLevel);
    on<ChangeMapType>(_onChangeMapType);
    on<SearchPlacesEvent>(_onSearchPlaces);
    on<ClearPlaceSearch>(_onClearPlaceSearch);
    on<RefreshMap>(_onRefreshMap);
  }

  Future<void> _onInitializeMap(
    InitializeMap event,
    Emitter<MapState> emit,
  ) async {
    emit(MapLoading());

    Location? initialLocation = event.initialLocation;

    // Get current location if not provided
    if (initialLocation == null) {
      final result = await getCurrentLocation(NoParams());
      result.fold(
        (failure) {
          // Use default location if current location fails
          initialLocation = const Location(
            latitude: 18.486058,
            longitude: -69.931212,
            city: 'Santo Domingo',
            state: 'Distrito Nacional',
            country: 'República Dominicana',
          );
        },
        (location) {
          initialLocation = location;
        },
      );
    }

    final radius = event.initialRadius ?? 10.0; // Default 10 km
    final searchArea = SearchArea(
      center: initialLocation!,
      radiusKm: radius,
    );

    // Load initial markers
    final markersResult = await searchVehiclesByLocation(
      SearchVehiclesByLocationParams(searchArea: searchArea),
    );

    markersResult.fold(
      (failure) => emit(MapError(message: failure.message)),
      (markers) async {
        // Cluster markers
        final clustersResult = await locationRepository.clusterMarkers(
          markers: markers,
          zoomLevel: 12.0,
        );

        clustersResult.fold(
          (failure) => emit(MapError(message: failure.message)),
          (clusters) {
            emit(MapLoaded(
              currentLocation: initialLocation,
              searchArea: searchArea,
              markers: markers,
              clusters: clusters,
              zoomLevel: 12.0,
            ));
          },
        );
      },
    );
  }

  Future<void> _onGetCurrentLocation(
    GetCurrentLocationEvent event,
    Emitter<MapState> emit,
  ) async {
    emit(MapLocationLoading());

    final result = await getCurrentLocation(NoParams());

    result.fold(
      (failure) => emit(MapLocationError(message: failure.message)),
      (location) {
        if (state is MapLoaded) {
          final currentState = state as MapLoaded;
          emit(currentState.copyWith(currentLocation: location));
        } else {
          // Initialize map with current location
          add(InitializeMap(initialLocation: location));
        }
      },
    );
  }

  Future<void> _onUpdateSearchArea(
    UpdateSearchArea event,
    Emitter<MapState> emit,
  ) async {
    if (state is! MapLoaded) return;

    final currentState = state as MapLoaded;

    // Load markers for new area
    final markersResult = await searchVehiclesByLocation(
      SearchVehiclesByLocationParams(searchArea: event.searchArea),
    );

    markersResult.fold(
      (failure) => emit(MapError(message: failure.message)),
      (markers) async {
        // Cluster markers
        final clustersResult = await locationRepository.clusterMarkers(
          markers: markers,
          zoomLevel: currentState.zoomLevel,
        );

        clustersResult.fold(
          (failure) => emit(MapError(message: failure.message)),
          (clusters) {
            emit(currentState.copyWith(
              searchArea: event.searchArea,
              markers: markers,
              clusters: clusters,
              clearSelection: true,
            ));
          },
        );
      },
    );
  }

  Future<void> _onUpdateRadius(
    UpdateRadius event,
    Emitter<MapState> emit,
  ) async {
    if (state is! MapLoaded) return;

    final currentState = state as MapLoaded;
    final newSearchArea = SearchArea(
      center: currentState.searchArea.center,
      radiusKm: event.radiusKm,
    );

    add(UpdateSearchArea(searchArea: newSearchArea));
  }

  Future<void> _onCenterMapOnLocation(
    CenterMapOnLocation event,
    Emitter<MapState> emit,
  ) async {
    if (state is! MapLoaded) return;

    final currentState = state as MapLoaded;
    final radius = event.radius ?? currentState.searchArea.radiusKm;
    final newSearchArea = SearchArea(
      center: event.location,
      radiusKm: radius,
    );

    add(UpdateSearchArea(searchArea: newSearchArea));
  }

  Future<void> _onSearchVehiclesInArea(
    SearchVehiclesInArea event,
    Emitter<MapState> emit,
  ) async {
    if (state is! MapLoaded) return;

    final currentState = state as MapLoaded;

    final markersResult = await searchVehiclesByLocation(
      SearchVehiclesByLocationParams(
        searchArea: currentState.searchArea,
        brands: event.brands,
        minPrice: event.minPrice,
        maxPrice: event.maxPrice,
      ),
    );

    markersResult.fold(
      (failure) => emit(MapError(message: failure.message)),
      (markers) async {
        // Cluster markers
        final clustersResult = await locationRepository.clusterMarkers(
          markers: markers,
          zoomLevel: currentState.zoomLevel,
        );

        clustersResult.fold(
          (failure) => emit(MapError(message: failure.message)),
          (clusters) {
            emit(currentState.copyWith(
              markers: markers,
              clusters: clusters,
              clearSelection: true,
            ));
          },
        );
      },
    );
  }

  void _onSelectMarker(
    SelectMarker event,
    Emitter<MapState> emit,
  ) {
    if (state is! MapLoaded) return;

    final currentState = state as MapLoaded;
    emit(currentState.copyWith(selectedMarker: event.marker));
  }

  void _onDeselectMarker(
    DeselectMarker event,
    Emitter<MapState> emit,
  ) {
    if (state is! MapLoaded) return;

    final currentState = state as MapLoaded;
    emit(currentState.copyWith(clearSelection: true));
  }

  Future<void> _onUpdateZoomLevel(
    UpdateZoomLevel event,
    Emitter<MapState> emit,
  ) async {
    if (state is! MapLoaded) return;

    final currentState = state as MapLoaded;

    // Re-cluster markers with new zoom level
    final clustersResult = await locationRepository.clusterMarkers(
      markers: currentState.markers,
      zoomLevel: event.zoomLevel,
    );

    clustersResult.fold(
      (failure) => emit(MapError(message: failure.message)),
      (clusters) {
        emit(currentState.copyWith(
          zoomLevel: event.zoomLevel,
          clusters: clusters,
        ));
      },
    );
  }

  void _onChangeMapType(
    ChangeMapType event,
    Emitter<MapState> emit,
  ) {
    if (state is! MapLoaded) return;

    final currentState = state as MapLoaded;
    emit(currentState.copyWith(mapType: event.mapType));
  }

  Future<void> _onSearchPlaces(
    SearchPlacesEvent event,
    Emitter<MapState> emit,
  ) async {
    emit(PlaceSearchLoading());

    final result = await searchPlaces(event.query);

    result.fold(
      (failure) => emit(PlaceSearchError(message: failure.message)),
      (places) => emit(PlaceSearchLoaded(places: places, query: event.query)),
    );
  }

  void _onClearPlaceSearch(
    ClearPlaceSearch event,
    Emitter<MapState> emit,
  ) {
    if (state is MapLoaded) {
      // Keep current map state
      return;
    }
    emit(MapInitial());
  }

  Future<void> _onRefreshMap(
    RefreshMap event,
    Emitter<MapState> emit,
  ) async {
    if (state is! MapLoaded) return;

    final currentState = state as MapLoaded;

    // Reload markers for current search area
    final markersResult = await searchVehiclesByLocation(
      SearchVehiclesByLocationParams(searchArea: currentState.searchArea),
    );

    markersResult.fold(
      (failure) => emit(MapError(message: failure.message)),
      (markers) async {
        // Cluster markers
        final clustersResult = await locationRepository.clusterMarkers(
          markers: markers,
          zoomLevel: currentState.zoomLevel,
        );

        clustersResult.fold(
          (failure) => emit(MapError(message: failure.message)),
          (clusters) {
            emit(currentState.copyWith(
              markers: markers,
              clusters: clusters,
            ));
          },
        );
      },
    );
  }
}
