import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/di/injection.dart';
import '../../bloc/vehicle_detail/vehicle_detail_bloc.dart';
import '../../bloc/vehicle_detail/vehicle_detail_event.dart';
import '../../bloc/vehicle_detail/vehicle_detail_state.dart';
import '../../../domain/entities/vehicle.dart';
import 'widgets/vehicle_image_gallery.dart';
import 'widgets/vehicle_info_header.dart';
import 'widgets/vehicle_specs_grid.dart';
import 'widgets/vehicle_features_list.dart';
import 'widgets/vehicle_description.dart';
import 'widgets/seller_info_card.dart';
import 'widgets/similar_vehicles_section.dart';
import 'widgets/contact_action_bar.dart';

/// Vehicle detail page showing full information
class VehicleDetailPage extends StatelessWidget {
  final String vehicleId;

  const VehicleDetailPage({
    super.key,
    required this.vehicleId,
  });

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) =>
          getIt<VehicleDetailBloc>()..add(LoadVehicleDetail(vehicleId)),
      child: Scaffold(
        body: BlocConsumer<VehicleDetailBloc, VehicleDetailState>(
          listener: (context, state) {
            if (state is ContactSellerSuccess) {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('Mensaje enviado exitosamente'),
                  backgroundColor: Colors.green,
                ),
              );
            } else if (state is ContactSellerError) {
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text(state.message),
                  backgroundColor: Colors.red,
                ),
              );
            } else if (state is ShareVehicleSuccess) {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('Vehículo compartido'),
                  duration: Duration(seconds: 1),
                ),
              );
            }
          },
          builder: (context, state) {
            if (state is VehicleDetailLoading) {
              return const Center(
                child: CircularProgressIndicator(),
              );
            }

            if (state is VehicleDetailError) {
              return Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Icon(
                      Icons.error_outline,
                      size: 64,
                      color: Colors.red,
                    ),
                    const SizedBox(height: 16),
                    Text(
                      'Error al cargar el vehículo',
                      style: Theme.of(context).textTheme.titleLarge,
                    ),
                    const SizedBox(height: 8),
                    Text(
                      state.message,
                      style: Theme.of(context).textTheme.bodyMedium,
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 24),
                    ElevatedButton.icon(
                      onPressed: () {
                        context.read<VehicleDetailBloc>().add(
                              LoadVehicleDetail(vehicleId),
                            );
                      },
                      icon: const Icon(Icons.refresh),
                      label: const Text('Reintentar'),
                    ),
                  ],
                ),
              );
            }

            if (state is VehicleDetailLoaded ||
                state is ContactingSellerState ||
                state is ContactSellerSuccess ||
                state is ContactSellerError) {
              final Vehicle vehicle;
              if (state is VehicleDetailLoaded) {
                vehicle = state.vehicle;
              } else if (state is ContactingSellerState) {
                vehicle = state.vehicle;
              } else if (state is ContactSellerSuccess) {
                vehicle = state.vehicle;
              } else {
                vehicle = (state as ContactSellerError).vehicle;
              }

              return Stack(
                children: [
                  CustomScrollView(
                    slivers: [
                      // App bar with back button and actions
                      SliverAppBar(
                        expandedHeight: 300,
                        pinned: true,
                        flexibleSpace: FlexibleSpaceBar(
                          background: VehicleImageGallery(
                            images: vehicle.images,
                            heroTag: 'vehicle_${vehicle.id}',
                          ),
                        ),
                        actions: [
                          // Favorite button
                          BlocBuilder<VehicleDetailBloc, VehicleDetailState>(
                            builder: (context, state) {
                              final isFavorite = state is VehicleDetailLoaded
                                  ? state.isFavorite
                                  : false;

                              return IconButton(
                                onPressed: () {
                                  context.read<VehicleDetailBloc>().add(
                                        ToggleVehicleFavorite(vehicle.id),
                                      );
                                },
                                icon: Icon(
                                  isFavorite
                                      ? Icons.favorite
                                      : Icons.favorite_border,
                                  color: isFavorite ? Colors.red : Colors.white,
                                ),
                              );
                            },
                          ),
                          // Share button
                          IconButton(
                            onPressed: () {
                              context.read<VehicleDetailBloc>().add(
                                    ShareVehicle(
                                      vehicleId: vehicle.id,
                                      title:
                                          '${vehicle.make} ${vehicle.model} ${vehicle.year}',
                                    ),
                                  );
                            },
                            icon: const Icon(Icons.share),
                          ),
                        ],
                      ),

                      // Content
                      SliverToBoxAdapter(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            // Price and title
                            VehicleInfoHeader(vehicle: vehicle),

                            const Divider(height: 1),

                            // Specs grid
                            VehicleSpecsGrid(vehicle: vehicle),

                            const Divider(height: 1),

                            // Features list
                            VehicleFeaturesList(vehicle: vehicle),

                            const Divider(height: 1),

                            // Description
                            VehicleDescription(vehicle: vehicle),

                            const Divider(height: 1),

                            // Seller info
                            SellerInfoCard(vehicle: vehicle),

                            const Divider(height: 1),

                            // Similar vehicles
                            BlocBuilder<VehicleDetailBloc, VehicleDetailState>(
                              builder: (context, state) {
                                if (state is VehicleDetailLoaded) {
                                  return SimilarVehiclesSection(
                                    vehicles: state.similarVehicles ?? [],
                                    isLoading: state.isLoadingSimilar,
                                  );
                                }
                                return const SizedBox.shrink();
                              },
                            ),

                            // Bottom padding for action bar
                            const SizedBox(height: 80),
                          ],
                        ),
                      ),
                    ],
                  ),

                  // Contact action bar (sticky bottom)
                  Positioned(
                    left: 0,
                    right: 0,
                    bottom: 0,
                    child: ContactActionBar(
                      vehicle: vehicle,
                      onCall: () => _handleCall(context, vehicle),
                      onWhatsApp: () => _handleWhatsApp(context, vehicle),
                      onMessage: () => _handleMessage(context, vehicle),
                    ),
                  ),
                ],
              );
            }

            return const SizedBox.shrink();
          },
        ),
      ),
    );
  }

  void _handleCall(BuildContext context, Vehicle vehicle) {
    // TODO: Implement phone call using url_launcher
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Llamando...')),
    );
  }

  void _handleWhatsApp(BuildContext context, Vehicle vehicle) {
    // TODO: Implement WhatsApp message using url_launcher
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Abriendo WhatsApp...')),
    );
  }

  void _handleMessage(BuildContext context, Vehicle vehicle) {
    // Show message dialog
    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Enviar mensaje'),
        content: TextField(
          maxLines: 4,
          decoration: const InputDecoration(
            hintText: 'Escribe tu mensaje...',
            border: OutlineInputBorder(),
          ),
          onChanged: (value) {
            // Store message
          },
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(),
            child: const Text('Cancelar'),
          ),
          ElevatedButton(
            onPressed: () {
              // TODO: Get message from TextField
              const message = 'Estoy interesado en este vehículo';

              context.read<VehicleDetailBloc>().add(
                    ContactSellerEvent(
                      vehicleId: vehicle.id,
                      sellerId:
                          'seller_${vehicle.id}', // TODO: Add sellerId to Vehicle entity
                      message: message,
                    ),
                  );

              Navigator.of(dialogContext).pop();
            },
            child: const Text('Enviar'),
          ),
        ],
      ),
    );
  }
}
