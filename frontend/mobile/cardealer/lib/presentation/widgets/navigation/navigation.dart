/// Navigation widgets for CarDealer Mobile
/// 
/// Provides adaptive navigation that adjusts based on screen size:
/// - Mobile (< 600dp): Bottom navigation bar
/// - Tablet (600dp - 1023dp): Navigation rail
/// - Desktop (>= 1024dp): Extended navigation rail
/// 
/// Usage:
/// ```dart
/// import 'package:cardealer/presentation/widgets/navigation/navigation.dart';
/// 
/// AdaptiveScaffold(
///   selectedIndex: _selectedIndex,
///   onDestinationSelected: (index) => setState(() => _selectedIndex = index),
///   pages: [HomePage(), FavoritesPage(), MessagesPage(), ProfilePage()],
/// )
/// ```
library;

export 'adaptive_navigation.dart';
export 'nav_rail.dart';
