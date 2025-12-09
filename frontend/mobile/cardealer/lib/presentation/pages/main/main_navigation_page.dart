import 'package:flutter/material.dart';
import '../home/home_page.dart';
import '../favorites/favorites_page_premium.dart';
import '../messages/conversations_list_page.dart';
import '../profile/profile_page.dart';
import '../../widgets/navigation/adaptive_navigation.dart';

/// MainNavigationPage - Adaptive navigation container
/// Uses BottomNav on mobile, NavigationRail on tablet, extended rail on desktop
class MainNavigationPage extends StatefulWidget {
  const MainNavigationPage({super.key});

  @override
  State<MainNavigationPage> createState() => _MainNavigationPageState();
}

class _MainNavigationPageState extends State<MainNavigationPage> {
  int _selectedIndex = 0;

  static final List<Widget> _pages = <Widget>[
    const HomePage(),
    const FavoritesPagePremium(),
    const ConversationsListPage(),
    const ProfilePage(),
  ];

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });
  }

  @override
  Widget build(BuildContext context) {
    return AdaptiveScaffold(
      selectedIndex: _selectedIndex,
      onDestinationSelected: _onItemTapped,
      pages: _pages,
    );
  }
}
