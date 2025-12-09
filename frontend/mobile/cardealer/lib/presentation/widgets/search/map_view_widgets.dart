import 'package:flutter/material.dart';
import 'package:google_maps_flutter/google_maps_flutter.dart';
import '../../../domain/entities/vehicle.dart';
import '../../pages/search/vehicle_map_view.dart';

/// Map View Button Widget
///
/// Botón flotante para abrir la vista de mapa desde la página de búsqueda
/// Sprint 4: Search Experience - SE-008
class MapViewButton extends StatelessWidget {
  final List<Vehicle> vehicles;
  final LatLng? currentLocation;

  const MapViewButton({
    super.key,
    required this.vehicles,
    this.currentLocation,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [Color(0xFFFF6B35), Color(0xFFE55A2B)],
        ),
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: const Color(0xFFFF6B35).withValues(alpha: 0.3),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: () {
            Navigator.of(context).push(
              MaterialPageRoute(
                builder: (context) => VehicleMapView(
                  vehicles: vehicles,
                  initialLocation: currentLocation,
                  onVehicleTap: (vehicle) {
                    // Navigate to vehicle detail
                    Navigator.of(context).pop();
                    // TODO: Navigate to vehicle detail page
                  },
                  onFilterTap: () {
                    // Show filter bottom sheet
                    Navigator.of(context).pop();
                    // TODO: Show filter bottom sheet
                  },
                ),
              ),
            );
          },
          borderRadius: BorderRadius.circular(16),
          child: const Padding(
            padding: EdgeInsets.symmetric(horizontal: 20, vertical: 12),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(
                  Icons.map,
                  color: Colors.white,
                  size: 20,
                ),
                SizedBox(width: 8),
                Text(
                  'Ver en Mapa',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

/// Mini Map Preview Widget
///
/// Vista previa pequeña del mapa en la página de búsqueda
/// Tap para expandir a vista completa
class MiniMapPreview extends StatelessWidget {
  final List<Vehicle> vehicles;
  final LatLng? centerLocation;
  final double height;

  const MiniMapPreview({
    super.key,
    required this.vehicles,
    this.centerLocation,
    this.height = 200,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: () {
        Navigator.of(context).push(
          MaterialPageRoute(
            builder: (context) => VehicleMapView(
              vehicles: vehicles,
              initialLocation: centerLocation,
            ),
          ),
        );
      },
      child: Container(
        height: height,
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(16),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.1),
              blurRadius: 8,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: ClipRRect(
          borderRadius: BorderRadius.circular(16),
          child: Stack(
            children: [
              // Mini map
              GoogleMap(
                initialCameraPosition: CameraPosition(
                  target: centerLocation ?? const LatLng(19.4326, -99.1332),
                  zoom: 11.0,
                ),
                markers: _buildMarkers(),
                myLocationEnabled: false,
                myLocationButtonEnabled: false,
                zoomControlsEnabled: false,
                scrollGesturesEnabled: false,
                zoomGesturesEnabled: false,
                tiltGesturesEnabled: false,
                rotateGesturesEnabled: false,
                mapToolbarEnabled: false,
              ),

              // Overlay with expand button
              Container(
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    begin: Alignment.topCenter,
                    end: Alignment.bottomCenter,
                    colors: [
                      Colors.transparent,
                      Colors.black.withValues(alpha: 0.3),
                    ],
                  ),
                ),
              ),

              // Expand button
              Positioned(
                right: 16,
                bottom: 16,
                child: Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 16,
                    vertical: 8,
                  ),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(20),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withValues(alpha: 0.2),
                        blurRadius: 8,
                        offset: const Offset(0, 2),
                      ),
                    ],
                  ),
                  child: const Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(
                        Icons.fullscreen,
                        size: 16,
                        color: Color(0xFF1E3A5F),
                      ),
                      SizedBox(width: 4),
                      Text(
                        'Expandir',
                        style: TextStyle(
                          fontSize: 12,
                          fontWeight: FontWeight.w600,
                          color: Color(0xFF1E3A5F),
                        ),
                      ),
                    ],
                  ),
                ),
              ),

              // Vehicle count badge
              Positioned(
                left: 16,
                top: 16,
                child: Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 12,
                    vertical: 6,
                  ),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(20),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.black.withValues(alpha: 0.2),
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
                        size: 14,
                        color: Color(0xFFFF6B35),
                      ),
                      const SizedBox(width: 4),
                      Text(
                        '${vehicles.length}',
                        style: const TextStyle(
                          fontSize: 12,
                          fontWeight: FontWeight.bold,
                          color: Color(0xFF1E3A5F),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Set<Marker> _buildMarkers() {
    return vehicles.take(50).map((vehicle) {
      final coords = _parseLocation(vehicle.location);
      return Marker(
        markerId: MarkerId(vehicle.id),
        position: LatLng(coords[0], coords[1]),
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
    return [19.4326, -99.1332]; // Mexico City default
  }
}
