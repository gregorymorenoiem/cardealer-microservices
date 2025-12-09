import 'package:flutter/material.dart';
import '../../../core/responsive/breakpoints.dart';
import 'nav_rail.dart';

/// Adaptive navigation that switches between:
/// - BottomNavigationBar on mobile (< 600dp)
/// - NavigationRail on tablet (600dp - 1023dp)
/// - NavigationDrawer on desktop (>= 1024dp)
class AdaptiveNavigation extends StatelessWidget {
  const AdaptiveNavigation({
    super.key,
    required this.selectedIndex,
    required this.onDestinationSelected,
    required this.body,
    this.destinations = appNavDestinations,
  });
  
  /// Currently selected navigation index
  final int selectedIndex;
  
  /// Callback when a destination is selected
  final ValueChanged<int> onDestinationSelected;
  
  /// The main content body
  final Widget body;
  
  /// Navigation destinations
  final List<NavDestination> destinations;

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;
    
    // Desktop: Use NavigationDrawer
    if (width >= Breakpoints.xxl) {
      return _DesktopLayout(
        selectedIndex: selectedIndex,
        onDestinationSelected: onDestinationSelected,
        destinations: destinations,
        body: body,
      );
    }
    
    // Tablet: Use NavigationRail
    if (width >= Breakpoints.lg) {
      return _TabletLayout(
        selectedIndex: selectedIndex,
        onDestinationSelected: onDestinationSelected,
        destinations: destinations,
        body: body,
      );
    }
    
    // Mobile: Use BottomNavigationBar
    return _MobileLayout(
      selectedIndex: selectedIndex,
      onDestinationSelected: onDestinationSelected,
      destinations: destinations,
      body: body,
    );
  }
}

/// Mobile layout with bottom navigation bar
class _MobileLayout extends StatelessWidget {
  const _MobileLayout({
    required this.selectedIndex,
    required this.onDestinationSelected,
    required this.destinations,
    required this.body,
  });
  
  final int selectedIndex;
  final ValueChanged<int> onDestinationSelected;
  final List<NavDestination> destinations;
  final Widget body;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: body,
      bottomNavigationBar: AppBottomNavBar(
        selectedIndex: selectedIndex,
        onDestinationSelected: onDestinationSelected,
        destinations: destinations,
      ),
    );
  }
}

/// Tablet layout with navigation rail on the left
class _TabletLayout extends StatelessWidget {
  const _TabletLayout({
    required this.selectedIndex,
    required this.onDestinationSelected,
    required this.destinations,
    required this.body,
  });
  
  final int selectedIndex;
  final ValueChanged<int> onDestinationSelected;
  final List<NavDestination> destinations;
  final Widget body;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Row(
        children: [
          AppNavigationRail(
            selectedIndex: selectedIndex,
            onDestinationSelected: onDestinationSelected,
            destinations: destinations,
            extended: false,
          ),
          const VerticalDivider(thickness: 1, width: 1),
          Expanded(child: body),
        ],
      ),
    );
  }
}

/// Desktop layout with expanded navigation drawer
class _DesktopLayout extends StatelessWidget {
  const _DesktopLayout({
    required this.selectedIndex,
    required this.onDestinationSelected,
    required this.destinations,
    required this.body,
  });
  
  final int selectedIndex;
  final ValueChanged<int> onDestinationSelected;
  final List<NavDestination> destinations;
  final Widget body;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Row(
        children: [
          // Extended navigation rail that looks like a drawer
          AppNavigationRail(
            selectedIndex: selectedIndex,
            onDestinationSelected: onDestinationSelected,
            destinations: destinations,
            extended: true,
          ),
          const VerticalDivider(thickness: 1, width: 1),
          Expanded(child: body),
        ],
      ),
    );
  }
}

/// A scaffold wrapper that provides adaptive navigation
/// Use this as the root of your main navigation page
class AdaptiveScaffold extends StatelessWidget {
  const AdaptiveScaffold({
    super.key,
    required this.selectedIndex,
    required this.onDestinationSelected,
    required this.pages,
    this.destinations = appNavDestinations,
  });
  
  /// Currently selected page index
  final int selectedIndex;
  
  /// Callback when a destination is selected
  final ValueChanged<int> onDestinationSelected;
  
  /// List of pages to display (corresponds to destinations)
  final List<Widget> pages;
  
  /// Navigation destinations
  final List<NavDestination> destinations;

  @override
  Widget build(BuildContext context) {
    assert(
      pages.length == destinations.length,
      'Number of pages must match number of destinations',
    );
    
    return AdaptiveNavigation(
      selectedIndex: selectedIndex,
      onDestinationSelected: onDestinationSelected,
      destinations: destinations,
      body: IndexedStack(
        index: selectedIndex,
        children: pages,
      ),
    );
  }
}
