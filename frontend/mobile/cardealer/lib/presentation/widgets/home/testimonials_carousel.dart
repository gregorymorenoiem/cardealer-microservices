import 'package:flutter/material.dart';
import 'dart:async';

/// Testimonial Model
class Testimonial {
  final String name;
  final String role;
  final String imageUrl;
  final String quote;
  final double rating;
  final String location;

  const Testimonial({
    required this.name,
    required this.role,
    required this.imageUrl,
    required this.quote,
    required this.rating,
    required this.location,
  });
}

/// Testimonials Carousel - HR-009
/// Auto-rotating customer testimonials with ratings
class TestimonialsCarousel extends StatefulWidget {
  final List<Testimonial> testimonials;
  final VoidCallback? onSeeAllReviews;

  const TestimonialsCarousel({
    super.key,
    required this.testimonials,
    this.onSeeAllReviews,
  });

  @override
  State<TestimonialsCarousel> createState() => _TestimonialsCarouselState();
}

class _TestimonialsCarouselState extends State<TestimonialsCarousel> {
  late PageController _pageController;
  late Timer _autoPlayTimer;
  int _currentPage = 0;

  // Default testimonials if none provided
  static const List<Testimonial> _defaultTestimonials = [
    Testimonial(
      name: 'María González',
      role: 'Comprador Verificado',
      imageUrl: 'https://i.pravatar.cc/150?img=1',
      quote:
          'Excelente experiencia! Encontré el auto perfecto para mi familia. El proceso fue rápido y transparente.',
      rating: 5.0,
      location: 'Ciudad de México',
    ),
    Testimonial(
      name: 'Carlos Ramírez',
      role: 'Vendedor',
      imageUrl: 'https://i.pravatar.cc/150?img=12',
      quote:
          'Vendí mi auto en menos de una semana. La plataforma es muy fácil de usar y el soporte fue excepcional.',
      rating: 5.0,
      location: 'Guadalajara',
    ),
    Testimonial(
      name: 'Ana Martínez',
      role: 'Comprador Verificado',
      imageUrl: 'https://i.pravatar.cc/150?img=5',
      quote:
          'Me encantó la variedad de opciones. Compré un híbrido a excelente precio. Totalmente recomendado!',
      rating: 4.5,
      location: 'Monterrey',
    ),
    Testimonial(
      name: 'Luis Hernández',
      role: 'Comprador Verificado',
      imageUrl: 'https://i.pravatar.cc/150?img=13',
      quote:
          'La mejor plataforma para comprar autos usados. Todo el proceso fue seguro y confiable.',
      rating: 5.0,
      location: 'Puebla',
    ),
  ];

  @override
  void initState() {
    super.initState();
    final testimonials = widget.testimonials.isEmpty
        ? _defaultTestimonials
        : widget.testimonials;

    _pageController = PageController(viewportFraction: 0.9);
    _startAutoPlay(testimonials.length);
  }

  void _startAutoPlay(int itemCount) {
    _autoPlayTimer = Timer.periodic(const Duration(seconds: 8), (timer) {
      if (mounted) {
        _currentPage = (_currentPage + 1) % itemCount;
        _pageController.animateToPage(
          _currentPage,
          duration: const Duration(milliseconds: 800),
          curve: Curves.easeInOut,
        );
      }
    });
  }

  @override
  void dispose() {
    _autoPlayTimer.cancel();
    _pageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final testimonials = widget.testimonials.isEmpty
        ? _defaultTestimonials
        : widget.testimonials;

    return Container(
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            const Color(0xFF001F54).withValues(alpha: 0.03),
            const Color(0xFF0A4B8F).withValues(alpha: 0.05),
          ],
        ),
      ),
      padding: const EdgeInsets.symmetric(vertical: 32),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Section Header
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16.0),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'What Our Customers Say',
                      style:
                          Theme.of(context).textTheme.headlineSmall?.copyWith(
                                fontWeight: FontWeight.bold,
                              ),
                    ),
                    const SizedBox(height: 4),
                    Row(
                      children: [
                        ...List.generate(
                          5,
                          (index) => const Icon(
                            Icons.star,
                            color: Colors.amber,
                            size: 16,
                          ),
                        ),
                        const SizedBox(width: 8),
                        Text(
                          '4.9 out of 5',
                          style:
                              Theme.of(context).textTheme.bodyMedium?.copyWith(
                                    color: Colors.grey.shade600,
                                    fontSize: 13,
                                  ),
                        ),
                      ],
                    ),
                  ],
                ),
                if (widget.onSeeAllReviews != null)
                  TextButton(
                    onPressed: widget.onSeeAllReviews,
                    child: const Row(
                      children: [
                        Text('See All'),
                        SizedBox(width: 4),
                        Icon(Icons.arrow_forward, size: 16),
                      ],
                    ),
                  ),
              ],
            ),
          ),
          const SizedBox(height: 24),
          // Carousel
          SizedBox(
            height: 280,
            child: PageView.builder(
              controller: _pageController,
              onPageChanged: (index) {
                setState(() {
                  _currentPage = index;
                });
              },
              itemCount: testimonials.length,
              itemBuilder: (context, index) {
                return Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 8),
                  child: _TestimonialCard(
                    testimonial: testimonials[index],
                  ),
                );
              },
            ),
          ),
          const SizedBox(height: 16),
          // Page Indicators
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: List.generate(
              testimonials.length,
              (index) => AnimatedContainer(
                duration: const Duration(milliseconds: 300),
                margin: const EdgeInsets.symmetric(horizontal: 4),
                width: _currentPage == index ? 24 : 8,
                height: 8,
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(4),
                  gradient: _currentPage == index
                      ? const LinearGradient(
                          colors: [Color(0xFF001F54), Color(0xFF0A4B8F)],
                        )
                      : null,
                  color: _currentPage == index ? null : Colors.grey.shade300,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _TestimonialCard extends StatefulWidget {
  final Testimonial testimonial;

  const _TestimonialCard({
    required this.testimonial,
  });

  @override
  State<_TestimonialCard> createState() => _TestimonialCardState();
}

class _TestimonialCardState extends State<_TestimonialCard>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _fadeAnimation;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      duration: const Duration(milliseconds: 800),
      vsync: this,
    );
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
    return FadeTransition(
      opacity: _fadeAnimation,
      child: Container(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(20),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.08),
              blurRadius: 16,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header: Avatar + Name + Rating
            Row(
              children: [
                // Avatar with glassmorphism border
                Container(
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    border: Border.all(
                      color: const Color(0xFFFFD700),
                      width: 2,
                    ),
                  ),
                  child: ClipOval(
                    child: Image.network(
                      widget.testimonial.imageUrl,
                      width: 60,
                      height: 60,
                      fit: BoxFit.cover,
                      errorBuilder: (context, error, stackTrace) {
                        return Container(
                          width: 60,
                          height: 60,
                          color: Colors.grey.shade300,
                          child: Icon(
                            Icons.person,
                            size: 32,
                            color: Colors.grey.shade600,
                          ),
                        );
                      },
                    ),
                  ),
                ),
                const SizedBox(width: 12),
                // Name, role, location
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        widget.testimonial.name,
                        style: const TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      Text(
                        widget.testimonial.role,
                        style: TextStyle(
                          fontSize: 12,
                          color: Colors.grey.shade600,
                        ),
                      ),
                      Row(
                        children: [
                          Icon(
                            Icons.location_on,
                            size: 12,
                            color: Colors.grey.shade500,
                          ),
                          const SizedBox(width: 2),
                          Text(
                            widget.testimonial.location,
                            style: TextStyle(
                              fontSize: 11,
                              color: Colors.grey.shade500,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
                // Verified Badge
                Container(
                  padding: const EdgeInsets.all(6),
                  decoration: BoxDecoration(
                    color: Colors.green.withValues(alpha: 0.1),
                    shape: BoxShape.circle,
                  ),
                  child: const Icon(
                    Icons.verified,
                    color: Colors.green,
                    size: 20,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            // Star Rating
            Row(
              children: List.generate(
                5,
                (index) => Icon(
                  index < widget.testimonial.rating.floor()
                      ? Icons.star
                      : (index < widget.testimonial.rating
                          ? Icons.star_half
                          : Icons.star_border),
                  color: Colors.amber,
                  size: 20,
                ),
              ),
            ),
            const SizedBox(height: 12),
            // Quote Icon
            const Icon(
              Icons.format_quote,
              color: Color(0xFF001F54),
              size: 32,
            ),
            const SizedBox(height: 8),
            // Quote Text
            Expanded(
              child: SingleChildScrollView(
                child: Text(
                  widget.testimonial.quote,
                  style: TextStyle(
                    fontSize: 14,
                    height: 1.5,
                    color: Colors.grey.shade800,
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
