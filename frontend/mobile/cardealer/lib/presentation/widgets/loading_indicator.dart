import 'package:flutter/material.dart';
import 'package:shimmer/shimmer.dart';
import '../../core/theme/colors.dart';

/// Loading indicator with shimmer effect
class LoadingIndicator extends StatelessWidget {
  final LoadingIndicatorType type;
  final double? size;
  final Color? color;

  const LoadingIndicator({
    super.key,
    this.type = LoadingIndicatorType.circular,
    this.size,
    this.color,
  });

  const LoadingIndicator.circular({super.key, this.size, this.color})
    : type = LoadingIndicatorType.circular;

  const LoadingIndicator.linear({super.key, this.size, this.color})
    : type = LoadingIndicatorType.linear;

  const LoadingIndicator.shimmer({super.key, this.size, this.color})
    : type = LoadingIndicatorType.shimmer;

  @override
  Widget build(BuildContext context) {
    switch (type) {
      case LoadingIndicatorType.circular:
        return Center(
          child: SizedBox(
            width: size ?? 40,
            height: size ?? 40,
            child: CircularProgressIndicator(
              valueColor: AlwaysStoppedAnimation<Color>(
                color ?? AppColors.primary,
              ),
            ),
          ),
        );
      case LoadingIndicatorType.linear:
        return LinearProgressIndicator(
          valueColor: AlwaysStoppedAnimation<Color>(color ?? AppColors.primary),
        );
      case LoadingIndicatorType.shimmer:
        return Shimmer.fromColors(
          baseColor: AppColors.surfaceVariant,
          highlightColor: AppColors.surface,
          child: Container(
            width: size ?? double.infinity,
            height: size ?? 200,
            decoration: BoxDecoration(
              color: AppColors.surfaceVariant,
              borderRadius: BorderRadius.circular(8),
            ),
          ),
        );
    }
  }
}

enum LoadingIndicatorType { circular, linear, shimmer }

/// Shimmer placeholder for loading states
class ShimmerPlaceholder extends StatelessWidget {
  final double width;
  final double height;
  final BorderRadius? borderRadius;

  const ShimmerPlaceholder({
    super.key,
    required this.width,
    required this.height,
    this.borderRadius,
  });

  @override
  Widget build(BuildContext context) {
    return Shimmer.fromColors(
      baseColor: AppColors.surfaceVariant,
      highlightColor: AppColors.surface,
      child: Container(
        width: width,
        height: height,
        decoration: BoxDecoration(
          color: AppColors.surfaceVariant,
          borderRadius: borderRadius ?? BorderRadius.circular(8),
        ),
      ),
    );
  }
}
