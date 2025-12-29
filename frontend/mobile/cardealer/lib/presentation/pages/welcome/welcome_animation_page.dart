import 'package:flutter/material.dart';
import 'dart:math' as math;
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../core/theme/typography.dart';

/// Confetti particle model
class ConfettiParticle {
  final Color color;
  final double size;
  final double startX;
  final double endX;
  final double endY;
  final double rotation;
  final double rotationSpeed;

  ConfettiParticle({
    required this.color,
    required this.size,
    required this.startX,
    required this.endX,
    required this.endY,
    required this.rotation,
    required this.rotationSpeed,
  });
}

/// Welcome animation screen shown after successful registration
class WelcomeAnimationPage extends StatefulWidget {
  final String userName;
  final VoidCallback onComplete;

  const WelcomeAnimationPage({
    super.key,
    required this.userName,
    required this.onComplete,
  });

  @override
  State<WelcomeAnimationPage> createState() => _WelcomeAnimationPageState();
}

class _WelcomeAnimationPageState extends State<WelcomeAnimationPage>
    with TickerProviderStateMixin {
  late AnimationController _confettiController;
  late AnimationController _textController;
  late Animation<double> _textFadeAnimation;
  late Animation<double> _textScaleAnimation;

  final List<ConfettiParticle> _particles = [];
  final _random = math.Random();

  @override
  void initState() {
    super.initState();

    // Confetti animation controller
    _confettiController = AnimationController(
      duration: const Duration(milliseconds: 3000),
      vsync: this,
    );

    // Text animation controller
    _textController = AnimationController(
      duration: const Duration(milliseconds: 800),
      vsync: this,
    );

    _textFadeAnimation = CurvedAnimation(
      parent: _textController,
      curve: Curves.easeIn,
    );

    _textScaleAnimation = Tween<double>(
      begin: 0.8,
      end: 1.0,
    ).animate(CurvedAnimation(
      parent: _textController,
      curve: Curves.elasticOut,
    ));

    // Generate confetti particles
    _generateConfetti();

    // Start animations
    _textController.forward();
    _confettiController.forward();

    // Auto-complete after animation
    Future.delayed(const Duration(milliseconds: 3500), () {
      if (mounted) {
        widget.onComplete();
      }
    });
  }

  void _generateConfetti() {
    for (int i = 0; i < 80; i++) {
      _particles.add(
        ConfettiParticle(
          color: _getRandomColor(),
          size: _random.nextDouble() * 8 + 4,
          startX: _random.nextDouble(),
          endX: _random.nextDouble() * 0.4 - 0.2,
          endY: _random.nextDouble() * 0.5 + 0.5,
          rotation: _random.nextDouble() * 2 * math.pi,
          rotationSpeed: (_random.nextDouble() - 0.5) * 4,
        ),
      );
    }
  }

  Color _getRandomColor() {
    final colors = [
      AppColors.primary,
      AppColors.accent,
      AppColors.secondary,
      AppColors.warning,
      AppColors.success,
      const Color(0xFFFFD700), // Gold
      const Color(0xFFFF69B4), // Pink
      const Color(0xFF00CED1), // Cyan
    ];
    return colors[_random.nextInt(colors.length)];
  }

  @override
  void dispose() {
    _confettiController.dispose();
    _textController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final size = MediaQuery.of(context).size;

    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [
              AppColors.primary,
              AppColors.primaryDark,
            ],
          ),
        ),
        child: Stack(
          children: [
            // Confetti animation
            AnimatedBuilder(
              animation: _confettiController,
              builder: (context, child) {
                return CustomPaint(
                  size: size,
                  painter: ConfettiPainter(
                    particles: _particles,
                    progress: _confettiController.value,
                  ),
                );
              },
            ),

            // Welcome text
            Center(
              child: FadeTransition(
                opacity: _textFadeAnimation,
                child: ScaleTransition(
                  scale: _textScaleAnimation,
                  child: Padding(
                    padding: const EdgeInsets.all(AppSpacing.xl),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        // Emoji or icon
                        Container(
                          width: 100,
                          height: 100,
                          decoration: BoxDecoration(
                            color: Colors.white.withValues(alpha: 0.2),
                            shape: BoxShape.circle,
                          ),
                          child: const Icon(
                            Icons.celebration,
                            size: 60,
                            color: Colors.white,
                          ),
                        ),
                        const SizedBox(height: AppSpacing.xxl),

                        // Welcome message
                        Text(
                          '¡Bienvenido!',
                          style: AppTypography.h1.copyWith(
                            color: Colors.white,
                            fontSize: 40,
                            fontWeight: FontWeight.w700,
                          ),
                          textAlign: TextAlign.center,
                        ),
                        const SizedBox(height: AppSpacing.md),

                        // User name
                        Text(
                          widget.userName,
                          style: AppTypography.h2.copyWith(
                            color: Colors.white.withValues(alpha: 0.9),
                            fontWeight: FontWeight.w600,
                          ),
                          textAlign: TextAlign.center,
                        ),
                        const SizedBox(height: AppSpacing.lg),

                        // Subtitle
                        Text(
                          'Estamos emocionados de tenerte\nen nuestra comunidad',
                          style: AppTypography.bodyLarge.copyWith(
                            color: Colors.white.withValues(alpha: 0.8),
                            height: 1.6,
                          ),
                          textAlign: TextAlign.center,
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Custom painter for confetti animation
class ConfettiPainter extends CustomPainter {
  final List<ConfettiParticle> particles;
  final double progress;

  ConfettiPainter({
    required this.particles,
    required this.progress,
  });

  @override
  void paint(Canvas canvas, Size size) {
    for (final particle in particles) {
      final paint = Paint()
        ..color = particle.color.withValues(
          alpha: 1.0 - (progress * 0.5),
        )
        ..style = PaintingStyle.fill;

      // Calculate position with physics
      final x = size.width * (particle.startX + particle.endX * progress);
      final y = size.height * particle.endY * progress * progress;

      // Apply rotation
      canvas.save();
      canvas.translate(x, y);
      canvas.rotate(particle.rotation + particle.rotationSpeed * progress);

      // Draw particle (rectangle)
      final rect = Rect.fromCenter(
        center: Offset.zero,
        width: particle.size,
        height: particle.size * 1.5,
      );
      canvas.drawRRect(
        RRect.fromRectAndRadius(rect, Radius.circular(particle.size * 0.2)),
        paint,
      );

      canvas.restore();
    }
  }

  @override
  bool shouldRepaint(ConfettiPainter oldDelegate) {
    return progress != oldDelegate.progress;
  }
}
