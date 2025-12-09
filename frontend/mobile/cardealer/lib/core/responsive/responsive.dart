/// Responsive design utilities for CarDealer Mobile
///
/// This library provides a comprehensive responsive design system including:
/// - [Breakpoints]: Centralized breakpoint constants
/// - [ResponsiveHelper]: Main helper class for responsive values
/// - [ResponsiveLayoutBuilder]: Widget for building responsive layouts
/// - [AdaptiveWidget]: Widgets that adapt to screen size
/// - [ScreenSize]: Enum for screen size categories
///
/// Usage:
/// ```dart
/// import 'package:cardealer/core/responsive/responsive.dart';
///
/// // Using ResponsiveHelper
/// final responsive = context.responsive;
/// final width = responsive.cardWidth;
///
/// // Using adaptive widget
/// AdaptiveWidget(
///   mobile: MobileLayout(),
///   tablet: TabletLayout(),
///   desktop: DesktopLayout(),
/// )
///
/// // Using context extensions
/// final isMobile = context.isMobileDevice;
/// final columns = context.adaptive(mobile: 1, tablet: 2, desktop: 3);
/// ```
library;

export 'breakpoints.dart';
export 'screen_size.dart';
export 'responsive_helper.dart';
export 'responsive_layout_builder.dart';
export 'adaptive_widget.dart';
// responsive_utils.dart is legacy - use breakpoints.dart and responsive_helper.dart instead
// export 'responsive_utils.dart' hide Breakpoints, DeviceType, ResponsiveContext;
export 'responsive_padding.dart';
