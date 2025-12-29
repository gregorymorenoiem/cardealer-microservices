import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';

/// MF-005: Testimonials Section
///
/// Features:
/// - Testimonios de dealers exitosos
/// - Foto, nombre, ventas
/// - Rating stars
class TestimonialsSection extends StatelessWidget {
  const TestimonialsSection({super.key});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.lg),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Section header
          const Text(
            'Lo que dicen nuestros dealers',
            style: TextStyle(
              fontFamily: 'Poppins',
              fontSize: 24,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: AppSpacing.xs),
          const Text(
            'Miles de dealers confían en nosotros',
            style: TextStyle(
              fontFamily: 'Inter',
              fontSize: 14,
              color: AppColors.textSecondary,
            ),
          ),
          const SizedBox(height: AppSpacing.xl),

          // Testimonials carousel
          SizedBox(
            height: 280,
            child: ListView(
              scrollDirection: Axis.horizontal,
              children: const [
                _TestimonialCard(
                  name: 'Carlos Méndez',
                  role: 'Dealer Premium',
                  dealership: 'Auto Express',
                  rating: 5,
                  testimonial:
                      'Desde que me uní al plan Pro, mis ventas aumentaron un 300%. La plataforma es increíble y el soporte es excelente.',
                  vehiclesSold: 142,
                  imageUrl: 'https://i.pravatar.cc/150?img=12',
                ),
                _TestimonialCard(
                  name: 'María González',
                  role: 'Dealer Enterprise',
                  dealership: 'Premium Motors',
                  rating: 5,
                  testimonial:
                      'El sistema CRM y las estadísticas avanzadas me ayudaron a optimizar mi inventario. ¡Altamente recomendado!',
                  vehiclesSold: 287,
                  imageUrl: 'https://i.pravatar.cc/150?img=47',
                ),
                _TestimonialCard(
                  name: 'Luis Ramírez',
                  role: 'Dealer Pro',
                  dealership: 'Autos del Valle',
                  rating: 5,
                  testimonial:
                      'La mejor inversión para mi negocio. Los clientes llegan más calificados y cierro ventas más rápido.',
                  vehiclesSold: 95,
                  imageUrl: 'https://i.pravatar.cc/150?img=33',
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _TestimonialCard extends StatelessWidget {
  final String name;
  final String role;
  final String dealership;
  final int rating;
  final String testimonial;
  final int vehiclesSold;
  final String imageUrl;

  const _TestimonialCard({
    required this.name,
    required this.role,
    required this.dealership,
    required this.rating,
    required this.testimonial,
    required this.vehiclesSold,
    required this.imageUrl,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 320,
      margin: const EdgeInsets.only(right: AppSpacing.md),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.08),
            blurRadius: 20,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(AppSpacing.lg),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header with avatar and info
            Row(
              children: [
                // Avatar
                Container(
                  width: 56,
                  height: 56,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    border: Border.all(
                      color: AppColors.primary.withValues(alpha: 0.2),
                      width: 2,
                    ),
                    image: DecorationImage(
                      image: NetworkImage(imageUrl),
                      fit: BoxFit.cover,
                    ),
                  ),
                ),

                const SizedBox(width: AppSpacing.sm),

                // Name and role
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        name,
                        style: const TextStyle(
                          fontFamily: 'Poppins',
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      Text(
                        role,
                        style: const TextStyle(
                          fontFamily: 'Inter',
                          fontSize: 12,
                          color: AppColors.textSecondary,
                        ),
                      ),
                      Text(
                        dealership,
                        style: const TextStyle(
                          fontFamily: 'Inter',
                          fontSize: 11,
                          color: AppColors.textTertiary,
                        ),
                      ),
                    ],
                  ),
                ),

                // Verified badge
                Container(
                  padding: const EdgeInsets.all(AppSpacing.xs),
                  decoration: BoxDecoration(
                    color: AppColors.success.withValues(alpha: 0.1),
                    shape: BoxShape.circle,
                  ),
                  child: const Icon(
                    Icons.verified,
                    color: AppColors.success,
                    size: 20,
                  ),
                ),
              ],
            ),

            const SizedBox(height: AppSpacing.md),

            // Rating stars
            Row(
              children: List.generate(
                5,
                (index) => Icon(
                  index < rating ? Icons.star : Icons.star_border,
                  color: AppColors.gold,
                  size: 18,
                ),
              ),
            ),

            const SizedBox(height: AppSpacing.sm),

            // Testimonial text
            Expanded(
              child: Text(
                '"$testimonial"',
                style: const TextStyle(
                  fontFamily: 'Inter',
                  fontSize: 14,
                  color: AppColors.textPrimary,
                  height: 1.5,
                ),
                maxLines: 5,
                overflow: TextOverflow.ellipsis,
              ),
            ),

            const SizedBox(height: AppSpacing.md),

            // Stats
            Container(
              padding: const EdgeInsets.all(AppSpacing.sm),
              decoration: BoxDecoration(
                color: AppColors.primary.withValues(alpha: 0.05),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(
                    Icons.local_shipping,
                    color: AppColors.primary,
                    size: 18,
                  ),
                  const SizedBox(width: AppSpacing.xs),
                  Text(
                    '$vehiclesSold vehículos vendidos',
                    style: const TextStyle(
                      fontFamily: 'Poppins',
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                      color: AppColors.primary,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
