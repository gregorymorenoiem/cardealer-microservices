import 'package:flutter/material.dart';
import '../../../domain/entities/vehicle.dart';
import '../../../core/responsive/responsive_helper.dart';
import 'package:cached_network_image/cached_network_image.dart';

enum ViewMode { grid, list }

/// SE-007: Results View Toggle
/// Grid view and list view with animation transitions
class SearchResultsView extends StatefulWidget {
  final List<Vehicle> results;
  final String query;
  final int totalResults;

  const SearchResultsView({
    super.key,
    required this.results,
    required this.query,
    required this.totalResults,
  });

  @override
  State<SearchResultsView> createState() => _SearchResultsViewState();
}

class _SearchResultsViewState extends State<SearchResultsView> {
  ViewMode _viewMode = ViewMode.grid;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        // Results header with view toggle
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
          decoration: BoxDecoration(
            color: Colors.white,
            border: Border(
              bottom: BorderSide(
                color: Colors.grey.shade200,
                width: 1,
              ),
            ),
          ),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              // Results count
              RichText(
                text: TextSpan(
                  style: Theme.of(context).textTheme.bodyMedium,
                  children: [
                    TextSpan(
                      text: '${widget.totalResults} ',
                      style: const TextStyle(
                        fontWeight: FontWeight.bold,
                        color: Colors.black87,
                      ),
                    ),
                    const TextSpan(
                      text: 'results for ',
                      style: TextStyle(color: Colors.black54),
                    ),
                    TextSpan(
                      text: '"${widget.query}"',
                      style: const TextStyle(
                        fontWeight: FontWeight.w600,
                        color: Colors.black87,
                      ),
                    ),
                  ],
                ),
              ),

              // View toggle buttons
              Container(
                decoration: BoxDecoration(
                  color: Colors.grey.shade100,
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Row(
                  children: [
                    _ViewToggleButton(
                      icon: Icons.grid_view,
                      isSelected: _viewMode == ViewMode.grid,
                      onTap: () {
                        setState(() {
                          _viewMode = ViewMode.grid;
                        });
                      },
                    ),
                    _ViewToggleButton(
                      icon: Icons.view_list,
                      isSelected: _viewMode == ViewMode.list,
                      onTap: () {
                        setState(() {
                          _viewMode = ViewMode.list;
                        });
                      },
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),

        // Results
        Expanded(
          child: AnimatedSwitcher(
            duration: const Duration(milliseconds: 300),
            child: _viewMode == ViewMode.grid
                ? _GridView(vehicles: widget.results)
                : _ListView(vehicles: widget.results),
          ),
        ),
      ],
    );
  }
}

class _ViewToggleButton extends StatelessWidget {
  final IconData icon;
  final bool isSelected;
  final VoidCallback onTap;

  const _ViewToggleButton({
    required this.icon,
    required this.isSelected,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(8),
      child: Container(
        padding: const EdgeInsets.all(8),
        decoration: BoxDecoration(
          color: isSelected ? Colors.white : Colors.transparent,
          borderRadius: BorderRadius.circular(8),
          boxShadow: isSelected
              ? [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.05),
                    blurRadius: 4,
                    offset: const Offset(0, 2),
                  ),
                ]
              : null,
        ),
        child: Icon(
          icon,
          size: 20,
          color: isSelected
              ? Theme.of(context).primaryColor
              : Colors.grey.shade600,
        ),
      ),
    );
  }
}

class _GridView extends StatelessWidget {
  final List<Vehicle> vehicles;

  const _GridView({required this.vehicles});

  @override
  Widget build(BuildContext context) {
    final responsive = context.responsive;
    final columns = responsive.gridColumns; // 2 for mobile, 3 for tablet, 4 for desktop
    
    return GridView.builder(
      padding: EdgeInsets.all(responsive.horizontalPadding),
      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: columns,
        crossAxisSpacing: responsive.cardSpacing,
        mainAxisSpacing: responsive.cardSpacing,
        childAspectRatio: 0.75,
      ),
      itemCount: vehicles.length,
      itemBuilder: (context, index) {
        return _GridVehicleCard(vehicle: vehicles[index]);
      },
    );
  }
}

class _ListView extends StatelessWidget {
  final List<Vehicle> vehicles;

  const _ListView({required this.vehicles});

  @override
  Widget build(BuildContext context) {
    final responsive = context.responsive;
    
    return ListView.builder(
      padding: EdgeInsets.all(responsive.horizontalPadding),
      itemCount: vehicles.length,
      itemBuilder: (context, index) {
        return _ListVehicleCard(vehicle: vehicles[index]);
      },
    );
  }
}

class _GridVehicleCard extends StatelessWidget {
  final Vehicle vehicle;

  const _GridVehicleCard({required this.vehicle});

  @override
  Widget build(BuildContext context) {
    final responsive = context.responsive;
    
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(responsive.borderRadius),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Image
          ClipRRect(
            borderRadius: BorderRadius.vertical(
              top: Radius.circular(responsive.borderRadius),
            ),
            child: Stack(
              children: [
                CachedNetworkImage(
                  imageUrl: vehicle.mainImage,
                  height: 120,
                  width: double.infinity,
                  fit: BoxFit.cover,
                  placeholder: (context, url) => Container(
                    color: Colors.grey.shade200,
                    child: const Center(child: CircularProgressIndicator()),
                  ),
                  errorWidget: (context, url, error) => Container(
                    color: Colors.grey.shade200,
                    child: const Icon(Icons.car_rental, size: 32),
                  ),
                ),
                if (vehicle.isVerified)
                  Positioned(
                    top: 8,
                    right: 8,
                    child: Container(
                      padding: const EdgeInsets.all(6),
                      decoration: BoxDecoration(
                        color: Colors.white.withValues(alpha: 0.95),
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(
                        Icons.verified,
                        color: Colors.blue,
                        size: 16,
                      ),
                    ),
                  ),
              ],
            ),
          ),
          // Info
          Padding(
            padding: EdgeInsets.all(responsive.cardSpacing),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  '${vehicle.year} ${vehicle.make}',
                  style: TextStyle(
                    fontSize: responsive.bodyFontSize,
                    fontWeight: FontWeight.bold,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                Text(
                  vehicle.model,
                  style: TextStyle(
                    fontSize: responsive.smallFontSize,
                    color: Colors.grey.shade600,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                SizedBox(height: responsive.cardSpacing * 0.5),
                Text(
                  vehicle.formattedPrice,
                  style: TextStyle(
                    fontSize: responsive.titleFontSize,
                    fontWeight: FontWeight.bold,
                    color: Theme.of(context).primaryColor,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _ListVehicleCard extends StatelessWidget {
  final Vehicle vehicle;

  const _ListVehicleCard({required this.vehicle});

  @override
  Widget build(BuildContext context) {
    final responsive = context.responsive;
    final imageSize = responsive.isMobile ? 100.0 : 120.0;
    
    return Card(
      margin: EdgeInsets.only(bottom: responsive.cardSpacing),
      elevation: 2,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(responsive.borderRadius),
      ),
      child: Padding(
        padding: EdgeInsets.all(responsive.cardSpacing),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Image
            ClipRRect(
              borderRadius: BorderRadius.circular(responsive.borderRadius * 0.67),
              child: Stack(
                children: [
                  CachedNetworkImage(
                    imageUrl: vehicle.mainImage,
                    width: imageSize,
                    height: imageSize,
                    fit: BoxFit.cover,
                    placeholder: (context, url) => Container(
                      color: Colors.grey.shade200,
                      child: const Center(child: CircularProgressIndicator()),
                    ),
                    errorWidget: (context, url, error) => Container(
                      color: Colors.grey.shade200,
                      child: const Icon(Icons.car_rental, size: 32),
                    ),
                  ),
                  if (vehicle.isVerified)
                    Positioned(
                      top: 6,
                      right: 6,
                      child: Container(
                        padding: const EdgeInsets.all(4),
                        decoration: BoxDecoration(
                          color: Colors.white.withValues(alpha: 0.95),
                          shape: BoxShape.circle,
                        ),
                        child: const Icon(
                          Icons.verified,
                          color: Colors.blue,
                          size: 14,
                        ),
                      ),
                    ),
                ],
              ),
            ),
            SizedBox(width: responsive.cardSpacing),
            // Info
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '${vehicle.year} ${vehicle.make}',
                    style: TextStyle(
                      fontSize: responsive.bodyFontSize,
                      fontWeight: FontWeight.bold,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  Text(
                    vehicle.model,
                    style: TextStyle(
                      fontSize: responsive.bodyFontSize,
                      color: Colors.grey.shade600,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  SizedBox(height: responsive.cardSpacing * 0.5),
                  Text(
                    vehicle.formattedPrice,
                    style: TextStyle(
                      fontSize: responsive.titleFontSize,
                      fontWeight: FontWeight.bold,
                      color: Theme.of(context).primaryColor,
                    ),
                  ),
                  SizedBox(height: responsive.cardSpacing * 0.5),
                  Row(
                    children: [
                      Icon(Icons.speed, size: responsive.iconSize * 0.7, color: Colors.grey.shade600),
                      SizedBox(width: responsive.cardSpacing * 0.25),
                      Text(
                        vehicle.formattedMileage,
                        style: TextStyle(
                          fontSize: responsive.smallFontSize,
                          color: Colors.grey.shade600,
                        ),
                      ),
                      SizedBox(width: responsive.cardSpacing),
                      Icon(Icons.location_on, size: responsive.iconSize * 0.7, color: Colors.grey.shade600),
                      SizedBox(width: responsive.cardSpacing * 0.25),
                      Expanded(
                        child: Text(
                          vehicle.location.split(',').first,
                          style: TextStyle(
                            fontSize: responsive.smallFontSize,
                            color: Colors.grey.shade600,
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
          ],
        ),
      ),
    );
  }
}
