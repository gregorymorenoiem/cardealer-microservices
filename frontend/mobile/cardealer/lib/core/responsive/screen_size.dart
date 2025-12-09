/// Screen size breakpoints for responsive design
enum ScreenSize {
  /// Extra small mobile devices (320dp - 359dp)
  /// Examples: iPhone SE, older Android devices
  mobileSmall,

  /// Standard mobile devices (360dp - 427dp)
  /// Examples: iPhone 12/13/14, most Android phones
  mobile,

  /// Large mobile devices (428dp - 599dp)
  /// Examples: iPhone Pro Max, large Android phones
  mobileLarge,

  /// Small tablets and large phablets (600dp - 767dp)
  /// Examples: iPad Mini, small tablets in portrait
  tabletSmall,

  /// Standard tablets (768dp - 1023dp)
  /// Examples: iPad, Android tablets in portrait
  tablet,

  /// Large tablets and desktops (1024dp+)
  /// Examples: iPad Pro, tablets in landscape, desktops
  tabletLarge,
}

extension ScreenSizeExtension on ScreenSize {
  /// Minimum width for this screen size
  double get minWidth {
    switch (this) {
      case ScreenSize.mobileSmall:
        return 320;
      case ScreenSize.mobile:
        return 360;
      case ScreenSize.mobileLarge:
        return 428;
      case ScreenSize.tabletSmall:
        return 600;
      case ScreenSize.tablet:
        return 768;
      case ScreenSize.tabletLarge:
        return 1024;
    }
  }

  /// Maximum width for this screen size (null for largest)
  double? get maxWidth {
    switch (this) {
      case ScreenSize.mobileSmall:
        return 359;
      case ScreenSize.mobile:
        return 427;
      case ScreenSize.mobileLarge:
        return 599;
      case ScreenSize.tabletSmall:
        return 767;
      case ScreenSize.tablet:
        return 1023;
      case ScreenSize.tabletLarge:
        return null;
    }
  }

  /// Human-readable name
  String get displayName {
    switch (this) {
      case ScreenSize.mobileSmall:
        return 'Small Mobile';
      case ScreenSize.mobile:
        return 'Mobile';
      case ScreenSize.mobileLarge:
        return 'Large Mobile';
      case ScreenSize.tabletSmall:
        return 'Small Tablet';
      case ScreenSize.tablet:
        return 'Tablet';
      case ScreenSize.tabletLarge:
        return 'Large Tablet';
    }
  }

  /// Whether this is a mobile device
  bool get isMobile =>
      this == ScreenSize.mobileSmall ||
      this == ScreenSize.mobile ||
      this == ScreenSize.mobileLarge;

  /// Whether this is a tablet device
  bool get isTablet =>
      this == ScreenSize.tabletSmall ||
      this == ScreenSize.tablet ||
      this == ScreenSize.tabletLarge;
}
