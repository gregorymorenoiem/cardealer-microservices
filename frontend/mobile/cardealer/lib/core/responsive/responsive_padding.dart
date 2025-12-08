import 'package:flutter/material.dart';
import 'responsive_utils.dart';

/// Widget para padding responsive automático
class ResponsivePadding extends StatelessWidget {
  final Widget child;
  final double? multiplier;
  final EdgeInsets? additionalPadding;

  const ResponsivePadding({
    super.key,
    required this.child,
    this.multiplier = 1.0,
    this.additionalPadding,
  });

  @override
  Widget build(BuildContext context) {
    final basePadding =
        ResponsiveUtils.responsivePadding(context) * (multiplier ?? 1.0);

    EdgeInsets padding = EdgeInsets.all(basePadding);

    if (additionalPadding != null) {
      padding = EdgeInsets.only(
        left: basePadding + additionalPadding!.left,
        top: basePadding + additionalPadding!.top,
        right: basePadding + additionalPadding!.right,
        bottom: basePadding + additionalPadding!.bottom,
      );
    }

    return Padding(
      padding: padding,
      child: child,
    );
  }
}

/// Container con ancho máximo responsive
class ResponsiveContainer extends StatelessWidget {
  final Widget child;
  final double? maxWidth;
  final EdgeInsets? padding;
  final bool center;

  const ResponsiveContainer({
    super.key,
    required this.child,
    this.maxWidth,
    this.padding,
    this.center = true,
  });

  @override
  Widget build(BuildContext context) {
    final containerMaxWidth =
        maxWidth ?? ResponsiveUtils.maxContentWidth(context);

    Widget content = Container(
      constraints: BoxConstraints(maxWidth: containerMaxWidth),
      padding: padding,
      child: child,
    );

    if (center) {
      content = Center(child: content);
    }

    return content;
  }
}

/// Slivers responsive padding
class ResponsiveSliverPadding extends StatelessWidget {
  final Widget sliver;
  final double? multiplier;

  const ResponsiveSliverPadding({
    super.key,
    required this.sliver,
    this.multiplier = 1.0,
  });

  @override
  Widget build(BuildContext context) {
    final basePadding =
        ResponsiveUtils.responsivePadding(context) * (multiplier ?? 1.0);

    return SliverPadding(
      padding: EdgeInsets.all(basePadding),
      sliver: sliver,
    );
  }
}
