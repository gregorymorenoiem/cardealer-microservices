import 'package:flutter/material.dart';
import 'breakpoints.dart';

/// A widget that builds different layouts based on screen size
/// Similar to LayoutBuilder but with pre-defined breakpoint callbacks
class ResponsiveLayoutBuilder extends StatelessWidget {
  const ResponsiveLayoutBuilder({
    super.key,
    required this.mobile,
    this.mobileLarge,
    this.tablet,
    this.desktop,
    this.mobileSmall,
    this.tabletSmall,
  });

  /// Required: Layout for mobile devices (default fallback)
  final Widget Function(BuildContext context, BoxConstraints constraints)
      mobile;

  /// Optional: Layout for extra small mobile (< 360dp)
  final Widget Function(BuildContext context, BoxConstraints constraints)?
      mobileSmall;

  /// Optional: Layout for large mobile (428-599dp)
  final Widget Function(BuildContext context, BoxConstraints constraints)?
      mobileLarge;

  /// Optional: Layout for small tablets (600-767dp)
  final Widget Function(BuildContext context, BoxConstraints constraints)?
      tabletSmall;

  /// Optional: Layout for tablets (768-1023dp)
  final Widget Function(BuildContext context, BoxConstraints constraints)?
      tablet;

  /// Optional: Layout for desktop/large tablets (>= 1024dp)
  final Widget Function(BuildContext context, BoxConstraints constraints)?
      desktop;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final width = MediaQuery.of(context).size.width;

        // Desktop (>= 1024dp)
        if (width >= Breakpoints.xxl) {
          return (desktop ?? tablet ?? mobile)(context, constraints);
        }

        // Tablet (768-1023dp)
        if (width >= Breakpoints.xl) {
          return (tablet ?? desktop ?? mobile)(context, constraints);
        }

        // Small Tablet (600-767dp)
        if (width >= Breakpoints.lg) {
          return (tabletSmall ?? tablet ?? mobile)(context, constraints);
        }

        // Large Mobile (428-599dp)
        if (width >= Breakpoints.md) {
          return (mobileLarge ?? mobile)(context, constraints);
        }

        // Small Mobile (320-359dp)
        if (width < Breakpoints.sm) {
          return (mobileSmall ?? mobile)(context, constraints);
        }

        // Standard Mobile (360-427dp)
        return mobile(context, constraints);
      },
    );
  }
}

/// Simplified responsive builder with just mobile/tablet/desktop
class SimpleResponsiveBuilder extends StatelessWidget {
  const SimpleResponsiveBuilder({
    super.key,
    required this.mobileBuilder,
    this.tabletBuilder,
    this.desktopBuilder,
  });

  /// Layout for mobile devices (< 600dp)
  final Widget Function(BuildContext context) mobileBuilder;

  /// Layout for tablets (600-1023dp)
  final Widget Function(BuildContext context)? tabletBuilder;

  /// Layout for desktop (>= 1024dp)
  final Widget Function(BuildContext context)? desktopBuilder;

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;

    if (width >= Breakpoints.xxl) {
      return (desktopBuilder ?? tabletBuilder ?? mobileBuilder)(context);
    }

    if (width >= Breakpoints.lg) {
      return (tabletBuilder ?? mobileBuilder)(context);
    }

    return mobileBuilder(context);
  }
}

/// A widget that shows/hides content based on screen size
class ResponsiveVisibility extends StatelessWidget {
  const ResponsiveVisibility({
    super.key,
    required this.child,
    this.visibleOnMobile = true,
    this.visibleOnTablet = true,
    this.visibleOnDesktop = true,
    this.replacement,
  });

  /// The child widget to show/hide
  final Widget child;

  /// Whether to show on mobile (< 600dp)
  final bool visibleOnMobile;

  /// Whether to show on tablet (600-1023dp)
  final bool visibleOnTablet;

  /// Whether to show on desktop (>= 1024dp)
  final bool visibleOnDesktop;

  /// Optional replacement widget when hidden
  final Widget? replacement;

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;
    final category = width.deviceCategory;

    bool isVisible;
    switch (category) {
      case DeviceCategory.mobile:
        isVisible = visibleOnMobile;
      case DeviceCategory.tablet:
        isVisible = visibleOnTablet;
      case DeviceCategory.desktop:
        isVisible = visibleOnDesktop;
    }

    if (isVisible) {
      return child;
    }

    return replacement ?? const SizedBox.shrink();
  }
}

/// Widget that shows only on mobile
class MobileOnly extends StatelessWidget {
  const MobileOnly({super.key, required this.child, this.replacement});

  final Widget child;
  final Widget? replacement;

  @override
  Widget build(BuildContext context) {
    return ResponsiveVisibility(
      visibleOnMobile: true,
      visibleOnTablet: false,
      visibleOnDesktop: false,
      replacement: replacement,
      child: child,
    );
  }
}

/// Widget that shows only on tablet and larger
class TabletAndUp extends StatelessWidget {
  const TabletAndUp({super.key, required this.child, this.replacement});

  final Widget child;
  final Widget? replacement;

  @override
  Widget build(BuildContext context) {
    return ResponsiveVisibility(
      visibleOnMobile: false,
      visibleOnTablet: true,
      visibleOnDesktop: true,
      replacement: replacement,
      child: child,
    );
  }
}

/// Widget that shows only on desktop
class DesktopOnly extends StatelessWidget {
  const DesktopOnly({super.key, required this.child, this.replacement});

  final Widget child;
  final Widget? replacement;

  @override
  Widget build(BuildContext context) {
    return ResponsiveVisibility(
      visibleOnMobile: false,
      visibleOnTablet: false,
      visibleOnDesktop: true,
      replacement: replacement,
      child: child,
    );
  }
}

/// Responsive grid that adjusts columns based on screen size
class ResponsiveGrid extends StatelessWidget {
  const ResponsiveGrid({
    super.key,
    required this.children,
    this.mobileColumns = 1,
    this.tabletColumns = 2,
    this.desktopColumns = 3,
    this.crossAxisSpacing = 16,
    this.mainAxisSpacing = 16,
    this.childAspectRatio = 1.0,
    this.shrinkWrap = false,
    this.physics,
    this.padding,
  });

  final List<Widget> children;
  final int mobileColumns;
  final int tabletColumns;
  final int desktopColumns;
  final double crossAxisSpacing;
  final double mainAxisSpacing;
  final double childAspectRatio;
  final bool shrinkWrap;
  final ScrollPhysics? physics;
  final EdgeInsets? padding;

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;

    int columns;
    if (width >= Breakpoints.xxl) {
      columns = desktopColumns;
    } else if (width >= Breakpoints.lg) {
      columns = tabletColumns;
    } else {
      columns = mobileColumns;
    }

    return GridView.builder(
      shrinkWrap: shrinkWrap,
      physics: physics,
      padding: padding,
      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: columns,
        crossAxisSpacing: crossAxisSpacing,
        mainAxisSpacing: mainAxisSpacing,
        childAspectRatio: childAspectRatio,
      ),
      itemCount: children.length,
      itemBuilder: (context, index) => children[index],
    );
  }
}

/// Responsive SliverGrid for CustomScrollView
class ResponsiveSliverGrid extends StatelessWidget {
  const ResponsiveSliverGrid({
    super.key,
    required this.delegate,
    this.mobileColumns = 1,
    this.tabletColumns = 2,
    this.desktopColumns = 3,
    this.crossAxisSpacing = 16,
    this.mainAxisSpacing = 16,
    this.childAspectRatio = 1.0,
  });

  final SliverChildDelegate delegate;
  final int mobileColumns;
  final int tabletColumns;
  final int desktopColumns;
  final double crossAxisSpacing;
  final double mainAxisSpacing;
  final double childAspectRatio;

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.of(context).size.width;

    int columns;
    if (width >= Breakpoints.xxl) {
      columns = desktopColumns;
    } else if (width >= Breakpoints.lg) {
      columns = tabletColumns;
    } else {
      columns = mobileColumns;
    }

    return SliverGrid(
      delegate: delegate,
      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: columns,
        crossAxisSpacing: crossAxisSpacing,
        mainAxisSpacing: mainAxisSpacing,
        childAspectRatio: childAspectRatio,
      ),
    );
  }
}
