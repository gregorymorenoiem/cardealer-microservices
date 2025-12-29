import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/di/injection.dart';
import '../../bloc/dealer/dealer_bloc.dart';
import '../../bloc/dealer/dealer_event.dart';
import '../../bloc/dealer/dealer_state.dart';
import '../../widgets/dealer/analytics_charts_widget.dart';

class DealerDashboardPage extends StatelessWidget {
  const DealerDashboardPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) => getIt<DealerBloc>()..add(LoadDashboard('dealer-1')),
      child: const _DealerDashboardContent(),
    );
  }
}

class _DealerDashboardContent extends StatefulWidget {
  const _DealerDashboardContent();

  @override
  State<_DealerDashboardContent> createState() =>
      _DealerDashboardContentState();
}

class _DealerDashboardContentState extends State<_DealerDashboardContent> {
  String _selectedDateRange = '7d';

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Panel del Dealer'),
        actions: [
          // Date range selector
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 8),
            child: DropdownButton<String>(
              value: _selectedDateRange,
              underline: const SizedBox(),
              icon: const Icon(Icons.arrow_drop_down),
              items: const [
                DropdownMenuItem(value: 'today', child: Text('Hoy')),
                DropdownMenuItem(value: '7d', child: Text('7 días')),
                DropdownMenuItem(value: '30d', child: Text('30 días')),
                DropdownMenuItem(value: '1y', child: Text('1 año')),
                DropdownMenuItem(value: 'custom', child: Text('Personalizado')),
              ],
              onChanged: (value) {
                if (value != null) {
                  setState(() {
                    _selectedDateRange = value;
                  });
                }
              },
            ),
          ),
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: () {
              context.read<DealerBloc>().add(RefreshDashboard('dealer-1'));
            },
          ),
        ],
      ),
      body: BlocBuilder<DealerBloc, DealerState>(
        builder: (context, state) {
          if (state is DashboardLoading) {
            return const Center(child: CircularProgressIndicator());
          }

          if (state is DashboardError) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(
                    Icons.error_outline,
                    size: 100,
                    color: Colors.red[300],
                  ),
                  const SizedBox(height: 16),
                  Text(
                    state.message,
                    style: Theme.of(context).textTheme.bodyLarge,
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 16),
                  ElevatedButton.icon(
                    onPressed: () {
                      context
                          .read<DealerBloc>()
                          .add(RefreshDashboard('dealer-1'));
                    },
                    icon: const Icon(Icons.refresh),
                    label: const Text('Reintentar'),
                  ),
                ],
              ),
            );
          }

          if (state is DashboardLoaded) {
            final stats = state.stats;

            return RefreshIndicator(
              onRefresh: () async {
                context.read<DealerBloc>().add(RefreshDashboard('dealer-1'));
                await Future.delayed(const Duration(seconds: 1));
              },
              child: ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  // KPIs Overview
                  _buildKPIsSection(context, stats),
                  const SizedBox(height: 16),

                  // Analytics Charts
                  const AnalyticsChartsWidget(),
                  const SizedBox(height: 24),

                  // Summary Cards
                  _buildSummarySection(context, stats),
                  const SizedBox(height: 24),

                  // Listings Overview
                  _buildListingsSection(context, stats),
                  const SizedBox(height: 24),

                  // Performance Metrics
                  _buildPerformanceSection(context, stats),
                  const SizedBox(height: 24),

                  // Top Performing Vehicles
                  _buildTopVehiclesSection(context, stats),
                  const SizedBox(height: 24),

                  // Recent Activity Feed
                  _buildRecentActivitySection(context),
                ],
              ),
            );
          }

          return const SizedBox.shrink();
        },
      ),
    );
  }

  Widget _buildKPIsSection(BuildContext context, stats) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'KPIs Principales',
                  style: Theme.of(context).textTheme.titleLarge?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                ),
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: Colors.green.shade100,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(Icons.trending_up,
                          size: 16, color: Colors.green.shade700),
                      const SizedBox(width: 4),
                      Text(
                        '+12.5%',
                        style: TextStyle(
                          color: Colors.green.shade700,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            const Row(
              children: [
                Expanded(
                  child: _KPICard(
                    title: 'Conversión',
                    value: '2.8%',
                    icon: Icons.swap_horiz,
                    trend: '+0.3%',
                  ),
                ),
                SizedBox(width: 8),
                Expanded(
                  child: _KPICard(
                    title: 'Tiempo Promedio',
                    value: '18d',
                    icon: Icons.access_time,
                    trend: '-2d',
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildRecentActivitySection(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Actividad Reciente',
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 16),
            const _ActivityItem(
              icon: Icons.shopping_cart,
              title: 'Nuevo lead generado',
              subtitle: 'Toyota Camry 2023',
              time: 'Hace 5 min',
              color: Colors.green,
            ),
            const _ActivityItem(
              icon: Icons.visibility,
              title: 'Publicación vista',
              subtitle: 'Honda Civic 2022',
              time: 'Hace 15 min',
              color: Colors.blue,
            ),
            const _ActivityItem(
              icon: Icons.edit,
              title: 'Publicación actualizada',
              subtitle: 'BMW X5 2024',
              time: 'Hace 1 hora',
              color: Colors.orange,
            ),
            const SizedBox(height: 8),
            Center(
              child: TextButton(
                onPressed: () {},
                child: const Text('Ver todas las actividades'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSummarySection(BuildContext context, stats) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Resumen',
          style: Theme.of(context).textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.bold,
              ),
        ),
        const SizedBox(height: 16),
        Row(
          children: [
            Expanded(
              child: _StatCard(
                title: 'Listados Activos',
                value: stats.activeListings.toString(),
                icon: Icons.directions_car,
                color: Colors.blue,
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: _StatCard(
                title: 'Leads Total',
                value: stats.totalLeads.toString(),
                icon: Icons.people,
                color: Colors.orange,
              ),
            ),
          ],
        ),
        const SizedBox(height: 12),
        Row(
          children: [
            Expanded(
              child: _StatCard(
                title: 'Vistas',
                value: stats.totalViews.toString(),
                icon: Icons.visibility,
                color: Colors.purple,
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: _StatCard(
                title: 'Vendidos',
                value: stats.soldListings.toString(),
                icon: Icons.check_circle,
                color: Colors.green,
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildListingsSection(BuildContext context, stats) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Mis Listados',
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                ),
                TextButton(
                  onPressed: () {
                    // TODO: Navigate to listings management
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(
                        content: Text('Gestión de listados - Próximamente'),
                      ),
                    );
                  },
                  child: const Text('Ver Todos'),
                ),
              ],
            ),
            const SizedBox(height: 16),
            _ListingStat(
              label: 'Total',
              value: stats.totalListings,
              color: Colors.grey,
            ),
            _ListingStat(
              label: 'Activos',
              value: stats.activeListings,
              color: Colors.green,
            ),
            _ListingStat(
              label: 'Pendientes',
              value: stats.pendingListings,
              color: Colors.orange,
            ),
            _ListingStat(
              label: 'Vendidos',
              value: stats.soldListings,
              color: Colors.blue,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildPerformanceSection(BuildContext context, stats) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Métricas de Rendimiento',
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 16),
            _MetricRow(
              label: 'Tasa de Conversión',
              value: '${stats.conversionRate.toStringAsFixed(2)}%',
              icon: Icons.trending_up,
            ),
            _MetricRow(
              label: 'Tiempo de Respuesta',
              value: '${stats.averageResponseTime.toStringAsFixed(1)} hrs',
              icon: Icons.schedule,
            ),
            _MetricRow(
              label: 'Ingresos del Mes',
              value: '\$${stats.monthlyRevenue.toStringAsFixed(0)}',
              icon: Icons.attach_money,
            ),
            _MetricRow(
              label: 'Ingresos Totales',
              value: '\$${stats.revenue.toStringAsFixed(0)}',
              icon: Icons.account_balance_wallet,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildTopVehiclesSection(BuildContext context, stats) {
    if (stats.topPerformingVehicles.isEmpty) {
      return const SizedBox.shrink();
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Mejores Vehículos',
          style: Theme.of(context).textTheme.titleLarge?.copyWith(
                fontWeight: FontWeight.bold,
              ),
        ),
        const SizedBox(height: 16),
        ...stats.topPerformingVehicles.map(
          (vehicle) => Card(
            margin: const EdgeInsets.only(bottom: 12),
            child: ListTile(
              leading: ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: Image.network(
                  vehicle.imageUrl,
                  width: 60,
                  height: 60,
                  fit: BoxFit.cover,
                  errorBuilder: (_, __, ___) => Container(
                    width: 60,
                    height: 60,
                    color: Colors.grey[300],
                    child: const Icon(Icons.directions_car),
                  ),
                ),
              ),
              title: Text(
                vehicle.title,
                style: const TextStyle(fontWeight: FontWeight.bold),
              ),
              subtitle: Text(
                '${vehicle.views} vistas • ${vehicle.leads} leads',
              ),
              trailing: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Text(
                    '${vehicle.conversionRate.toStringAsFixed(1)}%',
                    style: const TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 16,
                      color: Colors.green,
                    ),
                  ),
                  const Text(
                    'conversión',
                    style: TextStyle(fontSize: 10),
                  ),
                ],
              ),
            ),
          ),
        ),
      ],
    );
  }
}

class _StatCard extends StatelessWidget {
  final String title;
  final String value;
  final IconData icon;
  final Color color;

  const _StatCard({
    required this.title,
    required this.value,
    required this.icon,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Icon(icon, color: color, size: 32),
            const SizedBox(height: 12),
            Text(
              value,
              style: Theme.of(context).textTheme.headlineMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 4),
            Text(
              title,
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: Colors.grey[600],
                  ),
            ),
          ],
        ),
      ),
    );
  }
}

class _ListingStat extends StatelessWidget {
  final String label;
  final int value;
  final Color color;

  const _ListingStat({
    required this.label,
    required this.value,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Row(
            children: [
              Container(
                width: 12,
                height: 12,
                decoration: BoxDecoration(
                  color: color,
                  shape: BoxShape.circle,
                ),
              ),
              const SizedBox(width: 12),
              Text(label),
            ],
          ),
          Text(
            value.toString(),
            style: const TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 16,
            ),
          ),
        ],
      ),
    );
  }
}

class _MetricRow extends StatelessWidget {
  final String label;
  final String value;
  final IconData icon;

  const _MetricRow({
    required this.label,
    required this.value,
    required this.icon,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        children: [
          Icon(icon, size: 20, color: Colors.grey[600]),
          const SizedBox(width: 12),
          Expanded(child: Text(label)),
          Text(
            value,
            style: const TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 16,
            ),
          ),
        ],
      ),
    );
  }
}

class _KPICard extends StatelessWidget {
  final String title;
  final String value;
  final IconData icon;
  final String trend;

  const _KPICard({
    required this.title,
    required this.value,
    required this.icon,
    required this.trend,
  });

  @override
  Widget build(BuildContext context) {
    final isPositive = trend.startsWith('+');
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.grey.shade50,
        borderRadius: BorderRadius.circular(8),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Icon(icon, size: 20),
              Text(
                trend,
                style: TextStyle(
                  color: isPositive ? Colors.green : Colors.red,
                  fontSize: 12,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            value,
            style: const TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            title,
            style: TextStyle(
              fontSize: 12,
              color: Colors.grey.shade600,
            ),
          ),
        ],
      ),
    );
  }
}

class _ActivityItem extends StatelessWidget {
  final IconData icon;
  final String title;
  final String subtitle;
  final String time;
  final Color color;

  const _ActivityItem({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.time,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: color.withAlpha(25),
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
                  title,
                  style: const TextStyle(fontWeight: FontWeight.w500),
                ),
                Text(
                  subtitle,
                  style: TextStyle(
                    fontSize: 12,
                    color: Colors.grey.shade600,
                  ),
                ),
              ],
            ),
          ),
          Text(
            time,
            style: TextStyle(
              fontSize: 12,
              color: Colors.grey.shade600,
            ),
          ),
        ],
      ),
    );
  }
}
