library;

/// Accessibility utilities and helpers
/// Ensures app is accessible to all users including those with disabilities
import 'package:flutter/material.dart';

/// Accessibility constants
class A11yConstants {
  // Minimum touch target size (Material Design)
  static const double minTouchTarget = 48.0;

  // WCAG contrast ratios
  static const double minContrastNormal = 4.5;
  static const double minContrastLarge = 3.0;

  // Text scaling limits
  static const double minTextScale = 0.8;
  static const double maxTextScale = 2.0;
}

/// Semantic labels helper
class A11yLabels {
  static String vehicle({
    required String brand,
    required String model,
    required int year,
    required String price,
  }) =>
      'Vehículo $brand $model año $year, precio $price';

  static String image(String description) => 'Imagen de $description';

  static String button(String action) => 'Botón de $action';

  static String loading(String content) => 'Cargando $content';

  static String error(String message) => 'Error: $message';

  static String badge(int count, String item) =>
      '$count ${count == 1 ? item : '${item}s'} nuevos';

  static String progress(int current, int total) =>
      'Progreso: $current de $total';

  static String rating(double rating, {int? count}) =>
      'Calificación $rating estrellas${count != null ? ', $count valoraciones' : ''}';
}

/// Accessible widget wrapper
class AccessibleWidget extends StatelessWidget {
  final Widget child;
  final String? label;
  final String? hint;
  final bool excludeSemantics;
  final bool button;
  final bool link;
  final bool header;
  final VoidCallback? onTap;

  const AccessibleWidget({
    super.key,
    required this.child,
    this.label,
    this.hint,
    this.excludeSemantics = false,
    this.button = false,
    this.link = false,
    this.header = false,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    if (excludeSemantics) {
      return ExcludeSemantics(child: child);
    }

    return Semantics(
      label: label,
      hint: hint,
      button: button,
      link: link,
      header: header,
      onTap: onTap,
      child: child,
    );
  }
}

/// Ensures minimum touch target size
class TouchTargetWrapper extends StatelessWidget {
  final Widget child;
  final double minSize;

  const TouchTargetWrapper({
    super.key,
    required this.child,
    this.minSize = A11yConstants.minTouchTarget,
  });

  @override
  Widget build(BuildContext context) {
    return ConstrainedBox(
      constraints: BoxConstraints(
        minWidth: minSize,
        minHeight: minSize,
      ),
      child: child,
    );
  }
}

/// Screen reader announcements
class A11yAnnouncer {
  static void announce(
    BuildContext context,
    String message, {
    TextDirection textDirection = TextDirection.ltr,
  }) {
    final overlay = Overlay.of(context);
    final entry = OverlayEntry(
      builder: (context) => Semantics(
        liveRegion: true,
        child: SizedBox.shrink(
          child: Text(
            message,
            textDirection: textDirection,
          ),
        ),
      ),
    );

    overlay.insert(entry);
    Future.delayed(const Duration(milliseconds: 100), () {
      entry.remove();
    });
  }
}

/// Color contrast checker
class ContrastChecker {
  static double calculateLuminance(Color color) {
    final r = _linearize((color.r * 255.0).round().clamp(0, 255) / 255);
    final g = _linearize((color.g * 255.0).round().clamp(0, 255) / 255);
    final b = _linearize((color.b * 255.0).round().clamp(0, 255) / 255);
    return 0.2126 * r + 0.7152 * g + 0.0722 * b;
  }

  static double _linearize(double channel) {
    if (channel <= 0.03928) {
      return channel / 12.92;
    }
    return ((channel + 0.055) / 1.055).abs();
  }

  static double calculateContrast(Color foreground, Color background) {
    final lumFg = calculateLuminance(foreground);
    final lumBg = calculateLuminance(background);
    final lighter = lumFg > lumBg ? lumFg : lumBg;
    final darker = lumFg > lumBg ? lumBg : lumFg;
    return (lighter + 0.05) / (darker + 0.05);
  }

  static bool meetsWCAG(
    Color foreground,
    Color background, {
    bool largeText = false,
  }) {
    final contrast = calculateContrast(foreground, background);
    final minContrast = largeText
        ? A11yConstants.minContrastLarge
        : A11yConstants.minContrastNormal;
    return contrast >= minContrast;
  }

  static Color ensureContrast(
    Color foreground,
    Color background, {
    bool largeText = false,
  }) {
    if (meetsWCAG(foreground, background, largeText: largeText)) {
      return foreground;
    }

    // Try darkening or lightening
    final luminance = calculateLuminance(background);
    return luminance > 0.5 ? Colors.black : Colors.white;
  }
}

/// Text scale factor helper
class TextScaleHelper {
  static double clamp(double scale) {
    return scale.clamp(
      A11yConstants.minTextScale,
      A11yConstants.maxTextScale,
    );
  }

  static double get(BuildContext context) {
    return clamp(MediaQuery.textScalerOf(context).scale(1.0));
  }

  static bool isLarge(BuildContext context) {
    return get(context) > 1.3;
  }

  static Widget builder({
    required BuildContext context,
    required Widget Function(BuildContext, double) builder,
  }) {
    return Builder(
      builder: (context) {
        final scale = get(context);
        return builder(context, scale);
      },
    );
  }
}

/// Accessible icon button
class AccessibleIconButton extends StatelessWidget {
  final IconData icon;
  final String label;
  final VoidCallback? onPressed;
  final double? size;
  final Color? color;
  final String? tooltip;

  const AccessibleIconButton({
    super.key,
    required this.icon,
    required this.label,
    this.onPressed,
    this.size,
    this.color,
    this.tooltip,
  });

  @override
  Widget build(BuildContext context) {
    return TouchTargetWrapper(
      child: Semantics(
        label: label,
        button: true,
        enabled: onPressed != null,
        child: IconButton(
          icon: Icon(icon),
          onPressed: onPressed,
          iconSize: size,
          color: color,
          tooltip: tooltip ?? label,
        ),
      ),
    );
  }
}

/// Accessible text button
class AccessibleTextButton extends StatelessWidget {
  final String text;
  final VoidCallback? onPressed;
  final String? semanticLabel;

  const AccessibleTextButton({
    super.key,
    required this.text,
    this.onPressed,
    this.semanticLabel,
  });

  @override
  Widget build(BuildContext context) {
    return TouchTargetWrapper(
      child: Semantics(
        label: semanticLabel ?? text,
        button: true,
        enabled: onPressed != null,
        child: TextButton(
          onPressed: onPressed,
          child: Text(text),
        ),
      ),
    );
  }
}

/// Focus management helper
class FocusHelper {
  static void requestFocus(BuildContext context, FocusNode node) {
    FocusScope.of(context).requestFocus(node);
  }

  static void unfocus(BuildContext context) {
    FocusScope.of(context).unfocus();
  }

  static void nextFocus(BuildContext context) {
    FocusScope.of(context).nextFocus();
  }

  static void previousFocus(BuildContext context) {
    FocusScope.of(context).previousFocus();
  }
}

/// Accessible image with semantic label
class AccessibleImage extends StatelessWidget {
  final String imageUrl;
  final String semanticLabel;
  final BoxFit? fit;
  final double? width;
  final double? height;

  const AccessibleImage({
    super.key,
    required this.imageUrl,
    required this.semanticLabel,
    this.fit,
    this.width,
    this.height,
  });

  @override
  Widget build(BuildContext context) {
    return Semantics(
      label: A11yLabels.image(semanticLabel),
      image: true,
      child: Image.network(
        imageUrl,
        fit: fit,
        width: width,
        height: height,
        semanticLabel: semanticLabel,
      ),
    );
  }
}

/// Skip to content button
class SkipToContentButton extends StatelessWidget {
  final FocusNode contentFocus;
  final String label;

  const SkipToContentButton({
    super.key,
    required this.contentFocus,
    this.label = 'Saltar al contenido',
  });

  @override
  Widget build(BuildContext context) {
    return Positioned(
      top: -100,
      left: 0,
      child: Focus(
        onFocusChange: (hasFocus) {
          if (hasFocus) {
            // Bring button into view when focused
            Scrollable.ensureVisible(
              context,
              duration: const Duration(milliseconds: 300),
            );
          }
        },
        child: AccessibleTextButton(
          text: label,
          onPressed: () {
            FocusHelper.requestFocus(context, contentFocus);
          },
        ),
      ),
    );
  }
}

/// Accessible card with semantic grouping
class AccessibleCard extends StatelessWidget {
  final Widget child;
  final String? label;
  final VoidCallback? onTap;
  final EdgeInsets? padding;

  const AccessibleCard({
    super.key,
    required this.child,
    this.label,
    this.onTap,
    this.padding,
  });

  @override
  Widget build(BuildContext context) {
    return Semantics(
      label: label,
      button: onTap != null,
      child: Card(
        child: InkWell(
          onTap: onTap,
          child: Padding(
            padding: padding ?? const EdgeInsets.all(16),
            child: child,
          ),
        ),
      ),
    );
  }
}

/// Accessibility checker widget (debug only)
class A11yChecker extends StatelessWidget {
  final Widget child;

  const A11yChecker({
    super.key,
    required this.child,
  });

  @override
  Widget build(BuildContext context) {
    assert(() {
      // Debug mode checks
      _checkTouchTargets(context);
      _checkContrasts(context);
      return true;
    }());

    return child;
  }

  void _checkTouchTargets(BuildContext context) {
    // Implement touch target validation
    debugPrint('A11y: Checking touch targets...');
  }

  void _checkContrasts(BuildContext context) {
    // Implement contrast validation
    debugPrint('A11y: Checking color contrasts...');
  }
}

/// High contrast mode detector
class HighContrastDetector extends StatelessWidget {
  final Widget child;
  final Widget Function(BuildContext, bool)? builder;

  const HighContrastDetector({
    super.key,
    required this.child,
    this.builder,
  });

  @override
  Widget build(BuildContext context) {
    final highContrast = MediaQuery.highContrastOf(context);

    if (builder != null) {
      return builder!(context, highContrast);
    }

    return child;
  }
}
