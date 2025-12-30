/// Optimized image loading with caching and lazy loading
library;

import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../widgets/animated_widgets.dart';

/// Optimized network image with caching and loading states
class OptimizedNetworkImage extends StatelessWidget {
  final String imageUrl;
  final double? width;
  final double? height;
  final BoxFit fit;
  final BorderRadius? borderRadius;
  final Widget? placeholder;
  final Widget? errorWidget;
  final bool fadeIn;

  const OptimizedNetworkImage({
    super.key,
    required this.imageUrl,
    this.width,
    this.height,
    this.fit = BoxFit.cover,
    this.borderRadius,
    this.placeholder,
    this.errorWidget,
    this.fadeIn = true,
  });

  @override
  Widget build(BuildContext context) {
    return ClipRRect(
      borderRadius: borderRadius ?? BorderRadius.zero,
      child: CachedNetworkImage(
        imageUrl: imageUrl,
        width: width,
        height: height,
        fit: fit,
        fadeInDuration:
            fadeIn ? const Duration(milliseconds: 300) : Duration.zero,
        placeholder: (context, url) =>
            placeholder ??
            ShimmerLoading(
              width: width ?? double.infinity,
              height: height ?? 200,
              borderRadius: borderRadius,
            ),
        errorWidget: (context, url, error) =>
            errorWidget ??
            Container(
              width: width,
              height: height,
              color: Colors.grey[300],
              child: const Icon(
                Icons.broken_image,
                color: Colors.grey,
                size: 48,
              ),
            ),
      ),
    );
  }
}

/// Lazy loading list view with viewport detection
class LazyLoadListView extends StatefulWidget {
  final int itemCount;
  final IndexedWidgetBuilder itemBuilder;
  final ScrollController? controller;
  final VoidCallback? onLoadMore;
  final bool isLoading;
  final bool hasMore;
  final Axis scrollDirection;
  final EdgeInsets? padding;

  const LazyLoadListView({
    super.key,
    required this.itemCount,
    required this.itemBuilder,
    this.controller,
    this.onLoadMore,
    this.isLoading = false,
    this.hasMore = true,
    this.scrollDirection = Axis.vertical,
    this.padding,
  });

  @override
  State<LazyLoadListView> createState() => _LazyLoadListViewState();
}

class _LazyLoadListViewState extends State<LazyLoadListView> {
  late ScrollController _scrollController;

  @override
  void initState() {
    super.initState();
    _scrollController = widget.controller ?? ScrollController();
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    if (widget.controller == null) {
      _scrollController.dispose();
    }
    super.dispose();
  }

  void _onScroll() {
    if (_scrollController.position.pixels >=
            _scrollController.position.maxScrollExtent * 0.8 &&
        !widget.isLoading &&
        widget.hasMore) {
      widget.onLoadMore?.call();
    }
  }

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      controller: _scrollController,
      scrollDirection: widget.scrollDirection,
      padding: widget.padding,
      itemCount: widget.itemCount + (widget.hasMore ? 1 : 0),
      itemBuilder: (context, index) {
        if (index == widget.itemCount) {
          return _buildLoadingIndicator();
        }
        return widget.itemBuilder(context, index);
      },
    );
  }

  Widget _buildLoadingIndicator() {
    if (!widget.isLoading) return const SizedBox.shrink();

    return Center(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: CircularProgressIndicator(
          valueColor: AlwaysStoppedAnimation<Color>(
            Theme.of(context).primaryColor,
          ),
        ),
      ),
    );
  }
}

/// Preload images for better UX
class ImagePreloader {
  /// Preload a single image
  static Future<void> preloadImage(
    BuildContext context,
    String imageUrl,
  ) async {
    try {
      final image = CachedNetworkImageProvider(imageUrl);
      await precacheImage(image, context);
    } catch (e) {
      debugPrint('Error preloading image: $e');
    }
  }

  /// Preload multiple images
  static Future<void> preloadImages(
    BuildContext context,
    List<String> imageUrls,
  ) async {
    final futures = imageUrls.map((url) => preloadImage(context, url));
    await Future.wait(futures);
  }

  /// Preload images in the background with priority
  /// Note: Context must be checked for validity before calling this method
  static void preloadInBackground(
    BuildContext context,
    List<String> imageUrls, {
    int priority = 0,
  }) {
    Future.delayed(Duration(milliseconds: priority * 100), () {
      // Check if context is still mounted before using it
      if (context.mounted) {
        preloadImages(context, imageUrls);
      }
    });
  }
}

/// Progressive image loading with blur effect
class ProgressiveImage extends StatelessWidget {
  final String imageUrl;
  final String? thumbnailUrl;
  final double? width;
  final double? height;
  final BoxFit fit;
  final BorderRadius? borderRadius;

  const ProgressiveImage({
    super.key,
    required this.imageUrl,
    this.thumbnailUrl,
    this.width,
    this.height,
    this.fit = BoxFit.cover,
    this.borderRadius,
  });

  @override
  Widget build(BuildContext context) {
    return ClipRRect(
      borderRadius: borderRadius ?? BorderRadius.zero,
      child: Stack(
        children: [
          // Thumbnail (blurred)
          if (thumbnailUrl != null)
            CachedNetworkImage(
              imageUrl: thumbnailUrl!,
              width: width,
              height: height,
              fit: fit,
              imageBuilder: (context, imageProvider) {
                return Container(
                  decoration: BoxDecoration(
                    image: DecorationImage(
                      image: imageProvider,
                      fit: fit,
                      colorFilter: ColorFilter.mode(
                        Colors.black.withValues(alpha: 0.1),
                        BlendMode.darken,
                      ),
                    ),
                  ),
                );
              },
            ),
          // Full resolution image
          CachedNetworkImage(
            imageUrl: imageUrl,
            width: width,
            height: height,
            fit: fit,
            fadeInDuration: const Duration(milliseconds: 500),
            placeholder: (context, url) {
              if (thumbnailUrl != null) {
                return const SizedBox.shrink();
              }
              return ShimmerLoading(
                width: width ?? double.infinity,
                height: height ?? 200,
                borderRadius: borderRadius,
              );
            },
            errorWidget: (context, url, error) => Container(
              width: width,
              height: height,
              color: Colors.grey[300],
              child: const Icon(
                Icons.broken_image,
                color: Colors.grey,
                size: 48,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

/// Grid view with lazy loading
class LazyLoadGridView extends StatefulWidget {
  final int itemCount;
  final IndexedWidgetBuilder itemBuilder;
  final int crossAxisCount;
  final double childAspectRatio;
  final double crossAxisSpacing;
  final double mainAxisSpacing;
  final ScrollController? controller;
  final VoidCallback? onLoadMore;
  final bool isLoading;
  final bool hasMore;
  final EdgeInsets? padding;

  const LazyLoadGridView({
    super.key,
    required this.itemCount,
    required this.itemBuilder,
    this.crossAxisCount = 2,
    this.childAspectRatio = 1.0,
    this.crossAxisSpacing = 8.0,
    this.mainAxisSpacing = 8.0,
    this.controller,
    this.onLoadMore,
    this.isLoading = false,
    this.hasMore = true,
    this.padding,
  });

  @override
  State<LazyLoadGridView> createState() => _LazyLoadGridViewState();
}

class _LazyLoadGridViewState extends State<LazyLoadGridView> {
  late ScrollController _scrollController;

  @override
  void initState() {
    super.initState();
    _scrollController = widget.controller ?? ScrollController();
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    if (widget.controller == null) {
      _scrollController.dispose();
    }
    super.dispose();
  }

  void _onScroll() {
    if (_scrollController.position.pixels >=
            _scrollController.position.maxScrollExtent * 0.8 &&
        !widget.isLoading &&
        widget.hasMore) {
      widget.onLoadMore?.call();
    }
  }

  @override
  Widget build(BuildContext context) {
    return GridView.builder(
      controller: _scrollController,
      padding: widget.padding ?? const EdgeInsets.all(16),
      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: widget.crossAxisCount,
        childAspectRatio: widget.childAspectRatio,
        crossAxisSpacing: widget.crossAxisSpacing,
        mainAxisSpacing: widget.mainAxisSpacing,
      ),
      itemCount: widget.itemCount + (widget.hasMore ? 1 : 0),
      itemBuilder: (context, index) {
        if (index == widget.itemCount) {
          return _buildLoadingIndicator();
        }
        return widget.itemBuilder(context, index);
      },
    );
  }

  Widget _buildLoadingIndicator() {
    if (!widget.isLoading) return const SizedBox.shrink();

    return Center(
      child: CircularProgressIndicator(
        valueColor: AlwaysStoppedAnimation<Color>(
          Theme.of(context).primaryColor,
        ),
      ),
    );
  }
}

/// Image cache manager utilities
class ImageCacheManager {
  /// Clear all cached images
  static Future<void> clearCache() async {
    await CachedNetworkImage.evictFromCache('');
  }

  /// Clear specific image from cache
  static Future<void> clearImageCache(String imageUrl) async {
    await CachedNetworkImage.evictFromCache(imageUrl);
  }

  /// Get cache size (approximate)
  static Future<int> getCacheSize() async {
    // This would require implementing custom cache manager
    // For now, return 0 as placeholder
    return 0;
  }
}
