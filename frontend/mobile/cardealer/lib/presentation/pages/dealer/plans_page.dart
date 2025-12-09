import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/di/injection.dart';
import '../../../domain/entities/payment.dart';
import '../../bloc/payment/payment_bloc.dart';

class PlansPage extends StatelessWidget {
  const PlansPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => getIt<PaymentBloc>()
        ..add(const LoadAvailablePlansEvent())
        ..add(const LoadCurrentSubscriptionEvent()),
      child: const _PlansPageContent(),
    );
  }
}

class _PlansPageContent extends StatefulWidget {
  const _PlansPageContent();

  @override
  State<_PlansPageContent> createState() => _PlansPageContentState();
}

class _PlansPageContentState extends State<_PlansPageContent> {
  BillingPeriod _selectedPeriod = BillingPeriod.monthly;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Planes y Suscripciones'),
        elevation: 0,
      ),
      body: BlocConsumer<PaymentBloc, PaymentState>(
        listener: (context, state) {
          if (state is PaymentError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red,
              ),
            );
          } else if (state is PaymentSuccess) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(
                content: Text('¡Suscripción actualizada exitosamente!'),
                backgroundColor: Colors.green,
              ),
            );
            context.read<PaymentBloc>().add(const LoadCurrentSubscriptionEvent());
          }
        },
        builder: (context, state) {
          if (state is PaymentLoading) {
            return const Center(child: CircularProgressIndicator());
          }

          final plans = _getPlansFromState(state);
          final currentSubscription = _getCurrentSubscription(state);

          if (plans.isEmpty) {
            return _buildEmptyState();
          }

          return CustomScrollView(
            slivers: [
              // Header with billing period toggle
              SliverToBoxAdapter(
                child: Column(
                  children: [
                    _buildHeader(),
                    const SizedBox(height: 16),
                    _buildBillingPeriodToggle(),
                    const SizedBox(height: 8),
                    _buildSavingsLabel(),
                    const SizedBox(height: 24),
                  ],
                ),
              ),

              // Plans grid
              SliverPadding(
                padding: const EdgeInsets.symmetric(horizontal: 16),
                sliver: SliverGrid(
                  gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: _getCrossAxisCount(context),
                    childAspectRatio: 0.7,
                    crossAxisSpacing: 16,
                    mainAxisSpacing: 16,
                  ),
                  delegate: SliverChildBuilderDelegate(
                    (context, index) {
                      final plan = plans[index];
                      final isCurrentPlan = currentSubscription != null &&
                          currentSubscription.plan.type == plan.type;
                      return _buildPlanCard(
                        context,
                        plan,
                        isCurrentPlan,
                        state is PaymentProcessing,
                      );
                    },
                    childCount: plans.length,
                  ),
                ),
              ),

              // Features comparison
              SliverToBoxAdapter(
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    children: [
                      const SizedBox(height: 32),
                      _buildFeaturesComparison(plans),
                      const SizedBox(height: 32),
                      _buildFAQ(),
                      const SizedBox(height: 32),
                    ],
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }

  List<DealerPlan> _getPlansFromState(PaymentState state) {
    if (state is PlansLoaded) {
      return state.plans;
    } else if (state is PaymentDashboardLoaded) {
      return state.plans;
    }
    return [];
  }

  Subscription? _getCurrentSubscription(PaymentState state) {
    if (state is SubscriptionLoaded) {
      return state.subscription;
    } else if (state is PaymentDashboardLoaded) {
      return state.subscription;
    }
    return null;
  }

  Widget _buildHeader() {
    return Container(
      padding: const EdgeInsets.all(24),
      child: Column(
        children: [
          Icon(
            Icons.workspace_premium,
            size: 64,
            color: Theme.of(context).primaryColor,
          ),
          const SizedBox(height: 16),
          Text(
            'Elige el plan perfecto para tu negocio',
            style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 8),
          Text(
            'Comienza gratis, actualiza cuando quieras',
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: Colors.grey[600],
                ),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }

  Widget _buildBillingPeriodToggle() {
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 16),
      decoration: BoxDecoration(
        color: Colors.grey[200],
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          Expanded(
            child: _buildPeriodButton(
              'Mensual',
              BillingPeriod.monthly,
              _selectedPeriod == BillingPeriod.monthly,
            ),
          ),
          Expanded(
            child: _buildPeriodButton(
              'Anual',
              BillingPeriod.yearly,
              _selectedPeriod == BillingPeriod.yearly,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildPeriodButton(
      String label, BillingPeriod period, bool isSelected) {
    return GestureDetector(
      onTap: () {
        setState(() {
          _selectedPeriod = period;
        });
      },
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 12),
        decoration: BoxDecoration(
          color: isSelected ? Colors.white : Colors.transparent,
          borderRadius: BorderRadius.circular(12),
          boxShadow: isSelected
              ? [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.1),
                    blurRadius: 8,
                    offset: const Offset(0, 2),
                  ),
                ]
              : null,
        ),
        child: Text(
          label,
          textAlign: TextAlign.center,
          style: TextStyle(
            fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
            color:
                isSelected ? Theme.of(context).primaryColor : Colors.grey[600],
          ),
        ),
      ),
    );
  }

  Widget _buildSavingsLabel() {
    if (_selectedPeriod == BillingPeriod.yearly) {
      return Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
        decoration: BoxDecoration(
          color: Colors.green[100],
          borderRadius: BorderRadius.circular(20),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.savings, size: 16, color: Colors.green[700]),
            const SizedBox(width: 4),
            Text(
              'Ahorra hasta 20% con plan anual',
              style: TextStyle(
                color: Colors.green[700],
                fontWeight: FontWeight.bold,
                fontSize: 12,
              ),
            ),
          ],
        ),
      );
    }
    return const SizedBox.shrink();
  }

  Widget _buildPlanCard(
    BuildContext context,
    DealerPlan plan,
    bool isCurrentPlan,
    bool isProcessing,
  ) {
    final isPopular = plan.type == DealerPlanType.pro;
    final price = _selectedPeriod == BillingPeriod.monthly
        ? plan.priceMonthly
        : plan.priceYearly;
    final yearlyDiscount = plan.priceMonthly > 0
        ? ((1 - (plan.priceYearly / 12) / plan.priceMonthly) * 100).toInt()
        : 0;
    return Container(
      decoration: BoxDecoration(
        border: Border.all(
          color: isCurrentPlan
              ? Theme.of(context).primaryColor
              : isPopular
                  ? Colors.orange
                  : Colors.grey[300]!,
          width: isCurrentPlan || isPopular ? 2 : 1,
        ),
        borderRadius: BorderRadius.circular(16),
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Stack(
        children: [
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Header
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: isCurrentPlan
                      ? Theme.of(context).primaryColor.withValues(alpha: 0.1)
                      : isPopular
                          ? Colors.orange[50]
                          : Colors.grey[50],
                  borderRadius: const BorderRadius.only(
                    topLeft: Radius.circular(16),
                    topRight: Radius.circular(16),
                  ),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      plan.name,
                      style: const TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      plan.description,
                      style: TextStyle(
                        fontSize: 12,
                        color: Colors.grey[600],
                      ),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ],
                ),
              ),

              // Price
              Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    if (plan.priceMonthly == 0)
                      const Text(
                        'Gratis',
                        style: TextStyle(
                          fontSize: 32,
                          fontWeight: FontWeight.bold,
                        ),
                      )
                    else
                      Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Text(
                            '\$',
                            style: TextStyle(
                              fontSize: 20,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          Text(
                            price.toStringAsFixed(0),
                            style: const TextStyle(
                              fontSize: 36,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ],
                      ),
                    if (plan.priceMonthly > 0)
                      Text(
                        _selectedPeriod == BillingPeriod.monthly
                            ? '/mes'
                            : '/año (ahorra $yearlyDiscount%)',
                        style: TextStyle(
                          fontSize: 12,
                          color: Colors.grey[600],
                        ),
                      ),
                  ],
                ),
              ),

              // Features
              Expanded(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      ...plan.features.take(5).map((feature) => Padding(
                            padding: const EdgeInsets.only(bottom: 8),
                            child: Row(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Icon(
                                  Icons.check_circle,
                                  size: 16,
                                  color: Theme.of(context).primaryColor,
                                ),
                                const SizedBox(width: 8),
                                Expanded(
                                  child: Text(
                                    feature,
                                    style: const TextStyle(fontSize: 12),
                                    maxLines: 2,
                                    overflow: TextOverflow.ellipsis,
                                  ),
                                ),
                              ],
                            ),
                          )),
                    ],
                  ),
                ),
              ),

              // CTA Button
              Padding(
                padding: const EdgeInsets.all(16),
                child: SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: isCurrentPlan || isProcessing
                        ? null
                        : () => _subscribeToPlan(context, plan),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: isCurrentPlan
                          ? Colors.grey[300]
                          : isPopular
                              ? Colors.orange
                              : Theme.of(context).primaryColor,
                      foregroundColor:
                          isCurrentPlan ? Colors.grey[600] : Colors.white,
                      padding: const EdgeInsets.symmetric(vertical: 12),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8),
                      ),
                    ),
                    child: Text(
                      isCurrentPlan
                          ? 'Plan Actual'
                          : plan.priceMonthly == 0
                              ? 'Comenzar Gratis'
                              : 'Suscribirse',
                      style: const TextStyle(fontWeight: FontWeight.bold),
                    ),
                  ),
                ),
              ),
            ],
          ),

          // Badges
          if (isPopular)
            Positioned(
              top: 8,
              right: 8,
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(
                  color: Colors.orange,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: const Text(
                  'POPULAR',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 10,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ),
          if (isCurrentPlan)
            Positioned(
              top: 8,
              right: 8,
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(
                  color: Theme.of(context).primaryColor,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: const Text(
                  'ACTIVO',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 10,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildFeaturesComparison(List<DealerPlan> plans) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Comparación de características',
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 16),
            Text(
              'Todos los planes incluyen:',
              style: Theme.of(context).textTheme.titleMedium,
            ),
            const SizedBox(height: 8),
            ...[
              'Publicación de vehículos',
              'Gestión de leads',
              'Sistema de mensajería',
              'Estadísticas básicas',
              'Soporte por email',
            ].map((feature) => Padding(
                  padding: const EdgeInsets.symmetric(vertical: 4),
                  child: Row(
                    children: [
                      Icon(Icons.check, size: 20, color: Colors.green[600]),
                      const SizedBox(width: 8),
                      Text(feature),
                    ],
                  ),
                )),
          ],
        ),
      ),
    );
  }

  Widget _buildFAQ() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Preguntas frecuentes',
          style: Theme.of(context).textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.bold,
              ),
        ),
        const SizedBox(height: 16),
        _buildFAQItem(
          '¿Puedo cambiar de plan en cualquier momento?',
          'Sí, puedes actualizar o degradar tu plan cuando lo desees. Los cambios se aplican inmediatamente.',
        ),
        _buildFAQItem(
          '¿Qué métodos de pago aceptan?',
          'Aceptamos todas las tarjetas de crédito y débito principales (Visa, Mastercard, American Express).',
        ),
        _buildFAQItem(
          '¿Puedo cancelar mi suscripción?',
          'Sí, puedes cancelar en cualquier momento. Tu plan seguirá activo hasta el final del período de facturación.',
        ),
        _buildFAQItem(
          '¿Ofrecen reembolsos?',
          'Ofrecemos reembolso completo dentro de los primeros 30 días si no estás satisfecho.',
        ),
      ],
    );
  }

  Widget _buildFAQItem(String question, String answer) {
    return ExpansionTile(
      title: Text(
        question,
        style: const TextStyle(fontWeight: FontWeight.w600),
      ),
      children: [
        Padding(
          padding: const EdgeInsets.all(16),
          child: Text(
            answer,
            style: TextStyle(color: Colors.grey[600]),
          ),
        ),
      ],
    );
  }

  Widget _buildEmptyState() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.error_outline, size: 64, color: Colors.grey[400]),
          const SizedBox(height: 16),
          Text(
            'No hay planes disponibles',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 8),
          TextButton(
            onPressed: () {
              context.read<PaymentBloc>().add(const LoadAvailablePlansEvent());
            },
            child: const Text('Reintentar'),
          ),
        ],
      ),
    );
  }

  int _getCrossAxisCount(BuildContext context) {
    final width = MediaQuery.of(context).size.width;
    if (width > 1200) return 4;
    if (width > 800) return 3;
    if (width > 600) return 2;
    return 1;
  }

  void _subscribeToPlan(BuildContext context, DealerPlan plan) {
    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: Text('Suscribirse a ${plan.name}'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('¿Confirmas que deseas suscribirte al plan ${plan.name}?'),
            const SizedBox(height: 16),
            Text(
              'Precio: \$${_selectedPeriod == BillingPeriod.monthly ? plan.priceMonthly : plan.priceYearly}/${_selectedPeriod == BillingPeriod.monthly ? "mes" : "año"}',
              style: const TextStyle(fontWeight: FontWeight.bold),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogContext),
            child: const Text('Cancelar'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(dialogContext);
              context.read<PaymentBloc>().add(
                    SubscribeToPlanEvent(
                      planId: plan.id,
                      billingPeriod: _selectedPeriod,
                      paymentMethodId: 'pm_mock_default',
                    ),
                  );
            },
            child: const Text('Confirmar'),
          ),
        ],
      ),
    );
  }
}
