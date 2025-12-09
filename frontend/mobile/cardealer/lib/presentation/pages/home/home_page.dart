import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/di/injection.dart';
import '../../../core/responsive/responsive_utils.dart';
import '../../bloc/vehicles/vehicles_bloc.dart';
import '../../bloc/vehicles/vehicles_event.dart';
import '../../bloc/vehicles/vehicles_state.dart';
import '../../widgets/home/premium_app_bar.dart';
import '../../widgets/home/hero_search_section.dart';
import '../../widgets/home/categories_section.dart';
import '../../widgets/home/premium_hero_carousel.dart';
import '../../widgets/home/sell_car_cta.dart';
import '../../widgets/home/premium_featured_grid.dart';
import '../../widgets/home/horizontal_vehicle_section.dart';
import '../../widgets/home/daily_deals_section.dart';
import '../../widgets/home/recently_viewed_section.dart';
import '../../widgets/home/testimonials_carousel.dart';
import '../../widgets/home/stats_section.dart';
import '../../widgets/home/bottom_cta_section.dart';
import '../../widgets/home/premium_refresh_indicator.dart';

/// HomePage - Main landing page with premium design
/// Sprint 3: Home Redesign with new premium components
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
            // TODO: Navigate to search page
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
                      child: const Text('Retry'),
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
                    // Hero Search Section
                    HeroSearchSection(
                      onSearchTap: () {
                        // TODO: Navigate to search page
                      },
                      onVoiceSearchTap: () {
                        // TODO: Activate voice search
                      },
                      onSearchSubmitted: (query) {
                        // TODO: Execute search
                      },
                      quickSuggestions: const [
                        'Toyota Camry',
                        'SUVs 2024',
                        'Híbridos',
                        'Menos de \$20,000',
                      ],
                    ),

                    SizedBox(height: context.spacing(1)),

                    // Categories Section
                    CategoriesSection(
                      categories: getDefaultCategories(),
                      onCategoryTap: (category) {
                        // TODO: Filter by category
                      },
                    ),

                    SizedBox(height: context.spacing(1.5)),

                    // Premium Hero Carousel (5 featured vehicles with parallax)
                    PremiumHeroCarousel(
                      vehicles: state.heroCarousel,
                      onSeeAllTap: () {
                        // TODO: Navigate to featured vehicles
                      },
                    ),

                    SizedBox(height: context.spacing(2)),

                    // Sell Your Car CTA
                    SellYourCarCTA(
                      onTap: () {
                        // TODO: Navigate to sell car flow
                      },
                    ),

                    SizedBox(height: context.spacing(1.5)),

                    // Section 2: Premium Featured Grid (6 vehicles, glassmorphism)
                    PremiumFeaturedGrid(
                      vehicles: state.featuredGrid,
                      onSeeAllTap: () {
                        // TODO: Navigate to all featured vehicles
                      },
                    ),

                    SizedBox(height: context.spacing(1.5)),

                    // Section 3: Week's Featured (10 vehicles, horizontal scroll)
                    HorizontalVehicleSection(
                      title: "This Week's Featured",
                      subtitle: 'Hand-picked vehicles for you',
                      vehicles: state.weekFeatured,
                      onSeeAllTap: () {
                        // TODO: Navigate to full list
                      },
                      showBadge: true,
                      badgeText: 'FEATURED',
                    ),

                    SizedBox(height: context.spacing(1.5)),

                    // Section 4: Daily Deals Section with countdown timers
                    DailyDealsSection(
                      vehicles: state.dailyDeals,
                      onSeeAllTap: () {
                        // TODO: Navigate to deals page
                      },
                    ),

                    SizedBox(height: context.spacing(1.5)),

                    // Section 5: SUVs & Trucks (10 vehicles, horizontal scroll)
                    HorizontalVehicleSection(
                      title: 'SUVs & Trucks',
                      subtitle: 'Power and space combined',
                      vehicles: state.suvsAndTrucks,
                      onSeeAllTap: () {
                        // TODO: Navigate to SUVs page
                      },
                      showBadge: true,
                      badgeText: 'POWER',
                    ),

                    SizedBox(height: context.spacing(1.5)),

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

                    SizedBox(height: context.spacing(1.5)),

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

                    SizedBox(height: context.spacing(2)),

                    // Section 8: Recently Viewed
                    RecentlyViewedSection(
                      vehicles: state.weekFeatured.take(5).toList(),
                      onSeeAllTap: () {
                        // TODO: Clear history
                      },
                    ),

                    SizedBox(height: context.spacing(2)),

                    // Section 9: Testimonials Carousel
                    const TestimonialsCarousel(
                      testimonials: [],
                      onSeeAllReviews: null,
                    ),

                    SizedBox(height: context.spacing(2)),

                    // Section 10: Stats Section
                    const StatsSection(),

                    SizedBox(height: context.spacing(2)),

                    // Section 11: Bottom CTA
                    BottomCTASection(
                      onBrowseCars: () {
                        // TODO: Navigate to search/browse
                      },
                      onSellCar: () {
                        // TODO: Navigate to sell flow
                      },
                    ),

                    SizedBox(height: context.spacing(2)),
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
