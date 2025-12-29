import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/di/injection.dart';
import '../../../domain/entities/payment.dart';
import '../../bloc/payment/payment_bloc.dart';
import '../../widgets/payment/plan_card.dart';

/// Page displaying available subscription plans
class PlansPage extends StatelessWidget {
  const PlansPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) =>
          getIt<PaymentBloc>()..add(const LoadAvailablePlansEvent()),
      child: const _PlansPageContent(),
    );
  }
}

class _PlansPageContent extends StatelessWidget {
  const _PlansPageContent();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Subscription Plans'),
        centerTitle: true,
      ),
      body: BlocConsumer<PaymentBloc, PaymentState>(
        listener: (context, state) {
          if (state is PaymentSuccess) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.green,
              ),
            );
            Navigator.of(context).pop();
          } else if (state is PaymentError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: Colors.red,
              ),
            );
          }
        },
        builder: (context, state) {
          if (state is PaymentLoading) {
            return const Center(child: CircularProgressIndicator());
          }

          if (state is PlansLoaded) {
            return _buildPlansContent(context, state);
          }

          if (state is PaymentError) {
            return _buildErrorState(context, state.message);
          }

          return const SizedBox.shrink();
        },
      ),
    );
  }

  Widget _buildPlansContent(BuildContext context, PlansLoaded state) {
    return Column(
      children: [
        // Billing period toggle
        _buildBillingPeriodToggle(context, state.selectedPeriod),

        // Plans list
        Expanded(
          child: ListView(
            padding: const EdgeInsets.all(16),
            children: [
              // Savings banner for yearly
              if (state.selectedPeriod == BillingPeriod.yearly)
                _buildSavingsBanner(state.plans),

              const SizedBox(height: 16),

              // Plan cards
              ...state.plans.map((plan) => Padding(
                    padding: const EdgeInsets.only(bottom: 16),
                    child: PlanCard(
                      plan: plan,
                      billingPeriod: state.selectedPeriod,
                      onSelect: () => _handlePlanSelection(
                          context, plan, state.selectedPeriod),
                    ),
                  )),

              const SizedBox(height: 16),

              // Features comparison
              _buildFeaturesComparison(state.plans),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildBillingPeriodToggle(
      BuildContext context, BillingPeriod currentPeriod) {
    return Container(
      margin: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.grey[200],
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          Expanded(
            child: _buildPeriodButton(
              context,
              'Monthly',
              BillingPeriod.monthly,
              currentPeriod == BillingPeriod.monthly,
            ),
          ),
          Expanded(
            child: _buildPeriodButton(
              context,
              'Yearly (Save up to 17%)',
              BillingPeriod.yearly,
              currentPeriod == BillingPeriod.yearly,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildPeriodButton(
    BuildContext context,
    String label,
    BillingPeriod period,
    bool isSelected,
  ) {
    return GestureDetector(
      onTap: () {
        context.read<PaymentBloc>().add(ChangeBillingPeriodEvent(period));
      },
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 8),
        decoration: BoxDecoration(
          color:
              isSelected ? Theme.of(context).primaryColor : Colors.transparent,
          borderRadius: BorderRadius.circular(12),
        ),
        child: Text(
          label,
          textAlign: TextAlign.center,
          style: TextStyle(
            color: isSelected ? Colors.white : Colors.black87,
            fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
            fontSize: 13,
          ),
        ),
      ),
    );
  }

  Widget _buildSavingsBanner(List<DealerPlan> plans) {
    final maxSavings = plans
        .map((p) => p.yearlySavingsPercent)
        .reduce((a, b) => a > b ? a : b);

    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          colors: [Colors.green.shade400, Colors.green.shade600],
        ),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          const Icon(Icons.savings_outlined, color: Colors.white),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              'Save up to ${maxSavings.toStringAsFixed(0)}% with yearly billing',
              style: const TextStyle(
                color: Colors.white,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildFeaturesComparison(List<DealerPlan> plans) {
    return Card(
      elevation: 2,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Feature Comparison',
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 16),
            _buildFeatureRow(
                'Max Listings',
                plans
                    .map((p) =>
                        p.maxListings == -1 ? 'Unlimited' : '${p.maxListings}')
                    .toList()),
            _buildFeatureRow('Featured Listings',
                plans.map((p) => '${p.maxFeaturedListings}').toList()),
            _buildFeatureRow('Analytics',
                plans.map((p) => p.hasAnalytics ? '✓' : '—').toList()),
            _buildFeatureRow(
                'CRM', plans.map((p) => p.hasCRM ? '✓' : '—').toList()),
            _buildFeatureRow('Priority Support',
                plans.map((p) => p.hasPrioritySupport ? '✓' : '—').toList()),
          ],
        ),
      ),
    );
  }

  Widget _buildFeatureRow(String feature, List<String> values) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        children: [
          Expanded(
            flex: 2,
            child: Text(
              feature,
              style: const TextStyle(fontWeight: FontWeight.w500),
            ),
          ),
          ...values.map((value) => Expanded(
                child: Text(
                  value,
                  textAlign: TextAlign.center,
                  style: TextStyle(
                    color: value == '✓' ? Colors.green : Colors.grey[600],
                    fontWeight:
                        value == '✓' ? FontWeight.bold : FontWeight.normal,
                  ),
                ),
              )),
        ],
      ),
    );
  }

  Widget _buildErrorState(BuildContext context, String message) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.error_outline, size: 64, color: Colors.red),
          const SizedBox(height: 16),
          Text(
            message,
            textAlign: TextAlign.center,
            style: const TextStyle(fontSize: 16),
          ),
          const SizedBox(height: 24),
          ElevatedButton.icon(
            onPressed: () {
              context.read<PaymentBloc>().add(const LoadAvailablePlansEvent());
            },
            icon: const Icon(Icons.refresh),
            label: const Text('Retry'),
          ),
        ],
      ),
    );
  }

  void _handlePlanSelection(
    BuildContext context,
    DealerPlan plan,
    BillingPeriod period,
  ) {
    if (plan.isCurrentPlan) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('This is your current plan'),
          backgroundColor: Colors.blue,
        ),
      );
      return;
    }

    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: Text('Subscribe to ${plan.name}?'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              '${plan.getFormattedPrice(period)} / ${period == BillingPeriod.monthly ? "month" : "year"}',
              style: const TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            Text(plan.description),
            const SizedBox(height: 16),
            const Text(
              'Features:',
              style: TextStyle(fontWeight: FontWeight.bold),
            ),
            ...plan.features.map((feature) => Padding(
                  padding: const EdgeInsets.only(left: 8, top: 4),
                  child: Row(
                    children: [
                      const Icon(Icons.check, size: 16, color: Colors.green),
                      const SizedBox(width: 8),
                      Expanded(child: Text(feature)),
                    ],
                  ),
                )),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.of(dialogContext).pop();
              // In a real app, would navigate to payment method selection
              context.read<PaymentBloc>().add(
                    SubscribeToPlanEvent(
                      planId: plan.id,
                      billingPeriod: period,
                      paymentMethodId: 'pm_default', // Mock
                    ),
                  );
            },
            child: const Text('Subscribe'),
          ),
        ],
      ),
    );
  }
}
