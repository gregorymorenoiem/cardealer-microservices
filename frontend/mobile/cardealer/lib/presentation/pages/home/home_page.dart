import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/di/injection.dart';
import '../../bloc/vehicles/vehicles_bloc.dart';
import '../../bloc/vehicles/vehicles_event.dart';
import '../../bloc/vehicles/vehicles_state.dart';
import '../../widgets/home/hero_carousel_section.dart';
import '../../widgets/home/featured_grid_section.dart';
import '../../widgets/home/horizontal_vehicle_section.dart';
import '../../widgets/home/features_section.dart';
import '../../widgets/home/how_it_works_section.dart';
import '../../widgets/home/cta_section.dart';

/// HomePage - Main landing page with 7 monetization sections
/// Sprint 3: 71 vehicles across multiple sections
class HomePage extends StatelessWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => getIt<VehiclesBloc>()..add(RefreshAllSectionsEvent()),
      child: Scaffold(
        appBar: AppBar(
          title: const Text('CarDealer'),
          actions: [
            IconButton(
              icon: const Icon(Icons.search),
              onPressed: () {
                // TODO: Navigate to search page
              },
            ),
            IconButton(
              icon: const Icon(Icons.filter_list),
              onPressed: () {
                // TODO: Show filter bottom sheet
              },
            ),
          ],
        ),
        body: BlocBuilder<VehiclesBloc, VehiclesState>(
          builder: (context, state) {
            if (state is VehiclesLoading) {
              return const Center(
                child: CircularProgressIndicator(),
              );
            }

            if (state is VehiclesError) {
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
                      'Error: ${state.message}',
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 16),
                    ElevatedButton(
                      onPressed: () {
                        context.read<VehiclesBloc>().add(RefreshAllSectionsEvent());
                      },
                      child: const Text('Retry'),
                    ),
                  ],
                ),
              );
            }

            if (state is VehiclesLoaded) {
              return RefreshIndicator(
                onRefresh: () async {
                  context.read<VehiclesBloc>().add(RefreshAllSectionsEvent());
                },
                child: ListView(
                  children: [
                    // Section 1: Hero Carousel (5 featured vehicles)
                    HeroCarouselSection(vehicles: state.heroCarousel),
                    
                    const SizedBox(height: 24),

                    // Section 2: Featured Grid (6 vehicles, 2 columns)
                    FeaturedGridSection(vehicles: state.featuredGrid),
                    
                    const SizedBox(height: 24),

                    // Section 3: Week's Featured (10 vehicles, horizontal scroll)
                    HorizontalVehicleSection(
                      title: "This Week's Featured",
                      subtitle: 'Hand-picked vehicles for you',
                      vehicles: state.weekFeatured,
                      onSeeAllTap: () {
                        // TODO: Navigate to full list
                      },
                    ),
                    
                    const SizedBox(height: 24),

                    // Section 4: Daily Deals (10 vehicles, horizontal scroll)
                    HorizontalVehicleSection(
                      title: 'Daily Deals',
                      subtitle: 'Limited time offers',
                      vehicles: state.dailyDeals,
                      onSeeAllTap: () {
                        // TODO: Navigate to deals page
                      },
                      showBadge: true,
                      badgeText: 'DEAL',
                    ),
                    
                    const SizedBox(height: 24),

                    // Section 5: SUVs & Trucks (10 vehicles, horizontal scroll)
                    HorizontalVehicleSection(
                      title: 'SUVs & Trucks',
                      subtitle: 'Power and space combined',
                      vehicles: state.suvsAndTrucks,
                      onSeeAllTap: () {
                        // TODO: Navigate to SUVs page
                      },
                    ),
                    
                    const SizedBox(height: 24),

                    // Section 6: Premium Vehicles (10 vehicles, horizontal scroll)
                    HorizontalVehicleSection(
                      title: 'Premium Collection',
                      subtitle: 'Luxury at its finest',
                      vehicles: state.premium,
                      onSeeAllTap: () {
                        // TODO: Navigate to premium page
                      },
                      showBadge: true,
                      badgeText: 'PREMIUM',
                    ),
                    
                    const SizedBox(height: 24),

                    // Section 7: Electric & Hybrid (10 vehicles, horizontal scroll)
                    HorizontalVehicleSection(
                      title: 'Electric & Hybrid',
                      subtitle: 'The future of driving',
                      vehicles: state.electricAndHybrid,
                      onSeeAllTap: () {
                        // TODO: Navigate to electric page
                      },
                      showBadge: true,
                      badgeText: 'ECO',
                    ),
                    
                    const SizedBox(height: 32),

                    // Features Section
                    const FeaturesSection(),
                    
                    const SizedBox(height: 32),

                    // How It Works Section
                    const HowItWorksSection(),
                    
                    const SizedBox(height: 32),

                    // CTA Section
                    const CTASection(),
                    
                    const SizedBox(height: 32),
                  ],
                ),
              );
            }

            return const SizedBox.shrink();
          },
        ),
        floatingActionButton: FloatingActionButton.extended(
          onPressed: () {
            // TODO: Navigate to sell car page
          },
          icon: const Icon(Icons.add),
          label: const Text('Sell Your Car'),
        ),
      ),
    );
  }
}
