/// App spacing constants
/// Following 8px grid system
class AppSpacing {
  // Private constructor
  AppSpacing._();

  // Base unit (8px)
  static const double unit = 8.0;

  // Spacing scale (8pt grid system)
  static const double xxs = unit * 0.25; // 2px - for very tight spacing
  static const double xs = unit * 0.5; // 4px
  static const double sm = unit; // 8px
  static const double md = unit * 2; // 16px
  static const double lg = unit * 3; // 24px
  static const double xl = unit * 4; // 32px
  static const double xxl = unit * 6; // 48px
  static const double xxxl = unit * 8; // 64px

  // Padding
  static const double paddingXs = xs;
  static const double paddingSm = sm;
  static const double paddingMd = md;
  static const double paddingLg = lg;
  static const double paddingXl = xl;

  // Margin
  static const double marginXs = xs;
  static const double marginSm = sm;
  static const double marginMd = md;
  static const double marginLg = lg;
  static const double marginXl = xl;

  // Gap (for Flex layouts)
  static const double gapXs = xs;
  static const double gapSm = sm;
  static const double gapMd = md;
  static const double gapLg = lg;
  static const double gapXl = xl;

  // Border radius
  static const double radiusXs = 4.0;
  static const double radiusSm = 6.0;
  static const double radiusMd = 8.0;
  static const double radiusLg = 12.0;
  static const double radiusXl = 16.0;
  static const double radiusXxl = 24.0;
  static const double radiusFull = 9999.0;

  // Icon sizes
  static const double iconXs = 16.0;
  static const double iconSm = 20.0;
  static const double iconMd = 24.0;
  static const double iconLg = 32.0;
  static const double iconXl = 48.0;

  // Avatar sizes
  static const double avatarSm = 32.0;
  static const double avatarMd = 40.0;
  static const double avatarLg = 56.0;
  static const double avatarXl = 80.0;

  // Button heights
  static const double buttonHeightSm = 32.0;
  static const double buttonHeightMd = 40.0;
  static const double buttonHeightLg = 48.0;

  // Input heights
  static const double inputHeightSm = 32.0;
  static const double inputHeightMd = 40.0;
  static const double inputHeightLg = 48.0;

  // Card
  static const double cardPadding = md;
  static const double cardRadius = radiusLg;

  // Container widths
  static const double containerMaxWidth = 1280.0;
  static const double containerPadding = md;

  // AppBar
  static const double appBarHeight = 56.0;

  // Bottom navigation bar
  static const double bottomNavHeight = 60.0;

  // Tab bar
  static const double tabBarHeight = 48.0;

  // List item
  static const double listItemHeight = 72.0;
  static const double listItemPadding = md;

  // Divider
  static const double dividerThickness = 1.0;

  // Border
  static const double borderWidthThin = 1.0;
  static const double borderWidthMedium = 2.0;
  static const double borderWidthThick = 3.0;
}
