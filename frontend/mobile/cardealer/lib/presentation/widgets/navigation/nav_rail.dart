import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';

/// Navigation destination data
class NavDestination {
  final String label;
  final IconData icon;
  final IconData selectedIcon;

  const NavDestination({
    required this.label,
    required this.icon,
    required this.selectedIcon,
  });
}

/// Default navigation destinations for the app
const List<NavDestination> appNavDestinations = [
  NavDestination(
    label: 'Inicio',
    icon: Icons.home_outlined,
    selectedIcon: Icons.home,
  ),
  NavDestination(
    label: 'Favoritos',
    icon: Icons.favorite_outline,
    selectedIcon: Icons.favorite,
  ),
  NavDestination(
    label: 'Mensajes',
    icon: Icons.chat_bubble_outline,
    selectedIcon: Icons.chat_bubble,
  ),
  NavDestination(
    label: 'Perfil',
    icon: Icons.person_outline,
    selectedIcon: Icons.person,
  ),
];

/// NavigationRail for tablets (600dp - 1023dp)
/// Shows a vertical rail on the left side with icons and labels
class AppNavigationRail extends StatelessWidget {
  const AppNavigationRail({
    super.key,
    required this.selectedIndex,
    required this.onDestinationSelected,
    this.extended = false,
    this.destinations = appNavDestinations,
  });

  final int selectedIndex;
  final ValueChanged<int> onDestinationSelected;
  final bool extended;
  final List<NavDestination> destinations;

  @override
  Widget build(BuildContext context) {
    return NavigationRail(
      selectedIndex: selectedIndex,
      onDestinationSelected: onDestinationSelected,
      extended: extended,
      minWidth: 72,
      minExtendedWidth: 200,
      backgroundColor: Colors.white,
      elevation: 4,
      indicatorColor: AppColors.primary.withValues(alpha: 0.1),
      selectedIconTheme: IconThemeData(
        color: AppColors.primary,
        size: 24,
      ),
      unselectedIconTheme: IconThemeData(
        color: Colors.grey[600],
        size: 24,
      ),
      selectedLabelTextStyle: TextStyle(
        color: AppColors.primary,
        fontWeight: FontWeight.w600,
        fontSize: 13,
      ),
      unselectedLabelTextStyle: TextStyle(
        color: Colors.grey[600],
        fontSize: 13,
      ),
      useIndicator: true,
      labelType: extended
          ? NavigationRailLabelType.none
          : NavigationRailLabelType.selected,
      leading: extended
          ? Padding(
              padding: const EdgeInsets.symmetric(vertical: 16),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Image.asset(
                    'assets/logos/logo.png',
                    width: 32,
                    height: 32,
                  ),
                  const SizedBox(width: 8),
                  Text(
                    'CarDealer',
                    style: TextStyle(
                      color: AppColors.primary,
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            )
          : Padding(
              padding: const EdgeInsets.symmetric(vertical: 16),
              child: Image.asset(
                'assets/logos/logo.png',
                width: 32,
                height: 32,
              ),
            ),
      destinations: destinations.map((dest) {
        return NavigationRailDestination(
          icon: Icon(dest.icon),
          selectedIcon: Icon(dest.selectedIcon),
          label: Text(dest.label),
          padding: const EdgeInsets.symmetric(vertical: 8),
        );
      }).toList(),
    );
  }
}

/// NavigationDrawer for desktop/large tablets (>= 1024dp)
/// Shows a permanent drawer on the left side
class AppNavigationDrawer extends StatelessWidget {
  const AppNavigationDrawer({
    super.key,
    required this.selectedIndex,
    required this.onDestinationSelected,
    this.destinations = appNavDestinations,
  });

  final int selectedIndex;
  final ValueChanged<int> onDestinationSelected;
  final List<NavDestination> destinations;

  @override
  Widget build(BuildContext context) {
    return NavigationDrawer(
      selectedIndex: selectedIndex,
      onDestinationSelected: onDestinationSelected,
      backgroundColor: Colors.white,
      elevation: 4,
      indicatorColor: AppColors.primary.withValues(alpha: 0.1),
      children: [
        // Header
        Padding(
          padding: const EdgeInsets.fromLTRB(24, 24, 24, 16),
          child: Row(
            children: [
              Image.asset(
                'assets/logos/logo.png',
                width: 36,
                height: 36,
              ),
              const SizedBox(width: 12),
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'CarDealer',
                    style: TextStyle(
                      color: AppColors.primary,
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Text(
                    'Tu próximo auto te espera',
                    style: TextStyle(
                      color: Colors.grey[600],
                      fontSize: 12,
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
        const Divider(indent: 24, endIndent: 24),
        const SizedBox(height: 8),
        // Destinations
        ...destinations.map((dest) {
          return NavigationDrawerDestination(
            icon: Icon(dest.icon),
            selectedIcon: Icon(dest.selectedIcon),
            label: Text(dest.label),
          );
        }),
        const Spacer(),
        // Footer
        const Divider(indent: 24, endIndent: 24),
        Padding(
          padding: const EdgeInsets.all(24),
          child: _QuickActionsSection(),
        ),
      ],
    );
  }
}

/// Quick actions section for the drawer footer
class _QuickActionsSection extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Acciones rápidas',
          style: TextStyle(
            color: Colors.grey[600],
            fontSize: 12,
            fontWeight: FontWeight.w500,
          ),
        ),
        const SizedBox(height: 12),
        _QuickActionButton(
          icon: Icons.add_circle_outline,
          label: 'Publicar vehículo',
          onTap: () {
            // Navigate to publish vehicle
          },
        ),
        const SizedBox(height: 8),
        _QuickActionButton(
          icon: Icons.search,
          label: 'Buscar vehículos',
          onTap: () {
            // Navigate to search
          },
        ),
      ],
    );
  }
}

class _QuickActionButton extends StatelessWidget {
  const _QuickActionButton({
    required this.icon,
    required this.label,
    required this.onTap,
  });

  final IconData icon;
  final String label;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(8),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
        child: Row(
          children: [
            Icon(icon, size: 20, color: Colors.grey[700]),
            const SizedBox(width: 12),
            Text(
              label,
              style: TextStyle(
                color: Colors.grey[700],
                fontSize: 14,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Bottom navigation bar for mobile (< 600dp)
class AppBottomNavBar extends StatelessWidget {
  const AppBottomNavBar({
    super.key,
    required this.selectedIndex,
    required this.onDestinationSelected,
    this.destinations = appNavDestinations,
  });

  final int selectedIndex;
  final ValueChanged<int> onDestinationSelected;
  final List<NavDestination> destinations;

  @override
  Widget build(BuildContext context) {
    return NavigationBar(
      selectedIndex: selectedIndex,
      onDestinationSelected: onDestinationSelected,
      elevation: 8,
      height: 70,
      backgroundColor: Colors.white,
      indicatorColor: AppColors.primary.withValues(alpha: 0.1),
      destinations: destinations.map((dest) {
        return NavigationDestination(
          icon: Icon(dest.icon),
          selectedIcon: Icon(dest.selectedIcon),
          label: dest.label,
        );
      }).toList(),
    );
  }
}
