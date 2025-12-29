import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';

/// Unified skeleton loader with shimmer effect for loading states
/// Can be used for text, images, cards, lists, and custom shapes
class SkeletonLoader extends StatefulWidget {
  final SkeletonLoaderType type;
  final double? width;
  final double? height;
  final double? borderRadius;
  final int? lineCount; // For text type
  final EdgeInsets? padding;

  const SkeletonLoader({
    super.key,
    required this.type,
    this.width,
    this.height,
    this.borderRadius,
    this.lineCount,
    this.padding,
  });

  /// Creates a skeleton for text loading
  factory SkeletonLoader.text({
    double? width,
    double? height = 16,
    int lineCount = 3,
    EdgeInsets? padding,
  }) {
    return SkeletonLoader(
      type: SkeletonLoaderType.text,
      width: width,
      height: height,
      lineCount: lineCount,
      padding: padding,
    );
  }

  /// Creates a skeleton for image loading
  factory SkeletonLoader.image({
    double? width,
    double? height,
    double borderRadius = 12,
    EdgeInsets? padding,
  }) {
    return SkeletonLoader(
      type: SkeletonLoaderType.image,
      width: width,
      height: height,
      borderRadius: borderRadius,
      padding: padding,
    );
  }

  /// Creates a skeleton for card loading
  factory SkeletonLoader.card({
    double? width,
    double height = 200,
    double borderRadius = 12,
    EdgeInsets? padding,
  }) {
    return SkeletonLoader(
      type: SkeletonLoaderType.card,
      width: width,
      height: height,
      borderRadius: borderRadius,
      padding: padding,
    );
  }

  /// Creates a skeleton for list item loading
  factory SkeletonLoader.listItem({
    double? width,
    double height = 80,
    double borderRadius = 8,
    EdgeInsets? padding,
  }) {
    return SkeletonLoader(
      type: SkeletonLoaderType.listItem,
      width: width,
      height: height,
      borderRadius: borderRadius,
      padding: padding,
    );
  }

  /// Creates a custom skeleton with specific dimensions
  factory SkeletonLoader.custom({
    required double width,
    required double height,
    double borderRadius = 8,
    EdgeInsets? padding,
  }) {
    return SkeletonLoader(
      type: SkeletonLoaderType.custom,
      width: width,
      height: height,
      borderRadius: borderRadius,
      padding: padding,
    );
  }

  @override
  State<SkeletonLoader> createState() => _SkeletonLoaderState();
}

class _SkeletonLoaderState extends State<SkeletonLoader>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _animation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1500),
    )..repeat();

    _animation = Tween<double>(begin: -2, end: 2).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeInOut),
    );
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: widget.padding ?? EdgeInsets.zero,
      child: _buildSkeletonByType(),
    );
  }

  Widget _buildSkeletonByType() {
    switch (widget.type) {
      case SkeletonLoaderType.text:
        return _buildTextSkeleton();
      case SkeletonLoaderType.image:
        return _buildImageSkeleton();
      case SkeletonLoaderType.card:
        return _buildCardSkeleton();
      case SkeletonLoaderType.listItem:
        return _buildListItemSkeleton();
      case SkeletonLoaderType.custom:
        return _buildCustomSkeleton();
    }
  }

  Widget _buildTextSkeleton() {
    final lineCount = widget.lineCount ?? 3;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: List.generate(lineCount, (index) {
        final isLastLine = index == lineCount - 1;
        final width = isLastLine
            ? (widget.width ?? double.infinity) * 0.7
            : widget.width ?? double.infinity;

        return Padding(
          padding: EdgeInsets.only(
            bottom: index < lineCount - 1 ? AppSpacing.sm : 0,
          ),
          child: _buildShimmerBox(
            width: width,
            height: widget.height ?? 16,
            borderRadius: 4,
          ),
        );
      }),
    );
  }

  Widget _buildImageSkeleton() {
    return _buildShimmerBox(
      width: widget.width ?? double.infinity,
      height: widget.height ?? 200,
      borderRadius: widget.borderRadius ?? 12,
    );
  }

  Widget _buildCardSkeleton() {
    return Container(
      width: widget.width ?? double.infinity,
      height: widget.height ?? 200,
      decoration: BoxDecoration(
        color: AppColors.surfaceVariant,
        borderRadius: BorderRadius.circular(widget.borderRadius ?? 12),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Image placeholder
          _buildShimmerBox(
            width: double.infinity,
            height: 120,
            borderRadius: widget.borderRadius ?? 12,
          ),
          Padding(
            padding: const EdgeInsets.all(AppSpacing.md),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Title
                _buildShimmerBox(
                  width: double.infinity,
                  height: 20,
                  borderRadius: 4,
                ),
                const SizedBox(height: AppSpacing.sm),
                // Subtitle
                _buildShimmerBox(
                  width: 200,
                  height: 14,
                  borderRadius: 4,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildListItemSkeleton() {
    return Container(
      width: widget.width ?? double.infinity,
      height: widget.height ?? 80,
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.surfaceVariant,
        borderRadius: BorderRadius.circular(widget.borderRadius ?? 8),
      ),
      child: Row(
        children: [
          // Avatar/Image
          _buildShimmerBox(
            width: 60,
            height: 60,
            borderRadius: 8,
          ),
          const SizedBox(width: AppSpacing.md),
          // Content
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                _buildShimmerBox(
                  width: double.infinity,
                  height: 16,
                  borderRadius: 4,
                ),
                const SizedBox(height: AppSpacing.sm),
                _buildShimmerBox(
                  width: 150,
                  height: 14,
                  borderRadius: 4,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildCustomSkeleton() {
    return _buildShimmerBox(
      width: widget.width ?? 100,
      height: widget.height ?? 100,
      borderRadius: widget.borderRadius ?? 8,
    );
  }

  Widget _buildShimmerBox({
    required double width,
    required double height,
    required double borderRadius,
  }) {
    return AnimatedBuilder(
      animation: _animation,
      builder: (context, child) {
        return Container(
          width: width,
          height: height,
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(borderRadius),
            gradient: LinearGradient(
              begin: Alignment.centerLeft,
              end: Alignment.centerRight,
              colors: const [
                AppColors.border,
                AppColors.borderDark,
                AppColors.border,
              ],
              stops: [
                _animation.value - 0.3,
                _animation.value,
                _animation.value + 0.3,
              ].map((stop) => stop.clamp(0.0, 1.0)).toList(),
            ),
          ),
        );
      },
    );
  }
}

enum SkeletonLoaderType {
  text,
  image,
  card,
  listItem,
  custom,
}
