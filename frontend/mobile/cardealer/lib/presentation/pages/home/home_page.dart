import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/di/injection.dart';
import '../../../core/responsive/responsive_utils.dart';
import '../../bloc/vehicles/vehicles_bloc.dart';
import '../../bloc/vehicles/vehicles_event.dart';
import '../../bloc/vehicles/vehicles_state.dart';
import '../../widgets/home/premium_app_bar.dart';
import '../../widgets/home/categories_section.dart';
import '../../widgets/home/premium_hero_carousel.dart';
import '../../widgets/home/sell_car_cta.dart';
import '../../widgets/home/premium_featured_grid.dart';
import '../../widgets/home/horizontal_vehicle_section.dart';
import '../../widgets/home/daily_deals_section.dart';
import '../../widgets/home/recently_viewed_section.dart';
import '../../widgets/home/sponsored_listings_section.dart';
import '../../widgets/home/premium_refresh_indicator.dart';
import '../search/search_page.dart';

/// HomePage - Main landing page with premium design
/// Sprint 3: Home Reorganization - Revenue-optimized layout
/// Removed non-monetizable sections: Testimonials, Stats, Bottom CTA
/// Focus on vehicle listings with high engagement potential
class HomePage extends StatelessWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) =>
          getIt<VehiclesBloc>()..add(RefreshAllSectionsEvent()),
      child: Scaffold(
        appBar: PremiumHomeAppBar(
          onSearchTap: () {
            Navigator.of(context).push(
              MaterialPageRoute(
                builder: (context) => const SearchPage(),
              ),
            );
          },
          onNotificationsTap: () {
            // TODO: Navigate to notifications
          },
          onProfileTap: () {
            // TODO: Navigate to profile
          },
          showNotificationBadge: true,
          notificationCount: 3,
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
                        context
                            .read<VehiclesBloc>()
                            .add(RefreshAllSectionsEvent());
                      },
                      child: const Text('Reintentar'),
                    ),
                  ],
                ),
              );
            }

            if (state is VehiclesLoaded) {
              return PremiumRefreshIndicator(
                onRefresh: () async {
                  context.read<VehiclesBloc>().add(RefreshAllSectionsEvent());
                },
                child: ListView(
                  children: [
                    // Quick Filter Chips (below AppBar search)
                    CategoriesSection(
                      categories: getDefaultCategories(),
                      onCategoryTap: (category) {
                        // TODO: Filter by category
                      },
                    ),

                    SizedBox(height: context.spacing(1)),

                    // Premium Hero Carousel (5 featured vehicles with parallax)
                    PremiumHeroCarousel(
                      vehicles: state.heroCarousel,
                      onSeeAllTap: () {
                        // TODO: Navigate to featured vehicles
                      },
                    ),

                    SizedBox(height: context.spacing(1.5)),

                    // Sell Your Car CTA
                    SellYourCarCTA(
                      onTap: () {
                        // TODO: Navigate to sell car flow
                      },
                    ),

                    SizedBox(height: context.spacing(2)),

                    // Section 2: Sponsored Listings (Premium paid placement)
                    SponsoredListingsSection(
                      vehicles: state.featuredGrid.take(4).toList(),
                      onSeeAllTap: () {
                        // TODO: Navigate to sponsored vehicles
                      },
                    ),

                    SizedBox(height: context.spacing(1.5)),

                    // Section 3: Premium Featured Grid (6 vehicles, horizontal scroll)
                    PremiumFeaturedGrid(
                      vehicles: state.featuredGrid,
                      onSeeAllTap: () {
                        // TODO: Navigate to all featured vehicles
                      },
                    ),

                    SizedBox(height: context.spacing(1.5)),

                    // Section 4: Week's Featured (10 vehicles, horizontal scroll)
                    HorizontalVehicleSection(
                      title: 'Destacados',
                      subtitle: 'Esta semana',
                      vehicles: state.weekFeatured,
                      onSeeAllTap: () {
                        // TODO: Navigate to full list
                      },
                      showBadge: true,
                      badgeText: 'HOT',
                    ),

                    SizedBox(height: context.spacing(1.5)),

                    // Section 5: Daily Deals Section with countdown timers
                    DailyDealsSection(
                      vehicles: state.dailyDeals,
                      onSeeAllTap: () {
                        // TODO: Navigate to deals page
                      },
                    ),

                    SizedBox(height: context.spacing(1.5)),

                    // Section 6: SUVs & Camionetas (10 vehicles, horizontal scroll)
                    HorizontalVehicleSection(
                      title: 'SUVs',
                      subtitle: 'Potencia y espacio',
                      vehicles: state.suvsAndTrucks,
                      onSeeAllTap: () {
                        // TODO: Navigate to SUVs page
                      },
                      showBadge: true,
                      badgeText: '4x4',
                    ),

                    SizedBox(height: context.spacing(1.5)),

                    // Section 7: Colección Premium (10 vehicles, horizontal scroll)
                    HorizontalVehicleSection(
                      title: 'Premium',
                      subtitle: 'Lujo y exclusividad',
                      vehicles: state.premium,
                      onSeeAllTap: () {
                        // TODO: Navigate to premium page
                      },
                      showBadge: true,
                      badgeText: 'VIP',
                    ),

                    SizedBox(height: context.spacing(1.5)),

                    // Section 8: Eléctricos e Híbridos (10 vehicles, horizontal scroll)
                    HorizontalVehicleSection(
                      title: 'Eléctricos',
                      subtitle: 'Zero emisiones',
                      vehicles: state.electricAndHybrid,
                      onSeeAllTap: () {
                        // TODO: Navigate to electric page
                      },
                      showBadge: true,
                      badgeText: 'EV',
                    ),

                    SizedBox(height: context.spacing(2)),

                    // Section 9: Recently Viewed
                    RecentlyViewedSection(
                      vehicles: state.weekFeatured.take(5).toList(),
                      onSeeAllTap: () {
                        // TODO: Clear history
                      },
                    ),

                    SizedBox(height: context.spacing(3)),
                  ],
                ),
              );
            }

            return const SizedBox.shrink();
          },
        ),
      ),
    );
  }
}
