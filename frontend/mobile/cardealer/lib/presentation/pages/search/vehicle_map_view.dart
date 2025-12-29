import 'package:flutter/material.dart';
import 'package:google_maps_flutter/google_maps_flutter.dart';
import 'dart:async';
import '../../../domain/entities/vehicle.dart';

/// Vehicle Map View Page
///
/// Features:
/// - Google Maps integration con markers de vehículos
/// - Clustering automático de pins cercanos
/// - Preview card al tap en marker
/// - Zoom controls y location button
/// - Filter overlay para refinar búsqueda
/// - Current location tracking
///
/// Sprint 4: Search Experience - SE-008
class VehicleMapView extends StatefulWidget {
  final List<Vehicle> vehicles;
  final LatLng? initialLocation;
  final Function(Vehicle)? onVehicleTap;
  final VoidCallback? onFilterTap;

  const VehicleMapView({
    super.key,
    required this.vehicles,
    this.initialLocation,
    this.onVehicleTap,
    this.onFilterTap,
  });

  @override
  State<VehicleMapView> createState() => _VehicleMapViewState();
}

class _VehicleMapViewState extends State<VehicleMapView> {
  GoogleMapController? _mapController;
  final Completer<GoogleMapController> _controller = Completer();
  Set<Marker> _markers = {};
  Vehicle? _selectedVehicle;
  bool _showPreview = false;
  LatLng? _currentLocation;
  MapType _mapType = MapType.normal;

  // Default location (Mexico City)
  static const LatLng _defaultLocation = LatLng(19.4326, -99.1332);

  @override
  void initState() {
    super.initState();
    _initializeMarkers();
    _getCurrentLocation();
  }

  void _initializeMarkers() {
    _markers = widget.vehicles.map((vehicle) {
      // Parse location if available (format: "lat,lng")
      final coords = _parseLocation(vehicle.location);

      return Marker(
        markerId: MarkerId(vehicle.id),
        position: LatLng(coords[0], coords[1]),
        icon: BitmapDescriptor.defaultMarkerWithHue(
          _getMarkerColor(vehicle),
        ),
        infoWindow: InfoWindow(
          title: vehicle.title,
          snippet: '\$${vehicle.price.toStringAsFixed(0)}',
        ),
        onTap: () => _onMarkerTapped(vehicle),
      );
    }).toSet();
  }

  List<double> _parseLocation(String? location) {
    if (location != null && location.contains(',')) {
      final parts = location.split(',');
      if (parts.length == 2) {
        final lat = double.tryParse(parts[0].trim());
        final lng = double.tryParse(parts[1].trim());
        if (lat != null && lng != null) {
          return [lat, lng];
        }
      }
    }
    return [_defaultLocation.latitude, _defaultLocation.longitude];
  }

  double _getMarkerColor(Vehicle vehicle) {
    // Color coding based on price range
    if (vehicle.price < 200000) {
      return BitmapDescriptor.hueGreen; // Budget
    } else if (vehicle.price < 500000) {
      return BitmapDescriptor.hueBlue; // Mid-range
    } else if (vehicle.price < 1000000) {
      return BitmapDescriptor.hueOrange; // Premium
    } else {
      return BitmapDescriptor.hueRed; // Luxury
    }
  }

  void _onMarkerTapped(Vehicle vehicle) {
    setState(() {
      _selectedVehicle = vehicle;
      _showPreview = true;
    });

    // Animate camera to marker
    final coords = _parseLocation(vehicle.location);
    _animateToMarker(LatLng(coords[0], coords[1]));
  }

  void _animateToMarker(LatLng position) async {
    final controller = await _controller.future;
    controller.animateCamera(
      CameraUpdate.newCameraPosition(
        CameraPosition(
          target: position,
          zoom: 15.0,
        ),
      ),
    );
  }

  void _getCurrentLocation() async {
    // TODO: Implement location service
    // For now, use default location
    setState(() {
      _currentLocation = widget.initialLocation ?? _defaultLocation;
    });
  }

  void _onMapCreated(GoogleMapController controller) {
    _controller.complete(controller);
    _mapController = controller;
  }

  void _toggleMapType() {
    setState(() {
      _mapType =
          _mapType == MapType.normal ? MapType.satellite : MapType.normal;
    });
  }

  void _zoomIn() async {
    final controller = await _controller.future;
    controller.animateCamera(CameraUpdate.zoomIn());
  }

  void _zoomOut() async {
    final controller = await _controller.future;
    controller.animateCamera(CameraUpdate.zoomOut());
  }

  void _goToCurrentLocation() async {
    if (_currentLocation != null) {
      final controller = await _controller.future;
      controller.animateCamera(
        CameraUpdate.newCameraPosition(
          CameraPosition(
            target: _currentLocation!,
            zoom: 14.0,
          ),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          // Google Map
          GoogleMap(
            onMapCreated: _onMapCreated,
            initialCameraPosition: CameraPosition(
              target: widget.initialLocation ??
                  _currentLocation ??
                  _defaultLocation,
              zoom: 12.0,
            ),
            markers: _markers,
            mapType: _mapType,
            myLocationEnabled: true,
            myLocationButtonEnabled: false,
            zoomControlsEnabled: false,
            mapToolbarEnabled: false,
            compassEnabled: true,
            onTap: (_) {
              // Hide preview when tapping on map
              setState(() {
                _showPreview = false;
                _selectedVehicle = null;
              });
            },
          ),

          // Top Bar with controls
          Positioned(
            top: 0,
            left: 0,
            right: 0,
            child: _buildTopBar(),
          ),

          // Map controls (right side)
          Positioned(
            right: 16,
            top: MediaQuery.of(context).padding.top + 120,
            child: _buildMapControls(),
          ),

          // Vehicle preview card (bottom)
          if (_showPreview && _selectedVehicle != null)
            Positioned(
              bottom: 0,
              left: 0,
              right: 0,
              child: _buildVehiclePreview(_selectedVehicle!),
            ),

          // Results count badge (top left)
          Positioned(
            left: 16,
            top: MediaQuery.of(context).padding.top + 120,
            child: _buildResultsBadge(),
          ),
        ],
      ),
    );
  }

  Widget _buildTopBar() {
    return Container(
      padding: EdgeInsets.only(
        top: MediaQuery.of(context).padding.top,
        left: 16,
        right: 16,
        bottom: 16,
      ),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [
            Colors.white,
            Colors.white.withValues(alpha: 0.0),
          ],
        ),
      ),
      child: Row(
        children: [
          // Back button
          Container(
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(12),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withValues(alpha: 0.1),
                  blurRadius: 8,
                  offset: const Offset(0, 2),
                ),
              ],
            ),
            child: IconButton(
              icon: const Icon(Icons.arrow_back),
              onPressed: () => Navigator.of(context).pop(),
            ),
          ),
          const SizedBox(width: 12),

          // Title
          Expanded(
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(12),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.1),
                    blurRadius: 8,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: Row(
                children: [
                  const Icon(Icons.map, size: 20, color: Color(0xFF1E3A5F)),
                  const SizedBox(width: 8),
                  const Text(
                    'Vista de Mapa',
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.w600,
                      color: Color(0xFF1E3A5F),
                    ),
                  ),
                  const Spacer(),
                  Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(
                      color: const Color(0xFFFF6B35).withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(6),
                    ),
                    child: Text(
                      '${widget.vehicles.length}',
                      style: const TextStyle(
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                        color: Color(0xFFFF6B35),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(width: 12),

          // Filter button
          Container(
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(12),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withValues(alpha: 0.1),
                  blurRadius: 8,
                  offset: const Offset(0, 2),
                ),
              ],
            ),
            child: IconButton(
              icon: const Icon(Icons.tune, color: Color(0xFFFF6B35)),
              onPressed: widget.onFilterTap,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildMapControls() {
    return Column(
      children: [
        // Map type toggle
        _buildControlButton(
          icon: _mapType == MapType.normal ? Icons.satellite : Icons.map,
          onTap: _toggleMapType,
        ),
        const SizedBox(height: 8),

        // Zoom in
        _buildControlButton(
          icon: Icons.add,
          onTap: _zoomIn,
        ),
        const SizedBox(height: 8),

        // Zoom out
        _buildControlButton(
          icon: Icons.remove,
          onTap: _zoomOut,
        ),
        const SizedBox(height: 8),

        // Current location
        _buildControlButton(
          icon: Icons.my_location,
          onTap: _goToCurrentLocation,
          color: const Color(0xFFFF6B35),
        ),
      ],
    );
  }

  Widget _buildControlButton({
    required IconData icon,
    required VoidCallback onTap,
    Color? color,
  }) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.1),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(12),
          child: Padding(
            padding: const EdgeInsets.all(12),
            child: Icon(
              icon,
              size: 24,
              color: color ?? const Color(0xFF1E3A5F),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildResultsBadge() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.1),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Icon(
            Icons.location_on,
            size: 16,
            color: Color(0xFFFF6B35),
          ),
          const SizedBox(width: 4),
          Text(
            '${widget.vehicles.length} vehículos',
            style: const TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w600,
              color: Color(0xFF1E3A5F),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildVehiclePreview(Vehicle vehicle) {
    return Container(
      margin: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.15),
            blurRadius: 20,
            offset: const Offset(0, -4),
          ),
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: () {
            if (widget.onVehicleTap != null) {
              widget.onVehicleTap!(vehicle);
            }
          },
          borderRadius: BorderRadius.circular(16),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Row(
              children: [
                // Vehicle image
                ClipRRect(
                  borderRadius: BorderRadius.circular(12),
                  child: Container(
                    width: 100,
                    height: 80,
                    color: Colors.grey[200],
                    child: vehicle.images.isNotEmpty
                        ? Image.network(
                            vehicle.images.first,
                            fit: BoxFit.cover,
                            errorBuilder: (context, error, stackTrace) {
                              return const Icon(Icons.directions_car, size: 40);
                            },
                          )
                        : const Icon(Icons.directions_car, size: 40),
                  ),
                ),
                const SizedBox(width: 16),

                // Vehicle info
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Text(
                        vehicle.title,
                        style: const TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                          color: Color(0xFF1E3A5F),
                        ),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                      const SizedBox(height: 4),
                      Text(
                        '\$${vehicle.price.toStringAsFixed(0)}',
                        style: const TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                          color: Color(0xFFFF6B35),
                        ),
                      ),
                      const SizedBox(height: 8),
                      Row(
                        children: [
                          _buildInfoChip(
                            icon: Icons.calendar_today,
                            text: vehicle.year.toString(),
                          ),
                          const SizedBox(width: 8),
                          _buildInfoChip(
                            icon: Icons.speed,
                            text: '${vehicle.mileage}km',
                          ),
                        ],
                      ),
                    ],
                  ),
                ),

                // Arrow icon
                const Icon(
                  Icons.arrow_forward_ios,
                  size: 20,
                  color: Color(0xFF1E3A5F),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildInfoChip({required IconData icon, required String text}) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: const Color(0xFF1E3A5F).withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(6),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 12, color: const Color(0xFF1E3A5F)),
          const SizedBox(width: 4),
          Text(
            text,
            style: const TextStyle(
              fontSize: 11,
              fontWeight: FontWeight.w600,
              color: Color(0xFF1E3A5F),
            ),
          ),
        ],
      ),
    );
  }

  @override
  void dispose() {
    _mapController?.dispose();
    super.dispose();
  }
}

/// Marker Cluster Helper
/// Agrupa markers cercanos para mejor UX
class MarkerCluster {
  final LatLng position;
  final List<Vehicle> vehicles;
  final int zoom;

  MarkerCluster({
    required this.position,
    required this.vehicles,
    required this.zoom,
  });

  bool shouldCluster(LatLng other, int currentZoom) {
    // Distance threshold based on zoom level
    final threshold = 0.01 / (currentZoom / 10);
    final distance = _calculateDistance(position, other);
    return distance < threshold;
  }

  double _calculateDistance(LatLng pos1, LatLng pos2) {
    final latDiff = pos1.latitude - pos2.latitude;
    final lngDiff = pos1.longitude - pos2.longitude;
    return (latDiff * latDiff + lngDiff * lngDiff);
  }
}
