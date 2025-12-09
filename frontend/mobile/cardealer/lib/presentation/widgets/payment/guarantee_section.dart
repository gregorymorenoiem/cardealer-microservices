import 'package:flutter/material.dart';
import '../../../core/theme/colors.dart';
import '../../../core/theme/spacing.dart';

/// MF-006: Guarantee Section
///
/// Features:
/// - "30 días de garantía"
/// - Trust badges
/// - FAQ colapsable
class GuaranteeSection extends StatelessWidget {
  const GuaranteeSection({super.key});

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.all(AppSpacing.lg),
      padding: const EdgeInsets.all(AppSpacing.xl),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            AppColors.success.withValues(alpha: 0.05),
            AppColors.primary.withValues(alpha: 0.05),
          ],
        ),
        borderRadius: BorderRadius.circular(24),
        border: Border.all(
          color: AppColors.success.withValues(alpha: 0.3),
          width: 2,
        ),
      ),
      child: Column(
        children: [
          // Guarantee badge
          Container(
            padding: const EdgeInsets.all(AppSpacing.lg),
            decoration: BoxDecoration(
              color: Colors.white,
              shape: BoxShape.circle,
              boxShadow: [
                BoxShadow(
                  color: AppColors.success.withValues(alpha: 0.2),
                  blurRadius: 20,
                  spreadRadius: 5,
                ),
              ],
            ),
            child: const Icon(
              Icons.verified_user,
              color: AppColors.success,
              size: 48,
            ),
          ),

          const SizedBox(height: AppSpacing.lg),

          // Guarantee title
          const Text(
            'Garantía de 30 Días',
            style: TextStyle(
              fontFamily: 'Poppins',
              fontSize: 28,
              fontWeight: FontWeight.bold,
            ),
          ),

          const SizedBox(height: AppSpacing.sm),

          // Guarantee description
          const Text(
            'Si no estás 100% satisfecho, te devolvemos tu dinero. Sin preguntas.',
            textAlign: TextAlign.center,
            style: TextStyle(
              fontFamily: 'Inter',
              fontSize: 16,
              color: AppColors.textSecondary,
              height: 1.5,
            ),
          ),

          const SizedBox(height: AppSpacing.xl),

          // Trust badges
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceAround,
            children: [
              _buildTrustBadge(
                Icons.lock,
                'Pago\nSeguro',
              ),
              _buildTrustBadge(
                Icons.cancel,
                'Cancela\nCuando Quieras',
              ),
              _buildTrustBadge(
                Icons.support_agent,
                'Soporte\n24/7',
              ),
            ],
          ),

          const SizedBox(height: AppSpacing.xl),

          // FAQ Section
          const _FAQSection(),
        ],
      ),
    );
  }

  Widget _buildTrustBadge(IconData icon, String label) {
    return Column(
      children: [
        Container(
          padding: const EdgeInsets.all(AppSpacing.md),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(16),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withValues(alpha: 0.05),
                blurRadius: 10,
                offset: const Offset(0, 2),
              ),
            ],
          ),
          child: Icon(
            icon,
            color: AppColors.primary,
            size: 32,
          ),
        ),
        const SizedBox(height: AppSpacing.xs),
        Text(
          label,
          textAlign: TextAlign.center,
          style: const TextStyle(
            fontFamily: 'Inter',
            fontSize: 12,
            fontWeight: FontWeight.w500,
            height: 1.3,
          ),
        ),
      ],
    );
  }
}

class _FAQSection extends StatefulWidget {
  const _FAQSection();

  @override
  State<_FAQSection> createState() => _FAQSectionState();
}

class _FAQSectionState extends State<_FAQSection> {
  int? _expandedIndex;

  final List<Map<String, String>> _faqs = const [
    {
      'question': '¿Puedo cambiar de plan en cualquier momento?',
      'answer':
          'Sí, puedes actualizar o degradar tu plan en cualquier momento. Los cambios se aplican inmediatamente y solo pagas la diferencia prorrateada.',
    },
    {
      'question': '¿Qué métodos de pago aceptan?',
      'answer':
          'Aceptamos tarjetas de crédito y débito Visa, Mastercard, American Express, así como transferencias bancarias y pagos con PayPal.',
    },
    {
      'question': '¿Cómo funciona la garantía de 30 días?',
      'answer':
          'Si no estás satisfecho por cualquier razón en los primeros 30 días, contáctanos y te reembolsaremos el 100% de tu dinero sin hacer preguntas.',
    },
    {
      'question': '¿Hay contratos a largo plazo?',
      'answer':
          'No, todos nuestros planes son mes a mes o anuales sin compromiso. Puedes cancelar en cualquier momento sin penalización.',
    },
  ];

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Divider(),
        const SizedBox(height: AppSpacing.lg),
        const Text(
          'Preguntas Frecuentes',
          style: TextStyle(
            fontFamily: 'Poppins',
            fontSize: 20,
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: AppSpacing.md),
        ..._faqs.asMap().entries.map((entry) {
          final index = entry.key;
          final faq = entry.value;
          final isExpanded = _expandedIndex == index;

          return Container(
            margin: const EdgeInsets.only(bottom: AppSpacing.sm),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(
                color: isExpanded
                    ? AppColors.primary.withValues(alpha: 0.3)
                    : AppColors.border,
              ),
            ),
            child: Theme(
              data: Theme.of(context).copyWith(
                dividerColor: Colors.transparent,
              ),
              child: ExpansionTile(
                title: Text(
                  faq['question']!,
                  style: TextStyle(
                    fontFamily: 'Inter',
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                    color:
                        isExpanded ? AppColors.primary : AppColors.textPrimary,
                  ),
                ),
                trailing: Icon(
                  isExpanded ? Icons.remove_circle : Icons.add_circle,
                  color:
                      isExpanded ? AppColors.primary : AppColors.textSecondary,
                ),
                onExpansionChanged: (expanded) {
                  setState(() {
                    _expandedIndex = expanded ? index : null;
                  });
                },
                children: [
                  Padding(
                    padding: const EdgeInsets.fromLTRB(
                      AppSpacing.md,
                      0,
                      AppSpacing.md,
                      AppSpacing.md,
                    ),
                    child: Text(
                      faq['answer']!,
                      style: const TextStyle(
                        fontFamily: 'Inter',
                        fontSize: 13,
                        color: AppColors.textSecondary,
                        height: 1.5,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          );
        }),
      ],
    );
  }
}
