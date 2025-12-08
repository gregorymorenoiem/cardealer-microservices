import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:intl/intl.dart';
import '../../../core/di/injection.dart';
import '../../../domain/entities/payment.dart';
import '../../bloc/payment/payment_bloc.dart';
import '../../widgets/payment/subscription_dashboard_widget.dart';

/// Page displaying billing history and subscription dashboard
class BillingDashboardPage extends StatelessWidget {
  const BillingDashboardPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => getIt<PaymentBloc>()
        ..add(const LoadCurrentSubscriptionEvent())
        ..add(const LoadPaymentHistoryEvent()),
      child: const _BillingDashboardPageContent(),
    );
  }
}

class _BillingDashboardPageContent extends StatelessWidget {
  const _BillingDashboardPageContent();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Billing & Usage'),
        centerTitle: true,
        actions: [
          IconButton(
            icon: const Icon(Icons.file_download),
            onPressed: () => _exportInvoices(context),
            tooltip: 'Export invoices',
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: () async {
          final bloc = context.read<PaymentBloc>();
          bloc.add(const LoadCurrentSubscriptionEvent());
          bloc.add(const LoadPaymentHistoryEvent());
        },
        child: BlocBuilder<PaymentBloc, PaymentState>(
          builder: (context, state) {
            if (state is PaymentLoading) {
              return const Center(child: CircularProgressIndicator());
            }

            return ListView(
              padding: const EdgeInsets.all(16),
              children: [
                // Subscription overview
                BlocBuilder<PaymentBloc, PaymentState>(
                  buildWhen: (prev, curr) => curr is SubscriptionLoaded,
                  builder: (context, state) {
                    if (state is SubscriptionLoaded &&
                        state.subscription != null) {
                      return SubscriptionDashboardWidget(
                        subscription: state.subscription!,
                      );
                    }
                    return const SizedBox.shrink();
                  },
                ),

                const SizedBox(height: 24),

                // Payment history section
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Payment History',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    TextButton.icon(
                      onPressed: () => _filterHistory(context),
                      icon: const Icon(Icons.filter_list, size: 18),
                      label: const Text('Filter'),
                    ),
                  ],
                ),

                const SizedBox(height: 12),

                // Payment history list
                BlocBuilder<PaymentBloc, PaymentState>(
                  buildWhen: (prev, curr) => curr is PaymentHistoryLoaded,
                  builder: (context, state) {
                    if (state is PaymentHistoryLoaded) {
                      if (state.payments.isEmpty) {
                        return _buildEmptyHistory();
                      }
                      return _buildPaymentHistory(context, state.payments);
                    }
                    return const Center(
                      child: Padding(
                        padding: EdgeInsets.all(32),
                        child: CircularProgressIndicator(),
                      ),
                    );
                  },
                ),
              ],
            );
          },
        ),
      ),
    );
  }

  Widget _buildEmptyHistory() {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          children: [
            Icon(
              Icons.receipt_long_outlined,
              size: 64,
              color: Colors.grey[400],
            ),
            const SizedBox(height: 16),
            Text(
              'No payment history',
              style: TextStyle(
                fontSize: 16,
                color: Colors.grey[600],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildPaymentHistory(BuildContext context, List<Payment> payments) {
    return Column(
      children: payments
          .map((payment) => _buildPaymentItem(context, payment))
          .toList(),
    );
  }

  Widget _buildPaymentItem(BuildContext context, Payment payment) {
    final dateFormat = DateFormat('MMM d, y');

    Color statusColor;
    IconData statusIcon;

    switch (payment.status) {
      case PaymentStatus.completed:
        statusColor = Colors.green;
        statusIcon = Icons.check_circle;
        break;
      case PaymentStatus.pending:
        statusColor = Colors.orange;
        statusIcon = Icons.schedule;
        break;
      case PaymentStatus.failed:
        statusColor = Colors.red;
        statusIcon = Icons.error;
        break;
      case PaymentStatus.refunded:
        statusColor = Colors.blue;
        statusIcon = Icons.undo;
        break;
    }

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      child: ListTile(
        leading: CircleAvatar(
          backgroundColor: statusColor.withValues(alpha: 0.1),
          child: Icon(statusIcon, color: statusColor, size: 20),
        ),
        title: Text(
          payment.description,
          style: const TextStyle(fontWeight: FontWeight.w500),
        ),
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const SizedBox(height: 4),
            Text(dateFormat.format(payment.paymentDate)),
            if (payment.invoiceUrl != null)
              TextButton.icon(
                onPressed: () => _openInvoice(context, payment),
                icon: const Icon(Icons.receipt, size: 16),
                label: const Text('View Invoice'),
                style: TextButton.styleFrom(
                  padding: EdgeInsets.zero,
                  minimumSize: const Size(0, 0),
                  tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                ),
              ),
          ],
        ),
        trailing: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.end,
          children: [
            Text(
              '\$${payment.amount.toStringAsFixed(2)}',
              style: const TextStyle(
                fontWeight: FontWeight.bold,
                fontSize: 16,
              ),
            ),
            const SizedBox(height: 4),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
              decoration: BoxDecoration(
                color: statusColor.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Text(
                payment.status.name.toUpperCase(),
                style: TextStyle(
                  color: statusColor,
                  fontSize: 10,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ],
        ),
        isThreeLine: payment.invoiceUrl != null,
      ),
    );
  }

  void _openInvoice(BuildContext context, Payment payment) {
    if (payment.invoiceUrl == null) return;

    // In a real app, open browser or PDF viewer
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text('Opening invoice: ${payment.invoiceUrl}'),
        action: SnackBarAction(
          label: 'Copy',
          onPressed: () {
            // Copy URL to clipboard
          },
        ),
      ),
    );
  }

  void _exportInvoices(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Export Invoices'),
        content: const Text(
          'Export all invoices as PDF?\n\nThis will download a ZIP file containing all your invoices.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.of(context).pop();
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('Exporting invoices...'),
                  duration: Duration(seconds: 2),
                ),
              );
            },
            child: const Text('Export'),
          ),
        ],
      ),
    );
  }

  void _filterHistory(BuildContext context) {
    showModalBottomSheet(
      context: context,
      builder: (context) => Container(
        padding: const EdgeInsets.all(20),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Filter by Status',
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 16),
            ...[
              'All',
              'Completed',
              'Pending',
              'Failed',
              'Refunded',
            ].map((status) => ListTile(
                  title: Text(status),
                  onTap: () {
                    Navigator.of(context).pop();
                    // Apply filter
                  },
                )),
          ],
        ),
      ),
    );
  }
}
