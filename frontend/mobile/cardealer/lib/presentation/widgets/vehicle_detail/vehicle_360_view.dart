import 'package:flutter/material.dart';
import 'dart:math' as math;
import '../../../core/theme/colors.dart';

/// 360° View Widget for Interactive Vehicle Rotation
///
/// Features:
/// - Touch/swipe to rotate vehicle
/// - Frame-by-frame image sequence
/// - Smooth rotation animation
/// - Loading indicator
/// - Rotation progress indicator
class Vehicle360View extends StatefulWidget {
  final List<String>
      imageUrls; // Sequence of 360° images (e.g., 36 images = 10° each)
  final int framesPerRotation;
  final double height;
  final bool autoRotate;
  final Duration autoRotateDuration;

  const Vehicle360View({
    super.key,
    required this.imageUrls,
    this.framesPerRotation = 36,
    this.height = 300,
    this.autoRotate = true,
    this.autoRotateDuration = const Duration(seconds: 8),
  });

  @override
  State<Vehicle360View> createState() => _Vehicle360ViewState();
}

class _Vehicle360ViewState extends State<Vehicle360View>
    with SingleTickerProviderStateMixin {
  int _currentFrame = 0;
  double _dragStartPosition = 0.0;
  bool _isDragging = false;
  late AnimationController _autoRotateController;
  bool _imagesLoaded = false;
  final List<Image> _cachedImages = [];

  @override
  void initState() {
    super.initState();
    _autoRotateController = AnimationController(
      vsync: this,
      duration: widget.autoRotateDuration,
    );

    _autoRotateController.addListener(() {
      if (!_isDragging && widget.autoRotate) {
        setState(() {
          _currentFrame =
              (_autoRotateController.value * widget.framesPerRotation).floor() %
                  widget.framesPerRotation;
        });
      }
    });

    if (widget.autoRotate) {
      _autoRotateController.repeat();
    }

    _preloadImages();
  }

  Future<void> _preloadImages() async {
    for (final url in widget.imageUrls) {
      final image = Image.network(url);
      _cachedImages.add(image);

      // Precache for faster loading
      await precacheImage(image.image, context);
    }

    if (mounted) {
      setState(() {
        _imagesLoaded = true;
      });
    }
  }

  @override
  void dispose() {
    _autoRotateController.dispose();
    super.dispose();
  }

  void _onPanStart(DragStartDetails details) {
    setState(() {
      _isDragging = true;
      _dragStartPosition = details.globalPosition.dx;
    });

    if (widget.autoRotate) {
      _autoRotateController.stop();
    }
  }

  void _onPanUpdate(DragUpdateDetails details) {
    final double delta = details.globalPosition.dx - _dragStartPosition;
    const double sensitivity = 2.0; // Adjust for rotation speed

    final int frameDelta = (delta / sensitivity).round();

    if (frameDelta != 0) {
      setState(() {
        _currentFrame = (_currentFrame - frameDelta) % widget.framesPerRotation;
        if (_currentFrame < 0) {
          _currentFrame += widget.framesPerRotation;
        }
        _dragStartPosition = details.globalPosition.dx;
      });
    }
  }

  void _onPanEnd(DragEndDetails details) {
    setState(() {
      _isDragging = false;
    });

    if (widget.autoRotate) {
      _autoRotateController.repeat();
    }
  }

  @override
  Widget build(BuildContext context) {
    if (!_imagesLoaded) {
      return Container(
        height: widget.height,
        decoration: BoxDecoration(
          color: AppColors.surfaceVariant,
          borderRadius: BorderRadius.circular(12),
        ),
        child: const Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              CircularProgressIndicator(
                color: AppColors.primary,
              ),
              SizedBox(height: 16),
              Text(
                'Loading 360° View...',
                style: TextStyle(
                  color: AppColors.textSecondary,
                  fontSize: 14,
                ),
              ),
            ],
          ),
        ),
      );
    }

    final frameIndex = _currentFrame.clamp(0, widget.imageUrls.length - 1);

    return Container(
      height: widget.height,
      decoration: BoxDecoration(
        color: AppColors.surfaceVariant,
        borderRadius: BorderRadius.circular(24),
      ),
      child: Stack(
        children: [
          // 360° Image Display
          GestureDetector(
            onPanStart: _onPanStart,
            onPanUpdate: _onPanUpdate,
            onPanEnd: _onPanEnd,
            child: Center(
              child: _cachedImages[frameIndex],
            ),
          ),

          // 360° Badge
          Positioned(
            top: 16,
            left: 16,
            child: Container(
              padding: const EdgeInsets.symmetric(
                horizontal: 12,
                vertical: 6,
              ),
              decoration: BoxDecoration(
                gradient: SweepGradient(
                  colors: [
                    AppColors.primary,
                    AppColors.primary.withValues(alpha: 0.8),
                  ],
                ),
                borderRadius: BorderRadius.circular(20),
                boxShadow: [
                  BoxShadow(
                    color: AppColors.primary.withValues(alpha: 0.3),
                    blurRadius: 8,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Transform.rotate(
                    angle: _isDragging
                        ? 0
                        : _autoRotateController.value * 2 * math.pi,
                    child: const Icon(
                      Icons.threesixty,
                      color: Colors.white,
                      size: 16,
                    ),
                  ),
                  const SizedBox(width: 6),
                  const Text(
                    '360° VIEW',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 12,
                      fontWeight: FontWeight.bold,
                      letterSpacing: 0.5,
                    ),
                  ),
                ],
              ),
            ),
          ),

          // Instructions
          if (!_isDragging && _autoRotateController.isAnimating)
            Positioned(
              bottom: 60,
              left: 0,
              right: 0,
              child: Center(
                child: Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 16,
                    vertical: 8,
                  ),
                  decoration: BoxDecoration(
                    color: Colors.black.withValues(alpha: 0.7),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(
                        Icons.swipe,
                        color: Colors.white.withValues(alpha: 0.9),
                        size: 16,
                      ),
                      const SizedBox(width: 8),
                      Text(
                        'Swipe to rotate',
                        style: TextStyle(
                          color: Colors.white.withValues(alpha: 0.9),
                          fontSize: 12,
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),

          // Rotation Progress Indicator
          Positioned(
            bottom: 16,
            left: 16,
            right: 16,
            child: Column(
              children: [
                // Progress bar
                Container(
                  height: 4,
                  decoration: BoxDecoration(
                    color: Colors.white.withValues(alpha: 0.3),
                    borderRadius: BorderRadius.circular(2),
                  ),
                  child: FractionallySizedBox(
                    alignment: Alignment.centerLeft,
                    widthFactor: _currentFrame / widget.framesPerRotation,
                    child: Container(
                      decoration: BoxDecoration(
                        gradient: const LinearGradient(
                          colors: [
                            AppColors.primary,
                            AppColors.accent,
                          ],
                        ),
                        borderRadius: BorderRadius.circular(2),
                      ),
                    ),
                  ),
                ),
                const SizedBox(height: 8),

                // Angle display
                Text(
                  '${(_currentFrame * (360 / widget.framesPerRotation)).toStringAsFixed(0)}°',
                  style: TextStyle(
                    color: Colors.white.withValues(alpha: 0.9),
                    fontSize: 12,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ],
            ),
          ),

          // Rotation direction indicator
          if (_isDragging)
            Positioned(
              top: 16,
              right: 16,
              child: Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: Colors.black.withValues(alpha: 0.6),
                  shape: BoxShape.circle,
                ),
                child: const Icon(
                  Icons.rotate_right,
                  color: AppColors.accent,
                  size: 20,
                ),
              ),
            ),
        ],
      ),
    );
  }
}
