import 'package:flutter/material.dart';
import 'dart:async';

/// Stat Model
class PlatformStat {
  final String value;
  final String label;
  final IconData icon;
  final int numericValue;

  const PlatformStat({
    required this.value,
    required this.label,
    required this.icon,
    required this.numericValue,
  });
}

/// Stats Section - HR-010
/// Animated statistics with counters
class StatsSection extends StatefulWidget {
  final List<PlatformStat>? customStats;

  const StatsSection({
    super.key,
    this.customStats,
  });

  // Default platform statistics
  static const List<PlatformStat> defaultStats = [
    PlatformStat(
      value: '15K+',
      label: 'Cars Available',
      icon: Icons.directions_car,
      numericValue: 15000,
    ),
    PlatformStat(
      value: '8K+',
      label: 'Happy Customers',
      icon: Icons.people,
      numericValue: 8000,
    ),
    PlatformStat(
      value: '200+',
      label: 'Trusted Dealers',
      icon: Icons.store,
      numericValue: 200,
    ),
    PlatformStat(
      value: '50+',
      label: 'Cities Covered',
      icon: Icons.location_city,
      numericValue: 50,
    ),
  ];

  @override
  State<StatsSection> createState() => _StatsSectionState();
}

class _StatsSectionState extends State<StatsSection> {
  bool _isVisible = false;

  @override
  void initState() {
    super.initState();
    // Trigger animation after widget is built
    Future.delayed(const Duration(milliseconds: 300), () {
      if (mounted) {
        setState(() {
          _isVisible = true;
        });
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    final stats = widget.customStats ?? StatsSection.defaultStats;

    return Container(
      width: double.infinity,
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            const Color(0xFF001F54),
            const Color(0xFF0A4B8F),
            const Color(0xFF001F54).withValues(alpha: 0.9),
          ],
        ),
      ),
      padding: const EdgeInsets.symmetric(vertical: 48, horizontal: 16),
      child: Column(
        children: [
          // Section Title
          Text(
            'Our Impact',
            style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: Colors.white,
                ),
          ),
          const SizedBox(height: 8),
          Text(
            'Trusted by thousands across the country',
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: Colors.white.withValues(alpha: 0.8),
                ),
          ),
          const SizedBox(height: 32),
          // Stats Grid
          LayoutBuilder(
            builder: (context, constraints) {
              final isWide = constraints.maxWidth > 600;
              return Wrap(
                spacing: 16,
                runSpacing: 16,
                alignment: WrapAlignment.center,
                children: stats.asMap().entries.map((entry) {
                  final index = entry.key;
                  final stat = entry.value;
                  return SizedBox(
                    width: isWide
                        ? (constraints.maxWidth / 4) - 24
                        : (constraints.maxWidth / 2) - 24,
                    child: _StatCard(
                      stat: stat,
                      isVisible: _isVisible,
                      delay: Duration(milliseconds: index * 150),
                    ),
                  );
                }).toList(),
              );
            },
          ),
        ],
      ),
    );
  }
}

class _StatCard extends StatefulWidget {
  final PlatformStat stat;
  final bool isVisible;
  final Duration delay;

  const _StatCard({
    required this.stat,
    required this.isVisible,
    required this.delay,
  });

  @override
  State<_StatCard> createState() => _StatCardState();
}

class _StatCardState extends State<_StatCard>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _scaleAnimation;
  late Animation<double> _fadeAnimation;
  int _currentValue = 0;
  Timer? _counterTimer;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 600),
      vsync: this,
    );

    _scaleAnimation = Tween<double>(begin: 0.8, end: 1.0).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeOutBack),
    );

    _fadeAnimation = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeIn),
    );
  }

  @override
  void didUpdateWidget(_StatCard oldWidget) {
    super.didUpdateWidget(oldWidget);
    if (widget.isVisible && !oldWidget.isVisible) {
      _startAnimation();
    }
  }

  void _startAnimation() {
    Future.delayed(widget.delay, () {
      if (mounted) {
        _controller.forward();
        _startCounter();
      }
    });
  }

  void _startCounter() {
    const duration = Duration(milliseconds: 2000);
    const steps = 50;
    final increment = widget.stat.numericValue / steps;
    var step = 0;

    _counterTimer = Timer.periodic(
      Duration(milliseconds: duration.inMilliseconds ~/ steps),
      (timer) {
        if (mounted && step < steps) {
          setState(() {
            _currentValue = (increment * step).round();
          });
          step++;
        } else {
          setState(() {
            _currentValue = widget.stat.numericValue;
          });
          timer.cancel();
        }
      },
    );
  }

  @override
  void dispose() {
    _controller.dispose();
    _counterTimer?.cancel();
    super.dispose();
  }

  String _formatValue() {
    if (_currentValue >= 1000) {
      final k = (_currentValue / 1000).toStringAsFixed(1);
      return '${k}K+';
    }
    return '$_currentValue+';
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _controller,
      builder: (context, child) {
        return Transform.scale(
          scale: _scaleAnimation.value,
          child: Opacity(
            opacity: _fadeAnimation.value,
            child: child,
          ),
        );
      },
      child: Container(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: Colors.white.withValues(alpha: 0.1),
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: Colors.white.withValues(alpha: 0.2),
            width: 1,
          ),
        ),
        child: Column(
          children: [
            // Icon
            Container(
              width: 60,
              height: 60,
              decoration: BoxDecoration(
                gradient: const LinearGradient(
                  colors: [Color(0xFFFFD700), Color(0xFFFFAA00)],
                ),
                shape: BoxShape.circle,
                boxShadow: [
                  BoxShadow(
                    color: const Color(0xFFFFD700).withValues(alpha: 0.3),
                    blurRadius: 12,
                    spreadRadius: 2,
                  ),
                ],
              ),
              child: Icon(
                widget.stat.icon,
                color: Colors.white,
                size: 30,
              ),
            ),
            const SizedBox(height: 16),
            // Animated Counter
            Text(
              _formatValue(),
              style: const TextStyle(
                fontSize: 32,
                fontWeight: FontWeight.bold,
                color: Colors.white,
              ),
            ),
            const SizedBox(height: 4),
            // Label
            Text(
              widget.stat.label,
              textAlign: TextAlign.center,
              style: TextStyle(
                fontSize: 14,
                color: Colors.white.withValues(alpha: 0.9),
                fontWeight: FontWeight.w500,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
