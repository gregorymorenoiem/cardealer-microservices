/// Breakpoint constants for responsive design
/// Based on Material Design 3 guidelines and common device sizes
library;

/// Centralized breakpoint values in logical pixels (dp)
abstract class Breakpoints {
  /// Extra small mobile (iPhone SE, older Android devices)
  static const double xs = 320;

  /// Small mobile threshold (standard smartphones)
  static const double sm = 360;

  /// Medium mobile threshold (large smartphones, iPhone Pro Max)
  static const double md = 428;

  /// Large screen threshold (small tablets, iPad Mini)
  static const double lg = 600;

  /// Extra large threshold (tablets, iPad)
  static const double xl = 768;

  /// Extra extra large threshold (large tablets, iPad Pro, desktop)
  static const double xxl = 1024;

  /// Maximum content width for very large screens
  static const double maxContentWidth = 1440;

  // === Named breakpoints for clarity ===

  /// Minimum mobile small width
  static const double mobileSmallMin = xs; // 320

  /// Maximum mobile small width
  static const double mobileSmallMax = sm - 1; // 359

  /// Minimum standard mobile width
  static const double mobileMin = sm; // 360

  /// Maximum standard mobile width
  static const double mobileMax = md - 1; // 427

  /// Minimum large mobile width
  static const double mobileLargeMin = md; // 428

  /// Maximum large mobile width
  static const double mobileLargeMax = lg - 1; // 599

  /// Minimum small tablet width
  static const double tabletSmallMin = lg; // 600

  /// Maximum small tablet width
  static const double tabletSmallMax = xl - 1; // 767

  /// Minimum tablet width
  static const double tabletMin = xl; // 768

  /// Maximum tablet width
  static const double tabletMax = xxl - 1; // 1023

  /// Minimum large tablet / desktop width
  static const double tabletLargeMin = xxl; // 1024

  // === Convenience thresholds ===

  /// Threshold where we switch from bottom nav to rail
  static const double navigationRailThreshold = lg; // 600

  /// Threshold where we show sidebar/drawer navigation
  static const double navigationDrawerThreshold = xxl; // 1024

  /// Threshold for 2-column layouts
  static const double twoColumnThreshold = lg; // 600

  /// Threshold for 3-column layouts
  static const double threeColumnThreshold = xl; // 768

  /// Threshold for 4-column layouts
  static const double fourColumnThreshold = xxl; // 1024
}

/// Device category based on screen width
enum DeviceCategory {
  /// Mobile phones (< 600dp)
  mobile,

  /// Tablets (600dp - 1023dp)
  tablet,

  /// Desktop/Large tablets (>= 1024dp)
  desktop,
}

/// Detailed screen size categories
enum ScreenSizeCategory {
  /// 320-359dp: iPhone SE, small Android
  mobileSmall,

  /// 360-427dp: iPhone 12/13/14, standard Android
  mobile,

  /// 428-599dp: iPhone Pro Max, large Android
  mobileLarge,

  /// 600-767dp: iPad Mini, small tablets
  tabletSmall,

  /// 768-1023dp: iPad, Android tablets
  tablet,

  /// 1024dp+: iPad Pro, large tablets, desktop
  tabletLarge,
}

/// Extension to get category from width
extension BreakpointExtensions on double {
  /// Get device category from width
  DeviceCategory get deviceCategory {
    if (this < Breakpoints.lg) return DeviceCategory.mobile;
    if (this < Breakpoints.xxl) return DeviceCategory.tablet;
    return DeviceCategory.desktop;
  }

  /// Get detailed screen size category from width
  ScreenSizeCategory get screenSizeCategory {
    if (this < Breakpoints.sm) return ScreenSizeCategory.mobileSmall;
    if (this < Breakpoints.md) return ScreenSizeCategory.mobile;
    if (this < Breakpoints.lg) return ScreenSizeCategory.mobileLarge;
    if (this < Breakpoints.xl) return ScreenSizeCategory.tabletSmall;
    if (this < Breakpoints.xxl) return ScreenSizeCategory.tablet;
    return ScreenSizeCategory.tabletLarge;
  }

  /// Check if width is mobile
  bool get isMobileWidth => this < Breakpoints.lg;

  /// Check if width is tablet
  bool get isTabletWidth => this >= Breakpoints.lg && this < Breakpoints.xxl;

  /// Check if width is desktop/large tablet
  bool get isDesktopWidth => this >= Breakpoints.xxl;

  /// Check if should use navigation rail
  bool get shouldUseNavRail =>
      this >= Breakpoints.navigationRailThreshold &&
      this < Breakpoints.navigationDrawerThreshold;

  /// Check if should use navigation drawer
  bool get shouldUseNavDrawer => this >= Breakpoints.navigationDrawerThreshold;
}
