import 'package:flutter/material.dart';

/// Bottom CTA Section - HR-011
/// Final call-to-action before footer
class BottomCTASection extends StatefulWidget {
  final VoidCallback? onBrowseCars;
  final VoidCallback? onSellCar;

  const BottomCTASection({
    super.key,
    this.onBrowseCars,
    this.onSellCar,
  });

  @override
  State<BottomCTASection> createState() => _BottomCTASectionState();
}

class _BottomCTASectionState extends State<BottomCTASection>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<Offset> _slideAnimation;
  late Animation<double> _fadeAnimation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 800),
      vsync: this,
    );

    _slideAnimation = Tween<Offset>(
      begin: const Offset(0, 0.3),
      end: Offset.zero,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeOut));

    _fadeAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeIn),
    );

    _controller.forward();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return SlideTransition(
      position: _slideAnimation,
      child: FadeTransition(
        opacity: _fadeAnimation,
        child: Container(
          width: double.infinity,
          decoration: BoxDecoration(
            gradient: LinearGradient(
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
              colors: [
                const Color(0xFFFF6B35),
                const Color(0xFFFF8555),
                const Color(0xFFFF6B35).withValues(alpha: 0.8),
              ],
            ),
          ),
          padding: const EdgeInsets.symmetric(vertical: 48, horizontal: 24),
          child: LayoutBuilder(
            builder: (context, constraints) {
              final isWide = constraints.maxWidth > 600;
              return Column(
                children: [
                  // Decorative Elements
                  Stack(
                    alignment: Alignment.center,
                    children: [
                      // Background circles
                      const Positioned(
                        top: -50,
                        right: -50,
                        child: _DecorativeCircle(
                          size: 150,
                          opacity: 0.1,
                        ),
                      ),
                      const Positioned(
                        bottom: -30,
                        left: -30,
                        child: _DecorativeCircle(
                          size: 100,
                          opacity: 0.15,
                        ),
                      ),
                      // Main Content
                      Column(
                        children: [
                          // Icon
                          Container(
                            width: 80,
                            height: 80,
                            decoration: BoxDecoration(
                              color: Colors.white,
                              shape: BoxShape.circle,
                              boxShadow: [
                                BoxShadow(
                                  color: Colors.black.withValues(alpha: 0.2),
                                  blurRadius: 16,
                                  offset: const Offset(0, 4),
                                ),
                              ],
                            ),
                            child: const Icon(
                              Icons.directions_car,
                              color: Color(0xFFFF6B35),
                              size: 40,
                            ),
                          ),
                          const SizedBox(height: 24),
                          // Title
                          Text(
                            'Start Your Journey Today',
                            style: Theme.of(context)
                                .textTheme
                                .headlineMedium
                                ?.copyWith(
                                  fontWeight: FontWeight.bold,
                                  color: Colors.white,
                                ),
                            textAlign: TextAlign.center,
                          ),
                          const SizedBox(height: 12),
                          // Subtitle
                          Text(
                            'Find your perfect car or sell yours in minutes',
                            style: Theme.of(context)
                                .textTheme
                                .bodyLarge
                                ?.copyWith(
                                  color: Colors.white.withValues(alpha: 0.9),
                                ),
                            textAlign: TextAlign.center,
                          ),
                          const SizedBox(height: 32),
                          // CTA Buttons
                          if (isWide)
                            // Row layout for wide screens
                            Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                _CTAButton(
                                  label: 'Browse Cars',
                                  icon: Icons.search,
                                  isPrimary: false,
                                  onTap: widget.onBrowseCars,
                                ),
                                const SizedBox(width: 16),
                                _CTAButton(
                                  label: 'Sell Your Car',
                                  icon: Icons.sell,
                                  isPrimary: true,
                                  onTap: widget.onSellCar,
                                ),
                              ],
                            )
                          else
                            // Column layout for narrow screens
                            Column(
                              children: [
                                _CTAButton(
                                  label: 'Browse Cars',
                                  icon: Icons.search,
                                  isPrimary: false,
                                  onTap: widget.onBrowseCars,
                                  fullWidth: true,
                                ),
                                const SizedBox(height: 12),
                                _CTAButton(
                                  label: 'Sell Your Car',
                                  icon: Icons.sell,
                                  isPrimary: true,
                                  onTap: widget.onSellCar,
                                  fullWidth: true,
                                ),
                              ],
                            ),
                        ],
                      ),
                    ],
                  ),
                ],
              );
            },
          ),
        ),
      ),
    );
  }
}

class _CTAButton extends StatefulWidget {
  final String label;
  final IconData icon;
  final bool isPrimary;
  final VoidCallback? onTap;
  final bool fullWidth;

  const _CTAButton({
    required this.label,
    required this.icon,
    required this.isPrimary,
    this.onTap,
    this.fullWidth = false,
  });

  @override
  State<_CTAButton> createState() => _CTAButtonState();
}

class _CTAButtonState extends State<_CTAButton>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _scaleAnimation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 150),
      vsync: this,
    );
    _scaleAnimation = Tween<double>(begin: 1.0, end: 0.95).animate(
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
    return GestureDetector(
      onTapDown: (_) => _controller.forward(),
      onTapUp: (_) {
        _controller.reverse();
        widget.onTap?.call();
      },
      onTapCancel: () => _controller.reverse(),
      child: AnimatedBuilder(
        animation: _scaleAnimation,
        builder: (context, child) {
          return Transform.scale(
            scale: _scaleAnimation.value,
            child: child,
          );
        },
        child: Container(
          width: widget.fullWidth ? double.infinity : null,
          padding: const EdgeInsets.symmetric(
            horizontal: 32,
            vertical: 16,
          ),
          decoration: BoxDecoration(
            color: widget.isPrimary
                ? Colors.white
                : Colors.white.withValues(alpha: 0.2),
            borderRadius: BorderRadius.circular(30),
            border: widget.isPrimary
                ? null
                : Border.all(
                    color: Colors.white,
                    width: 2,
                  ),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withValues(alpha: 0.2),
                blurRadius: 12,
                offset: const Offset(0, 4),
              ),
            ],
          ),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                widget.icon,
                color:
                    widget.isPrimary ? const Color(0xFFFF6B35) : Colors.white,
                size: 24,
              ),
              const SizedBox(width: 12),
              Text(
                widget.label,
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                  color:
                      widget.isPrimary ? const Color(0xFFFF6B35) : Colors.white,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _DecorativeCircle extends StatelessWidget {
  final double size;
  final double opacity;

  const _DecorativeCircle({
    required this.size,
    required this.opacity,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: size,
      height: size,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: Colors.white.withValues(alpha: opacity),
      ),
    );
  }
}
