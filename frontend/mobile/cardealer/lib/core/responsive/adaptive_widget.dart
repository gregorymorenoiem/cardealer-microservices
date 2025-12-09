import 'package:flutter/material.dart';
import 'breakpoints.dart';

/// A widget that displays different content based on device type
/// Useful for simple conditional rendering without callbacks
class AdaptiveWidget extends StatelessWidget {
  const AdaptiveWidget({
    super.key,
    required this.mobile,
    this.tablet,
    this.desktop,
  });

  /// Widget to show on mobile devices (< 600dp)
  final Widget mobile;

  /// Widget to show on tablets (600-1023dp)
  /// Falls back to [mobile] if not provided
  final Widget? tablet;

  /// Widget to show on desktop (>= 1024dp)
  /// Falls back to [tablet] then [mobile] if not provided
  final Widget? desktop;

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;

    if (width >= Breakpoints.xxl) {
      return desktop ?? tablet ?? mobile;
    }

    if (width >= Breakpoints.lg) {
      return tablet ?? mobile;
    }

    return mobile;
  }
}

/// A widget that adapts its padding based on screen size
class AdaptivePadding extends StatelessWidget {
  const AdaptivePadding({
    super.key,
    required this.child,
    this.mobilePadding,
    this.tabletPadding,
    this.desktopPadding,
  });

  final Widget child;

  /// Padding for mobile (default: EdgeInsets.all(16))
  final EdgeInsets? mobilePadding;

  /// Padding for tablet (default: EdgeInsets.all(24))
  final EdgeInsets? tabletPadding;

  /// Padding for desktop (default: EdgeInsets.all(32))
  final EdgeInsets? desktopPadding;

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;

    EdgeInsets padding;
    if (width >= Breakpoints.xxl) {
      padding = desktopPadding ?? const EdgeInsets.all(32);
    } else if (width >= Breakpoints.lg) {
      padding = tabletPadding ?? const EdgeInsets.all(24);
    } else {
      padding = mobilePadding ?? const EdgeInsets.all(16);
    }

    return Padding(padding: padding, child: child);
  }
}

/// A container that centers content and limits max width on large screens
class AdaptiveContainer extends StatelessWidget {
  const AdaptiveContainer({
    super.key,
    required this.child,
    this.maxWidth,
    this.padding,
    this.alignment = Alignment.center,
    this.decoration,
    this.color,
  });

  final Widget child;

  /// Maximum width for the content (default: 1200dp)
  final double? maxWidth;

  /// Padding around the content
  final EdgeInsets? padding;

  /// Alignment of the content
  final Alignment alignment;

  /// Optional decoration
  final BoxDecoration? decoration;

  /// Optional background color
  final Color? color;

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;
    final effectiveMaxWidth = maxWidth ?? Breakpoints.maxContentWidth;

    Widget content = child;

    if (padding != null) {
      content = Padding(padding: padding!, child: content);
    }

    // Only constrain width on larger screens
    if (width > effectiveMaxWidth) {
      content = Center(
        child: ConstrainedBox(
          constraints: BoxConstraints(maxWidth: effectiveMaxWidth),
          child: content,
        ),
      );
    }

    if (decoration != null || color != null) {
      return Container(
        decoration: decoration,
        color: decoration == null ? color : null,
        alignment: alignment,
        child: content,
      );
    }

    return content;
  }
}

/// A row/column that adapts based on screen size
/// Switches from Column on mobile to Row on tablet/desktop
class AdaptiveRowColumn extends StatelessWidget {
  const AdaptiveRowColumn({
    super.key,
    required this.children,
    this.mainAxisAlignment = MainAxisAlignment.start,
    this.crossAxisAlignment = CrossAxisAlignment.center,
    this.mainAxisSize = MainAxisSize.max,
    this.rowOnMobile = false,
    this.columnOnTablet = false,
    this.spacing = 16,
  });

  final List<Widget> children;
  final MainAxisAlignment mainAxisAlignment;
  final CrossAxisAlignment crossAxisAlignment;
  final MainAxisSize mainAxisSize;

  /// If true, shows Row on mobile too
  final bool rowOnMobile;

  /// If true, shows Column on tablet (Row only on desktop)
  final bool columnOnTablet;

  /// Spacing between children
  final double spacing;

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;
    final isDesktop = width >= Breakpoints.xxl;
    final isTablet = width >= Breakpoints.lg && !isDesktop;

    bool useRow;
    if (isDesktop) {
      useRow = true;
    } else if (isTablet) {
      useRow = !columnOnTablet;
    } else {
      useRow = rowOnMobile;
    }

    // Add spacing between children
    final spacedChildren = <Widget>[];
    for (int i = 0; i < children.length; i++) {
      spacedChildren.add(children[i]);
      if (i < children.length - 1) {
        spacedChildren.add(SizedBox(
          width: useRow ? spacing : 0,
          height: useRow ? 0 : spacing,
        ));
      }
    }

    if (useRow) {
      return Row(
        mainAxisAlignment: mainAxisAlignment,
        crossAxisAlignment: crossAxisAlignment,
        mainAxisSize: mainAxisSize,
        children: spacedChildren,
      );
    }

    return Column(
      mainAxisAlignment: mainAxisAlignment,
      crossAxisAlignment: crossAxisAlignment,
      mainAxisSize: mainAxisSize,
      children: spacedChildren,
    );
  }
}

/// Adaptive spacing widget
class AdaptiveSpacing extends StatelessWidget {
  const AdaptiveSpacing({
    super.key,
    this.mobileSize = 16,
    this.tabletSize = 24,
    this.desktopSize = 32,
    this.axis = Axis.vertical,
  });

  final double mobileSize;
  final double tabletSize;
  final double desktopSize;
  final Axis axis;

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;

    double size;
    if (width >= Breakpoints.xxl) {
      size = desktopSize;
    } else if (width >= Breakpoints.lg) {
      size = tabletSize;
    } else {
      size = mobileSize;
    }

    if (axis == Axis.vertical) {
      return SizedBox(height: size);
    }
    return SizedBox(width: size);
  }
}

/// Extension for easy value selection based on screen size
extension AdaptiveContext on BuildContext {
  /// Get device category
  DeviceCategory get deviceCategory {
    return MediaQuery.of(this).size.width.deviceCategory;
  }

  /// Check if current device is mobile
  bool get isMobile => deviceCategory == DeviceCategory.mobile;

  /// Check if current device is tablet
  bool get isTablet => deviceCategory == DeviceCategory.tablet;

  /// Check if current device is desktop
  bool get isDesktop => deviceCategory == DeviceCategory.desktop;

  /// Check if should use navigation rail
  bool get shouldUseNavRail => MediaQuery.of(this).size.width.shouldUseNavRail;

  /// Check if should use navigation drawer
  bool get shouldUseNavDrawer =>
      MediaQuery.of(this).size.width.shouldUseNavDrawer;

  /// Get a value based on device category
  T adaptive<T>({
    required T mobile,
    T? tablet,
    T? desktop,
  }) {
    switch (deviceCategory) {
      case DeviceCategory.mobile:
        return mobile;
      case DeviceCategory.tablet:
        return tablet ?? mobile;
      case DeviceCategory.desktop:
        return desktop ?? tablet ?? mobile;
    }
  }

  /// Get detailed screen size category
  ScreenSizeCategory get screenSizeCategory {
    return MediaQuery.of(this).size.width.screenSizeCategory;
  }
}
