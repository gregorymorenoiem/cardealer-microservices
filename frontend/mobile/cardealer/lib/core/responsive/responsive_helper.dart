import 'package:flutter/material.dart';
import 'screen_size.dart';
import 'breakpoints.dart';

/// Helper class for responsive design calculations
class ResponsiveHelper {
  final BuildContext context;

  ResponsiveHelper(this.context);

  /// Current screen width in logical pixels
  double get screenWidth => MediaQuery.of(context).size.width;

  /// Current screen height in logical pixels
  double get screenHeight => MediaQuery.of(context).size.height;

  // === Device Type Checks ===

  /// Check if current device is mobile (< 600dp)
  bool get isMobile => screenWidth < Breakpoints.lg;

  /// Check if current device is tablet (600-1023dp)
  bool get isTablet =>
      screenWidth >= Breakpoints.lg && screenWidth < Breakpoints.xxl;

  /// Check if current device is desktop/large tablet (>= 1024dp)
  bool get isDesktop => screenWidth >= Breakpoints.xxl;

  /// Check if should use NavigationRail (600-1023dp)
  bool get shouldUseNavRail =>
      screenWidth >= Breakpoints.navigationRailThreshold &&
      screenWidth < Breakpoints.navigationDrawerThreshold;

  /// Check if should use NavigationDrawer (>= 1024dp)
  bool get shouldUseNavDrawer =>
      screenWidth >= Breakpoints.navigationDrawerThreshold;

  // === Orientation Checks ===

  /// Check if device is in portrait orientation
  bool get isPortrait =>
      MediaQuery.of(context).orientation == Orientation.portrait;

  /// Check if device is in landscape orientation
  bool get isLandscape =>
      MediaQuery.of(context).orientation == Orientation.landscape;

  // === Grid Columns ===

  /// Get recommended number of columns for grids
  int get gridColumns {
    if (screenWidth >= Breakpoints.xxl) return 4;
    if (screenWidth >= Breakpoints.xl) return 3;
    if (screenWidth >= Breakpoints.lg) return 2;
    return 1;
  }

  /// Get recommended number of columns for card grids
  int get cardGridColumns {
    if (screenWidth >= Breakpoints.xxl) return 4;
    if (screenWidth >= Breakpoints.xl) return 3;
    if (screenWidth >= Breakpoints.lg) return 2;
    if (screenWidth >= Breakpoints.md) return 2;
    return 1;
  }

  /// Current screen size category
  ScreenSize get screenType {
    final width = screenWidth;
    if (width < 360) return ScreenSize.mobileSmall;
    if (width < 428) return ScreenSize.mobile;
    if (width < 600) return ScreenSize.mobileLarge;
    if (width < 768) return ScreenSize.tabletSmall;
    if (width < 1024) return ScreenSize.tablet;
    return ScreenSize.tabletLarge;
  }

  /// Responsive card width for vehicle cards
  double get cardWidth {
    switch (screenType) {
      case ScreenSize.mobileSmall:
        return 260; // Compact for small screens
      case ScreenSize.mobile:
        return 280; // Standard mobile
      case ScreenSize.mobileLarge:
        return 300; // Slightly larger
      case ScreenSize.tabletSmall:
        return 320; // More spacious on tablets
      case ScreenSize.tablet:
        return 350; // Even more space
      case ScreenSize.tabletLarge:
        return 380; // Maximum size for large screens
    }
  }

  /// Responsive card height for vehicle cards
  double get cardHeight {
    switch (screenType) {
      case ScreenSize.mobileSmall:
        return 160; // Compact height for small screens
      case ScreenSize.mobile:
        return 180; // Standard mobile height
      case ScreenSize.mobileLarge:
        return 200; // More vertical space
      case ScreenSize.tabletSmall:
        return 220; // Tablet optimized
      case ScreenSize.tablet:
        return 240; // More content space
      case ScreenSize.tabletLarge:
        return 260; // Maximum height for large screens
    }
  }

  /// Image height for vehicle cards (70% of card height)
  double get cardImageHeight => cardHeight * 0.7;

  /// Info section height for vehicle cards (30% of card height)
  double get cardInfoHeight => cardHeight * 0.3;

  /// Responsive horizontal spacing
  double get horizontalPadding {
    switch (screenType) {
      case ScreenSize.mobileSmall:
        return 12;
      case ScreenSize.mobile:
        return 16;
      case ScreenSize.mobileLarge:
        return 16;
      case ScreenSize.tabletSmall:
        return 20;
      case ScreenSize.tablet:
        return 24;
      case ScreenSize.tabletLarge:
        return 32;
    }
  }

  /// Responsive spacing between cards
  double get cardSpacing {
    switch (screenType) {
      case ScreenSize.mobileSmall:
        return 8;
      case ScreenSize.mobile:
        return 12;
      case ScreenSize.mobileLarge:
        return 12;
      case ScreenSize.tabletSmall:
        return 16;
      case ScreenSize.tablet:
        return 16;
      case ScreenSize.tabletLarge:
        return 20;
    }
  }

  /// Responsive font size for titles
  double get titleFontSize {
    switch (screenType) {
      case ScreenSize.mobileSmall:
        return 13;
      case ScreenSize.mobile:
        return 14;
      case ScreenSize.mobileLarge:
        return 15;
      case ScreenSize.tabletSmall:
        return 16;
      case ScreenSize.tablet:
        return 17;
      case ScreenSize.tabletLarge:
        return 18;
    }
  }

  /// Responsive font size for body text
  double get bodyFontSize {
    switch (screenType) {
      case ScreenSize.mobileSmall:
        return 11;
      case ScreenSize.mobile:
        return 12;
      case ScreenSize.mobileLarge:
        return 13;
      case ScreenSize.tabletSmall:
        return 13;
      case ScreenSize.tablet:
        return 14;
      case ScreenSize.tabletLarge:
        return 15;
    }
  }

  /// Responsive font size for small text
  double get smallFontSize {
    switch (screenType) {
      case ScreenSize.mobileSmall:
        return 10;
      case ScreenSize.mobile:
        return 11;
      case ScreenSize.mobileLarge:
        return 12;
      case ScreenSize.tabletSmall:
        return 12;
      case ScreenSize.tablet:
        return 13;
      case ScreenSize.tabletLarge:
        return 14;
    }
  }

  /// Responsive icon size
  double get iconSize {
    switch (screenType) {
      case ScreenSize.mobileSmall:
        return 14;
      case ScreenSize.mobile:
        return 16;
      case ScreenSize.mobileLarge:
        return 16;
      case ScreenSize.tabletSmall:
        return 18;
      case ScreenSize.tablet:
        return 20;
      case ScreenSize.tabletLarge:
        return 22;
    }
  }

  /// Responsive border radius
  double get borderRadius {
    switch (screenType) {
      case ScreenSize.mobileSmall:
        return 8;
      case ScreenSize.mobile:
        return 12;
      case ScreenSize.mobileLarge:
        return 12;
      case ScreenSize.tabletSmall:
        return 16;
      case ScreenSize.tablet:
        return 16;
      case ScreenSize.tabletLarge:
        return 20;
    }
  }

  /// Get a responsive value based on screen size
  T responsiveValue<T>({
    required T mobileSmall,
    required T mobile,
    required T mobileLarge,
    required T tabletSmall,
    required T tablet,
    required T tabletLarge,
  }) {
    switch (screenType) {
      case ScreenSize.mobileSmall:
        return mobileSmall;
      case ScreenSize.mobile:
        return mobile;
      case ScreenSize.mobileLarge:
        return mobileLarge;
      case ScreenSize.tabletSmall:
        return tabletSmall;
      case ScreenSize.tablet:
        return tablet;
      case ScreenSize.tabletLarge:
        return tabletLarge;
    }
  }

  /// Get a simplified responsive value (mobile/tablet/desktop)
  T adaptive<T>({
    required T mobile,
    T? tablet,
    T? desktop,
  }) {
    if (isDesktop) return desktop ?? tablet ?? mobile;
    if (isTablet) return tablet ?? mobile;
    return mobile;
  }

  /// Get section height for horizontal scrolling sections
  double get sectionHeight {
    switch (screenType) {
      case ScreenSize.mobileSmall:
        return 200;
      case ScreenSize.mobile:
        return 220;
      case ScreenSize.mobileLarge:
        return 240;
      case ScreenSize.tabletSmall:
        return 280;
      case ScreenSize.tablet:
        return 300;
      case ScreenSize.tabletLarge:
        return 320;
    }
  }

  /// Get AppBar height
  double get appBarHeight {
    if (isMobile) return kToolbarHeight;
    if (isTablet) return kToolbarHeight + 8;
    return kToolbarHeight + 16;
  }

  /// Get navigation rail width
  double get navRailWidth {
    if (isDesktop) return 256; // Extended rail
    return 72; // Compact rail
  }

  /// Get bottom navigation height
  double get bottomNavHeight {
    switch (screenType) {
      case ScreenSize.mobileSmall:
        return 60;
      case ScreenSize.mobile:
        return 70;
      case ScreenSize.mobileLarge:
        return 70;
      default:
        return 0; // No bottom nav on tablet+
    }
  }
}

/// Extension to easily access ResponsiveHelper from BuildContext
extension ResponsiveContext on BuildContext {
  ResponsiveHelper get responsive => ResponsiveHelper(this);

  /// Quick check if device is mobile
  bool get isMobileDevice => responsive.isMobile;

  /// Quick check if device is tablet
  bool get isTabletDevice => responsive.isTablet;

  /// Quick check if device is desktop
  bool get isDesktopDevice => responsive.isDesktop;

  /// Get simplified adaptive value
  T adaptive<T>({
    required T mobile,
    T? tablet,
    T? desktop,
  }) =>
      responsive.adaptive(mobile: mobile, tablet: tablet, desktop: desktop);
}
