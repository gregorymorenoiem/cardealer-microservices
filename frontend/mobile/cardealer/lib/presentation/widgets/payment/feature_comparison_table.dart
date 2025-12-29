import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';
import '../../../domain/entities/payment.dart';

/// MF-003: Feature Comparison Table
///
/// Features:
/// - Tabla scrollable horizontal
/// - Headers sticky
/// - Iconos de check/cross
/// - Highlighting de diferencias
class FeatureComparisonTable extends StatelessWidget {
  final List<DealerPlan> plans;

  const FeatureComparisonTable({
    super.key,
    required this.plans,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.lg),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Section header
          const Text(
            'Comparación Detallada',
            style: TextStyle(
              fontFamily: 'Poppins',
              fontSize: 24,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: AppSpacing.xs),
          const Text(
            'Encuentra el plan perfecto para tu negocio',
            style: TextStyle(
              fontFamily: 'Inter',
              fontSize: 14,
              color: AppColors.textSecondary,
            ),
          ),
          const SizedBox(height: AppSpacing.xl),

          // Comparison table
          SingleChildScrollView(
            scrollDirection: Axis.horizontal,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Plan headers
                _buildPlanHeaders(),

                const SizedBox(height: AppSpacing.md),

                // Feature rows
                _buildFeatureRow(
                  'Publicaciones',
                  plans
                      .map((p) => p.maxListings == -1
                          ? 'Ilimitadas'
                          : '${p.maxListings}')
                      .toList(),
                ),
                _buildFeatureRow(
                  'Publicaciones Destacadas',
                  plans
                      .map((p) => p.maxFeaturedListings == -1
                          ? 'Ilimitadas'
                          : '${p.maxFeaturedListings}')
                      .toList(),
                ),
                _buildFeatureRow(
                  'Panel de Análisis',
                  plans.map((p) => p.hasAnalytics).toList(),
                  isBoolean: true,
                ),
                _buildFeatureRow(
                  'Sistema CRM',
                  plans.map((p) => p.hasCRM).toList(),
                  isBoolean: true,
                ),
                _buildFeatureRow(
                  'Soporte Prioritario',
                  plans.map((p) => p.hasPrioritySupport).toList(),
                  isBoolean: true,
                ),
                _buildFeatureRow(
                  'Estadísticas Avanzadas',
                  [false, true, true, true],
                  isBoolean: true,
                ),
                _buildFeatureRow(
                  'Reportes Personalizados',
                  [false, false, true, true],
                  isBoolean: true,
                ),
                _buildFeatureRow(
                  'API Access',
                  [false, false, false, true],
                  isBoolean: true,
                ),
                _buildFeatureRow(
                  'Gestor de Cuenta',
                  [false, false, false, true],
                  isBoolean: true,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildPlanHeaders() {
    return Row(
      children: [
        // Feature column header
        const SizedBox(
          width: 180,
          child: Text(
            'Características',
            style: TextStyle(
              fontFamily: 'Poppins',
              fontSize: 14,
              fontWeight: FontWeight.w600,
            ),
          ),
        ),

        // Plan headers
        ...plans.map((plan) => Container(
              width: 120,
              margin: const EdgeInsets.only(left: AppSpacing.sm),
              padding: const EdgeInsets.all(AppSpacing.sm),
              decoration: BoxDecoration(
                gradient: plan.isPopular
                    ? const LinearGradient(
                        colors: [AppColors.gold, AppColors.goldGradientEnd],
                      )
                    : null,
                color: plan.isPopular ? null : AppColors.surfaceVariant,
                borderRadius: BorderRadius.circular(12),
              ),
              child: Column(
                children: [
                  Text(
                    plan.name,
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      fontFamily: 'Poppins',
                      fontSize: 14,
                      fontWeight: FontWeight.bold,
                      color:
                          plan.isPopular ? Colors.white : AppColors.textPrimary,
                    ),
                  ),
                  if (plan.isPopular) ...[
                    const SizedBox(height: AppSpacing.xxs),
                    const Icon(
                      Icons.star,
                      color: Colors.white,
                      size: 16,
                    ),
                  ],
                ],
              ),
            )),
      ],
    );
  }

  Widget _buildFeatureRow(
    String featureName,
    List<dynamic> values, {
    bool isBoolean = false,
  }) {
    return Container(
      margin: const EdgeInsets.only(bottom: AppSpacing.xs),
      decoration: BoxDecoration(
        color: AppColors.backgroundPrimary,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        children: [
          // Feature name
          SizedBox(
            width: 180,
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.sm),
              child: Text(
                featureName,
                style: const TextStyle(
                  fontFamily: 'Inter',
                  fontSize: 13,
                ),
              ),
            ),
          ),

          // Feature values per plan
          ...List.generate(values.length, (index) {
            final value = values[index];
            return Container(
              width: 120,
              margin: const EdgeInsets.only(left: AppSpacing.sm),
              padding: const EdgeInsets.symmetric(vertical: AppSpacing.sm),
              child: Center(
                child: isBoolean
                    ? Icon(
                        value == true ? Icons.check_circle : Icons.cancel,
                        color:
                            value == true ? AppColors.success : AppColors.error,
                        size: 24,
                      )
                    : Text(
                        value.toString(),
                        textAlign: TextAlign.center,
                        style: const TextStyle(
                          fontFamily: 'Inter',
                          fontSize: 13,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
              ),
            );
          }),
        ],
      ),
    );
  }
}
