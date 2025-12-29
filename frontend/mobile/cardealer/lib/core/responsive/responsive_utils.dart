import 'package:flutter/material.dart';

/// Breakpoints para diseño responsive
class Breakpoints {
  static const double mobile = 600;
  static const double tablet = 900;
  static const double desktop = 1200;
}

/// Clase de utilidades para diseño responsive
class ResponsiveUtils {
  /// Obtiene el ancho de la pantalla
  static double screenWidth(BuildContext context) {
    return MediaQuery.of(context).size.width;
  }

  /// Obtiene el alto de la pantalla
  static double screenHeight(BuildContext context) {
    return MediaQuery.of(context).size.height;
  }

  /// Determina si es un dispositivo móvil
  static bool isMobile(BuildContext context) {
    return screenWidth(context) < Breakpoints.mobile;
  }

  /// Determina si es una tablet
  static bool isTablet(BuildContext context) {
    return screenWidth(context) >= Breakpoints.mobile &&
        screenWidth(context) < Breakpoints.desktop;
  }

  /// Determina si es desktop
  static bool isDesktop(BuildContext context) {
    return screenWidth(context) >= Breakpoints.desktop;
  }

  /// Obtiene el tipo de dispositivo
  static DeviceType getDeviceType(BuildContext context) {
    final width = screenWidth(context);
    if (width < Breakpoints.mobile) return DeviceType.mobile;
    if (width < Breakpoints.desktop) return DeviceType.tablet;
    return DeviceType.desktop;
  }

  /// Retorna un valor según el tipo de dispositivo
  static T responsiveValue<T>(
    BuildContext context, {
    required T mobile,
    T? tablet,
    T? desktop,
  }) {
    final deviceType = getDeviceType(context);
    switch (deviceType) {
      case DeviceType.mobile:
        return mobile;
      case DeviceType.tablet:
        return tablet ?? mobile;
      case DeviceType.desktop:
        return desktop ?? tablet ?? mobile;
    }
  }

  /// Padding responsive basado en el tamaño de pantalla
  static double responsivePadding(BuildContext context) {
    return responsiveValue(
      context,
      mobile: 16.0,
      tablet: 24.0,
      desktop: 32.0,
    );
  }

  /// Tamaño de fuente responsive
  static double responsiveFontSize(
    BuildContext context,
    double baseSize,
  ) {
    final width = screenWidth(context);
    if (width < Breakpoints.mobile) {
      return baseSize;
    } else if (width < Breakpoints.desktop) {
      return baseSize * 1.1;
    } else {
      return baseSize * 1.2;
    }
  }

  /// Número de columnas para grid responsive
  static int gridColumns(BuildContext context) {
    return responsiveValue(
      context,
      mobile: 2,
      tablet: 3,
      desktop: 4,
    );
  }

  /// Aspect ratio para cards responsive
  static double cardAspectRatio(BuildContext context) {
    return responsiveValue(
      context,
      mobile: 0.75,
      tablet: 0.8,
      desktop: 0.85,
    );
  }

  /// Espaciado responsive
  static double spacing(BuildContext context, double multiplier) {
    return responsivePadding(context) * multiplier;
  }

  /// Ancho máximo de contenido para desktop
  static double maxContentWidth(BuildContext context) {
    return responsiveValue(
      context,
      mobile: double.infinity,
      tablet: 900,
      desktop: 1200,
    );
  }

  /// Determina la orientación
  static bool isPortrait(BuildContext context) {
    return MediaQuery.of(context).orientation == Orientation.portrait;
  }

  /// Determina si es landscape
  static bool isLandscape(BuildContext context) {
    return MediaQuery.of(context).orientation == Orientation.landscape;
  }
}

/// Tipo de dispositivo
enum DeviceType {
  mobile,
  tablet,
  desktop,
}

/// Widget builder responsive
class ResponsiveBuilder extends StatelessWidget {
  final Widget Function(BuildContext context, DeviceType deviceType) builder;

  const ResponsiveBuilder({
    super.key,
    required this.builder,
  });

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final deviceType = ResponsiveUtils.getDeviceType(context);
        return builder(context, deviceType);
      },
    );
  }
}

/// Widget para diseño responsive con variantes específicas
class ResponsiveLayout extends StatelessWidget {
  final Widget mobile;
  final Widget? tablet;
  final Widget? desktop;

  const ResponsiveLayout({
    super.key,
    required this.mobile,
    this.tablet,
    this.desktop,
  });

  @override
  Widget build(BuildContext context) {
    return ResponsiveBuilder(
      builder: (context, deviceType) {
        switch (deviceType) {
          case DeviceType.mobile:
            return mobile;
          case DeviceType.tablet:
            return tablet ?? mobile;
          case DeviceType.desktop:
            return desktop ?? tablet ?? mobile;
        }
      },
    );
  }
}

/// Extension para facilitar el uso de responsive utils
extension ResponsiveContext on BuildContext {
  bool get isMobile => ResponsiveUtils.isMobile(this);
  bool get isTablet => ResponsiveUtils.isTablet(this);
  bool get isDesktop => ResponsiveUtils.isDesktop(this);
  bool get isPortrait => ResponsiveUtils.isPortrait(this);
  bool get isLandscape => ResponsiveUtils.isLandscape(this);

  DeviceType get deviceType => ResponsiveUtils.getDeviceType(this);
  double get screenWidth => ResponsiveUtils.screenWidth(this);
  double get screenHeight => ResponsiveUtils.screenHeight(this);
  double get responsivePadding => ResponsiveUtils.responsivePadding(this);
  double get maxContentWidth => ResponsiveUtils.maxContentWidth(this);

  int get gridColumns => ResponsiveUtils.gridColumns(this);

  T responsive<T>({
    required T mobile,
    T? tablet,
    T? desktop,
  }) {
    return ResponsiveUtils.responsiveValue(
      this,
      mobile: mobile,
      tablet: tablet,
      desktop: desktop,
    );
  }

  double spacing(double multiplier) {
    return ResponsiveUtils.spacing(this, multiplier);
  }
}
