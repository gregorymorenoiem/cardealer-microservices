import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';
import '../../../core/di/injection.dart';
import '../../../domain/entities/payment.dart';
import '../../bloc/payment/payment_bloc.dart';

class BillingDashboardPage extends StatelessWidget {
  const BillingDashboardPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => getIt<PaymentBloc>()
        ..add(const LoadCurrentSubscriptionEvent())
        ..add(const LoadPaymentHistoryEvent())
        ..add(const LoadUsageStatsEvent()),
      child: const _BillingDashboardPageContent(),
    );
  }
}

class _BillingDashboardPageContent extends StatefulWidget {
  const _BillingDashboardPageContent();

  @override
  State<_BillingDashboardPageContent> createState() =>
      _BillingDashboardPageContentState();
}

class _BillingDashboardPageContentState
    extends State<_BillingDashboardPageContent>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  String _searchQuery = '';
  PaymentStatus? _filterStatus;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Facturación'),
        elevation: 0,
        bottom: TabBar(
          controller: _tabController,
          tabs: const [
            Tab(icon: Icon(Icons.dashboard), text: 'Resumen'),
            Tab(icon: Icon(Icons.history), text: 'Historial'),
          ],
        ),
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
          }
        },
        builder: (context, state) {
          if (state is PaymentLoading) {
            return const Center(child: CircularProgressIndicator());
          }

          return TabBarView(
            controller: _tabController,
            children: [
              _buildDashboardTab(context, state),
              _buildHistoryTab(context, state),
            ],
          );
        },
      ),
    );
  }

  Widget _buildDashboardTab(BuildContext context, PaymentState state) {
    final subscription = _getCurrentSubscription(state);
    final usageStats = _getUsageStats(state);

    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Current subscription card
          if (subscription != null) ...[
            _buildSubscriptionCard(context, subscription),
            const SizedBox(height: 24),
          ],

          // Usage stats
          if (usageStats != null) ...[
            Text(
              'Uso actual',
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 16),
            _buildUsageStatsCard(context, usageStats),
            const SizedBox(height: 24),
          ],

          // Quick actions
          Text(
            'Acciones rápidas',
            style: Theme.of(context).textTheme.titleLarge?.copyWith(
                  fontWeight: FontWeight.bold,
                ),
          ),
          const SizedBox(height: 16),
          _buildQuickActions(context, subscription),

          const SizedBox(height: 24),

          // Billing info
          _buildBillingInfo(context, subscription),
        ],
      ),
    );
  }

  Widget _buildHistoryTab(BuildContext context, PaymentState state) {
    final payments = _getPaymentHistory(state);

    return Column(
      children: [
        // Search and filter
        Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            children: [
              TextField(
                decoration: InputDecoration(
                  hintText: 'Buscar por monto, fecha...',
                  prefixIcon: const Icon(Icons.search),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                  filled: true,
                  fillColor: Colors.grey[100],
                ),
                onChanged: (value) {
                  setState(() {
                    _searchQuery = value.toLowerCase();
                  });
                },
              ),
              const SizedBox(height: 8),
              SingleChildScrollView(
                scrollDirection: Axis.horizontal,
                child: Row(
                  children: [
                    _buildFilterChip('Todos', null),
                    _buildFilterChip('Completados', PaymentStatus.completed),
                    _buildFilterChip('Fallidos', PaymentStatus.failed),
                    _buildFilterChip('Reembolsados', PaymentStatus.refunded),
                  ],
                ),
              ),
            ],
          ),
        ),

        // Payment history list
        Expanded(
          child: payments.isEmpty
              ? _buildEmptyHistory()
              : ListView.builder(
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  itemCount: _getFilteredPayments(payments).length,
                  itemBuilder: (context, index) {
                    final payment = _getFilteredPayments(payments)[index];
                    return _buildPaymentHistoryItem(context, payment);
                  },
                ),
        ),
      ],
    );
  }

  Widget _buildSubscriptionCard(
      BuildContext context, Subscription subscription) {
    final daysUntilRenewal =
        subscription.nextBillingDate?.difference(DateTime.now()).inDays ?? 0;
    final progressUntilRenewal = (30 - daysUntilRenewal) / 30;

    return Card(
      elevation: 4,
      child: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: [
              Theme.of(context).primaryColor,
              Theme.of(context).primaryColor.withValues(alpha: 0.7),
            ],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
          borderRadius: BorderRadius.circular(12),
        ),
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Plan Actual',
                      style: TextStyle(
                        color: Colors.white70,
                        fontSize: 14,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      _getPlanName(subscription.plan.type),
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 28,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                ),
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                  decoration: BoxDecoration(
                    color: _getStatusColor(
                        subscription.isActive ? 'active' : 'inactive'),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Text(
                    _getStatusLabel(
                        subscription.isActive ? 'active' : 'inactive'),
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 12,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 24),
            Row(
              children: [
                const Text(
                  '\$',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                Text(
                  '\$${subscription.plan.getPriceForPeriod(subscription.billingPeriod).toStringAsFixed(2)}',
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 36,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                Text(
                  ' /${subscription.billingPeriod == BillingPeriod.monthly ? "mes" : "año"}',
                  style: const TextStyle(
                    color: Colors.white70,
                    fontSize: 16,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 24),
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Próxima facturación',
                      style: TextStyle(color: Colors.white70, fontSize: 12),
                    ),
                    Text(
                      '$daysUntilRenewal días',
                      style: const TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 8),
                ClipRRect(
                  borderRadius: BorderRadius.circular(4),
                  child: LinearProgressIndicator(
                    value: progressUntilRenewal,
                    backgroundColor: Colors.white30,
                    valueColor:
                        const AlwaysStoppedAnimation<Color>(Colors.white),
                    minHeight: 6,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  subscription.nextBillingDate != null
                      ? DateFormat('d MMM yyyy')
                          .format(subscription.nextBillingDate!)
                      : 'N/A',
                  style: const TextStyle(color: Colors.white70, fontSize: 12),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildUsageStatsCard(BuildContext context, UsageStats stats) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            _buildUsageItem(
              'Publicaciones activas',
              stats.currentListings,
              stats.listingsLimit,
              Icons.directions_car,
              Colors.blue,
            ),
            const Divider(height: 24),
            _buildUsageItem(
              'Fotos subidas',
              0,
              100,
              Icons.photo_library,
              Colors.green,
            ),
            const Divider(height: 24),
            _buildUsageItem(
              'Destacados usados',
              stats.currentFeaturedListings,
              stats.featuredListingsLimit,
              Icons.star,
              Colors.orange,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildUsageItem(
    String label,
    int current,
    int limit,
    IconData icon,
    Color color,
  ) {
    final percentage = limit > 0 ? (current / limit).clamp(0.0, 1.0) : 0.0;
    final isNearLimit = percentage > 0.8;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: color.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Icon(icon, color: color, size: 20),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    label,
                    style: const TextStyle(fontWeight: FontWeight.w600),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    '$current de $limit',
                    style: TextStyle(
                      color: isNearLimit ? Colors.orange : Colors.grey[600],
                      fontSize: 12,
                    ),
                  ),
                ],
              ),
            ),
            if (isNearLimit)
              const Icon(Icons.warning_amber, color: Colors.orange, size: 20),
          ],
        ),
        const SizedBox(height: 8),
        ClipRRect(
          borderRadius: BorderRadius.circular(4),
          child: LinearProgressIndicator(
            value: percentage,
            backgroundColor: color.withValues(alpha: 0.1),
            valueColor: AlwaysStoppedAnimation<Color>(
              isNearLimit ? Colors.orange : color,
            ),
            minHeight: 6,
          ),
        ),
      ],
    );
  }

  Widget _buildQuickActions(BuildContext context, Subscription? subscription) {
    return Row(
      children: [
        Expanded(
          child: _buildActionButton(
            context,
            'Cambiar Plan',
            Icons.workspace_premium,
            Colors.blue,
            () => Navigator.pushNamed(context, '/plans'),
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: _buildActionButton(
            context,
            'Métodos de Pago',
            Icons.credit_card,
            Colors.green,
            () => Navigator.pushNamed(context, '/payment-methods'),
          ),
        ),
      ],
    );
  }

  Widget _buildActionButton(
    BuildContext context,
    String label,
    IconData icon,
    Color color,
    VoidCallback onTap,
  ) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(12),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: color.withValues(alpha: 0.1),
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: color.withValues(alpha: 0.3)),
        ),
        child: Column(
          children: [
            Icon(icon, color: color, size: 32),
            const SizedBox(height: 8),
            Text(
              label,
              textAlign: TextAlign.center,
              style: TextStyle(
                color: color,
                fontWeight: FontWeight.bold,
                fontSize: 12,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildBillingInfo(BuildContext context, Subscription? subscription) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Información de facturación',
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 16),
            _buildInfoRow(
                'Período de facturación',
                subscription?.billingPeriod == BillingPeriod.monthly
                    ? 'Mensual'
                    : 'Anual'),
            _buildInfoRow('Método de pago', 'Visa ••• 4242'),
            _buildInfoRow(
                'Próximo cargo',
                subscription?.nextBillingDate != null
                    ? DateFormat('d MMM yyyy')
                        .format(subscription!.nextBillingDate!)
                    : '-'),
            const SizedBox(height: 16),
            SizedBox(
              width: double.infinity,
              child: OutlinedButton(
                onPressed: () {
                  // Download invoice
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(
                      content: Text('Descargando factura...'),
                    ),
                  );
                },
                child: const Text('Descargar última factura'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildInfoRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: TextStyle(color: Colors.grey[600]),
          ),
          Text(
            value,
            style: const TextStyle(fontWeight: FontWeight.w600),
          ),
        ],
      ),
    );
  }

  Widget _buildPaymentHistoryItem(BuildContext context, Payment payment) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: ListTile(
        leading: Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: _getStatusColor(payment.status).withValues(alpha: 0.1),
            borderRadius: BorderRadius.circular(8),
          ),
          child: Icon(
            _getPaymentIcon(payment.status),
            color: _getStatusColor(payment.status),
          ),
        ),
        title: Text(
          '\$${payment.amount.toStringAsFixed(2)}',
          style: const TextStyle(fontWeight: FontWeight.bold),
        ),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: 4),
            Text(DateFormat('d MMM yyyy, HH:mm').format(payment.paymentDate)),
            const SizedBox(height: 2),
            Text(
              payment.description,
              style: TextStyle(fontSize: 12, color: Colors.grey[600]),
            ),
          ],
        ),
        trailing: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.end,
          children: [
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              decoration: BoxDecoration(
                color: _getStatusColor(payment.status),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Text(
                _getStatusLabel(payment.status),
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 10,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
            if (payment.status == PaymentStatus.completed)
              TextButton(
                onPressed: () {
                  context
                      .read<PaymentBloc>()
                      .add(GetInvoiceUrlEvent(payment.id));
                },
                style: TextButton.styleFrom(
                  padding: EdgeInsets.zero,
                  minimumSize: const Size(0, 0),
                  tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                ),
                child:
                    const Text('Ver factura', style: TextStyle(fontSize: 11)),
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildFilterChip(String label, PaymentStatus? status) {
    final isSelected = _filterStatus == status;
    return Padding(
      padding: const EdgeInsets.only(right: 8),
      child: FilterChip(
        label: Text(label),
        selected: isSelected,
        onSelected: (selected) {
          setState(() {
            _filterStatus = selected ? status : null;
          });
        },
      ),
    );
  }

  Widget _buildEmptyHistory() {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.receipt_long, size: 64, color: Colors.grey[400]),
          const SizedBox(height: 16),
          Text(
            'No hay historial de pagos',
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: 8),
          Text(
            'Tus pagos aparecerán aquí',
            style: TextStyle(color: Colors.grey[600]),
          ),
        ],
      ),
    );
  }

  // Helper methods
  Subscription? _getCurrentSubscription(PaymentState state) {
    if (state is SubscriptionLoaded) {
      return state.subscription;
    } else if (state is PaymentDashboardLoaded) {
      return state.subscription;
    }
    return null;
  }

  UsageStats? _getUsageStats(PaymentState state) {
    if (state is PaymentDashboardLoaded) {
      return state.usageStats;
    }
    return null;
  }

  List<Payment> _getPaymentHistory(PaymentState state) {
    if (state is PaymentHistoryLoaded) {
      return state.payments;
    } else if (state is PaymentDashboardLoaded) {
      return state.recentPayments;
    }
    return [];
  }

  List<Payment> _getFilteredPayments(List<Payment> payments) {
    return payments.where((payment) {
      final matchesSearch = _searchQuery.isEmpty ||
          payment.amount.toString().contains(_searchQuery) ||
          payment.description.toLowerCase().contains(_searchQuery);
      final matchesFilter =
          _filterStatus == null || payment.status == _filterStatus;
      return matchesSearch && matchesFilter;
    }).toList();
  }

  String _getPlanName(DealerPlanType type) {
    switch (type) {
      case DealerPlanType.free:
        return 'Gratis';
      case DealerPlanType.basic:
        return 'Básico';
      case DealerPlanType.pro:
        return 'Pro';
      case DealerPlanType.enterprise:
        return 'Enterprise';
    }
  }

  String _getStatusLabel(dynamic status) {
    if (status is PaymentStatus) {
      switch (status) {
        case PaymentStatus.completed:
          return 'COMPLETADO';
        case PaymentStatus.pending:
          return 'PENDIENTE';
        case PaymentStatus.failed:
          return 'FALLIDO';
        case PaymentStatus.refunded:
          return 'REEMBOLSADO';
      }
    }
    return 'ACTIVO';
  }

  Color _getStatusColor(dynamic status) {
    if (status is PaymentStatus) {
      switch (status) {
        case PaymentStatus.completed:
          return Colors.green;
        case PaymentStatus.pending:
          return Colors.orange;
        case PaymentStatus.failed:
          return Colors.red;
        case PaymentStatus.refunded:
          return Colors.blue;
      }
    }
    return Colors.green;
  }

  IconData _getPaymentIcon(PaymentStatus status) {
    switch (status) {
      case PaymentStatus.completed:
        return Icons.check_circle;
      case PaymentStatus.pending:
        return Icons.pending;
      case PaymentStatus.failed:
        return Icons.error;
      case PaymentStatus.refunded:
        return Icons.replay;
    }
  }
}
