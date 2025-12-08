import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/di/injection.dart';
import '../../../domain/entities/location.dart';
import '../../bloc/map/map_bloc.dart';
import '../../bloc/map/map_event.dart';
import '../../bloc/map/map_state.dart';

class MapViewPage extends StatelessWidget {
  final Location? initialLocation;
  final double? initialRadius;

  const MapViewPage({
    super.key,
    this.initialLocation,
    this.initialRadius,
  });

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (_) => getIt<MapBloc>()
        ..add(InitializeMap(
          initialLocation: initialLocation,
          initialRadius: initialRadius,
        )),
      child: const _MapViewContent(),
    );
  }
}

class _MapViewContent extends StatelessWidget {
  const _MapViewContent();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Mapa de Vehículos'),
        actions: [
          IconButton(
            icon: const Icon(Icons.filter_list),
            onPressed: () => _showFilterSheet(context),
          ),
          IconButton(
            icon: const Icon(Icons.layers),
            onPressed: () => _showMapTypeSheet(context),
          ),
        ],
      ),
      body: BlocConsumer<MapBloc, MapState>(
        listener: (context, state) {
          if (state is MapError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red,
              ),
            );
          }
          if (state is MapLocationError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                action: SnackBarAction(
                  label: 'Reintentar',
                  onPressed: () {
                    context.read<MapBloc>().add(GetCurrentLocationEvent());
                  },
                ),
              ),
            );
          }
        },
        builder: (context, state) {
          if (state is MapLoading) {
            return const Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  CircularProgressIndicator(),
                  SizedBox(height: 16),
                  Text('Cargando mapa...'),
                ],
              ),
            );
          }

          if (state is MapError) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.error_outline, size: 64, color: Colors.red),
                  const SizedBox(height: 16),
                  Text(state.message),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () {
                      context.read<MapBloc>().add(RefreshMap());
                    },
                    child: const Text('Reintentar'),
                  ),
                ],
              ),
            );
          }

          if (state is MapLoaded) {
            return Stack(
              children: [
                // Map placeholder (Google Maps se integrará con el package)
                _MapWidget(
                  center: state.searchArea.center,
                  markers: state.markers,
                  clusters: state.clusters,
                  selectedMarker: state.selectedMarker,
                  zoomLevel: state.zoomLevel,
                  mapType: state.mapType,
                  onMarkerTapped: (marker) {
                    context.read<MapBloc>().add(SelectMarker(marker: marker));
                  },
                  onMapTapped: () {
                    context.read<MapBloc>().add(DeselectMarker());
                  },
                  onCameraMove: (center, zoom) {
                    context
                        .read<MapBloc>()
                        .add(UpdateZoomLevel(zoomLevel: zoom));
                  },
                ),

                // Search area indicator
                Positioned(
                  top: 16,
                  left: 16,
                  right: 16,
                  child: _SearchAreaCard(
                    radius: state.searchArea.radiusKm,
                    vehicleCount: state.markers.length,
                    onRadiusChanged: (radius) {
                      context
                          .read<MapBloc>()
                          .add(UpdateRadius(radiusKm: radius));
                    },
                  ),
                ),

                // My location button
                Positioned(
                  right: 16,
                  bottom: state.selectedMarker != null ? 280 : 100,
                  child: FloatingActionButton(
                    heroTag: 'my_location',
                    onPressed: () {
                      context.read<MapBloc>().add(GetCurrentLocationEvent());
                    },
                    child: const Icon(Icons.my_location),
                  ),
                ),

                // Selected marker info
                if (state.selectedMarker != null)
                  Positioned(
                    left: 0,
                    right: 0,
                    bottom: 0,
                    child: _VehicleInfoCard(
                      marker: state.selectedMarker!,
                      onClose: () {
                        context.read<MapBloc>().add(DeselectMarker());
                      },
                      onViewDetails: () {
                        // TODO: Navigate to vehicle detail
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(
                            content: Text(
                                'Ver detalles de ${state.selectedMarker!.title}'),
                          ),
                        );
                      },
                    ),
                  ),
              ],
            );
          }

          return const SizedBox.shrink();
        },
      ),
    );
  }

  void _showFilterSheet(BuildContext context) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (sheetContext) => _FilterBottomSheet(
        onApplyFilters: (brands, minPrice, maxPrice) {
          context.read<MapBloc>().add(SearchVehiclesInArea(
                brands: brands,
                minPrice: minPrice,
                maxPrice: maxPrice,
              ));
          Navigator.pop(sheetContext);
        },
      ),
    );
  }

  void _showMapTypeSheet(BuildContext context) {
    showModalBottomSheet(
      context: context,
      builder: (sheetContext) => _MapTypeSheet(
        onTypeSelected: (mapType) {
          context.read<MapBloc>().add(ChangeMapType(mapType: mapType));
          Navigator.pop(sheetContext);
        },
      ),
    );
  }
}

// Map widget placeholder (integrar Google Maps después)
class _MapWidget extends StatelessWidget {
  final Location center;
  final List<VehicleMarker> markers;
  final List<MarkerCluster> clusters;
  final VehicleMarker? selectedMarker;
  final double zoomLevel;
  final MapType mapType;
  final Function(VehicleMarker) onMarkerTapped;
  final VoidCallback onMapTapped;
  final Function(Location, double) onCameraMove;

  const _MapWidget({
    required this.center,
    required this.markers,
    required this.clusters,
    this.selectedMarker,
    required this.zoomLevel,
    required this.mapType,
    required this.onMarkerTapped,
    required this.onMapTapped,
    required this.onCameraMove,
  });

  @override
  Widget build(BuildContext context) {
    // TODO: Integrar Google Maps Flutter
    return Container(
      color: Colors.grey[300],
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.map, size: 64, color: Colors.grey),
            const SizedBox(height: 16),
            Text(
              'Mapa: ${center.displayText}',
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 8),
            Text(
              '${markers.length} vehículos encontrados',
              style: Theme.of(context).textTheme.bodyMedium,
            ),
            const SizedBox(height: 8),
            Text(
              'Zoom: ${zoomLevel.toStringAsFixed(1)}',
              style: Theme.of(context).textTheme.bodySmall,
            ),
            const SizedBox(height: 16),
            const Text(
              'Google Maps se integrará con el package\ngoogle_maps_flutter',
              textAlign: TextAlign.center,
              style: TextStyle(color: Colors.grey, fontSize: 12),
            ),
          ],
        ),
      ),
    );
  }
}

// Search area card
class _SearchAreaCard extends StatelessWidget {
  final double radius;
  final int vehicleCount;
  final Function(double) onRadiusChanged;

  const _SearchAreaCard({
    required this.radius,
    required this.vehicleCount,
    required this.onRadiusChanged,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisSize: MainAxisSize.min,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Radio: ${radius.toStringAsFixed(0)} km',
                  style: Theme.of(context).textTheme.titleMedium,
                ),
                Text(
                  '$vehicleCount vehículos',
                  style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                        color: Theme.of(context).colorScheme.primary,
                        fontWeight: FontWeight.bold,
                      ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Slider(
              value: radius,
              min: 1,
              max: 50,
              divisions: 49,
              label: '${radius.toStringAsFixed(0)} km',
              onChanged: onRadiusChanged,
            ),
          ],
        ),
      ),
    );
  }
}

// Vehicle info card
class _VehicleInfoCard extends StatelessWidget {
  final VehicleMarker marker;
  final VoidCallback onClose;
  final VoidCallback onViewDetails;

  const _VehicleInfoCard({
    required this.marker,
    required this.onClose,
    required this.onViewDetails,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 260,
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surface,
        borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.1),
            blurRadius: 10,
            offset: const Offset(0, -5),
          ),
        ],
      ),
      child: Column(
        children: [
          // Handle bar
          Container(
            margin: const EdgeInsets.symmetric(vertical: 8),
            width: 40,
            height: 4,
            decoration: BoxDecoration(
              color: Colors.grey[300],
              borderRadius: BorderRadius.circular(2),
            ),
          ),

          Expanded(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Image
                      ClipRRect(
                        borderRadius: BorderRadius.circular(8),
                        child: marker.imageUrl != null
                            ? Image.network(
                                marker.imageUrl!,
                                width: 100,
                                height: 80,
                                fit: BoxFit.cover,
                                errorBuilder: (_, __, ___) => Container(
                                  width: 100,
                                  height: 80,
                                  color: Colors.grey[300],
                                  child: const Icon(Icons.directions_car),
                                ),
                              )
                            : Container(
                                width: 100,
                                height: 80,
                                color: Colors.grey[300],
                                child: const Icon(Icons.directions_car),
                              ),
                      ),
                      const SizedBox(width: 12),

                      // Info
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              marker.title,
                              style: Theme.of(context)
                                  .textTheme
                                  .titleMedium
                                  ?.copyWith(
                                    fontWeight: FontWeight.bold,
                                  ),
                              maxLines: 2,
                              overflow: TextOverflow.ellipsis,
                            ),
                            const SizedBox(height: 4),
                            Text(
                              marker.priceFormatted,
                              style: Theme.of(context)
                                  .textTheme
                                  .titleLarge
                                  ?.copyWith(
                                    color:
                                        Theme.of(context).colorScheme.primary,
                                    fontWeight: FontWeight.bold,
                                  ),
                            ),
                            const SizedBox(height: 4),
                            Row(
                              children: [
                                const Icon(Icons.location_on,
                                    size: 14, color: Colors.grey),
                                const SizedBox(width: 4),
                                Expanded(
                                  child: Text(
                                    marker.location.shortDisplay,
                                    style: Theme.of(context)
                                        .textTheme
                                        .bodySmall
                                        ?.copyWith(
                                          color: Colors.grey[600],
                                        ),
                                    maxLines: 1,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),

                      // Close button
                      IconButton(
                        icon: const Icon(Icons.close),
                        onPressed: onClose,
                      ),
                    ],
                  ),

                  const SizedBox(height: 16),

                  // Badges
                  Wrap(
                    spacing: 8,
                    children: [
                      if (marker.isFeatured)
                        const Chip(
                          label: Text('Destacado'),
                          backgroundColor: Colors.amber,
                          labelStyle: TextStyle(fontSize: 11),
                        ),
                      if (marker.isFavorite)
                        const Chip(
                          label: Text('Favorito'),
                          avatar: Icon(Icons.favorite, size: 14),
                          labelStyle: TextStyle(fontSize: 11),
                        ),
                    ],
                  ),

                  const Spacer(),

                  // Action buttons
                  Row(
                    children: [
                      Expanded(
                        child: OutlinedButton.icon(
                          onPressed: () {
                            // TODO: Direcciones
                          },
                          icon: const Icon(Icons.directions),
                          label: const Text('Cómo llegar'),
                        ),
                      ),
                      const SizedBox(width: 8),
                      Expanded(
                        child: ElevatedButton.icon(
                          onPressed: onViewDetails,
                          icon: const Icon(Icons.info_outline),
                          label: const Text('Ver detalles'),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

// Filter bottom sheet
class _FilterBottomSheet extends StatefulWidget {
  final Function(List<String>?, double?, double?) onApplyFilters;

  const _FilterBottomSheet({required this.onApplyFilters});

  @override
  State<_FilterBottomSheet> createState() => _FilterBottomSheetState();
}

class _FilterBottomSheetState extends State<_FilterBottomSheet> {
  final List<String> _selectedBrands = [];
  double? _minPrice;
  double? _maxPrice;

  final List<String> _availableBrands = [
    'Toyota',
    'Honda',
    'Nissan',
    'Mazda',
    'Ford',
    'BMW',
    'Hyundai',
  ];

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                'Filtros',
                style: Theme.of(context).textTheme.titleLarge,
              ),
              TextButton(
                onPressed: () {
                  setState(() {
                    _selectedBrands.clear();
                    _minPrice = null;
                    _maxPrice = null;
                  });
                },
                child: const Text('Limpiar'),
              ),
            ],
          ),
          const SizedBox(height: 16),

          // Brands
          Text(
            'Marcas',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 8),
          Wrap(
            spacing: 8,
            children: _availableBrands.map((brand) {
              final isSelected = _selectedBrands.contains(brand);
              return FilterChip(
                label: Text(brand),
                selected: isSelected,
                onSelected: (selected) {
                  setState(() {
                    if (selected) {
                      _selectedBrands.add(brand);
                    } else {
                      _selectedBrands.remove(brand);
                    }
                  });
                },
              );
            }).toList(),
          ),

          const SizedBox(height: 24),

          // Price range
          Text(
            'Rango de precio',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              Expanded(
                child: TextField(
                  decoration: const InputDecoration(
                    labelText: 'Min',
                    prefixText: '\$',
                    border: OutlineInputBorder(),
                  ),
                  keyboardType: TextInputType.number,
                  onChanged: (value) {
                    _minPrice = double.tryParse(value);
                  },
                ),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: TextField(
                  decoration: const InputDecoration(
                    labelText: 'Max',
                    prefixText: '\$',
                    border: OutlineInputBorder(),
                  ),
                  keyboardType: TextInputType.number,
                  onChanged: (value) {
                    _maxPrice = double.tryParse(value);
                  },
                ),
              ),
            ],
          ),

          const SizedBox(height: 24),

          // Apply button
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: () {
                widget.onApplyFilters(
                  _selectedBrands.isEmpty ? null : _selectedBrands,
                  _minPrice,
                  _maxPrice,
                );
              },
              child: const Text('Aplicar filtros'),
            ),
          ),
        ],
      ),
    );
  }
}

// Map type sheet
class _MapTypeSheet extends StatelessWidget {
  final Function(MapType) onTypeSelected;

  const _MapTypeSheet({required this.onTypeSelected});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Tipo de mapa',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 16),
          ListTile(
            leading: const Icon(Icons.map),
            title: const Text('Normal'),
            onTap: () => onTypeSelected(MapType.normal),
          ),
          ListTile(
            leading: const Icon(Icons.satellite),
            title: const Text('Satélite'),
            onTap: () => onTypeSelected(MapType.satellite),
          ),
          ListTile(
            leading: const Icon(Icons.terrain),
            title: const Text('Terreno'),
            onTap: () => onTypeSelected(MapType.terrain),
          ),
          ListTile(
            leading: const Icon(Icons.layers),
            title: const Text('Híbrido'),
            onTap: () => onTypeSelected(MapType.hybrid),
          ),
        ],
      ),
    );
  }
}
